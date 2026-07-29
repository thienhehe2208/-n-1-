using System.ComponentModel.DataAnnotations;

namespace bài_tập_1.Models.ViewModels
{
    public class PhanHoiViewModel
    {
        [Required(ErrorMessage = "Vui lòng nhập họ và tên.")]
        [StringLength(100, ErrorMessage = "Họ tên không được vượt quá 100 ký tự.")]
        public string HoTen { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập email.")]
        [EmailAddress(ErrorMessage = "Địa chỉ email chưa đúng định dạng.")]
        [StringLength(150)]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập nội dung phản hồi.")]
        [StringLength(2000, MinimumLength = 10,
            ErrorMessage = "Nội dung cần từ 10 đến 2000 ký tự.")]
        public string NoiDung { get; set; } = string.Empty;

        public string? ReturnUrl { get; set; }
    }
}
