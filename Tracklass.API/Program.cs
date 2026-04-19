using Tracklass.API;
using Tracklass.API.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

// Fix for PostgreSQL DateTime "Kind=Unspecified" error when migrating from SQL Server
AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<TracklassDbContext>(op =>
{
    op.UseNpgsql(connectionString);
});

// Auth service
builder.Services.AddScoped<AuthService>();

// JWT Authentication
var jwtKey = builder.Configuration["Jwt:Key"] ?? "TraCkL4ss_S3cR3t_K3y_2026_D3v!@#$%^";
var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "Tracklass.API";
var jwtAudience = builder.Configuration["Jwt:Audience"] ?? "Tracklass.Frontend";

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
        ValidIssuer = jwtIssuer,
        ValidAudience = jwtAudience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
    };
});

// Enable CORS
builder.Services.AddCors(op =>
{
    op.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyHeader()
              .AllowAnyMethod()
              .AllowAnyOrigin();
    });
});


builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Auto-apply migrations on startup (works on Render and any hosted environment)
try
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<TracklassDbContext>();
    db.Database.Migrate();
    Console.WriteLine("✅ Database migration applied successfully.");
}
catch (Exception ex)
{
    Console.WriteLine($"⚠️ Migration failed (app will still start): {ex.Message}");
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowAll");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
