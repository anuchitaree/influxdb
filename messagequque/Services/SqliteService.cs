using messagequque.Models;
using Microsoft.Data.Sqlite;
using System.Data;

namespace messagequque.Services
{
    public class SqliteService
    {
        private readonly string _connectionString;

        public SqliteService(IConfiguration config)
        {
            var dbPath = config["Database:Path"];
            _connectionString = $"Data Source={dbPath}";
        }

        public async Task InitAsync()
        {
            using var conn = new SqliteConnection(_connectionString);
            await conn.OpenAsync();

            using var cmd = conn.CreateCommand();
            cmd.CommandText = """
                CREATE TABLE IF NOT EXISTS mqtt_messages
                    (
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        Topic TEXT NOT NULL,
                        Payload TEXT NOT NULL,
                        CreatedAt TEXT NOT NULL,
                        Sent INTEGER NOT NULL DEFAULT 0
                    );
                """;

            await cmd.ExecuteNonQueryAsync();
        }

        public async Task InsertAsync(string topic, string payload)
        {
            using var conn = new SqliteConnection(_connectionString);
            await conn.OpenAsync();

            using var cmd = conn.CreateCommand();
            cmd.CommandText = """
                INSERT INTO mqtt_messages
                (Topic, Payload, CreatedAt, Sent)
                VALUES
                (@Topic, @Payload, @CreatedAt, 0)
                """;

            cmd.Parameters.AddWithValue("@Topic", topic);
            cmd.Parameters.AddWithValue("@Payload", payload);
            cmd.Parameters.AddWithValue("@CreatedAt", DateTime.UtcNow.ToString("o"));

            await cmd.ExecuteNonQueryAsync();
        }

        public async Task<List<TelemetryData>> GetPendingAsync(int limit)
        {
            using var conn = new SqliteConnection(_connectionString);
            await conn.OpenAsync();

            using var cmd = conn.CreateCommand();
            cmd.CommandText = """
                SELECT *
                FROM mqtt_messages
                WHERE Sent = 0
                ORDER BY Id
                LIMIT @Limit
                """;

            cmd.Parameters.AddWithValue("@Limit", limit);

            var result = new List<TelemetryData>();
            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                result.Add(new TelemetryData
                {
                    Id = reader.GetInt64(reader.GetOrdinal("Id")),
                    Topic = reader.GetString(reader.GetOrdinal("Topic")),
                    Payload = reader.GetString(reader.GetOrdinal("Payload")),
                    CreatedAt = DateTime.Parse(reader.GetString(reader.GetOrdinal("CreatedAt"))),
                    Sent = reader.GetInt32(reader.GetOrdinal("Sent")) == 1
                });
            }
            return result;
        }

        public async Task MarkAsSentAsync(IEnumerable<long> ids)
        {
            using var conn = new SqliteConnection(_connectionString);
            await conn.OpenAsync();

            // Use a parameterized IN clause
            var idList = ids.ToList();
            if (idList.Count == 0)
                return;

            var parameters = idList.Select((id, i) => $"@id{i}").ToArray();
            var inClause = string.Join(",", parameters);

            using var cmd = conn.CreateCommand();
            cmd.CommandText = $"UPDATE mqtt_messages SET Sent = 1 WHERE Id IN ({inClause})";

            for (int i = 0; i < idList.Count; i++)
            {
                cmd.Parameters.AddWithValue(parameters[i], idList[i]);
            }

            await cmd.ExecuteNonQueryAsync();
        }
    }
}