I. Cài đặt
1. Môi trường, công cụ
  1.1 SQL Server: [Link](https://go.microsoft.com/fwlink/?linkid=866662)
  1.2 SQL Server Management Studio (SSMS): [Link](https://aka.ms/ssmsfullsetup)
  1.3 Cài đặt Visual Studio:  [Link](https://visualstudio.microsoft.com/thank-you-downloading-visual-studio/?sku=Enterprise&rel=16) 
2. Mở sourcecode bằng Visual Studio và build   
3. Thêm cơ sở dữ liệu
  - Mở SQL Server Management Studio sau khi bước 1.1 và 1.2 thành công: 
    + Tạo mới cơ sở dữ liệu có tên: chatrealtime
  - Mở sourcecode bằng Visual Studio:
    + Tạo kết nối với cơ sở dữ liệu local bằng cách thay đổi file Web.config:
      Data Source="Tên SQL Server"
      Initial Catalog="Tên cơ sở dữ liệu" => chatrealtime
  - Build code bằng:      
  - Sau khi thành công chạy lệnh cập nhật cơ sở dữ liệu: "update-database"
4. Chạy project và sử dụng
II. Cách sử dụng các tính năng
1. Tạo tài khoản
  - 
  Chú ý: Tạo 2 tài khoản và login trên 2 trình duyệt khác nhau hoặc ẩn danh để thấy trạng thái realtime
2. Tạo, sửa nhóm được cập nhật realtime 
  2.1 Tạo nhóm
    - Nhấn vào dấu "+" ở phía bên trên, trái để thấy giao diện tạo nhóm
    - Đặt tên nhóm và thêm các thành viên.
  2.2 Sửa nhóm (chỉ người - quản trị viên tạo được sửa nhóm)
    - Click vào biểu tượng edit ở phía trên khi mở nhóm
    - Thực hiện thay đổi và nhấn "Cập nhật"
3. Cập nhật trạng thái trực tuyến tài khoản
  - Khi các tài khoản được đăng nhập thì được cập nhật trạng thái được ở "LIST FRIENDS" trên giao diện
4. Nhắn tin realtime theo nhóm, cá nhân
  4.1 Theo nhóm
    - Click vào 1 nhóm trong "LIST ROOMS" sẽ mở ra màn hình chat như hình:
    - Nhập tin nhăn
