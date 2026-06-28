# Hướng Dẫn Quy Trình Phát Triển Chức Năng (5 Tầng Chuẩn Clean Architecture)

Tài liệu này hướng dẫn chi tiết từng bước, cấu trúc code mẫu, giải thích cặn kẽ ý nghĩa và luồng vận hành của **Quy trình 5 tầng** khi xây dựng một tính năng mới trong dịch vụ ASP.NET Core Web API.

---

## 1. Tổng Quan Về Kiến Trúc Phân Tầng (Clean Architecture)

Mô hình phân tầng giúp cô lập các trách nhiệm (Separation of Concerns). Khi có sự thay đổi ở một phần (ví dụ: đổi cấu trúc database), các phần khác (ví dụ: logic hiển thị giao diện API) không bị ảnh hưởng trực tiếp.

```text
       [ Trình Duyệt / Postman ]
                   │
                   ▼ (HTTP Request: DTO)
     +---------------------------+
     |   1. TẦNG CONTROLLERS     |  <--- Tiếp nhận Request, xác thực JWT, định tuyến API
     +---------------------------+
                   │
                   ▼ (DTO)
     +---------------------------+
     |     2. TẦNG SERVICES      |  <--- Xử lý nghiệp vụ chính, validate logic, ném Exception
     +---------------------------+
                   │
                   ▼ (Entities)
     +---------------------------+
     |   3. TẦNG REPOSITORIES    |  <--- Truy vấn LINQ / Entity Framework Core
     +---------------------------+
                   │
                   ▼ (SQL Query)
     +---------------------------+
     |       4. DATABASE         |  <--- PostgreSQL / SQL Server
     +---------------------------+
```

---

## 2. Chi Tiết Từng Bước Trong Quy Trình Phát Triển

Dưới đây là phân tích sâu kèm code mẫu thực tế của 5 bước phát triển khi bạn viết một tính năng (Ví dụ tính năng: **Nhập điểm thi cho học sinh**):

---

### Bước 1: Entities (Định nghĩa cấu trúc dữ liệu vật lý)

* **Nhiệm vụ:** Mô tả chính xác cấu trúc bảng trong Database. Đây là nơi ánh xạ trực tiếp các kiểu dữ liệu của C# sang CSDL (như Guid, string, int, decimal, DateTime).
* **Vị trí thư mục:** `Entities/`
* **Quy tắc vàng:**
  * Giữ Entity "sạch", chỉ chứa thuộc tính dữ liệu và quan hệ (Navigation Properties), không chứa logic nghiệp vụ hay logic bắt lỗi (validation).
  * Việc cấu hình chi tiết cột (như độ dài tối đa, tên bảng, chỉ mục unique) nên viết bằng **Fluent API** ở file `DbContext` để tránh làm ô nhiễm file Entity.

#### Code mẫu `Entities/Score.cs`:
```csharp
using System;

namespace ScoreService.Entities;

public class Score
{
    public Guid Id { get; set; } // Khóa chính
    public Guid StudentId { get; set; } // Liên kết đến học sinh
    public Guid SubjectId { get; set; } // Liên kết đến môn học
    
    public decimal ScoreValue { get; set; } // Điểm số từ 0.0 đến 10.0
    public int Semester { get; set; } // Kỳ 1 hoặc Kỳ 2
    public string SchoolYear { get; set; } = string.Empty; // Ví dụ: "2025-2026"
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
```

---

### Bước 2: DTOs (Data Transfer Objects - Kiểm soát dữ liệu đầu vào/đầu ra)

* **Nhiệm vụ:** Tách biệt cấu trúc bảng cơ sở dữ liệu và dữ liệu trao đổi với Client.
  * **Request DTO:** Chỉ chứa những trường mà Client **phép** truyền lên và thực hiện **Model Validation** (bắt lỗi nhập liệu).
  * **Response DTO:** Chỉ chứa những trường mà Client **phép** xem (ví dụ ẩn trường mật khẩu, Hash, dữ liệu nhạy cảm).
* **Vị trí thư mục:** `DTOs/`
* **Giải thích Model Validation:** Các thuộc tính như `[Required]`, `[Range]` được bộ lọc của ASP.NET Core kiểm tra tự động trước khi đi vào Controller. Nếu Client truyền dữ liệu sai định dạng (ví dụ nhập điểm 12.0), API lập tức trả về lỗi `400 Bad Request` kèm chi tiết lỗi tự động.

