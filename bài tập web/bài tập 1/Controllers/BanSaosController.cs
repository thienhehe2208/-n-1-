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
    public class BanSaosController : Controller
    {
        private readonly bài_tập_1Context _context;

        public BanSaosController(bài_tập_1Context context)
        {
            _context = context;
        }

        // GET: BanSaos
        public async Task<IActionResult> Index()
        {
            var bài_tập_1Context = _context.BanSao.Include(b => b.Sach);
            return View(await bài_tập_1Context.ToListAsync());
        }

        // GET: BanSaos/Details/5
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

        // GET: BanSaos/Create
        public IActionResult Create()
        {
            ViewData["MaSach"] = new SelectList(_context.Sach, "MaSach", "TenSach");
            return View();
        }

        // POST: BanSaos/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("MaBanSao,MaSach,MaVach,TinhTrang,ViTriKe")] BanSao banSao)
        {
            if (ModelState.IsValid)
            {
                _context.Add(banSao);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["MaSach"] = new SelectList(_context.Sach, "MaSach", "TenSach", banSao.MaSach);
            return View(banSao);
        }

        // GET: BanSaos/Edit/5
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

        // POST: BanSaos/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("MaBanSao,MaSach,MaVach,TinhTrang,ViTriKe")] BanSao banSao)
        {
            if (id != banSao.MaBanSao)
            {
                return NotFound();
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

        // GET: BanSaos/Delete/5
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

        // POST: BanSaos/Delete/5
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
