using bài_tập_1.Data;
using bài_tập_1.Models;
using bài_tập_1.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace bài_tập_1.Controllers
{
    [Authorize(Roles = "Admin")]
    public class NhanViensController : Controller
    {
        private readonly bài_tập_1Context _context;
        private readonly UserManager<IdentityUser> _userManager;

        public NhanViensController(
            bài_tập_1Context context,
            UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index(string? q, int page = 1)
        {
            var query = _context.NhanVien
                .Include(n => n.User)
                .AsNoTracking()
                .AsQueryable();

            ViewData["TongNhanVien"] = await query.CountAsync();
            ViewData["ChucVu"] = await query
                .Where(n => n.ChucVu != "")
                .Select(n => n.ChucVu)
                .Distinct()
                .CountAsync();
            ViewData["MoiTrongNam"] = await query.CountAsync(n =>
                n.NgayVaoLam.Year == DateTime.Today.Year);

            if (!string.IsNullOrWhiteSpace(q))
            {
                var keyword = q.Trim();
                query = query.Where(n =>
                    n.HoTen.Contains(keyword) ||
                    n.Email.Contains(keyword) ||
                    n.SoDienThoai.Contains(keyword) ||
                    n.ChucVu.Contains(keyword));
            }

            ViewData["Search"] = q;
            var pagination = Pagination.Create(page, await query.CountAsync());
            ViewData["Pagination"] = pagination;
            var nhanViens = await query.OrderBy(n => n.HoTen)
                .Skip((pagination.Page - 1) * pagination.PageSize)
                .Take(pagination.PageSize)
                .ToListAsync();

            return View(nhanViens);
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return NotFound();

            var nhanVien = await _context.NhanVien
                .Include(n => n.User)
                .AsNoTracking()
                .FirstOrDefaultAsync(n => n.MaNhanVien == id);

            if (nhanVien == null)
                return NotFound();

            ViewData["TongPhieuMuon"] = await _context.PhieuMuon
                .CountAsync(p => p.MaNhanVien == nhanVien.MaNhanVien);
            ViewData["PhieuMuonThangNay"] = await _context.PhieuMuon.CountAsync(p =>
                p.MaNhanVien == nhanVien.MaNhanVien &&
                p.NgayMuon.Year == DateTime.Today.Year &&
                p.NgayMuon.Month == DateTime.Today.Month);
            ViewData["TaiKhoanHoatDong"] = nhanVien.User?.LockoutEnd == null ||
                nhanVien.User.LockoutEnd <= DateTimeOffset.Now;

            return View(nhanVien);
        }

        public IActionResult Create()
        {
            return View(new CreateNhanVienViewModel
            {
                NgayVaoLam = DateTime.Today
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateNhanVienViewModel model)
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

            var createUserResult =
                await _userManager.CreateAsync(user, model.Password);

            if (!createUserResult.Succeeded)
            {
                AddIdentityErrors(createUserResult);
                await transaction.RollbackAsync();
                return View(model);
            }

            var addRoleResult =
                await _userManager.AddToRoleAsync(user, "NhanVien");

            if (!addRoleResult.Succeeded)
            {
                AddIdentityErrors(addRoleResult);
                await transaction.RollbackAsync();
                return View(model);
            }

            try
            {
                var nhanVien = new NhanVien
                {
                    UserId = user.Id,
                    HoTen = model.HoTen.Trim(),
                    NgaySinh = model.NgaySinh,
                    GioiTinh = model.GioiTinh.Trim(),
                    DiaChi = model.DiaChi.Trim(),
                    SoDienThoai = model.SoDienThoai.Trim(),
                    Email = email,
                    ChucVu = model.ChucVu.Trim(),
                    NgayVaoLam = model.NgayVaoLam
                };

                _context.NhanVien.Add(nhanVien);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                TempData["Success"] =
                    "Đã tạo tài khoản và hồ sơ nhân viên.";
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateException)
            {
                await transaction.RollbackAsync();
                ModelState.AddModelError(string.Empty,
                    "Không thể lưu hồ sơ nhân viên. Vui lòng thử lại.");
                return View(model);
            }
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var nhanVien = await _context.NhanVien
                .AsNoTracking()
                .FirstOrDefaultAsync(n => n.MaNhanVien == id);

            if (nhanVien == null)
                return NotFound();

            return View(new EditNhanVienViewModel
            {
                MaNhanVien = nhanVien.MaNhanVien,
                HoTen = nhanVien.HoTen,
                NgaySinh = nhanVien.NgaySinh,
                GioiTinh = nhanVien.GioiTinh,
                DiaChi = nhanVien.DiaChi,
                SoDienThoai = nhanVien.SoDienThoai,
                Email = nhanVien.Email,
                ChucVu = nhanVien.ChucVu,
                NgayVaoLam = nhanVien.NgayVaoLam
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            EditNhanVienViewModel model)
        {
            if (id != model.MaNhanVien)
                return NotFound();

            if (!ModelState.IsValid)
                return View(model);

            var nhanVien = await _context.NhanVien
                .Include(n => n.User)
                .FirstOrDefaultAsync(n => n.MaNhanVien == id);

            if (nhanVien == null || nhanVien.User == null)
                return NotFound();

            var email = model.Email.Trim().ToLowerInvariant();
            var laTaiKhoanAdmin = await _userManager.IsInRoleAsync(
                nhanVien.User,
                "Admin");
            if (laTaiKhoanAdmin &&
                !string.Equals(
                    email,
                    nhanVien.User.Email,
                    StringComparison.OrdinalIgnoreCase))
            {
                ModelState.AddModelError(
                    nameof(model.Email),
                    "Không thể đổi email của tài khoản Admin cấu hình.");
                return View(model);
            }

            var accountUsingEmail =
                await _userManager.FindByEmailAsync(email);

            if (accountUsingEmail != null &&
                accountUsingEmail.Id != nhanVien.UserId)
            {
                ModelState.AddModelError(nameof(model.Email),
                    "Email này đã được sử dụng.");
                return View(model);
            }

            await using var transaction =
                await _context.Database.BeginTransactionAsync();

            var setEmailResult =
                await _userManager.SetEmailAsync(nhanVien.User, email);
            if (!setEmailResult.Succeeded)
            {
                AddIdentityErrors(setEmailResult);
                await transaction.RollbackAsync();
                return View(model);
            }

            var setUserNameResult =
                await _userManager.SetUserNameAsync(nhanVien.User, email);
            if (!setUserNameResult.Succeeded)
            {
                AddIdentityErrors(setUserNameResult);
                await transaction.RollbackAsync();
                return View(model);
            }

            var setPhoneResult = await _userManager.SetPhoneNumberAsync(
                nhanVien.User,
                model.SoDienThoai.Trim());
            if (!setPhoneResult.Succeeded)
            {
                AddIdentityErrors(setPhoneResult);
                await transaction.RollbackAsync();
                return View(model);
            }

            nhanVien.HoTen = model.HoTen.Trim();
            nhanVien.NgaySinh = model.NgaySinh;
            nhanVien.GioiTinh = model.GioiTinh.Trim();
            nhanVien.DiaChi = model.DiaChi.Trim();
            nhanVien.SoDienThoai = model.SoDienThoai.Trim();
            nhanVien.Email = email;
            nhanVien.ChucVu = model.ChucVu.Trim();
            nhanVien.NgayVaoLam = model.NgayVaoLam;

            try
            {
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                TempData["Success"] =
                    "Đã cập nhật tài khoản và hồ sơ nhân viên.";
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateConcurrencyException)
            {
                await transaction.RollbackAsync();
                if (!await _context.NhanVien
                        .AnyAsync(n => n.MaNhanVien == id))
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

            var nhanVien = await _context.NhanVien
                .Include(n => n.User)
                .AsNoTracking()
                .FirstOrDefaultAsync(n => n.MaNhanVien == id);

            return nhanVien == null ? NotFound() : View(nhanVien);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var nhanVien = await _context.NhanVien
                .Include(n => n.User)
                .FirstOrDefaultAsync(n => n.MaNhanVien == id);

            if (nhanVien == null)
                return NotFound();

            if (nhanVien.UserId == _userManager.GetUserId(User))
            {
                TempData["Error"] =
                    "Bạn không thể xóa tài khoản đang đăng nhập.";
                return RedirectToAction(nameof(Index));
            }

            if (nhanVien.User != null &&
                await _userManager.IsInRoleAsync(nhanVien.User, "Admin"))
            {
                TempData["Error"] =
                    "Tài khoản Admin duy nhất không thể bị xóa.";
                return RedirectToAction(nameof(Index));
            }

            var daPhatSinhNghiepVu =
                await _context.PhieuMuon.AnyAsync(p => p.MaNhanVien == id) ||
                await _context.GiaoDichThanhToan.AnyAsync(g =>
                    g.MaNhanVienXacNhan == id);
            if (daPhatSinhNghiepVu)
            {
                TempData["Error"] =
                    "Không thể xóa nhân viên đã phát sinh phiếu mượn hoặc giao dịch thanh toán.";
                return RedirectToAction(nameof(Index));
            }

            await using var transaction =
                await _context.Database.BeginTransactionAsync();

            try
            {
                var identityUser = nhanVien.User;
                _context.NhanVien.Remove(nhanVien);
                await _context.SaveChangesAsync();

                if (identityUser != null)
                {
                    var deleteUserResult =
                        await _userManager.DeleteAsync(identityUser);
                    if (!deleteUserResult.Succeeded)
                    {
                        AddIdentityErrors(deleteUserResult);
                        await transaction.RollbackAsync();
                        TempData["Error"] =
                            "Không thể xóa tài khoản đăng nhập của nhân viên.";
                        return RedirectToAction(nameof(Index));
                    }
                }

                await transaction.CommitAsync();
                TempData["Success"] =
                    "Đã xóa hồ sơ và tài khoản nhân viên.";
            }
            catch (DbUpdateException)
            {
                await transaction.RollbackAsync();
                TempData["Error"] =
                    "Không thể xóa nhân viên đã phát sinh dữ liệu nghiệp vụ.";
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
