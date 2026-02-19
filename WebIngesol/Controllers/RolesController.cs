using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using WebIngesol.ConstantsRoute;
using WebIngesol.Controllers.Base;
using WebIngesol.Models;
using WebIngesol.Repository.IRepository;

namespace WebIngesol.Controllers;
[Authorize(Roles = "🛡️ Admin")]
public class RolesController(IRepository<Rol> repository,
                            IMapper mapper) : BaseController<Rol, 
                            RoleDto>(repository, mapper, CT.Roles) {}