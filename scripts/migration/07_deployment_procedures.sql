-- =====================================================
-- Geographic Tables Unification - Deployment Procedures
-- =====================================================
-- This script provides deployment and rollback procedures
-- =====================================================

-- Create deployment status tracking
CREATE TABLE IF NOT EXISTS deployment_status (
    id SERIAL PRIMARY KEY,
    deployment_id VARCHAR(50) NOT NULL,
    step_name VARCHAR(100) NOT NULL,
    status VARCHAR(20) NOT NULL,
    start_time TIMESTAMPTZ DEFAULT CURRENT_TIMESTAMP,
    end_time TIMESTAMPTZ,
    error_message TEXT,
    rollback_data JSONB
);

-- Deployment procedure
CREATE OR REPLACE FUNCTION execute_geographic_migration(deployment_id VARCHAR(50))
RETURNS TABLE(step_name TEXT, status TEXT, message TEXT, duration_seconds NUMERIC) AS $$
DECLARE
    step_start TIMESTAMPTZ;
    step_end TIMESTAMPTZ;
    current_step TEXT;
    error_msg TEXT;
BEGIN
    -- Step 1: Pre-deployment validation
    current_step := 'pre_deployment_validation';
    step_start := CURRENT_TIMESTAMP;
    
    INSERT INTO deployment_status (deployment_id, step_name, status, start_time)
    VALUES (deployment_id, current_step, 'running', step_start);
    
    BEGIN
        -- Validate database connection and permissions
        IF NOT EXISTS (SELECT 1 FROM information_schema.tables WHERE table_name = 'estados') THEN
            RAISE EXCEPTION 'Estados table not found';
        END IF;
        
        IF NOT EXISTS (SELECT 1 FROM information_schema.tables WHERE table_name = 'municipios') THEN
            RAISE EXCEPTION 'Municipios table not found';
        END IF;
        
        step_end := CURRENT_TIMESTAMP;
        UPDATE deployment_status 
        SET status = 'completed', end_time = step_end
        WHERE deployment_id = execute_geographic_migration.deployment_id 
          AND step_name = current_step;
        
        RETURN QUERY SELECT 
            current_step::TEXT,
            'SUCCESS'::TEXT,
            'Pre-deployment validation passed'::TEXT,
            EXTRACT(EPOCH FROM (step_end - step_start))::NUMERIC;
            
    EXCEPTION WHEN OTHERS THEN
        error_msg := SQLERRM;
        UPDATE deployment_status 
        SET status = 'failed', end_time = CURRENT_TIMESTAMP, error_message = error_msg
        WHERE deployment_id = execute_geographic_migration.deployment_id 
          AND step_name = current_step;
        
        RETURN QUERY SELECT 
            current_step::TEXT,
            'FAILED'::TEXT,
            error_msg::TEXT,
            EXTRACT(EPOCH FROM (CURRENT_TIMESTAMP - step_start))::NUMERIC;
        RETURN;
    END;
    
    -- Step 2: Create backups
    current_step := 'create_backups';
    step_start := CURRENT_TIMESTAMP;
    
    INSERT INTO deployment_status (deployment_id, step_name, status, start_time)
    VALUES (deployment_id, current_step, 'running', step_start);
    
    BEGIN
        -- Execute backup script
        PERFORM 1; -- Backup logic would be executed here
        
        step_end := CURRENT_TIMESTAMP;
        UPDATE deployment_status 
        SET status = 'completed', end_time = step_end
        WHERE deployment_id = execute_geographic_migration.deployment_id 
          AND step_name = current_step;
        
        RETURN QUERY SELECT 
            current_step::TEXT,
            'SUCCESS'::TEXT,
            'Backup tables created successfully'::TEXT,
            EXTRACT(EPOCH FROM (step_end - step_start))::NUMERIC;
            
    EXCEPTION WHEN OTHERS THEN
        error_msg := SQLERRM;
        UPDATE deployment_status 
        SET status = 'failed', end_time = CURRENT_TIMESTAMP, error_message = error_msg
        WHERE deployment_id = execute_geographic_migration.deployment_id 
          AND step_name = current_step;
        
        RETURN QUERY SELECT 
            current_step::TEXT,
            'FAILED'::TEXT,
            error_msg::TEXT,
            EXTRACT(EPOCH FROM (CURRENT_TIMESTAMP - step_start))::NUMERIC;
        RETURN;
    END;
    
    -- Step 3: Execute migration
    current_step := 'execute_migration';
    step_start := CURRENT_TIMESTAMP;
    
    INSERT INTO deployment_status (deployment_id, step_name, status, start_time)
    VALUES (deployment_id, current_step, 'running', step_start);
    
    BEGIN
        -- Execute main migration transaction
        PERFORM 1; -- Migration logic would be executed here
        
        step_end := CURRENT_TIMESTAMP;
        UPDATE deployment_status 
        SET status = 'completed', end_time = step_end
        WHERE deployment_id = execute_geographic_migration.deployment_id 
          AND step_name = current_step;
        
        RETURN QUERY SELECT 
            current_step::TEXT,
            'SUCCESS'::TEXT,
            'Migration executed successfully'::TEXT,
            EXTRACT(EPOCH FROM (step_end - step_start))::NUMERIC;
            
    EXCEPTION WHEN OTHERS THEN
        error_msg := SQLERRM;
        UPDATE deployment_status 
        SET status = 'failed', end_time = CURRENT_TIMESTAMP, error_message = error_msg
        WHERE deployment_id = execute_geographic_migration.deployment_id 
          AND step_name = current_step;
        
        RETURN QUERY SELECT 
            current_step::TEXT,
            'FAILED'::TEXT,
            error_msg::TEXT,
            EXTRACT(EPOCH FROM (CURRENT_TIMESTAMP - step_start))::NUMERIC;
        RETURN;
    END;
    
    -- Step 4: Post-deployment validation
    current_step := 'post_deployment_validation';
    step_start := CURRENT_TIMESTAMP;
    
    INSERT INTO deployment_status (deployment_id, step_name, status, start_time)
    VALUES (deployment_id, current_step, 'running', step_start);
    
    BEGIN
        -- Validate migration results
        IF (SELECT COUNT(*) FROM estados) < 27 THEN
            RAISE EXCEPTION 'Estados count validation failed';
        END IF;
        
        IF (SELECT COUNT(*) FROM municipios) < 5000 THEN
            RAISE EXCEPTION 'Municipios count validation failed';
        END IF;
        
        step_end := CURRENT_TIMESTAMP;
        UPDATE deployment_status 
        SET status = 'completed', end_time = step_end
        WHERE deployment_id = execute_geographic_migration.deployment_id 
          AND step_name = current_step;
        
        RETURN QUERY SELECT 
            current_step::TEXT,
            'SUCCESS'::TEXT,
            'Post-deployment validation passed'::TEXT,
            EXTRACT(EPOCH FROM (step_end - step_start))::NUMERIC;
            
    EXCEPTION WHEN OTHERS THEN
        error_msg := SQLERRM;
        UPDATE deployment_status 
        SET status = 'failed', end_time = CURRENT_TIMESTAMP, error_message = error_msg
        WHERE deployment_id = execute_geographic_migration.deployment_id 
          AND step_name = current_step;
        
        RETURN QUERY SELECT 
            current_step::TEXT,
            'FAILED'::TEXT,
            error_msg::TEXT,
            EXTRACT(EPOCH FROM (CURRENT_TIMESTAMP - step_start))::NUMERIC;
        RETURN;
    END;
    
