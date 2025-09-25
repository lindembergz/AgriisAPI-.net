-- Script para adicionar campos de telefone na tabela Produtor
-- Data: 2025-01-25
-- Descrição: Adiciona os campos Telefone1, Telefone2, Telefone3 e Email na tabela Produtor

-- Verificar se a tabela Produtor existe
DO $$
BEGIN
    IF NOT EXISTS (SELECT 1 FROM information_schema.tables WHERE table_name = 'Produtor' AND table_schema = 'public') THEN
        RAISE EXCEPTION 'Tabela Produtor não encontrada no schema public';
    END IF;
END $$;

-- Adicionar campo Telefone1 se não existir
DO $$
BEGIN
    IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'Produtor' AND column_name = 'Telefone1' AND table_schema = 'public') THEN
        ALTER TABLE public."Produtor" ADD COLUMN "Telefone1" VARCHAR(20) NULL;
        RAISE NOTICE 'Campo Telefone1 adicionado com sucesso';
    ELSE
        RAISE NOTICE 'Campo Telefone1 já existe';
    END IF;
END $$;

-- Adicionar campo Telefone2 se não existir
DO $$
BEGIN
    IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'Produtor' AND column_name = 'Telefone2' AND table_schema = 'public') THEN
        ALTER TABLE public."Produtor" ADD COLUMN "Telefone2" VARCHAR(20) NULL;
        RAISE NOTICE 'Campo Telefone2 adicionado com sucesso';
    ELSE
        RAISE NOTICE 'Campo Telefone2 já existe';
    END IF;
END $$;

-- Adicionar campo Telefone3 se não existir
DO $$
BEGIN
    IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'Produtor' AND column_name = 'Telefone3' AND table_schema = 'public') THEN
        ALTER TABLE public."Produtor" ADD COLUMN "Telefone3" VARCHAR(20) NULL;
        RAISE NOTICE 'Campo Telefone3 adicionado com sucesso';
    ELSE
        RAISE NOTICE 'Campo Telefone3 já existe';
    END IF;
END $$;

-- Adicionar campo Email se não existir
DO $$
BEGIN
    IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'Produtor' AND column_name = 'Email' AND table_schema = 'public') THEN
        ALTER TABLE public."Produtor" ADD COLUMN "Email" VARCHAR(100) NULL;
        RAISE NOTICE 'Campo Email adicionado com sucesso';
    ELSE
        RAISE NOTICE 'Campo Email já existe';
    END IF;
END $$;

-- Verificar se a tabela UsuarioProdutor existe, se não, criar
DO $$
BEGIN
    IF NOT EXISTS (SELECT 1 FROM information_schema.tables WHERE table_name = 'UsuarioProdutor' AND table_schema = 'public') THEN
        -- Criar tabela UsuarioProdutor
        CREATE TABLE public."UsuarioProdutor" (
            "Id" SERIAL PRIMARY KEY,
            "UsuarioId" INTEGER NOT NULL,
            "ProdutorId" INTEGER NOT NULL,
            "EhProprietario" BOOLEAN NOT NULL DEFAULT FALSE,
            "Ativo" BOOLEAN NOT NULL DEFAULT TRUE,
            "DataCriacao" TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
            "DataAtualizacao" TIMESTAMP WITH TIME ZONE NULL,
            
            -- Chaves estrangeiras
            CONSTRAINT "FK_UsuarioProdutor_Usuario_UsuarioId" 
                FOREIGN KEY ("UsuarioId") REFERENCES public."Usuario"("Id") ON DELETE CASCADE,
            CONSTRAINT "FK_UsuarioProdutor_Produtor_ProdutorId" 
                FOREIGN KEY ("ProdutorId") REFERENCES public."Produtor"("Id") ON DELETE CASCADE
        );
        
        -- Criar índices
        CREATE INDEX "IX_UsuarioProdutor_UsuarioId" ON public."UsuarioProdutor" ("UsuarioId");
        CREATE INDEX "IX_UsuarioProdutor_ProdutorId" ON public."UsuarioProdutor" ("ProdutorId");
        CREATE UNIQUE INDEX "IX_UsuarioProdutor_UsuarioId_ProdutorId" ON public."UsuarioProdutor" ("UsuarioId", "ProdutorId");
        CREATE INDEX "IX_UsuarioProdutor_EhProprietario" ON public."UsuarioProdutor" ("EhProprietario");
        CREATE INDEX "IX_UsuarioProdutor_Ativo" ON public."UsuarioProdutor" ("Ativo");
        CREATE INDEX "IX_UsuarioProdutor_DataCriacao" ON public."UsuarioProdutor" ("DataCriacao");
        
        RAISE NOTICE 'Tabela UsuarioProdutor criada com sucesso';
    ELSE
        RAISE NOTICE 'Tabela UsuarioProdutor já existe';
    END IF;
END $$;

-- Verificar se os campos foram adicionados corretamente
SELECT 
    column_name,
    data_type,
    character_maximum_length,
    is_nullable
FROM information_schema.columns 
WHERE table_name = 'Produtor' 
    AND table_schema = 'public'
    AND column_name IN ('Telefone1', 'Telefone2', 'Telefone3', 'Email')
ORDER BY column_name;

-- Verificar estrutura da tabela UsuarioProdutor
SELECT 
    column_name,
    data_type,
    character_maximum_length,
    is_nullable
FROM information_schema.columns 
WHERE table_name = 'UsuarioProdutor' 
    AND table_schema = 'public'
ORDER BY ordinal_position;

RAISE NOTICE 'Script executado com sucesso! Campos de telefone e email adicionados à tabela Produtor e tabela UsuarioProdutor verificada/criada.';