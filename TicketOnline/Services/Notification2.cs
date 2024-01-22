using Microsoft.Azure.Cosmos;
using Microsoft.Data.SqlClient;
using TicketOnline.Models;
using TicketOnline.Repository;

namespace TicketOnline.Services
{
    public class Notification2 : BackgroundService
    {
        private readonly IServiceProvider services;



        public Notification2(IServiceProvider services)
        {

            this.services = services;

        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                // Run your logic here
                addNotification();

                // Wait for one week before running again
                await Task.Delay(TimeSpan.FromMinutes(15), stoppingToken);  // await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);

            }
        }


        public void SaveNotificationToDatabase(string message)
        {
            try
            {
                using (var scope = services.CreateScope())
                {
                    var serviceProvider = scope.ServiceProvider;
                    var connectionString = serviceProvider.GetRequiredService<IConfiguration>().GetConnectionString("SqlServerDb");

                    // Implement your logic to remove blocked passengers
                    using (var connection = new SqlConnection(connectionString))
                    {
                        connection.Open();

                        string sql = "INSERT INTO Notifications2 ( Message, CreatedAt) " +
                                     "VALUES (@Message, @CreatedAt);";

                        using (var command = new SqlCommand(sql, connection))
                        {
                            command.Parameters.AddWithValue("@Message", message);
                            command.Parameters.AddWithValue("@CreatedAt", DateTime.Now);

                            command.ExecuteNonQuery();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Handle exception, e.g., log it or throw a custom exception
                Console.WriteLine($"An error occurred while saving notification to the database: {ex.Message}");
            }
        }
        // Other methods related to notification management can be added here
    

    private async void addNotification()
    {
        try
        {

            DateTime currentDate = DateTime.Now;
            DateTime newDate = currentDate.AddMinutes(15);

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

                    if (DateTime.TryParse(qrCode.DateQrCode, out DateTime itemDate) && itemDate <= newDate)
                    {
                            SaveNotificationToDatabase("the date of journey : " + itemDate  + " the bus will be go after 15 minutes");
                    }
                }


            }

        }
        catch (Exception ex)
        {
            // Handle exceptions as needed
            Console.WriteLine($"An error occurred while removing blocked passengers: {ex.Message}");
        }

    }
}

}

