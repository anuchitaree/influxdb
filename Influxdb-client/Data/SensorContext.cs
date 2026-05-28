using Influxdb_client.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Influxdb_client.Data;

namespace Influxdb_client.Data
{
    public class SensorContext : DbContext
    {

        public DbSet<MeasureRaw> MeasureModels { get; set; }
      



        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {

                // Example: PostgreSQL
                //optionsBuilder.UseNpgsql("Server=localhost;Port=5432;Database=sensordb;Username=postgres;Password=postgres;Pooling=true;");

                //optionsBuilder.UseSqlServer("Server=localhost\\SQLEXPRESS;Database=MyAppDb;Trusted_Connection=True;");

                optionsBuilder.UseSqlServer("Server=localhost,1433;Database=sensordb;User Id=Admin;Password=Admin;TrustServerCertificate=True;");

                //optionsBuilder.UseSqlServer("Server=192.168.1.100;Database=MyAppDb;User Id=myuser;Password=mypassword;");

            }

        }

    }
}
