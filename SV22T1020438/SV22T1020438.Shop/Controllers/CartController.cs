using Microsoft.AspNetCore.Mvc;
using SV22T1020438.Shop.DAL;

namespace SV22T1020438.Shop.Controllers
{
    public class CartController : Controller
    {
        private readonly CartDAL cartDAL;
        private readonly ProductDAL productDAL;

        public CartController(CartDAL cDal, ProductDAL pDal)
        {
            cartDAL = cDal;
            productDAL = pDal;
        }

        public IActionResult Index()
        {
            var idStr = HttpContext.Session.GetString("CustomerID");

            if (string.IsNullOrEmpty(idStr))
                return RedirectToAction("Login", "Account");

            int customerId = int.Parse(idStr);

            var cart = cartDAL.Get(customerId);

            return View(cart);
        }

        public IActionResult Add(int productId)
        {
            var idStr = HttpContext.Session.GetString("CustomerID");

            if (string.IsNullOrEmpty(idStr))
                return RedirectToAction("Login", "Account");

            int customerId = int.Parse(idStr);

            var product = productDAL.Get(productId);

            if (product == null)
                return Content("Sản phẩm không tồn tại");

            cartDAL.Add(customerId, productId, 1);

            return RedirectToAction("Index");
        }

        public IActionResult Remove(int productId)
        {
            var idStr = HttpContext.Session.GetString("CustomerID");

            if (string.IsNullOrEmpty(idStr))
                return RedirectToAction("Login", "Account");

            int customerId = int.Parse(idStr);

            cartDAL.Remove(customerId, productId);

            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult Update(int productId, int quantity)
        {
            var idStr = HttpContext.Session.GetString("CustomerID");

            if (string.IsNullOrEmpty(idStr))
                return RedirectToAction("Login", "Account");

            int customerId = int.Parse(idStr);

            cartDAL.Update(customerId, productId, quantity);

            return RedirectToAction("Index");
        }

        // XOÁ TOÀN BỘ GIỎ HÀNG
        [HttpPost]
        public IActionResult ClearCart()
        {
            var idStr = HttpContext.Session.GetString("CustomerID");

            if (string.IsNullOrEmpty(idStr))
                return RedirectToAction("Login", "Account");

            int customerId = int.Parse(idStr);

            cartDAL.Clear(customerId);

            return RedirectToAction("Index");
        }

        // CHECKOUT
        [HttpPost]
        public IActionResult Checkout()
        {
            var idStr = HttpContext.Session.GetString("CustomerID");

            if (string.IsNullOrEmpty(idStr))
                return RedirectToAction("Login", "Account");

            int customerId = int.Parse(idStr);

            try
            {
                int orderId = cartDAL.Checkout(customerId);
                return Content("Đặt hàng thành công! Mã đơn: " + orderId);
            }
            catch (Exception ex)
            {
                return Content("Lỗi: " + ex.Message);
            }
        }
    }
}