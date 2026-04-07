using Dapper;
using Microsoft.Extensions.Configuration;
using SV22T1020438.Shop.Models;
using System.Data;

namespace SV22T1020438.Shop.DAL
{
    public class ProductDAL : BaseDAL
    {
        public ProductDAL(IConfiguration config) : base(config) { }

        public List<Product> List(string search, int? categoryId, decimal? minPrice, decimal? maxPrice)
        {
            using var conn = OpenConnection();

            string sql = @"
SELECT * FROM Products
WHERE (@search = '' OR ProductName LIKE '%' + @search + '%')
AND (@categoryId = 0 OR CategoryID = @categoryId)
AND (@minPrice IS NULL OR Price >= @minPrice)
AND (@maxPrice IS NULL OR Price <= @maxPrice)
";

            return conn.Query<Product>(sql, new
            {
                search = search ?? "",
                categoryId = categoryId ?? 0,
                minPrice,
                maxPrice
            }).ToList();
        }

        // existing Get (keeps returning Product)
        public Product? Get(int id)
        {
            using var conn = OpenConnection();
            string sql = "SELECT * FROM Products WHERE ProductID = @id";
            return conn.QueryFirstOrDefault<Product>(sql, new { id });
        }

        // NEW: GetDetails returns product info + supplier name + category name
        public ProductDetailsViewModel? GetDetails(int id)
        {
            using var conn = OpenConnection();

            string sql = @"
SELECT 
    p.ProductID,
    p.ProductName,
    p.ProductDescription,
    p.SupplierID,
    p.CategoryID,
    p.Unit,
    p.Price,
    p.Photo,
    p.IsSelling,
    s.SupplierName,
    c.CategoryName
FROM Products p
LEFT JOIN Suppliers s ON p.SupplierID = s.SupplierID
LEFT JOIN Categories c ON p.CategoryID = c.CategoryID
WHERE p.ProductID = @id
";

            return conn.QueryFirstOrDefault<ProductDetailsViewModel>(sql, new { id });
        }
    }
}