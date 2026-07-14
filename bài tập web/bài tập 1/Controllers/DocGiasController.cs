using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using bài_tập_1.Data;
using bài_tập_1.Models;

namespace bài_tập_1.Controllers
{
    // Quản lý toàn bộ độc giả là nghiệp vụ nội bộ của thư viện,
    // không phải trang độc giả tự xem thông tin cá nhân (đó là ProfileController riêng)
    [Authorize(Roles = "Admin,NhanVien")]
    public class DocGiasController : Controller
    {
        private readonly bài_tập_1Context _context;

        public DocGiasController(bài_tập_1Context context)
        {
            _context = context;
        }

        // Danh sách độc giả
        public async Task<IActionResult> Index()
        {
            var bài_tập_1Context = _context.DocGia.Include(d => d.User);
            return View(await bài_tập_1Context.ToListAsync());
        }

        // Xem chi tiết 1 độc giả
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.DocGia == null)
            {
                return NotFound();
            }

            var docGia = await _context.DocGia
                .Include(d => d.User)
                .FirstOrDefaultAsync(m => m.MaDocGia == id);
            if (docGia == null)
            {
                return NotFound();
            }

            return View(docGia);
        }

        // Hiển thị form thêm độc giả
        public IActionResult Create()
        {
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "Id");
            return View();
        }

        // Xử lý lưu độc giả mới
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("MaDocGia,UserId,HoTen,NgaySinh,GioiTinh,DiaChi,SoDienThoai,Email,NgayDangKy,NgayHetHanThe,TrangThai")] DocGia docGia)
        {
            if (ModelState.IsValid)
            {
                _context.Add(docGia);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "Id", docGia.UserId);
            return View(docGia);
        }

        // Hiển thị form sửa độc giả
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.DocGia == null)
            {
                return NotFound();
            }

            var docGia = await _context.DocGia.FindAsync(id);
            if (docGia == null)
            {
                return NotFound();
            }
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "Id", docGia.UserId);
            return View(docGia);
        }

        // Xử lý cập nhật độc giả
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("MaDocGia,UserId,HoTen,NgaySinh,GioiTinh,DiaChi,SoDienThoai,Email,NgayDangKy,NgayHetHanThe,TrangThai")] DocGia docGia)
        {
            if (id != docGia.MaDocGia)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(docGia);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!DocGiaExists(docGia.MaDocGia))
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
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "Id", docGia.UserId);
            return View(docGia);
        }

        // Hiển thị xác nhận xóa độc giả
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.DocGia == null)
            {
                return NotFound();
            }

            var docGia = await _context.DocGia
                .Include(d => d.User)
                .FirstOrDefaultAsync(m => m.MaDocGia == id);
            if (docGia == null)
            {
                return NotFound();
            }

            return View(docGia);
        }

        // Xử lý xóa độc giả
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.DocGia == null)
            {
                return Problem("Entity set 'bài_tập_1Context.DocGia'  is null.");
            }
            var docGia = await _context.DocGia.FindAsync(id);
            if (docGia != null)
            {
                _context.DocGia.Remove(docGia);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool DocGiaExists(int id)
        {
            return (_context.DocGia?.Any(e => e.MaDocGia == id)).GetValueOrDefault();
        }
    }
}