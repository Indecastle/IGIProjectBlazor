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
    public class UserEditModel : PageModel
    {
        private readonly UserManager<User> _userManager;

        public UserEditModel(UserManager<User> userManager)
        {
            _userManager = userManager;
        }

        [BindProperty]
        public EditUserViewModel Input { get; set; }

        public class EditUserViewModel
        {
            public string Id { get; set; }
            public string Email { get; set; }
            public int Year { get; set; }
        }

        public async Task<IActionResult> OnGetAsync(string id)
        {
            User user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }
            Input = new EditUserViewModel { Id = user.Id, Email = user.Email, Year = user.Year };
            return Page();
        }

        public async Task<IActionResult> OnPostAsync(string id)
        {
            if (ModelState.IsValid)
            {
                User user = await _userManager.FindByIdAsync(Input.Id);
                if (user != null)
                {
                    user.Email = Input.Email;
                    user.UserName = Input.Email;
                    user.Year = Input.Year;

                    var result = await _userManager.UpdateAsync(user);
                    if (result.Succeeded)
                    {
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
            }
            return Page();
        }
    }
}
