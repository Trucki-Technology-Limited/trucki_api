using Duende.IdentityServer.EntityFramework.DbContexts;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using trucki.Models;

namespace trucki.CustomExtension;

public static class IdentityServerConfig
	{
		public static void AddIdentityServerConfig(this IServiceCollection serviceDescriptors, IConfiguration configuration)
		{
            var assembly = typeof(IdentityServerConfig).Assembly.GetName().Name;
            var connectionString = configuration.GetConnectionString("LocalConnection");
            var discoveryUrl = configuration.GetSection("IdentityServerSettings").GetSection("DiscoveryUrl").Value;


            var identityServerConfig = serviceDescriptors.AddIdentityServer(o =>
            {
                o.IssuerUri = discoveryUrl;
                o.InputLengthRestrictions.Scope = 1000;
                o.InputLengthRestrictions.Password = int.MaxValue;
                o.InputLengthRestrictions.UserName = int.MaxValue;

            });


            identityServerConfig.AddConfigurationStore(options =>
            {
                options.ConfigureDbContext = builder =>
                {
                    builder.UseNpgsql(connectionString, sql => sql.MigrationsAssembly(assembly));
                };
            }).AddOperationalStore(options =>
            {
                options.ConfigureDbContext = builder =>
                {
                    builder.UseNpgsql(connectionString, sql => sql.MigrationsAssembly(assembly));
                };
            }).AddAspNetIdentity<User>().AddDeveloperSigningCredential();
            serviceDescriptors.AddAuthentication(x =>
            {
                x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer("Bearer", options =>
            {
                options.Authority = discoveryUrl;
                options.Audience = "trucki";
                options.SaveToken = true;
                options.RequireHttpsMetadata = false;
            });

            var serviceProvider = serviceDescriptors.BuildServiceProvider();
            using var scope = serviceProvider.GetRequiredService<IServiceScopeFactory>().CreateScope();
            scope.ServiceProvider.GetService<PersistedGrantDbContext>().Database.Migrate();
            var context = scope.ServiceProvider.GetService<ConfigurationDbContext>();
            context.Database.Migrate();
        }
    }