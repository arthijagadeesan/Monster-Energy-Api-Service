using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MonsterEnergy.Infrastructure.Database.Migrations
{
    /// <inheritdoc />
    public partial class PerUserFavourites : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsFavourite",
                table: "Stores");

            migrationBuilder.CreateTable(
                name: "UserFavourites",
                columns: table => new
                {
                    Username = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    StoreGuid = table.Column<Guid>(type: "TEXT", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserFavourites", x => new { x.Username, x.StoreGuid });
                    table.ForeignKey(
                        name: "FK_UserFavourites_Stores_StoreGuid",
                        column: x => x.StoreGuid,
                        principalTable: "Stores",
                        principalColumn: "StoreGuid",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Stores_StoreChannel",
                table: "Stores",
                column: "StoreChannel");

            migrationBuilder.CreateIndex(
                name: "IX_UserFavourites_StoreGuid",
                table: "UserFavourites",
                column: "StoreGuid");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserFavourites");

            migrationBuilder.DropIndex(
                name: "IX_Stores_StoreChannel",
                table: "Stores");

            migrationBuilder.AddColumn<bool>(
                name: "IsFavourite",
                table: "Stores",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);
        }
    }
}
