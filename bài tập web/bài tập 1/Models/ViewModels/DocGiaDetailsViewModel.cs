namespace bài_tập_1.Models.ViewModels
{
    public class DocGiaDetailsViewModel
    {
        public DocGia DocGia { get; set; } = null!;
        public int TongLuotMuon { get; set; }
        public int SachDangMuon { get; set; }
        public int SachQuaHan { get; set; }
        public int DatTruocDangCho { get; set; }
        public decimal TienPhatChuaDong { get; set; }
        public List<DocGiaLoanItemViewModel> MuonGanDay { get; set; } = new();
        public List<DocGiaReservationItemViewModel> DatTruocGanDay { get; set; } = new();
    }

    public class DocGiaLoanItemViewModel
    {
        public int MaPhieuMuon { get; set; }
        public string TenSach { get; set; } = string.Empty;
        public string MaVach { get; set; } = string.Empty;
        public DateTime NgayMuon { get; set; }
        public DateTime NgayHenTra { get; set; }
        public DateTime? NgayTra { get; set; }
        public bool QuaHan { get; set; }
    }

    public class DocGiaReservationItemViewModel
    {
        public int MaDatTruoc { get; set; }
        public string TenSach { get; set; } = string.Empty;
        public DateTime NgayDat { get; set; }
        public TrangThaiDatTruoc TrangThai { get; set; }
    }
}
