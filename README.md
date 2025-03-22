# Order Management API

## Giới thiệu

Đây là một RESTful API được xây dựng bằng **ASP.NET Core 8** để quản lý đơn hàng, hỗ trợ các chức năng CRUD cơ bản và truy vấn dữ liệu từ **SQL Server**.

## Công nghệ sử dụng

- **ASP.NET Core 8**
- **Entity Framework Core** (Code-First Migration)
- **SQL Server**
- **FluentValidation** (Validation dữ liệu)
- **AutoMapper** (Mapping dữ liệu)
- **Serilog** (Logging)
- **Swagger** (API Documentation)
- **Repository Pattern** (Tách biệt logic)
- **Stored Procedures** (Tối ưu truy vấn SQL)
- **xUnit** (Unit Testing)

## Cài đặt

### 1. Clone Repository
```sh
git clone <repository_url>
cd order-management-api
```

### 2. Cấu hình kết nối cơ sở dữ liệu
Cập nhật chuỗi kết nối trong `appsettings.json`:
```json
"ConnectionStrings": {
  "DefaultConnection": "Server=YOUR_SERVER;Database=OrderDb;User Id=YOUR_USER;Password=YOUR_PASSWORD;TrustServerCertificate=True"
}
```

## 3. Cấu hình Repository (trong file `Program.cs`)

```csharp
// Cấu hình Repository sử dụng stored procedures hoặc truy vấn trực tiếp
builder.Services.ConfigureRepositories(useStoredProcedures: false);
```

### 4. Chạy ứng dụng (ứng dụng đã được cài đặt để tự tạo db nếu chưa có)
```sh
dotnet run
```

API sẽ chạy tại `http://localhost:5251` (hoặc `https://localhost:5001` nếu dùng HTTPS).

## Cấu trúc thư mục
```plaintext
📂 OrderManagementAPI
 ┣ 📂 Controllers       # Chứa các API Controllers
 ┣ 📂 Models            # Chứa các Model
 ┣ 📂 Data              # Chứa DbContext
 ┣ 📂 Migrations        # Chứa Migration và các Stored Procedures
 ┣ 📂 Repositories      # Chứa Repository Pattern
 ┣ 📂 DTOs              # Chứa các Data Transfer Objects
 ┣ 📂 Mappings          # Chứa AutoMapper Profile
 ┣ 📂 StoredProcedures  # Chứa các Stored Procedures SQL
 ┣ 📂 Tests				# Chứa các Unit Test
 ┣ 📂 Extensions		# Chứa các Extensions
 ┣ 📂 Validators		# Chứa các Validators
 ┣ 📜 appsettings.json  # Cấu hình ứng dụng
 ┣ 📜 Program.cs        # Khởi tạo ứng dụng
```

## API Endpoints

### 1. Quản lý Đơn hàng
| Phương thức | Endpoint                 | Mô tả                       |
|------------|-------------------------|-----------------------------|
| `POST`    | `/api/orders`            | Tạo đơn hàng mới           |
| `GET`     | `/api/orders`            | Lấy danh sách đơn hàng     |
| `GET`     | `/api/orders/{id}`       | Lấy chi tiết đơn hàng      |
| `PUT`     | `/api/orders/{id}`       | Cập nhật đơn hàng          |
| `DELETE`  | `/api/orders/{id}`       | Xóa đơn hàng               |

### 2. Quản lý Chi tiết đơn hàng
| Phương thức | Endpoint                                | Mô tả                       |
|------------|----------------------------------------|-----------------------------|
| `POST`    | `/api/orders/{id}/order-details`       | Thêm sản phẩm vào đơn hàng |
| `GET`     | `/api/orders/{id}/order-details`       | Lấy danh sách sản phẩm     |
| `DELETE`  | `/api/order-details/{id}`             | Xóa sản phẩm khỏi đơn hàng |

## Tính năng bổ sung
✅ **Validation dữ liệu** bằng FluentValidation  
✅ **Logging** với Serilog  
✅ **Repository Pattern** để quản lý dữ liệu  
✅ **Swagger UI** để kiểm thử API  
✅ **Stored Procedures** để tối ưu hóa truy vấn  
✅ **Unit Test** với xUnit  
✅ **Pagination** khi lấy danh sách đơn hàng  

## Hướng dẫn chạy Unit Test
```sh
dotnet test
```

---
📌 **Author:** Hoàng Phúc  
🚀 **Version:** 1.0.0  
📅 **Last Updated:** $(date +%Y-%m-%d)
