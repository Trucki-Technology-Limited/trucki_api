﻿using Microsoft.EntityFrameworkCore;
using trucki.CustomExtension;
using trucki.DatabaseContext;

var builder = WebApplication.CreateBuilder(args);
var config = builder.Configuration;

// Add services to the container.
builder.Services.AddDbConfiguration(config);
builder.Services.AddIdentityConfiguration();
builder.Services.AddIdentityServerConfig(config);
builder.Services.AddDependencyInjection();



builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();
var connectionString = config.GetConnectionString("LocalConnection");
SeedData.EnsureSeedData(connectionString).Wait();

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<TruckiDBContext>();
    context.Database.Migrate();
}

app.UseCors(builder =>
{
    builder
        .WithOrigins("http://localhost:3000", "https://localhost:3000")
        .SetIsOriginAllowedToAllowWildcardSubdomains()
        .AllowAnyHeader()
        .AllowCredentials()
        .WithMethods("GET", "PUT", "POST", "DELETE", "OPTIONS")
        .SetPreflightMaxAge(TimeSpan.FromSeconds(3600));

});

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseHttpsRedirection();
app.UseIdentityServer();
app.UseAuthentication();
app.MapControllers();
app.UseAuthorization();
app.Run();

