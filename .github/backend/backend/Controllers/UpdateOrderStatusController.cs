using backend.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.Data.SqlClient;

namespace backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UpdateOrderStatusController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public UpdateOrderStatusController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpPut]
        [Route("update-status/{orderId}")]
        public IActionResult UpdateOrderStatus(int orderId, [FromBody] OrderStatusUpdate orderStatus)
        {
            if (orderStatus == null || string.IsNullOrEmpty(orderStatus.NewStatus))
            {
                return BadRequest(new { success = false, Message = "Invalid payload. Ensure that the 'newStatus' field is provided." });
            }

            string newStatus = orderStatus.NewStatus;

            if (newStatus != "Pending" && newStatus != "Out for delivery" && newStatus != "Delivered")
            {
                return BadRequest(new { success = false, Message = "Invalid status. Status must be 'Pending', 'Out for delivery', or 'Delivered'." });
            }

            using (var conn = new SqlConnection(_configuration.GetConnectionString("BazaCon")))
            {
                string query = "UPDATE [Order] SET Status = @Status WHERE Id = @OrderId AND Status != 'Delivered'";

                var command = new SqlCommand(query, conn);
                command.Parameters.AddWithValue("@Status", newStatus);
                command.Parameters.AddWithValue("@OrderId", orderId);

                try
                {
                    conn.Open();
                    var rowsAffected = command.ExecuteNonQuery();

                    if (rowsAffected > 0)
                    {
                        return Ok(new { success = true, Message = "Order status updated successfully." });
                    }
                    else
                    {
                        return NotFound(new { success = false, Message = "Order not found or status is already 'Delivered'." });
                    }
                }
                catch (Exception ex)
                {
                    return StatusCode(500, new { success = false, Message = $"Error updating order status: {ex.Message}" });
                }
            }
        }


    }
}
