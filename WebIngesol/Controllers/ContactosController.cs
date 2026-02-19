using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using WebIngesol.ConstantsRoute;
using WebIngesol.Controllers.Base;
using WebIngesol.Models;
using WebIngesol.Repository.IRepository;

namespace WebIngesol.Controllers;
[Authorize(Roles = "🛡️ Admin")]
public class ContactosController(IRepository<Contacto> repository, IMapper mapper) 
    : BaseController<Contacto, ContactoDto>(repository, mapper, CT.Contactos);
