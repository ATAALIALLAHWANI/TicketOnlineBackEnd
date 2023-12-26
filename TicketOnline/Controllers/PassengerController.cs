using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System;
using TicketOnline.Models;

namespace TicketOnline.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PassengerController : ControllerBase
    {
        private readonly string connectionString;

        public PassengerController(IConfiguration configuration)
        {
            connectionString = configuration.GetConnectionString("SqlServerDb") ?? "";
            Console.WriteLine($"ConnectionString: {connectionString}");
        }

        [HttpPost("AddPassenger")]

        public IActionResult CreatePassenger([FromBody] PassengerDto passengerDto)
        {
            try
            {
                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    string sql = "INSERT INTO Passenger (PassengerName, PassengerPhone, PassengerEmail, PassengerPassword   )" +
                                 "VALUES (@PassengerName, @PassengerPhone, @PassengerEmail, @PassengerPassword   );";

                    using (var command = new SqlCommand(sql, connection))
                    {
                        // Replace these lines with actual values from passengerDto
                        command.Parameters.AddWithValue("@PassengerName", passengerDto.PassengerName);
                        command.Parameters.AddWithValue("@PassengerPhone", passengerDto.PassengerPhone);
                        command.Parameters.AddWithValue("@PassengerEmail", passengerDto.PassengerEmail);
                        command.Parameters.AddWithValue("@PassengerPassword", passengerDto.PassengerPassword); // Hash the password for security

                        command.ExecuteNonQuery();
                        connection.Close();
                    }
                }

                return Ok("Passenger created successfully");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("passenger", $"Sorry, but we have an exception: {ex.Message}");
                return BadRequest(ModelState);
            }
        }

        [HttpGet]
        [Route("GetPassengers")]
        public IActionResult GetPassengers()
        {
            List<Passenger> passengers = new List<Passenger>();

            try
            {
                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    string sql = "SELECT * FROM Passenger"; // Corrected the table name

                    using (var command = new SqlCommand(sql, connection))
                    {
                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                Passenger passenger = new Passenger();
                                passenger.IdPassenger = reader.GetInt32(0);
                                passenger.PassengerName = reader.GetString(1);
                                passenger.PassengerPhone = reader.GetString(2);
                                passenger.PassengerEmail = reader.GetString(3);
                                passenger.PassengerPassword = reader.GetString(4); // Assuming password is stored as a string, adapt accordingly
                                passenger.Blocked = reader.IsDBNull(5) ? 0 : reader.GetInt32(5);                                        // Add other properties as needed

                                passengers.Add(passenger);
                            }
                            connection.Close();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("passenger", $"Sorry, but we have an exception: {ex.Message}");
                return BadRequest(ModelState);
            }

            return Ok(passengers);
        }

        [HttpGet("AddBlocked")]
        public IActionResult AddBlocked(string phoneNumber)
        {
            try
            {
                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    string sql = "SELECT IdPassenger, blocked FROM Passenger WHERE PassengerPhone = @PhoneNumber;";

                    using (var command = new SqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@PhoneNumber", phoneNumber);

                        using (var reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                int idPassenger = reader.GetInt32(0);
                                int currentBlockedCount = reader.IsDBNull(1) ? 0 : reader.GetInt32(1);

                                // Increment the blocked count by 1
                                int newBlockedCount = currentBlockedCount + 1;

                                // Close the first using block
                                connection.Close();
                                connection.Open();

                                // Update the database with the new blocked count
                                string updateSql = "UPDATE Passenger SET blocked = @NewBlockedCount WHERE IdPassenger = @IdPassenger;";
                                using (var updateCommand = new SqlCommand(updateSql, connection))
                                {
                                    updateCommand.Parameters.AddWithValue("@NewBlockedCount", newBlockedCount);
                                    updateCommand.Parameters.AddWithValue("@IdPassenger", idPassenger);
                                    updateCommand.ExecuteNonQuery();
                                }

                                // Close the second using block
                                connection.Close();

                                return Ok("Blocked count updated successfully");
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("passenger", $"Sorry, but we have an exception: {ex.Message}");
                return BadRequest(ModelState);
            }

            return BadRequest("Passenger not found");
        }



        [HttpGet("CheckPassenger")]
        public IActionResult CheckPassenger([FromQuery] string email, [FromQuery] string password)
        {
            try
            {
                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    string sql = "SELECT COUNT(*) FROM Passenger WHERE PassengerEmail = @Email AND PassengerPassword = @Password";

                    using (var command = new SqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@Email", email);
                        command.Parameters.AddWithValue("@Password", password);

                        int count = (int)command.ExecuteScalar();
                        connection.Close();
                        return Ok(count > 0); // If count is greater than 0, it means the passenger exists
                    }
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("passenger", $"Sorry, but we have an exception: {ex.Message}");
                return BadRequest(ModelState);
            }
        }
        [HttpGet("CheckedPhonePassenger")]
        public IActionResult CheckedNamberPassenger([FromQuery] string phone)
        {
            try
            {
                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    string sql = "SELECT COUNT(*) FROM Passenger WHERE PassengerPhone = @Phone";

                    using (var command = new SqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@Phone", phone);

                        int count = (int)command.ExecuteScalar();
                        connection.Close();
                        return Ok(count > 0); // If count is greater than 0, it means the passenger exists

                    }
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("passenger", $"Sorry, but we have an exception: {ex.Message}");
                return BadRequest(ModelState);
            }
        }

        [HttpDelete("DeletePassengerById")]
        public IActionResult DeletePassengerById([FromQuery] int id)
        {
            try
            {
                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    // Check if the passenger with the given ID exists
                    string checkSql = "SELECT COUNT(*) FROM Passenger WHERE IdPassenger = @IdPassenger;";

                    using (var checkCommand = new SqlCommand(checkSql, connection))
                    {
                        checkCommand.Parameters.AddWithValue("@IdPassenger", id);

                        int passengerCount = (int)checkCommand.ExecuteScalar();

                        if (passengerCount == 0)
                        {
                            // Passenger with the given ID does not exist
                            return NotFound($"Passenger with ID {id} not found");
                        }
                    }

                    // If the passenger exists, proceed with deletion
                    string deleteSql = "DELETE FROM Passenger WHERE IdPassenger = @IdPassenger;";

                    using (var deleteCommand = new SqlCommand(deleteSql, connection))
                    {
                        deleteCommand.Parameters.AddWithValue("@IdPassenger", id);

                        int rowsAffected = deleteCommand.ExecuteNonQuery();

                        if (rowsAffected > 0)
                        {
                            // Passenger deleted successfully
                            return Ok($"Passenger with ID {id} deleted successfully");
                        }
                        else
                        {
                            // Deletion failed
                            return BadRequest("Unable to delete the passenger");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("passenger", $"Sorry, but we have an exception: {ex.Message}");
                return BadRequest(ModelState);
            }
        }


    }
}
