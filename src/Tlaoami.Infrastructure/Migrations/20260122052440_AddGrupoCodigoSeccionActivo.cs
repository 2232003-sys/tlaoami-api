using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Tlaoami.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddGrupoCodigoSeccionActivo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "Activo",
                table: "Grupos",
                type: "boolean",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<string>(
                name: "Seccion",
                table: "Grupos",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Codigo",
                table: "Grupos",
                type: "text",
                nullable: true);

            // Populate Codigo with unique values for existing rows
            migrationBuilder.Sql(@"
                UPDATE ""Grupos"" 
                SET ""Codigo"" = 'GRUPO-' || ""Id""::text 
                WHERE ""Codigo"" IS NULL;
            ");

            // Now make Codigo NOT NULL and create unique index
            migrationBuilder.AlterColumn<string>(
                name: "Codigo",
                table: "Grupos",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Grupos_Codigo",
                table: "Grupos",
                column: "Codigo",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Grupos_Codigo",
                table: "Grupos");

            migrationBuilder.DropColumn(
                name: "Activo",
                table: "Grupos");

            migrationBuilder.DropColumn(
                name: "Codigo",
                table: "Grupos");

            migrationBuilder.DropColumn(
                name: "Seccion",
                table: "Grupos");
        }
    }
}
