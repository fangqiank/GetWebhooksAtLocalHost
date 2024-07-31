using Azure.Messaging.ServiceBus;
using AzureWebhookEndpoints.Dtos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Newtonsoft.Json;
using System.Runtime.InteropServices;

namespace AzureWebhookEndpoints
{
    public class HttpTrigger1
    {
        static string connection = Environment.GetEnvironmentVariable(
            "ServiceBusConnectionString")!;
        static string topic = Environment.GetEnvironmentVariable(
            "ServiceBusTopic")!;

        static ServiceBusClient? client;
        static ServiceBusSender? sender;

        private readonly ILogger<HttpTrigger1> _logger;

        public HttpTrigger1(ILogger<HttpTrigger1> logger)
        {
            _logger = logger;
        }
        
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Function, 
            "get", "post")] HttpRequest request)
        {
            _logger.LogInformation("C# Http trigger function processed a request");

            WebhookEventDto webhookEvent = new WebhookEventDto(
                JsonConvert.SerializeObject(request.Headers), await new StreamReader(
                    request.Body).ReadToEndAsync());

            client = new ServiceBusClient(connection);
            sender = client.CreateSender(topic);

            using ServiceBusMessageBatch messageBatch = await 
                sender.CreateMessageBatchAsync();

            if (!messageBatch.TryAddMessage(new ServiceBusMessage(JsonConvert.SerializeObject(
                webhookEvent))))
            {
                throw new Exception("The message is too large to fit into the batch");
            }

            try
            {
                await sender.SendMessagesAsync(messageBatch);
                Console.WriteLine("A batch has been placed  onto the message bus topic");
            }
            finally
            {
                await sender.DisposeAsync();
                await client.DisposeAsync();
            }

            return new OkObjectResult("Welcome to Azure Function");
        }
    }
}
