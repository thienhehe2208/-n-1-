using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace bài_tập_1.Models
{
    public class ChiTietPhieuMuon
    {
        [Key]
        public int MaChiTiet { get; set; }

        public int MaPhieuMuon { get; set; }
        [ForeignKey(nameof(MaPhieuMuon))]
        public PhieuMuon PhieuMuon { get; set; }

        public int MaBanSao { get; set; }
        [ForeignKey(nameof(MaBanSao))]
        public BanSao BanSao { get; set; }

        public DateTime? NgayTra { get; set; }

        public TinhTrangKhiTra? TinhTrangKhiTra { get; set; }

        [MaxLength(250)]
        public string GhiChu { get; set; }

        // Navigation - 1 chi tiết phiếu mượn có thể phát sinh 1 phiếu phạt
        public PhieuPhat PhieuPhat { get; set; }
    }
}
