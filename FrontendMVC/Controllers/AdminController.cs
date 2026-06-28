using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using FrontendMVC.Models;
using Microsoft.AspNetCore.Mvc;

namespace FrontendMVC.Controllers;

public class AdminController : Controller
{
    private readonly IHttpClientFactory _clientFactory;

    public AdminController(IHttpClientFactory clientFactory)
    {
        _clientFactory = clientFactory;
    }

    private string? CheckAdminRole()
    {
        var role = Request.Cookies["user_role"];
        if (role != "Admin")
        {
            return "Auth/AccessDenied";
        }
        return null;
    }

    [HttpGet]
    public async Task<IActionResult> Dashboard()
    {
        var redirect = CheckAdminRole();
        if (redirect != null)
            return RedirectToAction("AccessDenied", "Auth");

        var userClient = _clientFactory.CreateClient("UserService");
        var classClient = _clientFactory.CreateClient("ClassService");
        var subjectClient = _clientFactory.CreateClient("SubjectService");

        int studentCount = 0;
        int teacherCount = 0;
        int classCount = 0;
        int subjectCount = 0;

        try
        {
            var students = await userClient.GetFromJsonAsync<IEnumerable<UserViewModel>>(
                "admin/students"
            );
            studentCount = students?.Count() ?? 0;
        }
        catch { }

        try
        {
            var teachers = await userClient.GetFromJsonAsync<IEnumerable<UserViewModel>>(
                "admin/teachers"
            );
            teacherCount = teachers?.Count() ?? 0;
        }
        catch { }

        try
        {
            var classes = await classClient.GetFromJsonAsync<IEnumerable<ClassViewModel>>(
                "classes"
            );
            classCount = classes?.Count() ?? 0;
        }
        catch { }

        try
        {
            var subjects = await subjectClient.GetFromJsonAsync<IEnumerable<dynamic>>("subjects");
            subjectCount = subjects?.Count() ?? 0;
        }
        catch { }

        ViewBag.StudentCount = studentCount;
        ViewBag.TeacherCount = teacherCount;
        ViewBag.ClassCount = classCount;
        ViewBag.SubjectCount = subjectCount;

        return View();
    }

    [HttpGet]
    public async Task<IActionResult> Classes(string? search, int? grade)
    {
        var redirect = CheckAdminRole();
        if (redirect != null)
            return RedirectToAction("AccessDenied", "Auth");

        var client = _clientFactory.CreateClient("ClassService");
        var classes = new List<ClassViewModel>();
        try
        {
            var response = await client.GetFromJsonAsync<IEnumerable<ClassViewModel>>("classes");
            if (response != null)
            {
                classes = response.ToList();
                if (grade.HasValue)
                {
                    classes = classes.Where(c => c.GradeLevel == grade.Value).ToList();
                }
                if (!string.IsNullOrEmpty(search))
                {
                    classes = classes
                        .Where(c => c.Name.Contains(search, StringComparison.OrdinalIgnoreCase))
                        .ToList();
                }
            }
        }
        catch { }

        ViewBag.Search = search;
        ViewBag.GradeFilter = grade;
        return View(classes);
    }

    [HttpPost]
    public async Task<IActionResult> CreateClass(CreateClassRequest request)
    {
        var redirect = CheckAdminRole();
        if (redirect != null)
            return RedirectToAction("AccessDenied", "Auth");

        if (!ModelState.IsValid)
        {
            return View(request);
        }

        var client = _clientFactory.CreateClient("ClassService");
        try
        {
            var response = await client.PostAsJsonAsync("classes", request);
            if (response.IsSuccessStatusCode)
            {
                TempData["SuccessMessage"] = "Tạo lớp học mới thành công!";
                return RedirectToAction("Classes");
            }
            else
            {
                TempData["ErrorMessage"] = "Không thể tạo lớp học. Vui lòng kiểm tra lại.";
            }
        }
        catch
        {
            TempData["ErrorMessage"] = "Lỗi kết nối đến máy chủ ClassService.";
        }

        return View(request);
    }

