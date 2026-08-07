using System.Data;
using bài_tập_1.Data;
using bài_tập_1.Models;
using bài_tập_1.Models.ViewModels;
using bài_tập_1.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace bài_tập_1.Controllers
{
    [Authorize(Roles = "Admin,NhanVien")]
    public class ChiTietPhieuMuonsController : Controller
    {
        private readonly bài_tập_1Context _context;
        private readonly DatTruocService _datTruocService;
        private readonly PhieuMuonService _phieuMuonService;
        private readonly DocGiaEligibilityService _eligibilityService;

        public ChiTietPhieuMuonsController(
            bài_tập_1Context context,
            DatTruocService datTruocService,
            PhieuMuonService phieuMuonService,
            DocGiaEligibilityService eligibilityService)
        {
            _context = context;
            _datTruocService = datTruocService;
            _phieuMuonService = phieuMuonService;
            _eligibilityService = eligibilityService;
        }

        public async Task<IActionResult> Index(
            string? q,
            string? trangThai,
            int page = 1)
        {
            await _phieuMuonService.CapNhatTrangThaiAsync();
            var homNay = DateTime.Today;
            var query = _context.ChiTietPhieuMuon
                .Include(c => c.BanSao)
                    .ThenInclude(b => b.Sach)
                        .ThenInclude(s => s.TheLoai)
                .Include(c => c.BanSao)
                    .ThenInclude(b => b.Sach)
                        .ThenInclude(s => s.NhaXuatBan)
                .Include(c => c.PhieuMuon)
                    .ThenInclude(p => p.DocGia)
                .Include(c => c.PhieuPhat)
                .AsNoTracking()
                .AsQueryable();

            ViewData["TongChiTiet"] = await query.CountAsync();
            ViewData["DangMuon"] =
                await query.CountAsync(c => c.NgayTra == null);
            ViewData["DaTra"] =
                await query.CountAsync(c => c.NgayTra != null);
            ViewData["QuaHan"] = await query.CountAsync(c =>
                c.NgayTra == null &&
                c.PhieuMuon.NgayHenTra < homNay);
            ViewData["CoSuCo"] = await query.CountAsync(c =>
                c.TinhTrangKhiTra == TinhTrangKhiTra.HuHong ||
                c.TinhTrangKhiTra == TinhTrangKhiTra.Mat);

            if (!string.IsNullOrWhiteSpace(q))
            {
                var keyword = q.Trim();
                var isId = int.TryParse(
                    keyword.TrimStart('#'),
                    out var id);

                query = query.Where(c =>
                    c.BanSao.MaVach.Contains(keyword) ||
                    c.BanSao.Sach.TenSach.Contains(keyword) ||
                    c.PhieuMuon.DocGia.HoTen.Contains(keyword) ||
                    (isId && c.MaPhieuMuon == id));
            }

            query = trangThai switch
            {
                "borrowing" => query.Where(c => c.NgayTra == null),
                "returned" => query.Where(c => c.NgayTra != null),
                "overdue" => query.Where(c =>
                    c.NgayTra == null &&
                    c.PhieuMuon.NgayHenTra < homNay),
                "incident" => query.Where(c =>
                    c.TinhTrangKhiTra == TinhTrangKhiTra.HuHong ||
                    c.TinhTrangKhiTra == TinhTrangKhiTra.Mat),
                _ => query
            };

            ViewData["Search"] = q;
            ViewData["Status"] = trangThai;

            var pagination = Pagination.Create(page, await query.CountAsync());
            ViewData["Pagination"] = pagination;
            var items = await query
                .OrderBy(c => c.PhieuMuon.TrangThai == TrangThaiPhieuMuon.DaTra)
                .ThenBy(c => c.PhieuMuon.NgayHenTra)
                .ThenBy(c => c.MaPhieuMuon)
                .ThenBy(c => c.MaChiTiet)
                .Skip((pagination.Page - 1) * pagination.PageSize)
                .Take(pagination.PageSize)
                .ToListAsync();

            var maPhieuMuons = items
                .Select(c => c.MaPhieuMuon)
                .Distinct()
                .ToList();
            var soSachTheoPhieu = await _context.ChiTietPhieuMuon
                .Where(c => maPhieuMuons.Contains(c.MaPhieuMuon))
                .GroupBy(c => c.MaPhieuMuon)
                .Select(g => new
                {
                    MaPhieuMuon = g.Key,
                    SoSach = g.Count()
                })
                .ToListAsync();
            ViewData["SoSachTheoPhieu"] = soSachTheoPhieu
                .ToDictionary(item => item.MaPhieuMuon, item => item.SoSach);

            return View(items);
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return NotFound();

            var chiTiet = await _context.ChiTietPhieuMuon
                .Include(c => c.BanSao)
                    .ThenInclude(b => b.Sach)
                        .ThenInclude(s => s.TheLoai)
                .Include(c => c.BanSao)
                    .ThenInclude(b => b.Sach)
                        .ThenInclude(s => s.NhaXuatBan)
                .Include(c => c.PhieuMuon)
                    .ThenInclude(p => p.DocGia)
                .Include(c => c.PhieuPhat)
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.MaChiTiet == id);

            return chiTiet == null ? NotFound() : View(chiTiet);
        }

        public async Task<IActionResult> Create(int? maPhieuMuon)
        {
            await LoadSelectionsAsync(maPhieuMuon);
            return View(new ThemSachVaoPhieuViewModel
            {
                MaPhieuMuon = maPhieuMuon ?? 0
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            ThemSachVaoPhieuViewModel model)
        {
            model.GhiChu = model.GhiChu?.Trim() ?? string.Empty;
            await using var transaction =
                await _context.Database.BeginTransactionAsync(
                    IsolationLevel.Serializable);

            var phieuMuon = await _context.PhieuMuon
                .Include(p => p.DocGia)
                .FirstOrDefaultAsync(
                    p => p.MaPhieuMuon == model.MaPhieuMuon);

            var banSao = await _context.BanSao
                .Include(b => b.Sach)
                .FirstOrDefaultAsync(
                    b => b.MaBanSao == model.MaBanSao);

            ValidatePhieuMuon(phieuMuon);

            if (phieuMuon != null && banSao != null)
            {
                var errors = await _eligibilityService.KiemTraAsync(
                    phieuMuon.MaDocGia,
                    new KiemTraDocGiaOptions { MaSach = banSao.MaSach });
                foreach (var error in errors)
                    ModelState.AddModelError(nameof(model.MaBanSao), error);
            }

            if (banSao == null)
            {
                ModelState.AddModelError(
                    nameof(model.MaBanSao),
                    "Không tìm thấy bản sao.");
            }
            else if (banSao.TinhTrang != TinhTrangBanSao.SanCo)
            {
                ModelState.AddModelError(
                    nameof(model.MaBanSao),
                    "Bản sao này hiện không sẵn sàng để cho mượn.");
            }

            var dangDuocMuon = await _context.ChiTietPhieuMuon
                .AnyAsync(c =>
                    c.MaBanSao == model.MaBanSao &&
                    c.NgayTra == null);

            if (dangDuocMuon)
            {
                ModelState.AddModelError(
                    nameof(model.MaBanSao),
                    "Bản sao này đang thuộc một lượt mượn khác.");
            }

            if (!ModelState.IsValid)
            {
                await transaction.RollbackAsync();
                await LoadSelectionsAsync(
                    model.MaPhieuMuon,
                    model.MaBanSao);
                return View(model);
            }

            _context.ChiTietPhieuMuon.Add(
                new ChiTietPhieuMuon
                {
                    MaPhieuMuon = phieuMuon!.MaPhieuMuon,
                    MaBanSao = banSao!.MaBanSao,
                    PhiThue = LibraryRules.PhiThueMoiCuon,
                    NgayTra = null,
                    TinhTrangKhiTra = null,
                    GhiChu = model.GhiChu
                });

            banSao.TinhTrang = TinhTrangBanSao.DangMuon;
            phieuMuon.TrangThai =
                phieuMuon.NgayHenTra < DateTime.Today
                    ? TrangThaiPhieuMuon.QuaHan
                    : TrangThaiPhieuMuon.DangMuon;

            try
            {
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch (DbUpdateException)
            {
                await transaction.RollbackAsync();
                ModelState.AddModelError(
                    string.Empty,
                    "Không thể thêm bản sao. " +
                    "Bản sao có thể vừa được cho mượn ở thao tác khác.");
                await LoadSelectionsAsync(
                    model.MaPhieuMuon,
                    model.MaBanSao);
                return View(model);
            }

            TempData["Success"] =
                $"Đã thêm “{banSao.Sach.TenSach}” vào phiếu.";

            return RedirectToAction(
                "Details",
                "PhieuMuons",
                new { id = phieuMuon.MaPhieuMuon });
        }

        [HttpGet]
        [ActionName("TraSach")]
        public async Task<IActionResult> ChuyenSangTraCaPhieu(int? id)
        {
            if (id == null)
                return NotFound();

            var maPhieuMuon = await _context.ChiTietPhieuMuon
                .Where(c => c.MaChiTiet == id.Value)
                .Select(c => (int?)c.MaPhieuMuon)
                .FirstOrDefaultAsync();
            if (!maPhieuMuon.HasValue)
                return NotFound();

            TempData["Error"] =
                "Không thể trả riêng một cuốn. Vui lòng xác nhận trả toàn bộ phiếu.";
            return RedirectToAction(
                "TraSach",
                "PhieuMuons",
                new { id = maPhieuMuon.Value });
        }

        [NonAction]
        private async Task<IActionResult> TraSachTungCuon(int? id)
        {
            if (id == null)
                return NotFound();

            var chiTiet = await LoadChiTietAsync(id.Value);
            if (chiTiet == null)
                return NotFound();

            if (chiTiet.NgayTra.HasValue)
            {
                TempData["Error"] = "Lượt mượn này đã được trả trước đó.";
                return RedirectToAction(nameof(Details), new { id });
            }

            return View(ToTraSachViewModel(chiTiet));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [NonAction]
        private async Task<IActionResult> TraSachTungCuon(
            int id,
            TraSachViewModel model)
        {
            if (id != model.MaChiTiet)
                return NotFound();

            await using var transaction =
                await _context.Database.BeginTransactionAsync(
                    IsolationLevel.Serializable);

            var chiTiet = await _context.ChiTietPhieuMuon
                .Include(c => c.BanSao)
                    .ThenInclude(b => b.Sach)
                .Include(c => c.PhieuMuon)
                    .ThenInclude(p => p.DocGia)
                .Include(c => c.PhieuMuon)
                    .ThenInclude(p => p.ChiTietPhieuMuons)
                .FirstOrDefaultAsync(c => c.MaChiTiet == id);

            if (chiTiet == null)
                return NotFound();

            if (chiTiet.NgayTra.HasValue)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "Lượt mượn này đã được trả trước đó.");
            }

            if (model.NgayTra.Date < chiTiet.PhieuMuon.NgayMuon.Date)
            {
                ModelState.AddModelError(
                    nameof(model.NgayTra),
                    "Ngày trả không thể trước ngày mượn.");
            }

            if (model.NgayTra.Date > DateTime.Today)
            {
                ModelState.AddModelError(
                    nameof(model.NgayTra),
                    "Ngày trả không thể nằm trong tương lai.");
            }

            if (!ModelState.IsValid)
            {
                await transaction.RollbackAsync();
                CopyDisplayData(model, chiTiet);
                return View(model);
            }

            chiTiet.NgayTra = model.NgayTra.Date;
            chiTiet.TinhTrangKhiTra = model.TinhTrangKhiTra;
            chiTiet.GhiChu = model.GhiChu.Trim();

            chiTiet.BanSao.TinhTrang = model.TinhTrangKhiTra switch
            {
                TinhTrangKhiTra.BinhThuong =>
                    TinhTrangBanSao.SanCo,
                TinhTrangKhiTra.HuHong =>
                    TinhTrangBanSao.HuHong,
                TinhTrangKhiTra.Mat =>
                    TinhTrangBanSao.Mat,
                _ => chiTiet.BanSao.TinhTrang
            };

            var daTraHet = chiTiet.PhieuMuon.ChiTietPhieuMuons
                .All(c =>
                    c.MaChiTiet == chiTiet.MaChiTiet ||
                    c.NgayTra.HasValue);

            chiTiet.PhieuMuon.TrangThai = daTraHet
                ? TrangThaiPhieuMuon.DaTra
                : chiTiet.PhieuMuon.NgayHenTra < DateTime.Today
                    ? TrangThaiPhieuMuon.QuaHan
                    : TrangThaiPhieuMuon.DangMuon;

            var tinhTrangText = model.TinhTrangKhiTra switch
            {
                TinhTrangKhiTra.BinhThuong => "Bình thường",
                TinhTrangKhiTra.HuHong => "Hư hỏng",
                TinhTrangKhiTra.Mat => "Mất sách",
                _ => "Đã kiểm tra"
            };
            var dungHanText = model.NgayTra.Date <=
                              chiTiet.PhieuMuon.NgayHenTra.Date
                ? "đúng hạn"
                : $"trễ {(model.NgayTra.Date - chiTiet.PhieuMuon.NgayHenTra.Date).Days} ngày";
            _context.ThongBao.Add(new ThongBao
            {
                MaDocGia = chiTiet.PhieuMuon.MaDocGia,
                MaSuKien = $"tra-sach-{chiTiet.MaChiTiet}",
                TieuDe = "Đã xác nhận trả sách",
                NoiDung = $"Sách “{chiTiet.BanSao.Sach.TenSach}” đã được trả ngày {model.NgayTra:dd/MM/yyyy} ({dungHanText}). Tình trạng khi trả: {tinhTrangText}.",
                LienKet = string.Empty,
                Loai = model.TinhTrangKhiTra == TinhTrangKhiTra.BinhThuong
                    ? "success"
                    : "warning",
                NgayTao = DateTime.Now
            });

            if (daTraHet)
            {
                var dueEvent = $"phieu-muon-{chiTiet.MaPhieuMuon}-han-tra";
                var oldDueNotification = await _context.ThongBao
                    .FirstOrDefaultAsync(t =>
                        t.MaDocGia == chiTiet.PhieuMuon.MaDocGia &&
                        t.MaSuKien == dueEvent);
                if (oldDueNotification != null)
                    _context.ThongBao.Remove(oldDueNotification);
            }

            if (model.TinhTrangKhiTra == TinhTrangKhiTra.BinhThuong)
            {
                await _datTruocService.PhanBoBanSaoAsync(
                    chiTiet.BanSao);
            }

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            var coPhatSinhPhat =
                model.NgayTra.Date >
                    chiTiet.PhieuMuon.NgayHenTra.Date ||
                model.TinhTrangKhiTra is
                    TinhTrangKhiTra.HuHong or
                    TinhTrangKhiTra.Mat;

            TempData["Success"] = coPhatSinhPhat
                ? "Đã ghi nhận trả sách. " +
                  "Lượt mượn này cần được kiểm tra để lập phiếu phạt."
                : "Đã ghi nhận trả sách thành công.";

            if (coPhatSinhPhat)
            {
                return RedirectToAction(
                    "Create",
                    "PhieuPhats",
                    new { maChiTiet = chiTiet.MaChiTiet });
            }

            return RedirectToAction(
                "Details",
                "PhieuMuons",
                new { id = chiTiet.MaPhieuMuon });
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
                return NotFound();

            var chiTiet = await LoadChiTietAsync(id.Value);
            return chiTiet == null ? NotFound() : View(chiTiet);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await using var transaction =
                await _context.Database.BeginTransactionAsync();

            var chiTiet = await _context.ChiTietPhieuMuon
                .Include(c => c.BanSao)
                .Include(c => c.PhieuMuon)
                    .ThenInclude(p => p.ChiTietPhieuMuons)
                .Include(c => c.PhieuPhat)
                .FirstOrDefaultAsync(c => c.MaChiTiet == id);

            if (chiTiet == null)
                return NotFound();

            if (chiTiet.NgayTra.HasValue || chiTiet.PhieuPhat != null)
            {
                TempData["Error"] =
                    "Không thể xóa lượt mượn đã trả hoặc đã có phiếu phạt.";
                return RedirectToAction(nameof(Index));
            }

            var maPhieuMuon = chiTiet.MaPhieuMuon;
            chiTiet.BanSao.TinhTrang = TinhTrangBanSao.SanCo;
            _context.ChiTietPhieuMuon.Remove(chiTiet);

            await _context.SaveChangesAsync();
            await _datTruocService.PhanBoBanSaoAsync(chiTiet.BanSao);
            await _phieuMuonService.CapNhatTrangThaiAsync(maPhieuMuon);
            await transaction.CommitAsync();

            TempData["Success"] =
                "Đã hủy lượt mượn và hoàn lại trạng thái bản sao.";

            return RedirectToAction(
                "Details",
                "PhieuMuons",
                new { id = maPhieuMuon });
        }

        private async Task LoadSelectionsAsync(
            int? maPhieuMuon = null,
            int? maBanSao = null)
        {
            var phieuMuons = await _context.PhieuMuon
                .Include(p => p.DocGia)
                .Where(p =>
                    p.TrangThai != TrangThaiPhieuMuon.DaTra &&
                    p.NgayHenTra >= DateTime.Today &&
                    p.DocGia.TrangThai ==
                        TrangThaiDocGia.HoatDong &&
                    p.DocGia.NgayHetHanThe >= DateTime.Today)
                .OrderByDescending(p => p.NgayMuon)
                .AsNoTracking()
                .Select(p => new
                {
                    p.MaPhieuMuon,
                    MoTa = "#PM" + p.MaPhieuMuon +
                           " - " + p.DocGia.HoTen
                })
                .ToListAsync();

            var banSaos = await _context.BanSao
                .Include(b => b.Sach)
                .Where(b => b.TinhTrang == TinhTrangBanSao.SanCo)
                .OrderBy(b => b.Sach.TenSach)
                .ThenBy(b => b.MaVach)
                .AsNoTracking()
                .Select(b => new
                {
                    b.MaBanSao,
                    MoTa = b.Sach.TenSach + " - " + b.MaVach
                })
                .ToListAsync();

            ViewData["SoPhieuHopLe"] = phieuMuons.Count;
            ViewData["SoBanSaoSanCo"] = banSaos.Count;

            ViewData["MaPhieuMuon"] = new SelectList(
                phieuMuons,
                "MaPhieuMuon",
                "MoTa",
                maPhieuMuon);

            ViewData["MaBanSao"] = new SelectList(
                banSaos,
                "MaBanSao",
                "MoTa",
                maBanSao);
        }

        private void ValidatePhieuMuon(PhieuMuon? phieuMuon)
        {
            if (phieuMuon == null)
            {
                ModelState.AddModelError(
                    nameof(ThemSachVaoPhieuViewModel.MaPhieuMuon),
                    "Không tìm thấy phiếu mượn.");
                return;
            }

            if (phieuMuon.TrangThai ==
                TrangThaiPhieuMuon.DaTra)
            {
                ModelState.AddModelError(
                    nameof(ThemSachVaoPhieuViewModel.MaPhieuMuon),
                    "Phiếu mượn đã hoàn tất.");
            }

            if (phieuMuon.NgayHenTra < DateTime.Today)
            {
                ModelState.AddModelError(
                    nameof(ThemSachVaoPhieuViewModel.MaPhieuMuon),
                    "Không thể thêm sách vào phiếu đã quá hạn.");
            }

            if (phieuMuon.DocGia.TrangThai !=
                TrangThaiDocGia.HoatDong)
            {
                ModelState.AddModelError(
                    nameof(ThemSachVaoPhieuViewModel.MaPhieuMuon),
                    "Thẻ độc giả đang bị khóa.");
            }

            if (phieuMuon.DocGia.NgayHetHanThe < DateTime.Today)
            {
                ModelState.AddModelError(
                    nameof(ThemSachVaoPhieuViewModel.MaPhieuMuon),
                    "Thẻ độc giả đã hết hạn.");
            }
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

        private static TraSachViewModel ToTraSachViewModel(
            ChiTietPhieuMuon chiTiet)
        {
            var model = new TraSachViewModel
            {
                MaChiTiet = chiTiet.MaChiTiet,
                NgayTra = DateTime.Today,
                TinhTrangKhiTra = TinhTrangKhiTra.BinhThuong,
                GhiChu = chiTiet.GhiChu ?? string.Empty
            };

            CopyDisplayData(model, chiTiet);
            return model;
        }

        private static void CopyDisplayData(
            TraSachViewModel model,
            ChiTietPhieuMuon chiTiet)
        {
            model.MaPhieuMuon = chiTiet.MaPhieuMuon;
            model.TenSach = chiTiet.BanSao.Sach.TenSach;
            model.MaVach = chiTiet.BanSao.MaVach;
            model.HoTenDocGia = chiTiet.PhieuMuon.DocGia.HoTen;
            model.NgayMuon = chiTiet.PhieuMuon.NgayMuon;
            model.NgayHenTra = chiTiet.PhieuMuon.NgayHenTra;
        }
    }
}
