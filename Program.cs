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

// 🔹 SESSION SERVICES
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromHours(1);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

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

// 🔹 MIGRATIONS + SEED DATA
using (var scope = app.Services.CreateScope())
{
    try
    {
        var db = scope.ServiceProvider.GetRequiredService<Dbcontext>();

        Console.WriteLine("DB PATH: " + db.Database.GetDbConnection().DataSource);

        db.Database.Migrate();

        // ============================
        // 🔹 SEED DEMO DATA (SAFE)
        // ============================

        if (!db.Provider.Any())
        {
            db.Provider.AddRange(
                new Providersignup
                {
                    Name = "Mike the Plumber",
                    Email = "plumber@test.com",
                    Password = "test123",
                    professionName = "Plumber",
                    City = "Chicago",
                    State = "IL",
                    Zipcode = "60616",
                    Description = "Licensed plumber with 10+ years experience"
                },
                new Providersignup
                {
                    Name = "Bug Busters",
                    Email = "exterminator@test.com",
                    Password = "test123",
                    professionName = "Exterminator",
                    City = "Chicago",
                    State = "IL",
                    Zipcode = "60610",
                    Description = "Residential and commercial pest control"
                }
            );
        }

        if (!db.Customer.Any())
        {
            db.Customer.Add(
                new Customersignup
                {
                    Name = "Jane Doe",
                    Email = "customer@test.com",
                    Password = "test123",
                    Whatservice = "Plumber",
                    City = "Chicago",
                    State = "IL",
                    Zipcode = "60616",
                    Description = "Kitchen sink leaking badly"
                }
            );
        }

        db.SaveChanges();

        // ============================
        // 🔹 DIAGNOSTIC LOG
        // ============================

        var tables = db.Database
            .SqlQueryRaw<string>("SELECT name FROM sqlite_master WHERE type='table'")
            .ToList();

        Console.WriteLine("TABLES IN DB:");
        foreach (var table in tables)
        {
            Console.WriteLine(" - " + table);
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine("Migration or seeding failed:");
        Console.WriteLine(ex);
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

// 🔹 SESSION MIDDLEWARE
app.UseSession();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
