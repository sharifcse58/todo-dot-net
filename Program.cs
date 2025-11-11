using Microsoft.OpenApi.Models;
using MyApiProject;
using MyApiProject.Repositories;
using MyApiProject.Middleware;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Set up Serilog logging
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()                // Log to the console
    .WriteTo.File("logs/log-.txt", rollingInterval: RollingInterval.Day)  // Log to a rolling file
    .CreateLogger();

// Use Serilog for ASP.NET Core logging
builder.Host.UseSerilog();

// Controllers + automatic 400 on invalid ModelState because of [ApiController]
builder.Services.AddControllers();


// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "MyApiProject",
        Version = "v1",
        Description = "ASP.NET Core Web API (MongoDB, Repository, Static API Key)"
    });

    // Show x-api-key in Swagger
    c.AddSecurityDefinition("ApiKey", new OpenApiSecurityScheme
    {
        Description = "Static API key via header: x-api-key",
        Type = SecuritySchemeType.ApiKey,
        Name = "x-api-key",
        In = ParameterLocation.Header,
        Scheme = "ApiKeyScheme"
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "ApiKey" }
            },
            Array.Empty<string>()
        }
    });
});

// Mongo: register MongoDbContext as a singleton (MongoClient is thread-safe)
builder.Services.AddSingleton<MongoDbContext>();

// Repository
builder.Services.AddScoped<IUserRepository, UserRepository>();

var app = builder.Build();

// Swagger (dev + prod for learning)
app.UseSwagger();
app.UseSwaggerUI();

// Simple static API-key gate: protects non-GET API calls, skips Swagger
app.UseStaticApiKeyAuth();

app.MapControllers();

app.Run();
