using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using FrontendMVC.Models;
using Microsoft.AspNetCore.Mvc;

namespace FrontendMVC.Controllers;

public class StudentController : Controller
{
    private readonly IHttpClientFactory _clientFactory;

    public StudentController(IHttpClientFactory clientFactory)
    {
        _clientFactory = clientFactory;
    }

    private string? CheckStudentRole()
    {
        var role = Request.Cookies["user_role"];
        if (role != "Student")
        {
            return "Auth/AccessDenied";
        }
        return null;
    }

    [HttpGet]
    public async Task<IActionResult> Dashboard()
    {
        var redirect = CheckStudentRole();
        if (redirect != null)
            return RedirectToAction("AccessDenied", "Auth");

        var studentIdStr = Request.Cookies["user_id"];
        if (string.IsNullOrEmpty(studentIdStr) || !Guid.TryParse(studentIdStr, out var studentId))
        {
            return RedirectToAction("Logout", "Auth");
        }

        var userClient = _clientFactory.CreateClient("UserService");
        var classClient = _clientFactory.CreateClient("ClassService");
        var scoreClient = _clientFactory.CreateClient("ScoreService");

        UserViewModel? profile = null;

        // 1. Get profile
        try
        {
            profile = await userClient.GetFromJsonAsync<UserViewModel>($"users/{studentId}");
        }
        catch { }

        if (profile == null)
            return NotFound();

        var academicYears = new List<StudentAcademicYearViewModel>();

        // 2. Get student class history
        var historyList = new List<StudentClassViewModel>();
        try
        {
            var historyResp = await classClient.GetFromJsonAsync<
                IEnumerable<StudentClassViewModel>
            >($"student-classes/students/{studentId}/history");
            if (historyResp != null)
            {
                historyList = historyResp.ToList();
            }
        }
        catch { }

        // 3. Get all scores
        var allScores = new List<ScoreResponseViewModel>();
        try
        {
            var scoresResp = await scoreClient.GetFromJsonAsync<
                IEnumerable<ScoreResponseViewModel>
            >($"scores/student/{studentId}");
            if (scoresResp != null)
            {
                allScores = scoresResp.ToList();
            }
        }
        catch { }

        // 4. For each class in history, load schedule and filter scores
        foreach (var sc in historyList)
        {
            var schedules = new List<ScheduleViewModel>();
            try
            {
                var schedulesResp = await classClient.GetFromJsonAsync<
                    IEnumerable<ScheduleViewModel>
                >(
                    $"classes/{sc.ClassId}/schedule?schoolYear={Uri.EscapeDataString(sc.SchoolYear)}"
                );
                if (schedulesResp != null)
                {
                    schedules = schedulesResp.ToList();
                }
            }
            catch { }

            var yearScores = allScores.Where(s => s.SchoolYear == sc.SchoolYear).ToList();

            academicYears.Add(
                new StudentAcademicYearViewModel
                {
                    SchoolYear = sc.SchoolYear,
                    ClassId = sc.ClassId,
                    ClassName = sc.ClassName,
                    Schedules = schedules,
                    Scores = yearScores,
                    IsCurrent = sc.IsCurrent,
                }
            );
        }

        ViewBag.Profile = profile;
        ViewBag.AcademicYears = academicYears;

        return View();
    }
}
