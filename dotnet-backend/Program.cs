using Microsoft.EntityFrameworkCore; // For DbContext
using workout_progress.Models; // Adjust according to your namespace where ApplicationDbContext is defined
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

DotNetEnv.Env.Load();

// Add CORS policy
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSpecificOrigin", builder =>
    {
        builder.WithOrigins("http://localhost:4200", "http://localhost:5166")
               .AllowAnyMethod()
               .AllowAnyHeader()
               .AllowCredentials();
    });
});

builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(60); // Set session timeout to 1 hour
    options.Cookie.HttpOnly = true; // Session cookie is HTTP-only
    options.Cookie.IsEssential = true; // Required for GDPR compliance
});

// Configure JWT Bearer Authentication
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        // ValidateIssuer = true,
        // ValidateAudience = true,
        // ValidateLifetime = true,
        // ValidateIssuerSigningKey = true,
        // ValidIssuer = "https://accounts.google.com",
        // ValidAudience = Environment.GetEnvironmentVariable("CLIENT_ID"),
        //IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Environment.GetEnvironmentVariable("CLIENT_SECRET")))
    };
});

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

app.UseCors("AllowSpecificOrigin"); // Apply the CORS policy before other middleware
// app.UseHttpsRedirection();
// app.UseAuthentication(); // Ensure you add this to enable authentication
// app.UseAuthorization(); // Enable authorization checks
app.MapControllers(); // In Program.cs
app.UseSession();


// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}


app.UseHttpsRedirection();

app.Run();