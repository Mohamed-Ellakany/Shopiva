using Shopiva.Seeding;

namespace Shopiva
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddControllers();
            builder.Services.AddHttpContextAccessor();

            builder.Services.AddDatabaseServices(builder.Configuration);
            builder.Services.AddAuthenticationServices(builder.Configuration);
            builder.Services.AddIdentityServices(builder.Configuration);
            builder.Services.AddEmailServices(builder.Configuration);
            builder.Services.AddRedisService(builder.Configuration);
            builder.Services.AddAppServices(builder.Configuration);
            builder.Services.AddSwaggerServices(builder.Configuration);

            builder.Services.AddCors(options =>
            {
                options.AddPolicy("PublicPolicy", policy =>
                {
                    policy.AllowAnyOrigin()
                          .AllowAnyMethod()
                          .AllowAnyHeader();
                });
            });

            var app = builder.Build();

            using (var scope = app.Services.CreateScope())
            {
                var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
                var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
                var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                // Seed admin and users with roles
                await SeedAdmin.SeedAsync(userManager);
                await SeedUsers.SeedAsync(userManager, roleManager);

                // Seed categories
                await SeedCategories.SeedAsync(dbContext);

                // Seed products after users and categories are created
                await SeedProducts.SeedAsync(userManager, dbContext);
            }

            app.UseSwagger();
            app.UseSwaggerUI();

            app.UseHttpsRedirection();

            app.UseCors("PublicPolicy");      
            app.MapStaticAssets();
            app.UseRouting();                 
            app.UseAuthentication();           
            app.UseAuthorization();            

            app.MapControllers();

            app.Run();
        }
    }
}