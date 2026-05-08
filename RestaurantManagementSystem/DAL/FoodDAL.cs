using RestaurantManagementSystem.Models;
using System;
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
            return DataProvider.Instance.ExecuteQuery("exec USP_GetListFood");
        }

        public bool InsertFood(string name, int categoryId, decimal price)
        {
            string query = "exec USP_InsertFood @name , @id , @price";
            int result = DataProvider.Instance.ExecuteNonQuery(query, new object[] { name, categoryId, price });
            return result > 0;
        }

        public bool UpdateFood(int id, string name, int categoryId, decimal price)
        {
            // Thứ tự truyền vào: name, categoryId, price, id
            string query = "exec USP_UpdateFood @id , @name , @catId , @price";
            int result = DataProvider.Instance.ExecuteNonQuery(query, new object[] { id, name, categoryId, price });
            return result > 0;
        }

        public bool DeleteFood(int id)
        {
            string query = "exec USP_DeleteFood @id";
            int result = DataProvider.Instance.ExecuteNonQuery(query, new object[] { id });
            return result > 0;
        }

        public bool CheckDuplicateFood(string name, int categoryId, int excludeFoodId = -1)
        {
            string query = "exec USP_CheckDuplicateFood @name , @catId , @excludeId";
            DataTable data = DataProvider.Instance.ExecuteQuery(query, new object[] { name, categoryId, excludeFoodId });

            return Convert.ToInt32(data.Rows[0][0]) > 0;
        }
    }
}