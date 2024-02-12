using Microsoft.EntityFrameworkCore;
using trucki.DBContext;

namespace trucki.CustomExtension
{
    public static class DBConfig
    {
        public static void AddDbConfiguration(this IServiceCollection services, IConfiguration configuration)
        {
            var connectionString = configuration.GetConnectionString("LocalConnection");

            services.AddDbContext<TruckiDBContext>(options =>
            {
                options.UseNpgsql(connectionString);
            });
            AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
        }
    }
}
