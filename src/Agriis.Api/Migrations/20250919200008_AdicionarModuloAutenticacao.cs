using System;
using Microsoft.EntityFrameworkCore.Migrations;
using NetTopologySuite.Geometries;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Agriis.Api.Migrations
{
    /// <inheritdoc />
    public partial class AdicionarModuloAutenticacao : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Enderecos_Estados_EstadoId",
                schema: "public",
                table: "Enderecos");

            migrationBuilder.DropForeignKey(
                name: "FK_Enderecos_Municipios_MunicipioId",
                schema: "public",
                table: "Enderecos");

            migrationBuilder.DropForeignKey(
                name: "FK_Municipios_Estados_EstadoId",
                schema: "public",
                table: "Municipios");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Municipios",
                schema: "public",
                table: "Municipios");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Estados",
                schema: "public",
                table: "Estados");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Enderecos",
                schema: "public",
                table: "Enderecos");

            migrationBuilder.RenameTable(
                name: "Municipios",
                schema: "public",
                newName: "municipios",
                newSchema: "public");

            migrationBuilder.RenameTable(
                name: "Estados",
                schema: "public",
                newName: "estados",
                newSchema: "public");

            migrationBuilder.RenameTable(
                name: "Enderecos",
                schema: "public",
                newName: "enderecos",
                newSchema: "public");

            migrationBuilder.RenameColumn(
                name: "Nome",
                schema: "public",
                table: "municipios",
                newName: "nome");

            migrationBuilder.RenameColumn(
                name: "Longitude",
                schema: "public",
                table: "municipios",
                newName: "longitude");

            migrationBuilder.RenameColumn(
                name: "Localizacao",
                schema: "public",
                table: "municipios",
                newName: "localizacao");

            migrationBuilder.RenameColumn(
                name: "Latitude",
                schema: "public",
                table: "municipios",
                newName: "latitude");

            migrationBuilder.RenameColumn(
                name: "Id",
                schema: "public",
                table: "municipios",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "EstadoId",
                schema: "public",
                table: "municipios",
                newName: "estado_id");

            migrationBuilder.RenameColumn(
                name: "DataCriacao",
                schema: "public",
                table: "municipios",
                newName: "data_criacao");

            migrationBuilder.RenameColumn(
                name: "DataAtualizacao",
                schema: "public",
                table: "municipios",
                newName: "data_atualizacao");

            migrationBuilder.RenameColumn(
                name: "CodigoIbge",
                schema: "public",
                table: "municipios",
                newName: "codigo_ibge");

            migrationBuilder.RenameColumn(
                name: "CepPrincipal",
                schema: "public",
                table: "municipios",
                newName: "cep_principal");

            migrationBuilder.RenameIndex(
                name: "IX_Municipios_EstadoId",
                schema: "public",
                table: "municipios",
                newName: "IX_municipios_estado_id");

            migrationBuilder.RenameColumn(
                name: "Uf",
                schema: "public",
                table: "estados",
                newName: "uf");

            migrationBuilder.RenameColumn(
                name: "Regiao",
                schema: "public",
                table: "estados",
                newName: "regiao");

            migrationBuilder.RenameColumn(
                name: "Nome",
                schema: "public",
                table: "estados",
                newName: "nome");

            migrationBuilder.RenameColumn(
                name: "Id",
                schema: "public",
                table: "estados",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "DataCriacao",
                schema: "public",
                table: "estados",
                newName: "data_criacao");

            migrationBuilder.RenameColumn(
                name: "DataAtualizacao",
                schema: "public",
                table: "estados",
                newName: "data_atualizacao");

            migrationBuilder.RenameColumn(
                name: "CodigoIbge",
                schema: "public",
                table: "estados",
                newName: "codigo_ibge");

            migrationBuilder.RenameColumn(
                name: "Numero",
                schema: "public",
                table: "enderecos",
                newName: "numero");

            migrationBuilder.RenameColumn(
                name: "Longitude",
                schema: "public",
                table: "enderecos",
                newName: "longitude");

            migrationBuilder.RenameColumn(
                name: "Logradouro",
                schema: "public",
                table: "enderecos",
                newName: "logradouro");

            migrationBuilder.RenameColumn(
                name: "Localizacao",
                schema: "public",
                table: "enderecos",
                newName: "localizacao");

            migrationBuilder.RenameColumn(
                name: "Latitude",
                schema: "public",
                table: "enderecos",
                newName: "latitude");

            migrationBuilder.RenameColumn(
                name: "Complemento",
                schema: "public",
                table: "enderecos",
                newName: "complemento");

            migrationBuilder.RenameColumn(
                name: "Cep",
                schema: "public",
                table: "enderecos",
                newName: "cep");

            migrationBuilder.RenameColumn(
                name: "Bairro",
                schema: "public",
                table: "enderecos",
                newName: "bairro");

            migrationBuilder.RenameColumn(
                name: "Id",
                schema: "public",
                table: "enderecos",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "MunicipioId",
                schema: "public",
                table: "enderecos",
                newName: "municipio_id");

            migrationBuilder.RenameColumn(
                name: "EstadoId",
                schema: "public",
                table: "enderecos",
                newName: "estado_id");

            migrationBuilder.RenameColumn(
                name: "DataCriacao",
                schema: "public",
                table: "enderecos",
                newName: "data_criacao");

            migrationBuilder.RenameColumn(
                name: "DataAtualizacao",
                schema: "public",
                table: "enderecos",
                newName: "data_atualizacao");

            migrationBuilder.RenameIndex(
                name: "IX_Enderecos_MunicipioId",
                schema: "public",
                table: "enderecos",
                newName: "IX_enderecos_municipio_id");

            migrationBuilder.RenameIndex(
                name: "IX_Enderecos_EstadoId",
                schema: "public",
                table: "enderecos",
                newName: "IX_enderecos_estado_id");

            migrationBuilder.AlterColumn<string>(
                name: "nome",
                schema: "public",
                table: "municipios",
                type: "character varying(150)",
                maxLength: 150,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<Point>(
                name: "localizacao",
                schema: "public",
                table: "municipios",
                type: "geometry(Point,4326)",
                nullable: true,
                oldClrType: typeof(Point),
                oldType: "geometry",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "cep_principal",
                schema: "public",
                table: "municipios",
                type: "character varying(8)",
                maxLength: 8,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "uf",
                schema: "public",
                table: "estados",
                type: "character varying(2)",
                maxLength: 2,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "regiao",
                schema: "public",
                table: "estados",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "nome",
                schema: "public",
                table: "estados",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "numero",
                schema: "public",
                table: "enderecos",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "logradouro",
                schema: "public",
                table: "enderecos",
                type: "character varying(200)",
                maxLength: 200,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<Point>(
                name: "localizacao",
                schema: "public",
                table: "enderecos",
                type: "geometry(Point,4326)",
                nullable: true,
                oldClrType: typeof(Point),
                oldType: "geometry",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "complemento",
                schema: "public",
                table: "enderecos",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "cep",
                schema: "public",
                table: "enderecos",
                type: "character varying(8)",
                maxLength: 8,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "bairro",
                schema: "public",
                table: "enderecos",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddPrimaryKey(
                name: "PK_municipios",
                schema: "public",
                table: "municipios",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_estados",
                schema: "public",
                table: "estados",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_enderecos",
                schema: "public",
                table: "enderecos",
                column: "id");

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
                name: "IX_municipios_localizacao",
                schema: "public",
                table: "municipios",
                column: "localizacao")
                .Annotation("Npgsql:IndexMethod", "gist");

            migrationBuilder.CreateIndex(
                name: "IX_municipios_nome",
                schema: "public",
                table: "municipios",
                column: "nome");

            migrationBuilder.CreateIndex(
                name: "IX_estados_codigo_ibge",
                schema: "public",
                table: "estados",
                column: "codigo_ibge",
                unique: true);

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
                name: "IX_enderecos_localizacao",
                schema: "public",
                table: "enderecos",
                column: "localizacao")
                .Annotation("Npgsql:IndexMethod", "gist");

            migrationBuilder.CreateIndex(
                name: "IX_enderecos_logradouro",
                schema: "public",
                table: "enderecos",
                column: "logradouro");

            migrationBuilder.CreateIndex(
                name: "IX_enderecos_unique_address",
                schema: "public",
                table: "enderecos",
                columns: new[] { "cep", "logradouro", "numero", "municipio_id" });

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

            migrationBuilder.AddForeignKey(
                name: "FK_enderecos_estados_estado_id",
                schema: "public",
                table: "enderecos",
                column: "estado_id",
                principalSchema: "public",
                principalTable: "estados",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_enderecos_municipios_municipio_id",
                schema: "public",
                table: "enderecos",
                column: "municipio_id",
                principalSchema: "public",
                principalTable: "municipios",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_municipios_estados_estado_id",
                schema: "public",
                table: "municipios",
                column: "estado_id",
                principalSchema: "public",
                principalTable: "estados",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_enderecos_estados_estado_id",
                schema: "public",
                table: "enderecos");

            migrationBuilder.DropForeignKey(
                name: "FK_enderecos_municipios_municipio_id",
                schema: "public",
                table: "enderecos");

            migrationBuilder.DropForeignKey(
                name: "FK_municipios_estados_estado_id",
                schema: "public",
                table: "municipios");

            migrationBuilder.DropTable(
                name: "refresh_tokens",
                schema: "public");

            migrationBuilder.DropTable(
                name: "usuario_roles",
                schema: "public");

            migrationBuilder.DropTable(
                name: "usuarios",
                schema: "public");

            migrationBuilder.DropPrimaryKey(
                name: "PK_municipios",
                schema: "public",
                table: "municipios");

            migrationBuilder.DropIndex(
                name: "IX_municipios_cep_principal",
                schema: "public",
                table: "municipios");

            migrationBuilder.DropIndex(
                name: "IX_municipios_codigo_ibge",
                schema: "public",
                table: "municipios");

            migrationBuilder.DropIndex(
                name: "IX_municipios_localizacao",
                schema: "public",
                table: "municipios");

            migrationBuilder.DropIndex(
                name: "IX_municipios_nome",
                schema: "public",
                table: "municipios");

            migrationBuilder.DropPrimaryKey(
                name: "PK_estados",
                schema: "public",
                table: "estados");

            migrationBuilder.DropIndex(
                name: "IX_estados_codigo_ibge",
                schema: "public",
                table: "estados");

            migrationBuilder.DropIndex(
                name: "IX_estados_nome",
                schema: "public",
                table: "estados");

            migrationBuilder.DropIndex(
                name: "IX_estados_regiao",
                schema: "public",
                table: "estados");

            migrationBuilder.DropIndex(
                name: "IX_estados_uf",
                schema: "public",
                table: "estados");

            migrationBuilder.DropPrimaryKey(
                name: "PK_enderecos",
                schema: "public",
                table: "enderecos");

            migrationBuilder.DropIndex(
                name: "IX_enderecos_bairro",
                schema: "public",
                table: "enderecos");

            migrationBuilder.DropIndex(
                name: "IX_enderecos_cep",
                schema: "public",
                table: "enderecos");

            migrationBuilder.DropIndex(
                name: "IX_enderecos_localizacao",
                schema: "public",
                table: "enderecos");

            migrationBuilder.DropIndex(
                name: "IX_enderecos_logradouro",
                schema: "public",
                table: "enderecos");

            migrationBuilder.DropIndex(
                name: "IX_enderecos_unique_address",
                schema: "public",
                table: "enderecos");

            migrationBuilder.RenameTable(
                name: "municipios",
                schema: "public",
                newName: "Municipios",
                newSchema: "public");

            migrationBuilder.RenameTable(
                name: "estados",
                schema: "public",
                newName: "Estados",
                newSchema: "public");

            migrationBuilder.RenameTable(
                name: "enderecos",
                schema: "public",
                newName: "Enderecos",
                newSchema: "public");

            migrationBuilder.RenameColumn(
                name: "nome",
                schema: "public",
                table: "Municipios",
                newName: "Nome");

            migrationBuilder.RenameColumn(
                name: "longitude",
                schema: "public",
                table: "Municipios",
                newName: "Longitude");

            migrationBuilder.RenameColumn(
                name: "localizacao",
                schema: "public",
                table: "Municipios",
                newName: "Localizacao");

            migrationBuilder.RenameColumn(
                name: "latitude",
                schema: "public",
                table: "Municipios",
                newName: "Latitude");

            migrationBuilder.RenameColumn(
                name: "id",
                schema: "public",
                table: "Municipios",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "estado_id",
                schema: "public",
                table: "Municipios",
                newName: "EstadoId");

            migrationBuilder.RenameColumn(
                name: "data_criacao",
                schema: "public",
                table: "Municipios",
                newName: "DataCriacao");

            migrationBuilder.RenameColumn(
                name: "data_atualizacao",
                schema: "public",
                table: "Municipios",
                newName: "DataAtualizacao");

            migrationBuilder.RenameColumn(
                name: "codigo_ibge",
                schema: "public",
                table: "Municipios",
                newName: "CodigoIbge");

            migrationBuilder.RenameColumn(
                name: "cep_principal",
                schema: "public",
                table: "Municipios",
                newName: "CepPrincipal");

            migrationBuilder.RenameIndex(
                name: "IX_municipios_estado_id",
                schema: "public",
                table: "Municipios",
                newName: "IX_Municipios_EstadoId");

            migrationBuilder.RenameColumn(
                name: "uf",
                schema: "public",
                table: "Estados",
                newName: "Uf");

            migrationBuilder.RenameColumn(
                name: "regiao",
                schema: "public",
                table: "Estados",
                newName: "Regiao");

            migrationBuilder.RenameColumn(
                name: "nome",
                schema: "public",
                table: "Estados",
                newName: "Nome");

            migrationBuilder.RenameColumn(
                name: "id",
                schema: "public",
                table: "Estados",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "data_criacao",
                schema: "public",
                table: "Estados",
                newName: "DataCriacao");

            migrationBuilder.RenameColumn(
                name: "data_atualizacao",
                schema: "public",
                table: "Estados",
                newName: "DataAtualizacao");

            migrationBuilder.RenameColumn(
                name: "codigo_ibge",
                schema: "public",
                table: "Estados",
                newName: "CodigoIbge");

            migrationBuilder.RenameColumn(
                name: "numero",
                schema: "public",
                table: "Enderecos",
                newName: "Numero");

            migrationBuilder.RenameColumn(
                name: "longitude",
                schema: "public",
                table: "Enderecos",
                newName: "Longitude");

            migrationBuilder.RenameColumn(
                name: "logradouro",
                schema: "public",
                table: "Enderecos",
                newName: "Logradouro");

            migrationBuilder.RenameColumn(
                name: "localizacao",
                schema: "public",
                table: "Enderecos",
                newName: "Localizacao");

            migrationBuilder.RenameColumn(
                name: "latitude",
                schema: "public",
                table: "Enderecos",
                newName: "Latitude");

            migrationBuilder.RenameColumn(
                name: "complemento",
                schema: "public",
                table: "Enderecos",
                newName: "Complemento");

            migrationBuilder.RenameColumn(
                name: "cep",
                schema: "public",
                table: "Enderecos",
                newName: "Cep");

            migrationBuilder.RenameColumn(
                name: "bairro",
                schema: "public",
                table: "Enderecos",
                newName: "Bairro");

            migrationBuilder.RenameColumn(
                name: "id",
                schema: "public",
                table: "Enderecos",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "municipio_id",
                schema: "public",
                table: "Enderecos",
                newName: "MunicipioId");

            migrationBuilder.RenameColumn(
                name: "estado_id",
                schema: "public",
                table: "Enderecos",
                newName: "EstadoId");

            migrationBuilder.RenameColumn(
                name: "data_criacao",
                schema: "public",
                table: "Enderecos",
                newName: "DataCriacao");

            migrationBuilder.RenameColumn(
                name: "data_atualizacao",
                schema: "public",
                table: "Enderecos",
                newName: "DataAtualizacao");

            migrationBuilder.RenameIndex(
                name: "IX_enderecos_municipio_id",
                schema: "public",
                table: "Enderecos",
                newName: "IX_Enderecos_MunicipioId");

            migrationBuilder.RenameIndex(
                name: "IX_enderecos_estado_id",
                schema: "public",
                table: "Enderecos",
                newName: "IX_Enderecos_EstadoId");

            migrationBuilder.AlterColumn<string>(
                name: "Nome",
                schema: "public",
                table: "Municipios",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(150)",
                oldMaxLength: 150);

            migrationBuilder.AlterColumn<Point>(
                name: "Localizacao",
                schema: "public",
                table: "Municipios",
                type: "geometry",
                nullable: true,
                oldClrType: typeof(Point),
                oldType: "geometry(Point,4326)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "CepPrincipal",
                schema: "public",
                table: "Municipios",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(8)",
                oldMaxLength: 8,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Uf",
                schema: "public",
                table: "Estados",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(2)",
                oldMaxLength: 2);

            migrationBuilder.AlterColumn<string>(
                name: "Regiao",
                schema: "public",
                table: "Estados",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<string>(
                name: "Nome",
                schema: "public",
                table: "Estados",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "Numero",
                schema: "public",
                table: "Enderecos",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(20)",
                oldMaxLength: 20,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Logradouro",
                schema: "public",
                table: "Enderecos",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(200)",
                oldMaxLength: 200);

            migrationBuilder.AlterColumn<Point>(
                name: "Localizacao",
                schema: "public",
                table: "Enderecos",
                type: "geometry",
                nullable: true,
                oldClrType: typeof(Point),
                oldType: "geometry(Point,4326)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Complemento",
                schema: "public",
                table: "Enderecos",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Cep",
                schema: "public",
                table: "Enderecos",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(8)",
                oldMaxLength: 8);

            migrationBuilder.AlterColumn<string>(
                name: "Bairro",
                schema: "public",
                table: "Enderecos",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100);

            migrationBuilder.AddPrimaryKey(
                name: "PK_Municipios",
                schema: "public",
                table: "Municipios",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Estados",
                schema: "public",
                table: "Estados",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Enderecos",
                schema: "public",
                table: "Enderecos",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Enderecos_Estados_EstadoId",
                schema: "public",
                table: "Enderecos",
                column: "EstadoId",
                principalSchema: "public",
                principalTable: "Estados",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Enderecos_Municipios_MunicipioId",
                schema: "public",
                table: "Enderecos",
                column: "MunicipioId",
                principalSchema: "public",
                principalTable: "Municipios",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Municipios_Estados_EstadoId",
                schema: "public",
                table: "Municipios",
                column: "EstadoId",
                principalSchema: "public",
                principalTable: "Estados",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
