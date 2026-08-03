using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace bài_tập_1.Migrations
{
    public partial class AddPhanHoiReplyFields : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "NgayTraLoi",
                table: "PhanHoi",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NguoiTraLoi",
                table: "PhanHoi",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NoiDungTraLoi",
                table: "PhanHoi",
                type: "nvarchar(2000)",
                maxLength: 2000,
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "NgayTraLoi",
                table: "PhanHoi");

            migrationBuilder.DropColumn(
                name: "NguoiTraLoi",
                table: "PhanHoi");

            migrationBuilder.DropColumn(
                name: "NoiDungTraLoi",
                table: "PhanHoi");
        }
    }
}
