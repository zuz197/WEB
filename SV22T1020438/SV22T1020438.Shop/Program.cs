using Microsoft.AspNetCore.Authentication.Cookies;
using SV22T1020438.Shop.DAL;

var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddControllersWithViews();

// DAL
builder.Services.AddScoped<CustomerDAL>();
builder.Services.AddScoped<ProductDAL>();
builder.Services.AddScoped<CartDAL>();
builder.Services.AddScoped<OrderDAL>();

// cookie
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(option =>
    {
        option.Cookie.Name = "LiteCommerce.Shop";
        option.LoginPath = "/Account/Login";
        option.AccessDeniedPath = "/Account/AccessDenied";
        option.ExpireTimeSpan = TimeSpan.FromDays(7);
        option.SlidingExpiration = true;
        option.Cookie.HttpOnly = true;
    });

// Session (giữ lại nếu cần captcha)
builder.Services.AddSession();

var app = builder.Build();

// Middleware
app.UseStaticFiles();
app.UseRouting();

app.UseSession(); // dùng cho captcha

// chú ý
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Product}/{action=Index}/{id?}");

app.Run();