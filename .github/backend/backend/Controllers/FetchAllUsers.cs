using backend.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Data;
using System.Data.SqlClient;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FetchAllUsers : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public FetchAllUsers(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpGet("GetAdminUsers")]
        public IActionResult GetAdminUsers()
        {
            List<Registration> adminUsers = new List<Registration>();
            string connectionString = _configuration.GetConnectionString("BazaCon");

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string query = "SELECT ID, UserName, Password, Email, IsActive FROM Registration WHERE IsActive = 1";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    connection.Open();
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            Registration user = new Registration
                            {
                                ID = reader.GetInt32(0),
                                UserName = reader.GetString(1),
                                Password = reader.GetString(2),
                                Email = reader.GetString(3),
                                IsActive = reader.GetInt32(4) == 1
                            };
                            adminUsers.Add(user);
                        }
                    }
                }
            }

            return Ok(adminUsers);
        }
        [HttpGet("GetUserById/{userId}")]
        public IActionResult GetUserById(int userId)
        {
            Registration user = null;
            string connectionString = _configuration.GetConnectionString("BazaCon");

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string query = "SELECT ID, UserName, Password, Email, IsActive FROM Registration WHERE ID = @UserId";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@UserId", userId);

                    connection.Open();
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            user = new Registration
                            {
                                ID = reader.GetInt32(0),
                                UserName = reader.GetString(1),
                                Password = reader.GetString(2),
                                Email = reader.GetString(3),
                                IsActive = reader.GetInt32(4) == 1
                            };
                        }
                    }
                }
            }

            if (user == null)
            {
                return NotFound(new { Message = "User not found" });
            }

            return Ok(user);
        }

        [HttpDelete("DeleteUser/{id}")]
        public IActionResult DeleteUser(int id)
        {
            string connectionString = _configuration.GetConnectionString("BazaCon");

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string query = "DELETE FROM Registration WHERE ID = @ID";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@ID", id);
                    connection.Open();
                    int rowsAffected = command.ExecuteNonQuery();
                    if (rowsAffected > 0)
                    {
                        return Ok(new { success = true, message = "User deleted successfully." });
                    }
                    else
                    {
                        return NotFound(new { success = false, message = "User not found." });
                    }
                }
            }
        }

        [HttpGet("SearchUsers")]
        public IActionResult SearchUsers(string query)
        {
            if (string.IsNullOrEmpty(query))
            {
                return BadRequest(new { success = false, message = "Query parameter is required." });
            }

            List<Registration> foundUsers = new List<Registration>();
            string connectionString = _configuration.GetConnectionString("BazaCon");

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                // Pretraga korisnika koji počinju sa unetim karakterom
                string sqlQuery = "SELECT ID, UserName, Password, Email, IsActive FROM Registration WHERE IsActive = 1 AND (UserName LIKE @Query OR Email LIKE @Query)";

                using (SqlCommand command = new SqlCommand(sqlQuery, connection))
                {
                    // Dodajemo '!' wildcard na kraj upita da pretraga bude za pocetni karakter
                    command.Parameters.AddWithValue("@Query", query + "%");
                    connection.Open();

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            Registration user = new Registration
                            {
                                ID = reader.GetInt32(0),
                                UserName = reader.GetString(1),
                                Password = reader.GetString(2),
                                Email = reader.GetString(3),
                                IsActive = reader.GetInt32(4) == 1
                            };
                            foundUsers.Add(user);
                        }
                    }
                }
            }

            if (foundUsers.Count == 0)
            {
                return NotFound(new { success = false, message = "No users found matching the query." });
            }

            return Ok(foundUsers);
        }


    }
}
