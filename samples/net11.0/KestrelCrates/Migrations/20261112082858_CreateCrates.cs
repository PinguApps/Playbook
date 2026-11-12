// snippet:complete
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KestrelCrates.Migrations;

/// <inheritdoc />
public partial class _20261112082858_CreateCrates : Migration
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

        migrationBuilder.CreateIndex(
            name: "IX_CratesSet_Label_Harbour_Label_Priority",
            table: "CratesSet",
            column: "Label");

        migrationBuilder.CreateIndex(
            name: "IX_CratesSet_Label_Slots_Code",
            table: "CratesSet",
            column: "Label");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "CratesSet");
    }
}

// endsnippet:complete
