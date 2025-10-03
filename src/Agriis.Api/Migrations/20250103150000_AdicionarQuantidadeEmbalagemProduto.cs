using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Agriis.Api.Migrations
{
    /// <inheritdoc />
    public partial class AdicionarQuantidadeEmbalagemProduto : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Adicionar campo QuantidadeEmbalagem à tabela Produto
            migrationBuilder.AddColumn<decimal>(
                name: "QuantidadeEmbalagem",
                schema: "public",
                table: "Produto",
                type: "decimal(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                defaultValue: 1.0m);

            // Criar índice para performance
            migrationBuilder.CreateIndex(
                name: "IX_Produto_QuantidadeEmbalagem",
                schema: "public",
                table: "Produto",
                column: "QuantidadeEmbalagem");

            // Adicionar comentário na coluna
            migrationBuilder.Sql(@"
                COMMENT ON COLUMN public.""Produto"".""QuantidadeEmbalagem"" IS 'Quantidade de produto por embalagem';
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Remover índice
            migrationBuilder.DropIndex(
                name: "IX_Produto_QuantidadeEmbalagem",
                schema: "public",
                table: "Produto");

            // Remover coluna
            migrationBuilder.DropColumn(
                name: "QuantidadeEmbalagem",
                schema: "public",
                table: "Produto");
        }
    }
}