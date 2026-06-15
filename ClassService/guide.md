# Hướng Dẫn Chi Tiết & Khung Code (Skeleton) Phát Triển ClassService

Tài liệu này hướng dẫn chi tiết cách viết code cho từng file của các nghiệp vụ tiếp theo trong `ClassService` theo kiến trúc Clean Architecture / Repository Pattern.

---

## BƯỚC 1: Xây dựng API Controller cho Quản lý Học sinh (`StudentClass`)

### Cần tạo file: [StudentClassesController.cs](file:///d:/ASP.NET/UniversityManagement/ClassService/Controllers/StudentClassesController.cs)

**Khung Code mẫu hoàn chỉnh:**
```csharp
using Microsoft.AspNetCore.Mvc;
using ClassService.Services.Interfaces;
using ClassService.DTOs.StudentClasses;

namespace ClassService.Controllers;

[ApiController]
[Route("api/student-classes")] // Chỉnh route chung cho thống nhất với thực thể StudentClass
public class StudentClassesController : ControllerBase
{
    private readonly IStudentClassService _studentClassService;

    public StudentClassesController(IStudentClassService studentClassService)
    {
        _studentClassService = studentClassService;
    }

    // 1. POST /api/student-classes/classes/{classId}/students
    // Thêm học sinh vào lớp học
    [HttpPost("classes/{classId:guid}/students")]
    public async Task<ActionResult<StudentClassResponseDto>> AssignStudent(
        Guid classId, 
        [FromBody] AssignStudentDto dto)
    {
        try
        {
            var result = await _studentClassService.AssignStudentAsync(classId, dto);
            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    // 2. GET /api/student-classes/classes/{classId}/students
    // Lấy danh sách học sinh đang học trong lớp
    [HttpGet("classes/{classId:guid}/students")]
    public async Task<ActionResult<IEnumerable<StudentClassResponseDto>>> GetStudents(Guid classId)
    {
        try
        {
            var result = await _studentClassService.GetStudentsByClassIdAsync(classId);
            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    // 3. DELETE /api/student-classes/classes/{classId}/students/{studentId}
    // Xóa học sinh khỏi lớp học
    [HttpDelete("classes/{classId:guid}/students/{studentId:guid}")]
    public async Task<IActionResult> RemoveStudent(Guid classId, Guid studentId)
    {
        var success = await _studentClassService.RemoveStudentAsync(classId, studentId);
        if (!success)
        {
            return BadRequest(new { message = "Học sinh không thuộc lớp này hoặc không phải lớp học hiện tại." });
        }
        return NoContent();
    }

    // 4. POST /api/student-classes/students/{studentId}/transfer
    // Chuyển lớp học sinh
    [HttpPost("students/{studentId:guid}/transfer")]
    public async Task<ActionResult<StudentClassResponseDto>> TransferStudent(
        Guid studentId, 
        [FromBody] TransferStudentDto dto)
    {
        try
        {
            var result = await _studentClassService.TransferStudentAsync(studentId, dto);
            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    // 5. POST /api/student-classes/students/{studentId}/promote
    // Lên lớp học sinh (qua năm học mới)
    [HttpPost("students/{studentId:guid}/promote")]
    public async Task<ActionResult<StudentClassResponseDto>> PromoteStudent(
        Guid studentId, 
        [FromBody] PromoteStudentDto dto)
    {
        try
        {
            var result = await _studentClassService.PromoteStudentAsync(studentId, dto);
            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}
```

---

## BƯỚC 2: Nghiệp vụ Giáo viên Chủ nhiệm (`HomeroomAssignment`)

### 1. DTOs
Tạo thư mục `DTOs/HomeroomAssignments` và tạo các file sau:

#### File: [AssignHomeroomDto.cs](file:///d:/ASP.NET/UniversityManagement/ClassService/DTOs/HomeroomAssignments/AssignHomeroomDto.cs)
```csharp
namespace ClassService.DTOs.HomeroomAssignments;

public class AssignHomeroomDto
{
    public Guid TeacherId { get; set; }
    public string SchoolYear { get; set; } = string.Empty;
}
```

