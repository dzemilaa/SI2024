using backend.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.Data.SqlClient;
using System.Linq;

namespace backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FetchAllOrdersController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public FetchAllOrdersController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpGet]
        [Route("all-orders")]
        public IActionResult GetAllOrders()
        {
            List<dynamic> allOrders = new List<dynamic>();

            using (var conn = new SqlConnection(_configuration.GetConnectionString("BazaCon")))
            {
                string query = @"
                SELECT o.Id, o.UserId, o.Address, o.TotalAmount, o.OrderDate, o.Status, o.Image, 
                       o.FirstName, o.LastName, o.Email, o.Phone
                FROM [Order] o
                ORDER BY o.OrderDate DESC"; 

                var command = new SqlCommand(query, conn);

                try
                {
                    conn.Open();
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            int orderId = reader.GetInt32(0);
                            int userId = reader.GetInt32(1); 
                            string address = reader.GetString(2);
                            decimal totalAmount = reader.GetDecimal(3);
                            DateTime orderDate = reader.GetDateTime(4);
                            string status = reader.GetString(5);
                            string image = reader["Image"] != DBNull.Value ? reader["Image"].ToString() : null;
                            string firstName = reader.GetString(7);
                            string lastName = reader.GetString(8);
                            string email = reader.GetString(9);
                            string phone = reader.GetString(10);

                           
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

                             
                                allOrders.Add(new
                                {
                                    OrderId = orderId,
                                    UserId = userId,
                                    Address = address,
                                    TotalAmount = totalAmount,
                                    OrderDate = orderDate,
                                    Status = status,
                                    Image = image,
                                    FirstName = firstName,
                                    LastName = lastName,
                                    Email = email,
                                    Phone = phone,
                                    OrderItems = orderItems
                                });
                            }
                        }
                    }

                    if (allOrders.Any())
                    {
                        return Ok(new { Success = true, Data = allOrders });
                    }
                    else
                    {
                        return Ok(new { Success = false, Message = "No orders found." });
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
