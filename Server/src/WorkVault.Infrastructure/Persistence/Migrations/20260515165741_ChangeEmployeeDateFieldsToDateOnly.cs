using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WorkVault.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class ChangeEmployeeDateFieldsToDateOnly : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_InviteToken_Users_UserId",
                table: "InviteToken");

            migrationBuilder.DropPrimaryKey(
                name: "PK_InviteToken",
                table: "InviteToken");

            migrationBuilder.RenameTable(
                name: "InviteToken",
                newName: "InviteTokens");

            migrationBuilder.RenameIndex(
                name: "IX_InviteToken_UserId",
                table: "InviteTokens",
                newName: "IX_InviteTokens_UserId");

            migrationBuilder.AlterColumn<DateOnly>(
                name: "ResignationDate",
                table: "Employees",
                type: "date",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateOnly>(
                name: "LastWorkingDay",
                table: "Employees",
                type: "date",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateOnly>(
                name: "JoinDate",
                table: "Employees",
                type: "date",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone");

            migrationBuilder.AlterColumn<DateOnly>(
                name: "DateOfBirth",
                table: "Employees",
                type: "date",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldNullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_InviteTokens",
                table: "InviteTokens",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_InviteTokens_Users_UserId",
                table: "InviteTokens",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_InviteTokens_Users_UserId",
                table: "InviteTokens");

            migrationBuilder.DropPrimaryKey(
                name: "PK_InviteTokens",
                table: "InviteTokens");

            migrationBuilder.RenameTable(
                name: "InviteTokens",
                newName: "InviteToken");

            migrationBuilder.RenameIndex(
                name: "IX_InviteTokens_UserId",
                table: "InviteToken",
                newName: "IX_InviteToken_UserId");

            migrationBuilder.AlterColumn<DateTime>(
                name: "ResignationDate",
                table: "Employees",
                type: "timestamp with time zone",
                nullable: true,
                oldClrType: typeof(DateOnly),
                oldType: "date",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "LastWorkingDay",
                table: "Employees",
                type: "timestamp with time zone",
                nullable: true,
                oldClrType: typeof(DateOnly),
                oldType: "date",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "JoinDate",
                table: "Employees",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(DateOnly),
                oldType: "date");

            migrationBuilder.AlterColumn<DateTime>(
                name: "DateOfBirth",
                table: "Employees",
                type: "timestamp with time zone",
                nullable: true,
                oldClrType: typeof(DateOnly),
                oldType: "date",
                oldNullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_InviteToken",
                table: "InviteToken",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_InviteToken_Users_UserId",
                table: "InviteToken",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
