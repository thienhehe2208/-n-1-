using bài_tập_1.Data;
using bài_tập_1.Models;
using bài_tập_1.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using bài_tập_1.Services;

namespace bài_tập_1.Controllers
{
    [Authorize]
    public class ProfileController : Controller
    {
        private readonly bài_tập_1Context _context;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly PhieuMuonService _phieuMuonService;

        public ProfileController(
            bài_tập_1Context context,
            UserManager<IdentityUser> userManager,
            PhieuMuonService phieuMuonService)
        {
            _context = context;
            _userManager = userManager;
            _phieuMuonService = phieuMuonService;
        }

        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrWhiteSpace(userId))
                return Challenge();

            var docGia = await _context.DocGia
                .AsNoTracking()
                .FirstOrDefaultAsync(d => d.UserId == userId);
            if (docGia != null)
                return View("IndexDocGia", docGia);

            var nhanVien = await _context.NhanVien
                .AsNoTracking()
                .FirstOrDefaultAsync(n => n.UserId == userId);
            if (nhanVien != null)
                return View("IndexNhanVien", nhanVien);

            // Khôi phục các tài khoản cũ chưa có role/hồ sơ liên kết.
            var identityUser = await _userManager.FindByIdAsync(userId);
            if (identityUser == null)
                return Challenge();

            var email = identityUser.Email ?? identityUser.UserName ?? string.Empty;
            docGia = new DocGia
            {
                UserId = identityUser.Id,
                HoTen = email.Contains('@') ? email[..email.IndexOf('@')] : email,
                NgaySinh = null,
                GioiTinh = string.Empty,
                DiaChi = string.Empty,
                SoDienThoai = identityUser.PhoneNumber ?? string.Empty,
                Email = email,
                NgayDangKy = DateTime.Now,
                NgayHetHanThe = DateTime.Now.AddYears(1),
                TrangThai = TrangThaiDocGia.HoatDong
            };

            _context.DocGia.Add(docGia);
            await _context.SaveChangesAsync();

            if (!await _userManager.IsInRoleAsync(identityUser, "DocGia"))
            {
                var roleResult = await _userManager.AddToRoleAsync(
                    identityUser,
                    "DocGia");
                if (!roleResult.Succeeded)
                {
                    var errors = string.Join(
                        "; ",
                        roleResult.Errors.Select(e => e.Description));
                    throw new InvalidOperationException(
                        $"Không thể khôi phục role Độc giả: {errors}");
                }
            }

