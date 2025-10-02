-- =====================================================
-- Geographic Tables Unification - Migration Transaction Wrapper
-- =====================================================
-- This script wraps all migration operations in a transaction
-- with comprehensive error handling and rollback procedures
-- =====================================================

-- Start main migration transaction
BEGIN;

-- Set transaction isolation level for consistency
SET TRANSACTION ISOLATION LEVEL SERIALIZABLE;

-- Create savepoints for granular rollback control
SAVEPOINT migration_start;

DO $$
DECLARE
    migration_start_time TIMESTAMPTZ;
    step_start_time TIMESTAMPTZ;
    error_occurred BOOLEAN := FALSE;
    error_message TEXT;
    error_detail TEXT;
    error_context TEXT;
BEGIN
    -- Initialize migration
    migration_start_time := CURRENT_TIMESTAMP;
    
    -- Log migration start
    INSERT INTO migration_log (step, status, message) 
    VALUES ('full_migration', 'started', 'Starting complete geographic tables unification migration');
    
    BEGIN
        -- Step 1: Setup and Backup
        step_start_time := CURRENT_TIMESTAMP;
        RAISE NOTICE 'Step 1: Running setup and backup...';
        
        -- Execute setup script content inline
        -- (Migration log table and functions should already exist)
        
        -- Execute backup script content inline
        SAVEPOINT backup_step;
        
        -- Create backup tables
        DROP TABLE IF EXISTS estados_referencia_backup CASCADE;
        CREATE TABLE estados_referencia_backup AS SELECT * FROM estados_referencia;
        
        DROP TABLE IF EXISTS municipios_referencia_backup CASCADE;
        CREATE TABLE municipios_referencia_backup AS SELECT * FROM municipios_referencia;
        
        DROP TABLE IF EXISTS estados_backup CASCADE;
        CREATE TABLE estados_backup AS SELECT * FROM estados;
        
        DROP TABLE IF EXISTS municipios_backup CASCADE;
        CREATE TABLE municipios_backup AS SELECT * FROM municipios;
        
        DROP TABLE IF EXISTS "Fornecedor_backup" CASCADE;
        CREATE TABLE "Fornecedor_backup" AS SELECT * FROM "Fornecedor";
        
        INSERT INTO migration_log (step, status, message) 
        VALUES ('backup', 'completed', 
                FORMAT('Backup completed in %s seconds', 
                       EXTRACT(EPOCH FROM (CURRENT_TIMESTAMP - step_start_time))));
        
    EXCEPTION
        WHEN OTHERS THEN
            ROLLBACK TO SAVEPOINT backup_step;
            error_occurred := TRUE;
            GET STACKED DIAGNOSTICS 
                error_message = MESSAGE_TEXT,
                error_detail = PG_EXCEPTION_DETAIL,
                error_context = PG_EXCEPTION_CONTEXT;
            
            INSERT INTO migration_log (step, status, message, error_details) 
            VALUES ('backup', 'failed', error_message, error_detail);
            
            RAISE EXCEPTION 'Backup step failed: %', error_message;
    END;
    
    BEGIN
        -- Step 2: Estado Unification
        step_start_time := CURRENT_TIMESTAMP;
        RAISE NOTICE 'Step 2: Running estado unification...';
        
        SAVEPOINT estado_unification_step;
        
        -- Execute estado unification inline
        INSERT INTO estados (nome, uf, codigo_ibge, regiao, data_criacao, data_atualizacao, ativo)
        SELECT DISTINCT 
            er.nome,
            er.uf,
            generate_codigo_ibge(er.uf) as codigo_ibge,
            map_uf_to_regiao(er.uf) as regiao,
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
        IF (SELECT COUNT(*) FROM estados e INNER JOIN estados_referencia er ON e.uf = er.uf) = 
           (SELECT COUNT(*) FROM estados_referencia WHERE uf IS NOT NULL AND uf != '') THEN
            
            INSERT INTO migration_log (step, status, message) 
            VALUES ('estado_unification', 'completed', 
                    FORMAT('Estado unification completed in %s seconds', 
                           EXTRACT(EPOCH FROM (CURRENT_TIMESTAMP - step_start_time))));
        ELSE
            RAISE EXCEPTION 'Estado unification validation failed';
        END IF;
        
    EXCEPTION
        WHEN OTHERS THEN
            ROLLBACK TO SAVEPOINT estado_unification_step;
            error_occurred := TRUE;
            GET STACKED DIAGNOSTICS 
                error_message = MESSAGE_TEXT,
                error_detail = PG_EXCEPTION_DETAIL,
                error_context = PG_EXCEPTION_CONTEXT;
            
            INSERT INTO migration_log (step, status, message, error_details) 
            VALUES ('estado_unification', 'failed', error_message, error_detail);
            
            RAISE EXCEPTION 'Estado unification step failed: %', error_message;
    END;
    
    BEGIN
        -- Step 3: Municipio Unification
        step_start_time := CURRENT_TIMESTAMP;
        RAISE NOTICE 'Step 3: Running municipio unification...';
        
        SAVEPOINT municipio_unification_step;
        
        -- Execute municipio unification inline
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
        DECLARE
            valid_municipios_count INTEGER;
            expected_count INTEGER;
        BEGIN
            SELECT COUNT(*) INTO valid_municipios_count
            FROM municipios m
            INNER JOIN estados e ON m.estado_id = e.id;
            
            SELECT COUNT(*) INTO expected_count
            FROM municipios_backup
            UNION ALL
            SELECT COUNT(*) 
            FROM municipios_referencia 
            WHERE codigo_ibge ~ '^[0-9]+$';
            
            IF valid_municipios_count >= expected_count THEN
                INSERT INTO migration_log (step, status, message) 
                VALUES ('municipio_unification', 'completed', 
                        FORMAT('Municipio unification completed in %s seconds', 
                               EXTRACT(EPOCH FROM (CURRENT_TIMESTAMP - step_start_time))));
            ELSE
                RAISE EXCEPTION 'Municipio unification validation failed: expected >= %, got %', 
                               expected_count, valid_municipios_count;
            END IF;
        END;
        
    EXCEPTION
        WHEN OTHERS THEN
            ROLLBACK TO SAVEPOINT municipio_unification_step;
            error_occurred := TRUE;
            GET STACKED DIAGNOSTICS 
                error_message = MESSAGE_TEXT,
                error_detail = PG_EXCEPTION_DETAIL,
                error_context = PG_EXCEPTION_CONTEXT;
            
            INSERT INTO migration_log (step, status, message, error_details) 
            VALUES ('municipio_unification', 'failed', error_message, error_detail);
            
            RAISE EXCEPTION 'Municipio unification step failed: %', error_message;
    END;
    
    BEGIN
        -- Step 4: Foreign Key Updates
        step_start_time := CURRENT_TIMESTAMP;
        RAISE NOTICE 'Step 4: Running foreign key updates...';
        
        SAVEPOINT foreign_key_updates_step;
        
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
        
        -- Validate foreign key updates
        DECLARE
            invalid_fk_count INTEGER;
        BEGIN
            SELECT COUNT(*) INTO invalid_fk_count
            FROM "Fornecedor" f
            LEFT JOIN estados e ON f."UfId" = e.id
            LEFT JOIN municipios m ON f."MunicipioId" = m.id
            WHERE (f."UfId" IS NOT NULL AND e.id IS NULL) OR
                  (f."MunicipioId" IS NOT NULL AND m.id IS NULL);
            
            IF invalid_fk_count = 0 THEN
                INSERT INTO migration_log (step, status, message) 
                VALUES ('foreign_key_updates', 'completed', 
                        FORMAT('Foreign key updates completed in %s seconds', 
                               EXTRACT(EPOCH FROM (CURRENT_TIMESTAMP - step_start_time))));
            ELSE
                RAISE EXCEPTION 'Foreign key validation failed: % invalid references found', invalid_fk_count;
            END IF;
        END;
        
    EXCEPTION
        WHEN OTHERS THEN
            ROLLBACK TO SAVEPOINT foreign_key_updates_step;
            error_occurred := TRUE;
            GET STACKED DIAGNOSTICS 
                error_message = MESSAGE_TEXT,
                error_detail = PG_EXCEPTION_DETAIL,
                error_context = PG_EXCEPTION_CONTEXT;
            
            INSERT INTO migration_log (step, status, message, error_details) 
            VALUES ('foreign_key_updates', 'failed', error_message, error_detail);
            
            RAISE EXCEPTION 'Foreign key updates step failed: %', error_message;
    END;
    
    -- Final validation before committing
    BEGIN
        RAISE NOTICE 'Running final validation...';
        
        DECLARE
            final_estados_count INTEGER;
            final_municipios_count INTEGER;
            final_fk_issues INTEGER;
        BEGIN
            SELECT COUNT(*) INTO final_estados_count FROM estados;
            SELECT COUNT(*) INTO final_municipios_count FROM municipios;
            
            SELECT COUNT(*) INTO final_fk_issues
            FROM "Fornecedor" f
            LEFT JOIN estados e ON f."UfId" = e.id
            LEFT JOIN municipios m ON f."MunicipioId" = m.id
            WHERE (f."UfId" IS NOT NULL AND e.id IS NULL) OR
                  (f."MunicipioId" IS NOT NULL AND m.id IS NULL);
            
            IF final_estados_count >= (SELECT COUNT(*) FROM estados_backup) AND
               final_municipios_count >= (SELECT COUNT(*) FROM municipios_backup) AND
               final_fk_issues = 0 THEN
                
                INSERT INTO migration_log (step, status, message) 
                VALUES ('final_validation', 'success', 
                        FORMAT('Migration validation passed: Estados=%s, Municipios=%s, FK Issues=%s', 
                               final_estados_count, final_municipios_count, final_fk_issues));
            ELSE
                RAISE EXCEPTION 'Final validation failed: Estados=%, Municipios=%, FK Issues=%', 
                               final_estados_count, final_municipios_count, final_fk_issues;
            END IF;
        END;
    END;
    
    -- Log successful completion
    INSERT INTO migration_log (step, status, message) 
    VALUES ('full_migration', 'completed', 
            FORMAT('Complete migration finished successfully in %s seconds', 
                   EXTRACT(EPOCH FROM (CURRENT_TIMESTAMP - migration_start_time))));
    
    RAISE NOTICE 'Migration completed successfully!';
    
