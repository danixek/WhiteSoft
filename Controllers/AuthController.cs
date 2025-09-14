using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using WhiteSoft.Models;
using WhiteSoft.Services;
using ConfigurationManager = System.Configuration.ConfigurationManager;

namespace WhiteSoft.Controllers
{
    public class AuthController : Controller
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ILogService _logService;
        public AuthController(
            SignInManager<ApplicationUser> signInManager, UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager, ILogService logService)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _roleManager = roleManager;
            _logService = logService;
        }

        /*  Pro autentizaci jsem použil ASP.NET Identity, protože poskytuje bezpečné a ověřené řešení;
         *  Microsoft by přeci k tomu nezaměstnával amatéry - ne?.
         * 
         *  Hesla se nikdy neukládají přímo, ale pouze ve formě hashů. Pokud bych implementoval vlastní ukládání hesel,
         *  ukládal bych také pouze hash a při přihlašování bych kontroloval shodu hashů.
         *
         *  Použití hashů místo šifrování zajišťuje, že původní heslo nelze zpětně získat
         *  – je to jako znát násobení, ale již neznát dělení.
         */

        [HttpGet]
        public IActionResult Login() => View();
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel LoginModel)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByNameAsync(LoginModel.Email);
                if (user != null)
                {
                    await _signInManager.SignOutAsync();

                    // XSS protection
                    user.ApplyXssProtection();

                    var result = await _signInManager.PasswordSignInAsync(user, LoginModel.Password, false, false);

                    if (result.RequiresTwoFactor)
                    {
                        var twoFactorModel = new TwoFactorViewModel
                        {
                            IsSetupMode = false // false = jen login, ne nastavení
                        };
                        return View("2FA", twoFactorModel);
                    }

                    if (result.Succeeded)
                    {
                        TempData["Success"] = $"Úspěšně přihlášen.";
                        return RedirectToAction("Index", "Home");
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, "Neplatné přihlašovací údaje.");
                        TempData["Error"] = $"Přihlášení selhalo.";
                        return View(LoginModel);
                    }
                }

                TempData["Error"] = $"Přihlášení selhalo.";
                ModelState.AddModelError(nameof(LoginModel.Email), "Invalid user name or password");
            }

            return View(LoginModel);
        }

        [HttpGet]
        public IActionResult Register() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            var isAdminCreating = User.IsInRole("Admin") || User.IsInRole("superadmin");
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = new ApplicationUser
            {
                UserName = model.Email!,
                Email = model.Email!,
                FirstName = model.FirstName!,
                LastName = model.LastName!
            };
            // XSS protection
            user.ApplyXssProtection();

            var username = model.FirstName + " " + model.LastName;

            var existingUser = await _userManager.FindByNameAsync(model.Email!);
            if (existingUser != null)
            {
                ModelState.AddModelError("Username", "Účet s tímto E-mailem již existuje.");
                return View(model);
            }
            var result = await _userManager.CreateAsync(user, model.Password!);

            if (result.Succeeded)
            {

                // Zkontroluj, zda je to první uživatel v systému
                var usersCount = await _userManager.Users.CountAsync();
                if (usersCount == 1)
                {
                    // Zkontroluj, že role "superadmin" existuje
                    if (!await _roleManager.RoleExistsAsync("superadmin"))
                    {
                        await _roleManager.CreateAsync(new IdentityRole("superadmin"));
                    }

                    // Přidej roli superadmin
                    await _userManager.AddToRoleAsync(user, "superadmin");
                    await _logService.LogAsync("Warning", $"Uživatel {username} se úspěšně registroval, a byly mu přiděleny superadmin práva.", username);

                }
                else
                {
                    var roleToAssign = model.Role == "admin" ? "admin" : "user";

                    // Zkontroluj, že role "user" existuje
                    if (!await _roleManager.RoleExistsAsync(roleToAssign))
                    {
                        await _roleManager.CreateAsync(new IdentityRole(roleToAssign));
                    }

                    // Přidělení role "user" (či "admin") nově registrovanému uživateli
                    await _userManager.AddToRoleAsync(user, roleToAssign);
                    if (!isAdminCreating)
                    {
                        await _logService.LogAsync("Success",
                            $"Uživatel {username} se úspěšně registroval. Počet uživatelů v DB: {usersCount}",
                            username);
                    }
                    else
                    {
                        await _logService.LogAsync("Warning",
                            $"Superadmin zaregistroval nový účet ({roleToAssign}) - {username}. Počet uživatelů v DB: {usersCount}",
                            User.Identity?.Name);
                    }
                }

                // Přihlášení uživatele po úspěšné registraci
                // V případě registraci superadminem k automatickému přihlášení nedojde
                if (!isAdminCreating)
                {
                    // Uživatel se registruje sám – přihlásíme ho a přesměrujeme
                    await _signInManager.SignInAsync(user, isPersistent: false);
                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    // Superadmin registruje – nechceme redirect ani automatické přihlášení
                    // Můžeš dát třeba ViewBag nebo TempData pro potvrzení úspěchu
                    ViewBag.Message = "Uživatel úspěšně vytvořen administrátorem.";
                    return RedirectToAction("Index", "Admin");
                }
            }

            return View(model);
        }

        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            await _logService.LogAsync("Info", $"Uživatel {User.Identity?.Name} se odhlásil.", User.Identity?.Name);
            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        [ActionName("Settings")]
        public async Task<IActionResult> UserSettings(SettingsViewModel model)
        {
            ModelState.Clear();
            var id = UserIdOfActualUser();
            if (id == null) return NotFound();

            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            // XSS protection
            user.ApplyXssProtection();

            if (!ModelState.IsValid) {
                return View(model);
            }
            model.FirstName = user.FirstName;
            model.LastName = user.LastName;
            model.Email = user.Email;
            model.TOTP = await _userManager.GetTwoFactorEnabledAsync(user);

            return View(model);
        }

        [HttpPost]
        [ActionName("Settings")]
        public async Task<IActionResult> ChangeSettings(SettingsViewModel model)
        {
            var id = UserIdOfActualUser();
            if (id == null) return NotFound();

            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Aktualizace dat uživatele
            user.FirstName = model.FirstName!;
            user.LastName = model.LastName!;
            user.Email = model.Email!;
            user.UserName = model.Email!;

            // XSS protection
            user.ApplyXssProtection();

            await _userManager.UpdateAsync(user);
            TempData["Success"] = $"Uživatel {model.Email} si změnil nastavení.";
            return View(model);
        }

        [HttpPost]
        [ActionName("Edit")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditUserByAdmin(string id, EditViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            // Aktualizace dat uživatele
            user.FirstName = model.FirstName!;
            user.LastName = model.LastName!;
            user.Email = model.Email!;
            user.UserName = model.Email!;

            // XSS protection
            user.ApplyXssProtection();

            var result = await _userManager.UpdateAsync(user);

            if (result.Succeeded)
            {
                await _logService.LogAsync("Warning",
                    $"Uživatel {user.UserName} byl zeditován superadminem.",
                    User.Identity?.Name);

                TempData["Success"] = $"Uživatel {user.UserName} byl zeditován.";
                return RedirectToAction("ManageUsers", "Admin");
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error.Description);
            }

            return View(model);
        }

        public async Task<IActionResult> Reset2FA()
        {
            var user = await _userManager.FindByIdAsync(UserIdOfActualUser()!);
            if (user == null) return NotFound();

            TempData["Success"] = "2FA klíč byl smazán a nahrazen novým.";

            await _userManager.ResetAuthenticatorKeyAsync(user);

            return RedirectToAction("2FA", "Auth");
        }
    
        public async Task<IActionResult> Disable2FA()
        {
            var user = await _userManager.FindByIdAsync(UserIdOfActualUser()!);
            if (user == null) return NotFound();

            await _userManager.SetTwoFactorEnabledAsync(user, false);

            return RedirectToAction("2FA", "Auth");
        }

        [HttpGet]
        [ActionName("2fa")]
        public async Task<IActionResult> Activate2FA()
        {
            var user = await _userManager.FindByIdAsync(UserIdOfActualUser()!);
            if (user == null) return NotFound();

            string? getKey = await _userManager.GetAuthenticatorKeyAsync(user);
            if (string.IsNullOrEmpty(getKey))
            {
                await _userManager.ResetAuthenticatorKeyAsync(user); // vygeneruje nový klíč
                getKey = await _userManager.GetAuthenticatorKeyAsync(user);
            }

            var email = await _userManager.GetEmailAsync(user);
            var totpUri = $"otpauth://totp/{ConfigurationManager.AppSettings["WhiteSoft"]}:{email}?secret={getKey}&issuer={ConfigurationManager.AppSettings["WhiteSoft"]}&digits=6";
            
            // QR code pomocí QRCoder
            using var qrGenerator = new QRCoder.QRCodeGenerator();
            var qrCodeData = qrGenerator.CreateQrCode(totpUri, QRCoder.QRCodeGenerator.ECCLevel.Q);
            var qrCode = new QRCoder.PngByteQRCode(qrCodeData);
            var qrCodeImage = qrCode.GetGraphic(20);

            var model = new TwoFactorViewModel
            {
                SharedKey = getKey,
                AuthenticatorUri = totpUri,
                QrCodeImageUrl = Convert.ToBase64String(qrCodeImage),
                TOTP = await _userManager.GetTwoFactorEnabledAsync(user),
                IsSetupMode = true
            };
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Verify2FA(TwoFactorViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View("2FA", model);
            }

            var user = model.IsSetupMode ? await _userManager.FindByIdAsync(UserIdOfActualUser()!)
                : await _signInManager.GetTwoFactorAuthenticationUserAsync();
            if (user == null) return NotFound();

            // Remove spaces and dashes
            var verificationCode = model.Code!.Replace(" ", string.Empty).Replace("-", string.Empty);

            var is2faTokenValid = await _userManager.VerifyTwoFactorTokenAsync(
                user, _userManager.Options.Tokens.AuthenticatorTokenProvider, verificationCode);

            if (!is2faTokenValid)
            {
                TempData["Error"] = "Neplatný kód. Zkuste to znovu.";
                return View("2FA", model);
            }

            if (model.IsSetupMode)
            {
                // Activation 2FA during setup in settings
                await _userManager.SetTwoFactorEnabledAsync(user, true);
                TempData["Success"] = "2FA bylo úspěšně aktivováno.";
                return RedirectToAction("Settings", "Auth");
            }
            else
            {
                // Login with 2FA
                await _signInManager.SignInAsync(user, isPersistent: false);
                TempData["Success"] = "Úspěšně přihlášen s 2FA.";
                return RedirectToAction("Index", "Home");
            }
        }

        public string? UserIdOfActualUser() => User.FindFirstValue(ClaimTypes.NameIdentifier);

    }
}
