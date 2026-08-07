using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace bài_tập_1.Migrations
{
    public partial class AddPaymentAuditTrail : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "GiaoDichThanhToan",
                columns: table => new
                {
                    MaGiaoDich = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MaPhieuMuon = table.Column<int>(type: "int", nullable: false),
                    MaNhanVienXacNhan = table.Column<int>(type: "int", nullable: false),
                    PhiThue = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TienPhat = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TongTien = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PhuongThuc = table.Column<int>(type: "int", nullable: false),
                    MaThamChieu = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    GhiChu = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    NgayThanhToan = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DuLieuKhoiTao = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GiaoDichThanhToan", x => x.MaGiaoDich);
                    table.ForeignKey(
                        name: "FK_GiaoDichThanhToan_NhanVien_MaNhanVienXacNhan",
                        column: x => x.MaNhanVienXacNhan,
                        principalTable: "NhanVien",
                        principalColumn: "MaNhanVien",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GiaoDichThanhToan_PhieuMuon_MaPhieuMuon",
                        column: x => x.MaPhieuMuon,
                        principalTable: "PhieuMuon",
                        principalColumn: "MaPhieuMuon",
                        onDelete: ReferentialAction.Restrict);
                });

            // Giữ lại doanh thu của những phiếu đã thanh toán trước khi có
            // bảng nhật ký. Nhân viên trên các dòng này là người xử lý phiếu
            // cũ, nên giao diện sẽ đánh dấu rõ đây là dữ liệu chuyển đổi.
            migrationBuilder.Sql(@"
                INSERT INTO [GiaoDichThanhToan]
                    ([MaPhieuMuon], [MaNhanVienXacNhan], [PhiThue],
                     [TienPhat], [TongTien], [PhuongThuc], [MaThamChieu],
                     [GhiChu], [NgayThanhToan], [DuLieuKhoiTao])
                SELECT
                    p.[MaPhieuMuon],
                    p.[MaNhanVien],
                    ISNULL(SUM(ct.[PhiThue]), 0),
                    CASE
                        WHEN p.[SoTienDaThanhToan] -
                             ISNULL(SUM(ct.[PhiThue]), 0) > 0
                        THEN p.[SoTienDaThanhToan] -
                             ISNULL(SUM(ct.[PhiThue]), 0)
                        ELSE 0
                    END,
                    p.[SoTienDaThanhToan],
                    0,
                    N'',
                    N'Dữ liệu thanh toán được chuyển đổi từ phiếu mượn cũ.',
                    COALESCE(p.[NgayThanhToan], p.[NgayMuon]),
                    1
                FROM [PhieuMuon] p
                LEFT JOIN [ChiTietPhieuMuon] ct
                    ON ct.[MaPhieuMuon] = p.[MaPhieuMuon]
                WHERE p.[TrangThaiThanhToan] = 1
                GROUP BY p.[MaPhieuMuon], p.[MaNhanVien],
                         p.[SoTienDaThanhToan], p.[NgayThanhToan], p.[NgayMuon];");

            migrationBuilder.CreateIndex(
                name: "IX_GiaoDichThanhToan_MaNhanVienXacNhan",
                table: "GiaoDichThanhToan",
                column: "MaNhanVienXacNhan");

            migrationBuilder.CreateIndex(
                name: "IX_GiaoDichThanhToan_MaPhieuMuon",
                table: "GiaoDichThanhToan",
                column: "MaPhieuMuon",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "GiaoDichThanhToan");
        }
    }
}
