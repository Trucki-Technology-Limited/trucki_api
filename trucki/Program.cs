﻿using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
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

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors();
app.UseIdentityServer();
app.UseAuthentication();
app.UseRouting();
app.UseAuthorization();
app.MapControllers();


app.Run();
