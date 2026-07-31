using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace bài_tập_1.Migrations
{
    public partial class ImproveReservationWorkflow : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "HanNhanSach",
                table: "DatTruoc",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "MaBanSaoDuocGiu",
                table: "DatTruoc",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "NgaySanSang",
                table: "DatTruoc",
                type: "datetime2",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "UX_DatTruoc_BanSaoDangGiu",
                table: "DatTruoc",
                column: "MaBanSaoDuocGiu",
                unique: true,
                filter: "[MaBanSaoDuocGiu] IS NOT NULL AND [TrangThai] = 1");

            migrationBuilder.AddForeignKey(
                name: "FK_DatTruoc_BanSao_MaBanSaoDuocGiu",
                table: "DatTruoc",
                column: "MaBanSaoDuocGiu",
                principalTable: "BanSao",
                principalColumn: "MaBanSao",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DatTruoc_BanSao_MaBanSaoDuocGiu",
                table: "DatTruoc");

            migrationBuilder.DropIndex(
                name: "UX_DatTruoc_BanSaoDangGiu",
                table: "DatTruoc");

            migrationBuilder.DropColumn(
                name: "HanNhanSach",
                table: "DatTruoc");

            migrationBuilder.DropColumn(
                name: "MaBanSaoDuocGiu",
                table: "DatTruoc");

            migrationBuilder.DropColumn(
                name: "NgaySanSang",
                table: "DatTruoc");
        }
    }
}
