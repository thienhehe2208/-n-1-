using System.ComponentModel.DataAnnotations;

namespace bài_tập_1.Models.ViewModels
{
    public class SachFormViewModel
    {
        public int MaSach { get; set; }

        [Required, StringLength(250)]
        [Display(Name = "Tên sách")]
        public string TenSach { get; set; } = string.Empty;

        [StringLength(20)]
        public string ISBN { get; set; } = string.Empty;

        [Range(0, double.MaxValue)]
        [Display(Name = "Giá sách")]
        public decimal GiaSach { get; set; }

        [Required]
        [Display(Name = "Thể loại")]
        public int MaTheLoai { get; set; }

        [Required]
        [Display(Name = "Nhà xuất bản")]
        public int MaNXB { get; set; }

        [Range(0, 9999)]
        [Display(Name = "Năm xuất bản")]
        public int? NamXuatBan { get; set; }

        [Range(1, int.MaxValue)]
        [Display(Name = "Số trang")]
        public int? SoTrang { get; set; }

        [StringLength(50)]
        [Display(Name = "Ngôn ngữ")]
        public string NgonNgu { get; set; } = string.Empty;

        [Display(Name = "Mô tả")]
        public string MoTa { get; set; } = string.Empty;

        [StringLength(300)]
        [Display(Name = "Ảnh bìa")]
        public string AnhBia { get; set; } = string.Empty;

        [MinLength(1, ErrorMessage = "Vui lòng chọn ít nhất một tác giả.")]
        [Display(Name = "Tác giả")]
        public List<int> TacGiaIds { get; set; } = new();
    }
}
