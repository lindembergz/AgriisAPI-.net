-- =====================================================
-- MIGRATION SCRIPT: Populate Reference Tables
-- Description: Populates reference tables with existing data from Enderecos module
-- Author: System Migration
-- Date: 2025-01-27
-- Requirements: 12.1, 12.2, 12.3, 12.4, 12.5, 12.6
-- =====================================================

-- Enable detailed logging
\set VERBOSITY verbose
\timing on

-- Create migration log table if not exists
CREATE TABLE IF NOT EXISTS public."MigrationLogs" (
    "Id" SERIAL PRIMARY KEY,
    "MigrationName" VARCHAR(200) NOT NULL,
    "Step" VARCHAR(100) NOT NULL,
    "Message" TEXT NOT NULL,
    "RecordsAffected" INTEGER DEFAULT 0,
    "ExecutionTime" TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    "Status" VARCHAR(20) DEFAULT 'INFO' -- INFO, WARNING, ERROR, SUCCESS
);

-- Start migration transaction
BEGIN;

-- Log migration start
INSERT INTO public."MigrationLogs" ("MigrationName", "Step", "Message", "Status")
VALUES ('01_populate_reference_tables', 'START', 'Starting reference tables population migration', 'INFO');

-- =====================================================
-- STEP 1: Create backup tables for rollback
-- =====================================================
DO $$
BEGIN
    -- Backup existing reference tables if they exist
    IF EXISTS (SELECT 1 FROM information_schema.tables WHERE table_name = 'Moedas' AND table_schema = 'public') THEN
        DROP TABLE IF EXISTS public."Moedas_backup_" || to_char(CURRENT_TIMESTAMP, 'YYYYMMDDHH24MISS');
        EXECUTE 'CREATE TABLE public."Moedas_backup_' || to_char(CURRENT_TIMESTAMP, 'YYYYMMDDHH24MISS') || '" AS SELECT * FROM public."Moedas"';
    END IF;
    
    IF EXISTS (SELECT 1 FROM information_schema.tables WHERE table_name = 'Paises' AND table_schema = 'public') THEN
        DROP TABLE IF EXISTS public."Paises_backup_" || to_char(CURRENT_TIMESTAMP, 'YYYYMMDDHH24MISS');
        EXECUTE 'CREATE TABLE public."Paises_backup_' || to_char(CURRENT_TIMESTAMP, 'YYYYMMDDHH24MISS') || '" AS SELECT * FROM public."Paises"';
    END IF;
END $$;

INSERT INTO public."MigrationLogs" ("MigrationName", "Step", "Message", "Status")
VALUES ('01_populate_reference_tables', 'BACKUP', 'Created backup tables for existing reference data', 'SUCCESS');

-- =====================================================
-- STEP 2: Create Paises table and populate with Brasil
-- =====================================================
CREATE TABLE IF NOT EXISTS public."Paises" (
    "Id" SERIAL PRIMARY KEY,
    "Codigo" VARCHAR(3) NOT NULL,
    "Nome" VARCHAR(100) NOT NULL,
    "Ativo" BOOLEAN NOT NULL DEFAULT true,
    "DataCriacao" TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "DataAtualizacao" TIMESTAMP WITH TIME ZONE NULL
);

-- Create indexes for Paises
CREATE UNIQUE INDEX IF NOT EXISTS "IX_Paises_Codigo_Unique" ON public."Paises" ("Codigo");
CREATE UNIQUE INDEX IF NOT EXISTS "IX_Paises_Nome_Unique" ON public."Paises" ("Nome");
CREATE INDEX IF NOT EXISTS "IX_Paises_DataCriacao" ON public."Paises" ("DataCriacao");

-- Insert Brasil as default country
INSERT INTO public."Paises" ("Codigo", "Nome", "Ativo") 
VALUES ('BR', 'Brasil', true)
ON CONFLICT ("Codigo") DO NOTHING;

INSERT INTO public."MigrationLogs" ("MigrationName", "Step", "Message", "RecordsAffected", "Status")
VALUES ('01_populate_reference_tables', 'PAISES', 'Created Paises table and inserted Brasil', 1, 'SUCCESS');

