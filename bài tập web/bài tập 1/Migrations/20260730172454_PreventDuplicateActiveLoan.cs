using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace bài_tập_1.Migrations
{
    public partial class PreventDuplicateActiveLoan : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ChiTietPhieuMuon_MaBanSao",
                table: "ChiTietPhieuMuon");

            migrationBuilder.CreateIndex(
                name: "UX_ChiTietPhieuMuon_MaBanSao_DangMuon",
                table: "ChiTietPhieuMuon",
                column: "MaBanSao",
                unique: true,
                filter: "[NgayTra] IS NULL");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "UX_ChiTietPhieuMuon_MaBanSao_DangMuon",
                table: "ChiTietPhieuMuon");

            migrationBuilder.CreateIndex(
                name: "IX_ChiTietPhieuMuon_MaBanSao",
                table: "ChiTietPhieuMuon",
                column: "MaBanSao");
        }
    }
}
