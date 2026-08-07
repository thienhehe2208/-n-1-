using System.Data;
using bài_tập_1.Data;
using bài_tập_1.Models;
using bài_tập_1.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using bài_tập_1.Services;

namespace bài_tập_1.Controllers
{
    [Authorize(Roles = "Admin,NhanVien")]
    public class PhieuPhatsController : Controller
    {
        private readonly bài_tập_1Context _context;
        private readonly AdminChangeNotificationService _adminChangeNotification;

        public PhieuPhatsController(
            bài_tập_1Context context,
            AdminChangeNotificationService adminChangeNotification)
        {
            _context = context;
            _adminChangeNotification = adminChangeNotification;
        }

        public async Task<IActionResult> Index(
            string? q,
            string? trangThai,
            int page = 1)
        {
            var query = _context.PhieuPhat
                .Include(p => p.ChiTietPhieuMuon)
                    .ThenInclude(c => c.BanSao)
                        .ThenInclude(b => b.Sach)
                .Include(p => p.ChiTietPhieuMuon)
                    .ThenInclude(c => c.PhieuMuon)
                        .ThenInclude(pm => pm.DocGia)
                .AsNoTracking()
                .AsQueryable();

            ViewData["TongPhieuPhat"] = await query.CountAsync();
            ViewData["ChuaDong"] = await query.CountAsync(p =>
                p.TrangThai == TrangThaiPhieuPhat.ChuaDong);
            ViewData["DaDong"] = await query.CountAsync(p =>
                p.TrangThai == TrangThaiPhieuPhat.DaDong);
            ViewData["TongNo"] = await query
                .Where(p =>
                    p.TrangThai == TrangThaiPhieuPhat.ChuaDong)
                .SumAsync(p => (decimal?)p.SoTien) ?? 0;

            if (!string.IsNullOrWhiteSpace(q))
            {
                var keyword = q.Trim();
                var isId = int.TryParse(
                    keyword.TrimStart('#'),
                    out var id);

                query = query.Where(p =>
                    p.ChiTietPhieuMuon.PhieuMuon.DocGia.HoTen
                        .Contains(keyword) ||
                    p.ChiTietPhieuMuon.BanSao.Sach.TenSach
                        .Contains(keyword) ||
                    p.ChiTietPhieuMuon.BanSao.MaVach
                        .Contains(keyword) ||
                    (isId && p.MaPhieuPhat == id));
            }

            query = trangThai switch
            {
                "unpaid" => query.Where(p =>
                    p.TrangThai == TrangThaiPhieuPhat.ChuaDong),
                "paid" => query.Where(p =>
                    p.TrangThai == TrangThaiPhieuPhat.DaDong),
                "cancelled" => query.Where(p =>
                    p.TrangThai == TrangThaiPhieuPhat.DaHuy),
                _ => query
            };

            ViewData["Search"] = q;
            ViewData["Status"] = trangThai;

            var pagination = Pagination.Create(page, await query.CountAsync());
            ViewData["Pagination"] = pagination;
            return View(await query
                .OrderBy(p =>
                    p.TrangThai != TrangThaiPhieuPhat.ChuaDong)
                .ThenByDescending(p => p.NgayLap)
                .Skip((pagination.Page - 1) * pagination.PageSize)
                .Take(pagination.PageSize)
                .ToListAsync());
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return NotFound();

            var phieuPhat = await LoadPhieuPhatAsync(id.Value);
            return phieuPhat == null
                ? NotFound()
                : View(phieuPhat);
        }

