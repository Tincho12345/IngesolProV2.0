using Newtonsoft.Json;
using System.Text;
using WebIngesol.Models;
using WebIngesol.Repository.IRepository;

namespace WebIngesol.Repository
{
    public class AccountRepositorio(IHttpClientFactory clientFactory) : IAccountRepositorio
    {
        private readonly IHttpClientFactory _clientFactory = clientFactory ?? throw new ArgumentNullException(nameof(clientFactory));

        public async Task<LoginResponse> LoginAsync(string url, LoginRequest loginRequest)
        {
            if (loginRequest == null)
                return new LoginResponse { Error = "Parámetros inválidos." };

            var client = _clientFactory.CreateClient();

            var request = new HttpRequestMessage(HttpMethod.Post, url)
            {
                Content = new StringContent(
                    JsonConvert.SerializeObject(loginRequest),
                    Encoding.UTF8,
                    "application/json")
            };

            var response = await client.SendAsync(request);
            var jsonString = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                try
                {
                    var loginResponse = JsonConvert.DeserializeObject<LoginResponse>(jsonString);
                    return loginResponse ?? new LoginResponse { Error = "Respuesta vacía o inválida del servidor." };
                }
                catch
                {
                    return new LoginResponse { Error = "Error al deserializar la respuesta." };
                }
            }

            return new LoginResponse { Error = $"Error HTTP: {response.StatusCode}, Detalle: {jsonString}" };
        }

        public async Task<Usuario?> ObtenerUsuarioPorUserName(string baseUrl, string userName, string jwtToken)
        {
            var client = _clientFactory.CreateClient();

            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", jwtToken);

            var response = await client.GetAsync($"{baseUrl}/{userName}");

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<Usuario>(content);
            }

            return null;
        }

        public async Task<bool> RegisterAsync(string url, RegisterDto usuario)
        {
            ArgumentNullException.ThrowIfNull(usuario);

            var client = _clientFactory.CreateClient();

            var formData = new MultipartFormDataContent
            {
                // Agregar campos simples
                { new StringContent(usuario.UserName), nameof(usuario.UserName) },
                { new StringContent(usuario.Email), nameof(usuario.Email) },
                { new StringContent(usuario.Password), nameof(usuario.Password) },
                { new StringContent(usuario.ConfirmPassword), nameof(usuario.ConfirmPassword) },
                { new StringContent(usuario.FirstName), nameof(usuario.FirstName) },
                { new StringContent(usuario.LastName), nameof(usuario.LastName) },
                { new StringContent(usuario.RolId.ToString()), nameof(usuario.RolId) }
            };

            // Agregar la imagen si existe
            if (usuario.UserPic != null)
            {
                // Suponiendo que tienes un Stream o un byte[] en usuario.UserPic
                // Pero IFormFile no es serializable para enviarlo así, entonces debes tener un Stream o archivo local

                var stream = usuario.UserPic.OpenReadStream(); // si tienes un IFormFile

                var fileContent = new StreamContent(stream);
                fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(usuario.UserPic.ContentType);

                formData.Add(fileContent, nameof(usuario.UserPic), usuario.UserPic.FileName);
            }

            var response = await client.PostAsync(url, formData);

            var responseText = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
                Console.WriteLine($"Registro fallido: {responseText}");

            return response.IsSuccessStatusCode;
        }
    }
}
