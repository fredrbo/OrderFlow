using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OrderFlow.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddOrderStatusHistory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "order_status_history",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    order_id = table.Column<Guid>(type: "uuid", nullable: false),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    data_alteracao = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_order_status_history", x => x.id);
                    table.ForeignKey(
                        name: "FK_order_status_history_orders_order_id",
                        column: x => x.order_id,
                        principalTable: "orders",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_order_status_history_order_id_data_alteracao",
                table: "order_status_history",
                columns: new[] { "order_id", "data_alteracao" });

            migrationBuilder.Sql("""
                INSERT INTO order_status_history (id, order_id, status, data_alteracao)
                SELECT gen_random_uuid(), id, 'Pendente', data_criacao FROM orders;

                INSERT INTO order_status_history (id, order_id, status, data_alteracao)
                SELECT gen_random_uuid(), id, 'Finalizado', data_finalizacao FROM orders
                WHERE data_finalizacao IS NOT NULL;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "order_status_history");
        }
    }
}
