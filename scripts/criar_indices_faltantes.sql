-- Script para criar índices faltantes no banco de dados
-- Data: 2025-01-27
-- Objetivo: Sincronizar índices do banco com as configurações do Entity Framework

-- =====================================================
-- ÍNDICES FALTANTES NA TABELA "Produto"
-- =====================================================

-- Índices definidos no EF mas não existentes no DDL
CREATE INDEX IF NOT EXISTS "IX_Produtos_Codigo" ON public."Produto" USING btree ("Codigo");
CREATE INDEX IF NOT EXISTS "IX_Produtos_Nome" ON public."Produto" USING btree ("Nome");
CREATE INDEX IF NOT EXISTS "IX_Produtos_FornecedorId" ON public."Produto" USING btree ("FornecedorId");
CREATE INDEX IF NOT EXISTS "IX_Produtos_Status" ON public."Produto" USING btree ("Status");
CREATE INDEX IF NOT EXISTS "IX_Produtos_Tipo" ON public."Produto" USING btree ("Tipo");
CREATE INDEX IF NOT EXISTS "IX_Produtos_ProdutoRestrito" ON public."Produto" USING btree ("ProdutoRestrito");
CREATE INDEX IF NOT EXISTS "IX_Produtos_EmbalagemId" ON public."Produto" USING btree ("EmbalagemId");
CREATE INDEX IF NOT EXISTS "IX_Produtos_AtividadeAgropecuariaId" ON public."Produto" USING btree ("AtividadeAgropecuariaId");

-- Índice único para código do produto (definido no EF)
CREATE UNIQUE INDEX IF NOT EXISTS "IX_Produtos_Codigo_Unique" ON public."Produto" USING btree ("Codigo");

-- =====================================================
-- ÍNDICES FALTANTES NA TABELA "Fornecedor"
-- =====================================================

-- Índices definidos no EF mas não existentes no DDL
CREATE INDEX IF NOT EXISTS "IX_Fornecedor_UfId" ON public."Fornecedor" USING btree ("UfId");
CREATE INDEX IF NOT EXISTS "IX_Fornecedor_MunicipioId" ON public."Fornecedor" USING btree ("MunicipioId");

-- =====================================================
-- ÍNDICES FALTANTES NA TABELA usuarios
-- =====================================================

-- Índice para DataCriacao (definido automaticamente pelo DbContext)
CREATE INDEX IF NOT EXISTS "IX_Usuarios_DataCriacao" ON public.usuarios USING btree (data_criacao);

-- =====================================================
-- ÍNDICES FALTANTES NA TABELA usuario_roles
-- =====================================================

-- Índice para DataCriacao (definido automaticamente pelo DbContext)
CREATE INDEX IF NOT EXISTS "IX_UsuarioRoles_DataCriacao" ON public.usuario_roles USING btree (data_criacao);

-- =====================================================
-- ÍNDICES FALTANTES NA TABELA refresh_tokens
-- =====================================================

-- Índice para DataCriacao (definido automaticamente pelo DbContext)
CREATE INDEX IF NOT EXISTS "IX_RefreshTokens_DataCriacao" ON public.refresh_tokens USING btree (data_criacao);

-- =====================================================
-- ÍNDICES FALTANTES NA TABELA "Cultura"
-- =====================================================

-- Índice para DataCriacao (definido automaticamente pelo DbContext)
CREATE INDEX IF NOT EXISTS "IX_Culturas_DataCriacao" ON public."Cultura" USING btree ("DataCriacao");

-- =====================================================
-- ÍNDICES FALTANTES NA TABELA "Safra"
-- =====================================================

-- Índice para DataCriacao (definido automaticamente pelo DbContext)
CREATE INDEX IF NOT EXISTS "IX_Safras_DataCriacao" ON public."Safra" USING btree ("DataCriacao");

-- =====================================================
-- ÍNDICES FALTANTES NA TABELA "Catalogo"
-- =====================================================

-- Índice para DataCriacao (definido automaticamente pelo DbContext)
CREATE INDEX IF NOT EXISTS "IX_Catalogos_DataCriacao" ON public."Catalogo" USING btree ("DataCriacao");

-- =====================================================
-- ÍNDICES FALTANTES NA TABELA "CatalogoItem"
-- =====================================================

-- Índice para DataCriacao (definido automaticamente pelo DbContext)
CREATE INDEX IF NOT EXISTS "IX_CatalogoItens_DataCriacao" ON public."CatalogoItem" USING btree ("DataCriacao");

-- =====================================================
-- ÍNDICES FALTANTES NA TABELA "Categoria"
-- =====================================================

