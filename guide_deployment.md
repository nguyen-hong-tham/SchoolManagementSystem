# Hướng Dẫn Deploy Hệ Thống Microservices (Docker Compose & VPS)

Tài liệu này hướng dẫn chi tiết cách deploy (triển khai) hệ thống Microservices **UniversityManagement** lên môi trường Production (hoặc máy chủ VPS như AWS, Azure, DigitalOcean) bằng phương pháp tối ưu và phổ biến nhất cho đồ án: **Docker & Docker Compose**.

---

## 1. Kiến Trúc Triển Khai (Deployment Architecture)

Đối với một đồ án microservices, việc triển khai trên một máy chủ ảo đơn lẻ (Single VPS) thông qua **Docker Compose** kết hợp với **Nginx** làm Reverse Proxy là phương án tối ưu nhất về cả chi phí lẫn thời gian cài đặt.

```text
                      [ TRÌNH DUYỆT NGƯỜI DÙNG ]
                                  |
                           (Cổng HTTP: 80)
                                  v
                        [ NGINX REVERSE PROXY ]
                                  |
        +-------------------------+-------------------------+
        | (Route: /)              | (Route: /api/users)     | (Route: /api/classes)
        v                         v                         v
  [ FrontendMVC ]           [ UserService ]           [ ClassService ]
   (Port: 5000)              (Port: 5156)              (Port: 5124)
                                  |                         |
                                  +------------+------------+
                                               | (Sự kiện qua AMQP: 5672)
                                               v
                                         [ RabbitMQ ]
                                               ^
                                               |
                                  +------------+------------+
                                  | (Sự kiện)               | (Sự kiện)
                                  v                         v
                            [ SubjectService ]        [ ScoreService ]
                             (Port: 5187)              (Port: 5221)

   * Cơ sở dữ liệu: Tất cả dịch vụ kết nối độc lập tới các DB tương ứng trong Postgres container (5432).
```

---

## 2. Viết Dockerfile Cho Từng Dịch Vụ (C# .NET 9)

Chúng ta cần tạo một file tên là `Dockerfile` (không có đuôi file) đặt tại thư mục gốc của **từng** project service:
* `UserService/Dockerfile`
* `ClassService/Dockerfile`
* `SubjectService/Dockerfile`
* `ScoreService/Dockerfile`
* `FrontendMVC/Dockerfile`

### File mẫu: `Dockerfile` chung cho các C# Web API / MVC
*(Dưới đây là ví dụ cho `UserService`, các service khác chỉ cần thay thế tên dự án tương ứng)*

```dockerfile
# 1. Sử dụng SDK để build ứng dụng
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build-env
WORKDIR /app

# Sao chép file project và restore các gói NuGet
COPY *.csproj ./
RUN dotnet restore

# Sao chép toàn bộ mã nguồn và build dự án
COPY . ./
RUN dotnet publish -c Release -o out

# 2. Tạo Image chạy gọn nhẹ (Runtime)
FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app
COPY --from=build-env /app/out .

# Mở cổng giao tiếp trong container
EXPOSE 8080

# Chạy ứng dụng
ENTRYPOINT ["dotnet", "UserService.dll"]
```
> [!NOTE]
> * Trong .NET 8 và .NET 9, cổng mặc định của container dotnet chạy ở chế độ Production không còn là cổng `80` nữa mà chuyển thành cổng **`8080`**.

---

## 3. Viết File Docker Compose Toàn Hệ Thống

Tạo file `docker-compose.yml` đặt tại thư mục gốc của giải pháp (`d:\ASP.NET\UniversityManagement\docker-compose.yml`):

