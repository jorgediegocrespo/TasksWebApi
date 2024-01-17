using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TasksWebApi.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class ListNameIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_TaskLists_Name",
                table: "TaskLists");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "c3025b21-c04b-4b48-bd7c-84412555594b");

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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
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
                values: new object[] { "c3025b21-c04b-4b48-bd7c-84412555594b", null, "SuperAdmin", "SUPERADMIN" });

            migrationBuilder.CreateIndex(
                name: "IX_TaskLists_Name",
                table: "TaskLists",
                column: "Name",
                unique: true);
        }
    }
}
