using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using System.Reflection;
using trucki.CustomExtension;
using trucki.DatabaseContext;

var builder = WebApplication.CreateBuilder(args);
var config = builder.Configuration;

// Add services to the container.
builder.Services.AddDbConfiguration(config);
builder.Services.AddIdentityConfiguration();
builder.Services.AddIdentityServerConfig(config);
builder.Services.AddDependencyInjection();  
builder.Services.ConfigureDatabaseContext(config);
builder.Services.ConfigureMailJet(config);

AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);


builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
//builder.Services.AddSwaggerGen();

builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Version = "v1",
        Title = "Trucki  API",
        Description = "This is an e-commerce application for various kinds of transportation",
    });
    /*// using System.Reflection;
    var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFilename));*/

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme()
    {
        In = ParameterLocation.Header,
        Description = @"Put **_ONLY_** your JWT Bearer token in textbox below!.
         <br/>
         Example: 'eyshdhdhdh'",
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement() {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                },
                Scheme = JwtBearerDefaults.AuthenticationScheme,
                Name = JwtBearerDefaults.AuthenticationScheme,
                In = ParameterLocation.Header
            },
            new List<string>()
        }
    });

});


var app = builder.Build();
/*var connectionString = config.GetConnectionString("LocalConnection");
SeedData.EnsureSeedData(connectionString).Wait();
 using (var scope = app.Services.CreateScope())
 {
     var context = scope.ServiceProvider.GetRequiredService<TruckiDBContext>();
    context.Database.Migrate();
 }*/

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors();
app.UseIdentityServer();
app.UseAuthentication();
app.UseHttpsRedirection();
app.MapControllers();
app.UseAuthorization();
app.Run();

