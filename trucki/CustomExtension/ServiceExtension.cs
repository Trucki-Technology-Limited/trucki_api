namespace trucki.CustomExtension
{
    public static class ServiceExtension
    {
        public static void AddDependencyInjection(this IServiceCollection services)
        {
            services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
            //Mapper
            //services.AddScoped<Interface, Implemenation>();

        }
    }
}