-- Índice para DataCriacao (definido automaticamente pelo DbContext)
CREATE INDEX IF NOT EXISTS "IX_Categoria_DataCriacao" ON public."Categoria" USING btree ("DataCriacao");

-- =====================================================
-- ÍNDICES FALTANTES NA TABELA "Pedido"
-- =====================================================

-- Índice para DataCriacao (definido automaticamente pelo DbContext)
CREATE INDEX IF NOT EXISTS "IX_Pedidos_DataCriacao" ON public."Pedido" USING btree ("DataCriacao");

-- =====================================================
-- ÍNDICES FALTANTES NA TABELA "PedidoItem"
-- =====================================================

-- Índice para DataCriacao (definido automaticamente pelo DbContext)
CREATE INDEX IF NOT EXISTS "IX_PedidoItens_DataCriacao" ON public."PedidoItem" USING btree ("DataCriacao");

-- =====================================================
-- ÍNDICES FALTANTES NA TABELA "PedidoItemTransporte"
-- =====================================================

-- Índice para DataCriacao (definido automaticamente pelo DbContext)
CREATE INDEX IF NOT EXISTS "IX_PedidoItensTransporte_DataCriacao" ON public."PedidoItemTransporte" USING btree ("DataCriacao");

-- =====================================================
-- ÍNDICES FALTANTES NA TABELA "Proposta"
-- =====================================================

-- Índice para DataCriacao (definido automaticamente pelo DbContext)
CREATE INDEX IF NOT EXISTS "IX_Proposta_DataCriacao" ON public."Proposta" USING btree ("DataCriacao");

-- =====================================================
-- ÍNDICES FALTANTES NA TABELA "Propriedade"
-- =====================================================

-- Índice para DataCriacao (definido automaticamente pelo DbContext)
CREATE INDEX IF NOT EXISTS "IX_Propriedades_DataCriacao" ON public."Propriedade" USING btree ("DataCriacao");

-- =====================================================
-- ÍNDICES FALTANTES NA TABELA "PropriedadeCultura"
-- =====================================================

-- Índice para DataCriacao (definido automaticamente pelo DbContext)
CREATE INDEX IF NOT EXISTS "IX_PropriedadeCulturas_DataCriacao" ON public."PropriedadeCultura" USING btree ("DataCriacao");

-- =====================================================
-- ÍNDICES FALTANTES NA TABELA "Talhao"
-- =====================================================

-- Índice para DataCriacao (definido automaticamente pelo DbContext)
CREATE INDEX IF NOT EXISTS "IX_Talhao_DataCriacao" ON public."Talhao" USING btree ("DataCriacao");

-- =====================================================
-- ÍNDICES FALTANTES NA TABELA "UsuarioProdutor"
-- =====================================================

-- Índice para DataCriacao (definido automaticamente pelo DbContext)
CREATE INDEX IF NOT EXISTS "IX_UsuariosProdutores_DataCriacao" ON public."UsuarioProdutor" USING btree ("DataCriacao");

-- =====================================================
-- ÍNDICES FALTANTES NA TABELA "PontoDistribuicao"
-- =====================================================

-- Índice para DataCriacao (definido automaticamente pelo DbContext)
CREATE INDEX IF NOT EXISTS "IX_PontoDistribuicao_DataCriacao" ON public."PontoDistribuicao" USING btree ("DataCriacao");

-- =====================================================
-- ÍNDICES FALTANTES NA TABELA "UsuarioFornecedor"
-- =====================================================

-- Índice para DataCriacao (definido automaticamente pelo DbContext)
CREATE INDEX IF NOT EXISTS "IX_UsuarioFornecedor_DataCriacao" ON public."UsuarioFornecedor" USING btree ("DataCriacao");

-- =====================================================
-- ÍNDICES FALTANTES NA TABELA "UsuarioFornecedorTerritorio"
-- =====================================================

-- Índice para DataCriacao (definido automaticamente pelo DbContext)
CREATE INDEX IF NOT EXISTS "IX_UsuarioFornecedorTerritorio_DataCriacao" ON public."UsuarioFornecedorTerritorio" USING btree ("DataCriacao");

-- =====================================================
-- ÍNDICES FALTANTES NA TABELA "Combo"
-- =====================================================

-- Índice para DataCriacao (definido automaticamente pelo DbContext)
CREATE INDEX IF NOT EXISTS "IX_Combos_DataCriacao" ON public."Combo" USING btree ("DataCriacao");

-- =====================================================
-- ÍNDICES FALTANTES NA TABELA "ComboItem"
-- =====================================================

-- Índice para DataCriacao (definido automaticamente pelo DbContext)
CREATE INDEX IF NOT EXISTS "IX_ComboItens_DataCriacao" ON public."ComboItem" USING btree ("DataCriacao");

