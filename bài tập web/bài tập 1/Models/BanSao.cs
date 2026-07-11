using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace bài_tập_1.Models
{
    public class BanSao
    {
        [Key]
        public int MaBanSao { get; set; }

        public int MaSach { get; set; }
        [ForeignKey(nameof(MaSach))]
        public Sach Sach { get; set; }

        [Required, MaxLength(50)]
        public string MaVach { get; set; } // Mã vạch/QR, cần cấu hình unique index trong DbContext

        public TinhTrangBanSao TinhTrang { get; set; } = TinhTrangBanSao.SanCo;

        [MaxLength(50)]
        public string ViTriKe { get; set; }

        // Navigation
        public ICollection<ChiTietPhieuMuon> ChiTietPhieuMuons { get; set; } = new List<ChiTietPhieuMuon>();
    }
}
