-- =====================================================
-- Geographic Tables Unification - Migration Setup
-- =====================================================
-- This script sets up the migration environment
-- =====================================================

-- Create migration log table for tracking progress
CREATE TABLE IF NOT EXISTS migration_log (
    id SERIAL PRIMARY KEY,
    step VARCHAR(100) NOT NULL,
    status VARCHAR(50) NOT NULL,
    message TEXT,
    error_details TEXT,
    created_at TIMESTAMPTZ DEFAULT CURRENT_TIMESTAMP
);

-- Create UF to region mapping function
CREATE OR REPLACE FUNCTION map_uf_to_regiao(uf_code VARCHAR(2))
RETURNS VARCHAR(50) AS $$
BEGIN
    RETURN CASE uf_code
        -- Norte
        WHEN 'AC' THEN 'Norte'
        WHEN 'AP' THEN 'Norte'
        WHEN 'AM' THEN 'Norte'
        WHEN 'PA' THEN 'Norte'
        WHEN 'RO' THEN 'Norte'
        WHEN 'RR' THEN 'Norte'
        WHEN 'TO' THEN 'Norte'
        -- Nordeste
        WHEN 'AL' THEN 'Nordeste'
        WHEN 'BA' THEN 'Nordeste'
        WHEN 'CE' THEN 'Nordeste'
        WHEN 'MA' THEN 'Nordeste'
        WHEN 'PB' THEN 'Nordeste'
        WHEN 'PE' THEN 'Nordeste'
        WHEN 'PI' THEN 'Nordeste'
        WHEN 'RN' THEN 'Nordeste'
        WHEN 'SE' THEN 'Nordeste'
        -- Centro-Oeste
        WHEN 'GO' THEN 'Centro-Oeste'
        WHEN 'MT' THEN 'Centro-Oeste'
        WHEN 'MS' THEN 'Centro-Oeste'
        WHEN 'DF' THEN 'Centro-Oeste'
        -- Sudeste
        WHEN 'ES' THEN 'Sudeste'
        WHEN 'MG' THEN 'Sudeste'
        WHEN 'RJ' THEN 'Sudeste'
        WHEN 'SP' THEN 'Sudeste'
        -- Sul
        WHEN 'PR' THEN 'Sul'
        WHEN 'RS' THEN 'Sul'
        WHEN 'SC' THEN 'Sul'
        ELSE 'Desconhecido'
    END;
END;
$$ LANGUAGE plpgsql;

-- Create IBGE code generation function for estados
CREATE OR REPLACE FUNCTION generate_codigo_ibge(uf_code VARCHAR(2))
RETURNS INTEGER AS $$
BEGIN
    RETURN CASE uf_code
        WHEN 'AC' THEN 12
        WHEN 'AL' THEN 27
        WHEN 'AP' THEN 16
        WHEN 'AM' THEN 13
        WHEN 'BA' THEN 29
        WHEN 'CE' THEN 23
        WHEN 'DF' THEN 53
        WHEN 'ES' THEN 32
        WHEN 'GO' THEN 52
        WHEN 'MA' THEN 21
        WHEN 'MT' THEN 51
        WHEN 'MS' THEN 50
        WHEN 'MG' THEN 31
        WHEN 'PA' THEN 15
        WHEN 'PB' THEN 25
        WHEN 'PR' THEN 41
        WHEN 'PE' THEN 26
        WHEN 'PI' THEN 22
        WHEN 'RJ' THEN 33
        WHEN 'RN' THEN 24
        WHEN 'RS' THEN 43
        WHEN 'RO' THEN 11
        WHEN 'RR' THEN 14
        WHEN 'SC' THEN 42
        WHEN 'SP' THEN 35
        WHEN 'SE' THEN 28
        WHEN 'TO' THEN 17
        ELSE 99
    END;
END;
$$ LANGUAGE plpgsql;

-- Add pais_id column to estados table if it doesn't exist
DO $$
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM information_schema.columns 
        WHERE table_name = 'estados' AND column_name = 'pais_id'
    ) THEN
        ALTER TABLE estados ADD COLUMN pais_id int4 DEFAULT 1 NOT NULL;
        
        -- Add foreign key constraint to paises table if it exists
        IF EXISTS (SELECT 1 FROM information_schema.tables WHERE table_name = 'paises') THEN
            ALTER TABLE estados 
            ADD CONSTRAINT fk_estados_pais 
            FOREIGN KEY (pais_id) REFERENCES paises(id);
        END IF;
        
        -- Create index for pais_id
        CREATE INDEX IF NOT EXISTS IX_estados_pais_id ON estados(pais_id);
        
        INSERT INTO migration_log (step, status, message) 
        VALUES ('add_pais_id_column', 'completed', 'Added pais_id column to estados table');
    ELSE
        INSERT INTO migration_log (step, status, message) 
        VALUES ('add_pais_id_column', 'skipped', 'pais_id column already exists in estados table');
    END IF;
END $$;

-- Log setup completion
INSERT INTO migration_log (step, status, message) 
VALUES ('setup', 'completed', 'Migration environment setup completed');

-- Display setup confirmation
SELECT 'Migration setup completed successfully' as status;