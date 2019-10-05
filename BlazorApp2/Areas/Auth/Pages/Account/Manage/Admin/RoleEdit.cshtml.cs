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
    public class EditModel : PageModel
    {
        private readonly SignInManager<User> _signInManager;
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        [BindProperty]
        public ChangeRoleViewModel Input { get; set; }
        public EditModel(SignInManager<User> signInManager, UserManager<User> userManager, RoleManager<IdentityRole> roleManager)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _roleManager = roleManager;
        }
        public class ChangeRoleViewModel
        {
            public string UserId { get; set; }
            public string UserEmail { get; set; }
            public List<IdentityRole> AllRoles { get; set; }
            public IList<string> UserRoles { get; set; }
            public ChangeRoleViewModel()
            {
                AllRoles = new List<IdentityRole>();
                UserRoles = new List<string>();
            }
        }

        public async Task<IActionResult> OnGetAsync(string userid2)
        {
            // получаем пользователя
            User user = await _userManager.FindByIdAsync(userid2);
            if (user != null)
            {
                // получем список ролей пользователя
                var userRoles = await _userManager.GetRolesAsync(user);
                var allRoles = _roleManager.Roles.ToList();
                Input = new ChangeRoleViewModel
                {
                    UserId = user.Id,
                    UserEmail = user.Email,
                    UserRoles = userRoles,
                    AllRoles = allRoles
                };
                return Page();
            }
            TempData["StatusMessage"] = $"Error: not found user (userid is {(userid2==null?"null":"bad")})";
            return RedirectToPage("RoleUserList");
            //return NotFound();
        }

        public async Task<IActionResult> OnPostAsync(string userId, List<string> roles)
        {
            // получаем пользователя
            User user = await _userManager.FindByIdAsync(userId);
            if (user != null)
            {
                // получем список ролей пользователя
                var userRoles = await _userManager.GetRolesAsync(user);
                // получаем все роли
                var allRoles = _roleManager.Roles.ToList();
                // получаем список ролей, которые были добавлены
                var addedRoles = roles.Except(userRoles);
                // получаем роли, которые были удалены
                var removedRoles = userRoles.Except(roles);

                /*if (!((user.UserName != "andred9991@gmail.com" && User.Identity.Name == "andred9991@gmail.com") || !(await _userManager.IsInRoleAsync(user, "admin"))))
                {
                    if (addedRoles.Contains("admin") && User.Identity.Name == "andred9991@gmail.com")
                    {
                        TempData["StatusMessage"] = "Error: Access Denied";
                        return RedirectToPage("RoleUserList");
                    }
                }*/

                if(User.Identity.Name != "andred9991@gmail.com")
                {
                    if (user.UserName == "andred9991@gmail.com")
                    {
                        TempData["StatusMessage"] = "Error: superAdmin is god!!!";
                        return RedirectToPage("RoleUserList");
                    }
                    else if (addedRoles.Contains("admin") || removedRoles.Contains("admin"))
                    {
                        TempData["StatusMessage"] = "Error: Access Denied";
                        return RedirectToPage("RoleUserList");
                    }
                }
                else if(user.UserName == "andred9991@gmail.com" && removedRoles.Contains("admin"))
                {
                    TempData["StatusMessage"] = "Error: you should be admin!!!";
                    return RedirectToPage("RoleUserList");
                }

                await _userManager.AddToRolesAsync(user, addedRoles);

                await _userManager.RemoveFromRolesAsync(user, removedRoles);

                if (user.UserName == User.Identity.Name)
                {
                    await _signInManager.RefreshSignInAsync(user);
                }

                TempData["StatusMessage"] = "Good Boy :)";
                return RedirectToPage("RoleUserList");
            }

            TempData["StatusMessage"] = $"Error: not found user (userid is {(userId == null ? "null" : "bad")})";
            return RedirectToPage("RoleUserList");
        }
    }
}
