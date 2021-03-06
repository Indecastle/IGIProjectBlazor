﻿using BlazorApp2.Models;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BlazorApp2.Data
{
    public class RoleInitializer
    {
        public static async Task InitializeAsync(ApplicationContext db, UserManager<User> userManager, RoleManager<IdentityRole> roleManager)
        {
            string adminEmail = "andred9991@gmail.com";
            string password = "Qwe`123";
            if (await roleManager.FindByNameAsync("admin") == null)
            {
                await roleManager.CreateAsync(new IdentityRole("admin"));
            }
            if (await roleManager.FindByNameAsync("user") == null)
            {
                await roleManager.CreateAsync(new IdentityRole("user"));
            }
            if (await userManager.FindByNameAsync(adminEmail) == null)
            {
                User admin = new User { Email = adminEmail, UserName = adminEmail };
                IdentityResult result = await userManager.CreateAsync(admin, password);
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(admin, "admin");
                }
            }
            //db.FastFiles.Add(new FastFile
            //{
            //    Name = "test",
            //    KeyName = "test",
            //    DateTime = DateTime.Now
            //});
            await db.SaveChangesAsync();
        }
    }

    public class NotifierService
    {
        private int counter1, counter2;
        // Can be called from anywhere
        public async Task Update(string key, int value)
        {
            if (Notify != null)
            {
                await Notify.Invoke(key, value);
            }
        }

        public async void Cycle()
        {
            while (true)
            {
                counter1 += 1;
                counter2 += 2;
                await Update(counter1.ToString(), counter2);
                await Task.Delay(1000);
            }
        }

        public event Func<string, int, Task> Notify;
    }
}
