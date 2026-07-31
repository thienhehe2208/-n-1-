using bài_tập_1.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace bài_tập_1.Data
{
    public class bài_tập_1Context : IdentityDbContext<IdentityUser>
    {
        public bài_tập_1Context(DbContextOptions<bài_tập_1Context> options)
            : base(options)
        {
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder); // BẮT BUỘC gọi base để Identity tạo đúng bảng AspNetUsers, AspNetRoles...
                                                // cấu hình Fluent API của bạn ở đây (khóa ghép SachTacGia, unique index MaVach...)
            modelBuilder.Entity<SachTacGia>()
                .HasIndex(st => new { st.MaSach, st.MaTacGia })
                .IsUnique();

            // Một bản sao chỉ được xuất hiện trong tối đa một lượt mượn chưa trả.
            // Filtered unique index cho phép cùng bản sao xuất hiện trong lịch sử,
            // nhưng chặn hai dòng cùng có NgayTra = null.
            modelBuilder.Entity<ChiTietPhieuMuon>()
                .HasIndex(ct => ct.MaBanSao)
                .HasDatabaseName(
                    "UX_ChiTietPhieuMuon_MaBanSao_DangMuon")
                .IsUnique()
                .HasFilter("[NgayTra] IS NULL");

            // Tắt cascade delete cho PhieuMuon - tránh lỗi "multiple cascade paths"
            modelBuilder.Entity<PhieuMuon>()
                .HasOne(p => p.DocGia)
                .WithMany(d => d.PhieuMuons)
                .HasForeignKey(p => p.MaDocGia)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<DatTruoc>()
                .HasOne(d => d.BanSaoDuocGiu)
                .WithMany(b => b.DatTruocsDuocGiu)
                .HasForeignKey(d => d.MaBanSaoDuocGiu)
                .OnDelete(DeleteBehavior.Restrict);

            // Chỉ một yêu cầu DaCoSach được giữ một bản sao tại một thời điểm.
            modelBuilder.Entity<DatTruoc>()
                .HasIndex(d => d.MaBanSaoDuocGiu)
                .HasDatabaseName("UX_DatTruoc_BanSaoDangGiu")
                .IsUnique()
                .HasFilter("[MaBanSaoDuocGiu] IS NOT NULL AND [TrangThai] = 1");

            modelBuilder.Entity<PhieuMuon>()
                .HasOne(p => p.NhanVien)
                .WithMany(n => n.PhieuMuons)
                .HasForeignKey(p => p.MaNhanVien)
                .OnDelete(DeleteBehavior.Restrict);
        }
        public DbSet<bài_tập_1.Models.Sach> Sach { get; set; } = default!;

        public DbSet<bài_tập_1.Models.TacGia> TacGia { get; set; } = default!;

        public DbSet<SachTacGia> SachTacGias { get; set; } = default!;

        public DbSet<bài_tập_1.Models.NhaXuatBan> NhaXuatBan { get; set; } = default!;

        public DbSet<bài_tập_1.Models.TheLoai> TheLoai { get; set; } = default!;

        public DbSet<bài_tập_1.Models.BanSao> BanSao { get; set; } = default!;

        public DbSet<bài_tập_1.Models.DocGia> DocGia { get; set; } = default!;

        public DbSet<bài_tập_1.Models.ChiTietPhieuMuon> ChiTietPhieuMuon { get; set; } = default!;

        public DbSet<bài_tập_1.Models.DatTruoc> DatTruoc { get; set; } = default!;

        public DbSet<bài_tập_1.Models.NhanVien> NhanVien { get; set; } = default!;

        public DbSet<bài_tập_1.Models.PhieuMuon> PhieuMuon { get; set; } = default!;

        public DbSet<bài_tập_1.Models.PhieuPhat> PhieuPhat { get; set; } = default!;

        public DbSet<YeuThich> YeuThich { get; set; } = default!;

        public DbSet<PhanHoi> PhanHoi { get; set; } = default!;

        public DbSet<ThongBao> ThongBao { get; set; } = default!;
    }
}
