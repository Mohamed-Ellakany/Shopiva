
using SharpGrip.FluentValidation.AutoValidation.Mvc.Extensions;


namespace Shopiva
{
    public static class DependencyInjection
    {
        
        
        extension(IServiceCollection services)
        {
            public  IServiceCollection AddAuthenticationServices( IConfiguration configuration)
            {
                services.AddScoped<IAuthService, AuthService>();
                services.AddSingleton<IJwtProvider, JwtProvider>();
                services.Configure<JwtOptions>(configuration.GetSection(JwtOptions.SectionName));

                var JwtSettings = configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>();


                services.AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;

                })
                    .AddJwtBearer(options =>
                    {
                        options.SaveToken = true;
                        options.TokenValidationParameters = new TokenValidationParameters
                        {
                            ValidateIssuer = true,
                            ValidateAudience = true,
                            ValidateLifetime = true,
                            ValidateIssuerSigningKey = true,
                            ValidAudience = JwtSettings?.Audience,
                            ValidIssuer = JwtSettings?.Issuer,
                            IssuerSigningKey = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(JwtSettings?.Key!))
                        };
                    });
                return services;
            }




            public IServiceCollection AddDatabaseServices(IConfiguration configuration)
            {
                services.AddDbContext<AppDbContext>(options =>
                    options.UseSqlServer(
                        configuration.GetConnectionString("DefaultConnection")));

                return services;
            }


            public  IServiceCollection AddIdentityServices(IConfiguration configuration)
            {

                services
                    .AddFluentValidationAutoValidation()
                    .AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

                services.AddIdentity<ApplicationUser, IdentityRole>()
                    .AddEntityFrameworkStores<AppDbContext>()
                    .AddDefaultTokenProviders();

                services.Configure<IdentityOptions>(options =>
                {
                    options.Password.RequireDigit = true;
                    options.Password.RequireLowercase = true;
                    options.Password.RequireNonAlphanumeric = false;
                    options.Password.RequireUppercase = true;
                    options.Password.RequiredLength = 8;
                    options.Password.RequiredUniqueChars = 1;
                    options.User.RequireUniqueEmail = true;
                    options.SignIn.RequireConfirmedEmail = false;
                    options.SignIn.RequireConfirmedPhoneNumber = false;

                });
                return services;

            }

            public  IServiceCollection AddSwaggerServices(IConfiguration configuration)
            {
                services.AddSwaggerGen();
                return services;
            }
        }

        

       
       

    }
}
