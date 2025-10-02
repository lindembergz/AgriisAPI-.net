-- =====================================================
-- Geographic Tables Unification - Main Transaction
-- =====================================================
-- This script wraps all migration operations in a transaction
-- with rollback procedures and error handling
-- =====================================================

-- Start main transaction
BEGIN;

-- Set transaction isolation level for consistency
SET TRANSACTION ISOLATION LEVEL SERIALIZABLE;

-- Log transaction start
INSERT INTO migration_log (step, status, message) 
VALUES ('main_transaction', 'started', 'Starting main migration transaction');

-- Create savepoints for rollback granularity
SAVEPOINT setup_complete;

DO $$
DECLARE
    error_occurred BOOLEAN := FALSE;
    error_message TEXT;
    step_name TEXT;
BEGIN
    -- Step 1: Setup and Backup
    step_name := 'setup_and_backup';
    BEGIN
        -- Execute setup script
        -- Note: In practice, this would be executed separately
        INSERT INTO migration_log (step, status, message) 
        VALUES (step_name, 'executing', 'Running setup and backup scripts');
        
        -- Verify backup tables exist
        IF NOT EXISTS (SELECT 1 FROM information_schema.tables WHERE table_name = 'estados_referencia_backup') THEN
            RAISE EXCEPTION 'Backup table estados_referencia_backup not found';
        END IF;
        
        IF NOT EXISTS (SELECT 1 FROM information_schema.tables WHERE table_name = 'municipios_referencia_backup') THEN
            RAISE EXCEPTION 'Backup table municipios_referencia_backup not found';
        END IF;
        
        INSERT INTO migration_log (step, status, message) 
        VALUES (step_name, 'completed', 'Setup and backup completed successfully');
        
    EXCEPTION WHEN OTHERS THEN
        error_occurred := TRUE;
        error_message := SQLERRM;
        INSERT INTO migration_log (step, status, message, error_details) 
        VALUES (step_name, 'failed', 'Setup and backup failed', error_message);
        RAISE;
    END;
    
    SAVEPOINT backup_complete;
    
    -- Step 2: Estado Unification
    step_name := 'estado_unification_transaction';
    BEGIN
        INSERT INTO migration_log (step, status, message) 
        VALUES (step_name, 'executing', 'Running estado unification in transaction');
        
        -- Execute estado unification logic
        INSERT INTO estados (nome, uf, codigo_ibge, regiao, pais_id, data_criacao, data_atualizacao, ativo)
        SELECT DISTINCT 
            er.nome,
            er.uf,
            generate_codigo_ibge(er.uf) as codigo_ibge,
            map_uf_to_regiao(er.uf) as regiao,
            COALESCE(er.pais_id, 1) as pais_id,
            er.data_criacao,
            er.data_atualizacao,
            er.ativo
        FROM estados_referencia er
        LEFT JOIN estados e ON e.uf = er.uf
        WHERE e.id IS NULL
          AND er.uf IS NOT NULL 
          AND er.uf != ''
          AND er.nome IS NOT NULL
          AND er.nome != '';
        
        -- Validate estado unification
        IF EXISTS (
            SELECT 1 FROM (
                SELECT uf, COUNT(*) 
                FROM estados 
                GROUP BY uf 
                HAVING COUNT(*) > 1
            ) duplicates
        ) THEN
            RAISE EXCEPTION 'Duplicate UF codes found after estado unification';
        END IF;
        
        INSERT INTO migration_log (step, status, message) 
        VALUES (step_name, 'completed', 'Estado unification completed successfully');
        
    EXCEPTION WHEN OTHERS THEN
        error_occurred := TRUE;
        error_message := SQLERRM;
        INSERT INTO migration_log (step, status, message, error_details) 
        VALUES (step_name, 'failed', 'Estado unification failed', error_message);
        ROLLBACK TO backup_complete;
        RAISE;
    END;
    
    SAVEPOINT estado_complete;
    
    -- Step 3: Municipio Unification
    step_name := 'municipio_unification_transaction';
    BEGIN
        INSERT INTO migration_log (step, status, message) 
        VALUES (step_name, 'executing', 'Running municipio unification in transaction');
        
        -- Execute municipio unification logic
        INSERT INTO municipios (nome, codigo_ibge, estado_id, data_criacao, data_atualizacao, ativo)
        SELECT DISTINCT 
            mr.nome,
            mr.codigo_ibge::integer as codigo_ibge,
            e.id as estado_id,
            mr.data_criacao,
            mr.data_atualizacao,
            mr.ativo
        FROM municipios_referencia mr
        INNER JOIN estados_referencia er ON mr.uf_id = er.id
        INNER JOIN estados e ON e.uf = er.uf
        LEFT JOIN municipios m ON m.codigo_ibge = mr.codigo_ibge::integer
        WHERE m.id IS NULL
          AND mr.codigo_ibge IS NOT NULL 
          AND mr.codigo_ibge != ''
          AND mr.codigo_ibge ~ '^[0-9]+$'
          AND mr.nome IS NOT NULL
          AND mr.nome != ''
          AND mr.uf_id IS NOT NULL;
        
        -- Validate municipio unification
        IF EXISTS (
            SELECT 1 FROM (
                SELECT codigo_ibge, COUNT(*) 
                FROM municipios 
                GROUP BY codigo_ibge 
                HAVING COUNT(*) > 1
            ) duplicates
        ) THEN
            RAISE EXCEPTION 'Duplicate codigo_ibge found after municipio unification';
        END IF;
        
        INSERT INTO migration_log (step, status, message) 
        VALUES (step_name, 'completed', 'Municipio unification completed successfully');
        
    EXCEPTION WHEN OTHERS THEN
        error_occurred := TRUE;
        error_message := SQLERRM;
        INSERT INTO migration_log (step, status, message, error_details) 
        VALUES (step_name, 'failed', 'Municipio unification failed', error_message);
        ROLLBACK TO estado_complete;
        RAISE;
    END;
    
    SAVEPOINT municipio_complete;
    
    -- Step 4: Foreign Key Updates
    step_name := 'foreign_key_updates_transaction';
    BEGIN
        INSERT INTO migration_log (step, status, message) 
        VALUES (step_name, 'executing', 'Running foreign key updates in transaction');
        
        -- Update Fornecedor UfId references
        UPDATE "Fornecedor" 
        SET "UfId" = e.id,
            "DataAtualizacao" = CURRENT_TIMESTAMP
        FROM estados_referencia er
        INNER JOIN estados e ON e.uf = er.uf
        WHERE "Fornecedor"."UfId" = er.id;
        
        -- Update Fornecedor MunicipioId references
        UPDATE "Fornecedor" 
        SET "MunicipioId" = m.id,
            "DataAtualizacao" = CURRENT_TIMESTAMP
        FROM municipios_referencia mr
        INNER JOIN estados_referencia er ON mr.uf_id = er.id
        INNER JOIN estados e ON e.uf = er.uf
        INNER JOIN municipios m ON m.codigo_ibge = mr.codigo_ibge::integer AND m.estado_id = e.id
        WHERE "Fornecedor"."MunicipioId" = mr.id
          AND mr.codigo_ibge ~ '^[0-9]+$';
        
        -- Validate foreign key integrity
        IF EXISTS (
            SELECT 1 FROM "Fornecedor" f
            LEFT JOIN estados e ON f."UfId" = e.id
            WHERE f."UfId" IS NOT NULL AND e.id IS NULL
        ) THEN
            RAISE EXCEPTION 'Invalid UfId references found after foreign key updates';
        END IF;
        
        IF EXISTS (
            SELECT 1 FROM "Fornecedor" f
            LEFT JOIN municipios m ON f."MunicipioId" = m.id
            WHERE f."MunicipioId" IS NOT NULL AND m.id IS NULL
        ) THEN
            RAISE EXCEPTION 'Invalid MunicipioId references found after foreign key updates';
        END IF;
        
        INSERT INTO migration_log (step, status, message) 
        VALUES (step_name, 'completed', 'Foreign key updates completed successfully');
        
    EXCEPTION WHEN OTHERS THEN
        error_occurred := TRUE;
        error_message := SQLERRM;
        INSERT INTO migration_log (step, status, message, error_details) 
        VALUES (step_name, 'failed', 'Foreign key updates failed', error_message);
        ROLLBACK TO municipio_complete;
        RAISE;
    END;
    
    SAVEPOINT foreign_keys_complete;
    
    -- Step 5: Final Validation
    step_name := 'final_validation';
    BEGIN
        INSERT INTO migration_log (step, status, message) 
        VALUES (step_name, 'executing', 'Running final validation');
        
        -- Comprehensive validation checks
        DECLARE
            estados_count INTEGER;
            municipios_count INTEGER;
            fornecedor_valid_count INTEGER;
        BEGIN
            SELECT COUNT(*) INTO estados_count FROM estados;
            SELECT COUNT(*) INTO municipios_count FROM municipios;
            
            SELECT COUNT(*) INTO fornecedor_valid_count
            FROM "Fornecedor" f
            LEFT JOIN estados e ON f."UfId" = e.id
            LEFT JOIN municipios m ON f."MunicipioId" = m.id
            WHERE (f."UfId" IS NULL OR e.id IS NOT NULL)
              AND (f."MunicipioId" IS NULL OR m.id IS NOT NULL);
            
            -- Validate minimum expected counts
            IF estados_count < 27 THEN
                RAISE EXCEPTION 'Estados count (%) is less than expected minimum (27)', estados_count;
            END IF;
            
            IF municipios_count < 5000 THEN
                RAISE EXCEPTION 'Municipios count (%) is less than expected minimum (5000)', municipios_count;
            END IF;
            
            IF fornecedor_valid_count != (SELECT COUNT(*) FROM "Fornecedor") THEN
                RAISE EXCEPTION 'Not all Fornecedor records have valid geographic references';
            END IF;
        END;
        
        INSERT INTO migration_log (step, status, message) 
        VALUES (step_name, 'completed', 'Final validation completed successfully');
        
    EXCEPTION WHEN OTHERS THEN
        error_occurred := TRUE;
        error_message := SQLERRM;
        INSERT INTO migration_log (step, status, message, error_details) 
        VALUES (step_name, 'failed', 'Final validation failed', error_message);
        ROLLBACK TO foreign_keys_complete;
        RAISE;
    END;
    
