using System.ComponentModel.DataAnnotations;

namespace bài_tập_1.Models
{
    public class NhaXuatBan
    {
        [Key]
        public int MaNXB { get; set; }

        [Required, MaxLength(150)]
        public string TenNXB { get; set; }

        [MaxLength(250)]
        public string DiaChi { get; set; }

        [MaxLength(20)]
        public string SoDienThoai { get; set; }

        [MaxLength(100)]
        public string Email { get; set; }

        // Navigation
        public ICollection<Sach> DanhSachSach { get; set; } = new List<Sach>();
    }
}
