using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using bài_tập_1.Data;
using bài_tập_1.Models;
using bài_tập_1.Models.ViewModels;
using bài_tập_1.Services;

namespace bài_tập_1.Controllers
{
    // Trang tổng quan sau khi Admin/NhanVien đăng nhập
    [Authorize(Roles = "Admin,NhanVien")]
    public class DashboardController : Controller
    {
        private readonly bài_tập_1Context _context;
        private readonly PhieuMuonService _phieuMuonService;

        public DashboardController(
            bài_tập_1Context context,
            PhieuMuonService phieuMuonService)
        {
            _context = context;
            _phieuMuonService = phieuMuonService;
        }

        public async Task<IActionResult> Index()
        {
            await _phieuMuonService.CapNhatTrangThaiAsync();
            var homNay = DateTime.Now;

            var model = new DashboardViewModel
            {
                TongSoSach = await _context.Sach.CountAsync(),
                TongSoBanSao = await _context.BanSao.CountAsync(),
                TongSoDocGiaHoatDong = await _context.DocGia.CountAsync(d => d.TrangThai == TrangThaiDocGia.HoatDong),
                SoPhieuDangMuon = await _context.PhieuMuon.CountAsync(p =>
                    p.TrangThai == TrangThaiPhieuMuon.DangMuon),
                SoPhieuQuaHan = await _context.PhieuMuon.CountAsync(p =>
                    p.TrangThai == TrangThaiPhieuMuon.QuaHan),
                TongTienPhatChuaThu = await _context.PhieuPhat
                    .Where(p => p.TrangThai == TrangThaiPhieuPhat.ChuaDong)
                    .SumAsync(p => (decimal?)p.SoTien) ?? 0,
                PhieuQuaHanGanNhat = await _context.PhieuMuon
                    .Include(p => p.DocGia)
                    .Where(p => p.TrangThai == TrangThaiPhieuMuon.QuaHan)
                    .OrderBy(p => p.NgayHenTra)
                    .Take(5)
                    .ToListAsync()
            };

            if (User.IsInRole("Admin"))
            {
                var dauNgay = DateTime.Today;
                var dauNgaySau = dauNgay.AddDays(1);
                var dauThang = new DateTime(homNay.Year, homNay.Month, 1);
                var dauThangSau = dauThang.AddMonths(1);
                var dauNam = new DateTime(homNay.Year, 1, 1);
                var dauNamSau = dauNam.AddYears(1);
                var giaoDich = _context.GiaoDichThanhToan.AsNoTracking();

                model.HienBaoCaoDoanhThu = true;
                model.DoanhThuHomNay = await giaoDich
                    .Where(g => g.NgayThanhToan >= dauNgay &&
                                g.NgayThanhToan < dauNgaySau)
                    .SumAsync(g => (decimal?)g.TongTien) ?? 0;
                model.DoanhThuThangNay = await giaoDich
                    .Where(g => g.NgayThanhToan >= dauThang &&
                                g.NgayThanhToan < dauThangSau)
                    .SumAsync(g => (decimal?)g.TongTien) ?? 0;
                model.DoanhThuNamNay = await giaoDich
                    .Where(g => g.NgayThanhToan >= dauNam &&
                                g.NgayThanhToan < dauNamSau)
                    .SumAsync(g => (decimal?)g.TongTien) ?? 0;
                model.SachMuonNhieuNhat = await _context.ChiTietPhieuMuon
                    .AsNoTracking()
                    .Where(c => c.PhieuMuon.TrangThai !=
                                TrangThaiPhieuMuon.Nhap)
                    .GroupBy(c => new
                    {
                        c.BanSao.MaSach,
                        c.BanSao.Sach.TenSach
                    })
                    .Select(g => new SachMuonNhieuViewModel
                    {
                        MaSach = g.Key.MaSach,
                        TenSach = g.Key.TenSach,
                        SoLuotMuon = g.Count()
                    })
                    .OrderByDescending(s => s.SoLuotMuon)
                    .ThenBy(s => s.TenSach)
                    .Take(5)
                    .ToListAsync();
            }

            return View(model);
        }
    }
}
