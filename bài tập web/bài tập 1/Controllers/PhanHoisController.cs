using bài_tập_1.Data;
using bài_tập_1.Models;
using bài_tập_1.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace bài_tập_1.Controllers
{
    public class PhanHoisController : Controller
    {
        private readonly bài_tập_1Context _context;

        public PhanHoisController(bài_tập_1Context context)
        {
            _context = context;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(PhanHoiViewModel model)
        {
            var returnUrl = Url.IsLocalUrl(model.ReturnUrl)
                ? model.ReturnUrl!
                : Url.Action("Index", "Home")!;

            if (!ModelState.IsValid)
            {
                TempData["Error"] = string.Join(" ", ModelState.Values
                    .SelectMany(value => value.Errors)
                    .Select(error => error.ErrorMessage)
                    .Where(message => !string.IsNullOrWhiteSpace(message)));
                TempData["OpenFeedbackModal"] = true;
                return LocalRedirect(returnUrl);
            }

            _context.PhanHoi.Add(new PhanHoi
            {
                UserId = User.FindFirstValue(ClaimTypes.NameIdentifier),
                HoTen = model.HoTen.Trim(),
                Email = model.Email.Trim().ToLowerInvariant(),
                NoiDung = model.NoiDung.Trim(),
                NgayGui = DateTime.Now,
                TrangThai = "Mới"
            });

            await _context.SaveChangesAsync();
            TempData["Success"] = "Cảm ơn bạn! Phản hồi đã được gửi đến thư viện.";
            return LocalRedirect(returnUrl);
        }
    }
}
