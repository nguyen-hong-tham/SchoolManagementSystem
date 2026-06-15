# Báo Cáo Kiến Trúc & Quy Trình Phát Triển Dịch Vụ (User Service)

Tài liệu này được biên soạn bởi **Senior .NET Backend Architect** nhằm làm rõ cấu trúc thư mục, quy trình hoạt động của JWT, cơ chế mã hóa mật khẩu, và quy chuẩn phát triển (workflow) khi thêm hoặc cập nhật một endpoint trong dự án **UniversityManagement**.

---

## 1. Cấu Trúc Thư Mục & Ý Nghĩa Từng Thành Phần

### Sơ đồ cấu trúc thư mục hiện tại:
```text
UserService/
│
├── Entities/               # Lớp mô hình chứa cấu trúc bảng dữ liệu (Domain Model)
│   └── User.cs
│
├── Data/                   # Kết nối cơ sở dữ liệu (EF Core DbContext & Migrations)
│   └── AppDbContext.cs
│
├── Repositories/           # Lớp trừu tượng hóa truy cập cơ sở dữ liệu (Data Access Layer)
│   ├── IUserRepository.cs
│   └── UserRepository.cs
│
├── Services/               # Lớp xử lý nghiệp vụ chính của hệ thống (Business Logic Layer)
│   ├── IAuthService.cs
│   ├── AuthService.cs
│   └── JwtService.cs
│
├── DTOs/                   # Lớp chứa cấu trúc dữ liệu truyền nhận giữa Client & Server
│   ├── RegisterRequest.cs
│   ├── LoginRequest.cs
│   ├── LoginResponse.cs
│   ├── UpdateRoleRequest.cs
│   └── UpdateUserRequest.cs
│
├── Controllers/            # Lớp tiếp nhận Request HTTP, điều hướng luồng và trả về Response
│   ├── AuthController.cs
│   ├── UserController.cs
│   └── AdminController.cs
│
├── Middleware/             # Lớp chặn và xử lý Request/Response toàn cục
│   └── ExceptionMiddleware.cs
│
└── Program.cs              # File cấu hình khởi tạo ứng dụng, Dependency Injection, Middleware Pipeline
```

### Ý nghĩa của từng Folder & Thứ tự xây dựng (Flow xây dựng từ dưới lên):

Để xây dựng hệ thống bền vững, thứ tự lập trình luôn đi **từ dưới lên (Database-first / Data-first)** để đảm bảo dữ liệu ổn định trước khi xử lý nghiệp vụ:

1.  **Entities**: Định nghĩa cấu trúc thực thể (Ví dụ: `User.cs`). Đây là nền tảng cốt lõi chứa các thuộc tính sẽ được ánh xạ thành các bảng trong PostgreSQL.
2.  **Data (DbContext)**: Nơi cấu hình EF Core kết nối tới PostgreSQL, định nghĩa các ràng buộc duy nhất (Unique Constraints), chỉ mục (Indexes) và quản lý migrations cơ sở dữ liệu.
3.  **Repositories**: Nơi trực tiếp nói chuyện với database thông qua DbContext. Chứa các câu lệnh CRUD thô (`FindAsync`, `Add`, `Remove`, `Update`). Lớp này giúp tách biệt EF Core ra khỏi logic nghiệp vụ, giúp dễ dàng đổi database hoặc viết Unit Test sau này.
4.  **DTOs (Data Transfer Objects)**: Định nghĩa dữ liệu đầu vào và đầu ra cho API. DTO giúp chúng ta **không để lộ cấu trúc thực thể Database (Entities)** ra ngoài Client, đồng thời áp dụng Data Annotations (`[Required]`, `[EmailAddress]`) để xác thực dữ liệu thô.
5.  **Services**: Trái tim nghiệp vụ của hệ thống. Nhận DTO từ Controller, thực hiện tính toán, gọi Repository để kiểm tra sự tồn tại của dữ liệu, so sánh mật khẩu, tạo mã Token JWT, và trả về dữ liệu chuẩn hóa cho Controller.
6.  **Controllers**: Cổng tiếp nhận HTTP Request. Chỉ có nhiệm vụ kiểm tra tính hợp lệ của DTO (ModelState), chuyển giao công việc cho Service, và đóng gói dữ liệu thành `IActionResult` (Ok, BadRequest, Created, NotFound) trả về Client.
7.  **Middleware**: Chạy trước hoặc sau Controller toàn cục (Ví dụ: `ExceptionMiddleware` bắt toàn bộ lỗi để format JSON đẹp mắt, hoặc Authentication middleware kiểm tra tính hợp lệ của JWT).

