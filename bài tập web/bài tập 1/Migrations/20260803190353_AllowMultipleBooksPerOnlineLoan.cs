using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace bài_tập_1.Migrations
{
    public partial class AllowMultipleBooksPerOnlineLoan : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_YeuCauMuonOnline_MaXacNhan",
                table: "YeuCauMuonOnline");

            migrationBuilder.CreateIndex(
                name: "IX_YeuCauMuonOnline_MaXacNhan",
                table: "YeuCauMuonOnline",
                column: "MaXacNhan");

            migrationBuilder.CreateIndex(
                name: "IX_YeuCauMuonOnline_MaXacNhan_MaBanSao",
                table: "YeuCauMuonOnline",
                columns: new[] { "MaXacNhan", "MaBanSao" },
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_YeuCauMuonOnline_MaXacNhan",
                table: "YeuCauMuonOnline");

            migrationBuilder.DropIndex(
                name: "IX_YeuCauMuonOnline_MaXacNhan_MaBanSao",
                table: "YeuCauMuonOnline");

            migrationBuilder.CreateIndex(
                name: "IX_YeuCauMuonOnline_MaXacNhan",
                table: "YeuCauMuonOnline",
                column: "MaXacNhan",
                unique: true);
        }
    }
}
