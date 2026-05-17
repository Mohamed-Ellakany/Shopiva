using SharpGrip.FluentValidation.AutoValidation.Mvc.Extensions;
namespace Shopiva
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddAuthenticationServices(this IServiceCollection services, IConfiguration configuration)
            {
                services.AddScoped<IAuthService, AuthService>();
                services.AddScoped<IJwtProvider, JwtProvider>();
            services.AddAuthorization();
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
                            IssuerSigningKey = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(JwtSettings?.Key!)),

                            NameClaimType = ClaimTypes.NameIdentifier,

                            RoleClaimType = ClaimTypes.Role
                        };
                    });
                return services;
        }

        public static IServiceCollection AddDatabaseServices(this IServiceCollection services, IConfiguration configuration)
            {
                services.AddDbContext<AppDbContext>(options =>
                    options.UseSqlServer(
                        configuration.GetConnectionString("DefaultConnection")));

                return services;
        }

        public static IServiceCollection AddIdentityServices(this IServiceCollection services, IConfiguration configuration)
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

        public static IServiceCollection AddSwaggerServices(this IServiceCollection services, IConfiguration configuration)
                {
                    services.AddSwaggerGen();
                    return services;
                }

                public static IServiceCollection AddAppServices(this IServiceCollection services, IConfiguration configuration)
                {
                    services.AddScoped<IProductService, ProductService>();
                    services.AddScoped<ICategoryService, CategoryService>();
                    services.AddScoped<IReviewService, ReviewService>();
                    services.AddScoped<IImageService, ImageService>();
                    return services;
                }
            }
}
