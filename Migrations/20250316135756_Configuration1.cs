using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MAppBnB.Migrations
{
    /// <inheritdoc />
    public partial class Configuration1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "GestioneAppartamentiID",
                table: "Configuration");

            migrationBuilder.AddColumn<bool>(
                name: "IsGestioneAppartamenti",
                table: "Configuration",
                type: "bit",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "AWIDAppartamento",
                table: "Accommodation",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsGestioneAppartamenti",
                table: "Configuration");

            migrationBuilder.AddColumn<string>(
                name: "GestioneAppartamentiID",
                table: "Configuration",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "AWIDAppartamento",
                table: "Accommodation",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);
        }
    }
}
