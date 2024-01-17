using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TasksWebApi.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class NotUniqueTaskListName : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_TaskLists_IsDeleted_Name",
                table: "TaskLists");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "a7b5e9df-3ad3-45f7-ba05-7123e45be3eb");

            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[] { "6d8e4a25-63b6-43ea-bb44-5ef04d6a5ef1", null, "SuperAdmin", "SUPERADMIN" });

            migrationBuilder.CreateIndex(
                name: "IX_TaskLists_IsDeleted_Name",
                table: "TaskLists",
                columns: new[] { "IsDeleted", "Name" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_TaskLists_IsDeleted_Name",
                table: "TaskLists");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "6d8e4a25-63b6-43ea-bb44-5ef04d6a5ef1");

            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[] { "a7b5e9df-3ad3-45f7-ba05-7123e45be3eb", null, "SuperAdmin", "SUPERADMIN" });

            migrationBuilder.CreateIndex(
                name: "IX_TaskLists_IsDeleted_Name",
                table: "TaskLists",
                columns: new[] { "IsDeleted", "Name" },
                unique: true);
        }
    }
}
