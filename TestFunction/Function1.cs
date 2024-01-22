using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Microsoft.Azure.Cosmos;
using TicketOnline.Services;

namespace TestFunction
{
    
    public static class Function1
    {
        [FunctionName("fun")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = "test")] HttpRequest req,
            ILogger log)
        {
            try
            {
                string EndpointUri = "https://ticketonlinedatabase.documents.azure.com:443/";
                string PrimaryKey = "ns1F6yzlZvWrCQ7ADj3qWIKIiKiMYjVYN0tdVgmo01wBw1IV5Z7hQDE5cjNf83n8cHZ02QR06QakACDbn1auNQ==";
                string databaseId = "ContainerDataBase";

                CosmosClient cosmosClient = new CosmosClient(EndpointUri, PrimaryKey,
                    new CosmosClientOptions()
                    {
                        ConnectionMode = ConnectionMode.Direct,
                        ApplicationName = "TestFunction"
                    });

                // Uncomment and complete the code for Cosmos DB connection
                // Database database = cosmosClient.GetDatabase(databaseId);
                // Container container = database.GetContainer("Container1");

                OrderingServices orderingServices = new OrderingServices();

                if (req.Query.TryGetValue("qrcode", out var qrcode) && req.Query.TryGetValue("id", out var idString) && int.TryParse(idString, out int id))
                {
                    bool result2 = orderingServices.CheckQrcode(qrcode, id);
                    return new OkObjectResult($"The result check is: {result2}");
                }

                return new BadRequestObjectResult("Invalid or missing query parameters.");
            }
            catch (Exception ex)
            {
                return new BadRequestObjectResult(ex.Message);
            }

            //log.LogInformation("C# HTTP trigger function processed a request.");

            //string name = req.Query["name"];

            //string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            //dynamic data = JsonConvert.DeserializeObject(requestBody);
            //name = name ?? data?.name;

            //string responseMessage = string.IsNullOrEmpty(name)
            //    ? "This HTTP triggered function executed successfully. Pass a name in the query string or in the request body for a personalized response."
            //    : $"Hello, {name}. This HTTP triggered function executed successfully.";

            //return new OkObjectResult(responseMessage);
        }
     

    }
}
