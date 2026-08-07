using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace bài_tập_1.Models
{
    public class PhieuMuon
    {
        [Key]
        public int MaPhieuMuon { get; set; }

        public int MaDocGia { get; set; }
        [ForeignKey(nameof(MaDocGia))]
        public DocGia DocGia { get; set; }

        public int MaNhanVien { get; set; }
        [ForeignKey(nameof(MaNhanVien))]
        public NhanVien NhanVien { get; set; }

        public DateTime NgayMuon { get; set; } = DateTime.Now;

        public DateTime NgayHenTra { get; set; }

        public TrangThaiPhieuMuon TrangThai { get; set; } = TrangThaiPhieuMuon.Nhap;

        public int SoLanGiaHan { get; set; }

        public DateTime? NgayGiaHanGanNhat { get; set; }

        public TrangThaiThanhToan TrangThaiThanhToan { get; set; } =
            global::bài_tập_1.Models.TrangThaiThanhToan.ChuaThanhToan;

        public DateTime? NgayThanhToan { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal SoTienDaThanhToan { get; set; }

        public GiaoDichThanhToan? GiaoDichThanhToan { get; set; }

        // Navigation
        public ICollection<ChiTietPhieuMuon> ChiTietPhieuMuons { get; set; } = new List<ChiTietPhieuMuon>();

        // Thuộc tính tính toán (không lưu DB) - tổng số sách trong phiếu
        [NotMapped]
        public int TongSoSach => ChiTietPhieuMuons?.Count ?? 0;

        [NotMapped]
        public decimal TongPhiThue =>
            ChiTietPhieuMuons?.Sum(c => c.PhiThue) ?? 0;

        [NotMapped]
        public decimal TongTienPhat =>
            ChiTietPhieuMuons?.Sum(c =>
                c.PhieuPhat != null &&
                c.PhieuPhat.TrangThai != TrangThaiPhieuPhat.DaHuy
                    ? c.PhieuPhat.SoTien
                    : 0) ?? 0;

        [NotMapped]
        public decimal TongThanhToan => TongPhiThue + TongTienPhat;

        [NotMapped]
        public decimal TongTienPhatChuaDong =>
            ChiTietPhieuMuons?.Sum(c =>
                c.PhieuPhat?.TrangThai == TrangThaiPhieuPhat.ChuaDong
                    ? c.PhieuPhat.SoTien
                    : 0) ?? 0;

        [NotMapped]
        public decimal TongCanThanhToan =>
            TrangThaiThanhToan ==
                global::bài_tập_1.Models.TrangThaiThanhToan.DaThanhToan
                ? 0
                : TongPhiThue + TongTienPhatChuaDong;
    }
}
