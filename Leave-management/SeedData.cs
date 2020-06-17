﻿using Leave_management.Data;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Leave_management
{
    public static class SeedData
    {
        public static void Seed(UserManager<Employee> userManager, RoleManager<IdentityRole> roleManager)
        {
            SeedRoles(roleManager);
            SeedUsers(userManager);
        }

        private async static void SeedUsers(UserManager<Employee> userManager)
        {
            if(userManager.FindByNameAsync("admin").Result == null)
            {
                var user = new Employee { UserName = "admin@localhost.com", Email = "admin@localhost.com" };
                var result = userManager.CreateAsync(user, "Password@123").Result.Succeeded;
                if (result)
                {
                    await userManager.AddToRoleAsync(user, "Administrator");
                }
            }
        }

        private async static void SeedRoles(RoleManager<IdentityRole> roleManager)
        {
            if (!roleManager.RoleExistsAsync("Administrator").Result)
            {
                var role = new IdentityRole { Name = "Administrator" };
                await roleManager.CreateAsync(role);
            }
            if (!roleManager.RoleExistsAsync("Employee").Result)
            {
                var role = new IdentityRole { Name = "Employee" };
                await roleManager.CreateAsync(role);
            }
        }
    }
}
