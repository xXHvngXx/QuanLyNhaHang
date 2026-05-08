using RestaurantManagementSystem.Models;
using System;
using System.Data;

namespace RestaurantManagementSystem.DAL
{
    public class AccountDAL
    {
        public static DataRow LoginAccount;

        private static AccountDAL instance;
        public static AccountDAL Instance
        {
            get { if (instance == null) instance = new AccountDAL(); return instance; }
        }
        private AccountDAL() { }

        public DataTable GetListAccount()
        {
            return DataProvider.Instance.ExecuteQuery("exec USP_GetListAccount");
        }

        public bool CheckAccountExist(string userName)
        {
            string query = "exec USP_CheckAccountExist @userName";
            DataTable data = DataProvider.Instance.ExecuteQuery(query, new object[] { userName });
            return Convert.ToInt32(data.Rows[0][0]) > 0;
        }

        public bool InsertAccount(string userName, string displayName, string password, int role)
        {
            string query = "exec USP_InsertAccount @u , @d , @p , @r";
            int result = DataProvider.Instance.ExecuteNonQuery(query, new object[] { userName, displayName, password, role });
            return result > 0;
        }

        public bool UpdateAccount(string userName, string displayName, int role)
        {
            string query = "exec USP_UpdateAccount @u , @d , @r";
            int result = DataProvider.Instance.ExecuteNonQuery(query, new object[] { userName, displayName, role });
            return result > 0;
        }

        public bool DeleteAccount(string userName)
        {
            string query = "exec USP_DeleteAccount @u";
            int result = DataProvider.Instance.ExecuteNonQuery(query, new object[] { userName });
            return result > 0;
        }

        public bool ResetPassword(string userName, string password)
        {
            string query = "exec USP_ResetPassword @u , @p";
            int result = DataProvider.Instance.ExecuteNonQuery(query, new object[] { userName, password });
            return result > 0;
        }

        public bool Login(string userName, string password)
        {
            string query = "exec USP_Login @userName";
            DataTable result = DataProvider.Instance.ExecuteQuery(query, new object[] { userName });

            if (result.Rows.Count > 0)
            {
                DataRow row = result.Rows[0];
                string hashedPassInDB = row["Password"].ToString().Trim();

                if (SecurityHelper.VerifyPassword(password, hashedPassInDB))
                {
                    LoginAccount = row;
                    return true;
                }
            }
            return false;
        }
    }
}