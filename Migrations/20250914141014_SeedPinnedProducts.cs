using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WhiteSoft.Migrations
{
    /// <inheritdoc />
    public partial class SeedPinnedProducts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "ImageUrl", "IsPinned" },
                values: new object[] { "/images/eshop.png", true });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "ImageUrl", "IsPinned" },
                values: new object[] { "/images/firma.png", true });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "ImageUrl", "IsPinned" },
                values: new object[] { "", false });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "ImageUrl", "IsPinned" },
                values: new object[] { "", false });
        }
    }
}
