using InventraBackend.Services;

var builder = WebApplication.CreateBuilder(args);

// --- CORS ---
builder.Services.AddCors(options =>
{
    options.AddPolicy("DefaultCors", policy =>
        policy.WithOrigins(
            "http://localhost:5500",
            "http://127.0.0.1:5500",
            "http://0.0.0.0:5500"
        )
        .AllowAnyHeader()
        .AllowAnyMethod()
        .AllowCredentials()
    );
});

// --- SESSION ---
builder.Services.AddDistributedMemoryCache();

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromHours(2);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;

    // IMPORTANT: allow cookies over HTTP for local testing
    options.Cookie.SameSite = SameSiteMode.None;
    options.Cookie.SecurePolicy = CookieSecurePolicy.None; 
});

// Services
builder.Services.AddSingleton<InventoryService>();
builder.Services.AddScoped<EmailService>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// --- PIPELINE ---

//app.UseHttpsRedirection();

app.UseRouting();       // MUST BE BEFORE CORS + SESSION

app.UseCors("DefaultCors");

app.UseSession();

app.UseAuthorization();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapControllers();

app.Run();