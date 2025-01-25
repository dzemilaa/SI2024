using backend.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Data;
using System.Data.SqlClient;
using System.IO;

namespace backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public ProductController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpPost]
        [Route("add")]
        public IActionResult AddProduct([FromForm] Product product)
        {
            
            if (product.Image == null || product.Image.Length == 0)
            {
                return BadRequest("No image uploaded.");
            }

         
            string[] allowedExtensions = { ".jpg", ".jpeg", ".png", ".gif" };
            string fileExtension = Path.GetExtension(product.Image.FileName).ToLower();
            if (Array.IndexOf(allowedExtensions, fileExtension) < 0)
            {
                return BadRequest("Invalid image type. Allowed types are .jpg, .jpeg, .png, .gif.");
            }

          
            string folderPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Images");
            Directory.CreateDirectory(folderPath); 

           
            string uniqueFileName = Path.GetFileNameWithoutExtension(Guid.NewGuid().ToString("N")) + fileExtension;
       
            string filePath = Path.Combine(folderPath, uniqueFileName);
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                product.Image.CopyTo(stream);
            }

            
            string imageFileName = uniqueFileName;

           
            SqlConnection con = new SqlConnection(_configuration.GetConnectionString("BazaCon").ToString());
            SqlCommand cmd = new SqlCommand("INSERT INTO Product (Name, Description, Price, Category, Image) VALUES (@Name, @Description, @Price, @Category, @Image)", con);

      
            cmd.Parameters.AddWithValue("@Name", product.Name);
            cmd.Parameters.AddWithValue("@Description", product.Description);
            cmd.Parameters.AddWithValue("@Price", product.Price);
            cmd.Parameters.AddWithValue("@Category", product.Category);
            cmd.Parameters.AddWithValue("@Image", imageFileName);

            try
            {
                con.Open();
                int i = cmd.ExecuteNonQuery();
                con.Close();

                if (i > 0)
                {
                    Console.WriteLine("Product successfully added");
                    return Ok("Product Added Successfully");
                }
                else
                {
                    Console.WriteLine("Error: Insert query affected 0 rows");
                    return BadRequest("Error Adding Product");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                return StatusCode(500, $"Error: {ex.Message}");
            }

        }
    }
}
