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
    public class ChiTietPhieuMuonsController : Controller
    {
        private readonly bài_tập_1Context _context;

        public ChiTietPhieuMuonsController(bài_tập_1Context context)
        {
            _context = context;
        }

        // GET: ChiTietPhieuMuons
        public async Task<IActionResult> Index()
        {
            var bài_tập_1Context = _context.ChiTietPhieuMuon.Include(c => c.BanSao).Include(c => c.PhieuMuon);
            return View(await bài_tập_1Context.ToListAsync());
        }

        // GET: ChiTietPhieuMuons/Details/5
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

        // GET: ChiTietPhieuMuons/Create
        public IActionResult Create()
        {
            ViewData["MaBanSao"] = new SelectList(_context.BanSao, "MaBanSao", "MaVach");
            ViewData["MaPhieuMuon"] = new SelectList(_context.Set<PhieuMuon>(), "MaPhieuMuon", "MaPhieuMuon");
            return View();
        }

        // POST: ChiTietPhieuMuons/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
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

        // GET: ChiTietPhieuMuons/Edit/5
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

        // POST: ChiTietPhieuMuons/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
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

        // GET: ChiTietPhieuMuons/Delete/5
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

        // POST: ChiTietPhieuMuons/Delete/5
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