#### File: [HomeroomAssignmentResponseDto.cs](file:///d:/ASP.NET/UniversityManagement/ClassService/DTOs/HomeroomAssignments/HomeroomAssignmentResponseDto.cs)
```csharp
namespace ClassService.DTOs.HomeroomAssignments;

public class HomeroomAssignmentResponseDto
{
    public Guid Id { get; set; }
    public Guid TeacherId { get; set; }
    public Guid ClassId { get; set; }
    public string SchoolYear { get; set; } = string.Empty;
    public DateTime AssignedDate { get; set; }
}
```

---

### 2. Repository Layer

#### File: [IHomeroomAssignmentRepository.cs](file:///d:/ASP.NET/UniversityManagement/ClassService/Repositories/Interfaces/IHomeroomAssignmentRepository.cs)
```csharp
using ClassService.Entities;

namespace ClassService.Repositories.Interfaces;

public interface IHomeroomAssignmentRepository
{
    // Tìm giáo viên chủ nhiệm hiện tại của lớp theo năm học
    Task<HomeroomAssignment?> GetActiveAssignmentByClassAsync(Guid classId, string schoolYear);

    // Xem lịch sử phân công chủ nhiệm của một giáo viên cụ thể
    Task<List<HomeroomAssignment>> GetHistoryByTeacherAsync(Guid teacherId);

    Task AddAsync(HomeroomAssignment entity);
    void Update(HomeroomAssignment entity);
    Task SaveChangesAsync();
}
```

#### File: [HomeroomAssignmentRepository.cs](file:///d:/ASP.NET/UniversityManagement/ClassService/Repositories/HomeroomAssignmentRepository.cs)
```csharp
using ClassService.Data;
using ClassService.Entities;
using ClassService.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ClassService.Repositories;

public class HomeroomAssignmentRepository : IHomeroomAssignmentRepository
{
    private readonly ApplicationDbContext _context;

    public HomeroomAssignmentRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<HomeroomAssignment?> GetActiveAssignmentByClassAsync(Guid classId, string schoolYear)
    {
        // Viết code Linq truy vấn HomeroomAssignments trùng classId và schoolYear
        return await _context.HomeroomAssignments
            .FirstOrDefaultAsync(x => x.ClassId == classId && x.SchoolYear == schoolYear);
    }

    public async Task<List<HomeroomAssignment>> GetHistoryByTeacherAsync(Guid teacherId)
    {
        // Viết code Linq lấy danh sách phân công của giáo viên theo teacherId, sắp xếp theo AssignedDate giảm dần
        return await _context.HomeroomAssignments
            .Where(x => x.TeacherId == teacherId)
            .OrderByDescending(x => x.AssignedDate)
            .ToListAsync();
    }

    public async Task AddAsync(HomeroomAssignment entity)
    {
        await _context.HomeroomAssignments.AddAsync(entity);
    }

    public void Update(HomeroomAssignment entity)
    {
        _context.HomeroomAssignments.Update(entity);
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}
```

---

### 3. Service Layer

#### File: [IHomeroomAssignmentService.cs](file:///d:/ASP.NET/UniversityManagement/ClassService/Services/Interfaces/IHomeroomAssignmentService.cs)
```csharp
using ClassService.DTOs.HomeroomAssignments;

namespace ClassService.Services.Interfaces;

public interface IHomeroomAssignmentService
{
    Task<HomeroomAssignmentResponseDto> AssignHomeroomAsync(Guid classId, AssignHomeroomDto dto);
    Task<HomeroomAssignmentResponseDto> ChangeHomeroomAsync(Guid classId, AssignHomeroomDto dto);
    Task<HomeroomAssignmentResponseDto?> GetCurrentHomeroomAsync(Guid classId, string schoolYear);
    Task<IEnumerable<HomeroomAssignmentResponseDto>> GetTeacherHomeroomHistoryAsync(Guid teacherId);
}
```

