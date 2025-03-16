using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MAppBnB.Migrations
{
    /// <inheritdoc />
    public partial class Configuration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IssuedBy",
                table: "Document");

            migrationBuilder.DropColumn(
                name: "IssuedDate",
                table: "Document");

            migrationBuilder.AlterColumn<string>(
                name: "DataFineVal",
                table: "Stati",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AlterColumn<string>(
                name: "RoleRelation",
                table: "Person",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<string>(
                name: "DocumentType",
                table: "Document",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "DataFineVal",
                table: "Comuni",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AddColumn<string>(
                name: "AWIDAppartamento",
                table: "Accommodation",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "Configuration",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PersonID = table.Column<int>(type: "int", nullable: true),
                    DocumentID = table.Column<int>(type: "int", nullable: true),
                    AlloggiatiWebUsername = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AlloggiatiWebPassword = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AlloggiatiWebWSKey = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    GestioneAppartamentiID = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Configuration", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "Province",
                columns: table => new
                {
                    Codice = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Descrizione = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Province", x => x.Codice);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Configuration");

            migrationBuilder.DropTable(
                name: "Province");

            migrationBuilder.DropColumn(
                name: "AWIDAppartamento",
                table: "Accommodation");

            migrationBuilder.AlterColumn<DateTime>(
                name: "DataFineVal",
                table: "Stati",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "RoleRelation",
                table: "Person",
                type: "int",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<int>(
                name: "DocumentType",
                table: "Document",
                type: "int",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "IssuedBy",
                table: "Document",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateOnly>(
                name: "IssuedDate",
                table: "Document",
                type: "date",
                nullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "DataFineVal",
                table: "Comuni",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);
        }
    }
}
