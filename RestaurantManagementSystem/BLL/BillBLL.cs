using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RestaurantManagementSystem.DAL;
using System.Data;

namespace RestaurantManagementSystem.BLL
{
    public class BillBLL
    {
        private static BillBLL instance;
        public static BillBLL Instance
        {
            get { if (instance == null) instance = new BillBLL(); return instance; }
        }
        private BillBLL() { }

        public DataTable GetListBill(DateTime from, DateTime to)
        {
            return BillDAL.Instance.GetBillListByDate(from, to);
        }

        public DataTable GetChartData(DateTime from, DateTime to)
        {
            return BillDAL.Instance.GetRevenueData(from, to);
        }
    }
}
