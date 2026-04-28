using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Lyke.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddAuthorUserIdToPost : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Step 1: Drop existing FK so we can alter CreatorId nullability
            migrationBuilder.DropForeignKey(
                name: "FK_Posts_Creators_CreatorId",
                table: "Posts");

            // Step 2: Add AuthorUserId as NULLABLE first (for backfill)
            migrationBuilder.AddColumn<Guid>(
                name: "AuthorUserId",
                table: "Posts",
                type: "uuid",
                nullable: true);

            // Step 3: Backfill AuthorUserId from Creator.UserId for all existing posts
            migrationBuilder.Sql(
                """
                UPDATE "Posts"
                SET "AuthorUserId" = c."UserId"
                FROM "Creators" c
                WHERE "Posts"."CreatorId" = c."Id"
                """);

            // Step 4: Make AuthorUserId NOT NULL now that all rows are populated
            migrationBuilder.AlterColumn<Guid>(
                name: "AuthorUserId",
                table: "Posts",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            // Step 5: Make CreatorId nullable
            migrationBuilder.AlterColumn<Guid>(
                name: "CreatorId",
                table: "Posts",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            // Step 6: Add DisplayName to Users
            migrationBuilder.AddColumn<string>(
                name: "DisplayName",
                table: "AspNetUsers",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            // Step 7: Add indexes and foreign keys
            migrationBuilder.CreateIndex(
                name: "IX_Posts_AuthorUserId_Status_PublishedAt",
                table: "Posts",
                columns: new[] { "AuthorUserId", "Status", "PublishedAt" });

            migrationBuilder.AddForeignKey(
                name: "FK_Posts_AspNetUsers_AuthorUserId",
                table: "Posts",
                column: "AuthorUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Posts_Creators_CreatorId",
                table: "Posts",
                column: "CreatorId",
                principalTable: "Creators",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Posts_AspNetUsers_AuthorUserId",
                table: "Posts");

            migrationBuilder.DropForeignKey(
                name: "FK_Posts_Creators_CreatorId",
                table: "Posts");

            migrationBuilder.DropIndex(
                name: "IX_Posts_AuthorUserId_Status_PublishedAt",
                table: "Posts");

            migrationBuilder.DropColumn(
                name: "AuthorUserId",
                table: "Posts");

            migrationBuilder.DropColumn(
                name: "DisplayName",
                table: "AspNetUsers");

            migrationBuilder.AlterColumn<Guid>(
                name: "CreatorId",
                table: "Posts",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Posts_Creators_CreatorId",
                table: "Posts",
                column: "CreatorId",
                principalTable: "Creators",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
