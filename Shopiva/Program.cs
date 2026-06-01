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
                await SeedAdmin.SeedAsync(userManager);
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