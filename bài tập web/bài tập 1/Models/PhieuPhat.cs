using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace bài_tập_1.Models
{
    public class PhieuPhat
    {

        [Key]
        public int MaPhieuPhat { get; set; }

        // Mỗi phiếu phạt gắn với đúng 1 cuốn sách cụ thể trong phiếu mượn
        [Required]
        public int MaChiTiet { get; set; }
        [ForeignKey(nameof(MaChiTiet))]
        public ChiTietPhieuMuon ChiTietPhieuMuon { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal SoTien { get; set; }

        public LyDoPhat LyDo { get; set; }

        public DateTime NgayLap { get; set; } = DateTime.Now;

        public TrangThaiPhieuPhat TrangThai { get; set; } = TrangThaiPhieuPhat.ChuaDong;
    }
}
