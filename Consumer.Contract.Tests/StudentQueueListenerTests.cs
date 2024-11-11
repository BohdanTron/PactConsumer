using System.Text.Json;
using PactNet;
using PactNet.Matchers;
using PactNet.Output.Xunit;
using Xunit.Abstractions;

namespace Consumer.Contract.Tests
{
    public class StudentQueueListenerTests
    {
        private readonly IMessagePactBuilderV4 _messagePactBuilder;

        public StudentQueueListenerTests(ITestOutputHelper output)
        {
            _messagePactBuilder = Pact.V4("StudentApiClient", "StudentApi", new PactConfig
            {
                PactDir = Directory.GetParent(Directory.GetCurrentDirectory())?.Parent?.Parent?.FullName + "/pacts",
                DefaultJsonSettings = new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                    PropertyNameCaseInsensitive = true
                },
                Outputters = [new XunitOutput(output)],
                LogLevel = PactLogLevel.Debug
            }).WithMessageInteractions();
        }

        [Fact]
        public async Task ReceiveStudentCreatedEvent()
        {
            await _messagePactBuilder
                .ExpectsToReceive("an event indicating that a student has been created")
                    .WithJsonContent(new
                    {
                        Id = Match.Integer(10)
                    })
                .VerifyAsync<StudentCreatedEvent>(async message =>
                {
                    // Do smth
                });
        }
    }
}
