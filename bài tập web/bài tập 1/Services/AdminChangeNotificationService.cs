using System.Security.Claims;
using bài_tập_1.Data;
using bài_tập_1.Models;
using Microsoft.EntityFrameworkCore;

namespace bài_tập_1.Services
{
    public class AdminChangeNotificationService
    {
        private readonly bài_tập_1Context _context;

        public AdminChangeNotificationService(bài_tập_1Context context)
        {
            _context = context;
        }

        public async Task ThemThongBaoAsync(
            ClaimsPrincipal user,
            string doiTuong,
            string maDoiTuong,
            string lienKet,
            string? chiTiet = null)
        {
            if (!user.IsInRole("NhanVien") || user.IsInRole("Admin"))
                return;

            var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
            var tenNhanVien = await _context.NhanVien
                .Where(n => n.UserId == userId)
                .Select(n => n.HoTen)
                .FirstOrDefaultAsync() ?? user.Identity?.Name ?? "Nhân viên";

            var noiDung =
                $"Nhân viên {tenNhanVien} đã chỉnh sửa {doiTuong} {maDoiTuong}.";
            if (!string.IsNullOrWhiteSpace(chiTiet))
                noiDung += $" {chiTiet.Trim()}";

            _context.ThongBao.Add(new ThongBao
            {
                MaSuKien = $"staff-edit-{Guid.NewGuid():N}",
                TieuDe = $"Nhân viên đã chỉnh sửa {doiTuong}",
                NoiDung = noiDung,
                LienKet = lienKet,
                Loai = "warning",
                NgayTao = DateTime.Now,
                DaDoc = false,
                LaThongBaoAdmin = false,
                DoiTuong = "Admin",
                SoNguoiNhan = 1
            });
        }
    }
}
