using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using FrontendMVC.Models;
using Microsoft.AspNetCore.Mvc;

namespace FrontendMVC.Controllers;

public class TeacherController : Controller
{
    private readonly IHttpClientFactory _clientFactory;

    public TeacherController(IHttpClientFactory clientFactory)
    {
        _clientFactory = clientFactory;
    }

    private string? CheckTeacherRole()
    {
        var role = Request.Cookies["user_role"];
        if (role != "Teacher")
        {
            return "Auth/AccessDenied";
        }
        return null;
    }

    [HttpGet]
    public async Task<IActionResult> Dashboard()
    {
        var redirect = CheckTeacherRole();
        if (redirect != null)
            return RedirectToAction("AccessDenied", "Auth");

        var teacherIdStr = Request.Cookies["user_id"];
        if (string.IsNullOrEmpty(teacherIdStr) || !Guid.TryParse(teacherIdStr, out var teacherId))
        {
            return RedirectToAction("Logout", "Auth");
        }

        var classClient = _clientFactory.CreateClient("ClassService");
        var assignments = new List<TeachingAssignmentViewModel>();
        try
        {
            var response = await classClient.GetFromJsonAsync<
                IEnumerable<TeachingAssignmentViewModel>
            >($"teachers/{teacherId}/classes");
            if (response != null)
            {
                assignments = response.ToList();
            }
        }
        catch { }

        return View(assignments);
    }

    [HttpGet]
    public async Task<IActionResult> ClassStudents(Guid classId, Guid subjectId)
    {
        var redirect = CheckTeacherRole();
        if (redirect != null)
            return RedirectToAction("AccessDenied", "Auth");

        var classClient = _clientFactory.CreateClient("ClassService");
        var scoreClient = _clientFactory.CreateClient("ScoreService");

        ClassViewModel? classInfo = null;
        var students = new List<StudentClassViewModel>();
        var subjectName = "Môn học";

        try
        {
            classInfo = await classClient.GetFromJsonAsync<ClassViewModel>($"classes/{classId}");
        }
        catch { }
        if (classInfo == null)
            return NotFound();

        try
        {
            var studentsResp = await classClient.GetFromJsonAsync<
                IEnumerable<StudentClassViewModel>
            >($"student-classes/classes/{classId}/students");
            if (studentsResp != null)
                students = studentsResp.ToList();
        }
        catch { }

        // Tìm tên môn học từ cached subjects
        try
        {
            var subResp = await classClient.GetFromJsonAsync<IEnumerable<dynamic>>(
                $"classes/{classId}/schedule?schoolYear={Uri.EscapeDataString(classInfo.SchoolYear)}"
            );
            if (subResp != null)
            {
                var sch = subResp.FirstOrDefault(s =>
                    s.subjectId.ToString() == subjectId.ToString()
                );
                if (sch != null)
                {
                    subjectName = sch.subjectName;
                }
            }
        }
        catch { }

        // Lấy điểm số của từng học sinh cho môn học này
        var studentScores = new Dictionary<Guid, List<ScoreResponseViewModel>>();
        foreach (var student in students)
        {
            var scoresList = new List<ScoreResponseViewModel>();
            try
            {
                var scoresResp = await scoreClient.GetFromJsonAsync<
                    IEnumerable<ScoreResponseViewModel>
                >($"scores/student/{student.StudentId}");
                if (scoresResp != null)
                {
                    scoresList = scoresResp.Where(s => s.SubjectId == subjectId).ToList();
                }
            }
            catch { }
            studentScores[student.StudentId] = scoresList;
        }

        ViewBag.ClassInfo = classInfo;
        ViewBag.SubjectId = subjectId;
        ViewBag.SubjectName = subjectName;
        ViewBag.StudentScores = studentScores;

        return View(students);
    }

    [HttpPost]
    public async Task<IActionResult> AddScore(
        Guid classId,
        Guid subjectId,
        CreateScoreRequestViewModel model
    )
    {
        var redirect = CheckTeacherRole();
        if (redirect != null)
            return RedirectToAction("AccessDenied", "Auth");

        var scoreClient = _clientFactory.CreateClient("ScoreService");
        try
        {
            model.SubjectId = subjectId;
            var response = await scoreClient.PostAsJsonAsync("scores", model);
            if (response.IsSuccessStatusCode)
            {
                TempData["SuccessMessage"] = "Nhập điểm mới thành công!";
            }
            else
            {
                var errorObj = await response.Content.ReadFromJsonAsync<ErrorResponse>();
                TempData["ErrorMessage"] =
                    errorObj?.Message ?? "Không thể nhập điểm. Vui lòng kiểm tra lại.";
            }
        }
        catch
        {
            TempData["ErrorMessage"] = "Lỗi kết nối đến ScoreService.";
        }

        return RedirectToAction("ClassStudents", new { classId, subjectId });
    }

