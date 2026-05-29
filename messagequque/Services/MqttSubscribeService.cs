using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text;  
using Microsoft.EntityFrameworkCore;  
using MQTTnet;  
using MQTTnet.Client;  
using MQTTnet.Protocol;  
using MqttWorker.Data;  

namespace messagequque.Services
{
    public  class MqttSubscribeService: BackgroundService 
{  
    private readonly IServiceScopeFactory _scopeFactory;  
    private readonly ILogger<MqttBackgroundService> _logger;  
    private IMqttClient? _client;  
  
    public MqttBackgroundService(  
        IServiceScopeFactory scopeFactory,  
        ILogger<MqttBackgroundService> logger)  
    {  
        _scopeFactory = scopeFactory;  
        _logger = logger;  
    }  
  
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)  
    {  
        await ConnectAndSubscribe(stoppingToken);  
  
        while (!stoppingToken.IsCancellationRequested)  
        {  
            await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);  
  
            if (_client is not null && !_client.IsConnected)  
            {  
                _logger.LogWarning("MQTT disconnected. Reconnecting...");  
                await ConnectAndSubscribe(stoppingToken);  
            }  
        }  
    }  
  
    private async Task ConnectAndSubscribe(CancellationToken cancellationToken)  
    {  
        try  
        {  
            var factory = new MqttFactory();  
            _client ??= factory.CreateMqttClient();  
  
            _client.ApplicationMessageReceivedAsync -= OnMessageReceivedAsync;  
            _client.ApplicationMessageReceivedAsync += OnMessageReceivedAsync;  
  
            _client.DisconnectedAsync -= OnDisconnectedAsync;  
            _client.DisconnectedAsync += OnDisconnectedAsync;  
  
            var options = new MqttClientOptionsBuilder()  
                .WithClientId("dotnet8-worker-001")  
                .WithTcpServer("127.0.0.1", 1883)  
                .WithCleanSession(false)  
                .Build();  
  
            if (_client.IsConnected)  
                return;  
  
            await _client.ConnectAsync(options, cancellationToken);  
            await _client.SubscribeAsync("your/topic/#", MqttQualityOfServiceLevel.AtLeastOnce, cancellationToken);  
  
            _logger.LogInformation("MQTT connected and subscribed.");  
        }  
        catch (Exception ex)  
        {  
            _logger.LogError(ex, "Failed to connect MQTT");  
        }  
    }  
  
    private async Task OnMessageReceivedAsync(MqttApplicationMessageReceivedEventArgs e)  
    {  
        try  
        {  
            var payload = Encoding.UTF8.GetString(e.ApplicationMessage.PayloadSegment);  
            var topic = e.ApplicationMessage.Topic;  
  
            using var scope = _scopeFactory.CreateScope();  
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();  
  
            var entity = new MessageEntity  
            {  
                MessageId = Guid.NewGuid().ToString(),  
                Topic = topic,  
                Payload = payload,  
                CreatedAt = DateTime.UtcNow,  
                Status = "pending",  
                RetryCount = 0  
            };  
  
            db.Messages.Add(entity);  
            await db.SaveChangesAsync();  
  
            _logger.LogInformation("Saved MQTT message: {Topic}", topic);  
        }  
        catch (Exception ex)  
        {  
            _logger.LogError(ex, "Error saving MQTT message");  
        }  
    }  
  
    private async Task OnDisconnectedAsync(MqttClientDisconnectedEventArgs e)  
    {  
        _logger.LogWarning("MQTT disconnected.");  
        await Task.Delay(3000);  
        await ConnectAndSubscribe(CancellationToken.None);  
    }  
} 
