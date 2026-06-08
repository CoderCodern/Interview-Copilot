using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InterviewCopilot.Infrastructure.Migrations;

/// <inheritdoc />
public partial class InitialCreate : Migration
{
    private static readonly string[] s_ownerCreatedColumns = ["owner_id", "created_at"];

    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AlterDatabase()
            .Annotation("Npgsql:PostgresExtension:pg_trgm", ",,")
            .Annotation("Npgsql:PostgresExtension:vector", ",,");

        migrationBuilder.CreateTable(
            name: "resumes",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                owner_id = table.Column<Guid>(type: "uuid", nullable: false),
                status = table.Column<string>(type: "text", nullable: false),
                profile = table.Column<string>(type: "jsonb", nullable: true),
                is_current = table.Column<bool>(type: "boolean", nullable: false),
                error = table.Column<string>(type: "text", nullable: true),
                created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                source = table.Column<string>(type: "jsonb", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_resumes", x => x.id);
            });

        migrationBuilder.CreateIndex(
            name: "IX_resumes_owner_id_created_at",
            table: "resumes",
            columns: s_ownerCreatedColumns);

        migrationBuilder.CreateIndex(
            name: "ux_resumes_one_current",
            table: "resumes",
            column: "owner_id",
            unique: true,
            filter: "is_current");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "resumes");
    }
}
