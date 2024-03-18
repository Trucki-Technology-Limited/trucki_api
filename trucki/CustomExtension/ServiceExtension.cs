using Mailjet.Client;
using Microsoft.EntityFrameworkCore;
using trucki.DatabaseContext;
using trucki.Interfaces.IRepository;
using trucki.Interfaces.IServices;
using trucki.Repository;
using trucki.Services;
using trucki.Shared;

namespace trucki.CustomExtension
{
    public static class ServiceExtension
    {
        public static void AddDependencyInjection(this IServiceCollection services)
        {
            services.AddCors(options =>
            {
                options.AddDefaultPolicy(builder =>
                {
                    builder.WithOrigins("http://localhost:3000","https://trucki-web.vercel.app/v1", "http://157.245.4.44") // Replace with your HTML file's origin
                        .AllowAnyHeader()
                        .AllowAnyMethod()
                        .AllowCredentials();
                });
            });
            services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
            services.AddScoped<ITokenService, TokenService>();
            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<IAdminRepository, AdminRepository>();
            services.AddScoped<IAdminService, AdminService>();
            services.AddScoped<ICompanyRepository, CompanyRepository>();
            services.AddScoped<ICompanyServices, CompanyServices>();  
            services.AddScoped<IDriverRepository, DriverRepository>();
            services.AddScoped<IDriverService, DriverService>();
            services.AddScoped<IBusinessRepository, BusinessRepository>();
            services.AddScoped<IBusinessService, BusinessService>();
            services.AddScoped<IManagerService, ManagerService>();
            services.AddScoped<IManagerRepository, ManagerRepository>();
            services.AddScoped<INotificationService, NotificationService>();
        }

        public static void ConfigureDatabaseContext(this IServiceCollection services, IConfiguration configuration) =>
          services.AddDbContext<TruckiDBContext>(opts => opts.UseNpgsql(configuration.GetConnectionString("LocalConnection1")));

        public static void ConfigureMailJet(this IServiceCollection services, IConfiguration configuration) =>
          services.AddHttpClient<IMailjetClient, MailjetClient>(client =>
          {
              client.UseBasicAuthentication(configuration.GetSection("MailJetKeys")["ApiKey"], configuration.GetSection("MailJetKeys")["ApiSecret"]);
          });

    }
}