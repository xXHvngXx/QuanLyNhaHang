using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RestaurantManagementSystem.DTO
{
    public class FoodDTO
    {
        public int FoodID { get; set; }
        public string FoodName { get; set; }
        public int CategoryID { get; set; }
        public decimal Price { get; set; }

        public FoodDTO(int id, string name, int categoryId, decimal price)
        {
            this.FoodID = id;
            this.FoodName = name;
            this.CategoryID = categoryId;
            this.Price = price;
        }
    }
}
