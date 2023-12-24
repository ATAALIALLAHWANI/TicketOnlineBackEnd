using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using TicketOnline.Models;

namespace TicketOnline.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BusController : ControllerBase
    {

        private readonly string connectionString;

        public BusController(IConfiguration configuration)
        {
            connectionString = configuration.GetConnectionString("SqlServerDb") ?? "";
            Console.WriteLine($"ConnectionString: {connectionString}");
        }
        [HttpPost("AddBus")]
        public IActionResult CreateBus([FromBody] BusDto busDto)
        {
            try
            {
                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    string sql = "INSERT INTO Bus (CapacityBus, ModelBus, IdDriver, IdScanner, NumberBus)" +
                        "VALUES (@CapacityBus, @ModelBus, @IdDriver, @IdScanner, @NumberBus);";

                    using (var command = new SqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@CapacityBus", busDto.CapacityBus);
                        command.Parameters.AddWithValue("@ModelBus", busDto.ModelBus);
                        command.Parameters.AddWithValue("@IdDriver", busDto.IdDriver);
                        command.Parameters.AddWithValue("@IdScanner", busDto.IdScanner); // Add this line
                        command.Parameters.AddWithValue("@NumberBus", busDto.NumberBus);

                        command.ExecuteNonQuery();
                        connection.Close();
                    }
                }

                return Ok("Bus created successfully");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("bus", $"Sorry, but we have an exception: {ex.Message}");
                return BadRequest(ModelState);
            }
        }

        [HttpGet("GetBus")]
        public IActionResult GetBus()
        {
            try
            {
                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    string sql = "SELECT * FROM Bus;";

                    using (var command = new SqlCommand(sql, connection))
                    {
                        using (var reader = command.ExecuteReader())
                        {
                            List<Bus> buses = new List<Bus>();

                            while (reader.Read())
                            {
                                Bus bus = new Bus
                                {
                                    IdBus = reader.GetInt32(0),
                                    CapacityBus = reader.GetInt32(1),
                                    ModelBus = reader.IsDBNull(2) ? null : reader.GetString(2),
                                    IdDriver = reader.GetInt32(3),
                                    IdScanner = reader.GetInt32(4),
                                    NumberBus = reader.IsDBNull(5) ? 0 : reader.GetInt32(4)
                                };

                                buses.Add(bus);
                            }

                            connection.Close();
                            return Ok(buses);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("bus", $"Sorry, but we have an exception: {ex.Message}");
                return BadRequest(ModelState);
            }
        }


    }
}
