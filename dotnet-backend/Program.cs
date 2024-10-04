using Microsoft.EntityFrameworkCore; // For DbContext
using workout_progress.Models; // Adjust according to your namespace where ApplicationDbContext is defined

var builder = WebApplication.CreateBuilder(args);

DotNetEnv.Env.Load();

builder.Services.AddControllers();
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Add services to the container.
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Logging.AddConsole();
builder.Services.AddHttpClient();
// var serverUrl = Environment.GetEnvironmentVariable("SERVER_URL");
// builder.WebHost.UseUrls(serverUrl);

var app = builder.Build();
app.MapControllers(); // In Program.cs


// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}


app.UseHttpsRedirection();

app.Run();