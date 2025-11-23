using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace YetAnotherTranslator.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddInputJsonToCache : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "input_json",
                table: "cache",
                type: "jsonb",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "input_json",
                table: "cache");
        }
    }
}
