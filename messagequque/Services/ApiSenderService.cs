using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Net.Http.Json;  
using Microsoft.EntityFrameworkCore;  
using MqttWorker.Data; 


namespace messagequque.Services
{
    public class ApiSenderService: BackgroundService 
    
  {  
    private readonly IServiceScopeFactory _scopeFactory;  
    private readonly IHttpClientFactory _httpClientFactory;  
    private readonly ILogger<ApiSenderService> _logger;  
  
    public ApiSenderService(  
        IServiceScopeFactory scopeFactory,  
        IHttpClientFactory httpClientFactory,  
        ILogger<ApiSenderService> logger)  
    {  
        _scopeFactory = scopeFactory;  
        _httpClientFactory = httpClientFactory;  
        _logger = logger;  
    }  
  
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)  
    {  
        while (!stoppingToken.IsCancellationRequested)  
        {  
            await SendPendingAsync(stoppingToken);  
            await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);  
        }  
    }  
  
    private async Task SendPendingAsync(CancellationToken stoppingToken)  
    {  
        using var scope = _scopeFactory.CreateScope();  
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();  
        var client = _httpClientFactory.CreateClient("ApiClient");  
  
        var pendingMessages = await db.Messages  
            .Where(x => x.Status == "pending" || (x.Status == "failed" && x.RetryCount < 5))  
            .OrderBy(x => x.Id)  
            .Take(20)  
            .ToListAsync(stoppingToken);  
  
        foreach (var msg in pendingMessages)  
        {  
            try  
            {  
                msg.Status = "sending";  
                await db.SaveChangesAsync(stoppingToken);  
  
                var request = new  
                {  
                    messageId = msg.MessageId,  
                    topic = msg.Topic,  
                    payload = msg.Payload,  
                    createdAt = msg.CreatedAt  
                };  
  
                var response = await client.PostAsJsonAsync("/api/data", request, stoppingToken);  
  
                if (response.IsSuccessStatusCode)  
                {  
                    msg.Status = "sent";  
                    _logger.LogInformation("Sent message {MessageId}", msg.MessageId);  
                }  
                else  
                {  
                    msg.Status = "failed";  
                    msg.RetryCount++;  
                    _logger.LogWarning("Failed to send message {MessageId}, status {StatusCode}",  
                        msg.MessageId, response.StatusCode);  
                }  
  
                await db.SaveChangesAsync(stoppingToken);  
            }  
            catch (Exception ex)  
            {  
                msg.Status = "failed";  
                msg.RetryCount++;  
                await db.SaveChangesAsync(stoppingToken);  
  
                _logger.LogError(ex, "Error sending message {MessageId}", msg.MessageId);  
            }  
        }  
    }  
}  
