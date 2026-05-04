using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RestaurantManagementSystem.Models;
using RestaurantManagementSystem.DAL;
using System.Data;

namespace RestaurantManagementSystem.BLL
{
    public class AccountBLL
    {
        private static AccountBLL instance;
        public static AccountBLL Instance
        {
            get { if (instance == null) instance = new AccountBLL(); return instance; }
        }
        private AccountBLL() { }

        public DataTable GetAccounts()
        {
            return AccountDAL.Instance.GetListAccount();
        }

        public string AddAccount(string userName, string displayName, int role)
        {
            if (string.IsNullOrWhiteSpace(userName) || string.IsNullOrWhiteSpace(displayName))
                return "Tên đăng nhập và Tên hiển thị không được để trống!";

            if (AccountDAL.Instance.CheckAccountExist(userName))
                return "Tên đăng nhập này đã tồn tại! Vui lòng chọn tên khác.";

            string defaultHashedPassword = SecurityHelper.HashPassword(userName);

            if (AccountDAL.Instance.InsertAccount(userName, displayName, defaultHashedPassword, role))
                return $"Thêm tài khoản thành công! Mật khẩu mặc định trùng với tên tài khoản: {userName}";

            return "Lỗi thêm tài khoản vào cơ sở dữ liệu!";
        }

        public string UpdateAccount(string userName, string displayName, int role)
        {
            if (string.IsNullOrWhiteSpace(userName))
                return "Vui lòng chọn tài khoản cần sửa!";
            if (string.IsNullOrWhiteSpace(displayName))
                return "Tên hiển thị không được để trống!";

            // Lưu ý: Update chỉ sửa thông tin, KHÔNG sửa mật khẩu
            if (AccountDAL.Instance.UpdateAccount(userName, displayName, role))
                return "Cập nhật tài khoản thành công!";

            return "Lỗi cập nhật cơ sở dữ liệu!";
        }

        public string DeleteAccount(string userName)
        {
            if (string.IsNullOrWhiteSpace(userName))
                return "Vui lòng chọn tài khoản cần xóa!";

            string currentUser = AccountDAL.LoginAccount["UserName"].ToString().Trim();
            if (userName.Trim().ToLower() == currentUser.ToLower())
                return "❌ Bạn không thể tự xóa tài khoản của chính mình!";

            DataTable dt = AccountDAL.Instance.GetListAccount();
            DataRow row = dt.AsEnumerable().FirstOrDefault(r => r.Field<string>("UserName").Trim() == userName.Trim());

            if (row != null)
            {
                int role = Convert.ToInt32(row["Role"]);
                if (role == 0)
                    return "⛔ CẢNH BÁO: Không được phép xóa tài khoản thuộc nhóm Quản trị viên!";
            }

            if (AccountDAL.Instance.DeleteAccount(userName))
                return "Xóa tài khoản thành công!";

            return "Lỗi xóa cơ sở dữ liệu!";
        }

        public string ResetPassword(string userName)
        {
            if (string.IsNullOrWhiteSpace(userName))
                return "Vui lòng chọn tài khoản để đặt lại mật khẩu!";

            // Dùng chính UserName làm mật khẩu mặc định khi Reset
            string defaultHashedPassword = SecurityHelper.HashPassword(userName);

            if (AccountDAL.Instance.ResetPassword(userName, defaultHashedPassword))
                return $"Thành công! Mật khẩu của '{userName}' đã được đưa về mặc định (trùng với tên tài khoản).";

            return "Lỗi đặt lại mật khẩu!";
        }
        public string ChangePassword(string oldPass, string newPass, string reEnterPass)
        {
            if (AccountDAL.LoginAccount == null)
                return "Lỗi: Không tìm thấy phiên đăng nhập!";

            string userName = AccountDAL.LoginAccount["UserName"].ToString().Trim();

            if (newPass != reEnterPass)
                return "Xác nhận mật khẩu mới không khớp!";

            // Lấy mật khẩu đã băm (Hash) đang lưu trong Database lên
            string query = "SELECT Password FROM Account WHERE UserName = @u";
            DataTable result = DataProvider.Instance.ExecuteQuery(query, new object[] { userName });

            if (result == null || result.Rows.Count <= 0)
                return "Lỗi: Tài khoản không tồn tại!";

            string hashedPassInDB = result.Rows[0]["Password"].ToString().Trim();

            // Sử dụng hàm Verify để kiểm tra mật khẩu cũ
            if (!SecurityHelper.VerifyPassword(oldPass, hashedPassInDB))
            {
                return "Mật khẩu hiện tại không chính xác!";
            }

            // Cập nhật mật khẩu mới
            string hashedNewPass = SecurityHelper.HashPassword(newPass).Trim();
            if (AccountDAL.Instance.ResetPassword(userName, hashedNewPass))
            {
                // Cập nhật lại mật khẩu trong bộ nhớ tạm để đồng bộ
                AccountDAL.LoginAccount["Password"] = hashedNewPass;
                return "Đổi mật khẩu thành công!";
            }

            return "Lỗi khi cập nhật mật khẩu!";
        }
    }
}