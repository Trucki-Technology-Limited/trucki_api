namespace trucki.CustomExtension
{
    public static class ServiceExtension
    {
        public static void AddDependencyInjection(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddAuthorization(options =>
            {

                options.AddPolicy("CanDoSomething", policy =>
                                  policy.RequireClaim("Permission", "Permission"));
            });

            //Mapper
            //services.AddScoped<Interface, Implemenation>();

        }
    }
}