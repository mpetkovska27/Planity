using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.Entity;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Planity.Models;


namespace Planity.Controllers
{
    [Authorize(Roles = "Student,TimLeader,Admin")]
    public class DashboardController : Controller
    {
        private readonly ApplicationDbContext db = new ApplicationDbContext();

        public ActionResult Index()
        {
            var userId = User.Identity.GetUserId();

            var myTasksQuery = db.TaskItems
                .Include(t => t.Subject)
                .Where(t =>
                    t.UserId == userId ||
                    (t.IsGroupTask && t.Group.Members.Any(m => m.UserId == userId)));

            var myTasks = myTasksQuery.ToList();
            var now = DateTime.Now;
            var today = now.Date;

            var total = myTasks.Count;
            var finished = myTasks.Count(t => t.Status == TaskStatus.Done);
            var inProgress = myTasks.Count(t => t.Status == TaskStatus.InProgress && t.DueDate >= now);
            var overdue = myTasks.Count(t => t.Status == TaskStatus.Overdue || (t.Status != TaskStatus.Done && t.DueDate < now));


            var myDayTasks = myTasks
                .Where(t => t.DueDate.HasValue && t.DueDate.Value.Date == today && t.Status != TaskStatus.Done)
                .OrderBy(t => t.DueDate)
                .ToList();

            var upcomingDeadlines = myTasks
                .Where(t => t.DueDate.HasValue && t.Status != TaskStatus.Done)
                .OrderBy(t => t.DueDate)
                .Take(3)
                .ToList();

            var calendarEvents = myTasks
                .Where(t => t.DueDate.HasValue)
                .Select(t => new
                {
                    title = t.Title,
                    start = t.DueDate.Value.ToString("yyyy-MM-dd")
                })
                .ToList();

            var gpaSubjects = db.Subjects
                .Include(s => s.Grades)
                .Where(s => s.UserId == userId)
                .OrderBy(s => s.Name)
                .ToList();

            ViewBag.TotalTasks = total;
            ViewBag.FinishedTasks = finished;
            ViewBag.InProgressTasks = inProgress;
            ViewBag.OverdueTasks = overdue;
            var weeklySubjectsQuery = db.Subjects
                .Where(s => s.ScheduleDay.HasValue);

            if (!User.IsInRole("Admin"))
            {
                weeklySubjectsQuery = weeklySubjectsQuery.Where(s => s.UserId == userId);
            }

            var weeklySubjects = weeklySubjectsQuery
                .OrderBy(s => s.ScheduleDay)
                .ThenBy(s => s.ScheduleTimeSlot)
                .ToList();

            var allSubjectsQuery = db.Subjects.AsQueryable();
            if (!User.IsInRole("Admin"))
            {
                allSubjectsQuery = allSubjectsQuery.Where(s => s.UserId == userId);
            }

            var allSubjects = allSubjectsQuery
                .OrderBy(s => s.Year)
                .ThenBy(s => s.Semester)
                .ThenBy(s => s.Name)
                .ToList();

            ViewBag.WeeklySubjects = weeklySubjects;
            ViewBag.AllSubjects = allSubjects;
            ViewBag.MyDayTasks = myDayTasks;
            ViewBag.UpcomingDeadlines = upcomingDeadlines;
            ViewBag.CalendarEvents = calendarEvents;
            ViewBag.GpaSubjects = gpaSubjects;

            return View();
        }
    }
}