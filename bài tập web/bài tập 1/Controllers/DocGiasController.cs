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
    public class DocGiasController : Controller
    {
        private readonly bài_tập_1Context _context;

        public DocGiasController(bài_tập_1Context context)
        {
            _context = context;
        }

        // GET: DocGias
        public async Task<IActionResult> Index()
        {
            var bài_tập_1Context = _context.DocGia.Include(d => d.User);
            return View(await bài_tập_1Context.ToListAsync());
        }

        // GET: DocGias/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.DocGia == null)
            {
                return NotFound();
            }

            var docGia = await _context.DocGia
                .Include(d => d.User)
                .FirstOrDefaultAsync(m => m.MaDocGia == id);
            if (docGia == null)
            {
                return NotFound();
            }

            return View(docGia);
        }

        // GET: DocGias/Create
        public IActionResult Create()
        {
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "Id");
            return View();
        }

        // POST: DocGias/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("MaDocGia,UserId,HoTen,NgaySinh,GioiTinh,DiaChi,SoDienThoai,Email,NgayDangKy,NgayHetHanThe,TrangThai")] DocGia docGia)
        {
            if (ModelState.IsValid)
            {
                _context.Add(docGia);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "Id", docGia.UserId);
            return View(docGia);
        }

        // GET: DocGias/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.DocGia == null)
            {
                return NotFound();
            }

            var docGia = await _context.DocGia.FindAsync(id);
            if (docGia == null)
            {
                return NotFound();
            }
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "Id", docGia.UserId);
            return View(docGia);
        }

        // POST: DocGias/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("MaDocGia,UserId,HoTen,NgaySinh,GioiTinh,DiaChi,SoDienThoai,Email,NgayDangKy,NgayHetHanThe,TrangThai")] DocGia docGia)
        {
            if (id != docGia.MaDocGia)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(docGia);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!DocGiaExists(docGia.MaDocGia))
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
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "Id", docGia.UserId);
            return View(docGia);
        }

        // GET: DocGias/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.DocGia == null)
            {
                return NotFound();
            }

            var docGia = await _context.DocGia
                .Include(d => d.User)
                .FirstOrDefaultAsync(m => m.MaDocGia == id);
            if (docGia == null)
            {
                return NotFound();
            }

            return View(docGia);
        }

        // POST: DocGias/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.DocGia == null)
            {
                return Problem("Entity set 'bài_tập_1Context.DocGia'  is null.");
            }
            var docGia = await _context.DocGia.FindAsync(id);
            if (docGia != null)
            {
                _context.DocGia.Remove(docGia);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool DocGiaExists(int id)
        {
          return (_context.DocGia?.Any(e => e.MaDocGia == id)).GetValueOrDefault();
        }
    }
}
