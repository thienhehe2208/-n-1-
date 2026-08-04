using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace bài_tập_1.Migrations
{
    public partial class AddApprovedOnlineLoanStage : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "UX_YeuCauMuonOnline_BanSaoChoNhan",
                table: "YeuCauMuonOnline");

            migrationBuilder.CreateIndex(
                name: "UX_YeuCauMuonOnline_BanSaoChoNhan",
                table: "YeuCauMuonOnline",
                column: "MaBanSao",
                unique: true,
                filter: "[TrangThai] IN (0, 4)");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "UX_YeuCauMuonOnline_BanSaoChoNhan",
                table: "YeuCauMuonOnline");

            migrationBuilder.CreateIndex(
                name: "UX_YeuCauMuonOnline_BanSaoChoNhan",
                table: "YeuCauMuonOnline",
                column: "MaBanSao",
                unique: true,
                filter: "[TrangThai] = 0");
        }
    }
}
