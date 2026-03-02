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
                        .Include(t => t.Subject)
                        .Include(t => t.User);

            var subjectsQuery = db.Subjects.Include(s => s.User);
            if (!User.IsInRole("Admin"))
            {
                subjectsQuery = subjectsQuery.Where(s => s.UserId == currentUserId);
            }
            ViewBag.Subjects = subjectsQuery.ToList();

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

        [HttpGet]
        public ActionResult DetailsPartial(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            string currentUserId = User.Identity.GetUserId();
            var subjectsQuery = db.Subjects.Include(s => s.User);
            if (!User.IsInRole("Admin"))
            {
                subjectsQuery = subjectsQuery.Where(s => s.UserId == currentUserId);
            }
            ViewBag.Subjects = subjectsQuery.ToList();
            var taskItem = db.TaskItems
                .Include(t => t.User)
                .Include(t => t.SubTasks)
                .Include(t => t.Group.Members)
                .FirstOrDefault(t => t.Id == id);
            if (taskItem == null)
            {
                return HttpNotFound();
            }

            bool isAdmin = User.IsInRole("Admin");
            bool isOwner = taskItem.UserId == currentUserId;
            bool isMember = taskItem.IsGroupTask && taskItem.Group != null && taskItem.Group.Members.Any(m => m.UserId == currentUserId);
            bool isLeader = taskItem.IsGroupTask && taskItem.Group != null && taskItem.Group.TeamLeaderId == currentUserId;
            if (!isAdmin && !isOwner && !isMember && !isLeader)
            {
                return new HttpStatusCodeResult(HttpStatusCode.Forbidden);
            }
            ViewBag.CanEdit = isAdmin || isOwner;

            return PartialView("_TaskDetailsPartial", taskItem);
        }


        // GET: TaskItems/Create
        public ActionResult Create()
        {
            string currentUserId = User.Identity.GetUserId();
            ViewBag.GroupId = new SelectList(db.Groups.Where(g => g.TeamLeaderId == currentUserId || g.Members.Any(m => m.UserId == currentUserId)), "Id", "Name");
            ViewBag.SubjectId = new SelectList(db.Subjects.Where(s => s.UserId == currentUserId), "Id", "Name");
            return View();
        }

        // POST: TaskItems/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
[ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,Title,Description,Type,Priority,Status,DueDate,Repeat,SubjectId,IsGroupTask,GroupId,UserId")] TaskItem taskItem, HttpPostedFileBase attachedFile)
        {
            if (string.IsNullOrEmpty(taskItem.UserId)) taskItem.UserId = User.Identity.GetUserId();

            if (ModelState.IsValid)
            {
                if (attachedFile != null && attachedFile.ContentLength > 0)
                {
                    string fileName = Guid.NewGuid().ToString() + "_" + System.IO.Path.GetFileName(attachedFile.FileName);
                    string path = System.IO.Path.Combine(Server.MapPath("~/Uploads/Tasks/"), fileName);
                    attachedFile.SaveAs(path);
                    taskItem.AttachedFilePath = "/Uploads/Tasks/" + fileName;
                }

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
            ViewBag.SubjectId = new SelectList(db.Subjects.Where(s => s.UserId == currentUserId), "Id", "Name", taskItem.SubjectId);
            return View(taskItem);
        }

        // POST: TaskItems/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,Title,Description,Type,Priority,Status,DueDate,Repeat,SubjectId,IsGroupTask,GroupId,AttachedFilePath")] TaskItem taskItem, HttpPostedFileBase attachedFile)
        {
            string currentUserId = User.Identity.GetUserId();
            if (!User.IsInRole("Admin") && taskItem.UserId != User.Identity.GetUserId()) 
            {
                return new HttpStatusCodeResult(HttpStatusCode.Forbidden);
            }

            if (ModelState.IsValid)
            {
                if (attachedFile != null && attachedFile.ContentLength > 0)
                {
                    string directoryPath = Server.MapPath("~/Uploads/Tasks/");
                    if (!System.IO.Directory.Exists(directoryPath))
                    {
                        System.IO.Directory.CreateDirectory(directoryPath);
                    }
                    string fileName = Guid.NewGuid().ToString() + "_" + System.IO.Path.GetFileName(attachedFile.FileName);
                    string path = System.IO.Path.Combine(directoryPath, fileName);
                    attachedFile.SaveAs(path);
                    taskItem.AttachedFilePath = "/Uploads/Tasks/" + fileName;
                }

                db.Entry(taskItem).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.GroupId = new SelectList(db.Groups, "Id", "Name", taskItem.GroupId);
            ViewBag.SubjectId = new SelectList(db.Subjects.Where(s => s.UserId == currentUserId), "Id", "Name", taskItem.SubjectId);
            return View(taskItem);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult UpdateStatus(int id, TaskStatus status)
        {
            TaskItem taskItem = db.TaskItems.FirstOrDefault(t => t.Id == id);
            if (taskItem == null)
            {
                return HttpNotFound();
            }

            if (!User.IsInRole("Admin") && taskItem.UserId != User.Identity.GetUserId())
            {
                return new HttpStatusCodeResult(HttpStatusCode.Forbidden);
            }

            var previousStatus = taskItem.Status;
            taskItem.Status = status;

            TaskItem repeatedTask = null;
            var repeatSetting = taskItem.Repeat ?? Repeat.None;
            if (previousStatus != TaskStatus.Done && status == TaskStatus.Done && repeatSetting != Repeat.None)
            {
                var nextDueDate = GetNextDueDate(taskItem.DueDate, repeatSetting);
                repeatedTask = new TaskItem
                {
                    Title = taskItem.Title,
                    Description = taskItem.Description,
                    Type = taskItem.Type,
                    Priority = taskItem.Priority,
                    Status = TaskStatus.ToDo,
                    DueDate = nextDueDate,
                    Repeat = repeatSetting,
                    SubjectId = taskItem.SubjectId,
                    UserId = taskItem.UserId,
                    IsGroupTask = taskItem.IsGroupTask,
                    GroupId = taskItem.GroupId,
                    ParentTaskId = taskItem.ParentTaskId
                };
                db.TaskItems.Add(repeatedTask);
            }
            db.SaveChanges();
            return Json(new
            {
                success = true,
                status = taskItem.Status.ToString(),
                repeatCreated = repeatedTask != null,
                repeatTaskId = repeatedTask?.Id
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult UpdatePriority(int id, Priority priority)
        {
            TaskItem taskItem = db.TaskItems.FirstOrDefault(t => t.Id == id);
            if (taskItem == null)
            {
                return HttpNotFound();
            }

            if (!User.IsInRole("Admin") && taskItem.UserId != User.Identity.GetUserId())
            {
                return new HttpStatusCodeResult(HttpStatusCode.Forbidden);
            }

            taskItem.Priority = priority;
            db.SaveChanges();
            return Json(new { success = true, priority = taskItem.Priority.ToString() });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult UpdateDetails(int id, string title, string description, DateTime? dueDate)
        {
            TaskItem taskItem = db.TaskItems.FirstOrDefault(t => t.Id == id);
            if (taskItem == null)
            {
                return HttpNotFound();
            }

            if (!User.IsInRole("Admin") && taskItem.UserId != User.Identity.GetUserId())
            {
                return new HttpStatusCodeResult(HttpStatusCode.Forbidden);
            }

            taskItem.Title = title ?? taskItem.Title;
            taskItem.Description = description;
            if (dueDate.HasValue)
            {
                taskItem.DueDate = dueDate.Value;
            }
            db.SaveChanges();

            return Json(new { success = true });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult UpdateRepeat(int id, Repeat repeat)
        {
            TaskItem taskItem = db.TaskItems.FirstOrDefault(t => t.Id == id);
            if (taskItem == null)
            {
                return HttpNotFound();
            }

            if (!User.IsInRole("Admin") && taskItem.UserId != User.Identity.GetUserId())
            {
                return new HttpStatusCodeResult(HttpStatusCode.Forbidden);
            }

            taskItem.Repeat = repeat;
            db.SaveChanges();
            return Json(new { success = true, repeat = taskItem.Repeat.ToString() });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult UpdateMeta(int id, TaskType type, int? subjectId)
        {
            TaskItem taskItem = db.TaskItems.Include(t => t.Group).FirstOrDefault(t => t.Id == id);
            if (taskItem == null)
            {
                return HttpNotFound();
            }

            if (!User.IsInRole("Admin") && taskItem.UserId != User.Identity.GetUserId())
            {
                return new HttpStatusCodeResult(HttpStatusCode.Forbidden);
            }

            taskItem.Type = type;
            taskItem.SubjectId = subjectId;
            db.SaveChanges();

            var subjectName = "-";
            if (subjectId.HasValue)
            {
                subjectName = db.Subjects.Where(s => s.Id == subjectId.Value).Select(s => s.Name).FirstOrDefault() ?? "-";
            }

            return Json(new { success = true, type = taskItem.Type.ToString(), subjectId = taskItem.SubjectId, subjectName });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreateSubtask(int parentId, string title)
        {
            if (string.IsNullOrWhiteSpace(title))
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            string currentUserId = User.Identity.GetUserId();
            TaskItem parentTask = db.TaskItems.FirstOrDefault(t => t.Id == parentId);
            if (parentTask == null)
            {
                return HttpNotFound();
            }

            if (!User.IsInRole("Admin") && parentTask.UserId != currentUserId)
            {
                return new HttpStatusCodeResult(HttpStatusCode.Forbidden);
            }

            var subtask = new TaskItem
            {
                Title = title.Trim(),
                Type = parentTask.Type,
                Priority = parentTask.Priority,
                Status = TaskStatus.ToDo,
                UserId = parentTask.UserId,
                SubjectId = parentTask.SubjectId,
                DueDate = parentTask.DueDate,
                ParentTaskId = parentTask.Id,
                Repeat = Repeat.None
            };

            db.TaskItems.Add(subtask);
            db.SaveChanges();

            return Json(new { success = true, id = subtask.Id, title = subtask.Title, status = subtask.Status.ToString() });
        }

        // GET: TaskItems/Delete/5
        public ActionResult Delete(int? id)
        {
            return RedirectToAction("Index");
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

        private static DateTime? GetNextDueDate(DateTime? currentDueDate, Repeat repeat)
        {
            if (repeat == Repeat.None)
            {
                return currentDueDate;
            }

            var today = DateTime.Today;
            var baseDate = (currentDueDate ?? today).Date;
            if (baseDate < today)
            {
                baseDate = today;
            }

            switch (repeat)
            {
                case Repeat.Daily:
                    return baseDate.AddDays(1);
                case Repeat.Weekdays:
                    var next = baseDate.AddDays(1);
                    while (next.DayOfWeek == DayOfWeek.Saturday || next.DayOfWeek == DayOfWeek.Sunday)
                    {
                        next = next.AddDays(1);
                    }
                    return next;
                case Repeat.Weekly:
                    return baseDate.AddDays(7);
                case Repeat.Monthly:
                    return baseDate.AddMonths(1);
                case Repeat.Yearly:
                    return baseDate.AddYears(1);
                default:
                    return baseDate;
            }
        }
    }
}