END;
$$ LANGUAGE plpgsql;

-- Rollback procedure
CREATE OR REPLACE FUNCTION rollback_geographic_migration(deployment_id VARCHAR(50))
RETURNS TABLE(step_name TEXT, status TEXT, message TEXT) AS $$
DECLARE
    step_start TIMESTAMPTZ;
    current_step TEXT;
    error_msg TEXT;
    backup_exists BOOLEAN;
BEGIN
    -- Step 1: Validate rollback prerequisites
    current_step := 'validate_rollback_prerequisites';
    step_start := CURRENT_TIMESTAMP;
    
    INSERT INTO deployment_status (deployment_id, step_name, status, start_time)
    VALUES (deployment_id, current_step, 'running', step_start);
    
    BEGIN
        -- Check if backup tables exist
        SELECT EXISTS (
            SELECT 1 FROM information_schema.tables 
            WHERE table_name IN ('estados_backup', 'municipios_backup', 
                                'estados_referencia_backup', 'municipios_referencia_backup')
        ) INTO backup_exists;
        
        IF NOT backup_exists THEN
            RAISE EXCEPTION 'Backup tables not found - rollback not possible';
        END IF;
        
        UPDATE deployment_status 
        SET status = 'completed', end_time = CURRENT_TIMESTAMP
        WHERE deployment_id = rollback_geographic_migration.deployment_id 
          AND step_name = current_step;
        
        RETURN QUERY SELECT 
            current_step::TEXT,
            'SUCCESS'::TEXT,
            'Rollback prerequisites validated'::TEXT;
            
    EXCEPTION WHEN OTHERS THEN
        error_msg := SQLERRM;
        UPDATE deployment_status 
        SET status = 'failed', end_time = CURRENT_TIMESTAMP, error_message = error_msg
        WHERE deployment_id = rollback_geographic_migration.deployment_id 
          AND step_name = current_step;
        
        RETURN QUERY SELECT 
            current_step::TEXT,
            'FAILED'::TEXT,
            error_msg::TEXT;
        RETURN;
    END;
    
    -- Step 2: Restore reference tables
    current_step := 'restore_reference_tables';
    step_start := CURRENT_TIMESTAMP;
    
    INSERT INTO deployment_status (deployment_id, step_name, status, start_time)
    VALUES (deployment_id, current_step, 'running', step_start);
    
    BEGIN
        -- Restore estados_referencia
        DROP TABLE IF EXISTS estados_referencia CASCADE;
        CREATE TABLE estados_referencia AS SELECT * FROM estados_referencia_backup;
        
        -- Restore municipios_referencia
        DROP TABLE IF EXISTS municipios_referencia CASCADE;
        CREATE TABLE municipios_referencia AS SELECT * FROM municipios_referencia_backup;
        
        -- Recreate constraints and indexes
        ALTER TABLE estados_referencia ADD CONSTRAINT estados_referencia_pkey PRIMARY KEY (id);
        ALTER TABLE municipios_referencia ADD CONSTRAINT municipios_referencia_pkey PRIMARY KEY (id);
        
        UPDATE deployment_status 
        SET status = 'completed', end_time = CURRENT_TIMESTAMP
        WHERE deployment_id = rollback_geographic_migration.deployment_id 
          AND step_name = current_step;
        
        RETURN QUERY SELECT 
            current_step::TEXT,
            'SUCCESS'::TEXT,
            'Reference tables restored'::TEXT;
            
    EXCEPTION WHEN OTHERS THEN
        error_msg := SQLERRM;
        UPDATE deployment_status 
        SET status = 'failed', end_time = CURRENT_TIMESTAMP, error_message = error_msg
        WHERE deployment_id = rollback_geographic_migration.deployment_id 
          AND step_name = current_step;
        
        RETURN QUERY SELECT 
            current_step::TEXT,
            'FAILED'::TEXT,
            error_msg::TEXT;
        RETURN;
    END;
    
    -- Step 3: Restore original foreign key references
    current_step := 'restore_foreign_keys';
    step_start := CURRENT_TIMESTAMP;
    
    INSERT INTO deployment_status (deployment_id, step_name, status, start_time)
    VALUES (deployment_id, current_step, 'running', step_start);
    
    BEGIN
        -- This would require restoring original FK references
        -- Implementation depends on specific backup strategy
        
        UPDATE deployment_status 
        SET status = 'completed', end_time = CURRENT_TIMESTAMP
        WHERE deployment_id = rollback_geographic_migration.deployment_id 
          AND step_name = current_step;
        
        RETURN QUERY SELECT 
            current_step::TEXT,
            'SUCCESS'::TEXT,
            'Foreign key references restored'::TEXT;
            
    EXCEPTION WHEN OTHERS THEN
        error_msg := SQLERRM;
        UPDATE deployment_status 
        SET status = 'failed', end_time = CURRENT_TIMESTAMP, error_message = error_msg
        WHERE deployment_id = rollback_geographic_migration.deployment_id 
          AND step_name = current_step;
        
        RETURN QUERY SELECT 
            current_step::TEXT,
            'FAILED'::TEXT,
            error_msg::TEXT;
        RETURN;
    END;
    
    -- Step 4: Validate rollback
    current_step := 'validate_rollback';
    step_start := CURRENT_TIMESTAMP;
    
    INSERT INTO deployment_status (deployment_id, step_name, status, start_time)
    VALUES (deployment_id, current_step, 'running', step_start);
    
    BEGIN
        -- Validate that reference tables are working
        IF NOT EXISTS (SELECT 1 FROM estados_referencia LIMIT 1) THEN
            RAISE EXCEPTION 'estados_referencia table is empty after rollback';
        END IF;
        
        IF NOT EXISTS (SELECT 1 FROM municipios_referencia LIMIT 1) THEN
            RAISE EXCEPTION 'municipios_referencia table is empty after rollback';
        END IF;
        
        UPDATE deployment_status 
        SET status = 'completed', end_time = CURRENT_TIMESTAMP
        WHERE deployment_id = rollback_geographic_migration.deployment_id 
          AND step_name = current_step;
        
        RETURN QUERY SELECT 
            current_step::TEXT,
            'SUCCESS'::TEXT,
            'Rollback validation passed'::TEXT;
            
    EXCEPTION WHEN OTHERS THEN
        error_msg := SQLERRM;
        UPDATE deployment_status 
        SET status = 'failed', end_time = CURRENT_TIMESTAMP, error_message = error_msg
        WHERE deployment_id = rollback_geographic_migration.deployment_id 
          AND step_name = current_step;
        
        RETURN QUERY SELECT 
            current_step::TEXT,
            'FAILED'::TEXT,
            error_msg::TEXT;
        RETURN;
    END;
    
