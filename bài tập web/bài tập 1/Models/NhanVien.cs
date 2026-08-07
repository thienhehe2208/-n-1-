
using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace bài_tập_1.Models
{
    [Index(nameof(UserId), IsUnique = true)]
    public class NhanVien
    {
        [Key]
        public int MaNhanVien { get; set; }

        // Liên kết 1-1 với tài khoản đăng nhập (AspNetUsers)
        // Áp dụng cho cả Nhân viên (thủ thư) lẫn Admin, phân biệt bằng Role
        [Required]
        public string UserId { get; set; }
        [ForeignKey(nameof(UserId))]
        public IdentityUser User { get; set; }

        [Required, MaxLength(150)]
        public string HoTen { get; set; }

        public DateTime? NgaySinh { get; set; }

        [MaxLength(10)]
        public string GioiTinh { get; set; }

        [MaxLength(250)]
        public string DiaChi { get; set; }

        [MaxLength(20)]
        public string SoDienThoai { get; set; }

        [MaxLength(100)]
        public string Email { get; set; }

        [MaxLength(100)]
        public string ChucVu { get; set; }

        public DateTime NgayVaoLam { get; set; } = DateTime.Now;

        // Navigation
        public ICollection<PhieuMuon> PhieuMuons { get; set; } = new List<PhieuMuon>();

        public ICollection<GiaoDichThanhToan> GiaoDichThanhToans { get; set; } =
            new List<GiaoDichThanhToan>();
    }
}
