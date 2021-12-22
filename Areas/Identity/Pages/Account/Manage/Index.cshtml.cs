// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SaaS.WebApp.Data;
using SaaS.WebApp.Model.Entity.Tables;
using SaaS.WebApp.Model.StaticDefinitions;
using SaaS.WebApp.Models;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace SaaS.WebApp.Areas.Identity.Pages.Account.Manage
{
    public class IndexModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ApplicationDbContext _context;
        private readonly SharedCatalogDbContext _currentContext;

        public IndexModel(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
             ApplicationDbContext _context,
             SharedCatalogDbContext _contextSharedDb)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            this._context = _context;
            this._currentContext = _contextSharedDb;

        }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public string Username { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        [TempData]
        public string StatusMessage { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        [BindProperty]
        public InputModel Input { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public class InputModel
        {
            /// <summary>
            ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>
            [Phone]
            [Display(Name = "Phone number")]
            public string PhoneNumber { get; set; }

            public string UserType { get; set; }
        }

        private async Task LoadAsync(ApplicationUser user)
        {
            var userName = await _userManager.GetUserNameAsync(user);
            var phoneNumber = await _userManager.GetPhoneNumberAsync(user);


            Username = userName;

            Input = new InputModel
            {
                PhoneNumber = phoneNumber,
                UserType = user.UserType
            };
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            await LoadAsync(user);
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            if (!ModelState.IsValid)
            {
                await LoadAsync(user);
                return Page();
            }



            //Fetching Existing Data
            List<Product> existingData = null;

            if (user.UserType != Input.UserType)
            {
                user.UserType = Input.UserType;

                switch (Input.UserType)
                {
                    // From Paid to Free
                    case "Free":

                        try
                        {
                            var t = _context.Tenants.Where(s => s.TID == user.TenantId).SingleOrDefault();

                            if (t is not null)
                            {
                                _currentContext.Database.SetConnectionString(t.ConnectionString);

                                //Fetching Existing Data
                                existingData = _currentContext.Products.Where(s => s.TenantId == user.TenantId).ToList();

                                //Removing Existing Tenant Data
                                _currentContext.RemoveRange(existingData);
                                _currentContext.SaveChanges();

                                t.Database = SD.SharedTenantDb;
                                t.ConnectionString = SD.createConnectionString(SD.MasterDbServer, SD.SharedTenantDb);
                                _context.Update(t);
                                _context.SaveChanges();


                                _currentContext.ChangeTracker.Entries().ToList().ForEach(e => e.State = EntityState.Detached);

                                _currentContext.Database.EnsureDeleted(); // Opens and disposes its own connection

                                _currentContext.SaveChanges();

                                _currentContext.Database.SetConnectionString(t.ConnectionString);

                                if (existingData.Count > 0)
                                {

                                    existingData.ForEach(x =>
                                    {
                                        x.Id = 0;
                                    });

                                    // Move Data
                                    _currentContext.AddRange(existingData);
                                    _currentContext.SaveChanges();
                                }

                                await _userManager.UpdateAsync(user);
                            }
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine("****************************");
                            Debug.WriteLine(ex.Message);
                            Debug.WriteLine("****************************");

                        }

                        break;

                    // From Free to Paid
                    case "Paid":

                        try
                        {
                            var tenant = _context.Tenants.Where(s => s.TID == user.TenantId).SingleOrDefault();

                            if (tenant is not null)
                            {
                                var str = tenant.ConnectionString;

                                tenant.Database = user.TenantId;
                                tenant.ConnectionString = SD.createConnectionString(SD.MasterDbServer, tenant.Database);
                                _context.Update(tenant);
                                _context.SaveChanges();


                                _currentContext.Database.SetConnectionString(str);

                                if (_currentContext.Products.Any(s => s.TenantId == user.TenantId))
                                {
                                    existingData = _currentContext.Products.Where(s => s.TenantId == user.TenantId).ToList();

                                    //Removing Existing Tenant Data
                                    _currentContext.RemoveRange(existingData);
                                    _currentContext.SaveChanges();

                                }

                                _currentContext.Database.SetConnectionString(tenant.ConnectionString);

                                if (_currentContext.Database.GetMigrations().Count() > 0)
                                {
                                    _currentContext.Database.Migrate();

                                    if (existingData.Count > 0)
                                    {
                                        existingData.ForEach(x =>
                                        {
                                            x.Id = 0;
                                        });


                                        // Move Data
                                        _currentContext.AddRange(existingData);
                                        _currentContext.SaveChanges();
                                    }


                                }

                                await _userManager.UpdateAsync(user);
                            }

                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine("****************************");
                            Debug.WriteLine(ex.Message);
                            Debug.WriteLine("****************************");
                        }



                        break;
                }


                HttpContext.Session.SetString("UserType", user.UserType);
            }

            var phoneNumber = await _userManager.GetPhoneNumberAsync(user);

            if (Input.PhoneNumber != phoneNumber)
            {
                var setPhoneResult = await _userManager.SetPhoneNumberAsync(user, Input.PhoneNumber);

                if (!setPhoneResult.Succeeded)
                {
                    StatusMessage = "Unexpected error when trying to set phone number.";
                    return RedirectToPage();
                }
            }

            await _signInManager.RefreshSignInAsync(user);
            StatusMessage = "Your profile has been updated";
            return RedirectToPage();
        }
    }
}