#### File: [HomeroomAssignmentService.cs](file:///d:/ASP.NET/UniversityManagement/ClassService/Services/HomeroomAssignmentService.cs)
```csharp
using ClassService.DTOs.HomeroomAssignments;
using ClassService.Entities;
using ClassService.Repositories.Interfaces;
using ClassService.Services.Interfaces;

namespace ClassService.Services;

public class HomeroomAssignmentService : IHomeroomAssignmentService
{
    private readonly IHomeroomAssignmentRepository _homeroomRepository;
    private readonly IClassRepository _classRepository;

    public HomeroomAssignmentService(
        IHomeroomAssignmentRepository homeroomRepository,
        IClassRepository classRepository)
    {
        _homeroomRepository = homeroomRepository;
        _classRepository = classRepository;
    }

    public async Task<HomeroomAssignmentResponseDto> AssignHomeroomAsync(Guid classId, AssignHomeroomDto dto)
    {
        // 1. Kiểm tra xem lớp học có tồn tại không qua ClassRepository. Nếu null ném KeyNotFoundException.
        // 2. Kiểm tra xem lớp học đó trong năm học đó đã có GVCN chưa qua GetActiveAssignmentByClassAsync. 
        //    Nếu có rồi, ném InvalidOperationException("Lớp học đã có giáo viên chủ nhiệm trong năm học này.").
        // 3. Khởi tạo đối tượng HomeroomAssignment, gọi AddAsync và SaveChangesAsync.
        // 4. Trả về kết quả đã map sang HomeroomAssignmentResponseDto.
        throw new NotImplementedException();
    }

    public async Task<HomeroomAssignmentResponseDto> ChangeHomeroomAsync(Guid classId, AssignHomeroomDto dto)
    {
        // 1. Kiểm tra xem lớp học có tồn tại không.
        // 2. Tìm phân công chủ nhiệm hiện tại của lớp trong năm học đó.
        // 3. Nếu chưa phân công bao giờ, có thể ném lỗi hoặc gọi hàm AssignHomeroomAsync.
        // 4. Nếu đã có, cập nhật TeacherId = dto.TeacherId, AssignedDate = DateTime.UtcNow.
        // 5. Gọi Update() và SaveChangesAsync(). Trả về DTO kết quả.
        throw new NotImplementedException();
    }

    public async Task<HomeroomAssignmentResponseDto?> GetCurrentHomeroomAsync(Guid classId, string schoolYear)
    {
        // Lấy phân công GVCN của lớp học theo năm học, map sang DTO nếu tồn tại, ngược lại trả về null.
        throw new NotImplementedException();
    }

    public async Task<IEnumerable<HomeroomAssignmentResponseDto>> GetTeacherHomeroomHistoryAsync(Guid teacherId)
    {
        // Lấy danh sách lịch sử chủ nhiệm của giáo viên và map sang IEnumerable DTO.
        throw new NotImplementedException();
    }

    private static HomeroomAssignmentResponseDto MapToResponseDto(HomeroomAssignment entity)
    {
        return new HomeroomAssignmentResponseDto
        {
            Id = entity.Id,
            TeacherId = entity.TeacherId,
            ClassId = entity.ClassId,
            SchoolYear = entity.SchoolYear,
            AssignedDate = entity.AssignedDate
        };
    }
}
```

---

### 4. Controller Layer

