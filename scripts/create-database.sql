-- Script para criar o banco de dados DBAgriis
-- Execute este script como superusuário do PostgreSQL

-- Criar o banco de dados
CREATE DATABASE "DBAgriis"
    WITH 
    OWNER = postgres
    ENCODING = 'UTF8'
    LC_COLLATE = 'en_US.utf8'
    LC_CTYPE = 'en_US.utf8'
    TABLESPACE = pg_default
    CONNECTION LIMIT = -1
    IS_TEMPLATE = False;

-- Comentário do banco
COMMENT ON DATABASE "DBAgriis" IS 'Banco de dados principal do sistema Agriis';

-- Conectar ao banco recém-criado para configurações adicionais
\c "DBAgriis";

-- Criar extensões necessárias (quando disponíveis)
-- CREATE EXTENSION IF NOT EXISTS "uuid-ossp";
-- CREATE EXTENSION IF NOT EXISTS "postgis"; -- Quando PostGIS estiver disponível

-- Criar schema público se não existir
CREATE SCHEMA IF NOT EXISTS public;

-- Dar permissões ao usuário postgres
GRANT ALL PRIVILEGES ON DATABASE "DBAgriis" TO postgres;
GRANT ALL ON SCHEMA public TO postgres;