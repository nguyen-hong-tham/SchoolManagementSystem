# School Management System (Hệ Thống Quản Lý Trường Học - Kiến Trúc Microservices)

Dự án **School Management System** là một hệ thống quản lý trường học (quản lý học sinh, giáo viên, lớp học, môn học và điểm số) được thiết kế theo kiến trúc **Microservices** hiện đại, kết hợp mô hình **Event-Driven Architecture (MassTransit + RabbitMQ)** và được triển khai tối ưu trên các nền tảng đám mây miễn phí trọn đời (**Oracle Cloud, Supabase, CloudAMQP**).

---

## 1. Mô Tả Chi Tiết Nghiệp Vụ Hệ Thống (Business Services)

Hệ thống được chia nhỏ thành 4 dịch vụ nghiệp vụ cốt lõi (Microservices) độc lập hoàn toàn về logic và cơ sở dữ liệu:

### 1.1. UserService (Dịch vụ Quản lý Người dùng & Xác thực)
*   **Mục tiêu:** Quản lý tài khoản, hồ sơ cá nhân và phân quyền truy cập hệ thống.
*   **Các vai trò quản lý (Roles):** 
    *   `Admin`: Quản trị viên hệ thống có quyền tạo, sửa, xóa người dùng và phân công vai trò.
    *   `Teacher`: Giáo viên (có hồ sơ riêng về học hàm, chuyên môn, khoa bộ môn).
    *   `Student`: Học sinh (có hồ sơ về mã học sinh, ngày nhập học, trạng thái).
*   **Nghiệp vụ cốt lõi:**
    *   **Xác thực & Phân quyền:** Đăng nhập, đăng ký tài khoản, mã hóa mật khẩu, cấp phát **JWT Token** và kiểm tra phân quyền (Role-based Authorization) trên các API API.
    *   **Quản lý người dùng:** Quản lý thông tin cá nhân (Họ tên, ngày sinh, giới tính, số điện thoại, địa chỉ) của toàn bộ nhân sự trong trường học.

### 1.2. ClassService (Dịch vụ Quản lý Lớp học & Phân công)
*   **Mục tiêu:** Quản lý toàn bộ cơ cấu lớp học, lịch học và phân công giảng dạy.
*   **Nghiệp vụ cốt lõi:**
    *   **Quản lý lớp học (`Class`):** Khởi tạo danh sách lớp theo từng khối (khối 10, 11, 12) và niên khóa (ví dụ: lớp `10A1` năm học `2025-2026`).
    *   **Phân lớp học sinh (`StudentClass`):** Quản lý lịch sử học sinh thuộc lớp nào theo từng năm học (phục vụ nghiệp vụ lên lớp, chuyển lớp).
    *   **Phân công chủ nhiệm (`HomeroomAssignment`):** Phân công giáo viên chủ nhiệm cho từng lớp học theo năm học.
    *   **Phân công giảng dạy (`TeachingAssignment`):** Gán giáo viên bộ môn phụ trách dạy môn học cụ thể cho từng lớp (ví dụ: Thầy A dạy Toán lớp 10A1).
    *   **Thời khóa biểu (`Schedule`):** Quản lý lịch học chi tiết theo thứ, tiết học và phòng học của từng lớp.

### 1.3. SubjectService (Dịch vụ Quản lý Môn học)
*   **Mục tiêu:** Quản lý danh mục các môn học và phân phối chương trình học theo khối lớp.
*   **Nghiệp vụ cốt lõi:**
    *   **Quản lý môn học (`Subject`):** Danh mục môn học trong trường (Toán, Văn, Anh, Vật Lý, Hóa Học, Tin Học, Thể Dục...).
    *   **Chương trình học theo khối (`GradeLevel`):** Quy định các môn học bắt buộc cho từng khối lớp (ví dụ: khối 10 học 10 môn, khối 12 học 8 môn).