#### File: [HomeroomAssignmentsController.cs](file:///d:/ASP.NET/UniversityManagement/ClassService/Controllers/HomeroomAssignmentsController.cs)
```csharp
using Microsoft.AspNetCore.Mvc;
using ClassService.Services.Interfaces;
using ClassService.DTOs.HomeroomAssignments;

namespace ClassService.Controllers;

[ApiController]
[Route("api/classes")]
public class HomeroomAssignmentsController : ControllerBase
{
    private readonly IHomeroomAssignmentService _homeroomService;

    public HomeroomAssignmentsController(IHomeroomAssignmentService homeroomService)
    {
        _homeroomService = homeroomService;
    }

    [HttpPost("{classId:guid}/homeroom")]
    public async Task<ActionResult<HomeroomAssignmentResponseDto>> AssignHomeroom(Guid classId, [FromBody] AssignHomeroomDto dto)
    {
        try
        {
            var result = await _homeroomService.AssignHomeroomAsync(classId, dto);
            return Ok(result);
        }
        catch (KeyNotFoundException ex) { return NotFound(new { message = ex.Message }); }
        catch (InvalidOperationException ex) { return BadRequest(new { message = ex.Message }); }
    }

    [HttpPut("{classId:guid}/homeroom")]
    public async Task<ActionResult<HomeroomAssignmentResponseDto>> ChangeHomeroom(Guid classId, [FromBody] AssignHomeroomDto dto)
    {
        try
        {
            var result = await _homeroomService.ChangeHomeroomAsync(classId, dto);
            return Ok(result);
        }
        catch (KeyNotFoundException ex) { return NotFound(new { message = ex.Message }); }
        catch (InvalidOperationException ex) { return BadRequest(new { message = ex.Message }); }
    }

    [HttpGet("{classId:guid}/homeroom")]
    public async Task<ActionResult<HomeroomAssignmentResponseDto>> GetHomeroom(Guid classId, [FromQuery] string schoolYear)
    {
        var result = await _homeroomService.GetCurrentHomeroomAsync(classId, schoolYear);
        if (result == null) return NotFound(new { message = "Lớp học chưa được phân công GVCN cho năm học này." });
        return Ok(result);
    }
}
```

---

## BƯỚC 3: Nghiệp vụ Giáo viên Bộ môn (`TeachingAssignment`)

Thực hiện tạo cấu trúc tương tự bước 2. Dưới đây là mô tả chi tiết:

### 1. DTOs (`DTOs/TeachingAssignments`)
* `AssignTeacherDto`: Chứa `TeacherId`, `SubjectId`, `SchoolYear`.
* `TeachingAssignmentResponseDto`: Chứa `Id`, `TeacherId`, `SubjectId`, `ClassId`, `SchoolYear`, `AssignedDate`.

### 2. Repository Layer

#### File: [ITeachingAssignmentRepository.cs](file:///d:/ASP.NET/UniversityManagement/ClassService/Repositories/Interfaces/ITeachingAssignmentRepository.cs)
```csharp
using ClassService.Entities;

namespace ClassService.Repositories.Interfaces;

public interface ITeachingAssignmentRepository
{
    Task<List<TeachingAssignment>> GetAssignmentsByClassAsync(Guid classId, string schoolYear);
    Task<List<TeachingAssignment>> GetAssignmentsByTeacherAsync(Guid teacherId, string schoolYear);
    Task<TeachingAssignment?> GetAssignmentAsync(Guid classId, Guid subjectId, string schoolYear);
    Task AddAsync(TeachingAssignment entity);
    void Update(TeachingAssignment entity);
    Task SaveChangesAsync();
}
```

#### File: [TeachingAssignmentRepository.cs](file:///d:/ASP.NET/UniversityManagement/ClassService/Repositories/TeachingAssignmentRepository.cs)
* Thực thi các phương thức dựa trên `ApplicationDbContext.TeachingAssignments`.
* Ví dụ: `GetAssignmentsByClassAsync` lấy danh sách giáo viên bộ môn dạy một lớp trong năm học cụ thể.

### 3. Service Layer

#### File: [ITeachingAssignmentService.cs](file:///d:/ASP.NET/UniversityManagement/ClassService/Services/Interfaces/ITeachingAssignmentService.cs)
```csharp
using ClassService.DTOs.TeachingAssignments;

namespace ClassService.Services.Interfaces;

public interface ITeachingAssignmentService
{
    Task<TeachingAssignmentResponseDto> AssignTeacherAsync(Guid classId, AssignTeacherDto dto);
    Task<TeachingAssignmentResponseDto> ChangeTeacherAsync(Guid classId, Guid subjectId, AssignTeacherDto dto);
    Task<IEnumerable<TeachingAssignmentResponseDto>> GetClassTeachersAsync(Guid classId, string schoolYear);
    Task<IEnumerable<TeachingAssignmentResponseDto>> GetTeacherClassesAsync(Guid teacherId, string schoolYear);
}
```

