using bài_tập_1.Data;
using bài_tập_1.Models;
using bài_tập_1.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace bài_tập_1.Controllers
{
    [Authorize(Roles = "Admin")]
    public class GiaoDichThanhToansController : Controller
    {
        private readonly bài_tập_1Context _context;

        public GiaoDichThanhToansController(bài_tập_1Context context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(
            DateTime? tuNgay,
            DateTime? denNgay,
            int? maNhanVien,
            PhuongThucThanhToan? phuongThuc,
            int page = 1)
        {
            var query = _context.GiaoDichThanhToan
                .Include(g => g.NhanVienXacNhan)
                .Include(g => g.PhieuMuon)
                    .ThenInclude(p => p.DocGia)
                .AsNoTracking()
                .AsQueryable();

            if (tuNgay.HasValue)
                query = query.Where(g => g.NgayThanhToan >= tuNgay.Value.Date);
            if (denNgay.HasValue)
                query = query.Where(g =>
                    g.NgayThanhToan < denNgay.Value.Date.AddDays(1));
            if (maNhanVien.HasValue)
                query = query.Where(g =>
                    g.MaNhanVienXacNhan == maNhanVien.Value);
            if (phuongThuc.HasValue)
                query = query.Where(g => g.PhuongThuc == phuongThuc.Value);

            ViewData["TuNgay"] = tuNgay?.ToString("yyyy-MM-dd");
            ViewData["DenNgay"] = denNgay?.ToString("yyyy-MM-dd");
            ViewData["MaNhanVien"] = maNhanVien;
            ViewData["PhuongThuc"] = phuongThuc;
            ViewData["TongDoanhThu"] =
                await query.SumAsync(g => (decimal?)g.TongTien) ?? 0;
            ViewData["TongPhiThue"] =
                await query.SumAsync(g => (decimal?)g.PhiThue) ?? 0;
            ViewData["TongTienPhat"] =
                await query.SumAsync(g => (decimal?)g.TienPhat) ?? 0;
            ViewData["NhanViens"] = new SelectList(
                await _context.NhanVien.AsNoTracking()
                    .OrderBy(n => n.HoTen)
                    .ToListAsync(),
                nameof(NhanVien.MaNhanVien),
                nameof(NhanVien.HoTen),
                maNhanVien);

            var pagination = Pagination.Create(page, await query.CountAsync(), 15);
            ViewData["Pagination"] = pagination;

            var items = await query
                .OrderByDescending(g => g.NgayThanhToan)
                .ThenByDescending(g => g.MaGiaoDich)
                .Skip((pagination.Page - 1) * pagination.PageSize)
                .Take(pagination.PageSize)
                .ToListAsync();

            return View(items);
        }
    }
}
