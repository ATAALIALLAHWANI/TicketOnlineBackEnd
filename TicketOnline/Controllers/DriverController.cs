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

        [HttpGet("CheckDriver")]
        public IActionResult CheckDriver([FromQuery] string email, [FromQuery] string password)
        {
            try
            {
                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    string sql = "SELECT COUNT(*) FROM Driver WHERE emaildriver = @Email AND PasswordDriver = @Password";

                    using (var command = new SqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@Email", email);
                        command.Parameters.AddWithValue("@Password", password);

                        int count = (int)command.ExecuteScalar();
                        connection.Close();
                        return Ok(count > 0); // If count is greater than 0, it means the admin exists
                    }
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("Check driver", $"Sorry, but we have an exception: {ex.Message}");
                return BadRequest(ModelState);
            }

        }

        [HttpPost("GetDriverUsingPass")]
        public IActionResult GetDriverUsingPass([FromQuery] string email, [FromQuery] string password)
        {
            try
            {
                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    string sql = "SELECT * FROM Driver WHERE EmailDriver = @EmailDriver AND PasswordDriver = @PasswordDriver;";

                    using (var command = new SqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@EmailDriver", email);
                        command.Parameters.AddWithValue("@PasswordDriver", password);

                        using (var reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                Driver driver = new Driver
                                {
                                    IdDriver = reader.GetInt32(0),
                                    NameDriver = reader.IsDBNull(1) ? null : reader.GetString(1),
                                    EmailDriver = reader.IsDBNull(2) ? null : reader.GetString(2),
                                    PasswordDriver = reader.IsDBNull(3) ? null : reader.GetString(3)
                                };

                                connection.Close();
                                return Ok(driver);
                            }
                            else
                            {
                                connection.Close();
                                return NotFound(new { Error = "Driver not found with the provided email and password" });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Log the exception
                Console.WriteLine($"An error occurred: {ex.Message}");
                return BadRequest(new { Error = "Failed to retrieve driver" });
            }
        }

        [HttpPost("GetAllJourneyDriver")]
        public IActionResult GetAllJourneyDriver(int idDriver)
        {
            try
            {
                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    // Fetch all buses related to the given driver
                    string busQuery = "SELECT IdBus FROM Bus WHERE IdDriver = @IdDriver;";
                    List<int> busIds = new List<int>();

                    using (var busCommand = new SqlCommand(busQuery, connection))
                    {
                        busCommand.Parameters.AddWithValue("@IdDriver", idDriver);

                        using (var busReader = busCommand.ExecuteReader())
                        {
                            while (busReader.Read())
                            {
                                busIds.Add(busReader.GetInt32(0));
                            }
                        }
                    }

                    // Fetch all journeys related to the buses
                    List<Journey> journeys = new List<Journey>();

                    foreach (int busId in busIds)
                    {
                        string journeyQuery = "SELECT * FROM Journey WHERE BusID = @BusID;";

                        using (var journeyCommand = new SqlCommand(journeyQuery, connection))
                        {
                            journeyCommand.Parameters.AddWithValue("@BusID", busId);

                            using (var journeyReader = journeyCommand.ExecuteReader())
                            {
                                while (journeyReader.Read())
                                {
                                    Journey journey = new Journey
                                    {
                                        IdJourney = journeyReader.GetInt32(0),
                                        RouteJourney = journeyReader.IsDBNull(1) ? null : journeyReader.GetString(1),
                                        DepartuerJourney = journeyReader.GetTimeSpan(2).ToString(@"hh\:mm"),
                                        DestinationJourney = journeyReader.IsDBNull(3) ? null : journeyReader.GetString(3),
                                        DateJourney = journeyReader.GetDateTime(4).ToString("yyyy-MM-dd"),
                                        NumberBus = journeyReader.GetInt32(5),
                                        BusID = journeyReader.GetInt32(6)
                                    };

                                    journeys.Add(journey);
                                }
                            }
                        }
                    }

                    // Close the connection after all queries
                    connection.Close();

                    return Ok(journeys);
                }
            }
            catch (Exception ex)
            {
                // Log the exception
                Console.WriteLine($"An error occurred: {ex.Message}");
                return BadRequest(new { Error = "Failed to retrieve journeys for the given driver" });
            }
        }


    }
}
