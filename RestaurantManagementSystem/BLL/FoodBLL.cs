using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RestaurantManagementSystem.DAL;
using System.Data;

namespace RestaurantManagementSystem.BLL
{
    public class FoodBLL
    {
        private static FoodBLL instance;
        public static FoodBLL Instance
        {
            get { if (instance == null) instance = new FoodBLL(); return instance; }
        }
        private FoodBLL() { }

        public DataTable GetFoods()
        {
            return FoodDAL.Instance.GetListFood();
        }

        public string AddFood(string name, object selectedCategory, string priceText)
        {
            if (string.IsNullOrWhiteSpace(name)) return "Tên món ăn không được để trống!";
            if (selectedCategory == null) return "Vui lòng chọn Danh mục món ăn!";
            if (!decimal.TryParse(priceText, out decimal price) || price < 0) return "Giá tiền không hợp lệ (Phải là số >= 0)!";

            int categoryId = System.Convert.ToInt32(selectedCategory);

            //Kiểm tra trùng tên
            if (FoodDAL.Instance.CheckDuplicateFood(name, categoryId))
                return "⛔ Món ăn này đã tồn tại trong danh mục! Vui lòng nhập tên khác.";

            try
            {
                if (FoodDAL.Instance.InsertFood(name, categoryId, price))
                    return "Thêm món ăn thành công!";
            }
            catch { return "Lỗi CSDL: Không thể thêm lúc này!"; } // Bọc try-catch để lỡ SQL báo lỗi khác thì app không văng

            return "Lỗi khi thêm vào cơ sở dữ liệu!";
        }

        public string UpdateFood(string idText, string name, object selectedCategory, string priceText)
        {
            if (!int.TryParse(idText, out int id)) return "Vui lòng chọn một món ăn để sửa!";
            if (string.IsNullOrWhiteSpace(name)) return "Tên món ăn không được để trống!";
            if (selectedCategory == null) return "Vui lòng chọn Danh mục món ăn!";
            if (!decimal.TryParse(priceText, out decimal price) || price < 0) return "Giá tiền không hợp lệ!";

            int categoryId = System.Convert.ToInt32(selectedCategory);

            // Kiểm tra trùng tên (nhưng bỏ qua ID của chính nó đang sửa)
            if (FoodDAL.Instance.CheckDuplicateFood(name, categoryId, id))
                return "⛔ Tên món ăn mới bị trùng với một món khác trong danh mục này!";

            try
            {
                if (FoodDAL.Instance.UpdateFood(id, name, categoryId, price))
                    return "Cập nhật thành công!";
            }
            catch { return "Lỗi CSDL: Không thể cập nhật lúc này!"; }

            return "Lỗi khi cập nhật cơ sở dữ liệu!";
        }

        public string DeleteFood(string idText)
        {
            if (!int.TryParse(idText, out int id)) return "Vui lòng chọn một món ăn để xóa!";

            if (FoodDAL.Instance.DeleteFood(id))
                return "Xóa món ăn thành công!";
            return "Lỗi khi xóa cơ sở dữ liệu!";
        }
    }
}
