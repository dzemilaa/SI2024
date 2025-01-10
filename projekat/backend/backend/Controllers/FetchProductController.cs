using backend.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Data;
using System.Data.SqlClient;
using System.IO;

namespace backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FetchProductController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public FetchProductController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpGet]
        [Route("list")]
        public IActionResult GetProductList()
        {
            List<FetchProduct> productList = new List<FetchProduct>();

            // Konekcija sa bazom
            using (SqlConnection con = new SqlConnection(_configuration.GetConnectionString("BazaCon").ToString()))
            {
                string query = "SELECT ProductId, Name, Description, Price, Category, Image FROM Product";
                SqlCommand cmd = new SqlCommand(query, con);

                try
                {
                    con.Open();
                    SqlDataReader reader = cmd.ExecuteReader();

                    while (reader.Read())
                    {
                        FetchProduct product = new FetchProduct
                        {
                            ProductId = Convert.ToInt32(reader["ProductId"]),  // Učitaj ProductId
                            Name = reader["Name"].ToString(),
                            Description = reader["Description"].ToString(),
                            Price = Convert.ToDecimal(reader["Price"]),
                            Category = reader["Category"].ToString(),
                            Image = reader["Image"] != DBNull.Value ? reader["Image"].ToString() : null  // Uverite se da je Image ispravno učitan
                        };
                        productList.Add(product);
                    }
                    reader.Close();

                    if (productList.Count > 0)
                    {
                        return Ok(new { success = true, data = productList });
                    }
                    else
                    {
                        return Ok(new { success = false, message = "No products found." });
                    }
                }
                catch (Exception ex)
                {
                    return StatusCode(500, new { success = false, message = $"Error: {ex.Message}" });
                }
            }
        }


        [HttpPost]
        [Route("remove")]
        public IActionResult RemoveProduct([FromBody] RemoveProduct request)
        {
            if (request.ProductId <= 0)
            {
                return BadRequest("Invalid ProductId.");
            }

            // Konekcija sa bazom
            using (SqlConnection con = new SqlConnection(_configuration.GetConnectionString("BazaCon").ToString()))
            {
                SqlCommand cmd = new SqlCommand("DELETE FROM Product WHERE ProductId = @ProductId", con);
                cmd.Parameters.AddWithValue("@ProductId", request.ProductId);

                try
                {
                    con.Open();
                    int rowsAffected = cmd.ExecuteNonQuery();
                    if (rowsAffected > 0)
                    {

                        SqlCommand reseedCmd = new SqlCommand("DBCC CHECKIDENT ('Product', RESEED, 0);", con);
                        reseedCmd.ExecuteNonQuery();

                        return Ok(new { success = true, message = "Product deleted successfully." });
                    }
                    else
                    {
                        return NotFound(new { success = false, message = "Product not found." });
                    }
                }
                catch (Exception ex)
                {
                    return StatusCode(500, new { success = false, message = $"Error: {ex.Message}" });
                }
            }
        }
    }
}
