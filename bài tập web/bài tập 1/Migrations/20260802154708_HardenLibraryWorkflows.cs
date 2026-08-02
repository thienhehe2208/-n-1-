using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace bài_tập_1.Migrations
{
    public partial class HardenLibraryWorkflows : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_YeuCauMuonOnline_MaBanSao",
                table: "YeuCauMuonOnline");

            migrationBuilder.CreateIndex(
                name: "UX_YeuCauMuonOnline_BanSaoChoNhan",
                table: "YeuCauMuonOnline",
                column: "MaBanSao",
                unique: true,
                filter: "[TrangThai] = 0");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "UX_YeuCauMuonOnline_BanSaoChoNhan",
                table: "YeuCauMuonOnline");

            migrationBuilder.CreateIndex(
                name: "IX_YeuCauMuonOnline_MaBanSao",
                table: "YeuCauMuonOnline",
                column: "MaBanSao");
        }
    }
}
