using System.ComponentModel.DataAnnotations;

namespace bài_tập_1.Models
{
    public class PhanHoi
    {
        [Key]
        public int MaPhanHoi { get; set; }

        [MaxLength(450)]
        public string? UserId { get; set; }

        [Required, MaxLength(100)]
        public string HoTen { get; set; } = string.Empty;

        [Required, EmailAddress, MaxLength(150)]
        public string Email { get; set; } = string.Empty;

        [Required, MaxLength(2000)]
        public string NoiDung { get; set; } = string.Empty;

        public DateTime NgayGui { get; set; } = DateTime.Now;

        [Required, MaxLength(30)]
        public string TrangThai { get; set; } = "Mới";
    }
}
