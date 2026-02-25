using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Lyke.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddStatureAndBuild : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Build",
                table: "BodyProfiles",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Stature",
                table: "BodyProfiles",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            // Data migration: map existing FrameSizeId values to new Stature and Build columns
            // FrameSizeId 1 (Petite)  → Stature=Petite, Build=Standard
            // FrameSizeId 2 (Average) → Stature=Average, Build=Standard
            // FrameSizeId 3 (Tall)    → Stature=Tall, Build=Standard
            // FrameSizeId 4 (Plus)    → Stature=Average, Build=Plus
            migrationBuilder.Sql("""
                UPDATE "BodyProfiles" SET
                    "Stature" = CASE "FrameSizeId"
                        WHEN 1 THEN 'Petite'
                        WHEN 2 THEN 'Average'
                        WHEN 3 THEN 'Tall'
                        WHEN 4 THEN 'Average'
                        ELSE NULL
                    END,
                    "Build" = CASE "FrameSizeId"
                        WHEN 1 THEN 'Standard'
                        WHEN 2 THEN 'Standard'
                        WHEN 3 THEN 'Standard'
                        WHEN 4 THEN 'Plus'
                        ELSE NULL
                    END
                WHERE "FrameSizeId" IS NOT NULL;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Build",
                table: "BodyProfiles");

            migrationBuilder.DropColumn(
                name: "Stature",
                table: "BodyProfiles");
        }
    }
}
