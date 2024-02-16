using Microsoft.AspNetCore.Identity;
using trucki.DatabaseContext;
using trucki.Entities;


namespace trucki.CustomExtension;

public static class IdentityConfiguration
{
    public static void AddIdentityConfiguration(this IServiceCollection services)
    {
        services.AddIdentity<User, IdentityRole>(options =>
            {

                options.User.RequireUniqueEmail = true;

                options.SignIn.RequireConfirmedEmail = false;
                options.Password.RequireDigit = false;
                options.Password.RequireLowercase = false;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireUppercase = false;
                options.Password.RequiredLength = 0;

            })
            .AddEntityFrameworkStores<TruckiDBContext>()
            .AddDefaultTokenProviders(); // to provide the need token for reset password and confirm email
        // services.AddDefaultIdentity<ApplicationUser>();
        // services.AddIdentityCore<ApplicationRole>().AddRoles<ApplicationRole>()
        //     .AddEntityFrameworkStores<TruckiDBContext>();
    }
}