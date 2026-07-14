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
    // Chi tiết phiếu mượn là dữ liệu con của PhieuMuon, chỉ nhân viên thao tác
    [Authorize(Roles = "Admin,NhanVien")]
    public class ChiTietPhieuMuonsController : Controller
    {
        private readonly bài_tập_1Context _context;

        public ChiTietPhieuMuonsController(bài_tập_1Context context)
        {
            _context = context;
        }

        // Danh sách chi tiết phiếu mượn
        public async Task<IActionResult> Index()
        {
            var bài_tập_1Context = _context.ChiTietPhieuMuon.Include(c => c.BanSao).Include(c => c.PhieuMuon);
            return View(await bài_tập_1Context.ToListAsync());
        }

        // Xem chi tiết 1 dòng
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.ChiTietPhieuMuon == null)
            {
                return NotFound();
            }

            var chiTietPhieuMuon = await _context.ChiTietPhieuMuon
                .Include(c => c.BanSao)
                .Include(c => c.PhieuMuon)
                .FirstOrDefaultAsync(m => m.MaChiTiet == id);
            if (chiTietPhieuMuon == null)
            {
                return NotFound();
            }

            return View(chiTietPhieuMuon);
        }

        // Hiển thị form thêm chi tiết
        public IActionResult Create()
        {
            ViewData["MaBanSao"] = new SelectList(_context.BanSao, "MaBanSao", "MaVach");
            ViewData["MaPhieuMuon"] = new SelectList(_context.Set<PhieuMuon>(), "MaPhieuMuon", "MaPhieuMuon");
            return View();
        }

        // Xử lý lưu chi tiết mới
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("MaChiTiet,MaPhieuMuon,MaBanSao,NgayTra,TinhTrangKhiTra,GhiChu")] ChiTietPhieuMuon chiTietPhieuMuon)
        {
            if (ModelState.IsValid)
            {
                _context.Add(chiTietPhieuMuon);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["MaBanSao"] = new SelectList(_context.BanSao, "MaBanSao", "MaVach", chiTietPhieuMuon.MaBanSao);
            ViewData["MaPhieuMuon"] = new SelectList(_context.Set<PhieuMuon>(), "MaPhieuMuon", "MaPhieuMuon", chiTietPhieuMuon.MaPhieuMuon);
            return View(chiTietPhieuMuon);
        }

        // Hiển thị form sửa chi tiết (dùng cho chức năng "trả sách")
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.ChiTietPhieuMuon == null)
            {
                return NotFound();
            }

            var chiTietPhieuMuon = await _context.ChiTietPhieuMuon.FindAsync(id);
            if (chiTietPhieuMuon == null)
            {
                return NotFound();
            }
            ViewData["MaBanSao"] = new SelectList(_context.BanSao, "MaBanSao", "MaVach", chiTietPhieuMuon.MaBanSao);
            ViewData["MaPhieuMuon"] = new SelectList(_context.Set<PhieuMuon>(), "MaPhieuMuon", "MaPhieuMuon", chiTietPhieuMuon.MaPhieuMuon);
            return View(chiTietPhieuMuon);
        }

        // Xử lý cập nhật chi tiết
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("MaChiTiet,MaPhieuMuon,MaBanSao,NgayTra,TinhTrangKhiTra,GhiChu")] ChiTietPhieuMuon chiTietPhieuMuon)
        {
            if (id != chiTietPhieuMuon.MaChiTiet)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(chiTietPhieuMuon);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ChiTietPhieuMuonExists(chiTietPhieuMuon.MaChiTiet))
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
            ViewData["MaBanSao"] = new SelectList(_context.BanSao, "MaBanSao", "MaVach", chiTietPhieuMuon.MaBanSao);
            ViewData["MaPhieuMuon"] = new SelectList(_context.Set<PhieuMuon>(), "MaPhieuMuon", "MaPhieuMuon", chiTietPhieuMuon.MaPhieuMuon);
            return View(chiTietPhieuMuon);
        }

        // Hiển thị xác nhận xóa chi tiết
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.ChiTietPhieuMuon == null)
            {
                return NotFound();
            }

            var chiTietPhieuMuon = await _context.ChiTietPhieuMuon
                .Include(c => c.BanSao)
                .Include(c => c.PhieuMuon)
                .FirstOrDefaultAsync(m => m.MaChiTiet == id);
            if (chiTietPhieuMuon == null)
            {
                return NotFound();
            }

            return View(chiTietPhieuMuon);
        }

        // Xử lý xóa chi tiết
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.ChiTietPhieuMuon == null)
            {
                return Problem("Entity set 'bài_tập_1Context.ChiTietPhieuMuon'  is null.");
            }
            var chiTietPhieuMuon = await _context.ChiTietPhieuMuon.FindAsync(id);
            if (chiTietPhieuMuon != null)
            {
                _context.ChiTietPhieuMuon.Remove(chiTietPhieuMuon);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ChiTietPhieuMuonExists(int id)
        {
            return (_context.ChiTietPhieuMuon?.Any(e => e.MaChiTiet == id)).GetValueOrDefault();
        }
    }
}