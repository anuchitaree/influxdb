using MQTTnet;
using MQTTnet.Protocol;
using System.Buffers;
using System.Text;

namespace messagequque.Services
{
    public class MqttService : IMqttService
    {

        private readonly IConfiguration _config;
        private readonly ILogger<MqttService> _logger;

        public MqttService(
            IConfiguration config,
            ILogger<MqttService> logger)
        {
            _config = config;
            _logger = logger;
        }


        public async Task StartAsync(Func<string, string, Task> onMessage, CancellationToken cancellationToken)
        {
            var factory = new MqttClientFactory();

            var client = factory.CreateMqttClient();

            client.ApplicationMessageReceivedAsync += async e =>
            {
                try
                {
                    var topic = e.ApplicationMessage.Topic;

                    var payload = Encoding.UTF8.GetString(
                        e.ApplicationMessage.Payload.ToArray());

                    await onMessage(topic, payload);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "MQTT Message Error");
                }
            };

            var options = new MqttClientOptionsBuilder()
                .WithTcpServer(
                    _config["Mqtt:Host"],
                    int.Parse(_config["Mqtt:Port"]!))
                .WithClientId(_config["Mqtt:ClientId"])
                .WithProtocolVersion(
                    MQTTnet.Formatter.MqttProtocolVersion.V500)
                .WithCleanSession(false)
                .Build();


            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    if (!client.IsConnected)
                    {
                        await client.ConnectAsync(
                            options,
                            cancellationToken);

                        _logger.LogInformation(
                            "MQTT Connected");

                        await client.SubscribeAsync(
                            new MqttTopicFilterBuilder()
                            .WithTopic(_config["Mqtt:Topic"])
                            .WithQualityOfServiceLevel(
                                MqttQualityOfServiceLevel.AtLeastOnce)
                            .Build());

                        _logger.LogInformation(
                            "MQTT Subscribed");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "MQTT Connect Error");
                }

                await Task.Delay(5000, cancellationToken);
            }



        }
    }
}
