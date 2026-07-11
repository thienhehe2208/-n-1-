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
    public class DatTruocsController : Controller
    {
        private readonly bài_tập_1Context _context;

        public DatTruocsController(bài_tập_1Context context)
        {
            _context = context;
        }

        // GET: DatTruocs
        public async Task<IActionResult> Index()
        {
            var bài_tập_1Context = _context.DatTruoc.Include(d => d.DocGia).Include(d => d.Sach);
            return View(await bài_tập_1Context.ToListAsync());
        }

        // GET: DatTruocs/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.DatTruoc == null)
            {
                return NotFound();
            }

            var datTruoc = await _context.DatTruoc
                .Include(d => d.DocGia)
                .Include(d => d.Sach)
                .FirstOrDefaultAsync(m => m.MaDatTruoc == id);
            if (datTruoc == null)
            {
                return NotFound();
            }

            return View(datTruoc);
        }

        // GET: DatTruocs/Create
        public IActionResult Create()
        {
            ViewData["MaDocGia"] = new SelectList(_context.DocGia, "MaDocGia", "HoTen");
            ViewData["MaSach"] = new SelectList(_context.Sach, "MaSach", "TenSach");
            return View();
        }

        // POST: DatTruocs/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("MaDatTruoc,MaDocGia,MaSach,NgayDat,NgayHetHanDat,TrangThai")] DatTruoc datTruoc)
        {
            if (ModelState.IsValid)
            {
                _context.Add(datTruoc);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["MaDocGia"] = new SelectList(_context.DocGia, "MaDocGia", "HoTen", datTruoc.MaDocGia);
            ViewData["MaSach"] = new SelectList(_context.Sach, "MaSach", "TenSach", datTruoc.MaSach);
            return View(datTruoc);
        }

        // GET: DatTruocs/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.DatTruoc == null)
            {
                return NotFound();
            }

            var datTruoc = await _context.DatTruoc.FindAsync(id);
            if (datTruoc == null)
            {
                return NotFound();
            }
            ViewData["MaDocGia"] = new SelectList(_context.DocGia, "MaDocGia", "HoTen", datTruoc.MaDocGia);
            ViewData["MaSach"] = new SelectList(_context.Sach, "MaSach", "TenSach", datTruoc.MaSach);
            return View(datTruoc);
        }

        // POST: DatTruocs/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("MaDatTruoc,MaDocGia,MaSach,NgayDat,NgayHetHanDat,TrangThai")] DatTruoc datTruoc)
        {
            if (id != datTruoc.MaDatTruoc)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(datTruoc);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!DatTruocExists(datTruoc.MaDatTruoc))
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
            ViewData["MaDocGia"] = new SelectList(_context.DocGia, "MaDocGia", "HoTen", datTruoc.MaDocGia);
            ViewData["MaSach"] = new SelectList(_context.Sach, "MaSach", "TenSach", datTruoc.MaSach);
            return View(datTruoc);
        }

        // GET: DatTruocs/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.DatTruoc == null)
            {
                return NotFound();
            }

            var datTruoc = await _context.DatTruoc
                .Include(d => d.DocGia)
                .Include(d => d.Sach)
                .FirstOrDefaultAsync(m => m.MaDatTruoc == id);
            if (datTruoc == null)
            {
                return NotFound();
            }

            return View(datTruoc);
        }

        // POST: DatTruocs/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.DatTruoc == null)
            {
                return Problem("Entity set 'bài_tập_1Context.DatTruoc'  is null.");
            }
            var datTruoc = await _context.DatTruoc.FindAsync(id);
            if (datTruoc != null)
            {
                _context.DatTruoc.Remove(datTruoc);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool DatTruocExists(int id)
        {
          return (_context.DatTruoc?.Any(e => e.MaDatTruoc == id)).GetValueOrDefault();
        }
    }
}
