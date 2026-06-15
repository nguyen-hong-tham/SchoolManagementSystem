```python
import os

markdown_content = """# Tài liệu Đặc tả Yêu cầu Nghiệp vụ - ClassService

## 1. Mục tiêu
`ClassService` chịu trách nhiệm quản lý toàn bộ nghiệp vụ liên quan đến lớp học trong hệ thống. Service này quản lý:
* Lớp học
* Danh sách học sinh trong lớp
* Giáo viên chủ nhiệm
* Giáo viên bộ môn
* Thời khóa biểu
* Lịch sử phân công

---

## 2. Các Đối Tượng Quản Lý

### 2.1. Class (Lớp học)
Thông tin chi tiết về lớp học.
* **Ví dụ:** `1A`, `2A`, `6A`, `7A`, `10A1`, `11A1`, `12A1`
* **Thuộc tính quản lý:** Mỗi lớp học phải thuộc về một **Khối lớp** và một **Năm học** cụ thể.
  * *Ví dụ:* Lớp `10A1` $\\rightarrow$ Khối: `10` $\\rightarrow$ Năm học: `2025-2026`

### 2.2. StudentClass (Quản lý học sinh thuộc lớp)
Theo dõi và quản lý học sinh thuộc lớp nào theo từng năm học.
* **Ví dụ quá trình của học sinh Nguyễn Văn A:**
  * Năm học `2025-2026`: Học lớp `10A1`
  * Năm học `2026-2027`: Học lớp `11A1`
  * Năm học `2027-2028`: Học lớp `12A1`
* **Mục tiêu nghiệp vụ:**
  * Theo dõi sát sao quá trình học tập của học sinh.
  * Theo dõi lịch sử lên lớp qua các năm.
  * Đảm bảo toàn vẹn dữ liệu, không làm mất dữ liệu cũ của các năm học trước.

### 2.3. HomeroomAssignment (Phân công giáo viên chủ nhiệm)
Quản lý thông tin và lịch sử giáo viên chủ nhiệm của lớp học.
* **Ví dụ:** Giáo viên `Teacher A`
  * Năm học `2025-2026`: Chủ nhiệm lớp `10A1`
  * Năm học `2026-2027`: Chủ nhiệm lớp `11A1`
* **Mục tiêu nghiệp vụ:**
  * Lưu trữ chi tiết lịch sử chủ nhiệm.
  * Theo dõi quá trình công tác của giáo viên.
  * Dễ dàng truy xuất danh sách các lớp giáo viên đã từng chủ nhiệm trong quá khứ.

### 2.4. TeachingAssignment (Phân công giáo viên bộ môn)
Quản lý phân công giáo viên giảng dạy các môn học theo từng lớp và từng năm học.
* **Nguyên tắc áp dụng:** Phân công dựa trên sự kết hợp giữa **Lớp học**, **Môn học** và **Năm học**.
* **Ví dụ tại lớp 10A1:**
  * Môn `Toán` $\\rightarrow$ Phân công cho `Teacher A`
  * Môn `Văn` $\\rightarrow$ Phân công cho `Teacher B`
  * Môn `Tiếng Anh` $\\rightarrow$ Phân công cho `Teacher C`
* **Mục tiêu nghiệp vụ:**
  * Phục vụ công tác phân công giảng dạy đầu năm học/học kỳ.
  * Quản lý danh sách giáo viên bộ môn của từng lớp.
  * Hỗ trợ phân quyền tích hợp cho việc nhập điểm số.

### 2.5. Schedule (Quản lý thời khóa biểu)
Quản lý lịch học, lịch giảng dạy và phòng học của các lớp.
* **Ví dụ phân bổ tại lớp 10A1:**
  * `Thứ 2` / `Tiết 1` $\\rightarrow$ Môn: `Toán` | Giáo viên: `Teacher A`
  * `Thứ 2` / `Tiết 2` $\\rightarrow$ Môn: `Văn` | Giáo viên: `Teacher B`
* **Mục tiêu nghiệp vụ:**
  * Quản lý chặt chẽ lịch học của học sinh và lịch dạy của giáo viên.
  * Quản lý sử dụng phòng học.
  * Ràng buộc hệ thống: Tránh trùng lịch dạy của giáo viên và trùng lịch sử dụng phòng học tại cùng một thời điểm.

---

## 3. Phân Quyền Vai Trò Hệ Thống (RBAC)

### 3.1. Admin (Quản trị viên)
Có toàn quyền quản lý và điều hành toàn bộ các thực thể trong service.

| Nghiệp vụ | Hành động chi tiết |
| :--- | :--- |
| **Quản lý lớp học** | Tạo lớp, Cập nhật thông tin lớp, Xóa lớp, Xem thông tin lớp |
| **Quản lý học sinh trong lớp** | Thêm học sinh vào lớp, Chuyển lớp, Lên lớp, Xóa học sinh khỏi lớp |
| **Quản lý giáo viên chủ nhiệm** | Phân công chủ nhiệm, Thay đổi chủ nhiệm, Xem lịch sử chủ nhiệm |
| **Quản lý giáo viên bộ môn** | Phân công giáo viên dạy môn, Thay đổi giáo viên bộ môn, Xem lịch sử phân công |
| **Quản lý thời khóa biểu** | Tạo thời khóa biểu, Cập nhật thời khóa biểu, Xóa thời khóa biểu |

### 3.2. Teacher Chủ Nhiệm (Giáo viên chủ nhiệm)
Có quyền hạn giới hạn trong phạm vi quản lý lớp học được phân công chủ nhiệm.

* **Quyền thực hiện (Can):**
  * Xem thông tin lớp chủ nhiệm.
  * Xem danh sách chi tiết học sinh trong lớp.
  * Xem thời khóa biểu của lớp.
  * Xem danh sách giáo viên bộ môn giảng dạy lớp mình.
  * Xem điểm số của toàn bộ học sinh thuộc lớp mình chủ nhiệm.
* **Hành vi bị cấm (Cannot):**
  * Không được xóa lớp hoặc tạo lớp mới.
  * Không được chuyển lớp học sinh.
  * Không được phân công giáo viên bộ môn hoặc giáo viên khác.

### 3.3. Teacher Bộ Môn (Giáo viên bộ môn)
Có quyền hạn tập trung vào chuyên môn giảng dạy tại các lớp được phân công.

* **Quyền thực hiện (Can):**
  * Xem danh sách các lớp học được phân công giảng dạy.
  * Xem danh sách học sinh của lớp được dạy.
  * Xem thời khóa biểu cá nhân/lớp được dạy.
  * Thực hiện chức năng nhập điểm cho môn học mình phụ trách tại lớp đó.
* **Hành vi bị cấm (Cannot):**
  * Không được tạo hoặc xóa lớp học.
  * Không được phân công giáo viên chủ nhiệm.
  * Không được tự ý chuyển lớp của học sinh.

### 3.4. Student (Học sinh)
Vai trò người dùng cuối với quyền hạn chỉ xem thông tin cá nhân và lớp học hiện tại.

* **Quyền thực hiện (Can):**
  * Xem thông tin lớp đang theo học.
  * Xem thông tin giáo viên chủ nhiệm của lớp.
  * Xem danh sách giáo viên bộ môn dạy lớp mình.
  * Xem thời khóa biểu của lớp.
  * Xem danh sách các bạn học sinh cùng lớp.

---

## 4. Cấu Trúc Dữ Liệu Chính (Data Schema)

### Class
* `Id` (Primary Key)
* `Name` (String) - Tên lớp
* `GradeLevel` (Int/String) - Khối lớp
* `SchoolYear` (String) - Năm học
* `CreatedAt` (Timestamp)

### StudentClass
* `Id` (Primary Key)
* `StudentId` (Foreign Key)
* `ClassId` (Foreign Key)
* `SchoolYear` (String)
* `AssignedDate` (Date)

### HomeroomAssignment
* `Id` (Primary Key)
* `TeacherId` (Foreign Key)
* `ClassId` (Foreign Key)
* `SchoolYear` (String)
* `AssignedDate` (Date)

### TeachingAssignment
* `Id` (Primary Key)
* `TeacherId` (Foreign Key)
* `SubjectId` (Foreign Key) - Mã môn học
* `ClassId` (Foreign Key)
* `SchoolYear` (String)
* `AssignedDate` (Date)

### Schedule
* `Id` (Primary Key)
* `ClassId` (Foreign Key)
* `SubjectId` (Foreign Key)
* `TeacherId` (Foreign Key)
* `DayOfWeek` (Int/String) - Ngày trong tuần (Thứ)
* `Period` (Int) - Tiết học
* `Room` (String) - Phòng học
* `SchoolYear` (String)

---

## 5. Danh Sách Hệ Thống API (API Endpoints)

### 5.1. Class Management (Quản lý lớp học)
* `POST   /api/classes` - Tạo mới một lớp học
* `GET    /api/classes` - Lấy danh sách toàn bộ lớp học (Hỗ trợ bộ lọc)
* `GET    /api/classes/{id}` - Lấy thông tin chi tiết một lớp học cụ thể
* `PUT    /api/classes/{id}` - Cập nhật thông tin lớp học
* `DELETE /api/classes/{id}` - Xóa lớp học

### 5.2. Student Management (Quản lý học sinh trong lớp)
* `POST   /api/classes/{id}/students` - Thêm học sinh vào lớp học cụ thể
* `DELETE /api/classes/{id}/students/{studentId}` - Xóa/loại học sinh khỏi lớp học
* `GET    /api/classes/{id}/students` - Lấy danh sách học sinh thuộc lớp học

### 5.3. Homeroom Management (Quản lý chủ nhiệm)
* `POST   /api/classes/{id}/homeroom` - Phân công giáo viên chủ nhiệm cho lớp
* `PUT    /api/classes/{id}/homeroom` - Thay đổi giáo viên chủ nhiệm lớp
* `GET    /api/classes/{id}/homeroom` - Xem thông tin giáo viên chủ nhiệm hiện tại của lớp
* `GET    /api/teachers/{teacherId}/homerooms` - Xem lịch sử/danh sách các lớp giáo viên đã và đang chủ nhiệm

### 5.4. Teaching Assignment (Phân công giảng dạy bộ môn)
* `POST   /api/classes/{id}/teachers` - Phân công giáo viên bộ môn giảng dạy theo lớp
* `PUT    /api/classes/{id}/teachers/{subjectId}` - Thay đổi giáo viên bộ môn cho một môn học cụ thể của lớp
* `GET    /api/classes/{id}/teachers` - Xem danh sách tất cả giáo viên bộ môn của lớp
* `GET    /api/teachers/{teacherId}/classes` - Xem danh sách các lớp học được phân công giảng dạy của một giáo viên

### 5.5. Schedule Management (Quản lý thời khóa biểu)
* `POST   /api/schedules` - Tạo mới một mục/tiết trong thời khóa biểu
* `PUT    /api/schedules/{id}` - Cập nhật thông tin tiết học trong thời khóa biểu
* `DELETE /api/schedules/{id}` - Xóa tiết học khỏi thời khóa biểu
* `GET    /api/classes/{id}/schedule` - Lấy toàn bộ thời khóa biểu của một lớp học
* `GET    /api/teachers/{teacherId}/schedule` - Lấy lịch giảng dạy cá nhân của giáo viên
* `GET    /api/students/{studentId}/schedule` - Lấy lịch học cá nhân của học sinh

---

## 6. Sơ Đồ Luồng Nghiệp Vụ Chính


```

