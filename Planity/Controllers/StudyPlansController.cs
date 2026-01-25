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
    public class StudyPlansController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: StudyPlans
        public ActionResult Index()
        {
            string currentUserId = User.Identity.GetUserId();

            if (User.IsInRole("Admin"))
            {
                var allPlans = db.StudyPlans
                    .Include(s => s.User)
                    .OrderBy(s => s.StartDate)
                    .ToList();
                return View(allPlans);
            }

            var studyPlans = db.StudyPlans
                .Where(s => s.UserId == currentUserId)
                .OrderBy(s => s.StartDate)
                .ToList();
            return View(studyPlans);
        }

        // GET: StudyPlans/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            string currentUserId = User.Identity.GetUserId();

            StudyPlan studyPlan = db.StudyPlans
                .Include(s => s.Tasks)
                .Include(s => s.User) 
                .FirstOrDefault(s => s.Id == id);

            if (studyPlan == null || (!User.IsInRole("Admin") && studyPlan.UserId != currentUserId))
            {
                return HttpNotFound();
            }
            return View(studyPlan);
        }

        // GET: StudyPlans/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: StudyPlans/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,Name,StartDate,EndDate,UserId")] StudyPlan studyPlan)
        {
            studyPlan.UserId = User.Identity.GetUserId();

            if (ModelState.IsValid)
            {
                db.StudyPlans.Add(studyPlan);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(studyPlan);
        }

        // GET: StudyPlans/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            string currentUserId = User.Identity.GetUserId();
            StudyPlan studyPlan = db.StudyPlans.Find(id);

            if (studyPlan == null || (!User.IsInRole("Admin") && studyPlan.UserId != currentUserId))
            {
                return HttpNotFound();
            }
            return View(studyPlan);
        }

        // POST: StudyPlans/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,Name,StartDate,EndDate")] StudyPlan studyPlan)
        {
            string currentUserId = User.Identity.GetUserId();
            
            if (!User.IsInRole("Admin") && studyPlan.UserId != currentUserId)
            {
                return new HttpStatusCodeResult(HttpStatusCode.Forbidden);
            }

            if (ModelState.IsValid)
            {
                db.Entry(studyPlan).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(studyPlan);
        }

        // GET: StudyPlans/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            string currentUserId = User.Identity.GetUserId();
            StudyPlan studyPlan = db.StudyPlans.Include(s => s.User).FirstOrDefault(s => s.Id == id);

            if (studyPlan == null || (!User.IsInRole("Admin") && studyPlan.UserId != currentUserId))
            {
                return HttpNotFound();
            }
            return View(studyPlan);
        }

        // POST: StudyPlans/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            string currentUserId = User.Identity.GetUserId();
            StudyPlan studyPlan = db.StudyPlans.Find(id);

            if (studyPlan != null && (studyPlan.UserId == currentUserId || User.IsInRole("Admin")))
            {
                var tasksInPlan = db.TaskItems.Where(t => t.StudyPlanId == id);
                foreach (var task in tasksInPlan)
                {
                    task.StudyPlanId = null;
                }

                db.StudyPlans.Remove(studyPlan);
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
