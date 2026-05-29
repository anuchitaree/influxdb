using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace messagequque.Services
{
    public interface IMqttService
    {
        Task StartAsync(
        Func<string, string, Task> onMessage,
        CancellationToken cancellationToken);
    }
}
