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

        public StudentApiClient(Uri? uri = null)
        {
            // TODO: Replace with DI
            _httpClient = new HttpClient { BaseAddress = uri ?? new Uri("http://localhost:5126/") };
        }

        public async Task<Student?> GetStudentById(int studentId)
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, $"/students/{studentId}");
            request.Headers.Add("Accept", "application/json");
            request.Headers.Add("Authorization", $"Bearer {DateTime.Now:yyyy-MM-ddTHH:mm:ss.fffZ}");

            var response = await _httpClient.SendAsync(request);
            var content = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
                return null;

            return JsonSerializer.Deserialize<Student>(content,
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
        }
    }
}
