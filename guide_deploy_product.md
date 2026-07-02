# Hướng Dẫn Deploy Lên Môi Trường Free (Product Live Portfolio)

Tài liệu này hướng dẫn chi tiết cách triển khai (deploy) hệ thống Microservices **UniversityManagement** lên máy chủ miễn phí (**Oracle Cloud Always Free VM**) kết hợp tên miền miễn phí (**DuckDNS** hoặc Cloudflare) để tạo một link sản phẩm (portfolio) chạy thực tế trực tuyến có HTTPS.

---

## 1. Lựa chọn nhà cung cấp Hosting Miễn Phí (Free Tiers)

Đối với hệ thống Microservices chạy bằng **Docker Compose** bao gồm nhiều thành phần:
* 5 dịch vụ .NET 9 (UserService, ClassService, SubjectService, ScoreService, FrontendMVC)
* 1 Database PostgreSQL
* 1 Message Broker RabbitMQ

Hệ thống này cần tối thiểu **2GB - 4GB RAM** để vận hành ổn định. Các giải pháp như Render Free, Railway Free hay AWS/GCP Free (chỉ có 1GB RAM) sẽ dễ bị lỗi treo máy (Out of Memory - OOM).

### Giải pháp tối ưu nhất: Oracle Cloud Always Free Tier
* **Cấu hình:** 4 vCPUs (ARM Ampere), **24 GB RAM**, 200 GB SSD.
* **Chi phí:** **Hoàn toàn miễn phí trọn đời (Always Free)**.
* **Đánh giá:** Đây là cấu hình cực mạnh, dư sức chạy mượt mà hệ thống Microservices này và hàng chục dịch vụ khác.

---

## 2. Bước 1: Đăng ký Oracle Cloud Free Tier & Tạo Máy Chủ (VM Instance)

