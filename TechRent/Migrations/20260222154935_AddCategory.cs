using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TechRent.Migrations
{
    /// <inheritdoc />
    public partial class AddCategory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Type",
                table: "Equipments");

            migrationBuilder.AddColumn<int>(
                name: "CategoryId",
                table: "Equipments",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Categories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Description = table.Column<string>(type: "TEXT", nullable: true),
                    DisplayOrder = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Categories", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Equipments_CategoryId",
                table: "Equipments",
                column: "CategoryId");

            migrationBuilder.AddForeignKey(
                name: "FK_Equipments_Categories_CategoryId",
                table: "Equipments",
                column: "CategoryId",
                principalTable: "Categories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Equipments_Categories_CategoryId",
                table: "Equipments");

            migrationBuilder.DropTable(
                name: "Categories");

            migrationBuilder.DropIndex(
                name: "IX_Equipments_CategoryId",
                table: "Equipments");

            migrationBuilder.DropColumn(
                name: "CategoryId",
                table: "Equipments");

            migrationBuilder.AddColumn<string>(
                name: "Type",
                table: "Equipments",
                type: "TEXT",
                nullable: false,
                defaultValue: "");
        }
    }
}
