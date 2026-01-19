using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Tlaoami.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddAlumnosGruposCiclos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Email",
                table: "Alumnos",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT");

            migrationBuilder.AddColumn<bool>(
                name: "Activo",
                table: "Alumnos",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "Matricula",
                table: "Alumnos",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Telefono",
                table: "Alumnos",
                type: "TEXT",
                nullable: true);

            // Backfill matrículas únicas para registros existentes antes de crear el índice único
            migrationBuilder.Sql("UPDATE Alumnos SET Matricula = 'LEG-' || substr(Id, 1, 8) WHERE Matricula IS NULL OR Matricula = '';");

            migrationBuilder.CreateTable(
                name: "CiclosEscolares",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Nombre = table.Column<string>(type: "TEXT", nullable: true),
                    FechaInicio = table.Column<DateTime>(type: "TEXT", nullable: false),
                    FechaFin = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Activo = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CiclosEscolares", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Grupos",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Nombre = table.Column<string>(type: "TEXT", nullable: true),
                    Grado = table.Column<int>(type: "INTEGER", nullable: false),
                    Turno = table.Column<string>(type: "TEXT", nullable: true),
                    CicloEscolarId = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Grupos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Grupos_CiclosEscolares_CicloEscolarId",
                        column: x => x.CicloEscolarId,
                        principalTable: "CiclosEscolares",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AsignacionesGrupo",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    AlumnoId = table.Column<Guid>(type: "TEXT", nullable: false),
                    GrupoId = table.Column<Guid>(type: "TEXT", nullable: false),
                    FechaInicio = table.Column<DateTime>(type: "TEXT", nullable: false),
                    FechaFin = table.Column<DateTime>(type: "TEXT", nullable: true),
                    Activo = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AsignacionesGrupo", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AsignacionesGrupo_Alumnos_AlumnoId",
                        column: x => x.AlumnoId,
                        principalTable: "Alumnos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AsignacionesGrupo_Grupos_GrupoId",
                        column: x => x.GrupoId,
                        principalTable: "Grupos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Alumnos_Matricula",
                table: "Alumnos",
                column: "Matricula",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AlumnoGrupo_AlumnoId_Activo",
                table: "AsignacionesGrupo",
                columns: new[] { "AlumnoId", "Activo" });

            migrationBuilder.CreateIndex(
                name: "IX_AlumnoGrupo_GrupoId_Activo",
                table: "AsignacionesGrupo",
                columns: new[] { "GrupoId", "Activo" });

            migrationBuilder.CreateIndex(
                name: "IX_Grupos_CicloEscolarId",
                table: "Grupos",
                column: "CicloEscolarId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AsignacionesGrupo");

            migrationBuilder.DropTable(
                name: "Grupos");

            migrationBuilder.DropTable(
                name: "CiclosEscolares");

            migrationBuilder.DropIndex(
                name: "IX_Alumnos_Matricula",
                table: "Alumnos");

            migrationBuilder.DropColumn(
                name: "Activo",
                table: "Alumnos");

            migrationBuilder.DropColumn(
                name: "Matricula",
                table: "Alumnos");

            migrationBuilder.DropColumn(
                name: "Telefono",
                table: "Alumnos");

            migrationBuilder.AlterColumn<string>(
                name: "Email",
                table: "Alumnos",
                type: "TEXT",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldNullable: true);
        }
    }
}
