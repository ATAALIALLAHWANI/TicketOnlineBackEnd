using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using TicketOnline.Models;
using Microsoft.Extensions.Configuration;
using System;

namespace TicketOnline.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DriverController : ControllerBase
    {

        private readonly string connectionString;

        public DriverController(IConfiguration configuration)
        {
            connectionString = configuration.GetConnectionString("SqlServerDb") ?? "";
            Console.WriteLine($"ConnectionString: {connectionString}");
        }

        [HttpPost("AddDriver")]
        public IActionResult CreateDriver([FromBody] DriverDto driverDto)
        {
            try
            {
                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    string sql = "INSERT INTO Driver (NameDriver, EmailDriver, PasswordDriver)" +
                                 "VALUES (@NameDriver, @EmailDriver, @PasswordDriver);";

                    using (var command = new SqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@NameDriver", driverDto.NameDriver);
                        command.Parameters.AddWithValue("@EmailDriver", driverDto.EmailDriver);
                        command.Parameters.AddWithValue("@PasswordDriver", driverDto.PasswordDriver); // Hash the password for security

                        command.ExecuteNonQuery();
                        connection.Close();
                    }
                }

                return Ok("Driver created successfully");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("driver", $"Sorry, but we have an exception: {ex.Message}");
                return BadRequest(ModelState);
            }
        }


        [HttpGet("GetDriver")]
        public IActionResult GetDriver()
        {
            try
            {
                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    string sql = "SELECT * FROM Driver";

                    using (var command = new SqlCommand(sql, connection))
                    {
                        using (var reader = command.ExecuteReader())
                        {
                            List<Driver> drivers = new List<Driver>();

                            while (reader.Read())
                            {
                                Driver driver = new Driver
                                {
                                    IdDriver = reader.GetInt32(0),
                                    NameDriver = reader.GetString(1),
                                    EmailDriver = reader.GetString(2),
                                    PasswordDriver = reader.GetString(3) // Assuming password is stored as a string, adapt accordingly
                                                                         // Add other properties as needed
                                };

                                drivers.Add(driver);
                            }
                            connection.Close();
                            return Ok(drivers);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("driver", $"Sorry, but we have an exception: {ex.Message}");
                return BadRequest(ModelState);
            }
        }


    }
}
