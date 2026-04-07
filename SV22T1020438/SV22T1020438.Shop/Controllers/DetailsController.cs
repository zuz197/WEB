using Microsoft.AspNetCore.Mvc;
using SV22T1020438.Shop.DAL;

namespace SV22T1020438.Shop.Controllers
{
    public class DetailsController : Controller
    {
        private readonly ProductDAL productDAL;

        public DetailsController(ProductDAL dal)
        {
            productDAL = dal;
        }

        public IActionResult Index(int id)
        {
            var product = productDAL.Get(id);

            if (product == null)
                return Content("Không tìm thấy sản phẩm");

            return View(product);
        }
    }
}