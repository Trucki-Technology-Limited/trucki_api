using Duende.IdentityServer.EntityFramework.DbContexts;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using trucki.DatabaseContext;
using trucki.Entities;

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
            // Add this section for SignalR token handling
            options.Events = new JwtBearerEvents
            {
                OnMessageReceived = context =>
                    {
        var accessToken = context.Request.Query["access_token"];

                        // If the request is for the hub...
        var path = context.HttpContext.Request.Path;
        if (!string.IsNullOrEmpty(accessToken) &&
            path.StartsWithSegments("/chathub"))
        {
                            // Read the token out of the query string
            context.Token = accessToken;
        }
        return Task.CompletedTask;
    }
            };
        });
        var serviceProvider = serviceDescriptors.BuildServiceProvider();
        using var scope = serviceProvider.GetRequiredService<IServiceScopeFactory>().CreateScope();
        scope.ServiceProvider.GetService<PersistedGrantDbContext>().Database.Migrate();
        //scope.ServiceProvider.GetService<OperationalDbContext>().Database.Migrate();
        var context = scope.ServiceProvider.GetService<ConfigurationDbContext>();
        context.Database.Migrate();
        //EnsureSeedData(context, configuration);
    }
}