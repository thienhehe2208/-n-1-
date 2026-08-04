using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace bài_tập_1.Models
{
    public class YeuCauMuonOnline
    {
        [Key]
        public int MaYeuCau { get; set; }

        [Required, MaxLength(32)]
        public string MaXacNhan { get; set; } = string.Empty;

        public int MaDocGia { get; set; }
        [ForeignKey(nameof(MaDocGia))]
        public DocGia DocGia { get; set; } = null!;

        public int MaSach { get; set; }
        [ForeignKey(nameof(MaSach))]
        public Sach Sach { get; set; } = null!;

        public int MaBanSao { get; set; }
        [ForeignKey(nameof(MaBanSao))]
        public BanSao BanSao { get; set; } = null!;

        public DateTime NgayTao { get; set; } = DateTime.Now;
        public DateTime NgayHenNhan { get; set; }
        public DateTime NgayHenTra { get; set; }
        public DateTime HanNhanSach { get; set; }

        [MaxLength(250)]
        public string GhiChu { get; set; } = string.Empty;

        [MaxLength(500)]
        public string LyDoTuChoi { get; set; } = string.Empty;

        public TrangThaiYeuCauMuonOnline TrangThai { get; set; } =
            TrangThaiYeuCauMuonOnline.ChoNhan;

        public int? MaPhieuMuon { get; set; }
        [ForeignKey(nameof(MaPhieuMuon))]
        public PhieuMuon? PhieuMuon { get; set; }
    }
}
