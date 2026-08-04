using bài_tập_1.Data;
using bài_tập_1.Models;
using bài_tập_1.Models.ViewModels;
using bài_tập_1.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace bài_tập_1.Controllers
{
    [Authorize(Roles = "Admin,NhanVien,DocGia")]
    public class ThongBaosController : Controller
    {
        private readonly bài_tập_1Context _context;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly ThongBaoService _service;

        public ThongBaosController(bài_tập_1Context context, UserManager<IdentityUser> userManager, ThongBaoService service)
        {
            _context = context;
            _userManager = userManager;
            _service = service;
        }

        public async Task<IActionResult> Index(int page = 1)
        {
            IQueryable<ThongBao> query;
            if (User.IsInRole("Admin"))
            {
                query = _context.ThongBao.Where(t => t.LaThongBaoAdmin)
                    .Where(t => t.MaThongBao == _context.ThongBao
                        .Where(x => x.MaBanTin == t.MaBanTin)
                        .Min(x => x.MaThongBao));
                ViewData["IsAdminOutbox"] = true;
            }
            else if (User.IsInRole("DocGia"))
            {
                var docGia = await GetDocGiaAsync();
                if (docGia == null) return NotFound();
                await _service.DongBoChoDocGiaAsync(docGia.MaDocGia);
                query = _context.ThongBao.Where(t => t.MaDocGia == docGia.MaDocGia);
            }
            else
            {
                var nhanVien = await GetNhanVienAsync();
                if (nhanVien == null) return NotFound();
                query = _context.ThongBao.Where(t => t.MaNhanVien == nhanVien.MaNhanVien);
            }

            query = query.AsNoTracking();
            var pagination = Pagination.Create(page, await query.CountAsync());
            ViewData["Pagination"] = pagination;
            var items = await query.OrderBy(t => t.DaDoc).ThenByDescending(t => t.NgayTao)
                .Skip((pagination.Page - 1) * pagination.PageSize).Take(pagination.PageSize).ToListAsync();
            return View(items);
        }

        [Authorize(Roles = "Admin")]
        public IActionResult Create() => View(new TaoThongBaoViewModel());

        [HttpPost, ValidateAntiForgeryToken, Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create(TaoThongBaoViewModel model)
        {
            if (!new[] { "TatCa", "DocGia", "NhanVien" }.Contains(model.DoiTuong))
                ModelState.AddModelError(nameof(model.DoiTuong), "Đối tượng nhận không hợp lệ.");
            if (!new[] { "info", "success", "warning", "danger" }.Contains(model.Loai))
                ModelState.AddModelError(nameof(model.Loai), "Mức độ thông báo không hợp lệ.");
            if (!string.IsNullOrWhiteSpace(model.LienKet) && !Url.IsLocalUrl(model.LienKet))
                ModelState.AddModelError(nameof(model.LienKet), "Liên kết phải là đường dẫn nội bộ, ví dụ /Saches.");
            if (!ModelState.IsValid) return View(model);

            var batchId = (await _context.ThongBao.MaxAsync(t => (int?)t.MaBanTin) ?? 0) + 1;
            var now = DateTime.Now;
            var items = new List<ThongBao>();
            if (model.DoiTuong is "TatCa" or "DocGia")
                items.AddRange((await _context.DocGia.AsNoTracking().Select(d => d.MaDocGia).ToListAsync()).Select(id => TaoBanSao(model, batchId, now, maDocGia: id)));
            if (model.DoiTuong is "TatCa" or "NhanVien")
                items.AddRange((await _context.NhanVien.AsNoTracking().Select(n => n.MaNhanVien).ToListAsync()).Select(id => TaoBanSao(model, batchId, now, maNhanVien: id)));

            foreach (var item in items) item.SoNguoiNhan = items.Count;

            _context.ThongBao.AddRange(items);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = $"Đã gửi thông báo đến {items.Count} tài khoản.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> DanhDauDaDoc(int id, string? returnUrl = null)
        {
            var item = await TimThongBaoCuaNguoiDungAsync(id);
            if (item == null) return NotFound();
            item.DaDoc = true;
            await _context.SaveChangesAsync();
            if (!string.IsNullOrWhiteSpace(item.LienKet) && Url.IsLocalUrl(item.LienKet))
                return LocalRedirect(item.LienKet);
            if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
                return LocalRedirect(returnUrl);
            return RedirectToAction(nameof(Index));
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> DanhDauTatCaDaDoc(string? returnUrl = null)
        {
            IQueryable<ThongBao> query = _context.ThongBao.Where(t => !t.DaDoc);
            if (User.IsInRole("DocGia")) { var d = await GetDocGiaAsync(); if (d == null) return NotFound(); query = query.Where(t => t.MaDocGia == d.MaDocGia); }
            else { var n = await GetNhanVienAsync(); if (n == null) return NotFound(); query = query.Where(t => t.MaNhanVien == n.MaNhanVien); }
            foreach (var item in await query.ToListAsync()) item.DaDoc = true;
            await _context.SaveChangesAsync();
            if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
                return LocalRedirect(returnUrl);
            return RedirectToAction(nameof(Index));
        }

        private ThongBao TaoBanSao(TaoThongBaoViewModel m, int batchId, DateTime now, int? maDocGia = null, int? maNhanVien = null) => new()
        {
            MaDocGia = maDocGia,
            MaNhanVien = maNhanVien,
            MaBanTin = batchId,
            MaSuKien = $"admin-{batchId}",
            TieuDe = m.TieuDe.Trim(),
            NoiDung = m.NoiDung.Trim(),
            LienKet = m.LienKet?.Trim() ?? string.Empty,
            Loai = m.Loai,
            NgayTao = now,
            LaThongBaoAdmin = true,
            DoiTuong = m.DoiTuong
        };

        private async Task<ThongBao?> TimThongBaoCuaNguoiDungAsync(int id)
        {
            if (User.IsInRole("Admin")) return null;
            if (User.IsInRole("DocGia")) { var d = await GetDocGiaAsync(); return d == null ? null : await _context.ThongBao.FirstOrDefaultAsync(t => t.MaThongBao == id && t.MaDocGia == d.MaDocGia); }
            var n = await GetNhanVienAsync(); return n == null ? null : await _context.ThongBao.FirstOrDefaultAsync(t => t.MaThongBao == id && t.MaNhanVien == n.MaNhanVien);
        }

        private async Task<DocGia?> GetDocGiaAsync() { var id = _userManager.GetUserId(User); return await _context.DocGia.FirstOrDefaultAsync(d => d.UserId == id); }
        private async Task<NhanVien?> GetNhanVienAsync() { var id = _userManager.GetUserId(User); return await _context.NhanVien.FirstOrDefaultAsync(n => n.UserId == id); }
    }
}
