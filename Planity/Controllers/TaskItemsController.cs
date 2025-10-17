using System;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Planity.Models;

namespace Planity.Controllers
{
    [Authorize(Roles = "Student,TimLeader,Admin")]
    public class TaskItemsController : Controller
    {
        private readonly ApplicationDbContext db = new ApplicationDbContext();

        public ActionResult Index()
        {
            var userId = User.Identity.GetUserId();
            var q = db.TaskItems
                .Include(t => t.Subject)
                .Include(t => t.Group)
                .Include(t => t.Group.Members);

            if (!User.IsInRole("Admin"))
            {
                q = q.Where(t =>
                    t.UserId == userId ||
                    (t.IsGroupTask && t.Group != null && t.Group.Members.Any(m => m.UserId == userId)));
            }
            return View(q.ToList());
        }

        public ActionResult Details(int? id)
        {
            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            var task = db.TaskItems.Include(t => t.Group).Include(t => t.Group.Members).FirstOrDefault(t => t.Id == id);
            if (task == null) return HttpNotFound();
            if (!User.IsInRole("Admin"))
            {
                var userId = User.Identity.GetUserId();
                var canView = task.UserId == userId || (task.IsGroupTask && task.Group != null && task.Group.Members.Any(m => m.UserId == userId));
                if (!canView) return new HttpStatusCodeResult(HttpStatusCode.Forbidden);
            }
            return View(task);
        }

        public ActionResult Create()
        {
            var users = db.Users.ToList();
            ViewBag.UserId = new SelectList(users, "Id", "UserName");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(TaskItem model)
        {
            if (!ModelState.IsValid) return View(model);

            var userId = User.Identity.GetUserId();

            if (!model.IsGroupTask)
            {
                model.UserId = userId;
            }
            else if (!User.IsInRole("Admin"))
            {
                var group = db.Groups.FirstOrDefault(g => g.Id == model.GroupId);
                if (group == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                if (group.TeamLeaderId != userId)
                    return new HttpStatusCodeResult(HttpStatusCode.Forbidden);
            }

            db.TaskItems.Add(model);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        public ActionResult Edit(int? id)
        {
            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            var task = db.TaskItems.Include(t => t.Group).FirstOrDefault(t => t.Id == id);
            if (task == null) return HttpNotFound();

            if (!User.IsInRole("Admin"))
            {
                var userId = User.Identity.GetUserId();
                var canEdit = (task.IsGroupTask && task.Group != null && task.Group.TeamLeaderId == userId) ||
                              (!task.IsGroupTask && task.UserId == userId);
                if (!canEdit) return new HttpStatusCodeResult(HttpStatusCode.Forbidden);
            }
            return View(task);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(TaskItem model)
        {
            var task = db.TaskItems.Include(t => t.Group).FirstOrDefault(t => t.Id == model.Id);
            if (task == null) return HttpNotFound();

            if (!User.IsInRole("Admin"))
            {
                var userId = User.Identity.GetUserId();
                var canEdit = (task.IsGroupTask && task.Group != null && task.Group.TeamLeaderId == userId) ||
                              (!task.IsGroupTask && task.UserId == userId);
                if (!canEdit) return new HttpStatusCodeResult(HttpStatusCode.Forbidden);
            }

            if (!ModelState.IsValid) return View(model);

            task.Title = model.Title;
            task.Description = model.Description;
            task.Type = model.Type;
            task.DueDate = model.DueDate;
            task.IsCompleted = model.IsCompleted;
            task.SubjectId = model.SubjectId;
            task.IsGroupTask = model.IsGroupTask;
            task.GroupId = model.GroupId;

            db.SaveChanges();
            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Finish(int id)
        {
            var task = db.TaskItems.Include(t => t.Group).Include(t => t.Group.Members).FirstOrDefault(t => t.Id == id);
            if (task == null) return HttpNotFound();

            if (!User.IsInRole("Admin"))
            {
                var userId = User.Identity.GetUserId();
                var canFinish = (!task.IsGroupTask && task.UserId == userId) ||
                                (task.IsGroupTask && task.Group != null && task.Group.Members.Any(m => m.UserId == userId));
                if (!canFinish) return new HttpStatusCodeResult(HttpStatusCode.Forbidden);
            }

            task.IsCompleted = true;
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        public ActionResult Delete(int? id)
        {
            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            var task = db.TaskItems.Include(t => t.Group).FirstOrDefault(t => t.Id == id);
            if (task == null) return HttpNotFound();

            if (!User.IsInRole("Admin"))
            {
                var userId = User.Identity.GetUserId();
                var canDelete = (task.IsGroupTask && task.Group != null && task.Group.TeamLeaderId == userId) ||
                                (!task.IsGroupTask && task.UserId == userId);
                if (!canDelete) return new HttpStatusCodeResult(HttpStatusCode.Forbidden);
            }
            return View(task);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            var task = db.TaskItems.Find(id);
            if (task == null) return HttpNotFound();
            db.TaskItems.Remove(task);
            db.SaveChanges();
            return RedirectToAction("Index");
        }
    }
}