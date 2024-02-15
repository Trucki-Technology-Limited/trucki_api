using Duende.IdentityServer.EntityFramework.DbContexts;
using Duende.IdentityServer.EntityFramework.Mappers;
using Duende.IdentityServer.EntityFramework.Storage;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using trucki.DBContext;
using trucki.Models;

namespace trucki.CustomExtension;

public class SeedData
    {
        public static async Task EnsureSeedData(string connectionString)
        {
            var services = new ServiceCollection();
            services.AddLogging();


            services.AddDbContext<TruckiDBContext>(
              options => options.UseNpgsql(connectionString));
            services.AddIdentity<User, IdentityRole>()
                    .AddEntityFrameworkStores<TruckiDBContext>()
                    .AddDefaultTokenProviders();
            services.AddOperationalDbContext(options =>
            {
                options.ConfigureDbContext = db =>
                   db.UseNpgsql(connectionString, sql => sql.MigrationsAssembly(typeof(SeedData).Assembly.FullName));
            });
            services.AddConfigurationDbContext(options =>
            {
                options.ConfigureDbContext = db =>
                   db.UseNpgsql(connectionString, sql => sql.MigrationsAssembly(typeof(SeedData).Assembly.FullName));
            });
            var serviceProvider = services.BuildServiceProvider();
            using var scope = serviceProvider.GetRequiredService<IServiceScopeFactory>().CreateScope();
            scope.ServiceProvider.GetService<PersistedGrantDbContext>().Database.Migrate();
            var context = scope.ServiceProvider.GetService<ConfigurationDbContext>();
            context.Database.Migrate();
            EnsureSeedData(context);
            var ctx = scope.ServiceProvider.GetService<TruckiDBContext>();
            ctx.Database.Migrate();
        }
    
        private static void EnsureSeedData(ConfigurationDbContext context)
        {
            // TODO :: refactor the condition below and check that each client is added

            if (!context.Clients.Any())
            {
                foreach (var client in IdentityServerConfiguration.Clients.ToList())
                {
                    context.Clients.Add(client.ToEntity());
                }

                context.SaveChanges();
            }
            else
            {
                int existingScopes = 0;
                var existingClients = context.Clients.Include(x => x.AllowedScopes).ToList();

                foreach (var client in existingClients)
                {
                    existingScopes = existingScopes + client.AllowedScopes.Count;
                }

                //this is to fix an existing prod bug with clients not having serial ids, an id is missing which requires that we add one step increment
                existingScopes = existingScopes + 1;

                foreach (var client in IdentityServerConfiguration.Clients.ToList())
                {
                    var checkResource = existingClients.FirstOrDefault(x => x.ClientId == client.ClientId);

                    if (checkResource == null)
                    {
                        context.Clients.Add(client.ToEntity());
                    }
                    else
                    {
                        foreach (var allowedScopes in client.AllowedScopes)
                        {
                            var scopeCheck = checkResource.AllowedScopes.FirstOrDefault(x => x.Scope == allowedScopes);




                            if (scopeCheck == null)
                            {
                                existingScopes = existingScopes + 1;
                                //context.i
                                //checkResource.AllowedScopes.Add(new ClientScope { Client = checkResource, ClientId = checkResource.Id, Id = existingScopes, Scope= allowedScopes });
                                var sql = "INSERT INTO \"ClientScopes\" (\"Id\", \"Scope\", \"ClientId\") VALUES (@p0, @p1, @p2)";
                                var parameters = new object[] { existingScopes, allowedScopes, checkResource.Id };
                                context.Database.ExecuteSqlRaw(sql, parameters);
                            }

                        }
                    }


                }

                context.SaveChanges();
            }

            if (!context.IdentityResources.Any())
            {
                foreach (var resource in IdentityServerConfiguration.IdentityResources.ToList())
                {
                    context.IdentityResources.Add(resource.ToEntity());
                }

                context.SaveChanges();
            }
            else
            {
                foreach (var resource in IdentityServerConfiguration.IdentityResources.ToList())
                {
                    var checkResource = context.IdentityResources.FirstOrDefault(x => x.Name == resource.Name);

                    if (checkResource == null)
                    {
                        context.IdentityResources.Add(resource.ToEntity());
                    }

                }

                context.SaveChanges();
            }

            if (!context.ApiScopes.Any())
            {
                foreach (var resource in IdentityServerConfiguration.ApiScopes.ToList())
                {
                    context.ApiScopes.Add(resource.ToEntity());
                }

                context.SaveChanges();
            }
            else
            {
                foreach (var resource in IdentityServerConfiguration.ApiScopes.ToList())
                {
                    var checkResource = context.ApiScopes.FirstOrDefault(x => x.Name == resource.Name);

                    if (checkResource == null)
                    {
                        context.ApiScopes.Add(resource.ToEntity());
                    }


                }
                context.SaveChanges();
            }

            if (!context.ApiResources.Any())
            {
                foreach (var resource in IdentityServerConfiguration.ApiResources.ToList())
                {
                    context.ApiResources.Add(resource.ToEntity());
                }

                context.SaveChanges();
            }
            else
            {

                foreach (var resource in IdentityServerConfiguration.ApiResources.ToList())
                {
                    var checkRescource = context.ApiResources.Where(x => x.Name == resource.Name).FirstOrDefault();

                    if (checkRescource == null)
                    {
                        context.ApiResources.Add(resource.ToEntity());
                    }


                }
                context.SaveChanges();
            }

        }
    }