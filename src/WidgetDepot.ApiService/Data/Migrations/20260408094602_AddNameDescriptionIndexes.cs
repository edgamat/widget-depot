using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WidgetDepot.ApiService.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddNameDescriptionIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"CREATE INDEX ""IX_Widgets_Name_Lower"" ON ""Widgets"" (lower(""Name""));");
            migrationBuilder.Sql(@"CREATE INDEX ""IX_Widgets_Description_Lower"" ON ""Widgets"" (lower(""Description""));");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"DROP INDEX ""IX_Widgets_Name_Lower"";");
            migrationBuilder.Sql(@"DROP INDEX ""IX_Widgets_Description_Lower"";");
        }
    }
}
