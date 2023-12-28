using Newtonsoft.Json;
using TicketOnline.Models;

namespace TicketOnline.Services
{
    public class OrderingServices : BackgroundService
    {

        private readonly IServiceProvider services;



        private readonly string FilesDirectory = "Files";
        private readonly string JsonFileExist = Path.Combine("Files", "data.json");
        List<QrCode> existingJson = new List<QrCode>();
        public OrderingServices(IServiceProvider services)
        {
            if (System.IO.File.Exists(JsonFileExist))
            {
                // Read the existing JSON content
                var existingJsonContent = System.IO.File.ReadAllText(JsonFileExist);

                // Deserialize the existing JSON into a list of JsonModel
                existingJson = JsonConvert.DeserializeObject<List<QrCode>>(existingJsonContent);
              

            }
            this.services = services;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                // Run your logic here
                eckedEXpierDate();
                existingJson = OrdringQrCode(existingJson);

                // Wait for one week before running again

                await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);  // await Task.Delay(TimeSpan.FromDays(7), stoppingToken);
            }
        }

        public List<QrCode> OrdringQrCode(List<QrCode> qrCodes)
        {


            var orderedQrCodes = qrCodes.OrderBy(qrCode => DateTime.Parse(qrCode.DateََQrCode)).ToList();

            return orderedQrCodes;
        }


        public void eckedEXpierDate()
        {
            DateTime currentDate = DateTime.Now;

            DateTime newDate = currentDate.AddMinutes(-30);
            for (int i = existingJson.Count - 1; i >= 0; i--)
            {
                QrCode item = existingJson[i];
                if (DateTime.Parse(item.DateََQrCode) <= newDate)
                {
                    existingJson.RemoveAt(i);
                }
            }
            System.IO.File.WriteAllText(JsonFileExist, JsonConvert.SerializeObject(existingJson));


        }
    }
}
