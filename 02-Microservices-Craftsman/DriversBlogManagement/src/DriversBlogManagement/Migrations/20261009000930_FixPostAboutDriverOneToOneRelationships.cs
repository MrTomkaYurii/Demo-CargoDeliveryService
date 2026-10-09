using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DriversBlogManagement.Migrations
{
    /// <inheritdoc />
    public partial class FixPostAboutDriverOneToOneRelationships : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_blog_users_post_about_drivers_post_about_driver_id",
                table: "blog_users");

            migrationBuilder.DropForeignKey(
                name: "fk_drivers_post_about_drivers_post_about_driver_id",
                table: "drivers");

            migrationBuilder.AddColumn<Guid>(
                name: "blog_user_id",
                table: "post_about_drivers",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "driver_id",
                table: "post_about_drivers",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "ix_post_about_drivers_blog_user_id",
                table: "post_about_drivers",
                column: "blog_user_id",
                unique: true,
                filter: "[blog_user_id] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "ix_post_about_drivers_driver_id",
                table: "post_about_drivers",
                column: "driver_id",
                unique: true,
                filter: "[driver_id] IS NOT NULL");

            migrationBuilder.AddForeignKey(
                name: "fk_post_about_drivers_blog_users_blog_user_id",
                table: "post_about_drivers",
                column: "blog_user_id",
                principalTable: "blog_users",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "fk_post_about_drivers_drivers_driver_id",
                table: "post_about_drivers",
                column: "driver_id",
                principalTable: "drivers",
                principalColumn: "id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_post_about_drivers_blog_users_blog_user_id",
                table: "post_about_drivers");

            migrationBuilder.DropForeignKey(
                name: "fk_post_about_drivers_drivers_driver_id",
                table: "post_about_drivers");

            migrationBuilder.DropIndex(
                name: "ix_post_about_drivers_blog_user_id",
                table: "post_about_drivers");

            migrationBuilder.DropIndex(
                name: "ix_post_about_drivers_driver_id",
                table: "post_about_drivers");

            migrationBuilder.DropColumn(
                name: "blog_user_id",
                table: "post_about_drivers");

            migrationBuilder.DropColumn(
                name: "driver_id",
                table: "post_about_drivers");

            migrationBuilder.AddForeignKey(
                name: "fk_blog_users_post_about_drivers_post_about_driver_id",
                table: "blog_users",
                column: "id",
                principalTable: "post_about_drivers",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_drivers_post_about_drivers_post_about_driver_id",
                table: "drivers",
                column: "id",
                principalTable: "post_about_drivers",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
