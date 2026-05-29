using messagequque.Services;
using MQTTnet;
using MQTTnet.Protocol;
using System.Buffers;
using System.Text;

namespace messagequque.Worker
{
    public class MqttWorker : BackgroundService
    {
        private readonly IMqttService _mqttService;
        private readonly SqliteService _sqlite;

        public MqttWorker(
            IMqttService mqttService,
            SqliteService sqlite)
        {
            _mqttService = mqttService;
            _sqlite = sqlite;
        }


        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await _mqttService.StartAsync(
            async (topic, payload) =>
            {
                await _sqlite.InsertAsync(
                    topic,
                    payload);
            },
            stoppingToken);
        }

    }
}

