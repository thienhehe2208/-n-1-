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
using bài_tập_1.Models.ViewModels;
using bài_tập_1.Services;

namespace bài_tập_1.Controllers
{
    public class TacGiasController : Controller
    {
        private readonly bài_tập_1Context _context;
        private readonly AdminChangeNotificationService _adminChangeNotification;

        public TacGiasController(
            bài_tập_1Context context,
            AdminChangeNotificationService adminChangeNotification)
        {
            _context = context;
            _adminChangeNotification = adminChangeNotification;
        }

        // Danh sách tác giả - ai cũng xem được
        public async Task<IActionResult> Index(string? q, int page = 1)
        {
            var source = _context.TacGia.AsNoTracking();
            ViewData["TongTacGia"] = await source.CountAsync();
            ViewData["TongLienKetSach"] = await source
                .SelectMany(t => t.SachTacGias)
                .CountAsync();
            ViewData["SoQuocTich"] = await source.Where(t => !string.IsNullOrEmpty(t.QuocTich)).Select(t => t.QuocTich).Distinct().CountAsync();
            q = q?.Trim();
            var query = source.Include(t => t.SachTacGias).AsQueryable();
            if (!string.IsNullOrWhiteSpace(q))
                query = query.Where(t => t.HoTen.Contains(q) || t.QuocTich.Contains(q) || t.TieuSu.Contains(q));
            ViewData["TuKhoa"] = q;
            var pagination = Pagination.Create(page, await query.CountAsync());
            ViewData["Pagination"] = pagination;
            return View(await query.OrderBy(t => t.HoTen)
                .Skip((pagination.Page - 1) * pagination.PageSize)
                .Take(pagination.PageSize)
                .ToListAsync());
        }

        // Xem chi tiết 1 tác giả - ai cũng xem được
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.TacGia == null)
            {
                return NotFound();
            }

            var tacGia = await _context.TacGia
                .FirstOrDefaultAsync(m => m.MaTacGia == id);
            if (tacGia == null)
            {
                return NotFound();
            }

            return View(tacGia);
        }

        // Hiển thị form thêm tác giả - chỉ Admin/NhanVien
        [Authorize(Roles = "Admin,NhanVien")]
        public IActionResult Create()
        {
            return View();
        }

        // Xử lý lưu tác giả mới - chỉ Admin/NhanVien
        [Authorize(Roles = "Admin,NhanVien")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("MaTacGia,HoTen,NgaySinh,QuocTich,TieuSu")] TacGia tacGia)
        {
            if (ModelState.IsValid)
            {
                _context.Add(tacGia);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(tacGia);
        }

        // Hiển thị form sửa tác giả - chỉ Admin/NhanVien
        [Authorize(Roles = "Admin,NhanVien")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.TacGia == null)
            {
                return NotFound();
            }

            var tacGia = await _context.TacGia
                .Include(t => t.SachTacGias)
                .ThenInclude(st => st.Sach)
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.MaTacGia == id);
            if (tacGia == null)
            {
                return NotFound();
            }
            ViewData["SachCuaTacGia"] = tacGia.SachTacGias
                .Where(st => st.Sach != null)
                .Select(st => st.Sach.TenSach)
                .OrderBy(name => name)
                .ToList();
            return View(tacGia);
        }

        // Xử lý cập nhật tác giả - chỉ Admin/NhanVien
        [Authorize(Roles = "Admin,NhanVien")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("MaTacGia,HoTen,NgaySinh,QuocTich,TieuSu")] TacGia tacGia)
        {
            if (id != tacGia.MaTacGia)
            {
                return NotFound();
            }

            tacGia.HoTen = tacGia.HoTen?.Trim() ?? string.Empty;
            tacGia.QuocTich = tacGia.QuocTich?.Trim() ?? string.Empty;
            tacGia.TieuSu = tacGia.TieuSu?.Trim() ?? string.Empty;
            if (tacGia.NgaySinh.HasValue && tacGia.NgaySinh.Value.Date > DateTime.Today)
                ModelState.AddModelError(nameof(tacGia.NgaySinh), "Ngày sinh không thể lớn hơn ngày hiện tại.");
            if (tacGia.NgaySinh.HasValue && tacGia.NgaySinh.Value.Date < DateTime.Today.AddYears(-120))
                ModelState.AddModelError(nameof(tacGia.NgaySinh), "Ngày sinh không hợp lệ (tuổi không được vượt quá 120).");

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(tacGia);
                    await _adminChangeNotification.ThemThongBaoAsync(
                        User,
                        "tác giả",
                        $"TG-{tacGia.MaTacGia:D5}",
                        Url.Action(nameof(Details), new { id = tacGia.MaTacGia })
                            ?? $"/TacGias/Details/{tacGia.MaTacGia}",
                        $"Tên tác giả: {tacGia.HoTen}.");
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TacGiaExists(tacGia.MaTacGia))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                TempData["Success"] = $"Đã cập nhật hồ sơ tác giả {tacGia.HoTen}.";
                return RedirectToAction(nameof(Index));
            }
            ViewData["SachCuaTacGia"] = await _context.TacGia
                .Where(t => t.MaTacGia == id)
                .SelectMany(t => t.SachTacGias)
                .Select(st => st.Sach.TenSach)
                .OrderBy(name => name)
                .ToListAsync();
            return View(tacGia);
        }

        // Hiển thị xác nhận xóa tác giả - chỉ Admin/NhanVien
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.TacGia == null)
            {
                return NotFound();
            }

            var tacGia = await _context.TacGia
                .FirstOrDefaultAsync(m => m.MaTacGia == id);
            if (tacGia == null)
            {
                return NotFound();
            }

            return View(tacGia);
        }

        // Xử lý xóa tác giả - chỉ Admin/NhanVien
        [Authorize(Roles = "Admin")]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.TacGia == null)
            {
                return Problem("Entity set 'bài_tập_1Context.TacGia'  is null.");
            }
            var tacGia = await _context.TacGia.FindAsync(id);
            if (tacGia != null)
            {
                _context.TacGia.Remove(tacGia);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool TacGiaExists(int id)
        {
            return (_context.TacGia?.Any(e => e.MaTacGia == id)).GetValueOrDefault();
        }
    }
}
