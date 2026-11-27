using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyAcademy_IdentityEmailProject.Migrations
{
    public partial class mig2 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ValidCode",
                table: "AspNetUsers",
                type: "int",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ValidCode",
                table: "AspNetUsers");
        }
    }
}
