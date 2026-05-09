using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WidgetDepot.ApiService.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddOrderItemWidgetForeignKey : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_OrderItems_WidgetId",
                table: "OrderItems",
                column: "WidgetId");

            migrationBuilder.AddForeignKey(
                name: "FK_OrderItems_Widgets_WidgetId",
                table: "OrderItems",
                column: "WidgetId",
                principalTable: "Widgets",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_OrderItems_Widgets_WidgetId",
                table: "OrderItems");

            migrationBuilder.DropIndex(
                name: "IX_OrderItems_WidgetId",
                table: "OrderItems");
        }
    }
}
