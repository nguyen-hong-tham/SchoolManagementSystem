# Hướng Dẫn Kiến Trúc Triển Khai Hệ Thống (Production Deployment Guide)

Tài liệu này giải thích chi tiết kiến trúc triển khai hệ thống Microservices **UniversityManagement**, lý do tại sao chúng ta thiết lập như vậy, cách hoạt động của từng thành phần và cách giải quyết các vấn đề kỹ thuật phát sinh.

---

## 1. Sơ Đồ Kiến Trúc Hệ Thống (Deployment Architecture)

Dưới đây là sơ đồ luồng hoạt động thực tế của hệ thống khi chạy trên môi trường Production:

```mermaid
graph TD
    User([Trình duyệt Người dùng]) -->|HTTPS: Cổng 443| Nginx[Nginx Reverse Proxy / Gateway]
    
    subgraph VPS Oracle Cloud Always Free (1GB RAM)
        Nginx -->|Proxy nội bộ: Cổng 5281| FE[Frontend MVC Container]
        FE -->|Gọi API nội bộ: Cổng 8080| US[User Service Container]
        FE -->|Gọi API nội bộ: Cổng 8080| CS[Class Service Container]
        FE -->|Gọi API nội bộ: Cổng 8080| SubS[Subject Service Container]
        FE -->|Gọi API nội bộ: Cổng 8080| ScoreS[Score Service Container]
    end

    subgraph Supabase (Cloud PostgreSQL)
        US -->|Đọc/Ghi Schema: users| DB[(Database: postgres)]
        CS -->|Đọc/Ghi Schema: classes| DB
        SubS -->|Đọc/Ghi Schema: subjects| DB
        ScoreS -->|Đọc/Ghi Schema: scores| DB
    end

    subgraph CloudAMQP (Cloud RabbitMQ)
        US -->|Publish / Subscribe| MQ{RabbitMQ Broker}
        CS -->|Publish / Subscribe| MQ
        ScoreS -->|Publish / Subscribe| MQ
    end

    DuckDNS[DuckDNS / Cloudflare] -.->|Phân giải Domain sang IP VPS| User
    Certbot[Certbot / Let's Encrypt] -->|Cấp chứng chỉ SSL tự động| Nginx
```

---

## 2. Giải Thích Chi Tiết Từng Thành Phần & Lý Do Thiết Lập

### 2.1. VPS Oracle Cloud (Always Free)
*   **Tại sao lại chọn?** Oracle Cloud cung cấp gói máy chủ ảo miễn phí trọn đời (Always Free). Bạn đang sử dụng gói `VM.Standard.E2.1.Micro` với cấu hình **1 vCPU (AMD) và 1 GB RAM**.
*   **Vấn đề RAM và Swap (Bộ nhớ ảo):**
    *   *Tại sao phải thiết lập Swap 4GB?* 5 container .NET Core (4 backend + 1 frontend) khi chạy cùng lúc sẽ tiêu tốn khoảng 500MB - 700MB RAM. Hệ điều hành Ubuntu trên VPS cũng cần khoảng 200MB - 300MB RAM để duy trì hoạt động. Nếu không có bộ nhớ ảo Swap, khi RAM vật lý (1GB) bị sử dụng hết, cơ chế **OOM-Killer (Out of Memory)** của Linux sẽ tự động tắt các ứng dụng .NET của bạn để tránh treo máy.
    *   *Swap hoạt động như thế nào?* Nó cắt 1 phần ổ cứng SSD (4GB) để làm "RAM ảo". Khi RAM vật lý bị đầy, OS sẽ đẩy các dữ liệu ít sử dụng từ RAM sang Swap để nhường chỗ cho dữ liệu mới, giúp hệ thống hoạt động liên tục 24/7 mà không bị sập.

---

### 2.2. Supabase (Database PostgreSQL Đám Mây)
*   **Tại sao không chạy PostgreSQL trực tiếp trên VPS?** Một container PostgreSQL tự host chạy ngốn tối thiểu 150MB - 300MB RAM. Chạy chung trên VPS 1GB RAM sẽ làm sập máy chủ ngay lập tức. Đẩy DB sang Supabase giúp giảm tải hoàn toàn phần cứng cho VPS.
*   **Phân biệt Database và Schema:**
    *   *Database (Cơ sở dữ liệu):* Giống như một **tòa nhà lớn**. Supabase gói Free chỉ cấp cho chúng ta đúng 1 database vật lý tên là `postgres`.
    *   *Schema:* Giống như các **phòng ban độc lập** trong tòa nhà đó.
    *   *Tại sao phải chia Schema?* Dự án của chúng ta là Microservices. Theo đúng nguyên tắc, mỗi service phải sở hữu database riêng để cô lập dữ liệu. Vì Supabase chỉ cho 1 database, chúng ta đã cấu hình EF Core chia nhỏ database thành 4 schema cô lập về mặt logic: `users.*`, `classes.*`, `subjects.*`, `scores.*`.
    *   *Tránh xung đột cấu trúc bảng:* Cả `ClassService` và `ScoreService` đều có thực thể cache tên là `CachedUsers`, nhưng cấu trúc các cột của chúng khác nhau (ClassService có thêm trường `StudentStatus`). Nếu không dùng schema riêng, hai bảng này sẽ ghi đè lên nhau gây lỗi hệ thống.
