using Cinema_Reservation.Factories;
using Cinema_Reservation.MiddleWares;
using DomainLayer.Contracts;
using HealthChecks.UI.Client;
using MassTransit;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Persistence.Data;
using Persistence.Repositories;
using QuestPDF.Infrastructure;
using Service;
using Service.Consumers;
using Service.Mapping_Profiles;
using ServiceAbstraction;
using Shared.Hubs;
using StackExchange.Redis;
using System.Security.Claims;
using System.Text.Json;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// Config Helpers
var keycloakAuthority = builder.Configuration["Keycloak:Authority"]!;
var keycloakExternalUrl = builder.Configuration["Keycloak:ExternalUrl"]!;
var keycloakClientId = builder.Configuration["Keycloak:ClientId"]!;

// 1. DATABASE CONFIGURATION
builder.Services.AddDbContext<EventReservationDbcontext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        npgsqlOptions =>
        {
            npgsqlOptions.EnableRetryOnFailure(
                maxRetryCount: 5,
                maxRetryDelay: TimeSpan.FromSeconds(10),
                errorCodesToAdd: null);
        }));

// 2. REDIS CONFIGURATION
builder.Services.AddSingleton<IConnectionMultiplexer>(_ =>
{
    var configuration = ConfigurationOptions.Parse(builder.Configuration.GetConnectionString("Redis")!, true);
    return ConnectionMultiplexer.Connect(configuration);
});

builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = builder.Configuration.GetConnectionString("Redis");
    options.InstanceName = "Events_";
});

builder.Services.AddOutputCache(options =>
{
    options.AddBasePolicy(policy => policy.Expire(TimeSpan.FromSeconds(10)));
});

builder.Services.AddStackExchangeRedisOutputCache(options =>
{
    options.Configuration = builder.Configuration.GetConnectionString("Redis");
    options.InstanceName = "OutputEvents_";
});

// 3. DEPENDENCY INJECTION (SERVICES & REPOSITORIES)
builder.Services.AddScoped<IUnitOfWorkRepository, UnitOfWorkRepository>();
builder.Services.AddScoped<ISeatService, SeatService>();
builder.Services.AddScoped<ISeatReservationService, SeatReservationService>();
builder.Services.AddScoped<IEventService, EventService>();
builder.Services.AddTransient<IEmailService, EmailService>();

// Factory Registrations
builder.Services.AddScoped<Func<ISeatReservationService>>(provider => () => provider.GetRequiredService<ISeatReservationService>());
builder.Services.AddScoped<Func<IPaymentService>>(provider => () => provider.GetRequiredService<IPaymentService>());
builder.Services.AddScoped<Func<IEventService>>(provider => () => provider.GetRequiredService<IEventService>());
builder.Services.AddScoped<Func<ISeatService>>(provider => () => provider.GetRequiredService<ISeatService>());

builder.Services.AddScoped<IServiceManager, ServiceManager>();
builder.Services.AddScoped<TicketPdfService>();
builder.Services.AddSignalR();
builder.Services.AddHttpClient();
builder.Services.AddScoped<IPaymentService, PaymentService>();

// AutoMapper
builder.Services.AddAutoMapper(x => x.AddProfile(new SeatProfile()));
builder.Services.AddAutoMapper(x => x.AddProfile(new EventProfile()));

// 4. CONTROLLERS & JSON OPTIONS
builder.Services.AddControllers()
    .AddJsonOptions(options => { options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles; })
    .ConfigureApiBehaviorOptions(options =>
    {
        options.InvalidModelStateResponseFactory = ApiResponseFactory.GenerateApiValidationErrorResponse;
    });

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("http://localhost:3000")
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

