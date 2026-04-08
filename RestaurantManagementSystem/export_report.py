import pandas as pd
import pyodbc
import sys
import os

def export_to_excel(from_date, to_date):
    try:
        # 1. Kết nối SQL Server 
        # Hưng kiểm tra lại SERVER=... xem có đúng tên máy cậu không nhé
        # r'...' giúp xử lý các dấu gạch chéo ngược trong tên Server
        conn_str = (
            r'DRIVER={SQL Server};'
            r'SERVER=ADMIN-PC\SQLEXPRESS;' 
            r'DATABASE=QL_NhaHang;'
            r'Trusted_Connection=yes;'
        )
        conn = pyodbc.connect(conn_str)

        # 2. Câu lệnh SQL để lấy dữ liệu (Giống hệt logic tính tiền trong DAL của cậu)
        query = f"""
        SELECT b.BillID as [Mã HĐ], t.TableName as [Bàn], 
               CONVERT(VARCHAR, b.DateCheckOut, 103) as [Ngày ra], 
               SUM(f.Price * bi.Quantity) as [Doanh thu (VNĐ)]
        FROM Bill b 
        JOIN TableFood t ON b.TableID = t.TableID
        JOIN BillInfo bi ON b.BillID = bi.BillID
        JOIN Food f ON bi.FoodID = f.FoodID
        WHERE b.Status = 1 AND b.DateCheckOut BETWEEN '{from_date}' AND '{to_date}'
        GROUP BY b.BillID, t.TableName, b.DateCheckOut
        """

        # 3. Đọc dữ liệu vào Pandas DataFrame
        df = pd.read_sql(query, conn)
        
        if df.empty:
            print("Error: Không có dữ liệu trong khoảng thời gian này.")
            return

        # 4. Đặt tên file và xuất ra Excel
        # File sẽ được lưu ngay tại thư mục chứa file .exe của C#
        file_name = f"BaoCao_DoanhThu_{from_date}_den_{to_date}.xlsx"
        df.to_excel(file_name, index=False)
        
        # In ra đường dẫn tuyệt đối để C# bắt được và mở file
        print(f"Success: {os.path.abspath(file_name)}")

    except Exception as e:
        print(f"Error: {str(e)}")
    finally:
        if 'conn' in locals(): conn.close()

if __name__ == "__main__":
    # Nhận 2 tham số: Ngày bắt đầu và Ngày kết thúc từ C# truyền sang
    if len(sys.argv) > 2:
        export_to_excel(sys.argv[1], sys.argv[2])
    else:
        print("Error: Thiếu tham số ngày tháng.")