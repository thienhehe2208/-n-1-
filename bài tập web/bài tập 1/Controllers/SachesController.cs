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
    public class SachesController : Controller
    {
        private readonly bài_tập_1Context _context;

        public SachesController(bài_tập_1Context context)
        {
            _context = context;
        }

        // Danh sách sách - ai cũng xem được, không cần đăng nhập
        public async Task<IActionResult> Index()
        {
            var bài_tập_1Context = _context.Sach.Include(s => s.NhaXuatBan).Include(s => s.TheLoai);
            return View(await bài_tập_1Context.ToListAsync());
        }

        // Xem chi tiết 1 sách - ai cũng xem được
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Sach == null)
            {
                return NotFound();
            }

            var sach = await _context.Sach
                .Include(s => s.NhaXuatBan)
                .Include(s => s.TheLoai)
                .FirstOrDefaultAsync(m => m.MaSach == id);
            if (sach == null)
            {
                return NotFound();
            }

            return View(sach);
        }

        // Hiển thị form thêm sách - chỉ Admin/NhanVien
        [Authorize(Roles = "Admin,NhanVien")]
        public IActionResult Create()
        {
            ViewData["MaNXB"] = new SelectList(_context.Set<NhaXuatBan>(), "MaNXB", "TenNXB");
            ViewData["MaTheLoai"] = new SelectList(_context.Set<TheLoai>(), "MaTheLoai", "TenTheLoai");
            return View();
        }

        // Xử lý lưu sách mới - chỉ Admin/NhanVien
        [Authorize(Roles = "Admin,NhanVien")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("MaSach,TenSach,ISBN,GiaSach,MaTheLoai,MaNXB,NamXuatBan,SoTrang,NgonNgu,MoTa,AnhBia")] Sach sach)
        {
            if (ModelState.IsValid)
            {
                _context.Add(sach);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["MaNXB"] = new SelectList(_context.Set<NhaXuatBan>(), "MaNXB", "TenNXB", sach.MaNXB);
            ViewData["MaTheLoai"] = new SelectList(_context.Set<TheLoai>(), "MaTheLoai", "TenTheLoai", sach.MaTheLoai);
            return View(sach);
        }

        // Hiển thị form sửa sách - chỉ Admin/NhanVien
        [Authorize(Roles = "Admin,NhanVien")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Sach == null)
            {
                return NotFound();
            }

            var sach = await _context.Sach.FindAsync(id);
            if (sach == null)
            {
                return NotFound();
            }
            ViewData["MaNXB"] = new SelectList(_context.Set<NhaXuatBan>(), "MaNXB", "TenNXB", sach.MaNXB);
            ViewData["MaTheLoai"] = new SelectList(_context.Set<TheLoai>(), "MaTheLoai", "TenTheLoai", sach.MaTheLoai);
            return View(sach);
        }

        // Xử lý cập nhật sách - chỉ Admin/NhanVien
        [Authorize(Roles = "Admin,NhanVien")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("MaSach,TenSach,ISBN,GiaSach,MaTheLoai,MaNXB,NamXuatBan,SoTrang,NgonNgu,MoTa,AnhBia")] Sach sach)
        {
            if (id != sach.MaSach)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(sach);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!SachExists(sach.MaSach))
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
            ViewData["MaNXB"] = new SelectList(_context.Set<NhaXuatBan>(), "MaNXB", "TenNXB", sach.MaNXB);
            ViewData["MaTheLoai"] = new SelectList(_context.Set<TheLoai>(), "MaTheLoai", "TenTheLoai", sach.MaTheLoai);
            return View(sach);
        }

        // Hiển thị xác nhận xóa sách - chỉ Admin/NhanVien
        [Authorize(Roles = "Admin,NhanVien")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Sach == null)
            {
                return NotFound();
            }

            var sach = await _context.Sach
                .Include(s => s.NhaXuatBan)
                .Include(s => s.TheLoai)
                .FirstOrDefaultAsync(m => m.MaSach == id);
            if (sach == null)
            {
                return NotFound();
            }

            return View(sach);
        }

        // Xử lý xóa sách - chỉ Admin/NhanVien
        [Authorize(Roles = "Admin,NhanVien")]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Sach == null)
            {
                return Problem("Entity set 'bài_tập_1Context.Sach'  is null.");
            }
            var sach = await _context.Sach.FindAsync(id);
            if (sach != null)
            {
                _context.Sach.Remove(sach);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool SachExists(int id)
        {
            return (_context.Sach?.Any(e => e.MaSach == id)).GetValueOrDefault();
        }
    }
}