using bài_tập_1.Data;
using bài_tập_1.Models;
using bài_tập_1.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
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
        [AllowAnonymous]
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

        [Authorize(Roles = "Admin,NhanVien")]
        public async Task<IActionResult> Index(string? q, string? trangThai, int page = 1)
        {
            var query = _context.PhanHoi.AsNoTracking().AsQueryable();

            if (!string.IsNullOrWhiteSpace(q))
            {
                var keyword = q.Trim();
                query = query.Where(p =>
                    p.HoTen.Contains(keyword) ||
                    p.Email.Contains(keyword) ||
                    p.NoiDung.Contains(keyword));
            }

            if (!string.IsNullOrWhiteSpace(trangThai))
                query = query.Where(p => p.TrangThai == trangThai);

            ViewData["Search"] = q;
            ViewData["Status"] = trangThai;
            ViewData["Moi"] = await _context.PhanHoi.CountAsync(p => p.TrangThai == "Mới");
            ViewData["Tong"] = await _context.PhanHoi.CountAsync();
            ViewData["DaTraLoi"] = await _context.PhanHoi.CountAsync(p => p.NoiDungTraLoi != null);
            ViewData["DangXuLy"] = await _context.PhanHoi.CountAsync(p => p.TrangThai == "Đang xử lý");

            var pagination = Pagination.Create(page, await query.CountAsync());
            ViewData["Pagination"] = pagination;
            return View(await query
                .OrderBy(p => p.TrangThai != "Mới")
                .ThenByDescending(p => p.NgayGui)
                .Skip((pagination.Page - 1) * pagination.PageSize)
                .Take(pagination.PageSize)
                .ToListAsync());
        }

        [Authorize(Roles = "Admin,NhanVien")]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return NotFound();

            var phanHoi = await _context.PhanHoi
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.MaPhanHoi == id);
            return phanHoi == null ? NotFound() : View(phanHoi);
        }

        [Authorize(Roles = "DocGia")]
        public async Task<IActionResult> CuaToi()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(userId))
                return Challenge();

            var docGia = await _context.DocGia
                .AsNoTracking()
                .FirstOrDefaultAsync(d => d.UserId == userId);
            if (docGia == null)
                return NotFound();

            var email = docGia.Email.Trim().ToLowerInvariant();
            var items = await _context.PhanHoi
                .Where(p => p.UserId == userId ||
                    (p.UserId == null && p.Email.ToLower() == email))
                .OrderByDescending(p => p.NgayGui)
                .AsNoTracking()
                .ToListAsync();

            return View(items);
        }

        [Authorize(Roles = "Admin,NhanVien")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CapNhatTrangThai(
            int id,
            string trangThai)
        {
            var trangThaiHopLe = new[] { "Mới", "Đang xử lý", "Đã xử lý" };
            if (!trangThaiHopLe.Contains(trangThai))
                return BadRequest();

            var phanHoi = await _context.PhanHoi.FindAsync(id);
            if (phanHoi == null)
                return NotFound();

            phanHoi.TrangThai = trangThai;
            await _context.SaveChangesAsync();
            TempData["Success"] = "Đã cập nhật trạng thái phản hồi.";
            return RedirectToAction(nameof(Details), new { id });
        }

        [Authorize(Roles = "Admin,NhanVien")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> TraLoi(int id, string noiDung)
        {
            noiDung = noiDung?.Trim() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(noiDung))
                return BadRequest(new { message = "Vui lòng nhập nội dung trả lời." });
            if (noiDung.Length > 2000)
                return BadRequest(new { message = "Nội dung trả lời không được vượt quá 2.000 ký tự." });

            var phanHoi = await _context.PhanHoi.FindAsync(id);
            if (phanHoi == null)
                return NotFound(new { message = "Không tìm thấy phản hồi cần trả lời." });

            phanHoi.NoiDungTraLoi = noiDung;
            phanHoi.NgayTraLoi = DateTime.Now;
            phanHoi.NguoiTraLoi = User.Identity?.Name ?? "Nhân viên thư viện";
            phanHoi.TrangThai = "Đã xử lý";

            var docGia = !string.IsNullOrWhiteSpace(phanHoi.UserId)
                ? await _context.DocGia.FirstOrDefaultAsync(d => d.UserId == phanHoi.UserId)
                : null;
            docGia ??= await _context.DocGia.FirstOrDefaultAsync(d =>
                d.Email.ToLower() == phanHoi.Email.ToLower());

            var daGuiThongBao = docGia != null;
            if (docGia != null)
            {
                // Liên kết lại các phản hồi cũ gửi khi chưa đăng nhập bằng email tài khoản.
                phanHoi.UserId ??= docGia.UserId;
                var maSuKien = $"phan-hoi-tra-loi-{phanHoi.MaPhanHoi}";
                var tomTat = noiDung.Length > 440
                    ? $"{noiDung[..437]}..."
                    : noiDung;
                var thongBao = await _context.ThongBao.FirstOrDefaultAsync(t =>
                    t.MaDocGia == docGia.MaDocGia && t.MaSuKien == maSuKien);

                if (thongBao == null)
                {
                    _context.ThongBao.Add(new ThongBao
                    {
                        MaDocGia = docGia.MaDocGia,
                        MaSuKien = maSuKien,
                        TieuDe = $"Phản hồi PH-{phanHoi.MaPhanHoi:D4} đã được trả lời",
                        NoiDung = tomTat,
                        LienKet = $"/PhanHois/CuaToi#phan-hoi-{phanHoi.MaPhanHoi}",
                        Loai = "success",
                        NgayTao = DateTime.Now,
                        DaDoc = false,
                        DoiTuong = "DocGia",
                        SoNguoiNhan = 1
                    });
                }
                else
                {
                    thongBao.TieuDe = $"Phản hồi PH-{phanHoi.MaPhanHoi:D4} đã được cập nhật";
                    thongBao.NoiDung = tomTat;
                    thongBao.LienKet = $"/PhanHois/CuaToi#phan-hoi-{phanHoi.MaPhanHoi}";
                    thongBao.Loai = "success";
                    thongBao.NgayTao = DateTime.Now;
                    thongBao.DaDoc = false;
                }
            }

            await _context.SaveChangesAsync();

            return Json(new
            {
                success = true,
                message = "Đã lưu câu trả lời cho độc giả.",
                id = phanHoi.MaPhanHoi,
                answer = phanHoi.NoiDungTraLoi,
                answeredAt = phanHoi.NgayTraLoi.Value.ToString("HH:mm · dd/MM/yyyy"),
                answeredBy = phanHoi.NguoiTraLoi,
                status = phanHoi.TrangThai,
                recipientNotified = daGuiThongBao
            });
        }
    }
}
