using Dapper;
using Microsoft.Extensions.Configuration;
using SV22T1020438.Shop.Models;

namespace SV22T1020438.Shop.DAL
{
    public class OrderDAL : BaseDAL
    {
        public OrderDAL(IConfiguration config) : base(config) { }

        // Lấy danh sách đơn hàng
        public List<Order> List(int customerId)
        {
            using var conn = OpenConnection();

            string sql = @"
SELECT OrderID, CustomerID, OrderTime, DeliveryAddress, Status
FROM Orders
WHERE CustomerID = @cid
ORDER BY OrderTime DESC";

            return conn.Query<Order>(sql, new { cid = customerId }).ToList();
        }

        // Chi tiết đơn hàng
        public List<OrderDetail> GetDetails(int orderId)
        {
            using var conn = OpenConnection();

            string sql = @"
SELECT 
    od.OrderID,
    od.ProductID,
    od.Quantity,
    od.SalePrice,
    p.ProductName
FROM OrderDetails od
JOIN Products p ON od.ProductID = p.ProductID
WHERE od.OrderID = @oid";

            return conn.Query<OrderDetail>(sql, new { oid = orderId }).ToList();
        }

        // XÓA ĐƠN HÀNG 
        public void Delete(int orderId)
        {
            using var conn = OpenConnection();

            using var tran = conn.BeginTransaction();

            try
            {
                conn.Execute("DELETE FROM OrderDetails WHERE OrderID = @id",
                    new { id = orderId }, tran);

                conn.Execute("DELETE FROM Orders WHERE OrderID = @id",
                    new { id = orderId }, tran);

                tran.Commit();
            }
            catch
            {
                tran.Rollback();
                throw;
            }
        }

        //  THÊM: HUỶ ĐƠN 
        public bool CancelOrder(int orderId)
        {
            using var conn = OpenConnection();

            string sql = @"
UPDATE Orders
SET Status = -1
WHERE OrderID = @id";

            return conn.Execute(sql, new { id = orderId }) > 0;
        }
    }
}