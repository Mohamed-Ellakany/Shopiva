namespace Shopiva.Data
{
    public static class SeedAdmin
    {
        public static async Task SeedAsync(UserManager<ApplicationUser> userManager)
        {

            var adminUser = await userManager.FindByNameAsync("admin");
            if (adminUser == null)
            {
                var admin = new ApplicationUser
                {
                    UserName = "admin",
                    Email = "admin@example.com",
                    FirstName = "Admin",
                    LastName = "User"
                };

                await userManager.CreateAsync(admin, "Admin@123");
                await userManager.AddToRoleAsync(admin, "Admin");

            }

        }
    }
}