using System;
using System.Text.Json;
using Microsoft.EntityFrameworkCore.Migrations;
using NetTopologySuite.Geometries;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Agriis.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddRemainingTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Catalogo",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    SafraId = table.Column<int>(type: "integer", nullable: false),
                    PontoDistribuicaoId = table.Column<int>(type: "integer", nullable: false),
                    CulturaId = table.Column<int>(type: "integer", nullable: false),
                    CategoriaId = table.Column<int>(type: "integer", nullable: false),
                    Moeda = table.Column<string>(type: "text", nullable: false),
                    DataInicio = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    DataFim = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    Ativo = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    DataCriacao = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    DataAtualizacao = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Catalogo", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Categoria",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Nome = table.Column<string>(type: "text", nullable: false),
                    Descricao = table.Column<string>(type: "text", nullable: true),
                    Tipo = table.Column<int>(type: "integer", nullable: false),
                    Ativo = table.Column<bool>(type: "boolean", nullable: false),
                    CategoriaPaiId = table.Column<int>(type: "integer", nullable: true),
                    Ordem = table.Column<int>(type: "integer", nullable: false),
                    DataCriacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    DataAtualizacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Categoria", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Categoria_Categoria_CategoriaPaiId",
                        column: x => x.CategoriaPaiId,
                        principalSchema: "public",
                        principalTable: "Categoria",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Combo",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Nome = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Descricao = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    HectareMinimo = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    HectareMaximo = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    DataInicio = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DataFim = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ModalidadePagamento = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    RestricoesMunicipios = table.Column<JsonDocument>(type: "jsonb", nullable: true),
                    PermiteAlteracaoItem = table.Column<bool>(type: "boolean", nullable: false),
                    PermiteExclusaoItem = table.Column<bool>(type: "boolean", nullable: false),
                    FornecedorId = table.Column<int>(type: "integer", nullable: false),
                    SafraId = table.Column<int>(type: "integer", nullable: false),
                    DataCriacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    DataAtualizacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Combo", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "forma_pagamento",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    descricao = table.Column<string>(type: "character varying(45)", maxLength: 45, nullable: false),
                    ativo = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    data_criacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    data_atualizacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_forma_pagamento", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "Fornecedor",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Nome = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Cnpj = table.Column<string>(type: "character varying(14)", maxLength: 14, nullable: false),
                    InscricaoEstadual = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Endereco = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Telefone = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    Email = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    LogoUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    MoedaPadrao = table.Column<int>(type: "integer", nullable: false),
                    PedidoMinimo = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    TokenLincros = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Ativo = table.Column<bool>(type: "boolean", nullable: false),
                    DadosAdicionais = table.Column<JsonDocument>(type: "jsonb", nullable: true),
                    DataCriacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    DataAtualizacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Fornecedor", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Pedido",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    StatusCarrinho = table.Column<int>(type: "integer", nullable: false),
                    QuantidadeItens = table.Column<int>(type: "integer", nullable: false),
                    Totais = table.Column<JsonDocument>(type: "jsonb", nullable: true),
                    PermiteContato = table.Column<bool>(type: "boolean", nullable: false),
                    NegociarPedido = table.Column<bool>(type: "boolean", nullable: false),
                    DataLimiteInteracao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    FornecedorId = table.Column<int>(type: "integer", nullable: false),
                    ProdutorId = table.Column<int>(type: "integer", nullable: false),
                    DataCriacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    DataAtualizacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Pedido", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PontoDistribuicao",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Nome = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false, collation: "pt_BR"),
                    Descricao = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true, collation: "pt_BR"),
                    Ativo = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    FornecedorId = table.Column<int>(type: "integer", nullable: false),
                    EnderecoId = table.Column<int>(type: "integer", nullable: false),
                    CoberturaTerritorios = table.Column<JsonDocument>(type: "jsonb", nullable: true),
                    RaioCobertura = table.Column<double>(type: "double precision", precision: 10, scale: 2, nullable: true),
                    CapacidadeMaxima = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: true),
                    UnidadeCapacidade = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    HorarioFuncionamento = table.Column<JsonDocument>(type: "jsonb", nullable: true),
                    Observacoes = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true, collation: "pt_BR"),
                    DataCriacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    DataAtualizacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PontoDistribuicao", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PontoDistribuicao_enderecos_EnderecoId",
                        column: x => x.EnderecoId,
                        principalSchema: "public",
                        principalTable: "enderecos",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Propriedade",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Nome = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Nirf = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    InscricaoEstadual = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    AreaTotal = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    ProdutorId = table.Column<int>(type: "integer", nullable: false),
                    EnderecoId = table.Column<int>(type: "integer", nullable: true),
                    DadosAdicionais = table.Column<JsonDocument>(type: "jsonb", nullable: true),
                    DataCriacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    DataAtualizacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Propriedade", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Safra",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PlantioInicial = table.Column<DateTime>(type: "date", nullable: false),
                    PlantioFinal = table.Column<DateTime>(type: "date", nullable: false),
                    PlantioNome = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Descricao = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    AnoColheita = table.Column<int>(type: "integer", nullable: false),
                    DataCriacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    DataAtualizacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Safra", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CatalogoItem",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CatalogoId = table.Column<int>(type: "integer", nullable: false),
                    ProdutoId = table.Column<int>(type: "integer", nullable: false),
                    EstruturaPrecosJson = table.Column<JsonDocument>(type: "jsonb", nullable: false),
                    PrecoBase = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    Ativo = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    DataCriacao = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    DataAtualizacao = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CatalogoItem", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CatalogoItem_Catalogo_CatalogoId",
                        column: x => x.CatalogoId,
                        principalSchema: "public",
                        principalTable: "Catalogo",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Produto",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Nome = table.Column<string>(type: "text", nullable: false),
                    Descricao = table.Column<string>(type: "text", nullable: true),
                    Codigo = table.Column<string>(type: "text", nullable: false),
                    Marca = table.Column<string>(type: "text", nullable: true),
                    Tipo = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    Unidade = table.Column<int>(type: "integer", nullable: false),
                    TipoCalculoPeso = table.Column<int>(type: "integer", nullable: false),
                    ProdutoRestrito = table.Column<bool>(type: "boolean", nullable: false),
                    ObservacoesRestricao = table.Column<string>(type: "text", nullable: true),
                    CategoriaId = table.Column<int>(type: "integer", nullable: false),
                    FornecedorId = table.Column<int>(type: "integer", nullable: false),
                    ProdutoPaiId = table.Column<int>(type: "integer", nullable: true),
                    DadosAdicionais = table.Column<JsonDocument>(type: "jsonb", nullable: true),
                    DataCriacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    DataAtualizacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Produto", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Produto_Categoria_CategoriaId",
                        column: x => x.CategoriaId,
                        principalSchema: "public",
                        principalTable: "Categoria",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Produto_Produto_ProdutoPaiId",
                        column: x => x.ProdutoPaiId,
                        principalSchema: "public",
                        principalTable: "Produto",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "ComboCategoriaDesconto",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ComboId = table.Column<int>(type: "integer", nullable: false),
                    CategoriaId = table.Column<int>(type: "integer", nullable: false),
                    PercentualDesconto = table.Column<decimal>(type: "numeric(5,2)", nullable: false),
                    ValorDescontoFixo = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    DescontoPorHectare = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    TipoDesconto = table.Column<int>(type: "integer", nullable: false),
                    HectareMinimo = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    HectareMaximo = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    Ativo = table.Column<bool>(type: "boolean", nullable: false),
                    DataCriacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    DataAtualizacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ComboCategoriaDesconto", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ComboCategoriaDesconto_Combo_ComboId",
                        column: x => x.ComboId,
                        principalSchema: "public",
                        principalTable: "Combo",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ComboItem",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ComboId = table.Column<int>(type: "integer", nullable: false),
                    ProdutoId = table.Column<int>(type: "integer", nullable: false),
                    Quantidade = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    PrecoUnitario = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    PercentualDesconto = table.Column<decimal>(type: "numeric(5,2)", nullable: false),
                    ProdutoObrigatorio = table.Column<bool>(type: "boolean", nullable: false),
                    Ordem = table.Column<int>(type: "integer", nullable: false),
                    DataCriacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    DataAtualizacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ComboItem", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ComboItem_Combo_ComboId",
                        column: x => x.ComboId,
                        principalSchema: "public",
                        principalTable: "Combo",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ComboLocalRecebimento",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ComboId = table.Column<int>(type: "integer", nullable: false),
                    PontoDistribuicaoId = table.Column<int>(type: "integer", nullable: false),
                    PrecoAdicional = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    PercentualDesconto = table.Column<decimal>(type: "numeric(5,2)", nullable: false),
                    LocalPadrao = table.Column<bool>(type: "boolean", nullable: false),
                    Observacoes = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    DataCriacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    DataAtualizacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ComboLocalRecebimento", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ComboLocalRecebimento_Combo_ComboId",
                        column: x => x.ComboId,
                        principalSchema: "public",
                        principalTable: "Combo",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "cultura_forma_pagamento",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    fornecedor_id = table.Column<int>(type: "integer", nullable: false),
                    cultura_id = table.Column<int>(type: "integer", nullable: false),
                    forma_pagamento_id = table.Column<int>(type: "integer", nullable: false),
                    ativo = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    data_criacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    data_atualizacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_cultura_forma_pagamento", x => x.id);
                    table.ForeignKey(
                        name: "FK_cultura_forma_pagamento_forma_pagamento_forma_pagamento_id",
                        column: x => x.forma_pagamento_id,
                        principalSchema: "public",
                        principalTable: "forma_pagamento",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "UsuarioFornecedor",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UsuarioId = table.Column<int>(type: "integer", nullable: false),
                    FornecedorId = table.Column<int>(type: "integer", nullable: false),
                    Role = table.Column<int>(type: "integer", nullable: false),
                    Ativo = table.Column<bool>(type: "boolean", nullable: false),
                    DataInicio = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DataFim = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DataCriacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    DataAtualizacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UsuarioFornecedor", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UsuarioFornecedor_Fornecedor_FornecedorId",
                        column: x => x.FornecedorId,
                        principalSchema: "public",
                        principalTable: "Fornecedor",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UsuarioFornecedor_usuarios_UsuarioId",
                        column: x => x.UsuarioId,
                        principalSchema: "public",
                        principalTable: "usuarios",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Proposta",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PedidoId = table.Column<int>(type: "integer", nullable: false),
                    AcaoComprador = table.Column<int>(type: "integer", nullable: true),
                    Observacao = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: true),
                    UsuarioProdutorId = table.Column<int>(type: "integer", nullable: true),
                    UsuarioFornecedorId = table.Column<int>(type: "integer", nullable: true),
                    DataCriacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    DataAtualizacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Proposta", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Proposta_Pedido_PedidoId",
                        column: x => x.PedidoId,
                        principalSchema: "public",
                        principalTable: "Pedido",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PropriedadeCultura",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PropriedadeId = table.Column<int>(type: "integer", nullable: false),
                    CulturaId = table.Column<int>(type: "integer", nullable: false),
                    Area = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    SafraId = table.Column<int>(type: "integer", nullable: true),
                    DataPlantio = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DataColheitaPrevista = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Observacoes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    DataCriacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    DataAtualizacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PropriedadeCultura", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PropriedadeCultura_Propriedade_PropriedadeId",
                        column: x => x.PropriedadeId,
                        principalSchema: "public",
                        principalTable: "Propriedade",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Talhao",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Nome = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Area = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    Descricao = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Localizacao = table.Column<Point>(type: "geography(POINT, 4326)", nullable: true),
                    Geometria = table.Column<Polygon>(type: "geography(POLYGON, 4326)", nullable: true),
                    PropriedadeId = table.Column<int>(type: "integer", nullable: false),
                    DataCriacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    DataAtualizacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Talhao", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Talhao_Propriedade_PropriedadeId",
                        column: x => x.PropriedadeId,
                        principalSchema: "public",
                        principalTable: "Propriedade",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PedidoItem",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PedidoId = table.Column<int>(type: "integer", nullable: false),
                    ProdutoId = table.Column<int>(type: "integer", nullable: false),
                    Quantidade = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    PrecoUnitario = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    ValorTotal = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    PercentualDesconto = table.Column<decimal>(type: "numeric(5,2)", nullable: false),
                    ValorDesconto = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    ValorFinal = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    DadosAdicionais = table.Column<JsonDocument>(type: "jsonb", nullable: true),
                    Observacoes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    DataCriacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    DataAtualizacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PedidoItem", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PedidoItem_Pedido_PedidoId",
                        column: x => x.PedidoId,
                        principalSchema: "public",
                        principalTable: "Pedido",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PedidoItem_Produto_ProdutoId",
                        column: x => x.ProdutoId,
                        principalSchema: "public",
                        principalTable: "Produto",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProdutoCultura",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ProdutoId = table.Column<int>(type: "integer", nullable: false),
                    CulturaId = table.Column<int>(type: "integer", nullable: false),
                    Ativo = table.Column<bool>(type: "boolean", nullable: false),
                    Observacoes = table.Column<string>(type: "text", nullable: true),
                    DataCriacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    DataAtualizacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProdutoCultura", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProdutoCultura_Produto_ProdutoId",
                        column: x => x.ProdutoId,
                        principalSchema: "public",
                        principalTable: "Produto",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UsuarioFornecedorTerritorio",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UsuarioFornecedorId = table.Column<int>(type: "integer", nullable: false),
                    Estados = table.Column<JsonDocument>(type: "jsonb", nullable: false),
                    Municipios = table.Column<JsonDocument>(type: "jsonb", nullable: true),
                    TerritorioPadrao = table.Column<bool>(type: "boolean", nullable: false),
                    Ativo = table.Column<bool>(type: "boolean", nullable: false),
                    DataCriacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    DataAtualizacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UsuarioFornecedorTerritorio", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UsuarioFornecedorTerritorio_UsuarioFornecedor_UsuarioFornec~",
                        column: x => x.UsuarioFornecedorId,
                        principalSchema: "public",
                        principalTable: "UsuarioFornecedor",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PedidoItemTransporte",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PedidoItemId = table.Column<int>(type: "integer", nullable: false),
                    Quantidade = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    DataAgendamento = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ValorFrete = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    PesoTotal = table.Column<decimal>(type: "numeric(18,4)", nullable: true),
                    VolumeTotal = table.Column<decimal>(type: "numeric(18,4)", nullable: true),
                    EnderecoOrigem = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    EnderecoDestino = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    InformacoesTransporte = table.Column<JsonDocument>(type: "jsonb", nullable: true),
                    Observacoes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    PedidoItemId1 = table.Column<int>(type: "integer", nullable: true),
                    DataCriacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    DataAtualizacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PedidoItemTransporte", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PedidoItemTransporte_PedidoItem_PedidoItemId",
                        column: x => x.PedidoItemId,
                        principalSchema: "public",
                        principalTable: "PedidoItem",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PedidoItemTransporte_PedidoItem_PedidoItemId1",
                        column: x => x.PedidoItemId1,
                        principalSchema: "public",
                        principalTable: "PedidoItem",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Catalogo_CategoriaId",
                schema: "public",
                table: "Catalogo",
                column: "CategoriaId");

            migrationBuilder.CreateIndex(
                name: "IX_Catalogo_ChaveUnica",
                schema: "public",
                table: "Catalogo",
                columns: new[] { "SafraId", "PontoDistribuicaoId", "CulturaId", "CategoriaId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Catalogo_CulturaId",
                schema: "public",
                table: "Catalogo",
                column: "CulturaId");

            migrationBuilder.CreateIndex(
                name: "IX_Catalogo_PontoDistribuicaoId",
                schema: "public",
                table: "Catalogo",
                column: "PontoDistribuicaoId");

            migrationBuilder.CreateIndex(
                name: "IX_Catalogo_SafraId",
                schema: "public",
                table: "Catalogo",
                column: "SafraId");

            migrationBuilder.CreateIndex(
                name: "IX_Catalogo_Vigencia",
                schema: "public",
                table: "Catalogo",
                columns: new[] { "DataInicio", "DataFim", "Ativo" });

            migrationBuilder.CreateIndex(
                name: "IX_Catalogos_DataCriacao",
                schema: "public",
                table: "Catalogo",
                column: "DataCriacao");

            migrationBuilder.CreateIndex(
                name: "IX_CatalogoItem_Ativo",
                schema: "public",
                table: "CatalogoItem",
                column: "Ativo");

            migrationBuilder.CreateIndex(
                name: "IX_CatalogoItem_CatalogoId",
                schema: "public",
                table: "CatalogoItem",
                column: "CatalogoId");

            migrationBuilder.CreateIndex(
                name: "IX_CatalogoItem_CatalogoProduto",
                schema: "public",
                table: "CatalogoItem",
                columns: new[] { "CatalogoId", "ProdutoId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CatalogoItem_ProdutoId",
                schema: "public",
                table: "CatalogoItem",
                column: "ProdutoId");

            migrationBuilder.CreateIndex(
                name: "IX_CatalogoItens_DataCriacao",
                schema: "public",
                table: "CatalogoItem",
                column: "DataCriacao");

            migrationBuilder.CreateIndex(
                name: "IX_Categoria_CategoriaPaiId",
                schema: "public",
                table: "Categoria",
                column: "CategoriaPaiId");

            migrationBuilder.CreateIndex(
                name: "IX_Categoria_DataCriacao",
                schema: "public",
                table: "Categoria",
                column: "DataCriacao");

            migrationBuilder.CreateIndex(
                name: "IX_Combo_FornecedorId",
                schema: "public",
                table: "Combo",
                column: "FornecedorId");

            migrationBuilder.CreateIndex(
                name: "IX_Combo_FornecedorSafraNome",
                schema: "public",
                table: "Combo",
                columns: new[] { "FornecedorId", "SafraId", "Nome" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Combo_Periodo",
                schema: "public",
                table: "Combo",
                columns: new[] { "DataInicio", "DataFim" });

            migrationBuilder.CreateIndex(
                name: "IX_Combo_SafraId",
                schema: "public",
                table: "Combo",
                column: "SafraId");

            migrationBuilder.CreateIndex(
                name: "IX_Combo_Status",
                schema: "public",
                table: "Combo",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_Combos_DataCriacao",
                schema: "public",
                table: "Combo",
                column: "DataCriacao");

            migrationBuilder.CreateIndex(
                name: "IX_ComboCategoriaDesconto_Ativo",
                schema: "public",
                table: "ComboCategoriaDesconto",
                column: "Ativo");

            migrationBuilder.CreateIndex(
                name: "IX_ComboCategoriaDesconto_CategoriaId",
                schema: "public",
                table: "ComboCategoriaDesconto",
                column: "CategoriaId");

            migrationBuilder.CreateIndex(
                name: "IX_ComboCategoriaDesconto_ComboCategoria",
                schema: "public",
                table: "ComboCategoriaDesconto",
                columns: new[] { "ComboId", "CategoriaId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ComboCategoriaDesconto_ComboId",
                schema: "public",
                table: "ComboCategoriaDesconto",
                column: "ComboId");

            migrationBuilder.CreateIndex(
                name: "IX_ComboCategoriasDesconto_DataCriacao",
                schema: "public",
                table: "ComboCategoriaDesconto",
                column: "DataCriacao");

            migrationBuilder.CreateIndex(
                name: "IX_ComboItem_ComboId",
                schema: "public",
                table: "ComboItem",
                column: "ComboId");

            migrationBuilder.CreateIndex(
                name: "IX_ComboItem_ComboOrdem",
                schema: "public",
                table: "ComboItem",
                columns: new[] { "ComboId", "Ordem" });

            migrationBuilder.CreateIndex(
                name: "IX_ComboItem_ProdutoId",
                schema: "public",
                table: "ComboItem",
                column: "ProdutoId");

            migrationBuilder.CreateIndex(
                name: "IX_ComboItens_DataCriacao",
                schema: "public",
                table: "ComboItem",
                column: "DataCriacao");

            migrationBuilder.CreateIndex(
                name: "IX_ComboLocaisRecebimento_DataCriacao",
                schema: "public",
                table: "ComboLocalRecebimento",
                column: "DataCriacao");

            migrationBuilder.CreateIndex(
                name: "IX_ComboLocalRecebimento_ComboId",
                schema: "public",
                table: "ComboLocalRecebimento",
                column: "ComboId");

            migrationBuilder.CreateIndex(
                name: "IX_ComboLocalRecebimento_ComboPonto",
                schema: "public",
                table: "ComboLocalRecebimento",
                columns: new[] { "ComboId", "PontoDistribuicaoId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ComboLocalRecebimento_PontoDistribuicaoId",
                schema: "public",
                table: "ComboLocalRecebimento",
                column: "PontoDistribuicaoId");

            migrationBuilder.CreateIndex(
                name: "ix_cultura_forma_pagamento_ativo",
                schema: "public",
                table: "cultura_forma_pagamento",
                column: "ativo");

            migrationBuilder.CreateIndex(
                name: "ix_cultura_forma_pagamento_cultura_id",
                schema: "public",
                table: "cultura_forma_pagamento",
                column: "cultura_id");

            migrationBuilder.CreateIndex(
                name: "ix_cultura_forma_pagamento_forma_pagamento_id",
                schema: "public",
                table: "cultura_forma_pagamento",
                column: "forma_pagamento_id");

            migrationBuilder.CreateIndex(
                name: "ix_cultura_forma_pagamento_fornecedor_id",
                schema: "public",
                table: "cultura_forma_pagamento",
                column: "fornecedor_id");

            migrationBuilder.CreateIndex(
                name: "ix_cultura_forma_pagamento_unique",
                schema: "public",
                table: "cultura_forma_pagamento",
                columns: new[] { "fornecedor_id", "cultura_id", "forma_pagamento_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CulturaFormasPagamento_DataCriacao",
                schema: "public",
                table: "cultura_forma_pagamento",
                column: "data_criacao");

            migrationBuilder.CreateIndex(
                name: "ix_forma_pagamento_ativo",
                schema: "public",
                table: "forma_pagamento",
                column: "ativo");

            migrationBuilder.CreateIndex(
                name: "ix_forma_pagamento_descricao",
                schema: "public",
                table: "forma_pagamento",
                column: "descricao");

            migrationBuilder.CreateIndex(
                name: "IX_FormasPagamento_DataCriacao",
                schema: "public",
                table: "forma_pagamento",
                column: "data_criacao");

            migrationBuilder.CreateIndex(
                name: "IX_Fornecedor_Ativo",
                schema: "public",
                table: "Fornecedor",
                column: "Ativo");

            migrationBuilder.CreateIndex(
                name: "IX_Fornecedor_Cnpj",
                schema: "public",
                table: "Fornecedor",
                column: "Cnpj",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Fornecedor_DataCriacao",
                schema: "public",
                table: "Fornecedor",
                column: "DataCriacao");

            migrationBuilder.CreateIndex(
                name: "IX_Fornecedor_Email",
                schema: "public",
                table: "Fornecedor",
                column: "Email",
                filter: "\"Email\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Fornecedor_MoedaPadrao",
                schema: "public",
                table: "Fornecedor",
                column: "MoedaPadrao");

            migrationBuilder.CreateIndex(
                name: "IX_Fornecedor_Nome",
                schema: "public",
                table: "Fornecedor",
                column: "Nome");

            migrationBuilder.CreateIndex(
                name: "IX_Pedido_DataLimiteInteracao",
                schema: "public",
                table: "Pedido",
                column: "DataLimiteInteracao");

            migrationBuilder.CreateIndex(
                name: "IX_Pedido_FornecedorId",
                schema: "public",
                table: "Pedido",
                column: "FornecedorId");

            migrationBuilder.CreateIndex(
                name: "IX_Pedido_ProdutorId",
                schema: "public",
                table: "Pedido",
                column: "ProdutorId");

            migrationBuilder.CreateIndex(
                name: "IX_Pedido_ProdutorId_FornecedorId",
                schema: "public",
                table: "Pedido",
                columns: new[] { "ProdutorId", "FornecedorId" });

            migrationBuilder.CreateIndex(
                name: "IX_Pedido_Status",
                schema: "public",
                table: "Pedido",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_Pedidos_DataCriacao",
                schema: "public",
                table: "Pedido",
                column: "DataCriacao");

            migrationBuilder.CreateIndex(
                name: "IX_PedidoItem_PedidoId",
                schema: "public",
                table: "PedidoItem",
                column: "PedidoId");

            migrationBuilder.CreateIndex(
                name: "IX_PedidoItem_ProdutoId",
                schema: "public",
                table: "PedidoItem",
                column: "ProdutoId");

            migrationBuilder.CreateIndex(
                name: "IX_PedidoItem_ValorFinal",
                schema: "public",
                table: "PedidoItem",
                column: "ValorFinal");

            migrationBuilder.CreateIndex(
                name: "IX_PedidoItens_DataCriacao",
                schema: "public",
                table: "PedidoItem",
                column: "DataCriacao");

            migrationBuilder.CreateIndex(
                name: "IX_PedidoItemTransporte_DataAgendamento",
                schema: "public",
                table: "PedidoItemTransporte",
                column: "DataAgendamento");

            migrationBuilder.CreateIndex(
                name: "IX_PedidoItemTransporte_PedidoItemId",
                schema: "public",
                table: "PedidoItemTransporte",
                column: "PedidoItemId");

            migrationBuilder.CreateIndex(
                name: "IX_PedidoItemTransporte_PedidoItemId1",
                schema: "public",
                table: "PedidoItemTransporte",
                column: "PedidoItemId1");

            migrationBuilder.CreateIndex(
                name: "IX_PedidoItemTransporte_ValorFrete",
                schema: "public",
                table: "PedidoItemTransporte",
                column: "ValorFrete");

            migrationBuilder.CreateIndex(
                name: "IX_PedidoItensTransporte_DataCriacao",
                schema: "public",
                table: "PedidoItemTransporte",
                column: "DataCriacao");

            migrationBuilder.CreateIndex(
                name: "IX_PontoDistribuicao_Ativo",
                schema: "public",
                table: "PontoDistribuicao",
                column: "Ativo");

            migrationBuilder.CreateIndex(
                name: "IX_PontoDistribuicao_CoberturaTerritorios",
                schema: "public",
                table: "PontoDistribuicao",
                column: "CoberturaTerritorios")
                .Annotation("Npgsql:IndexMethod", "gin");

            migrationBuilder.CreateIndex(
                name: "IX_PontoDistribuicao_EnderecoId",
                schema: "public",
                table: "PontoDistribuicao",
                column: "EnderecoId");

            migrationBuilder.CreateIndex(
                name: "IX_PontoDistribuicao_FornecedorId",
                schema: "public",
                table: "PontoDistribuicao",
                column: "FornecedorId");

            migrationBuilder.CreateIndex(
                name: "IX_PontoDistribuicao_FornecedorId_Nome",
                schema: "public",
                table: "PontoDistribuicao",
                columns: new[] { "FornecedorId", "Nome" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PontosDistribuicao_DataCriacao",
                schema: "public",
                table: "PontoDistribuicao",
                column: "DataCriacao");

            migrationBuilder.CreateIndex(
                name: "IX_Produto_CategoriaId",
                schema: "public",
                table: "Produto",
                column: "CategoriaId");

            migrationBuilder.CreateIndex(
                name: "IX_Produto_DataCriacao",
                schema: "public",
                table: "Produto",
                column: "DataCriacao");

            migrationBuilder.CreateIndex(
                name: "IX_Produto_ProdutoPaiId",
                schema: "public",
                table: "Produto",
                column: "ProdutoPaiId");

            migrationBuilder.CreateIndex(
                name: "IX_ProdutoCultura_DataCriacao",
                schema: "public",
                table: "ProdutoCultura",
                column: "DataCriacao");

            migrationBuilder.CreateIndex(
                name: "IX_ProdutoCultura_ProdutoId",
                schema: "public",
                table: "ProdutoCultura",
                column: "ProdutoId");

            migrationBuilder.CreateIndex(
                name: "IX_Proposta_DataCriacao",
                schema: "public",
                table: "Proposta",
                column: "DataCriacao");

            migrationBuilder.CreateIndex(
                name: "IX_Proposta_PedidoId",
                schema: "public",
                table: "Proposta",
                column: "PedidoId");

            migrationBuilder.CreateIndex(
                name: "IX_Proposta_UsuarioFornecedorId",
                schema: "public",
                table: "Proposta",
                column: "UsuarioFornecedorId");

            migrationBuilder.CreateIndex(
                name: "IX_Proposta_UsuarioProdutorId",
                schema: "public",
                table: "Proposta",
                column: "UsuarioProdutorId");

            migrationBuilder.CreateIndex(
                name: "IX_Propriedade_EnderecoId",
                schema: "public",
                table: "Propriedade",
                column: "EnderecoId");

            migrationBuilder.CreateIndex(
                name: "IX_Propriedade_Nome",
                schema: "public",
                table: "Propriedade",
                column: "Nome");

            migrationBuilder.CreateIndex(
                name: "IX_Propriedade_ProdutorId",
                schema: "public",
                table: "Propriedade",
                column: "ProdutorId");

            migrationBuilder.CreateIndex(
                name: "IX_Propriedades_DataCriacao",
                schema: "public",
                table: "Propriedade",
                column: "DataCriacao");

            migrationBuilder.CreateIndex(
                name: "IX_PropriedadeCultura_CulturaId",
                schema: "public",
                table: "PropriedadeCultura",
                column: "CulturaId");

            migrationBuilder.CreateIndex(
                name: "IX_PropriedadeCultura_PeriodoPlantio",
                schema: "public",
                table: "PropriedadeCultura",
                columns: new[] { "DataPlantio", "DataColheitaPrevista" });

            migrationBuilder.CreateIndex(
                name: "IX_PropriedadeCultura_PropriedadeId",
                schema: "public",
                table: "PropriedadeCultura",
                column: "PropriedadeId");

            migrationBuilder.CreateIndex(
                name: "IX_PropriedadeCultura_PropriedadeId_CulturaId",
                schema: "public",
                table: "PropriedadeCultura",
                columns: new[] { "PropriedadeId", "CulturaId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PropriedadeCultura_SafraId",
                schema: "public",
                table: "PropriedadeCultura",
                column: "SafraId");

            migrationBuilder.CreateIndex(
                name: "IX_PropriedadeCulturas_DataCriacao",
                schema: "public",
                table: "PropriedadeCultura",
                column: "DataCriacao");

            migrationBuilder.CreateIndex(
                name: "IX_Safra_AnoColheita",
                schema: "public",
                table: "Safra",
                column: "AnoColheita");

            migrationBuilder.CreateIndex(
                name: "IX_Safra_Atual",
                schema: "public",
                table: "Safra",
                columns: new[] { "PlantioNome", "PlantioInicial", "PlantioFinal" },
                unique: true,
                filter: "PlantioNome = 'S1'");

            migrationBuilder.CreateIndex(
                name: "IX_Safra_PlantioFinal",
                schema: "public",
                table: "Safra",
                column: "PlantioFinal");

            migrationBuilder.CreateIndex(
                name: "IX_Safra_PlantioInicial",
                schema: "public",
                table: "Safra",
                column: "PlantioInicial");

            migrationBuilder.CreateIndex(
                name: "IX_Safras_DataCriacao",
                schema: "public",
                table: "Safra",
                column: "DataCriacao");

            migrationBuilder.CreateIndex(
                name: "IX_Talhao_Geometria",
                schema: "public",
                table: "Talhao",
                column: "Geometria")
                .Annotation("Npgsql:IndexMethod", "gist");

            migrationBuilder.CreateIndex(
                name: "IX_Talhao_Localizacao",
                schema: "public",
                table: "Talhao",
                column: "Localizacao")
                .Annotation("Npgsql:IndexMethod", "gist");

            migrationBuilder.CreateIndex(
                name: "IX_Talhao_Nome",
                schema: "public",
                table: "Talhao",
                column: "Nome");

            migrationBuilder.CreateIndex(
                name: "IX_Talhao_PropriedadeId",
                schema: "public",
                table: "Talhao",
                column: "PropriedadeId");

            migrationBuilder.CreateIndex(
                name: "IX_Talhoes_DataCriacao",
                schema: "public",
                table: "Talhao",
                column: "DataCriacao");

            migrationBuilder.CreateIndex(
                name: "IX_UsuarioFornecedor_Ativo",
                schema: "public",
                table: "UsuarioFornecedor",
                column: "Ativo");

            migrationBuilder.CreateIndex(
                name: "IX_UsuarioFornecedor_DataCriacao",
                schema: "public",
                table: "UsuarioFornecedor",
                column: "DataCriacao");

            migrationBuilder.CreateIndex(
                name: "IX_UsuarioFornecedor_DataInicio",
                schema: "public",
                table: "UsuarioFornecedor",
                column: "DataInicio");

            migrationBuilder.CreateIndex(
                name: "IX_UsuarioFornecedor_FornecedorId",
                schema: "public",
                table: "UsuarioFornecedor",
                column: "FornecedorId");

            migrationBuilder.CreateIndex(
                name: "IX_UsuarioFornecedor_Role",
                schema: "public",
                table: "UsuarioFornecedor",
                column: "Role");

            migrationBuilder.CreateIndex(
                name: "IX_UsuarioFornecedor_Usuario_Fornecedor",
                schema: "public",
                table: "UsuarioFornecedor",
                columns: new[] { "UsuarioId", "FornecedorId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UsuarioFornecedor_UsuarioId",
                schema: "public",
                table: "UsuarioFornecedor",
                column: "UsuarioId");

            migrationBuilder.CreateIndex(
                name: "IX_UsuarioFornecedorTerritorio_Ativo",
                schema: "public",
                table: "UsuarioFornecedorTerritorio",
                column: "Ativo");

            migrationBuilder.CreateIndex(
                name: "IX_UsuarioFornecedorTerritorio_DataCriacao",
                schema: "public",
                table: "UsuarioFornecedorTerritorio",
                column: "DataCriacao");

            migrationBuilder.CreateIndex(
                name: "IX_UsuarioFornecedorTerritorio_Estados_GIN",
                schema: "public",
                table: "UsuarioFornecedorTerritorio",
                column: "Estados")
                .Annotation("Npgsql:IndexMethod", "gin");

            migrationBuilder.CreateIndex(
                name: "IX_UsuarioFornecedorTerritorio_Municipios_GIN",
                schema: "public",
                table: "UsuarioFornecedorTerritorio",
                column: "Municipios",
                filter: "\"Municipios\" IS NOT NULL")
                .Annotation("Npgsql:IndexMethod", "gin");

            migrationBuilder.CreateIndex(
                name: "IX_UsuarioFornecedorTerritorio_TerritorioPadrao",
                schema: "public",
                table: "UsuarioFornecedorTerritorio",
                column: "TerritorioPadrao");

            migrationBuilder.CreateIndex(
                name: "IX_UsuarioFornecedorTerritorio_UsuarioFornecedorId",
                schema: "public",
                table: "UsuarioFornecedorTerritorio",
                column: "UsuarioFornecedorId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CatalogoItem",
                schema: "public");

            migrationBuilder.DropTable(
                name: "ComboCategoriaDesconto",
                schema: "public");

            migrationBuilder.DropTable(
                name: "ComboItem",
                schema: "public");

            migrationBuilder.DropTable(
                name: "ComboLocalRecebimento",
                schema: "public");

            migrationBuilder.DropTable(
                name: "cultura_forma_pagamento",
                schema: "public");

            migrationBuilder.DropTable(
                name: "PedidoItemTransporte",
                schema: "public");

            migrationBuilder.DropTable(
                name: "PontoDistribuicao",
                schema: "public");

            migrationBuilder.DropTable(
                name: "ProdutoCultura",
                schema: "public");

            migrationBuilder.DropTable(
                name: "Proposta",
                schema: "public");

            migrationBuilder.DropTable(
                name: "PropriedadeCultura",
                schema: "public");

            migrationBuilder.DropTable(
                name: "Safra",
                schema: "public");

            migrationBuilder.DropTable(
                name: "Talhao",
                schema: "public");

            migrationBuilder.DropTable(
                name: "UsuarioFornecedorTerritorio",
                schema: "public");

            migrationBuilder.DropTable(
                name: "Catalogo",
                schema: "public");

            migrationBuilder.DropTable(
                name: "Combo",
                schema: "public");

            migrationBuilder.DropTable(
                name: "forma_pagamento",
                schema: "public");

            migrationBuilder.DropTable(
                name: "PedidoItem",
                schema: "public");

            migrationBuilder.DropTable(
                name: "Propriedade",
                schema: "public");

            migrationBuilder.DropTable(
                name: "UsuarioFornecedor",
                schema: "public");

            migrationBuilder.DropTable(
                name: "Pedido",
                schema: "public");

            migrationBuilder.DropTable(
                name: "Produto",
                schema: "public");

            migrationBuilder.DropTable(
                name: "Fornecedor",
                schema: "public");

            migrationBuilder.DropTable(
                name: "Categoria",
                schema: "public");
        }
    }
}
