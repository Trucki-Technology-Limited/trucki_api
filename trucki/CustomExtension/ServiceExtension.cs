using Microsoft.AspNetCore.Identity.UI.Services;
using trucki.BackgroundServices;
using trucki.Interfaces.IRepository;
using trucki.Interfaces.IServices;
using trucki.Repositories;
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
                    builder.WithOrigins("http://localhost:3000", "http://172.20.10.3:3000", "https://localhost:3000", "https://trucki-drab.vercel.app", "https://web.trucki.co","https://staging-app.trucki.co","https://app.trucki.co") // Replace with your HTML file's origin
                        .AllowAnyHeader()
                        .AllowAnyMethod()
                        .AllowCredentials();
                });
            });
            services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
            services.AddHostedService<BackgroundNotificationService>();
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
            services.AddScoped<IDocumentTypeRepository, DocumentTypeRepository>();
            services.AddScoped<IDocumentTypeService, DocumentTypeService>();
            services.AddScoped<IDriverDocumentService, DriverDocumentService>();
            services.AddScoped<IDriverDocumentRepository, DriverDocumentRepository>();
            services.AddScoped<ICargoOwnerRepository, CargoOwnerRepository>();
            services.AddScoped<ICargoOwnerService, CargoOwnerService>();
            services.AddScoped<ICargoOrderService, CargoOrderService>();
            services.AddScoped<IInvoiceService, InvoiceService>();
            services.AddScoped<IDriverBankAccountService, DriverBankAccountService>();
            services.AddScoped<IChatService, ChatService>();
            services.AddScoped<ITermsAndConditionsService, TermsAndConditionsService>();
            services.AddScoped<ITermsAndConditionsRepository, TermsAndConditionsRepository>();
            services.AddScoped<IStripeService, StripeService>();
            services.AddScoped<INotificationService, NotificationService>();
            services.AddScoped<INotificationRepository, NotificationRepository>();
            services.AddScoped<NotificationEventService>();
            services.AddScoped<IPDFService, PDFService>();
            services.AddScoped<IWalletService, WalletService>();
            services.AddScoped<IDriverWalletRepository, DriverWalletRepository>();

            services.AddScoped<IDriverWalletService, DriverWalletService>();
            services.AddHostedService<DriverWalletBackgroundService>();
            services.AddScoped<IOrderCancellationService, OrderCancellationService>();
            services.AddAutoMapper(typeof(OrderCancellationMappingProfile));
            services.AddScoped<IStripeConnectService, StripeConnectService>();
            services.AddScoped<IDriverPayoutService, DriverPayoutService>();

            services.AddScoped<IIntegratedPayoutService, IntegratedPayoutService>();


            services.AddHostedService<WeeklyPayoutBackgroundService>();
            services.AddScoped<IDriverRatingRepository, DriverRatingRepository>();
            services.AddScoped<IAccountDeletionRepository, AccountDeletionRepository>();
            services.AddScoped<IAccountDeletionService, AccountDeletionService>();


        }

    }
}