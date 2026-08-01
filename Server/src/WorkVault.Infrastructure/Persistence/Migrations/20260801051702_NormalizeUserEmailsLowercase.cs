using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WorkVault.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class NormalizeUserEmailsLowercase : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Normalize existing user emails to trimmed lowercase so they match the
            // now-normalized writes/lookups. Only touches rows that actually differ.
            migrationBuilder.Sql(
                "UPDATE \"Users\" SET \"Email\" = LOWER(TRIM(\"Email\")) " +
                "WHERE \"Email\" <> LOWER(TRIM(\"Email\"));");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // No-op: the original casing can't be restored.
        }
    }
}
