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
    public class SubjectsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: Subjects
        public ActionResult Index()
        {
            var subjects = db.Subjects
                .Include(s => s.User)
                .Include(s => s.Tasks)
                .Include(s => s.Grades);

            if (User.IsInRole("Admin"))
            {
                return View(subjects.ToList());
            }

            string currentUserId = User.Identity.GetUserId();
            return View(subjects.Where(s => s.UserId == currentUserId).ToList());
        }

        [HttpPost]
        public ActionResult UpdateGrade(int id, double value)
        {
            var subject = db.Subjects.Find(id);
            // Basic security check
            if (subject == null) return HttpNotFound();
            
            // Find existing grade or create new one
            var grade = db.Grades.FirstOrDefault(g => g.SubjectId == id);
            if (grade == null)
            {
                grade = new Grade
                {
                    SubjectId = id,
                    UserId = subject.UserId,
                    Value = value,
                    Date = DateTime.Now,
                    Type = "Final"
                };
                db.Grades.Add(grade);
            }
            else
            {
                grade.Value = value;
                grade.Date = DateTime.Now;
                db.Entry(grade).State = EntityState.Modified;
            }

            // If grade is passing, complete the subject
            if (value >= 6)
            {
                subject.IsCompleted = true;
                db.Entry(subject).State = EntityState.Modified;
            }

            db.SaveChanges();
            return Json(new { success = true });
        }

        [HttpPost]
        public ActionResult ToggleStatus(int id)
        {
            var subject = db.Subjects.Find(id);
            if (subject != null)
            {
                subject.IsCompleted = !subject.IsCompleted;
                db.SaveChanges();
            }
            return RedirectToAction("Index");
        }

        // GET: Subjects/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Subject subject = db.Subjects.Include(s => s.User).FirstOrDefault(s => s.Id == id);
            string currentUserId = User.Identity.GetUserId();

            if (subject == null || (!User.IsInRole("Admin") && subject.UserId != currentUserId))
            {
                return HttpNotFound();
            }
            return View(subject);
        }

        // GET: Subjects/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Subjects/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,Name,Year,Semester,IsCompleted,Credits,UserId")] Subject subject)
        {
            subject.UserId = User.Identity.GetUserId();
            if (ModelState.IsValid)
            {
                db.Subjects.Add(subject);
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(subject);
        }

        // GET: Subjects/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            Subject subject = db.Subjects.Find(id);
            string currentUserId = User.Identity.GetUserId();

            if (subject == null || (!User.IsInRole("Admin") && subject.UserId != currentUserId))
            {
                return HttpNotFound();
            }
            return View(subject);
        }

        // POST: Subjects/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,Name,Year,Semester,IsCompleted,Credits")] Subject subject)
        {
            if (!User.IsInRole("Admin") && subject.UserId != User.Identity.GetUserId())
            {
                return new HttpStatusCodeResult(HttpStatusCode.Forbidden);
            }

            if (ModelState.IsValid)
            {
                db.Entry(subject).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(subject);
        }

        // GET: Subjects/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Subject subject = db.Subjects.Include(s => s.User).FirstOrDefault(s => s.Id == id);
            if (subject == null || (!User.IsInRole("Admin") && subject.UserId != User.Identity.GetUserId()))
            {
                return HttpNotFound();
            }
            return View(subject);
        }

        // POST: Subjects/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Subject subject = db.Subjects.Find(id);
            if (subject != null && (User.IsInRole("Admin") || subject.UserId == User.Identity.GetUserId()))
            {
                db.Subjects.Remove(subject);
                db.SaveChanges();
            }
            return RedirectToAction("Index");
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
