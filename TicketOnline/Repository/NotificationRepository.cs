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

        public void SaveNotificationToDatabase(int passengerId, string message)
        {
            try
            {
                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    string sql = "INSERT INTO Notifications (PassengerId, Message, CreatedAt) " +
                                 "VALUES (@PassengerId, @Message, @CreatedAt);";

                    using (var command = new SqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@PassengerId", passengerId);
                        command.Parameters.AddWithValue("@Message", message);
                        command.Parameters.AddWithValue("@CreatedAt", DateTime.Now);

                        command.ExecuteNonQuery();
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
    }
}