### Vì sao cần cấu tạo theo Flow này?

*   **Không phụ thuộc chặt chẽ (Loose Coupling)**: Các lớp giao tiếp thông qua Interface (Ví dụ: `IAuthService`, `IUserRepository`). Khi cần thay đổi logic DB, chỉ cần sửa class Repository mà không làm ảnh hưởng đến Services hay Controllers.
*   **Tái sử dụng (Reusability)**: Một phương thức trong `UserRepository` (như `GetByIdAsync`) có thể được dùng lại bởi nhiều dịch vụ khác nhau (`AuthService`, `UserService`).
*   **Khả năng bảo trì (Maintainability)**: Khi có lỗi phát sinh (ví dụ: lỗi lưu trữ DB), ta biết ngay phải debug ở `Repository`. Khi lỗi nghiệp vụ (ví dụ: sai cách tính điểm hoặc phân quyền), ta debug ở `Service`.
*   **Dễ viết Unit Test**: Chúng ta có thể dễ dàng "giả lập" (mock) dữ liệu từ `IUserRepository` để viết test độc lập cho `AuthService` mà không cần kết nối vào Database PostgreSQL thật.

---

## 2. Quy Trình Hoạt Động Của JWT & Mã Hóa Mật Khẩu (Password Hashing)

### Quy trình hoạt động của JWT (JSON Web Token):

1.  **Đăng nhập**: Client gửi thông tin đăng nhập (Email/Password) lên API `POST /api/auth/login`.
2.  **Xác thực mật khẩu**: `AuthService` kiểm tra email và dùng thư viện BCrypt so sánh mật khẩu thô gửi lên với `PasswordHash` trong DB.
3.  **Khởi tạo mã**: Nếu đúng, `JwtService` tạo mã JWT dựa trên các Claims (Id, Email, Role) và ký số bằng khóa bí mật (`Jwt:Key`).
4.  **Trả về Token**: API trả về HTTP 200 kèm JWT Token dạng string.
5.  **Sử dụng Token**: Trong các Request sau, Client gửi token kèm theo Header: `Authorization: Bearer <Token>`.
6.  **Giải mã tự động**: Middleware xác thực trong `Program.cs` giải mã và kiểm tra token. Nếu hợp lệ, nó sẽ cho phép Request đi qua Controller và tự động điền thông tin User vào context (`User.FindFirst(...)`).

### Các thành phần & file cấu hình tham gia vào luồng JWT:

1.  **Cấu hình khóa ký số**: Nằm trong `appsettings.json` dưới dạng `Jwt:Key`, `Jwt:Issuer`, `Jwt:Audience`.
2.  **Đăng ký Middleware**: Trong `Program.cs`, dịch vụ Authentication và Middleware `JwtBearer` được cấu hình để tự động kiểm tra chữ ký token trong header của mọi HTTP request đi vào.
3.  **Tạo Token (Generation)**: Nằm trong `JwtService.cs`. Nhận đối tượng `User`, tạo danh sách `Claims` (Id, Email, Role), xác định thời gian hết hạn (Expires), ký số bằng thuật toán `HmacSha256` và trả về token dạng string.
4.  **Bảo vệ Endpoint**: Dùng thuộc tính `[Authorize]` hoặc `[Authorize(Roles = "Admin")]` trên các Controller/Action để chỉ cho phép các request có token JWT hợp lệ đi qua.

### Bước mã hóa mật khẩu (Password Hashing):

