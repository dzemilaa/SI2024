using backend.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Data;
using System.Data.SqlClient;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using System.Net.Mail;
using System.Net;
using System.Threading.Tasks;

namespace backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RegistrationController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly SmtpSettings _smtpSettings;

        public RegistrationController(IConfiguration configuration, IOptions<SmtpSettings> smtpSettings)
        {
            _configuration = configuration;
            _smtpSettings = smtpSettings.Value;
        }


        // Generiše JWT token za verifikaciju emaila
        private string GenerateVerificationToken(string email)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_configuration["JwtSettings:SecretKey"]);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[] {
                    new Claim(ClaimTypes.Email, email),
                }),
                Expires = DateTime.UtcNow.AddHours(1), // Token važi 1 sat
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        // Slanje verifikacionog emaila korisniku
        private async Task SendVerificationEmail(string email, string token)
        {
            var verifyUrl = $"{_configuration["AppSettings:BaseUrl"]}/verify?token={token}";

            var smtpClient = new SmtpClient(_smtpSettings.Host)
            {
                Port = _smtpSettings.Port,
                Credentials = new NetworkCredential(_smtpSettings.Username, _smtpSettings.Password),
                EnableSsl = true
            };

            var mailMessage = new MailMessage
            {
                From = new MailAddress(_smtpSettings.FromEmail),
                Subject = "Verify your email",
                Body = $"Click the following link to verify your email: {verifyUrl}",
                IsBodyHtml = true
            };

            mailMessage.To.Add(email);


            try
            {
                // Pošaljite email
                await smtpClient.SendMailAsync(mailMessage);
            }
            catch (Exception ex)
            {
                // Obrada greške u slanju email-a
                throw new Exception($"Error sending verification email: {ex.Message}");
            }
        }

        // Registracija korisnika
        [HttpPost]
        [Route("registration")]
        public async Task<IActionResult> registration(Registration registration)
        {
            SqlConnection con = new SqlConnection(_configuration.GetConnectionString("BazaCon").ToString());
            SqlCommand cmd = new SqlCommand("INSERT INTO Registration (UserName, Password, Email, IsActive, IsVerified) VALUES (@UserName, @Password, @Email, @IsActive, @IsVerified)", con);
            cmd.Parameters.AddWithValue("@UserName", registration.UserName);
            cmd.Parameters.AddWithValue("@Password", registration.Password);
            cmd.Parameters.AddWithValue("@Email", registration.Email);
            cmd.Parameters.AddWithValue("@IsActive", registration.IsActive);
            cmd.Parameters.AddWithValue("@IsVerified", false);  // Korisnik je inicijalno ne-verifikovan

            con.Open();
            int i = cmd.ExecuteNonQuery();
            con.Close();

            if (i > 0)
            {
                // Generiši verifikacioni token
                var verificationToken = GenerateVerificationToken(registration.Email);

                // Pošaljite email sa linkom za verifikaciju
                await SendVerificationEmail(registration.Email, verificationToken);

                return Ok(new { Message = "Registration successful. Please check your email to verify your account." });
            }
            else
            {
                return BadRequest(new { Message = "Error during registration." });
            }
        }

        [HttpGet]
        [Route("verify")]
        public IActionResult VerifyEmail(string token)
        {
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.ASCII.GetBytes(_configuration["JwtSettings:SecretKey"]);
                var principal = tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ClockSkew = TimeSpan.Zero
                }, out var validatedToken);

                var email = principal.FindFirstValue(ClaimTypes.Email);

                SqlConnection con = new SqlConnection(_configuration.GetConnectionString("BazaCon").ToString());
                SqlCommand cmd = new SqlCommand("UPDATE Registration SET IsVerified = 1 WHERE Email = @Email", con);
                cmd.Parameters.AddWithValue("@Email", email);

                con.Open();
                int rowsUpdated = cmd.ExecuteNonQuery();
                con.Close();

                if (rowsUpdated > 0)
                {
                    // Generisanje login token-a
                    var loginToken = GenerateLoginToken(email); // Generiši token za login
                    return Ok(new { message = "Email successfully verified!", loginToken });
                }
                else
                {
                    return BadRequest(new { message = "Verification failed. Please try again later." });
                }
            }
            catch (Exception)
            {
                return BadRequest(new { message = "Invalid or expired token." });
            }
        }

        // Generiši token za automatski login
        private string GenerateLoginToken(string email)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_configuration["JwtSettings:SecretKey"]);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[] {
            new Claim(ClaimTypes.Email, email),
        }),
                Expires = DateTime.UtcNow.AddHours(1),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }



        // Logovanje korisnika
        [HttpPost]
        [Route("login")]
        public IActionResult login(Login registration)
        {
            SqlConnection con = new SqlConnection(_configuration.GetConnectionString("BazaCon").ToString());
            SqlDataAdapter da = new SqlDataAdapter(
                "SELECT * FROM Registration WHERE Email=@Email AND Password=@Password AND IsActive=1 AND IsVerified=1", con);
            da.SelectCommand.Parameters.AddWithValue("@Email", registration.Email);
            da.SelectCommand.Parameters.AddWithValue("@Password", registration.Password);
            DataTable dt = new DataTable();
            da.Fill(dt);

            if (dt.Rows.Count > 0)
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.ASCII.GetBytes(_configuration["JwtSettings:SecretKey"]);
                var tokenDescriptor = new SecurityTokenDescriptor
                {
                    Subject = new ClaimsIdentity(new[] {
                        new Claim(ClaimTypes.Email, registration.Email),
                        new Claim("ID", dt.Rows[0]["ID"].ToString())
                    }),
                    Expires = DateTime.UtcNow.AddHours(1),
                    SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
                };

                var token = tokenHandler.CreateToken(tokenDescriptor);
                var tokenString = tokenHandler.WriteToken(token);

                return Ok(new
                {
                    Token = tokenString,
                    UserId = dt.Rows[0]["ID"].ToString(),
                    Message = "Login successful"
                });
            }
            else
            {
                return Unauthorized(new { Message = "Invalid credentials or email not verified." });
            }
        }
    }
}
