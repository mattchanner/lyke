using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Lyke.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddCreatorVerificationFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "VerificationDocumentUrls",
                table: "Creators",
                type: "jsonb",
                nullable: true
            );

            migrationBuilder.AddColumn<string>(
                name: "VerificationNotes",
                table: "Creators",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true
            );

            migrationBuilder.AddColumn<string>(
                name: "VerificationRejectionReason",
                table: "Creators",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true
            );

            migrationBuilder.AddColumn<DateTime>(
                name: "VerificationRequestedAt",
                table: "Creators",
                type: "timestamp with time zone",
                nullable: true
            );

            migrationBuilder.AddColumn<DateTime>(
                name: "VerificationReviewedAt",
                table: "Creators",
                type: "timestamp with time zone",
                nullable: true
            );

            migrationBuilder.AddColumn<Guid>(
                name: "VerificationReviewedByUserId",
                table: "Creators",
                type: "uuid",
                nullable: true
            );

            migrationBuilder.AddColumn<int>(
                name: "VerificationStatus",
                table: "Creators",
                type: "integer",
                nullable: false,
                defaultValue: 0
            );

            migrationBuilder.CreateIndex(
                name: "IX_Creators_VerificationReviewedByUserId",
                table: "Creators",
                column: "VerificationReviewedByUserId"
            );

            migrationBuilder.CreateIndex(
                name: "IX_Creators_VerificationStatus",
                table: "Creators",
                column: "VerificationStatus"
            );

            migrationBuilder.AddForeignKey(
                name: "FK_Creators_AspNetUsers_VerificationReviewedByUserId",
                table: "Creators",
                column: "VerificationReviewedByUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Creators_AspNetUsers_VerificationReviewedByUserId",
                table: "Creators"
            );

            migrationBuilder.DropIndex(
                name: "IX_Creators_VerificationReviewedByUserId",
                table: "Creators"
            );

            migrationBuilder.DropIndex(name: "IX_Creators_VerificationStatus", table: "Creators");

            migrationBuilder.DropColumn(name: "VerificationDocumentUrls", table: "Creators");

            migrationBuilder.DropColumn(name: "VerificationNotes", table: "Creators");

            migrationBuilder.DropColumn(name: "VerificationRejectionReason", table: "Creators");

            migrationBuilder.DropColumn(name: "VerificationRequestedAt", table: "Creators");

            migrationBuilder.DropColumn(name: "VerificationReviewedAt", table: "Creators");

            migrationBuilder.DropColumn(name: "VerificationReviewedByUserId", table: "Creators");

            migrationBuilder.DropColumn(name: "VerificationStatus", table: "Creators");
        }
    }
}
