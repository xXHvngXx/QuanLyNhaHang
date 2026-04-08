using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RestaurantManagementSystem.DTO
{
    public class AccountDTO
    {
        public string UserName { get; set; }
        public string DisplayName { get; set; }
        public string Password { get; set; }
        public int Role { get; set; }

        public AccountDTO(string userName, string displayName, string password, int role)
        {
            this.UserName = userName;
            this.DisplayName = displayName;
            this.Password = password;
            this.Role = role;
        }
    }
}
