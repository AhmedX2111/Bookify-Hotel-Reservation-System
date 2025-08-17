using Microsoft.EntityFrameworkCore;
using Serilog;
using System;

var builder = WebApplication.CreateBuilder(args);

// Read frontend URL from appsettings
var frontendUrl = builder.Configuration["Frontend:Url"];

// Add services to the container.
builder.Services.AddOpenApi();
builder.Services.AddControllers();               // Add Controllers
builder.Services.AddEndpointsApiExplorer();      // Needed for Swagger

// Register DbContext with Connection String from appsettings.json
//builder.Services.AddDbContext<AppDbContext>(options =>
//    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Register your services here (DI)

// Configure Serilog (from dev branch)
builder.Host.UseSerilog((context, configuration) =>
	configuration.ReadFrom.Configuration(context.Configuration));

// Add CORS policy for frontend (Angular/React/...)
// using value from appsettings.json
builder.Services.AddCors(options =>
{
	options.AddPolicy("AllowFrontend",
		policy => policy
			.WithOrigins(frontendUrl!)
			.AllowAnyHeader()
			.AllowAnyMethod());
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
	app.MapOpenApi();
}

// Enable Serilog request logging
app.UseSerilogRequestLogging();

app.UseHttpsRedirection();

// Enable CORS
app.UseCors("AllowFrontend");

app.UseAuthorization();

// Map Controllers 
app.MapControllers();

using var scope = app.Services.CreateScope();
var services = scope.ServiceProvider;
//var context = services.GetRequiredService<AppDbContext>();
//context.Database.EnsureCreated();
//context.Database.Migrate();

app.Run();
