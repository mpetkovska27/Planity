using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Planity.Models;

namespace Planity.Controllers
{
    [Authorize(Roles = "Student,TimLeader,Admin")]
    public class SubjectsController : Controller
    {
        private readonly ApplicationDbContext db = new ApplicationDbContext();

        public ActionResult Index()
        {
            var userId = User.IsInRole("Admin") ? null : User.Identity.GetUserId();
            var q = db.Subjects.AsQueryable();
            if (userId != null) q = q.Where(s => s.UserId == userId);
            return View(q.ToList());
        }

        public ActionResult Details(int? id)
        {
            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            var subject = db.Subjects.Find(id);
            if (subject == null) return HttpNotFound();
            if (!User.IsInRole("Admin") && subject.UserId != User.Identity.GetUserId())
                return new HttpStatusCodeResult(HttpStatusCode.Forbidden);
            return View(subject);
        }

        public ActionResult Create()
        {
            var users = db.Users.ToList();
            ViewBag.UserId = new SelectList(users, "Id", "UserName");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Subject subject)
        {
            if (ModelState.IsValid)
            {
                if (!User.IsInRole("Admin"))
                    subject.UserId = User.Identity.GetUserId();
                db.Subjects.Add(subject);
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            
            var users = db.Users.ToList();
            ViewBag.UserId = new SelectList(users, "Id", "UserName", subject.UserId);
            return View(subject);
        }

        public ActionResult Edit(int? id)
        {
            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            var subject = db.Subjects.Find(id);
            if (subject == null) return HttpNotFound();
            if (!User.IsInRole("Admin") && subject.UserId != User.Identity.GetUserId())
                return new HttpStatusCodeResult(HttpStatusCode.Forbidden);
            
            var users = db.Users.ToList();
            ViewBag.UserId = new SelectList(users, "Id", "UserName", subject.UserId);
            return View(subject);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Subject model)
        {
            var subject = db.Subjects.Find(model.Id);
            if (subject == null) return HttpNotFound();
            if (!User.IsInRole("Admin") && subject.UserId != User.Identity.GetUserId())
                return new HttpStatusCodeResult(HttpStatusCode.Forbidden);

            if (ModelState.IsValid)
            {
                subject.Name = model.Name;
                subject.Year = model.Year;
                subject.Semester = model.Semester;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            
            var users = db.Users.ToList();
            ViewBag.UserId = new SelectList(users, "Id", "UserName", model.UserId);
            return View(model);
        }

        public ActionResult Delete(int? id)
        {
            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            var subject = db.Subjects.Find(id);
            if (subject == null) return HttpNotFound();
            if (!User.IsInRole("Admin") && subject.UserId != User.Identity.GetUserId())
                return new HttpStatusCodeResult(HttpStatusCode.Forbidden);
            return View(subject);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            var subject = db.Subjects.Find(id);
            if (subject == null) return HttpNotFound();
            if (!User.IsInRole("Admin") && subject.UserId != User.Identity.GetUserId())
                return new HttpStatusCodeResult(HttpStatusCode.Forbidden);

            db.Subjects.Remove(subject);
            db.SaveChanges();
            return RedirectToAction("Index");
        }
    }
}