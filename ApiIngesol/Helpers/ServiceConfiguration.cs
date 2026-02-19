using ApiIngesol.Data;
using ApiIngesol.Mappers;
using ApiIngesol.Models;
using ApiIngesol.Models.Users;
using ApiIngesol.Repository;
using ApiIngesol.Repository.IRepository;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using OfficeOpenXml;
using System.Security.Claims;
using System.Text;

namespace ApiIngesol.Helpers
{
    public static class ServiceConfiguration
    {
        // Recibe la connection string como parámetro
        public static void ConfigureServices(WebApplicationBuilder builder, string connectionString)
        {
            // EPPlus License (uso no comercial)
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            // Configuración DB Context con SQL Server usando la connection string recibida
            builder.Services.AddDbContext<AppDbContext>(options =>
                options.UseSqlServer(connectionString));

            // Configuración de validación de modelos
            builder.Services.Configure<ApiBehaviorOptions>(options =>
            {
                options.InvalidModelStateResponseFactory = context =>
                {
                    var errores = context.ModelState
                        .Where(e => e.Value!.Errors.Any())
                        .Select(e => new
                        {
                            Campo = e.Key,
                            Errores = e.Value!.Errors.Select(x => x.ErrorMessage).ToList()
                        });

                    return new BadRequestObjectResult(errores);
                };
            });

            // Configuración Identity
            builder.Services.AddIdentity<ApplicationUser, ApplicationRole>(options =>
            {
                options.Password.RequireDigit = true;
                options.Password.RequiredLength = 6;
                options.Password.RequireUppercase = false;
                options.Password.RequireLowercase = false;
                options.Password.RequireNonAlphanumeric = false;
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
                options.Lockout.MaxFailedAccessAttempts = 5;
            })
            .AddEntityFrameworkStores<AppDbContext>()
            .AddDefaultTokenProviders();

            // Serialización JSON
            builder.Services.AddControllers()
                .AddJsonOptions(options =>
                {
                    options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
                    options.JsonSerializerOptions.DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
                });

            // JWT
            builder.Services.Configure<ApiSettings>(builder.Configuration.GetSection("ApiSettings"));
            var apiSettings = builder.Configuration.GetSection("ApiSettings").Get<ApiSettings>()!;

            builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.RequireHttpsMetadata = false;
                options.SaveToken = true;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(apiSettings.Secret)),
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    RoleClaimType = ClaimTypes.Role,
                    NameClaimType = ClaimTypes.Name
                };
            });

            // AutoMapper
            builder.Services.AddAutoMapper(typeof(IngeSolMapper));

            // IHttpContextAccessor
            builder.Services.AddHttpContextAccessor();

            // Repositorios y servicios
            builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
            builder.Services.AddScoped(typeof(IService<>), typeof(Service<>));
            builder.Services.AddScoped<IMaterialService, MaterialService>();
            builder.Services.AddScoped<IUserService, UserService>();

            // Swagger
            builder.Services.ConfigureSwagger();

            // Endpoints
            builder.Services.AddEndpointsApiExplorer();

            // CORS
            builder.Services.AddCors(o => o.AddPolicy("PoliticaCors", p =>
                p.AllowAnyOrigin()
                 .AllowAnyHeader()
                 .AllowAnyMethod()));
        }
    }
}