using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Sisloc.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Motoristas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NomeCompleto = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    NumeroCnh = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    VencimentoCnh = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CategoriaCnh = table.Column<string>(type: "nvarchar(5)", maxLength: 5, nullable: false),
                    Telefone = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: false),
                    DataExameToxicologico = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    Observacoes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Motoristas", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Veiculos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Placa = table.Column<string>(type: "nvarchar(8)", maxLength: 8, nullable: false),
                    Modelo = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Categoria = table.Column<int>(type: "int", nullable: false),
                    CapacidadePassageiros = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    Observacoes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Veiculos", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Agendamentos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Protocolo = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    DataPartida = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DataChegada = table.Column<DateTime>(type: "datetime2", nullable: false),
                    NomeSolicitante = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    QuantidadePessoas = table.Column<int>(type: "int", nullable: false),
                    Destino = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Descricao = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CategoriaVeiculo = table.Column<int>(type: "int", nullable: false),
                    PrecisaMotorista = table.Column<bool>(type: "bit", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    DataCriacao = table.Column<DateTime>(type: "datetime2", nullable: false),
                    VeiculoAlocadoId = table.Column<int>(type: "int", nullable: true),
                    MotoristaAlocadoId = table.Column<int>(type: "int", nullable: true),
                    ObservacoesAdmin = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Agendamentos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Agendamentos_Motoristas_MotoristaAlocadoId",
                        column: x => x.MotoristaAlocadoId,
                        principalTable: "Motoristas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Agendamentos_Veiculos_VeiculoAlocadoId",
                        column: x => x.VeiculoAlocadoId,
                        principalTable: "Veiculos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.InsertData(
                table: "Motoristas",
                columns: new[] { "Id", "CategoriaCnh", "DataExameToxicologico", "NomeCompleto", "NumeroCnh", "Observacoes", "Status", "Telefone", "VencimentoCnh" },
                values: new object[,]
                {
                    { 1, "B", new DateTime(2024, 6, 15, 0, 0, 0, 0, DateTimeKind.Unspecified), "João Silva Santos", "12345678901", null, 1, "(11) 99999-9999", new DateTime(2026, 12, 31, 0, 0, 0, 0, DateTimeKind.Unspecified) },
                    { 2, "C", new DateTime(2024, 9, 15, 0, 0, 0, 0, DateTimeKind.Unspecified), "Maria Oliveira Costa", "98765432109", null, 1, "(11) 88888-8888", new DateTime(2025, 12, 31, 0, 0, 0, 0, DateTimeKind.Unspecified) }
                });

            migrationBuilder.InsertData(
                table: "Veiculos",
                columns: new[] { "Id", "CapacidadePassageiros", "Categoria", "Modelo", "Observacoes", "Placa", "Status" },
                values: new object[,]
                {
                    { 1, 5, 1, "Gol 1.0", null, "ABC-1234", 1 },
                    { 2, 5, 2, "Corolla XEI", null, "DEF-5678", 1 },
                    { 3, 5, 4, "Hilux SR", null, "GHI-9012", 1 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Agendamentos_DataPartida",
                table: "Agendamentos",
                column: "DataPartida");

            migrationBuilder.CreateIndex(
                name: "IX_Agendamentos_MotoristaAlocadoId",
                table: "Agendamentos",
                column: "MotoristaAlocadoId");

            migrationBuilder.CreateIndex(
                name: "IX_Agendamentos_Protocolo",
                table: "Agendamentos",
                column: "Protocolo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Agendamentos_Status",
                table: "Agendamentos",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_Agendamentos_VeiculoAlocadoId",
                table: "Agendamentos",
                column: "VeiculoAlocadoId");

            migrationBuilder.CreateIndex(
                name: "IX_Motoristas_NumeroCnh",
                table: "Motoristas",
                column: "NumeroCnh",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Motoristas_Status",
                table: "Motoristas",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_Veiculos_Categoria",
                table: "Veiculos",
                column: "Categoria");

            migrationBuilder.CreateIndex(
                name: "IX_Veiculos_Placa",
                table: "Veiculos",
                column: "Placa",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Veiculos_Status",
                table: "Veiculos",
                column: "Status");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Agendamentos");

            migrationBuilder.DropTable(
                name: "Motoristas");

            migrationBuilder.DropTable(
                name: "Veiculos");
        }
    }
}
