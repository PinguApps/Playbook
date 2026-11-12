// snippet:complete
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KestrelCrates.Migrations
{
    /// <inheritdoc />
    public partial class CreateCrates : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CratesSet",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false),
                    Label = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CratesSet", x => x.Id);
                });
            migrationBuilder.Sql("CREATE INDEX IX_Crates_Harbour_Priority ON CratesSet (Label ->> 'Harbour', Label ->> 'Priority' DESC)");
            migrationBuilder.Sql("CREATE INDEX IX_Crates_FirstSlot ON CratesSet (Label ->> '$.Slots[0].Code')");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CratesSet");
        }
    }
}

// endsnippet:complete
