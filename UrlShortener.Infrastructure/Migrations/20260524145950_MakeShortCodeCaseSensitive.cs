using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UrlShortener.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class MakeShortCodeCaseSensitive : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "ShortCode",
                table: "UrlTables",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true,
                collation: "Latin1_General_100_BIN2",
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "ShortCode",
                table: "UrlTables",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20,
                oldNullable: true,
                oldCollation: "Latin1_General_100_BIN2");
        }
    }
}
