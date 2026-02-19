using System.Net.Http.Headers;
using System.Text.Json;
using WebIngesol.Repository.IRepository;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace WebIngesol.Repository;

public class Repository<T> : IRepository<T> where T : class
{
    private readonly IHttpClientFactory _clientFactory;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly JsonSerializerOptions _options;

    public Repository(IHttpClientFactory clientFactory, IHttpContextAccessor httpContextAccessor)
    {
        _clientFactory = clientFactory;
        _httpContextAccessor = httpContextAccessor;
        _options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };
    }

    private void AgregarToken(HttpClient client)
    {
        var token = _httpContextAccessor.HttpContext?.Session.GetString("JWToken");
        if (!string.IsNullOrEmpty(token))
        {
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }
    }

    public async Task<IEnumerable<T>> GetAllAsync(string url, string? filter)
    {
        var client = _clientFactory.CreateClient();
        AgregarToken(client);

        var requestUrl = string.IsNullOrWhiteSpace(filter)
         ? url
         : $"{url}?filter={Uri.EscapeDataString(filter)}";

        var response = await client.GetAsync(requestUrl);
        if (!response.IsSuccessStatusCode) return [];

        var json = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<IEnumerable<T>>(json, _options) ?? [];
    }

    public async Task<T?> GetByIdAsync(string url, Guid id)
    {
        var client = _clientFactory.CreateClient();
        AgregarToken(client);

        var response = await client.GetAsync($"{url}/{id}");
        if (!response.IsSuccessStatusCode) return null;

        var json = await response.Content.ReadAsStringAsync();

        var respuesta = JsonSerializer.Deserialize<T>(json, _options);
        return respuesta; 
    }

    public async Task<bool> CreateAsync(string url, T objToCreate)
    {
        if (objToCreate == null) return false;

        var client = _clientFactory.CreateClient();
        AgregarToken(client); // ✅ Este método agrega el JWT si está en sesión

        var form = new MultipartFormDataContent();

        foreach (var prop in objToCreate.GetType().GetProperties())
        {
            var nombre = prop.Name;
            var valor = prop.GetValue(objToCreate);

            if (valor == null)
            {
                Console.WriteLine($"⚠️ {nombre} viene NULL");
                continue;
            }

            Console.WriteLine($"📦 Enviando: {nombre} = {valor}");

            // 📁 Archivos subidos desde formulario (IFormFile)
            if (valor is IFormFile archivo)
            {
                var fileContent = new StreamContent(archivo.OpenReadStream());
                fileContent.Headers.ContentType = new MediaTypeHeaderValue(archivo.ContentType ?? "application/octet-stream");
                form.Add(fileContent, nombre, archivo.FileName);
            }
            // 📦 byte[]
            else if (valor is byte[] bytes)
            {
                var byteContent = new ByteArrayContent(bytes);
                form.Add(byteContent, nombre, nombre);
            }
            // 🆔 GUID u otros tipos simples
            else
            {
                //var stringValue = valor.ToString();
                var stringValue = NormalizarDecimal(valor);
                if (!string.IsNullOrEmpty(stringValue))
                    form.Add(new StringContent(stringValue), nombre);
            }
        }

        var response = await client.PostAsync(url, form);

        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"❌ Error {response.StatusCode}: {errorContent}");
        }

        return response.IsSuccessStatusCode;
    }

    public async Task<bool> UpdateAsync(string url, Guid id, T objToUpdate)
    {
        if (objToUpdate == null) return false;

        var client = _clientFactory.CreateClient();
        AgregarToken(client);

        var request = new HttpRequestMessage(HttpMethod.Put, $"{url}/{id}");
        var form = new MultipartFormDataContent();

        foreach (var prop in objToUpdate.GetType().GetProperties())
        {
            var nombre = prop.Name;
            var valor = prop.GetValue(objToUpdate);

            if (valor == null) continue;

            if (valor is IFormFile archivo)
            {
                var streamContent = new StreamContent(archivo.OpenReadStream());
                form.Add(streamContent, nombre, archivo.FileName);
            }
            else if (valor is byte[] bytes)
            {
                var byteContent = new ByteArrayContent(bytes);
                form.Add(byteContent, nombre, nombre);
            }
            else
            {
                var texto = NormalizarDecimal(valor);
                form.Add(new StringContent(texto), nombre);
            }
        }

        request.Content = form;
        var response = await client.SendAsync(request);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> DeleteAsync(string url, Guid id)
    {
        var client = _clientFactory.CreateClient();
        AgregarToken(client);

        var response = await client.DeleteAsync($"{url}/{id}");
        return response.IsSuccessStatusCode;
    }

    public async Task<IEnumerable<T>> GetAllByPropertyGuidAsync(string url, string propertyName, Guid value)
    {
        var client = _clientFactory.CreateClient();
        AgregarToken(client);

        var requestUrl = $"{url}/filter-by-guid?propertyName={propertyName}&value={value}";
        var response = await client.GetAsync(requestUrl);

        if (!response.IsSuccessStatusCode) return [];

        var json = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<IEnumerable<T>>(json, _options) ?? [];
    }

    private static string NormalizarDecimal(object valor)
    {
        if (valor == null) return string.Empty;

        // ============
        // 1) STRING
        // ============
        if (valor is string s)
        {
            s = s.Trim();

            // 👉 Si tiene "/", intentamos interpretar dd/MM/yyyy
            if (s.Contains('/') && !s.Contains('-'))
            {
                if (DateTime.TryParseExact(
                    s,
                    "dd/MM/yyyy",
                    System.Globalization.CultureInfo.GetCultureInfo("es-AR"),
                    System.Globalization.DateTimeStyles.None,
                    out var fecha))
                {
                    return fecha.ToString("yyyy-MM-dd HH:mm:ss");
                }
            }

            // 👉 Si tiene "/", con hora
            if (s.Contains('/') && s.Contains(':'))
            {
                if (DateTime.TryParseExact(
                    s,
                    "dd/MM/yyyy HH:mm:ss",
                    System.Globalization.CultureInfo.GetCultureInfo("es-AR"),
                    System.Globalization.DateTimeStyles.None,
                    out var fechaHora))
                {
                    return fechaHora.ToString("yyyy-MM-dd HH:mm:ss");
                }
            }

            // Decimales: reemplazar coma por punto
            s = s.Replace(",", ".");

            if (decimal.TryParse(s,
                System.Globalization.NumberStyles.Any,
                System.Globalization.CultureInfo.InvariantCulture,
                out var num))
            {
                return num.ToString(System.Globalization.CultureInfo.InvariantCulture);
            }

            return s;
        }

        // ============
        // 2) DateTime
        // ============
        if (valor is DateTime dt)
            return dt.ToString("yyyy-MM-dd HH:mm:ss");

        // ============
        // 3) Números
        // ============
        return valor switch
        {
            decimal d => d.ToString(System.Globalization.CultureInfo.InvariantCulture),
            double db => db.ToString(System.Globalization.CultureInfo.InvariantCulture),
            float f => f.ToString(System.Globalization.CultureInfo.InvariantCulture),
            _ => valor.ToString() ?? string.Empty
        };
    }
}
