using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using bài_tập_1.Data;
using bài_tập_1.Models;
using bài_tập_1.Models.ViewModels;

namespace bài_tập_1.Controllers
{
    // Trang tổng quan sau khi Admin/NhanVien đăng nhập
    [Authorize(Roles = "Admin,NhanVien")]
    public class DashboardController : Controller
    {
        private readonly bài_tập_1Context _context;

        public DashboardController(bài_tập_1Context context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var homNay = DateTime.Now;

            var model = new DashboardViewModel
            {
                TongSoSach = await _context.Sach.CountAsync(),
                TongSoBanSao = await _context.BanSao.CountAsync(),
                TongSoDocGiaHoatDong = await _context.DocGia.CountAsync(d => d.TrangThai == TrangThaiDocGia.HoatDong),
                SoPhieuDangMuon = await _context.PhieuMuon.CountAsync(p => p.TrangThai == TrangThaiPhieuMuon.DangMuon),
                SoPhieuQuaHan = await _context.PhieuMuon.CountAsync(p =>
                    p.TrangThai == TrangThaiPhieuMuon.DangMuon && p.NgayHenTra < homNay),
                TongTienPhatChuaThu = await _context.PhieuPhat
                    .Where(p => p.TrangThai == TrangThaiPhieuPhat.ChuaDong)
                    .SumAsync(p => (decimal?)p.SoTien) ?? 0,
                PhieuQuaHanGanNhat = await _context.PhieuMuon
                    .Include(p => p.DocGia)
                    .Where(p => p.TrangThai == TrangThaiPhieuMuon.DangMuon && p.NgayHenTra < homNay)
                    .OrderBy(p => p.NgayHenTra)
                    .Take(5)
                    .ToListAsync()
            };

            return View(model);
        }
    }
}