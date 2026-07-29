using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace bài_tập_1.Models
{
    [Index(nameof(MaDocGia), nameof(MaSach), IsUnique = true)]
    public class YeuThich
    {
        [Key]
        public int MaYeuThich { get; set; }

        public int MaDocGia { get; set; }
        [ForeignKey(nameof(MaDocGia))]
        public DocGia DocGia { get; set; } = null!;

        public int MaSach { get; set; }
        [ForeignKey(nameof(MaSach))]
        public Sach Sach { get; set; } = null!;

        public DateTime NgayThem { get; set; } = DateTime.Now;
    }
}
