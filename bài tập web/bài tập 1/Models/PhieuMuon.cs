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

        // Navigation
        public ICollection<ChiTietPhieuMuon> ChiTietPhieuMuons { get; set; } = new List<ChiTietPhieuMuon>();

        // Thuộc tính tính toán (không lưu DB) - tổng số sách trong phiếu
        [NotMapped]
        public int TongSoSach => ChiTietPhieuMuons?.Count ?? 0;
    }
}
