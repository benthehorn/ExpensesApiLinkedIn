using ExpensesApi.Data;
using ExpensesApi.Models;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Web.Http;
using System.Web.Http.Cors;

namespace ExpensesApi.Controllers
{
    [EnableCors("http://localhost:4200", "*", "*" )]
    [RoutePrefix("auth")]
    public class AuthenticationController : ApiController
    {
        [Route("login")]
        [HttpPost]
        public IHttpActionResult Login([FromBody]User user)
        {
            if (string.IsNullOrEmpty(user.UserName) || string.IsNullOrEmpty(user.Password))
                return BadRequest("The username or password was missing");

            try
            {
                using (var context = new AppDbContext())
                {
                    var exists = context.Users.Any(n => n.UserName == user.UserName && n.Password == user.Password);
                    if (exists) return Ok(CreateToken(user));

                    return BadRequest("The credentials were wrong");
                }
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [Route("register")]
        [HttpPost]
        public IHttpActionResult Register([FromBody]User user)
        {
            try
            {
                using (var context = new AppDbContext())
                {
                    var exists = context.Users.Any(n => n.UserName == user.UserName);
                    if (exists) return BadRequest("User already exists");

                    var hashValues = hashPassword(user.Password);
                    user.Salt = hashValues[0];
                    user.Hash = hashValues[1];
                    context.Users.Add(user);
                    context.SaveChanges();

                    var tok = CreateToken(user);
                    return Ok(tok);
                }

            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        private JwtPackage CreateToken(User user)
        {
            var tokenHandler =  new JwtSecurityTokenHandler();

            var claims = new ClaimsIdentity(new[] {
                new Claim(ClaimTypes.Email, user.UserName)
            });

            const string secretKey = "benssecret the long version for the key";
            var securityKey = new SymmetricSecurityKey(Encoding.Default.GetBytes(secretKey));
            var signinCredentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256Signature);

            var token = (JwtSecurityToken)tokenHandler.CreateJwtSecurityToken(
                subject: claims,
                signingCredentials: signinCredentials
                );

            var tokenString = tokenHandler.WriteToken(token);

            return new JwtPackage()
            {
                UserName = user.UserName,
                Token = tokenString
            };
        }
        private string[] hashPassword(string password)
        {
            string[] hashed = new string[2];
            var saltBytes = new byte[64];
            var provider = new RNGCryptoServiceProvider();
            provider.GetNonZeroBytes(saltBytes);
            var salt = Convert.ToBase64String(saltBytes);

            var rfcDeriveBytes = new Rfc2898DeriveBytes(password, saltBytes, 10000);
            var hash = Convert.ToBase64String(rfcDeriveBytes.GetBytes(256));
            hashed[0] = salt;
            hashed[1] = hash;
          
            return hashed;
        }
    }
}

public class JwtPackage
{
    public string Token { get; set; }
    public string UserName { get; set; }
}
