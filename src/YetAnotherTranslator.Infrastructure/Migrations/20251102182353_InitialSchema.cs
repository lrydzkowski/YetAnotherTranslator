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
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Timestamp = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CommandType = table.Column<string>(type: "text", nullable: false),
                    InputText = table.Column<string>(type: "character varying(5000)", maxLength: 5000, nullable: false),
                    OutputText = table.Column<string>(type: "jsonb", nullable: false),
                    LlmMetadata = table.Column<string>(type: "jsonb", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_history_entries", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "pronunciation_cache",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CacheKey = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Text = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    PartOfSpeech = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    VoiceId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    AudioData = table.Column<byte[]>(type: "bytea", nullable: false),
                    AudioFormat = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    AudioSizeBytes = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    AccessedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    AccessCount = table.Column<int>(type: "integer", nullable: false, defaultValue: 1)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_pronunciation_cache", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "text_translation_cache",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CacheKey = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    SourceTextHash = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    SourceLanguage = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    TargetLanguage = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    ResultJson = table.Column<string>(type: "jsonb", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    AccessedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    AccessCount = table.Column<int>(type: "integer", nullable: false, defaultValue: 1)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_text_translation_cache", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "translation_cache",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CacheKey = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    SourceWord = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    SourceLanguage = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    TargetLanguage = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    ResultJson = table.Column<string>(type: "jsonb", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    AccessedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    AccessCount = table.Column<int>(type: "integer", nullable: false, defaultValue: 1)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_translation_cache", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_history_entries_CommandType",
                table: "history_entries",
                column: "CommandType");

            migrationBuilder.CreateIndex(
                name: "IX_history_entries_Timestamp",
                table: "history_entries",
                column: "Timestamp");

            migrationBuilder.CreateIndex(
                name: "IX_pronunciation_cache_CacheKey",
                table: "pronunciation_cache",
                column: "CacheKey",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_pronunciation_cache_CreatedAt",
                table: "pronunciation_cache",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_text_translation_cache_CacheKey",
                table: "text_translation_cache",
                column: "CacheKey",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_text_translation_cache_CreatedAt",
                table: "text_translation_cache",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_translation_cache_CacheKey",
                table: "translation_cache",
                column: "CacheKey",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_translation_cache_CreatedAt",
                table: "translation_cache",
                column: "CreatedAt");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "history_entries");

            migrationBuilder.DropTable(
                name: "pronunciation_cache");

            migrationBuilder.DropTable(
                name: "text_translation_cache");

            migrationBuilder.DropTable(
                name: "translation_cache");
        }
    }
}
