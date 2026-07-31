using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace bài_tập_1.Migrations
{
    public partial class CompleteSubmissionFeatures : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "NgayGiaHanGanNhat",
                table: "PhieuMuon",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SoLanGiaHan",
                table: "PhieuMuon",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "ThongBao",
                columns: table => new
                {
                    MaThongBao = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MaDocGia = table.Column<int>(type: "int", nullable: false),
                    MaSuKien = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    TieuDe = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    NoiDung = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    LienKet = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    Loai = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    NgayTao = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DaDoc = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ThongBao", x => x.MaThongBao);
                    table.ForeignKey(
                        name: "FK_ThongBao_DocGia_MaDocGia",
                        column: x => x.MaDocGia,
                        principalTable: "DocGia",
                        principalColumn: "MaDocGia",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ThongBao_MaDocGia_MaSuKien",
                table: "ThongBao",
                columns: new[] { "MaDocGia", "MaSuKien" },
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ThongBao");

            migrationBuilder.DropColumn(
                name: "NgayGiaHanGanNhat",
                table: "PhieuMuon");

            migrationBuilder.DropColumn(
                name: "SoLanGiaHan",
                table: "PhieuMuon");
        }
    }
}
