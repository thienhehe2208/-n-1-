using System.Data;
using bài_tập_1.Data;
using bài_tập_1.Models;
using bài_tập_1.Models.ViewModels;
using bài_tập_1.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace bài_tập_1.Controllers
{
    [Authorize]
    public class MuonOnlineController : Controller
    {
        private readonly bài_tập_1Context _context;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly DocGiaEligibilityService _eligibilityService;
        private readonly MuonOnlineService _muonOnlineService;

        public MuonOnlineController(
            bài_tập_1Context context,
            UserManager<IdentityUser> userManager,
            DocGiaEligibilityService eligibilityService,
            MuonOnlineService muonOnlineService)
        {
            _context = context;
            _userManager = userManager;
            _eligibilityService = eligibilityService;
            _muonOnlineService = muonOnlineService;
        }

        [Authorize(Roles = "DocGia")]
        public async Task<IActionResult> Create(int maSach)
        {
            await _muonOnlineService.XuLyHetHanAsync();
            var sach = await _context.Sach
                .Include(s => s.BanSaos)
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.MaSach == maSach);
            if (sach == null)
                return NotFound();

            var model = new TaoYeuCauMuonOnlineViewModel
            {
                MaSach = sach.MaSach,
                TenSach = sach.TenSach,
                AnhBia = sach.AnhBia,
                SoBanSanCo = sach.BanSaos.Count(b =>
                    b.TinhTrang == TinhTrangBanSao.SanCo)
            };
            return View(model);
        }

        [Authorize(Roles = "DocGia")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            TaoYeuCauMuonOnlineViewModel model)
        {
            await _muonOnlineService.XuLyHetHanAsync();
            await using var transaction = await _context.Database
                .BeginTransactionAsync(IsolationLevel.Serializable);

            var userId = _userManager.GetUserId(User);
            var docGia = await _context.DocGia
                .FirstOrDefaultAsync(d => d.UserId == userId);
            var sach = await _context.Sach
                .Include(s => s.BanSaos)
                .FirstOrDefaultAsync(s => s.MaSach == model.MaSach);

            if (docGia == null)
                ModelState.AddModelError(string.Empty, "Không tìm thấy hồ sơ độc giả.");
            else if (docGia.TrangThai != TrangThaiDocGia.HoatDong ||
                     docGia.NgayHetHanThe < DateTime.Today)
                ModelState.AddModelError(string.Empty, "Thẻ độc giả không còn hiệu lực.");

            if (sach == null)
                ModelState.AddModelError(nameof(model.MaSach), "Cuốn sách không tồn tại.");

            if (model.NgayHenNhan.Date < DateTime.Today ||
                model.NgayHenNhan.Date > DateTime.Today.AddDays(3))
                ModelState.AddModelError(nameof(model.NgayHenNhan),
                    "Ngày nhận phải từ hôm nay đến 3 ngày tới.");

            if (model.NgayHenTra.Date <= model.NgayHenNhan.Date ||
                model.NgayHenTra.Date > model.NgayHenNhan.Date
                    .AddDays(LibraryRules.SoNgayMuonToiDa))
                ModelState.AddModelError(nameof(model.NgayHenTra),
                    $"Hạn trả phải sau ngày nhận và không quá {LibraryRules.SoNgayMuonToiDa} ngày.");

            BanSao? banSao = sach?.BanSaos
                .Where(b => b.TinhTrang == TinhTrangBanSao.SanCo)
                .OrderBy(b => b.MaBanSao)
                .FirstOrDefault();
            if (sach != null && banSao == null)
                ModelState.AddModelError(string.Empty,
                    "Sách vừa hết bản sẵn có. Vui lòng chọn Đặt trước.");

            if (docGia != null)
            {
                var errors = await _eligibilityService.KiemTraAsync(
                    docGia.MaDocGia,
                    new KiemTraDocGiaOptions { MaSach = model.MaSach });
                foreach (var error in errors)
                    ModelState.AddModelError(string.Empty, error);
            }

            if (!ModelState.IsValid)
            {
                await transaction.RollbackAsync();
                model.TenSach = sach?.TenSach ?? model.TenSach;
                model.AnhBia = sach?.AnhBia ?? model.AnhBia;
                model.SoBanSanCo = sach?.BanSaos.Count(b =>
                    b.TinhTrang == TinhTrangBanSao.SanCo) ?? 0;
                return View(model);
            }

            banSao!.TinhTrang = TinhTrangBanSao.DaGiu;
            var request = new YeuCauMuonOnline
            {
                MaXacNhan = Guid.NewGuid().ToString("N")[..12].ToUpperInvariant(),
                MaDocGia = docGia!.MaDocGia,
                MaSach = sach!.MaSach,
                MaBanSao = banSao.MaBanSao,
                NgayHenNhan = model.NgayHenNhan.Date,
                NgayHenTra = model.NgayHenTra.Date,
                HanNhanSach = model.NgayHenNhan.Date.AddDays(1).AddTicks(-1),
                GhiChu = model.GhiChu?.Trim() ?? string.Empty
            };
            _context.YeuCauMuonOnline.Add(request);
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
                    "Bản sao vừa được người khác giữ. Vui lòng thử lại hoặc đặt trước.");
                model.TenSach = sach.TenSach;
                model.AnhBia = sach.AnhBia;
                model.SoBanSanCo = sach.BanSaos.Count(b =>
                    b.TinhTrang == TinhTrangBanSao.SanCo);
                return View(model);
            }

            return RedirectToAction(nameof(Phieu), new { id = request.MaYeuCau });
        }

        [Authorize(Roles = "DocGia")]
        public async Task<IActionResult> Phieu(int id)
        {
            await _muonOnlineService.XuLyHetHanAsync();
            var userId = _userManager.GetUserId(User);
            var request = await RequestQuery()
                .FirstOrDefaultAsync(y => y.MaYeuCau == id && y.DocGia.UserId == userId);
            return request == null ? NotFound() : View(request);
        }

        [Authorize(Roles = "Admin,NhanVien")]
        public async Task<IActionResult> XacNhan(string ma)
        {
            await _muonOnlineService.XuLyHetHanAsync();
            var request = await RequestQuery()
                .FirstOrDefaultAsync(y => y.MaXacNhan == ma);
            return request == null ? NotFound() : View(request);
        }

        [Authorize(Roles = "Admin,NhanVien")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> XacNhanNhanSach(string ma)
        {
            await _muonOnlineService.XuLyHetHanAsync();
            await using var transaction = await _context.Database
                .BeginTransactionAsync(IsolationLevel.Serializable);
            var request = await _context.YeuCauMuonOnline
                .Include(y => y.DocGia)
                .Include(y => y.BanSao)
                .FirstOrDefaultAsync(y => y.MaXacNhan == ma);
            if (request == null)
                return NotFound();
            if (request.TrangThai != TrangThaiYeuCauMuonOnline.ChoNhan ||
                request.BanSao.TinhTrang != TinhTrangBanSao.DaGiu ||
                request.HanNhanSach < DateTime.Now)
            {
                TempData["Error"] = "Phiếu không còn hiệu lực hoặc đã được xử lý.";
                return RedirectToAction(nameof(XacNhan), new { ma });
            }

            var eligibilityErrors = await _eligibilityService.KiemTraAsync(
                request.MaDocGia,
                new KiemTraDocGiaOptions
                {
                    MaSach = request.MaSach,
                    BoQuaMaYeuCauOnline = request.MaYeuCau
                });
            if (eligibilityErrors.Count != 0)
            {
                await transaction.RollbackAsync();
                TempData["Error"] = string.Join(" ", eligibilityErrors);
                return RedirectToAction(nameof(XacNhan), new { ma });
            }

            var userId = _userManager.GetUserId(User);
            var nhanVien = await _context.NhanVien
                .FirstOrDefaultAsync(n => n.UserId == userId);
            if (nhanVien == null)
                return Forbid();

            var loan = new PhieuMuon
            {
                MaDocGia = request.MaDocGia,
                MaNhanVien = nhanVien.MaNhanVien,
                NgayMuon = DateTime.Now,
                NgayHenTra = request.NgayHenTra,
                TrangThai = TrangThaiPhieuMuon.DangMuon
            };
            loan.ChiTietPhieuMuons.Add(new ChiTietPhieuMuon
            {
                MaBanSao = request.MaBanSao,
                GhiChu = "Tạo từ phiếu mượn online " + request.MaXacNhan
            });
            request.BanSao.TinhTrang = TinhTrangBanSao.DangMuon;
            request.TrangThai = TrangThaiYeuCauMuonOnline.DaNhan;
            request.PhieuMuon = loan;
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            TempData["Success"] = "Đã xác nhận giao sách và tạo phiếu mượn.";
            return RedirectToAction("Details", "PhieuMuons", new { id = loan.MaPhieuMuon });
        }

        private IQueryable<YeuCauMuonOnline> RequestQuery() =>
            _context.YeuCauMuonOnline
                .Include(y => y.DocGia)
                .Include(y => y.Sach)
                .Include(y => y.BanSao)
                .Include(y => y.PhieuMuon)
                .AsNoTracking();

    }
}
