using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TechRent.Migrations
{
    /// <inheritdoc />
    public partial class AddSupplierFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "SupplierId",
                table: "Offices",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SupplierId",
                table: "Equipments",
                type: "TEXT",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Offices_SupplierId",
                table: "Offices",
                column: "SupplierId");

            migrationBuilder.CreateIndex(
                name: "IX_Equipments_SupplierId",
                table: "Equipments",
                column: "SupplierId");

            migrationBuilder.AddForeignKey(
                name: "FK_Equipments_AspNetUsers_SupplierId",
                table: "Equipments",
                column: "SupplierId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Offices_AspNetUsers_SupplierId",
                table: "Offices",
                column: "SupplierId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Equipments_AspNetUsers_SupplierId",
                table: "Equipments");

            migrationBuilder.DropForeignKey(
                name: "FK_Offices_AspNetUsers_SupplierId",
                table: "Offices");

            migrationBuilder.DropIndex(
                name: "IX_Offices_SupplierId",
                table: "Offices");

            migrationBuilder.DropIndex(
                name: "IX_Equipments_SupplierId",
                table: "Equipments");

            migrationBuilder.DropColumn(
                name: "SupplierId",
                table: "Offices");

            migrationBuilder.DropColumn(
                name: "SupplierId",
                table: "Equipments");
        }
    }
}