*   **Mã hóa diễn ra ở đâu?**: Việc mã hóa/xác thực mật khẩu diễn ra hoàn toàn ở **Service Layer (`AuthService.cs`)** trước khi lưu vào DB hoặc khi đối sánh đăng nhập.
*   **Bước Đăng ký (Register) / Tạo mới**:
    *   Người dùng gửi lên mật khẩu thô (ví dụ: `"123456"`).
    *   Trong `AuthService.RegisterAsync` hoặc `CreateUserAsync`, mật khẩu này được mã hóa bằng hàm băm một chiều: 
        `user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);`
    *   Chuỗi băm ngẫu nhiên thu được sẽ được lưu vào DB. Không thể dịch ngược chuỗi này về `"123456"`.
*   **Bước Đăng nhập (Login)**:
    *   Người dùng gửi lên Email và mật khẩu thô `"123456"`.
    *   `AuthService` lấy `User` từ database dựa vào Email, trong đó có trường `PasswordHash`.
    *   Đối sánh bằng hàm xác thực của thư viện BCrypt:
        `bool isValidPassword = BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash);`
    *   Nếu trả về `true`, mật khẩu hợp lệ; ngược lại trả về `false`.

---

## 3. Quy Trình Phát Triển Khi Thêm Hoặc Cập Nhật 1 Endpoint (API)

Khi có yêu cầu cập nhật hoặc thêm mới một API, bạn cần đi theo đúng luồng từ dữ liệu thô $\to$ nghiệp vụ $\to$ endpoint để giữ hệ thống không bị lỗi xung đột hoặc thiếu dependencies:

1.  **Bước 1: Cập nhật Entity/Database**: Nếu API cần thêm trường thông tin mới lưu trữ dưới DB. Cần cập nhật `Entities/User.cs`, cấu hình `AppDbContext.cs` và chạy lệnh tạo Migration.
2.  **Bước 2: Tạo/Cập nhật các Request/Response DTO**: Định nghĩa cấu trúc dữ liệu truyền nhận và ràng buộc validation (`[Required]`, `[EmailAddress]`).
3.  **Bước 3: Thêm hàm vào IUserRepository & UserRepository**: Khai báo và viết câu lệnh truy vấn SQL/EF Core tương tác với DB (nếu cần câu truy vấn mới phức tạp).
4.  **Bước 4: Khai báo Service Interface**: Thêm mô tả hàm vào `IAuthService.cs` hoặc interface dịch vụ tương ứng.
5.  **Bước 5: Cấu hình logic nghiệp vụ trong Service**: Triển khai code logic thật trong `AuthService.cs`.
6.  **Bước 6: Viết API Action trong Controller**: Tạo endpoint mới trong `UserController.cs` / `AdminController.cs`, phân quyền `[Authorize]` và đóng gói response trả về.

### Bảng tóm tắt các file cần bổ sung & thay đổi:

| Nhiệm vụ | Các file cần bổ sung (NEW) | Các file cần chỉnh sửa (MODIFY) | Lý do |
| :--- | :--- | :--- | :--- |
| **1. Cập nhật DB** | Các file Migrations mới (nếu có) | `Entities/User.cs`, `AppDbContext.cs` | Thêm cột mới trong database. |
| **2. Định nghĩa DTO** | `DTOs/NewRequest.cs`, `DTOs/NewResponse.cs` | Không có | Để định cấu trúc và validate input/output. |
| **3. Cập nhật Repo** | Không có | `IUserRepository.cs`, `UserRepository.cs` | Thêm các hàm CRUD thô tương tác DB. |
| **4. Thiết lập Interface** | Không có | `IAuthService.cs` | Khai báo thiết kế trước (Dependency Injection). |
| **5. Viết logic nghiệp vụ** | Không có | `AuthService.cs` | Xử lý logic nghiệp vụ chính của tính năng. |
| **6. Khai báo API Endpoint** | Không có | `UserController.cs` / `AdminController.cs` | Khai báo routing và phân quyền API. |
