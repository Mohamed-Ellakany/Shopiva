namespace Shopiva.Data
{
    public static class SeedUsers
    {
        public static async Task SeedAsync(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            // Seed Customer Users
            var customerUsers = new List<(string username, string email, string firstName, string lastName, string phone, string address, string city, string country)>
            {
                ("ahmed.hassan", "ahmed@shopiva.com", "Ahmed", "Hassan", "01001234567", "123 Main Street", "Cairo", "Egypt"),
                ("leila.samir", "leila@shopiva.com", "Leila", "Samir", "01334567890", "321 Elm Street", "Cairo", "Egypt")
            };

            foreach (var user in customerUsers)
            {
                var existingUser = await userManager.FindByNameAsync(user.username);
                if (existingUser == null)
                {
                    var newUser = new ApplicationUser
                    {
                        UserName = user.username,
                        Email = user.email,
                        FirstName = user.firstName,
                        LastName = user.lastName,
                        PhoneNumber = user.phone,
                        Address = user.address,
                        City = user.city,
                        Country = user.country,
                        EmailConfirmed = true
                    };

                    await userManager.CreateAsync(newUser, "Password@123");
                    await userManager.AddToRoleAsync(newUser, "Customer");
                }
            }

            // Seed Seller Users
            var sellerUsers = new List<(string username, string email, string firstName, string lastName, string phone, string address, string city, string country)>
            {
                ("fatima.ali", "fatima@shopiva.com", "Fatima", "Ali", "01112345678", "456 Oak Avenue", "Alexandria", "Egypt"),
                ("mohamed.ibrahim", "mohamed@shopiva.com", "Mohamed", "Ibrahim", "01223456789", "789 Pine Road", "Giza", "Egypt"),
                ("omar.ahmed", "omar@shopiva.com", "Omar", "Ahmed", "01445678901", "654 Maple Drive", "Helwan", "Egypt")
            };

            foreach (var user in sellerUsers)
            {
                var existingUser = await userManager.FindByNameAsync(user.username);
                if (existingUser == null)
                {
                    var newUser = new ApplicationUser
                    {
                        UserName = user.username,
                        Email = user.email,
                        FirstName = user.firstName,
                        LastName = user.lastName,
                        PhoneNumber = user.phone,
                        Address = user.address,
                        City = user.city,
                        Country = user.country,
                        EmailConfirmed = true
                    };

                    await userManager.CreateAsync(newUser, "Password@123");
                    await userManager.AddToRoleAsync(newUser, "Seller");
                }
            }
        }
    }
}
