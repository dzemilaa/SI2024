using backend.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.Data;
using System.Data.SqlClient;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrderController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public OrderController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpPost]
        [Route("Order")]
        public async Task<IActionResult> AddToOrder([FromForm] int userId,
                                              [FromForm] string address,
                                              [FromForm] string firstName,
                                              [FromForm] string lastName,
                                              [FromForm] string email,
                                              [FromForm] string phone)
        {
            if (userId <= 0)
            {
                return BadRequest(new { Message = "Invalid user ID." });
            }

            try
            {
                using (var conn = new SqlConnection(_configuration.GetConnectionString("BazaCon")))
                {
                    conn.Open();

           
                    var command = new SqlCommand("SELECT c.Id FROM Carts c WHERE c.UserId = @UserId", conn);
                    command.Parameters.AddWithValue("@UserId", userId);
                    var cartId = command.ExecuteScalar();

                    if (cartId == null)
                    {
                        return BadRequest(new { Message = "Cart is empty or does not exist." });
                    }

                    command = new SqlCommand(@"
            SELECT ci.ProductId, ci.Quantity, p.Price, p.Image 
            FROM CartItems ci 
            INNER JOIN Product p ON ci.ProductId = p.ProductId
            WHERE ci.CartId = @CartId", conn);
                    command.Parameters.AddWithValue("@CartId", cartId);

                    var cartItems = new List<dynamic>();
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            cartItems.Add(new
                            {
                                ProductId = reader.GetInt32(0),
                                Quantity = reader.GetInt32(1),
                                Price = reader.GetDecimal(2),
                                Image = reader["Image"]
                            });
                        }
                    }

                    if (!cartItems.Any())
                    {
                        return BadRequest(new { Message = "Cart is empty." });
                    }

            
                    decimal totalAmount = cartItems.Sum(item => (decimal)item.Quantity * (decimal)item.Price);
                    command = new SqlCommand("INSERT INTO [Order] (UserId, Address, TotalAmount, OrderDate, Status, FirstName, LastName, Email, Phone) OUTPUT INSERTED.Id VALUES (@UserId, @Address, @TotalAmount, @OrderDate, 'Pending', @FirstName, @LastName, @Email, @Phone)", conn);
                    command.Parameters.AddWithValue("@UserId", userId);
                    command.Parameters.AddWithValue("@Address", address);
                    command.Parameters.AddWithValue("@TotalAmount", totalAmount);
                    command.Parameters.AddWithValue("@OrderDate", DateTime.Now);
                    command.Parameters.AddWithValue("@FirstName", firstName);  
                    command.Parameters.AddWithValue("@LastName", lastName);    
                    command.Parameters.AddWithValue("@Email", email);         
                    command.Parameters.AddWithValue("@Phone", phone);          

                    var orderId = command.ExecuteScalar();

             
                    foreach (var item in cartItems)
                    {
                        command = new SqlCommand("INSERT INTO OrderItems (OrderId, ProductId, Quantity, Price, Image) VALUES (@OrderId, @ProductId, @Quantity, @Price, @Image)", conn);
                        command.Parameters.AddWithValue("@OrderId", orderId);
                        command.Parameters.AddWithValue("@ProductId", item.ProductId);
                        command.Parameters.AddWithValue("@Quantity", item.Quantity);
                        command.Parameters.AddWithValue("@Price", item.Price);
                        command.Parameters.AddWithValue("@Image", item.Image);
                        command.ExecuteNonQuery();
                    }

          
                    command = new SqlCommand("SELECT TOP 1 Image FROM OrderItems WHERE OrderId = @OrderId", conn);
                    command.Parameters.AddWithValue("@OrderId", orderId);

                    var image = command.ExecuteScalar();

                    if (image != null)
                    {
                        command = new SqlCommand("UPDATE [Order] SET Image = @Image WHERE Id = @OrderId", conn);
                        command.Parameters.AddWithValue("@Image", image);
                        command.Parameters.AddWithValue("@OrderId", orderId);
                        command.ExecuteNonQuery();
                    }

             
                    command = new SqlCommand("DELETE FROM CartItems WHERE CartId = @CartId", conn);
                    command.Parameters.AddWithValue("@CartId", cartId);
                    command.ExecuteNonQuery();

                    command = new SqlCommand("DELETE FROM Carts WHERE UserId = @UserId", conn);
                    command.Parameters.AddWithValue("@UserId", userId);
                    command.ExecuteNonQuery();
                }

                return Ok(new { Message = "Order created successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = $"Internal server error: {ex.Message}" });
            }
        }



    }
}
