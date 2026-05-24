using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UrlShortener.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddIndexesToUrlTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "ShortCode",
                table: "UrlTables",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "LongUrl",
                table: "UrlTables",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.CreateIndex(
                name: "IX_UrlTables_LongUrl",
                table: "UrlTables",
                column: "LongUrl");

            migrationBuilder.CreateIndex(
                name: "IX_UrlTables_ShortCode",
                table: "UrlTables",
                column: "ShortCode",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_UrlTables_LongUrl",
                table: "UrlTables");

            migrationBuilder.DropIndex(
                name: "IX_UrlTables_ShortCode",
                table: "UrlTables");

            migrationBuilder.AlterColumn<string>(
                name: "ShortCode",
                table: "UrlTables",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AlterColumn<string>(
                name: "LongUrl",
                table: "UrlTables",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");
        }
    }
}