#### File: [TeachingAssignmentService.cs](file:///d:/ASP.NET/UniversityManagement/ClassService/Services/TeachingAssignmentService.cs)
* **Logic AssignTeacherAsync**:
  * Kiểm tra lớp tồn tại.
  * Kiểm tra xem môn học (`SubjectId`) trong lớp đó (`ClassId`) và năm học đó đã có ai dạy chưa bằng cách dùng `GetAssignmentAsync`. Nếu có, ném lỗi yêu cầu dùng API `ChangeTeacher` để thay thế.
  * Thêm bản ghi mới.
* **Logic ChangeTeacherAsync**:
  * Tìm bản ghi giảng dạy hiện tại của môn học đó tại lớp đó.
  * Cập nhật sang `TeacherId` mới.

### 4. Controller Layer (`TeachingAssignmentsController.cs`)
Expose các endpoint:
* `POST /api/classes/{id}/teachers` ➔ gọi `AssignTeacherAsync`
* `PUT /api/classes/{id}/teachers/{subjectId}` ➔ gọi `ChangeTeacherAsync`
* `GET /api/classes/{id}/teachers` ➔ gọi `GetClassTeachersAsync`
* `GET /api/teachers/{teacherId}/classes` ➔ gọi `GetTeacherClassesAsync`

---

## BƯỚC 4: Nghiệp vụ Thời khóa biểu (`Schedule`)

Đây là nghiệp vụ quan trọng nhất cần kiểm soát va chạm (Collision Detection).

### 1. DTOs (`DTOs/Schedules`)
* `CreateScheduleDto`: Chứa `ClassId`, `SubjectId`, `TeacherId`, `DayOfWeek` (int), `Period` (int), `Room` (string), `SchoolYear`.
* `ScheduleResponseDto`.

### 2. Repository Layer

#### File: [IScheduleRepository.cs](file:///d:/ASP.NET/UniversityManagement/ClassService/Repositories/Interfaces/IScheduleRepository.cs)
```csharp
using ClassService.Entities;

namespace ClassService.Repositories.Interfaces;

public interface IScheduleRepository
{
    Task<List<Schedule>> GetScheduleByClassAsync(Guid classId, string schoolYear);
    Task<List<Schedule>> GetScheduleByTeacherAsync(Guid teacherId, string schoolYear);
    
    // Tìm tiết trùng lịch của Giáo viên
    Task<Schedule?> CheckTeacherCollisionAsync(Guid teacherId, int dayOfWeek, int period, string schoolYear);
    
    // Tìm tiết trùng lịch của Phòng học
    Task<Schedule?> CheckRoomCollisionAsync(string room, int dayOfWeek, int period, string schoolYear);
    
    // Tìm tiết trùng lịch của Lớp học
    Task<Schedule?> CheckClassCollisionAsync(Guid classId, int dayOfWeek, int period, string schoolYear);

    Task AddAsync(Schedule entity);
    void Delete(Schedule entity);
    Task SaveChangesAsync();
}
```

#### File: [ScheduleRepository.cs](file:///d:/ASP.NET/UniversityManagement/ClassService/Repositories/ScheduleRepository.cs)
Thực thi các câu truy vấn va chạm:
```csharp
public async Task<Schedule?> CheckTeacherCollisionAsync(Guid teacherId, int dayOfWeek, int period, string schoolYear)
{
    return await _context.Schedules.FirstOrDefaultAsync(s => 
        s.TeacherId == teacherId && 
        s.DayOfWeek == dayOfWeek && 
        s.Period == period && 
        s.SchoolYear == schoolYear);
}
```
*Tương tự viết cho `CheckRoomCollisionAsync` (so sánh thuộc tính `Room`) và `CheckClassCollisionAsync` (so sánh thuộc tính `ClassId`).*

### 3. Service Layer

#### File: [IScheduleService.cs](file:///d:/ASP.NET/UniversityManagement/ClassService/Services/Interfaces/IScheduleService.cs)
```csharp
using ClassService.DTOs.Schedules;

namespace ClassService.Services.Interfaces;

public interface IScheduleService
{
    Task<ScheduleResponseDto> CreateScheduleAsync(CreateScheduleDto dto);
    Task<IEnumerable<ScheduleResponseDto>> GetClassScheduleAsync(Guid classId, string schoolYear);
    Task<IEnumerable<ScheduleResponseDto>> GetTeacherScheduleAsync(Guid teacherId, string schoolYear);
}
```

