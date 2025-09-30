using AutoMapper;

namespace Agriis.Compartilhado.Aplicacao.Mapeamentos;

/// <summary>
/// Profile global do AutoMapper para conversões comuns
/// </summary>
public class GlobalMappingProfile : Profile
{
    public GlobalMappingProfile()
    {
        // Conversão global de DateTimeOffset para DateTime
        CreateMap<DateTimeOffset, DateTime>()
            .ConvertUsing(src => src.DateTime);
            
        // Conversão global de DateTimeOffset? para DateTime?
        CreateMap<DateTimeOffset?, DateTime?>()
            .ConvertUsing(src => src.HasValue ? src.Value.DateTime : (DateTime?)null);
            
        // Conversão global de DateTime para DateTimeOffset
        CreateMap<DateTime, DateTimeOffset>()
            .ConvertUsing(src => new DateTimeOffset(src, TimeSpan.Zero));
            
        // Conversão global de DateTime? para DateTimeOffset?
        CreateMap<DateTime?, DateTimeOffset?>()
            .ConvertUsing(src => src.HasValue ? new DateTimeOffset(src.Value, TimeSpan.Zero) : (DateTimeOffset?)null);
    }
}