-- Script para corrigir as referências do módulo Fornecedores
-- Criando tabelas do módulo Referencias que são diferentes das tabelas do módulo Enderecos

-- 1. Criar tabela paises (se não existir)
CREATE TABLE IF NOT EXISTS public.paises (
    id SERIAL PRIMARY KEY,
    codigo VARCHAR(3) NOT NULL,
    nome VARCHAR(100) NOT NULL,
    ativo BOOLEAN NOT NULL DEFAULT true,
    data_criacao TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    data_atualizacao TIMESTAMP,
    
    CONSTRAINT UK_paises_codigo UNIQUE (codigo)
);

-- Criar índices para paises
CREATE INDEX IF NOT EXISTS IX_paises_codigo ON public.paises(codigo);
CREATE INDEX IF NOT EXISTS IX_paises_nome ON public.paises(nome);
CREATE INDEX IF NOT EXISTS IX_paises_ativo ON public.paises(ativo);
CREATE INDEX IF NOT EXISTS IX_paises_data_criacao ON public.paises(data_criacao);

-- Inserir Brasil como país padrão (se não existir)
INSERT INTO public.paises (codigo, nome, ativo) 
VALUES ('BRA', 'Brasil', true)
ON CONFLICT (codigo) DO NOTHING;

-- 2. Criar tabela estados_referencia (para o módulo Referencias)
CREATE TABLE IF NOT EXISTS public.estados_referencia (
    id SERIAL PRIMARY KEY,
    uf VARCHAR(2) NOT NULL,
    nome VARCHAR(100) NOT NULL,
    pais_id INTEGER NOT NULL DEFAULT 1,
    ativo BOOLEAN NOT NULL DEFAULT true,
    data_criacao TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    data_atualizacao TIMESTAMP,
    
    CONSTRAINT UK_estados_referencia_uf UNIQUE (uf),
    CONSTRAINT FK_estados_referencia_pais FOREIGN KEY (pais_id) REFERENCES public.paises(id)
);

-- 3. Criar índices para estados_referencia
CREATE INDEX IF NOT EXISTS IX_estados_referencia_uf ON public.estados_referencia(uf);
CREATE INDEX IF NOT EXISTS IX_estados_referencia_nome ON public.estados_referencia(nome);
CREATE INDEX IF NOT EXISTS IX_estados_referencia_ativo ON public.estados_referencia(ativo);
CREATE INDEX IF NOT EXISTS IX_estados_referencia_data_criacao ON public.estados_referencia(data_criacao);

-- 4. Criar tabela municipios_referencia (para o módulo Referencias)
CREATE TABLE IF NOT EXISTS public.municipios_referencia (
    id SERIAL PRIMARY KEY,
    nome VARCHAR(100) NOT NULL,
    codigo_ibge VARCHAR(10) NOT NULL,
    uf_id INTEGER NOT NULL,
    ativo BOOLEAN NOT NULL DEFAULT true,
    data_criacao TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    data_atualizacao TIMESTAMP,
    
    CONSTRAINT UK_municipios_referencia_codigo_ibge UNIQUE (codigo_ibge),
    CONSTRAINT FK_municipios_referencia_uf FOREIGN KEY (uf_id) REFERENCES public.estados_referencia(id)
);

-- 5. Criar índices para municipios_referencia
CREATE INDEX IF NOT EXISTS IX_municipios_referencia_nome ON public.municipios_referencia(nome);
CREATE INDEX IF NOT EXISTS IX_municipios_referencia_codigo_ibge ON public.municipios_referencia(codigo_ibge);
CREATE INDEX IF NOT EXISTS IX_municipios_referencia_uf_id ON public.municipios_referencia(uf_id);
CREATE INDEX IF NOT EXISTS IX_municipios_referencia_ativo ON public.municipios_referencia(ativo);
CREATE INDEX IF NOT EXISTS IX_municipios_referencia_data_criacao ON public.municipios_referencia(data_criacao);

