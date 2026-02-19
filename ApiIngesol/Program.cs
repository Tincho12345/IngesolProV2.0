using ApiIngesol.Helpers;
using static ApiIngesol.SeedData;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// 🔧 Cultura igual en TODOS los ambientes (local, Azure)
// Fuerza separador decimal = PUNTO
var culture = System.Globalization.CultureInfo.InvariantCulture;
System.Globalization.CultureInfo.DefaultThreadCurrentCulture = culture;
System.Globalization.CultureInfo.DefaultThreadCurrentUICulture = culture;

// 🔹 Obtener la connection string según el environment y validar que no sea nula
var connectionString = builder.Configuration.GetConnectionString("ConexionSql")
                       ?? throw new InvalidOperationException("La cadena de conexión 'ConexionSql' no está definida.");

// ✅ Configuración de servicios pasando la connection string segura
ServiceConfiguration.ConfigureServices(builder, connectionString);

var app = builder.Build();

// 🌐 Middleware y pipeline HTTP
app.UseStaticFiles();

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "ApiIngesol v1");
    c.RoutePrefix = "swagger";
});

// 🔁 Redirección a Swagger desde raíz
app.UseSwaggerRootRedirect();

app.UseHttpsRedirection();
app.UseCors("PoliticaCors");

// 🌱 Sembrar roles por única vez al inicio
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    await SeedRolesAsync(services);
}

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();
