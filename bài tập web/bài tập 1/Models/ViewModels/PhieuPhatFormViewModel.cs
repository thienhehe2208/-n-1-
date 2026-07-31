using System.ComponentModel.DataAnnotations;

namespace bài_tập_1.Models.ViewModels
{
    public class PhieuPhatFormViewModel
    {
        public int MaPhieuPhat { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Vui lòng chọn lượt mượn cần phạt.")]
        [Display(Name = "Lượt mượn")]
        public int MaChiTiet { get; set; }

        public int MaPhieuMuon { get; set; }

        public string HoTenDocGia { get; set; } = string.Empty;

        public string TenSach { get; set; } = string.Empty;

        public string MaVach { get; set; } = string.Empty;

        public DateTime NgayHenTra { get; set; }

        public DateTime? NgayTra { get; set; }

        public int SoNgayTre { get; set; }

        public TinhTrangKhiTra? TinhTrangKhiTra { get; set; }

        [Range(
            typeof(decimal),
            "0.01",
            "999999999",
            ErrorMessage = "Số tiền phạt phải lớn hơn 0.")]
        [Display(Name = "Số tiền phạt")]
        public decimal SoTien { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn lý do phạt.")]
        [Display(Name = "Lý do phạt")]
        public LyDoPhat? LyDo { get; set; }
    }
}
