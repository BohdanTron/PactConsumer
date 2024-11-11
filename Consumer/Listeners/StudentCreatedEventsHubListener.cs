using System.Text;
using Azure.Messaging.EventHubs;
using Azure.Messaging.EventHubs.Processor;

namespace Consumer.Listeners
{
    public class StudentCreatedEventsHubListener : BackgroundService
    {
        private readonly EventProcessorClient _processor;

        public StudentCreatedEventsHubListener(EventProcessorClient processor)
        {
            _processor = processor;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _processor.ProcessEventAsync += ProcessEventHandler;
            _processor.ProcessErrorAsync += ProcessErrorHandler;

            await _processor.StartProcessingAsync(stoppingToken);

            await Task.Delay(Timeout.Infinite, stoppingToken);

            await _processor.StopProcessingAsync(stoppingToken);
        }

        private Task ProcessEventHandler(ProcessEventArgs arg)
        {
            Console.WriteLine("\tReceived event: {0}", Encoding.UTF8.GetString(arg.Data.Body.ToArray()));
            return Task.CompletedTask;
        }

        private Task ProcessErrorHandler(ProcessErrorEventArgs arg)
        {
            Console.WriteLine($"\tPartition '{arg.PartitionId}': an unhandled exception was encountered. This was not expected to happen.");
            Console.WriteLine(arg.Exception.Message);
            return Task.CompletedTask;
        }
    }
}
