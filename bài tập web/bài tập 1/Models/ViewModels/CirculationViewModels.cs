using System.ComponentModel.DataAnnotations;

namespace bài_tập_1.Models.ViewModels
{
    public class LapPhieuMuonViewModel
    {
        [Range(1, int.MaxValue, ErrorMessage = "Vui lòng chọn độc giả.")]
        [Display(Name = "Độc giả")]
        public int MaDocGia { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn hạn trả.")]
        [DataType(DataType.Date)]
        [Display(Name = "Ngày hẹn trả")]
        public DateTime NgayHenTra { get; set; } = DateTime.Today.AddDays(14);
    }

    public class CapNhatHanTraViewModel
    {
        public int MaPhieuMuon { get; set; }

        public string HoTenDocGia { get; set; } = string.Empty;

        public DateTime NgayMuon { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn hạn trả.")]
        [DataType(DataType.Date)]
        [Display(Name = "Ngày hẹn trả")]
        public DateTime NgayHenTra { get; set; }
    }

    public class ThemSachVaoPhieuViewModel
    {
        [Range(1, int.MaxValue, ErrorMessage = "Vui lòng chọn phiếu mượn.")]
        [Display(Name = "Phiếu mượn")]
        public int MaPhieuMuon { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Vui lòng chọn bản sao.")]
        [Display(Name = "Bản sao")]
        public int MaBanSao { get; set; }

        [StringLength(250, ErrorMessage = "Ghi chú không được vượt quá 250 ký tự.")]
        [Display(Name = "Ghi chú")]
        public string GhiChu { get; set; } = string.Empty;
    }

    public class TraSachViewModel
    {
        public int MaChiTiet { get; set; }

        public int MaPhieuMuon { get; set; }

        public string TenSach { get; set; } = string.Empty;

        public string MaVach { get; set; } = string.Empty;

        public string HoTenDocGia { get; set; } = string.Empty;

        public DateTime NgayMuon { get; set; }

        public DateTime NgayHenTra { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn ngày trả.")]
        [DataType(DataType.Date)]
        [Display(Name = "Ngày trả")]
        public DateTime NgayTra { get; set; } = DateTime.Today;

        [Required(ErrorMessage = "Vui lòng chọn tình trạng khi trả.")]
        [Display(Name = "Tình trạng khi trả")]
        public TinhTrangKhiTra? TinhTrangKhiTra { get; set; }

        [StringLength(250, ErrorMessage = "Ghi chú không được vượt quá 250 ký tự.")]
        [Display(Name = "Ghi chú")]
        public string GhiChu { get; set; } = string.Empty;
    }
}
