using backend.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.Data.SqlClient;
using System.Linq;

namespace backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FetchOrdersController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public FetchOrdersController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpGet]
        [Route("user-orders")]
        public IActionResult GetUserOrders([FromQuery] int userId)
        {
            if (userId <= 0)
            {
                return BadRequest(new { Message = "Invalid user ID." });
            }

            List<dynamic> userOrders = new List<dynamic>();

            using (var conn = new SqlConnection(_configuration.GetConnectionString("BazaCon")))
            {
                string query = @"
        SELECT o.Id, o.Address, o.TotalAmount, o.OrderDate, o.Status, o.Image 
        FROM [Order] o 
        WHERE o.UserId = @UserId 
        ORDER BY o.OrderDate DESC";

                var command = new SqlCommand(query, conn);
                command.Parameters.AddWithValue("@UserId", userId);

                try
                {
                    conn.Open();
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            int orderId = reader.GetInt32(0);
                            string address = reader.GetString(1);
                            decimal totalAmount = reader.GetDecimal(2);
                            DateTime orderDate = reader.GetDateTime(3);
                            string status = reader.GetString(4);
                            string image = reader["Image"] != DBNull.Value ? reader["Image"].ToString() : null;

                         
                            using (var itemConn = new SqlConnection(_configuration.GetConnectionString("BazaCon")))
                            {
                                itemConn.Open();
                                var orderItems = new List<dynamic>();
                                var itemsCommand = new SqlCommand(@"
                        SELECT oi.ProductId, oi.Quantity, oi.Price, oi.Image, p.Name, p.Description, p.Category 
                        FROM OrderItems oi 
                        INNER JOIN Product p ON oi.ProductId = p.ProductId
                        WHERE oi.OrderId = @OrderId", itemConn);
                                itemsCommand.Parameters.AddWithValue("@OrderId", orderId);

                                using (var itemsReader = itemsCommand.ExecuteReader())
                                {
                                    while (itemsReader.Read())
                                    {
                                        orderItems.Add(new
                                        {
                                            ProductId = itemsReader.GetInt32(0),
                                            Quantity = itemsReader.GetInt32(1),
                                            Price = itemsReader.GetDecimal(2),
                                            Image = itemsReader["Image"] != DBNull.Value ? itemsReader["Image"].ToString() : null,
                                            Name = itemsReader.GetString(4),
                                            Description = itemsReader["Description"] != DBNull.Value ? itemsReader["Description"].ToString() : null,
                                            Category = itemsReader["Category"] != DBNull.Value ? itemsReader["Category"].ToString() : null
                                        });
                                    }
                                }

                                userOrders.Add(new
                                {
                                    OrderId = orderId,
                                    Address = address,
                                    TotalAmount = totalAmount,
                                    OrderDate = orderDate,
                                    Status = status,
                                    Image = image,
                                    OrderItems = orderItems
                                });
                            }
                        }
                    }

                    if (userOrders.Any())
                    {
                        return Ok(new { Success = true, Data = userOrders });
                    }
                    else
                    {
                        return Ok(new { Success = false, Message = "No orders found for this user." });
                    }
                }
                catch (Exception ex)
                {
                    return StatusCode(500, new { Success = false, Message = $"Error: {ex.Message}" });
                }
            }

        }
    }
}
