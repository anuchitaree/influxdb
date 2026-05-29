using Microsoft.EntityFrameworkCore;  
using MqttWorker.Data;  
using MqttWorker.Services; 
namespace messagequque
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = Host.CreateApplicationBuilder(args);
            //builder.Services.AddHostedService<Worker>();

            //var host = builder.Build();
            //host.Run();
              
builder.Services.AddDbContext<AppDbContext>(options =>  
{  
    options.UseSqlite("Data Source=/var/lib/mqttworker/app.db");  
});  
  
builder.Services.AddHttpClient("ApiClient", client =>  
{  
    client.BaseAddress = new Uri("https://your-api-host.com");  
    client.Timeout = TimeSpan.FromSeconds(30);  
})  
.AddStandardResilienceHandler();  
  
builder.Services.AddHostedService<MqttBackgroundService>();  
builder.Services.AddHostedService<ApiSenderService>();  
  
var app = builder.Build();  
  
// create db if not exists  
using (var scope = app.Services.CreateScope())  
{  
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();  
    db.Database.EnsureCreated();  
}  
  
await app.RunAsync();  
        }
    }
}
