using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Tlaoami.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddDocenteTitularToGrupos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "DocenteTitularId",
                table: "Grupos",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Grupos_DocenteTitularId",
                table: "Grupos",
                column: "DocenteTitularId");

            migrationBuilder.AddForeignKey(
                name: "FK_Grupos_Users_DocenteTitularId",
                table: "Grupos",
                column: "DocenteTitularId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Grupos_Users_DocenteTitularId",
                table: "Grupos");

            migrationBuilder.DropIndex(
                name: "IX_Grupos_DocenteTitularId",
                table: "Grupos");

            migrationBuilder.DropColumn(
                name: "DocenteTitularId",
                table: "Grupos");
        }
    }
}
