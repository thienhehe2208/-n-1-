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
    public class TheLoaisController : Controller
    {
        private readonly bài_tập_1Context _context;

        public TheLoaisController(bài_tập_1Context context)
        {
            _context = context;
        }

        // Danh sách thể loại - ai cũng xem được
        public async Task<IActionResult> Index()
        {
            return View(await _context.TheLoai
                .Include(t => t.DanhSachSach)
                .AsNoTracking()
                .OrderBy(t => t.TenTheLoai)
                .ToListAsync());
        }

        // Xem chi tiết 1 thể loại - ai cũng xem được
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.TheLoai == null)
            {
                return NotFound();
            }

            var theLoai = await _context.TheLoai
                .FirstOrDefaultAsync(m => m.MaTheLoai == id);
            if (theLoai == null)
            {
                return NotFound();
            }

            return View(theLoai);
        }

        // Hiển thị form thêm thể loại - chỉ Admin/NhanVien
        [Authorize(Roles = "Admin,NhanVien")]
        public IActionResult Create()
        {
            return View();
        }

        // Xử lý lưu thể loại mới - chỉ Admin/NhanVien
        [Authorize(Roles = "Admin,NhanVien")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("MaTheLoai,TenTheLoai,MoTa")] TheLoai theLoai)
        {
            if (ModelState.IsValid)
            {
                _context.Add(theLoai);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(theLoai);
        }

        // Hiển thị form sửa thể loại - chỉ Admin/NhanVien
        [Authorize(Roles = "Admin,NhanVien")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.TheLoai == null)
            {
                return NotFound();
            }

            var theLoai = await _context.TheLoai.FindAsync(id);
            if (theLoai == null)
            {
                return NotFound();
            }
            return View(theLoai);
        }

        // Xử lý cập nhật thể loại - chỉ Admin/NhanVien
        [Authorize(Roles = "Admin,NhanVien")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("MaTheLoai,TenTheLoai,MoTa")] TheLoai theLoai)
        {
            if (id != theLoai.MaTheLoai)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(theLoai);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TheLoaiExists(theLoai.MaTheLoai))
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
            return View(theLoai);
        }

        // Hiển thị xác nhận xóa thể loại - chỉ Admin/NhanVien
        [Authorize(Roles = "Admin,NhanVien")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.TheLoai == null)
            {
                return NotFound();
            }

            var theLoai = await _context.TheLoai
                .FirstOrDefaultAsync(m => m.MaTheLoai == id);
            if (theLoai == null)
            {
                return NotFound();
            }

            return View(theLoai);
        }

        // Xử lý xóa thể loại - chỉ Admin/NhanVien
        [Authorize(Roles = "Admin,NhanVien")]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.TheLoai == null)
            {
                return Problem("Entity set 'bài_tập_1Context.TheLoai'  is null.");
            }
            var theLoai = await _context.TheLoai.FindAsync(id);
            if (theLoai != null)
            {
                _context.TheLoai.Remove(theLoai);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool TheLoaiExists(int id)
        {
            return (_context.TheLoai?.Any(e => e.MaTheLoai == id)).GetValueOrDefault();
        }
    }
}
