using messagequque.Data;
using messagequque.Services;
using messagequque.Worker;
using Microsoft.EntityFrameworkCore;

namespace messagequque
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = Host.CreateApplicationBuilder(args);

            builder.Services.AddSingleton<SqliteService>();

            builder.Services.AddSingleton<IMqttService, MqttService>();

            builder.Services.AddSingleton<IApiService, ApiService>();


            builder.Services.AddHostedService<MqttWorker>();

            builder.Services.AddHostedService<HttpSenderWorker>();

            builder.Services.AddHttpClient();

            var app = builder.Build();

            using (var scope = app.Services.CreateScope())
            {
                var sqlite =
                    scope.ServiceProvider.GetRequiredService<SqliteService>();

                await sqlite.InitAsync();
            }

            await app.RunAsync();



            //var builder = Host.CreateApplicationBuilder(args);

            //var mqttHost = builder.Configuration["Mqtt:Host"] ?? "127.0.0.1";
            //var mqttPort = int.TryParse(builder.Configuration["Mqtt:Port"], out var port) ? port : 1883;
            //var apiBaseUrl = builder.Configuration["Api:BaseUrl"] ?? "https://your-api-host.com";

            ////builder.Services.AddHostedService<Worker>();

            ////var host = builder.Build();
            ////host.Run();

            //builder.Services.AddDbContext<AppDbContext>(options =>
            //{
            //    options.UseSqlite("Data Source=/data/app.db");
            //});


            //builder.Services.AddHttpClient("ApiClient", client =>
            //{
            //    client.BaseAddress = new Uri(apiBaseUrl);
            //    client.Timeout = TimeSpan.FromSeconds(30);
            //})
            //.AddStandardResilienceHandler();

            //builder.Services.Configure<MqttConfig>(config =>
            //{
            //    config.Host = mqttHost;
            //    config.Port = mqttPort;
            //});

            //builder.Services.AddHostedService<MqttSubscribeService>();
            //builder.Services.AddHostedService<ApiSenderService>();

            //var app = builder.Build();

            //// create db if not exists  
            //using (var scope = app.Services.CreateScope())
            //{
            //    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            //    db.Database.EnsureCreated();
            //    db.Database.ExecuteSqlRaw("PRAGMA journal_mode=WAL;");
            //}

            //app.Run();
        }
    }
}