-- 6. Popular estados_referencia com dados básicos do Brasil (se não existirem)
-- Primeiro, verificar se a tabela estados existe e popular com dados básicos
DO $$
BEGIN
    IF EXISTS (SELECT FROM information_schema.tables WHERE table_schema = 'public' AND table_name = 'estados') THEN
        INSERT INTO public.estados_referencia (uf, nome, pais_id, ativo) 
        SELECT uf, nome, 1, ativo 
        FROM public.estados 
        WHERE NOT EXISTS (
            SELECT 1 FROM public.estados_referencia er WHERE er.uf = estados.uf
        )
        ON CONFLICT (uf) DO NOTHING;
    ELSE
        -- Se não existe a tabela estados, inserir dados básicos do Brasil
        INSERT INTO public.estados_referencia (uf, nome, pais_id, ativo) VALUES
        ('AC', 'Acre', 1, true),
        ('AL', 'Alagoas', 1, true),
        ('AP', 'Amapá', 1, true),
        ('AM', 'Amazonas', 1, true),
        ('BA', 'Bahia', 1, true),
        ('CE', 'Ceará', 1, true),
        ('DF', 'Distrito Federal', 1, true),
        ('ES', 'Espírito Santo', 1, true),
        ('GO', 'Goiás', 1, true),
        ('MA', 'Maranhão', 1, true),
        ('MT', 'Mato Grosso', 1, true),
        ('MS', 'Mato Grosso do Sul', 1, true),
        ('MG', 'Minas Gerais', 1, true),
        ('PA', 'Pará', 1, true),
        ('PB', 'Paraíba', 1, true),
        ('PR', 'Paraná', 1, true),
        ('PE', 'Pernambuco', 1, true),
        ('PI', 'Piauí', 1, true),
        ('RJ', 'Rio de Janeiro', 1, true),
        ('RN', 'Rio Grande do Norte', 1, true),
        ('RS', 'Rio Grande do Sul', 1, true),
        ('RO', 'Rondônia', 1, true),
        ('RR', 'Roraima', 1, true),
        ('SC', 'Santa Catarina', 1, true),
        ('SP', 'São Paulo', 1, true),
        ('SE', 'Sergipe', 1, true),
        ('TO', 'Tocantins', 1, true)
        ON CONFLICT (uf) DO NOTHING;
    END IF;
END $$;

-- 7. Popular municipios_referencia com dados básicos (se não existirem)
-- Verificar se as tabelas municipios e estados existem para popular os dados
DO $$
BEGIN
    IF EXISTS (SELECT FROM information_schema.tables WHERE table_schema = 'public' AND table_name = 'municipios') 
       AND EXISTS (SELECT FROM information_schema.tables WHERE table_schema = 'public' AND table_name = 'estados') THEN
        INSERT INTO public.municipios_referencia (nome, codigo_ibge, uf_id, ativo)
        SELECT 
            m.nome,
            CAST(m.codigo_ibge AS VARCHAR(10)),
            er.id,
            m.ativo
        FROM public.municipios m
        INNER JOIN public.estados e ON CAST(m.estado_id AS INTEGER) = CAST(e.id AS INTEGER)
        INNER JOIN public.estados_referencia er ON CAST(er.uf AS VARCHAR(2)) = CAST(e.uf AS VARCHAR(2))
        WHERE NOT EXISTS (
            SELECT 1 FROM public.municipios_referencia mr WHERE mr.codigo_ibge = CAST(m.codigo_ibge AS VARCHAR(10))
        )
        ON CONFLICT (codigo_ibge) DO NOTHING;
    ELSE
        -- Se não existem as tabelas de origem, inserir alguns municípios básicos como exemplo
        INSERT INTO public.municipios_referencia (nome, codigo_ibge, uf_id, ativo)
        SELECT 'São Paulo', '3550308', er.id, true FROM public.estados_referencia er WHERE er.uf = 'SP'
        UNION ALL
        SELECT 'Rio de Janeiro', '3304557', er.id, true FROM public.estados_referencia er WHERE er.uf = 'RJ'
        UNION ALL
        SELECT 'Belo Horizonte', '3106200', er.id, true FROM public.estados_referencia er WHERE er.uf = 'MG'
        UNION ALL
        SELECT 'Brasília', '5300108', er.id, true FROM public.estados_referencia er WHERE er.uf = 'DF'
        ON CONFLICT (codigo_ibge) DO NOTHING;
    END IF;
END $$;

-- 8. Verificar se as tabelas foram criadas corretamente
SELECT 'paises' as tabela, COUNT(*) as registros FROM public.paises
UNION ALL
SELECT 'estados_referencia' as tabela, COUNT(*) as registros FROM public.estados_referencia
UNION ALL
SELECT 'municipios_referencia' as tabela, COUNT(*) as registros FROM public.municipios_referencia;