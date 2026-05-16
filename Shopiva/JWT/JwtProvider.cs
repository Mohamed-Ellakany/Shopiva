

namespace Shopiva.JWT
{ 
    public class JwtProvider(IOptions<JwtOptions> jwtOptions) : IJwtProvider
    {
        
        private readonly JwtOptions _jwtOptions = jwtOptions.Value;

        public (string token, int expireIn) GenerateJwtToken(ApplicationUser user)
        {
            Claim[] claims = [
                 
                new (JwtRegisteredClaimNames.Sub , user.Id),
                new (JwtRegisteredClaimNames.Email , user.Email!),
                new (JwtRegisteredClaimNames.GivenName , user.FirstName),
                new (JwtRegisteredClaimNames.FamilyName , user.LastName),
                new (JwtRegisteredClaimNames.Jti , Guid.CreateVersion7().ToString())

                ];

            var symmetricSecurityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtOptions.Key));

            var signingCredentials = new SigningCredentials(symmetricSecurityKey, SecurityAlgorithms.HmacSha256);

            
            var token = new JwtSecurityToken(
                issuer: _jwtOptions.Issuer,
                audience: _jwtOptions.Audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(_jwtOptions.ExpireInMinutes) ,
                signingCredentials: signingCredentials
                );

           
            return (token: new JwtSecurityTokenHandler().WriteToken(token) , expireIn: _jwtOptions.ExpireInMinutes*60);

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
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ValidIssuer = _jwtOptions.Issuer,
                    ValidAudience = _jwtOptions.Audience,
                    ClockSkew = TimeSpan.Zero
                }, out SecurityToken validatedToken);

                var jwtToken = (JwtSecurityToken)validatedToken;

                return jwtToken.Claims.First(x=>x.Type== JwtRegisteredClaimNames.Sub).Value ;
                
            }
            catch
            {
                return null;
            }
        }
    }
}
