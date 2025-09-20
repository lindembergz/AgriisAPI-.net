using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace Agriis.Compartilhado.Infraestrutura.Integracoes;

public interface INotificationService
{
    Task SendNotificationToUserAsync(string userId, string message, object? data = null);
    Task SendNotificationToGroupAsync(string groupName, string message, object? data = null);
    Task SendNotificationToAllAsync(string message, object? data = null);
    Task AddUserToGroupAsync(string userId, string groupName);
    Task RemoveUserFromGroupAsync(string userId, string groupName);
    Task SendPedidoNotificationAsync(int pedidoId, string userId, string message, object? data = null);
    Task SendPropostaNotificationAsync(int propostaId, string userId, string message, object? data = null);
    Task SendSystemNotificationAsync(string message, object? data = null);
}

public class NotificationService : INotificationService
{
    private readonly IHubContext<NotificationHub> _hubContext;
    private readonly ILogger<NotificationService> _logger;

    public NotificationService(IHubContext<NotificationHub> hubContext, ILogger<NotificationService> logger)
    {
        _hubContext = hubContext ?? throw new ArgumentNullException(nameof(hubContext));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task SendNotificationToUserAsync(string userId, string message, object? data = null)
    {
        try
        {
            var notification = new
            {
                Message = message,
                Data = data,
                Timestamp = DateTime.UtcNow,
                Type = "user"
            };

            await _hubContext.Clients.User(userId).SendAsync("ReceiveNotification", notification);
            
            _logger.LogInformation("Notificação enviada para usuário {UserId}: {Message}", userId, message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao enviar notificação para usuário {UserId}: {Message}", userId, message);
            throw;
        }
    }

    public async Task SendNotificationToGroupAsync(string groupName, string message, object? data = null)
    {
        try
        {
            var notification = new
            {
                Message = message,
                Data = data,
                Timestamp = DateTime.UtcNow,
                Type = "group"
            };

            await _hubContext.Clients.Group(groupName).SendAsync("ReceiveNotification", notification);
            
            _logger.LogInformation("Notificação enviada para grupo {GroupName}: {Message}", groupName, message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao enviar notificação para grupo {GroupName}: {Message}", groupName, message);
            throw;
        }
    }

    public async Task SendNotificationToAllAsync(string message, object? data = null)
    {
        try
        {
            var notification = new
            {
                Message = message,
                Data = data,
                Timestamp = DateTime.UtcNow,
                Type = "broadcast"
            };

            await _hubContext.Clients.All.SendAsync("ReceiveNotification", notification);
            
            _logger.LogInformation("Notificação broadcast enviada: {Message}", message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao enviar notificação broadcast: {Message}", message);
            throw;
        }
    }

    public async Task AddUserToGroupAsync(string userId, string groupName)
    {
        try
        {
            await _hubContext.Groups.AddToGroupAsync(userId, groupName);
            
            _logger.LogInformation("Usuário {UserId} adicionado ao grupo {GroupName}", userId, groupName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao adicionar usuário {UserId} ao grupo {GroupName}", userId, groupName);
            throw;
        }
    }

    public async Task RemoveUserFromGroupAsync(string userId, string groupName)
    {
        try
        {
            await _hubContext.Groups.RemoveFromGroupAsync(userId, groupName);
            
            _logger.LogInformation("Usuário {UserId} removido do grupo {GroupName}", userId, groupName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao remover usuário {UserId} do grupo {GroupName}", userId, groupName);
            throw;
        }
    }

    public async Task SendPedidoNotificationAsync(int pedidoId, string userId, string message, object? data = null)
    {
        try
        {
            var notification = new
            {
                Message = message,
                Data = data,
                Timestamp = DateTime.UtcNow,
                Type = "pedido",
                PedidoId = pedidoId
            };

            await _hubContext.Clients.User(userId).SendAsync("ReceivePedidoNotification", notification);
            
            _logger.LogInformation("Notificação de pedido {PedidoId} enviada para usuário {UserId}: {Message}", 
                pedidoId, userId, message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao enviar notificação de pedido {PedidoId} para usuário {UserId}: {Message}", 
                pedidoId, userId, message);
            throw;
        }
    }

    public async Task SendPropostaNotificationAsync(int propostaId, string userId, string message, object? data = null)
    {
        try
        {
            var notification = new
            {
                Message = message,
                Data = data,
                Timestamp = DateTime.UtcNow,
                Type = "proposta",
                PropostaId = propostaId
            };

            await _hubContext.Clients.User(userId).SendAsync("ReceivePropostaNotification", notification);
            
            _logger.LogInformation("Notificação de proposta {PropostaId} enviada para usuário {UserId}: {Message}", 
                propostaId, userId, message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao enviar notificação de proposta {PropostaId} para usuário {UserId}: {Message}", 
                propostaId, userId, message);
            throw;
        }
    }

    public async Task SendSystemNotificationAsync(string message, object? data = null)
    {
        try
        {
            var notification = new
            {
                Message = message,
                Data = data,
                Timestamp = DateTime.UtcNow,
                Type = "system"
            };

            await _hubContext.Clients.All.SendAsync("ReceiveSystemNotification", notification);
            
            _logger.LogInformation("Notificação do sistema enviada: {Message}", message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao enviar notificação do sistema: {Message}", message);
            throw;
        }
    }
}

public class NotificationHub : Hub
{
    private readonly ILogger<NotificationHub> _logger;

    public NotificationHub(ILogger<NotificationHub> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public override async Task OnConnectedAsync()
    {
        var userId = Context.UserIdentifier;
        _logger.LogInformation("Cliente conectado ao hub de notificações. ConnectionId: {ConnectionId}, UserId: {UserId}", 
            Context.ConnectionId, userId);

        // Adicionar usuário a grupos baseados em suas roles ou características
        if (!string.IsNullOrEmpty(userId))
        {
            // Adicionar a grupo geral de usuários autenticados
            await Groups.AddToGroupAsync(Context.ConnectionId, "authenticated-users");
            
            // Adicionar a grupos específicos baseados em claims do usuário
            var userType = Context.User?.FindFirst("user_type")?.Value;
            if (!string.IsNullOrEmpty(userType))
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, $"{userType}-users");
            }
        }

        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var userId = Context.UserIdentifier;
        _logger.LogInformation("Cliente desconectado do hub de notificações. ConnectionId: {ConnectionId}, UserId: {UserId}", 
            Context.ConnectionId, userId);

        if (exception != null)
        {
            _logger.LogError(exception, "Cliente desconectado com erro. ConnectionId: {ConnectionId}", Context.ConnectionId);
        }

        await base.OnDisconnectedAsync(exception);
    }

    public async Task JoinGroup(string groupName)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
        _logger.LogInformation("Cliente {ConnectionId} adicionado ao grupo {GroupName}", Context.ConnectionId, groupName);
    }

    public async Task LeaveGroup(string groupName)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
        _logger.LogInformation("Cliente {ConnectionId} removido do grupo {GroupName}", Context.ConnectionId, groupName);
    }

    public async Task SendMessageToGroup(string groupName, string message)
    {
        await Clients.Group(groupName).SendAsync("ReceiveMessage", Context.User?.Identity?.Name ?? "Anônimo", message);
        _logger.LogInformation("Mensagem enviada para grupo {GroupName} por {User}: {Message}", 
            groupName, Context.User?.Identity?.Name ?? "Anônimo", message);
    }
}