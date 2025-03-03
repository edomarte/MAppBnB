using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MAppBnB.Migrations
{
    /// <inheritdoc />
    public partial class DocumentTypes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "DocumentType",
                table: "Document",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DocumentType",
                table: "Document");
        }
    }
}
