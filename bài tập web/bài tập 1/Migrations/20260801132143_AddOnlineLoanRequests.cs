using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace bài_tập_1.Migrations
{
    public partial class AddOnlineLoanRequests : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "YeuCauMuonOnline",
                columns: table => new
                {
                    MaYeuCau = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MaXacNhan = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    MaDocGia = table.Column<int>(type: "int", nullable: false),
                    MaSach = table.Column<int>(type: "int", nullable: false),
                    MaBanSao = table.Column<int>(type: "int", nullable: false),
                    NgayTao = table.Column<DateTime>(type: "datetime2", nullable: false),
                    NgayHenNhan = table.Column<DateTime>(type: "datetime2", nullable: false),
                    NgayHenTra = table.Column<DateTime>(type: "datetime2", nullable: false),
                    HanNhanSach = table.Column<DateTime>(type: "datetime2", nullable: false),
                    GhiChu = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    TrangThai = table.Column<int>(type: "int", nullable: false),
                    MaPhieuMuon = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_YeuCauMuonOnline", x => x.MaYeuCau);
                    table.ForeignKey(
                        name: "FK_YeuCauMuonOnline_BanSao_MaBanSao",
                        column: x => x.MaBanSao,
                        principalTable: "BanSao",
                        principalColumn: "MaBanSao",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_YeuCauMuonOnline_DocGia_MaDocGia",
                        column: x => x.MaDocGia,
                        principalTable: "DocGia",
                        principalColumn: "MaDocGia",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_YeuCauMuonOnline_PhieuMuon_MaPhieuMuon",
                        column: x => x.MaPhieuMuon,
                        principalTable: "PhieuMuon",
                        principalColumn: "MaPhieuMuon",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_YeuCauMuonOnline_Sach_MaSach",
                        column: x => x.MaSach,
                        principalTable: "Sach",
                        principalColumn: "MaSach",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_YeuCauMuonOnline_MaBanSao",
                table: "YeuCauMuonOnline",
                column: "MaBanSao");

            migrationBuilder.CreateIndex(
                name: "IX_YeuCauMuonOnline_MaDocGia",
                table: "YeuCauMuonOnline",
                column: "MaDocGia");

            migrationBuilder.CreateIndex(
                name: "IX_YeuCauMuonOnline_MaPhieuMuon",
                table: "YeuCauMuonOnline",
                column: "MaPhieuMuon");

            migrationBuilder.CreateIndex(
                name: "IX_YeuCauMuonOnline_MaSach",
                table: "YeuCauMuonOnline",
                column: "MaSach");

            migrationBuilder.CreateIndex(
                name: "IX_YeuCauMuonOnline_MaXacNhan",
                table: "YeuCauMuonOnline",
                column: "MaXacNhan",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "YeuCauMuonOnline");
        }
    }
}
