using System.Text;

namespace Consumer
{
    public interface IApiClient
    {
        Task<object> CallApiAsync(
            string path,
            HttpMethod method,
            Dictionary<string, string> queryParams,
            string? postBody,
            Dictionary<string, string> headerParams,
            Dictionary<string, string> formParams,
            Dictionary<string, string> fileParams);
    }

    public class ApiClient : IApiClient
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public ApiClient(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public async Task<object> CallApiAsync(
            string path,
            HttpMethod method,
            Dictionary<string, string> queryParams,
            string? postBody,
            Dictionary<string, string> headerParams,
            Dictionary<string, string> formParams,
            Dictionary<string, string> fileParams)
        {
            var request = new HttpRequestMessage(method, path);

            request.Headers.Add("Accept", "application/json");
            request.Headers.Add("Authorization", $"Bearer {DateTime.Now:yyyy-MM-ddTHH:mm:ss.fffZ}");

            // add header parameter, if any
            foreach (var param in headerParams)
                request.Headers.Add(param.Key, param.Value);

            // add query parameter, if any
            //if (queryParams.Any())
            //{
            //    var queryString = AddQueryString(path, queryParams);
            //    request.RequestUri = new Uri(queryString);
            //}


            // add form parameter, if any
            if (formParams.Any())
                request.Content = new FormUrlEncodedContent(formParams);

            if (postBody != null) // http body (model) parameter
            {
                request.Content = new StringContent(postBody, Encoding.UTF8, "application/json");
            }

            var httpClient = _httpClientFactory.CreateClient(ServiceCollectionExtensions.HttpClientName);
            return await httpClient.SendAsync(request);
        }
    }
}