EXCEPTION
    WHEN OTHERS THEN
        -- Log the error
        GET STACKED DIAGNOSTICS 
            error_message = MESSAGE_TEXT,
            error_detail = PG_EXCEPTION_DETAIL,
            error_context = PG_EXCEPTION_CONTEXT;
        
        INSERT INTO migration_log (step, status, message, error_details) 
        VALUES ('full_migration', 'failed', error_message, 
                FORMAT('Detail: %s, Context: %s', error_detail, error_context));
        
        -- Rollback to start
        ROLLBACK TO SAVEPOINT migration_start;
        
        RAISE NOTICE 'Migration failed and was rolled back: %', error_message;
        RAISE;
END $$;

-- If we reach here, commit the transaction
COMMIT;

-- Generate final migration report
SELECT 
    'MIGRATION REPORT' as section,
    '' as detail
UNION ALL
SELECT 
    'Migration Status',
    CASE 
        WHEN (SELECT COUNT(*) FROM migration_log WHERE step = 'full_migration' AND status = 'completed') > 0
        THEN 'SUCCESS'
        ELSE 'FAILED'
    END
UNION ALL
SELECT 
    'Total Estados',
    (SELECT COUNT(*)::text FROM estados)
UNION ALL
SELECT 
    'Total Municipios', 
    (SELECT COUNT(*)::text FROM municipios)
UNION ALL
SELECT 
    'Fornecedor FK Updates',
    (SELECT COUNT(*)::text FROM "Fornecedor" f 
     INNER JOIN estados e ON f."UfId" = e.id)
UNION ALL
SELECT 
    'Migration Duration',
    (SELECT EXTRACT(EPOCH FROM (MAX(created_at) - MIN(created_at)))::text || ' seconds'
     FROM migration_log 
     WHERE step = 'full_migration')
UNION ALL
SELECT 
    'Backup Tables Created',
    CASE 
        WHEN EXISTS (SELECT 1 FROM information_schema.tables WHERE table_name = 'estados_referencia_backup')
        THEN 'YES'
        ELSE 'NO'
    END;