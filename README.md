# FPTMart - Hệ Thống Quản Lý Bán Hàng

## 📋 Mô Tả
Ứng dụng WPF quản lý bán hàng cho cửa hàng tiện lợi với các tính năng:
- **POS (Bán hàng)**: Quét barcode, tìm kiếm sản phẩm, thanh toán, in hóa đơn
- **Quản lý Sản phẩm**: CRUD sản phẩm, danh mục
- **Nhập kho**: Nhập hàng từ nhà cung cấp, tự động cập nhật tồn kho
- **Khách hàng**: Quản lý thông tin khách hàng, tích điểm
- **Báo cáo**: Dashboard thống kê doanh thu, sản phẩm bán chạy
- **Phân quyền**: Admin và Nhân viên

## 🛠 Công Nghệ
- **Framework**: .NET 9 (WPF)
- **Database**: SQL Server (Entity Framework Core)
- **UI**: MaterialDesignInXaml
- **Architecture**: 3-Layer (DAL, BLL, Presentation)

## 📦 Cài Đặt

### Yêu Cầu
- .NET 9 SDK
- SQL Server (LocalDB hoặc SQL Server Express)
- Visual Studio 2022 (khuyến nghị)

### Các Bước Cài Đặt

1. **Clone repository**
   ```bash
   git clone <repository-url>
   cd FPTMart
   ```

2. **Cấu hình database**
   - Copy file `FPTMart/appsettings.json.example` thành `FPTMart/appsettings.json`
   - Sửa connection string trong `appsettings.json`:
   ```json
   {
     "ConnectionStrings": {
       "DefaultConnection": "Server=YOUR_SERVER;Database=FPTMartDB;User Id=YOUR_USER;Password=YOUR_PASS;TrustServerCertificate=True;"
     }
   }
   ```

3. **Chạy ứng dụng**
   ```bash
   dotnet build
   dotnet run --project FPTMart
   ```
   
   ⚡ **Database sẽ tự động được tạo** khi chạy lần đầu tiên!

### Tài Khoản Mặc Định
| Role | Username | Password |
|------|----------|----------|
| Admin | admin | admin123 |

## 📁 Cấu Trúc Project

```
FPTMart/
├── FPTMart/              # Presentation Layer (WPF)
│   ├── Views/            # XAML Views
│   ├── ViewModels/       # MVVM ViewModels
│   └── Converters/       # Value Converters
├── FPTMart.BLL/          # Business Logic Layer
│   ├── Services/         # Business Services
│   └── DTOs/             # Data Transfer Objects
├── FPTMart.DAL/          # Data Access Layer
│   ├── Entities/         # Entity Models
│   ├── Repositories/     # Repository Pattern
│   └── Data/             # DbContext & Migrations
└── note/                 # Documentation
```

## 💡 Lưu Ý Quan Trọng

1. **KHÔNG commit `appsettings.json`** - file này chứa thông tin nhạy cảm
2. Mỗi thành viên tự tạo file `appsettings.json` từ file `.example`
3. Đảm bảo SQL Server đang chạy trước khi khởi động app
4. Nếu gặp lỗi connection, kiểm tra lại connection string

## 👥 Nhóm Phát Triển
- [Thành viên 1]
- [Thành viên 2]
- [Thành viên 3]