#### Code mẫu `DTOs/CreateScoreRequest.cs`:
```csharp
using System;
using System.ComponentModel.DataAnnotations;

namespace ScoreService.DTOs;

public class CreateScoreRequest
{
    [Required(ErrorMessage = "Mã học sinh là bắt buộc")]
    public Guid StudentId { get; set; }

    [Required(ErrorMessage = "Mã môn học là bắt buộc")]
    public Guid SubjectId { get; set; }

    [Range(0.0, 10.0, ErrorMessage = "Điểm số phải nằm trong khoảng từ 0.0 đến 10.0")]
    public decimal ScoreValue { get; set; }

    [Range(1, 2, ErrorMessage = "Học kỳ chỉ nhận giá trị 1 hoặc 2")]
    public int Semester { get; set; }

    [Required(ErrorMessage = "Năm học là bắt buộc")]
    [RegularExpression(@"^\d{4}-\d{4}$", ErrorMessage = "Năm học phải có định dạng YYYY-YYYY (ví dụ: 2025-2026)")]
    public string SchoolYear { get; set; } = string.Empty;
}
```

---

### Bước 3: Repositories (Data Access Layer - Giao tiếp Database)

* **Nhiệm vụ:** Nơi duy nhất chứa các câu lệnh LINQ và EF Core để thao tác trực tiếp với Database. Tách thành Interface và Class.
* **Vị trí thư mục:** `Repositories/`
* **Tại sao cần có Interface?** Giúp tầng Service không phụ thuộc cứng vào Entity Framework. Sau này nếu bạn chuyển từ EF Core sang Dapper hay MongoDB, bạn chỉ cần viết Class triển khai mới mà không phải sửa một dòng code nào trong tầng Service.

#### 3.1 Định nghĩa Interface (`Repositories/IScoreRepository.cs`):
```csharp
using System;
using System.Threading.Tasks;
using ScoreService.Entities;

namespace ScoreService.Repositories;

public interface IScoreRepository
{
    Task<Score?> GetByIdAsync(Guid id);
    Task<bool> CheckDuplicateScoreAsync(Guid studentId, Guid subjectId, int semester, string schoolYear);
    Task AddAsync(Score score); // Chỉ thêm vào tracking bộ nhớ, trả về Task
    Task SaveChangesAsync(); // Thực sự lưu xuống Database
}
```

#### 3.2 Triển khai Class (`Repositories/ScoreRepository.cs`):
```csharp
using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ScoreService.Data;
using ScoreService.Entities;

namespace ScoreService.Repositories;

public class ScoreRepository : IScoreRepository
{
    private readonly ScoreDbContext _db;

    // Tiêm DbContext qua Constructor
    public ScoreRepository(ScoreDbContext db)
    {
        _db = db;
    }

    public async Task<Score?> GetByIdAsync(Guid id)
    {
        return await _db.Scores.FindAsync(id);
    }

    public async Task<bool> CheckDuplicateScoreAsync(Guid studentId, Guid subjectId, int semester, string schoolYear)
    {
        return await _db.Scores.AnyAsync(s => 
            s.StudentId == studentId && 
            s.SubjectId == subjectId && 
            s.Semester == semester && 
            s.SchoolYear == schoolYear);
    }

    public async Task AddAsync(Score score)
    {
        await _db.Scores.AddAsync(score); // Thao tác bộ nhớ
    }

    public async Task SaveChangesAsync()
    {
        await _db.SaveChangesAsync(); // Thao tác mạng/ghi ổ đĩa
    }
}
```

---

### Bước 4: Services (Business Logic Layer - Tầng xử lý nghiệp vụ)

* **Nhiệm vụ:** Đây là "bộ não" của hệ thống, chịu trách nhiệm kiểm tra tất cả các quy tắc logic nghiệp vụ.
* **Vị trí thư mục:** `Services/` (hoặc viết trực tiếp tích hợp trong kiến trúc nếu dự án nhỏ).
* **Hành động tại tầng này:**
  1. Kiểm tra logic nghiệp vụ nâng cao (không kiểm tra bằng Annotation được).
  2. Bắt các điều kiện lỗi và ném ra các ngoại lệ tùy chỉnh (Custom Exceptions) như `KeyNotFoundException` hay `InvalidOperationException`.
  3. Phối hợp giữa nhiều Repositories khác nhau nếu cần (Ví dụ: vừa kiểm tra bảng học sinh, vừa kiểm tra bảng môn học).
  4. Thực hiện ánh xạ dữ liệu (Mapping) từ DTO sang Entity.
  5. Gọi gửi tin nhắn (Publish Event) sang RabbitMQ.

