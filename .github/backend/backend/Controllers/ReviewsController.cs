using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Data.SqlClient;
using backend.Models;

namespace backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReviewsController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public ReviewsController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpPost]
        [Route("submit")]
        public IActionResult SubmitReview([FromBody] ReviewDto review)
        {
            if (review == null || string.IsNullOrWhiteSpace(review.ReviewText))
            {
                return BadRequest(new { success = false, message = "Review text cannot be empty." });
            }

            if (review.OrderId <= 0 || review.UserId <= 0 || review.ProductIds == null || review.ProductIds.Count == 0)
            {
                return BadRequest(new { success = false, message = "Invalid OrderId, UserId, or ProductIds." });
            }

            string connectionString = _configuration.GetConnectionString("BazaCon");
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                try
                {
                    con.Open();

                    // Check if order exists
                    string checkOrderQuery = "SELECT COUNT(*) FROM [Order] WHERE Id = @OrderId";
                    using (SqlCommand cmd = new SqlCommand(checkOrderQuery, con))
                    {
                        cmd.Parameters.AddWithValue("@OrderId", review.OrderId);
                        int orderExists = (int)cmd.ExecuteScalar();
                        if (orderExists == 0)
                        {
                            return NotFound(new { success = false, message = "Order not found." });
                        }
                    }

                    // Check if user exists
                    string checkUserQuery = "SELECT COUNT(*) FROM Registration WHERE ID = @UserId";
                    using (SqlCommand cmd = new SqlCommand(checkUserQuery, con))
                    {
                        cmd.Parameters.AddWithValue("@UserId", review.UserId);
                        int userExists = (int)cmd.ExecuteScalar();
                        if (userExists == 0)
                        {
                            return NotFound(new { success = false, message = "User not found." });
                        }
                    }

                    // Insert review into database
                    string insertQuery = @"
                INSERT INTO Reviews (OrderId, UserId, ReviewText, CreatedAt)
                VALUES (@OrderId, @UserId, @ReviewText, @CreatedAt);
                SELECT SCOPE_IDENTITY();"; // Getting the ReviewId of the newly inserted review

                    using (SqlCommand cmd = new SqlCommand(insertQuery, con))
                    {
                        cmd.Parameters.AddWithValue("@OrderId", review.OrderId);
                        cmd.Parameters.AddWithValue("@UserId", review.UserId);
                        cmd.Parameters.AddWithValue("@ReviewText", review.ReviewText);
                        cmd.Parameters.AddWithValue("@CreatedAt", DateTime.Now);

                        int reviewId = Convert.ToInt32(cmd.ExecuteScalar()); // Get the inserted ReviewId

                        // Insert the product associations into ReviewProducts table
                        foreach (var productId in review.ProductIds)
                        {
                            string insertReviewProductQuery = @"
                        INSERT INTO ReviewProducts (ReviewId, ProductId)
                        VALUES (@ReviewId, @ProductId)";
                            using (SqlCommand cmdProduct = new SqlCommand(insertReviewProductQuery, con))
                            {
                                cmdProduct.Parameters.AddWithValue("@ReviewId", reviewId);
                                cmdProduct.Parameters.AddWithValue("@ProductId", productId);
                                cmdProduct.ExecuteNonQuery();
                            }
                        }

                        return Ok(new { success = true, message = "Review submitted successfully." });
                    }
                }
                catch (Exception ex)
                {
                    return StatusCode(500, new { success = false, message = $"Error: {ex.Message}" });
                }
            }
        }

        [HttpGet]
        [Route("fetch-all")]
        public IActionResult FetchAllReviews()
        {
            string connectionString = _configuration.GetConnectionString("BazaCon");
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                try
                {
                    con.Open();

                    string query = @"
                SELECT r.ReviewId, r.OrderId, r.UserId, r.ReviewText, r.CreatedAt, 
                       o.TotalAmount, u.UserName 
                FROM Reviews r
                JOIN [Order] o ON r.OrderId = o.Id
                JOIN Registration u ON r.UserId = u.ID";

                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            var reviews = new List<object>();

                            while (reader.Read())
                            {
                                reviews.Add(new
                                {
                                    ReviewId = reader.GetInt32(0),
                                    OrderId = reader.GetInt32(1),
                                    UserId = reader.GetInt32(2),
                                    ReviewText = reader.GetString(3),
                                    CreatedAt = reader.GetDateTime(4),
                                    TotalAmount = reader.GetDecimal(5),
                                    UserName = reader.GetString(6)
                                });
                            }

                            return Ok(new { success = true, data = reviews });
                        }
                    }
                }
                catch (Exception ex)
                {
                    return StatusCode(500, new { success = false, message = $"Error: {ex.Message}" });
                }
            }
        }
        [HttpGet]
        [Route("fetch-by-product")]
        public IActionResult FetchReviewsByProductId(int productId)
        {
            if (productId <= 0)
            {
                return BadRequest(new { success = false, message = "Invalid ProductId." });
            }

            string connectionString = _configuration.GetConnectionString("BazaCon");
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                try
                {
                    con.Open();

                    string query = @"
                SELECT r.ReviewId, r.OrderId, r.UserId, r.ReviewText, r.CreatedAt, 
                       o.TotalAmount, u.UserName 
                FROM Reviews r
                JOIN [Order] o ON r.OrderId = o.Id
                JOIN Registration u ON r.UserId = u.ID
                JOIN ReviewProducts rp ON rp.ReviewId = r.ReviewId
                WHERE rp.ProductId = @ProductId";

                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        cmd.Parameters.AddWithValue("@ProductId", productId);

                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            var reviews = new List<object>();

                            while (reader.Read())
                            {
                                reviews.Add(new
                                {
                                    ReviewId = reader.GetInt32(0),
                                    OrderId = reader.GetInt32(1),
                                    UserId = reader.GetInt32(2),
                                    ReviewText = reader.GetString(3),
                                    CreatedAt = reader.GetDateTime(4),
                                    TotalAmount = reader.GetDecimal(5),
                                    UserName = reader.GetString(6)
                                });
                            }

                            if (reviews.Count == 0)
                            {
                                return NotFound(new { success = false, message = "No reviews found for this product." });
                            }

                            return Ok(new { success = true, data = reviews });
                        }
                    }
                }
                catch (Exception ex)
                {
                    return StatusCode(500, new { success = false, message = $"Error: {ex.Message}" });
                }
            }
        }
        [HttpDelete]
        [Route("remove/{reviewId}")]
        public IActionResult RemoveReview(int reviewId)
        {
            if (reviewId <= 0)
            {
                return BadRequest(new { success = false, message = "Invalid ReviewId." });
            }

            string connectionString = _configuration.GetConnectionString("BazaCon");
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                try
                {
                    con.Open();

                    // Provera da li recenzija postoji
                    string checkReviewQuery = "SELECT COUNT(*) FROM Reviews WHERE ReviewId = @ReviewId";
                    using (SqlCommand cmd = new SqlCommand(checkReviewQuery, con))
                    {
                        cmd.Parameters.AddWithValue("@ReviewId", reviewId);
                        int reviewExists = (int)cmd.ExecuteScalar();
                        if (reviewExists == 0)
                        {
                            return NotFound(new { success = false, message = "Review not found." });
                        }
                    }

                    // Prvo brišemo povezane podatke u tabeli ReviewProducts
                    string deleteReviewProductsQuery = "DELETE FROM ReviewProducts WHERE ReviewId = @ReviewId";
                    using (SqlCommand cmd = new SqlCommand(deleteReviewProductsQuery, con))
                    {
                        cmd.Parameters.AddWithValue("@ReviewId", reviewId);
                        cmd.ExecuteNonQuery();
                    }

                    // Brisanje recenzije
                    string deleteReviewQuery = "DELETE FROM Reviews WHERE ReviewId = @ReviewId";
                    using (SqlCommand cmd = new SqlCommand(deleteReviewQuery, con))
                    {
                        cmd.Parameters.AddWithValue("@ReviewId", reviewId);

                        int rowsAffected = cmd.ExecuteNonQuery();
                        if (rowsAffected > 0)
                        {
                            return Ok(new { success = true, message = "Review deleted successfully." });
                        }
                        else
                        {
                            return StatusCode(500, new { success = false, message = "Failed to delete review." });
                        }
                    }
                }
                catch (Exception ex)
                {
                    return StatusCode(500, new { success = false, message = $"Error: {ex.Message}" });
                }
            }
        }
        [HttpGet]
        [Route("search-reviews")]
        public IActionResult SearchReviews(string username = null, int? orderId = null)
        {
            string connectionString = _configuration.GetConnectionString("BazaCon");
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                try
                {
                    con.Open();

                    // Dinamički kreiraj SQL upit na osnovu prosleđenih parametara
                    var query = @"
                SELECT r.ReviewId, r.OrderId, r.UserId, r.ReviewText, r.CreatedAt, 
                       o.TotalAmount, u.UserName 
                FROM Reviews r
                JOIN [Order] o ON r.OrderId = o.Id
                JOIN Registration u ON r.UserId = u.ID
                WHERE 1 = 1"; // Ovaj uslov omogućava jednostavno dodavanje dodatnih filtera

                    // Dodaj filtere na osnovu prosleđenih parametara
                    if (!string.IsNullOrEmpty(username))
                    {
                        query += " AND u.UserName LIKE @Username";
                    }

                    if (orderId.HasValue)
                    {
                        query += " AND r.OrderId = @OrderId";
                    }

                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        if (!string.IsNullOrEmpty(username))
                        {
                            cmd.Parameters.AddWithValue("@Username", "%" + username + "%");
                        }

                        if (orderId.HasValue)
                        {
                            cmd.Parameters.AddWithValue("@OrderId", orderId);
                        }

                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            var reviews = new List<object>();

                            while (reader.Read())
                            {
                                reviews.Add(new
                                {
                                    ReviewId = reader.GetInt32(0),
                                    OrderId = reader.GetInt32(1),
                                    UserId = reader.GetInt32(2),
                                    ReviewText = reader.GetString(3),
                                    CreatedAt = reader.GetDateTime(4),
                                    TotalAmount = reader.GetDecimal(5),
                                    UserName = reader.GetString(6)
                                });
                            }

                            if (reviews.Count == 0)
                            {
                                return NotFound(new { success = false, message = "No reviews found." });
                            }

                            return Ok(new { success = true, data = reviews });
                        }
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
