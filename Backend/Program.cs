using System.Text;
using Backend.Application.Service;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.OpenApi;
using Microsoft.OpenApi.Models;
using Backend.Service;
using Backend.Application.Service.Interfaces;
using Backend.External;
using SportsTimerBackend.External.APIControllers;
using Microsoft.IdentityModel.Tokens;
using Backend.External.Repos;
using Backend.Gateway;
using Npgsql;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        // Configure JSON serialization for MongoDB types
        options.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
    });
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "Swagger API", Version = "v1" });

    // Swagger base path at root since no reverse proxy
    options.AddServer(new OpenApiServer { Url = "/" });

    // Add JWT Authentication to Swagger
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter 'Bearer' [space] and then your valid JWT token.\n\nExample: \"Bearer eyJhbGciOiJIUzI1NiIs...\""
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// Add Rate Limiting service
builder.Services.AddRateLimiter(rateLimiterOptions =>
    rateLimiterOptions.AddFixedWindowLimiter("FixedWindowPolicy", options =>
    {
        options.PermitLimit = 1000; // 1000 requests
        options.Window = TimeSpan.FromSeconds(60); // per 60 seconds
        options.QueueProcessingOrder = System.Threading.RateLimiting.QueueProcessingOrder.OldestFirst;
        options.QueueLimit = 20; // Queue 20 requests when limit is reached
    }));


// Add environment-specific configuration
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

builder.Services.AddSingleton<ConnectionService>();
builder.Services.AddScoped<ILoginRepo, LoginRepo>();


// Register application-specific services with scoped lifetime

// Repositories
builder.Services.AddScoped<IUserRepo, UserRepo>();

// Services
builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<IJWTService, JWTService>();

// Authentication & Authorization

// Configure Authentication
// In Program.cs, update JWT configuration:
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["JwtConfig:Issuer"],
            ValidAudience = builder.Configuration["JwtConfig:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["JwtConfig:Key"])),

            // Add these for HTTPS/HTTP flexibility:
            ValidIssuers = new[] {
                "https://localhost:5153",
                "http://localhost:5153",
                "Backend"
            },
            ValidAudiences = new[] {
                "https://localhost:5153",
                "http://localhost:5153",
                "Frontend"
            }
        };
    });

builder.Services.AddAuthorization();



var app = builder.Build();

// Run seeding of database in background to avoid blocking startup
//Task task = Task.Run(() => SeedDatabase(mongoDatabase));

// Only use HTTPS redirection in production
//app.UseHttpsRedirection();

// Use CORS before other middleware
//app.UseCors("ReactApp");

// Configure the HTTP request pipeline.
// if (app.Environment.IsDevelopment())
// {

app.UseDefaultFiles();

// Allow all MIME types for static files
var contentTypeProvider = new FileExtensionContentTypeProvider();
// Ensure critical MIME types are correct
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
    ServeUnknownFileTypes = true // Serve any file type not explicitly mapped
});

app.UseSwagger(c =>
{
    c.RouteTemplate = "swagger/{documentname}/swagger.json";
});

app.UseSwaggerUI(c =>
{
    // Make the endpoint relative to the RoutePrefix so it works under /api/swagger
    c.SwaggerEndpoint("v1/swagger.json", "SportsTimer API v1");
    c.RoutePrefix = "swagger";
});

// Authentication & Authorization
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

/// <summary>
/// Seeds the database with a default admin user if one doesn't exist
/// </summary>
/// <param name="database">Database instance</param>
// static async Task SeedDatabase(IMongoDatabase database)
// {
//     var collection = database.GetCollection<Admin>("Admins");
//     var existing = await collection.Find(a => a.Username == "Admin").FirstOrDefaultAsync();
//     if (existing == null)
//     {
//         var user = new User { Username = "default_user", Password = "topsecret2024-2027" };
//         await collection.InsertOneAsync(user);
//     }
// }