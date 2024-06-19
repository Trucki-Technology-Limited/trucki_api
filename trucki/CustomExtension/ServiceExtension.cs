using Microsoft.AspNetCore.Identity.UI.Services;
using trucki.Interfaces.IRepository;
using trucki.Interfaces.IServices;
using trucki.Repository;
using trucki.Services;

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
                    builder.WithOrigins("http://localhost:3000","https://localhost:3000", "https://trucki-drab.vercel.app") // Replace with your HTML file's origin
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
            services.AddScoped<IUploadService, UploadService>();
            services.AddTransient<IEmailService, EmailService>();
            services.AddScoped<IBusinessRepository, BusinessRepository>();
            services.AddScoped<ICustomerRepository, CustomerRepository>();
            services.AddScoped<IDriverRepository, DriverRepository>();
            services.AddScoped<IFieldOfficerRepository, FieldOfficerRepository>();
            services.AddScoped<IManagerRepository, ManagerRepository>();
            services.AddScoped<IOrderRepository, OrderRepository>();
            services.AddScoped<ITruckOwnerRepository, TruckOwnerRepository>();
            services.AddScoped<ITruckRepository, TruckRepository>();
            services.AddScoped<IBusinessService, BusinessService>();
            services.AddScoped<ICustomerService, CustomerService>();
            services.AddScoped<IDriverService, DriverService>();
            services.AddScoped<IFieldOfficerService, FieldOfficerService>();
            services.AddScoped<IManagerService, ManagerService>();
            services.AddScoped<IOrderService, OrderService>();
            services.AddScoped<ITruckOwnerService, TruckOwnerService>();
            services.AddScoped<ITruckService, TruckService>();
        }
    }
}