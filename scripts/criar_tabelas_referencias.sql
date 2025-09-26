-- Script para criar tabelas de referências
-- Executar este script manualmente no banco de dados

-- =====================================================
-- TABELA: Moedas
-- =====================================================
CREATE TABLE IF NOT EXISTS public."Moedas" (
    "Id" SERIAL PRIMARY KEY,
    "Codigo" VARCHAR(3) NOT NULL,
    "Nome" VARCHAR(100) NOT NULL,
    "Simbolo" VARCHAR(5) NOT NULL,
    "Ativo" BOOLEAN NOT NULL DEFAULT true,
    "DataCriacao" TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "DataAtualizacao" TIMESTAMP WITH TIME ZONE NULL
);

-- Índices únicos para Moedas
CREATE UNIQUE INDEX IF NOT EXISTS "IX_Moedas_Codigo_Unique" ON public."Moedas" ("Codigo");
CREATE UNIQUE INDEX IF NOT EXISTS "IX_Moedas_Nome_Unique" ON public."Moedas" ("Nome");
CREATE UNIQUE INDEX IF NOT EXISTS "IX_Moedas_Simbolo_Unique" ON public."Moedas" ("Simbolo");
CREATE INDEX IF NOT EXISTS "IX_Moedas_DataCriacao" ON public."Moedas" ("DataCriacao");

-- =====================================================
-- TABELA: AtividadesAgropecuarias
-- =====================================================
CREATE TABLE IF NOT EXISTS public."AtividadesAgropecuarias" (
    "Id" SERIAL PRIMARY KEY,
    "Codigo" VARCHAR(20) NOT NULL,
    "Descricao" VARCHAR(200) NOT NULL,
    "Tipo" VARCHAR(50) NOT NULL,
    "Ativo" BOOLEAN NOT NULL DEFAULT true,
    "DataCriacao" TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "DataAtualizacao" TIMESTAMP WITH TIME ZONE NULL
);

-- Índices para AtividadesAgropecuarias
CREATE UNIQUE INDEX IF NOT EXISTS "IX_AtividadesAgropecuarias_Codigo_Unique" ON public."AtividadesAgropecuarias" ("Codigo");
CREATE INDEX IF NOT EXISTS "IX_AtividadesAgropecuarias_Tipo" ON public."AtividadesAgropecuarias" ("Tipo");
CREATE INDEX IF NOT EXISTS "IX_AtividadesAgropecuarias_Descricao" ON public."AtividadesAgropecuarias" ("Descricao");
CREATE INDEX IF NOT EXISTS "IX_AtividadesAgropecuarias_DataCriacao" ON public."AtividadesAgropecuarias" ("DataCriacao");

-- =====================================================
-- TABELA: UnidadesMedida
-- =====================================================
CREATE TABLE IF NOT EXISTS public."UnidadesMedida" (
    "Id" SERIAL PRIMARY KEY,
    "Simbolo" VARCHAR(10) NOT NULL,
    "Nome" VARCHAR(100) NOT NULL,
    "Tipo" VARCHAR(50) NOT NULL,
    "FatorConversao" DECIMAL(18,6) NULL,
    "Ativo" BOOLEAN NOT NULL DEFAULT true,
    "DataCriacao" TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "DataAtualizacao" TIMESTAMP WITH TIME ZONE NULL
);

-- Índices para UnidadesMedida
CREATE UNIQUE INDEX IF NOT EXISTS "IX_UnidadesMedida_Simbolo_Unique" ON public."UnidadesMedida" ("Simbolo");
CREATE INDEX IF NOT EXISTS "IX_UnidadesMedida_Tipo" ON public."UnidadesMedida" ("Tipo");
CREATE INDEX IF NOT EXISTS "IX_UnidadesMedida_Nome" ON public."UnidadesMedida" ("Nome");
CREATE INDEX IF NOT EXISTS "IX_UnidadesMedida_DataCriacao" ON public."UnidadesMedida" ("DataCriacao");

-- =====================================================
-- TABELA: Embalagens
-- =====================================================
CREATE TABLE IF NOT EXISTS public."Embalagens" (
    "Id" SERIAL PRIMARY KEY,
    "Nome" VARCHAR(100) NOT NULL,
    "Descricao" VARCHAR(500) NULL,
    "UnidadeMedidaId" INTEGER NOT NULL,
    "Ativo" BOOLEAN NOT NULL DEFAULT true,
    "DataCriacao" TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "DataAtualizacao" TIMESTAMP WITH TIME ZONE NULL,
    
    -- Foreign Key
    CONSTRAINT "FK_Embalagens_UnidadesMedida_UnidadeMedidaId" 
        FOREIGN KEY ("UnidadeMedidaId") 
        REFERENCES public."UnidadesMedida" ("Id") 
        ON DELETE RESTRICT
);

-- Índices para Embalagens
CREATE INDEX IF NOT EXISTS "IX_Embalagens_Nome" ON public."Embalagens" ("Nome");
CREATE INDEX IF NOT EXISTS "IX_Embalagens_UnidadeMedidaId" ON public."Embalagens" ("UnidadeMedidaId");
CREATE UNIQUE INDEX IF NOT EXISTS "IX_Embalagens_Nome_UnidadeMedidaId_Unique" ON public."Embalagens" ("Nome", "UnidadeMedidaId");
CREATE INDEX IF NOT EXISTS "IX_Embalagens_DataCriacao" ON public."Embalagens" ("DataCriacao");

