using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace bài_tập_1.Models
{
    public class GiaoDichThanhToan
    {
        [Key]
        public int MaGiaoDich { get; set; }

        public int MaPhieuMuon { get; set; }

        [ForeignKey(nameof(MaPhieuMuon))]
        public PhieuMuon PhieuMuon { get; set; } = null!;

        public int MaNhanVienXacNhan { get; set; }

        [ForeignKey(nameof(MaNhanVienXacNhan))]
        public NhanVien NhanVienXacNhan { get; set; } = null!;

        [Column(TypeName = "decimal(18,2)")]
        public decimal PhiThue { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal TienPhat { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal TongTien { get; set; }

        public PhuongThucThanhToan PhuongThuc { get; set; }

        [MaxLength(100)]
        public string MaThamChieu { get; set; } = string.Empty;

        [MaxLength(250)]
        public string GhiChu { get; set; } = string.Empty;

        public DateTime NgayThanhToan { get; set; } = DateTime.Now;

        public bool DuLieuKhoiTao { get; set; }
    }
}
