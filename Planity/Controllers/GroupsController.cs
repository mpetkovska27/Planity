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
    public class GroupsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: Groups
        public ActionResult Index()
        {
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

            Group group = db.Groups
                .Include(g => g.TeamLeader)
                .Include(g => g.Members.Select(m => m.User))
                .Include(g => g.Tasks)
                .FirstOrDefault(g => g.Id == id);

            if (group == null)
            {
                return HttpNotFound();
            }
            return View(group);
        }

        // GET: Groups/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Groups/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,Name")] Group group)
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
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Group group = db.Groups.Find(id);
            if (group == null || group.TeamLeaderId != User.Identity.GetUserId())
            {
                return HttpNotFound();
            }
            return View(group);
        }

        // POST: Groups/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,Name,TeamLeaderId")] Group group)
        {
            if (group.TeamLeaderId != User.Identity.GetUserId())
            {
                return new HttpStatusCodeResult(HttpStatusCode.Forbidden);
            }

            if (ModelState.IsValid)
            {
                db.Entry(group).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(group);
        }

        // GET: Groups/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Group group = db.Groups.Find(id);
            if (group == null || group.TeamLeaderId != User.Identity.GetUserId())
            {
                return HttpNotFound();
            }
            return View(group);
        }

        // POST: Groups/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Group group = db.Groups.Find(id);
            if (group.TeamLeaderId != User.Identity.GetUserId())
            {
                return new HttpStatusCodeResult(HttpStatusCode.Forbidden);
            }

            var memberships = db.GroupMembers.Where(m => m.GroupId == id);
            db.GroupMembers.RemoveRange(memberships);

            db.Groups.Remove(group);
            db.SaveChanges();
            return RedirectToAction("Index");
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AddMember(int groupId, string userEmail)
        {
            var group = db.Groups.Find(groupId);
            if (group.TeamLeaderId != User.Identity.GetUserId())
            {
                return new HttpStatusCodeResult(HttpStatusCode.Forbidden);
            }

            var userToAdd = db.Users.FirstOrDefault(u => u.Email == userEmail);
            if (userToAdd != null && !db.GroupMembers.Any(m => m.GroupId == groupId && m.UserId == userToAdd.Id))
            {
                db.GroupMembers.Add(new GroupMember { GroupId = groupId, UserId = userToAdd.Id });
                db.SaveChanges();
            }

            return RedirectToAction("Details", new { id = groupId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult RemoveMember(int groupId, string userId)
        {
            var group = db.Groups.Find(groupId);
            if (group.TeamLeaderId != User.Identity.GetUserId() && userId != User.Identity.GetUserId())
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