            TempData["Success"] =
                "Hồ sơ độc giả đã được khôi phục. Hãy cập nhật thông tin còn thiếu.";
            return View("IndexDocGia", docGia);
        }

        public async Task<IActionResult> Edit()
        {
            var userId = _userManager.GetUserId(User);

            if (User.IsInRole("DocGia"))
            {
                var docGia = await _context.DocGia
                    .AsNoTracking()
                    .FirstOrDefaultAsync(d => d.UserId == userId);
                if (docGia == null)
                    return NotFound();

                return View("EditDocGia", ToProfileModel(docGia));
            }

            var nhanVien = await _context.NhanVien
                .AsNoTracking()
                .FirstOrDefaultAsync(n => n.UserId == userId);
            if (nhanVien == null)
                return NotFound();

            return View("EditNhanVien", ToProfileModel(nhanVien));
        }

        [Authorize(Roles = "DocGia")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditDocGia(
            ProfileEditViewModel model)
        {
            if (!ModelState.IsValid)
                return View("EditDocGia", model);

            var userId = _userManager.GetUserId(User);
            var docGia = await _context.DocGia
                .Include(d => d.User)
                .FirstOrDefaultAsync(d => d.UserId == userId);
            if (docGia == null || docGia.User == null)
                return NotFound();

            await using var transaction =
                await _context.Database.BeginTransactionAsync();
            if (!await UpdateIdentityAsync(docGia.User, model))
            {
                await transaction.RollbackAsync();
                return View("EditDocGia", model);
            }

            ApplyProfile(docGia, model);
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            TempData["Success"] = "Đã cập nhật thông tin cá nhân.";
            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = "Admin,NhanVien")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditNhanVien(
            ProfileEditViewModel model)
        {
            if (!ModelState.IsValid)
                return View("EditNhanVien", model);

            var userId = _userManager.GetUserId(User);
            var nhanVien = await _context.NhanVien
                .Include(n => n.User)
                .FirstOrDefaultAsync(n => n.UserId == userId);
            if (nhanVien == null || nhanVien.User == null)
                return NotFound();

            await using var transaction =
                await _context.Database.BeginTransactionAsync();
            if (!await UpdateIdentityAsync(nhanVien.User, model))
            {
                await transaction.RollbackAsync();
                return View("EditNhanVien", model);
            }

            ApplyProfile(nhanVien, model);
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            TempData["Success"] = "Đã cập nhật thông tin cá nhân.";
            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = "DocGia")]
        public async Task<IActionResult> LichSuMuon(
            string? trangThai,
            int page = 1)
        {
            await _phieuMuonService.CapNhatTrangThaiAsync();
            var userId = _userManager.GetUserId(User);
            var docGia = await _context.DocGia
                .AsNoTracking()
                .FirstOrDefaultAsync(d => d.UserId == userId);
            if (docGia == null)
                return NotFound();

            var query = _context.PhieuMuon
                .Include(p => p.ChiTietPhieuMuons)
                    .ThenInclude(ct => ct.BanSao)
                        .ThenInclude(b => b.Sach)
                .Include(p => p.ChiTietPhieuMuons)
                    .ThenInclude(ct => ct.PhieuPhat)
                .Where(p => p.MaDocGia == docGia.MaDocGia)
                .AsNoTracking()
                .AsQueryable();

            var homNay = DateTime.Today;
            ViewData["TongPhieuMuon"] = await query.CountAsync();
            ViewData["SoPhieuDangMuon"] = await query.CountAsync(p =>
                p.ChiTietPhieuMuons.Any(c => c.NgayTra == null) &&
                p.NgayHenTra >= homNay);
            ViewData["SoPhieuQuaHan"] = await query.CountAsync(p =>
                p.ChiTietPhieuMuons.Any(c => c.NgayTra == null) &&
                p.NgayHenTra < homNay);
            ViewData["TongSachDaMuon"] = await _context.ChiTietPhieuMuon
                .CountAsync(c => c.PhieuMuon.MaDocGia == docGia.MaDocGia);

            query = trangThai switch
            {
                "borrowing" => query.Where(p =>
                    p.ChiTietPhieuMuons.Any(c => c.NgayTra == null)),
                "fines" => query.Where(p =>
                    p.ChiTietPhieuMuons.Any(c =>
                        c.PhieuPhat != null &&
                        c.PhieuPhat.TrangThai == TrangThaiPhieuPhat.ChuaDong)),
                _ => query
            };

            var pagination = Pagination.Create(page, await query.CountAsync());
            ViewData["Pagination"] = pagination;
            ViewData["Status"] = trangThai;

            var danhSachPhieuMuon = await query
                .OrderByDescending(p => p.NgayMuon)
                .Skip((pagination.Page - 1) * pagination.PageSize)
                .Take(pagination.PageSize)
                .ToListAsync();

            return View(danhSachPhieuMuon);
        }

        [Authorize(Roles = "DocGia")]
        public async Task<IActionResult> PhieuPhatCuaToi(
            string? trangThai,
            int page = 1)
        {
            var userId = _userManager.GetUserId(User);
            var docGia = await _context.DocGia
                .AsNoTracking()
                .FirstOrDefaultAsync(d => d.UserId == userId);
            if (docGia == null)
                return NotFound();

            var query = _context.PhieuPhat
                .Include(p => p.ChiTietPhieuMuon)
                    .ThenInclude(c => c.BanSao)
                        .ThenInclude(b => b.Sach)
                .Include(p => p.ChiTietPhieuMuon)
                    .ThenInclude(c => c.PhieuMuon)
                .Where(p =>
                    p.ChiTietPhieuMuon.PhieuMuon.MaDocGia == docGia.MaDocGia)
                .AsNoTracking()
                .AsQueryable();

            ViewData["TongPhieuPhat"] = await query.CountAsync();
            ViewData["SoPhieuChuaDong"] = await query.CountAsync(p =>
                p.TrangThai == TrangThaiPhieuPhat.ChuaDong);
            ViewData["TongTienChuaDong"] = await query
                .Where(p => p.TrangThai == TrangThaiPhieuPhat.ChuaDong)
                .SumAsync(p => (decimal?)p.SoTien) ?? 0;

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

            var pagination = Pagination.Create(page, await query.CountAsync());
            ViewData["Pagination"] = pagination;
            ViewData["Status"] = trangThai;

            var danhSachPhieuPhat = await query
                .OrderByDescending(p => p.NgayLap)
                .ThenByDescending(p => p.MaPhieuPhat)
                .Skip((pagination.Page - 1) * pagination.PageSize)
                .Take(pagination.PageSize)
                .ToListAsync();

            return View(danhSachPhieuPhat);
        }

        private async Task<bool> UpdateIdentityAsync(
            IdentityUser user,
            ProfileEditViewModel model)
        {
            var email = model.Email.Trim().ToLowerInvariant();
            var accountUsingEmail =
                await _userManager.FindByEmailAsync(email);
            if (accountUsingEmail != null &&
                accountUsingEmail.Id != user.Id)
            {
                ModelState.AddModelError(nameof(model.Email),
                    "Email này đã được sử dụng.");
                return false;
            }

            var emailResult =
                await _userManager.SetEmailAsync(user, email);
            var userNameResult =
                await _userManager.SetUserNameAsync(user, email);
            var phoneResult = await _userManager.SetPhoneNumberAsync(
                user,
                model.SoDienThoai.Trim());

            foreach (var result in new[]
                     {
                         emailResult,
                         userNameResult,
                         phoneResult
                     })
            {
                foreach (var error in result.Errors)
                    ModelState.AddModelError(
                        string.Empty,
                        error.Description);
            }

            return emailResult.Succeeded &&
                   userNameResult.Succeeded &&
                   phoneResult.Succeeded;
        }

        private static ProfileEditViewModel ToProfileModel(DocGia docGia)
        {
            return new ProfileEditViewModel
            {
                HoTen = docGia.HoTen,
                NgaySinh = docGia.NgaySinh,
                GioiTinh = docGia.GioiTinh,
                DiaChi = docGia.DiaChi,
                SoDienThoai = docGia.SoDienThoai,
                Email = docGia.Email
            };
        }

        private static ProfileEditViewModel ToProfileModel(
            NhanVien nhanVien)
        {
            return new ProfileEditViewModel
            {
                HoTen = nhanVien.HoTen,
                NgaySinh = nhanVien.NgaySinh,
                GioiTinh = nhanVien.GioiTinh,
                DiaChi = nhanVien.DiaChi,
                SoDienThoai = nhanVien.SoDienThoai,
                Email = nhanVien.Email
            };
        }

        private static void ApplyProfile(
            DocGia docGia,
            ProfileEditViewModel model)
        {
            docGia.HoTen = model.HoTen.Trim();
            docGia.NgaySinh = model.NgaySinh;
            docGia.GioiTinh = model.GioiTinh.Trim();
            docGia.DiaChi = model.DiaChi.Trim();
            docGia.SoDienThoai = model.SoDienThoai.Trim();
            docGia.Email = model.Email.Trim().ToLowerInvariant();
        }

        private static void ApplyProfile(
            NhanVien nhanVien,
            ProfileEditViewModel model)
        {
            nhanVien.HoTen = model.HoTen.Trim();
            nhanVien.NgaySinh = model.NgaySinh;
            nhanVien.GioiTinh = model.GioiTinh.Trim();
            nhanVien.DiaChi = model.DiaChi.Trim();
            nhanVien.SoDienThoai = model.SoDienThoai.Trim();
            nhanVien.Email = model.Email.Trim().ToLowerInvariant();
        }
    }
}
