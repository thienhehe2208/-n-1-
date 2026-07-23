using bài_tập_1.Models;

namespace bài_tập_1.Models.ViewModels
{
    public class TrangChuViewModel
    {
        public List<Sach> SachMoi { get; set; } = new();

        public List<SachDangMuonItem> SachDangMuon { get; set; } = new();

        public List<TheLoaiItem> TheLoaiPhoBien { get; set; } = new();

        public List<ThongBaoItem> ThongBao { get; set; } = new();
    }

    public class SachDangMuonItem
    {
        public int MaSach { get; set; }

        public string TenSach { get; set; } = string.Empty;

        public string? AnhBia { get; set; }

        public DateTime NgayTra { get; set; }

        public int SoNgayConLai { get; set; }
    }

    public class TheLoaiItem
    {
        public int MaTheLoai { get; set; }

        public string TenTheLoai { get; set; } = string.Empty;

        public int SoLuongSach { get; set; }

        public string Icon { get; set; } = "bi-book";

        public string LopMau { get; set; } = "category-green";
    }

    public class ThongBaoItem
    {
        public string NoiDung { get; set; } = string.Empty;

        public DateTime NgayThongBao { get; set; }

        public string Loai { get; set; } = "info";
    }
}