using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WidgetDepot.ApiService.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddCustomerAddresses : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "BillingCity",
                table: "Customers",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BillingRecipientName",
                table: "Customers",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BillingState",
                table: "Customers",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BillingStreetLine1",
                table: "Customers",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BillingStreetLine2",
                table: "Customers",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BillingZipCode",
                table: "Customers",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ShippingCity",
                table: "Customers",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ShippingRecipientName",
                table: "Customers",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ShippingState",
                table: "Customers",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ShippingStreetLine1",
                table: "Customers",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ShippingStreetLine2",
                table: "Customers",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ShippingZipCode",
                table: "Customers",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BillingCity",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "BillingRecipientName",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "BillingState",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "BillingStreetLine1",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "BillingStreetLine2",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "BillingZipCode",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "ShippingCity",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "ShippingRecipientName",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "ShippingState",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "ShippingStreetLine1",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "ShippingStreetLine2",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "ShippingZipCode",
                table: "Customers");
        }
    }
}
