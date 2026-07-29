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
    // Nghiệp vụ lập/quản lý phiếu mượn do nhân viên xử lý, không công khai cho độc giả
    [Authorize(Roles = "Admin,NhanVien")]
    public class PhieuMuonsController : Controller
    {
        private readonly bài_tập_1Context _context;

        public PhieuMuonsController(bài_tập_1Context context)
        {
            _context = context;
        }

        // Danh sách phiếu mượn
        public async Task<IActionResult> Index(string? q, string? trangThai)
        {
            var homNay = DateTime.Today;
            var query = _context.PhieuMuon
                .Include(p => p.DocGia)
                .Include(p => p.NhanVien)
                .Include(p => p.ChiTietPhieuMuons)
                .AsNoTracking()
                .AsQueryable();

            ViewData["TongPhieu"] = await query.CountAsync();
            ViewData["DangMuon"] = await query.CountAsync(p =>
                p.TrangThai == TrangThaiPhieuMuon.DangMuon &&
                p.NgayHenTra >= homNay);
            ViewData["DaTra"] = await query.CountAsync(p =>
                p.TrangThai == TrangThaiPhieuMuon.DaTra);
            ViewData["QuaHan"] = await query.CountAsync(p =>
                p.TrangThai != TrangThaiPhieuMuon.DaTra &&
                p.NgayHenTra < homNay);

            if (!string.IsNullOrWhiteSpace(q))
            {
                var keyword = q.Trim();
                var isId = int.TryParse(keyword.TrimStart('#'), out var maPhieu);
                query = query.Where(p =>
                    p.DocGia.HoTen.Contains(keyword) ||
                    p.NhanVien.HoTen.Contains(keyword) ||
                    (isId && p.MaPhieuMuon == maPhieu));
            }

            query = trangThai switch
            {
                "borrowing" => query.Where(p =>
                    p.TrangThai == TrangThaiPhieuMuon.DangMuon &&
                    p.NgayHenTra >= homNay),
                "returned" => query.Where(p =>
                    p.TrangThai == TrangThaiPhieuMuon.DaTra),
                "overdue" => query.Where(p =>
                    p.TrangThai != TrangThaiPhieuMuon.DaTra &&
                    p.NgayHenTra < homNay),
                _ => query
            };

            ViewData["Search"] = q;
            ViewData["Status"] = trangThai;
            return View(await query
                .OrderByDescending(p => p.NgayMuon)
                .ThenByDescending(p => p.MaPhieuMuon)
                .ToListAsync());
        }

        // Xem chi tiết 1 phiếu mượn
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.PhieuMuon == null)
            {
                return NotFound();
            }

            var phieuMuon = await _context.PhieuMuon
                .Include(p => p.DocGia)
                .Include(p => p.NhanVien)
                .FirstOrDefaultAsync(m => m.MaPhieuMuon == id);
            if (phieuMuon == null)
            {
                return NotFound();
            }

            return View(phieuMuon);
        }

        // Hiển thị form lập phiếu mượn
        public IActionResult Create()
        {
            ViewData["MaDocGia"] = new SelectList(_context.DocGia, "MaDocGia", "HoTen");
            ViewData["MaNhanVien"] = new SelectList(_context.NhanVien, "MaNhanVien", "HoTen");
            return View();
        }

        // Xử lý lưu phiếu mượn mới
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("MaPhieuMuon,MaDocGia,MaNhanVien,NgayMuon,NgayHenTra,TrangThai")] PhieuMuon phieuMuon)
        {
            if (ModelState.IsValid)
            {
                _context.Add(phieuMuon);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["MaDocGia"] = new SelectList(_context.DocGia, "MaDocGia", "HoTen", phieuMuon.MaDocGia);
            ViewData["MaNhanVien"] = new SelectList(_context.NhanVien, "MaNhanVien", "HoTen", phieuMuon.MaNhanVien);
            return View(phieuMuon);
        }

        // Hiển thị form sửa phiếu mượn
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.PhieuMuon == null)
            {
                return NotFound();
            }

            var phieuMuon = await _context.PhieuMuon.FindAsync(id);
            if (phieuMuon == null)
            {
                return NotFound();
            }
            ViewData["MaDocGia"] = new SelectList(_context.DocGia, "MaDocGia", "HoTen", phieuMuon.MaDocGia);
            ViewData["MaNhanVien"] = new SelectList(_context.NhanVien, "MaNhanVien", "HoTen", phieuMuon.MaNhanVien);
            return View(phieuMuon);
        }

        // Xử lý cập nhật phiếu mượn
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("MaPhieuMuon,MaDocGia,MaNhanVien,NgayMuon,NgayHenTra,TrangThai")] PhieuMuon phieuMuon)
        {
            if (id != phieuMuon.MaPhieuMuon)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(phieuMuon);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PhieuMuonExists(phieuMuon.MaPhieuMuon))
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
            ViewData["MaDocGia"] = new SelectList(_context.DocGia, "MaDocGia", "HoTen", phieuMuon.MaDocGia);
            ViewData["MaNhanVien"] = new SelectList(_context.NhanVien, "MaNhanVien", "HoTen", phieuMuon.MaNhanVien);
            return View(phieuMuon);
        }

        // Hiển thị xác nhận xóa phiếu mượn
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.PhieuMuon == null)
            {
                return NotFound();
            }

            var phieuMuon = await _context.PhieuMuon
                .Include(p => p.DocGia)
                .Include(p => p.NhanVien)
                .FirstOrDefaultAsync(m => m.MaPhieuMuon == id);
            if (phieuMuon == null)
            {
                return NotFound();
            }

            return View(phieuMuon);
        }

        // Xử lý xóa phiếu mượn
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.PhieuMuon == null)
            {
                return Problem("Entity set 'bài_tập_1Context.PhieuMuon'  is null.");
            }
            var phieuMuon = await _context.PhieuMuon.FindAsync(id);
            if (phieuMuon != null)
            {
                _context.PhieuMuon.Remove(phieuMuon);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool PhieuMuonExists(int id)
        {
            return (_context.PhieuMuon?.Any(e => e.MaPhieuMuon == id)).GetValueOrDefault();
        }
    }
}
