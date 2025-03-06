using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Hub.Migrations
{
    /// <inheritdoc />
    public partial class CurrentSessionField : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "CurrentSessionStart",
                table: "TimeEntries",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CurrentSessionStart",
                table: "TimeEntries");
        }
    }
}
