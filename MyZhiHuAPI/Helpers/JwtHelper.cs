using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace MyZhiHuAPI.Helpers;

public class JwtHelper(IConfiguration config)
{
    public string CreateToken(int userId)
    {
        var claims = new[]
        {
            new Claim(ClaimTypes.Name, "u_admin"),
            new Claim(ClaimTypes.Role, "r_admin"),
            new Claim(JwtRegisteredClaimNames.Jti, "admin"),
            new Claim("UserId", userId.ToString())
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["Jwt:SecretKey"]!));

        var sign = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: config["Jwt:Issuer"],
            audience: config["Jwt:Audience"],
            claims: claims,
            notBefore: DateTime.Now,
            expires: DateTime.Now.AddSeconds(10),
            signingCredentials: sign
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

}
