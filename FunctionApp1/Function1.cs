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
using TicketOnline.Models;

namespace FunctionApp1
{
    public static class Function1
    {
        [FunctionName("Function1")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req,
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
                        ApplicationName = "FunctionApp1"
                    });

                Database database = cosmosClient.GetDatabase(databaseId);
                Container container = database.GetContainer("Container1");
                var ob = new helloTest(Guid.NewGuid().ToString(), "hello");
                var result = await container.CreateItemAsync<helloTest>(ob, new PartitionKey(ob.id));
                return new OkObjectResult($"created item id:{ob.id}");
            }
            catch (Exception ex)
            {
                return new BadRequestObjectResult(ex.Message);
            }
        }
    }
}
