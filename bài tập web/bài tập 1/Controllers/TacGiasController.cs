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
    public class TacGiasController : Controller
    {
        private readonly bài_tập_1Context _context;

        public TacGiasController(bài_tập_1Context context)
        {
            _context = context;
        }

        // Danh sách tác giả - ai cũng xem được
        public async Task<IActionResult> Index()
        {
            return _context.TacGia != null ?
                        View(await _context.TacGia.ToListAsync()) :
                        Problem("Entity set 'bài_tập_1Context.TacGia'  is null.");
        }

        // Xem chi tiết 1 tác giả - ai cũng xem được
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.TacGia == null)
            {
                return NotFound();
            }

            var tacGia = await _context.TacGia
                .FirstOrDefaultAsync(m => m.MaTacGia == id);
            if (tacGia == null)
            {
                return NotFound();
            }

            return View(tacGia);
        }

        // Hiển thị form thêm tác giả - chỉ Admin/NhanVien
        [Authorize(Roles = "Admin,NhanVien")]
        public IActionResult Create()
        {
            return View();
        }

        // Xử lý lưu tác giả mới - chỉ Admin/NhanVien
        [Authorize(Roles = "Admin,NhanVien")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("MaTacGia,HoTen,NgaySinh,QuocTich,TieuSu")] TacGia tacGia)
        {
            if (ModelState.IsValid)
            {
                _context.Add(tacGia);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(tacGia);
        }

        // Hiển thị form sửa tác giả - chỉ Admin/NhanVien
        [Authorize(Roles = "Admin,NhanVien")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.TacGia == null)
            {
                return NotFound();
            }

            var tacGia = await _context.TacGia.FindAsync(id);
            if (tacGia == null)
            {
                return NotFound();
            }
            return View(tacGia);
        }

        // Xử lý cập nhật tác giả - chỉ Admin/NhanVien
        [Authorize(Roles = "Admin,NhanVien")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("MaTacGia,HoTen,NgaySinh,QuocTich,TieuSu")] TacGia tacGia)
        {
            if (id != tacGia.MaTacGia)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(tacGia);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TacGiaExists(tacGia.MaTacGia))
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
            return View(tacGia);
        }

        // Hiển thị xác nhận xóa tác giả - chỉ Admin/NhanVien
        [Authorize(Roles = "Admin,NhanVien")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.TacGia == null)
            {
                return NotFound();
            }

            var tacGia = await _context.TacGia
                .FirstOrDefaultAsync(m => m.MaTacGia == id);
            if (tacGia == null)
            {
                return NotFound();
            }

            return View(tacGia);
        }

        // Xử lý xóa tác giả - chỉ Admin/NhanVien
        [Authorize(Roles = "Admin,NhanVien")]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.TacGia == null)
            {
                return Problem("Entity set 'bài_tập_1Context.TacGia'  is null.");
            }
            var tacGia = await _context.TacGia.FindAsync(id);
            if (tacGia != null)
            {
                _context.TacGia.Remove(tacGia);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool TacGiaExists(int id)
        {
            return (_context.TacGia?.Any(e => e.MaTacGia == id)).GetValueOrDefault();
        }
    }
}