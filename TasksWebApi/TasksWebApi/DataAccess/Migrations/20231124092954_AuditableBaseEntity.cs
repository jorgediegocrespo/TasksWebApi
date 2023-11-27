using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TasksWebApi.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AuditableBaseEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "27bd0248-69fa-4bab-ab1b-07b5fafc9b56");

            migrationBuilder.AddColumn<string>(
                name: "CreationUser",
                table: "Tasks",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DateOfCreation",
                table: "Tasks",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "DateOfDelete",
                table: "Tasks",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DateOfLastUpdate",
                table: "Tasks",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeleteUser",
                table: "Tasks",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LastUpdateUser",
                table: "Tasks",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CreationUser",
                table: "TaskLists",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DateOfCreation",
                table: "TaskLists",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "DateOfDelete",
                table: "TaskLists",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DateOfLastUpdate",
                table: "TaskLists",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeleteUser",
                table: "TaskLists",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LastUpdateUser",
                table: "TaskLists",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[] { "c3025b21-c04b-4b48-bd7c-84412555594b", null, "SuperAdmin", "SUPERADMIN" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "c3025b21-c04b-4b48-bd7c-84412555594b");

            migrationBuilder.DropColumn(
                name: "CreationUser",
                table: "Tasks");

            migrationBuilder.DropColumn(
                name: "DateOfCreation",
                table: "Tasks");

            migrationBuilder.DropColumn(
                name: "DateOfDelete",
                table: "Tasks");

            migrationBuilder.DropColumn(
                name: "DateOfLastUpdate",
                table: "Tasks");

            migrationBuilder.DropColumn(
                name: "DeleteUser",
                table: "Tasks");

            migrationBuilder.DropColumn(
                name: "LastUpdateUser",
                table: "Tasks");

            migrationBuilder.DropColumn(
                name: "CreationUser",
                table: "TaskLists");

            migrationBuilder.DropColumn(
                name: "DateOfCreation",
                table: "TaskLists");

            migrationBuilder.DropColumn(
                name: "DateOfDelete",
                table: "TaskLists");

            migrationBuilder.DropColumn(
                name: "DateOfLastUpdate",
                table: "TaskLists");

            migrationBuilder.DropColumn(
                name: "DeleteUser",
                table: "TaskLists");

            migrationBuilder.DropColumn(
                name: "LastUpdateUser",
                table: "TaskLists");

            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[] { "27bd0248-69fa-4bab-ab1b-07b5fafc9b56", "86499b70-e78a-49af-88a9-9348db5d56ed", "SuperAdmin", "SUPERADMIN" });
        }
    }
}
