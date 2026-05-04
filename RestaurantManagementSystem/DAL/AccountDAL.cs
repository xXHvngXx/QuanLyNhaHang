using RestaurantManagementSystem.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        // Lấy danh sách, kèm theo cột RoleName dịch ra tiếng Việt
        public DataTable GetListAccount()
        {
            // 0: Admin, 2: Cashier, 1: Staff
            string query = "SELECT UserName, DisplayName, Role, " +
                           "CASE " +
                           "WHEN Role = 0 THEN N'Quản trị viên (Admin)' " +
                           "WHEN Role = 2 THEN N'Thu ngân (Cashier)' " +
                           "ELSE N'Phục vụ (Staff)' " +
                           "END AS RoleName " +
                           "FROM Account";

            return DataProvider.Instance.ExecuteQuery(query);
        }

        // Kiểm tra tài khoản đã tồn tại chưa (Vì UserName là Khóa chính)
        public bool CheckAccountExist(string userName)
        {
            string query = "SELECT COUNT(*) FROM Account WHERE UserName = @userName";
            System.Data.DataTable data = DataProvider.Instance.ExecuteQuery(query, new object[] { userName });
            return System.Convert.ToInt32(data.Rows[0][0]) > 0;
        }

        // Thêm tham số password
        public bool InsertAccount(string userName, string displayName, string password, int role)
        {
            // Thay N'1' thành @p
            string query = "INSERT INTO Account (UserName, DisplayName, Password, Role) VALUES ( @u , @d , @p , @r )";
            int result = DataProvider.Instance.ExecuteNonQuery(query, new object[] { userName, displayName, password, role });
            return result > 0;
        }

        // Sửa tên hiển thị và quyền (Không cho sửa UserName vì nó là Khóa chính)
        public bool UpdateAccount(string userName, string displayName, int role)
        {
            string query = "UPDATE Account SET DisplayName = @d , Role = @r WHERE UserName = @u";
            int result = DataProvider.Instance.ExecuteNonQuery(query, new object[] { displayName, role, userName });
            return result > 0;
        }

        // Xóa
        public bool DeleteAccount(string userName)
        {
            string query = "DELETE FROM Account WHERE UserName = @u";
            int result = DataProvider.Instance.ExecuteNonQuery(query, new object[] { userName });
            return result > 0;
        }

        // Thêm tham số password
        public bool ResetPassword(string userName, string password)
        {
            // Thay N'1' thành @p
            string query = "UPDATE Account SET Password = @p WHERE UserName = @u";
            int result = DataProvider.Instance.ExecuteNonQuery(query, new object[] { password, userName });
            return result > 0;
        }
        public bool Login(string userName, string password)
        {
            string query = "SELECT * FROM Account WHERE UserName = @u AND Password = @p";
            DataTable result = DataProvider.Instance.ExecuteQuery(query, new object[] { userName, password });

            if (result.Rows.Count > 0)
            {
                LoginAccount = result.Rows[0];
                return true;
            }
            return false;
        }
    }
}