-- =====================================================
-- STEP 3: Create Ufs table and populate from Estados
-- =====================================================
CREATE TABLE IF NOT EXISTS public."Ufs" (
    "Id" SERIAL PRIMARY KEY,
    "Codigo" VARCHAR(2) NOT NULL,
    "Nome" VARCHAR(100) NOT NULL,
    "PaisId" INTEGER NOT NULL,
    "Ativo" BOOLEAN NOT NULL DEFAULT true,
    "DataCriacao" TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "DataAtualizacao" TIMESTAMP WITH TIME ZONE NULL,
    
    CONSTRAINT "FK_Ufs_Paises_PaisId" 
        FOREIGN KEY ("PaisId") 
        REFERENCES public."Paises" ("Id") 
        ON DELETE RESTRICT
);

-- Create indexes for Ufs
CREATE UNIQUE INDEX IF NOT EXISTS "IX_Ufs_Codigo_Unique" ON public."Ufs" ("Codigo");
CREATE INDEX IF NOT EXISTS "IX_Ufs_Nome" ON public."Ufs" ("Nome");
CREATE INDEX IF NOT EXISTS "IX_Ufs_PaisId" ON public."Ufs" ("PaisId");
CREATE INDEX IF NOT EXISTS "IX_Ufs_DataCriacao" ON public."Ufs" ("DataCriacao");

-- Populate Ufs from existing Estados table
DO $$
DECLARE
    brasil_id INTEGER;
    ufs_inserted INTEGER := 0;
BEGIN
    -- Get Brasil ID
    SELECT "Id" INTO brasil_id FROM public."Paises" WHERE "Codigo" = 'BR';
    
    -- Check if Estados table exists
    IF EXISTS (SELECT 1 FROM information_schema.tables WHERE table_name = 'Estado' AND table_schema = 'public') THEN
        -- Insert UFs from Estados table
        INSERT INTO public."Ufs" ("Codigo", "Nome", "PaisId", "Ativo", "DataCriacao")
        SELECT 
            e."Uf" as "Codigo",
            e."Nome",
            brasil_id as "PaisId",
            true as "Ativo",
            COALESCE(e."DataCriacao", CURRENT_TIMESTAMP) as "DataCriacao"
        FROM public."Estado" e
        WHERE e."Uf" IS NOT NULL 
        AND LENGTH(TRIM(e."Uf")) = 2
        ON CONFLICT ("Codigo") DO NOTHING;
        
        GET DIAGNOSTICS ufs_inserted = ROW_COUNT;
    ELSE
        -- Insert default Brazilian states if Estados table doesn't exist
        INSERT INTO public."Ufs" ("Codigo", "Nome", "PaisId", "Ativo") VALUES
        ('AC', 'Acre', brasil_id, true),
        ('AL', 'Alagoas', brasil_id, true),
        ('AP', 'Amapá', brasil_id, true),
        ('AM', 'Amazonas', brasil_id, true),
        ('BA', 'Bahia', brasil_id, true),
        ('CE', 'Ceará', brasil_id, true),
        ('DF', 'Distrito Federal', brasil_id, true),
        ('ES', 'Espírito Santo', brasil_id, true),
        ('GO', 'Goiás', brasil_id, true),
        ('MA', 'Maranhão', brasil_id, true),
        ('MT', 'Mato Grosso', brasil_id, true),
        ('MS', 'Mato Grosso do Sul', brasil_id, true),
        ('MG', 'Minas Gerais', brasil_id, true),
        ('PA', 'Pará', brasil_id, true),
        ('PB', 'Paraíba', brasil_id, true),
        ('PR', 'Paraná', brasil_id, true),
        ('PE', 'Pernambuco', brasil_id, true),
        ('PI', 'Piauí', brasil_id, true),
        ('RJ', 'Rio de Janeiro', brasil_id, true),
        ('RN', 'Rio Grande do Norte', brasil_id, true),
        ('RS', 'Rio Grande do Sul', brasil_id, true),
        ('RO', 'Rondônia', brasil_id, true),
        ('RR', 'Roraima', brasil_id, true),
        ('SC', 'Santa Catarina', brasil_id, true),
        ('SP', 'São Paulo', brasil_id, true),
        ('SE', 'Sergipe', brasil_id, true),
        ('TO', 'Tocantins', brasil_id, true)
        ON CONFLICT ("Codigo") DO NOTHING;
        
        GET DIAGNOSTICS ufs_inserted = ROW_COUNT;
    END IF;
    
    INSERT INTO public."MigrationLogs" ("MigrationName", "Step", "Message", "RecordsAffected", "Status")
    VALUES ('01_populate_reference_tables', 'UFS', 'Populated Ufs table from Estados or default data', ufs_inserted, 'SUCCESS');
END $$;

