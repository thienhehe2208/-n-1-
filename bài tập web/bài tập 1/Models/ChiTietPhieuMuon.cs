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

        // Lưu đơn giá tại thời điểm mượn để việc thay đổi giá sau này
        // không làm sai lịch sử thanh toán của các phiếu cũ.
        [Column(TypeName = "decimal(18,2)")]
        public decimal PhiThue { get; set; } = 3000m;

        public DateTime? NgayTra { get; set; }

        public TinhTrangKhiTra? TinhTrangKhiTra { get; set; }

        [MaxLength(250)]
        public string GhiChu { get; set; } = string.Empty;

        // Navigation - 1 chi tiết phiếu mượn có thể phát sinh 1 phiếu phạt
        public PhieuPhat PhieuPhat { get; set; }
    }
}
