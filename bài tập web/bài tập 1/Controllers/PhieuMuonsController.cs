using bài_tập_1.Data;
using bài_tập_1.Models;
using bài_tập_1.Models.ViewModels;
using bài_tập_1.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace bài_tập_1.Controllers
{
    [Authorize(Roles = "Admin,NhanVien")]
    public class PhieuMuonsController : Controller
    {
        private readonly bài_tập_1Context _context;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly PhieuMuonService _phieuMuonService;

        public PhieuMuonsController(
            bài_tập_1Context context,
            UserManager<IdentityUser> userManager,
            PhieuMuonService phieuMuonService)
        {
            _context = context;
            _userManager = userManager;
            _phieuMuonService = phieuMuonService;
        }

        public async Task<IActionResult> Index(
            string? q,
            string? trangThai,
            int page = 1)
        {
            await _phieuMuonService.CapNhatTrangThaiAsync();
            var homNay = DateTime.Today;
            var query = _context.PhieuMuon
                .Include(p => p.DocGia)
                .Include(p => p.NhanVien)
                .Include(p => p.ChiTietPhieuMuons)
                .AsNoTracking()
                .AsQueryable();

            ViewData["TongPhieu"] = await query.CountAsync();
            ViewData["DangMuon"] = await query.CountAsync(p =>
                p.TrangThai != TrangThaiPhieuMuon.DaTra &&
                p.NgayHenTra >= homNay);
            ViewData["DaTra"] = await query.CountAsync(p =>
                p.TrangThai == TrangThaiPhieuMuon.DaTra);
            ViewData["QuaHan"] = await query.CountAsync(p =>
                p.TrangThai != TrangThaiPhieuMuon.DaTra &&
                p.NgayHenTra < homNay);

            if (!string.IsNullOrWhiteSpace(q))
            {
                var keyword = q.Trim();
                var isId = int.TryParse(
                    keyword.TrimStart('#'),
                    out var maPhieu);

                query = query.Where(p =>
                    p.DocGia.HoTen.Contains(keyword) ||
                    p.NhanVien.HoTen.Contains(keyword) ||
                    (isId && p.MaPhieuMuon == maPhieu));
            }

            query = trangThai switch
            {
                "borrowing" => query.Where(p =>
                    p.TrangThai != TrangThaiPhieuMuon.DaTra &&
                    p.NgayHenTra >= homNay),
                "returned" => query.Where(p =>
                    p.TrangThai == TrangThaiPhieuMuon.DaTra),
                "overdue" => query.Where(p =>
                    p.TrangThai != TrangThaiPhieuMuon.DaTra &&
                    p.NgayHenTra < homNay),
                _ => query
            };

            ViewData["Search"] = q;
            ViewData["Status"] = trangThai;

            var pagination = Pagination.Create(page, await query.CountAsync());
            ViewData["Pagination"] = pagination;
            return View(await query
                .OrderByDescending(p => p.NgayMuon)
                .ThenByDescending(p => p.MaPhieuMuon)
                .Skip((pagination.Page - 1) * pagination.PageSize)
                .Take(pagination.PageSize)
                .ToListAsync());
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return NotFound();

            await _phieuMuonService.CapNhatTrangThaiAsync(id.Value);

            var phieuMuon = await _context.PhieuMuon
                .Include(p => p.DocGia)
                .Include(p => p.NhanVien)
                .Include(p => p.ChiTietPhieuMuons)
                    .ThenInclude(ct => ct.BanSao)
                        .ThenInclude(b => b.Sach)
                .Include(p => p.ChiTietPhieuMuons)
                    .ThenInclude(ct => ct.PhieuPhat)
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.MaPhieuMuon == id);

            return phieuMuon == null
                ? NotFound()
                : View(phieuMuon);
        }

        public async Task<IActionResult> Create()
        {
            await LoadDocGiasAsync();
            return View(new LapPhieuMuonViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            LapPhieuMuonViewModel model)
        {
            var docGia = await _context.DocGia
                .AsNoTracking()
                .FirstOrDefaultAsync(d => d.MaDocGia == model.MaDocGia);

            ValidateDocGia(docGia);

            if (docGia != null)
            {
                var conNoPhat = await _context.PhieuPhat
                    .AnyAsync(p =>
                        p.TrangThai ==
                            TrangThaiPhieuPhat.ChuaDong &&
                        p.ChiTietPhieuMuon.PhieuMuon.MaDocGia ==
                            docGia.MaDocGia);

                if (conNoPhat)
                {
                    ModelState.AddModelError(
                        nameof(model.MaDocGia),
                        "Độc giả còn phiếu phạt chưa thanh toán.");
                }
            }

            if (model.NgayHenTra.Date <= DateTime.Today)
            {
                ModelState.AddModelError(
                    nameof(model.NgayHenTra),
                    "Ngày hẹn trả phải sau ngày lập phiếu.");
            }

            if (model.NgayHenTra.Date >
                DateTime.Today.AddDays(LibraryRules.SoNgayMuonToiDa))
            {
                ModelState.AddModelError(
                    nameof(model.NgayHenTra),
                    $"Thời hạn mượn không được vượt quá {LibraryRules.SoNgayMuonToiDa} ngày.");
            }

            var userId = _userManager.GetUserId(User);
            var nhanVien = await _context.NhanVien
                .AsNoTracking()
                .FirstOrDefaultAsync(n => n.UserId == userId);

            if (nhanVien == null)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "Tài khoản hiện tại chưa có hồ sơ nhân viên.");
            }

            if (!ModelState.IsValid)
            {
                await LoadDocGiasAsync(model.MaDocGia);
                return View(model);
            }

            var phieuMuon = new PhieuMuon
            {
                MaDocGia = docGia!.MaDocGia,
                MaNhanVien = nhanVien!.MaNhanVien,
                NgayMuon = DateTime.Now,
                NgayHenTra = model.NgayHenTra.Date,
                TrangThai = TrangThaiPhieuMuon.DangMuon
            };

            _context.PhieuMuon.Add(phieuMuon);
            await _context.SaveChangesAsync();

            TempData["Success"] =
                "Đã lập phiếu. Hãy thêm các bản sao cần cho mượn.";

            return RedirectToAction(
                "Create",
                "ChiTietPhieuMuons",
                new { maPhieuMuon = phieuMuon.MaPhieuMuon });
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var phieuMuon = await _context.PhieuMuon
                .Include(p => p.DocGia)
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.MaPhieuMuon == id);

            if (phieuMuon == null)
                return NotFound();

            if (phieuMuon.TrangThai == TrangThaiPhieuMuon.DaTra)
            {
                TempData["Error"] =
                    "Không thể thay đổi hạn trả của phiếu đã hoàn tất.";
                return RedirectToAction(
                    nameof(Details),
                    new { id = phieuMuon.MaPhieuMuon });
            }

            return View(new CapNhatHanTraViewModel
            {
                MaPhieuMuon = phieuMuon.MaPhieuMuon,
                HoTenDocGia = phieuMuon.DocGia.HoTen,
                NgayMuon = phieuMuon.NgayMuon,
                NgayHenTra = phieuMuon.NgayHenTra
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            CapNhatHanTraViewModel model)
        {
            if (id != model.MaPhieuMuon)
                return NotFound();

            var phieuMuon = await _context.PhieuMuon
                .Include(p => p.DocGia)
                .FirstOrDefaultAsync(p => p.MaPhieuMuon == id);

            if (phieuMuon == null)
                return NotFound();

            if (phieuMuon.TrangThai == TrangThaiPhieuMuon.DaTra)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "Không thể thay đổi phiếu đã hoàn tất.");
            }

            if (model.NgayHenTra.Date <= phieuMuon.NgayMuon.Date)
            {
                ModelState.AddModelError(
                    nameof(model.NgayHenTra),
                    "Ngày hẹn trả phải sau ngày mượn.");
            }

            if (model.NgayHenTra.Date >
                phieuMuon.NgayMuon.Date.AddDays(LibraryRules.SoNgayMuonToiDa))
            {
                ModelState.AddModelError(
                    nameof(model.NgayHenTra),
                    $"Thời hạn mượn ban đầu không được vượt quá {LibraryRules.SoNgayMuonToiDa} ngày.");
            }

            if (!ModelState.IsValid)
            {
                model.HoTenDocGia = phieuMuon.DocGia.HoTen;
                model.NgayMuon = phieuMuon.NgayMuon;
                return View(model);
            }

            phieuMuon.NgayHenTra = model.NgayHenTra.Date;
            phieuMuon.TrangThai =
                phieuMuon.NgayHenTra < DateTime.Today
                    ? TrangThaiPhieuMuon.QuaHan
                    : TrangThaiPhieuMuon.DangMuon;

            await _context.SaveChangesAsync();
            TempData["Success"] = "Đã cập nhật hạn trả.";

            return RedirectToAction(
                nameof(Details),
                new { id = phieuMuon.MaPhieuMuon });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GiaHan(int id)
        {
            await using var transaction =
                await _context.Database.BeginTransactionAsync();

            await _phieuMuonService.CapNhatTrangThaiAsync(id);

            var phieuMuon = await _context.PhieuMuon
                .Include(p => p.ChiTietPhieuMuons)
                    .ThenInclude(c => c.BanSao)
                .Include(p => p.DocGia)
                .FirstOrDefaultAsync(p => p.MaPhieuMuon == id);

            if (phieuMuon == null)
                return NotFound();

            if (phieuMuon.TrangThai != TrangThaiPhieuMuon.DangMuon ||
                phieuMuon.ChiTietPhieuMuons.All(c => c.NgayTra.HasValue))
            {
                TempData["Error"] = "Chỉ phiếu đang mượn và chưa quá hạn mới được gia hạn.";
                return RedirectToAction(nameof(Details), new { id });
            }

            if (phieuMuon.SoLanGiaHan >= LibraryRules.SoLanGiaHanToiDa)
            {
                TempData["Error"] =
                    $"Phiếu đã đạt giới hạn {LibraryRules.SoLanGiaHanToiDa} lần gia hạn.";
                return RedirectToAction(nameof(Details), new { id });
            }

            var conNoPhat = await _context.PhieuPhat.AnyAsync(p =>
                p.TrangThai == TrangThaiPhieuPhat.ChuaDong &&
                p.ChiTietPhieuMuon.PhieuMuon.MaDocGia == phieuMuon.MaDocGia);

            var maSachDangMuon = phieuMuon.ChiTietPhieuMuons
                .Where(c => !c.NgayTra.HasValue)
                .Select(c => c.BanSao.MaSach)
                .Distinct()
                .ToList();

            var coNguoiDatTruoc = await _context.DatTruoc.AnyAsync(d =>
                maSachDangMuon.Contains(d.MaSach) &&
                d.MaDocGia != phieuMuon.MaDocGia &&
                (d.TrangThai == TrangThaiDatTruoc.DangCho ||
                 d.TrangThai == TrangThaiDatTruoc.DaCoSach));

            if (conNoPhat || coNguoiDatTruoc)
            {
                TempData["Error"] = conNoPhat
                    ? "Độc giả còn phiếu phạt chưa thanh toán."
                    : "Không thể gia hạn vì có độc giả khác đang đặt trước sách trong phiếu.";
                return RedirectToAction(nameof(Details), new { id });
            }

            phieuMuon.NgayHenTra = phieuMuon.NgayHenTra.Date
                .AddDays(LibraryRules.SoNgayMoiLanGiaHan);
            phieuMuon.SoLanGiaHan++;
            phieuMuon.NgayGiaHanGanNhat = DateTime.Now;

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            TempData["Success"] =
                $"Đã gia hạn thêm {LibraryRules.SoNgayMoiLanGiaHan} ngày.";
            return RedirectToAction(nameof(Details), new { id });
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
                return NotFound();

            var phieuMuon = await _context.PhieuMuon
                .Include(p => p.DocGia)
                .Include(p => p.NhanVien)
                .Include(p => p.ChiTietPhieuMuons)
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.MaPhieuMuon == id);

            return phieuMuon == null
                ? NotFound()
                : View(phieuMuon);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var phieuMuon = await _context.PhieuMuon
                .Include(p => p.ChiTietPhieuMuons)
                .FirstOrDefaultAsync(p => p.MaPhieuMuon == id);

            if (phieuMuon == null)
                return NotFound();

            if (phieuMuon.ChiTietPhieuMuons.Count != 0)
            {
                TempData["Error"] =
                    "Không thể xóa phiếu đã có sách. " +
                    "Hãy xử lý từng lượt mượn thay vì xóa lịch sử.";
                return RedirectToAction(nameof(Index));
            }

            _context.PhieuMuon.Remove(phieuMuon);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Đã xóa phiếu trống.";
            return RedirectToAction(nameof(Index));
        }

        private async Task LoadDocGiasAsync(int? selectedId = null)
        {
            var docGias = await _context.DocGia
                .Where(d =>
                    d.TrangThai == TrangThaiDocGia.HoatDong &&
                    d.NgayHetHanThe >= DateTime.Today)
                .OrderBy(d => d.HoTen)
                .AsNoTracking()
                .ToListAsync();

            ViewData["MaDocGia"] = new SelectList(
                docGias,
                "MaDocGia",
                "HoTen",
                selectedId);
        }

        private void ValidateDocGia(DocGia? docGia)
        {
            if (docGia == null)
            {
                ModelState.AddModelError(
                    nameof(LapPhieuMuonViewModel.MaDocGia),
                    "Không tìm thấy độc giả.");
                return;
            }

            if (docGia.TrangThai != TrangThaiDocGia.HoatDong)
            {
                ModelState.AddModelError(
                    nameof(LapPhieuMuonViewModel.MaDocGia),
                    "Thẻ độc giả đang bị khóa.");
            }

            if (docGia.NgayHetHanThe < DateTime.Today)
            {
                ModelState.AddModelError(
                    nameof(LapPhieuMuonViewModel.MaDocGia),
                    "Thẻ độc giả đã hết hạn.");
            }
        }
    }
}
