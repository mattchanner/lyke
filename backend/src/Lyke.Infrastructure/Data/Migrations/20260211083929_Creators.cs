using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Lyke.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class Creators : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ContactEmail",
                table: "Retailers",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true
            );

            migrationBuilder.AddColumn<Guid>(
                name: "UserId",
                table: "Retailers",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000")
            );

            migrationBuilder.CreateIndex(
                name: "IX_Retailers_UserId",
                table: "Retailers",
                column: "UserId",
                unique: true
            );

            migrationBuilder.AddForeignKey(
                name: "FK_Retailers_AspNetUsers_UserId",
                table: "Retailers",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Retailers_AspNetUsers_UserId",
                table: "Retailers"
            );

            migrationBuilder.DropIndex(name: "IX_Retailers_UserId", table: "Retailers");

            migrationBuilder.DropColumn(name: "ContactEmail", table: "Retailers");

            migrationBuilder.DropColumn(name: "UserId", table: "Retailers");
        }
    }
}
