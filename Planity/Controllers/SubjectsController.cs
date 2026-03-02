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
        [ValidateAntiForgeryToken]
        public ActionResult UpdateGrade(int id, double value)
        {
            var subject = db.Subjects.Find(id);
            if (subject == null) return Json(new { success = false, message = "Subject not found" });

            if (!User.IsInRole("Admin") && subject.UserId != User.Identity.GetUserId())
            {
                return new HttpStatusCodeResult(HttpStatusCode.Forbidden);
            }

            // Targeting specifically the "Final" grade for this subject
            var grade = db.Grades.FirstOrDefault(g => g.SubjectId == id && g.Type == "Final");
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

            subject.IsCompleted = true;
            db.Configuration.ValidateOnSaveEnabled = false;
            db.SaveChanges();
            db.Configuration.ValidateOnSaveEnabled = true;
            return Json(new { success = true });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ToggleStatus(int id)
        {
            var subject = db.Subjects.Find(id);
            if (subject == null) return Json(new { success = false });

            if (!User.IsInRole("Admin") && subject.UserId != User.Identity.GetUserId())
            {
                return new HttpStatusCodeResult(HttpStatusCode.Forbidden);
            }

            subject.IsCompleted = !subject.IsCompleted;
            db.Configuration.ValidateOnSaveEnabled = false;
            db.SaveChanges();
            db.Configuration.ValidateOnSaveEnabled = true;

            return Json(new { success = true });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult UpdateSchedule(int id, string scheduleDay, string scheduleTimeSlot)
        {
            var subject = db.Subjects.Find(id);
            if (subject == null) return Json(new { success = false, message = "Subject not found" });

            if (!User.IsInRole("Admin") && subject.UserId != User.Identity.GetUserId())
            {
                return new HttpStatusCodeResult(HttpStatusCode.Forbidden);
            }

            if (string.IsNullOrWhiteSpace(scheduleDay))
            {
                return Json(new { success = false, message = "Invalid schedule day" });
            }

            DayOfWeek day;
            if (int.TryParse(scheduleDay, out var dayValue) && Enum.IsDefined(typeof(DayOfWeek), dayValue))
            {
                day = (DayOfWeek)dayValue;
            }
            else if (!Enum.TryParse(scheduleDay, true, out day))
            {
                return Json(new { success = false, message = "Invalid schedule day" });
            }

            subject.ScheduleDay = day;
            subject.ScheduleTimeSlot = string.IsNullOrWhiteSpace(scheduleTimeSlot) ? null : scheduleTimeSlot.Trim();

            try
            {
                db.Configuration.ValidateOnSaveEnabled = false;
                db.SaveChanges();
            }
            catch (System.Data.Entity.Validation.DbEntityValidationException ex)
            {
                var message = string.Join("; ",
                    ex.EntityValidationErrors.SelectMany(e => e.ValidationErrors).Select(e => e.ErrorMessage));
                return Json(new { success = false, message });
            }
            finally
            {
                db.Configuration.ValidateOnSaveEnabled = true;
            }

            return Json(new
            {
                success = true,
                day = subject.ScheduleDay.ToString(),
                timeSlot = subject.ScheduleTimeSlot ?? string.Empty
            });
        }

        // GET: Subjects/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Subject subject = db.Subjects
                .Include(s => s.User)
                .Include(s => s.Tasks)
                .Include(s => s.Grades)
                .FirstOrDefault(s => s.Id == id);
            string currentUserId = User.Identity.GetUserId();

            if (subject == null || (!User.IsInRole("Admin") && subject.UserId != currentUserId))
            {
                return HttpNotFound();
            }
            return View(subject);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AddSubjectTask(int subjectId, string title, string type, string priority, DateTime? dueDate)
        {
            var subject = db.Subjects.Find(subjectId);
            if (subject == null) return Json(new { success = false, message = "Subject not found" });

            if (!User.IsInRole("Admin") && subject.UserId != User.Identity.GetUserId())
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
                SubjectId = subjectId,
                UserId = subject.UserId,
                IsGroupTask = false
            };

            db.TaskItems.Add(task);
            db.SaveChanges();

            var totalTasks = db.TaskItems.Count(t => t.SubjectId == subjectId);
            var completedTasks = db.TaskItems.Count(t => t.SubjectId == subjectId && t.Status == TaskStatus.Done);
            var progress = totalTasks > 0 ? (int)Math.Round((double)completedTasks / totalTasks * 100) : 0;

            return Json(new
            {
                success = true,
                id = task.Id,
                title = task.Title,
                status = task.Status.ToString(),
                total = totalTasks,
                completed = completedTasks,
                progress = progress
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ToggleTaskStatus(int taskId)
        {
            var task = db.TaskItems.Include(t => t.Subject).FirstOrDefault(t => t.Id == taskId);
            if (task == null) return Json(new { success = false, message = "Task not found" });

            if (!User.IsInRole("Admin") && task.Subject.UserId != User.Identity.GetUserId())
            {
                return new HttpStatusCodeResult(HttpStatusCode.Forbidden);
            }

            task.Status = task.Status == TaskStatus.Done ? TaskStatus.ToDo : TaskStatus.Done;
            db.SaveChanges();

            var totalTasks = db.TaskItems.Count(t => t.SubjectId == task.SubjectId);
            var completedTasks = db.TaskItems.Count(t => t.SubjectId == task.SubjectId && t.Status == TaskStatus.Done);
            var progress = totalTasks > 0 ? (int)Math.Round((double)completedTasks / totalTasks * 100) : 0;

            return Json(new
            {
                success = true,
                id = task.Id,
                status = task.Status.ToString(),
                total = totalTasks,
                completed = completedTasks,
                progress = progress
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult UpdateSubjectTask(int taskId, string title, string type, string priority, DateTime? dueDate)
        {
            var task = db.TaskItems.Include(t => t.Subject).FirstOrDefault(t => t.Id == taskId);
            if (task == null) return Json(new { success = false, message = "Task not found" });

            if (!User.IsInRole("Admin") && task.Subject.UserId != User.Identity.GetUserId())
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
            task.DueDate = dueDate ?? task.DueDate;

            db.SaveChanges();

            return Json(new { success = true });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteSubjectTask(int taskId)
        {
            var task = db.TaskItems.Include(t => t.Subject).FirstOrDefault(t => t.Id == taskId);
            if (task == null) return Json(new { success = false, message = "Task not found" });

            if (!User.IsInRole("Admin") && task.Subject.UserId != User.Identity.GetUserId())
            {
                return new HttpStatusCodeResult(HttpStatusCode.Forbidden);
            }

            db.TaskItems.Remove(task);
            db.SaveChanges();

            return Json(new { success = true });
        }

        // GET: Subjects/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Subjects/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,Name,Year,Semester,Credits")] Subject subject)
        {
            if (ModelState.IsValid)
            {
                subject.UserId = User.Identity.GetUserId();
                subject.IsCompleted = false;
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
        public ActionResult Edit([Bind(Include = "Id,Name,Year,Semester,Credits")] Subject subject)
        {
            var existingSubject = db.Subjects.Find(subject.Id);
            if (existingSubject == null)
            {
                return HttpNotFound();
            }
            if (!User.IsInRole("Admin") && existingSubject.UserId != User.Identity.GetUserId())
            {
                return new HttpStatusCodeResult(HttpStatusCode.Forbidden);
            }

            if (ModelState.IsValid)
            {
                existingSubject.Name = subject.Name;
                existingSubject.Year = subject.Year;
                existingSubject.Semester = subject.Semester;
                existingSubject.Credits = subject.Credits;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(existingSubject);
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
            if (subject == null)
            {
                return Json(new { success = false });
            }

            if (!User.IsInRole("Admin") && subject.UserId != User.Identity.GetUserId())
            {
                return new HttpStatusCodeResult(HttpStatusCode.Forbidden);
            }

            db.TaskItems.RemoveRange(db.TaskItems.Where(t => t.SubjectId == id));
            db.Grades.RemoveRange(db.Grades.Where(g => g.SubjectId == id));
            db.Subjects.Remove(subject);
            db.SaveChanges();

            return Json(new { success = true });
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