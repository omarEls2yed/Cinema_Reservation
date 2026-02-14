using Cinema_Reservation.Factories;
using Cinema_Reservation.MiddleWares;
using DomainLayer.Contracts;
using DomainLayer.Models;
using MassTransit;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Persistence.Data;
using Persistence.identity;
using Persistence.Repositories;
using QuestPDF.Infrastructure;
using Service;
using Service.Mapping_Profiles;
using ServiceAbstraction;
using Shared.Hubs;
using StackExchange.Redis;
using System.Text;
using System.Text.Json.Serialization;

{
    var builder = WebApplication.CreateBuilder(args);

    builder.Services.AddDbContext<EventReservationDbcontext>(options =>
        options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
    
    builder.Services.AddDbContext<EventIdentityDbContext>(options =>
        options.UseSqlServer(builder.Configuration.GetConnectionString("AuthConnection")));

    builder.Services.AddSingleton<IConnectionMultiplexer>(c =>
    {
        var configuration = ConfigurationOptions.Parse(builder.Configuration.GetConnectionString("Redis"), true);
        return ConnectionMultiplexer.Connect(configuration);
    });


    builder.Services.AddStackExchangeRedisCache(options =>
    {
        options.Configuration = builder.Configuration.GetConnectionString("Redis");
        options.InstanceName = "Events_";
    });

    builder.Services.AddOutputCache(options =>
    {
        options.AddBasePolicy(builder => builder.Expire(TimeSpan.FromSeconds(10)));
    });

    builder.Services.AddStackExchangeRedisOutputCache(options =>
    {
        options.Configuration = builder.Configuration.GetConnectionString("Redis");
        options.InstanceName = "OutputEvents_";
    });

    builder.Services.AddScoped<IUnitOfWorkRepository, UnitOfWorkRepository>();
    builder.Services.AddScoped<ISeatService, SeatService>();
    builder.Services.AddScoped<ISeatReservationService, SeatReservationService>();
    builder.Services.AddScoped<IEventService, EventService>();
    builder.Services.AddTransient<IEmailService, EmailService>();
    builder.Services.AddScoped<IDataSeeding, DataSeeding>();

    builder.Services.AddScoped<Func<ISeatReservationService>>(provider => () => provider.GetRequiredService<ISeatReservationService>());
    builder.Services.AddScoped<Func<IPaymentService>>(provider => () => provider.GetRequiredService<IPaymentService>());
    builder.Services.AddScoped<Func<IEventService>>(provider => () => provider.GetRequiredService<IEventService>());
    builder.Services.AddScoped<Func<ISeatService>>(provider => () => provider.GetRequiredService<ISeatService>());
    
    builder.Services.AddScoped<IServiceManager, ServiceManager>();
    builder.Services.AddScoped<TicketPdfService>();

    builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
        {
            options.Password.RequireDigit = true;
            options.Password.RequiredLength = 8;
            options.SignIn.RequireConfirmedEmail = true;
        })
        .AddEntityFrameworkStores<EventIdentityDbContext>()
        .AddDefaultTokenProviders();

    builder.Services.AddHttpClient();
    builder.Services.AddScoped<IPaymentService, PaymentService>();

    builder.Services.AddAutoMapper(x => x.AddProfile(new SeatProfile()));
    builder.Services.AddAutoMapper(x => x.AddProfile(new EventProfile()));


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

    var jwtKey = builder.Configuration["Jwt:Key"] ?? throw new InvalidOperationException("JWT Key is missing!");
    builder.Services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = builder.Configuration["Jwt:Issuer"],
                ValidAudience = builder.Configuration["Jwt:Issuer"],
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!))
            };
        });

    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen(c =>
    {
        c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
        {
            Name = "Authorization",
            Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
            Scheme = "Bearer",
            BearerFormat = "JWT",
            In = Microsoft.OpenApi.Models.ParameterLocation.Header,
            Description = "Enter your valid token in the text input below.\r\n\r\nExample: \"eyJhbGciOiJIUzI1NiIsInR5cCI...\""
        });
        c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
        {
            {
                new Microsoft.OpenApi.Models.OpenApiSecurityScheme
                {
                    Reference = new Microsoft.OpenApi.Models.OpenApiReference
                    {
                        Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                        Id = "Bearer"
                    }
                },
                new string[] {}
            }
        });
    });
    var app = builder.Build();

    QuestPDF.Settings.License = LicenseType.Community;

    app.UseMiddleware<CustomExceptionHandlerMiddleware>();

    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    app.UseHttpsRedirection();

    using (var scope = app.Services.CreateScope())
    {
        var services = scope.ServiceProvider;
        try
        {
            var dataSeeder = services.GetRequiredService<IDataSeeding>();
            await dataSeeder.IdentityDataSeedAsync();
        }
        catch (Exception ex)
        {
            var logger = services.GetRequiredService<ILogger<Program>>();
            logger.LogError(ex, "An error occurred while seeding the database.");
        }
    }

    app.UseCors("AllowFrontend");
    app.UseOutputCache();
    app.UseAuthentication();
    app.UseAuthorization();
    app.MapHub<BookingHub>("/bookingHub");
    app.MapControllers();
    app.Run();
}