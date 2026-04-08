using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RestaurantManagementSystem.Models;
using System.Data;

namespace RestaurantManagementSystem.DAL
{
    public class FoodDAL
    {
        private static FoodDAL instance;
        public static FoodDAL Instance
        {
            get { if (instance == null) instance = new FoodDAL(); return instance; }
        }
        private FoodDAL() { }

        public DataTable GetListFood()
        {
            return DataProvider.Instance.ExecuteQuery("SELECT * FROM Food");
        }

        public bool InsertFood(string name, int categoryId, decimal price)
        {
            string query = "INSERT INTO Food (FoodName, CategoryID, Price) VALUES ( @name , @id , @price )";
            int result = DataProvider.Instance.ExecuteNonQuery(query, new object[] { name, categoryId, price });
            return result > 0;
        }

        public bool UpdateFood(int id, string name, int categoryId, decimal price)
        {
            string query = "UPDATE Food SET FoodName = @name , CategoryID = @catId , Price = @price WHERE FoodID = @id";
            int result = DataProvider.Instance.ExecuteNonQuery(query, new object[] { name, categoryId, price, id });
            return result > 0;
        }

        public bool DeleteFood(int id)
        {
            string query = "DELETE FROM Food WHERE FoodID = @id";
            int result = DataProvider.Instance.ExecuteNonQuery(query, new object[] { id });
            return result > 0;
        }
        // Hàm kiểm tra xem món ăn đã tồn tại trong danh mục chưa
        public bool CheckDuplicateFood(string name, int categoryId, int excludeFoodId = -1)
        {
            // excludeFoodId dùng cho trường hợp Update (bỏ qua chính nó)
            string query = "SELECT COUNT(*) FROM Food WHERE FoodName = @name AND CategoryID = @catId AND FoodID != @id";
            System.Data.DataTable data = DataProvider.Instance.ExecuteQuery(query, new object[] { name, categoryId, excludeFoodId });

            return System.Convert.ToInt32(data.Rows[0][0]) > 0;
        }
    }
}
