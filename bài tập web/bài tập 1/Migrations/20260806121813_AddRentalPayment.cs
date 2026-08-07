using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace bài_tập_1.Migrations
{
    public partial class AddRentalPayment : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "NgayThanhToan",
                table: "PhieuMuon",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "SoTienDaThanhToan",
                table: "PhieuMuon",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "TrangThaiThanhToan",
                table: "PhieuMuon",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<decimal>(
                name: "PhiThue",
                table: "ChiTietPhieuMuon",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "NgayThanhToan",
                table: "PhieuMuon");

            migrationBuilder.DropColumn(
                name: "SoTienDaThanhToan",
                table: "PhieuMuon");

            migrationBuilder.DropColumn(
                name: "TrangThaiThanhToan",
                table: "PhieuMuon");

            migrationBuilder.DropColumn(
                name: "PhiThue",
                table: "ChiTietPhieuMuon");
        }
    }
}
