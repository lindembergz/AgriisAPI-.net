using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Agriis.Api.Migrations
{
    /// <inheritdoc />
    public partial class AdicionarCamposEnderecoFornecedor : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Cep",
                schema: "public",
                table: "Fornecedor",
                type: "character varying(10)",
                maxLength: 10,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Complemento",
                schema: "public",
                table: "Fornecedor",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "Latitude",
                schema: "public",
                table: "Fornecedor",
                type: "numeric(10,8)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "Longitude",
                schema: "public",
                table: "Fornecedor",
                type: "numeric(11,8)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Municipio",
                schema: "public",
                table: "Fornecedor",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Uf",
                schema: "public",
                table: "Fornecedor",
                type: "character varying(2)",
                maxLength: 2,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Cep",
                schema: "public",
                table: "Fornecedor");

            migrationBuilder.DropColumn(
                name: "Complemento",
                schema: "public",
                table: "Fornecedor");

            migrationBuilder.DropColumn(
                name: "Latitude",
                schema: "public",
                table: "Fornecedor");

            migrationBuilder.DropColumn(
                name: "Longitude",
                schema: "public",
                table: "Fornecedor");

            migrationBuilder.DropColumn(
                name: "Municipio",
                schema: "public",
                table: "Fornecedor");

            migrationBuilder.DropColumn(
                name: "Uf",
                schema: "public",
                table: "Fornecedor");
        }
    }
}
