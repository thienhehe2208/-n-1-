using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace bài_tập_1.Models
{
    [Index(nameof(MaDocGia), nameof(MaSuKien), IsUnique = true)]
    public class ThongBao
    {
        [Key]
        public int MaThongBao { get; set; }

        public int MaDocGia { get; set; }

        [ForeignKey(nameof(MaDocGia))]
        public DocGia DocGia { get; set; } = null!;

        [Required, MaxLength(100)]
        public string MaSuKien { get; set; } = string.Empty;

        [Required, MaxLength(150)]
        public string TieuDe { get; set; } = string.Empty;

        [Required, MaxLength(500)]
        public string NoiDung { get; set; } = string.Empty;

        [MaxLength(250)]
        public string LienKet { get; set; } = string.Empty;

        [Required, MaxLength(30)]
        public string Loai { get; set; } = "info";

        public DateTime NgayTao { get; set; } = DateTime.Now;

        public bool DaDoc { get; set; }
    }
}
