using Bookify.Application.Business.Interfaces;
using Bookify.Application.Business.Interfaces.Admin;
using Bookify.Application.Business.Interfaces.Data;
using Bookify.Application.Business.Interfaces.Services;
using Bookify.Application.Business.Mappings;
using Bookify.Application.Business.Services;
using Bookify.Domain.Entities;
using Bookify.Infrastructure.Data;
using Bookify.Infrastructure.Data.Data.AdminServices;
using Bookify.Infrastructure.Data.Data.Context;
using Bookify.Infrastructure.Data.Data.Repositories;
using Bookify.Infrastructure.Data.Data.UnitOfWork;
using Bookify.Infrastructure.Data.Services;
using Bookify.Api.HealthChecks;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// Allow controllers to receive model state and return detailed validation errors
// explicitly instead of the automatic 400 produced by [ApiController]. This
// makes it easier to log and return enriched errors during local testing.
builder.Services.Configure<Microsoft.AspNetCore.Mvc.ApiBehaviorOptions>(options =>
{
    options.SuppressModelStateInvalidFilter = true;
});

// Swagger/OpenAPI configuration
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Bookify API", Version = "v1" });

    // Add JWT Bearer authentication to Swagger
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
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

// Database Configuration
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<BookifyDbContext>(options =>
    options.UseSqlServer(connectionString));

// Health Checks - Database connectivity check
builder.Services.AddHealthChecks()
    .AddCheck<Bookify.Api.HealthChecks.DatabaseHealthCheck>("database");

// ASP.NET Identity Configuration
builder.Services.AddIdentityCore<User>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = true;
    options.Password.RequiredLength = 6;
    options.Password.RequiredUniqueChars = 1;

    options.User.RequireUniqueEmail = true;
})
.AddRoles<IdentityRole>()
.AddEntityFrameworkStores<BookifyDbContext>()
.AddDefaultTokenProviders();

// JWT Authentication Configuration
var jwtSettings = builder.Configuration.GetSection("Jwt");
var key = Encoding.UTF8.GetBytes(jwtSettings["Key"]!);

// Program.cs - JWT Configuration
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
        };

        // For development - easier debugging
        options.Events = new JwtBearerEvents
        {
            OnAuthenticationFailed = context =>
            {
                Console.WriteLine($"Authentication failed: {context.Exception.Message}");
                return Task.CompletedTask;
            },
            OnTokenValidated = context =>
            {
                Console.WriteLine("Token validated successfully");
                return Task.CompletedTask;
            }
        };
    });

// Application Services
builder.Services.AddMediatR(cfg =>
    cfg.RegisterServicesFromAssembly(typeof(IAuthService).Assembly));

builder.Services.AddAutoMapper(cfg =>
{
    cfg.AddMaps(typeof(IAuthService).Assembly);
});
builder.Services.AddAutoMapper(cfg =>
{
    cfg.AddMaps(typeof(AdminMappingProfile).Assembly);
});

// In Program.cs, add this line:
builder.Services.AddScoped<IRoomService, RoomService>();

// Infrastructure Services - Repositories
builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
builder.Services.AddScoped<IRoomRepository, RoomRepository>();
builder.Services.AddScoped<IBookingRepository, BookingRepository>();
builder.Services.AddScoped<IRoomTypeRepository, RoomTypeRepository>();

//admin services
builder.Services.AddScoped<IAdminRoomService, AdminRoomService>();
builder.Services.AddScoped<IAdminRoomTypeService, AdminRoomTypeService>();
builder.Services.AddScoped<IAdminBookingService, AdminBookingService>();


// Infrastructure Services - Unit of Work
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// Infrastructure Services - Custom Services
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IBookingService, BookingService>();
builder.Services.AddScoped<IUserService, UserService>();

// Session State Configuration (for reservation cart)
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

builder.Services.AddHostedService<DatabaseSeeder>();

// Register DbContext interface
builder.Services.AddScoped<IBookfiyDbContext>(provider =>
    provider.GetRequiredService<BookifyDbContext>());

// Add CORS services
builder.Services.AddCors(options =>
{
    options.AddPolicy("Frontend", policy =>
    {
        policy.WithOrigins("http://localhost:4200")
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});

builder.Host.UseSerilog((context, config) => config.ReadFrom.Configuration(context.Configuration));

var stripeSecretFromConfig = builder.Configuration["Stripe:SecretKey"];
var stripePublishableFromConfig = builder.Configuration["Stripe:PublishableKey"];

// For local testing, you can provide a default from environment variables
var stripeSecret = !string.IsNullOrWhiteSpace(stripeSecretFromConfig)
    ? stripeSecretFromConfig
    : Environment.GetEnvironmentVariable("STRIPE_TEST_SECRET");

var stripePublishable = !string.IsNullOrWhiteSpace(stripePublishableFromConfig)
    ? stripePublishableFromConfig
    : Environment.GetEnvironmentVariable("STRIPE_TEST_PUBLISHABLE");

Stripe.StripeConfiguration.ApiKey = stripeSecret;
builder.Configuration["Stripe:PublishableKey"] = stripePublishable;


var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("Frontend");

app.UseStaticFiles();

app.UseHttpsRedirection();

// Session middleware (must be before UseRouting/UseEndpoints)
app.UseSession();
app.UseRouting();
// Authentication & Authorization middleware
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Health Check Endpoint
app.MapHealthChecks("/health");

app.Run();