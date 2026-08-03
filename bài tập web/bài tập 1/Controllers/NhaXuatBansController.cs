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
using bài_tập_1.Models.ViewModels;

namespace bài_tập_1.Controllers
{
    public class NhaXuatBansController : Controller
    {
        private readonly bài_tập_1Context _context;

        public NhaXuatBansController(bài_tập_1Context context)
        {
            _context = context;
        }

        // Danh sách NXB - ai cũng xem được
        public async Task<IActionResult> Index(string? q, int page = 1)
        {
            var source = _context.NhaXuatBan.AsNoTracking();
            ViewData["TongNhaXuatBan"] = await source.CountAsync();
            ViewData["TongDauSach"] = await _context.Sach.AsNoTracking().CountAsync();
            ViewData["CoThongTinLienHe"] = await source.CountAsync(n => !string.IsNullOrEmpty(n.Email) || !string.IsNullOrEmpty(n.SoDienThoai));
            q = q?.Trim();
            var query = source.Include(n => n.DanhSachSach).AsQueryable();
            if (!string.IsNullOrWhiteSpace(q))
                query = query.Where(n => n.TenNXB.Contains(q) || n.DiaChi.Contains(q) || n.Email.Contains(q) || n.SoDienThoai.Contains(q));
            ViewData["TuKhoa"] = q;
            var pagination = Pagination.Create(page, await query.CountAsync());
            ViewData["Pagination"] = pagination;
            return View(await query.OrderBy(n => n.TenNXB)
                .Skip((pagination.Page - 1) * pagination.PageSize)
                .Take(pagination.PageSize)
                .ToListAsync());
        }

        // Xem chi tiết 1 NXB - ai cũng xem được
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.NhaXuatBan == null)
            {
                return NotFound();
            }

            var nhaXuatBan = await _context.NhaXuatBan
                .FirstOrDefaultAsync(m => m.MaNXB == id);
            if (nhaXuatBan == null)
            {
                return NotFound();
            }

            return View(nhaXuatBan);
        }

        // Hiển thị form thêm NXB - chỉ Admin/NhanVien
        [Authorize(Roles = "Admin,NhanVien")]
        public IActionResult Create()
        {
            return View();
        }

        // Xử lý lưu NXB mới - chỉ Admin/NhanVien
        [Authorize(Roles = "Admin,NhanVien")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("MaNXB,TenNXB,DiaChi,SoDienThoai,Email")] NhaXuatBan nhaXuatBan)
        {
            if (ModelState.IsValid)
            {
                _context.Add(nhaXuatBan);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(nhaXuatBan);
        }

        // Hiển thị form sửa NXB - chỉ Admin/NhanVien
        [Authorize(Roles = "Admin,NhanVien")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.NhaXuatBan == null)
            {
                return NotFound();
            }

            var nhaXuatBan = await _context.NhaXuatBan.FindAsync(id);
            if (nhaXuatBan == null)
            {
                return NotFound();
            }
            return View(nhaXuatBan);
        }

        // Xử lý cập nhật NXB - chỉ Admin/NhanVien
        [Authorize(Roles = "Admin,NhanVien")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("MaNXB,TenNXB,DiaChi,SoDienThoai,Email")] NhaXuatBan nhaXuatBan)
        {
            if (id != nhaXuatBan.MaNXB)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(nhaXuatBan);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!NhaXuatBanExists(nhaXuatBan.MaNXB))
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
            return View(nhaXuatBan);
        }

        // Hiển thị xác nhận xóa NXB - chỉ Admin/NhanVien
        [Authorize(Roles = "Admin,NhanVien")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.NhaXuatBan == null)
            {
                return NotFound();
            }

            var nhaXuatBan = await _context.NhaXuatBan
                .FirstOrDefaultAsync(m => m.MaNXB == id);
            if (nhaXuatBan == null)
            {
                return NotFound();
            }

            return View(nhaXuatBan);
        }

        // Xử lý xóa NXB - chỉ Admin/NhanVien
        [Authorize(Roles = "Admin,NhanVien")]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.NhaXuatBan == null)
            {
                return Problem("Entity set 'bài_tập_1Context.NhaXuatBan'  is null.");
            }
            var nhaXuatBan = await _context.NhaXuatBan.FindAsync(id);
            if (nhaXuatBan != null)
            {
                _context.NhaXuatBan.Remove(nhaXuatBan);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool NhaXuatBanExists(int id)
        {
            return (_context.NhaXuatBan?.Any(e => e.MaNXB == id)).GetValueOrDefault();
        }
    }
}
