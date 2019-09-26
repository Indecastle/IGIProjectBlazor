using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using BlazorApp2.Models;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;

namespace BlazorApp2.Pages
{


    [AllowAnonymous]
    public class LoginModel : PageModel
    {
        SignInManager<User> _signInManager;
        UserManager<User> _userManager;
        [BindProperty]
        public string Email { get; set; }
        [BindProperty]
        public string Password { get; set; }
        [BindProperty]
        public bool RememberMe { get; set; }
        public LoginModel(SignInManager<User> signInManager, UserManager<User> userManager)
        {
            _signInManager = signInManager;
            _userManager = userManager;
        }
        [BindProperty(SupportsGet = true)]
        public string ReturnUrl { get; set; } = "~/";
        public async Task<IActionResult> OnPostAsync()
        {
            //string returnUrl = Url.Content("~/");
            try
            {
                // Clear the existing external cookie
                await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            }
            catch { }
            
            var user3 = await _signInManager.UserManager.FindByEmailAsync(Email);
            await _signInManager.PasswordSignInAsync(Email, Password, RememberMe, false);
            return LocalRedirect(ReturnUrl);
        }
    }
}
