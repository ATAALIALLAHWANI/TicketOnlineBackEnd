using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using TicketOnline.Models;

namespace TicketOnline.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BookingController : ControllerBase
    {
        private readonly string connectionString;

        public BookingController(IConfiguration configuration)
        {
            connectionString = configuration.GetConnectionString("SqlServerDb") ?? "";
            Console.WriteLine($"ConnectionString: {connectionString}");
        }

        [HttpPost("AddBooking")]
        public IActionResult AddBooking([FromBody] BookingDto bookingDto)
        {
            try
            {
                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    string sql = "INSERT INTO Booking (DateBook, StatusBooking, PhonePassenger, JourneyId)" +
                                 "VALUES (@DateBook, @StatusBooking, @PhonePassenger, @JourneyId);";

                    using (var command = new SqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@DateBook", DateTime.Parse(bookingDto.DateBook)); // Assuming you want to parse the string to DateTime
                        command.Parameters.AddWithValue("@StatusBooking", bookingDto.StatusBooking);
                        command.Parameters.AddWithValue("@PhonePassenger", bookingDto.PhonePassenger);

                        // Set JourneyId based on the selected Journey
                        command.Parameters.AddWithValue("@JourneyId", bookingDto.JourneyoBo.IdJourney);

                        command.ExecuteNonQuery();
                        connection.Close();
                    }
                }

                return Ok("Booking created successfully");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("booking", $"Sorry, but we have an exception: {ex.Message}");
                return BadRequest(ModelState);
            }
        }

        [HttpGet("GetAllBooking")]
        public IActionResult GetAllBooking()
        {
            try
            {
                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    string sql = "SELECT * FROM Booking;";

                    using (var command = new SqlCommand(sql, connection))
                    {
                        using (var reader = command.ExecuteReader())
                        {
                            List<Booking> bookings = new List<Booking>();

                            while (reader.Read())
                            {
                                Booking booking = new Booking
                                {
                                    IdBooking = reader.GetInt32(0),
                                    DateBook = reader.GetDateTime(1).ToString("yyyy-MM-dd"), // Assuming DateBook is DateTime in the database
                                    StatusBooking = reader.GetString(2),
                                    PhonePassenger = reader.GetString(3),
                                    // Assuming JourneyId is the fourth column
                                    JourneyoBo = GetJourneyById(reader.GetInt32(4))
                                };

                                bookings.Add(booking);
                            }
                            connection.Close();
                            return Ok(bookings);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("booking", $"Sorry, but we have an exception: {ex.Message}");
                return BadRequest(ModelState);
            }
        }

        [ApiExplorerSettings(IgnoreApi = true)]
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
                                    DepartuerJourney = reader.IsDBNull(2) ? null : reader.GetString(2),
                                    DestinationJourney = reader.IsDBNull(3) ? null : reader.GetString(3),
                                    DateJourney = reader.IsDBNull(4) ? null : reader.GetDateTime(4).ToString("yyyy-MM-dd"),
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


    }
}
