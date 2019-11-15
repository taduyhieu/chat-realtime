# SignalR-Chat-master

#I. Cài đặt
##1. Môi trường, công cụ
  1.1 SQL Server: https://go.microsoft.com/fwlink/?linkid=866662
  1.2 SQL Server Management Studio (SSMS: https://aka.ms/ssmsfullsetup
  1.3 Cài đặt Visual Studio:  https://visualstudio.microsoft.com/thank-you-downloading-visual-studio/?sku=Enterprise&rel=16 
##2. Mở sourcecode bằng Visual Studio và build   
##3. Thêm cơ sở dữ liệu
  - Mở SQL Server Management Studio sau khi bước 1.1 và 1.2 thành công: 
    + Tạo mới cơ sở dữ liệu có tên: chatrealtime
  - Mở sourcecode bằng Visual Studio:
    + Tạo kết nối với cơ sở dữ liệu local bằng cách thay đổi file Web.config:
      Data Source=[Tên SQL Server]
      Initial Catalog=[Tên cơ sở dữ liệu] => chatrealtime
  - Build code bằng:      
  - Sau khi thành công chạy lệnh cập nhật cơ sở dữ liệu: "update-database"
##4. Chạy project và sử dụng
#II. Cách sử dụng các tính năng
##1. Tạo tài khoản
  - 