*   **Chế độ kết nối: Connection Pooler (Session vs Transaction Mode):**
    *   *Direct Connection (Cổng 5432):* Kết nối trực tiếp đến PostgreSQL. Supabase free chỉ hỗ trợ giao thức **IPv6** cho kết nối trực tiếp này. Vì thế máy tính local của bạn (chỉ có IPv4) không thể chạy lệnh migration qua cổng này được.
    *   *Connection Pooler (Cổng 6543 / 5432 Pooler):* Là một trạm trung chuyển kết nối hỗ trợ mạng **IPv4**, giúp local kết nối được.
    *   *Session Mode vs Transaction Mode:* Chế độ `Transaction` chia sẻ kết nối cực nhanh nhưng không hỗ trợ các lệnh thay đổi cấu trúc bảng (DDL) của EF Core Migration. Chế độ `Session` giữ kết nối lâu hơn và hỗ trợ đầy đủ lệnh Migration, đó là lý do vì sao ta phải chuyển Pool Mode sang `Session` để chạy migration từ máy Local thành công.

---

### 2.3. CloudAMQP (RabbitMQ Đám Mây)
*   **Tại sao không chạy RabbitMQ trực tiếp trên VPS?** Giống như PostgreSQL, RabbitMQ viết bằng Erlang nên ngốn rất nhiều RAM (khoảng 200MB - 400MB RAM). Tự host RabbitMQ trên VPS 1GB RAM là bất khả thi.
*   **Virtual Host (VHost) & SSL:**
    *   *Virtual Host:* Trên CloudAMQP gói Free, bạn được cấp một "không gian riêng" (Virtual Host) trùng với tên Username của bạn (ví dụ: `nimkrytd`), chứ không phải là dấu `/` mặc định của RabbitMQ local.
    *   *Tại sao phải bật SSL?* Kết nối đi ra ngoài môi trường Internet cần được mã hóa bảo mật. Vì vậy, ta phải cấu hình MassTransit bật SSL (`MessageBroker__UseSsl=true`) và truyền chính xác cấu hình `VirtualHost` đã lấy từ CloudAMQP.

---

### 2.4. DuckDNS & Nginx làm Gateway
*   **DuckDNS (DDNS):** Do IP Public của VPS Oracle Free đôi khi có thể bị thay đổi nếu bạn khởi động lại hoặc tắt máy tạo lại, DuckDNS giúp gán một tên miền động miễn phí trỏ về IP của VPS đó.
*   **Nginx đóng vai trò API Gateway kiêm Reverse Proxy:**
    *   *Tại sao không dùng API Gateway viết bằng .NET (như YARP hoặc Ocelot)?* Một ứng dụng .NET chạy Gateway sẽ ngốn thêm khoảng 150MB RAM của VPS. Nginx viết bằng C, chạy cực nhẹ (chỉ tốn khoảng 5MB - 10MB RAM) mà hiệu năng lại cực kỳ cao.
    *   *Cách hoạt động:* Nginx gánh cổng công cộng 80 (HTTP) và 443 (HTTPS), nhận yêu cầu từ người dùng, giải mã SSL, và đẩy thẳng yêu cầu nội bộ đến container `frontend-mvc` (cổng 5281) bên trong Docker. Các cổng API của microservices được bảo mật ẩn bên trong mạng ảo Docker (`university-network`) và không cần phải mở ra ngoài internet.
*   **Certbot (Let's Encrypt):** Là một công cụ tự động giao tiếp với Let's Encrypt để xin chứng chỉ bảo mật SSL miễn phí và tự chèn cấu hình HTTPS vào Nginx, tự động gia hạn mỗi 90 ngày.

---

## 3. Giải Thích Các Lỗi Đã Gặp Và Cách Sửa

| Lỗi gặp phải | Nguyên nhân | Cách xử lý |
| :--- | :--- | :--- |
| **Timeout khi chạy `dotnet ef database update` từ Local** | Supabase direct host chỉ hỗ trợ IPv6, mạng internet local của bạn là IPv4 nên không thể kết nối. | Đổi cấu hình sang địa chỉ **Connection Pooler** hỗ trợ IPv4 qua cổng `6543` hoặc `5432` của Pooler. |
| **Lỗi DDL / History Table Timeout trên Supabase** | Supabase Pooler đang chạy ở chế độ `Transaction Mode`, chặn các câu lệnh tạo bảng của EF Core. | Truy cập Supabase Console đổi **Pool Mode** sang **`Session`**. |
| **Lỗi `NOT_ALLOWED - 'nimkrytd' doesn't have access to '/'`** | Code C# ban đầu hardcode Virtual Host của RabbitMQ là `"/"`, trong khi CloudAMQP yêu cầu VHost là `nimkrytd`. | Cập nhật `Program.cs` để đọc biến `MessageBroker:VirtualHost` từ file cấu hình `.env`. |
| **Lỗi `Unable to load the service index for source https://api.nuget.org`** | Docker trên hệ điều hành Ubuntu mặc định không thể phân giải DNS của container ra ngoài internet do cơ chế loopback (`127.0.0.53`). | Tạo file cấu hình `/etc/docker/daemon.json` và cấu hình DNS cố định là `8.8.8.8` và `1.1.1.1`. |
| **Lỗi `git pull` bị hủy (Aborted)** | Do chúng ta chỉnh sửa file `docker-compose.prod.yml` trực tiếp trên VPS nên xảy ra xung đột khi kéo code mới. | Chạy lệnh `git checkout -- docker-compose.prod.yml` để xóa bỏ thay đổi tạm thời trên VPS rồi chạy lại `git pull`. |
