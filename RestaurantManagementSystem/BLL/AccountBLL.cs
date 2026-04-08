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

            // BĂM MẬT KHẨU MẶC ĐỊNH LÀ 1
            string defaultHashedPassword = SecurityHelper.HashPassword("1");

            // Truyền mật khẩu đã băm xuống DAL
            if (AccountDAL.Instance.InsertAccount(userName, displayName, defaultHashedPassword, role))
                return "Thêm tài khoản thành công! Mật khẩu mặc định là: 1";

            return "Lỗi thêm tài khoản vào cơ sở dữ liệu!";
        }

        public string UpdateAccount(string userName, string displayName, int role)
        {
            if (string.IsNullOrWhiteSpace(userName))
                return "Vui lòng chọn tài khoản cần sửa!";
            if (string.IsNullOrWhiteSpace(displayName))
                return "Tên hiển thị không được để trống!";

            if (AccountDAL.Instance.UpdateAccount(userName, displayName, role))
                return "Cập nhật tài khoản thành công!";

            return "Lỗi cập nhật cơ sở dữ liệu!";
        }

        public string DeleteAccount(string userName)
        {
            if (string.IsNullOrWhiteSpace(userName))
                return "Vui lòng chọn tài khoản cần xóa!";

            if (userName.ToLower() == "admin")
                return "⛔ CẢNH BÁO: Không được phép xóa tài khoản Admin tối cao!";

            if (AccountDAL.Instance.DeleteAccount(userName))
                return "Xóa tài khoản thành công!";

            return "Lỗi xóa cơ sở dữ liệu!";
        }

        public string ResetPassword(string userName)
        {
            if (string.IsNullOrWhiteSpace(userName))
                return "Vui lòng chọn tài khoản để đặt lại mật khẩu!";

            // 🔒 BĂM LẠI MẬT KHẨU "1"
            string defaultHashedPassword = SecurityHelper.HashPassword("1");

            // Truyền mật khẩu đã băm xuống DAL
            if (AccountDAL.Instance.ResetPassword(userName, defaultHashedPassword))
                return "Thành công! Mật khẩu đã được đưa về mặc định: 1";

            return "Lỗi đặt lại mật khẩu!";
        }
    }
}
