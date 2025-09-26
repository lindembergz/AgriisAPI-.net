-- Script para migrar tabela Produto para usar referências
-- Baseado na migration 20250925120000_RefatorarProdutoParaReferencias

BEGIN;

-- 1. Adicionar novas colunas de foreign keys
ALTER TABLE "public"."Produto" 
ADD COLUMN "UnidadeMedidaId" integer NOT NULL DEFAULT 1;

ALTER TABLE "public"."Produto" 
ADD COLUMN "EmbalagemId" integer NULL;

ALTER TABLE "public"."Produto" 
ADD COLUMN "AtividadeAgropecuariaId" integer NULL;

-- 2. Adicionar campos de dimensões (objeto valor)
ALTER TABLE "public"."Produto" 
ADD COLUMN "Altura" numeric(10,2) NOT NULL DEFAULT 0;

ALTER TABLE "public"."Produto" 
ADD COLUMN "Largura" numeric(10,2) NOT NULL DEFAULT 0;

ALTER TABLE "public"."Produto" 
ADD COLUMN "Comprimento" numeric(10,2) NOT NULL DEFAULT 0;

ALTER TABLE "public"."Produto" 
ADD COLUMN "PesoNominal" numeric(10,3) NOT NULL DEFAULT 0;

ALTER TABLE "public"."Produto" 
ADD COLUMN "PesoEmbalagem" numeric(10,3) NOT NULL DEFAULT 0;

ALTER TABLE "public"."Produto" 
ADD COLUMN "Pms" numeric(10,3) NULL;

ALTER TABLE "public"."Produto" 
ADD COLUMN "QuantidadeMinima" numeric(10,3) NOT NULL DEFAULT 1;

ALTER TABLE "public"."Produto" 
ADD COLUMN "Embalagem" character varying(100) NOT NULL DEFAULT 'Unidade';

ALTER TABLE "public"."Produto" 
ADD COLUMN "FaixaDensidadeInicial" numeric(10,3) NULL;

ALTER TABLE "public"."Produto" 
ADD COLUMN "FaixaDensidadeFinal" numeric(10,3) NULL;

-- 3. Migração de dados - mapear valores de enum Unidade para UnidadeMedidaId
-- Primeiro, vamos verificar se as unidades de medida existem, se não, criar
INSERT INTO "public"."UnidadeMedida" ("Nome", "Simbolo", "Tipo", "FatorConversao", "Ativo", "DataCriacao")
SELECT 'Sementes', 'SEMENTES', 0, 1.0, true, CURRENT_TIMESTAMP
WHERE NOT EXISTS (SELECT 1 FROM "public"."UnidadeMedida" WHERE "Simbolo" = 'SEMENTES');

INSERT INTO "public"."UnidadeMedida" ("Nome", "Simbolo", "Tipo", "FatorConversao", "Ativo", "DataCriacao")
SELECT 'Quilograma', 'KG', 1, 1.0, true, CURRENT_TIMESTAMP
WHERE NOT EXISTS (SELECT 1 FROM "public"."UnidadeMedida" WHERE "Simbolo" = 'KG');

INSERT INTO "public"."UnidadeMedida" ("Nome", "Simbolo", "Tipo", "FatorConversao", "Ativo", "DataCriacao")
SELECT 'Tonelada', 'T', 1, 1000.0, true, CURRENT_TIMESTAMP
WHERE NOT EXISTS (SELECT 1 FROM "public"."UnidadeMedida" WHERE "Simbolo" = 'T');

INSERT INTO "public"."UnidadeMedida" ("Nome", "Simbolo", "Tipo", "FatorConversao", "Ativo", "DataCriacao")
SELECT 'Litro', 'L', 2, 1.0, true, CURRENT_TIMESTAMP
WHERE NOT EXISTS (SELECT 1 FROM "public"."UnidadeMedida" WHERE "Simbolo" = 'L');

INSERT INTO "public"."UnidadeMedida" ("Nome", "Simbolo", "Tipo", "FatorConversao", "Ativo", "DataCriacao")
SELECT 'Hectare', 'HA', 3, 1.0, true, CURRENT_TIMESTAMP
WHERE NOT EXISTS (SELECT 1 FROM "public"."UnidadeMedida" WHERE "Simbolo" = 'HA');

INSERT INTO "public"."UnidadeMedida" ("Nome", "Simbolo", "Tipo", "FatorConversao", "Ativo", "DataCriacao")
SELECT 'Dose', 'DOSE', 4, 1.0, true, CURRENT_TIMESTAMP
WHERE NOT EXISTS (SELECT 1 FROM "public"."UnidadeMedida" WHERE "Simbolo" = 'DOSE');

