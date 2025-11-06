using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace YetAnotherTranslator.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "history_entries",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    command_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    input_text = table.Column<string>(type: "text", nullable: false),
                    output_text = table.Column<string>(type: "text", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_history_entries", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "llm_response_cache",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    cache_key = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    operation_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    request_hash = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    response_json = table.Column<string>(type: "jsonb", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    expires_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_llm_response_cache", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "pronunciation_cache",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    cache_key = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    text = table.Column<string>(type: "text", nullable: false),
                    part_of_speech = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    audio_data = table.Column<byte[]>(type: "bytea", nullable: false),
                    voice_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_pronunciation_cache", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "text_translation_cache",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    cache_key = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    source_language = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    target_language = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    input_text = table.Column<string>(type: "text", nullable: false),
                    translated_text = table.Column<string>(type: "text", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_text_translation_cache", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "translation_cache",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    cache_key = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    source_language = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    target_language = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    input_text = table.Column<string>(type: "text", nullable: false),
                    result_json = table.Column<string>(type: "jsonb", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_translation_cache", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_history_entries_created_at",
                table: "history_entries",
                column: "created_at");

            migrationBuilder.CreateIndex(
                name: "IX_llm_response_cache_cache_key",
                table: "llm_response_cache",
                column: "cache_key",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_llm_response_cache_created_at",
                table: "llm_response_cache",
                column: "created_at");

            migrationBuilder.CreateIndex(
                name: "IX_llm_response_cache_expires_at",
                table: "llm_response_cache",
                column: "expires_at");

            migrationBuilder.CreateIndex(
                name: "IX_pronunciation_cache_cache_key",
                table: "pronunciation_cache",
                column: "cache_key",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_pronunciation_cache_created_at",
                table: "pronunciation_cache",
                column: "created_at");

            migrationBuilder.CreateIndex(
                name: "IX_text_translation_cache_cache_key",
                table: "text_translation_cache",
                column: "cache_key",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_text_translation_cache_created_at",
                table: "text_translation_cache",
                column: "created_at");

            migrationBuilder.CreateIndex(
                name: "IX_translation_cache_cache_key",
                table: "translation_cache",
                column: "cache_key",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_translation_cache_created_at",
                table: "translation_cache",
                column: "created_at");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "history_entries");

            migrationBuilder.DropTable(
                name: "llm_response_cache");

            migrationBuilder.DropTable(
                name: "pronunciation_cache");

            migrationBuilder.DropTable(
                name: "text_translation_cache");

            migrationBuilder.DropTable(
                name: "translation_cache");
        }
    }
}