-- =====================================================
-- STEP 4: Create Municipios table and populate from existing data
-- =====================================================
CREATE TABLE IF NOT EXISTS public."Municipios" (
    "Id" SERIAL PRIMARY KEY,
    "Nome" VARCHAR(200) NOT NULL,
    "CodigoIbge" VARCHAR(10) NULL,
    "UfId" INTEGER NOT NULL,
    "Ativo" BOOLEAN NOT NULL DEFAULT true,
    "DataCriacao" TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "DataAtualizacao" TIMESTAMP WITH TIME ZONE NULL,
    
    CONSTRAINT "FK_Municipios_Ufs_UfId" 
        FOREIGN KEY ("UfId") 
        REFERENCES public."Ufs" ("Id") 
        ON DELETE RESTRICT
);

-- Create indexes for Municipios
CREATE INDEX IF NOT EXISTS "IX_Municipios_Nome" ON public."Municipios" ("Nome");
CREATE INDEX IF NOT EXISTS "IX_Municipios_CodigoIbge" ON public."Municipios" ("CodigoIbge");
CREATE INDEX IF NOT EXISTS "IX_Municipios_UfId" ON public."Municipios" ("UfId");
CREATE UNIQUE INDEX IF NOT EXISTS "IX_Municipios_Nome_UfId_Unique" ON public."Municipios" ("Nome", "UfId");
CREATE INDEX IF NOT EXISTS "IX_Municipios_DataCriacao" ON public."Municipios" ("DataCriacao");

-- Populate Municipios from existing Municipio table
DO $$
DECLARE
    municipios_inserted INTEGER := 0;
BEGIN
    -- Check if Municipio table exists
    IF EXISTS (SELECT 1 FROM information_schema.tables WHERE table_name = 'Municipio' AND table_schema = 'public') THEN
        -- Insert Municipios from existing table
        INSERT INTO public."Municipios" ("Nome", "CodigoIbge", "UfId", "Ativo", "DataCriacao")
        SELECT DISTINCT
            m."Nome",
            CASE 
                WHEN m."CodigoIbge" > 0 THEN m."CodigoIbge"::VARCHAR
                ELSE NULL 
            END as "CodigoIbge",
            u."Id" as "UfId",
            true as "Ativo",
            COALESCE(m."DataCriacao", CURRENT_TIMESTAMP) as "DataCriacao"
        FROM public."Municipio" m
        INNER JOIN public."Estado" e ON e."Id" = m."EstadoId"
        INNER JOIN public."Ufs" u ON u."Codigo" = e."Uf"
        WHERE m."Nome" IS NOT NULL 
        AND TRIM(m."Nome") != ''
        ON CONFLICT ("Nome", "UfId") DO NOTHING;
        
        GET DIAGNOSTICS municipios_inserted = ROW_COUNT;
        
        INSERT INTO public."MigrationLogs" ("MigrationName", "Step", "Message", "RecordsAffected", "Status")
        VALUES ('01_populate_reference_tables', 'MUNICIPIOS', 'Populated Municipios table from existing data', municipios_inserted, 'SUCCESS');
    ELSE
        INSERT INTO public."MigrationLogs" ("MigrationName", "Step", "Message", "Status")
        VALUES ('01_populate_reference_tables', 'MUNICIPIOS', 'Municipio table not found, skipping population', 'WARNING');
    END IF;
END $$;

-- =====================================================
-- STEP 5: Create and populate Moedas table
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

-- Create indexes for Moedas
CREATE UNIQUE INDEX IF NOT EXISTS "IX_Moedas_Codigo_Unique" ON public."Moedas" ("Codigo");
CREATE UNIQUE INDEX IF NOT EXISTS "IX_Moedas_Nome_Unique" ON public."Moedas" ("Nome");
CREATE UNIQUE INDEX IF NOT EXISTS "IX_Moedas_Simbolo_Unique" ON public."Moedas" ("Simbolo");
CREATE INDEX IF NOT EXISTS "IX_Moedas_DataCriacao" ON public."Moedas" ("DataCriacao");

-- Insert basic currencies
INSERT INTO public."Moedas" ("Codigo", "Nome", "Simbolo", "Ativo") VALUES
('BRL', 'Real Brasileiro', 'R$', true),
('USD', 'Dólar Americano', '$', true),
('EUR', 'Euro', '€', true),
('ARS', 'Peso Argentino', '$', true),
('UYU', 'Peso Uruguaio', '$U', true),
('PYG', 'Guarani Paraguaio', '₲', true)
ON CONFLICT ("Codigo") DO NOTHING;

