using bài_tập_1.Data;
using bài_tập_1.Models;
using Microsoft.EntityFrameworkCore;

namespace bài_tập_1.Services
{
    public class DatTruocService
    {
        public const int SoNgayToiDaTrongHangDoi = 30;
        public const int SoNgayGiuSach = 3;

        private readonly bài_tập_1Context _context;

        public DatTruocService(bài_tập_1Context context)
        {
            _context = context;
        }

        public async Task XuLyHetHanAsync(int? maSach = null)
        {
            var now = DateTime.Now;

            var waitingQuery = _context.DatTruoc
                .Include(d => d.DocGia)
                .Where(d => d.TrangThai == TrangThaiDatTruoc.DangCho);

            if (maSach.HasValue)
                waitingQuery = waitingQuery.Where(d => d.MaSach == maSach.Value);

            var invalidWaiting = await waitingQuery
                .Where(d =>
                    (d.NgayHetHanDat.HasValue && d.NgayHetHanDat < now) ||
                    d.DocGia.TrangThai != TrangThaiDocGia.HoatDong ||
                    d.DocGia.NgayHetHanThe < DateTime.Today ||
                    _context.PhieuPhat.Any(p =>
                        p.TrangThai == TrangThaiPhieuPhat.ChuaDong &&
                        p.ChiTietPhieuMuon.PhieuMuon.MaDocGia == d.MaDocGia) ||
                    _context.ChiTietPhieuMuon.Any(c =>
                        c.PhieuMuon.MaDocGia == d.MaDocGia &&
                        c.NgayTra == null &&
                        c.PhieuMuon.NgayHenTra < DateTime.Today))
                .ToListAsync();

            foreach (var item in invalidWaiting)
                item.TrangThai = TrangThaiDatTruoc.HetHan;

            var readyQuery = _context.DatTruoc
                .Include(d => d.BanSaoDuocGiu)
                .Where(d =>
                    d.TrangThai == TrangThaiDatTruoc.DaCoSach &&
                    d.HanNhanSach.HasValue &&
                    d.HanNhanSach < now);

            if (maSach.HasValue)
                readyQuery = readyQuery.Where(d => d.MaSach == maSach.Value);

            var expiredReady = await readyQuery
                .OrderBy(d => d.HanNhanSach)
                .ToListAsync();

            var copiesToReassign = new List<(BanSao Copy, int MaSach)>();
            foreach (var item in expiredReady)
            {
                var copy = item.BanSaoDuocGiu;
                item.TrangThai = TrangThaiDatTruoc.HetHan;
                item.MaBanSaoDuocGiu = null;

                if (copy != null)
                    copiesToReassign.Add((copy, item.MaSach));
            }

            // Giải phóng liên kết giữ cũ trước để không vi phạm chỉ mục duy nhất
            // khi cùng bản sao được chuyển ngay cho người tiếp theo.
            await _context.SaveChangesAsync();

            foreach (var item in copiesToReassign)
                await GanChoNguoiTiepTheoAsync(item.Copy, item.MaSach);

            if (copiesToReassign.Count > 0)
                await _context.SaveChangesAsync();
        }

        public async Task<bool> PhanBoBanSaoAsync(BanSao banSao)
        {
            await XuLyHetHanAsync(banSao.MaSach);

            if (banSao.TinhTrang != TinhTrangBanSao.SanCo)
                return false;

            var assigned = await GanChoNguoiTiepTheoAsync(
                banSao,
                banSao.MaSach);

            await _context.SaveChangesAsync();
            return assigned;
        }

        public async Task HuyVaChuyenLuotAsync(DatTruoc datTruoc)
        {
            var copy = datTruoc.TrangThai == TrangThaiDatTruoc.DaCoSach
                ? datTruoc.BanSaoDuocGiu
                : null;

            datTruoc.TrangThai = TrangThaiDatTruoc.DaHuy;
            datTruoc.MaBanSaoDuocGiu = null;

            // Xóa lượt giữ hiện tại trước khi cấp cùng bản sao cho lượt kế tiếp.
            await _context.SaveChangesAsync();

            if (copy != null)
                await GanChoNguoiTiepTheoAsync(copy, datTruoc.MaSach);

            await _context.SaveChangesAsync();
        }

        private async Task<bool> GanChoNguoiTiepTheoAsync(
            BanSao banSao,
            int maSach)
        {
            var next = await _context.DatTruoc
                .Include(d => d.DocGia)
                .Where(d =>
                    d.MaSach == maSach &&
                    d.TrangThai == TrangThaiDatTruoc.DangCho &&
                    d.DocGia.TrangThai == TrangThaiDocGia.HoatDong &&
                    d.DocGia.NgayHetHanThe >= DateTime.Today &&
                    (!d.NgayHetHanDat.HasValue || d.NgayHetHanDat >= DateTime.Now) &&
                    !_context.PhieuPhat.Any(p =>
                        p.TrangThai == TrangThaiPhieuPhat.ChuaDong &&
                        p.ChiTietPhieuMuon.PhieuMuon.MaDocGia == d.MaDocGia) &&
                    !_context.ChiTietPhieuMuon.Any(c =>
                        c.PhieuMuon.MaDocGia == d.MaDocGia &&
                        c.NgayTra == null &&
                        c.PhieuMuon.NgayHenTra < DateTime.Today))
                .OrderBy(d => d.NgayDat)
                .ThenBy(d => d.MaDatTruoc)
                .FirstOrDefaultAsync();

            if (next == null)
            {
                banSao.TinhTrang = TinhTrangBanSao.SanCo;
                return false;
            }

            next.MaBanSaoDuocGiu = banSao.MaBanSao;
            next.NgaySanSang = DateTime.Now;
            next.HanNhanSach = DateTime.Now.AddDays(SoNgayGiuSach);
            next.TrangThai = TrangThaiDatTruoc.DaCoSach;
            banSao.TinhTrang = TinhTrangBanSao.DaGiu;

            var eventCode = $"dat-truoc-ready:{next.MaDatTruoc}";
            if (!await _context.ThongBao.AnyAsync(t =>
                    t.MaDocGia == next.MaDocGia && t.MaSuKien == eventCode))
            {
                _context.ThongBao.Add(new ThongBao
                {
                    MaDocGia = next.MaDocGia,
                    MaSuKien = eventCode,
                    TieuDe = "Sách đặt trước đã sẵn sàng",
                    NoiDung = $"Sách bạn đặt trước đã có. Thư viện giữ bản {banSao.MaVach} đến {next.HanNhanSach:HH:mm dd/MM/yyyy}. Hãy đến quầy và xuất trình thẻ độc giả để nhận sách.",
                    LienKet = $"/DatTruocs/Details/{next.MaDatTruoc}",
                    Loai = "success",
                    NgayTao = DateTime.Now
                });
            }

            return true;
        }
    }
}
