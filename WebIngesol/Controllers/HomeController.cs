using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Security.Claims;
using WebIngesol.ConstantsRoute;
using WebIngesol.Models;
using WebIngesol.Repository.IRepository;

namespace WebIngesol.Controllers;

public class HomeController : Controller
{
    private readonly IAccountRepositorio _accRepo;

    public HomeController(IAccountRepositorio accRepo)
    {
        _accRepo = accRepo;
    }

    public IActionResult Index(string companyId)
    {
        const string COMPANY_INGESOL_1 = "EC2DBFC2-0CF1-4FDE-9613-8EC9A3BBEEA6";
        const string COMPANY_INGESOL_2 = "934AEB13-4273-43C2-90C7-08DE594503A4";

        if (!string.IsNullOrWhiteSpace(companyId))
        {
            HttpContext.Session.SetString("CompanyId", companyId);
        }

        if (companyId != null &&
            (companyId.Equals(COMPANY_INGESOL_1, StringComparison.OrdinalIgnoreCase) ||
             companyId.Equals(COMPANY_INGESOL_2, StringComparison.OrdinalIgnoreCase)))
        {
            return View("Index");
        }

        return View("Index");
    }

    // =========================
    // AUTH
    // =========================

    [HttpGet]
    public IActionResult Login()
    {
        return View(new LoginRequest());
    }

    [HttpPost]
    public async Task<IActionResult> Login(LoginRequest model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var respuesta = await _accRepo.LoginAsync(CT.User + "/login", model);

        if (respuesta == null || string.IsNullOrWhiteSpace(respuesta.Token))
        {
            TempData["error"] = respuesta?.Error ?? "Ocurrió un error inesperado.";
            return View(model);
        }

        if (respuesta.CompanyId == null)
        {
            TempData["error"] = "El usuario no tiene una empresa asignada.";
            return View(model);
        }

        // =========================
        // SESIÓN
        // =========================
        HttpContext.Session.SetString("Nombre", respuesta.FirstName ?? string.Empty);
        HttpContext.Session.SetString("Usuario", respuesta.UserName ?? string.Empty);
        HttpContext.Session.SetString("User", respuesta.UserName ?? string.Empty);
        HttpContext.Session.SetString("ImagePath", respuesta.ImagePath ?? string.Empty);
        HttpContext.Session.SetString("JWToken", respuesta.Token);
        HttpContext.Session.SetString("CompanyId", respuesta.CompanyId.ToString());

        if (respuesta.Roles != null && respuesta.Roles.Any())
        {
            HttpContext.Session.SetString("Role", respuesta.Roles.First());
        }

        // =========================
        // CLAIMS
        // =========================
        var claims = new List<Claim>
    {
        new Claim(ClaimTypes.NameIdentifier, respuesta.Id),
        new Claim(ClaimTypes.Name, respuesta.UserName ?? model.NombreUsuario),
        new Claim("JWT", respuesta.Token),
        new Claim("CompanyId", respuesta.CompanyId.ToString())
    };

        if (respuesta.Roles != null)
        {
            foreach (var rol in respuesta.Roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, rol));
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

        // =========================
        // REDIRECCIÓN (GUID REAL)
        // =========================
        return RedirectToAction(
            "Index",
            "Home",
            new { companyId = respuesta.CompanyId.ToString() }
        );
    }

    [HttpGet]
    public IActionResult Register()
    {
        return View(new RegisterDto());
    }

    [HttpPost]
    public async Task<IActionResult> Register(RegisterDto model)
    {
        if (!ModelState.IsValid)
            return View("Register", model);

        var respuesta = await _accRepo.RegisterAsync(CT.User, model);

        if (!respuesta)
        {
            ModelState.AddModelError("", "No se pudo completar el registro.");
            return View("Register", model);
        }

        var loginRequest = new LoginRequest
        {
            NombreUsuario = model.UserName,
            Password = model.Password
        };

        return await Login(loginRequest);
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
    // PERFIL
    // =========================
    [Authorize(Roles = "Admin")]
    [HttpGet]
    public async Task<IActionResult> EditarPerfil()
    {
        var userName = HttpContext.Session.GetString("Usuario");
        var token = HttpContext.Session.GetString("JWToken");

        if (string.IsNullOrEmpty(userName) || string.IsNullOrEmpty(token))
        {
            return RedirectToAction("Login");
        }

        var usuario = await _accRepo.ObtenerUsuarioPorUserName(
            CT.User + "/ByUserName",
            userName,
            token
        );

        if (usuario == null)
        {
            return NotFound("Usuario no encontrado.");
        }

        var backendOrigin = CT.Base;
        var imagePath = usuario.ImagePath ?? string.Empty;

        var profileImageUrl = string.IsNullOrEmpty(imagePath)
            ? Url.Content("~/images/SinFoto.png")
            : $"{backendOrigin}{(imagePath.StartsWith('/') ? "" : "/")}{imagePath}";

        var model = new EditarPerfilDto
        {
            UserName = usuario.UserName,
            FirstName = usuario.FirstName,
            LastName = usuario.LastName,
            Email = usuario.Email,
            ImagePath = profileImageUrl
        };

        return View(model);
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
    public IActionResult AccessDenied(string returnUrl)
    {
        ViewData["ReturnUrl"] = returnUrl;
        return View();
    }
}
