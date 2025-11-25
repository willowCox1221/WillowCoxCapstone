using InventraBackend.Services;

var builder = WebApplication.CreateBuilder(args);

// --- CORS ---
builder.Services.AddCors(options =>
{
    options.AddPolicy("DefaultCors",
        policy => policy
            .WithOrigins("http://localhost:5500", "http://127.0.0.1:5500", "null")
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials());
});

builder.Services.AddDistributedMemoryCache();

builder.Services.AddSingleton<InventoryService>();
builder.Services.AddScoped<EmailService>();

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