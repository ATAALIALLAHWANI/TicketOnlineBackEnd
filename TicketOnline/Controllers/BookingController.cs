﻿using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using TicketOnline.Models;
using System.Security.Cryptography;
using System.Text;
using Newtonsoft.Json;
using TicketOnline.Repository;
using Microsoft.Azure.Cosmos;

namespace TicketOnline.Controllers
{


    class helloTest
    {
        public helloTest(string id, string hello)
        {
            this.id = id;
            this.helloHello = hello;
        }
        public string id { get; set; }

        public string helloHello { get; set; }

    }

    [Route("api/[controller]")]
    [ApiController]
    public class BookingController : ControllerBase
    {
        private readonly string connectionString;
        NotificationRepository notificationRepository;
        SaveQrcodePassenger saveQrcodePassenger;
        private readonly string FilesDirectory = "Files";
        private readonly string JsonFileExist = Path.Combine("Files", "data.json");
        List<QrCode> existingJson = new List<QrCode>();


        //
      

        public BookingController(IConfiguration configuration)
        {
            connectionString = configuration.GetConnectionString("SqlServerDb") ?? "";
            Console.WriteLine($"ConnectionString: {connectionString}");
            saveQrcodePassenger = new SaveQrcodePassenger(connectionString);
            notificationRepository = new NotificationRepository(connectionString);

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

                DateTime newDate = currentDate.AddMinutes(5);
                 string DateJourney = bookingDto.JourneyoBo.DateJourney + " " + bookingDto.JourneyoBo.DepartuerJourney;
                if (GetBlocked(bookingDto.PhonePassenger) >= 3) return BadRequest("this passenger is blocked ");

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

        [HttpPut("AcceptPay")]
        public async Task<IActionResult> AcceptPay([FromQuery] int IdBooking)
        {
            try
            {
                int bookingId = IdBooking;
                using (var connection = new SqlConnection(connectionString))
                {
                      
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
                                    string dateJourney = $"{bookingDto.JourneyoBo.DateJourney} {bookingDto.JourneyoBo.DepartuerJourney}";
                                    QrCode jsonContent = new QrCode
                                    {
                                        IdQrcode = Guid.NewGuid().ToString(),
                                        DateQrCode = dateJourney,
                                        DateExpierDate = bookingDto.JourneyoBo.DepartuerJourney,
                                        IdScanner = GetIdScanner(bookingDto.JourneyoBo.BusID),
                                        QrCodeList = new List<string>()
                                    };

                                   
                                    CosmosClient cosmosClient = new CosmosClient(EndpointUri, PrimaryKey,
                                        new CosmosClientOptions()
                                        {
                                            ConnectionMode = ConnectionMode.Direct,
                                            ApplicationName = "Azure2"
                                        });
                                    Database database = cosmosClient.GetDatabase(databaseId);
                                    Container container = database.GetContainer("Container1");

                                    // Query Cosmos DB to get all items
                                    var query = new QueryDefinition("SELECT * FROM c");
                                    var items = new List<dynamic>();
                                    FeedIterator<dynamic> resultSetIterator = container.GetItemQueryIterator<dynamic>(query);

                                    while (resultSetIterator.HasMoreResults)
                                    {
                                        FeedResponse<dynamic> response = await resultSetIterator.ReadNextAsync();
                                        items.AddRange(response);

                                    }
                                    bool flag2 = false;
                                    string symbol2 = GenerateUniqueValue(bookingDto.PhonePassenger);

                                    if (items.Any())
                                    {

                                        foreach (var item in items)
                                        {
                                            var qrCode2 = Newtonsoft.Json.JsonConvert.DeserializeObject<QrCode>(item.ToString());

                                            if (qrCode2.DateQrCode.Equals(dateJourney))
                                            {
                                                qrCode2.QrCodeList.Add(symbol2);
                                                flag2 = true;
                                                var response = await container.ReplaceItemAsync(qrCode2, qrCode2.IdQrcode, new PartitionKey(qrCode2.IdQrcode));
                                            }

                                        }
                                    }

                                        if (!flag2)
                                        {
                                            jsonContent.QrCodeList.Add(symbol2);
                                            var result = await container.CreateItemAsync<QrCode>(jsonContent, new PartitionKey(jsonContent.IdQrcode));
                                        }

                                        saveQrcodePassenger.SavePassangerQrcodeToDatabase(bookingDto.PhonePassenger, symbol2);

                                        Console.WriteLine("File created successfully");
                                        connection.Close();

                                        return Ok("ok");
                                    
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



        [HttpGet("GetNotification")]
        public IActionResult GetAllNotification()
        {
            List<string> message = new List<string>();

            try
            {
                message = notificationRepository.GetAllNotifications();
            }
            catch (Exception ex)
            {
                // Handle exception, e.g., log it or throw a custom exception
                Console.WriteLine($"An error occurred while retrieving notifications from the database: {ex.Message}");
                return StatusCode(500, "Internal Server Error");
            }

            return Ok(message);
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
