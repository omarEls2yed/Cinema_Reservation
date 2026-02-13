using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DomainLayer.Models;
using Microsoft.AspNetCore.Identity;
using Persistence.identity;
using ServiceAbstraction;

namespace Service
{
    public class DataSeeding(
        UserManager<ApplicationUser>_userManager,
        RoleManager<IdentityRole>_roleManager,
        EventIdentityDbContext _dbContext) : IDataSeeding
    {
        public async Task IdentityDataSeedAsync()
        {
            try
            {
                if (!_roleManager.Roles.Any())
                {
                    await _roleManager.CreateAsync(new IdentityRole("Admin"));
                    await _roleManager.CreateAsync(new IdentityRole("User"));
                    await _roleManager.CreateAsync(new IdentityRole("Manager"));
                }
                if (!_userManager.Users.Any())
                {
                    var User1 = new ApplicationUser()
                    {
                        Email = "Mohamed123@gmail.com",
                        PhoneNumber = "12345678912",
                        UserName = "MohamedTarek",
                    };
                    var User2 = new ApplicationUser()
                    {
                        Email = "Omar123@gmail.com",
                        PhoneNumber = "12555678912",
                        UserName = "OmarTarek",
                    };
                    await _userManager.CreateAsync(User1, "P@ssw0rd");
                    await _userManager.CreateAsync(User2, "P@ssw0rd");
                    await _userManager.AddToRoleAsync(User1, "Admin");
                    await _userManager.AddToRoleAsync(User2, "User");
                }
                await _dbContext.SaveChangesAsync();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }
    }
}
