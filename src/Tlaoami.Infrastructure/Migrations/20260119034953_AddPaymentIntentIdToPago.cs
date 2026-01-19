using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Tlaoami.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPaymentIntentIdToPago : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "PaymentIntentId",
                table: "Pagos",
                type: "TEXT",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Pagos_PaymentIntentId",
                table: "Pagos",
                column: "PaymentIntentId",
                unique: true,
                filter: "[PaymentIntentId] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Pagos_PaymentIntentId",
                table: "Pagos");

            migrationBuilder.DropColumn(
                name: "PaymentIntentId",
                table: "Pagos");
        }
    }
}
