using messagequque.Services;

namespace messagequque.Worker
{
    public class HttpSenderWorker : BackgroundService
    {
        private readonly SqliteService _sqlite;
        private readonly IApiService _apiService;
        private readonly IConfiguration _config;

        public HttpSenderWorker(
            SqliteService sqlite,
            IApiService apiService,
            IConfiguration config)
        {
            _sqlite = sqlite;
            _apiService = apiService;
            _config = config;
        }
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var interval =
           int.Parse(_config["Batch:IntervalSeconds"]!);

            var batchSize =
                int.Parse(_config["Batch:BatchSize"]!);

            while (!stoppingToken.IsCancellationRequested)
            {
                var rows =
                    await _sqlite.GetPendingAsync(batchSize);

                if (rows.Count > 0)
                {
                    var success =
                        await _apiService.SendAsync(
                            rows,
                            stoppingToken);

                    if (success)
                    {
                        await _sqlite.MarkAsSentAsync(
                            rows.Select(x => x.Id));
                    }
                }

                await Task.Delay(
                    TimeSpan.FromSeconds(interval),
                    stoppingToken);


            }

        }
    }



