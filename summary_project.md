# Báo Cáo Tóm Tắt Đề Tài: Hệ Thống Quản Lý Trường Học (Kiến Trúc Microservices)

Báo cáo này cung cấp thông tin chi tiết về mặt nghiệp vụ, kiến trúc công nghệ, các điểm nổi bật của từng dịch vụ và giải pháp kỹ thuật áp dụng trong đề tài **Hệ thống Quản lý Trường học (School Management System)** phục vụ báo cáo hội đồng khoa chuyên ngành Công nghệ thông tin / Kỹ thuật phần mềm.

---

## I. TỔNG QUAN ĐỀ TÀI

*   **Tên đề tài:** Nghiên cứu và Xây dựng Hệ thống Quản lý Trường học dựa trên Kiến trúc Microservices và Event-Driven.
*   **Kiến trúc cốt lõi:** Microservices (Kiến trúc dịch vụ nhỏ) kết hợp Event-Driven (Mô hình hợp tác hướng sự kiện).
*   **Mục tiêu đề tài:** 
    *   Phân rã nghiệp vụ quản lý giáo dục thành các dịch vụ độc lập nhằm tăng khả năng chịu tải, mở rộng độc lập và tăng tính tự trị (autonomy) của từng phân hệ.
    *   Tối ưu hóa chi phí hạ tầng thực tế ($0 USD) bằng các giải pháp chia nhỏ cơ sở dữ liệu trên đám mây và tối ưu bộ nhớ cho máy chủ cấu hình yếu.

---

## II. MÔ TẢ CHI TIẾT NGHIỆP VỤ HỆ THỐNG

Hệ thống được thiết kế gồm 4 dịch vụ độc lập, phối hợp đồng bộ để hoàn thành các nghiệp vụ giáo dục:

### 1. Phân hệ UserService (Quản lý Người dùng & Xác thực)
*   **Nhiệm vụ:** Quản lý vòng đời tài khoản và phân quyền cho toàn bộ nhân sự (Admin, Giáo viên, Học sinh).
*   **Nghiệp vụ chi tiết:**
    *   *Xác thực (Authentication):* Đăng ký, đăng nhập tài khoản bằng Email và mật khẩu mã hóa Hash. Cấp phát mã khóa bảo mật JWT Token.
    *   *Phân quyền (Authorization):* Phân quyền truy cập dựa trên vai trò (Role-based Access Control - RBAC) gồm 3 cấp bậc: Admin (Quản trị hệ thống), Teacher (Giáo viên), Student (Học sinh).
    *   *Quản lý thông tin hồ sơ (Profiles):* Quản lý thông tin chung (Họ tên, tuổi, giới tính, số điện thoại, địa chỉ) kết hợp hồ sơ đặc thù của Giáo viên (`TeacherProfile` gồm học hàm, bộ môn công tác, ngày tuyển dụng) và Học sinh (Trạng thái học tập, ngày nhập học).

### 2. Phân hệ ClassService (Quản lý Lớp học & Điều phối)
*   **Nhiệm vụ:** Quản lý cơ cấu tổ chức học tập, nhân sự lớp và thời khóa biểu của trường học.
*   **Nghiệp vụ chi tiết:**
    *   *Cơ cấu lớp học (`Class`):* Quản lý danh sách lớp học theo niên khóa và khối lớp (10, 11, 12).
    *   *Điều phối học sinh (`StudentClass`):* Phân phối học sinh vào các lớp học theo từng năm học. Ghi nhận lịch sử chuyển lớp, chuyển trường hoặc lên lớp của học sinh.
    *   *Bố trí chủ nhiệm (`HomeroomAssignment`):* Phân công duy nhất một giáo viên làm chủ nhiệm của một lớp học trong một năm học.
    *   *Phân công giảng dạy (`TeachingAssignment`):* Phân công các giáo viên bộ môn phụ trách dạy từng môn học cụ thể cho các lớp.
    *   *Thời khóa biểu (`Schedule`):* Quản lý lịch học chi tiết hàng ngày cho học sinh bao gồm phòng học, tiết học (1-10) và thứ trong tuần.

### 3. Phân hệ SubjectService (Quản lý Môn học)
*   **Nhiệm vụ:** Quản lý chương trình đào tạo của nhà trường.
*   **Nghiệp vụ chi tiết:**
    *   *Danh mục môn học (`Subject`):* Khai báo thông tin các môn học (Toán, Lý, Hóa, Sinh, Văn, Sử, Địa, Anh...).
    *   *Định mức khối lớp (`GradeLevel`):* Phân bổ môn học tương ứng với khối học sinh (ví dụ môn Công nghệ chỉ học ở khối 10, môn Giáo dục công dân học ở khối 12).

