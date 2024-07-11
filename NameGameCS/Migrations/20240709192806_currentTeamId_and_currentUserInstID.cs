using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NameGameCS.Migrations
{
    /// <inheritdoc />
    public partial class currentTeamId_and_currentUserInstID : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "isTheirTurn",
                table: "UserInstances");

            migrationBuilder.RenameColumn(
                name: "isTheirTurn",
                table: "Teams",
                newName: "current_user_inst_id");

            migrationBuilder.AddColumn<int>(
                name: "current_team_id",
                table: "Games",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "current_team_id",
                table: "Games");

            migrationBuilder.RenameColumn(
                name: "current_user_inst_id",
                table: "Teams",
                newName: "isTheirTurn");

            migrationBuilder.AddColumn<bool>(
                name: "isTheirTurn",
                table: "UserInstances",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);
        }
    }
}
