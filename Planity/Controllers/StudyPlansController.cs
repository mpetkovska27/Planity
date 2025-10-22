using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Planity.Models;
using Microsoft.AspNet.Identity;

namespace Planity.Controllers
{
    [Authorize(Roles = "Student,TimLeader,Admin")]
    public class StudyPlansController : Controller
    {
        private readonly ApplicationDbContext db = new ApplicationDbContext();

        public ActionResult Index()
        {
            var userId = User.IsInRole("Admin") ? null : User.Identity.GetUserId();
            var q = db.StudyPlans.AsQueryable();
            if (userId != null) q = q.Where(s => s.UserId == userId);
            return View(q.ToList());
        }

        public ActionResult Details(int? id)
        {
            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            var sp = db.StudyPlans.Find(id);
            if (sp == null) return HttpNotFound();
            if (!User.IsInRole("Admin") && sp.UserId != User.Identity.GetUserId())
                return new HttpStatusCodeResult(HttpStatusCode.Forbidden);
            return View(sp);
        }

        public ActionResult Create()
        {
            var users = db.Users.ToList();
            ViewBag.UserId = new SelectList(users, "Id", "UserName");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(StudyPlan model)
        {
            if (!ModelState.IsValid) 
            {
                var users = db.Users.ToList();
                ViewBag.UserId = new SelectList(users, "Id", "UserName", model.UserId);
                return View(model);
            }
            model.UserId = User.Identity.GetUserId();
            db.StudyPlans.Add(model);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        public ActionResult Edit(int? id)
        {
            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            var sp = db.StudyPlans.Find(id);
            if (sp == null) return HttpNotFound();
            if (!User.IsInRole("Admin") && sp.UserId != User.Identity.GetUserId())
                return new HttpStatusCodeResult(HttpStatusCode.Forbidden);
            
            var users = db.Users.ToList();
            ViewBag.UserId = new SelectList(users, "Id", "UserName", sp.UserId);
            return View(sp);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(StudyPlan model)
        {
            var sp = db.StudyPlans.Find(model.Id);
            if (sp == null) return HttpNotFound();
            if (!User.IsInRole("Admin") && sp.UserId != User.Identity.GetUserId())
                return new HttpStatusCodeResult(HttpStatusCode.Forbidden);

            if (!ModelState.IsValid) 
            {
                var users = db.Users.ToList();
                ViewBag.UserId = new SelectList(users, "Id", "UserName", model.UserId);
                return View(model);
            }

            sp.Name = model.Name;
            sp.StartDate = model.StartDate;
            sp.EndDate = model.EndDate;
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        public ActionResult Delete(int? id)
        {
            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            var sp = db.StudyPlans.Find(id);
            if (sp == null) return HttpNotFound();
            if (!User.IsInRole("Admin") && sp.UserId != User.Identity.GetUserId())
                return new HttpStatusCodeResult(HttpStatusCode.Forbidden);
            return View(sp);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            var sp = db.StudyPlans.Find(id);
            if (sp == null) return HttpNotFound();
            db.StudyPlans.Remove(sp);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        // Study plan generator UI (logic later)
        public ActionResult Generator()
        {
            return View();
        }
    }
}