```yaml
version: '3.8'

services:
  # 1. Cơ sở dữ liệu PostgreSQL chung
  postgres:
    image: postgres:16-alpine
    container_name: university-postgres
    ports:
      - "5432:5432"
    environment:
      POSTGRES_USER: postgres
      POSTGRES_PASSWORD: password123
    volumes:
      - postgres_data:/var/lib/postgresql/data
    networks:
      - university-network

  # 2. Message Broker RabbitMQ
  rabbitmq:
    image: rabbitmq:3-management-alpine
    container_name: university-rabbitmq
    ports:
      - "5672:5672"      # AMQP Broker port
      - "15672:15672"    # Web UI Management
    environment:
      RABBITMQ_DEFAULT_USER: guest
      RABBITMQ_DEFAULT_PASS: guest
    volumes:
      - rabbitmq_data:/var/lib/rabbitmq
    networks:
      - university-network

  # 3. Dịch vụ UserService
  user-service:
    build:
      context: ./UserService
      dockerfile: Dockerfile
    container_name: user-service
    ports:
      - "5156:8080"
    environment:
      - ASPNETCORE_ENVIRONMENT=Production
      - ConnectionStrings__DefaultConnection=Host=postgres;Port=5432;Database=user_db;Username=postgres;Password=password123
      - MessageBroker__Host=rabbitmq
      - MessageBroker__Username=guest
      - MessageBroker__Password=guest
      - Jwt__Key=SuperSecretKeyForJwtAuthenticationThatIsLongEnoughToAvoidErrors123456!
      - Jwt__Issuer=UserService
      - Jwt__Audience=UniversityManagement
    depends_on:
      - postgres
      - rabbitmq
    networks:
      - university-network

  # 4. Dịch vụ ClassService
  class-service:
    build:
      context: ./ClassService
      dockerfile: Dockerfile
    container_name: class-service
    ports:
      - "5124:8080"
    environment:
      - ASPNETCORE_ENVIRONMENT=Production
      - ConnectionStrings__DefaultConnection=Host=postgres;Port=5432;Database=ClassDb;Username=postgres;Password=password123
      - MessageBroker__Host=rabbitmq
      - MessageBroker__Username=guest
      - MessageBroker__Password=guest
    depends_on:
      - postgres
      - rabbitmq
    networks:
      - university-network

  # 5. Dịch vụ SubjectService
  subject-service:
    build:
      context: ./SubjectService
      dockerfile: Dockerfile
    container_name: subject-service
    ports:
      - "5187:8080"
    environment:
      - ASPNETCORE_ENVIRONMENT=Production
      - ConnectionStrings__DefaultConnection=Host=postgres;Port=5432;Database=SubjectDb;Username=postgres;Password=password123
      - MessageBroker__Host=rabbitmq
      - MessageBroker__Username=guest
      - MessageBroker__Password=guest
      - Jwt__Key=SuperSecretKeyForJwtAuthenticationThatIsLongEnoughToAvoidErrors123456!
      - Jwt__Issuer=UserService
      - Jwt__Audience=UniversityManagement
    depends_on:
      - postgres
      - rabbitmq
    networks:
      - university-network

  # 6. Dịch vụ ScoreService
  score-service:
    build:
      context: ./ScoreService
      dockerfile: Dockerfile
    container_name: score-service
    ports:
      - "5221:8080"
    environment:
      - ASPNETCORE_ENVIRONMENT=Production
      - ConnectionStrings__DefaultConnection=Host=postgres;Port=5432;Database=ScoreDb;Username=postgres;Password=password123
      - MessageBroker__Host=rabbitmq
      - MessageBroker__Username=guest
      - MessageBroker__Password=guest
      - Jwt__Key=SuperSecretKeyForJwtAuthenticationThatIsLongEnoughToAvoidErrors123456!
      - Jwt__Issuer=UserService
      - Jwt__Audience=UniversityManagement
    depends_on:
      - postgres
      - rabbitmq
    networks:
      - university-network

  # 7. Dự án Frontend MVC
  frontend-mvc:
    build:
      context: ./FrontendMVC
      dockerfile: Dockerfile
    container_name: frontend-mvc
    ports:
      - "5000:8080"
    environment:
      - ASPNETCORE_ENVIRONMENT=Production
      - Microservices__UserService=http://user-service:8080/api
      - Microservices__ClassService=http://class-service:8080/api
      - Microservices__SubjectService=http://subject-service:8080/api
      - Microservices__ScoreService=http://score-service:8080/api
    depends_on:
      - user-service
      - class-service
      - subject-service
      - score-service
    networks:
      - university-network

# Định nghĩa phân vùng lưu trữ bền vững (Volumes)
volumes:
  postgres_data:
  rabbitmq_data:

# Định nghĩa mạng nội bộ của Docker
networks:
  university-network:
    driver: bridge
```

> [!IMPORTANT]
> * **Tên Host trong Docker Network:** Hãy lưu ý các chuỗi kết nối (`Host=postgres`) và các API endpoint (`http://user-service:8080/api`). Trong môi trường Docker, các container liên lạc với nhau thông qua **Tên Service** làm hostname thay vì dùng `localhost`.
> * **Tự động áp dụng Migration:** Do các file `Program.cs` của hệ thống đều đã có mã nguồn tự động gọi `context.Database.MigrateAsync()` lúc khởi động, nên khi Docker Compose chạy lên, các cơ sở dữ liệu (`user_db`, `ClassDb`, `SubjectDb`, `ScoreDb`) sẽ tự động được tạo và seed dữ liệu đệm mà không cần thao tác thủ công.

