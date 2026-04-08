using RestaurantManagementSystem.DAL;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RestaurantManagementSystem.BLL
{
    public class CategoryBLL
    {
        private static CategoryBLL instance;
        public static CategoryBLL Instance
        {
            get { if (instance == null) instance = new CategoryBLL(); return instance; }
        }
        private CategoryBLL() { }

        public DataTable GetCategories()
        {
            return CategoryDAL.Instance.GetListCategory();
        }

        public string AddCategory(string name)
        {
            // BLL bắt lỗi: Không được để trống tên
            if (string.IsNullOrWhiteSpace(name))
                return "Tên danh mục không được để trống!";

            if (CategoryDAL.Instance.InsertCategory(name))
                return "Thêm danh mục thành công!";

            return "Lỗi khi thêm danh mục vào CSDL!";
        }

        public string UpdateCategory(string idText, string name)
        {
            if (string.IsNullOrWhiteSpace(idText) || !int.TryParse(idText, out int id))
                return "Vui lòng chọn một danh mục để sửa!";

            if (string.IsNullOrWhiteSpace(name))
                return "Tên danh mục không được để trống!";

            if (CategoryDAL.Instance.UpdateCategory(id, name))
                return "Cập nhật danh mục thành công!";

            return "Lỗi khi cập nhật CSDL!";
        }

        public string DeleteCategory(string idText)
        {
            if (string.IsNullOrWhiteSpace(idText) || !int.TryParse(idText, out int id))
                return "Vui lòng chọn một danh mục để xóa!";

            // 1. KIỂM TRA: BLL hỏi DAL xem danh mục này có món ăn không?
            if (CategoryDAL.Instance.CheckFoodExist(id))
            {
                return "⛔ CẢNH BÁO: Không thể xóa! Đang có món ăn thuộc danh mục này. Vui lòng xóa món ăn trước.";
            }

            // 2. Nếu an toàn thì mới ra lệnh Xóa
            try
            {
                if (CategoryDAL.Instance.DeleteCategory(id))
                    return "Xóa danh mục thành công!";
            }
            catch
            {
                return "Lỗi CSDL: Không thể xóa lúc này!";
            }

            return "Xóa thất bại!";
        }
    }
}
