using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using bài_tập_1.Data;
using bài_tập_1.Models;
using bài_tập_1.Models.ViewModels;
namespace bài_tập_1.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly bài_tập_1Context _context;

        public AccountController(UserManager<IdentityUser> userManager,
                                  SignInManager<IdentityUser> signInManager,
                                  bài_tập_1Context context)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _context = context;
        }

        // GET: /Account/Register
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        // POST: /Account/Register
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var email = model.Email.Trim().ToLowerInvariant();
            if (await _userManager.FindByEmailAsync(email) != null)
            {
                ModelState.AddModelError(nameof(model.Email), "Email này đã được đăng ký.");
                return View(model);
            }

            await using var transaction = await _context.Database.BeginTransactionAsync();

            var user = new IdentityUser
            {
                UserName = email,
                Email = email,
                PhoneNumber = model.SoDienThoai.Trim(),
                EmailConfirmed = true
            };

            var createUserResult = await _userManager.CreateAsync(user, model.Password);

            if (!createUserResult.Succeeded)
            {
                foreach (var error in createUserResult.Errors)
                    ModelState.AddModelError(string.Empty, error.Description);

                await transaction.RollbackAsync();
                return View(model);
            }

            try
            {
                var addRoleResult = await _userManager.AddToRoleAsync(user, "DocGia");
                if (!addRoleResult.Succeeded)
                {
                    var message = string.Join("; ", addRoleResult.Errors.Select(e => e.Description));
                    throw new InvalidOperationException($"Không thể gán quyền Độc giả: {message}");
                }

                var docGia = new DocGia
                {
                    UserId = user.Id,
                    HoTen = model.HoTen.Trim(),
                    SoDienThoai = model.SoDienThoai.Trim(),
                    Email = email,
                    DiaChi = string.Empty,
                    GioiTinh = string.Empty,
                    NgayDangKy = DateTime.Now,
                    NgayHetHanThe = DateTime.Now.AddYears(1),
                    TrangThai = TrangThaiDocGia.HoatDong
                };
                _context.DocGia.Add(docGia);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                TempData["RegisterSuccess"] = "Đăng ký thành công. Bạn có thể đăng nhập bằng tài khoản vừa tạo.";
                return RedirectToAction(nameof(Login));
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                ModelState.AddModelError(string.Empty, "Không thể lưu tài khoản độc giả. Vui lòng thử lại.");
                return View(model);
            }
        }

        // GET: /Account/Login
        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            return View(new LoginViewModel { ReturnUrl = returnUrl });
        }

        // POST: /Account/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var result = await _signInManager.PasswordSignInAsync(
                model.Email, model.Password, model.RememberMe, lockoutOnFailure: false);

            if (result.Succeeded)
            {
                var user = await _userManager.FindByEmailAsync(model.Email);
                var roles = await _userManager.GetRolesAsync(user);

                if (roles.Contains("Admin") || roles.Contains("NhanVien"))
                    return RedirectToAction("Index", "Dashboard");

                if (!string.IsNullOrEmpty(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
                    return Redirect(model.ReturnUrl);

                return RedirectToAction("Index", "Home");
            }

            ModelState.AddModelError(string.Empty, "Email hoặc mật khẩu không đúng.");
            return View(model);
        }

        // POST: /Account/Logout
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }

        // GET: /Account/AccessDenied
        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}
