using System.ComponentModel.DataAnnotations;

namespace bài_tập_1.Models.ViewModels
{
    public class ChonSachMuonOnlineItem
    {
        public int MaSach { get; set; }
        public string TenSach { get; set; } = string.Empty;
        public string TacGia { get; set; } = string.Empty;
        public string AnhBia { get; set; } = string.Empty;
        public int SoBanSanCo { get; set; }
    }

    public class TaoYeuCauMuonOnlineViewModel
    {
        [MinLength(1, ErrorMessage = "Vui lòng chọn ít nhất một cuốn sách.")]
        [MaxLength(5, ErrorMessage = "Mỗi phiếu được chọn tối đa 5 cuốn sách.")]
        public List<int> MaSachIds { get; set; } = new();

        public List<ChonSachMuonOnlineItem> DanhSachSach { get; set; } = new();

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

    public class PhieuMuonOnlineViewModel
    {
        public string MaXacNhan { get; set; } = string.Empty;
        public int MaDocGia { get; set; }
        public DocGia DocGia { get; set; } = null!;
        public DateTime NgayTao { get; set; }
        public DateTime NgayHenNhan { get; set; }
        public DateTime NgayHenTra { get; set; }
        public DateTime HanNhanSach { get; set; }
        public string GhiChu { get; set; } = string.Empty;
        public string LyDoTuChoi { get; set; } = string.Empty;
        public TrangThaiYeuCauMuonOnline TrangThai { get; set; }
        public int? MaPhieuMuon { get; set; }
        public List<YeuCauMuonOnline> ChiTiet { get; set; } = new();
    }
}
