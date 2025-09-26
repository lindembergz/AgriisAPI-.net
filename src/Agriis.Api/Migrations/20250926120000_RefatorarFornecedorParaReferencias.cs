using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Agriis.Api.Migrations
{
    /// <inheritdoc />
    public partial class RefatorarFornecedorParaReferencias : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Remover índices existentes
            migrationBuilder.DropIndex(
                name: "IX_Fornecedor_EstadoId",
                table: "Fornecedor");

            // Remover foreign keys existentes se existirem
            migrationBuilder.DropForeignKey(
                name: "FK_Fornecedor_Estado_EstadoId",
                table: "Fornecedor");

            // Adicionar nova coluna UfId
            migrationBuilder.AddColumn<int>(
                name: "UfId",
                table: "Fornecedor",
                type: "integer",
                nullable: true);

            // Migrar dados de EstadoId para UfId
            // Assumindo que existe uma correspondência entre Estado e Uf
            migrationBuilder.Sql(@"
                UPDATE ""Fornecedor"" 
                SET ""UfId"" = (
                    SELECT u.""Id"" 
                    FROM ""Ufs"" u 
                    INNER JOIN ""Estados"" e ON e.""Codigo"" = u.""Codigo""
                    WHERE e.""Id"" = ""Fornecedor"".""EstadoId""
                )
                WHERE ""EstadoId"" IS NOT NULL;
            ");

            // Atualizar MunicipioId para usar a nova tabela de referências
            // Primeiro, criar uma tabela temporária para mapear os dados
            migrationBuilder.Sql(@"
                UPDATE ""Fornecedor"" 
                SET ""MunicipioId"" = (
                    SELECT m.""Id"" 
                    FROM ""Municipios"" m 
                    INNER JOIN ""MunicipiosEnderecos"" me ON me.""Nome"" = m.""Nome""
                    INNER JOIN ""Ufs"" u ON u.""Id"" = m.""UfId""
                    WHERE me.""Id"" = ""Fornecedor"".""MunicipioId""
                    AND u.""Id"" = ""Fornecedor"".""UfId""
                    LIMIT 1
                )
                WHERE ""MunicipioId"" IS NOT NULL AND ""UfId"" IS NOT NULL;
            ");

            // Remover coluna EstadoId
            migrationBuilder.DropColumn(
                name: "EstadoId",
                table: "Fornecedor");

            // Criar novos índices
            migrationBuilder.CreateIndex(
                name: "IX_Fornecedor_UfId",
                table: "Fornecedor",
                column: "UfId");

            // Criar foreign keys para as novas referências
            migrationBuilder.AddForeignKey(
                name: "FK_Fornecedor_Ufs_UfId",
                table: "Fornecedor",
                column: "UfId",
                principalTable: "Ufs",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Fornecedor_Municipios_MunicipioId",
                table: "Fornecedor",
                column: "MunicipioId",
                principalTable: "Municipios",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Remover foreign keys
            migrationBuilder.DropForeignKey(
                name: "FK_Fornecedor_Ufs_UfId",
                table: "Fornecedor");

            migrationBuilder.DropForeignKey(
                name: "FK_Fornecedor_Municipios_MunicipioId",
                table: "Fornecedor");

            // Remover índices
            migrationBuilder.DropIndex(
                name: "IX_Fornecedor_UfId",
                table: "Fornecedor");

            // Adicionar coluna EstadoId de volta
            migrationBuilder.AddColumn<int>(
                name: "EstadoId",
                table: "Fornecedor",
                type: "integer",
                nullable: true);

            // Migrar dados de volta (rollback)
            migrationBuilder.Sql(@"
                UPDATE ""Fornecedor"" 
                SET ""EstadoId"" = (
                    SELECT e.""Id"" 
                    FROM ""Estados"" e 
                    INNER JOIN ""Ufs"" u ON u.""Codigo"" = e.""Codigo""
                    WHERE u.""Id"" = ""Fornecedor"".""UfId""
                )
                WHERE ""UfId"" IS NOT NULL;
            ");

            // Reverter MunicipioId para usar a tabela antiga
            migrationBuilder.Sql(@"
                UPDATE ""Fornecedor"" 
                SET ""MunicipioId"" = (
                    SELECT me.""Id"" 
                    FROM ""MunicipiosEnderecos"" me 
                    INNER JOIN ""Municipios"" m ON m.""Nome"" = me.""Nome""
                    WHERE m.""Id"" = ""Fornecedor"".""MunicipioId""
                    LIMIT 1
                )
                WHERE ""MunicipioId"" IS NOT NULL;
            ");

            // Remover coluna UfId
            migrationBuilder.DropColumn(
                name: "UfId",
                table: "Fornecedor");

            // Recriar índice EstadoId
            migrationBuilder.CreateIndex(
                name: "IX_Fornecedor_EstadoId",
                table: "Fornecedor",
                column: "EstadoId");

            // Recriar foreign key para Estado
            migrationBuilder.AddForeignKey(
                name: "FK_Fornecedor_Estado_EstadoId",
                table: "Fornecedor",
                column: "EstadoId",
                principalTable: "Estados",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}