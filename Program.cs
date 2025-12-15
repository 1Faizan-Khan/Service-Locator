using ServiceLocator.Services;
using Microsoft.EntityFrameworkCore;
using ServiceLocator.Models;
using System;

var builder = WebApplication.CreateBuilder(args);

// Bind to Render port
string port = Environment.GetEnvironmentVariable("PORT") ?? "10000";
builder.WebHost.UseUrls($"http://0.0.0.0:{port}");

builder.Services.AddControllersWithViews();
builder.Services.AddScoped<GeoServices>();

// DB config
builder.Services.AddDbContext<Dbcontext>(options =>
{
    if (builder.Environment.IsDevelopment())
    {
        options.UseSqlite("Data Source=serviceLocator.db");
    }
    else
    {
        options.UseSqlite("Data Source=/app/serviceLocator.db");
    }
});

var app = builder.Build();

// Force migrations + DIAGNOSTIC LOG
using (var scope = app.Services.CreateScope())
{
    try
    {
        var db = scope.ServiceProvider.GetRequiredService<Dbcontext>();

        // 🔍 DIAGNOSTIC — DO NOT REMOVE YET
        Console.WriteLine("DB PATH: " + db.Database.GetDbConnection().DataSource);

        db.Database.Migrate();
    }
    catch (Exception ex)
    {
        Console.WriteLine("Migration failed:");
        Console.WriteLine(ex.Message);
        throw;
    }
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
