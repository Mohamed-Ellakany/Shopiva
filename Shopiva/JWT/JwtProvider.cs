

namespace Shopiva.JWT
{
    public class JwtProvider(IOptions<JwtOptions> jwtOptions, UserManager<ApplicationUser> userManager) : IJwtProvider
    {

        private readonly JwtOptions _jwtOptions = jwtOptions.Value;
        private readonly UserManager<ApplicationUser> _userManager = userManager;
        public async Task<(string token, int expireIn)> GenerateJwtTokenAsync(ApplicationUser user)
        {
            var roles = await _userManager.GetRolesAsync(user);

            var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id),
            new(JwtRegisteredClaimNames.Email, user.Email!),
            new(JwtRegisteredClaimNames.GivenName, user.FirstName),
            new(JwtRegisteredClaimNames.FamilyName, user.LastName),
            new(JwtRegisteredClaimNames.Jti, Guid.CreateVersion7().ToString()),

            new(ClaimTypes.NameIdentifier, user.Id)
        };

            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            var symmetricSecurityKey =
                new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(_jwtOptions.Key));

            var signingCredentials =
                new SigningCredentials(
                    symmetricSecurityKey,
                    SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _jwtOptions.Issuer,
                audience: _jwtOptions.Audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(_jwtOptions.ExpireInMinutes),
                signingCredentials: signingCredentials
            );

            return (
                token: new JwtSecurityTokenHandler().WriteToken(token),
                expireIn: _jwtOptions.ExpireInMinutes * 60
            );
        }


        public string? ValidateToken(string token)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var symmetricSecurityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtOptions.Key));

            try
            {
                tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    IssuerSigningKey = symmetricSecurityKey,
                    ValidateIssuerSigningKey = true,
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidIssuer = _jwtOptions.Issuer,
                    ValidAudience = _jwtOptions.Audience,
                    ClockSkew = TimeSpan.Zero,
                    NameClaimType = ClaimTypes.NameIdentifier,
                    RoleClaimType = ClaimTypes.Role

                }, out SecurityToken validatedToken);

                var jwtToken = (JwtSecurityToken)validatedToken;

                return jwtToken.Claims.First(x => x.Type == JwtRegisteredClaimNames.Sub).Value;

            }
            catch
            {
                return null;
            }
        }
    }
}
