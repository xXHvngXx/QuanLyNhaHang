using RestaurantManagementSystem.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RestaurantManagementSystem.DAL
{
    public class CategoryDAL
    {
        // Dùng Singleton Pattern để gọi 
        private static CategoryDAL instance;
        public static CategoryDAL Instance
        {
            get { if (instance == null) instance = new CategoryDAL(); return instance; }
        }
        private CategoryDAL() { }

        // Lấy danh sách
        public DataTable GetListCategory()
        {
            return DataProvider.Instance.ExecuteQuery("SELECT * FROM Category");
        }

        // Thêm
        public bool InsertCategory(string name)
        {
            string query = "INSERT INTO Category (CategoryName) VALUES ( @name )";
            int result = DataProvider.Instance.ExecuteNonQuery(query, new object[] { name });
            return result > 0;
        }

        // Sửa
        public bool UpdateCategory(int id, string name)
        {
            string query = "UPDATE Category SET CategoryName = @name WHERE CategoryID = @id";
            int result = DataProvider.Instance.ExecuteNonQuery(query, new object[] { name, id });
            return result > 0;
        }

        // Xóa
        public bool DeleteCategory(int id)
        {
            string query = "DELETE FROM Category WHERE CategoryID = @id";
            int result = DataProvider.Instance.ExecuteNonQuery(query, new object[] { id });
            return result > 0;
        }

        // Hàm kiểm tra xem Danh mục có đang chứa món ăn nào không
        public bool CheckFoodExist(int categoryId)
        {
            string query = "SELECT COUNT(*) FROM Food WHERE CategoryID = @id";

            // Lấy kết quả đếm từ SQL
            System.Data.DataTable data = DataProvider.Instance.ExecuteQuery(query, new object[] { categoryId });

            if (data.Rows.Count > 0)
            {
                int count = System.Convert.ToInt32(data.Rows[0][0]);
                return count > 0; // Trả về true nếu có món ăn
            }
            return false;
        }
    }
}
