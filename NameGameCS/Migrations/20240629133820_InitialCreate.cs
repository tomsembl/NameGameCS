using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NameGameCS.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Answers",
                columns: table => new
                {
                    answer_id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    game_id = table.Column<int>(type: "INTEGER", nullable: false),
                    team_id = table.Column<int>(type: "INTEGER", nullable: false),
                    user_inst_id = table.Column<int>(type: "INTEGER", nullable: false),
                    name_id = table.Column<int>(type: "INTEGER", nullable: false),
                    name = table.Column<string>(type: "TEXT", maxLength: 40, nullable: false),
                    success = table.Column<bool>(type: "INTEGER", nullable: false),
                    round = table.Column<int>(type: "INTEGER", nullable: false),
                    time_start = table.Column<DateTime>(type: "TEXT", nullable: false),
                    time_finish = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Answers", x => x.answer_id);
                });

            migrationBuilder.CreateTable(
                name: "DefaultNames",
                columns: table => new
                {
                    default_name_id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    name = table.Column<string>(type: "TEXT", maxLength: 40, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DefaultNames", x => x.default_name_id);
                });

            migrationBuilder.CreateTable(
                name: "Games",
                columns: table => new
                {
                    game_id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    game_name = table.Column<string>(type: "TEXT", maxLength: 40, nullable: false),
                    is_active = table.Column<bool>(type: "INTEGER", nullable: false),
                    stage = table.Column<int>(type: "INTEGER", nullable: false),
                    round = table.Column<int>(type: "INTEGER", nullable: false),
                    number_teams = table.Column<int>(type: "INTEGER", nullable: false),
                    number_names = table.Column<int>(type: "INTEGER", nullable: false),
                    time_limit_sec = table.Column<int>(type: "INTEGER", nullable: false),
                    round1 = table.Column<bool>(type: "INTEGER", nullable: false),
                    round2 = table.Column<bool>(type: "INTEGER", nullable: false),
                    round3 = table.Column<bool>(type: "INTEGER", nullable: false),
                    round4 = table.Column<bool>(type: "INTEGER", nullable: false),
                    date_created = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Games", x => x.game_id);
                });

            migrationBuilder.CreateTable(
                name: "Mp3Order",
                columns: table => new
                {
                    id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    number_stops = table.Column<int>(type: "INTEGER", nullable: false),
                    current_stop = table.Column<int>(type: "INTEGER", nullable: false),
                    number_starts = table.Column<int>(type: "INTEGER", nullable: false),
                    current_start = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Mp3Order", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "Names",
                columns: table => new
                {
                    name_id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    game_id = table.Column<int>(type: "INTEGER", nullable: false),
                    user_inst_id = table.Column<int>(type: "INTEGER", nullable: false),
                    name = table.Column<string>(type: "TEXT", maxLength: 40, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Names", x => x.name_id);
                });

            migrationBuilder.CreateTable(
                name: "Teams",
                columns: table => new
                {
                    team_id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    game_id = table.Column<int>(type: "INTEGER", nullable: false),
                    team_name = table.Column<string>(type: "TEXT", maxLength: 40, nullable: false),
                    order = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Teams", x => x.team_id);
                });

            migrationBuilder.CreateTable(
                name: "Turns",
                columns: table => new
                {
                    turn_id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    user_inst_id = table.Column<int>(type: "INTEGER", nullable: false),
                    game_id = table.Column<int>(type: "INTEGER", nullable: false),
                    round = table.Column<int>(type: "INTEGER", nullable: false),
                    time_start = table.Column<DateTime>(type: "TEXT", nullable: false),
                    time_finish = table.Column<DateTime>(type: "TEXT", nullable: false),
                    isActive = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Turns", x => x.turn_id);
                });

            migrationBuilder.CreateTable(
                name: "UserInstances",
                columns: table => new
                {
                    user_inst_id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    user_id = table.Column<int>(type: "INTEGER", nullable: false),
                    username = table.Column<string>(type: "TEXT", maxLength: 40, nullable: false),
                    team_id = table.Column<int>(type: "INTEGER", nullable: false),
                    game_id = table.Column<int>(type: "INTEGER", nullable: false),
                    order = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserInstances", x => x.user_inst_id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    user_id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    username = table.Column<string>(type: "TEXT", maxLength: 40, nullable: false),
                    created = table.Column<DateTime>(type: "TEXT", nullable: false),
                    last_login = table.Column<DateTime>(type: "TEXT", nullable: false),
                    last_ip = table.Column<string>(type: "TEXT", maxLength: 30, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.user_id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Answers");

            migrationBuilder.DropTable(
                name: "DefaultNames");

            migrationBuilder.DropTable(
                name: "Games");

            migrationBuilder.DropTable(
                name: "Mp3Order");

            migrationBuilder.DropTable(
                name: "Names");

            migrationBuilder.DropTable(
                name: "Teams");

            migrationBuilder.DropTable(
                name: "Turns");

            migrationBuilder.DropTable(
                name: "UserInstances");

            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}
