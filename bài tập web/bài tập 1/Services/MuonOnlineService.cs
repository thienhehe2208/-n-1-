using bài_tập_1.Data;
using bài_tập_1.Models;
using Microsoft.EntityFrameworkCore;

namespace bài_tập_1.Services
{
    public class MuonOnlineService
    {
        private readonly bài_tập_1Context _context;
        private readonly DatTruocService _datTruocService;

        public MuonOnlineService(
            bài_tập_1Context context,
            DatTruocService datTruocService)
        {
            _context = context;
            _datTruocService = datTruocService;
        }

        public async Task<int> XuLyHetHanAsync()
        {
            var expired = await _context.YeuCauMuonOnline
                .Include(y => y.BanSao)
                .Where(y =>
                    y.TrangThai == TrangThaiYeuCauMuonOnline.ChoNhan &&
                    y.HanNhanSach < DateTime.Now)
                .ToListAsync();

            if (expired.Count == 0)
                return 0;

            foreach (var request in expired)
                request.TrangThai = TrangThaiYeuCauMuonOnline.HetHan;

            await _context.SaveChangesAsync();

            foreach (var banSao in expired
                         .Select(y => y.BanSao)
                         .GroupBy(b => b.MaBanSao)
                         .Select(group => group.First()))
            {
                var dangDuocDatTruocGiu = await _context.DatTruoc.AnyAsync(d =>
                    d.MaBanSaoDuocGiu == banSao.MaBanSao &&
                    d.TrangThai == TrangThaiDatTruoc.DaCoSach);
                var conYeuCauOnlineKhac = await _context.YeuCauMuonOnline
                    .AnyAsync(y =>
                        y.MaBanSao == banSao.MaBanSao &&
                        y.TrangThai == TrangThaiYeuCauMuonOnline.ChoNhan);

                if (banSao.TinhTrang != TinhTrangBanSao.DaGiu ||
                    dangDuocDatTruocGiu || conYeuCauOnlineKhac)
                {
                    continue;
                }

                banSao.TinhTrang = TinhTrangBanSao.SanCo;
                await _datTruocService.PhanBoBanSaoAsync(banSao);
            }

            await _context.SaveChangesAsync();
            return expired.Count;
        }
    }
}
