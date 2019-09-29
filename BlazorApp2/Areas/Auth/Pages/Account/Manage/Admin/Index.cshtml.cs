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
    public class IndexModel : PageModel
    {
        private readonly SignInManager<User> _signInManager;
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public IndexModel(SignInManager<User> signInManager, UserManager<User> userManager, RoleManager<IdentityRole> roleManager)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public async Task<IActionResult> OnPostDeleteAsync(string id)
        {
            IdentityRole role = await _roleManager.FindByIdAsync(id);
            if (role != null)
            {
                if (role.Name != "admin" && role.Name != "user")
                {
                    IdentityResult result = await _roleManager.DeleteAsync(role);
                }
                else
                {
                    ModelState.AddModelError(string.Empty, $"нельзя удалить {role.Name}");
                }
            }
            return RedirectToPage("./Index");
        }
    }
}
