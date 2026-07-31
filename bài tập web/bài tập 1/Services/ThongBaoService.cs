using bài_tập_1.Data;
using bài_tập_1.Models;
using Microsoft.EntityFrameworkCore;

namespace bài_tập_1.Services
{
    public class ThongBaoService
    {
        private readonly bài_tập_1Context _context;

        public ThongBaoService(bài_tập_1Context context)
        {
            _context = context;
        }

        public async Task DongBoChoDocGiaAsync(int maDocGia)
        {
            var homNay = DateTime.Today;
            var thayDoi = false;

            var luotMuonCanNhac = await _context.ChiTietPhieuMuon
                .Include(c => c.PhieuMuon)
                .Include(c => c.BanSao)
                    .ThenInclude(b => b.Sach)
                .Where(c =>
                    c.PhieuMuon.MaDocGia == maDocGia &&
                    c.NgayTra == null &&
                    c.PhieuMuon.NgayHenTra.Date <= homNay.AddDays(7))
                .AsNoTracking()
                .ToListAsync();

            foreach (var item in luotMuonCanNhac)
            {
                var soNgay = (item.PhieuMuon.NgayHenTra.Date - homNay).Days;
                var maSuKien = $"muon-{item.MaChiTiet}-han-tra";
                var thongBao = await _context.ThongBao.FirstOrDefaultAsync(t =>
                    t.MaDocGia == maDocGia && t.MaSuKien == maSuKien);

                var noiDung = soNgay < 0
                    ? $"Sách “{item.BanSao.Sach.TenSach}” đã quá hạn {-soNgay} ngày."
                    : $"Sách “{item.BanSao.Sach.TenSach}” còn {soNgay} ngày đến hạn trả.";
                var loai = soNgay < 0 ? "danger" : "warning";

                if (thongBao == null)
                {
                    _context.ThongBao.Add(new ThongBao
                    {
                        MaDocGia = maDocGia,
                        MaSuKien = maSuKien,
                        TieuDe = soNgay < 0 ? "Sách quá hạn" : "Sắp đến hạn trả",
                        NoiDung = noiDung,
                        Loai = loai,
                        LienKet = "/Profile/LichSuMuon"
                    });
                    thayDoi = true;
                }
                else if (thongBao.NoiDung != noiDung || thongBao.Loai != loai)
                {
                    thongBao.NoiDung = noiDung;
                    thongBao.Loai = loai;
                    thongBao.NgayTao = DateTime.Now;
                    thongBao.DaDoc = false;
                    thayDoi = true;
                }
            }

            var datTruocs = await _context.DatTruoc
                .Include(d => d.Sach)
                .Where(d =>
                    d.MaDocGia == maDocGia &&
                    d.TrangThai == TrangThaiDatTruoc.DaCoSach)
                .AsNoTracking()
                .ToListAsync();

            foreach (var item in datTruocs)
            {
                var maSuKien = $"dat-truoc-{item.MaDatTruoc}-san-sang";
                if (await _context.ThongBao.AnyAsync(t =>
                        t.MaDocGia == maDocGia && t.MaSuKien == maSuKien))
                    continue;

                _context.ThongBao.Add(new ThongBao
                {
                    MaDocGia = maDocGia,
                    MaSuKien = maSuKien,
                    TieuDe = "Sách đặt trước đã sẵn sàng",
                    NoiDung = $"Sách “{item.Sach.TenSach}” đã sẵn sàng để nhận.",
                    Loai = "success",
                    LienKet = $"/DatTruocs/Details/{item.MaDatTruoc}"
                });
                thayDoi = true;
            }

            if (thayDoi)
                await _context.SaveChangesAsync();
        }
    }
}