#### Code mẫu triển khai Service (`Services/ScoreService.cs`):
```csharp
using System;
using System.Threading.Tasks;
using ScoreService.DTOs;
using ScoreService.Entities;
using ScoreService.Repositories;

namespace ScoreService.Services;

public interface IScoreService
{
    Task<ScoreResponse> EnterScoreAsync(CreateScoreRequest request, Guid teacherId);
}

public class ScoreService : IScoreService
{
    private readonly IScoreRepository _scoreRepository;

    public ScoreService(IScoreRepository scoreRepository)
    {
        _scoreRepository = scoreRepository;
    }

    public async Task<ScoreResponse> EnterScoreAsync(CreateScoreRequest request, Guid teacherId)
    {
        // 1. Kiểm tra nghiệp vụ: Học sinh đã có điểm môn này trong kỳ này chưa?
        var isDuplicate = await _scoreRepository.CheckDuplicateScoreAsync(
            request.StudentId, request.SubjectId, request.Semester, request.SchoolYear);
            
        if (isDuplicate)
        {
            throw new InvalidOperationException("Học sinh này đã được nhập điểm cho môn học này trong học kỳ được chọn.");
        }

        // 2. Mapping DTO -> Entity
        var score = new Score
        {
            Id = Guid.NewGuid(),
            StudentId = request.StudentId,
            SubjectId = request.SubjectId,
            ScoreValue = request.ScoreValue,
            Semester = request.Semester,
            SchoolYear = request.SchoolYear,
            CreatedAt = DateTime.UtcNow
        };

        // 3. Lưu xuống database qua Repository
        await _scoreRepository.AddAsync(score);
        await _scoreRepository.SaveChangesAsync();

        // 4. Trả về Response DTO sạch sẽ
        return new ScoreResponse
        {
            Id = score.Id,
            StudentId = score.StudentId,
            SubjectId = score.SubjectId,
            ScoreValue = score.ScoreValue,
            Semester = score.Semester,
            SchoolYear = score.SchoolYear,
            CreatedAt = score.CreatedAt
        };
    }
}
```

---

### Bước 5: Controllers (Presentation Layer - Tầng điều hướng API)

* **Nhiệm vụ:** Tiếp nhận HTTP Request từ bên ngoài, trích xuất dữ liệu, kiểm tra phân quyền (JWT Roles) và trả về mã trạng thái HTTP chuẩn xác.
* **Vị trí thư mục:** `Controllers/`
* **Quy tắc tối mật:** Controller phải cực kỳ mỏng (Skinny Controller). Không được phép viết truy vấn Database (`_context.Scores.Add(...)`) hay viết code xử lý logic điều kiện tại đây. Mọi việc xử lý phải giao cho tầng `Service` làm.

#### Code mẫu `Controllers/ScoreController.cs`:
```csharp
using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ScoreService.DTOs;
using ScoreService.Services;

namespace ScoreService.Controllers;

[ApiController]
[Route("api/scores")]
[Authorize] // Bắt buộc đăng nhập
public class ScoreController : ControllerBase
{
    private readonly IScoreService _scoreService;

    public ScoreController(IScoreService scoreService)
    {
        _scoreService = scoreService;
    }

    [HttpPost]
    [Authorize(Roles = "Teacher,Admin")] // Chỉ giáo viên hoặc quản trị viên được phép nhập điểm
    public async Task<IActionResult> Create([FromBody] CreateScoreRequest request)
    {
        // 1. Lấy mã định danh giáo viên từ Token JWT đã giải mã
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userIdClaim == null) return Unauthorized();
        
        var teacherId = Guid.Parse(userIdClaim);

        try
        {
            // 2. Gọi tầng Service xử lý logic
            var result = await _scoreService.EnterScoreAsync(request, teacherId);
            
            // 3. Trả về mã HTTP 201 Created chuẩn RESTful API
            return CreatedAtAction(nameof(Create), new { id = result.Id }, result);
        }
        catch (InvalidOperationException ex)
        {
            // Trả về lỗi nghiệp vụ 400 Bad Request
            return BadRequest(new { message = ex.Message });
        }
    }
}
```

---

## 3. Quản Lý Vòng Đời Đăng Ký Dịch Vụ (Dependency Injection Lifetimes)

Trong file `Program.cs`, khi bạn đăng ký Interface và Class, bạn cần hiểu rõ 3 kiểu đăng ký sau để tránh rò rỉ bộ nhớ hoặc lỗi đồng bộ dữ liệu:

| Phương thức | Vòng đời hoạt động | Trường hợp áp dụng thích hợp |
| :--- | :--- | :--- |
| **`AddTransient`** | Tạo một đối tượng mới **mỗi khi có yêu cầu** giải quyết dịch vụ. | Các dịch vụ xử lý tiện ích ngắn hạn độc lập (như Class Helper, Mapper, Hashing). |
| **`AddScoped`** | Tạo một đối tượng duy nhất **cho mỗi HTTP Request**. | **Bắt buộc cho Repositories, Services và DbContext**. Nhờ vậy, tất cả thao tác trong cùng 1 request đều dùng chung 1 kết nối database duy nhất. |
| **`AddSingleton`** | Tạo một đối tượng duy nhất **trong suốt vòng đời của ứng dụng**. | Các tác vụ dùng chung vĩnh viễn (như cấu hình cấu trúc hàng đợi RabbitMQ, Cache trong bộ nhớ, Logger). |

### Ví dụ đăng ký trong `Program.cs`:
```csharp
builder.Services.AddScoped<IScoreRepository, ScoreRepository>();
builder.Services.AddScoped<IScoreService, ScoreService.Services.ScoreService>();
```
