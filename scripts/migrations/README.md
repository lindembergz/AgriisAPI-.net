# Data Migration Scripts

This directory contains comprehensive data migration scripts for the Reference Entities CRUD implementation.

## Migration Structure

### 1. Reference Data Population (`01_populate_reference_tables.sql`)
- Populates reference tables with existing data from Enderecos module
- Maps existing geographic data (Estados -> UFs, Municipios)
- Creates initial reference data for Moedas, UnidadesMedida, AtividadesAgropecuarias, Embalagens

### 2. Produto Migration (`02_migrate_produto_references.sql`)
- Maps existing Produto string fields to reference IDs
- Handles conflict resolution for duplicate/invalid data
- Preserves existing business logic and data integrity

### 3. Fornecedor Migration (`03_migrate_fornecedor_references.sql`)
- Maps existing Fornecedor geographic strings to reference IDs
- Handles UF and Municipio mapping with validation
- Preserves existing address data

### 4. Rollback Scripts (`rollback/`)
- Individual rollback scripts for each migration step
- Complete rollback script for full migration reversal
- Data backup and restore procedures

### 5. Validation Scripts (`validation/`)
- Foreign key relationship validation
- Data integrity checking
- Orphaned records detection
- Migration report generation

## Execution Order

1. **Backup**: Always create backup before migration
2. **Reference Population**: Execute `01_populate_reference_tables.sql`
3. **Produto Migration**: Execute `02_migrate_produto_references.sql`
4. **Fornecedor Migration**: Execute `03_migrate_fornecedor_references.sql`
5. **Validation**: Execute validation scripts to verify integrity
6. **Cleanup**: Remove temporary columns and data after validation

## Usage

### PowerShell Execution
```powershell
# Execute complete migration
.\execute_complete_migration.ps1

# Execute individual migration
.\execute_migration.ps1 -MigrationFile "01_populate_reference_tables.sql"

# Validate migration
.\validate_migration.ps1

# Rollback migration
.\rollback_migration.ps1 -Step "produto" # or "all"
```

### Manual Execution
```sql
-- Execute in PostgreSQL in order:
\i 01_populate_reference_tables.sql
\i 02_migrate_produto_references.sql
\i 03_migrate_fornecedor_references.sql
\i validation/validate_all_references.sql
```

## Important Notes

- **Always backup before migration**
- **Test in development environment first**
- **Validate data integrity after each step**
- **Monitor migration progress and logs**
- **Have rollback plan ready**

## Migration Reports

Each migration generates detailed reports including:
- Records processed
- Conflicts found and resolved
- Data quality issues
- Performance metrics
- Validation results