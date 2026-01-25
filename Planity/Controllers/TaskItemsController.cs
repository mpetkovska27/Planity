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
    public class TaskItemsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: TaskItems
        public ActionResult Index()
        {
            string currentUserId = User.Identity.GetUserId();
            var query = db.TaskItems
                        .Include(t => t.Group)
                        .Include(t => t.StudyPlan)
                        .Include(t => t.Subject)
                        .Include(t => t.User);
            //za admin
            if (User.IsInRole("Admin"))
            {
                return View(query.ToList());
            }
            //za teamleader - gleda svoi zadaci i kade sto e lider 
            if (User.IsInRole("TimLeader"))
            {
                var tlTasks = query.Where(t => t.UserId == currentUserId || (t.IsGroupTask && t.Group.TeamLeaderId == currentUserId)).ToList();
                return View(tlTasks);
            }
            //za student gleda samo svoi i grupni od grupi koi e clen
            var studentTasks = query.Where(t => t.UserId == currentUserId || (t.IsGroupTask && t.Group.Members.Any(m => m.UserId == currentUserId))).ToList();
            return View(studentTasks);
        }

        // GET: TaskItems/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            string currentUserId = User.Identity.GetUserId();
            var taskItem = db.TaskItems.Include(t => t.Group.Members).FirstOrDefault(t => t.Id == id);

            if (taskItem == null)
            {
                return HttpNotFound();
            }
            bool isAdmin = User.IsInRole("Admin");
            bool isOwner = taskItem.UserId == currentUserId;
            bool isMember = taskItem.IsGroupTask && taskItem.Group.Members.Any(m => m.UserId == currentUserId);
            bool isLeader = taskItem.IsGroupTask && taskItem.Group.TeamLeaderId == currentUserId;

            if (!isAdmin && !isOwner && !isMember && !isLeader) 
            { 
                return new HttpStatusCodeResult(HttpStatusCode.Forbidden); 
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
        public ActionResult Create([Bind(Include = "Id,Title,Description,Type,Priority,Status,DueDate,PlanedHours,SubjectId,IsGroupTask,GroupId,StudyPlanId,UserId")] TaskItem taskItem)
        {
            if (string.IsNullOrEmpty(taskItem.UserId)) taskItem.UserId = User.Identity.GetUserId();

            if (ModelState.IsValid)
            {
                db.TaskItems.Add(taskItem);
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(taskItem);
        }

        // GET: TaskItems/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            TaskItem taskItem = db.TaskItems.Find(id);
            string currentUserId = User.Identity.GetUserId();

            if (taskItem == null || (!User.IsInRole("Admin") && taskItem.UserId != currentUserId))
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
            if (!User.IsInRole("Admin") && taskItem.UserId != User.Identity.GetUserId()) 
            {
                return new HttpStatusCodeResult(HttpStatusCode.Forbidden);
            }

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
            TaskItem taskItem = db.TaskItems.FirstOrDefault(t => t.Id == id);
            if (taskItem == null || (!User.IsInRole("Admin") && taskItem.UserId != User.Identity.GetUserId())) 
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
            TaskItem taskItem = db.TaskItems.Find(id);
            if (taskItem != null && (User.IsInRole("Admin") || taskItem.UserId == User.Identity.GetUserId()))
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
