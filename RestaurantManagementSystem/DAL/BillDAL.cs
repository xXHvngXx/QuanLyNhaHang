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

        // 1. Lấy danh sách hóa đơn (Tính TotalPrice từ BillInfo và Food)
        public DataTable GetBillListByDate(DateTime from, DateTime to)
        {

            string query = "SELECT b.BillID , t.TableName , b.DateCheckIn , b.DateCheckOut , 0 AS Discount , SUM( f.Price * bi.Quantity ) AS TotalPrice " +
                           "FROM Bill AS b " +
                           "JOIN TableFood AS t ON b.TableID = t.TableID " +
                           "JOIN BillInfo AS bi ON b.BillID = bi.BillID " +
                           "JOIN Food AS f ON bi.FoodID = f.FoodID " +
                           "WHERE b.Status = 1 AND b.DateCheckOut BETWEEN @from AND @to " +
                           "GROUP BY b.BillID , t.TableName , b.DateCheckIn , b.DateCheckOut";

            return DataProvider.Instance.ExecuteQuery(query, new object[] { from, to });
        }

        // 2. Lấy dữ liệu biểu đồ (Cộng dồn tiền theo ngày)
        public DataTable GetRevenueData(DateTime from, DateTime to)
        {
            string query = "SELECT CAST( b.DateCheckOut AS DATE ) AS Date , SUM( f.Price * bi.Quantity ) AS Total " +
                           "FROM Bill AS b " +
                           "JOIN BillInfo AS bi ON b.BillID = bi.BillID " +
                           "JOIN Food AS f ON bi.FoodID = f.FoodID " +
                           "WHERE b.Status = 1 AND b.DateCheckOut BETWEEN @from AND @to " +
                           "GROUP BY CAST( b.DateCheckOut AS DATE ) " +
                           "ORDER BY Date ASC";

            return DataProvider.Instance.ExecuteQuery(query, new object[] { from, to });
        }
    }
}