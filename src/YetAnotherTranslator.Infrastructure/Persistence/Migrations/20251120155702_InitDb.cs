using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace YetAnotherTranslator.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitDb : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "cache",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    cache_key = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    result_json = table.Column<string>(type: "jsonb", nullable: true),
                    result_byte = table.Column<byte[]>(type: "bytea", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_cache", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "history_entries",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    command_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    input_text = table.Column<string>(type: "character varying(6000)", maxLength: 6000, nullable: false),
                    output_text = table.Column<string>(type: "character varying(6000)", maxLength: 6000, nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_history_entries", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_cache_cache_key",
                table: "cache",
                column: "cache_key",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_cache_created_at",
                table: "cache",
                column: "created_at");

            migrationBuilder.CreateIndex(
                name: "IX_history_entries_created_at",
                table: "history_entries",
                column: "created_at");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "cache");

            migrationBuilder.DropTable(
                name: "history_entries");
        }
    }
}
