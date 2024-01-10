using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using TicketOnline.Models;
using System.Security.Cryptography;
using System.Text;
using Newtonsoft.Json;
using TicketOnline.Repository;

namespace TicketOnline.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BookingController : ControllerBase
    {
        private readonly string connectionString;
         SaveQrcodePassenger saveQrcodePassenger;
        private readonly string FilesDirectory = "Files";
        private readonly string JsonFileExist = Path.Combine("Files", "data.json");
        List<QrCode> existingJson = new List<QrCode>();
       
        public BookingController(IConfiguration configuration)
        {
            connectionString = configuration.GetConnectionString("SqlServerDb") ?? "";
            Console.WriteLine($"ConnectionString: {connectionString}");
            saveQrcodePassenger = new SaveQrcodePassenger(connectionString);

            if (System.IO.File.Exists(JsonFileExist))
            {
                // Read the existing JSON content
                var existingJsonContent = System.IO.File.ReadAllText(JsonFileExist);

                existingJson = JsonConvert.DeserializeObject<List<QrCode>>(existingJsonContent);

            }

        }

        [HttpPost("AddBooking")]
        public IActionResult AddBooking([FromBody] BookingDto bookingDto)
        {
            try
            {
                DateTime currentDate = DateTime.Now;

                DateTime newDate = currentDate.AddMinutes(10);
                 string DateJourney = bookingDto.JourneyoBo.DateJourney + " " + bookingDto.JourneyoBo.DepartuerJourney;
                if (GetBlocked(bookingDto.PhonePassenger) >= 3) return Ok("this passenger is blocked ");

                if (DateTime.Parse(DateJourney) <= newDate) return BadRequest("this booking is not true the date the journy is end cannot resrvation this booking  ");


                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    string sql = "INSERT INTO Booking (DateBook, StatusBooking,TickitPrice , SeatsBooking ,PhonePassenger, JourneyId)" +
                                 "VALUES (@DateBook, @StatusBooking  , @TickitPrice ,@SeatsBooking , @PhonePassenger, @JourneyId);";

                    using (var command = new SqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@DateBook", DateTime.Now.ToString("yyyy-MM-dd hh:mm"));
                        Console.WriteLine($"Formatted Date: {DateTime.Now.ToString("yyyy-MM-dd hh:mm")}");
                        // Assuming you want to parse the string to DateTime
                        command.Parameters.AddWithValue("@StatusBooking", bookingDto.StatusBooking);
                        command.Parameters.AddWithValue("@TickitPrice", bookingDto.TickitPrice);
                        command.Parameters.AddWithValue("@SeatsBooking", bookingDto.SeatsBooking);

                        command.Parameters.AddWithValue("@PhonePassenger", bookingDto.PhonePassenger);

                        // Set JourneyId based on the selected Journey
                        command.Parameters.AddWithValue("@JourneyId", bookingDto.JourneyoBo.IdJourney);

                        command.ExecuteNonQuery();
                        connection.Close();
                    }
                }

                return Ok("successfully create Booking ");
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
                                    DateBook = reader.GetDateTime(1).ToString("yyyy-MM-dd HH:mm"), // Use HH for 24-hour format
                                    StatusBooking = reader.GetString(2),
                                    TickitPrice = reader.IsDBNull(3) ? 0 : reader.GetDouble(3),
                                    SeatsBooking = reader.IsDBNull(4) ? 0 : reader.GetInt32(4),
                                    PhonePassenger = reader.GetString(5),
                                    // Assuming JourneyId is the fourth column
                                    JourneyoBo = GetJourneyById(reader.GetInt32(6))
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




        [HttpGet("GetBookingNotPay")]
        public IActionResult GetBookingNotPay()
        {
            try
            {
                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    string sql = "SELECT * FROM Booking WHERE StatusBooking = 'not pay';";

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
                                    DateBook = reader.GetDateTime(1).ToString("yyyy-MM-dd HH:mm"), // Use HH for 24-hour format
                                    StatusBooking = reader.GetString(2),
                                    TickitPrice = reader.IsDBNull(3) ? 0 : reader.GetDouble(3),
                                    SeatsBooking = reader.IsDBNull(4) ? 0 : reader.GetInt32(4),
                                    PhonePassenger = reader.GetString(5),
                                    // Assuming JourneyId is the fourth column
                                    JourneyoBo = GetJourneyById(reader.GetInt32(6))
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

        [HttpPost("CheckQeCode")]
        public bool CheckQrcode([FromQuery] string qrCode, int IdScanner)
        {
            bool result = false;

            if (existingJson != null && existingJson.Count > 0)
            {
                for (int i = 0; i < existingJson.Count; i++)
                {
                    if (existingJson[i].QrCodeList.Contains(qrCode) && existingJson[i].IdScanner == IdScanner )
                    {
                      
                        result = true;
                        existingJson[i].QrCodeList.Remove(qrCode);
                        System.IO.File.WriteAllText(JsonFileExist, JsonConvert.SerializeObject(existingJson));
                        break; // You can exit the loop early since the condition is met
                    }
                }
            }

            return result;
        }

        [HttpGet("AcceptPay")]
        public IActionResult AcceptPay([FromQuery] string phoneNumber)
        {
            try
            {
                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    // Check if a booking with the given phone number exists
                    string bookingSql = "SELECT IdBooking FROM Booking WHERE PhonePassenger = @PhoneNumber AND StatusBooking = 'not pay';";
                    using (var bookingCommand = new SqlCommand(bookingSql, connection))
                    {
                        bookingCommand.Parameters.AddWithValue("@PhoneNumber", phoneNumber);

                        using (var bookingReader = bookingCommand.ExecuteReader())
                        {
                            if (bookingReader.Read())
                            {
                                int bookingId = bookingReader.GetInt32(0);
                                connection.Close();
                                connection.Open();
                                // Update the booking status to "pay"
                                string updateBookingSql = "UPDATE Booking SET StatusBooking = 'pay' WHERE IdBooking = @BookingId;";
                                using (var updateBookingCommand = new SqlCommand(updateBookingSql, connection))
                                {
                                    updateBookingCommand.Parameters.AddWithValue("@BookingId", bookingId);
                                    updateBookingCommand.ExecuteNonQuery();
                                }

                                


                                // Retrieve the booking details
                                Booking bookingDto = GetBookingId(bookingId);

                                if (bookingDto != null)
                                {
                                    string dateJourney = bookingDto.JourneyoBo.DateJourney + " " + bookingDto.JourneyoBo.DepartuerJourney;
                                    QrCode jsonContent = new QrCode
                                    {
                                        IdQrcode = 1,
                                        DateََQrCode = dateJourney,
                                        DateExpierDate = bookingDto.JourneyoBo.DepartuerJourney,
                                        IdScanner = GetIdScanner(bookingDto.JourneyoBo.BusID),
                                        QrCodeList = new List<string>()
                                    };

                                    if (System.IO.File.Exists(JsonFileExist))
                                    {
                                        string symbol = GenerateUniqueValue(bookingDto.PhonePassenger);

                                        bool flag = false;

                                        // Ensure synchronization when reading and writing to existingJson
                                        lock (existingJson)
                                        {
                                            foreach (var existingEntry in existingJson)
                                            {
                                                if (existingEntry.DateََQrCode.Equals(dateJourney))
                                                {
                                                    existingEntry.QrCodeList.Add(symbol);
                                                    flag = true;
                                                    break;
                                                }
                                            }

                                            if (!flag)
                                            {
                                                jsonContent.QrCodeList.Add(symbol);
                                                existingJson.Add(jsonContent);
                                            }
                                            saveQrcodePassenger.SavePassangerQrcodeToDatabase(phoneNumber, symbol);
                                            // Write back the modified list to the file
                                            System.IO.File.WriteAllText(JsonFileExist, JsonConvert.SerializeObject(existingJson));
                                        }

                                        Console.WriteLine("File created successfully");
                                        connection.Close();
                                        return Ok("ok is don payment");
                                    }
                                }
                            }
                        }
                    }
                }

                return BadRequest("No pending booking found for the given phone number");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("booking", $"Sorry, but we have an exception: {ex.Message}");
                return BadRequest(ModelState);
            }
        }

        [HttpGet("GetAllPassengerQrcode")]
        public IActionResult GetAllPassengerQrcode([FromQuery] string PhoneNumber)
        {
            try
            {
                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    string sql = "SELECT PassQrCode FROM PassengerQrCode WHERE PassengerPhone = @PhoneNumber;";

                    using (var command = new SqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@PhoneNumber", PhoneNumber);

                        using (var reader = command.ExecuteReader())
                        {
                            List<string> qrCodes = new List<string>();

                            while (reader.Read())
                            {
                                string qrCode = reader.GetString(0);
                                qrCodes.Add(qrCode);
                            }

                            connection.Close();
                            return Ok(qrCodes);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("passengerQrcode", $"Sorry, but we have an exception: {ex.Message}");
                return BadRequest(ModelState);
            }
        }



        [ApiExplorerSettings(IgnoreApi = true)]
        public Booking GetBookingId(int id)
        {
            try
            {
                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    string sql = "SELECT * FROM Booking WHERE IdBooking = @IdBooking;";

                    using (var command = new SqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@IdBooking", id);

                        using (var reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                return new Booking
                                {
                                    IdBooking = reader.GetInt32(0),
                                    DateBook = reader.GetDateTime(1).ToString("yyyy-MM-dd hh:mm"), // Assuming DateBook is DateTime in the database
                                    StatusBooking = reader.GetString(2),
                                    TickitPrice = reader.IsDBNull(3) ? 0 : reader.GetDouble(3),
                                    SeatsBooking = reader.IsDBNull(4) ? 0 : reader.GetInt32(4),
                                    PhonePassenger = reader.GetString(5),
                                    JourneyoBo = GetJourneyById(reader.GetInt32(6))
                                };
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Handle exception, e.g., log it or throw a custom exception
                Console.WriteLine($"An error occurred while fetching Booking by ID: {ex.Message}");
            }

            return null; // Return null if Booking with the specified ID is not found
        }



        //helper
        [ApiExplorerSettings(IgnoreApi = true)]

        public int GetBlocked(string phoneNumber)
        {
            try
            {
                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    string sql = "SELECT blocked FROM Passenger WHERE PassengerPhone = @PhoneNumber;";

                    using (var command = new SqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@PhoneNumber", phoneNumber);

                        var result = command.ExecuteScalar();

                        // Check if the result is not null
                        if (result != null && result != DBNull.Value)
                        {
                            return Convert.ToInt32(result);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while getting the blocked count: {ex.Message}");
            }

            return 0; // Return 0 if the passenger is not found or blocked count is not available
        }



       

        //helper 

        [ApiExplorerSettings(IgnoreApi = true)]
        public int GetIdScanner(int idBus)
        {
            int idScanner = 0;

            try
            {
                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    string sql = $"SELECT IdScanner FROM Bus WHERE IdBus = @IdBus;";

                    using (var command = new SqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@IdBus", idBus);

                        using (var reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                idScanner = reader.GetInt32(0);
                            }
                        }
                    }

                    connection.Close();
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("bus", $"Sorry, but we have an exception: {ex.Message}");
                Console.WriteLine(ModelState);
            }

            return idScanner;
        }



       


        // helper 
        [ApiExplorerSettings(IgnoreApi = true)]
        public  string GenerateUniqueValue(string passengerId)
        {
            // Get the current date and time
            DateTime currentDateTime = DateTime.Now;

            // Combine date and passenger ID
            string dataToHash = $"{currentDateTime}{passengerId}";

            // Hash the combined data to create a unique value using SHA-256
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(dataToHash));
                string hashedValue = BitConverter.ToString(hashedBytes).Replace("-", "").ToLower();
                return hashedValue;
            }
        }

       


        //helper 
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


    }
}