END $$;

-- If we reach here, all steps completed successfully
INSERT INTO migration_log (step, status, message) 
VALUES ('main_transaction', 'completed', 'All migration steps completed successfully');

-- Generate final migration report
SELECT 
    'Migration Summary Report' as report_section,
    step,
    status,
    message,
    created_at
FROM migration_log 
WHERE created_at >= (
    SELECT created_at 
    FROM migration_log 
    WHERE step = 'main_transaction' AND status = 'started'
    ORDER BY created_at DESC 
    LIMIT 1
)
ORDER BY created_at;

-- Commit the transaction
COMMIT;

-- Post-commit validation and cleanup
DO $$
BEGIN
    -- Log successful commit
    INSERT INTO migration_log (step, status, message) 
    VALUES ('transaction_commit', 'completed', 'Migration transaction committed successfully');
    
    -- Final statistics
    INSERT INTO migration_log (step, status, message) 
    VALUES ('final_statistics', 'info', 
            FORMAT('Final counts - Estados: %s, Municipios: %s, Fornecedores: %s',
                   (SELECT COUNT(*) FROM estados),
                   (SELECT COUNT(*) FROM municipios),
                   (SELECT COUNT(*) FROM "Fornecedor")));
END $$;

-- Display success message
SELECT 
    'MIGRATION COMPLETED SUCCESSFULLY' as status,
    'All geographic tables have been unified' as message,
    'Check migration_log table for detailed results' as next_steps;