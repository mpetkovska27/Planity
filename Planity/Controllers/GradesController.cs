using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Planity.Models;

namespace Planity.Controllers
{
    [Authorize(Roles = "Student,TimLeader,Admin")]
    public class GradesController : Controller
    {
        private readonly ApplicationDbContext db = new ApplicationDbContext();

        public ActionResult Index()
        {
            var userId = User.IsInRole("Admin") ? null : User.Identity.GetUserId();
            var q = db.Grades.Include(g => g.Subject).AsQueryable();
            if (userId != null) q = q.Where(g => g.UserId == userId);
            return View(q.ToList());
        }

        public ActionResult Details(int? id)
        {
            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            var g = db.Grades.Include(x => x.Subject).FirstOrDefault(x => x.Id == id);
            if (g == null) return HttpNotFound();
            if (!User.IsInRole("Admin") && g.UserId != User.Identity.GetUserId())
                return new HttpStatusCodeResult(HttpStatusCode.Forbidden);
            return View(g);
        }

        public ActionResult Create()
        {
            var userId = User.Identity.GetUserId();
            var subjects = db.Subjects.Where(s => s.UserId == userId).ToList();
            var users = db.Users.ToList();
            
            ViewBag.SubjectId = new SelectList(subjects, "Id", "Name");
            ViewBag.UserId = new SelectList(users, "Id", "UserName");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Grade grade)
        {
            if (!ModelState.IsValid) 
            {
                var userId = User.Identity.GetUserId();
                var subjects = db.Subjects.Where(s => s.UserId == userId).ToList();
                var users = db.Users.ToList();
                
                ViewBag.SubjectId = new SelectList(subjects, "Id", "Name", grade.SubjectId);
                ViewBag.UserId = new SelectList(users, "Id", "UserName", grade.UserId);
                return View(grade);
            }

            if (!User.IsInRole("Admin"))
                grade.UserId = User.Identity.GetUserId();

            db.Grades.Add(grade);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        public ActionResult Edit(int? id)
        {
            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            var g = db.Grades.Find(id);
            if (g == null) return HttpNotFound();
            if (!User.IsInRole("Admin") && g.UserId != User.Identity.GetUserId())
                return new HttpStatusCodeResult(HttpStatusCode.Forbidden);
            
            var userId = User.Identity.GetUserId();
            var subjects = db.Subjects.Where(s => s.UserId == userId).ToList();
            var users = db.Users.ToList();
            
            ViewBag.SubjectId = new SelectList(subjects, "Id", "Name", g.SubjectId);
            ViewBag.UserId = new SelectList(users, "Id", "UserName", g.UserId);
            return View(g);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Grade model)
        {
            var g = db.Grades.Find(model.Id);
            if (g == null) return HttpNotFound();
            if (!User.IsInRole("Admin") && g.UserId != User.Identity.GetUserId())
                return new HttpStatusCodeResult(HttpStatusCode.Forbidden);

            if (!ModelState.IsValid) 
            {
                var userId = User.Identity.GetUserId();
                var subjects = db.Subjects.Where(s => s.UserId == userId).ToList();
                var users = db.Users.ToList();
                
                ViewBag.SubjectId = new SelectList(subjects, "Id", "Name", model.SubjectId);
                ViewBag.UserId = new SelectList(users, "Id", "UserName", model.UserId);
                return View(model);
            }

            g.Value = model.Value;
            g.Date = model.Date;
            g.Type = model.Type;
            g.SubjectId = model.SubjectId;

            db.SaveChanges();
            return RedirectToAction("Index");
        }

        public ActionResult Delete(int? id)
        {
            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            var g = db.Grades.Find(id);
            if (g == null) return HttpNotFound();
            if (!User.IsInRole("Admin") && g.UserId != User.Identity.GetUserId())
                return new HttpStatusCodeResult(HttpStatusCode.Forbidden);
            return View(g);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            var g = db.Grades.Find(id);
            if (g == null) return HttpNotFound();
            db.Grades.Remove(g);
            db.SaveChanges();
            return RedirectToAction("Index");
        }
    }
}