using bài_tập_1.Data;
using bài_tập_1.Models;
using bài_tập_1.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace bài_tập_1.ViewComponents
{
    public class AdminLoanNotificationViewComponent : ViewComponent
    {
        private readonly bài_tập_1Context _context;

        public AdminLoanNotificationViewComponent(bài_tập_1Context context)
        {
            _context = context;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var rows = await _context.YeuCauMuonOnline
                .Include(y => y.DocGia)
                .Include(y => y.Sach)
                .Where(y =>
                    y.TrangThai == TrangThaiYeuCauMuonOnline.ChoNhan &&
                    y.HanNhanSach >= DateTime.Now)
                .OrderByDescending(y => y.NgayTao)
                .AsNoTracking()
                .ToListAsync();

            var groups = rows
                .GroupBy(y => y.MaXacNhan)
                .Select(group =>
                {
                    var first = group.First();
                    return new AdminLoanNotificationItemViewModel
                    {
                        MaXacNhan = group.Key,
                        TenDocGia = first.DocGia.HoTen,
                        NgayTao = first.NgayTao,
                        HanNhanSach = first.HanNhanSach,
                        TenSaches = group.Select(y => y.Sach.TenSach).ToList()
                    };
                })
                .OrderByDescending(item => item.NgayTao)
                .ToList();

            var thayDoiQuery = _context.ThongBao
                .Where(t =>
                    t.DoiTuong == "Admin" &&
                    !t.LaThongBaoAdmin)
                .AsNoTracking();

            var thayDoiMoi = await thayDoiQuery
                .OrderBy(t => t.DaDoc)
                .ThenByDescending(t => t.NgayTao)
                .Take(5)
                .ToListAsync();

            return View(new AdminLoanNotificationViewModel
            {
                TongPhieuChoNhan = groups.Count,
                PhieuMoi = groups.Take(5).ToList(),
                TongThayDoiChuaDoc = await thayDoiQuery.CountAsync(t => !t.DaDoc),
                ThayDoiMoi = thayDoiMoi
            });
        }
    }
}
