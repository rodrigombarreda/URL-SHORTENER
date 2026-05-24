using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UrlShortener.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class MakeShortCodeNullable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_UrlTables_ShortCode",
                table: "UrlTables");

            migrationBuilder.AlterColumn<string>(
                name: "ShortCode",
                table: "UrlTables",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.CreateIndex(
                name: "IX_UrlTables_ShortCode",
                table: "UrlTables",
                column: "ShortCode",
                unique: true,
                filter: "[ShortCode] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_UrlTables_ShortCode",
                table: "UrlTables");

            migrationBuilder.AlterColumn<string>(
                name: "ShortCode",
                table: "UrlTables",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_UrlTables_ShortCode",
                table: "UrlTables",
                column: "ShortCode",
                unique: true);
        }
    }
}
