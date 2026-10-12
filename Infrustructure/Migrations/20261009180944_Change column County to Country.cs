using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrustructure.Migrations
{
    /// <inheritdoc />
    public partial class ChangecolumnCountytoCountry : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Address_County",
                table: "Users",
                newName: "Address_Country");

            migrationBuilder.RenameColumn(
                name: "Address_County",
                table: "Companies",
                newName: "Address_Country");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Address_Country",
                table: "Users",
                newName: "Address_County");

            migrationBuilder.RenameColumn(
                name: "Address_Country",
                table: "Companies",
                newName: "Address_County");
        }
    }
}
