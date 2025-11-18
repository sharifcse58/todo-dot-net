using Microsoft.OpenApi.Models;
using MyApiProject;
using MyApiProject.Repositories;
using MyApiProject.Middleware;
using Serilog;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Swashbuckle.AspNetCore.SwaggerGen;
using Microsoft.Extensions.Options;
using System.Reflection;
using MyApiProject.Swagger;


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

// ---------- API Versioning ----------
builder.Services.AddApiVersioning(options =>
{
    // Default API version if not specified
    options.DefaultApiVersion = new ApiVersion(1, 0);
    options.AssumeDefaultVersionWhenUnspecified = true;

    // Report supported versions in response headers
    options.ReportApiVersions = true;

    // Allow versioning via URL path (/api/v1/controller)
    options.ApiVersionReader = new Microsoft.AspNetCore.Mvc.Versioning.UrlSegmentApiVersionReader();
});

// ---------- Versioned API Explorer for Swagger ----------
builder.Services.AddVersionedApiExplorer(options =>
{
    options.GroupNameFormat = "'v'VVV"; // e.g., v1, v2
    options.SubstituteApiVersionInUrl = true;
});



// Swagger
builder.Services.AddEndpointsApiExplorer();
//builder.Services.AddSwaggerGen();
builder.Services.AddSwaggerGen(c =>
{
    //c.SwaggerDoc("v1", new OpenApiInfo
    //{
    //    Title = "MyApiProject V1",
    //    Version = "v1",
    //    Description = "ASP.NET Core Web API (MongoDB, Repository, Static API Key)"
    //});

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

builder.Services.ConfigureOptions<ConfigureSwaggerOptions>();


// Repository
builder.Services.AddScoped<IUserRepository, UserRepository>();

var app = builder.Build();

// Swagger (dev + prod for learning)

// ---------- Swagger UI with versioning ----------
var provider = app.Services.GetRequiredService<IApiVersionDescriptionProvider>();


app.UseSwagger();
app.UseSwaggerUI(options =>
{
    foreach (var description in provider.ApiVersionDescriptions)
    {
        options.SwaggerEndpoint($"/swagger/{description.GroupName}/swagger.json", description.GroupName.ToUpperInvariant());
    }
});


// Simple static API-key gate: protects non-GET API calls, skips Swagger
app.UseStaticApiKeyAuth();




app.MapControllers();

app.Run();
