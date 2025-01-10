using backend.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Data;
using System.Data.SqlClient;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RegistrationController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        public RegistrationController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpPost]
        [Route("registration")]
        public string registration(Registration registration)
        {
            SqlConnection con = new SqlConnection(_configuration.GetConnectionString("BazaCon").ToString());
            SqlCommand cmd = new SqlCommand("INSERT INTO Registration (UserName, Password, Email, IsActive) VALUES (@UserName, @Password, @Email, @IsActive)", con);
            cmd.Parameters.AddWithValue("@UserName", registration.UserName);
            cmd.Parameters.AddWithValue("@Password", registration.Password);
            cmd.Parameters.AddWithValue("@Email", registration.Email);
            cmd.Parameters.AddWithValue("@IsActive", registration.IsActive);

            con.Open();
            int i = cmd.ExecuteNonQuery();
            con.Close();
            if (i > 0)
            {
                return "Data Inserted";
            }
            else
            {
                return "Error";
            }
        }

        [HttpPost]
        [Route("login")]
        public IActionResult login(Login registration)
        {
            SqlConnection con = new SqlConnection(_configuration.GetConnectionString("BazaCon").ToString());
            SqlDataAdapter da = new SqlDataAdapter(
                "SELECT * FROM Registration WHERE Email=@Email AND Password=@Password AND IsActive=1", con);
            da.SelectCommand.Parameters.AddWithValue("@Email", registration.Email);
            da.SelectCommand.Parameters.AddWithValue("@Password", registration.Password);
            DataTable dt = new DataTable();
            da.Fill(dt);

            if (dt.Rows.Count > 0)
            {
                // Generisanje JWT tokena
                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.ASCII.GetBytes(_configuration["JwtSettings:SecretKey"]);
                var tokenDescriptor = new SecurityTokenDescriptor
                {
                    Subject = new ClaimsIdentity(new[] {
                        new Claim(ClaimTypes.Email, registration.Email),
                        new Claim("ID", dt.Rows[0]["ID"].ToString()) // Dodaj ID korisnika iz baze
                    }),
                    Expires = DateTime.UtcNow.AddHours(1), // Token važi 1 sat
                    SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
                };

                var token = tokenHandler.CreateToken(tokenDescriptor);
                var tokenString = tokenHandler.WriteToken(token);

                // Vraćanje tokena i ID-a korisnika
                return Ok(new
                {
                    Token = tokenString,
                    UserId = dt.Rows[0]["ID"].ToString(),  // Dodavanje ID korisnika u odgovor
                    Message = "Login successful"
                });
            }
            else
            {
                return Unauthorized(new { Message = "Invalid credentials" });
            }
        }

    }
}
