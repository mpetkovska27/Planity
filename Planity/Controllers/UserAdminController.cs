using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Planity.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace Planity.Controllers
{
    [Authorize(Roles = "Admin")]
    public class UserAdminController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();
        // GET: UserAdmin
        public ActionResult Index()
        {
            var users = db.Users.ToList();
            var roleStore = new RoleStore<IdentityRole>(db);
            var roleManager = new RoleManager<IdentityRole>(roleStore);
            var rolesDictionary = roleManager.Roles.ToDictionary(r => r.Id, r => r.Name);

            ViewBag.RolesDictionary = rolesDictionary;
            return View(users);
        }
        //metoda za promena na uloga
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ToggleRole(string userId)
        {
            if (string.IsNullOrEmpty(userId))
            {
                return new HttpStatusCodeResult(System.Net.HttpStatusCode.BadRequest);
            }
            var userStore = new UserStore<ApplicationUser>(db);
            var userManager = new UserManager<ApplicationUser>(userStore);

            var user = userManager.FindById(userId);
            if (user == null)
            {
                return HttpNotFound();
            }

            bool isTeamLeader = userManager.IsInRole(userId, "TeamLeader");
            bool isStudent = userManager.IsInRole(userId, "Student");
            bool isAdmin = userManager.IsInRole(userId, "Admin");

            if (isAdmin)
            {
                TempData["Error"] = "Cannot change role for Admin users!";
                return RedirectToAction("Index");
            }

            if (isTeamLeader)
            {
                // Ako e TeamLeader, vrati go vo Student
                if (isTeamLeader) userManager.RemoveFromRole(userId, "TeamLeader");
                if (!isStudent) userManager.AddToRole(userId, "Student");
            }
            else if (isStudent)
            {
                // Ako e Student, stavi go TeamLeader
                userManager.RemoveFromRole(userId, "Student");
                userManager.AddToRole(userId, "TeamLeader");
            }
            else
            {
                userManager.AddToRole(userId, "Student");
            }

            return RedirectToAction("Index");

        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteUser(string userId)
        {
            if (string.IsNullOrEmpty(userId))
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var user = db.Users.Find(userId);
            if (user != null)
            {
                if (user.UserName == User.Identity.Name)
                {
                    TempData["Error"] = "You cannot delete your own admin account!";
                    return RedirectToAction("Index");
                }

                var leaderGroups = db.Groups.Where(g => g.TeamLeaderId == userId).ToList();
                foreach (var group in leaderGroups)
                {
                    var groupTasks = db.TaskItems.Where(t => t.GroupId == group.Id).ToList();
                    var groupTaskIds = groupTasks.Select(t => t.Id).ToList();
                    var groupSubTasks = db.TaskItems
                        .Where(t => t.ParentTaskId.HasValue && groupTaskIds.Contains(t.ParentTaskId.Value))
                        .ToList();

                    db.TaskItems.RemoveRange(groupSubTasks);
                    db.TaskItems.RemoveRange(groupTasks);

                    var groupMembers = db.GroupMembers.Where(m => m.GroupId == group.Id).ToList();
                    db.GroupMembers.RemoveRange(groupMembers);

                    db.Groups.Remove(group);
                }

                var userGroupMemberships = db.GroupMembers.Where(m => m.UserId == userId).ToList();
                db.GroupMembers.RemoveRange(userGroupMemberships);

                var userTasks = db.TaskItems.Where(t => t.UserId == userId).ToList();
                var userTaskIds = userTasks.Select(t => t.Id).ToList();
                var userSubTasks = db.TaskItems
                    .Where(t => t.ParentTaskId.HasValue && userTaskIds.Contains(t.ParentTaskId.Value))
                    .ToList();

                db.TaskItems.RemoveRange(userSubTasks);
                db.TaskItems.RemoveRange(userTasks);

                var userGrades = db.Grades.Where(g => g.UserId == userId).ToList();
                db.Grades.RemoveRange(userGrades);

                var userSubjects = db.Subjects.Where(s => s.UserId == userId).ToList();
                var userSubjectIds = userSubjects.Select(s => s.Id).ToList();
                var subjectTasks = db.TaskItems.Where(t => t.SubjectId.HasValue && userSubjectIds.Contains(t.SubjectId.Value)).ToList();
                var subjectTaskIds = subjectTasks.Select(t => t.Id).ToList();
                var subjectSubTasks = db.TaskItems
                    .Where(t => t.ParentTaskId.HasValue && subjectTaskIds.Contains(t.ParentTaskId.Value))
                    .ToList();
                db.TaskItems.RemoveRange(subjectSubTasks);
                db.TaskItems.RemoveRange(subjectTasks);

                db.Subjects.RemoveRange(userSubjects);

                db.Users.Remove(user);
                db.SaveChanges();
            }

            return RedirectToAction("Index");
        }
        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }
    }
}