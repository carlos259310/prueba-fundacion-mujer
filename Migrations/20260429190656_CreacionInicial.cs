using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace ProductCatalog.Api.Migrations
{
    /// <inheritdoc />
    public partial class CreacionInicial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "bodegas",
                columns: table => new
                {
                    bod_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    bod_nombre = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    bod_principal = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_bodegas", x => x.bod_id);
                });

            migrationBuilder.CreateTable(
                name: "productos",
                columns: table => new
                {
                    prod_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    prod_nombre = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    prod_codigo = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    prod_descripcion = table.Column<string>(type: "text", nullable: true),
                    prod_marca = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_productos", x => x.prod_id);
                });

            migrationBuilder.CreateTable(
                name: "inventario",
                columns: table => new
                {
                    prod_id = table.Column<int>(type: "integer", nullable: false),
                    bod_id = table.Column<int>(type: "integer", nullable: false),
                    inv_stock = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    inv_last_update = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_inventario", x => new { x.prod_id, x.bod_id });
                    table.ForeignKey(
                        name: "FK_inventario_bodegas_bod_id",
                        column: x => x.bod_id,
                        principalTable: "bodegas",
                        principalColumn: "bod_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_inventario_productos_prod_id",
                        column: x => x.prod_id,
                        principalTable: "productos",
                        principalColumn: "prod_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "movimientos",
                columns: table => new
                {
                    mov_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    prod_id = table.Column<int>(type: "integer", nullable: false),
                    mov_bodega_inicial = table.Column<int>(type: "integer", nullable: true),
                    mov_bodega_final = table.Column<int>(type: "integer", nullable: true),
                    mov_cantidad = table.Column<int>(type: "integer", nullable: false),
                    mov_tipo = table.Column<string>(type: "text", nullable: false),
                    mov_concepto = table.Column<string>(type: "text", nullable: false),
                    mov_fecha = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_movimientos", x => x.mov_id);
                    table.ForeignKey(
                        name: "FK_movimientos_productos_prod_id",
                        column: x => x.prod_id,
                        principalTable: "productos",
                        principalColumn: "prod_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_inventario_bod_id",
                table: "inventario",
                column: "bod_id");

            migrationBuilder.CreateIndex(
                name: "IX_movimientos_prod_id",
                table: "movimientos",
                column: "prod_id");

            migrationBuilder.CreateIndex(
                name: "IX_productos_prod_codigo",
                table: "productos",
                column: "prod_codigo",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "inventario");

            migrationBuilder.DropTable(
                name: "movimientos");

            migrationBuilder.DropTable(
                name: "bodegas");

            migrationBuilder.DropTable(
                name: "productos");
        }
    }
}
