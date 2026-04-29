🍽️ Restaurant Management System - Luxury Edition
Dự án Hệ thống Quản lý Nhà hàng được xây dựng nhằm tối ưu hóa quy trình vận hành, từ khâu phục vụ, thu ngân đến quản lý và báo cáo doanh thu dành cho mô hình nhà hàng cao cấp.

🚀 Tính năng cốt lõi
Quản lý nghiệp vụ: Phân quyền chi tiết cho Phục vụ, Thu ngân và Quản trị viên.

Thực đơn phong phú: Hệ thống sẵn có 150 món ăn thượng hạng được phân loại theo 16 danh mục.

Quản lý bàn: Sơ đồ 24 bàn ăn hoạt động thời gian thực.

Báo cáo chuyên sâu: Tích hợp biểu đồ xu hướng doanh thu (LiveCharts) và module xuất báo cáo Excel bằng Python.

Bảo mật: Hệ thống băm mật khẩu chuẩn PBKDF2 (100,000 iterations).

💻 Công nghệ sử dụng

Frontend/Logic: C# (WPF trên nền .NET 9.0).

Database: Microsoft SQL Server.

Reporting Module: Python 3.x (Pandas, SQLAlchemy).

UI/UX: Material Design phong cách hiện đại.


🛠️ Hướng dẫn cài đặt dành cho Thành viên (Developers)
Để hệ thống hoạt động ổn định trên máy cục bộ, các thành viên cần thực hiện đúng các bước sau:

1. Cơ sở dữ liệu (Database)
Khởi tạo: Chạy file Database_Master.sql trong thư mục /Data để tạo cấu trúc bảng và dữ liệu mẫu.

Kết nối: Dự án sử dụng chuỗi kết nối Portable Data Source=.. Nếu máy sử dụng Instance Name (ví dụ: .\SQLEXPRESS), cập nhật lại trong Models/DataProvider.cs.

2. Thư viện C# (NuGet Packages)
Mở Package Manager Console trong Visual Studio và thực hiện cài đặt:

PowerShell

Install-Package Microsoft.Data.SqlClient
Install-Package Microsoft.AspNetCore.Cryptography.KeyDerivation
Install-Package LiveCharts.Wpf
3. Môi trường Python (Reporting Service)
Yêu cầu cài đặt Python 3.7+ và các thư viện hỗ trợ xử lý dữ liệu:

Bash

pip install pandas sqlalchemy pyodbc openpyxl
📜 Quy định phát triển (Developer Guidelines)
Nhằm đảm bảo tính đồng nhất và tránh xung đột mã nguồn, Leader yêu cầu các thành viên tuân thủ:

Cú pháp Truy vấn SQL: Luôn sử dụng khoảng trắng bao quanh các tham số @ để DataProvider có thể bóc tách dữ liệu chính xác.

Ví dụ: WHERE UserName = @u (Đúng) | WHERE UserName=@u (Sai).

Quy trình Git: Luôn thực hiện Pull trước khi bắt đầu phiên làm việc mới và Commit kèm theo mô tả rõ ràng các thay đổi.


Cấu hình File: Không tự ý thay đổi file .csproj trừ khi có yêu cầu thêm thư viện mới từ Leader.

👤 Thông tin tác giả
Leader: Hưng (Directeur Hưng) - Khoa Kỹ thuật Hệ thống Thông tin, HUIT.

Project Status: In Development (Luxury Edition).
