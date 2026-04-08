using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataBase.Migrations
{
    /// <inheritdoc />
    public partial class AddManagerRoutingFlow : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ManagerUserId",
                table: "Tasks",
                type: "integer",
                nullable: true);

            migrationBuilder.Sql("""
                UPDATE "Tasks"
                SET "ManagerUserId" = "CreatedByUserId"
                WHERE "ManagerUserId" IS NULL;
                """);

            migrationBuilder.AlterColumn<int>(
                name: "ManagerUserId",
                table: "Tasks",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Tasks_ManagerUserId",
                table: "Tasks",
                column: "ManagerUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Tasks_Users_ManagerUserId",
                table: "Tasks",
                column: "ManagerUserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Tasks_Users_ManagerUserId",
                table: "Tasks");

            migrationBuilder.DropIndex(
                name: "IX_Tasks_ManagerUserId",
                table: "Tasks");

            migrationBuilder.DropColumn(
                name: "ManagerUserId",
                table: "Tasks");
        }
    }
}
