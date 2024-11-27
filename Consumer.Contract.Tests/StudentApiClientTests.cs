using System.Net;
using System.Text.Json;
using FluentAssertions;
using Moq;
using PactNet;
using PactNet.Matchers;
using PactNet.Output.Xunit;
using Xunit.Abstractions;
using Match = PactNet.Matchers.Match;

namespace Consumer.Contract.Tests
{
    public class StudentApiClientTests
    {
        private readonly IPactBuilderV4 _pactBuilder;

        private readonly Mock<IHttpClientFactory> _httpClientFactoryMock = new();

        public StudentApiClientTests(ITestOutputHelper output)
        {
            _pactBuilder = Pact.V4("StudentApiClient", "StudentApi", new PactConfig
            {
                PactDir = Directory.GetParent(Directory.GetCurrentDirectory())?.Parent?.Parent?.FullName + "/pacts",
                Outputters = [new XunitOutput(output)],
                LogLevel = PactLogLevel.Debug,
                DefaultJsonSettings = new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                    PropertyNameCaseInsensitive = true
                }
            }).WithHttpInteractions();
        }

        [Fact]
        public async Task GetById_Exist()
        {
            // Arrange
            var expectedStudent = new { id = 10, firstName = "James", lastName = "Hetfield", address = "1234, 56th Street, San Francisco, USA" };

            _pactBuilder
                .UponReceiving("a request to get a student")
                    .Given("student with id 10 exists")
                    .WithRequest(HttpMethod.Get, "/students/10")
                    .WithHeader("Authorization", Match.Regex("Bearer 2024-01-14T11:34:18.045Z", "Bearer \\d{4}-\\d{2}-\\d{2}T\\d{2}:\\d{2}:\\d{2}\\.\\d{3}Z"))
                    .WithHeader("X-correlation-id", Match.Type("AA8F3DB6-3FE3-489C-8BFB-F310CE493584"))
                    .WithHeader("Accept", "application/json")
                .WillRespond()
                .WithStatus(HttpStatusCode.OK)
                    .WithHeader("Content-Type", "application/json; charset=utf-8")
                    .WithJsonBody(new TypeMatcher(expectedStudent));

            await _pactBuilder.VerifyAsync(async ctx =>
            {
                // Act
                _httpClientFactoryMock
                    .Setup(x => x.CreateClient(ServiceCollectionExtensions.HttpClientName))
                    .Returns(new HttpClient { BaseAddress = ctx.MockServerUri });

                var apiClient = new StudentApiClient(new ApiClient(_httpClientFactoryMock.Object));
                var student = await apiClient.GetStudentById(10);

                // Assert
                student?.Id.Should().Be(10);
            });
        }

        [Fact]
        public async Task GetById_NotExist()
        {
            // Arrange
            _pactBuilder
                .UponReceiving("a request to get a non-existing student")
                    .Given("no student with id 11 exists")
                    .WithRequest(HttpMethod.Get, "/students/11")
                    .WithHeader("Authorization", Match.Regex("Bearer 2024-01-14T11:34:18.045Z", "Bearer \\d{4}-\\d{2}-\\d{2}T\\d{2}:\\d{2}:\\d{2}\\.\\d{3}Z"))
                    .WithHeader("Accept", "application/json")
                    .WithHeader("X-correlation-id", Match.Type("AA8F3DB6-3FE3-489C-8BFB-F310CE493584"))
                .WillRespond()
                    .WithStatus(HttpStatusCode.NotFound);

            await _pactBuilder.VerifyAsync(async ctx =>
            {
                // Act
                _httpClientFactoryMock
                    .Setup(x => x.CreateClient(ServiceCollectionExtensions.HttpClientName))
                    .Returns(new HttpClient { BaseAddress = ctx.MockServerUri });

                var apiClient = new StudentApiClient(new ApiClient(_httpClientFactoryMock.Object));
                var student = await apiClient.GetStudentById(11);

                // Assert
                student.Should().BeNull();
            });
        }

        [Fact]
        public async Task GetStudent_MissingAuthHeader()
        {
            // Arrange
            _pactBuilder
                .UponReceiving("an unauthorized request to get a student")
                    .Given("no auth token is provided")
                    .WithRequest(HttpMethod.Get, "/students/10")
                .WillRespond()
                    .WithStatus(HttpStatusCode.Unauthorized);

            await _pactBuilder.VerifyAsync(async ctx =>
            {
                // Act
                _httpClientFactoryMock
                    .Setup(x => x.CreateClient(ServiceCollectionExtensions.HttpClientName))
                    .Returns(new HttpClient { BaseAddress = ctx.MockServerUri });

                var apiClient = new StudentApiClient(new ApiClient(_httpClientFactoryMock.Object));
                var student = await apiClient.GetStudentById(10);

                // Assert
                student.Should().BeNull();
            });
        }
    }
}