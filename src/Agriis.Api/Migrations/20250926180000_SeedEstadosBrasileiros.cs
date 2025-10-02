using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Agriis.Api.Migrations
{
    /// <inheritdoc />
    public partial class SeedEstadosBrasileiros : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Criar tabela de países
            migrationBuilder.CreateTable(
                name: "paises",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    codigo = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    nome = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ativo = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    data_criacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    data_atualizacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_paises", x => x.id);
                });

            // Criar índices para países
            migrationBuilder.CreateIndex(
                name: "IX_paises_codigo",
                schema: "public",
                table: "paises",
                column: "codigo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_paises_nome",
                schema: "public",
                table: "paises",
                column: "nome");

            migrationBuilder.CreateIndex(
                name: "IX_paises_ativo",
                schema: "public",
                table: "paises",
                column: "ativo");

            migrationBuilder.CreateIndex(
                name: "IX_paises_data_criacao",
                schema: "public",
                table: "paises",
                column: "data_criacao");

            // Inserir Brasil
            migrationBuilder.Sql(@"
                INSERT INTO public.paises (codigo, nome, ativo, data_criacao) VALUES
                ('BR', 'Brasil', true, CURRENT_TIMESTAMP);
            ");

            // Inserir dados dos Estados brasileiros
            migrationBuilder.Sql(@"
                INSERT INTO public.estados (nome, uf, codigo_ibge, regiao, pais_id, data_criacao) VALUES
                ('Acre', 'AC', 12, 'Norte', 1, CURRENT_TIMESTAMP),
                ('Alagoas', 'AL', 27, 'Nordeste', 1, CURRENT_TIMESTAMP),
                ('Amapá', 'AP', 16, 'Norte', 1, CURRENT_TIMESTAMP),
                ('Amazonas', 'AM', 13, 'Norte', 1, CURRENT_TIMESTAMP),
                ('Bahia', 'BA', 29, 'Nordeste', 1, CURRENT_TIMESTAMP),
                ('Ceará', 'CE', 23, 'Nordeste', 1, CURRENT_TIMESTAMP),
                ('Distrito Federal', 'DF', 53, 'Centro-Oeste', 1, CURRENT_TIMESTAMP),
                ('Espírito Santo', 'ES', 32, 'Sudeste', 1, CURRENT_TIMESTAMP),
                ('Goiás', 'GO', 52, 'Centro-Oeste', 1, CURRENT_TIMESTAMP),
                ('Maranhão', 'MA', 21, 'Nordeste', 1, CURRENT_TIMESTAMP),
                ('Mato Grosso', 'MT', 51, 'Centro-Oeste', 1, CURRENT_TIMESTAMP),
                ('Mato Grosso do Sul', 'MS', 50, 'Centro-Oeste', 1, CURRENT_TIMESTAMP),
                ('Minas Gerais', 'MG', 31, 'Sudeste', 1, CURRENT_TIMESTAMP),
                ('Pará', 'PA', 15, 'Norte', 1, CURRENT_TIMESTAMP),
                ('Paraíba', 'PB', 25, 'Nordeste', 1, CURRENT_TIMESTAMP),
                ('Paraná', 'PR', 41, 'Sul', 1, CURRENT_TIMESTAMP),
                ('Pernambuco', 'PE', 26, 'Nordeste', 1, CURRENT_TIMESTAMP),
                ('Piauí', 'PI', 22, 'Nordeste', 1, CURRENT_TIMESTAMP),
                ('Rio de Janeiro', 'RJ', 33, 'Sudeste', 1, CURRENT_TIMESTAMP),
                ('Rio Grande do Norte', 'RN', 24, 'Nordeste', 1, CURRENT_TIMESTAMP),
                ('Rio Grande do Sul', 'RS', 43, 'Sul', 1, CURRENT_TIMESTAMP),
                ('Rondônia', 'RO', 11, 'Norte', 1, CURRENT_TIMESTAMP),
                ('Roraima', 'RR', 14, 'Norte', 1, CURRENT_TIMESTAMP),
                ('Santa Catarina', 'SC', 42, 'Sul', 1, CURRENT_TIMESTAMP),
                ('São Paulo', 'SP', 35, 'Sudeste', 1, CURRENT_TIMESTAMP),
                ('Sergipe', 'SE', 28, 'Nordeste', 1, CURRENT_TIMESTAMP),
                ('Tocantins', 'TO', 17, 'Norte', 1, CURRENT_TIMESTAMP);
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Remover dados dos Estados brasileiros
            migrationBuilder.Sql(@"
                DELETE FROM public.estados WHERE pais_id = 1;
            ");

            // Remover Brasil
            migrationBuilder.Sql(@"
                DELETE FROM public.paises WHERE codigo = 'BR';
            ");

            // Remover tabela de países
            migrationBuilder.DropTable(
                name: "paises",
                schema: "public");
        }
    }
}