        public async Task<IActionResult> Create(int? maChiTiet)
        {
            await LoadEligibleDetailsAsync(maChiTiet);

            if (!maChiTiet.HasValue)
                return View(new PhieuPhatFormViewModel());

            var chiTiet = await LoadChiTietAsync(maChiTiet.Value);
            if (chiTiet == null)
                return NotFound();

            var reasons = GetApplicableReasons(chiTiet);
            if (chiTiet.PhieuPhat != null || reasons.Count == 0)
            {
                TempData["Error"] =
                    "Lượt mượn này không đủ điều kiện lập phiếu phạt " +
                    "hoặc đã có phiếu phạt.";
                return RedirectToAction(nameof(Index));
            }

            var model = new PhieuPhatFormViewModel
            {
                MaChiTiet = chiTiet.MaChiTiet,
                LyDo = reasons[0]
            };
            CopyDisplayData(model, chiTiet);
            LoadReasonOptions(reasons, model.LyDo);

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            PhieuPhatFormViewModel model)
        {
            await using var transaction =
                await _context.Database.BeginTransactionAsync(
                    IsolationLevel.Serializable);

            var chiTiet = await LoadChiTietForUpdateAsync(
                model.MaChiTiet);

            ValidateFineRequest(model, chiTiet);

            if (!ModelState.IsValid)
            {
                await transaction.RollbackAsync();
                await PrepareInvalidModelAsync(model, chiTiet);
                return View(model);
            }

            var phieuPhat = new PhieuPhat
            {
                MaChiTiet = chiTiet!.MaChiTiet,
                SoTien = model.SoTien,
                LyDo = model.LyDo!.Value,
                NgayLap = DateTime.Now,
                TrangThai = TrangThaiPhieuPhat.ChuaDong
            };
            _context.PhieuPhat.Add(phieuPhat);

            try
            {
                await _context.SaveChangesAsync();
                await _adminChangeNotification.ThemThongBaoAsync(
                    User,
                    "phiếu phạt",
                    $"PP-{phieuPhat.MaPhieuPhat:D5}",
                    Url.Action(
                        nameof(Details),
                        "PhieuPhats",
                        new { id = phieuPhat.MaPhieuPhat }) ??
                    $"/PhieuPhats/Details/{phieuPhat.MaPhieuPhat}",
                    $"Đã lập phiếu với số tiền {phieuPhat.SoTien:N0} đồng.");
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch (DbUpdateException)
            {
                await transaction.RollbackAsync();
                ModelState.AddModelError(
                    string.Empty,
                    "Không thể lập phiếu phạt. " +
                    "Lượt mượn này có thể đã được lập phiếu phạt.");
                await PrepareInvalidModelAsync(model, chiTiet);
                return View(model);
            }

            TempData["Success"] = "Đã lập phiếu phạt.";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var phieuPhat = await LoadPhieuPhatAsync(id.Value);
            if (phieuPhat == null)
                return NotFound();

            if (phieuPhat.TrangThai !=
                TrangThaiPhieuPhat.ChuaDong)
            {
                TempData["Error"] =
                    "Chỉ phiếu chưa đóng mới được chỉnh sửa.";
                return RedirectToAction(
                    nameof(Details),
                    new { id });
            }

            var reasons = GetApplicableReasons(
                phieuPhat.ChiTietPhieuMuon);
            var model = new PhieuPhatFormViewModel
            {
                MaPhieuPhat = phieuPhat.MaPhieuPhat,
                MaChiTiet = phieuPhat.MaChiTiet,
                SoTien = phieuPhat.SoTien,
                LyDo = phieuPhat.LyDo
            };

            CopyDisplayData(model, phieuPhat.ChiTietPhieuMuon);
            LoadReasonOptions(reasons, model.LyDo);
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            PhieuPhatFormViewModel model)
        {
            if (id != model.MaPhieuPhat)
                return NotFound();

            var phieuPhat = await _context.PhieuPhat
                .Include(p => p.ChiTietPhieuMuon)
                    .ThenInclude(c => c.BanSao)
                        .ThenInclude(b => b.Sach)
                .Include(p => p.ChiTietPhieuMuon)
                    .ThenInclude(c => c.PhieuMuon)
                        .ThenInclude(pm => pm.DocGia)
                .FirstOrDefaultAsync(p => p.MaPhieuPhat == id);

            if (phieuPhat == null)
                return NotFound();

            if (phieuPhat.TrangThai !=
                TrangThaiPhieuPhat.ChuaDong)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "Chỉ phiếu chưa đóng mới được chỉnh sửa.");
            }

            if (model.MaChiTiet != phieuPhat.MaChiTiet)
                return BadRequest();

            var reasons = GetApplicableReasons(
                phieuPhat.ChiTietPhieuMuon);
            if (!model.LyDo.HasValue ||
                !reasons.Contains(model.LyDo.Value))
            {
                ModelState.AddModelError(
                    nameof(model.LyDo),
                    "Lý do phạt không phù hợp với lượt trả sách.");
            }

            if (!ModelState.IsValid)
            {
                CopyDisplayData(
                    model,
                    phieuPhat.ChiTietPhieuMuon);
                LoadReasonOptions(reasons, model.LyDo);
                return View(model);
            }

            var thayDoi = new List<string>();
            if (phieuPhat.SoTien != model.SoTien)
            {
                thayDoi.Add(
                    $"Số tiền: {phieuPhat.SoTien:N0} → {model.SoTien:N0} đồng.");
            }

            if (phieuPhat.LyDo != model.LyDo!.Value)
            {
                thayDoi.Add(
                    $"Lý do: {GetReasonText(phieuPhat.LyDo)} → " +
                    $"{GetReasonText(model.LyDo.Value)}.");
            }

            phieuPhat.SoTien = model.SoTien;
            phieuPhat.LyDo = model.LyDo.Value;

