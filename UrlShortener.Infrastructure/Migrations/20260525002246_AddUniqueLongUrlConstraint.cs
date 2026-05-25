using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UrlShortener.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddUniqueLongUrlConstraint : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_UrlTables_LongUrl",
                table: "UrlTables");

            migrationBuilder.CreateIndex(
                name: "IX_UrlTables_LongUrl",
                table: "UrlTables",
                column: "LongUrl",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_UrlTables_LongUrl",
                table: "UrlTables");

            migrationBuilder.CreateIndex(
                name: "IX_UrlTables_LongUrl",
                table: "UrlTables",
                column: "LongUrl");
        }
    }
}
