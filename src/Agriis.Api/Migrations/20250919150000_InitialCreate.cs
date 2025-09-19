using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Agriis.Api.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Migração inicial vazia - apenas para criar a estrutura básica
            // As tabelas serão criadas quando as entidades forem adicionadas
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Nada para reverter na migração inicial vazia
        }
    }
}