using System.Text.Json;

namespace Consumer
{
    public interface IStudentApiClient
    {
        Task<Student?> GetStudentById(int studentId);
    }

    public class StudentApiClient : IStudentApiClient
    {
        private readonly IApiClient _apiClient;

        public StudentApiClient(IApiClient apiClient)
        {
            _apiClient = apiClient;
        }

        public async Task<Student?> GetStudentById(int studentId)
        {
            var headerParams = new Dictionary<string, string>
            {
                { "X-correlation-id", Guid.NewGuid().ToString() }
            };

            var response = (HttpResponseMessage) await _apiClient.CallApiAsync(
                $"/students/{studentId}",
                HttpMethod.Get,
                new Dictionary<string, string>(),
                null,
                headerParams,
                new Dictionary<string, string>(),
                new Dictionary<string, string>());

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