---

## 4. Cấu Hình NGINX làm Reverse Proxy (Tùy chọn - Tăng Điểm Đồ Án)

Để ứng dụng chuyên nghiệp, bạn nên cấu hình **Nginx** đứng trước để định tuyến (Route) toàn bộ luồng dữ liệu thông qua cổng `80` tiêu chuẩn.

Tạo file cấu hình `nginx.conf` tại thư mục gốc:
```text
server {
    listen 80;
    server_name localhost;

    # Định tuyến sang giao diện Frontend
    location / {
        proxy_pass http://frontend-mvc:8080;
        proxy_http_version 1.1;
        proxy_set_header Upgrade $http_upgrade;
        proxy_set_header Connection keep-alive;
        proxy_set_header Host $host;
        proxy_cache_bypass $http_upgrade;
    }

    # Định tuyến trực tiếp các API UserService (nếu Frontend gọi Ajax từ Client)
    location /api/users {
        proxy_pass http://user-service:8080;
    }

    # Định tuyến trực tiếp các API ClassService
    location /api/classes {
        proxy_pass http://class-service:8080;
    }
}
```

Thêm cấu hình Nginx vào `docker-compose.yml`:
```yaml
  nginx:
    image: nginx:alpine
    container_name: university-nginx
    ports:
      - "80:80"
    volumes:
      - ./nginx.conf:/etc/nginx/conf.d/default.conf
    depends_on:
      - frontend-mvc
    networks:
      - university-network
```

---

## 5. Hướng Dẫn Deploy Từng Bước Lên VPS (Ví dụ: Ubuntu Server)

Khi đã thuê một VPS chạy hệ điều hành Ubuntu Server, bạn hãy thực hiện các bước sau:

### Bước 1: Cài đặt Docker & Docker Compose trên VPS
Mở Terminal của VPS (qua SSH) và chạy lệnh:
```bash
# Cập nhật hệ thống
sudo apt update && sudo apt upgrade -y

# Cài đặt Docker
curl -fsSL https://get.docker.com -o get-docker.sh
sudo sh get-docker.sh

# Thêm quyền chạy docker không cần sudo
sudo usermod -aG docker $USER
newgrp docker

# Kiểm tra phiên bản docker compose đã cài đặt sẵn
docker compose version
```

### Bước 2: Tải Source Code lên VPS
Bạn có thể tải code lên bằng 2 cách:
1. **Dùng Git (Khuyên dùng):** Đẩy code lên GitHub cá nhân, sau đó trên VPS chạy lệnh `git clone <URL_kho_chứa>`.
2. **Dùng WinSCP / FileZilla:** Truyền file nén `.zip` của dự án lên và giải nén trên VPS (`unzip project.zip`).

### Bước 3: Build & Chạy Hệ Thống
Di chuyển vào thư mục chứa file `docker-compose.yml` trên VPS và chạy lệnh:
```bash
# Khởi chạy hệ thống ở chế độ chạy ngầm (Detached mode)
docker compose up --build -d
```
* Lệnh `--build` sẽ ra lệnh cho Docker tự động đọc các file `Dockerfile` của từng service để tải SDK về biên dịch ra file chạy của C#.
* Quá trình build lần đầu tiên có thể mất từ 3 đến 5 phút để tải các base image.

### Bước 4: Kiểm tra trạng thái hệ thống
Sau khi chạy xong, dùng lệnh sau để kiểm tra xem các container đã hoạt động bình thường chưa:
```bash
docker compose ps
```
Nếu tất cả đều hiển thị trạng thái `Up`, hệ thống đã sẵn sàng!

### Bước 5: Xem Log hệ thống để kiểm tra lỗi (Troubleshooting)
Nếu có bất kỳ dịch vụ nào bị sập hoặc báo lỗi kết nối DB/RabbitMQ, hãy xem log chi tiết bằng lệnh:
```bash
# Xem log thời gian thực của toàn bộ hệ thống
docker compose logs -f

# Hoặc xem log của một service cụ thể
docker compose logs -f user-service
```
Bây giờ, bạn chỉ cần gõ địa chỉ IP công khai (Public IP) của VPS lên trình duyệt là có thể sử dụng đồ án trực tuyến của mình!
