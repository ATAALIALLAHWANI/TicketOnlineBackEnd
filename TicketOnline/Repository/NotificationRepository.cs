using Microsoft.Data.SqlClient;
using System;
using System.Data;

namespace TicketOnline.Repository
{
    public class NotificationRepository
    {
        private readonly string connectionString;

        public NotificationRepository(string connectionString)
        {
            this.connectionString = connectionString;
        }


        public List<string> GetAllNotifications()
        {
            List<string> messages = new List<string>();

            try
            {
                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    string sql = "SELECT Message FROM Notifications2 ORDER BY CreatedAt DESC;";

                    using (var command = new SqlCommand(sql, connection))
                    {
                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                messages.Add(reader.GetString(0));
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Handle exception, e.g., log it or throw a custom exception
                Console.WriteLine($"An error occurred while retrieving notifications from the database: {ex.Message}");
            }

            return messages;
        }





        //    public void SaveNotificationToDatabase( string message)
        //    {
        //        try
        //        {
        //            using (var connection = new SqlConnection(connectionString))
        //            {
        //                connection.Open();

        //                string sql = "INSERT INTO Notifications ( Message, CreatedAt) " +
        //                             "VALUES (@Message, @CreatedAt);";

        //                using (var command = new SqlCommand(sql, connection))
        //                {
        //                    command.Parameters.AddWithValue("@Message", message);
        //                    command.Parameters.AddWithValue("@CreatedAt", DateTime.Now);

        //                    command.ExecuteNonQuery();
        //                }
        //            }
        //        }
        //        catch (Exception ex)
        //        {
        //            // Handle exception, e.g., log it or throw a custom exception
        //            Console.WriteLine($"An error occurred while saving notification to the database: {ex.Message}");
        //        }
        //    }
        //    // Other methods related to notification management can be added here
        //}
    }
}
