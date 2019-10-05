using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BlazorApp2.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BlazorApp2.Areas.Auth.Pages.Account.Manage.Admin
{
    public class UserCreateModel : PageModel
    {
        private readonly UserManager<User> _userManager;

        public UserCreateModel(UserManager<User> userManager)
        {
            _userManager = userManager;
        }

        [BindProperty]
        public CreateUserViewModel Input { get; set; }

        public class CreateUserViewModel
        {
            public string Email { get; set; }
            public string Password { get; set; }
            public int Year { get; set; }
        }

        public void OnGetAsync()
        { }

        public async Task<IActionResult> OnPostAsync()
        {
            if (ModelState.IsValid)
            {
                User user = new User { Email = Input.Email, UserName = Input.Email, Year = Input.Year };
                var result = await _userManager.CreateAsync(user, Input.Password);
                if (result.Succeeded)
                {
                    await _userManager.AddToRoleAsync(user, "user");
                    return RedirectToPage("UserIndex");
                }
                else
                {
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                }
            }
            return Page();
        }
    }
}
