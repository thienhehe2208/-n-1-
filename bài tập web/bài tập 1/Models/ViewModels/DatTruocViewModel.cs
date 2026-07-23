using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using bài_tập_1.Models;

namespace bài_tập_1.Models.ViewModels
{
    public class DatTruocViewModel
    {
        [Required]
        public int MaSach { get; set; }

        public int? MaDocGia { get; set; }

        [ValidateNever]
        public Sach Sach { get; set; } = default!;

        [ValidateNever]
        public DocGia? DocGia { get; set; }

        public bool IsStaff { get; set; }

        public int TongBanSao { get; set; }

        public int SoBanSanCo { get; set; }

        public int SoNguoiDangCho { get; set; }

        public DateTime NgayHetHanDuKien { get; set; }

        [Range(typeof(bool), "true", "true",
            ErrorMessage = "Bạn cần xác nhận đã đọc quy định đặt trước.")]
        public bool DongYQuyDinh { get; set; }
    }
}
