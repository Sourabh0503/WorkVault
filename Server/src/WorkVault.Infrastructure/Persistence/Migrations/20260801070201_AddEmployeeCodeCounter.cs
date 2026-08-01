using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WorkVault.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddEmployeeCodeCounter : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "EmployeeCodeCounters",
                columns: table => new
                {
                    CompanyId = table.Column<Guid>(type: "uuid", nullable: false),
                    Year = table.Column<int>(type: "integer", nullable: false),
                    LastValue = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmployeeCodeCounters", x => new { x.CompanyId, x.Year });
                });

            // Seed counters from existing employee codes (EMP-{year}-{seq}) so the first
            // allocation continues from the current high-water mark and never reuses a
            // number. Includes soft-deleted rows on purpose.
            migrationBuilder.Sql(
                """
                INSERT INTO "EmployeeCodeCounters" ("CompanyId", "Year", "LastValue")
                SELECT "CompanyId",
                       CAST(SPLIT_PART("EmployeeCode", '-', 2) AS INTEGER) AS yr,
                       MAX(CAST(SPLIT_PART("EmployeeCode", '-', 3) AS INTEGER))
                FROM "Employees"
                WHERE "EmployeeCode" ~ '^EMP-[0-9]+-[0-9]+$'
                GROUP BY "CompanyId", CAST(SPLIT_PART("EmployeeCode", '-', 2) AS INTEGER);
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EmployeeCodeCounters");
        }
    }
}
