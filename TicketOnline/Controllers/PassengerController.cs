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

                    string sql = "INSERT INTO Passenger (PassengerName, PassengerPhone, PassengerEmail, PassengerPassword)" +
                                 "VALUES (@PassengerName, @PassengerPhone, @PassengerEmail, @PassengerPassword);";

                    using (var command = new SqlCommand(sql, connection))
                    {
                        // Replace these lines with actual values from passengerDto
                        command.Parameters.AddWithValue("@PassengerName", passengerDto.PassengerName);
                        command.Parameters.AddWithValue("@PassengerPhone", passengerDto.PassengerPhone);
                        command.Parameters.AddWithValue("@PassengerEmail", passengerDto.PassengerEmail);
                        command.Parameters.AddWithValue("@PassengerPassword", passengerDto.PassengerPassword); // Hash the password for security

                        command.ExecuteNonQuery();
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
                                                                                   // Add other properties as needed

                                passengers.Add(passenger);
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

            return Ok(passengers);
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


    }
}
