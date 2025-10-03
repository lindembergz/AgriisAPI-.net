using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Agriis.Api.Migrations
{
    /// <inheritdoc />
    public partial class AdicionarCamposAdicionaisFornecedor : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "NomeFantasia",
                schema: "public",
                table: "Fornecedor",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RamosAtividade",
                schema: "public",
                table: "Fornecedor",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "EnderecoCorrespondencia",
                schema: "public",
                table: "Fornecedor",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "MesmoFaturamento");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "NomeFantasia",
                schema: "public",
                table: "Fornecedor");

            migrationBuilder.DropColumn(
                name: "RamosAtividade",
                schema: "public",
                table: "Fornecedor");

            migrationBuilder.DropColumn(
                name: "EnderecoCorrespondencia",
                schema: "public",
                table: "Fornecedor");
        }
    }
}