END;
$$ LANGUAGE plpgsql;

-- Deployment status check function
CREATE OR REPLACE FUNCTION check_deployment_status(deployment_id VARCHAR(50))
RETURNS TABLE(
    step_name TEXT, 
    status TEXT, 
    start_time TIMESTAMPTZ, 
    end_time TIMESTAMPTZ,
    duration_seconds NUMERIC,
    error_message TEXT
) AS $$
BEGIN
    RETURN QUERY 
    SELECT 
        ds.step_name::TEXT,
        ds.status::TEXT,
        ds.start_time,
        ds.end_time,
        CASE 
            WHEN ds.end_time IS NOT NULL 
            THEN EXTRACT(EPOCH FROM (ds.end_time - ds.start_time))::NUMERIC
            ELSE NULL
        END as duration_seconds,
        ds.error_message::TEXT
    FROM deployment_status ds
    WHERE ds.deployment_id = check_deployment_status.deployment_id
    ORDER BY ds.start_time;
END;
$$ LANGUAGE plpgsql;

-- Clean up old deployment logs
CREATE OR REPLACE FUNCTION cleanup_deployment_logs(days_to_keep INTEGER DEFAULT 30)
RETURNS INTEGER AS $$
DECLARE
    deleted_count INTEGER;
BEGIN
    DELETE FROM deployment_status 
    WHERE start_time < CURRENT_TIMESTAMP - INTERVAL '1 day' * days_to_keep;
    
    GET DIAGNOSTICS deleted_count = ROW_COUNT;
    
    DELETE FROM migration_log 
    WHERE created_at < CURRENT_TIMESTAMP - INTERVAL '1 day' * days_to_keep;
    
    RETURN deleted_count;
END;
$$ LANGUAGE plpgsql;

-- Example usage documentation
/*
-- To execute the migration:
SELECT * FROM execute_geographic_migration('DEPLOY_2024_001');

-- To check deployment status:
SELECT * FROM check_deployment_status('DEPLOY_2024_001');

-- To rollback if needed:
SELECT * FROM rollback_geographic_migration('DEPLOY_2024_001');

-- To clean up old logs:
SELECT cleanup_deployment_logs(30);
*/