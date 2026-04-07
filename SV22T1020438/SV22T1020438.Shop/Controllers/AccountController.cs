using Microsoft.AspNetCore.Mvc;
using SV22T1020438.Shop.DAL;
using SV22T1020438.Shop.Models;
using SV22T1020438.Shop.AppCodes;

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;

namespace SV22T1020438.Shop.Controllers
{
    public class AccountController : Controller
    {
        private readonly CustomerDAL db;
        private readonly ProductDAL productDAL;

        public AccountController(CustomerDAL dal, ProductDAL pDal)
        {
            db = dal;
            productDAL = pDal;
        }

        // ================= REGISTER =================
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(Customer model, string PasswordConfirm)
        {
            if (!ModelState.IsValid) return View(model);

            var exist = db.GetByEmail(model.Email ?? "");
            if (exist != null)
            {
                ViewBag.Error = "Email đã tồn tại";
                return View(model);
            }

            if (string.IsNullOrWhiteSpace(model.Password) || model.Password != PasswordConfirm)
            {
                ViewBag.Error = "Mật khẩu và xác nhận không khớp";
                return View(model);
            }

            model.Password = CryptHelper.MD5(model.Password);
            model.IsLocked = false;

            int id = db.Add(model);
            if (id > 0)
            {
                // Session (cũ)
                HttpContext.Session.SetString("CustomerID", id.ToString());
                HttpContext.Session.SetString("CustomerName", model.CustomerName ?? "");

                // Cookie (mới)
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, model.CustomerName ?? ""),
                    new Claim(ClaimTypes.NameIdentifier, id.ToString()),
                    new Claim(ClaimTypes.Role, "Shop")
                };

                var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var principal = new ClaimsPrincipal(identity);

                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

                return RedirectToAction("Index", "Product");
            }

            ViewBag.Error = "Đăng ký thất bại";
            return View(model);
        }

        // ================= LOGIN =================
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(string email, string password, string captcha)
        {
            int failCount = HttpContext.Session.GetInt32("LoginFail_Shop") ?? 0;

            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            {
                ViewBag.Error = "Nhập email và mật khẩu";
                return View();
            }

            if (failCount >= 3)
            {
                var sessionCaptcha = HttpContext.Session.GetString("Captcha_Shop");

                if (string.IsNullOrWhiteSpace(captcha) || captcha != sessionCaptcha)
                {
                    failCount++;
                    HttpContext.Session.SetInt32("LoginFail_Shop", failCount);
                    ViewBag.Error = "Sai mã captcha";
                    return View();
                }
            }

            var user = db.GetByEmail(email);
            string hash = CryptHelper.MD5(password);

            if (user != null && user.Password != null && user.Password.Trim() == hash.Trim())
            {
                if (user.IsLocked == true)
                {
                    ViewBag.Error = "Tài khoản bị khoá";
                    return View();
                }

                HttpContext.Session.Remove("LoginFail_Shop");
                HttpContext.Session.Remove("Captcha_Shop");

                // Session (cũ)
                HttpContext.Session.SetString("CustomerID", user.CustomerID.ToString());
                HttpContext.Session.SetString("CustomerName", user.CustomerName ?? "");

                // Cookie (mới)
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, user.CustomerName ?? ""),
                    new Claim(ClaimTypes.NameIdentifier, user.CustomerID.ToString()),
                    new Claim(ClaimTypes.Role, "Shop")
                };

                var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var principal = new ClaimsPrincipal(identity);

                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

                return RedirectToAction("Index", "Product");
            }

            failCount++;
            HttpContext.Session.SetInt32("LoginFail_Shop", failCount);

            ViewBag.Error = "Sai tài khoản hoặc mật khẩu";
            return View();
        }

        // ================= CAPTCHA =================
        [HttpGet]
        public IActionResult GenerateCaptcha()
        {
            var code = new Random().Next(1000, 9999).ToString();
            HttpContext.Session.SetString("Captcha_Shop", code);
            return Json(new { captcha = code });
        }

        // ================= LOGOUT =================
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }

        // ================= PROFILE =================
        public IActionResult Profile()
        {
            int? id = GetCurrentUserId();
            if (id == null) return RedirectToAction("Login");

            var user = db.Get(id.Value);
            if (user == null) return RedirectToAction("Login");

            return View(user);
        }

        [HttpPost]
        public IActionResult Profile(Customer model)
        {
            int? id = GetCurrentUserId();
            if (id == null) return RedirectToAction("Login");

            var user = db.Get(id.Value);
            if (user == null) return RedirectToAction("Login");

            user.CustomerName = model.CustomerName;
            user.ContactName = model.ContactName;
            user.Address = model.Address;
            user.Province = model.Province;
            user.Phone = model.Phone;

            db.Update(user);

            ViewBag.Message = "Cập nhật thành công";
            return View(user);
        }

        // ================= CHANGE PASSWORD =================
        public IActionResult ChangePassword()
        {
            int? id = GetCurrentUserId();
            if (id == null) return RedirectToAction("Login");

            return View();
        }

        [HttpPost]
        public IActionResult ChangePassword(string currentPassword, string newPassword, string confirmPassword)
        {
            int? id = GetCurrentUserId();
            if (id == null) return RedirectToAction("Login");

            var user = db.Get(id.Value);
            if (user == null) return RedirectToAction("Login");

            string currentHash = CryptHelper.MD5(currentPassword);

            if (user.Password == null || user.Password.Trim() != currentHash.Trim())
            {
                ViewBag.Error = "Mật khẩu hiện tại không đúng";
                return View();
            }

            if (string.IsNullOrWhiteSpace(newPassword) || newPassword != confirmPassword)
            {
                ViewBag.Error = "Mật khẩu mới và xác nhận không khớp";
                return View();
            }

            string newHash = CryptHelper.MD5(newPassword);
            var success = db.ChangePassword(id.Value, newHash);

            if (success)
                ViewBag.Message = "Đổi mật khẩu thành công";
            else
                ViewBag.Error = "Đổi mật khẩu thất bại";

            return View();
        }

        // ================= HELPER =================
        private int? GetCurrentUserId()
        {
            // Ưu tiên Cookie
            var claim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (claim != null && !string.IsNullOrEmpty(claim.Value))
            {
                if (int.TryParse(claim.Value, out int id))
                    return id;
            }

            // fallback Session
            var idStr = HttpContext.Session.GetString("CustomerID");
            if (!string.IsNullOrEmpty(idStr))
            {
                if (int.TryParse(idStr, out int id))
                    return id;
            }

            return null;
        }
    }
}