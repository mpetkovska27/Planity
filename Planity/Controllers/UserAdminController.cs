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

            bool isTimLeader = userManager.IsInRole(userId, "TimLeader");
            bool isStudent = userManager.IsInRole(userId, "Student");
            bool isAdmin = userManager.IsInRole(userId, "Admin");

            if (isAdmin)
            {
                TempData["Error"] = "Cannot change role for Admin users!";
                return RedirectToAction("Index");
            }

            if (isTimLeader)
            {
                // Ako e TimLeader, vrati go vo Student
                userManager.RemoveFromRole(userId, "TimLeader");
                if (!isStudent) userManager.AddToRole(userId, "Student");
            }
            else if (isStudent)
            {
                // Ako e Student, stavi go TimLeader
                userManager.RemoveFromRole(userId, "Student");
                userManager.AddToRole(userId, "TimLeader");
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