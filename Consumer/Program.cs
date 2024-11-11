using Azure.Messaging.EventHubs;
using Azure.Messaging.EventHubs.Consumer;
using Azure.Storage.Blobs;
using Consumer;
using Consumer.Listeners;

var builder = WebApplication.CreateBuilder(args);

//builder.Services.AddHostedService<StudentCreatedEventListener>();
//builder.Services.AddHostedService<StudentCreatedEventsHubListener>();

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddSingleton<IStudentApiClient, StudentApiClient>();

builder.Services.AddSingleton(_ =>
{
    var blobConnection = builder.Configuration.GetConnectionString("StorageAccount");
    var blobContainerClient = new BlobContainerClient(
        blobConnection,
        "events");

    var eventHubsConnection = builder.Configuration.GetConnectionString("EventHubs");
    var processor = new EventProcessorClient(
        blobContainerClient,
        EventHubConsumerClient.DefaultConsumerGroupName,
        eventHubsConnection,
        "my_event_hub");

    return processor;
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
