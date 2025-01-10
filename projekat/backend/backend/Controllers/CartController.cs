using backend.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CartController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public CartController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        // Funkcija za dodavanje proizvoda u korpu
        [HttpPost]
        [Route("add")]
        public IActionResult AddToCart([FromBody] AddToCartRequest request)
        {
            if (request.UserId == null || request.UserId <= 0)
            {
                return BadRequest(new { Message = "Invalid user ID." });
            }

            try
            {
                using (var conn = new SqlConnection(_configuration.GetConnectionString("BazaCon")))
                {
                    conn.Open();

                    // Provera da li korisnik već ima korpu
                    var command = new SqlCommand("SELECT Id FROM Carts WHERE UserId = @UserId", conn);
                    command.Parameters.AddWithValue("@UserId", request.UserId);
                    var cartId = command.ExecuteScalar();

                    if (cartId == null)
                    {
                        command = new SqlCommand("INSERT INTO Carts (UserId) OUTPUT INSERTED.Id VALUES (@UserId)", conn);
                        command.Parameters.AddWithValue("@UserId", request.UserId);
                        cartId = command.ExecuteScalar();
                    }

                    // Provera da li proizvod već postoji u korpi
                    command = new SqlCommand("SELECT Id FROM CartItems WHERE CartId = @CartId AND ProductId = @ProductId", conn);
                    command.Parameters.AddWithValue("@CartId", cartId);
                    command.Parameters.AddWithValue("@ProductId", request.ProductId);
                    var cartItemId = command.ExecuteScalar();

                    if (cartItemId != null)
                    {
                        command = new SqlCommand("UPDATE CartItems SET Quantity = Quantity + 1 WHERE Id = @CartItemId", conn);
                        command.Parameters.AddWithValue("@CartItemId", cartItemId);
                        command.ExecuteNonQuery();
                    }
                    else
                    {
                        command = new SqlCommand("INSERT INTO CartItems (CartId, ProductId, Quantity) VALUES (@CartId, @ProductId, 1)", conn);
                        command.Parameters.AddWithValue("@CartId", cartId);
                        command.Parameters.AddWithValue("@ProductId", request.ProductId);
                        command.ExecuteNonQuery();
                    }
                }

                return Ok(new { Message = "Product added to cart successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = $"Internal server error: {ex.Message}" });
            }
        }

        [HttpGet]
        [Route("get/{userId}")]
        public IActionResult GetCartItems(int userId)
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

                    // Provera da li korisnik već ima korpu
                    var command = new SqlCommand("SELECT Id FROM Carts WHERE UserId = @UserId", conn);
                    command.Parameters.AddWithValue("@UserId", userId);
                    var cartId = command.ExecuteScalar();

                    if (cartId == null)
                    {
                        return Ok(new { Message = "No cart found for this user." });
                    }

                    // Dohvatanje svih proizvoda u korpi za datog korisnika
                    command = new SqlCommand(
                        @"SELECT ci.ProductId, p.Name, p.Price, ci.Quantity, p.Image
                  FROM CartItems ci
                  JOIN Product p ON ci.ProductId = p.ProductId
                  WHERE ci.CartId = @CartId", conn);
                    command.Parameters.AddWithValue("@CartId", cartId);

                    var reader = command.ExecuteReader();
                    var cartItems = new List<object>();

                    while (reader.Read())
                    {
                        cartItems.Add(new
                        {
                            ProductId = reader["ProductId"],
                            Name = reader["Name"],
                            Price = reader["Price"],
                            Quantity = reader["Quantity"],
                            Image = reader["Image"]

                        });
                    }

                    if (cartItems.Count == 0)
                    {
                        return Ok(new { Message = "Your cart is empty." });
                    }

                    return Ok(cartItems);
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = $"Internal server error: {ex.Message}" });
            }
        }

        [HttpPost]
        [Route("remove")]
        public IActionResult RemoveFromCart([FromBody] RemoveFromCartRequest request)
        {
            if (request.UserId <= 0 || request.ProductId <= 0)
            {
                return BadRequest(new { Message = "Invalid user ID or product ID." });
            }

            try
            {
                using (var conn = new SqlConnection(_configuration.GetConnectionString("BazaCon")))
                {
                    conn.Open();

                    // Provera da li korisnik već ima korpu
                    var command = new SqlCommand("SELECT Id FROM Carts WHERE UserId = @UserId", conn);
                    command.Parameters.AddWithValue("@UserId", request.UserId);
                    var cartId = command.ExecuteScalar();

                    if (cartId == null)
                    {
                        return NotFound(new { Message = "Cart not found for this user." });
                    }

                    // Provera da li proizvod postoji u korpi
                    command = new SqlCommand("SELECT Id, Quantity FROM CartItems WHERE CartId = @CartId AND ProductId = @ProductId", conn);
                    command.Parameters.AddWithValue("@CartId", cartId);
                    command.Parameters.AddWithValue("@ProductId", request.ProductId);
                    var reader = command.ExecuteReader();

                    if (!reader.Read())
                    {
                        return NotFound(new { Message = "Product not found in the cart." });
                    }

                    int cartItemId = (int)reader["Id"];
                    int quantity = (int)reader["Quantity"];
                    reader.Close();

                    // Ako količina proizvoda je veća od 1, smanjujemo je za 1
                    if (quantity > 1)
                    {
                        command = new SqlCommand("UPDATE CartItems SET Quantity = Quantity - 1 WHERE Id = @CartItemId", conn);
                        command.Parameters.AddWithValue("@CartItemId", cartItemId);
                        command.ExecuteNonQuery();
                    }
                    else
                    {
                        // Ako je količina 1, brišemo proizvod iz korpe
                        command = new SqlCommand("DELETE FROM CartItems WHERE Id = @CartItemId", conn);
                        command.Parameters.AddWithValue("@CartItemId", cartItemId);
                        command.ExecuteNonQuery();
                    }

                    return Ok(new { Message = "Product quantity updated successfully." });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = $"Internal server error: {ex.Message}" });
            }
        }

    }

}