### 4. Phân hệ ScoreService (Quản lý Điểm & Đánh giá)
*   **Nhiệm vụ:** Số hóa bảng điểm học tập, tính toán kết quả và phân loại học sinh.
*   **Nghiệp vụ chi tiết:**
    *   *Nhập điểm thành phần (`Score`):* Nhập điểm chi tiết cho học sinh theo từng môn học và học kỳ (Điểm miệng, Điểm 15 phút, Điểm giữa kỳ, Điểm cuối kỳ).
    *   *Tính toán điểm số:* Tự động áp dụng trọng số điểm để tính toán điểm trung bình môn học.
    *   *Đánh giá tổng kết (`AcademicResult`):* Tự động tính toán điểm trung bình tích lũy (GPA) toàn học kỳ/năm học và phân loại xếp học lực (*Xuất sắc, Giỏi, Khá, Trung bình, Yếu*).

---

## III. MÔ TẢ CHI TIẾT CÁC CÔNG NGHỆ ÁP DỤNG & GIẢI PHÁP KỸ THUẬT

Đề tài áp dụng các công nghệ hiện đại trong hệ sinh thái .NET và DevOps:

```text
+-----------------------------------------------------------------------------------+
|                                 TRÌNH DUYỆT USER                                  |
+-----------------------------------------------------------------------------------+
                                         │
                                       (HTTPS)
                                         ▼
+-----------------------------------------------------------------------------------+
|                        NGINX GATEWAY / REVERSE PROXY                              |
+-----------------------------------------------------------------------------------+
                                         │
                         (Định tuyến nội bộ qua Docker Network)
                                         ▼
+-----------------------------------------------------------------------------------+
|                              FRONTEND MVC (Razor)                                 |
+-----------------------------------------------------------------------------------+
       │                       │                       │                       │
  (Call HTTP)             (Call HTTP)             (Call HTTP)             (Call HTTP)
       ▼                       ▼                       ▼                       ▼
+--------------+        +--------------+        +--------------+        +--------------+
| UserService  |        | ClassService |        |SubjectService|        | ScoreService |
+--------------+        +--------------+        +--------------+        +--------------+
       │                       │                       │                       │
 (MassTransit)           (MassTransit)                 │                 (MassTransit)
       └───┬───────────────────┴───────────────────────┼───────────────────────┘
           ▼                                           │
+---------------------+                                │
|   RABBITMQ CLOUD    |                                │
|    (CloudAMQP)      |                                │
+---------------------+                                ▼
           │                             +---------------------------+
           │                             |      SUPABASE CLOUD       |
           └────────────────────────────►|  (PostgreSQL Multi-Schema)|
                                         +---------------------------+
```

### 1. Backend Core: ASP.NET Core 9 & Entity Framework Core
*   **Công nghệ:** Phiên bản .NET 9 LTS mới nhất giúp nâng cao hiệu năng thực thi, giảm thời gian khởi động container và tiết kiệm bộ nhớ RAM.
*   **EF Core & Npgsql:** Sử dụng thư viện EF Core để giao tiếp với PostgreSQL thông qua ORM. Sử dụng cơ chế Code-First Migration để đồng bộ hóa mã nguồn C# và cơ sở dữ liệu.

