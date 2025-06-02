using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Backend.Migrations
{
    /// <inheritdoc />
    public partial class Inicial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AlgoritmosCompresion",
                columns: table => new
                {
                    IdAlgoritmoCompresion = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NombreAlgoritmo = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AlgoritmosCompresion", x => x.IdAlgoritmoCompresion);
                });

            migrationBuilder.CreateTable(
                name: "Imagenes",
                columns: table => new
                {
                    IdImagen = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    DatosImagen = table.Column<byte[]>(type: "varbinary(max)", nullable: false),
                    AnchoOriginal = table.Column<int>(type: "int", nullable: false),
                    AltoOriginal = table.Column<int>(type: "int", nullable: false),
                    FechaCarga = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Imagenes", x => x.IdImagen);
                });

            migrationBuilder.CreateTable(
                name: "ImagenesProcesadas",
                columns: table => new
                {
                    IdImagenProcesada = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdImagenOriginal = table.Column<int>(type: "int", nullable: false),
                    AnchoResolucion = table.Column<int>(type: "int", nullable: false),
                    AltoResolucion = table.Column<int>(type: "int", nullable: false),
                    ProfundidadBits = table.Column<byte>(type: "tinyint", nullable: false),
                    IdAlgoritmoCompresion = table.Column<int>(type: "int", nullable: true),
                    RatioCompresion = table.Column<float>(type: "real", nullable: true),
                    DatosProcesados = table.Column<byte[]>(type: "varbinary(max)", nullable: false),
                    FechaProcesamiento = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ImagenesProcesadas", x => x.IdImagenProcesada);
                    table.CheckConstraint("CHK_ProfundidadBits", "[ProfundidadBits] IN (1,8,24)");
                    table.ForeignKey(
                        name: "FK_ImagenesProcesadas_AlgoritmosCompresion_IdAlgoritmoCompresion",
                        column: x => x.IdAlgoritmoCompresion,
                        principalTable: "AlgoritmosCompresion",
                        principalColumn: "IdAlgoritmoCompresion");
                    table.ForeignKey(
                        name: "FK_ImagenesProcesadas_Imagenes_IdImagenOriginal",
                        column: x => x.IdImagenOriginal,
                        principalTable: "Imagenes",
                        principalColumn: "IdImagen",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Comparaciones",
                columns: table => new
                {
                    IdComparacion = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdImagenOriginal = table.Column<int>(type: "int", nullable: false),
                    IdImagenProcesada = table.Column<int>(type: "int", nullable: false),
                    MSE = table.Column<float>(type: "real", nullable: true),
                    PSNR = table.Column<float>(type: "real", nullable: true),
                    ImagenDiferencias = table.Column<byte[]>(type: "varbinary(max)", nullable: false),
                    FechaComparacion = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Comparaciones", x => x.IdComparacion);
                    table.ForeignKey(
                        name: "FK_Comparaciones_ImagenesProcesadas_IdImagenProcesada",
                        column: x => x.IdImagenProcesada,
                        principalTable: "ImagenesProcesadas",
                        principalColumn: "IdImagenProcesada",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Comparaciones_Imagenes_IdImagenOriginal",
                        column: x => x.IdImagenOriginal,
                        principalTable: "Imagenes",
                        principalColumn: "IdImagen");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Comparaciones_IdImagenOriginal",
                table: "Comparaciones",
                column: "IdImagenOriginal");

            migrationBuilder.CreateIndex(
                name: "IX_Comparaciones_IdImagenProcesada",
                table: "Comparaciones",
                column: "IdImagenProcesada");

            migrationBuilder.CreateIndex(
                name: "IX_ImagenesProcesadas_IdAlgoritmoCompresion",
                table: "ImagenesProcesadas",
                column: "IdAlgoritmoCompresion");

            migrationBuilder.CreateIndex(
                name: "IX_ImagenesProcesadas_IdImagenOriginal",
                table: "ImagenesProcesadas",
                column: "IdImagenOriginal");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Comparaciones");

            migrationBuilder.DropTable(
                name: "ImagenesProcesadas");

            migrationBuilder.DropTable(
                name: "AlgoritmosCompresion");

            migrationBuilder.DropTable(
                name: "Imagenes");
        }
    }
}
