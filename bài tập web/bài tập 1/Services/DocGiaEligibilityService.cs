using bài_tập_1.Data;
using bài_tập_1.Models;
using Microsoft.EntityFrameworkCore;

namespace bài_tập_1.Services
{
    public sealed class KiemTraDocGiaOptions
    {
        public bool KiemTraGioiHanSach { get; init; } = true;
        public int? MaSach { get; init; }
        public int? BoQuaMaDatTruoc { get; init; }
        public int? BoQuaMaYeuCauOnline { get; init; }
    }

    public class DocGiaEligibilityService
    {
        private readonly bài_tập_1Context _context;

        public DocGiaEligibilityService(bài_tập_1Context context)
        {
            _context = context;
        }

        public async Task<IReadOnlyList<string>> KiemTraAsync(
            int maDocGia,
            KiemTraDocGiaOptions? options = null)
        {
            options ??= new KiemTraDocGiaOptions();
            var errors = new List<string>();
            var docGia = await _context.DocGia
                .AsNoTracking()
                .FirstOrDefaultAsync(d => d.MaDocGia == maDocGia);

            if (docGia == null)
            {
                errors.Add("Không tìm thấy hồ sơ độc giả.");
                return errors;
            }

            if (docGia.TrangThai != TrangThaiDocGia.HoatDong)
                errors.Add("Thẻ độc giả đang bị khóa.");

            if (docGia.NgayHetHanThe < DateTime.Today)
                errors.Add("Thẻ độc giả đã hết hạn.");

            var conNoPhat = await _context.PhieuPhat.AnyAsync(p =>
                p.TrangThai == TrangThaiPhieuPhat.ChuaDong &&
                p.ChiTietPhieuMuon.PhieuMuon.MaDocGia == maDocGia);
            if (conNoPhat)
                errors.Add("Độc giả còn phiếu phạt chưa thanh toán.");

            var coSachQuaHan = await _context.ChiTietPhieuMuon.AnyAsync(c =>
                c.PhieuMuon.MaDocGia == maDocGia &&
                c.NgayTra == null &&
                c.PhieuMuon.NgayHenTra < DateTime.Today);
            if (coSachQuaHan)
                errors.Add("Độc giả đang có sách quá hạn chưa trả.");

            if (options.KiemTraGioiHanSach)
            {
                var soSachDangMuon = await _context.ChiTietPhieuMuon.CountAsync(c =>
                    c.PhieuMuon.MaDocGia == maDocGia && c.NgayTra == null);
                var soYeuCauDangGiu = await _context.YeuCauMuonOnline.CountAsync(y =>
                    y.MaDocGia == maDocGia &&
                    y.TrangThai == TrangThaiYeuCauMuonOnline.ChoNhan &&
                    (!options.BoQuaMaYeuCauOnline.HasValue ||
                     y.MaYeuCau != options.BoQuaMaYeuCauOnline.Value));

                if (soSachDangMuon + soYeuCauDangGiu >=
                    LibraryRules.SoSachMuonToiDa)
                {
                    errors.Add(
                        $"Độc giả đã đạt giới hạn {LibraryRules.SoSachMuonToiDa} sách đang mượn hoặc chờ nhận.");
                }
            }

            if (options.MaSach.HasValue)
            {
                var maSach = options.MaSach.Value;
                var dangMuonCungDauSach = await _context.ChiTietPhieuMuon
                    .AnyAsync(c =>
                        c.PhieuMuon.MaDocGia == maDocGia &&
                        c.BanSao.MaSach == maSach &&
                        c.NgayTra == null);

                var datTruocCungDauSach = await _context.DatTruoc.AnyAsync(d =>
                    d.MaDocGia == maDocGia &&
                    d.MaSach == maSach &&
                    (d.TrangThai == TrangThaiDatTruoc.DangCho ||
                     d.TrangThai == TrangThaiDatTruoc.DaCoSach) &&
                    (!options.BoQuaMaDatTruoc.HasValue ||
                     d.MaDatTruoc != options.BoQuaMaDatTruoc.Value));

                var onlineCungDauSach = await _context.YeuCauMuonOnline
                    .AnyAsync(y =>
                        y.MaDocGia == maDocGia &&
                        y.MaSach == maSach &&
                        y.TrangThai == TrangThaiYeuCauMuonOnline.ChoNhan &&
                        (!options.BoQuaMaYeuCauOnline.HasValue ||
                         y.MaYeuCau != options.BoQuaMaYeuCauOnline.Value));

                if (dangMuonCungDauSach || datTruocCungDauSach ||
                    onlineCungDauSach)
                {
                    errors.Add(
                        "Độc giả đang mượn, đặt trước hoặc chờ nhận đầu sách này.");
                }
            }

            return errors.Distinct().ToList();
        }
    }
}