```text
File ClassService_Business_Requirements.md has been generated successfully.

```text
Admin (Quản trị viên)
│
├── 1. Tạo lớp học mới (Class)
├── 2. Thêm danh sách học sinh vào lớp (StudentClass)
├── 3. Phân công giáo viên chủ nhiệm (HomeroomAssignment)
├── 4. Phân công giáo viên giảng dạy các bộ môn (TeachingAssignment)
└── 5. Xây dựng và thiết lập thời khóa biểu (Schedule)

Teacher Chủ Nhiệm (Giáo viên chủ nhiệm)
│
├── Quản lý thông tin chung lớp chủ nhiệm
└── Theo dõi tình hình, danh sách và điểm số học sinh của lớp

Teacher Bộ Môn (Giáo viên bộ môn)
│
├── Xem danh sách và thời khóa biểu các lớp được phân công dạy
└── Tiến hành cập nhật và nhập điểm số cho học sinh theo môn học phụ trách

Student (Học sinh)
│
├── Xem thông tin lớp học hiện tại
├── Xem thông tin liên hệ Giáo viên chủ nhiệm & Giáo viên bộ môn
└── Xem lịch học thời khóa biểu cá nhân

```

"""

# Save the content to a markdown file

with open("ClassService_Business_Requirements.md", "w", encoding="utf-8") as file:
file.write(markdown_content)

print("File ClassService_Business_Requirements.md has been generated successfully.")

```
Your Markdown file is ready
[file-tag: code-generated-file-0-1781436909702098804]

