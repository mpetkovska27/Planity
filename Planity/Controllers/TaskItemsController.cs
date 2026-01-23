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
    public class TaskItemsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: TaskItems
        public ActionResult Index()
        {
            string currentUserId = User.Identity.GetUserId();
            var taskItems = db.TaskItems
                .Where(t => t.UserId == currentUserId)
                .Include(t => t.Group)
                .Include(t => t.StudyPlan)
                .Include(t => t.Subject)
                .ToList();
            return View(taskItems);
        }

        // GET: TaskItems/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            string currentUserId = User.Identity.GetUserId();
            TaskItem taskItem = db.TaskItems.FirstOrDefault(t => t.Id == id && t.UserId == currentUserId);
            if (taskItem == null)
            {
                return HttpNotFound();
            }
            return View(taskItem);
        }

        // GET: TaskItems/Create
        public ActionResult Create()
        {
            string currentUserId = User.Identity.GetUserId();
            ViewBag.GroupId = new SelectList(db.Groups.Where(g => g.TeamLeaderId == currentUserId || g.Members.Any(m => m.UserId == currentUserId)), "Id", "Name");
            ViewBag.StudyPlanId = new SelectList(db.StudyPlans.Where(s => s.UserId == currentUserId), "Id", "Name");
            ViewBag.SubjectId = new SelectList(db.Subjects.Where(s => s.UserId == currentUserId), "Id", "Name");
            return View();
        }

        // POST: TaskItems/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,Title,Description,Type,Priority,Status,DueDate,PlanedHours,SubjectId,IsGroupTask,GroupId,StudyPlanId")] TaskItem taskItem)
        {
            taskItem.UserId = User.Identity.GetUserId();

            taskItem.UserId = User.Identity.GetUserId();

            if (ModelState.IsValid)
            {
                db.TaskItems.Add(taskItem);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.GroupId = new SelectList(db.Groups, "Id", "Name", taskItem.GroupId);
            ViewBag.StudyPlanId = new SelectList(db.StudyPlans.Where(s => s.UserId == taskItem.UserId), "Id", "Name", taskItem.StudyPlanId);
            ViewBag.SubjectId = new SelectList(db.Subjects.Where(s => s.UserId == taskItem.UserId), "Id", "Name", taskItem.SubjectId);
            return View(taskItem);
        }

        // GET: TaskItems/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            string currentUserId = User.Identity.GetUserId();
            TaskItem taskItem = db.TaskItems.FirstOrDefault(t => t.Id == id && t.UserId == currentUserId);
            if (taskItem == null)
            {
                return HttpNotFound();
            }
            ViewBag.GroupId = new SelectList(db.Groups, "Id", "Name", taskItem.GroupId);
            ViewBag.StudyPlanId = new SelectList(db.StudyPlans.Where(s => s.UserId == currentUserId), "Id", "Name", taskItem.StudyPlanId);
            ViewBag.SubjectId = new SelectList(db.Subjects.Where(s => s.UserId == currentUserId), "Id", "Name", taskItem.SubjectId);
            return View(taskItem);
        }

        // POST: TaskItems/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,Title,Description,Type,Priority,Status,DueDate,PlanedHours,SubjectId,IsGroupTask,GroupId,StudyPlanId")] TaskItem taskItem)
        {
            taskItem.UserId = User.Identity.GetUserId();

            if (ModelState.IsValid)
            {
                db.Entry(taskItem).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(taskItem);
        }

        // GET: TaskItems/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            string currentUserId = User.Identity.GetUserId();
            TaskItem taskItem = db.TaskItems.FirstOrDefault(t => t.Id == id && t.UserId == currentUserId);
            if (taskItem == null)
            {
                return HttpNotFound();
            }
            return View(taskItem);
        }

        // POST: TaskItems/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            string currentUserId = User.Identity.GetUserId();
            TaskItem taskItem = db.TaskItems.FirstOrDefault(t => t.Id == id && t.UserId == currentUserId);
            if (taskItem != null)
            {
                db.TaskItems.Remove(taskItem);
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
