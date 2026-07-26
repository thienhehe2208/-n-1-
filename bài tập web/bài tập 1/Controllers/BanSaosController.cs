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
    // Quản lý bản sao (mã vạch, vị trí kệ, tình trạng) là dữ liệu vận hành nội bộ,
    // không phải thông tin cho độc giả tra cứu -> khóa toàn bộ Controller
    [Authorize(Roles = "Admin,NhanVien")]
    public class BanSaosController : Controller
    {
        private readonly bài_tập_1Context _context;

        public BanSaosController(bài_tập_1Context context)
        {
            _context = context;
        }

        // Danh sách bản sao
        public async Task<IActionResult> Index()
        {
            var bài_tập_1Context = _context.BanSao.Include(b => b.Sach);
            return View(await bài_tập_1Context.ToListAsync());
        }

        // Xem chi tiết 1 bản sao
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.BanSao == null)
            {
                return NotFound();
            }

            var banSao = await _context.BanSao
                .Include(b => b.Sach)
                .FirstOrDefaultAsync(m => m.MaBanSao == id);
            if (banSao == null)
            {
                return NotFound();
            }

            return View(banSao);
        }

        // Hiển thị form thêm bản sao
        public IActionResult Create()
        {
            ViewData["MaSach"] = new SelectList(_context.Sach, "MaSach", "TenSach");
            return View();
        }

        // Xử lý lưu bản sao mới
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("MaBanSao,MaSach,MaVach,TinhTrang,ViTriKe")] BanSao banSao)
        {
            banSao.MaVach = banSao.MaVach?.Trim() ?? string.Empty;
            if (await _context.BanSao.AnyAsync(
                    b => b.MaVach == banSao.MaVach))
            {
                ModelState.AddModelError(nameof(banSao.MaVach),
                    "Mã vạch này đã tồn tại.");
            }

            if (ModelState.IsValid)
            {
                _context.Add(banSao);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["MaSach"] = new SelectList(_context.Sach, "MaSach", "TenSach", banSao.MaSach);
            return View(banSao);
        }

        // Hiển thị form sửa bản sao
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.BanSao == null)
            {
                return NotFound();
            }

            var banSao = await _context.BanSao.FindAsync(id);
            if (banSao == null)
            {
                return NotFound();
            }
            ViewData["MaSach"] = new SelectList(_context.Sach, "MaSach", "TenSach", banSao.MaSach);
            return View(banSao);
        }

        // Xử lý cập nhật bản sao
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("MaBanSao,MaSach,MaVach,TinhTrang,ViTriKe")] BanSao banSao)
        {
            if (id != banSao.MaBanSao)
            {
                return NotFound();
            }

            banSao.MaVach = banSao.MaVach?.Trim() ?? string.Empty;
            if (await _context.BanSao.AnyAsync(
                    b => b.MaVach == banSao.MaVach &&
                         b.MaBanSao != id))
            {
                ModelState.AddModelError(nameof(banSao.MaVach),
                    "Mã vạch này đã tồn tại.");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(banSao);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!BanSaoExists(banSao.MaBanSao))
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
            ViewData["MaSach"] = new SelectList(_context.Sach, "MaSach", "TenSach", banSao.MaSach);
            return View(banSao);
        }

        // Hiển thị xác nhận xóa bản sao
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.BanSao == null)
            {
                return NotFound();
            }

            var banSao = await _context.BanSao
                .Include(b => b.Sach)
                .FirstOrDefaultAsync(m => m.MaBanSao == id);
            if (banSao == null)
            {
                return NotFound();
            }

            return View(banSao);
        }

        // Xử lý xóa bản sao
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.BanSao == null)
            {
                return Problem("Entity set 'bài_tập_1Context.BanSao'  is null.");
            }
            var banSao = await _context.BanSao.FindAsync(id);
            if (banSao != null)
            {
                _context.BanSao.Remove(banSao);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool BanSaoExists(int id)
        {
            return (_context.BanSao?.Any(e => e.MaBanSao == id)).GetValueOrDefault();
        }
    }
}
