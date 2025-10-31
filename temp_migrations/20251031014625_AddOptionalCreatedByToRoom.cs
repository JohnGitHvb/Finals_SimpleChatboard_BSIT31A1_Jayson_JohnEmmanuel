using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SimpleChatboard.Web.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddOptionalCreatedByToRoom : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsPrivate",
                table: "Rooms");

            migrationBuilder.RenameColumn(
                name: "JoinCode",
                table: "Rooms",
                newName: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Rooms_CreatedByUserId",
                table: "Rooms",
                column: "CreatedByUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Rooms_AspNetUsers_CreatedByUserId",
                table: "Rooms",
                column: "CreatedByUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Rooms_AspNetUsers_CreatedByUserId",
                table: "Rooms");

            migrationBuilder.DropIndex(
                name: "IX_Rooms_CreatedByUserId",
                table: "Rooms");

            migrationBuilder.RenameColumn(
                name: "CreatedByUserId",
                table: "Rooms",
                newName: "JoinCode");

            migrationBuilder.AddColumn<bool>(
                name: "IsPrivate",
                table: "Rooms",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);
        }
    }
}
