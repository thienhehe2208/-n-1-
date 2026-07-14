using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using bài_tập_1.Data;
using bài_tập_1.Models;

namespace bài_tập_1.Controllers
{
    // Chỉ cần đăng nhập là được vào Controller (không khóa ở class),
    // vì Create cho phép cả DocGia, còn các action khác chỉ Admin/NhanVien
    [Authorize]
    public class DatTruocsController : Controller
    {
        private readonly bài_tập_1Context _context;
        private readonly UserManager<IdentityUser> _userManager;

        public DatTruocsController(bài_tập_1Context context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // Danh sách toàn bộ đặt trước - chỉ Admin/NhanVien duyệt
        [Authorize(Roles = "Admin,NhanVien")]
        public async Task<IActionResult> Index()
        {
            var bài_tập_1Context = _context.DatTruoc.Include(d => d.DocGia).Include(d => d.Sach);
            return View(await bài_tập_1Context.ToListAsync());
        }

        // Xem chi tiết 1 đặt trước - chỉ Admin/NhanVien
        [Authorize(Roles = "Admin,NhanVien")]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.DatTruoc == null)
            {
                return NotFound();
            }

            var datTruoc = await _context.DatTruoc
                .Include(d => d.DocGia)
                .Include(d => d.Sach)
                .FirstOrDefaultAsync(m => m.MaDatTruoc == id);
            if (datTruoc == null)
            {
                return NotFound();
            }

            return View(datTruoc);
        }

        // Hiển thị form đặt trước - độc giả tự đặt được, nhân viên cũng đặt hộ được
        [Authorize(Roles = "Admin,NhanVien,DocGia")]
        public IActionResult Create()
        {
            bool isStaff = User.IsInRole("Admin") || User.IsInRole("NhanVien");
            ViewBag.IsStaff = isStaff;

            if (isStaff)
            {
                // Nhân viên/Admin được chọn đặt hộ cho bất kỳ độc giả nào
                ViewData["MaDocGia"] = new SelectList(_context.DocGia, "MaDocGia", "HoTen");
            }
            // Nếu là độc giả tự đặt, không cần dropdown - View sẽ tự ẩn dựa vào ViewBag.IsStaff

            ViewData["MaSach"] = new SelectList(_context.Sach, "MaSach", "TenSach");
            return View();
        }

        // Xử lý lưu đặt trước mới
        [Authorize(Roles = "Admin,NhanVien,DocGia")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("MaDatTruoc,MaDocGia,MaSach,NgayDat,NgayHetHanDat,TrangThai")] DatTruoc datTruoc)
        {
            bool isStaff = User.IsInRole("Admin") || User.IsInRole("NhanVien");

            if (!isStaff)
            {
                // Độc giả tự đặt: bỏ qua MaDocGia người dùng gửi lên (nếu có),
                // luôn lấy đúng hồ sơ của chính người đang đăng nhập -> chống giả mạo đặt hộ người khác
                var userId = _userManager.GetUserId(User);
                var docGia = await _context.DocGia.FirstOrDefaultAsync(d => d.UserId == userId);
                if (docGia == null)
                {
                    return Forbid();
                }
                datTruoc.MaDocGia = docGia.MaDocGia;
                ModelState.Remove(nameof(datTruoc.MaDocGia));
            }

            datTruoc.NgayDat = DateTime.Now;
            datTruoc.TrangThai = TrangThaiDatTruoc.DangCho;
            ModelState.Remove(nameof(datTruoc.TrangThai));

            if (ModelState.IsValid)
            {
                _context.Add(datTruoc);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            ViewBag.IsStaff = isStaff;
            if (isStaff)
            {
                ViewData["MaDocGia"] = new SelectList(_context.DocGia, "MaDocGia", "HoTen", datTruoc.MaDocGia);
            }
            ViewData["MaSach"] = new SelectList(_context.Sach, "MaSach", "TenSach", datTruoc.MaSach);
            return View(datTruoc);
        }

        // Hiển thị form sửa đặt trước - chỉ Admin/NhanVien (cập nhật trạng thái)
        [Authorize(Roles = "Admin,NhanVien")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.DatTruoc == null)
            {
                return NotFound();
            }

            var datTruoc = await _context.DatTruoc.FindAsync(id);
            if (datTruoc == null)
            {
                return NotFound();
            }
            ViewData["MaDocGia"] = new SelectList(_context.DocGia, "MaDocGia", "HoTen", datTruoc.MaDocGia);
            ViewData["MaSach"] = new SelectList(_context.Sach, "MaSach", "TenSach", datTruoc.MaSach);
            return View(datTruoc);
        }

        // Xử lý cập nhật đặt trước - chỉ Admin/NhanVien
        [Authorize(Roles = "Admin,NhanVien")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("MaDatTruoc,MaDocGia,MaSach,NgayDat,NgayHetHanDat,TrangThai")] DatTruoc datTruoc)
        {
            if (id != datTruoc.MaDatTruoc)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(datTruoc);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!DatTruocExists(datTruoc.MaDatTruoc))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["MaDocGia"] = new SelectList(_context.DocGia, "MaDocGia", "HoTen", datTruoc.MaDocGia);
            ViewData["MaSach"] = new SelectList(_context.Sach, "MaSach", "TenSach", datTruoc.MaSach);
            return View(datTruoc);
        }

        // Hiển thị xác nhận xóa đặt trước - chỉ Admin/NhanVien
        [Authorize(Roles = "Admin,NhanVien")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.DatTruoc == null)
            {
                return NotFound();
            }

            var datTruoc = await _context.DatTruoc
                .Include(d => d.DocGia)
                .Include(d => d.Sach)
                .FirstOrDefaultAsync(m => m.MaDatTruoc == id);
            if (datTruoc == null)
            {
                return NotFound();
            }

            return View(datTruoc);
        }

        // Xử lý xóa đặt trước - chỉ Admin/NhanVien
        [Authorize(Roles = "Admin,NhanVien")]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.DatTruoc == null)
            {
                return Problem("Entity set 'bài_tập_1Context.DatTruoc'  is null.");
            }
            var datTruoc = await _context.DatTruoc.FindAsync(id);
            if (datTruoc != null)
            {
                _context.DatTruoc.Remove(datTruoc);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool DatTruocExists(int id)
        {
            return (_context.DatTruoc?.Any(e => e.MaDatTruoc == id)).GetValueOrDefault();
        }
    }
}