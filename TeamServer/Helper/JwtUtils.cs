using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using TeamServer.Models;

namespace TeamServer.Helper
{
    public interface IJwtUtils
    {
        public string GenerateToken(User user);
        public UserContext ValidateToken(string token);
    }

    public class UserContext
    {
        public UserContext(User user, string session)
        {
            User = user;
            Session = session;
        }
        public User User { get; set; }
        public string Session { get; private set; }
    }

    public class JwtUtils : IJwtUtils
    {
        private readonly IUserService _userService;

        public JwtUtils(IUserService userService)
        {
            _userService = userService;
        }

        public string GenerateToken(User user)
        {
            // generate token that is valid for 7 days
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(user.Key);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[] { new Claim("id", user.Id.ToString()) }),
                Expires = DateTime.UtcNow.AddDays(7),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        public UserContext ValidateToken(string token)
        {
            if (token == null)
                return null;

            var tokenHandler = new JwtSecurityTokenHandler();
            //var key = Encoding.ASCII.GetBytes(_appSettings.Secret);
            try
            {
                var rt = tokenHandler.ReadJwtToken(token);
                if (!rt.Payload.ContainsKey("id"))
                    return null;
                if (!rt.Payload.ContainsKey("session"))
                    return null;
                var userId = rt.Payload["id"].ToString();
                var session = rt.Payload["session"].ToString();
                var user = _userService.GetUser(userId);
                if (user == null)
                    return null;

                tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(user.Key)),
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    // set clockskew to zero so tokens expire exactly at token expiration time (instead of 5 minutes later)
                    ClockSkew = TimeSpan.Zero
                }, out SecurityToken validatedToken);

                //var jwtToken = (JwtSecurityToken)validatedToken;
                //userId = jwtToken.Claims.First(x => x.Type == "id").Value;

                // return user id from JWT token if validation successful
                return new UserContext(user, session);
            }
            catch
            {
                // return null if validation fails
                return null;
            }
        }
    }
}