INSERT INTO public."MigrationLogs" ("MigrationName", "Step", "Message", "RecordsAffected", "Status")
VALUES ('01_populate_reference_tables', 'MOEDAS', 'Created and populated Moedas table', 6, 'SUCCESS');

-- =====================================================
-- STEP 6: Create and populate UnidadesMedida table
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

-- Create indexes for UnidadesMedida
CREATE UNIQUE INDEX IF NOT EXISTS "IX_UnidadesMedida_Simbolo_Unique" ON public."UnidadesMedida" ("Simbolo");
CREATE INDEX IF NOT EXISTS "IX_UnidadesMedida_Tipo" ON public."UnidadesMedida" ("Tipo");
CREATE INDEX IF NOT EXISTS "IX_UnidadesMedida_Nome" ON public."UnidadesMedida" ("Nome");
CREATE INDEX IF NOT EXISTS "IX_UnidadesMedida_DataCriacao" ON public."UnidadesMedida" ("DataCriacao");

-- Insert units of measure (including existing ones from Produto migration)
INSERT INTO public."UnidadesMedida" ("Simbolo", "Nome", "Tipo", "FatorConversao", "Ativo") VALUES
-- Weight units
('kg', 'Quilograma', 'Peso', 1.0, true),
('g', 'Grama', 'Peso', 0.001, true),
('t', 'Tonelada', 'Peso', 1000.0, true),
('T', 'Tonelada', 'Peso', 1000.0, true), -- Compatibility with existing data
('KG', 'Quilograma', 'Peso', 1.0, true), -- Compatibility with existing data
-- Volume units
('L', 'Litro', 'Volume', 1.0, true),
('mL', 'Mililitro', 'Volume', 0.001, true),
('m³', 'Metro Cúbico', 'Volume', 1000.0, true),
-- Area units
('m²', 'Metro Quadrado', 'Area', 1.0, true),
('ha', 'Hectare', 'Area', 10000.0, true),
('HA', 'Hectare', 'Area', 10000.0, true), -- Compatibility with existing data
-- Unit/Count units
('un', 'Unidade', 'Unidade', 1.0, true),
('pc', 'Peça', 'Unidade', 1.0, true),
-- Agricultural specific units (from existing Produto enum mapping)
('SEMENTES', 'Sementes', 'Unidade', 1.0, true),
('DOSE', 'Dose', 'Unidade', 1.0, true),
('FRASCO', 'Frasco', 'Unidade', 1.0, true),
('OVOS', 'Ovos', 'Unidade', 1.0, true),
('PARASITOIDE', 'Parasitoide', 'Unidade', 1.0, true)
ON CONFLICT ("Simbolo") DO NOTHING;

INSERT INTO public."MigrationLogs" ("MigrationName", "Step", "Message", "RecordsAffected", "Status")
VALUES ('01_populate_reference_tables', 'UNIDADES_MEDIDA', 'Created and populated UnidadesMedida table', 17, 'SUCCESS');

-- =====================================================
-- STEP 7: Create and populate AtividadesAgropecuarias table
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

-- Create indexes for AtividadesAgropecuarias
CREATE UNIQUE INDEX IF NOT EXISTS "IX_AtividadesAgropecuarias_Codigo_Unique" ON public."AtividadesAgropecuarias" ("Codigo");
CREATE INDEX IF NOT EXISTS "IX_AtividadesAgropecuarias_Tipo" ON public."AtividadesAgropecuarias" ("Tipo");
CREATE INDEX IF NOT EXISTS "IX_AtividadesAgropecuarias_Descricao" ON public."AtividadesAgropecuarias" ("Descricao");
CREATE INDEX IF NOT EXISTS "IX_AtividadesAgropecuarias_DataCriacao" ON public."AtividadesAgropecuarias" ("DataCriacao");

