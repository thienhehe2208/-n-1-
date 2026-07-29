using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace bài_tập_1.Models
{
    public class Sach
    {
        [Key]
        public int MaSach { get; set; }

        [Required, MaxLength(250)]
        public string TenSach { get; set; }

        [MaxLength(20)]
        public string ISBN { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal GiaSach { get; set; }

        public int MaTheLoai { get; set; }
        [ForeignKey(nameof(MaTheLoai))]
        public TheLoai TheLoai { get; set; }

        public int MaNXB { get; set; }
        [ForeignKey(nameof(MaNXB))]
        public NhaXuatBan NhaXuatBan { get; set; }

        public int? NamXuatBan { get; set; }

        public int? SoTrang { get; set; }

        [MaxLength(50)]
        public string NgonNgu { get; set; }

        public string MoTa { get; set; }

        [MaxLength(300)]
        public string AnhBia { get; set; }

        // Navigation
        public ICollection<SachTacGia> SachTacGias { get; set; } = new List<SachTacGia>();
        public ICollection<BanSao> BanSaos { get; set; } = new List<BanSao>();
        public ICollection<DatTruoc> DatTruocs { get; set; } = new List<DatTruoc>();
        public ICollection<YeuThich> YeuThichs { get; set; } = new List<YeuThich>();
    }
}
