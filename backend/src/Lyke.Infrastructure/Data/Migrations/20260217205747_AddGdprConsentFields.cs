using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Lyke.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddGdprConsentFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "MarketingOptIn",
                table: "AspNetUsers",
                type: "boolean",
                nullable: false,
                defaultValue: false
            );

            migrationBuilder.AddColumn<DateTime>(
                name: "PrivacyPolicyAcceptedAt",
                table: "AspNetUsers",
                type: "timestamp with time zone",
                nullable: true
            );

            migrationBuilder.AddColumn<string>(
                name: "PrivacyPolicyVersion",
                table: "AspNetUsers",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(name: "MarketingOptIn", table: "AspNetUsers");

            migrationBuilder.DropColumn(name: "PrivacyPolicyAcceptedAt", table: "AspNetUsers");

            migrationBuilder.DropColumn(name: "PrivacyPolicyVersion", table: "AspNetUsers");
        }
    }
}
