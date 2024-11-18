namespace Consumer
{
    public static class ServiceCollectionExtensions
    {
        public static readonly string HttpClientName = "StudentApiHttpClient";

        public static void AddStudentApiClient(this IServiceCollection services)
        {
            services.AddHttpClient(HttpClientName, client =>
            {
                client.BaseAddress = new Uri("http://localhost:5126/");
            });

            services.AddSingleton<IApiClient, ApiClient>();

            services.AddSingleton<IStudentApiClient, StudentApiClient>();
        }
    }
}
