using System.Security.Claims;
using bài_tập_1.Data;
using bài_tập_1.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using bài_tập_1.Models.ViewModels;

namespace bài_tập_1.Controllers
{
    [Authorize]
    public class YeuThichsController : Controller
    {
        private readonly bài_tập_1Context _context;

        public YeuThichsController(bài_tập_1Context context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(int page = 1)
        {
            var docGia = await GetDocGiaAsync();
            if (docGia == null)
                return RedirectToAction("Index", "Profile");

            var query = _context.YeuThich
                .Where(y => y.MaDocGia == docGia.MaDocGia)
                .Include(y => y.Sach)
                    .ThenInclude(s => s.TheLoai)
                .Include(y => y.Sach)
                    .ThenInclude(s => s.NhaXuatBan)
                .Include(y => y.Sach)
                    .ThenInclude(s => s.BanSaos)
                .AsNoTracking();
            var pagination = Pagination.Create(page, await query.CountAsync());
            ViewData["Pagination"] = pagination;
            var favorites = await query
                .OrderByDescending(y => y.NgayThem)
                .Skip((pagination.Page - 1) * pagination.PageSize)
                .Take(pagination.PageSize)
                .ToListAsync();

            return View(favorites);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Toggle(int maSach, string? returnUrl)
        {
            var docGia = await GetDocGiaAsync();
            if (docGia == null)
                return RedirectToAction("Index", "Profile");

            if (!await _context.Sach.AnyAsync(s => s.MaSach == maSach))
                return NotFound();

            var favorite = await _context.YeuThich.FirstOrDefaultAsync(y =>
                y.MaDocGia == docGia.MaDocGia && y.MaSach == maSach);

            if (favorite == null)
            {
                _context.YeuThich.Add(new YeuThich
                {
                    MaDocGia = docGia.MaDocGia,
                    MaSach = maSach,
                    NgayThem = DateTime.Now
                });
                TempData["Success"] = "Đã thêm sách vào danh mục yêu thích.";
            }
            else
            {
                _context.YeuThich.Remove(favorite);
                TempData["Success"] = "Đã xóa sách khỏi danh mục yêu thích.";
            }

            await _context.SaveChangesAsync();

            if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
                return LocalRedirect(returnUrl);

            return RedirectToAction(nameof(Index));
        }

        private async Task<DocGia?> GetDocGiaAsync()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return string.IsNullOrWhiteSpace(userId)
                ? null
                : await _context.DocGia.FirstOrDefaultAsync(d => d.UserId == userId);
        }
    }
}
