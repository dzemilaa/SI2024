using backend.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Data;
using System.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserChangeController : ControllerBase
    {
        private readonly string _connectionString;

        
        public UserChangeController(IConfiguration configuration)
        {
           
            _connectionString = configuration.GetConnectionString("BazaCon");
        }

        [HttpPut("ChangeUsername")]
        public IActionResult ChangeUsername(int id, string newUsername)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                var query = "UPDATE Registration SET UserName = @NewUsername WHERE ID = @Id";
                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@NewUsername", newUsername);
                    command.Parameters.AddWithValue("@Id", id);

                    int rowsAffected = command.ExecuteNonQuery();
                    if (rowsAffected == 0)
                        return NotFound("User not found.");
                }
            }
            return Ok("Username updated successfully.");
        }

        
        [HttpPut("ChangeEmail")]
        public IActionResult ChangeEmail(int id, string newEmail)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                var query = "UPDATE Registration SET Email = @NewEmail WHERE ID = @Id";
                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@NewEmail", newEmail);
                    command.Parameters.AddWithValue("@Id", id);

                    int rowsAffected = command.ExecuteNonQuery();
                    if (rowsAffected == 0)
                        return NotFound("User not found.");
                }
            }
            return Ok("Email updated successfully.");
        }

        [HttpDelete("DeleteProfile")]
        public IActionResult DeleteProfile(int id)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                       
                        var deleteReviewProductsQuery = "DELETE FROM ReviewProducts WHERE ReviewId IN (SELECT ReviewId FROM Reviews WHERE OrderId IN (SELECT OrderId FROM [Order] WHERE UserId = @Id))";
                        using (var deleteReviewProductsCommand = new SqlCommand(deleteReviewProductsQuery, connection, transaction))
                        {
                            deleteReviewProductsCommand.Parameters.AddWithValue("@Id", id);
                            deleteReviewProductsCommand.ExecuteNonQuery();
                        }

                        var deleteReviewsQuery = "DELETE FROM Reviews WHERE OrderId IN (SELECT OrderId FROM [Order] WHERE UserId = @Id)";
                        using (var deleteReviewsCommand = new SqlCommand(deleteReviewsQuery, connection, transaction))
                        {
                            deleteReviewsCommand.Parameters.AddWithValue("@Id", id);
                            deleteReviewsCommand.ExecuteNonQuery();
                        }

                      
                        var deleteOrderItemsQuery = "DELETE FROM OrderItems WHERE OrderId IN (SELECT OrderId FROM [Order] WHERE UserId = @Id)";
                        using (var deleteOrderItemsCommand = new SqlCommand(deleteOrderItemsQuery, connection, transaction))
                        {
                            deleteOrderItemsCommand.Parameters.AddWithValue("@Id", id);
                            deleteOrderItemsCommand.ExecuteNonQuery();
                        }

                       
                        var deleteOrdersQuery = "DELETE FROM [Order] WHERE UserId = @Id";
                        using (var deleteOrdersCommand = new SqlCommand(deleteOrdersQuery, connection, transaction))
                        {
                            deleteOrdersCommand.Parameters.AddWithValue("@Id", id);
                            deleteOrdersCommand.ExecuteNonQuery();
                        }

                  
                        var deleteUserQuery = "DELETE FROM Registration WHERE ID = @Id";
                        using (var deleteUserCommand = new SqlCommand(deleteUserQuery, connection, transaction))
                        {
                            deleteUserCommand.Parameters.AddWithValue("@Id", id);
                            int rowsAffected = deleteUserCommand.ExecuteNonQuery();
                            if (rowsAffected == 0)
                            {
                                throw new Exception("User not found.");
                            }
                        }

                       
                        transaction.Commit();
                    }
                    catch (Exception ex)
                    {
                    
                        transaction.Rollback();
                        return StatusCode(500, $"Došlo je do greške prilikom brisanja korisnika i njegovih narudžbina: {ex.Message}");
                    }
                }
            }

            return Ok("Profil i povezane narudžbine su uspešno obrisani.");
        }





     
        [HttpPut("ChangePassword")]
        public IActionResult ChangePassword(int id, string oldPassword, string newPassword)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                var selectQuery = "SELECT Password FROM Registration WHERE ID = @Id";
                using (var selectCommand = new SqlCommand(selectQuery, connection))
                {
                    selectCommand.Parameters.AddWithValue("@Id", id);
                    var existingPassword = selectCommand.ExecuteScalar() as string;

                    if (existingPassword == null)
                        return NotFound("User not found.");

                    if (existingPassword != oldPassword)
                        return BadRequest("Incorrect old password.");
                }

                var updateQuery = "UPDATE Registration SET Password = @NewPassword WHERE ID = @Id";
                using (var updateCommand = new SqlCommand(updateQuery, connection))
                {
                    updateCommand.Parameters.AddWithValue("@NewPassword", newPassword);
                    updateCommand.Parameters.AddWithValue("@Id", id);

                    updateCommand.ExecuteNonQuery();
                }
            }
            return Ok("Password updated successfully.");
        }
    }
}