-- Insert agricultural activities
INSERT INTO public."AtividadesAgropecuarias" ("Codigo", "Descricao", "Tipo", "Ativo") VALUES
-- Agriculture
('SOJA', 'Cultivo de Soja', 'Agricultura', true),
('MILHO', 'Cultivo de Milho', 'Agricultura', true),
('ALGODAO', 'Cultivo de Algodão', 'Agricultura', true),
('CANA', 'Cultivo de Cana-de-açúcar', 'Agricultura', true),
('CAFE', 'Cultivo de Café', 'Agricultura', true),
('ARROZ', 'Cultivo de Arroz', 'Agricultura', true),
('FEIJAO', 'Cultivo de Feijão', 'Agricultura', true),
('TRIGO', 'Cultivo de Trigo', 'Agricultura', true),
('SORGO', 'Cultivo de Sorgo', 'Agricultura', true),
('GIRASSOL', 'Cultivo de Girassol', 'Agricultura', true),
-- Livestock
('BOVINO', 'Criação de Bovinos', 'Pecuaria', true),
('SUINO', 'Criação de Suínos', 'Pecuaria', true),
('AVES', 'Criação de Aves', 'Pecuaria', true),
('OVINO', 'Criação de Ovinos', 'Pecuaria', true),
('CAPRINO', 'Criação de Caprinos', 'Pecuaria', true),
('PISCICULTURA', 'Piscicultura', 'Pecuaria', true),
-- Mixed
('MISTA', 'Atividade Mista', 'Mista', true),
('AGROPECUARIA', 'Agropecuária Geral', 'Mista', true)
ON CONFLICT ("Codigo") DO NOTHING;

INSERT INTO public."MigrationLogs" ("MigrationName", "Step", "Message", "RecordsAffected", "Status")
VALUES ('01_populate_reference_tables', 'ATIVIDADES_AGROPECUARIAS', 'Created and populated AtividadesAgropecuarias table', 18, 'SUCCESS');

-- =====================================================
-- STEP 8: Create and populate Embalagens table
-- =====================================================
CREATE TABLE IF NOT EXISTS public."Embalagens" (
    "Id" SERIAL PRIMARY KEY,
    "Nome" VARCHAR(100) NOT NULL,
    "Descricao" VARCHAR(500) NULL,
    "UnidadeMedidaId" INTEGER NOT NULL,
    "Ativo" BOOLEAN NOT NULL DEFAULT true,
    "DataCriacao" TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "DataAtualizacao" TIMESTAMP WITH TIME ZONE NULL,
    
    CONSTRAINT "FK_Embalagens_UnidadesMedida_UnidadeMedidaId" 
        FOREIGN KEY ("UnidadeMedidaId") 
        REFERENCES public."UnidadesMedida" ("Id") 
        ON DELETE RESTRICT
);

-- Create indexes for Embalagens
CREATE INDEX IF NOT EXISTS "IX_Embalagens_Nome" ON public."Embalagens" ("Nome");
CREATE INDEX IF NOT EXISTS "IX_Embalagens_UnidadeMedidaId" ON public."Embalagens" ("UnidadeMedidaId");
CREATE UNIQUE INDEX IF NOT EXISTS "IX_Embalagens_Nome_UnidadeMedidaId_Unique" ON public."Embalagens" ("Nome", "UnidadeMedidaId");
CREATE INDEX IF NOT EXISTS "IX_Embalagens_DataCriacao" ON public."Embalagens" ("DataCriacao");

-- Insert basic packaging types
DO $$
DECLARE
    kg_id INTEGER;
    l_id INTEGER;
    un_id INTEGER;
    t_id INTEGER;
    embalagens_inserted INTEGER := 0;
