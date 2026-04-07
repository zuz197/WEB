using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SV22T1020438.BusinessLayers;
using SV22T1020438.Admin.Models;
using SV22T1020438.Models.Security;

namespace SV22T1020438.Admin.Controllers
{
    [Authorize]
    public class AccountController : Controller
    {
        [HttpGet]
        public IActionResult ChangePassword()
        {
            return View(new AccountChangePasswordViewModel());
        }

        [HttpPost]
        public async Task<IActionResult> ChangePassword(AccountChangePasswordViewModel model)
        {
            if (string.IsNullOrWhiteSpace(model.OldPassword))
                ModelState.AddModelError(nameof(model.OldPassword), "Vui lòng nhập mật khẩu cũ");
            if (string.IsNullOrWhiteSpace(model.NewPassword))
                ModelState.AddModelError(nameof(model.NewPassword), "Vui lòng nhập mật khẩu mới");
            if (model.NewPassword != model.ConfirmPassword)
                ModelState.AddModelError(nameof(model.ConfirmPassword), "Xác nhận mật khẩu không khớp");

            var userData = User.GetUserData();
            if (userData == null || string.IsNullOrWhiteSpace(userData.UserId) || !int.TryParse(userData.UserId, out int employeeId))
                return RedirectToAction(nameof(Login));

            if (!ModelState.IsValid)
                return View(model);

            var oldHash = CryptHelper.HashMD5(model.OldPassword);
            var newHash = CryptHelper.HashMD5(model.NewPassword);
            bool ok = await HRDataService.ChangeEmployeePasswordAsync(employeeId, oldHash, newHash);
            if (!ok)
            {
                ModelState.AddModelError("Error", "Mật khẩu cũ không đúng hoặc tài khoản không tồn tại");
                return View(model);
            }

            HttpContext.Session.Clear();
            await HttpContext.SignOutAsync();
            TempData["Message"] = "Đổi mật khẩu thành công. Vui lòng đăng nhập lại.";
            return RedirectToAction(nameof(Login));
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Login() 
        {
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Login(string username, string password, string captcha) 
        {
            ViewBag.Username = username;

            int failCount = HttpContext.Session.GetInt32("LoginFail") ?? 0;

            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                ModelState.AddModelError("Error", "Nhập đủ tên và mật khẩu");
                return View();
            }

            // kiểm tra captcha nếu >= 3 lần sai
            if (failCount >= 3)
            {
                var sessionCaptcha = HttpContext.Session.GetString("Captcha");

                if (string.IsNullOrWhiteSpace(captcha) || captcha != sessionCaptcha)
                {
                    failCount++;
                    HttpContext.Session.SetInt32("LoginFail", failCount);

                    ModelState.AddModelError("Error", "Sai mã captcha");
                    return View();
                }
            }

            password = CryptHelper.HashMD5(password);

            var userAccount = await SecurityDataSerer.EmployeeAuthorizeAsync(username, password);
            if (userAccount == null)
            {
                failCount++;
                HttpContext.Session.SetInt32("LoginFail", failCount);

                ModelState.AddModelError("Error", "Đăng nhập thất bại");
                return View();
            }

            // thành công
            HttpContext.Session.Remove("LoginFail");
            HttpContext.Session.Remove("Captcha");

            var userData = new WebUserData()
            {
                UserId = userAccount.UserId,
                UserName = userAccount.UserName,
                DisplayName= userAccount.DisplayName,
                Email = userAccount.Email,
                Photo = userAccount.Photo,
                Roles = (userAccount.RoleNames ?? "")
                    .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                    .ToList()
            };

            await HttpContext.SignInAsync
            (
                CookieAuthenticationDefaults.AuthenticationScheme,
                userData.CreatePrincipal()
            );

            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult GenerateCaptcha()
        {
            var code = new Random().Next(1000, 9999).ToString();
            HttpContext.Session.SetString("Captcha", code);
            return Json(new { captcha = code });
        }

        public async Task<IActionResult> Logout()
        {
            HttpContext.Session.Clear();
            await HttpContext.SignOutAsync();
            return RedirectToAction("Login");
        }

        public IActionResult AccessDenied() 
        { 
            return View(); 
        }
    }
}