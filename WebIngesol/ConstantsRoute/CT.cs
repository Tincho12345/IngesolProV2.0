namespace WebIngesol.ConstantsRoute
{
    public class CT
    {
        private static readonly string BaseUrl;

        static CT()
        {
            BaseUrl = (Environment.GetEnvironmentVariable("ENVIRONMENT")?.ToLower()) switch
            {
                "local" => "https://localhost:7268",
                _ => "https://apiingesol-b3gfdje9bga8feah.canadacentral-01.azurewebsites.net"
            };
        }

        // 🔹 Propiedad pública para usar directamente la base en cualquier vista o clase
        public static string Base => BaseUrl;

        // 🔹 Endpoints
        public static string AreasTecnicas => $"{BaseUrl}/api/AreasTecnicas";
        public static string Clientes => $"{BaseUrl}/api/Clientes";
        public static string Ciudades => $"{BaseUrl}/api/Ciudades";
        public static string Clases => $"{BaseUrl}/api/Clases";
        public static string Familias => $"{BaseUrl}/api/Familias";
        public static string Materiales => $"{BaseUrl}/api/Materiales";
        public static string Paises => $"{BaseUrl}/api/Paises";
        public static string Plantas => $"{BaseUrl}/api/Plantas";
        public static string Areas => $"{BaseUrl}/api/Areas";
        public static string Provincias => $"{BaseUrl}/api/Provincias";
        public static string Puestos => $"{BaseUrl}/api/Puestos";
        public static string Tipos => $"{BaseUrl}/api/Tipos";
        public static string UnidadesMedida => $"{BaseUrl}/api/UnidadesMedida";
        public static string User => $"{BaseUrl}/api/User";
        public static string Telefonos => $"{BaseUrl}/api/Telefonos";
        public static string Contactos => $"{BaseUrl}/api/Contactos";
        public static string Roles => $"{BaseUrl}/api/Roles";
        public static string Proyectos => $"{BaseUrl}/api/Proyectos";
        public static string Ordenes => $"{BaseUrl}/api/Ordenes";
        public static string Presupuestos => $"{BaseUrl}/api/Presupuestos";
        public static string ItemPresupuestos => $"{BaseUrl}/api/ItemPresupuestos";
        public static string ValorViaticos => $"{BaseUrl}/api/ValorViaticos";
        public static string TipoViaticos => $"{BaseUrl}/api/TipoViaticos";
        public static string ValoresViaticos => $"{BaseUrl}/api/ValoresViaticos";
        public static string RegistrosViaticos => $"{BaseUrl}/api/RegistrosViaticos";
        public static string TiposMovilidad => $"{BaseUrl}/api/TiposMovilidad";
        public static string ValoresMovilidad => $"{BaseUrl}/api/ValoresMovilidad";
        public static string RegistrosMovilidad => $"{BaseUrl}/api/RegistrosMovilidad";
        public static string RegistrosAsistencias => $"{BaseUrl}/api/RegistrosAsistencias";
        public static string SolicitudesProyecto => $"{BaseUrl}/api/SolicitudesProyecto";
    }    
}
