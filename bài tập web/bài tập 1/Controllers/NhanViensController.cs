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
    // Quản lý tài khoản nhân viên/admin - quyền cao nhất, chỉ Admin được vào
    [Authorize(Roles = "Admin")]
    public class NhanViensController : Controller
    {
        private readonly bài_tập_1Context _context;

        public NhanViensController(bài_tập_1Context context)
        {
            _context = context;
        }

        // Danh sách nhân viên
        public async Task<IActionResult> Index()
        {
            var bài_tập_1Context = _context.NhanVien.Include(n => n.User);
            return View(await bài_tập_1Context.ToListAsync());
        }

        // Xem chi tiết 1 nhân viên
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.NhanVien == null)
            {
                return NotFound();
            }

            var nhanVien = await _context.NhanVien
                .Include(n => n.User)
                .FirstOrDefaultAsync(m => m.MaNhanVien == id);
            if (nhanVien == null)
            {
                return NotFound();
            }

            return View(nhanVien);
        }

        // Hiển thị form thêm nhân viên
        public IActionResult Create()
        {
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "Id");
            return View();
        }

        // Xử lý lưu nhân viên mới
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("MaNhanVien,UserId,HoTen,NgaySinh,GioiTinh,DiaChi,SoDienThoai,Email,ChucVu,NgayVaoLam")] NhanVien nhanVien)
        {
            if (ModelState.IsValid)
            {
                _context.Add(nhanVien);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "Id", nhanVien.UserId);
            return View(nhanVien);
        }

        // Hiển thị form sửa nhân viên
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.NhanVien == null)
            {
                return NotFound();
            }

            var nhanVien = await _context.NhanVien.FindAsync(id);
            if (nhanVien == null)
            {
                return NotFound();
            }
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "Id", nhanVien.UserId);
            return View(nhanVien);
        }

        // Xử lý cập nhật nhân viên
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("MaNhanVien,UserId,HoTen,NgaySinh,GioiTinh,DiaChi,SoDienThoai,Email,ChucVu,NgayVaoLam")] NhanVien nhanVien)
        {
            if (id != nhanVien.MaNhanVien)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(nhanVien);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!NhanVienExists(nhanVien.MaNhanVien))
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
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "Id", nhanVien.UserId);
            return View(nhanVien);
        }

        // Hiển thị xác nhận xóa nhân viên
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.NhanVien == null)
            {
                return NotFound();
            }

            var nhanVien = await _context.NhanVien
                .Include(n => n.User)
                .FirstOrDefaultAsync(m => m.MaNhanVien == id);
            if (nhanVien == null)
            {
                return NotFound();
            }

            return View(nhanVien);
        }

        // Xử lý xóa nhân viên
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.NhanVien == null)
            {
                return Problem("Entity set 'bài_tập_1Context.NhanVien'  is null.");
            }
            var nhanVien = await _context.NhanVien.FindAsync(id);
            if (nhanVien != null)
            {
                _context.NhanVien.Remove(nhanVien);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool NhanVienExists(int id)
        {
            return (_context.NhanVien?.Any(e => e.MaNhanVien == id)).GetValueOrDefault();
        }
    }
}