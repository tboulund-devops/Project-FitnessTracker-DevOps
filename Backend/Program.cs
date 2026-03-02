using System.Text;
using Backend.Application.Service;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.OpenApi.Models;
using Backend.Service;
using Backend.Application.Service.Interfaces;
using Backend.External;
using Backend.External.APIControllers;
using Backend.External.Repos;
using Backend.External.Repos.Interface;
using Backend.Gateway;
using Npgsql;
using ILoginRepo = Backend.Gateway.ILoginRepo;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
    });

builder.Services.AddEndpointsApiExplorer();

// Swagger configuration
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "Swagger API", Version = "v1" });
    options.AddServer(new OpenApiServer { Url = "/" });
});


// Environment-specific configuration
builder.Configuration
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true)
    .AddEnvironmentVariables();

// Database Configuration
builder.Services.AddScoped<NpgsqlConnection>(sp =>
{
    var configuration = sp.GetRequiredService<IConfiguration>();
    var connectionString = configuration.GetConnectionString("DefaultConnection");
    return new NpgsqlConnection(connectionString);
});

builder.Services.AddSingleton<IConnectionService,ConnectionService>();

// Repositories
builder.Services.AddScoped<IUserRepo, UserRepo>();
builder.Services.AddScoped<ILoginRepo, LoginRepo>();
builder.Services.AddScoped<IWorkoutRepo, WorkoutRepo>();

// Services
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddSingleton<IDatabaseSeedingService, DatabaseSeedingService>();
builder.Services.AddScoped<ILoginService, LoginService>();
builder.Services.AddScoped<IWorkoutService, WorkoutService>();

var app = builder.Build();

// Seed database
using (var scope = app.Services.CreateScope())
{
    var seeder = scope.ServiceProvider.GetRequiredService<IDatabaseSeedingService>();
    seeder.Seed();
}

// Static files setup
app.UseDefaultFiles();

var contentTypeProvider = new FileExtensionContentTypeProvider();
contentTypeProvider.Mappings[".js"] = "application/javascript";
contentTypeProvider.Mappings[".mjs"] = "application/javascript";
contentTypeProvider.Mappings[".css"] = "text/css";
contentTypeProvider.Mappings[".weba"] = "audio/webp";
contentTypeProvider.Mappings[".webp"] = "image/webp";
contentTypeProvider.Mappings[".webm"] = "video/webm";
contentTypeProvider.Mappings[".svg"] = "image/svg+xml";
contentTypeProvider.Mappings[".svgz"] = "image/svg+xml";
contentTypeProvider.Mappings[".json"] = "application/json";
contentTypeProvider.Mappings[".wasm"] = "application/wasm";
contentTypeProvider.Mappings[".woff"] = "font/woff";
contentTypeProvider.Mappings[".woff2"] = "font/woff2";
contentTypeProvider.Mappings[".ttf"] = "font/ttf";
contentTypeProvider.Mappings[".otf"] = "font/otf";
contentTypeProvider.Mappings[".eot"] = "application/vnd.ms-fontobject";

app.UseStaticFiles(new StaticFileOptions
{
    ContentTypeProvider = contentTypeProvider,
    ServeUnknownFileTypes = true
});

// Swagger middleware
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Swagger API v1");
    c.RoutePrefix = "swagger";
});

app.MapControllers();

app.Run();

public partial class Program{}
