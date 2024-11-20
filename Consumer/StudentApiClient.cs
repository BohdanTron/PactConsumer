using System.IO;
using System.Text.Json;

namespace Consumer
{
    public interface IStudentApiClient
    {
        Task<Student?> GetStudentById(int studentId);
    }

    public class StudentApiClient : IStudentApiClient
    {
        private readonly HttpClient _httpClient;

        public StudentApiClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<Student?> GetStudentById(int studentId)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, $"/students/{studentId}");

            request.Headers.Add("Accept", "application/json");
            request.Headers.Add("Authorization", $"Bearer {DateTime.Now:yyyy-MM-ddTHH:mm:ss.fffZ}");
            request.Headers.Add("X-correlation-id", Guid.NewGuid().ToString());

            var response = await _httpClient.SendAsync(request);

            if (!response.IsSuccessStatusCode)
                return null;

            var content = await response.Content.ReadAsStringAsync();

            return JsonSerializer.Deserialize<Student>(content,
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
        }
    }
}
