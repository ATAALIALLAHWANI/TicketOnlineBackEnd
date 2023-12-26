using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using TicketOnline.Models;
using System.Security.Cryptography;
using System.Text;
using Newtonsoft.Json;

namespace TicketOnline.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BookingController : ControllerBase
    {
        private readonly string connectionString;

        private readonly string FilesDirectory = "Files";
        private readonly string JsonFileExist = Path.Combine("Files", "data.json");
        List<QrCode> existingJson = new List<QrCode>();
        //QrCode obj = null;
        public BookingController(IConfiguration configuration)
        {
            connectionString = configuration.GetConnectionString("SqlServerDb") ?? "";
            Console.WriteLine($"ConnectionString: {connectionString}");


            if (System.IO.File.Exists(JsonFileExist))
            {
                // Read the existing JSON content
                var existingJsonContent = System.IO.File.ReadAllText(JsonFileExist);

                // Deserialize the existing JSON into a list of JsonModel
                existingJson = JsonConvert.DeserializeObject<List<QrCode>>(existingJsonContent);
                checkedEXpierDate();
                existingJson = OrdringQrCode(existingJson);

            }

        }

        [HttpPost("AddBooking")]
        public IActionResult AddBooking([FromBody] BookingDto bookingDto)
        {
            try
            {
                if (!bookingDto.StatusBooking.Equals("not pay"))
                {
                    QrCode jsonContent = new QrCode
                    {
                        IdQrcode = 1,
                        DateََQrCode = bookingDto.JourneyoBo.DateJourney + " " + bookingDto.JourneyoBo.DepartuerJourney,
                        DateExpierDate = bookingDto.JourneyoBo.DepartuerJourney,
                        IdScanner = GetIdScanner(bookingDto.JourneyoBo.BusID),
                        QrCodeList = new List<string>()

                    };


                    if (System.IO.File.Exists(JsonFileExist))
                    {

                        bool flag = false;
                        string s = bookingDto.JourneyoBo.DateJourney + " " + bookingDto.JourneyoBo.DepartuerJourney;
                        for (int i = 0; i < existingJson.Count; i++)
                        {


                            if (existingJson[i].DateََQrCode.Equals(s))
                            {


                                existingJson[i].QrCodeList.Add(GenerateUniqueValue(bookingDto.PhonePassenger));
                                flag = true;
                                break;

                            }
                        }
                        if (!flag)
                        {
                            jsonContent.QrCodeList.Add(GenerateUniqueValue(bookingDto.PhonePassenger));
                            existingJson.Add(jsonContent);
                        }

                        System.IO.File.WriteAllText(JsonFileExist, JsonConvert.SerializeObject(existingJson));

                        Console.WriteLine("file create sucssefuly");
                    }



                }






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

        [HttpGet("CheckQeCode")]
        public bool CheckQrcode([FromQuery] string qrCode, int IdScanner)
        {
            bool result = false;

            if (existingJson != null && existingJson.Count > 0)
            {
                for (int i = 0; i < existingJson.Count; i++)
                {
                    if (existingJson[i].QrCodeList.Contains(qrCode) && existingJson[i].IdScanner == IdScanner)
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









        //helper
        [ApiExplorerSettings(IgnoreApi = true)]

        public void checkedEXpierDate()
        {
            DateTime currentDate = DateTime.Now;

            DateTime newDate = currentDate.AddMinutes(-30);
            for (int i = existingJson.Count - 1; i >= 0; i--)
            {
                QrCode item = existingJson[i];
                if (DateTime.Parse(item.DateََQrCode) <= newDate)
                {
                    existingJson.RemoveAt(i);
                }
            }
            System.IO.File.WriteAllText(JsonFileExist, JsonConvert.SerializeObject(existingJson));


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



        //helper
        [ApiExplorerSettings(IgnoreApi = true)]
        public List<QrCode> OrdringQrCode(List <QrCode> qrCodes)
        {
 

            
            var orderedQrCodes = qrCodes.OrderBy(qrCode => DateTime.Parse(qrCode.DateََQrCode)).ToList();

            return orderedQrCodes;
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