INSERT INTO "public"."UnidadeMedida" ("Nome", "Simbolo", "Tipo", "FatorConversao", "Ativo", "DataCriacao")
SELECT 'Frasco', 'FRASCO', 4, 1.0, true, CURRENT_TIMESTAMP
WHERE NOT EXISTS (SELECT 1 FROM "public"."UnidadeMedida" WHERE "Simbolo" = 'FRASCO');

INSERT INTO "public"."UnidadeMedida" ("Nome", "Simbolo", "Tipo", "FatorConversao", "Ativo", "DataCriacao")
SELECT 'Ovos', 'OVOS', 4, 1.0, true, CURRENT_TIMESTAMP
WHERE NOT EXISTS (SELECT 1 FROM "public"."UnidadeMedida" WHERE "Simbolo" = 'OVOS');

INSERT INTO "public"."UnidadeMedida" ("Nome", "Simbolo", "Tipo", "FatorConversao", "Ativo", "DataCriacao")
SELECT 'Parasitoide', 'PARASITOIDE', 4, 1.0, true, CURRENT_TIMESTAMP
WHERE NOT EXISTS (SELECT 1 FROM "public"."UnidadeMedida" WHERE "Simbolo" = 'PARASITOIDE');

-- Agora mapear os valores de enum para IDs
UPDATE "public"."Produto" 
SET "UnidadeMedidaId" = CASE 
    WHEN "Unidade" = 0 THEN (SELECT "Id" FROM "public"."UnidadeMedida" WHERE "Simbolo" = 'SEMENTES' LIMIT 1)
    WHEN "Unidade" = 1 THEN (SELECT "Id" FROM "public"."UnidadeMedida" WHERE "Simbolo" = 'KG' LIMIT 1)
    WHEN "Unidade" = 2 THEN (SELECT "Id" FROM "public"."UnidadeMedida" WHERE "Simbolo" = 'T' LIMIT 1)
    WHEN "Unidade" = 3 THEN (SELECT "Id" FROM "public"."UnidadeMedida" WHERE "Simbolo" = 'L' LIMIT 1)
    WHEN "Unidade" = 4 THEN (SELECT "Id" FROM "public"."UnidadeMedida" WHERE "Simbolo" = 'HA' LIMIT 1)
    WHEN "Unidade" = 5 THEN (SELECT "Id" FROM "public"."UnidadeMedida" WHERE "Simbolo" = 'DOSE' LIMIT 1)
    WHEN "Unidade" = 6 THEN (SELECT "Id" FROM "public"."UnidadeMedida" WHERE "Simbolo" = 'FRASCO' LIMIT 1)
    WHEN "Unidade" = 7 THEN (SELECT "Id" FROM "public"."UnidadeMedida" WHERE "Simbolo" = 'OVOS' LIMIT 1)
    WHEN "Unidade" = 8 THEN (SELECT "Id" FROM "public"."UnidadeMedida" WHERE "Simbolo" = 'PARASITOIDE' LIMIT 1)
    ELSE (SELECT "Id" FROM "public"."UnidadeMedida" WHERE "Simbolo" = 'KG' LIMIT 1)
END
WHERE "UnidadeMedidaId" = 1;

-- 4. Criar índices para as novas foreign keys
CREATE INDEX "IX_Produto_UnidadeMedidaId" ON "public"."Produto" ("UnidadeMedidaId");
CREATE INDEX "IX_Produto_EmbalagemId" ON "public"."Produto" ("EmbalagemId");
CREATE INDEX "IX_Produto_AtividadeAgropecuariaId" ON "public"."Produto" ("AtividadeAgropecuariaId");

-- 5. Adicionar foreign key constraints
ALTER TABLE "public"."Produto" 
ADD CONSTRAINT "FK_Produto_UnidadeMedida_UnidadeMedidaId" 
FOREIGN KEY ("UnidadeMedidaId") 
REFERENCES "public"."UnidadeMedida" ("Id") 
ON DELETE RESTRICT;

ALTER TABLE "public"."Produto" 
ADD CONSTRAINT "FK_Produto_Embalagem_EmbalagemId" 
FOREIGN KEY ("EmbalagemId") 
REFERENCES "public"."Embalagem" ("Id") 
ON DELETE RESTRICT;

ALTER TABLE "public"."Produto" 
ADD CONSTRAINT "FK_Produto_AtividadeAgropecuaria_AtividadeAgropecuariaId" 
FOREIGN KEY ("AtividadeAgropecuariaId") 
REFERENCES "public"."AtividadeAgropecuaria" ("Id") 
ON DELETE RESTRICT;

-- 6. Remover a coluna antiga "Unidade" após a migração dos dados
ALTER TABLE "public"."Produto" DROP COLUMN "Unidade";

-- 7. Inserir registro na tabela de migrations
INSERT INTO "public"."__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20250925120000_RefatorarProdutoParaReferencias', '9.0.0');

COMMIT;