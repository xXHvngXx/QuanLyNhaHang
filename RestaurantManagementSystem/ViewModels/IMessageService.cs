using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RestaurantManagementSystem.ViewModels
{
    public interface IMessageService
    {
        void ShowInfo(string title, string message);
        void ShowError(string title, string message);
        bool ShowConfirm(string title, string message); // Trả về true nếu chọn Yes/OK
    }
}
