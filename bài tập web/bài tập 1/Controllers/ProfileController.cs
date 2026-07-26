using bài_tập_1.Data;
using bài_tập_1.Models;
using bài_tập_1.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace bài_tập_1.Controllers
{
    [Authorize]
    public class ProfileController : Controller
    {
        private readonly bài_tập_1Context _context;
        private readonly UserManager<IdentityUser> _userManager;

        public ProfileController(
            bài_tập_1Context context,
            UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User);

            if (User.IsInRole("DocGia"))
            {
                var docGia = await _context.DocGia
                    .AsNoTracking()
                    .FirstOrDefaultAsync(d => d.UserId == userId);
                return docGia == null
                    ? NotFound()
                    : View("IndexDocGia", docGia);
            }

            var nhanVien = await _context.NhanVien
                .AsNoTracking()
                .FirstOrDefaultAsync(n => n.UserId == userId);
            return nhanVien == null
                ? NotFound()
                : View("IndexNhanVien", nhanVien);
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
        public async Task<IActionResult> LichSuMuon()
        {
            var userId = _userManager.GetUserId(User);
            var docGia = await _context.DocGia
                .AsNoTracking()
                .FirstOrDefaultAsync(d => d.UserId == userId);
            if (docGia == null)
                return NotFound();

            var danhSachPhieuMuon = await _context.PhieuMuon
                .Include(p => p.ChiTietPhieuMuons)
                    .ThenInclude(ct => ct.BanSao)
                        .ThenInclude(b => b.Sach)
                .Where(p => p.MaDocGia == docGia.MaDocGia)
                .OrderByDescending(p => p.NgayMuon)
                .AsNoTracking()
                .ToListAsync();

            return View(danhSachPhieuMuon);
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