### 2. Frontend MVC & Giải pháp bảo mật JWT Token
*   **Giải pháp BFF (Backend-For-Frontend):** Thay vì để Client Javascript gọi trực tiếp các API Backend, dự án sử dụng **ASP.NET Core MVC (C# Razor)** làm trung gian định tuyến.
*   **Bảo mật JWT an toàn:** JWT Token sau khi UserService cấp phát sẽ được lưu trong **Secure HTTP-Only Cookie** ở trình duyệt. Cookie này có các cờ bảo mật (`HttpOnly`, `Secure`, `SameSite=Strict`), giúp loại bỏ hoàn toàn nguy cơ bị mã độc Javascript tấn công XSS lấy cắp token. Khi gọi API, Controller phía Backend của Frontend MVC tự động đọc Cookie này và đính kèm vào Header của `HttpClient`.

### 3. Database: Supabase PostgreSQL & Phân tách logical Schema
*   **Giải pháp Multi-Schema:** Dự án chia nhỏ cơ sở dữ liệu dùng chung thành các Schema riêng biệt:
    *   UserService sở hữu schema `users`.
    *   ClassService sở hữu schema `classes`.
    *   SubjectService sở hữu schema `subjects`.
    *   ScoreService sở hữu schema `scores`.
*   **Lợi ích:** Giải quyết được bài toán kinh tế khi triển khai đám mây (dùng chung 1 database miễn phí nhưng vẫn đảm bảo tính cô lập dữ liệu của Microservices). Hệ thống có thể mở rộng độc lập, dễ dàng phân mảnh dữ liệu (database sharding) sau này.

### 4. Giao tiếp Event-Driven: MassTransit & CloudAMQP (RabbitMQ)
*   **Công nghệ:** Sử dụng **MassTransit** làm thư viện trung gian (Abstraction Layer) kết nối đến **CloudAMQP (RabbitMQ)** thông qua giao thức bảo mật **AMQPS/SSL (Cổng 5672/5671)**.
*   **Mô hình Pub/Sub (Publish/Subscribe):** 
    *   Khi `UserService` có thay đổi về người dùng, nó sẽ bắn ra sự kiện `UserCreatedEvent`, `UserUpdatedEvent` hoặc `UserDeletedEvent`.
    *   `ClassService` và `ScoreService` đăng ký lắng nghe (Consume) để cập nhật dữ liệu đệm local.
*   **Ưu điểm:** Giảm thiểu độ trễ, loại bỏ sự phụ thuộc đồng thời (tight coupling) giữa các dịch vụ, nâng cao tính chịu lỗi (Fault Tolerance) của hệ thống.

### 5. Hạ tầng DevOps: Docker, Nginx & VPS Swap
*   **Dockerization:** Đóng gói toàn bộ 5 dịch vụ thành các Docker Image gọn nhẹ dựa trên Alpine Linux.
*   **Nginx Reverse Proxy:** Nginx hoạt động như một API Gateway gọn nhẹ (chỉ tốn ~5MB RAM), gánh cổng HTTPS bảo mật SSL Let's Encrypt (Certbot) và định tuyến lưu lượng vào mạng nội bộ Docker.
*   **RAM ảo hóa (Swap file 4GB):** Cấu hình thêm 4GB Swap file trên VPS SSD của Oracle Cloud. Điều này giải quyết triệt để lỗi Out of Memory (OOM) cho các tiến trình .NET khi chạy trên hệ thống RAM vật lý chỉ có 1GB.

---

## IV. CÁC ĐIỂM NỔI BẬT VÀ GIẢI PHÁP ĐẶC THÙ CỦA TỪNG SERVICE

### 1. UserService
*   **Mã hóa bảo mật:** Sử dụng thuật toán BCrypt để băm mật khẩu (password hashing) chống lại các cuộc tấn công Brute-force dữ liệu.
*   **Hồ sơ đa hình (Polymorphism Database):** Thiết lập quan hệ 1-1 có điều kiện (`One-to-One Cascade`) giữa bảng `Users` với bảng đặc thù `TeacherProfiles` để tách biệt cấu trúc dữ liệu của các nhóm đối tượng mà không làm phình to bảng thông tin chung.

### 2. ClassService
*   **Ràng buộc chỉ mục phức tạp (Composite Unique Index):** 
    *   Thiết lập index duy nhất kết hợp 3 cột: `StudentId`, `SchoolYear` và trạng thái `IsCurrent` có điều kiện lọc (`WHERE "IsCurrent" = true`).
    *   **Mục đích:** Ràng buộc chặt chẽ nghiệp vụ ở mức cơ sở dữ liệu: Một học sinh không bao giờ được phép nằm trong 2 lớp học khác nhau trong cùng một năm học hiện tại.
*   **Cơ chế Local Cache (Bảng đệm):** Tự xây dựng bảng đệm `CachedUsers` và `CachedSubjects` để phục vụ các câu lệnh JOIN hoặc kiểm tra điều kiện lớp học tức thời mà không phải thực hiện các cuộc gọi API xuyên suốt mạng (cross-network calls).

### 3. SubjectService
*   **Index mã môn học (Unique Code Index):** Thiết lập index duy nhất trên cột `Code` của môn học nhằm tối ưu hóa hiệu năng tìm kiếm và tăng tốc độ xử lý khi phân phối chương trình đào tạo.

### 4. ScoreService
*   **Ràng buộc kiểm tra phạm vi (Check Constraint):**
    *   Thiết lập ràng buộc `CK_Score_Points` trực tiếp trên cột điểm số: `ScoreValue >= 0.0 AND ScoreValue <= 10.0`.
    *   **Mục đích:** Đảm bảo toàn vẹn dữ liệu điểm số học tập của học sinh luôn nằm trong thang điểm 10 chuẩn của Bộ Giáo Dục, ngăn ngừa lỗi nhập liệu sai từ phía API.
*   **Tính toán xếp loại học lực tự động (GPA engine):** Hệ thống tự động quét và tính toán điểm trung bình có trọng số của toàn bộ các môn học và tự động xếp loại học lực ngay khi có điểm mới.

---

## V. ĐÁNH GIÁ ĐỀ TÀI & KẾT LUẬN

1.  **Tính khoa học và thực tiễn:** Đề tài áp dụng các mẫu thiết kế phần mềm tiên tiến (Microservices, Event-Driven, BFF Pattern) đang được các tập đoàn công nghệ lớn sử dụng rộng rãi, mang tính thực tiễn cao trong quản lý giáo dục.
2.  **Khả năng chịu tải và tính sẵn sàng:** Nhờ tính phi tập trung của cơ sở dữ liệu (Supabase Schemas) và broker tin nhắn (CloudAMQP), hệ thống có khả năng phục hồi lỗi rất tốt. Nếu một dịch vụ bị dừng, các dịch vụ khác vẫn chạy bình thường.
3.  **Tối ưu hóa kinh tế:** Đề tài đã minh chứng việc triển khai thành công một hệ thống lớn chạy 5 container độc lập lên máy chủ ảo RAM yếu (1GB RAM) bằng cách thiết kế phân tách hạ tầng và áp dụng kỹ thuật Swap RAM ảo mà không tốn bất kỳ chi phí duy trì nào.

Tài liệu này được biên soạn đầy đủ cấu trúc của một báo cáo khoa học, giúp sinh viên/học viên dễ dàng trích xuất thông tin để trình bày slide và viết báo cáo đề tài nộp hội đồng khoa!