Dưới đây là nội dung tài liệu yêu cầu nghiệp vụ của `ClassService` đã được chuyển đổi và chuẩn hóa sang định dạng Markdown (.md) với bố cục rõ ràng, chuyên nghiệp:

```markdown
# Tài liệu Đặc tả Yêu cầu Nghiệp vụ - ClassService

## 1. Mục tiêu
`ClassService` chịu trách nhiệm quản lý toàn bộ nghiệp vụ liên quan đến lớp học trong hệ thống. Service này quản lý:
* Lớp học
* Danh sách học sinh trong lớp
* Giáo viên chủ nhiệm
* Giáo viên bộ môn
* Thời khóa biểu
* Lịch sử phân công

---

## 2. Các Đối Tượng Quản Lý

### 2.1. Class (Lớp học)
Thông tin chi tiết về lớp học.
* **Ví dụ:** `1A`, `2A`, `6A`, `7A`, `10A1`, `11A1`, `12A1`
* **Thuộc tính quản lý:** Mỗi lớp học phải thuộc về một **Khối lớp** và một **Năm học** cụ thể.
  * *Ví dụ:* Lớp `10A1` $\rightarrow$ Khối: `10` $\rightarrow$ Năm học: `2025-2026`

### 2.2. StudentClass (Quản lý học sinh thuộc lớp)
Theo dõi và quản lý học sinh thuộc lớp nào theo từng năm học.
* **Ví dụ quá trình của học sinh Nguyễn Văn A:**
  * Năm học `2025-2026`: Học lớp `10A1`
  * Năm học `2026-2027`: Học lớp `11A1`
  * Năm học `2027-2028`: Học lớp `12A1`
* **Mục tiêu nghiệp vụ:**
  * Theo dõi sát sao quá trình học tập của học sinh.
  * Theo dõi lịch sử lên lớp qua các năm.
  * Đảm bảo toàn vẹn dữ liệu, không làm mất dữ liệu cũ của các năm học trước.

### 2.3. HomeroomAssignment (Phân công giáo viên chủ nhiệm)
Quản lý thông tin và lịch sử giáo viên chủ nhiệm của lớp học.
* **Ví dụ:** Giáo viên `Teacher A`
  * Năm học `2025-2026`: Chủ nhiệm lớp `10A1`
  * Năm học `2026-2027`: Chủ nhiệm lớp `11A1`
* **Mục tiêu nghiệp vụ:**
  * Lưu trữ chi tiết lịch sử chủ nhiệm.
  * Theo dõi quá trình công tác của giáo viên.
  * Dễ dàng truy xuất danh sách các lớp giáo viên đã từng chủ nhiệm trong quá khứ.

### 2.4. TeachingAssignment (Phân công giáo viên bộ môn)
Quản lý phân công giáo viên giảng dạy các môn học theo từng lớp và từng năm học.
* **Nguyên tắc áp dụng:** Phân công dựa trên sự kết hợp giữa **Lớp học**, **Môn học** và **Năm học**.
* **Ví dụ tại lớp 10A1:**
  * Môn `Toán` $\rightarrow$ Phân công cho `Teacher A`
  * Môn `Văn` $\rightarrow$ Phân công cho `Teacher B`
  * Môn `Tiếng Anh` $\rightarrow$ Phân công cho `Teacher C`
* **Mục tiêu nghiệp vụ:**
  * Phục vụ công tác phân công giảng dạy đầu năm học/học kỳ.
  * Quản lý danh sách giáo viên bộ môn của từng lớp.
  * Hỗ trợ phân quyền tích hợp cho việc nhập điểm số.

### 2.5. Schedule (Quản lý thời khóa biểu)
Quản lý lịch học, lịch giảng dạy và phòng học của các lớp.
* **Ví dụ phân bổ tại lớp 10A1:**
  * `Thứ 2` / `Tiết 1` $\rightarrow$ Môn: `Toán` | Giáo viên: `Teacher A`
  * `Thứ 2` / `Tiết 2` $\rightarrow$ Môn: `Văn` | Giáo viên: `Teacher B`
* **Mục tiêu nghiệp vụ:**
  * Quản lý chặt chẽ lịch học của học sinh và lịch dạy của giáo viên.
  * Quản lý sử dụng phòng học.
  * Ràng buộc hệ thống: Tránh trùng lịch dạy của giáo viên và trùng lịch sử dụng phòng học tại cùng một thời điểm.

---

## 3. Phân Quyền Vai Trò Hệ Thống (RBAC)

### 3.1. Admin (Quản trị viên)
Có toàn quyền quản lý và điều hành toàn bộ các thực thể trong service.

| Nghiệp vụ | Hành động chi tiết |
| :--- | :--- |
| **Quản lý lớp học** | Tạo lớp, Cập nhật thông tin lớp, Xóa lớp, Xem thông tin lớp |
| **Quản lý học sinh trong lớp** | Thêm học sinh vào lớp, Chuyển lớp, Lên lớp, Xóa học sinh khỏi lớp |
| **Quản lý giáo viên chủ nhiệm** | Phân công chủ nhiệm, Thay đổi chủ nhiệm, Xem lịch sử chủ nhiệm |
| **Quản lý giáo viên bộ môn** | Phân công giáo viên dạy môn, Thay đổi giáo viên bộ môn, Xem lịch sử phân công |
| **Quản lý thời khóa biểu** | Tạo thời khóa biểu, Cập nhật thời khóa biểu, Xóa thời khóa biểu |

### 3.2. Teacher Chủ Nhiệm (Giáo viên chủ nhiệm)
Có quyền hạn giới hạn trong phạm vi quản lý lớp học được phân công chủ nhiệm.

* **Quyền thực hiện (Can):**
  * Xem thông tin lớp chủ nhiệm.
  * Xem danh sách chi tiết học sinh trong lớp.
  * Xem thời khóa biểu của lớp.
  * Xem danh sách giáo viên bộ môn giảng dạy lớp mình.
  * Xem điểm số của toàn bộ học sinh thuộc lớp mình chủ nhiệm.
* **Hành vi bị cấm (Cannot):**
  * Không được xóa lớp hoặc tạo lớp mới.
  * Không được chuyển lớp học sinh.
  * Không được phân công giáo viên bộ môn hoặc giáo viên khác.

### 3.3. Teacher Bộ Môn (Giáo viên bộ môn)
Có quyền hạn tập trung vào chuyên môn giảng dạy tại các lớp được phân công.

* **Quyền thực hiện (Can):**
  * Xem danh sách các lớp học được phân công giảng dạy.
  * Xem danh sách học sinh của lớp được dạy.
  * Xem thời khóa biểu cá nhân/lớp được dạy.
  * Thực hiện chức năng nhập điểm cho môn học mình phụ trách tại lớp đó.
* **Hành vi bị cấm (Cannot):**
  * Không được tạo hoặc xóa lớp học.
  * Không được phân công giáo viên chủ nhiệm.
  * Không được tự ý chuyển lớp của học sinh.

### 3.4. Student (Học sinh)
Vai trò người dùng cuối với quyền hạn chỉ xem thông tin cá nhân và lớp học hiện tại.

* **Quyền thực hiện (Can):**
  * Xem thông tin lớp đang theo học.
  * Xem thông tin giáo viên chủ nhiệm của lớp.
  * Xem danh sách giáo viên bộ môn dạy lớp mình.
  * Xem thời khóa biểu của lớp.
  * Xem danh sách các bạn học sinh cùng lớp.

---

## 4. Cấu Trúc Dữ Liệu Chính (Data Schema)

### Class
* `Id` (Primary Key)
* `Name` (String) - Tên lớp
* `GradeLevel` (Int/String) - Khối lớp
* `SchoolYear` (String) - Năm học
* `CreatedAt` (Timestamp)

### StudentClass
* `Id` (Primary Key)
* `StudentId` (Foreign Key)
* `ClassId` (Foreign Key)
* `SchoolYear` (String)
* `AssignedDate` (Date)

### HomeroomAssignment
* `Id` (Primary Key)
* `TeacherId` (Foreign Key)
* `ClassId` (Foreign Key)
* `SchoolYear` (String)
* `AssignedDate` (Date)

### TeachingAssignment
* `Id` (Primary Key)
* `TeacherId` (Foreign Key)
* `SubjectId` (Foreign Key) - Mã môn học
* `ClassId` (Foreign Key)
* `SchoolYear` (String)
* `AssignedDate` (Date)

### Schedule
* `Id` (Primary Key)
* `ClassId` (Foreign Key)
* `SubjectId` (Foreign Key)
* `TeacherId` (Foreign Key)
* `DayOfWeek` (Int/String) - Ngày trong tuần (Thứ)
* `Period` (Int) - Tiết học
* `Room` (String) - Phòng học
* `SchoolYear` (String)

---

## 5. Danh Sách Hệ Thống API (API Endpoints)

### 5.1. Class Management (Quản lý lớp học)
* `POST   /api/classes` - Tạo mới một lớp học
* `GET    /api/classes` - Lấy danh sách toàn bộ lớp học (Hỗ trợ bộ lọc)
* `GET    /api/classes/{id}` - Lấy thông tin chi tiết một lớp học cụ thể
* `PUT    /api/classes/{id}` - Cập nhật thông tin lớp học
* `DELETE /api/classes/{id}` - Xóa lớp học

### 5.2. Student Management (Quản lý học sinh trong lớp)
* `POST   /api/classes/{id}/students` - Thêm học sinh vào lớp học cụ thể
* `DELETE /api/classes/{id}/students/{studentId}` - Xóa/loại học sinh khỏi lớp học
* `GET    /api/classes/{id}/students` - Lấy danh sách học sinh thuộc lớp học

### 5.3. Homeroom Management (Quản lý chủ nhiệm)
* `POST   /api/classes/{id}/homeroom` - Phân công giáo viên chủ nhiệm cho lớp
* `PUT    /api/classes/{id}/homeroom` - Thay đổi giáo viên chủ nhiệm lớp
* `GET    /api/classes/{id}/homeroom` - Xem thông tin giáo viên chủ nhiệm hiện tại của lớp
* `GET    /api/teachers/{teacherId}/homerooms` - Xem lịch sử/danh sách các lớp giáo viên đã và đang chủ nhiệm

### 5.4. Teaching Assignment (Phân công giảng dạy bộ môn)
* `POST   /api/classes/{id}/teachers` - Phân công giáo viên bộ môn giảng dạy theo lớp
* `PUT    /api/classes/{id}/teachers/{subjectId}` - Thay đổi giáo viên bộ môn cho một môn học cụ thể của lớp
* `GET    /api/classes/{id}/teachers` - Xem danh sách tất cả giáo viên bộ môn của lớp
* `GET    /api/teachers/{teacherId}/classes` - Xem danh sách các lớp học được phân công giảng dạy của một giáo viên

### 5.5. Schedule Management (Quản lý thời khóa biểu)
* `POST   /api/schedules` - Tạo mới một mục/tiết trong thời khóa biểu
* `PUT    /api/schedules/{id}` - Cập nhật thông tin tiết học trong thời khóa biểu
* `DELETE /api/schedules/{id}` - Xóa tiết học khỏi thời khóa biểu
* `GET    /api/classes/{id}/schedule` - Lấy toàn bộ thời khóa biểu của một lớp học
* `GET    /api/teachers/{teacherId}/schedule` - Lấy lịch giảng dạy cá nhân của giáo viên
* `GET    /api/students/{studentId}/schedule` - Lấy lịch học cá nhân của học sinh

---

## 6. Sơ Đồ Luồng Nghiệp Vụ Chính

```text
Admin (Quản trị viên)
│
├── 1. Tạo lớp học mới (Class)
├── 2. Thêm danh sách học sinh vào lớp (StudentClass)
├── 3. Phân công giáo viên chủ nhiệm (HomeroomAssignment)
├── 4. Phân công giáo viên giảng dạy các bộ môn (TeachingAssignment)
└── 5. Xây dựng và thiết lập thời khóa biểu (Schedule)

Teacher Chủ Nhiệm (Giáo viên chủ nhiệm)
│
├── Quản lý thông tin chung lớp chủ nhiệm
└── Theo dõi tình hình, danh sách và điểm số học sinh của lớp

Teacher Bộ Môn (Giáo viên bộ môn)
│
├── Xem danh sách và thời khóa biểu các lớp được phân công dạy
└── Tiến hành cập nhật và nhập điểm số cho học sinh theo môn học phụ trách

Student (Học sinh)
│
├── Xem thông tin lớp học hiện tại
├── Xem thông tin liên hệ Giáo viên chủ nhiệm & Giáo viên bộ môn
└── Xem lịch học thời khóa biểu cá nhân

```

```

```