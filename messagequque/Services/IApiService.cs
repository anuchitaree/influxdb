using messagequque.Models;

namespace messagequque.Services
{
    public interface IApiService
    {
        Task<bool> SendAsync(
      IEnumerable<TelemetryData> rows,
      CancellationToken cancellationToken);
    }
}