### 1.4. ScoreService (Dịch vụ Quản lý Điểm & Học lực)
*   **Mục tiêu:** Quản lý quá trình nhập điểm, tính điểm trung bình và xếp loại học lực của học sinh.
*   **Nghiệp vụ cốt lõi:**
    *   **Quản lý điểm số (`Score`):** Nhập điểm chi tiết cho học sinh theo môn học, học kỳ và năm học bao gồm: *Điểm miệng, Điểm 15 phút, Điểm giữa kỳ, và Điểm cuối kỳ*.
    *   **Điểm trung bình:** Tự động tính toán điểm trung bình môn học dựa trên trọng số điểm thành phần.
    *   **Xếp loại học lực (`AcademicResult`):** Tính điểm trung bình chung học kỳ (GPA) và xếp loại học lực học sinh (*Xuất sắc, Giỏi, Khá, Trung bình, Yếu*).

---

## 2. Thiết Kế & Giải Pháp Công Nghệ Áp Dụng 

Dự án được xây dựng với mục tiêu tối ưu hiệu năng chạy trên phần cứng yếu (VPS Free 1GB RAM) và tuân thủ các nguyên tắc thiết kế hệ thống phân tán:

### 2.1. Cô lập dữ liệu bằng Schema trên Supabase PostgreSQL (Database per Service)
*   **Vấn đề:** Theo chuẩn Microservices, mỗi dịch vụ phải có database riêng để tránh liên kết chặt (tight coupling). Tuy nhiên, các nhà cung cấp đám mây miễn phí (như Supabase) chỉ cấp **1 Database vật lý miễn phí**.
*   **Giải pháp:** Sử dụng tính năng **PostgreSQL Schema** của Supabase để phân vùng logic database thành 4 vùng độc lập: `users.*`, `classes.*`, `subjects.*`, `scores.*`.
*   **Kết quả:** Mỗi service chỉ được truy cập vào schema của chính mình thông qua cấu hình EF Core `modelBuilder.HasDefaultSchema()`. Việc này giúp loại bỏ hoàn toàn xung đột tên bảng (ví dụ: bảng `CachedUsers` xuất hiện ở cả ClassService và ScoreService nhưng có cấu trúc cột khác nhau).

### 2.2. Giao tiếp bất đồng bộ qua Sự kiện (Event-Driven Collaboration)
*   **Vấn đề:** Khi xếp lớp học sinh ở `ClassService`, hệ thống cần kiểm tra học sinh đó có tồn tại hay không. Nếu gọi HTTP API trực tiếp sang `UserService`, hệ thống sẽ bị trễ mạng và sập dây chuyền nếu `UserService` offline (Single Point of Failure).
*   **Giải pháp:** Dùng **MassTransit** tích hợp **CloudAMQP (RabbitMQ)** để gửi sự kiện bất đồng bộ. Khi tạo học sinh mới ở `UserService`, nó sẽ bắn ra sự kiện `UserCreatedEvent`. `ClassService` và `ScoreService` lắng nghe sự kiện này và tự động cập nhật thông tin học sinh vào bảng đệm local (`CachedUsers`).
*   **Kết quả:** Khi xếp lớp hoặc nhập điểm, các dịch vụ chỉ cần query bảng `CachedUsers` ngay trong schema của mình, đảm bảo tốc độ cực nhanh và hoạt động độc lập ngay cả khi các dịch vụ khác đang dừng.

### 2.3. Nginx làm API Gateway siêu nhẹ (Tiết kiệm RAM)
*   **Vấn đề:** Chạy thêm 1 gateway viết bằng .NET (như YARP/Ocelot) sẽ ngốn thêm khoảng 150MB RAM của VPS, vượt quá giới hạn RAM 1GB của máy chủ Oracle Free.
*   **Giải pháp:** Dùng **Nginx** cài trực tiếp trên máy chủ VPS làm API Gateway / Reverse Proxy.
*   **Kết quả:** Nginx chỉ tốn **5MB RAM**, đảm nhận vai trò định tuyến cổng 80/443 của tên miền sang cổng `5281` của Frontend MVC. Đồng thời, toàn bộ 4 API microservices backend được giữ an toàn ẩn bên trong mạng nội bộ của Docker (`university-network`), không bị lộ ra ngoài internet.