-- =====================================================
-- ÍNDICES FALTANTES NA TABELA "ComboLocalRecebimento"
-- =====================================================

-- Índice para DataCriacao (definido automaticamente pelo DbContext)
CREATE INDEX IF NOT EXISTS "IX_ComboLocaisRecebimento_DataCriacao" ON public."ComboLocalRecebimento" USING btree ("DataCriacao");

-- =====================================================
-- ÍNDICES FALTANTES NA TABELA "ComboCategoriaDesconto"
-- =====================================================

-- Índice para DataCriacao (definido automaticamente pelo DbContext)
CREATE INDEX IF NOT EXISTS "IX_ComboCategoriasDesconto_DataCriacao" ON public."ComboCategoriaDesconto" USING btree ("DataCriacao");

-- =====================================================
-- ÍNDICES FALTANTES NA TABELA "ProdutoCultura"
-- =====================================================

-- Índice para DataCriacao (definido automaticamente pelo DbContext)
CREATE INDEX IF NOT EXISTS "IX_ProdutoCultura_DataCriacao" ON public."ProdutoCultura" USING btree ("DataCriacao");

-- =====================================================
-- ÍNDICES FALTANTES NAS TABELAS DE REFERÊNCIA
-- =====================================================

-- Tabela "Moedas"
CREATE INDEX IF NOT EXISTS "IX_Moedas_DataCriacao" ON public."Moedas" USING btree ("DataCriacao");

-- Tabela "AtividadesAgropecuarias"
CREATE INDEX IF NOT EXISTS "IX_AtividadesAgropecuarias_DataCriacao" ON public."AtividadesAgropecuarias" USING btree ("DataCriacao");

-- Tabela "UnidadesMedida"
CREATE INDEX IF NOT EXISTS "IX_UnidadesMedida_DataCriacao" ON public."UnidadesMedida" USING btree ("DataCriacao");

-- Tabela "Embalagens"
CREATE INDEX IF NOT EXISTS "IX_Embalagens_DataCriacao" ON public."Embalagens" USING btree ("DataCriacao");

-- =====================================================
-- ÍNDICES FALTANTES NAS TABELAS DE ENDEREÇOS
-- =====================================================

-- Tabela estados
CREATE INDEX IF NOT EXISTS "IX_Estados_DataCriacao" ON public.estados USING btree (data_criacao);

-- Tabela municipios
CREATE INDEX IF NOT EXISTS "IX_Municipios_DataCriacao" ON public.municipios USING btree (data_criacao);

-- Tabela enderecos
CREATE INDEX IF NOT EXISTS "IX_Enderecos_DataCriacao" ON public.enderecos USING btree (data_criacao);

-- =====================================================
-- ÍNDICES FALTANTES NAS TABELAS DE PAGAMENTOS
-- =====================================================

-- Tabela forma_pagamento
CREATE INDEX IF NOT EXISTS "IX_FormasPagamento_DataCriacao" ON public.forma_pagamento USING btree (data_criacao);

-- Tabela cultura_forma_pagamento
CREATE INDEX IF NOT EXISTS "IX_CulturaFormasPagamento_DataCriacao" ON public.cultura_forma_pagamento USING btree (data_criacao);

-- =====================================================
-- COMENTÁRIOS PARA DOCUMENTAÇÃO
-- =====================================================

COMMENT ON INDEX "IX_Produtos_Codigo" IS 'Índice para otimizar consultas por código do produto';
COMMENT ON INDEX "IX_Produtos_Nome" IS 'Índice para otimizar consultas por nome do produto';
COMMENT ON INDEX "IX_Produtos_FornecedorId" IS 'Índice para otimizar consultas por fornecedor';
COMMENT ON INDEX "IX_Produtos_Status" IS 'Índice para otimizar consultas por status do produto';
COMMENT ON INDEX "IX_Produtos_Tipo" IS 'Índice para otimizar consultas por tipo do produto';

-- =====================================================
-- VERIFICAÇÃO DOS ÍNDICES CRIADOS
-- =====================================================

-- Consulta para verificar os índices criados na tabela Produto
SELECT 
    schemaname,
    tablename,
    indexname,
    indexdef
FROM pg_indexes 
WHERE tablename = 'Produto' 
    AND schemaname = 'public'
    AND indexname LIKE 'IX_Produtos_%'
ORDER BY indexname;

-- Consulta para verificar todos os índices de DataCriacao criados
SELECT 
    schemaname,
    tablename,
    indexname,
    indexdef
FROM pg_indexes 
WHERE indexname LIKE '%DataCriacao%'
    AND schemaname = 'public'
ORDER BY tablename, indexname;