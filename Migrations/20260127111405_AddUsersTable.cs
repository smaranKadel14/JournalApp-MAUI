using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace JournalApp.Migrations
{
    /// <inheritdoc />
    public partial class AddUsersTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Tags",
                table: "JournalEntries",
                newName: "TagsCsv");

            migrationBuilder.RenameColumn(
                name: "SecondaryMoods",
                table: "JournalEntries",
                newName: "SecondaryMoodsCsv");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "TagsCsv",
                table: "JournalEntries",
                newName: "Tags");

            migrationBuilder.RenameColumn(
                name: "SecondaryMoodsCsv",
                table: "JournalEntries",
                newName: "SecondaryMoods");
        }
    }
}
