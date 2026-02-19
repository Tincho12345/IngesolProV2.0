using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ApiIngesol.Migrations
{
    /// <inheritdoc />
    public partial class FirstMigration1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<bool>(
                name: "IsActiveBackground",
                table: "BackgroundImages",
                type: "bit",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.CreateIndex(
                name: "IX_BackgroundImages_IsActiveBackground",
                table: "BackgroundImages",
                column: "IsActiveBackground",
                unique: true,
                filter: "[IsActiveBackground] = 1");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_BackgroundImages_IsActiveBackground",
                table: "BackgroundImages");

            migrationBuilder.AlterColumn<bool>(
                name: "IsActiveBackground",
                table: "BackgroundImages",
                type: "bit",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldDefaultValue: false);
        }
    }
}
