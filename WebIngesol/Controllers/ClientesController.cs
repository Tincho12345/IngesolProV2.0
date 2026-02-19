using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using WebIngesol.ConstantsRoute;
using WebIngesol.Controllers.Base;
using WebIngesol.Models;
using WebIngesol.Repository.IRepository;

namespace WebIngesol.Controllers;
[Authorize(Roles = "🛡️ Admin")]
public class ClientesController(IRepository<Cliente> repository,
                                IMapper mapper) : BaseController<Cliente, ClienteDto>(repository, mapper, CT.Clientes)
{
}