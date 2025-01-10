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
            // Proverite da li je slika poslata
            if (product.Image == null || product.Image.Length == 0)
            {
                return BadRequest("No image uploaded.");
            }

            // Proverite tip slike
            string[] allowedExtensions = { ".jpg", ".jpeg", ".png", ".gif" };
            string fileExtension = Path.GetExtension(product.Image.FileName).ToLower();
            if (Array.IndexOf(allowedExtensions, fileExtension) < 0)
            {
                return BadRequest("Invalid image type. Allowed types are .jpg, .jpeg, .png, .gif.");
            }

            // Putanja gde će se sačuvati slike
            string folderPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Images");
            Directory.CreateDirectory(folderPath); // Kreiraj folder ako ne postoji

            // Generiši jedinstveno ime fajla
            string uniqueFileName = Path.GetFileNameWithoutExtension(Guid.NewGuid().ToString("N")) + fileExtension;

            // Kreiraj putanju sa novim jedinstvenim imenom
            string filePath = Path.Combine(folderPath, uniqueFileName);

            // Sačuvaj sliku na serveru
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                product.Image.CopyTo(stream);
            }

            // Spremi ime fajla u bazi
            string imageFileName = uniqueFileName;

            // Konekcija sa bazom
            SqlConnection con = new SqlConnection(_configuration.GetConnectionString("BazaCon").ToString());
            SqlCommand cmd = new SqlCommand("INSERT INTO Product (Name, Description, Price, Category, Image) VALUES (@Name, @Description, @Price, @Category, @Image)", con);

            // Dodajte parametre za unos u bazu
            cmd.Parameters.AddWithValue("@Name", product.Name);
            cmd.Parameters.AddWithValue("@Description", product.Description);
            cmd.Parameters.AddWithValue("@Price", product.Price);
            cmd.Parameters.AddWithValue("@Category", product.Category);
            cmd.Parameters.AddWithValue("@Image", imageFileName); // Čuvanje imena fajla (putanja do slike)

            try
            {
                con.Open();
                int i = cmd.ExecuteNonQuery();
                con.Close();

                if (i > 0)
                {
                    return Ok("Product Added Successfully");
                }
                else
                {
                    return BadRequest("Error Adding Product");
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error: {ex.Message}");
            }
        }
    }
}
