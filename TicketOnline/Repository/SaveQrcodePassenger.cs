using Microsoft.Data.SqlClient;

namespace TicketOnline.Repository
{
    public class SaveQrcodePassenger
    {
        private readonly string connectionString;

        public SaveQrcodePassenger(string connectionString)
        {
            this.connectionString = connectionString;
        }

        public void SavePassangerQrcodeToDatabase(string PassengerPhone, string PassQrCode)
        {
            try
            {
                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    string sql = "INSERT INTO PassengerQrCode (PassengerPhone, PassQrCode) " +
                                 "VALUES (@PassengerPhone, @PassQrCode);";

                    using (var command = new SqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@PassengerPhone", PassengerPhone);
                        command.Parameters.AddWithValue("@PassQrCode", PassQrCode);

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
    }

}
