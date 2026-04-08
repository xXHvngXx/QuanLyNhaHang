using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RestaurantManagementSystem.DTO
{
    public class CategoryDTO
    {
        public int CategoryID { get; set; }
        public string CategoryName { get; set; }

        // Hàm khởi tạo (Constructor) để gán giá trị nhanh
        public CategoryDTO(int id, string name)
        {
            this.CategoryID = id;
            this.CategoryName = name;
        }
    }
}
