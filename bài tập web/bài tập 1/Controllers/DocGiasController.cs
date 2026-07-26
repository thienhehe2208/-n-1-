using bài_tập_1.Data;
using bài_tập_1.Models;
using bài_tập_1.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace bài_tập_1.Controllers
{
    [Authorize(Roles = "Admin,NhanVien")]
    public class DocGiasController : Controller
    {
        private readonly bài_tập_1Context _context;
        private readonly UserManager<IdentityUser> _userManager;

        public DocGiasController(
            bài_tập_1Context context,
            UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            return View(await _context.DocGia
                .Include(d => d.User)
                .AsNoTracking()
                .ToListAsync());
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return NotFound();

            var docGia = await _context.DocGia
                .Include(d => d.User)
                .AsNoTracking()
                .FirstOrDefaultAsync(d => d.MaDocGia == id);

            return docGia == null ? NotFound() : View(docGia);
        }

        public IActionResult Create()
        {
            return View(new CreateDocGiaViewModel
            {
                NgayHetHanThe = DateTime.Today.AddYears(1)
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateDocGiaViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var email = model.Email.Trim().ToLowerInvariant();
            if (await _userManager.FindByEmailAsync(email) != null)
            {
                ModelState.AddModelError(nameof(model.Email),
                    "Email này đã được sử dụng.");
                return View(model);
            }

            await using var transaction =
                await _context.Database.BeginTransactionAsync();

            var user = new IdentityUser
            {
                UserName = email,
                Email = email,
                PhoneNumber = model.SoDienThoai.Trim(),
                EmailConfirmed = true
            };

            var createResult =
                await _userManager.CreateAsync(user, model.Password);
            if (!createResult.Succeeded)
            {
                AddIdentityErrors(createResult);
                await transaction.RollbackAsync();
                return View(model);
            }

            var roleResult =
                await _userManager.AddToRoleAsync(user, "DocGia");
            if (!roleResult.Succeeded)
            {
                AddIdentityErrors(roleResult);
                await transaction.RollbackAsync();
                return View(model);
            }

            try
            {
                _context.DocGia.Add(new DocGia
                {
                    UserId = user.Id,
                    HoTen = model.HoTen.Trim(),
                    NgaySinh = model.NgaySinh,
                    GioiTinh = model.GioiTinh.Trim(),
                    DiaChi = model.DiaChi.Trim(),
                    SoDienThoai = model.SoDienThoai.Trim(),
                    Email = email,
                    NgayDangKy = DateTime.Now,
                    NgayHetHanThe = model.NgayHetHanThe,
                    TrangThai = model.TrangThai
                });

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                TempData["Success"] =
                    "Đã tạo tài khoản và hồ sơ độc giả.";
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateException)
            {
                await transaction.RollbackAsync();
                ModelState.AddModelError(string.Empty,
                    "Không thể lưu hồ sơ độc giả. Vui lòng thử lại.");
                return View(model);
            }
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var docGia = await _context.DocGia
                .AsNoTracking()
                .FirstOrDefaultAsync(d => d.MaDocGia == id);
            if (docGia == null)
                return NotFound();

            return View(new EditDocGiaViewModel
            {
                MaDocGia = docGia.MaDocGia,
                HoTen = docGia.HoTen,
                NgaySinh = docGia.NgaySinh,
                GioiTinh = docGia.GioiTinh,
                DiaChi = docGia.DiaChi,
                SoDienThoai = docGia.SoDienThoai,
                Email = docGia.Email,
                NgayDangKy = docGia.NgayDangKy,
                NgayHetHanThe = docGia.NgayHetHanThe,
                TrangThai = docGia.TrangThai
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            EditDocGiaViewModel model)
        {
            if (id != model.MaDocGia)
                return NotFound();
            if (!ModelState.IsValid)
                return View(model);

            var docGia = await _context.DocGia
                .Include(d => d.User)
                .FirstOrDefaultAsync(d => d.MaDocGia == id);
            if (docGia == null || docGia.User == null)
                return NotFound();

            var email = model.Email.Trim().ToLowerInvariant();
            var accountUsingEmail =
                await _userManager.FindByEmailAsync(email);
            if (accountUsingEmail != null &&
                accountUsingEmail.Id != docGia.UserId)
            {
                ModelState.AddModelError(nameof(model.Email),
                    "Email này đã được sử dụng.");
                return View(model);
            }

            await using var transaction =
                await _context.Database.BeginTransactionAsync();

            var emailResult =
                await _userManager.SetEmailAsync(docGia.User, email);
            var userNameResult =
                await _userManager.SetUserNameAsync(docGia.User, email);
            var phoneResult = await _userManager.SetPhoneNumberAsync(
                docGia.User,
                model.SoDienThoai.Trim());

            if (!emailResult.Succeeded ||
                !userNameResult.Succeeded ||
                !phoneResult.Succeeded)
            {
                AddIdentityErrors(emailResult);
                AddIdentityErrors(userNameResult);
                AddIdentityErrors(phoneResult);
                await transaction.RollbackAsync();
                return View(model);
            }

            docGia.HoTen = model.HoTen.Trim();
            docGia.NgaySinh = model.NgaySinh;
            docGia.GioiTinh = model.GioiTinh.Trim();
            docGia.DiaChi = model.DiaChi.Trim();
            docGia.SoDienThoai = model.SoDienThoai.Trim();
            docGia.Email = email;
            docGia.NgayDangKy = model.NgayDangKy;
            docGia.NgayHetHanThe = model.NgayHetHanThe;
            docGia.TrangThai = model.TrangThai;

            try
            {
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                TempData["Success"] =
                    "Đã cập nhật tài khoản và hồ sơ độc giả.";
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateConcurrencyException)
            {
                await transaction.RollbackAsync();
                if (!await _context.DocGia
                        .AnyAsync(d => d.MaDocGia == id))
                    return NotFound();

                ModelState.AddModelError(string.Empty,
                    "Dữ liệu đã được thay đổi. Vui lòng tải lại trang.");
                return View(model);
            }
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
                return NotFound();

            var docGia = await _context.DocGia
                .Include(d => d.User)
                .AsNoTracking()
                .FirstOrDefaultAsync(d => d.MaDocGia == id);

            return docGia == null ? NotFound() : View(docGia);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var docGia = await _context.DocGia
                .Include(d => d.User)
                .FirstOrDefaultAsync(d => d.MaDocGia == id);
            if (docGia == null)
                return NotFound();

            await using var transaction =
                await _context.Database.BeginTransactionAsync();

            try
            {
                var identityUser = docGia.User;
                _context.DocGia.Remove(docGia);
                await _context.SaveChangesAsync();

                if (identityUser != null)
                {
                    var deleteResult =
                        await _userManager.DeleteAsync(identityUser);
                    if (!deleteResult.Succeeded)
                    {
                        await transaction.RollbackAsync();
                        TempData["Error"] =
                            "Không thể xóa tài khoản đăng nhập của độc giả.";
                        return RedirectToAction(nameof(Index));
                    }
                }

                await transaction.CommitAsync();
                TempData["Success"] =
                    "Đã xóa hồ sơ và tài khoản độc giả.";
            }
            catch (DbUpdateException)
            {
                await transaction.RollbackAsync();
                TempData["Error"] =
                    "Không thể xóa độc giả đã phát sinh mượn hoặc đặt trước.";
            }

            return RedirectToAction(nameof(Index));
        }

        private void AddIdentityErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
                ModelState.AddModelError(string.Empty, error.Description);
        }
    }
}
