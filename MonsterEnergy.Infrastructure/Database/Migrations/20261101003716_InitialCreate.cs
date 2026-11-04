using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MonsterEnergy.Infrastructure.Database.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Stores",
                columns: table => new
                {
                    StoreGuid = table.Column<Guid>(type: "TEXT", nullable: false),
                    Tdm = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Territory = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Region = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    StoreName = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Address = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    PostCode = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    StoreChannel = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    StoreFascia = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Date = table.Column<DateOnly>(type: "date", nullable: false),
                    StoreStart = table.Column<TimeOnly>(type: "time", nullable: false),
                    IsFavourite = table.Column<bool>(type: "INTEGER", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Stores", x => x.StoreGuid);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Stores_Date",
                table: "Stores",
                column: "Date");

            migrationBuilder.CreateIndex(
                name: "IX_Stores_Region",
                table: "Stores",
                column: "Region");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Stores");
        }
    }
}
