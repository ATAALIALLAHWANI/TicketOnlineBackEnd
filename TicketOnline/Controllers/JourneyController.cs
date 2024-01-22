using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using TicketOnline.Models;

namespace TicketOnline.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class JourneyController : ControllerBase
    {

        private readonly string connectionString;

        public JourneyController(IConfiguration configuration)
        {
            connectionString = configuration.GetConnectionString("SqlServerDb") ?? "";
            Console.WriteLine($"ConnectionString: {connectionString}");
        }

        [HttpPost("AddJourney")]
        public IActionResult CreateJourney([FromBody] JourneyDto journeyDto)
        {
            try
            {
                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    string sql = "INSERT INTO Journey (RouteJourney, DepartuerJourney, DestinationJourney, DateJourney, NumberBus, BusID)" +
                                 "VALUES (@RouteJourney, @DepartuerJourney, @DestinationJourney, @DateJourney, @NumberBus, @BusID);";

                    using (var command = new SqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@RouteJourney", journeyDto.RouteJourney);
                        command.Parameters.AddWithValue("@DepartuerJourney", DateTime.Parse(journeyDto.DepartuerJourney));
                        command.Parameters.AddWithValue("@DestinationJourney", journeyDto.DestinationJourney);
                        command.Parameters.AddWithValue("@DateJourney", DateTime.Parse(journeyDto.DateJourney)); // Assuming you want to parse the string to DateTime
                        command.Parameters.AddWithValue("@NumberBus", journeyDto.NumberBus);
                        command.Parameters.AddWithValue("@BusID", journeyDto.BusID);

                        command.ExecuteNonQuery();
                        connection.Close();
                    }
                }

                return Ok("Journey created successfully");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("journey", $"Sorry, but we have an exception: {ex.Message}");
                return BadRequest(ModelState);
            }
        }

    [HttpGet("GetAllJourney")]
public IActionResult GetAllJoureny()
{
    try
    {
        using (var connection = new SqlConnection(connectionString))
        {
            connection.Open();

            string sql = "SELECT * FROM Journey;";

            using (var command = new SqlCommand(sql, connection))
            {
                using (var reader = command.ExecuteReader())
                {
                    List<Journey> journeys = new List<Journey>();

                    while (reader.Read())
                    {
                        Journey journey = new Journey
                        {
                            IdJourney = reader.GetInt32(0),
                            RouteJourney = reader.IsDBNull(1) ? null : reader.GetString(1),
                            DepartuerJourney = reader.GetTimeSpan(2).ToString(@"hh\:mm"),
                            DestinationJourney = reader.IsDBNull(3) ? null : reader.GetString(3),
                            DateJourney = reader.GetDateTime(4).ToString("yyyy-MM-dd"),
                            NumberBus = reader.GetInt32(5),
                            BusID = reader.GetInt32(6)
                        };

                        journeys.Add(journey);
                    }
                            connection.Close();
                    return Ok(journeys);
                }
            }
        }
    }
    catch (Exception ex)
    {
        ModelState.AddModelError("journey", $"Sorry, but we have an exception: {ex.Message}");
        return BadRequest(ModelState);
    }
}

        //[HttpPost("GetAllJourneyForDriver")]
        //public IActionResult GetAllJourneyForDriver([FromQuery] int driverId)
        //{
        //    try
        //    {
        //        using (var connection = new SqlConnection(connectionString))
        //        {
        //            connection.Open();

        //            string sql = "SELECT * FROM Journey WHERE DriverId = @DriverId;";

        //            using (var command = new SqlCommand(sql, connection))
        //            {
        //                command.Parameters.AddWithValue("@DriverId", driverId);

        //                using (var reader = command.ExecuteReader())
        //                {
        //                    List<Journey> journeys = new List<Journey>();

        //                    while (reader.Read())
        //                    {
        //                        Journey journey = new Journey
        //                        {
        //                            IdJourney = reader.GetInt32(0),
        //                            RouteJourney = reader.IsDBNull(1) ? null : reader.GetString(1),
        //                            DepartuerJourney = reader.GetTimeSpan(2).ToString(@"hh\:mm"),
        //                            DestinationJourney = reader.IsDBNull(3) ? null : reader.GetString(3),
        //                            DateJourney = reader.GetDateTime(4).ToString("yyyy-MM-dd"),
        //                            NumberBus = reader.GetInt32(5),
        //                            BusID = reader.GetInt32(6)
        //                        };

        //                        journeys.Add(journey);
        //                    }

        //                    connection.Close();
        //                    return Ok(journeys);
        //                }
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        ModelState.AddModelError("journey", $"Sorry, but we have an exception: {ex.Message}");
        //        return BadRequest(ModelState);
        //    }
        //}



        [HttpDelete("DeleteJourneyById")]
        public async Task<IActionResult> DeleteJourneyById([FromQuery] int id)
        {
            try
            {
                // Check if there are associated bookings
                if (HasAssociatedBookings(id))
                {
                    return BadRequest(new { Error = "Cannot delete journey with associated bookings" });
                }

                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    string sql = "DELETE FROM Journey WHERE IdJourney = @IdJourney;";

                    using (var command = new SqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@IdJourney", id);
                        int rowsAffected = await command.ExecuteNonQueryAsync();

                        if (rowsAffected > 0)
                        {
                            return Ok(new { Message = $"Journey with ID {id} deleted successfully" });
                        }
                        else
                        {
                            return NotFound(new { Error = $"Journey with ID {id} not found" });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Log the exception
                Console.WriteLine($"An error occurred: {ex.Message}");
                return BadRequest(new { Error = $"Failed to delete journey with ID {id}" });
            }
        }
        //helper 
        [ApiExplorerSettings(IgnoreApi = true)]
        private bool HasAssociatedBookings(int journeyId)
        {
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();

                string sql = "SELECT COUNT(*) FROM Booking WHERE JourneyId = @JourneyId;";

                using (var command = new SqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@JourneyId", journeyId);
                    int count = (int)command.ExecuteScalar();

                    // Return true if there are associated bookings, false otherwise
                    return count > 0;
                }
            }
        }

    }
}
