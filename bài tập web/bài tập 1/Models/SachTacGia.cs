using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace bài_tập_1.Models
{
    // Bảng trung gian N-N giữa Sach và TacGia.
    // Dùng khóa chính riêng (surrogate key) thay vì khóa ghép,
    // để không cần cấu hình Fluent API trong OnModelCreating.
    public class SachTacGia
    {
        [Key]
        public int MaSachTacGia { get; set; }

        public int MaSach { get; set; }
        [ForeignKey(nameof(MaSach))]
        public Sach Sach { get; set; }

        public int MaTacGia { get; set; }
        [ForeignKey(nameof(MaTacGia))]
        public TacGia TacGia { get; set; }
    }
}
