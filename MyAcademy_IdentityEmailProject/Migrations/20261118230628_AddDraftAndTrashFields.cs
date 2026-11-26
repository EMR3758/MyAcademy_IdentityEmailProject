using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyAcademy_IdentityEmailProject.Migrations
{
    /// <inheritdoc />
    public partial class AddDraftAndTrashFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsDraft",
                table: "Messages",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsTrash",
                table: "Messages",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsDraft",
                table: "Messages");

            migrationBuilder.DropColumn(
                name: "IsTrash",
                table: "Messages");
        }
    }
}
