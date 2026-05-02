using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WidgetDepot.ApiService.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddOrderShippingEstimate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "ShippingEstimate",
                table: "Orders",
                type: "numeric(10,2)",
                precision: 10,
                scale: 2,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ShippingEstimate",
                table: "Orders");
        }
    }
}
