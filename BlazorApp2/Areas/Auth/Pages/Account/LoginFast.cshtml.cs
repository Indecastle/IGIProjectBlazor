using BlazorApp2.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BlazorApp2.Areas.Auth.Pages.Account
{
    [IgnoreAntiforgeryToken]
    public class LoginFast : PageModel
    {
        private readonly SignInManager<User> _signInManager;

        public LoginFast(SignInManager<User> signInManager)
        {
            _signInManager = signInManager;
        }

        public async Task<IActionResult> OnGetAsync(string paramUsername, string paramPassword)
        {
            string returnUrl = Url.Content("~/");
            try
            {
                if (_signInManager.IsSignedIn(User))
                {
                    await _signInManager.SignOutAsync();
                }
            }
            catch { }
            var user3 = await _signInManager.UserManager.FindByEmailAsync(paramUsername);
            if (user3 != null)
                await _signInManager.PasswordSignInAsync(user3, paramPassword, true, true);
            return LocalRedirect(returnUrl);
        }

        public async Task<IActionResult> OnPostAsync(string username, string password)
        {
            //Console.WriteLine(username + ",  " + password);
            string returnUrl = Url.Content("~/");
            try
            {
                if (_signInManager.IsSignedIn(User))
                {
                    await _signInManager.SignOutAsync();
                }
            }
            catch { }
            var user3 = await _signInManager.UserManager.FindByEmailAsync(username);
            if (user3 != null)
                await _signInManager.PasswordSignInAsync(user3, password, true, true);
            return LocalRedirect(returnUrl);
        }
    }
}
