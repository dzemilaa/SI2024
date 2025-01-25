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

        // GET: api/FetchProduct/list
        [HttpGet]
        [Route("list")]
        public IActionResult GetProductList()
        {
            List<FetchProduct> productList = new List<FetchProduct>();

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
                            ProductId = Convert.ToInt32(reader["ProductId"]),
                            Name = reader["Name"].ToString(),
                            Description = reader["Description"].ToString(),
                            Price = Convert.ToDecimal(reader["Price"]),
                            Category = reader["Category"].ToString(),
                            Image = reader["Image"] != DBNull.Value ? reader["Image"].ToString() : null
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
        [HttpGet]
        [Route("{id}")]
        public IActionResult GetProductById(int id)
        {
            FetchProduct product = null;

            using (SqlConnection con = new SqlConnection(_configuration.GetConnectionString("BazaCon").ToString()))
            {
                string query = "SELECT ProductId, Name, Description, Price, Category, Image FROM Product WHERE ProductId = @ProductId";
                SqlCommand cmd = new SqlCommand(query, con);
                cmd.Parameters.AddWithValue("@ProductId", id);

                try
                {
                    con.Open();
                    SqlDataReader reader = cmd.ExecuteReader();

                    if (reader.Read())
                    {
                        product = new FetchProduct
                        {
                            ProductId = Convert.ToInt32(reader["ProductId"]),
                            Name = reader["Name"].ToString(),
                            Description = reader["Description"].ToString(),
                            Price = Convert.ToDecimal(reader["Price"]),
                            Category = reader["Category"].ToString(),
                            Image = reader["Image"] != DBNull.Value ? reader["Image"].ToString() : null
                        };
                    }
                    reader.Close();

                    if (product != null)
                    {
                        return Ok(new { success = true, data = product });
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

// GET: api/FetchProduct/search/{searchTerm}
[HttpGet]
        [Route("search/{searchTerm}")]
        public IActionResult SearchProduct(string searchTerm)
        {
            List<FetchProduct> searchResults = new List<FetchProduct>();

            using (SqlConnection con = new SqlConnection(_configuration.GetConnectionString("BazaCon").ToString()))
            {
                string query = "SELECT ProductId, Name, Description, Price, Category, Image FROM Product WHERE Name LIKE @SearchTerm";
                SqlCommand cmd = new SqlCommand(query, con);
                cmd.Parameters.AddWithValue("@SearchTerm", "%" + searchTerm + "%");

                try
                {
                    con.Open();
                    SqlDataReader reader = cmd.ExecuteReader();

                    while (reader.Read())
                    {
                        FetchProduct product = new FetchProduct
                        {
                            ProductId = Convert.ToInt32(reader["ProductId"]),
                            Name = reader["Name"].ToString(),
                            Description = reader["Description"].ToString(),
                            Price = Convert.ToDecimal(reader["Price"]),
                            Category = reader["Category"].ToString(),
                            Image = reader["Image"] != DBNull.Value ? reader["Image"].ToString() : null
                        };
                        searchResults.Add(product);
                    }
                    reader.Close();

                    if (searchResults.Count > 0)
                    {
                        return Ok(new { success = true, data = searchResults });
                    }
                    else
                    {
                        return Ok(new { success = false, message = "No products found for the search term." });
                    }
                }
                catch (Exception ex)
                {
                    return StatusCode(500, new { success = false, message = $"Error: {ex.Message}" });
                }
            }
        }

        [HttpPost]
        [Route("update")]
        public async Task<IActionResult> UpdateProduct([FromForm] Product request)
        {
            if (request.ProductId <= 0)
            {
                return BadRequest("Invalid ProductId.");
            }

            // Dohvati trenutnu sliku iz baze (ako postoji)
            string currentImageFileName = null;
            using (SqlConnection con = new SqlConnection(_configuration.GetConnectionString("BazaCon").ToString()))
            {
                SqlCommand cmd = new SqlCommand("SELECT Image FROM Product WHERE ProductId = @ProductId", con);
                cmd.Parameters.AddWithValue("@ProductId", request.ProductId);

                try
                {
                    con.Open();
                    var result = await cmd.ExecuteScalarAsync();
                    if (result != null)
                    {
                        currentImageFileName = result.ToString(); // Trenutna slika
                    }
                }
                catch (Exception ex)
                {
                    return StatusCode(500, new { success = false, message = $"Error: {ex.Message}" });
                }
            }

            // Obrada nove slike (ako postoji)
            string imageFileName = currentImageFileName; // Postavite trenutnu sliku kao podrazumevanu vrednost
            if (request.Image != null)
            {
                // Sačuvaj sliku na serveru (npr. u folderu 'images')
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", request.Image.FileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await request.Image.CopyToAsync(stream); // Kopiraj sliku u fajl
                }

                imageFileName = request.Image.FileName; // Pošaljemo ime fajla u bazi
            }

            using (SqlConnection con = new SqlConnection(_configuration.GetConnectionString("BazaCon").ToString()))
            {
                SqlCommand cmd = new SqlCommand("UPDATE Product SET Name = @Name, Description = @Description, Price = @Price, Category = @Category, Image = @Image WHERE ProductId = @ProductId", con);
                cmd.Parameters.AddWithValue("@ProductId", request.ProductId);
                cmd.Parameters.AddWithValue("@Name", request.Name);
                cmd.Parameters.AddWithValue("@Description", request.Description);
                cmd.Parameters.AddWithValue("@Price", request.Price);
                cmd.Parameters.AddWithValue("@Category", request.Category);
                cmd.Parameters.AddWithValue("@Image", imageFileName); // Ako nije poslata nova slika, koristi staru

                try
                {
                    con.Open();
                    int rowsAffected = cmd.ExecuteNonQuery();
                    if (rowsAffected > 0)
                    {
                        return Ok(new { success = true, message = "Product updated successfully." });
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




        // POST: api/FetchProduct/remove
        [HttpPost]
        [Route("remove")]
        public IActionResult RemoveProduct([FromBody] RemoveProduct request)
        {
            if (request.ProductId <= 0)
            {
                return BadRequest("Invalid ProductId.");
            }

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
