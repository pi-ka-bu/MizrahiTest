using System.Text;
using Microsoft.AspNetCore.Authentication;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using StackExchange.Redis;
using TestAPI.Configuration;
using TestAPI.Filters;
using TestAPI.Interfaces;
using TestAPI.Middlewares;
using TestAPI.Security;
using TestAPI.Services;

var builder = WebApplication.CreateBuilder(args);

// Configuration
builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection(JwtSettings.SectionName));
builder.Services.Configure<KafkaSettings>(
    builder.Configuration.GetSection(KafkaSettings.SectionName)
);
builder.Services.Configure<CacheSettings>(
    builder.Configuration.GetSection(CacheSettings.SectionName)
);
builder.Services.Configure<ExternalApiSettings>(
    builder.Configuration.GetSection(ExternalApiSettings.SectionName)
);

// Controllers
builder
    .Services.AddControllers(options =>
    {
        options.InputFormatters.RemoveType<Microsoft.AspNetCore.Mvc.Formatters.SystemTextJsonInputFormatter>();
        options.OutputFormatters.RemoveType<Microsoft.AspNetCore.Mvc.Formatters.SystemTextJsonOutputFormatter>();
    })
    .AddNewtonsoftJson(opts =>
    {
        opts.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
        opts.SerializerSettings.Converters.Add(
            new StringEnumConverter(new CamelCaseNamingStrategy())
        );
    })
    .AddXmlSerializerFormatters();

// Authentication & Authorization
builder
    .Services.AddAuthentication(BearerAuthenticationHandler.SchemeName)
    .AddScheme<AuthenticationSchemeOptions, BearerAuthenticationHandler>(
        BearerAuthenticationHandler.SchemeName,
        null
    );
builder.Services.AddAuthorization();

// Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc(
        "v1",
        new OpenApiInfo
        {
            Version = "v1",
            Title = "TestAPI",
            Description = "API for performing simple arithmetic operations (ASP.NET Core 8.0)",
            Contact = new OpenApiContact { Name = "Calculator API" },
        }
    );

    c.CustomSchemaIds(type => type.FullName?.Replace("+", "."));

    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        c.IncludeXmlComments(xmlPath);
    }

    c.OperationFilter<OpValidationFilter>();

    // JWT Authentication in Swagger
    c.AddSecurityDefinition(
        "Bearer",
        new OpenApiSecurityScheme
        {
            Description =
                "JWT Authorization header using the Bearer scheme. Enter 'Bearer' [space] and then your token.",
            Name = "Authorization",
            In = ParameterLocation.Header,
            Type = SecuritySchemeType.ApiKey,
            Scheme = "Bearer",
        }
    );

    c.AddSecurityRequirement(
        new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference
                    {
                        Type = ReferenceType.SecurityScheme,
                        Id = "Bearer",
                    },
                },
                Array.Empty<string>()
            },
        }
    );
});

// Cache Services
builder.Services.AddMemoryCache();
var cacheSettings = builder
    .Configuration.GetSection(CacheSettings.SectionName)
    .Get<CacheSettings>();
if (cacheSettings?.Type == CacheType.Redis)
{
    builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
    {
        var config = ConfigurationOptions.Parse(cacheSettings.RedisConnectionString!);
        config.AbortOnConnectFail = false;
        return ConnectionMultiplexer.Connect(config);
    });
    builder.Services.AddSingleton<ICacheService, RedisCacheService>();
}
else
{
    builder.Services.AddSingleton<ICacheService, MemoryCacheService>();
}

// Core Services
builder.Services.AddScoped<CalculationService>();
builder.Services.AddScoped<ICalculationService>(sp =>
{
    var calculator = sp.GetRequiredService<CalculationService>();
    var cache = sp.GetRequiredService<ICacheService>();
    var metadata = sp.GetRequiredService<IMetadataService>();
    var eventPublisher = sp.GetRequiredService<IEventPublisher>();
    var cacheConfig = sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<CacheSettings>>();
    var logger = sp.GetRequiredService<ILogger<CachedCalculatorService>>();

    return new CachedCalculatorService(
        calculator,
        cache,
        metadata,
        eventPublisher,
        cacheConfig,
        logger
    );
});

// External Services
builder.Services.AddHttpClient<IMetadataService, MetadataService>();

// Event Publisher
builder.Services.AddSingleton<IEventPublisher, KafkaEventPublisher>();

// JWT Token Generator
builder.Services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();

var app = builder.Build();

// Configure the HTTP request pipeline
app.UseMiddleware<ErrorHandlingMiddleware>();
app.UseMiddleware<RequestResponseLoggingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "TestAPI v1");
        c.RoutePrefix = "swagger";
    });
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();

// Make Program class accessible for integration tests
public partial class Program { }
