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
    public class UserIndexModel : PageModel
    {
        public readonly UserManager<User> _userManager;
        public readonly RoleManager<IdentityRole> _roleManager;
        public UserIndexModel(UserManager<User> userManager, RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        [TempData]
        public string StatusMessage { get; set; }

        [BindProperty]
        public IEnumerable<User> UserLists { get; set; }

        public void OnGet()
        {
            UserLists = _userManager.Users.ToList();
        }

        public async Task<IActionResult> OnPostDeleteAsync(string id)
        {
            User user = await _userManager.FindByIdAsync(id);
            if (user != null && ((user.UserName != "andred9991@gmail.com" && User.Identity.Name == "andred9991@gmail.com") || !(await _userManager.IsInRoleAsync(user, "admin"))) )
            {
                IdentityResult result = await _userManager.DeleteAsync(user);
            }
            else
            {
                StatusMessage = "Error: Access Denied";
            }
            return RedirectToPage("UserIndex");
        }
    }
}
