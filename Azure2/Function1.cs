using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Microsoft.DotNet.PlatformAbstractions;
using System.Linq;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http.Internal;
using System.Resources;
using System.Reflection;
using Microsoft.Azure.Cosmos;

namespace Azure2
{
    public class QrCode
    {
        [JsonProperty("id")] // This attribute is important for Cosmos DB
        public string IdQrcode { get; set; }

        // Other properties
        public string DateQrCode { get; set; }
        public string DateExpierDate { get; set; }
        public int IdScanner { get; set; }
        public List<string> QrCodeList { get; set; }
    }

    public static class Function1
    {
        [FunctionName("Function1")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            try
            {
              
                CosmosClient cosmosClient = new CosmosClient(EndpointUri, PrimaryKey,
                       new CosmosClientOptions()
                       {
                           ConnectionMode = ConnectionMode.Direct,
                           ApplicationName = "Azure2"
                       });
                Database database = cosmosClient.GetDatabase(databaseId);
                Container container = database.GetContainer("Container1");
                var query = new QueryDefinition("SELECT * FROM c");

                var items = new List<dynamic>(); // Use strongly-typed list

                FeedIterator<dynamic> resultSetIterator = container.GetItemQueryIterator<dynamic>(query);
                while (resultSetIterator.HasMoreResults)
                {
                    FeedResponse<dynamic> response = await resultSetIterator.ReadNextAsync();
                    items.AddRange(response);
                }

                if (items.Any())
                {
                    foreach (var item in items)
                    {
                        var qrCode = Newtonsoft.Json.JsonConvert.DeserializeObject<QrCode>(item.ToString());
                        // Convert req.Query["id"] to an integer
                        if (int.TryParse(req.Query["id"], out int scannerId) &&
                            qrCode.QrCodeList.Contains(req.Query["code"]) && qrCode.IdScanner == scannerId)
                        {
                            qrCode.QrCodeList.Remove(req.Query["code"]);
                            var response = await container.ReplaceItemAsync(qrCode, qrCode.IdQrcode, new PartitionKey(qrCode.IdQrcode));

                            return new OkObjectResult("ok");
                        }
                    }
                }

                return new OkObjectResult("no");
            }
            catch (Exception ex)
            {
                return new BadRequestObjectResult(ex.Message);
            }
        }
    }
}
