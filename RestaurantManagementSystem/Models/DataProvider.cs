using System.Data;
using Microsoft.Data.SqlClient; 

namespace RestaurantManagementSystem.Models
{
    public class DataProvider
    {
        private string connectionSTR = @"Data Source=.;Initial Catalog=QL_NhaHang;Integrated Security=True;TrustServerCertificate=True";

        private static DataProvider? instance;
        public static DataProvider Instance
        {
            get { if (instance == null) instance = new DataProvider(); return DataProvider.instance; }
            private set { DataProvider.instance = value; }
        }

        private DataProvider() { }

        // 1. Hàm SELECT (Trả về bảng dữ liệu)
        public DataTable ExecuteQuery(string query, object[]? parameter = null)
        {
            DataTable data = new DataTable();
            using (SqlConnection connection = new SqlConnection(connectionSTR))
            {
                connection.Open();
                SqlCommand command = new SqlCommand(query, connection);

                if (parameter != null)
                {
                    string[] listPara = query.Split(' ');
                    int i = 0;
                    foreach (string item in listPara)
                    {
                        if (item.Contains('@'))
                        {
                            // Bỏ các ký tự thừa như dấu phẩy, dấu ngoặc
                            string cleanParam = item.Replace(",", "").Replace("(", "").Replace(")", "").Replace(";", "");
                            command.Parameters.AddWithValue(cleanParam, parameter[i]);
                            i++;
                        }
                    }
                }

                SqlDataAdapter adapter = new SqlDataAdapter(command);
                adapter.Fill(data);
                connection.Close();
            }
            return data;
        } 

        // 2. Hàm INSERT, UPDATE, DELETE (Trả về số dòng thành công)
        public int ExecuteNonQuery(string query, object[]? parameter = null)
        {
            int data = 0;
            using (SqlConnection connection = new SqlConnection(connectionSTR))
            {
                connection.Open();
                SqlCommand command = new SqlCommand(query, connection);

                if (parameter != null)
                {
                    string[] listPara = query.Split(' ');
                    int i = 0;
                    foreach (string item in listPara)
                    {
                        if (item.Contains('@'))
                        {
                            string cleanParam = item.Replace(",", "").Replace("(", "").Replace(")", "").Replace(";", "");
                            command.Parameters.AddWithValue(cleanParam, parameter[i]);
                            i++;
                        }
                    }
                }

                data = command.ExecuteNonQuery();
                connection.Close();
            }
            return data;
        }
    }
}