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
    public class ProgressController : Controller
    {
        private readonly ApplicationDbContext db = new ApplicationDbContext();

        public ActionResult Index()
        {
            var userId = User.Identity.GetUserId();
            var grades = db.Grades.Where(g => g.UserId == userId).ToList();

            var passedSubjects = grades.Where(g => g.Value >= 6.0).Select(g => g.SubjectId).Distinct().Count();
            var totalSubjects = db.Subjects.Count(s => s.UserId == userId);
            var remaining = totalSubjects - passedSubjects;

            ViewBag.Passed = passedSubjects;
            ViewBag.TotalSubjects = totalSubjects;
            ViewBag.Remaining = remaining;

            return View(grades);
        }
    }
}