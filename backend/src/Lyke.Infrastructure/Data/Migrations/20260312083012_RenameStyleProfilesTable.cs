using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Lyke.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class RenameStyleProfilesTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_style_profiles_AspNetUsers_UserId",
                table: "style_profiles"
            );

            migrationBuilder.DropPrimaryKey(name: "PK_style_profiles", table: "style_profiles");

            migrationBuilder.RenameTable(name: "style_profiles", newName: "StyleProfiles");

            migrationBuilder.RenameIndex(
                name: "IX_style_profiles_UserId",
                table: "StyleProfiles",
                newName: "IX_StyleProfiles_UserId"
            );

            migrationBuilder.AddPrimaryKey(
                name: "PK_StyleProfiles",
                table: "StyleProfiles",
                column: "Id"
            );

            migrationBuilder.AddForeignKey(
                name: "FK_StyleProfiles_AspNetUsers_UserId",
                table: "StyleProfiles",
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
                name: "FK_StyleProfiles_AspNetUsers_UserId",
                table: "StyleProfiles"
            );

            migrationBuilder.DropPrimaryKey(name: "PK_StyleProfiles", table: "StyleProfiles");

            migrationBuilder.RenameTable(name: "StyleProfiles", newName: "style_profiles");

            migrationBuilder.RenameIndex(
                name: "IX_StyleProfiles_UserId",
                table: "style_profiles",
                newName: "IX_style_profiles_UserId"
            );

            migrationBuilder.AddPrimaryKey(
                name: "PK_style_profiles",
                table: "style_profiles",
                column: "Id"
            );

            migrationBuilder.AddForeignKey(
                name: "FK_style_profiles_AspNetUsers_UserId",
                table: "style_profiles",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade
            );
        }
    }
}
