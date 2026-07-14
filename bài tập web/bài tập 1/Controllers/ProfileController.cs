using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using bài_tập_1.Data;
using bài_tập_1.Models;

namespace bài_tập_1.Controllers
{
    // Trang cá nhân - chỉ cần đăng nhập, mỗi người chỉ thấy/sửa được đúng hồ sơ của mình
    [Authorize]
    public class ProfileController : Controller
    {
        private readonly bài_tập_1Context _context;
        private readonly UserManager<IdentityUser> _userManager;

        public ProfileController(bài_tập_1Context context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // Xem thông tin cá nhân
        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User);

            if (User.IsInRole("DocGia"))
            {
                var docGia = await _context.DocGia.FirstOrDefaultAsync(d => d.UserId == userId);
                if (docGia == null) return NotFound();
                return View("IndexDocGia", docGia);
            }

            // Admin/NhanVien
            var nhanVien = await _context.NhanVien.FirstOrDefaultAsync(n => n.UserId == userId);
            if (nhanVien == null) return NotFound();
            return View("IndexNhanVien", nhanVien);
        }

        // Hiển thị form sửa thông tin cá nhân
        public async Task<IActionResult> Edit()
        {
            var userId = _userManager.GetUserId(User);

            if (User.IsInRole("DocGia"))
            {
                var docGia = await _context.DocGia.FirstOrDefaultAsync(d => d.UserId == userId);
                if (docGia == null) return NotFound();
                return View("EditDocGia", docGia);
            }

            var nhanVien = await _context.NhanVien.FirstOrDefaultAsync(n => n.UserId == userId);
            if (nhanVien == null) return NotFound();
            return View("EditNhanVien", nhanVien);
        }

        // Xử lý cập nhật thông tin cá nhân của độc giả
        // Chỉ cho sửa field liên hệ - không cho tự sửa NgayHetHanThe, TrangThai, UserId
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditDocGia([Bind("HoTen,NgaySinh,GioiTinh,DiaChi,SoDienThoai,Email")] DocGia input)
        {
            var userId = _userManager.GetUserId(User);
            var docGia = await _context.DocGia.FirstOrDefaultAsync(d => d.UserId == userId);
            if (docGia == null) return NotFound();

            if (ModelState.IsValid)
            {
                docGia.HoTen = input.HoTen;
                docGia.NgaySinh = input.NgaySinh;
                docGia.GioiTinh = input.GioiTinh;
                docGia.DiaChi = input.DiaChi;
                docGia.SoDienThoai = input.SoDienThoai;
                docGia.Email = input.Email;

                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            return View("EditDocGia", docGia);
        }

        // Xử lý cập nhật thông tin cá nhân của nhân viên/admin
        // Chỉ cho sửa field liên hệ - không cho tự sửa ChucVu, UserId
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditNhanVien([Bind("HoTen,NgaySinh,GioiTinh,DiaChi,SoDienThoai,Email")] NhanVien input)
        {
            var userId = _userManager.GetUserId(User);
            var nhanVien = await _context.NhanVien.FirstOrDefaultAsync(n => n.UserId == userId);
            if (nhanVien == null) return NotFound();

            if (ModelState.IsValid)
            {
                nhanVien.HoTen = input.HoTen;
                nhanVien.NgaySinh = input.NgaySinh;
                nhanVien.GioiTinh = input.GioiTinh;
                nhanVien.DiaChi = input.DiaChi;
                nhanVien.SoDienThoai = input.SoDienThoai;
                nhanVien.Email = input.Email;

                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            return View("EditNhanVien", nhanVien);
        }

        // Lịch sử mượn sách của chính độc giả đang đăng nhập
        [Authorize(Roles = "DocGia")]
        public async Task<IActionResult> LichSuMuon()
        {
            var userId = _userManager.GetUserId(User);
            var docGia = await _context.DocGia.FirstOrDefaultAsync(d => d.UserId == userId);
            if (docGia == null) return NotFound();

            var danhSachPhieuMuon = await _context.PhieuMuon
                .Include(p => p.ChiTietPhieuMuons)
                    .ThenInclude(ct => ct.BanSao)
                        .ThenInclude(b => b.Sach)
                .Where(p => p.MaDocGia == docGia.MaDocGia)
                .OrderByDescending(p => p.NgayMuon)
                .ToListAsync();

            return View(danhSachPhieuMuon);
        }
    }
}