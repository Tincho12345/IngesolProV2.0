using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Security.Claims;
using System.Text.Json;
using WebIngesol.ConstantsRoute;
using WebIngesol.Models;
using WebIngesol.Repository.IRepository;

namespace WebIngesol.Controllers;

public class HomeController(
    IAccountRepositorio accRepo,
    IWebHostEnvironment env) : Controller
{
    private readonly IAccountRepositorio _accRepo = accRepo;
    private readonly IWebHostEnvironment _env = env;

    // =========================
    // INDEX
    // =========================
    public IActionResult Index(string? companyId)
    {
        if (!string.IsNullOrWhiteSpace(companyId))
        {
            HttpContext.Session.SetString("CompanyId", companyId);
        }

        var clientes = CargarClientes();

        return View("Index", clientes);
    }

    // =========================
    // MÉTODO AUXILIAR: CARGAR CLIENTES
    // =========================
    private List<VisibleClient> CargarClientes()
    {
        var folderPath = Path.Combine(_env.WebRootPath, "clients");
        var clientes = new List<VisibleClient>();

        if (!Directory.Exists(folderPath))
            return clientes;

        var jsonFiles = Directory.GetFiles(folderPath, "*.json");

        foreach (var jsonFile in jsonFiles)
        {
            try
            {
                var json = System.IO.File.ReadAllText(jsonFile);
                var cliente = JsonSerializer.Deserialize<VisibleClient>(json);

                if (cliente is not null && !string.IsNullOrWhiteSpace(cliente.Imagen))
                {
                    // Ajustar ruta pública
                    cliente.Imagen = "/clients/" + cliente.Imagen;
                    clientes.Add(cliente);
                }
            }
            catch
            {
                continue; // Ignorar JSON corruptos
            }
        }

        return clientes;
    }

    // =========================
    // AUTH (tu código original intacto)
    // =========================

    [HttpGet]
    public IActionResult Login()
    {
        ModelState.Clear();
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Login(LoginRequest model)
    {
        if (!ModelState.IsValid)
        {
            TempData["error"] = "Datos inválidos.";
            return RedirectToAction("Login");
        }

        var respuesta = await _accRepo.LoginAsync(CT.User + "/login", model);

        if (respuesta == null || string.IsNullOrWhiteSpace(respuesta.Token))
        {
            TempData["error"] = respuesta?.Error ?? "Usuario o contraseña incorrectos.";
            return RedirectToAction("Login");
        }

        if (respuesta.CompanyId == null)
        {
            TempData["error"] = "El usuario no tiene una empresa asignada.";
            return RedirectToAction("Login");
        }

        HttpContext.Session.SetString("Nombre", respuesta.FirstName ?? string.Empty);
        HttpContext.Session.SetString("Usuario", respuesta.UserName ?? string.Empty);
        HttpContext.Session.SetString("User", respuesta.UserName ?? string.Empty);
        HttpContext.Session.SetString("ImagePath", respuesta.ImagePath ?? string.Empty);
        HttpContext.Session.SetString("JWToken", respuesta.Token ?? string.Empty);
        HttpContext.Session.SetString("CompanyId", respuesta.CompanyId?.ToString() ?? string.Empty);

        if (respuesta.Roles?.Count > 0)
        {
            HttpContext.Session.SetString("Role", respuesta.Roles[0]);
        }

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, respuesta.Id ?? string.Empty),
            new(ClaimTypes.Name, respuesta.UserName ?? model.NombreUsuario ?? string.Empty),
            new("JWT", respuesta.Token ?? string.Empty),
            new("CompanyId", respuesta.CompanyId?.ToString() ?? string.Empty)
        };

        if (respuesta.Roles?.Count > 0)
        {
            foreach (var rol in respuesta.Roles)
            {
                claims.Add(new(ClaimTypes.Role, rol));
            }
        }

        var identity = new ClaimsIdentity(
            claims,
            CookieAuthenticationDefaults.AuthenticationScheme
        );

        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(identity)
        );

        return RedirectToAction(
            "Index",
            "Home",
            new { companyId = respuesta.CompanyId.ToString() }
        );
    }

    [HttpGet]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync();
        HttpContext.Session.Clear();

        if (Request.Cookies.ContainsKey(".AspNetCore.Session"))
        {
            Response.Cookies.Delete(".AspNetCore.Session");
        }

        return RedirectToAction("Index");
    }

    // =========================
    // MISC
    // =========================

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel
        {
            RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier
        });
    }

    [HttpGet]
    public IActionResult ClientesPartial()
    {
        var clientes = CargarClientes();
        return PartialView("Sections/_ClientesPartial", clientes);
    }

    [HttpGet]
    public IActionResult AccessDenied(string returnUrl)
    {
        ViewData["ReturnUrl"] = returnUrl;
        return View();
    }
}