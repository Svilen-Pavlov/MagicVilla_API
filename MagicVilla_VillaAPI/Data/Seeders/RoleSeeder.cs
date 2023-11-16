using MagicVilla_VillaAPI.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;

namespace MagicVilla_VillaAPI.Data.Seeders
{
    public class RoleSeeder
    {
  


        public static async Task Initialize(IServiceProvider serviceProvider)
        {
            using (var scope = serviceProvider.CreateScope())
            {
                var context = scope.ServiceProvider.GetService<ApplicationDbContext>();
                var roleManager = scope.ServiceProvider.GetService<RoleManager<IdentityRole>>();
                string[] roles = new string[] { "admin", "customer" };
          
                foreach (var role in roles)
                {
                    if (await roleManager.RoleExistsAsync(role) == false)
                    {
                        await roleManager.CreateAsync(new IdentityRole(role));
                    }
                }
                await context.SaveChangesAsync();
            }
        }

    }
}
