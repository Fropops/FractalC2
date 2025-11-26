using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace TeamServer.UI.Services
{
    public class AuthService
    {
        private readonly string _apiKey = "lFAsXztlvBRVMr2DduUI7S2cSyIkodgC?S42aLF6-BHJD?2n1HlEQzPFn9SRGvfKrgyaXRAzkTFYR!xSkKQr6P6mOWPUitnIu8K-2dq0DEtaZ3BNX/Pzf11sBq?Dfpe9";
        private readonly string _user = "Fropops";
        private readonly string _sessionId;

        public AuthService()
        {
            _sessionId = Guid.NewGuid().ToString();
        }

        public string GenerateToken()
        {
            // generate token that is valid for 7 days
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_apiKey);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[] { new Claim("id", _user), new Claim("session", _sessionId) }),
                Expires = DateTime.UtcNow.AddDays(7),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }
}
