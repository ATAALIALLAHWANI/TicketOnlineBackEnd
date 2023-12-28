using Microsoft.Data.SqlClient;
using Newtonsoft.Json;
using TicketOnline.Models;

namespace TicketOnline.Services
{
    public class BlockedRemovalService : BackgroundService
    {

        private readonly IServiceProvider services;


        public BlockedRemovalService(IServiceProvider services)
        {
            
            this.services = services;
        }


        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                // Run your logic here
                RemoveBlockedPassengers();

                // Wait for one week before running again
                await Task.Delay(TimeSpan.FromDays(7), stoppingToken);  // await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);

            }
        }
       
      
     
        private void RemoveBlockedPassengers()
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

                        string sql = "UPDATE Passenger SET blocked = 0 WHERE blocked > 0;";

                        using (var command = new SqlCommand(sql, connection))
                        {
                            command.ExecuteNonQuery();
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

