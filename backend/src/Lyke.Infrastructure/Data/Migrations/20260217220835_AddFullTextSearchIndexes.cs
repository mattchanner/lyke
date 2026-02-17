using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Lyke.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddFullTextSearchIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                CREATE INDEX "IX_Posts_FullTextSearch" ON "Posts"
                USING GIN (to_tsvector('english', coalesce("Title",'') || ' ' || coalesce("Description",'')));
                """);

            migrationBuilder.Sql("""
                CREATE INDEX "IX_Products_FullTextSearch" ON "Products"
                USING GIN (to_tsvector('english', coalesce("Name",'') || ' ' || coalesce("Description",'') || ' ' || coalesce("ExternalSku",'')));
                """);

            migrationBuilder.Sql("""
                CREATE INDEX "IX_Creators_FullTextSearch" ON "Creators"
                USING GIN (to_tsvector('english', coalesce("DisplayName",'') || ' ' || coalesce("Bio",'')));
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""DROP INDEX IF EXISTS "IX_Posts_FullTextSearch";""");
            migrationBuilder.Sql("""DROP INDEX IF EXISTS "IX_Products_FullTextSearch";""");
            migrationBuilder.Sql("""DROP INDEX IF EXISTS "IX_Creators_FullTextSearch";""");
        }
    }
}
