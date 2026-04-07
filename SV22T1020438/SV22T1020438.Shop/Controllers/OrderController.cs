using Microsoft.AspNetCore.Mvc;
using SV22T1020438.Shop.DAL;

namespace SV22T1020438.Shop.Controllers
{
    public class OrderController : Controller
    {
        private readonly OrderDAL orderDAL;
        private readonly CartDAL cartDAL;

        public OrderController(OrderDAL oDal, CartDAL cDal)
        {
            orderDAL = oDal;
            cartDAL = cDal;
        }

        //  LỊCH SỬ ĐƠN HÀNG
        public IActionResult Index()
        {
            var idStr = HttpContext.Session.GetString("CustomerID");

            if (string.IsNullOrEmpty(idStr))
                return RedirectToAction("Login", "Account");

            int customerId = int.Parse(idStr);

            var orders = orderDAL.List(customerId);

            return View(orders);
        }

        //  CHI TIẾT ĐƠN
        public IActionResult Details(int id)
        {
            var idStr = HttpContext.Session.GetString("CustomerID");

            if (string.IsNullOrEmpty(idStr))
                return RedirectToAction("Login", "Account");

            int customerId = int.Parse(idStr);

            var data = orderDAL.GetDetails(id);

            if (data == null || data.Count == 0)
                return Content("Đơn hàng không tồn tại");

            ViewBag.OrderID = id;

            var order = orderDAL.List(customerId)
                                .FirstOrDefault(o => o.OrderID == id);

            ViewBag.Status = order?.Status ?? 0;

            return View(data);
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
                return RedirectToAction("Details", new { id = orderId });
            }
            catch (Exception ex)
            {
                return Content("Lỗi khi thanh toán: " + ex.Message);
            }
        }

        [HttpPost]
        public IActionResult Cancel(int id)
        {
            orderDAL.CancelOrder(id); // 🔥 đổi sang update status
            return RedirectToAction("Index");
        }
    }
}