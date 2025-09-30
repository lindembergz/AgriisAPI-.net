using System;
using System.IO;
using System.Threading.Tasks;
using Npgsql;

class Program
{
        static async Task Main(string[] args)
        {
            var connectionString = args.Length > 0 
                ? args[0] 
                : "Host=localhost;Database=DBAgriis;Username=postgres;Password=RootPassword123;Port=5432";

            Console.WriteLine("Executando script de correção das referências do Fornecedor...");

            try
            {
                // Ler o script SQL
                var scriptPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "fix_fornecedor_references.sql");
                if (!File.Exists(scriptPath))
                {
                    scriptPath = Path.Combine(Directory.GetCurrentDirectory(), "scripts", "fix_fornecedor_references.sql");
                }

                if (!File.Exists(scriptPath))
                {
                    Console.WriteLine($"ERRO: Script SQL não encontrado em {scriptPath}");
                    Environment.Exit(1);
                }

                var sqlScript = await File.ReadAllTextAsync(scriptPath);
                Console.WriteLine($"Script SQL carregado: {scriptPath}");

                // Executar o script
                using var connection = new NpgsqlConnection(connectionString);
                await connection.OpenAsync();
                Console.WriteLine("Conectado ao banco de dados.");

                using var command = new NpgsqlCommand(sqlScript, connection);
                command.CommandTimeout = 300; // 5 minutos

                var result = await command.ExecuteNonQueryAsync();
                Console.WriteLine($"Script executado com sucesso! Linhas afetadas: {result}");

                // Verificar os resultados
                using var verifyCommand = new NpgsqlCommand(@"
                    SELECT 'estados_referencia' as tabela, COUNT(*) as registros FROM public.estados_referencia
                    UNION ALL
                    SELECT 'municipios_referencia' as tabela, COUNT(*) as registros FROM public.municipios_referencia;
                ", connection);

                using var reader = await verifyCommand.ExecuteReaderAsync();
                Console.WriteLine("\nResultados:");
                while (await reader.ReadAsync())
                {
                    Console.WriteLine($"  {reader["tabela"]}: {reader["registros"]} registros");
                }

                Console.WriteLine("\nScript executado com sucesso!");
                Console.WriteLine("Agora você pode testar a API novamente.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERRO: {ex.Message}");
                Environment.Exit(1);
            }
        }
    }