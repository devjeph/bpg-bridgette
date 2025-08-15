using Bridgette.Core.Services;
using Bridgette.Data;
using Bridgette.Google.Clients;
using Bridgette.Google.Dtos;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// 1. Add services to the container.

// Configure and register the DbContext
builder.Services.AddDbContext<BridgetteDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("BridgetteDb")));

// Configure Google API settings
builder.Services.Configure<GoogleApiSettings>(builder.Configuration.GetSection("GoogleApiSettings"));

// Register custom services
// Scoped: A new instance is created for each web request.
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<ITimesheetCheckService, TimesheetCheckService>();

// Singleton: A single instance is created for the lifetime of the application.
builder.Services.AddSingleton<IGoogleChatClient, GoogleChatClient>();
builder.Services.AddSingleton<IGoogleSheetsClients, GoogleSheetsClient>();

builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
