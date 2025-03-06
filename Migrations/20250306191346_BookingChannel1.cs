using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MAppBnB.Migrations
{
    /// <inheritdoc />
    public partial class BookingChannel1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "BookingChannel",
                table: "Booking",
                newName: "BookingDate");

            migrationBuilder.AlterColumn<int>(
                name: "IsPaid",
                table: "Booking",
                type: "int",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<int>(
                name: "ChannelID",
                table: "Booking",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "ContractPrinted",
                table: "Booking",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Sent2Police",
                table: "Booking",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Sent2Region",
                table: "Booking",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Sent2Town",
                table: "Booking",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<decimal>(
                name: "CleaningFee",
                table: "Accommodation",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "ContractPrinted",
                table: "Accommodation",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "TownFee",
                table: "Accommodation",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "BookingChannel",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Fee = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BookingChannel", x => x.id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BookingChannel");

            migrationBuilder.DropColumn(
                name: "ChannelID",
                table: "Booking");

            migrationBuilder.DropColumn(
                name: "ContractPrinted",
                table: "Booking");

            migrationBuilder.DropColumn(
                name: "Sent2Police",
                table: "Booking");

            migrationBuilder.DropColumn(
                name: "Sent2Region",
                table: "Booking");

            migrationBuilder.DropColumn(
                name: "Sent2Town",
                table: "Booking");

            migrationBuilder.DropColumn(
                name: "CleaningFee",
                table: "Accommodation");

            migrationBuilder.DropColumn(
                name: "ContractPrinted",
                table: "Accommodation");

            migrationBuilder.DropColumn(
                name: "TownFee",
                table: "Accommodation");

            migrationBuilder.RenameColumn(
                name: "BookingDate",
                table: "Booking",
                newName: "BookingChannel");

            migrationBuilder.AlterColumn<string>(
                name: "IsPaid",
                table: "Booking",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");
        }
    }
}
