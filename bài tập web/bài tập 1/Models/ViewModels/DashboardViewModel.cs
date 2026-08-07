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

        public decimal DoanhThuHomNay { get; set; }

        public decimal DoanhThuThangNay { get; set; }

        public decimal DoanhThuNamNay { get; set; }

        public bool HienBaoCaoDoanhThu { get; set; }

        public List<PhieuMuon> PhieuQuaHanGanNhat { get; set; } = new();

        public List<SachMuonNhieuViewModel> SachMuonNhieuNhat { get; set; } = new();
    }

    public class SachMuonNhieuViewModel
    {
        public int MaSach { get; set; }

        public string TenSach { get; set; } = string.Empty;

        public int SoLuotMuon { get; set; }
    }
}