---

## 3. Hướng Dẫn Cài Đặt Hệ Thống

### 3.1. Cấu hình file môi trường `.env`
Sao chép file cấu hình mẫu `.env.example` thành file thực tế `.env` tại thư mục gốc dự án:
```powershell
# Chạy lệnh này trên Powershell local hoặc dùng File Explorer sao chép
Copy-Item .env.example .env
```
Mở file `.env` và điền các tài khoản Supabase (Pooler Session mode cổng `5432` hoặc `6543`) và CloudAMQP của bạn vào:
```env
SUPABASE_DB_CONNECTION=Host=aws-1-ap-southeast-2.pooler.supabase.com;Port=5432;Database=postgres;Username=postgres.kyyggmybqrcujlgxhdss;Password=YOUR_PASSWORD;SSL Mode=Require;Trust Server Certificate=true;

CLOUDAMQP_HOST=capybara.lmq.cloudamqp.com
CLOUDAMQP_USERNAME=nimkrytd
CLOUDAMQP_PASSWORD=YOUR_PASSWORD
CLOUDAMQP_VIRTUALHOST=nimkrytd

JWT_KEY=SuperSecretKeyForJwtAuthenticationThatIsLongEnoughToAvoidErrors123456!
```

---

### 3.2. Khởi chạy dưới máy Local (Máy cá nhân)

#### Yêu cầu:
*   Đã cài [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
*   Đã cài [Docker Desktop](https://www.docker.com/products/docker-desktop/)

#### Các bước thực hiện:
1.  Khởi chạy dịch vụ RabbitMQ cục bộ bằng Docker:
    ```bash
    docker compose up -d
    ```
2.  Khởi động đồng thời 5 dịch vụ .NET bằng file script tự động:
    ```cmd
    run_all.bat
    ```
3.  Truy cập giao diện hệ thống tại địa chỉ: `http://localhost:5281`

---

### 3.3. Khởi chạy trên Production (VPS Oracle Cloud)

#### Bước 1: SSH vào VPS của bạn và thiết lập bộ nhớ Swap 4GB (RAM ảo)
*Đây là bước bắt buộc để tránh tràn bộ nhớ RAM vật lý 1GB trên VPS.*
```bash
sudo fallocate -l 4G /swapfile
sudo chmod 600 /swapfile
sudo mkswap /swapfile
sudo swapon /swapfile
echo '/swapfile none swap sw 0 0' | sudo tee -a /etc/fstab
sudo sysctl vm.swappiness=10
echo 'vm.swappiness=10' | sudo tee -a /etc/sysctl.conf
```

#### Bước 2: Tải code và tạo file `.env` trên VPS
```bash
git clone https://github.com/nguyen-hong-tham/SchoolManagementSystem
cd SchoolManagementSystem
nano .env # Dán nội dung file .env thực tế vào đây rồi lưu lại
```

#### Bước 3: Khởi chạy Docker Compose
```bash
docker compose -f docker-compose.prod.yml up -d --build
```
*Lưu ý: Docker sẽ tự khởi động các dịch vụ và tự kết nối Supabase chạy migration tạo toàn bộ bảng biểu.*

#### Bước 4: Cài đặt Nginx & SSL HTTPS miễn phí với Certbot
1.  Cài đặt Nginx:
    ```bash
    sudo apt update && sudo apt install nginx -y
    ```
2.  Tạo file cấu hình chuyển tiếp định tuyến:
    ```bash
    sudo nano /etc/nginx/sites-available/university-management
    ```
    *Dán cấu hình server block trỏ cổng 80 vào cổng `5281` của Frontend MVC (như chi tiết ở tài liệu hướng dẫn bên dưới).*
3.  Cài đặt Certbot và xin chứng chỉ SSL:
    ```bash
    sudo apt install certbot python3-certbot-nginx -y
    sudo certbot --nginx -d tên-miền-duckdns-của-bạn.duckdns.org
    ```

---

