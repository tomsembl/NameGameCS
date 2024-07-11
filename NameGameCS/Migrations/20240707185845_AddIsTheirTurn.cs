using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NameGameCS.Migrations
{
    /// <inheritdoc />
    public partial class AddIsTheirTurn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "isTheirTurn",
                table: "UserInstances",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "isTheirTurn",
                table: "Teams",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "isTheirTurn",
                table: "UserInstances");

            migrationBuilder.DropColumn(
                name: "isTheirTurn",
                table: "Teams");
        }
    }
}