### 1. Đăng ký tài khoản Oracle Cloud
1. Truy cập [Oracle Cloud Free Tier](https://www.oracle.com/cloud/free/).
2. Nhấn **Start for free** và điền thông tin đăng ký.
3. *Lưu ý quan trọng:* Cần một thẻ tín dụng (Visa/Mastercard) có khoảng 1-2 USD để xác thực tài khoản (Oracle sẽ trừ thử rồi hoàn trả ngay lập tức). Bạn nên chọn **Home Region** ở gần Việt Nam (như Singapore hoặc Tokyo) để có tốc độ kết nối nhanh nhất.

### 2. Tạo máy chủ ảo (VM Instance)
Sau khi đăng nhập vào Oracle Cloud Console:
1. Nhấn **Create a VM instance**.
2. **Placement & Image and Shape:**
   * **Image:** Chọn **Ubuntu 22.04 LTS** hoặc **Ubuntu 24.04 LTS** (Mặc định thường là Oracle Linux, hãy nhấn *Edit* để đổi sang Ubuntu).
   * **Shape:** Nhấn *Edit* > Chọn **Ampere (ARM)** > Đặt cấu hình: **2 hoặc 4 OCPUs** và **12 hoặc 24 GB RAM**.
3. **Networking:**
   * Chọn tạo mới **Virtual Cloud Network (VCN)** và Subnet mặc định.
   * Đảm bảo đã tích chọn **Assign a public IPv4 address** (để lấy IP Public kết nối internet).
4. **SSH Keys:**
   * Nhấn **Save private key** để tải file `.key` (hoặc `.pem`) về máy tính của bạn. File này rất quan trọng để đăng nhập vào VPS sau này.
5. Nhấn **Create** và đợi 2-3 phút để máy chủ khởi tạo. Sau khi hoàn thành, bạn sẽ thấy cột **Public IP Address** (ví dụ: `132.145.12.34`).

---

## 3. Bước 2: Đăng ký Tên Miền Miễn Phí (DuckDNS)

Nếu bạn chưa muốn mua tên miền thương mại (như `.com` hay `.net`), bạn có thể sử dụng dịch vụ tên miền động miễn phí của **DuckDNS** để có link dạng `<ten-cua-ban>.duckdns.org`.

1. Truy cập trang web [DuckDNS](https://www.duckdns.org/).
2. Đăng nhập bằng tài khoản Github hoặc Google.
3. Trong ô **sub domain**, nhập tên bạn muốn đặt (ví dụ: `university-portfolio`) rồi nhấn **add domain**.
4. Tại dòng subdomain vừa tạo, điền địa chỉ **Public IP** của máy chủ Oracle Cloud của bạn vào ô `ip` rồi nhấn **update ip**.
5. Bây giờ, tên miền của bạn sẽ là: `university-portfolio.duckdns.org`.

---

## 4. Bước 3: Mở Cổng Tường Lửa (Firewall) trên Oracle Cloud

Mặc định, Oracle Cloud chặn toàn bộ các cổng kết nối ngoại trừ cổng SSH (22). Bạn cần mở cổng **80** (HTTP) và **443** (HTTPS) để người dùng truy cập được web:

1. Tại trang chi tiết VM Instance trên Oracle Cloud, click vào link của **Virtual Cloud Network (VCN)**.
2. Click vào **Subnets** > Click chọn **Default Security List**.
3. Nhấn **Add Ingress Rules** để thêm 2 luật mở cổng:
   * **Ingress Rule 1 (HTTP):**
     * Source CIDR: `0.0.0.0/0`
     * IP Protocol: `TCP`
     * Destination Port Range: `80`
   * **Ingress Rule 2 (HTTPS):**
     * Source CIDR: `0.0.0.0/0`
     * IP Protocol: `TCP`
     * Destination Port Range: `443`
4. Nhấn **Add Ingress Rules**.

---

## 5. Bước 4: Cài Đặt Trên VPS và Khởi Chạy Docker

Sử dụng phần mềm SSH (như Termius, MobaXterm hoặc Git Bash) kết nối tới VPS của bạn bằng địa chỉ IP Public và file Private Key đã tải ở Bước 1:
* **Username mặc định:** `ubuntu`

### 1. Cập nhật hệ thống và mở khóa Firewall nội bộ của Ubuntu:
Chạy lần lượt các lệnh sau trong VPS Terminal để tắt tường lửa phụ của Oracle cài trên Ubuntu:
```bash
sudo iptables -F
sudo iptables-save | sudo tee /etc/iptables/rules.v4
sudo ufw disable
```

### 2. Cài đặt Docker & Docker Compose:
```bash
sudo apt update && sudo apt upgrade -y
curl -fsSL https://get.docker.com -o get-docker.sh
sudo sh get-docker.sh
sudo usermod -aG docker ubuntu
newgrp docker
```
*Kiểm tra cài đặt:* `docker compose version`

### 3. Tải mã nguồn lên VPS:
Đẩy code của bạn lên GitHub, sau đó clone về VPS:
```bash
git clone <đường-dẫn-github-repository-của-bạn>
cd UniversityManagement
```

### 4. Build và khởi chạy Docker Compose:
```bash
docker compose -f docker-compose.prod.yml up -d --build
```
Kiểm tra xem các container đã hoạt động chưa:
```bash
docker compose ps
```

---

## 6. Bước 5: Cấu Hình Nginx và Cài HTTPS SSL Miễn Phí

Để gán link portfolio đẹp và chuyên nghiệp, chúng ta sẽ cài **Nginx** làm Reverse Proxy ngay trên VPS để nhận yêu cầu từ cổng 80/443 rồi chuyển tiếp vào container `frontend-mvc` (đang chạy ở cổng `5281` bên trong Docker).

### 1. Cài đặt Nginx trên VPS:
```bash
sudo apt install nginx -y
```

### 2. Cấu hình Nginx chuyển tiếp tới Frontend MVC:
Tạo cấu hình file cấu hình mới:
```bash
sudo nano /etc/nginx/sites-available/university-management
```
Copy và dán cấu hình này vào (nhớ đổi tên miền DuckDNS của bạn):
```nginx
server {
    listen 80;
    server_name university-portfolio.duckdns.org; # Đổi thành tên miền của bạn

    location / {
        proxy_pass http://127.0.0.1:5281; # Chuyển tiếp tới cổng Frontend MVC
        proxy_http_version 1.1;
        proxy_set_header Upgrade $http_upgrade;
        proxy_set_header Connection keep-alive;
        proxy_set_header Host $host;
        proxy_cache_bypass $http_upgrade;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto $scheme;
    }
}
```
*Nhấn `Ctrl + O` -> `Enter` để lưu, `Ctrl + X` để thoát nano.*

Kích hoạt cấu hình mới và restart Nginx:
```bash
sudo ln -s /etc/nginx/sites-available/university-management /etc/nginx/sites-enabled/
sudo nginx -t
sudo systemctl restart nginx
```

### 3. Cài đặt chứng chỉ SSL tự động (Certbot - Let's Encrypt):
```bash
sudo apt install certbot python3-certbot-nginx -y
sudo certbot --nginx -d university-portfolio.duckdns.org
```
* Làm theo hướng dẫn trên màn hình: nhập email của bạn, gõ `Y` để đồng ý điều khoản. 
* Khi hoàn thành, Certbot sẽ tự động đăng ký SSL và chuyển hướng toàn bộ lưu lượng HTTP (cổng 80) sang HTTPS (cổng 443).

---

## 7. Bước 6: Kiểm tra và hoàn tất Portfolio

1. Truy cập link của bạn trên trình duyệt: `https://university-portfolio.duckdns.org`
2. Nhìn lên thanh địa chỉ sẽ thấy biểu tượng **Khóa an toàn (HTTPS)** màu xanh lá.
3. Khi viết Portfolio trên GitHub hoặc trang CV (như TopCV, resume):
   * **Project Name:** Hệ Thống Quản Lý Trường Học (Microservices Architecture)
   * **Live URL:** `https://university-portfolio.duckdns.org`
   * **Demo Account:** 
     * *Admin:* `admin@example.com` / Mật khẩu: `Admin@123` (hoặc tài khoản demo có sẵn trong DB của bạn)
     * *Sinh viên:* `student@example.com` / Mật khẩu: `Student@123`
