using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.OpenApi;
using SharpGrip.FluentValidation.AutoValidation.Mvc.Extensions;
using Shopiva.Abstractions.Options;
using Shopiva.Interfaces.Cart;
using Shopiva.Interfaces.Redis;
using Shopiva.Interfaces.UnitOfWork;
using StackExchange.Redis;

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
                    options.SignIn.RequireConfirmedEmail = true;
                    options.SignIn.RequireConfirmedPhoneNumber = false;

            });


            return services;
        }

        public static IServiceCollection AddSwaggerServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddSwaggerGen(options=>
            {
                options.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "My API",
                    Version = "v1"
                });

                options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    Type = SecuritySchemeType.Http,
                    Scheme = "Bearer",
                    BearerFormat = "JWT",
                    In = ParameterLocation.Header,
                    Description = "Enter your token"
                });

                options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
                {
                    [new OpenApiSecuritySchemeReference("bearer", document)] = []
                });
            });
            return services;
        }

        //public static IServiceCollection AddRedisService(this IServiceCollection services, IConfiguration configuration)
        //{
        //    var redisSection = configuration.GetSection("Redis");

        //    var configOptions = new ConfigurationOptions
        //    {
        //        EndPoints = { { redisSection["Host"]!, int.Parse(redisSection["Port"]!) } },
        //        User = redisSection["User"],
        //        Password = redisSection["Password"],
        //        AbortOnConnectFail = false,   // ← don't crash app if Redis is down
        //        ConnectTimeout = 5000,
        //        SyncTimeout = 5000,
        //    };

        //    var multiplexer = ConnectionMultiplexer.Connect(configOptions);
        //    services.AddSingleton<IConnectionMultiplexer>(multiplexer);
        //    services.AddScoped<IRedisService, RedisService>();

        //    // ── Health Check ───────────────────────────────────────────
        //    services.AddHealthChecks()
        //        .AddCheck("redis", () =>
        //        {
        //            try
        //            {
        //                var db = multiplexer.GetDatabase();
        //                db.Ping();
        //                return HealthCheckResult.Healthy("Redis is reachable.");
        //            }
        //            catch (Exception ex)
        //            {
        //                return HealthCheckResult.Unhealthy("Redis is unreachable.", ex);
        //            }
        //        });

        //    return services;
        //}

        public static IServiceCollection AddRedisService(this IServiceCollection services, IConfiguration configuration)
        {


            var redisSection = configuration.GetSection("Redis");
            services.AddSingleton<IConnectionMultiplexer>(ConnectionMultiplexer.Connect(
                    new ConfigurationOptions
                    {
                        EndPoints = { { redisSection["Host"]!, int.Parse(redisSection["Port"]!) } },
                        User = redisSection["User"],
                        Password = redisSection["Password"],
                        AbortOnConnectFail = false,
                    }
            ));

            return services;
        }

        public static IServiceCollection AddEmailServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<EmailOptions>(configuration.GetSection(EmailOptions.SectionName));
            services.AddScoped<IEmailService, EmailService>();
            return services;
        }

        public static IServiceCollection AddAppServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddScoped<IUnitOfWork, UnitOfWork.UnitOfWork>();
            services.AddScoped<IRedisService, RedisService>();
            services.AddScoped<IProductService, ProductService>();
            services.AddScoped<ICategoryService, CategoryService>();
            services.AddScoped<IReviewService, ReviewService>();
            services.AddScoped<IImageService, ImageService>();
            services.AddScoped<ICartService, CartService>();
            return services;
        }
    }
}
