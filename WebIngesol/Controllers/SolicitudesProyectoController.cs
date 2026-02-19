using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebIngesol.ConstantsRoute;
using WebIngesol.Controllers.Base;
using WebIngesol.Models;
using WebIngesol.Repository.IRepository;

namespace WebIngesol.Controllers;

[AllowAnonymous]
public class SolicitudesProyectoController(
    IRepository<SolicitudProyectoReadDto> repository,
    IMapper mapper
) : BaseController<SolicitudProyectoReadDto, SolicitudProyectoDto>(
        repository,
        mapper,
        CT.SolicitudesProyecto
    )
{
    [HttpPost]
    [ValidateAntiForgeryToken]
    public override async Task<IActionResult> Create([FromForm] SolicitudProyectoDto dto)
    {
        // 🔐 Valores forzados por backend
        dto.EtapaProyecto = "Consulta inicial";
        dto.TipoProyecto = "Profesional";
        dto.Estado = EstadoSolicitudProyecto.Nuevo;

        return await base.Create(dto);
    }
}
