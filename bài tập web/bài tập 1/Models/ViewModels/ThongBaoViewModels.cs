using System.ComponentModel.DataAnnotations;

namespace bài_tập_1.Models.ViewModels
{
    public class TaoThongBaoViewModel
    {
        [Required(ErrorMessage = "Vui lòng nhập tiêu đề."), MaxLength(150)]
        [Display(Name = "Tiêu đề")]
        public string TieuDe { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập nội dung."), MaxLength(500)]
        [Display(Name = "Nội dung")]
        public string NoiDung { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng chọn đối tượng nhận.")]
        [Display(Name = "Gửi đến")]
        public string DoiTuong { get; set; } = "TatCa";

        [Required, MaxLength(30)]
        [Display(Name = "Mức độ")]
        public string Loai { get; set; } = "info";

        [MaxLength(250)]
        [Display(Name = "Liên kết đính kèm")]
        public string? LienKet { get; set; }
    }
}
