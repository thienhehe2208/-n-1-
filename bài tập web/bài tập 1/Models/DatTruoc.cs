using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace bài_tập_1.Models
{
    public class DatTruoc
    {
        [Key]
        public int MaDatTruoc { get; set; }

        public int MaDocGia { get; set; }
        [ForeignKey(nameof(MaDocGia))]
        public DocGia DocGia { get; set; }

        public int MaSach { get; set; }
        [ForeignKey(nameof(MaSach))]
        public Sach Sach { get; set; }

        public DateTime NgayDat { get; set; } = DateTime.Now;

        public DateTime? NgayHetHanDat { get; set; }

        public int? MaBanSaoDuocGiu { get; set; }
        [ForeignKey(nameof(MaBanSaoDuocGiu))]
        public BanSao? BanSaoDuocGiu { get; set; }

        public DateTime? NgaySanSang { get; set; }

        public DateTime? HanNhanSach { get; set; }

        public TrangThaiDatTruoc TrangThai { get; set; } = TrangThaiDatTruoc.DangCho;
    }
}
