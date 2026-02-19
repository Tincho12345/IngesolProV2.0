using ApiIngesol.Data;
using ApiIngesol.Models;
using ApiIngesol.Models.Users;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace ApiIngesol
{
    public static class SeedData
    {
        public static async Task SeedRolesAsync(IServiceProvider serviceProvider)
        {
            var roleManager = serviceProvider.GetRequiredService<RoleManager<ApplicationRole>>();
            var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var context = serviceProvider.GetRequiredService<AppDbContext>();

            // =========================
            // 1️⃣ ROLES
            // =========================
            var roleNames = new[] { "🛡️ Admin", "👁️ Users" };

            foreach (var roleName in roleNames)
            {
                if (!await roleManager.RoleExistsAsync(roleName))
                {
                    var role = new ApplicationRole
                    {
                        Name = roleName,
                        IsActive = true
                    };

                    await roleManager.CreateAsync(role);
                }
            }

            // =========================
            // 2️⃣ EMPRESA POR DEFECTO
            // =========================
            var company = await context.Companies.FirstOrDefaultAsync();

            if (company == null)
            {
                company = new Company
                {
                    Nombre = "Empresa Principal",
                    IsActive = true,
                    LogoUrl = null
                };

                context.Companies.Add(company);
                await context.SaveChangesAsync();
            }

            // =========================
            // 3️⃣ USUARIO ADMIN
            // =========================
            var adminEmail = "admin@admin.com";
            var adminUser = await userManager.FindByEmailAsync(adminEmail);

            if (adminUser == null)
            {
                var newAdminUser = new ApplicationUser
                {
                    UserName = "Admin",
                    Email = adminEmail,
                    EmailConfirmed = true,

                    FirstName = "Administrador",
                    LastName = "Sistema",

                    ImagePath = "/Images/SinFoto.png",
                    LocalImagePath = "\\Images\\SinFoto.png",

                    CompanyId = company.Id
                };

                var result = await userManager.CreateAsync(newAdminUser, "MiContraseña1234");

                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(newAdminUser, "🛡️ Admin");
                }
                else
                {
                    foreach (var error in result.Errors)
                    {
                        Console.WriteLine($"Error al crear el usuario admin: {error.Description}");
                    }
                }
            }
        }
    }
}
