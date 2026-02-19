using System.Globalization;
using WebIngesol;

var builder = WebApplication.CreateBuilder(args);

// 🌍 FIX DECIMALES Azure → Azure
// Fuerza la cultura a es-AR para que ASP.NET no convierta 23.50 en 2350
var cultura = new CultureInfo("es-AR");
CultureInfo.DefaultThreadCurrentCulture = cultura;
CultureInfo.DefaultThreadCurrentUICulture = cultura;

// 🛠️ Configuración de servicios
ServiceConfiguration.ConfigureServices(builder.Services);

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

// ▶️ Ejecutamos la app
app.Run();
