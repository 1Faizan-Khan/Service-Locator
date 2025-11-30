using ServiceLocator.Services;
using Microsoft.EntityFrameworkCore;
using ServiceLocator.Models;
using Microsoft.Extensions.Options;
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

// Add EF Core DI
builder.Services.AddDbContext<Dbcontext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

var app = builder.Build();

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