#### File: [ScheduleService.cs](file:///d:/ASP.NET/UniversityManagement/ClassService/Services/ScheduleService.cs)
Trong hàm `CreateScheduleAsync`, bạn phải triển khai logic kiểm soát va chạm:

```csharp
public async Task<ScheduleResponseDto> CreateScheduleAsync(CreateScheduleDto dto)
{
    // 1. Kiểm tra va chạm lịch dạy của Giáo viên
    var teacherCollision = await _scheduleRepository.CheckTeacherCollisionAsync(
        dto.TeacherId, dto.DayOfWeek, dto.Period, dto.SchoolYear);
    if (teacherCollision != null)
    {
        throw new InvalidOperationException("Giáo viên này đã có lịch dạy ở lớp học khác vào thời gian này.");
    }

    // 2. Kiểm tra va chạm Phòng học
    var roomCollision = await _scheduleRepository.CheckRoomCollisionAsync(
        dto.Room, dto.DayOfWeek, dto.Period, dto.SchoolYear);
    if (roomCollision != null)
    {
        throw new InvalidOperationException("Phòng học này đã được sử dụng bởi lớp khác vào thời gian này.");
    }

    // 3. Kiểm tra va chạm lịch của chính Lớp học đó
    var classCollision = await _scheduleRepository.CheckClassCollisionAsync(
        dto.ClassId, dto.DayOfWeek, dto.Period, dto.SchoolYear);
    if (classCollision != null)
    {
        throw new InvalidOperationException("Lớp học này đã có môn học khác được xếp lịch vào thời gian này.");
    }

    // 4. Nếu vượt qua tất cả kiểm tra, tiến hành Add và Save
    var schedule = new Schedule { ... };
    await _scheduleRepository.AddAsync(schedule);
    await _scheduleRepository.SaveChangesAsync();

    return MapToResponseDto(schedule);
}
```

### 4. Controller Layer (`SchedulesController.cs`)
Expose các API endpoint:
* `POST /api/schedules` (Tạo mới thời khóa biểu)
* `GET /api/classes/{classId}/schedule` (Xem thời khóa biểu của lớp học)
* `GET /api/teachers/{teacherId}/schedule` (Xem lịch dạy giáo viên)

---

## BƯỚC 5: Thiết lập Phân Quyền Người Dùng (RBAC)

Để phân quyền hiệu quả, bạn cần thực hiện theo cơ chế sau:

1. **Claims-based Authorization**: Khi người dùng đăng nhập thành công và JWT Token được sinh ra, Token đó bắt buộc phải chứa Claim `"role"` (nhận giá trị là `Admin`, `Teacher`, `Student`).
2. **Cấu hình Authorization trong Endpoint**:
   Sử dụng attribute `[Authorize]` trên các Controller/Action tương ứng.
   * Ví dụ: Đối với các API thay đổi dữ liệu thời khóa biểu hoặc phân công giảng dạy (chỉ Admin được làm):
     ```csharp
     [Authorize(Roles = "Admin")]
     [HttpPost]
     public async Task<IActionResult> CreateSchedule(...)
     ```
3. **Phân Quyền Logic Mềm (Soft Authorization)**:
   Đối với giáo viên chủ nhiệm xem điểm lớp mình, hoặc giáo viên bộ môn nhập điểm môn mình dạy:
   * **Trong code Controller/Service**, hãy lấy `UserId` / `TeacherId` từ token hiện tại:
     `var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;`
   * Kiểm tra trong DB xem giáo viên này có liên kết với lớp yêu cầu không. Ví dụ:
     ```csharp
     var isHomeroom = await _homeroomRepository.GetActiveAssignmentByClassAsync(classId, schoolYear);
     if (isHomeroom == null || isHomeroom.TeacherId != Guid.Parse(currentUserId))
     {
         return Forbid(); // Giáo viên không phải chủ nhiệm của lớp này
     }
     ```
