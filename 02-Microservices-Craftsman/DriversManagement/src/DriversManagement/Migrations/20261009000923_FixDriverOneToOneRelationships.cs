using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DriversManagement.Migrations
{
    /// <inheritdoc />
    public partial class FixDriverOneToOneRelationships : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_drivers_companies_company_id",
                table: "drivers");

            migrationBuilder.DropForeignKey(
                name: "fk_drivers_photos_photo_id",
                table: "drivers");

            migrationBuilder.AddColumn<Guid>(
                name: "driver_id",
                table: "photos",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "driver_id",
                table: "companies",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "ix_photos_driver_id",
                table: "photos",
                column: "driver_id",
                unique: true,
                filter: "[driver_id] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "ix_companies_driver_id",
                table: "companies",
                column: "driver_id",
                unique: true,
                filter: "[driver_id] IS NOT NULL");

            migrationBuilder.AddForeignKey(
                name: "fk_companies_drivers_driver_id",
                table: "companies",
                column: "driver_id",
                principalTable: "drivers",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "fk_photos_drivers_driver_id",
                table: "photos",
                column: "driver_id",
                principalTable: "drivers",
                principalColumn: "id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_companies_drivers_driver_id",
                table: "companies");

            migrationBuilder.DropForeignKey(
                name: "fk_photos_drivers_driver_id",
                table: "photos");

            migrationBuilder.DropIndex(
                name: "ix_photos_driver_id",
                table: "photos");

            migrationBuilder.DropIndex(
                name: "ix_companies_driver_id",
                table: "companies");

            migrationBuilder.DropColumn(
                name: "driver_id",
                table: "photos");

            migrationBuilder.DropColumn(
                name: "driver_id",
                table: "companies");

            migrationBuilder.AddForeignKey(
                name: "fk_drivers_companies_company_id",
                table: "drivers",
                column: "id",
                principalTable: "companies",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_drivers_photos_photo_id",
                table: "drivers",
                column: "id",
                principalTable: "photos",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
