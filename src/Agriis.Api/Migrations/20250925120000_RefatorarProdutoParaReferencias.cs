using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Agriis.Api.Migrations
{
    /// <inheritdoc />
    public partial class RefatorarProdutoParaReferencias : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 1. Adicionar novas colunas de foreign keys
            migrationBuilder.AddColumn<int>(
                name: "UnidadeMedidaId",
                schema: "public",
                table: "Produto",
                type: "integer",
                nullable: false,
                defaultValue: 1); // Assumindo que existe uma unidade padrão

            migrationBuilder.AddColumn<int>(
                name: "EmbalagemId",
                schema: "public",
                table: "Produto",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "AtividadeAgropecuariaId",
                schema: "public",
                table: "Produto",
                type: "integer",
                nullable: true);

            // 2. Adicionar campos de dimensões (objeto valor)
            migrationBuilder.AddColumn<decimal>(
                name: "Altura",
                schema: "public",
                table: "Produto",
                type: "numeric(10,2)",
                precision: 10,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "Largura",
                schema: "public",
                table: "Produto",
                type: "numeric(10,2)",
                precision: 10,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "Comprimento",
                schema: "public",
                table: "Produto",
                type: "numeric(10,2)",
                precision: 10,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "PesoNominal",
                schema: "public",
                table: "Produto",
                type: "numeric(10,3)",
                precision: 10,
                scale: 3,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "PesoEmbalagem",
                schema: "public",
                table: "Produto",
                type: "numeric(10,3)",
                precision: 10,
                scale: 3,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "Pms",
                schema: "public",
                table: "Produto",
                type: "numeric(10,3)",
                precision: 10,
                scale: 3,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "QuantidadeMinima",
                schema: "public",
                table: "Produto",
                type: "numeric(10,3)",
                precision: 10,
                scale: 3,
                nullable: false,
                defaultValue: 1m);

            migrationBuilder.AddColumn<string>(
                name: "Embalagem",
                schema: "public",
                table: "Produto",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "Unidade");

            migrationBuilder.AddColumn<decimal>(
                name: "FaixaDensidadeInicial",
                schema: "public",
                table: "Produto",
                type: "numeric(10,3)",
                precision: 10,
                scale: 3,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "FaixaDensidadeFinal",
                schema: "public",
                table: "Produto",
                type: "numeric(10,3)",
                precision: 10,
                scale: 3,
                nullable: true);

            // 3. Migração de dados - mapear valores de string para IDs de referência
            // Mapear campo "Unidade" para UnidadeMedidaId
            migrationBuilder.Sql(@"
                UPDATE ""public"".""Produto"" 
                SET ""UnidadeMedidaId"" = CASE 
                    WHEN ""Unidade"" = 0 THEN (SELECT ""Id"" FROM ""public"".""UnidadeMedida"" WHERE ""Simbolo"" = 'SEMENTES' LIMIT 1)
                    WHEN ""Unidade"" = 1 THEN (SELECT ""Id"" FROM ""public"".""UnidadeMedida"" WHERE ""Simbolo"" = 'KG' LIMIT 1)
                    WHEN ""Unidade"" = 2 THEN (SELECT ""Id"" FROM ""public"".""UnidadeMedida"" WHERE ""Simbolo"" = 'T' LIMIT 1)
                    WHEN ""Unidade"" = 3 THEN (SELECT ""Id"" FROM ""public"".""UnidadeMedida"" WHERE ""Simbolo"" = 'L' LIMIT 1)
                    WHEN ""Unidade"" = 4 THEN (SELECT ""Id"" FROM ""public"".""UnidadeMedida"" WHERE ""Simbolo"" = 'HA' LIMIT 1)
                    WHEN ""Unidade"" = 5 THEN (SELECT ""Id"" FROM ""public"".""UnidadeMedida"" WHERE ""Simbolo"" = 'DOSE' LIMIT 1)
                    WHEN ""Unidade"" = 6 THEN (SELECT ""Id"" FROM ""public"".""UnidadeMedida"" WHERE ""Simbolo"" = 'FRASCO' LIMIT 1)
                    WHEN ""Unidade"" = 7 THEN (SELECT ""Id"" FROM ""public"".""UnidadeMedida"" WHERE ""Simbolo"" = 'OVOS' LIMIT 1)
                    WHEN ""Unidade"" = 8 THEN (SELECT ""Id"" FROM ""public"".""UnidadeMedida"" WHERE ""Simbolo"" = 'PARASITOIDE' LIMIT 1)
                    ELSE (SELECT ""Id"" FROM ""public"".""UnidadeMedida"" WHERE ""Simbolo"" = 'KG' LIMIT 1)
                END
                WHERE ""UnidadeMedidaId"" = 1;
            ");

            // 4. Criar índices para as novas foreign keys
            migrationBuilder.CreateIndex(
                name: "IX_Produto_UnidadeMedidaId",
                schema: "public",
                table: "Produto",
                column: "UnidadeMedidaId");

            migrationBuilder.CreateIndex(
                name: "IX_Produto_EmbalagemId",
                schema: "public",
                table: "Produto",
                column: "EmbalagemId");

            migrationBuilder.CreateIndex(
                name: "IX_Produto_AtividadeAgropecuariaId",
                schema: "public",
                table: "Produto",
                column: "AtividadeAgropecuariaId");

            // 5. Adicionar foreign key constraints
            migrationBuilder.AddForeignKey(
                name: "FK_Produto_UnidadeMedida_UnidadeMedidaId",
                schema: "public",
                table: "Produto",
                column: "UnidadeMedidaId",
                principalSchema: "public",
                principalTable: "UnidadeMedida",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Produto_Embalagem_EmbalagemId",
                schema: "public",
                table: "Produto",
                column: "EmbalagemId",
                principalSchema: "public",
                principalTable: "Embalagem",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Produto_AtividadeAgropecuaria_AtividadeAgropecuariaId",
                schema: "public",
                table: "Produto",
                column: "AtividadeAgropecuariaId",
                principalSchema: "public",
                principalTable: "AtividadeAgropecuaria",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            // 6. Remover a coluna antiga "Unidade" após a migração dos dados
            migrationBuilder.DropColumn(
                name: "Unidade",
                schema: "public",
                table: "Produto");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Remover foreign key constraints
            migrationBuilder.DropForeignKey(
                name: "FK_Produto_UnidadeMedida_UnidadeMedidaId",
                schema: "public",
                table: "Produto");

            migrationBuilder.DropForeignKey(
                name: "FK_Produto_Embalagem_EmbalagemId",
                schema: "public",
                table: "Produto");

            migrationBuilder.DropForeignKey(
                name: "FK_Produto_AtividadeAgropecuaria_AtividadeAgropecuariaId",
                schema: "public",
                table: "Produto");

            // Remover índices
            migrationBuilder.DropIndex(
                name: "IX_Produto_UnidadeMedidaId",
                schema: "public",
                table: "Produto");

            migrationBuilder.DropIndex(
                name: "IX_Produto_EmbalagemId",
                schema: "public",
                table: "Produto");

            migrationBuilder.DropIndex(
                name: "IX_Produto_AtividadeAgropecuariaId",
                schema: "public",
                table: "Produto");

            // Recriar coluna Unidade
            migrationBuilder.AddColumn<int>(
                name: "Unidade",
                schema: "public",
                table: "Produto",
                type: "integer",
                nullable: false,
                defaultValue: 1);

            // Remover novas colunas
            migrationBuilder.DropColumn(
                name: "UnidadeMedidaId",
                schema: "public",
                table: "Produto");

            migrationBuilder.DropColumn(
                name: "EmbalagemId",
                schema: "public",
                table: "Produto");

            migrationBuilder.DropColumn(
                name: "AtividadeAgropecuariaId",
                schema: "public",
                table: "Produto");

            migrationBuilder.DropColumn(
                name: "Altura",
                schema: "public",
                table: "Produto");

            migrationBuilder.DropColumn(
                name: "Largura",
                schema: "public",
                table: "Produto");

            migrationBuilder.DropColumn(
                name: "Comprimento",
                schema: "public",
                table: "Produto");

            migrationBuilder.DropColumn(
                name: "PesoNominal",
                schema: "public",
                table: "Produto");

            migrationBuilder.DropColumn(
                name: "PesoEmbalagem",
                schema: "public",
                table: "Produto");

            migrationBuilder.DropColumn(
                name: "Pms",
                schema: "public",
                table: "Produto");

            migrationBuilder.DropColumn(
                name: "QuantidadeMinima",
                schema: "public",
                table: "Produto");

            migrationBuilder.DropColumn(
                name: "Embalagem",
                schema: "public",
                table: "Produto");

            migrationBuilder.DropColumn(
                name: "FaixaDensidadeInicial",
                schema: "public",
                table: "Produto");

            migrationBuilder.DropColumn(
                name: "FaixaDensidadeFinal",
                schema: "public",
                table: "Produto");
        }
    }
}