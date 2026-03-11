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
    public class GroupsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: Groups
        public ActionResult Index()
        {
            if (User.IsInRole("Admin"))
            {
                var allGroups = db.Groups.Include(g => g.TeamLeader).Include(g => g.Members).ToList();
                return View(allGroups);
            }

            string currentUserId = User.Identity.GetUserId();
            var groups = db.Groups
                .Where(g => g.TeamLeaderId == currentUserId || g.Members.Any(m => m.UserId == currentUserId))
                .Include(g => g.TeamLeader);
            return View(groups.ToList());
        }

        // GET: Groups/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            string currentUserId = User.Identity.GetUserId();

            Group group = db.Groups
                .Include(g => g.TeamLeader)
                .Include(g => g.Members.Select(m => m.User))
                .Include(g => g.Tasks)
                .FirstOrDefault(g => g.Id == id);

            if (group == null)
            {
                return HttpNotFound();
            }

            bool isMember = group.Members.Any(m => m.UserId == currentUserId);
            bool isLeader = group.TeamLeaderId == currentUserId;
            bool isAdmin = User.IsInRole("Admin");

            if (!isAdmin && !isLeader && !isMember)
            {
                return new HttpStatusCodeResult(HttpStatusCode.Forbidden); // Немаш пристап!
            }

            return View(group);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public ActionResult AddGroupTask(int groupId, string title, string type, string priority, DateTime? dueDate)
        {
            var group = db.Groups
                .Include(g => g.Members)
                .FirstOrDefault(g => g.Id == groupId);
            if (group == null)
            {
                return Json(new { success = false, message = "Group not found" });
            }

            string currentUserId = User.Identity.GetUserId();
            bool isAdmin = User.IsInRole("Admin");
            bool isLeader = group.TeamLeaderId == currentUserId;
            bool isMember = group.Members.Any(m => m.UserId == currentUserId);
            if (!isAdmin && !isLeader && !isMember)
            {
                return new HttpStatusCodeResult(HttpStatusCode.Forbidden);
            }

            if (string.IsNullOrWhiteSpace(title))
            {
                return Json(new { success = false, message = "Task title is required" });
            }

            var taskType = TaskType.Homework;
            if (!string.IsNullOrWhiteSpace(type) && Enum.TryParse(type, true, out TaskType parsedType))
            {
                taskType = parsedType;
            }

            var taskPriority = Priority.Medium;
            if (!string.IsNullOrWhiteSpace(priority) && Enum.TryParse(priority, true, out Priority parsedPriority))
            {
                taskPriority = parsedPriority;
            }

            var resolvedDueDate = dueDate ?? DateTime.Now.AddDays(7);

            var task = new TaskItem
            {
                Title = title.Trim(),
                Description = "",
                Type = taskType,
                Priority = taskPriority,
                Status = TaskStatus.ToDo,
                DueDate = resolvedDueDate,
                GroupId = groupId,
                UserId = currentUserId,
                IsGroupTask = true
            };

            db.TaskItems.Add(task);
            db.SaveChanges();

            return Json(new { success = true, id = task.Id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public ActionResult UpdateGroupTaskStatus(int taskId, TaskStatus status)
        {
            var task = db.TaskItems
                .Include(t => t.Group.Members)
                .FirstOrDefault(t => t.Id == taskId);
            if (task == null || !task.IsGroupTask || task.GroupId == null)
            {
                return HttpNotFound();
            }

            string currentUserId = User.Identity.GetUserId();
            bool isAdmin = User.IsInRole("Admin");
            bool isLeader = task.Group.TeamLeaderId == currentUserId;
            bool isMember = task.Group.Members.Any(m => m.UserId == currentUserId);

            if (!isAdmin && !isLeader && !isMember)
            {
                return new HttpStatusCodeResult(HttpStatusCode.Forbidden);
            }

            task.Status = status;
            db.SaveChanges();

            return Json(new { success = true, status = task.Status.ToString() });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public ActionResult UpdateGroupTask(int taskId, string title, string type, string priority, DateTime? dueDate)
        {
            var task = db.TaskItems
                .Include(t => t.Group.Members)
                .FirstOrDefault(t => t.Id == taskId);
            if (task == null || !task.IsGroupTask || task.GroupId == null)
            {
                return HttpNotFound();
            }

            string currentUserId = User.Identity.GetUserId();
            bool isAdmin = User.IsInRole("Admin");
            bool isLeader = task.Group.TeamLeaderId == currentUserId;
            bool isMember = task.Group.Members.Any(m => m.UserId == currentUserId);
            if (!isAdmin && !isLeader && !isMember)
            {
                return new HttpStatusCodeResult(HttpStatusCode.Forbidden);
            }

            if (string.IsNullOrWhiteSpace(title))
            {
                return Json(new { success = false, message = "Task title is required" });
            }

            if (!string.IsNullOrWhiteSpace(type) && Enum.TryParse(type, true, out TaskType parsedType))
            {
                task.Type = parsedType;
            }

            if (!string.IsNullOrWhiteSpace(priority) && Enum.TryParse(priority, true, out Priority parsedPriority))
            {
                task.Priority = parsedPriority;
            }

            task.Title = title.Trim();
            task.DueDate = dueDate;
            db.SaveChanges();

            return Json(new { success = true });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public ActionResult DeleteGroupTask(int taskId)
        {
            var task = db.TaskItems
                .Include(t => t.Group.Members)
                .FirstOrDefault(t => t.Id == taskId);
            if (task == null || !task.IsGroupTask || task.GroupId == null)
            {
                return HttpNotFound();
            }

            string currentUserId = User.Identity.GetUserId();
            bool isAdmin = User.IsInRole("Admin");
            bool isLeader = task.Group.TeamLeaderId == currentUserId;
            bool isMember = task.Group.Members.Any(m => m.UserId == currentUserId);
            if (!isAdmin && !isLeader && !isMember)
            {
                return new HttpStatusCodeResult(HttpStatusCode.Forbidden);
            }

            var subTasks = db.TaskItems.Where(t => t.ParentTaskId == task.Id).ToList();
            db.TaskItems.RemoveRange(subTasks);
            db.TaskItems.Remove(task);
            db.SaveChanges();

            return Json(new { success = true });
        }

        // GET: Groups/Create
        [Authorize(Roles = "Admin,TeamLeader")]
        public ActionResult Create()
        {
            return View();
        }

        // POST: Groups/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,TeamLeader")]
        public ActionResult Create([Bind(Include = "Id,Name,Description")] Group group)
        {
            if (ModelState.IsValid)
            {
                string currentUserId = User.Identity.GetUserId();
                group.TeamLeaderId = currentUserId;
                db.Groups.Add(group);
                db.SaveChanges();

                var membership = new GroupMember
                {
                    GroupId = group.Id,
                    UserId = currentUserId
                };
                db.GroupMembers.Add(membership);
                db.SaveChanges();

                return RedirectToAction("Index");
            }

            return View(group);
        }

        // GET: Groups/Edit/5
        [Authorize(Roles = "Admin,TeamLeader")]
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Group group = db.Groups.Find(id);
            if (group == null)
            {
                return HttpNotFound();
            }
            bool isLeader = group.TeamLeaderId == User.Identity.GetUserId();
            bool isAdmin = User.IsInRole("Admin");
            if (!isAdmin && !isLeader)
            {
                return new HttpStatusCodeResult(HttpStatusCode.Forbidden);
            }
            return View(group);
        }

        // POST: Groups/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,TeamLeader")]
        public ActionResult Edit([Bind(Include = "Id,Name,Description")] Group group)
        {
            if (group == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var existingGroup = db.Groups.Find(group.Id);
            if (existingGroup == null)
            {
                return HttpNotFound();
            }
            bool isLeader = existingGroup.TeamLeaderId == User.Identity.GetUserId();
            bool isAdmin = User.IsInRole("Admin");
            if (!isAdmin && !isLeader)
            {
                return new HttpStatusCodeResult(HttpStatusCode.Forbidden);
            }

            if (ModelState.IsValid)
            {
                existingGroup.Name = group.Name;
                existingGroup.Description = group.Description;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            group.TeamLeaderId = existingGroup.TeamLeaderId;
            return View(group);
        }

        // GET: Groups/Delete/5
        [Authorize(Roles = "Admin,TeamLeader")]
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Group group = db.Groups.Find(id);
            if (group == null)
            {
                return HttpNotFound();
            }
            bool isLeader = group.TeamLeaderId == User.Identity.GetUserId();
            bool isAdmin = User.IsInRole("Admin");
            if (!isAdmin && !isLeader)
            {
                return new HttpStatusCodeResult(HttpStatusCode.Forbidden);
            }
            return View(group);
        }

        // POST: Groups/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,TeamLeader")]
        public ActionResult DeleteConfirmed(int id)
        {
            Group group = db.Groups.Find(id);
            if (group == null || (group.TeamLeaderId != User.Identity.GetUserId() && !User.IsInRole("Admin")))
            {
                return new HttpStatusCodeResult(HttpStatusCode.Forbidden);
            }

            var groupTasks = db.TaskItems.Where(t => t.GroupId == id).ToList();
            var groupTaskIds = groupTasks.Select(t => t.Id).ToList();
            var groupSubTasks = db.TaskItems
                .Where(t => t.ParentTaskId.HasValue && groupTaskIds.Contains(t.ParentTaskId.Value))
                .ToList();
            db.TaskItems.RemoveRange(groupSubTasks);
            db.TaskItems.RemoveRange(groupTasks);

            var memberships = db.GroupMembers.Where(m => m.GroupId == id);
            db.GroupMembers.RemoveRange(memberships);

            db.Groups.Remove(group);
            db.SaveChanges();
            return RedirectToAction("Index");
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,TeamLeader")]
        public ActionResult AddMember(int groupId, string userEmail)
        {
            var group = db.Groups.Find(groupId);
            if (group == null) return HttpNotFound();

            bool isLeader = group.TeamLeaderId == User.Identity.GetUserId();
            bool isAdmin = User.IsInRole("Admin");

            if (!isAdmin && !isLeader)
            {
                return new HttpStatusCodeResult(HttpStatusCode.Forbidden);
            }

            var userToAdd = db.Users.FirstOrDefault(u => u.Email == userEmail);
            if (userToAdd == null)
            {
                TempData["Error"] = "User with that email was not found.";
            }
            else if (db.GroupMembers.Any(m => m.GroupId == groupId && m.UserId == userToAdd.Id))
            {
                TempData["Error"] = "User is already a member of this group.";
            }
            else
            {
                db.GroupMembers.Add(new GroupMember { GroupId = groupId, UserId = userToAdd.Id });
                db.SaveChanges();
                TempData["Success"] = "Member added successfully!";
            }

            return RedirectToAction("Details", new { id = groupId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,TeamLeader")]
        public ActionResult RemoveMember(int groupId, string userId)
        {
            var group = db.Groups.Find(groupId);
            if (group == null)
            {
                return HttpNotFound();
            }
            bool isLeader = group.TeamLeaderId == User.Identity.GetUserId();
            bool isAdmin = User.IsInRole("Admin");
            if (!isAdmin && !isLeader && userId != User.Identity.GetUserId())
            {
                return new HttpStatusCodeResult(HttpStatusCode.Forbidden);
            }

            var membership = db.GroupMembers.FirstOrDefault(m => m.GroupId == groupId && m.UserId == userId);
            if (membership != null)
            {
                db.GroupMembers.Remove(membership);
                db.SaveChanges();
            }

            return RedirectToAction("Details", new { id = groupId });
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