            if (thayDoi.Count > 0)
            {
                await _adminChangeNotification.ThemThongBaoAsync(
                    User,
                    "phiếu phạt",
                    $"PP-{phieuPhat.MaPhieuPhat:D5}",
                    Url.Action(
                        nameof(Details),
                        "PhieuPhats",
                        new { id = phieuPhat.MaPhieuPhat }) ??
                    $"/PhieuPhats/Details/{phieuPhat.MaPhieuPhat}",
                    string.Join(" ", thayDoi));
            }

            await _context.SaveChangesAsync();
            TempData["Success"] = "Đã cập nhật phiếu phạt.";

            return RedirectToAction(
                nameof(Details),
                new { id = phieuPhat.MaPhieuPhat });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Huy(int id)
        {
            var phieuPhat = await _context.PhieuPhat
                .Include(p => p.ChiTietPhieuMuon)
                    .ThenInclude(c => c.PhieuMuon)
                .FirstOrDefaultAsync(p => p.MaPhieuPhat == id);

            if (phieuPhat == null)
                return NotFound();

            if (phieuPhat.TrangThai !=
                TrangThaiPhieuPhat.ChuaDong)
            {
                TempData["Error"] =
                    "Chỉ phiếu chưa đóng mới có thể hủy.";
                return RedirectToAction(
                    nameof(Details),
                    new { id });
            }

            phieuPhat.TrangThai = TrangThaiPhieuPhat.DaHuy;
            _context.ThongBao.Add(new ThongBao
            {
                MaDocGia = phieuPhat.ChiTietPhieuMuon.PhieuMuon.MaDocGia,
                MaSuKien = $"phieu-phat-{phieuPhat.MaPhieuPhat}-huy",
                TieuDe = "Phiếu phạt đã được hủy",
                NoiDung = $"Phiếu phạt #PP-{phieuPhat.MaPhieuPhat:D5} trị giá {phieuPhat.SoTien:N0} đồng đã được thư viện hủy. Phiếu vẫn được lưu trong lịch sử nhưng không còn tính vào công nợ của bạn.",
                LienKet = string.Empty,
                Loai = "info",
                NgayTao = DateTime.Now
            });
            await _context.SaveChangesAsync();

            TempData["Success"] =
                "Đã hủy phiếu phạt. Trạng thái đã được lưu và lịch sử vẫn được giữ lại.";
            return RedirectToAction(
                nameof(Details),
                new { id });
        }

        private async Task<PhieuPhat?> LoadPhieuPhatAsync(int id)
        {
            return await _context.PhieuPhat
                .Include(p => p.ChiTietPhieuMuon)
                    .ThenInclude(c => c.BanSao)
                        .ThenInclude(b => b.Sach)
                .Include(p => p.ChiTietPhieuMuon)
                    .ThenInclude(c => c.PhieuMuon)
                        .ThenInclude(pm => pm.DocGia)
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.MaPhieuPhat == id);
        }

