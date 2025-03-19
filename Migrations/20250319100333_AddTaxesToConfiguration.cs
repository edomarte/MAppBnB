using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MAppBnB.Migrations
{
    /// <inheritdoc />
    public partial class AddTaxesToConfiguration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "CedolareSecca",
                table: "Configuration",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "CommissioneBancaria",
                table: "Configuration",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "IVACommissioni",
                table: "Configuration",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "IVAVendite",
                table: "Configuration",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CedolareSecca",
                table: "Configuration");

            migrationBuilder.DropColumn(
                name: "CommissioneBancaria",
                table: "Configuration");

            migrationBuilder.DropColumn(
                name: "IVACommissioni",
                table: "Configuration");

            migrationBuilder.DropColumn(
                name: "IVAVendite",
                table: "Configuration");
        }
    }
}
