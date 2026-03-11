using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Lyke.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class MultiAxisBodyTypeAndMultiSelectFitPreferences : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // ── 1. Create new tables ────────────────────────────────────────

            migrationBuilder.CreateTable(
                name: "FrameSizes",
                columns: table => new
                {
                    Id = table
                        .Column<int>(type: "integer", nullable: false)
                        .Annotation(
                            "Npgsql:ValueGenerationStrategy",
                            NpgsqlValueGenerationStrategy.IdentityByDefaultColumn
                        ),
                    Name = table.Column<string>(
                        type: "character varying(100)",
                        maxLength: 100,
                        nullable: false
                    ),
                    Description = table.Column<string>(
                        type: "character varying(500)",
                        maxLength: 500,
                        nullable: true
                    ),
                    DisplayOrder = table.Column<int>(type: "integer", nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FrameSizes", x => x.Id);
                }
            );

            migrationBuilder.InsertData(
                table: "FrameSizes",
                columns: new[] { "Id", "Description", "DisplayOrder", "Name" },
                values: new object[,]
                {
                    { 1, "Shorter stature with a smaller overall frame", 1, "Petite" },
                    { 2, "Medium height and proportional build", 2, "Average" },
                    { 3, "Taller stature with a longer frame", 3, "Tall" },
                    { 4, "Fuller figure across all areas", 4, "Plus" },
                }
            );

            migrationBuilder.CreateTable(
                name: "BodyProfileFitPreferences",
                columns: table => new
                {
                    BodyProfileId = table.Column<Guid>(type: "uuid", nullable: false),
                    FitPreference = table.Column<string>(
                        type: "character varying(20)",
                        maxLength: 20,
                        nullable: false
                    ),
                },
                constraints: table =>
                {
                    table.PrimaryKey(
                        "PK_BodyProfileFitPreferences",
                        x => new { x.BodyProfileId, x.FitPreference }
                    );
                    table.ForeignKey(
                        name: "FK_BodyProfileFitPreferences_BodyProfiles_BodyProfileId",
                        column: x => x.BodyProfileId,
                        principalTable: "BodyProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade
                    );
                }
            );

            // ── 2. Add FrameSizeId column ───────────────────────────────────

            migrationBuilder.AddColumn<int>(
                name: "FrameSizeId",
                table: "BodyProfiles",
                type: "integer",
                nullable: true
            );

            // ── 3. Drop old index (before data migration) ──────────────────

            migrationBuilder.DropIndex(
                name: "IX_BodyProfiles_BodyTypeId_HeightCm_WeightKg",
                table: "BodyProfiles"
            );

            // ── 4. Data migration: set FrameSizeId from old BodyTypeId ─────
            //
            // Old taxonomy mixed shape + frame. Map frame-like types to FrameSize:
            //   1 (Petite)    → FrameSize 1 (Petite)
            //   2 (Slim)      → FrameSize 2 (Average)
            //   3 (Athletic)  → FrameSize 2 (Average)
            //   8 (Plus Size) → FrameSize 4 (Plus)
            //   4-7           → NULL (pure shapes, no frame info)

            migrationBuilder.Sql(
                """
                UPDATE "BodyProfiles" SET "FrameSizeId" = CASE "BodyTypeId"
                    WHEN 1 THEN 1
                    WHEN 2 THEN 2
                    WHEN 3 THEN 2
                    WHEN 8 THEN 4
                    ELSE NULL
                END
                WHERE "BodyTypeId" IS NOT NULL;
                """
            );

            // ── 5. Data migration: copy scalar FitPreference → join table ──

            migrationBuilder.Sql(
                """
                INSERT INTO "BodyProfileFitPreferences" ("BodyProfileId", "FitPreference")
                SELECT "Id", "FitPreference"
                FROM "BodyProfiles"
                WHERE "FitPreference" IS NOT NULL;
                """
            );

            // ── 6. Data migration: remap BodyTypeId to new pure-shape IDs ──
            //
            // Must be a single UPDATE so CASE reads pre-update values:
            //   1 (Petite)     → 4 (Rectangle)
            //   2 (Slim)       → 4 (Rectangle)
            //   3 (Athletic)   → 5 (Inverted Triangle)
            //   4 (Hourglass)  → 1 (Hourglass)
            //   5 (Pear)       → 2 (Pear)
            //   6 (Apple)      → 3 (Apple)
            //   7 (Rectangle)  → 4 (Rectangle)
            //   8 (Plus Size)  → 4 (Rectangle)

            migrationBuilder.Sql(
                """
                UPDATE "BodyProfiles" SET "BodyTypeId" = CASE "BodyTypeId"
                    WHEN 1 THEN 4
                    WHEN 2 THEN 4
                    WHEN 3 THEN 5
                    WHEN 4 THEN 1
                    WHEN 5 THEN 2
                    WHEN 6 THEN 3
                    WHEN 7 THEN 4
                    WHEN 8 THEN 4
                    ELSE "BodyTypeId"
                END
                WHERE "BodyTypeId" IS NOT NULL;
                """
            );

            // ── 7. Now safe to delete unreferenced BodyType rows ───────────

            migrationBuilder.DeleteData(table: "BodyTypes", keyColumn: "Id", keyValue: 6);

            migrationBuilder.DeleteData(table: "BodyTypes", keyColumn: "Id", keyValue: 7);

            migrationBuilder.DeleteData(table: "BodyTypes", keyColumn: "Id", keyValue: 8);

            // ── 8. Update BodyType rows 1-5 with new pure-shape names ──────

            migrationBuilder.UpdateData(
                table: "BodyTypes",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "Description", "Name" },
                values: new object[] { "Balanced bust and hips with defined waist", "Hourglass" }
            );

            migrationBuilder.UpdateData(
                table: "BodyTypes",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "Description", "Name" },
                values: new object[] { "Hips wider than shoulders", "Pear" }
            );

            migrationBuilder.UpdateData(
                table: "BodyTypes",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "Description", "Name" },
                values: new object[] { "Fuller midsection with slimmer legs", "Apple" }
            );

            migrationBuilder.UpdateData(
                table: "BodyTypes",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "Description", "Name" },
                values: new object[] { "Balanced proportions, less waist definition", "Rectangle" }
            );

            migrationBuilder.UpdateData(
                table: "BodyTypes",
                keyColumn: "Id",
                keyValue: 5,
                columns: new[] { "Description", "Name" },
                values: new object[] { "Shoulders wider than hips", "Inverted Triangle" }
            );

            // ── 9. Create new indexes + FK ─────────────────────────────────

            migrationBuilder.CreateIndex(
                name: "IX_BodyProfiles_BodyType_FrameSize_Height_Weight",
                table: "BodyProfiles",
                columns: new[] { "BodyTypeId", "FrameSizeId", "HeightCm", "WeightKg" }
            );

            migrationBuilder.CreateIndex(
                name: "IX_BodyProfiles_FrameSizeId",
                table: "BodyProfiles",
                column: "FrameSizeId"
            );

            migrationBuilder.AddForeignKey(
                name: "FK_BodyProfiles_FrameSizes_FrameSizeId",
                table: "BodyProfiles",
                column: "FrameSizeId",
                principalTable: "FrameSizes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // ── 1. Drop FK + indexes ───────────────────────────────────────

            migrationBuilder.DropForeignKey(
                name: "FK_BodyProfiles_FrameSizes_FrameSizeId",
                table: "BodyProfiles"
            );

            migrationBuilder.DropIndex(
                name: "IX_BodyProfiles_BodyType_FrameSize_Height_Weight",
                table: "BodyProfiles"
            );

            migrationBuilder.DropIndex(name: "IX_BodyProfiles_FrameSizeId", table: "BodyProfiles");

            // ── 2. Data migration: reverse-remap BodyTypeId ────────────────
            //
            // Uses FrameSizeId to recover original type where possible:
            //   (4, FrameSize=1) → 1 (Petite)
            //   (4, FrameSize=2) → 2 (Slim)
            //   (5, any)         → 3 (Athletic)
            //   (1, any)         → 4 (Hourglass)
            //   (2, any)         → 5 (Pear)
            //   (3, any)         → 6 (Apple)
            //   (4, NULL)        → 7 (Rectangle)
            //   (4, FrameSize=4) → 8 (Plus Size)

            migrationBuilder.Sql(
                """
                UPDATE "BodyProfiles" SET "BodyTypeId" = CASE
                    WHEN "BodyTypeId" = 4 AND "FrameSizeId" = 1 THEN 1
                    WHEN "BodyTypeId" = 4 AND "FrameSizeId" = 2 THEN 2
                    WHEN "BodyTypeId" = 5 THEN 3
                    WHEN "BodyTypeId" = 1 THEN 4
                    WHEN "BodyTypeId" = 2 THEN 5
                    WHEN "BodyTypeId" = 3 THEN 6
                    WHEN "BodyTypeId" = 4 AND "FrameSizeId" IS NULL THEN 7
                    WHEN "BodyTypeId" = 4 AND "FrameSizeId" = 4 THEN 8
                    ELSE "BodyTypeId"
                END
                WHERE "BodyTypeId" IS NOT NULL;
                """
            );

            // ── 3. Data migration: copy first fit preference back to scalar ─

            migrationBuilder.Sql(
                """
                UPDATE "BodyProfiles" bp SET "FitPreference" = sub."FitPreference"
                FROM (
                    SELECT DISTINCT ON ("BodyProfileId") "BodyProfileId", "FitPreference"
                    FROM "BodyProfileFitPreferences"
                    ORDER BY "BodyProfileId", "FitPreference"
                ) sub
                WHERE bp."Id" = sub."BodyProfileId";
                """
            );

            // ── 4. Drop new tables ─────────────────────────────────────────

            migrationBuilder.DropTable(name: "BodyProfileFitPreferences");

            migrationBuilder.DropTable(name: "FrameSizes");

            // ── 5. Drop FrameSizeId column ─────────────────────────────────

            migrationBuilder.DropColumn(name: "FrameSizeId", table: "BodyProfiles");

            // ── 6. Restore old BodyType names ──────────────────────────────

            migrationBuilder.UpdateData(
                table: "BodyTypes",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "Description", "Name" },
                values: new object[] { "Shorter stature with proportional frame", "Petite" }
            );

            migrationBuilder.UpdateData(
                table: "BodyTypes",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "Description", "Name" },
                values: new object[] { "Lean build with narrow shoulders and hips", "Slim" }
            );

            migrationBuilder.UpdateData(
                table: "BodyTypes",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "Description", "Name" },
                values: new object[] { "Muscular build with broader shoulders", "Athletic" }
            );

            migrationBuilder.UpdateData(
                table: "BodyTypes",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "Description", "Name" },
                values: new object[] { "Balanced bust and hips with defined waist", "Hourglass" }
            );

            migrationBuilder.UpdateData(
                table: "BodyTypes",
                keyColumn: "Id",
                keyValue: 5,
                columns: new[] { "Description", "Name" },
                values: new object[] { "Hips wider than shoulders", "Pear" }
            );

            // ── 7. Re-insert deleted BodyType rows ─────────────────────────

            migrationBuilder.InsertData(
                table: "BodyTypes",
                columns: new[] { "Id", "Description", "DisplayOrder", "Name" },
                values: new object[,]
                {
                    { 6, "Fuller midsection with slimmer legs", 6, "Apple" },
                    { 7, "Balanced proportions throughout", 7, "Rectangle" },
                    { 8, "Fuller figure across all areas", 8, "Plus Size" },
                }
            );

            // ── 8. Restore old index ───────────────────────────────────────

            migrationBuilder.CreateIndex(
                name: "IX_BodyProfiles_BodyTypeId_HeightCm_WeightKg",
                table: "BodyProfiles",
                columns: new[] { "BodyTypeId", "HeightCm", "WeightKg" }
            );
        }
    }
}
