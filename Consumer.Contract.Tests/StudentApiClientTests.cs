using System.Net;
using FluentAssertions;
using PactNet;
using PactNet.Matchers;
using PactNet.Output.Xunit;
using Xunit.Abstractions;

namespace Consumer.Contract.Tests
{
    public class StudentApiClientTests
    {
        private readonly IPactBuilderV4 _pactBuilder;

        public StudentApiClientTests(ITestOutputHelper output)
        {
            _pactBuilder = Pact.V4("StudentApiClient", "StudentApi", new PactConfig
            {
                PactDir = Directory.GetParent(Directory.GetCurrentDirectory())?.Parent?.Parent?.FullName + "/pacts",
                Outputters = [new XunitOutput(output)],
                LogLevel = PactLogLevel.Debug
            }).WithHttpInteractions();
        }

        [Fact]
        public async Task GetById_Exist()
        {
            // Arrange
            var expectedStudent = new { id = 10, firstName = "James", lastName = "Hetfield", address = "1234, 56th Street, San Francisco, USA", gender = "male" };

            _pactBuilder
                .UponReceiving("a request to get a student")
                    .Given("student with id 10 exists")
                    .WithRequest(HttpMethod.Get, "/students/10")
                    .WithHeader("Authorization", Match.Regex("Bearer 2024-01-14T11:34:18.045Z", "Bearer \\d{4}-\\d{2}-\\d{2}T\\d{2}:\\d{2}:\\d{2}\\.\\d{3}Z"))
                    .WithHeader("Accept", "application/json")
                .WillRespond()
                .WithStatus(HttpStatusCode.OK)
                    .WithHeader("Content-Type", "application/json; charset=utf-8")
                    .WithJsonBody(new TypeMatcher(expectedStudent));

            await _pactBuilder.VerifyAsync(async ctx =>
            {
                // Act
                var apiClient = new StudentApiClient(ctx.MockServerUri);
                var student = await apiClient.GetStudentById(10);

                // Assert
                student?.Id.Should().Be(10);
            });
        }

        [Fact]
        public async Task GetProduct_MissingAuthHeader()
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
                var apiClient = new StudentApiClient(ctx.MockServerUri);
                var student = await apiClient.GetStudentById(10);

                // Assert
                student.Should().BeNull();
            });
        }
    }
}