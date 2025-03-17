using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.Filters;
using trucki.CustomExtension;
using trucki.DatabaseContext;
using trucki.Hubs;

var builder = WebApplication.CreateBuilder(args);
var config = builder.Configuration;

// Add services to the container.
builder.Services.AddDbConfiguration(config);
builder.Services.AddIdentityConfiguration();
builder.Services.AddIdentityServerConfig(config);
builder.Services.AddDependencyInjection();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSignalR();
//builder.Services.AddSwaggerGen();
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("oauth2", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey
    });
    options.OperationFilter<SecurityRequirementsOperationFilter>();

});

var app = builder.Build();
var connectionString = config.GetConnectionString("LocalConnection");
SeedData.EnsureSeedData(connectionString).Wait();
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<TruckiDBContext>();
    context.Database.Migrate();
}

// if (app.Environment.IsDevelopment())
// {
app.UseSwagger();
app.UseSwaggerUI();
// }

app.UseCors();
app.UseIdentityServer();
app.UseAuthentication();
app.UseRouting();
app.UseAuthorization();

// Configure the HTTP request pipeline.
app.UseEndpoints(endpoints =>
{
    endpoints.MapControllers();
    // Map the SignalR hub
    endpoints.MapHub<ChatHub>("/chathub");
});
app.Run();
