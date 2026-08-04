using bài_tập_1.Data;
using bài_tập_1.Models.ViewModels;
using bài_tập_1.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace bài_tập_1.ViewComponents
{
    public class ReaderHeaderViewComponent : ViewComponent
    {
        private readonly bài_tập_1Context _context;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly ThongBaoService _thongBaoService;

        public ReaderHeaderViewComponent(
            bài_tập_1Context context,
            UserManager<IdentityUser> userManager,
            ThongBaoService thongBaoService)
        {
            _context = context;
            _userManager = userManager;
            _thongBaoService = thongBaoService;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var userId = _userManager.GetUserId(HttpContext.User);
            var email = HttpContext.User.Identity?.Name ?? "Tài khoản";
            var docGia = string.IsNullOrWhiteSpace(userId)
                ? null
                : await _context.DocGia.AsNoTracking()
                    .FirstOrDefaultAsync(d => d.UserId == userId);

            if (docGia == null)
            {
                return View(new ReaderHeaderViewModel
                {
                    DisplayName = email,
                    Email = email,
                    Initials = TaoChuCaiDau(email)
                });
            }

            await _thongBaoService.DongBoChoDocGiaAsync(docGia.MaDocGia);

            var query = _context.ThongBao
                .Where(t => t.MaDocGia == docGia.MaDocGia)
                .AsNoTracking();

            var notifications = await query
                .OrderBy(t => t.DaDoc)
                .ThenByDescending(t => t.NgayTao)
                .Take(6)
                .ToListAsync();

            return View(new ReaderHeaderViewModel
            {
                DisplayName = docGia.HoTen,
                Email = docGia.Email,
                Initials = TaoChuCaiDau(docGia.HoTen),
                UnreadCount = await query.CountAsync(t => !t.DaDoc),
                Notifications = notifications
            });
        }

        private static string TaoChuCaiDau(string value)
        {
            var words = value.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (words.Length == 0) return "DG";
            if (words.Length == 1) return words[0][..1].ToUpperInvariant();
            return $"{words[0][0]}{words[^1][0]}".ToUpperInvariant();
        }
    }
}
