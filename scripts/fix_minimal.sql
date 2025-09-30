-- Script mínimo para resolver o problema do Fornecedor

-- 1. Criar e popular tabela paises
CREATE TABLE IF NOT EXISTS public.paises (
    id SERIAL PRIMARY KEY,
    codigo VARCHAR(3) NOT NULL UNIQUE,
    nome VARCHAR(100) NOT NULL,
    ativo BOOLEAN NOT NULL DEFAULT true,
    data_criacao TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    data_atualizacao TIMESTAMP
);

INSERT INTO public.paises (codigo, nome) VALUES ('BRA', 'Brasil') ON CONFLICT (codigo) DO NOTHING;

-- 2. Criar e popular tabela estados_referencia
CREATE TABLE IF NOT EXISTS public.estados_referencia (
    id SERIAL PRIMARY KEY,
    uf VARCHAR(2) NOT NULL UNIQUE,
    nome VARCHAR(100) NOT NULL,
    pais_id INTEGER NOT NULL DEFAULT 1,
    ativo BOOLEAN NOT NULL DEFAULT true,
    data_criacao TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    data_atualizacao TIMESTAMP,
    FOREIGN KEY (pais_id) REFERENCES public.paises(id)
);

INSERT INTO public.estados_referencia (uf, nome) VALUES 
('SP', 'São Paulo'), ('RJ', 'Rio de Janeiro'), ('MG', 'Minas Gerais'), 
('BA', 'Bahia'), ('PR', 'Paraná'), ('RS', 'Rio Grande do Sul'),
('PE', 'Pernambuco'), ('CE', 'Ceará'), ('PA', 'Pará'), ('SC', 'Santa Catarina'),
('GO', 'Goiás'), ('MA', 'Maranhão'), ('ES', 'Espírito Santo'), ('PB', 'Paraíba'),
('AM', 'Amazonas'), ('RN', 'Rio Grande do Norte'), ('MT', 'Mato Grosso'),
('MS', 'Mato Grosso do Sul'), ('DF', 'Distrito Federal'), ('SE', 'Sergipe'),
('AL', 'Alagoas'), ('RO', 'Rondônia'), ('PI', 'Piauí'), ('AC', 'Acre'),
('TO', 'Tocantins'), ('RR', 'Roraima'), ('AP', 'Amapá')
ON CONFLICT (uf) DO NOTHING;

-- 3. Criar e popular tabela municipios_referencia
CREATE TABLE IF NOT EXISTS public.municipios_referencia (
    id SERIAL PRIMARY KEY,
    nome VARCHAR(100) NOT NULL,
    codigo_ibge VARCHAR(10) NOT NULL UNIQUE,
    uf_id INTEGER NOT NULL,
    ativo BOOLEAN NOT NULL DEFAULT true,
    data_criacao TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    data_atualizacao TIMESTAMP,
    FOREIGN KEY (uf_id) REFERENCES public.estados_referencia(id)
);

-- Inserir capitais principais
INSERT INTO public.municipios_referencia (nome, codigo_ibge, uf_id) VALUES
('São Paulo', '3550308', (SELECT id FROM public.estados_referencia WHERE uf = 'SP')),
('Rio de Janeiro', '3304557', (SELECT id FROM public.estados_referencia WHERE uf = 'RJ')),
('Belo Horizonte', '3106200', (SELECT id FROM public.estados_referencia WHERE uf = 'MG')),
('Salvador', '2927408', (SELECT id FROM public.estados_referencia WHERE uf = 'BA')),
('Brasília', '5300108', (SELECT id FROM public.estados_referencia WHERE uf = 'DF')),
('Fortaleza', '2304400', (SELECT id FROM public.estados_referencia WHERE uf = 'CE')),
('Manaus', '1302603', (SELECT id FROM public.estados_referencia WHERE uf = 'AM')),
('Curitiba', '4106902', (SELECT id FROM public.estados_referencia WHERE uf = 'PR')),
('Recife', '2611606', (SELECT id FROM public.estados_referencia WHERE uf = 'PE')),
('Porto Alegre', '4314902', (SELECT id FROM public.estados_referencia WHERE uf = 'RS'))
ON CONFLICT (codigo_ibge) DO NOTHING;

-- 4. Verificar
SELECT 'paises' as tabela, COUNT(*) as total FROM public.paises
UNION ALL
SELECT 'estados_referencia', COUNT(*) FROM public.estados_referencia  
UNION ALL
SELECT 'municipios_referencia', COUNT(*) FROM public.municipios_referencia;