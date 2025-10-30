using Azure.Storage.Blobs;
using Microsoft.AspNetCore.Cors;
using RecipeAPI.Services;
using RecipeAPI.Models;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configure CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("http://localhost:8080", "http://127.0.0.1:8080")
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});

// Configure Azure Blob Storage
builder.Services.Configure<AzureStorageConfig>(builder.Configuration.GetSection("AzureStorage"));
builder.Services.AddSingleton<BlobServiceClient>(provider =>
{
    var config = builder.Configuration.GetSection("AzureStorage").Get<AzureStorageConfig>();
    return new BlobServiceClient(config.ConnectionString);
});

// Add custom services
builder.Services.AddSingleton<ILeaderboardService, LeaderboardService>();
builder.Services.AddScoped<IRecipeService, RecipeService>();

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowFrontend");
app.UseAuthorization();
app.MapControllers();

// Start server on specific port
app.Urls.Add("http://localhost:52112");

Console.WriteLine("🍳 Recipe CTF Backend API starting...");
Console.WriteLine("📍 Server running on: http://localhost:52112");
Console.WriteLine("📖 Swagger UI: http://localhost:52112/swagger");
Console.WriteLine("🔥 Ready to receive recipes!");

app.Run();