using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace bài_tập_1.Models
{
    [Index(nameof(MaVach), IsUnique = true)]
    public class BanSao
    {
        [Key]
        public int MaBanSao { get; set; }

        public int MaSach { get; set; }
        [ForeignKey(nameof(MaSach))]
        [ValidateNever]
        public Sach Sach { get; set; } = null!;

        [Required, MaxLength(50)]
        public string MaVach { get; set; } // Mã vạch/QR, cần cấu hình unique index trong DbContext

        public TinhTrangBanSao TinhTrang { get; set; } = TinhTrangBanSao.SanCo;

        [MaxLength(50)]
        public string ViTriKe { get; set; }

        // Navigation
        [ValidateNever]
        public ICollection<ChiTietPhieuMuon> ChiTietPhieuMuons { get; set; } = new List<ChiTietPhieuMuon>();
        [ValidateNever]
        public ICollection<DatTruoc> DatTruocsDuocGiu { get; set; } = new List<DatTruoc>();
    }
}