// 5. KEYCLOAK AUTHENTICATION
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
.AddJwtBearer(options =>
{
    options.MetadataAddress = builder.Configuration["Keycloak:MetadataAddress"]!;
    options.Audience = builder.Configuration["Keycloak:Audience"];
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidIssuer = builder.Configuration["Keycloak:Authority"],
        ValidateAudience = false,
        ValidateLifetime = true
    };
    options.RequireHttpsMetadata = false;

    options.Events = new JwtBearerEvents
    {
        OnTokenValidated = context =>
        {
            var identity = context.Principal?.Identity as ClaimsIdentity;
            var realmAccessClaim = identity?.FindFirst("realm_access");
            if (realmAccessClaim != null)
            {
                var realmAccessObj = JsonDocument.Parse(realmAccessClaim.Value).RootElement;
                if (realmAccessObj.TryGetProperty("roles", out var roles))
                {
                    foreach (var role in roles.EnumerateArray())
                        identity!.AddClaim(new Claim(ClaimTypes.Role, role.GetString()!));
                }
            }
            return Task.CompletedTask;
        }
    };
});

// 6. SWAGGER (External Browser Fix)
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("OAuth2", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.OAuth2,
        Flows = new Microsoft.OpenApi.Models.OpenApiOAuthFlows
        {
            AuthorizationCode = new Microsoft.OpenApi.Models.OpenApiOAuthFlow
            {
                // Fix: Uses the External URL for the browser redirect
                AuthorizationUrl = new Uri($"{keycloakExternalUrl}/protocol/openid-connect/auth"),
                TokenUrl = new Uri($"{keycloakExternalUrl}/protocol/openid-connect/token"),
                Scopes = new Dictionary<string, string>
                {
                    { "openid", "OpenID Connect scope" },
                    { "profile", "User profile" }
                }
            }
        }
    });

    options.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference { Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme, Id = "OAuth2" }
            },
            Array.Empty<string>()
        }
    });
});

// 7. MASSTRANSIT
builder.Services.AddMassTransit(conf =>
{
    conf.AddConsumer<BookingConsumer>();
    conf.AddConsumer<BookingNotificationConsumer>();
    conf.AddEntityFrameworkOutbox<EventReservationDbcontext>(o => { o.UsePostgres(); o.UseBusOutbox(); });
    conf.SetKebabCaseEndpointNameFormatter();
    conf.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host(new Uri(builder.Configuration["RabbitMQ:ConnectionString"]!));
        cfg.UseMessageRetry(r => { r.Interval(3, TimeSpan.FromSeconds(5)); r.Ignore<ArgumentNullException>(); });
        cfg.ReceiveEndpoint("booking-processing-queue", e => e.ConfigureConsumer<BookingConsumer>(context));
        cfg.ReceiveEndpoint("booking-notifications-queue", e => e.ConfigureConsumer<BookingNotificationConsumer>(context));
        cfg.ConfigureEndpoints(context);
    });
});

// 8. HEALTH CHECKS
builder.Services.AddHealthChecks()
    .AddNpgSql(builder.Configuration.GetConnectionString("DefaultConnection")!, name: "postgresql", tags: new[] { "ready" })
    .AddRedis(builder.Configuration.GetConnectionString("Redis")!, name: "redis", tags: new[] { "ready" })
    .AddUrlGroup(new Uri("http://rabbitmq:15672"), name: "rabbitmq", tags: new[] { "ready" })
    .AddUrlGroup(new Uri(builder.Configuration["Keycloak:Authority"]!), name: "keycloak", tags: new[] { "ready" });

var app = builder.Build();

// 9. AUTOMATIC MIGRATIONS
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<EventReservationDbcontext>();
    context.Database.Migrate();
}

QuestPDF.Settings.License = LicenseType.Community;
app.UseMiddleware<CustomExceptionHandlerMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options => { options.OAuthClientId(keycloakClientId); options.OAuthUsePkce(); });
}

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}
app.UseCors("AllowFrontend");
app.UseOutputCache();
app.MapGet("/api/ping", () => "Pong").AllowAnonymous();
app.UseAuthentication();
app.UseAuthorization();

app.MapHub<BookingHub>("/bookingHub");
app.MapControllers();

// 10. HEALTH ENDPOINTS
app.MapHealthChecks("/health/live", new HealthCheckOptions { Predicate = _ => false, ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse });
app.MapHealthChecks("/health/ready", new HealthCheckOptions { Predicate = check => check.Tags.Contains("ready"), ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse });
app.MapHealthChecks("/health", new HealthCheckOptions { Predicate = _ => true, ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse });

app.Run();