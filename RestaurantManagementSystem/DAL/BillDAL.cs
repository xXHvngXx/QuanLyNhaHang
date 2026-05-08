using RestaurantManagementSystem.Models;
using System;
using System.Data;

namespace RestaurantManagementSystem.DAL
{
    public class BillDAL
    {
        private static BillDAL instance;
        public static BillDAL Instance
        {
            get { if (instance == null) instance = new BillDAL(); return instance; }
        }
        private BillDAL() { }

        public DataTable GetBillListByDate(DateTime from, DateTime to)
        {
            string query = "exec USP_GetBillListByDate @from , @to";
            return DataProvider.Instance.ExecuteQuery(query, new object[] { from, to });
        }

        public DataTable GetRevenueData(DateTime from, DateTime to)
        {
            string query = "exec USP_GetRevenueData @from , @to";
            return DataProvider.Instance.ExecuteQuery(query, new object[] { from, to });
        }
    }
}