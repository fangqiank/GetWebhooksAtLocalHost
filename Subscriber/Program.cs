using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Subscriber.Dtos;

IConfiguration configuration = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
    .AddUserSecrets(typeof(Program).Assembly, optional: true)
    .Build();


string connection = configuration["ServiceBusConnectionString"]!;
string topicName = configuration["ServiceBusTopicName"]!;
string subscriptionName = configuration["ServiceBusSubscriptionName"]!;

ServiceBusClient client;
ServiceBusProcessor processor;

client = new ServiceBusClient(connection);

processor = client.CreateProcessor(topicName, subscriptionName, 
    new ServiceBusProcessorOptions());

try
{
    processor.ProcessMessageAsync += MessageHandler;
    processor.ProcessErrorAsync += ErrorHandler;

    await processor.StartProcessingAsync();
    Console.WriteLine("--> Listening to Service Bus");

    Console.WriteLine("Press any  key to end processing");
    Console.ReadKey();

    Console.WriteLine("--> Attempting to stop receiver");
    await processor.StopProcessingAsync();
    Console.WriteLine("--> Receiver stopped");
}
finally
{
    await processor.DisposeAsync();
    await client.DisposeAsync();
}

//Handle receive messages
async Task MessageHandler(ProcessMessageEventArgs args)
{
    string body = args.Message.Body.ToString();

    var webDto = JsonConvert.DeserializeObject<WebhookEventDto>(body);

    Console.WriteLine($"--> Headers: {webDto!.Headers} / Body: {webDto.Body}");

    await args.CompleteMessageAsync(args.Message);
}

Task ErrorHandler(ProcessErrorEventArgs args)
{
    Console.WriteLine(args.Exception.ToString());

    return Task.CompletedTask;
}
