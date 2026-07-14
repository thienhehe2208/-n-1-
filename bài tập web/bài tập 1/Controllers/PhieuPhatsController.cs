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
    // Lập/quản lý phiếu phạt do nhân viên xử lý khi độc giả trả trễ/mất/hỏng sách
    [Authorize(Roles = "Admin,NhanVien")]
    public class PhieuPhatsController : Controller
    {
        private readonly bài_tập_1Context _context;

        public PhieuPhatsController(bài_tập_1Context context)
        {
            _context = context;
        }

        // Danh sách phiếu phạt
        public async Task<IActionResult> Index()
        {
            var bài_tập_1Context = _context.PhieuPhat.Include(p => p.ChiTietPhieuMuon);
            return View(await bài_tập_1Context.ToListAsync());
        }

        // Xem chi tiết 1 phiếu phạt
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.PhieuPhat == null)
            {
                return NotFound();
            }

            var phieuPhat = await _context.PhieuPhat
                .Include(p => p.ChiTietPhieuMuon)
                .FirstOrDefaultAsync(m => m.MaPhieuPhat == id);
            if (phieuPhat == null)
            {
                return NotFound();
            }

            return View(phieuPhat);
        }

        // Hiển thị form lập phiếu phạt
        public IActionResult Create()
        {
            ViewData["MaChiTiet"] = new SelectList(_context.ChiTietPhieuMuon, "MaChiTiet", "MaChiTiet");
            return View();
        }

        // Xử lý lưu phiếu phạt mới
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("MaPhieuPhat,MaChiTiet,SoTien,LyDo,NgayLap,TrangThai")] PhieuPhat phieuPhat)
        {
            if (ModelState.IsValid)
            {
                _context.Add(phieuPhat);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["MaChiTiet"] = new SelectList(_context.ChiTietPhieuMuon, "MaChiTiet", "MaChiTiet", phieuPhat.MaChiTiet);
            return View(phieuPhat);
        }

        // Hiển thị form sửa phiếu phạt
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.PhieuPhat == null)
            {
                return NotFound();
            }

            var phieuPhat = await _context.PhieuPhat.FindAsync(id);
            if (phieuPhat == null)
            {
                return NotFound();
            }
            ViewData["MaChiTiet"] = new SelectList(_context.ChiTietPhieuMuon, "MaChiTiet", "MaChiTiet", phieuPhat.MaChiTiet);
            return View(phieuPhat);
        }

        // Xử lý cập nhật phiếu phạt
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("MaPhieuPhat,MaChiTiet,SoTien,LyDo,NgayLap,TrangThai")] PhieuPhat phieuPhat)
        {
            if (id != phieuPhat.MaPhieuPhat)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(phieuPhat);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PhieuPhatExists(phieuPhat.MaPhieuPhat))
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
            ViewData["MaChiTiet"] = new SelectList(_context.ChiTietPhieuMuon, "MaChiTiet", "MaChiTiet", phieuPhat.MaChiTiet);
            return View(phieuPhat);
        }

        // Hiển thị xác nhận xóa phiếu phạt
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.PhieuPhat == null)
            {
                return NotFound();
            }

            var phieuPhat = await _context.PhieuPhat
                .Include(p => p.ChiTietPhieuMuon)
                .FirstOrDefaultAsync(m => m.MaPhieuPhat == id);
            if (phieuPhat == null)
            {
                return NotFound();
            }

            return View(phieuPhat);
        }

        // Xử lý xóa phiếu phạt
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.PhieuPhat == null)
            {
                return Problem("Entity set 'bài_tập_1Context.PhieuPhat'  is null.");
            }
            var phieuPhat = await _context.PhieuPhat.FindAsync(id);
            if (phieuPhat != null)
            {
                _context.PhieuPhat.Remove(phieuPhat);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool PhieuPhatExists(int id)
        {
            return (_context.PhieuPhat?.Any(e => e.MaPhieuPhat == id)).GetValueOrDefault();
        }
    }
}