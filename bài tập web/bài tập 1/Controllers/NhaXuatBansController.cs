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
    public class NhaXuatBansController : Controller
    {
        private readonly bài_tập_1Context _context;

        public NhaXuatBansController(bài_tập_1Context context)
        {
            _context = context;
        }

        // GET: NhaXuatBans
        public async Task<IActionResult> Index()
        {
              return _context.NhaXuatBan != null ? 
                          View(await _context.NhaXuatBan.ToListAsync()) :
                          Problem("Entity set 'bài_tập_1Context.NhaXuatBan'  is null.");
        }

        // GET: NhaXuatBans/Details/5
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

        // GET: NhaXuatBans/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: NhaXuatBans/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
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

        // GET: NhaXuatBans/Edit/5
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

        // POST: NhaXuatBans/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
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

        // GET: NhaXuatBans/Delete/5
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

        // POST: NhaXuatBans/Delete/5
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
