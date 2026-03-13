using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Inventory.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "inventory_sku",
                columns: table => new
                {
                    sku = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    available = table.Column<long>(type: "bigint", nullable: false),
                    reserved = table.Column<long>(type: "bigint", nullable: false),
                    version = table.Column<long>(type: "bigint", nullable: false, defaultValue: 0L),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_inventory_sku", x => x.sku);
                    table.CheckConstraint("ck_available_nonneg", "available >= 0");
                    table.CheckConstraint("ck_reserved_nonneg", "reserved >= 0");
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "inventory_sku");
        }
    }
}
