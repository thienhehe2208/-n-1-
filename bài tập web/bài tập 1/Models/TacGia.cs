using System.ComponentModel.DataAnnotations;

namespace bài_tập_1.Models
{
    public class TacGia
    {
        [Key]
        public int MaTacGia { get; set; }

        [Required, MaxLength(150)]
        public string HoTen { get; set; }

        public DateTime? NgaySinh { get; set; }

        [MaxLength(100)]
        public string QuocTich { get; set; }

        public string TieuSu { get; set; }

        // Navigation
        public ICollection<SachTacGia> SachTacGias { get; set; } = new List<SachTacGia>();
    }
}
