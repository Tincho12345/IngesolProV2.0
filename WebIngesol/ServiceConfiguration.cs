using Microsoft.AspNetCore.Authentication.Cookies;
using WebIngesol.Repository.IRepository;
using WebIngesol.Repository;
using System.Text.Json;

namespace WebIngesol;

public static class ServiceConfiguration
{
    public static void ConfigureServices(IServiceCollection services)
    {
        services.AddControllersWithViews()
            .AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
            });

        services.AddHttpClient();

        // Repositorios
        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
        services.AddScoped<IAccountRepositorio, AccountRepositorio>();
        services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

        // 🧠 ACCESO A HttpContext
        services.AddHttpContextAccessor();

        // 🟢 AUTENTICACIÓN por COOKIES
        services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
            .AddCookie(options =>
            {
                options.Cookie.HttpOnly = true;
                options.ExpireTimeSpan = TimeSpan.FromMinutes(60);
                options.LoginPath = "/Home/Login";
                options.AccessDeniedPath = "/Home/AccessDenied";
                options.SlidingExpiration = true;
            });

        // 🟢 CONFIGURACIÓN DE SESIÓN
        services.AddSession(options =>
        {
            options.IdleTimeout = TimeSpan.FromMinutes(30);
            options.Cookie.HttpOnly = true;
            options.Cookie.IsEssential = true;
        });

        services.AddAutoMapper(typeof(Program));
    }
}
