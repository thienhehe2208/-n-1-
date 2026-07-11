using System.ComponentModel.DataAnnotations;

namespace bài_tập_1.Models
{
    public class TheLoai
    {
        [Key]
        public int MaTheLoai { get; set; }

        [Required, MaxLength(100)]
        public string TenTheLoai { get; set; }

        public string MoTa { get; set; }

        // Navigation
        public ICollection<Sach> DanhSachSach { get; set; } = new List<Sach>();
    }
}
