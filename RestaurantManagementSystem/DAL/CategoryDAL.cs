using RestaurantManagementSystem.Models;
using System;
using System.Data;

namespace RestaurantManagementSystem.DAL
{
    public class CategoryDAL
    {
        private static CategoryDAL instance;
        public static CategoryDAL Instance
        {
            get { if (instance == null) instance = new CategoryDAL(); return instance; }
        }
        private CategoryDAL() { }

        public DataTable GetListCategory()
        {
            return DataProvider.Instance.ExecuteQuery("exec USP_GetListCategory");
        }

        public bool InsertCategory(string name)
        {
            string query = "exec USP_InsertCategory @name";
            int result = DataProvider.Instance.ExecuteNonQuery(query, new object[] { name });
            return result > 0;
        }

        public bool UpdateCategory(int id, string name)
        {
            string query = "exec USP_UpdateCategory @id , @name";
            int result = DataProvider.Instance.ExecuteNonQuery(query, new object[] { id, name });
            return result > 0;
        }

        public bool DeleteCategory(int id)
        {
            string query = "exec USP_DeleteCategory @id";
            int result = DataProvider.Instance.ExecuteNonQuery(query, new object[] { id });
            return result > 0;
        }

        public bool CheckFoodExist(int categoryId)
        {
            string query = "exec USP_CheckFoodExistByCategory @id";
            DataTable data = DataProvider.Instance.ExecuteQuery(query, new object[] { categoryId });

            if (data.Rows.Count > 0)
            {
                return Convert.ToInt32(data.Rows[0][0]) > 0;
            }
            return false;
        }
    }
}