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
    public static class GetPassScanner
    {
        [FunctionName("GetPassScanner")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {

            log.LogInformation("C# HTTP trigger function processed a request.");
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

            var container = database.GetContainer("Container1");
            PassengerQrCode Obj1 = new PassengerQrCode();
            Obj1.Id = 1;
            Obj1.PassengerPhone = "007";
            Obj1.PassQrCode.Add("ddddd");

            var result = await container.CreateItemAsync<PassengerQrCode>(Obj1, new PartitionKey(Obj1.Id));




            return new OkObjectResult(result.StatusCode);


        }
    }
}
