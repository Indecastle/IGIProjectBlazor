﻿using System;
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
        public LoginModel(SignInManager<User> signInManager, UserManager<User> userManager)
        {
            _signInManager = signInManager;
            _userManager = userManager;
        }
        public string ReturnUrl { get; set; }
        public async Task<IActionResult> OnGetAsync(string paramUsername, string paramPassword)
        {
            string returnUrl = Url.Content("~/");
            try
            {
                // Clear the existing external cookie
                await HttpContext
                    .SignOutAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme);
            }
            catch { }
            // *** !!! This is where you would validate the user !!! ***
            // In this example we just log the user in
            // (Always log the user in for this demo)
            /*var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, paramUsername),
                new Claim(ClaimTypes.Role, "Administrator"),
            };
            var claimsIdentity = new ClaimsIdentity(
                claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var authProperties = new AuthenticationProperties
            {
                IsPersistent = true,
                RedirectUri = this.Request.Host.Value
            };
            try
            {
                await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity),
                authProperties);
            }
            catch (Exception ex)
            {
                string error = ex.Message;
            }*/
            var user3 = await _signInManager.UserManager.FindByEmailAsync(paramUsername);
            await _signInManager.SignInAsync(user3, false);
            return LocalRedirect(returnUrl);
        }
    }
}
