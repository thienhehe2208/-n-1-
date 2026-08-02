using bài_tập_1.Data;
using bài_tập_1.Models;
using Microsoft.EntityFrameworkCore;

namespace bài_tập_1.Services
{
    public class PhieuMuonService
    {
        private readonly bài_tập_1Context _context;

        public PhieuMuonService(bài_tập_1Context context)
        {
            _context = context;
        }

        public async Task CapNhatTrangThaiAsync(int? maPhieuMuon = null)
        {
            var query = _context.PhieuMuon
                .Include(p => p.ChiTietPhieuMuons)
                .AsQueryable();

            if (maPhieuMuon.HasValue)
                query = query.Where(p => p.MaPhieuMuon == maPhieuMuon.Value);

            var phieuMuons = await query.ToListAsync();
            var homNay = DateTime.Today;
            var changed = false;

            foreach (var phieu in phieuMuons)
            {
                var trangThaiMoi = phieu.ChiTietPhieuMuons.Count == 0
                    ? TrangThaiPhieuMuon.Nhap
                    : phieu.ChiTietPhieuMuons.All(c => c.NgayTra.HasValue)
                        ? TrangThaiPhieuMuon.DaTra
                        : phieu.NgayHenTra.Date < homNay
                            ? TrangThaiPhieuMuon.QuaHan
                            : TrangThaiPhieuMuon.DangMuon;

                if (phieu.TrangThai == trangThaiMoi)
                    continue;

                phieu.TrangThai = trangThaiMoi;
                changed = true;
            }

            if (changed)
                await _context.SaveChangesAsync();
        }

        public Task<int> DemSoSachDangMuonAsync(int maDocGia)
        {
            return _context.ChiTietPhieuMuon.CountAsync(c =>
                c.PhieuMuon.MaDocGia == maDocGia &&
                c.NgayTra == null);
        }
    }
}
