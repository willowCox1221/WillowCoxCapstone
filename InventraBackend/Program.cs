using InventraBackend.Services;

var builder = WebApplication.CreateBuilder(args);

// --- CORS ---
var allowedFrontendOrigin = builder.Configuration["AllowedFrontendOrigin"] ?? "http://localhost:5500";

builder.Services.AddCors(options =>
{
    options.AddPolicy("DefaultCors",
        policy => policy
            .WithOrigins(allowedFrontendOrigin, "null")
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

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromHours(2);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.SameSite = SameSiteMode.None;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
});

var app = builder.Build();

// --- PIPELINE (ORDER MATTERS!!) ---

app.UseCors("DefaultCors");       // ✅ MUST BE FIRST
app.UseHttpsRedirection();
app.UseRouting();
app.UseSession();
app.UseAuthorization();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapControllers();

app.Run();