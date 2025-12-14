using ServiceLocator.Services;
using Microsoft.EntityFrameworkCore;
using ServiceLocator.Models;
using System;

var builder = WebApplication.CreateBuilder(args);

// ===============================
// BIND TO RENDER PORT
// ===============================
string port = Environment.GetEnvironmentVariable("PORT") ?? "10000";
builder.WebHost.UseUrls($"http://0.0.0.0:{port}");
// ===============================

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddScoped<GeoServices>();

// ===============================
// DATABASE CONFIG (LOCAL vs RENDER)
// ===============================
builder.Services.AddDbContext<Dbcontext>(options =>
{
    if (builder.Environment.IsDevelopment())
    {
        // Local machine
        options.UseSqlite("Data Source=serviceLocator.db");
    }
    else
    {
        // Render
        options.UseSqlite("Data Source=/app/serviceLocator.db");
    }
});
// ===============================

var app = builder.Build();

// ===============================
// APPLY MIGRATIONS SAFELY
// ===============================
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;

    try
    {
        var db = services.GetRequiredService<Dbcontext>();
        db.Database.Migrate();
    }
    catch (Exception ex)
    {
        Console.WriteLine("❌ Migration failed:");
        Console.WriteLine(ex.Message);
        throw;
    }
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();