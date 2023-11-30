using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

namespace TicketOnline.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AdminController : ControllerBase
    {

        private readonly string connectionString;

        public AdminController(IConfiguration configuration)
        {
            connectionString = configuration.GetConnectionString("SqlServerDb") ?? "";
            Console.WriteLine($"ConnectionString: {connectionString}");
        }
        [HttpGet("CheckedAdmin")]
        public IActionResult CheckedAdmin([FromQuery] string email, [FromQuery] string password)
        {
            try
            {
                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    string sql = "SELECT COUNT(*) FROM Admin WHERE emailAdmin = @Email AND PasswordAdmin = @Password";

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
                ModelState.AddModelError("admin", $"Sorry, but we have an exception: {ex.Message}");
                return BadRequest(ModelState);
            }
        }


    }
}
