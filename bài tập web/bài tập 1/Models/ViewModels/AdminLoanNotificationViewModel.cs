namespace bài_tập_1.Models.ViewModels
{
    public class AdminLoanNotificationItemViewModel
    {
        public string MaXacNhan { get; set; } = string.Empty;
        public string TenDocGia { get; set; } = string.Empty;
        public DateTime NgayTao { get; set; }
        public DateTime HanNhanSach { get; set; }
        public List<string> TenSaches { get; set; } = new();
    }

    public class AdminLoanNotificationViewModel
    {
        public int TongPhieuChoNhan { get; set; }
        public IReadOnlyList<AdminLoanNotificationItemViewModel> PhieuMoi { get; set; } =
            Array.Empty<AdminLoanNotificationItemViewModel>();
    }
}
