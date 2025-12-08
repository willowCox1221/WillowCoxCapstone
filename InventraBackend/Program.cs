using InventraBackend.Services;
using Microsoft.EntityFrameworkCore;
using InventraBackend.Data;

var builder = WebApplication.CreateBuilder(args);
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");



// --- CORS ---
builder.Services.AddCors(options =>
{
    options.AddPolicy("DefaultCors",
        policy => policy
            .WithOrigins(
                "http://localhost:5500",
                "http://127.0.0.1:5500",
                "null",
                "https://localhost:7264",   // ADD THIS
                "http://localhost:7264"     // ADD THIS
            )
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials());
});



builder.Services.AddDistributedMemoryCache();

    builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseMySql(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        ServerVersion.AutoDetect(builder.Configuration.GetConnectionString("DefaultConnection"))
    )
);

builder.Services.AddScoped<InventoryService>();
builder.Services.AddScoped<EmailService>();
builder.Services.AddSingleton<DatabaseService>();


builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// -------------------------------
// ⭐ FIXED SESSION CONFIG ⭐
// -------------------------------
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromHours(2);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;

    // ⭐ Correct settings for HTTP localhost
    options.Cookie.SameSite = SameSiteMode.Lax;
    options.Cookie.SecurePolicy = CookieSecurePolicy.None;
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// ---------------------------------------------
// ⭐ CORRECT PIPELINE ORDER ⭐
// ---------------------------------------------
app.UseHttpsRedirection();

app.UseRouting();

app.UseCors("DefaultCors");   // After routing

app.UseSession();             // Before Authorization

app.UseAuthorization();

app.MapControllers();

app.Run();