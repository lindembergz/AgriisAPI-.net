using Microsoft.OpenApi.Models;
using System.Reflection;

namespace Agriis.Api.Configuration;

/// <summary>
/// Configuração do Swagger/OpenAPI
/// </summary>
public static class SwaggerConfiguration
{
    public static IServiceCollection AddSwaggerConfiguration(this IServiceCollection services)
    {
        services.AddSwaggerGen(options =>
        {
            // Informações básicas da API
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "Agriis API",
                Version = "v1",
                Description = "API do sistema Agriis - Plataforma de Agronegócio",
                Contact = new OpenApiContact
                {
                    Name = "Equipe Agriis",
                    Email = "dev@agriis.com"
                },
                License = new OpenApiLicense
                {
                    Name = "Proprietary License"
                }
            });

            // Configuração de autenticação JWT
            options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Name = "Authorization",
                Type = SecuritySchemeType.Http,
                Scheme = "Bearer",
                BearerFormat = "JWT",
                In = ParameterLocation.Header,
                Description = "Insira o token JWT no formato: Bearer {seu_token}"
            });

            options.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        }
                    },
                    Array.Empty<string>()
                }
            });

            // Incluir comentários XML
            var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
            var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
            if (File.Exists(xmlPath))
            {
                options.IncludeXmlComments(xmlPath);
            }

            // Configurações adicionais
            options.UseInlineDefinitionsForEnums();
            
            // Configurar serialização de enums como strings
            options.SchemaFilter<EnumSchemaFilter>();
            
            // Configurar exemplos de resposta
            options.OperationFilter<ResponseExamplesOperationFilter>();
            
            // Configurar tags por controller
            options.TagActionsBy(api => new[] { api.GroupName ?? api.ActionDescriptor.RouteValues["controller"] });
            options.DocInclusionPredicate((name, api) => true);

            // Configurar ordenação
            options.OrderActionsBy(apiDesc => $"{apiDesc.ActionDescriptor.RouteValues["controller"]}_{apiDesc.HttpMethod}");
        });

        return services;
    }

    public static WebApplication UseSwaggerConfiguration(this WebApplication app)
    {
        if (app.Environment.IsDevelopment() || app.Environment.IsStaging())
        {
            app.UseSwagger(options =>
            {
                options.RouteTemplate = "api-docs/{documentName}/swagger.json";
            });

            app.UseSwaggerUI(options =>
            {
                options.SwaggerEndpoint("/api-docs/v1/swagger.json", "Agriis API v1");
                options.RoutePrefix = "api-docs";
                options.DocumentTitle = "Agriis API Documentation";
                
                // Configurações de UI
                options.DefaultModelsExpandDepth(-1); // Ocultar modelos por padrão
                options.DefaultModelRendering(Swashbuckle.AspNetCore.SwaggerUI.ModelRendering.Model);
                options.DisplayRequestDuration();
                options.EnableDeepLinking();
                options.EnableFilter();
                options.ShowExtensions();
                options.EnableValidator();
                
                // Configurar autenticação persistente
                options.ConfigObject.AdditionalItems.Add("persistAuthorization", "true");
                
                // CSS customizado
                options.InjectStylesheet("/swagger-ui/custom.css");
            });

            // Servir arquivos estáticos para customização do Swagger
            app.UseStaticFiles();
        }

        return app;
    }
}

/// <summary>
/// Filtro para configurar enums como strings no schema
/// </summary>
public class EnumSchemaFilter : Swashbuckle.AspNetCore.SwaggerGen.ISchemaFilter
{
    public void Apply(OpenApiSchema schema, Swashbuckle.AspNetCore.SwaggerGen.SchemaFilterContext context)
    {
        if (context.Type.IsEnum)
        {
            schema.Enum.Clear();
            foreach (var enumName in Enum.GetNames(context.Type))
            {
                schema.Enum.Add(new Microsoft.OpenApi.Any.OpenApiString(enumName));
            }
            schema.Type = "string";
            schema.Format = null;
        }
    }
}

/// <summary>
/// Filtro para adicionar exemplos de resposta
/// </summary>
public class ResponseExamplesOperationFilter : Swashbuckle.AspNetCore.SwaggerGen.IOperationFilter
{
    public void Apply(OpenApiOperation operation, Swashbuckle.AspNetCore.SwaggerGen.OperationFilterContext context)
    {
        // Adicionar exemplos de resposta de erro padrão
        if (!operation.Responses.ContainsKey("400"))
        {
            operation.Responses.Add("400", new OpenApiResponse
            {
                Description = "Requisição inválida",
                Content = new Dictionary<string, OpenApiMediaType>
                {
                    ["application/json"] = new OpenApiMediaType
                    {
                        Example = new Microsoft.OpenApi.Any.OpenApiObject
                        {
                            ["error_code"] = new Microsoft.OpenApi.Any.OpenApiString("VALIDATION_ERROR"),
                            ["error_description"] = new Microsoft.OpenApi.Any.OpenApiString("Dados de entrada inválidos"),
                            ["timestamp"] = new Microsoft.OpenApi.Any.OpenApiString(DateTime.UtcNow.ToString("O"))
                        }
                    }
                }
            });
        }

        if (!operation.Responses.ContainsKey("401"))
        {
            operation.Responses.Add("401", new OpenApiResponse
            {
                Description = "Não autorizado",
                Content = new Dictionary<string, OpenApiMediaType>
                {
                    ["application/json"] = new OpenApiMediaType
                    {
                        Example = new Microsoft.OpenApi.Any.OpenApiObject
                        {
                            ["error_code"] = new Microsoft.OpenApi.Any.OpenApiString("UNAUTHORIZED"),
                            ["error_description"] = new Microsoft.OpenApi.Any.OpenApiString("Acesso não autorizado"),
                            ["timestamp"] = new Microsoft.OpenApi.Any.OpenApiString(DateTime.UtcNow.ToString("O"))
                        }
                    }
                }
            });
        }

        if (!operation.Responses.ContainsKey("500"))
        {
            operation.Responses.Add("500", new OpenApiResponse
            {
                Description = "Erro interno do servidor",
                Content = new Dictionary<string, OpenApiMediaType>
                {
                    ["application/json"] = new OpenApiMediaType
                    {
                        Example = new Microsoft.OpenApi.Any.OpenApiObject
                        {
                            ["error_code"] = new Microsoft.OpenApi.Any.OpenApiString("INTERNAL_ERROR"),
                            ["error_description"] = new Microsoft.OpenApi.Any.OpenApiString("Erro interno do servidor"),
                            ["timestamp"] = new Microsoft.OpenApi.Any.OpenApiString(DateTime.UtcNow.ToString("O"))
                        }
                    }
                }
            });
        }
    }
}