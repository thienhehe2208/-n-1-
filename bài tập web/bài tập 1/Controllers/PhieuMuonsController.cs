using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using bài_tập_1.Data;
using bài_tập_1.Models;

namespace bài_tập_1.Controllers
{
    public class PhieuMuonsController : Controller
    {
        private readonly bài_tập_1Context _context;

        public PhieuMuonsController(bài_tập_1Context context)
        {
            _context = context;
        }

        // GET: PhieuMuons
        public async Task<IActionResult> Index()
        {
            var bài_tập_1Context = _context.PhieuMuon.Include(p => p.DocGia).Include(p => p.NhanVien);
            return View(await bài_tập_1Context.ToListAsync());
        }

        // GET: PhieuMuons/Details/5
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

        // GET: PhieuMuons/Create
        public IActionResult Create()
        {
            ViewData["MaDocGia"] = new SelectList(_context.DocGia, "MaDocGia", "HoTen");
            ViewData["MaNhanVien"] = new SelectList(_context.NhanVien, "MaNhanVien", "HoTen");
            return View();
        }

        // POST: PhieuMuons/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
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

        // GET: PhieuMuons/Edit/5
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

        // POST: PhieuMuons/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
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

        // GET: PhieuMuons/Delete/5
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

        // POST: PhieuMuons/Delete/5
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
