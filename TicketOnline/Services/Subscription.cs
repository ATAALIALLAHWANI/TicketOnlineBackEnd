using Microsoft.Data.SqlClient;
using TicketOnline.Models;

namespace TicketOnline.Services
{
    public class Subscription : IServiceNot
    {
        private readonly string connectionString;
        public Subscription(IConfiguration configuration)
        {
            connectionString = configuration.GetConnectionString("SqlServerDb") ?? "";
            Console.WriteLine($"ConnectionString: {connectionString}");
        }
            public void Getbooking (string Message)
        {

            try
            {
                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    List<string> phoneNumbers = new List<string>();
                    string sql = "SELECT PhonePassenger FROM Booking;";

                    using (var command = new SqlCommand(sql, connection))
                    {
                        using (var reader = command.ExecuteReader())
                        {
                           

                            while (reader.Read())
                            {
                                string phoneNumber = reader.GetString(0);
                                phoneNumbers.Add(phoneNumber);
                            }

                        }
                    }


                    connection.Close();
                           
                    
                        foreach (string number in phoneNumbers  ){ 

                        sendMessage(Message, number);  

                    }
                    
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine("Eerror get booking in services ");               
            }
        }


        private string sendMessage (string Message , string Number)
        {
            return Message + "is sent to : " + Number; 
        }

        public Journey GetJourneyById(int id)
        {
            try
            {
                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    string sql = "SELECT * FROM Journey WHERE IdJourney = @IdJourney;";

                    using (var command = new SqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@IdJourney", id);

                        using (var reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                return new Journey
                                {
                                    IdJourney = reader.GetInt32(0),
                                    RouteJourney = reader.IsDBNull(1) ? null : reader.GetString(1),
                                    DepartuerJourney = reader.GetTimeSpan(2).ToString(@"hh\:mm"),
                                    DestinationJourney = reader.IsDBNull(3) ? null : reader.GetString(3),
                                    DateJourney = reader.GetDateTime(4).ToString("yyyy-MM-dd"),
                                    NumberBus = reader.GetInt32(5),
                                    BusID = reader.GetInt32(6)
                                };

                            }

                        }
                    }

                    connection.Close();
                }
            }
            catch (Exception ex)
            {
                // Handle exception, e.g., log it or throw a custom exception
                Console.WriteLine($"An error occurred while fetching Journey by ID: {ex.Message}");
            }

            return null; // Return null if Journey with the specified ID is not found
        }


        public void Update(string Message)
        {
            throw new NotImplementedException();
        }


    }
}
