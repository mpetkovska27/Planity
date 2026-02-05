using Microsoft.AspNet.Identity;
using Planity.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace Planity.Controllers
{
    [Authorize]
    public class GradesController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: Grades
        public ActionResult Index()
        {
            return RedirectToAction("Index", "Progress");
        }

        // GET: Grades/Details/5
        public ActionResult Details(int? id)
        {
            return RedirectToAction("Index", "Progress");
        }

        // GET: Grades/Create
        public ActionResult Create()
        {
            return RedirectToAction("Index", "Progress");
        }

        // POST: Grades/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,Value,Date,Type,SubjectId")] Grade grade)
        {
            return new HttpStatusCodeResult(HttpStatusCode.Gone, "Grades CRUD is now inline on Subjects.");
        }

        // GET: Grades/Edit/5
        public ActionResult Edit(int? id)
        {
            return RedirectToAction("Index", "Progress");
        }

        // POST: Grades/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,Value,Date,Type,SubjectId,UserId")] Grade grade)
        {
            return new HttpStatusCodeResult(HttpStatusCode.Gone, "Grades CRUD is now inline on Subjects.");
        }

        // GET: Grades/Delete/5
        public ActionResult Delete(int? id)
        {
            return RedirectToAction("Index", "Progress");
        }

        // POST: Grades/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            return new HttpStatusCodeResult(HttpStatusCode.Gone, "Grades CRUD is now inline on Subjects.");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AddGradeEntry(int subjectId, string type, double value)
        {
            var subject = db.Subjects.Find(subjectId);
            if (subject == null) return Json(new { success = false, message = "Subject not found" });

            if (!User.IsInRole("Admin") && subject.UserId != User.Identity.GetUserId())
            {
                return new HttpStatusCodeResult(HttpStatusCode.Forbidden);
            }

            if (string.IsNullOrWhiteSpace(type) || value < 1 || value > 10)
            {
                return Json(new { success = false, message = "Invalid grade data" });
            }

            var grade = new Grade
            {
                SubjectId = subjectId,
                UserId = subject.UserId,
                Value = value,
                Date = DateTime.Now,
                Type = type.Trim()
            };

            db.Grades.Add(grade);
            db.SaveChanges();

            return Json(new
            {
                success = true,
                id = grade.Id,
                type = grade.Type,
                value = grade.Value,
                date = grade.Date.ToString("yyyy-MM-dd")
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult UpdateGradeEntry(int id, string type, double value)
        {
            var grade = db.Grades.Include(g => g.Subject).FirstOrDefault(g => g.Id == id);
            if (grade == null) return Json(new { success = false, message = "Grade not found" });

            if (!User.IsInRole("Admin") && grade.UserId != User.Identity.GetUserId())
            {
                return new HttpStatusCodeResult(HttpStatusCode.Forbidden);
            }

            if (string.IsNullOrWhiteSpace(type) || value < 1 || value > 10)
            {
                return Json(new { success = false, message = "Invalid grade data" });
            }

            grade.Type = type.Trim();
            grade.Value = value;
            // Removed automatic Date = DateTime.Now to preserve original entry date
            db.SaveChanges();

            return Json(new
            {
                success = true,
                type = grade.Type,
                value = grade.Value,
                date = grade.Date.ToString("yyyy-MM-dd")
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteGradeEntry(int id)
        {
            var grade = db.Grades.FirstOrDefault(g => g.Id == id);
            if (grade == null) return Json(new { success = false, message = "Grade not found" });

            if (!User.IsInRole("Admin") && grade.UserId != User.Identity.GetUserId())
            {
                return new HttpStatusCodeResult(HttpStatusCode.Forbidden);
            }

            db.Grades.Remove(grade);
            db.SaveChanges();

            return Json(new { success = true });
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}