using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
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

            var myTasks = db.TaskItems.Where(t =>
                t.UserId == userId ||
                (t.IsGroupTask && t.Group.Members.Any(m => m.UserId == userId)));

            var total = myTasks.Count();
            var finished = myTasks.Count(t => t.IsCompleted);
            var inProgress = myTasks.Count(t => !t.IsCompleted && t.DueDate >= DateTime.Now);
            var overdue = myTasks.Count(t => !t.IsCompleted && t.DueDate < DateTime.Now);

            ViewBag.TotalTasks = total;
            ViewBag.FinishedTasks = finished;
            ViewBag.InProgressTasks = inProgress;
            ViewBag.OverdueTasks = overdue;

            return View();
        }
    }
}