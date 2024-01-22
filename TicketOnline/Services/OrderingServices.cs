using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TicketOnline.Models;

namespace TicketOnline.Services
{
    public class OrderingServices : BackgroundService
    {
        private readonly string FilesDirectory = "Files";
        private readonly string JsonFileExist = Path.Combine("Files", "data.json");
        private  List<QrCode> existingJson;

        public OrderingServices()
        {
            if (File.Exists(JsonFileExist))
            {
                // Read the existing JSON content
                var existingJsonContent = File.ReadAllText(JsonFileExist);

                // Deserialize the existing JSON into a list of QrCode
                existingJson = JsonConvert.DeserializeObject<List<QrCode>>(existingJsonContent) ?? new List<QrCode>();
            }
            else
            {
                existingJson = new List<QrCode>();
            }
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                // Run your logic here
                CheckExpireDate();
                 

                // Wait for one minute before running again
                await Task.Delay(TimeSpan.FromDays(7), stoppingToken);
            }
        }

        //public void OrdringQrCode()
        //{
        //    try
        //    {
        //        var orderedQrCodes = existingJson.OrderBy(qrCode => DateTime.Parse(qrCode.DateََQrCode)).ToList();
        //        File.WriteAllText(JsonFileExist, JsonConvert.SerializeObject(orderedQrCodes));
        //    }
        //    catch (Exception ex)
        //    {
        //        // Handle or log the exception
        //        Console.WriteLine($"An error occurred while ordering QR codes: {ex.Message}");
        //    }
        //}


        //public bool CheckQrcode(string qrCode, int IdScanner)
        //{
        //    bool result = false;

        //    if (existingJson != null && existingJson.Count > 0)
        //    {
        //        for (int i = 0; i < existingJson.Count; i++)
        //        {
        //            if (existingJson[i].QrCodeList.Contains(qrCode) && existingJson[i].IdScanner == IdScanner)
        //            {

        //                result = true;
        //                existingJson[i].QrCodeList.Remove(qrCode);
        //                System.IO.File.WriteAllText(JsonFileExist, JsonConvert.SerializeObject(existingJson));
        //                break; // You can exit the loop early since the condition is met
        //            }
        //        }
        //    }

        //    return result;
        //}


        public async void CheckExpireDate()
        {
            try
            {
                DateTime currentDate = DateTime.Now;
                DateTime newDate = currentDate.AddMinutes(-30);

                // Ensure synchronization when reading and writing to existingJson


                string EndpointUri = "https://ticketonlinedatabase.documents.azure.com:443/";
                string PrimaryKey = "ns1F6yzlZvWrCQ7ADj3qWIKIiKiMYjVYN0tdVgmo01wBw1IV5Z7hQDE5cjNf83n8cHZ02QR06QakACDbn1auNQ==";
                string databaseId = "ContainerDataBase";
                CosmosClient cosmosClient = new CosmosClient(EndpointUri, PrimaryKey,
                       new CosmosClientOptions()
                       {
                           ConnectionMode = ConnectionMode.Direct,
                           ApplicationName = "Azure2"
                       });
                Database database = cosmosClient.GetDatabase(databaseId);
                Container container = database.GetContainer("Container1");
                var query = new QueryDefinition("SELECT * FROM c");

                var items = new List<dynamic>();
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
                        // Deserialize the item into a QrCode object
                        var qrCode = Newtonsoft.Json.JsonConvert.DeserializeObject<QrCode>(item.ToString());

                        if(DateTime.TryParse(qrCode.DateQrCode, out DateTime itemDate) && itemDate <= newDate)
                        {
                            await container.DeleteItemAsync<QrCode>(qrCode.IdQrcode, new PartitionKey(qrCode.IdQrcode));

                        }
                    }


                }


                //    lock (existingJson)
                //{
                //    for (int i = existingJson.Count - 1; i >= 0; i--)
                //    {
                //        QrCode item = existingJson[i];

                //        // Ensure that the date parsing doesn't throw an exception
                //        if (DateTime.TryParse(item.DateQrCode, out DateTime itemDate) && itemDate <= newDate)
                //        {
                //            existingJson.RemoveAt(i);
                //        }
                //    }

                //    // Write back the modified list to the file
                //    File.WriteAllText(JsonFileExist, JsonConvert.SerializeObject(existingJson));
                //}

            }
            catch (Exception ex)
            {
                // Handle or log the exception
                Console.WriteLine($"An error occurred while checking and updating expiration dates: {ex.Message}");
            }
        }

    }
}
