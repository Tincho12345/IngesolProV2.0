namespace WebIngesol.Mappers
{
    using AutoMapper;
    using WebIngesol.Models;
    using WebIngesol.Models.AreaTecnica;
    using WebIngesol.Models.Materiales;
    using WebIngesol.Models.Movilidad;
    using WebIngesol.Models.Ubicacion;
    using WebIngesol.Models.Viatico;
    using WebIngesol.Models.Viaticos;

    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<AreaTecnica, AreaTecnicaDto>().ReverseMap();
            CreateMap<Pais, PaisDtos>().ReverseMap();
            CreateMap<Provincia, ProvinciaDto>().ReverseMap();
            CreateMap<Ciudad, CiudadDto>().ReverseMap();
            CreateMap<Tipo, TipoDto>().ReverseMap();
            CreateMap<Clase, ClaseDto>().ReverseMap();
            CreateMap<Familia, FamiliaDtos>().ReverseMap();
            CreateMap<UnidadMedida, UnidadMedidaDto>().ReverseMap();
            CreateMap<CreateMaterialDto, ReadMaterialDto>().ReverseMap();
            CreateMap<Material, CreateMaterialDto>().ReverseMap();
            CreateMap<Material, ReadMaterialDto>();
            CreateMap<Puesto, PuestoDto>().ReverseMap();
            CreateMap<Planta, PlantaDto>().ReverseMap();
            CreateMap<Area, AreaDto>().ReverseMap();
            CreateMap<Cliente, ClienteDto>().ReverseMap();
            CreateMap<Contacto, ContactoDto>().ReverseMap();
            CreateMap<Usuario, UsuarioDto>().ReverseMap();
            CreateMap<Rol, RoleDto>().ReverseMap();
            CreateMap<Proyecto, CreateProyectoDto>().ReverseMap();

            // Orden <-> OrdenDto
            CreateMap<Orden, OrdenDto>().ReverseMap();

            // Orden -> OrdenReadDto
            CreateMap<Orden, OrdenReadDto>().ReverseMap();
            CreateMap<OrdenDto, OrdenReadDto>().ReverseMap();

            CreateMap<Presupuesto, PresupuestoDto>().ReverseMap();
            CreateMap<Presupuesto, PresupuestoReadDto>().ReverseMap();
            CreateMap<PresupuestoDto, PresupuestoReadDto>().ReverseMap();
            CreateMap<ItemPresupuesto, ItemPresupuestoDto>().ReverseMap();
            CreateMap<ItemPresupuesto, ItemPresupuestoReadDto>().ReverseMap();
            CreateMap<ItemPresupuestoDto, ItemPresupuestoReadDto>().ReverseMap();
            CreateMap<TipoViatico, TipoViaticoDto>().ReverseMap();

            CreateMap<TipoMovilidad, TipoMovilidadCreateDto>().ReverseMap();
            CreateMap<TipoMovilidad, TipoMovilidadDto>().ReverseMap();

            CreateMap<ValorMovilidad, ValorMovilidadCreateDto>().ReverseMap();
            CreateMap<ValorMovilidad, ValorMovilidadReadDto>().ReverseMap();
            // Viaticos -> ValoresViaticos
            CreateMap<ValorViatico, ValorViaticoDto>().ReverseMap();
            // Crear registro → Modelo
            CreateMap<RegistroViaticoCreateDto, RegistroViatico>().ReverseMap();

            // Leer registro ← Modelo
            CreateMap<RegistroViatico, RegistroViaticoReadDto>()
                .ForMember(dest => dest.ValorViaticoId, opt => opt.MapFrom(src => Guid.Parse(src.ValorViaticoId)))
                .ForMember(dest => dest.TipoViaticoNombre, opt => opt.MapFrom(src => src.TipoViaticoNombre))
                .ForMember(dest => dest.Valor, opt => opt.MapFrom(src => src.Valor))
                .ForMember(dest => dest.PresupuestoId, opt => opt.MapFrom(src => Guid.Parse(src.PresupuestoId)))
                .ForMember(dest => dest.PresupuestoNombre, opt => opt.MapFrom(src => src.PresupuestoNombre))
                .ForMember(dest => dest.Fecha, opt => opt.MapFrom(src => src.Fecha))
                .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.UserId));

            // 🔄 Backend → Front (DTO → Modelo)
            CreateMap<RegistroMovilidadCreateDto, RegistroMovilidad>().ReverseMap();

            CreateMap<RegistroMovilidad, RegistroMovilidadReadDto>()
                .ForMember(dest => dest.userNombre, opt => opt.MapFrom(src => src.userNombre))
                .ForMember(dest => dest.ValorMovilidadId, opt => opt.MapFrom(src => src.ValorMovilidadId))
                .ForMember(dest => dest.PresupuestoId, opt => opt.MapFrom(src => src.PresupuestoId))
                .ForMember(dest => dest.Valor, opt => opt.MapFrom(src => src.Valor))
                .ForMember(dest => dest.TipoMovilidadNombre, opt => opt.MapFrom(src => src.TipoMovilidadNombre))
                .ForMember(dest => dest.PresupuestoNombre, opt => opt.MapFrom(src => src.PresupuestoNombre))
                .ForMember(dest => dest.Fecha, opt => opt.MapFrom(src => src.Fecha))
                .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.UserId));

                CreateMap<RegistroAsistencia, RegistroAsistenciaCreateDto>().ReverseMap();

                CreateMap<RegistroAsistencia, RegistroAsistenciaReadDto>()
                .ForMember(dest => dest.HorasTrabajadas,
                    opt => opt.MapFrom(src =>
                        src.HoraSalida.HasValue
                            ? (src.HoraSalida.Value - src.HoraEntrada).ToString(@"hh\:mm")
                            : null
                    ));

            // =======================================
            // Solicitudes de Proyecto
            // =======================================
            CreateMap<SolicitudProyectoReadDto, SolicitudProyectoDto>().ReverseMap();
        }
    }
}