    [HttpPost]
    public async Task<IActionResult> EditScore(
        Guid classId,
        Guid subjectId,
        Guid scoreId,
        UpdateScoreRequestViewModel model
    )
    {
        var redirect = CheckTeacherRole();
        if (redirect != null)
            return RedirectToAction("AccessDenied", "Auth");

        var scoreClient = _clientFactory.CreateClient("ScoreService");
        try
        {
            var response = await scoreClient.HttpPutAsJsonAsync($"scores/{scoreId}", model);
            if (response.IsSuccessStatusCode)
            {
                TempData["SuccessMessage"] = "Cập nhật điểm thành công!";
            }
            else
            {
                TempData["ErrorMessage"] = "Không thể cập nhật điểm số.";
            }
        }
        catch
        {
            TempData["ErrorMessage"] = "Lỗi kết nối đến ScoreService.";
        }

        return RedirectToAction("ClassStudents", new { classId, subjectId });
    }

    [HttpPost]
    public async Task<IActionResult> DeleteScore(Guid classId, Guid subjectId, Guid scoreId)
    {
        var redirect = CheckTeacherRole();
        if (redirect != null)
            return RedirectToAction("AccessDenied", "Auth");

        var scoreClient = _clientFactory.CreateClient("ScoreService");
        try
        {
            var response = await scoreClient.DeleteAsync($"scores/{scoreId}");
            if (response.IsSuccessStatusCode)
            {
                TempData["SuccessMessage"] = "Đã xoá điểm số thành công!";
            }
            else
            {
                TempData["ErrorMessage"] = "Không thể xoá điểm số.";
            }
        }
        catch
        {
            TempData["ErrorMessage"] = "Lỗi kết nối đến ScoreService.";
        }

        return RedirectToAction("ClassStudents", new { classId, subjectId });
    }

    [HttpGet]
    public async Task<IActionResult> Profile()
    {
        var redirect = CheckTeacherRole();
        if (redirect != null)
            return RedirectToAction("AccessDenied", "Auth");

        var teacherIdStr = Request.Cookies["user_id"];
        if (string.IsNullOrEmpty(teacherIdStr) || !Guid.TryParse(teacherIdStr, out var teacherId))
        {
            return RedirectToAction("Logout", "Auth");
        }

        var userClient = _clientFactory.CreateClient("UserService");
        var classClient = _clientFactory.CreateClient("ClassService");

        UserViewModel? profile = null;
        try
        {
            profile = await userClient.GetFromJsonAsync<UserViewModel>($"users/{teacherId}");
        }
        catch { }

        if (profile == null)
            return NotFound();

        ClassViewModel? homeroomClass = null;
        try
        {
            var hrResp = await classClient.GetFromJsonAsync<
                IEnumerable<HomeroomAssignmentViewModel>
            >("classes/homerooms");
            if (hrResp != null)
            {
                var hr = hrResp.FirstOrDefault(h => h.TeacherId == teacherId);
                if (hr != null)
                {
                    homeroomClass = new ClassViewModel
                    {
                        Id = hr.ClassId,
                        Name = hr.ClassName,
                        SchoolYear = hr.SchoolYear
                    };
                }
            }
        }
        catch { }
        ViewBag.HomeroomClass = homeroomClass;

        var assignments = new List<TeachingAssignmentViewModel>();
        try
        {
            var response = await classClient.GetFromJsonAsync<
                IEnumerable<TeachingAssignmentViewModel>
            >($"teachers/{teacherId}/classes");
            if (response != null)
            {
                assignments = response.ToList();
            }
        }
        catch { }
        ViewBag.TeachingAssignments = assignments;

        return View(profile);
    }
}

public static class HttpClientExtensions
{
    public static Task<HttpResponseMessage> HttpPutAsJsonAsync<T>(
        this HttpClient client,
        string requestUri,
        T value
    )
    {
        return client.PutAsJsonAsync(requestUri, value);
    }
}
