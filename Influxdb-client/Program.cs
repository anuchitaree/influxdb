using InfluxDB.Client;
using Influxdb_client.Data;
using Influxdb_client.Models;
using Newtonsoft.Json;
using System.Text.Json;

namespace Influxdb_client
{
    public class Program
    {
        public static async Task Main(string[] args)
        {

            // ตั้งค่าเชื่อมต่อ
            const string url = "http://localhost:8086";   // URL ของ InfluxDB
            const string token = "yJECQJFxVOXPFxl5w8ZbIXXrHx_TqaR_0FMDAmQVH6WU9axLhNAnBUXu-nSGvQ8MkFi1eXn7nSel6GFuQgHyqg==";            // ใส่ token ที่มีสิทธิ์อ่าน
            const string org = "influxdata";              // org ที่คุณใช้
            const string bucket = "result";               // bucket ที่ restore มา

            using var client = new InfluxDBClient(url, token);

            var queryApi = client.GetQueryApi();

            // Flux query ดึงข้อมูลทั้งหมด
            var flux = $@"from(bucket: ""{bucket}"")
                      |> range(start: -2d)";
            //|> limit(n: 10)";


            var tables = await queryApi.QueryAsync(flux, org);

            var recordsList = new List<Dictionary<string, object>>();

            try
            {


                using (var db = new SensorContext())
                {
                    foreach (var table in tables)
                    {
                        List<MeasureRaw> raw = new List<MeasureRaw>();
                        foreach (var record in table.Records)
                        {
                            //var dict = new Dictionary<string, object>
                            //{
                            //    { "Time", record.GetTime() },
                            //    { "Measurement", record.GetMeasurement() },
                            //    { "Field", record.GetField() },
                            //    { "Value", record.GetValue() },
                            //    { "Tags", record.Values } // รวม tag และ metadata ทั้งหมด
                            //};

                            //recordsList.Add(dict);
                            try
                            {
                                string jsonOutput = JsonConvert.SerializeObject(record, Formatting.Indented);
                                using JsonDocument doc = JsonDocument.Parse(jsonOutput);

                                string time = doc.RootElement.GetProperty("Values")
                                             .GetProperty("_time").GetString()!;

                                JsonElement valueElement = doc.RootElement.GetProperty("Values").GetProperty("_value");

                                double value = 0;

                                if (valueElement.ValueKind == JsonValueKind.Null)
                                {
                                    value = 0;
                                }
                                else if (valueElement.ValueKind == JsonValueKind.Number)
                                {
                                    value = valueElement.GetDouble();
                                }


                                string sensor = doc.RootElement.GetProperty("Values")
                                            .GetProperty("_field").GetString()!;

                                string line = doc.RootElement.GetProperty("Values")
                                            .GetProperty("_measurement").GetString()!;


                                var rec = new MeasureRaw();

                                rec.ID = 0;
                                rec.Time = Convert.ToDateTime(time).ToUniversalTime();
                                rec.Measurement = line;
                                rec.Field = sensor;
                                rec.Value = value;

                                raw.Add(rec);
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine(ex.Message.ToString());
                            }
                        }
                        db.AddRange(raw);
                        db.SaveChanges();
                    }

                   
                }
                Console.WriteLine("Completed");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message.ToString());
            }

            //// แปลงเป็น JSON string
            //string jsonOutput = JsonConvert.SerializeObject(recordsList, Formatting.Indented);

            //// เขียนลงไฟล์
            //File.WriteAllText("C:\\tmp\\influx_result.json", jsonOutput);

            //Console.WriteLine("✅ Export เสร็จสิ้น: influx_result.json");
        }
    }
}
