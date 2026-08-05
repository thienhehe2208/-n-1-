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

        public async Task<IActionResult> Index(string? q, string? trangThai, int page = 1)
        {
            var baseQuery = _context.DocGia
                .Include(d => d.User)
                .AsNoTracking()
                .AsQueryable();

            ViewData["TongDocGia"] = await baseQuery.CountAsync();
            ViewData["DangHoatDong"] = await baseQuery.CountAsync(
                d => d.TrangThai == TrangThaiDocGia.HoatDong);
            ViewData["BiKhoa"] = await baseQuery.CountAsync(
                d => d.TrangThai == TrangThaiDocGia.Khoa);
            ViewData["SapHetHan"] = await baseQuery.CountAsync(d =>
                d.NgayHetHanThe >= DateTime.Today &&
                d.NgayHetHanThe <= DateTime.Today.AddDays(30));

            if (!string.IsNullOrWhiteSpace(q))
            {
                var keyword = q.Trim();
                baseQuery = baseQuery.Where(d =>
                    d.HoTen.Contains(keyword) ||
                    d.Email.Contains(keyword) ||
                    d.SoDienThoai.Contains(keyword));
            }

            baseQuery = trangThai switch
            {
                "active" => baseQuery.Where(d =>
                    d.TrangThai == TrangThaiDocGia.HoatDong),
                "locked" => baseQuery.Where(d =>
                    d.TrangThai == TrangThaiDocGia.Khoa),
                "expiring" => baseQuery.Where(d =>
                    d.NgayHetHanThe >= DateTime.Today &&
                    d.NgayHetHanThe <= DateTime.Today.AddDays(30)),
                _ => baseQuery
            };

            ViewData["Search"] = q;
            ViewData["Status"] = trangThai;
            var pagination = Pagination.Create(page, await baseQuery.CountAsync());
            ViewData["Pagination"] = pagination;
            return View(await baseQuery.OrderBy(d => d.HoTen)
                .Skip((pagination.Page - 1) * pagination.PageSize)
                .Take(pagination.PageSize)
                .ToListAsync());
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return NotFound();

            var docGia = await _context.DocGia
                .Include(d => d.User)
                .Include(d => d.PhieuMuons)
                    .ThenInclude(p => p.ChiTietPhieuMuons)
                        .ThenInclude(c => c.BanSao)
                            .ThenInclude(b => b.Sach)
                .Include(d => d.PhieuMuons)
                    .ThenInclude(p => p.ChiTietPhieuMuons)
                        .ThenInclude(c => c.PhieuPhat)
                .Include(d => d.DatTruocs)
                    .ThenInclude(d => d.Sach)
                .AsNoTracking()
                .FirstOrDefaultAsync(d => d.MaDocGia == id);

            if (docGia == null)
                return NotFound();

            var loanItems = docGia.PhieuMuons
                .SelectMany(p => p.ChiTietPhieuMuons.Select(c => new
                {
                    Phieu = p,
                    ChiTiet = c
                }))
                .ToList();

            var model = new DocGiaDetailsViewModel
            {
                DocGia = docGia,
                TongLuotMuon = docGia.PhieuMuons.Count(p => p.TrangThai != TrangThaiPhieuMuon.Nhap),
                SachDangMuon = loanItems.Count(x => x.ChiTiet.NgayTra == null && x.Phieu.TrangThai != TrangThaiPhieuMuon.Nhap),
                SachQuaHan = loanItems.Count(x => x.ChiTiet.NgayTra == null && x.Phieu.NgayHenTra.Date < DateTime.Today),
                DatTruocDangCho = docGia.DatTruocs.Count(d =>
                    d.TrangThai == TrangThaiDatTruoc.DangCho || d.TrangThai == TrangThaiDatTruoc.DaCoSach),
                TienPhatChuaDong = loanItems
                    .Where(x => x.ChiTiet.PhieuPhat?.TrangThai == TrangThaiPhieuPhat.ChuaDong)
                    .Sum(x => x.ChiTiet.PhieuPhat?.SoTien ?? 0),
                MuonGanDay = loanItems
                    .Where(x => x.Phieu.TrangThai != TrangThaiPhieuMuon.Nhap)
                    .OrderByDescending(x => x.Phieu.NgayMuon)
                    .Take(6)
                    .Select(x => new DocGiaLoanItemViewModel
                    {
                        MaPhieuMuon = x.Phieu.MaPhieuMuon,
                        TenSach = x.ChiTiet.BanSao?.Sach?.TenSach ?? "Sách không còn thông tin",
                        MaVach = x.ChiTiet.BanSao?.MaVach ?? "—",
                        NgayMuon = x.Phieu.NgayMuon,
                        NgayHenTra = x.Phieu.NgayHenTra,
                        NgayTra = x.ChiTiet.NgayTra,
                        QuaHan = x.ChiTiet.NgayTra == null && x.Phieu.NgayHenTra.Date < DateTime.Today
                    }).ToList(),
                DatTruocGanDay = docGia.DatTruocs
                    .OrderByDescending(d => d.NgayDat)
                    .Take(5)
                    .Select(d => new DocGiaReservationItemViewModel
                    {
                        MaDatTruoc = d.MaDatTruoc,
                        TenSach = d.Sach?.TenSach ?? "Sách không còn thông tin",
                        NgayDat = d.NgayDat,
                        TrangThai = d.TrangThai
                    }).ToList()
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleStatus(int id)
        {
            var docGia = await _context.DocGia.Include(d => d.User)
                .FirstOrDefaultAsync(d => d.MaDocGia == id);
            if (docGia == null || docGia.User == null)
                return NotFound();

            var lockAccount = docGia.TrangThai == TrangThaiDocGia.HoatDong;
            var result = await _userManager.SetLockoutEndDateAsync(
                docGia.User, lockAccount ? DateTimeOffset.MaxValue : null);
            if (!result.Succeeded)
            {
                TempData["Error"] = "Không thể cập nhật trạng thái tài khoản. Vui lòng thử lại.";
                return RedirectToAction(nameof(Details), new { id });
            }

            docGia.TrangThai = lockAccount ? TrangThaiDocGia.Khoa : TrangThaiDocGia.HoatDong;
            await _context.SaveChangesAsync();
            TempData["Success"] = lockAccount ? "Đã khóa tài khoản độc giả." : "Đã mở khóa tài khoản độc giả.";
            return RedirectToAction(nameof(Details), new { id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RenewCard(int id)
        {
            var docGia = await _context.DocGia.FindAsync(id);
            if (docGia == null)
                return NotFound();

            var startDate = docGia.NgayHetHanThe.Date > DateTime.Today
                ? docGia.NgayHetHanThe.Date
                : DateTime.Today;
            docGia.NgayHetHanThe = startDate.AddYears(1);
            await _context.SaveChangesAsync();
            TempData["Success"] = $"Đã gia hạn thẻ đến {docGia.NgayHetHanThe:dd/MM/yyyy}.";
            return RedirectToAction(nameof(Details), new { id });
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
                EmailConfirmed = true,
                LockoutEnabled = true,
                LockoutEnd = model.TrangThai == TrangThaiDocGia.Khoa
                    ? DateTimeOffset.MaxValue
                    : null
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
            var lockoutEnabledResult = await _userManager.SetLockoutEnabledAsync(
                docGia.User,
                true);
            var lockoutResult = await _userManager.SetLockoutEndDateAsync(
                docGia.User,
                model.TrangThai == TrangThaiDocGia.Khoa
                    ? DateTimeOffset.MaxValue
                    : null);

            if (!emailResult.Succeeded ||
                !userNameResult.Succeeded ||
                !phoneResult.Succeeded ||
                !lockoutEnabledResult.Succeeded ||
                !lockoutResult.Succeeded)
            {
                AddIdentityErrors(emailResult);
                AddIdentityErrors(userNameResult);
                AddIdentityErrors(phoneResult);
                AddIdentityErrors(lockoutEnabledResult);
                AddIdentityErrors(lockoutResult);
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

            var coYeuCauMuonOnline = await _context.YeuCauMuonOnline
                .AnyAsync(y => y.MaDocGia == id);
            if (coYeuCauMuonOnline)
            {
                TempData["Error"] =
                    "Không thể xóa độc giả đã có yêu cầu mượn online. " +
                    "Hãy khóa tài khoản để giữ nguyên lịch sử nghiệp vụ.";
                return RedirectToAction(nameof(Index));
            }

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
