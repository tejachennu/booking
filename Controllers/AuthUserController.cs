﻿using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using BusBooking.Server.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.EntityFrameworkCore;
using System.Net.Mail;
using System.Net;
using System.Security.Cryptography;

namespace BusBooking.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthUserController : ControllerBase
    {
   
        private readonly BusBookingContext context;
        private readonly JWTSetting setting;
        private readonly IRefreshTokenGenerator tokenGenerator;

        public AuthUserController(BusBookingContext busBooking, IOptions<JWTSetting> options, IConfiguration configuration, IRefreshTokenGenerator refreshToken)
        {
            context = busBooking;
            setting = options.Value;
            tokenGenerator = refreshToken;

        }

        [NonAction]
        public TokenResponses Authenticate(string Email, Claim[] claims)
        {
            TokenResponses tokenResponses = new TokenResponses();
            var tokenkey = Encoding.UTF8.GetBytes(setting.securitykey);
            var tokenhandler = new JwtSecurityToken(
                claims: claims,
                expires: DateTime.Now.AddMinutes(1500),
                 signingCredentials: new SigningCredentials(new SymmetricSecurityKey(tokenkey), SecurityAlgorithms.HmacSha256)

                );
            tokenResponses.JWTToken = new JwtSecurityTokenHandler().WriteToken(tokenhandler);
            tokenResponses.RefreshToken = tokenGenerator.GenerateToken(Email);

            return tokenResponses;
        }



        [Route("Authenticate")]
        [HttpPost]
        public IActionResult Authenticate([FromBody] UserCred user)
        {
            if (user == null || string.IsNullOrEmpty(user.Email) || string.IsNullOrEmpty(user.password))
            {
                return BadRequest("Invalid user credentials.");
            }

            // Assuming TblUsers has Password stored as a hash and salt mechanism
            var _user = context.TblUsers
                               .FirstOrDefault(o => o.Email == user.Email && o.Password == user.password && o.IsActive == "Active");

            if (_user == null)
                return NotFound("User details mismatch or inactive.");

            var tokenhandler = new JwtSecurityTokenHandler();
            var tokenkey = Encoding.UTF8.GetBytes(setting.securitykey);

            // Create the claims for the JWT token
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
            new Claim(ClaimTypes.Name, _user.Email),
            new Claim(ClaimTypes.SerialNumber, _user.UserId.ToString()) // Make sure UserId is a string
                }),
                Expires = DateTime.Now.AddMinutes(20),  // Ideally get from configuration
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(tokenkey),
                    SecurityAlgorithms.HmacSha256
                )
            };

            var token = tokenhandler.CreateToken(tokenDescriptor);
            string finaltoken = tokenhandler.WriteToken(token);

            // Response object containing both JWT and Refresh Tokens
            TokenResponses tokenResponse = new TokenResponses
            {
                JWTToken = finaltoken,
                RefreshToken = tokenGenerator.GenerateToken(user.Email)
            };
            var response = new
            {
                Token = tokenResponse,
                User = _user
            };

            return Ok(response);
        }


        [Route("Refersh")]
        [HttpPost]
        public IActionResult Refresh([FromBody] TokenResponses token)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var securityToken = (JwtSecurityToken)tokenHandler.ReadToken(token.JWTToken);
            var Email = securityToken.Claims.FirstOrDefault(c => c.Type == "unique_name")?.Value;

            var _reftable = context.TblRefreshtokens.FirstOrDefault(o => o.Email == Email && o.RefreshToken == token.RefreshToken);
            if (_reftable == null)
            {
                return Unauthorized();
            }
            TokenResponses _result = Authenticate(Email, securityToken.Claims.ToArray());
            return Ok(_result);

        }

        [Route("Register")]
        [HttpPost]
        public async Task<IActionResult> Register([FromBody] TblNewUser user)
        {
            //Check if the user already exists
            var existingUser = await context.TblUsers.FirstOrDefaultAsync(o => o.Email == user.Email);
            if (existingUser != null)
            {
                return BadRequest("User already exists");
            }
            string verificationCode = GenerateVerificationCode();

            var existingUserInRegistrations = await context.TblRegistrations.FirstOrDefaultAsync(o => o.Email == user.Email);
            if (existingUserInRegistrations != null)
            {
                // Update existing user
                existingUserInRegistrations.MobileNumber = user.MobileNumber;
                existingUserInRegistrations.Name = user.Name;
                existingUserInRegistrations.Role = "user"; // Assuming Role can be updated, otherwise remove this line
                existingUserInRegistrations.Password = user.Password; // Hash the password before storing it
                existingUserInRegistrations.VerificationCode = GenerateVerificationCode();
                existingUserInRegistrations.CreatedDate = DateTime.Now;

                await SendVerificationEmail(user.Email, existingUserInRegistrations.VerificationCode);

                // Update the existing user in the database
                context.TblRegistrations.Update(existingUserInRegistrations);
            }
            else
            {
                // Create the new user
                var newUser = new TblRegistration
                {
                    MobileNumber = user.MobileNumber,
                    Name = user.Name,
                    Email = user.Email,
                    Role = "user",
                    Password = user.Password, // Hash the password before storing it
                    VerificationCode = GenerateVerificationCode(),
                };
                await SendVerificationEmail(user.Email, newUser.VerificationCode);

                // Add the new user to the database
                await context.TblRegistrations.AddAsync(newUser);
            }
            await context.SaveChangesAsync();
            // Return the created user
            return Ok("Created Suceefully");
        }

        [HttpPost("VerifyEmail")]
        public async Task<IActionResult> VerifyEmail([FromBody] VerificationRequest request)
        {
            // Find the user by email
            var user = await context.TblRegistrations.FirstOrDefaultAsync(u => u.Email == request.Email);
            if (user == null)
            {
                return NotFound("User not found");
            }

            // Check if verification code matches
            if (user.VerificationCode != request.VerificationCode)
            {
                return BadRequest("Invalid verification code");
            }

            TimeSpan timeOnly = user.CreatedDate.TimeOfDay;

            // Get the current time
            DateTime currentDateTime = DateTime.Now;
            TimeSpan currentTimeOnly = currentDateTime.TimeOfDay;


            // Calculate the time difference
            double timeDifference = currentTimeOnly.TotalMinutes - timeOnly.TotalMinutes;
            // Check if the time difference is greater than 5 minutes
            if (timeDifference > 5)
            {
                return BadRequest("Otp is expired");
            }

            var newUser = new TblUser
            {
                Email = user.Email,
                Name = user.Name,
                Role = "user",
                Password = user.Password,
                MobileNumber = long.Parse(user.MobileNumber),
                IsActive = "Active"
            };

            // Verify email and activate user account
            await context.TblUsers.AddAsync(newUser);
            context.TblRegistrations.Remove(user);
            await context.SaveChangesAsync();

            return Ok("Email verified successfully. Your account is now active.");
        }

        // Method to generate a random verification code
        private string GenerateVerificationCode()
        {
            const string chars = "0123456789";
            var random = new Random();
            var verificationCode = new StringBuilder();
            for (int i = 0; i < 5; i++)
            {
                verificationCode.Append(chars[random.Next(chars.Length)]);
            }
            return verificationCode.ToString();
        }
        [HttpPost("ResendOtp")]
        public async Task<IActionResult> ResendOtp([FromBody] EmailValue value)
        {
            var user = await context.TblRegistrations.FirstOrDefaultAsync(u => u.Email == value.Email);
            if (user == null)
            {
                return BadRequest("Invalid user");
            }
            var varficationCode = GenerateVerificationCode();
            user.VerificationCode = varficationCode;
            await context.SaveChangesAsync();
            await SendVerificationEmail(user.Email, varficationCode);
            return Ok("Email is sent sucessfully");
        }

        private async Task SendVerificationEmail(string email, string verificationCode)
        {
            try
            {
                if (string.IsNullOrEmpty(verificationCode) || verificationCode.Length < 5)
                {
                    var user = await context.TblRegistrations.FirstOrDefaultAsync(u => u.Email == email);
                    if (user == null) throw new Exception("User not found.");

                    var varficationCode = GenerateVerificationCode();
                    user.VerificationCode = varficationCode;

                    context.TblRegistrations.Update(user);
                    await context.SaveChangesAsync();
                    verificationCode = varficationCode; // Update to use the new code
                }

                // Gmail SMTP settings
                string smtpServer = "smtp.gmail.com";
                int smtpPort = 587; // SMTP port for TLS
                string smtpUsername = "tejachennu17@gmail.com"; // Your Gmail address
                string smtpPassword = "bkkx loba oyxv nipu"; // Your Gmail app password

                // Sender and receiver email addresses
                MailAddress from = new MailAddress(smtpUsername);
                MailAddress to = new MailAddress(email);

                // Create email message
                using (MailMessage message = new MailMessage(from, to))
                {
                    message.Subject = "Email Verification";
                    message.Body = $@"
                <html>
                <head>
                    <style>
                        body {{
                            font-family: Arial, sans-serif;
                            background-color: #f6f6f6;
                            margin: 0;
                            padding: 0;
                        }}
                        .container {{
                            width: 100%;
                            max-width: 600px;
                            margin: 0 auto;
                            background-color: #ffffff;
                            box-shadow: 0 0 10px rgba(0, 0, 0, 0.1);
                        }}
                        .header {{
                            background-color: #ff9800;
                            color: #ffffff;
                            padding: 10px;
                            text-align: center;
                        }}
                        .content {{
                            padding: 20px;
                            color: #333333;
                        }}
                        .footer {{
                            background-color: #333333;
                            color: #ffffff;
                            text-align: center;
                            padding: 10px;
                        }}
                        .button {{
                            display: inline-block;
                            padding: 10px 20px;
                            margin: 20px 0;
                            font-size: 16px;
                            color: #ffffff;
                            background-color: #ff9800;
                            text-decoration: none;
                            border-radius: 5px;
                        }}
                        @media (max-width: 600px) {{
                            .container {{
                                width: 100%;
                                padding: 10px;
                            }}
                        }}
                    </style>
                </head>
                <body>
                    <div class='container'>
                        <div class='header'>
                            <h1>Email Verification</h1>
                        </div>
                        <div class='content'>
                            <p>Hello,</p>
                            <p>Thank you for registering. Your verification code is:</p>
                            <h2>{verificationCode}</h2>
                            <p>Use this code to complete your registration.</p>
                            <a href='#' class='button'>Verify Email</a>
                        </div>
                        <div class='footer'>
                            <p>&copy; 2024 Your Company. All rights reserved.</p>
                        </div>
                    </div>
                </body>
                </html>";
                    message.IsBodyHtml = true;

                    // Create SMTP client
                    using (SmtpClient client = new SmtpClient(smtpServer, smtpPort))
                    {
                        client.UseDefaultCredentials = false;
                        client.Credentials = new NetworkCredential(smtpUsername, smtpPassword);
                        client.EnableSsl = true;

                        await client.SendMailAsync(message); // Use SendMailAsync for async operation
                    }
                }
            }
            catch (Exception ex)
            {
                // Handle exceptions
                Console.WriteLine($"Error sending email: {ex.Message}");
                throw;
            }
        }



        // Method to hash the password
        private string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                return BitConverter.ToString(hashedBytes).Replace("-", "").ToLower();
            }
        }

        public class TblNewUser
        {
            public string MobileNumber { get; set; } = null!;

            public string Name { get; set; } = null!;

            public string Email { get; set; } = null!;

            public string Password { get; set; } = null!;

        }
    }
}


