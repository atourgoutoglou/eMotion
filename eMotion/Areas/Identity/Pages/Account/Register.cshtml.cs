// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

namespace eMotion.Areas.Identity.Pages.Account
{
    public class RegisterModel : PageModel
    {
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IUserStore<IdentityUser> _userStore;
        private readonly IUserEmailStore<IdentityUser> _emailStore;
        private readonly ILogger<RegisterModel> _logger;
        private readonly IEmailSender _emailSender;
        private readonly IConfiguration _configuration;

        public RegisterModel(
            UserManager<IdentityUser> userManager,
            IUserStore<IdentityUser> userStore,
            SignInManager<IdentityUser> signInManager,
            ILogger<RegisterModel> logger,
            IEmailSender emailSender,
            IConfiguration configuration)
        {
            _userManager = userManager;
            _userStore = userStore;
            _emailStore = GetEmailStore();
            _signInManager = signInManager;
            _logger = logger;
            _emailSender = emailSender;
            _configuration = configuration;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public string ReturnUrl { get; set; }

        public IList<AuthenticationScheme> ExternalLogins { get; set; }

        public class InputModel
        {
            [Required(ErrorMessage = "Το όνομα χρήστη είναι υποχρεωτικό.")]
            [StringLength(30, MinimumLength = 3, ErrorMessage = "Το όνομα χρήστη πρέπει να έχει από 3 έως 30 χαρακτήρες.")]
            [Display(Name = "Όνομα Χρήστη")]
            public string Username { get; set; } = string.Empty;

            [Required(ErrorMessage = "Το email είναι υποχρεωτικό.")]
            [EmailAddress(ErrorMessage = "Δώσε έγκυρο email.")]
            [Display(Name = "Email")]
            public string Email { get; set; } = string.Empty;

            [Required(ErrorMessage = "Ο κωδικός είναι υποχρεωτικός.")]
            [StringLength(100, ErrorMessage = "Το {0} πρέπει να έχει τουλάχιστον {2} και το πολύ {1} χαρακτήρες.", MinimumLength = 6)]
            [DataType(DataType.Password)]
            [Display(Name = "Κωδικός")]
            public string Password { get; set; } = string.Empty;

            [Required(ErrorMessage = "Η επιβεβαίωση κωδικού είναι υποχρεωτική.")]
            [DataType(DataType.Password)]
            [Display(Name = "Επιβεβαίωση κωδικού")]
            [Compare("Password", ErrorMessage = "Ο κωδικός και η επιβεβαίωση δεν ταιριάζουν.")]
            public string ConfirmPassword { get; set; } = string.Empty;

            [Required(ErrorMessage = "Επίλεξε τύπο χρήστη.")]
            [Display(Name = "Τύπος Χρήστη")]
            public string Role { get; set; } = "Student";

            [Display(Name = "Μυστικός Κωδικός Καθηγητή")]
            public string TeacherSecretCode { get; set; }
        }

        public async Task OnGetAsync(string returnUrl = null)
        {
            ReturnUrl = returnUrl;
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

            if (!ModelState.IsValid)
            {
                return Page();
            }

            var existingUserByUsername = await _userManager.FindByNameAsync(Input.Username);
            if (existingUserByUsername != null)
            {
                ModelState.AddModelError(string.Empty, "Το όνομα χρήστη χρησιμοποιείται ήδη.");
                return Page();
            }

            var existingUserByEmail = await _userManager.FindByEmailAsync(Input.Email);
            if (existingUserByEmail != null)
            {
                ModelState.AddModelError(string.Empty, "Το email χρησιμοποιείται ήδη.");
                return Page();
            }

            if (Input.Role == "Teacher" && Input.TeacherSecretCode != (_configuration["Registration:TeacherSecretCode"] ?? "TEACHER123"))
            {
                ModelState.AddModelError(string.Empty, "Λάθος μυστικός κωδικός καθηγητή.");
                return Page();
            }

            var user = CreateUser();

            await _userStore.SetUserNameAsync(user, Input.Username, CancellationToken.None);
            await _emailStore.SetEmailAsync(user, Input.Email, CancellationToken.None);

            var result = await _userManager.CreateAsync(user, Input.Password);

            if (result.Succeeded)
            {
                _logger.LogInformation("User created a new account with password.");

                if (Input.Role == "Teacher")
                {
                    await _userManager.AddToRoleAsync(user, "Teacher");
                }
                else
                {
                    await _userManager.AddToRoleAsync(user, "Student");
                }

                await _signInManager.SignInAsync(user, isPersistent: false);
                return LocalRedirect(returnUrl);
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return Page();
        }

        private IdentityUser CreateUser()
        {
            try
            {
                return Activator.CreateInstance<IdentityUser>();
            }
            catch
            {
                throw new InvalidOperationException(
                    $"Can't create an instance of '{nameof(IdentityUser)}'. " +
                    $"Ensure that '{nameof(IdentityUser)}' is not an abstract class and has a parameterless constructor, " +
                    $"or alternatively override the register page in /Areas/Identity/Pages/Account/Register.cshtml");
            }
        }

        private IUserEmailStore<IdentityUser> GetEmailStore()
        {
            if (!_userManager.SupportsUserEmail)
            {
                throw new NotSupportedException("The default UI requires a user store with email support.");
            }

            return (IUserEmailStore<IdentityUser>)_userStore;
        }
    }
}