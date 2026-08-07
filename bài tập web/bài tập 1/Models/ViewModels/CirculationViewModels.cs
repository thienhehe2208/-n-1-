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

        [MinLength(1, ErrorMessage = "Vui lòng chọn ít nhất một cuốn sách.")]
        [MaxLength(5, ErrorMessage = "Mỗi độc giả chỉ được giữ tối đa 5 cuốn sách.")]
        [Display(Name = "Sách mượn")]
        public List<int> MaBanSaos { get; set; } = new();
    }

    public class BanSaoMuonOptionViewModel
    {
        public int MaBanSao { get; set; }

        public int MaSach { get; set; }

        public string TenSach { get; set; } = string.Empty;

        public string MaVach { get; set; } = string.Empty;

        public string TheLoai { get; set; } = string.Empty;

        public string ViTriKe { get; set; } = string.Empty;

        public string AnhBia { get; set; } = string.Empty;
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

    public class XacNhanThanhToanViewModel
    {
        public int MaPhieuMuon { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn phương thức thanh toán.")]
        [Display(Name = "Phương thức thanh toán")]
        public PhuongThucThanhToan? PhuongThuc { get; set; }

        [StringLength(100, ErrorMessage = "Mã giao dịch không được vượt quá 100 ký tự.")]
        [Display(Name = "Mã giao dịch ngân hàng")]
        public string MaThamChieu { get; set; } = string.Empty;

        [StringLength(250, ErrorMessage = "Ghi chú không được vượt quá 250 ký tự.")]
        [Display(Name = "Ghi chú")]
        public string GhiChu { get; set; } = string.Empty;
    }

    public class TraPhieuMuonViewModel
    {
        public int MaPhieuMuon { get; set; }

        public string HoTenDocGia { get; set; } = string.Empty;

        public DateTime NgayMuon { get; set; }

        public DateTime NgayHenTra { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn ngày trả.")]
        [DataType(DataType.Date)]
        [Display(Name = "Ngày trả")]
        public DateTime NgayTra { get; set; } = DateTime.Today;

        [MinLength(1, ErrorMessage = "Phiếu mượn không có sách cần trả.")]
        public List<TraSachTrongPhieuViewModel> Sach { get; set; } = new();
    }

    public class TraSachTrongPhieuViewModel
    {
        public int MaChiTiet { get; set; }

        public string TenSach { get; set; } = string.Empty;

        public string MaVach { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng chọn tình trạng của sách.")]
        [Display(Name = "Tình trạng khi trả")]
        public TinhTrangKhiTra? TinhTrangKhiTra { get; set; }

        [StringLength(250, ErrorMessage = "Ghi chú không được vượt quá 250 ký tự.")]
        [Display(Name = "Ghi chú")]
        public string GhiChu { get; set; } = string.Empty;
    }
}
