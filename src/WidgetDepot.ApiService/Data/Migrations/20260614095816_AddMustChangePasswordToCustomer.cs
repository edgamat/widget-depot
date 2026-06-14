using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WidgetDepot.ApiService.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddMustChangePasswordToCustomer : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "MustChangePassword",
                table: "Customers",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MustChangePassword",
                table: "Customers");
        }
    }
}
