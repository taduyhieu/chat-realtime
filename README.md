## I. Môi trường phát triển
   * Công cụ: Visual Studio, Microsoft SQL Server Management Studio 18
   * Framework: ASP.NET MVC 5
   * Thư viện: SignaIR 2.2.0, thư viện hỗ trợ bindling knockoutjs 3.4.2

##I. Cài đặt
#### 1. Môi trường, công cụ

  1.1 SQL Server 2018: [Link](https://go.microsoft.com/fwlink/?linkid=866662)
  
  1.2 SQL Server Management Studio (SSMS): [Link](https://aka.ms/ssmsfullsetup)
  
  1.3 Cài đặt Visual Studio:  [Link](https://visualstudio.microsoft.com/thank-you-downloading-visual-studio/?sku=Enterprise&rel=16) 
### 2. Tạo mở cơ sở dữ liệu

  * Mở SQL Server Management Studio (SSMS) tạo mới CSDL có tên: `chatrealtime`   
  
### 3. Mở sourcecode bằng Visual Studio và build   
  * Mở sourcecode bằng cách open Project từ Visual Studio, sau đó chọn file `Chat.sln` trong sourcecode
  ** Lưu ý: Clone sourccode ở nhánh `master` để ổn định nhất!
### 4. Thêm cơ sở dữ liệu

  * Mở SQL Server Management Studio: 
    * Tạo mới cơ sở dữ liệu có tên: chatrealtime
    
  * Mở sourcecode bằng Visual Studio:
    * Tạo kết nối với cơ sở dữ liệu local bằng cách thay đổi file `Web.config`:
      * Data Source = `Tên SQL Server`
      * Initial Catalog =`Tên cơ sở dữ liệu` => `chatrealtime`      
  
  * Sau khi thành công chạy lệnh cập nhật cơ sở dữ liệu `update-database`
### 5. Chạy project 
  * Click chuột phải vào Solution cảu project trong Visual Studio, chọn Build Solution 
     
### 5. Chạy project và sử dụng
  * Trong Visual Studio, nhấn `Ctrl + F5` để chạy

## II. Cách sử dụng các tính năng

### 1. Tạo tài khoản
  
  * Chú ý: Tạo tài khoản với mật khẩu có chữ hoa, chữ thường, số, ký tự đặc biệt và trên 8 ký tự.
   
  * Chú ý: Tạo 2 tài khoản và login trên 2 trình duyệt khác nhau hoặc ẩn danh để thấy trạng thái realtime

### 2. Tạo, sửa nhóm được cập nhật realtime 

  * Tạo nhóm
  
    * Nhấn vào dấu `+` ở phía bên trên, trái để thấy giao diện tạo nhóm
    * Đặt tên nhóm và thêm các thành viên.
    
  * Sửa nhóm - chỉ quản trị viên tạo được sửa nhóm
  
    * Click vào biểu tượng `edit` ở phía trên khi mở nhóm
    * Thực hiện thay đổi và nhấn `Cập nhật`
    
### 3. Cập nhật trạng thái trực tuyến tài khoản

  * Khi các tài khoản được đăng nhập thì được cập nhật trạng thái được ở `LIST FRIENDS` trên giao diện

### 4. Nhắn tin realtime theo nhóm, cá nhân
  
  * Theo nhóm
    * Click vào 1 nhóm trong `LIST ROOMS` sẽ mở ra màn hình chat
        * Nhập tin nhắn và nhấn Enter
  
  * Cá nhân
    * Click vào 1 nhóm trong `LIST FRIENDS` sẽ mở ra màn hình chat 
        * Nhấp tin nhắn và nhấn Enter
        
### 5. Ghim tin nhắn

  * Mở cửa sổ chat theo nhóm hoặc cá nhân
  * Click vào biểu tượng `ghim` cạnh mỗi tin nhắn
  * Click vào tin nhắn đã ghim thì sẽ thấy cửa sổ tin nhắn tự cuộn về vị trí tin nhắn đã được ghim
   
###### Nếu có thắc mắc về sourcecode và cách sử dụng có thể mail về: `taduyhieucntt98@gmail.com`