    [HttpGet]
    public async Task<IActionResult> ClassDetail(Guid id)
    {
        var redirect = CheckAdminRole();
        if (redirect != null)
            return RedirectToAction("AccessDenied", "Auth");

        var classClient = _clientFactory.CreateClient("ClassService");
        var userClient = _clientFactory.CreateClient("UserService");
        var subjectClient = _clientFactory.CreateClient("SubjectService");

        ClassViewModel? classInfo = null;
        var studentsInClass = new List<StudentClassViewModel>();
        HomeroomAssignmentViewModel? homeroom = null;
        var teachingAssignments = new List<TeachingAssignmentViewModel>();
        var schedules = new List<ScheduleViewModel>();

        var allTeachers = new List<UserViewModel>();
        var allStudents = new List<UserViewModel>();
        var allSubjects = new List<SubjectViewModel>();

        try
        {
            classInfo = await classClient.GetFromJsonAsync<ClassViewModel>($"classes/{id}");
        }
        catch { }
        if (classInfo == null)
            return NotFound();

        try
        {
            var studentsResp = await classClient.GetFromJsonAsync<
                IEnumerable<StudentClassViewModel>
            >($"student-classes/classes/{id}/students");
            if (studentsResp != null)
                studentsInClass = studentsResp.ToList();
        }
        catch { }

        try
        {
            homeroom = await classClient.GetFromJsonAsync<HomeroomAssignmentViewModel>(
                $"classes/{id}/homeroom?schoolYear={Uri.EscapeDataString(classInfo.SchoolYear)}"
            );
        }
        catch { }

        try
        {
            var teachingResp = await classClient.GetFromJsonAsync<
                IEnumerable<TeachingAssignmentViewModel>
            >($"classes/{id}/teachers?schoolYear={Uri.EscapeDataString(classInfo.SchoolYear)}");
            if (teachingResp != null)
                teachingAssignments = teachingResp.ToList();
        }
        catch { }

        try
        {
            var schedulesResp = await classClient.GetFromJsonAsync<IEnumerable<ScheduleViewModel>>(
                $"classes/{id}/schedule?schoolYear={Uri.EscapeDataString(classInfo.SchoolYear)}"
            );
            if (schedulesResp != null)
                schedules = schedulesResp.ToList();
        }
        catch { }

        try
        {
            var tResp = await userClient.GetFromJsonAsync<IEnumerable<UserViewModel>>(
                "admin/teachers"
            );
            if (tResp != null)
                allTeachers = tResp.ToList();
        }
        catch { }

        var assignedStudentIds = new List<Guid>();
        try
        {
            var assignedResp = await classClient.GetFromJsonAsync<IEnumerable<Guid>>(
                "student-classes/assigned-student-ids"
            );
            if (assignedResp != null)
                assignedStudentIds = assignedResp.ToList();
        }
        catch { }

        try
        {
            var sResp = await userClient.GetFromJsonAsync<IEnumerable<UserViewModel>>(
                "admin/students"
            );
            if (sResp != null)
            {
                allStudents = sResp.Where(s => !assignedStudentIds.Contains(s.Id)).ToList();
            }
        }
        catch { }
        try
        {
            var subResp = await subjectClient.GetFromJsonAsync<IEnumerable<SubjectViewModel>>(
                "subjects"
            );
            if (subResp != null)
                allSubjects = subResp.ToList();
        }
        catch { }

        var assignedHomerooms = new List<HomeroomAssignmentViewModel>();
        try
        {
            var hrResp = await classClient.GetFromJsonAsync<
                IEnumerable<HomeroomAssignmentViewModel>
            >($"classes/homerooms?schoolYear={Uri.EscapeDataString(classInfo.SchoolYear)}");
            if (hrResp != null)
                assignedHomerooms = hrResp.ToList();
        }
        catch { }

        // Lọc giáo viên chủ nhiệm: loại bỏ giáo viên đã là GVCN của lớp khác
        var assignedTeacherIds = assignedHomerooms
            .Where(h => h.ClassId != id)
            .Select(h => h.TeacherId)
            .ToList();

        var availableHomeroomTeachers = allTeachers
            .Where(t => !assignedTeacherIds.Contains(t.Id))
            .ToList();

        // Lọc môn học chưa được phân công trong lớp này
        var assignedSubjectIds = teachingAssignments.Select(ta => ta.SubjectId).ToList();
        var availableSubjects = allSubjects
            .Where(s => s.GradeLevel == classInfo.GradeLevel && !assignedSubjectIds.Contains(s.Id))
            .ToList();

        ViewBag.ClassInfo = classInfo;
        ViewBag.StudentsInClass = studentsInClass;
        ViewBag.Homeroom = homeroom;
        ViewBag.TeachingAssignments = teachingAssignments;
        ViewBag.Schedules = schedules;

        ViewBag.AllTeachers = allTeachers;
        ViewBag.AvailableHomeroomTeachers = availableHomeroomTeachers;
        ViewBag.AvailableSubjects = availableSubjects;
        ViewBag.AllStudents = allStudents;
        ViewBag.AllSubjects = allSubjects;

        return View();
    }

