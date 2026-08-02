using System.ComponentModel.DataAnnotations;

namespace bài_tập_1.Models.ViewModels
{
    public class TaoYeuCauMuonOnlineViewModel
    {
        public int MaSach { get; set; }
        public string TenSach { get; set; } = string.Empty;
        public string AnhBia { get; set; } = string.Empty;
        public int SoBanSanCo { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn ngày đến nhận sách.")]
        [DataType(DataType.Date)]
        [Display(Name = "Ngày đến nhận sách")]
        public DateTime NgayHenNhan { get; set; } = DateTime.Today.AddDays(1);

        [Required(ErrorMessage = "Vui lòng chọn ngày hẹn trả.")]
        [DataType(DataType.Date)]
        [Display(Name = "Ngày hẹn trả")]
        public DateTime NgayHenTra { get; set; } = DateTime.Today.AddDays(15);

        [StringLength(250, ErrorMessage = "Ghi chú không được vượt quá 250 ký tự.")]
        [Display(Name = "Ghi chú cho nhân viên")]
        public string GhiChu { get; set; } = string.Empty;
    }
}
