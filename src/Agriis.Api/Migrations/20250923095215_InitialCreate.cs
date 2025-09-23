using System;
using System.Text.Json;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Agriis.Api.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "public");

            migrationBuilder.CreateTable(
                name: "Cultura",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Nome = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Descricao = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Ativo = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    DataCriacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    DataAtualizacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Cultura", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "estados",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    nome = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    uf = table.Column<string>(type: "character varying(2)", maxLength: 2, nullable: false),
                    codigo_ibge = table.Column<int>(type: "integer", nullable: false),
                    regiao = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    data_criacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    data_atualizacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_estados", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "refresh_tokens",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    token = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    usuario_id = table.Column<int>(type: "integer", nullable: false),
                    data_expiracao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    revogado = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    data_revogacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    endereco_ip = table.Column<string>(type: "character varying(45)", maxLength: 45, nullable: true),
                    user_agent = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    data_criacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    data_atualizacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_refresh_tokens", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "usuarios",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    nome = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    email = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    celular = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    cpf = table.Column<string>(type: "character varying(11)", maxLength: 11, nullable: true),
                    senha_hash = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    ativo = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    ultimo_login = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    logo_url = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    data_criacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    data_atualizacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_usuarios", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "municipios",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    nome = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    codigo_ibge = table.Column<int>(type: "integer", nullable: false),
                    cep_principal = table.Column<string>(type: "character varying(8)", maxLength: 8, nullable: true),
                    latitude = table.Column<double>(type: "double precision", precision: 10, scale: 8, nullable: true),
                    longitude = table.Column<double>(type: "double precision", precision: 11, scale: 8, nullable: true),
                    estado_id = table.Column<int>(type: "integer", nullable: false),
                    data_criacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    data_atualizacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_municipios", x => x.id);
                    table.ForeignKey(
                        name: "FK_municipios_estados_estado_id",
                        column: x => x.estado_id,
                        principalSchema: "public",
                        principalTable: "estados",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Produtor",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Nome = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Cpf = table.Column<string>(type: "character varying(11)", maxLength: 11, nullable: true),
                    Cnpj = table.Column<string>(type: "character varying(14)", maxLength: 14, nullable: true),
                    InscricaoEstadual = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    TipoAtividade = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    AreaPlantio = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    DataAutorizacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    RetornosApiCheckProdutor = table.Column<JsonDocument>(type: "jsonb", nullable: true),
                    UsuarioAutorizacaoId = table.Column<int>(type: "integer", nullable: true),
                    Culturas = table.Column<string>(type: "jsonb", nullable: false),
                    DataCriacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    DataAtualizacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Produtor", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Produtor_usuarios_UsuarioAutorizacaoId",
                        column: x => x.UsuarioAutorizacaoId,
                        principalSchema: "public",
                        principalTable: "usuarios",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "usuario_roles",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    usuario_id = table.Column<int>(type: "integer", nullable: false),
                    role = table.Column<int>(type: "integer", nullable: false),
                    data_atribuicao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    data_criacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    data_atualizacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_usuario_roles", x => x.id);
                    table.ForeignKey(
                        name: "FK_usuario_roles_usuarios_usuario_id",
                        column: x => x.usuario_id,
                        principalSchema: "public",
                        principalTable: "usuarios",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "enderecos",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    cep = table.Column<string>(type: "character varying(8)", maxLength: 8, nullable: false),
                    logradouro = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    numero = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    complemento = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    bairro = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    latitude = table.Column<double>(type: "double precision", precision: 10, scale: 8, nullable: true),
                    longitude = table.Column<double>(type: "double precision", precision: 11, scale: 8, nullable: true),
                    municipio_id = table.Column<int>(type: "integer", nullable: false),
                    estado_id = table.Column<int>(type: "integer", nullable: false),
                    data_criacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    data_atualizacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_enderecos", x => x.id);
                    table.ForeignKey(
                        name: "FK_enderecos_estados_estado_id",
                        column: x => x.estado_id,
                        principalSchema: "public",
                        principalTable: "estados",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_enderecos_municipios_municipio_id",
                        column: x => x.municipio_id,
                        principalSchema: "public",
                        principalTable: "municipios",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "UsuarioProdutor",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UsuarioId = table.Column<int>(type: "integer", nullable: false),
                    ProdutorId = table.Column<int>(type: "integer", nullable: false),
                    EhProprietario = table.Column<bool>(type: "boolean", nullable: false),
                    Ativo = table.Column<bool>(type: "boolean", nullable: false),
                    DataCriacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    DataAtualizacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UsuarioProdutor", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UsuarioProdutor_Produtor_ProdutorId",
                        column: x => x.ProdutorId,
                        principalSchema: "public",
                        principalTable: "Produtor",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UsuarioProdutor_usuarios_UsuarioId",
                        column: x => x.UsuarioId,
                        principalSchema: "public",
                        principalTable: "usuarios",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Cultura_Ativo",
                schema: "public",
                table: "Cultura",
                column: "Ativo");

            migrationBuilder.CreateIndex(
                name: "IX_Cultura_Nome",
                schema: "public",
                table: "Cultura",
                column: "Nome",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Culturas_DataCriacao",
                schema: "public",
                table: "Cultura",
                column: "DataCriacao");

            migrationBuilder.CreateIndex(
                name: "IX_enderecos_bairro",
                schema: "public",
                table: "enderecos",
                column: "bairro");

            migrationBuilder.CreateIndex(
                name: "IX_enderecos_cep",
                schema: "public",
                table: "enderecos",
                column: "cep");

            migrationBuilder.CreateIndex(
                name: "IX_Enderecos_DataCriacao",
                schema: "public",
                table: "enderecos",
                column: "data_criacao");

            migrationBuilder.CreateIndex(
                name: "IX_enderecos_estado_id",
                schema: "public",
                table: "enderecos",
                column: "estado_id");

            migrationBuilder.CreateIndex(
                name: "IX_enderecos_logradouro",
                schema: "public",
                table: "enderecos",
                column: "logradouro");

            migrationBuilder.CreateIndex(
                name: "IX_enderecos_municipio_id",
                schema: "public",
                table: "enderecos",
                column: "municipio_id");

            migrationBuilder.CreateIndex(
                name: "IX_enderecos_unique_address",
                schema: "public",
                table: "enderecos",
                columns: new[] { "cep", "logradouro", "numero", "municipio_id" });

            migrationBuilder.CreateIndex(
                name: "IX_estados_codigo_ibge",
                schema: "public",
                table: "estados",
                column: "codigo_ibge",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Estados_DataCriacao",
                schema: "public",
                table: "estados",
                column: "data_criacao");

            migrationBuilder.CreateIndex(
                name: "IX_estados_nome",
                schema: "public",
                table: "estados",
                column: "nome");

            migrationBuilder.CreateIndex(
                name: "IX_estados_regiao",
                schema: "public",
                table: "estados",
                column: "regiao");

            migrationBuilder.CreateIndex(
                name: "IX_estados_uf",
                schema: "public",
                table: "estados",
                column: "uf",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_municipios_cep_principal",
                schema: "public",
                table: "municipios",
                column: "cep_principal");

            migrationBuilder.CreateIndex(
                name: "IX_municipios_codigo_ibge",
                schema: "public",
                table: "municipios",
                column: "codigo_ibge",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Municipios_DataCriacao",
                schema: "public",
                table: "municipios",
                column: "data_criacao");

            migrationBuilder.CreateIndex(
                name: "IX_municipios_estado_id",
                schema: "public",
                table: "municipios",
                column: "estado_id");

            migrationBuilder.CreateIndex(
                name: "IX_municipios_nome",
                schema: "public",
                table: "municipios",
                column: "nome");

            migrationBuilder.CreateIndex(
                name: "IX_Produtor_AreaPlantio",
                schema: "public",
                table: "Produtor",
                column: "AreaPlantio");

            migrationBuilder.CreateIndex(
                name: "IX_Produtor_Cnpj",
                schema: "public",
                table: "Produtor",
                column: "Cnpj",
                unique: true,
                filter: "\"Cnpj\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Produtor_Cpf",
                schema: "public",
                table: "Produtor",
                column: "Cpf",
                unique: true,
                filter: "\"Cpf\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Produtor_DataCriacao",
                schema: "public",
                table: "Produtor",
                column: "DataCriacao");

            migrationBuilder.CreateIndex(
                name: "IX_Produtor_Status",
                schema: "public",
                table: "Produtor",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_Produtor_UsuarioAutorizacaoId",
                schema: "public",
                table: "Produtor",
                column: "UsuarioAutorizacaoId");

            migrationBuilder.CreateIndex(
                name: "ix_refresh_tokens_data_expiracao",
                schema: "public",
                table: "refresh_tokens",
                column: "data_expiracao");

            migrationBuilder.CreateIndex(
                name: "ix_refresh_tokens_token",
                schema: "public",
                table: "refresh_tokens",
                column: "token",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_refresh_tokens_usuario_id",
                schema: "public",
                table: "refresh_tokens",
                column: "usuario_id");

            migrationBuilder.CreateIndex(
                name: "ix_refresh_tokens_usuario_valido",
                schema: "public",
                table: "refresh_tokens",
                columns: new[] { "usuario_id", "revogado", "data_expiracao" });

            migrationBuilder.CreateIndex(
                name: "IX_RefreshTokens_DataCriacao",
                schema: "public",
                table: "refresh_tokens",
                column: "data_criacao");

            migrationBuilder.CreateIndex(
                name: "ix_usuario_roles_role",
                schema: "public",
                table: "usuario_roles",
                column: "role");

            migrationBuilder.CreateIndex(
                name: "ix_usuario_roles_usuario_id",
                schema: "public",
                table: "usuario_roles",
                column: "usuario_id");

            migrationBuilder.CreateIndex(
                name: "ix_usuario_roles_usuario_role_unique",
                schema: "public",
                table: "usuario_roles",
                columns: new[] { "usuario_id", "role" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UsuarioRoles_DataCriacao",
                schema: "public",
                table: "usuario_roles",
                column: "data_criacao");

            migrationBuilder.CreateIndex(
                name: "IX_UsuarioProdutor_Ativo",
                schema: "public",
                table: "UsuarioProdutor",
                column: "Ativo");

            migrationBuilder.CreateIndex(
                name: "IX_UsuarioProdutor_EhProprietario",
                schema: "public",
                table: "UsuarioProdutor",
                column: "EhProprietario");

            migrationBuilder.CreateIndex(
                name: "IX_UsuarioProdutor_ProdutorId",
                schema: "public",
                table: "UsuarioProdutor",
                column: "ProdutorId");

            migrationBuilder.CreateIndex(
                name: "IX_UsuarioProdutor_UsuarioId",
                schema: "public",
                table: "UsuarioProdutor",
                column: "UsuarioId");

            migrationBuilder.CreateIndex(
                name: "IX_UsuarioProdutor_UsuarioId_ProdutorId",
                schema: "public",
                table: "UsuarioProdutor",
                columns: new[] { "UsuarioId", "ProdutorId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UsuariosProdutores_DataCriacao",
                schema: "public",
                table: "UsuarioProdutor",
                column: "DataCriacao");

            migrationBuilder.CreateIndex(
                name: "ix_usuarios_ativo",
                schema: "public",
                table: "usuarios",
                column: "ativo");

            migrationBuilder.CreateIndex(
                name: "ix_usuarios_cpf",
                schema: "public",
                table: "usuarios",
                column: "cpf",
                unique: true,
                filter: "cpf IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Usuarios_DataCriacao",
                schema: "public",
                table: "usuarios",
                column: "data_criacao");

            migrationBuilder.CreateIndex(
                name: "ix_usuarios_email",
                schema: "public",
                table: "usuarios",
                column: "email",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Cultura",
                schema: "public");

            migrationBuilder.DropTable(
                name: "enderecos",
                schema: "public");

            migrationBuilder.DropTable(
                name: "refresh_tokens",
                schema: "public");

            migrationBuilder.DropTable(
                name: "usuario_roles",
                schema: "public");

            migrationBuilder.DropTable(
                name: "UsuarioProdutor",
                schema: "public");

            migrationBuilder.DropTable(
                name: "municipios",
                schema: "public");

            migrationBuilder.DropTable(
                name: "Produtor",
                schema: "public");

            migrationBuilder.DropTable(
                name: "estados",
                schema: "public");

            migrationBuilder.DropTable(
                name: "usuarios",
                schema: "public");
        }
    }
}