    [HttpPost]
    public async Task<IActionResult> AssignHomeroom(Guid classId, AssignHomeroomViewModel model)
    {
        var redirect = CheckAdminRole();
        if (redirect != null)
            return RedirectToAction("AccessDenied", "Auth");

        var client = classClientHelper();
        try
        {
            var response = await client.PostAsJsonAsync($"classes/{classId}/homeroom", model);
            bool success = response.IsSuccessStatusCode;
            string message = "Phân công giáo viên chủ nhiệm thành công!";

            if (!success)
            {
                // Thử PUT nếu đã tồn tại homeroom
                var responsePut = await client.PutAsJsonAsync($"classes/{classId}/homeroom", model);
                success = responsePut.IsSuccessStatusCode;
                message = success
                    ? "Cập nhật giáo viên chủ nhiệm thành công!"
                    : "Không thể phân công giáo viên chủ nhiệm.";
            }

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return Json(new { success, message });
            }

            if (success)
                TempData["SuccessMessage"] = message;
            else
                TempData["ErrorMessage"] = message;
        }
        catch
        {
            var message = "Lỗi kết nối đến ClassService.";
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return Json(new { success = false, message });
            }
            TempData["ErrorMessage"] = message;
        }

        return RedirectToAction("ClassDetail", new { id = classId });
    }

    [HttpPost]
    public async Task<IActionResult> AssignTeaching(Guid classId, AssignTeacherViewModel model)
    {
        var redirect = CheckAdminRole();
        if (redirect != null)
            return RedirectToAction("AccessDenied", "Auth");

        var client = classClientHelper();
        try
        {
            var response = await client.PostAsJsonAsync($"classes/{classId}/teachers", model);
            bool success = response.IsSuccessStatusCode;
            string message = "Phân công giáo viên bộ môn thành công!";

            if (!success)
            {
                // Thử PUT nếu đã được phân công giáo viên bộ môn trước đó (để cập nhật lại giáo viên)
                var responsePut = await client.PutAsJsonAsync(
                    $"classes/{classId}/teachers/{model.SubjectId}",
                    model
                );
                success = responsePut.IsSuccessStatusCode;
                message = success
                    ? "Cập nhật giáo viên bộ môn thành công!"
                    : "Không thể phân công giáo viên bộ môn.";
            }

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return Json(new { success, message });
            }

            if (success)
                TempData["SuccessMessage"] = message;
            else
                TempData["ErrorMessage"] = message;
        }
        catch
        {
            var message = "Lỗi kết nối đến ClassService.";
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return Json(new { success = false, message });
            }
            TempData["ErrorMessage"] = message;
        }

        return RedirectToAction("ClassDetail", new { id = classId });
    }

    [HttpPost]
    public async Task<IActionResult> AssignStudent(Guid classId, AssignStudentViewModel model)
    {
        var redirect = CheckAdminRole();
        if (redirect != null)
            return RedirectToAction("AccessDenied", "Auth");

        var client = classClientHelper();
        try
        {
            var response = await client.PostAsJsonAsync(
                $"student-classes/classes/{classId}/students",
                model
            );
            if (response.IsSuccessStatusCode)
            {
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return Json(
                        new { success = true, message = "Thêm học sinh vào lớp thành công!" }
                    );
                }
                TempData["SuccessMessage"] = "Thêm học sinh vào lớp thành công!";
            }
            else
            {
                var errorObj = await response.Content.ReadFromJsonAsync<ErrorResponse>();
                var errorMsg =
                    errorObj?.Message
                    ?? "Không thể thêm học sinh. Có thể học sinh đã có lớp hiện tại.";
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return Json(new { success = false, message = errorMsg });
                }
                TempData["ErrorMessage"] = errorMsg;
            }
        }
        catch
        {
            var errorMsg = "Lỗi kết nối đến ClassService.";
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return Json(new { success = false, message = errorMsg });
            }
            TempData["ErrorMessage"] = errorMsg;
        }

        return RedirectToAction("ClassDetail", new { id = classId });
    }

    [HttpPost]
    public async Task<IActionResult> RemoveStudent(Guid classId, Guid studentId)
    {
        var redirect = CheckAdminRole();
        if (redirect != null)
            return RedirectToAction("AccessDenied", "Auth");

        var client = classClientHelper();
        try
        {
            var response = await client.DeleteAsync(
                $"student-classes/classes/{classId}/students/{studentId}"
            );
            if (response.IsSuccessStatusCode)
            {
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return Json(new { success = true, message = "Đã xoá học sinh khỏi lớp." });
                }
                TempData["SuccessMessage"] = "Đã xoá học sinh khỏi lớp.";
            }
            else
            {
                var errorMsg = "Không thể xoá học sinh khỏi lớp.";
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return Json(new { success = false, message = errorMsg });
                }
                TempData["ErrorMessage"] = errorMsg;
            }
        }
        catch
        {
            var errorMsg = "Lỗi kết nối đến ClassService.";
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return Json(new { success = false, message = errorMsg });
            }
            TempData["ErrorMessage"] = errorMsg;
        }

        return RedirectToAction("ClassDetail", new { id = classId });
    }

    [HttpPost]
    public async Task<IActionResult> CreateSchedule(Guid classId, ScheduleViewModel model)
    {
        var redirect = CheckAdminRole();
        if (redirect != null)
            return RedirectToAction("AccessDenied", "Auth");

        var client = classClientHelper();
        try
        {
            model.ClassId = classId;
            var response = await client.PostAsJsonAsync("schedules", model);
            if (response.IsSuccessStatusCode)
            {
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return Json(
                        new
                        {
                            success = true,
                            message = "Thêm tiết học vào thời khóa biểu thành công!",
                        }
                    );
                }
                TempData["SuccessMessage"] = "Thêm tiết học vào thời khóa biểu thành công!";
            }
            else
            {
                var errorObj = await response.Content.ReadFromJsonAsync<ErrorResponse>();
                var errorMsg =
                    errorObj?.Message ?? "Trùng lặp thời gian phòng học, giáo viên hoặc lớp học.";
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return Json(new { success = false, message = errorMsg });
                }
                TempData["ErrorMessage"] = errorMsg;
            }
        }
        catch
        {
            var errorMsg = "Lỗi kết nối đến ClassService.";
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return Json(new { success = false, message = errorMsg });
            }
            TempData["ErrorMessage"] = errorMsg;
        }

        return RedirectToAction("ClassDetail", new { id = classId });
    }

    // ==========================================
    // CLASS CRUD ACTIONS
    // ==========================================
    [HttpGet]
    public IActionResult CreateClass()
    {
        var redirect = CheckAdminRole();
        if (redirect != null)
            return RedirectToAction("AccessDenied", "Auth");
        return View(new CreateClassRequest());
    }

    [HttpGet]
    public async Task<IActionResult> EditClass(Guid id)
    {
        var redirect = CheckAdminRole();
        if (redirect != null)
            return RedirectToAction("AccessDenied", "Auth");

        var client = classClientHelper();
        try
        {
            var cls = await client.GetFromJsonAsync<ClassViewModel>($"classes/{id}");
            if (cls == null)
                return NotFound();

            var model = new CreateClassRequest
            {
                Name = cls.Name,
                GradeLevel = cls.GradeLevel,
                SchoolYear = cls.SchoolYear,
            };
            ViewBag.ClassId = id;
            return View(model);
        }
        catch
        {
            TempData["ErrorMessage"] = "Không thể tìm thấy thông tin lớp học.";
            return RedirectToAction("Classes");
        }
    }

    [HttpPost]
    public async Task<IActionResult> EditClass(Guid id, CreateClassRequest request)
    {
        var redirect = CheckAdminRole();
        if (redirect != null)
            return RedirectToAction("AccessDenied", "Auth");

        if (!ModelState.IsValid)
        {
            ViewBag.ClassId = id;
            return View(request);
        }

        var client = classClientHelper();
        try
        {
            var response = await client.PutAsJsonAsync($"classes/{id}", request);
            if (response.IsSuccessStatusCode)
            {
                TempData["SuccessMessage"] = "Cập nhật lớp học thành công!";
                return RedirectToAction("Classes");
            }
            TempData["ErrorMessage"] = "Không thể cập nhật lớp học.";
        }
        catch
        {
            TempData["ErrorMessage"] = "Lỗi kết nối đến ClassService.";
        }

        ViewBag.ClassId = id;
        return View(request);
    }

    [HttpPost]
    public async Task<IActionResult> DeleteClass(Guid id)
    {
        var redirect = CheckAdminRole();
        if (redirect != null)
            return RedirectToAction("AccessDenied", "Auth");

        var client = classClientHelper();
        try
        {
            var response = await client.DeleteAsync($"classes/{id}");
            if (response.IsSuccessStatusCode)
            {
                TempData["SuccessMessage"] = "Xóa lớp học thành công!";
            }
            else
            {
                TempData["ErrorMessage"] =
                    "Không thể xóa lớp học này (có thể có học sinh/phân công đang liên kết).";
            }
        }
        catch
        {
            TempData["ErrorMessage"] = "Lỗi kết nối đến ClassService.";
        }

        return RedirectToAction("Classes");
    }

    // ==========================================
    // SUBJECT CRUD ACTIONS
    // ==========================================
    [HttpGet]
    public async Task<IActionResult> Subjects(string? search, int? grade)
    {
        var redirect = CheckAdminRole();
        if (redirect != null)
            return RedirectToAction("AccessDenied", "Auth");

        var client = _clientFactory.CreateClient("SubjectService");
        var subjects = new List<SubjectViewModel>();
        try
        {
            var response = await client.GetFromJsonAsync<IEnumerable<SubjectViewModel>>("subjects");
            if (response != null)
            {
                subjects = response.ToList();
                if (grade.HasValue)
                {
                    subjects = subjects.Where(s => s.GradeLevel == grade.Value).ToList();
                }
                if (!string.IsNullOrEmpty(search))
                {
                    subjects = subjects
                        .Where(s =>
                            s.Name.Contains(search, StringComparison.OrdinalIgnoreCase)
                            || s.Code.Contains(search, StringComparison.OrdinalIgnoreCase)
                        )
                        .ToList();
                }
            }
        }
        catch
        {
            TempData["ErrorMessage"] = "Lỗi kết nối đến SubjectService.";
        }

        ViewBag.Search = search;
        ViewBag.GradeFilter = grade;
        return View(subjects);
    }

    [HttpGet]
    public IActionResult CreateSubject()
    {
        var redirect = CheckAdminRole();
        if (redirect != null)
            return RedirectToAction("AccessDenied", "Auth");
        return View(new CreateSubjectRequest());
    }

    [HttpPost]
    public async Task<IActionResult> CreateSubject(CreateSubjectRequest request)
    {
        var redirect = CheckAdminRole();
        if (redirect != null)
            return RedirectToAction("AccessDenied", "Auth");

        if (!ModelState.IsValid)
            return View(request);

        var client = _clientFactory.CreateClient("SubjectService");
        try
        {
            var response = await client.PostAsJsonAsync("subjects", request);
            if (response.IsSuccessStatusCode)
            {
                TempData["SuccessMessage"] = "Thêm môn học mới thành công!";
                return RedirectToAction("Subjects");
            }

            var errObj = await response.Content.ReadFromJsonAsync<ErrorResponse>();
            TempData["ErrorMessage"] = errObj?.Message ?? "Không thể tạo môn học mới.";
        }
        catch
        {
            TempData["ErrorMessage"] = "Lỗi kết nối đến SubjectService.";
        }
        return View(request);
    }

    [HttpGet]
    public async Task<IActionResult> EditSubject(Guid id)
    {
        var redirect = CheckAdminRole();
        if (redirect != null)
            return RedirectToAction("AccessDenied", "Auth");

        var client = _clientFactory.CreateClient("SubjectService");
        try
        {
            var sub = await client.GetFromJsonAsync<SubjectViewModel>($"subjects/{id}");
            if (sub == null)
                return NotFound();

            var model = new UpdateSubjectRequest
            {
                Name = sub.Name,
                Description = sub.Description,
                GradeLevel = sub.GradeLevel,
            };
            ViewBag.SubjectId = id;
            ViewBag.Code = sub.Code;
            return View(model);
        }
        catch
        {
            TempData["ErrorMessage"] = "Không thể tìm thấy môn học.";
            return RedirectToAction("Subjects");
        }
    }

    [HttpPost]
    public async Task<IActionResult> EditSubject(Guid id, UpdateSubjectRequest request)
    {
        var redirect = CheckAdminRole();
        if (redirect != null)
            return RedirectToAction("AccessDenied", "Auth");

        if (!ModelState.IsValid)
        {
            ViewBag.SubjectId = id;
            return View(request);
        }

        var client = _clientFactory.CreateClient("SubjectService");
        try
        {
            var response = await client.PutAsJsonAsync($"subjects/{id}", request);
            if (response.IsSuccessStatusCode)
            {
                TempData["SuccessMessage"] = "Cập nhật môn học thành công!";
                return RedirectToAction("Subjects");
            }
            TempData["ErrorMessage"] = "Không thể cập nhật môn học.";
        }
        catch
        {
            TempData["ErrorMessage"] = "Lỗi kết nối đến SubjectService.";
        }

        ViewBag.SubjectId = id;
        return View(request);
    }

    [HttpPost]
    public async Task<IActionResult> DeleteSubject(Guid id)
    {
        var redirect = CheckAdminRole();
        if (redirect != null)
            return RedirectToAction("AccessDenied", "Auth");

        var client = _clientFactory.CreateClient("SubjectService");
        try
        {
            var response = await client.DeleteAsync($"subjects/{id}");
            if (response.IsSuccessStatusCode)
            {
                TempData["SuccessMessage"] = "Xóa môn học thành công!";
            }
            else
            {
                TempData["ErrorMessage"] = "Không thể xóa môn học.";
            }
        }
        catch
        {
            TempData["ErrorMessage"] = "Lỗi kết nối đến SubjectService.";
        }

        return RedirectToAction("Subjects");
    }

    // ==========================================
    // USER CRUD ACTIONS
    // ==========================================
    [HttpGet("users")]
    [HttpGet("users/teachers")]
    [HttpGet("users/students")]
    [HttpGet("Admin/Users")]
    public async Task<IActionResult> Users(string? search, string? role)
    {
        var redirect = CheckAdminRole();
        if (redirect != null)
            return RedirectToAction("AccessDenied", "Auth");

        var path = HttpContext.Request.Path.Value?.ToLower() ?? "";
        if (path.Contains("/teachers"))
        {
            role = "Teacher";
        }
        else if (path.Contains("/students"))
        {
            role = "Student";
        }

        var userClient = _clientFactory.CreateClient("UserService");
        var classClient = classClientHelper();

        var users = new List<UserViewModel>();
        var classes = new List<ClassViewModel>();
        var homerooms = new List<HomeroomAssignmentViewModel>();
        var teachingAssignments = new List<TeachingAssignmentViewModel>();

        try
        {
            var response = await userClient.GetFromJsonAsync<IEnumerable<UserViewModel>>(
                "admin/users"
            );
            if (response != null)
            {
                users = response.ToList();
            }
        }
        catch
        {
            TempData["ErrorMessage"] = "Lỗi kết nối đến UserService.";
        }

        try
        {
            var response = await classClient.GetFromJsonAsync<IEnumerable<ClassViewModel>>(
                "classes"
            );
            if (response != null)
            {
                classes = response.ToList();
            }
        }
        catch { }

        try
        {
            var response = await classClient.GetFromJsonAsync<
                IEnumerable<HomeroomAssignmentViewModel>
            >("classes/homerooms");
            if (response != null)
            {
                homerooms = response.ToList();
            }
        }
        catch { }

        try
        {
            var response = await classClient.GetFromJsonAsync<
                IEnumerable<TeachingAssignmentViewModel>
            >("teaching-assignments");
            if (response != null)
            {
                teachingAssignments = response.ToList();
            }
        }
        catch { }

        ViewBag.Search = search;
        ViewBag.RoleFilter = role;
        ViewBag.Classes = classes;
        ViewBag.Homerooms = homerooms;
        ViewBag.TeachingAssignments = teachingAssignments;
        return View(users);
    }

    [HttpGet]
    public async Task<IActionResult> CreateUser(string? role)
    {
        var redirect = CheckAdminRole();
        if (redirect != null)
            return RedirectToAction("AccessDenied", "Auth");

        var classClient = classClientHelper();
        var classes = new List<ClassViewModel>();
        try
        {
            var classesResp = await classClient.GetFromJsonAsync<IEnumerable<ClassViewModel>>(
                "classes"
            );
            if (classesResp != null)
                classes = classesResp.ToList();
        }
        catch { }

        ViewBag.Classes = classes;
        var model = new AdminCreateUserRequest();
        if (!string.IsNullOrEmpty(role))
        {
            if (role.Equals("Teacher", StringComparison.OrdinalIgnoreCase))
                model.Role = "Teacher";
            else if (role.Equals("Student", StringComparison.OrdinalIgnoreCase))
                model.Role = "Student";
            else if (role.Equals("Admin", StringComparison.OrdinalIgnoreCase))
                model.Role = "Admin";
        }
        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> CreateUser(AdminCreateUserRequest request)
    {
        var redirect = CheckAdminRole();
        if (redirect != null)
            return RedirectToAction("AccessDenied", "Auth");

        if (!ModelState.IsValid)
        {
            var classClient = classClientHelper();
            var classes = new List<ClassViewModel>();
            try
            {
                var classesResp = await classClient.GetFromJsonAsync<IEnumerable<ClassViewModel>>(
                    "classes"
                );
                if (classesResp != null)
                    classes = classesResp.ToList();
            }
            catch { }
            ViewBag.Classes = classes;
            return View(request);
        }

        var client = _clientFactory.CreateClient("UserService");
        try
        {
            var response = await client.PostAsJsonAsync("admin/users", request);
            if (response.IsSuccessStatusCode)
            {
                TempData["SuccessMessage"] = "Thêm người dùng mới thành công!";
                return RedirectToAction("Users", new { role = request.Role });
            }
            var errObj = await response.Content.ReadFromJsonAsync<ErrorResponse>();
            TempData["ErrorMessage"] = errObj?.Message ?? "Không thể thêm người dùng.";
        }
        catch
        {
            TempData["ErrorMessage"] = "Lỗi kết nối đến UserService.";
        }

        var cc = classClientHelper();
        var clList = new List<ClassViewModel>();
        try
        {
            var classesResp = await cc.GetFromJsonAsync<IEnumerable<ClassViewModel>>("classes");
            if (classesResp != null)
                clList = classesResp.ToList();
        }
        catch { }
        ViewBag.Classes = clList;
        return View(request);
    }

    [HttpGet]
    public async Task<IActionResult> EditUser(Guid id)
    {
        var redirect = CheckAdminRole();
        if (redirect != null)
            return RedirectToAction("AccessDenied", "Auth");

        var client = _clientFactory.CreateClient("UserService");
        var classClient = classClientHelper();

        try
        {
            var user = await client.GetFromJsonAsync<UserViewModel>($"admin/users/{id}");
            if (user == null)
                return NotFound();

            var classes = new List<ClassViewModel>();
            try
            {
                var classesResp = await classClient.GetFromJsonAsync<IEnumerable<ClassViewModel>>(
                    "classes"
                );
                if (classesResp != null)
                    classes = classesResp.ToList();
            }
            catch { }

            var request = new UpdateUserRequest
            {
                FullName = user.FullName,
                Email = user.Email,
                UserCode = user.UserCode,
                Gender = (Gender)user.Gender,
                DateOfBirth = user.DateOfBirth,
                PhoneNumber = user.PhoneNumber,
                Address = user.Address,
                ClassId = user.ClassId,
                AcademicDegree = user.TeacherProfile?.AcademicDegree,
                Specialization = user.TeacherProfile?.Specialization,
                Department = user.TeacherProfile?.Department,
                HireDate = user.TeacherProfile?.HireDate,
            };

            ViewBag.UserId = id;
            ViewBag.Username = user.Username;
            var roleName =
                user.Role == 0 ? "Admin"
                : user.Role == 1 ? "Student"
                : "Teacher";
            ViewBag.Role = roleName;
            ViewBag.Classes = classes;

            return View(request);
        }
        catch
        {
            TempData["ErrorMessage"] = "Không thể tìm thấy người dùng.";
            return RedirectToAction("Users");
        }
    }

    [HttpPost]
    public async Task<IActionResult> EditUser(Guid id, UpdateUserRequest request)
    {
        var redirect = CheckAdminRole();
        if (redirect != null)
            return RedirectToAction("AccessDenied", "Auth");

        if (!ModelState.IsValid)
        {
            var cc = classClientHelper();
            var clList = new List<ClassViewModel>();
            try
            {
                var classesResp = await cc.GetFromJsonAsync<IEnumerable<ClassViewModel>>("classes");
                if (classesResp != null)
                    clList = classesResp.ToList();
            }
            catch { }
            ViewBag.UserId = id;
            ViewBag.Classes = clList;
            return View(request);
        }

        var client = _clientFactory.CreateClient("UserService");
        try
        {
            var response = await client.PutAsJsonAsync($"admin/users/{id}", request);
            if (response.IsSuccessStatusCode)
            {
                TempData["SuccessMessage"] = "Cập nhật người dùng thành công!";
                return RedirectToAction("Users");
            }
            var errObj = await response.Content.ReadFromJsonAsync<ErrorResponse>();
            TempData["ErrorMessage"] = errObj?.Message ?? "Không thể cập nhật người dùng.";
        }
        catch
        {
            TempData["ErrorMessage"] = "Lỗi kết nối đến UserService.";
        }

        var cc2 = classClientHelper();
        var clList2 = new List<ClassViewModel>();
        try
        {
            var classesResp = await cc2.GetFromJsonAsync<IEnumerable<ClassViewModel>>("classes");
            if (classesResp != null)
                clList2 = classesResp.ToList();
        }
        catch { }
        ViewBag.UserId = id;
        ViewBag.Classes = clList2;
        return View(request);
    }

    [HttpPost]
    public async Task<IActionResult> DeleteUser(Guid id)
    {
        var redirect = CheckAdminRole();
        if (redirect != null)
            return RedirectToAction("AccessDenied", "Auth");

        var client = _clientFactory.CreateClient("UserService");
        try
        {
            var response = await client.DeleteAsync($"admin/users/{id}");
            if (response.IsSuccessStatusCode)
            {
                TempData["SuccessMessage"] = "Xóa người dùng thành công!";
            }
            else
            {
                TempData["ErrorMessage"] = "Không thể xóa người dùng.";
            }
        }
        catch
        {
            TempData["ErrorMessage"] = "Lỗi kết nối đến UserService.";
        }

        return RedirectToAction("Users");
    }

    [HttpGet]
    public async Task<IActionResult> GetEligibleStudentsJson()
    {
        var userClient = _clientFactory.CreateClient("UserService");
        var classClient = _clientFactory.CreateClient("ClassService");

        var allStudents = new List<UserViewModel>();
        var assignedStudentIds = new List<Guid>();

        try
        {
            var assignedResp = await classClient.GetFromJsonAsync<IEnumerable<Guid>>(
                "student-classes/assigned-student-ids"
            );
            if (assignedResp != null)
                assignedStudentIds = assignedResp.ToList();
        }
        catch { }

        try
        {
            var sResp = await userClient.GetFromJsonAsync<IEnumerable<UserViewModel>>(
                "admin/students"
            );
            if (sResp != null)
                allStudents = sResp.ToList();
        }
        catch { }

        var eligibleStudents = allStudents
            .Where(s => !assignedStudentIds.Contains(s.Id))
            .Select(s => new
            {
                s.Id,
                s.FullName,
                s.UserCode,
            })
            .ToList();

        return Json(eligibleStudents);
    }

    [HttpGet]
    public async Task<IActionResult> GetClassStudentsJson(Guid id)
    {
        var classClient = _clientFactory.CreateClient("ClassService");
        try
        {
            var studentsResp = await classClient.GetFromJsonAsync<
                IEnumerable<StudentClassViewModel>
            >($"student-classes/classes/{id}/students");
            return Json(studentsResp ?? new List<StudentClassViewModel>());
        }
        catch
        {
            return Json(new List<StudentClassViewModel>());
        }
    }

    [HttpGet]
    public async Task<IActionResult> GetClassScheduleJson(Guid id)
    {
        var classClient = _clientFactory.CreateClient("ClassService");
        try
        {
            var classInfo = await classClient.GetFromJsonAsync<ClassViewModel>($"classes/{id}");
            var schoolYear = classInfo?.SchoolYear ?? "2025-2026";
            var schedulesResp = await classClient.GetFromJsonAsync<IEnumerable<ScheduleViewModel>>(
                $"classes/{id}/schedule?schoolYear={Uri.EscapeDataString(schoolYear)}"
            );
            return Json(schedulesResp ?? new List<ScheduleViewModel>());
        }
        catch
        {
            return Json(new List<ScheduleViewModel>());
        }
    }

    [HttpPost]
    public async Task<IActionResult> RemoveTeaching(Guid classId, Guid subjectId)
    {
        var redirect = CheckAdminRole();
        if (redirect != null)
            return RedirectToAction("AccessDenied", "Auth");

        var client = classClientHelper();
        try
        {
            var response = await client.DeleteAsync($"classes/{classId}/teachers/{subjectId}");
            if (response.IsSuccessStatusCode)
            {
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return Json(
                        new { success = true, message = "Đã xoá phân công giảng dạy thành công!" }
                    );
                }
                TempData["SuccessMessage"] = "Đã xoá phân công giảng dạy thành công!";
            }
            else
            {
                var errorMsg = "Không thể xoá phân công giảng dạy.";
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return Json(new { success = false, message = errorMsg });
                }
                TempData["ErrorMessage"] = errorMsg;
            }
        }
        catch
        {
            var errorMsg = "Lỗi kết nối đến ClassService.";
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return Json(new { success = false, message = errorMsg });
            }
            TempData["ErrorMessage"] = errorMsg;
        }

        return RedirectToAction("ClassDetail", new { id = classId });
    }

    [HttpGet]
    public async Task<IActionResult> GetClassTeachersJson(Guid id)
    {
        var classClient = _clientFactory.CreateClient("ClassService");
        try
        {
            var classInfo = await classClient.GetFromJsonAsync<ClassViewModel>($"classes/{id}");
            var schoolYear = classInfo?.SchoolYear ?? "2025-2026";
            var response = await classClient.GetFromJsonAsync<
                IEnumerable<TeachingAssignmentViewModel>
            >($"classes/{id}/teachers?schoolYear={Uri.EscapeDataString(schoolYear)}");
            return Json(response ?? new List<TeachingAssignmentViewModel>());
        }
        catch
        {
            return Json(new List<TeachingAssignmentViewModel>());
        }
    }

    [HttpGet]
    public async Task<IActionResult> GetClassHomeroomJson(Guid id)
    {
        var classClient = _clientFactory.CreateClient("ClassService");
        try
        {
            var classInfo = await classClient.GetFromJsonAsync<ClassViewModel>($"classes/{id}");
            var schoolYear = classInfo?.SchoolYear ?? "2025-2026";
            var homeroom = await classClient.GetFromJsonAsync<HomeroomAssignmentViewModel>(
                $"classes/{id}/homeroom?schoolYear={Uri.EscapeDataString(schoolYear)}"
            );
            return Json(new { success = true, homeroom });
        }
        catch
        {
            return Json(new { success = false });
        }
    }

    [HttpGet]
    public async Task<IActionResult> GetUserJson(Guid id)
    {
        var userClient = _clientFactory.CreateClient("UserService");
        var classClient = _clientFactory.CreateClient("ClassService");
        var scoreClient = _clientFactory.CreateClient("ScoreService");

        try
        {
            var user = await userClient.GetFromJsonAsync<UserViewModel>($"admin/users/{id}");
            if (user == null)
            {
                return Json(new { success = false, message = "Không tìm thấy người dùng." });
            }

            string className = "";
            var historyData = new List<dynamic>();

            if (user.Role == 1) // Student
            {
                var historyList = new List<StudentClassViewModel>();
                try
                {
                    var historyResp = await classClient.GetFromJsonAsync<IEnumerable<StudentClassViewModel>>(
                        $"student-classes/students/{id}/history"
                    );
                    if (historyResp != null)
                    {
                        historyList = historyResp.ToList();
                    }
                }
                catch { }

                var scoresList = new List<ScoreResponseViewModel>();
                try
                {
                    var scoresResp = await scoreClient.GetFromJsonAsync<IEnumerable<ScoreResponseViewModel>>(
                        $"scores/student/{id}"
                    );
                    if (scoresResp != null)
                    {
                        scoresList = scoresResp.ToList();
                    }
                }
                catch { }

                foreach (var sc in historyList)
                {
                    var yearScores = scoresList.Where(s => s.SchoolYear == sc.SchoolYear).ToList();
                    
                    var groupedScores = yearScores.GroupBy(s => s.SubjectName)
                        .Select(g => {
                            var list = g.ToList();
                            var oral = list.FirstOrDefault(s => s.Type == "Oral")?.ScoreValue;
                            var fifteen = list.FirstOrDefault(s => s.Type == "FifteenMinutes")?.ScoreValue;
                            var period = list.FirstOrDefault(s => s.Type == "OnePeriod")?.ScoreValue;
                            var midterm = list.FirstOrDefault(s => s.Type == "MidTerm")?.ScoreValue;
                            var final = list.FirstOrDefault(s => s.Type == "Final")?.ScoreValue;

                            decimal? tbm = null;
                            decimal totalWeight = 0;
                            decimal weightedSum = 0;

                            if (oral.HasValue) { weightedSum += oral.Value * 1; totalWeight += 1; }
                            if (fifteen.HasValue) { weightedSum += fifteen.Value * 1; totalWeight += 1; }
                            if (period.HasValue) { weightedSum += period.Value * 2; totalWeight += 2; }
                            if (midterm.HasValue) { weightedSum += midterm.Value * 2; totalWeight += 2; }
                            if (final.HasValue) { weightedSum += final.Value * 3; totalWeight += 3; }

                            if (totalWeight > 0)
                            {
                                tbm = Math.Round(weightedSum / totalWeight, 1);
                            }

                            return new {
                                subjectName = g.Key,
                                oral = oral.HasValue ? oral.Value.ToString("0.0") : "—",
                                fifteen = fifteen.HasValue ? fifteen.Value.ToString("0.0") : "—",
                                period = period.HasValue ? period.Value.ToString("0.0") : "—",
                                midterm = midterm.HasValue ? midterm.Value.ToString("0.0") : "—",
                                final = final.HasValue ? final.Value.ToString("0.0") : "—",
                                tbm = tbm.HasValue ? tbm.Value.ToString("0.0") : "—"
                            };
                        }).ToList();

                    var finalScores = yearScores.Where(s => s.Type == "Final").Select(s => (double)s.ScoreValue).ToList();
                    double? gpa = null;
                    if (finalScores.Any())
                    {
                        gpa = Math.Round(finalScores.Average(), 2);
                    }

                    historyData.Add(new {
                        classId = sc.ClassId,
                        className = sc.ClassName,
                        schoolYear = sc.SchoolYear,
                        isCurrent = sc.IsCurrent,
                        scores = groupedScores,
                        gpa = gpa
                    });
                }

                className = historyList.FirstOrDefault(sc => sc.IsCurrent)?.ClassName ?? "Chưa xếp lớp";
            }

            var teachingSubjects = new List<string>();
            var teachingClasses = new List<string>();
            string homeroomClassName = "";
            if (user.Role == 2) // Teacher
            {
                var classes = new List<ClassViewModel>();
                try
                {
                    var classesResp = await classClient.GetFromJsonAsync<
                        IEnumerable<ClassViewModel>
                    >("classes");
                    if (classesResp != null)
                    {
                        classes = classesResp.ToList();
                    }
                }
                catch { }

                try
                {
                    var assignments = await classClient.GetFromJsonAsync<
                        IEnumerable<TeachingAssignmentViewModel>
                    >($"teachers/{id}/classes");
                    if (assignments != null)
                    {
                        teachingSubjects = assignments
                            .Select(a => a.SubjectName)
                            .Distinct()
                            .ToList();
                        teachingClasses = assignments
                            .Select(a =>
                            {
                                var cls = classes.FirstOrDefault(c => c.Id == a.ClassId);
                                return cls != null ? cls.Name : a.ClassName;
                            })
                            .Where(name => !string.IsNullOrEmpty(name))
                            .Distinct()
                            .ToList();
                    }
                }
                catch { }

                try
                {
                    var hrResp = await classClient.GetFromJsonAsync<
                        IEnumerable<HomeroomAssignmentViewModel>
                    >("classes/homerooms");
                    if (hrResp != null)
                    {
                        var hr = hrResp.FirstOrDefault(h => h.TeacherId == id);
                        if (hr != null)
                        {
                            var cls = classes.FirstOrDefault(c => c.Id == hr.ClassId);
                            homeroomClassName = cls != null ? cls.Name : hr.ClassName;
                        }
                    }
                }
                catch { }
            }

            return Json(
                new
                {
                    success = true,
                    user,
                    className,
                    history = historyData,
                    teachingSubjects,
                    teachingClasses,
                    homeroomClassName,
                }
            );
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = ex.Message });
        }
    }

    private string GetNextSchoolYear(string currentSchoolYear)
    {
        var parts = currentSchoolYear.Split('-');
        if (
            parts.Length == 2
            && int.TryParse(parts[0], out int startYear)
            && int.TryParse(parts[1], out int endYear)
        )
        {
            return $"{startYear + 1}-{endYear + 1}";
        }
        return currentSchoolYear;
    }

    [HttpGet]
    public async Task<IActionResult> Promotion()
    {
        var redirect = CheckAdminRole();
        if (redirect != null)
            return RedirectToAction("AccessDenied", "Auth");

        var classClient = classClientHelper();
        var classes = new List<ClassViewModel>();
        try
        {
            var response = await classClient.GetFromJsonAsync<IEnumerable<ClassViewModel>>(
                "classes"
            );
            if (response != null)
            {
                classes = response.ToList();
            }
        }
        catch { }

        var schoolYears = classes
            .Select(c => c.SchoolYear)
            .Distinct()
            .OrderByDescending(y => y)
            .ToList();
        ViewBag.SchoolYears = schoolYears;
        ViewBag.Classes = classes;
        return View();
    }

    [HttpGet]
    public async Task<IActionResult> GetPromotionStudentsJson(Guid classId)
    {
        var redirect = CheckAdminRole();
        if (redirect != null)
            return Json(new { success = false, message = "Access denied" });

        var classClient = classClientHelper();
        try
        {
            var students = await classClient.GetFromJsonAsync<IEnumerable<StudentClassViewModel>>(
                $"student-classes/classes/{classId}/students?onlyCurrent=true"
            );
            return Json(new { success = true, students });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = ex.Message });
        }
    }

    [HttpGet]
    public async Task<IActionResult> GetTargetClassesJson(Guid classId)
    {
        var redirect = CheckAdminRole();
        if (redirect != null)
            return Json(new { success = false, message = "Access denied" });

        var classClient = classClientHelper();
        try
        {
            var currentClass = await classClient.GetFromJsonAsync<ClassViewModel>(
                $"classes/{classId}"
            );
            if (currentClass == null)
            {
                return Json(
                    new { success = false, message = "Không tìm thấy thông tin lớp học hiện tại." }
                );
            }

            if (currentClass.GradeLevel == 12)
            {
                return Json(
                    new
                    {
                        success = true,
                        isGrade12 = true,
                        targetClasses = new List<ClassViewModel>(),
                    }
                );
            }

            var nextYear = GetNextSchoolYear(currentClass.SchoolYear);
            var allClasses = await classClient.GetFromJsonAsync<IEnumerable<ClassViewModel>>(
                "classes"
            );
            var targetClasses =
                allClasses
                    ?.Where(c =>
                        c.GradeLevel == currentClass.GradeLevel + 1 && c.SchoolYear == nextYear
                    )
                    .ToList()
                ?? new List<ClassViewModel>();

            return Json(
                new
                {
                    success = true,
                    isGrade12 = false,
                    targetSchoolYear = nextYear,
                    targetClasses,
                }
            );
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = ex.Message });
        }
    }

    [HttpPost]
    public async Task<IActionResult> PromoteBatch([FromBody] PromoteBatchViewModel model)
    {
        var redirect = CheckAdminRole();
        if (redirect != null)
            return Json(new { success = false, message = "Access denied" });

        if (model == null || model.StudentIds == null || !model.StudentIds.Any())
        {
            return Json(new { success = false, message = "Danh sách học sinh không hợp lệ." });
        }

        var classClient = classClientHelper();

        try
        {
            // Call Promote Batch API in ClassService.
            // Under the hood, ClassService publishes StudentPromotedEvent messages to RabbitMQ,
            // which are consumed by UserService to asynchronously update the profiles.
            var response = await classClient.PostAsJsonAsync(
                "student-classes/promote-batch",
                model
            );
            if (!response.IsSuccessStatusCode)
            {
                var errorObj = await response.Content.ReadFromJsonAsync<ErrorResponse>();
                return Json(
                    new
                    {
                        success = false,
                        message = errorObj?.Message
                            ?? "Lỗi xảy ra tại ClassService khi thực hiện lên lớp hàng loạt.",
                    }
                );
            }

            return Json(
                new
                {
                    success = true,
                    message = model.IsGraduating
                        ? "Lên lớp/Tốt nghiệp hàng loạt thành công!"
                        : "Lên lớp hàng loạt thành công!",
                }
            );
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = $"Lỗi hệ thống: {ex.Message}" });
        }
    }

    private HttpClient classClientHelper()
    {
        return _clientFactory.CreateClient("ClassService");
    }
}
