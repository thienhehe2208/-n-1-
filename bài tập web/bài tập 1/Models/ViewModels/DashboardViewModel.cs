namespace bài_tập_1.Models.ViewModels
{
    public class DashboardViewModel
    {
        public int TongSoSach { get; set; }
        public int TongSoBanSao { get; set; }
        public int TongSoDocGiaHoatDong { get; set; }
        public int SoPhieuDangMuon { get; set; }
        public int SoPhieuQuaHan { get; set; }
        public decimal TongTienPhatChuaThu { get; set; }

        public List<PhieuMuon> PhieuQuaHanGanNhat { get; set; } = new();
    }
}
