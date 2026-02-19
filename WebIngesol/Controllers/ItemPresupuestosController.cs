using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using WebIngesol.ConstantsRoute;
using WebIngesol.Controllers.Base;
using WebIngesol.Models;
using WebIngesol.Repository.IRepository;

namespace WebIngesol.Controllers;
[Authorize(Roles = "🛡️ Admin, ✏️ Data Entry")]
public class ItemPresupuestosController(IRepository<ItemPresupuestoReadDto> repository, IMapper mapper) 
    : BaseController<ItemPresupuestoReadDto, ItemPresupuestoDto>(repository, mapper, CT.ItemPresupuestos);
