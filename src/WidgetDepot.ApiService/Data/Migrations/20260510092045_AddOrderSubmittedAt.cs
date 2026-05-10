using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WidgetDepot.ApiService.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddOrderSubmittedAt : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "SubmittedAt",
                table: "Orders",
                type: "timestamp with time zone",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SubmittedAt",
                table: "Orders");
        }
    }
}
