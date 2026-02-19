using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebIngesol.ConstantsRoute;
using WebIngesol.Controllers.Base;
using WebIngesol.Models;
using WebIngesol.Repository.IRepository;

namespace WebIngesol.Controllers
{
    [Authorize(Roles = "🛡️ Admin, 👤 Empleados, ✏️ Data Entry, 👁️ Users")]
    public class RegistrosAsistenciasController : BaseController<RegistroAsistencia, RegistroAsistenciaCreateDto>
    {
        private readonly IRepository<Presupuesto> _presupuestoRepository;

        public RegistrosAsistenciasController(
            IRepository<RegistroAsistencia> registroRepository,
            IRepository<Presupuesto> presupuestoRepository,
            IMapper mapper
        ) : base(registroRepository, mapper, CT.RegistrosAsistencias)
        {
            _presupuestoRepository = presupuestoRepository;
        }

        // ----------------------------------------------------------------------
        // 📌 Presupuestos (similar a ProvinciasController)
        // ----------------------------------------------------------------------
        [HttpGet]
        public async Task<IActionResult> ObtenerPresupuesto()
        {
            var presupuestos = await _presupuestoRepository.GetAllAsync(CT.Presupuestos);

            var result = presupuestos
                .OrderBy(p => p.NumeroOrden)
                .Select(p => new
                {
                    id = p.Id,
                    nombre = $"Orden {p.NumeroOrden} - {p.Descripcion}"
                });

            return Json(result);
        }
    }
}
