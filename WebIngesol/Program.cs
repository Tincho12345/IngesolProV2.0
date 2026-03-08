using System.Globalization;
using WebIngesol;
using WebIngesol.Hubs;

var builder = WebApplication.CreateBuilder(args);

// 🌍 FIX DECIMALES Azure → Azure
var cultura = new CultureInfo("es-AR");
CultureInfo.DefaultThreadCurrentCulture = cultura;
CultureInfo.DefaultThreadCurrentUICulture = cultura;

// 🛠️ Configuración de servicios
ServiceConfiguration.ConfigureServices(builder.Services);
builder.Services.AddSignalR();
builder.Services.AddSession(); // si no está

// 🔴 CORS: permitir SignalR desde este origen (ajustá según tu URL)
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins("https://webingesolversion2-afdfbkcjd6f6cccj.canadacentral-01.azurewebsites.net/")  // ✅ aquí usás la URL correcta según ENVIRONMENT
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
}); 

var app = builder.Build();

// 🌐 Middleware de entorno
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

// 🔒 HTTPS y archivos estáticos
app.UseHttpsRedirection();
app.UseStaticFiles();

// 🔴 Usar CORS antes de routing
app.UseCors();

// 🧠 Habilitamos la sesión (antes de Auth)
app.UseSession();

// 🔁 Enrutamiento
app.UseRouting();

// 🟢 AUTENTICACIÓN y AUTORIZACIÓN
app.UseAuthentication();
app.UseAuthorization();

// 🗂️ Archivos estáticos personalizados
app.MapStaticAssets();

// 🧭 Ruta por defecto del MVC
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

// 🔴 SignalR Hub
app.MapHub<ClientsHub>("/clientsHub");

// ▶️ Ejecutamos la app
app.Run();