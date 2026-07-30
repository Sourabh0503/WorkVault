using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WorkVault.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class EmployeeCodeUniquePerCompany : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Employees_EmployeeCode",
                table: "Employees");

            migrationBuilder.CreateIndex(
                name: "IX_Employees_EmployeeCode_CompanyId",
                table: "Employees",
                columns: new[] { "EmployeeCode", "CompanyId" },
                unique: true,
                filter: "\"IsDeleted\" = false");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Employees_EmployeeCode_CompanyId",
                table: "Employees");

            migrationBuilder.CreateIndex(
                name: "IX_Employees_EmployeeCode",
                table: "Employees",
                column: "EmployeeCode",
                unique: true,
                filter: "\"IsDeleted\" = false");
        }
    }
}