BEGIN
    -- Get unit IDs
    SELECT "Id" INTO kg_id FROM public."UnidadesMedida" WHERE "Simbolo" = 'kg' LIMIT 1;
    SELECT "Id" INTO l_id FROM public."UnidadesMedida" WHERE "Simbolo" = 'L' LIMIT 1;
    SELECT "Id" INTO un_id FROM public."UnidadesMedida" WHERE "Simbolo" = 'un' LIMIT 1;
    SELECT "Id" INTO t_id FROM public."UnidadesMedida" WHERE "Simbolo" = 't' LIMIT 1;
    
    -- Insert packaging types
    INSERT INTO public."Embalagens" ("Nome", "Descricao", "UnidadeMedidaId", "Ativo") VALUES
    -- Weight-based packaging
    ('Saco 1kg', 'Saco de 1 quilograma', kg_id, true),
    ('Saco 5kg', 'Saco de 5 quilogramas', kg_id, true),
    ('Saco 10kg', 'Saco de 10 quilogramas', kg_id, true),
    ('Saco 20kg', 'Saco de 20 quilogramas', kg_id, true),
    ('Saco 25kg', 'Saco de 25 quilogramas', kg_id, true),
    ('Saco 50kg', 'Saco de 50 quilogramas', kg_id, true),
    ('Big Bag 500kg', 'Big Bag de 500 quilogramas', kg_id, true),
    ('Big Bag 1000kg', 'Big Bag de 1000 quilogramas', kg_id, true),
    -- Volume-based packaging
    ('Frasco 100mL', 'Frasco de 100 mililitros', l_id, true),
    ('Frasco 250mL', 'Frasco de 250 mililitros', l_id, true),
    ('Frasco 500mL', 'Frasco de 500 mililitros', l_id, true),
    ('Frasco 1L', 'Frasco de 1 litro', l_id, true),
    ('Galão 5L', 'Galão de 5 litros', l_id, true),
    ('Galão 10L', 'Galão de 10 litros', l_id, true),
    ('Galão 20L', 'Galão de 20 litros', l_id, true),
    ('Tambor 50L', 'Tambor de 50 litros', l_id, true),
    ('Tambor 100L', 'Tambor de 100 litros', l_id, true),
    ('Tambor 200L', 'Tambor de 200 litros', l_id, true),
    ('IBC 1000L', 'Container IBC de 1000 litros', l_id, true),
    -- Unit-based packaging
    ('Caixa', 'Caixa unitária', un_id, true),
    ('Pacote', 'Pacote unitário', un_id, true),
    ('Unidade', 'Unidade individual', un_id, true),
    ('Bandeja', 'Bandeja unitária', un_id, true),
    ('Pote', 'Pote unitário', un_id, true)
    ON CONFLICT ("Nome", "UnidadeMedidaId") DO NOTHING;
    
    GET DIAGNOSTICS embalagens_inserted = ROW_COUNT;
    
    INSERT INTO public."MigrationLogs" ("MigrationName", "Step", "Message", "RecordsAffected", "Status")
    VALUES ('01_populate_reference_tables', 'EMBALAGENS', 'Created and populated Embalagens table', embalagens_inserted, 'SUCCESS');
END $$;

-- =====================================================
-- STEP 9: Generate migration summary report
-- =====================================================
DO $$
DECLARE
    paises_count INTEGER;
    ufs_count INTEGER;
    municipios_count INTEGER;
    moedas_count INTEGER;
    unidades_count INTEGER;
    atividades_count INTEGER;
    embalagens_count INTEGER;
    report_text TEXT;
BEGIN
    -- Count records in each table
    SELECT COUNT(*) INTO paises_count FROM public."Paises";
    SELECT COUNT(*) INTO ufs_count FROM public."Ufs";
    SELECT COUNT(*) INTO municipios_count FROM public."Municipios";
    SELECT COUNT(*) INTO moedas_count FROM public."Moedas";
    SELECT COUNT(*) INTO unidades_count FROM public."UnidadesMedida";
    SELECT COUNT(*) INTO atividades_count FROM public."AtividadesAgropecuarias";
    SELECT COUNT(*) INTO embalagens_count FROM public."Embalagens";
    
    -- Generate report
    report_text := 'MIGRATION SUMMARY REPORT' || E'\n' ||
                   '========================' || E'\n' ||
                   'Paises: ' || paises_count || ' records' || E'\n' ||
                   'UFs: ' || ufs_count || ' records' || E'\n' ||
                   'Municipios: ' || municipios_count || ' records' || E'\n' ||
                   'Moedas: ' || moedas_count || ' records' || E'\n' ||
                   'UnidadesMedida: ' || unidades_count || ' records' || E'\n' ||
                   'AtividadesAgropecuarias: ' || atividades_count || ' records' || E'\n' ||
                   'Embalagens: ' || embalagens_count || ' records' || E'\n' ||
                   '========================' || E'\n' ||
                   'Total reference records: ' || (paises_count + ufs_count + municipios_count + moedas_count + unidades_count + atividades_count + embalagens_count);
    
    INSERT INTO public."MigrationLogs" ("MigrationName", "Step", "Message", "Status")
    VALUES ('01_populate_reference_tables', 'SUMMARY', report_text, 'SUCCESS');
    
    -- Output summary to console
    RAISE NOTICE '%', report_text;
END $$;

-- Log migration completion
INSERT INTO public."MigrationLogs" ("MigrationName", "Step", "Message", "Status")
VALUES ('01_populate_reference_tables', 'COMPLETE', 'Reference tables population migration completed successfully', 'SUCCESS');

-- Commit transaction
COMMIT;

-- Display final message
\echo 'Migration 01_populate_reference_tables completed successfully!'
\echo 'Check MigrationLogs table for detailed execution log.'