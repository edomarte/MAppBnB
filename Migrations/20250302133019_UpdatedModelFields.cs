using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MAppBnB.Migrations
{
    /// <inheritdoc />
    public partial class UpdatedModelFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "email",
                table: "Person",
                newName: "Email");

            migrationBuilder.RenameColumn(
                name: "phone_prefix",
                table: "Person",
                newName: "PhonePrefix");

            migrationBuilder.RenameColumn(
                name: "phone_number",
                table: "Person",
                newName: "PhoneNumber");

            migrationBuilder.RenameColumn(
                name: "birth_province",
                table: "Person",
                newName: "BirthProvince");

            migrationBuilder.RenameColumn(
                name: "birth_place",
                table: "Person",
                newName: "BirthPlace");

            migrationBuilder.RenameColumn(
                name: "birth_country",
                table: "Person",
                newName: "BirthCountry");

            migrationBuilder.RenameColumn(
                name: "email",
                table: "Accommodation",
                newName: "Email");

            migrationBuilder.RenameColumn(
                name: "phone_prefix",
                table: "Accommodation",
                newName: "PhonePrefix");

            migrationBuilder.RenameColumn(
                name: "phone_number",
                table: "Accommodation",
                newName: "PhoneNumber");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Email",
                table: "Person",
                newName: "email");

            migrationBuilder.RenameColumn(
                name: "PhonePrefix",
                table: "Person",
                newName: "phone_prefix");

            migrationBuilder.RenameColumn(
                name: "PhoneNumber",
                table: "Person",
                newName: "phone_number");

            migrationBuilder.RenameColumn(
                name: "BirthProvince",
                table: "Person",
                newName: "birth_province");

            migrationBuilder.RenameColumn(
                name: "BirthPlace",
                table: "Person",
                newName: "birth_place");

            migrationBuilder.RenameColumn(
                name: "BirthCountry",
                table: "Person",
                newName: "birth_country");

            migrationBuilder.RenameColumn(
                name: "Email",
                table: "Accommodation",
                newName: "email");

            migrationBuilder.RenameColumn(
                name: "PhonePrefix",
                table: "Accommodation",
                newName: "phone_prefix");

            migrationBuilder.RenameColumn(
                name: "PhoneNumber",
                table: "Accommodation",
                newName: "phone_number");
        }
    }
}