        private async Task<ChiTietPhieuMuon?> LoadChiTietAsync(int id)
        {
            return await _context.ChiTietPhieuMuon
                .Include(c => c.BanSao)
                    .ThenInclude(b => b.Sach)
                .Include(c => c.PhieuMuon)
                    .ThenInclude(p => p.DocGia)
                .Include(c => c.PhieuPhat)
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.MaChiTiet == id);
        }

        private async Task<ChiTietPhieuMuon?>
            LoadChiTietForUpdateAsync(int id)
        {
            return await _context.ChiTietPhieuMuon
                .Include(c => c.BanSao)
                    .ThenInclude(b => b.Sach)
                .Include(c => c.PhieuMuon)
                    .ThenInclude(p => p.DocGia)
                .Include(c => c.PhieuPhat)
                .FirstOrDefaultAsync(c => c.MaChiTiet == id);
        }

        private void ValidateFineRequest(
            PhieuPhatFormViewModel model,
            ChiTietPhieuMuon? chiTiet)
        {
            if (chiTiet == null)
            {
                ModelState.AddModelError(
                    nameof(model.MaChiTiet),
                    "Không tìm thấy lượt mượn.");
                return;
            }

            if (!chiTiet.NgayTra.HasValue)
            {
                ModelState.AddModelError(
                    nameof(model.MaChiTiet),
                    "Chỉ được lập phiếu phạt sau khi trả sách.");
            }

            if (chiTiet.PhieuPhat != null)
            {
                ModelState.AddModelError(
                    nameof(model.MaChiTiet),
                    "Lượt mượn này đã có phiếu phạt.");
            }

            var reasons = GetApplicableReasons(chiTiet);
            if (reasons.Count == 0)
            {
                ModelState.AddModelError(
                    nameof(model.MaChiTiet),
                    "Lượt mượn không bị trễ, hỏng hoặc mất.");
            }
            else if (!model.LyDo.HasValue ||
                     !reasons.Contains(model.LyDo.Value))
            {
                ModelState.AddModelError(
                    nameof(model.LyDo),
                    "Lý do phạt không phù hợp với lượt trả sách.");
            }
        }

        private async Task PrepareInvalidModelAsync(
            PhieuPhatFormViewModel model,
            ChiTietPhieuMuon? chiTiet)
        {
            if (chiTiet != null)
            {
                CopyDisplayData(model, chiTiet);
                LoadReasonOptions(
                    GetApplicableReasons(chiTiet),
                    model.LyDo);
            }
            else
            {
                LoadReasonOptions(
                    Array.Empty<LyDoPhat>(),
                    model.LyDo);
            }

            await LoadEligibleDetailsAsync(model.MaChiTiet);
        }

        private async Task LoadEligibleDetailsAsync(
            int? selectedId = null)
        {
            var items = await _context.ChiTietPhieuMuon
                .Where(c =>
                    c.NgayTra != null &&
                    c.PhieuPhat == null &&
                    (c.NgayTra > c.PhieuMuon.NgayHenTra ||
                     c.TinhTrangKhiTra ==
                        TinhTrangKhiTra.HuHong ||
                     c.TinhTrangKhiTra ==
                        TinhTrangKhiTra.Mat))
                .Include(c => c.BanSao)
                    .ThenInclude(b => b.Sach)
                .Include(c => c.PhieuMuon)
                    .ThenInclude(p => p.DocGia)
                .OrderByDescending(c => c.NgayTra)
                .AsNoTracking()
                .ToListAsync();

            var options = items.Select(c => new
            {
                c.MaChiTiet,
                MoTa = "#PM-" + c.MaPhieuMuon.ToString("D5") +
                       " · " + c.PhieuMuon.DocGia.HoTen +
                       " · " + c.BanSao.Sach.TenSach +
                       " (" + c.BanSao.MaVach + ")"
            });

            ViewData["MaChiTiet"] = new SelectList(
                options,
                "MaChiTiet",
                "MoTa",
                selectedId);
        }

        private static List<LyDoPhat> GetApplicableReasons(
            ChiTietPhieuMuon chiTiet)
        {
            var reasons = new List<LyDoPhat>();

            if (chiTiet.NgayTra.HasValue &&
                chiTiet.NgayTra.Value.Date >
                    chiTiet.PhieuMuon.NgayHenTra.Date)
            {
                reasons.Add(LyDoPhat.TraTre);
            }

            if (chiTiet.TinhTrangKhiTra ==
                TinhTrangKhiTra.HuHong)
            {
                reasons.Add(LyDoPhat.HuHong);
            }

            if (chiTiet.TinhTrangKhiTra ==
                TinhTrangKhiTra.Mat)
            {
                reasons.Add(LyDoPhat.MatSach);
            }

            return reasons;
        }

        private void LoadReasonOptions(
            IEnumerable<LyDoPhat> reasons,
            LyDoPhat? selected)
        {
            var options = reasons
                .Distinct()
                .Select(reason => new
                {
                    Value = (int)reason,
                    Text = GetReasonText(reason)
                })
                .ToList();

            ViewData["LyDo"] = new SelectList(
                options,
                "Value",
                "Text",
                selected.HasValue ? (int)selected.Value : null);
        }

        private static void CopyDisplayData(
            PhieuPhatFormViewModel model,
            ChiTietPhieuMuon chiTiet)
        {
            model.MaPhieuMuon = chiTiet.MaPhieuMuon;
            model.HoTenDocGia =
                chiTiet.PhieuMuon.DocGia.HoTen;
            model.TenSach = chiTiet.BanSao.Sach.TenSach;
            model.MaVach = chiTiet.BanSao.MaVach;
            model.NgayHenTra = chiTiet.PhieuMuon.NgayHenTra;
            model.NgayTra = chiTiet.NgayTra;
            model.TinhTrangKhiTra =
                chiTiet.TinhTrangKhiTra;
            model.SoNgayTre = chiTiet.NgayTra.HasValue
                ? Math.Max(
                    0,
                    (chiTiet.NgayTra.Value.Date -
                     chiTiet.PhieuMuon.NgayHenTra.Date).Days)
                : 0;
        }

        private static string GetReasonText(LyDoPhat reason)
        {
            return reason switch
            {
                LyDoPhat.TraTre => "Trả sách trễ hạn",
                LyDoPhat.MatSach => "Làm mất sách",
                LyDoPhat.HuHong => "Làm hư hỏng sách",
                _ => reason.ToString()
            };
        }
    }
}
