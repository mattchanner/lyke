using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Lyke.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddPostThumbnailUrls : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ThumbnailUrls",
                table: "Posts",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ThumbnailUrls",
                table: "Posts");
        }
    }
}
