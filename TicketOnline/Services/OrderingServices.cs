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
        private readonly List<QrCode> existingJson;

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
                await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
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

        public void CheckExpireDate()
        {
            try
            {
                DateTime currentDate = DateTime.Now;
                DateTime newDate = currentDate.AddMinutes(-30);

                // Ensure synchronization when reading and writing to existingJson
                lock (existingJson)
                {
                    for (int i = existingJson.Count - 1; i >= 0; i--)
                    {
                        QrCode item = existingJson[i];

                        // Ensure that the date parsing doesn't throw an exception
                        if (DateTime.TryParse(item.DateََQrCode, out DateTime itemDate) && itemDate <= newDate)
                        {
                            existingJson.RemoveAt(i);
                        }
                    }

                    // Write back the modified list to the file
                    File.WriteAllText(JsonFileExist, JsonConvert.SerializeObject(existingJson));
                }
            }
            catch (Exception ex)
            {
                // Handle or log the exception
                Console.WriteLine($"An error occurred while checking and updating expiration dates: {ex.Message}");
            }
        }

    }
}
