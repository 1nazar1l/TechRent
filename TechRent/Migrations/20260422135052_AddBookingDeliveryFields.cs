using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TechRent.Migrations
{
    /// <inheritdoc />
    public partial class AddBookingDeliveryFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DeliveryAddress",
                table: "Bookings",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "DeliveryCost",
                table: "Bookings",
                type: "TEXT",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<double>(
                name: "DeliveryLat",
                table: "Bookings",
                type: "REAL",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "DeliveryLng",
                table: "Bookings",
                type: "REAL",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "OfficeId",
                table: "Bookings",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Bookings_OfficeId",
                table: "Bookings",
                column: "OfficeId");

            migrationBuilder.AddForeignKey(
                name: "FK_Bookings_Offices_OfficeId",
                table: "Bookings",
                column: "OfficeId",
                principalTable: "Offices",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Bookings_Offices_OfficeId",
                table: "Bookings");

            migrationBuilder.DropIndex(
                name: "IX_Bookings_OfficeId",
                table: "Bookings");

            migrationBuilder.DropColumn(
                name: "DeliveryAddress",
                table: "Bookings");

            migrationBuilder.DropColumn(
                name: "DeliveryCost",
                table: "Bookings");

            migrationBuilder.DropColumn(
                name: "DeliveryLat",
                table: "Bookings");

            migrationBuilder.DropColumn(
                name: "DeliveryLng",
                table: "Bookings");

            migrationBuilder.DropColumn(
                name: "OfficeId",
                table: "Bookings");
        }
    }
}
