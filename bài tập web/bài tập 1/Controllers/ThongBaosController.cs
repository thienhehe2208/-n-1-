using bài_tập_1.Data;
using bài_tập_1.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using bài_tập_1.Models.ViewModels;

namespace bài_tập_1.Controllers
{
    [Authorize(Roles = "DocGia")]
    public class ThongBaosController : Controller
    {
        private readonly bài_tập_1Context _context;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly ThongBaoService _thongBaoService;

        public ThongBaosController(
            bài_tập_1Context context,
            UserManager<IdentityUser> userManager,
            ThongBaoService thongBaoService)
        {
            _context = context;
            _userManager = userManager;
            _thongBaoService = thongBaoService;
        }

        public async Task<IActionResult> Index(int page = 1)
        {
            var docGia = await GetDocGiaAsync();
            if (docGia == null)
                return NotFound();

            await _thongBaoService.DongBoChoDocGiaAsync(docGia.MaDocGia);
            var query = _context.ThongBao
                .Where(t => t.MaDocGia == docGia.MaDocGia)
                .AsNoTracking();
            var pagination = Pagination.Create(page, await query.CountAsync());
            ViewData["Pagination"] = pagination;
            var items = await query
                .OrderBy(t => t.DaDoc)
                .ThenByDescending(t => t.NgayTao)
                .Skip((pagination.Page - 1) * pagination.PageSize)
                .Take(pagination.PageSize)
                .ToListAsync();

            return View(items);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DanhDauDaDoc(int id)
        {
            var docGia = await GetDocGiaAsync();
            if (docGia == null)
                return NotFound();

            var thongBao = await _context.ThongBao.FirstOrDefaultAsync(t =>
                t.MaThongBao == id && t.MaDocGia == docGia.MaDocGia);
            if (thongBao == null)
                return NotFound();

            thongBao.DaDoc = true;
            await _context.SaveChangesAsync();

            return string.IsNullOrWhiteSpace(thongBao.LienKet)
                ? RedirectToAction(nameof(Index))
                : LocalRedirect(thongBao.LienKet);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DanhDauTatCaDaDoc()
        {
            var docGia = await GetDocGiaAsync();
            if (docGia == null)
                return NotFound();

            var items = await _context.ThongBao
                .Where(t => t.MaDocGia == docGia.MaDocGia && !t.DaDoc)
                .ToListAsync();
            foreach (var item in items)
                item.DaDoc = true;

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private async Task<Models.DocGia?> GetDocGiaAsync()
        {
            var userId = _userManager.GetUserId(User);
            return await _context.DocGia.FirstOrDefaultAsync(d => d.UserId == userId);
        }
    }
}
