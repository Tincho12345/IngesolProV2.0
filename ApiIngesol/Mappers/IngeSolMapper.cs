using ApiIngesol.Models;
using ApiIngesol.Models.Materiales;
using ApiIngesol.Models.Movilidad;
using ApiIngesol.Models.Ubicacion;
using ApiIngesol.Models.Users;
using ApiIngesol.Models.Users.Dtos;
using ApiIngesol.Models.Viatico;
using AutoMapper;

namespace ApiIngesol.Mappers
{
    public class IngeSolMapper : Profile
    {
        public IngeSolMapper()
        {
            // Material
            CreateMap<UnidadMedida, UnidadMedidaDto>().ReverseMap();
            CreateMap<FamiliaDto, Familia>().ReverseMap();
            CreateMap<FamiliaReadDto, Familia>().ReverseMap();

            // Contacto
            CreateMap<ContactoDto, Contacto>()
                .ForMember(dest => dest.Id, opt => opt.Ignore());

            CreateMap<ClaseDto, Clase>().ReverseMap();
            CreateMap<Clase, ClaseReadDto>()
                .ForMember(dest => dest.FamiliaNombre, opt => opt.MapFrom(src => src.Familia.Nombre));

            CreateMap<TipoDto, Tipo>().ReverseMap();
            CreateMap<Tipo, TipoReadDto>()
                .ForMember(dest => dest.ClaseNombre, opt => opt.MapFrom(src => src.Clase.Nombre));

            CreateMap<Material, MaterialReadDto>()
                .ForMember(dest => dest.TipoNombre,
                    opt => opt.MapFrom(src => src.Tipo != null ? src.Tipo.Nombre : string.Empty))
                .ForMember(dest => dest.ClaseNombre,
                    opt => opt.MapFrom(src =>
                        src.Tipo != null && src.Tipo.Clase != null
                            ? src.Tipo.Clase.Nombre
                            : string.Empty))
                .ForMember(dest => dest.FamiliaNombre,
                    opt => opt.MapFrom(src =>
                        src.Tipo != null && src.Tipo.Clase != null && src.Tipo.Clase.Familia != null
                            ? src.Tipo.Clase.Familia.Nombre
                            : string.Empty))
                .ForMember(dest => dest.UnidadNombre,
                    opt => opt.MapFrom(src => src.UnidadMedida != null ? src.UnidadMedida.Nombre : string.Empty));

            CreateMap<MaterialCreateDto, Material>().ReverseMap();

            // Usuarios
            CreateMap<LoginDto, ApplicationUser>();
            CreateMap<LoginResponseDto, ApplicationUser>();
            CreateMap<RegisterDto, ApplicationUser>();
            CreateMap<UpdateUserDto, ApplicationUser>();

            // Infraestructura
            CreateMap<AreaTecnicaDto, AreaTecnica>();
            CreateMap<Puesto, PuestoDto>().ReverseMap();
            CreateMap<AreaTecnica, AreaTecnicaReadDto>();

            // Ubicación
            CreateMap<PaisDto, Pais>().ReverseMap();
            CreateMap<ProvinciaDto, Provincia>().ReverseMap();
            CreateMap<Provincia, ProvinciaReadDto>()
                .ForMember(dest => dest.PaisNombre, opt => opt.MapFrom(src => src.Pais.Nombre));

            CreateMap<CiudadDto, Ciudad>().ReverseMap();
            CreateMap<CiudadReadDto, Ciudad>().ReverseMap();

            // Plantas
            CreateMap<Planta, PlantaDto>().ReverseMap();

            // Areas
            CreateMap<Area, AreaDto>().ReverseMap();
            CreateMap<Area, AreaReadDto>()
                .ForMember(dest => dest.PlantaNombre, opt => opt.MapFrom(src => src.Planta.Nombre));
            CreateMap<AreaDto, Area>();

            // Clientes
            CreateMap<Cliente, ClienteDto>().ReverseMap();
            CreateMap<Cliente, ClienteReadDto>().ReverseMap();

            // País
            CreateMap<Pais, PaisDto>().ReverseMap();

            // Proyectos
            CreateMap<Proyecto, CreateProyectoDto>().ReverseMap();

            CreateMap<Proyecto, ProyectoReadDto>()
                .ForMember(dest => dest.AreaNombre,
                    opt => opt.MapFrom(src => src.Area.Nombre))
                .ForMember(dest => dest.PlantaNombre,
                    opt => opt.MapFrom(src => src.Area.Planta.Nombre))
                .ForMember(dest => dest.Ordenes,
                    opt => opt.MapFrom(src => src.Ordenes
                        .Select(o => new OrdenResumenDto
                        {
                            NumeroOrden = o.NumeroOrden,
                            DescripcionOrden = o.DescripcionOrden,
                            Subtotal = o.Presupuestos.Count == 1
                                ? o.Presupuestos.First().Total
                                : o.Presupuestos.Sum(p => p.Total)
                        }).ToList()
                    ))
                .ForMember(dest => dest.TotalPresupuestos, opt => opt.Ignore());

            // Roles
            CreateMap<RolDto, ApplicationRole>().ReverseMap();
            CreateMap<CreateRoleDto, ApplicationRole>().ReverseMap();


            // Ordenes
            CreateMap<Orden, OrdenReadDto>()
               .ForMember(dest => dest.ProyectoNombre, opt => opt.MapFrom(src => src.Proyecto != null ? src.Proyecto.Descripcion : string.Empty))
               .ForMember(dest => dest.AreaNombre, opt => opt.MapFrom(src => src.Proyecto != null ? src.Proyecto.Area.Nombre : string.Empty))
               .ForMember(dest => dest.PlantaNombre, opt => opt.MapFrom(src => src.Proyecto != null ? src.Proyecto.Area.Planta.Nombre : string.Empty));

            CreateMap<OrdenDto, Orden>().ReverseMap();

            // Presupuestos
            CreateMap<Presupuesto, PresupuestoDto>().ReverseMap();
            CreateMap<Presupuesto, PresupuestoReadDto>()
                .ForMember(dest => dest.NumeroOrden, opt => opt.MapFrom(src => src.Orden.NumeroOrden))
                .ForMember(dest => dest.AreaNombre, opt => opt.MapFrom(src => src.Orden.Proyecto.Area.Nombre))
                .ForMember(dest => dest.PlantaNombre, opt => opt.MapFrom(src => src.Orden.Proyecto.Area.Planta.Nombre ?? string.Empty))
                .ReverseMap();

            // ItemPresupuestos
            CreateMap<ItemPresupuesto, ItemPresupuestoDto>().ReverseMap();
            CreateMap<ItemPresupuesto, ItemPresupuestoReadDto>()
                .ForMember(d => d.MaterialNombre,
                    o => o.MapFrom(s => s.Material != null ? s.Material.Descripcion : string.Empty))
                .ForMember(d => d.UnidadMedidaNombre,
                    o => o.MapFrom(s => s.Material != null && s.Material.UnidadMedida != null
                        ? s.Material.UnidadMedida.Nombre
                        : string.Empty))
                .ForMember(d => d.CodigoBarra,
                    o => o.MapFrom(s => s.Material != null ? s.Material.CodigoBarra : null))

                // ✅ ESTE ES EL CLAVE
                .ForMember(d => d.ImagePath,
                    o => o.MapFrom(s => s.Material != null ? s.Material.ImagePath : null))

                .ForMember(d => d.PesoUnitario,
                    o => o.MapFrom(s => s.Material != null ? s.Material.PesoUnitario : 0))
                .ForMember(d => d.PrecioUnitario,
                    o => o.MapFrom(s => s.Material != null ? s.Material.PrecioUnitario : 0));

            // ItemPresupuestos
            CreateMap<TipoViatico, TipoViaticoDto>().ReverseMap();

            // ItemPresupuestos
            CreateMap<ValorViatico, ValorViaticoCreateDto>().ReverseMap();

            CreateMap<ValorViatico, ValorViaticoReadDto>()
                .ForMember(dest => dest.TipoViaticoNombre,
                           opt => opt.MapFrom(src => src.TipoViatico.Nombre))
                .ForMember(dest => dest.TipoViaticoObservaciones,
                           opt => opt.MapFrom(src => src.TipoViatico.Observaciones));

            // RegistroViatico -> RegistroViaticoReadDto
            CreateMap<RegistroViatico, RegistroViaticoReadDto>()

                // Tipo de viático (desde la navegación ValorViatico -> TipoViatico)
                .ForMember(dest => dest.TipoViaticoNombre,
                           opt => opt.MapFrom(src => src.ValorViatico != null && src.ValorViatico.TipoViatico != null
                               ? src.ValorViatico.TipoViatico.Nombre
                               : string.Empty))

                // Valor guardado en el registro (valor vigente al consumir)
                .ForMember(dest => dest.Valor,
                           opt => opt.MapFrom(src => src.Valor))

                // Presupuesto directamente relacionado en RegistroViatico
                .ForMember(dest => dest.PresupuestoId,
                           opt => opt.MapFrom(src => src.Presupuesto != null ? src.Presupuesto.Id : Guid.Empty))
                .ForMember(dest => dest.PresupuestoNombre,
                           opt => opt.MapFrom(src => src.Presupuesto != null ? src.Presupuesto.Descripcion : string.Empty))

                // Fecha del consumo
                .ForMember(dest => dest.Fecha,
                           opt => opt.MapFrom(src => src.Fecha))

                // 🔥 Nuevo: EmpleadoNombre desde User
                .ForMember(dest => dest.EmpleadoNombre,
                           opt => opt.MapFrom(src => src.User != null
                               ? $"{src.User.FirstName} {src.User.LastName}".Trim()
                               : string.Empty));


            // RegistroViaticoCreateDto -> RegistroViatico
            CreateMap<RegistroViaticoCreateDto, RegistroViatico>();

            // *****************************************
            // **************   MOVILIDAD   ************
            // *****************************************

            // TipoMovilidad
            CreateMap<TipoMovilidad, TipoMovilidadDto>()
                .ForMember(dest => dest.Valores,
                           opt => opt.MapFrom(src => src.Valores));

            // ValorMovilidad
            CreateMap<ValorMovilidad, ValorMovilidadCreateDto>().ReverseMap();

            CreateMap<ValorMovilidad, ValorMovilidadReadDto>()
                .ForMember(dest => dest.TipoMovilidadNombre,
                           opt => opt.MapFrom(src =>
                                src.TipoMovilidad.Desde + " - " + src.TipoMovilidad.Hasta));

            // RegistroMovilidad
            CreateMap<RegistroMovilidad, RegistroMovilidadReadDto>()
                .ForMember(dest => dest.UserNombre,
                           opt => opt.MapFrom(src =>
                                (src.User.FirstName + " " + src.User.LastName).Trim()))
                .ForMember(dest => dest.TipoMovilidadNombre,
                           opt => opt.MapFrom(src =>
                                src.ValorMovilidad.TipoMovilidad.Desde + " - " +
                                src.ValorMovilidad.TipoMovilidad.Hasta))
                .ForMember(dest => dest.PresupuestoNombre,
                           opt => opt.MapFrom(src => src.Presupuesto.Nombre));


            CreateMap<RegistroMovilidadCreateDto, RegistroMovilidad>();

            CreateMap<TipoMovilidad, TipoMovilidadCreateDto>().ReverseMap();

            // *****************************************
            // **************   ASISTENCIA   ***********
            // *****************************************

            // RegistroAsistencia -> RegistroAsistenciaReadDto
            CreateMap<RegistroAsistencia, RegistroAsistenciaReadDto>()
                .ForMember(dest => dest.EmpleadoNombre,
                    opt => opt.MapFrom(src => src.Empleado != null
                        ? $"{src.Empleado.FirstName} {src.Empleado.LastName}".Trim()
                        : string.Empty))
                .ForMember(dest => dest.PresupuestoNombre,
                    opt => opt.MapFrom(src => src.Presupuesto != null
                        ? src.Presupuesto.Descripcion
                        : string.Empty))
                .ForMember(dest => dest.HoraEntrada,
                    opt => opt.MapFrom(src => src.HoraEntrada))
                .ForMember(dest => dest.HoraSalida,
                    opt => opt.MapFrom(src => src.HoraSalida))
                .ForMember(dest => dest.Activo,
                    opt => opt.MapFrom(src => src.IsActive))
                .ForMember(dest => dest.Observaciones,
                    opt => opt.MapFrom(src => src.Observaciones))

                // 👇👇 **AGREGADO CORRECTO DEL CAMPO CALCULADO** 👇👇
                .ForMember(dest => dest.HorasTrabajadas,
                    opt => opt.MapFrom(src =>
                        src.HorasTrabajadas.HasValue
                            ? src.HorasTrabajadas.Value.ToString(@"hh\:mm")
                            : null));

            // RegistroAsistenciaCreateDto -> RegistroAsistencia
            CreateMap<RegistroAsistenciaCreateDto, RegistroAsistencia>()
                .ForMember(dest => dest.Id, opt => opt.Ignore()); // activo por defecto

            // ================================
            // UI - Background Images
            // ================================

            // Create DTO -> Entity
            // ================================
            // Solicitud de Proyectos (Formulario Web)
            // ================================
            CreateMap<CreateSolicitudProyectoDto, SolicitudProyecto>()
                .ForMember(dest => dest.Id, opt => opt.Ignore());

            CreateMap<SolicitudProyecto, SolicitudProyectoReadDto>();

        }
    }
}
