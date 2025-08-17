using Microsoft.EntityFrameworkCore;
using Microsoft.Win32;
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
//	options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Register your services here (DI)



// Add CORS policy for frontend (Angular/React/...)
builder.Services.AddCors(options =>
{
	options.AddPolicy("AllowFrontend",
		policy => policy
			.WithOrigins(frontendUrl!)  // using value from appsettings.json
			.AllowAnyHeader()
			.AllowAnyMethod());
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}


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