-- =====================================================
-- ALTERAR TABELA FORNECEDOR: Endereco -> Logradouro
-- =====================================================
-- Renomear coluna Endereco para Logradouro na tabela Fornecedor
DO $$
BEGIN
    IF EXISTS (
        SELECT 1 
        FROM information_schema.columns 
        WHERE table_schema = 'public' 
        AND table_name = 'Fornecedor' 
        AND column_name = 'Endereco'
    ) THEN
        ALTER TABLE public."Fornecedor" RENAME COLUMN "Endereco" TO "Logradouro";
    END IF;
END $$;

-- =====================================================
-- DADOS INICIAIS
-- =====================================================

-- Inserir moedas básicas
INSERT INTO public."Moedas" ("Codigo", "Nome", "Simbolo", "Ativo") 
VALUES 
    ('BRL', 'Real Brasileiro', 'R$', true),
    ('USD', 'Dólar Americano', '$', true),
    ('EUR', 'Euro', '€', true)
ON CONFLICT ("Codigo") DO NOTHING;

-- Inserir unidades de medida básicas
INSERT INTO public."UnidadesMedida" ("Simbolo", "Nome", "Tipo", "FatorConversao", "Ativo") 
VALUES 
    ('kg', 'Quilograma', 'Peso', 1.0, true),
    ('g', 'Grama', 'Peso', 0.001, true),
    ('t', 'Tonelada', 'Peso', 1000.0, true),
    ('L', 'Litro', 'Volume', 1.0, true),
    ('mL', 'Mililitro', 'Volume', 0.001, true),
    ('m³', 'Metro Cúbico', 'Volume', 1000.0, true),
    ('m²', 'Metro Quadrado', 'Area', 1.0, true),
    ('ha', 'Hectare', 'Area', 10000.0, true),
    ('un', 'Unidade', 'Unidade', 1.0, true),
    ('pc', 'Peça', 'Unidade', 1.0, true)
ON CONFLICT ("Simbolo") DO NOTHING;

-- Inserir atividades agropecuárias básicas
INSERT INTO public."AtividadesAgropecuarias" ("Codigo", "Descricao", "Tipo", "Ativo") 
VALUES 
    ('SOJA', 'Cultivo de Soja', 'Agricultura', true),
    ('MILHO', 'Cultivo de Milho', 'Agricultura', true),
    ('ALGODAO', 'Cultivo de Algodão', 'Agricultura', true),
    ('CANA', 'Cultivo de Cana-de-açúcar', 'Agricultura', true),
    ('CAFE', 'Cultivo de Café', 'Agricultura', true),
    ('BOVINO', 'Criação de Bovinos', 'Pecuaria', true),
    ('SUINO', 'Criação de Suínos', 'Pecuaria', true),
    ('AVES', 'Criação de Aves', 'Pecuaria', true),
    ('MISTA', 'Atividade Mista', 'Mista', true)
ON CONFLICT ("Codigo") DO NOTHING;

-- Inserir embalagens básicas
INSERT INTO public."Embalagens" ("Nome", "Descricao", "UnidadeMedidaId", "Ativo") 
SELECT 
    'Saco 50kg', 
    'Saco de 50 quilogramas', 
    um."Id", 
    true
FROM public."UnidadesMedida" um 
WHERE um."Simbolo" = 'kg'
ON CONFLICT DO NOTHING;

INSERT INTO public."Embalagens" ("Nome", "Descricao", "UnidadeMedidaId", "Ativo") 
SELECT 
    'Tambor 200L', 
    'Tambor de 200 litros', 
    um."Id", 
    true
FROM public."UnidadesMedida" um 
WHERE um."Simbolo" = 'L'
ON CONFLICT DO NOTHING;

INSERT INTO public."Embalagens" ("Nome", "Descricao", "UnidadeMedidaId", "Ativo") 
SELECT 
    'Caixa', 
    'Caixa unitária', 
    um."Id", 
    true
FROM public."UnidadesMedida" um 
WHERE um."Simbolo" = 'un'
ON CONFLICT DO NOTHING;

-- =====================================================
-- VERIFICAÇÕES FINAIS
-- =====================================================

-- Verificar se as tabelas foram criadas
SELECT 
    'Tabelas criadas com sucesso!' as status,
    COUNT(*) as total_tabelas
FROM information_schema.tables 
WHERE table_schema = 'public' 
AND table_name IN ('Moedas', 'AtividadesAgropecuarias', 'UnidadesMedida', 'Embalagens');

-- Verificar dados inseridos
SELECT 'Moedas' as tabela, COUNT(*) as registros FROM public."Moedas"
UNION ALL
SELECT 'AtividadesAgropecuarias' as tabela, COUNT(*) as registros FROM public."AtividadesAgropecuarias"
UNION ALL
SELECT 'UnidadesMedida' as tabela, COUNT(*) as registros FROM public."UnidadesMedida"
UNION ALL
SELECT 'Embalagens' as tabela, COUNT(*) as registros FROM public."Embalagens";