using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Planity.Models;
using Microsoft.AspNet.Identity;

namespace Planity.Controllers
{
    [Authorize(Roles = "TimLeader,Admin")]
    public class GroupsController : Controller
    {
        private readonly ApplicationDbContext db = new ApplicationDbContext();

        public ActionResult Index()
        {
            var userId = User.Identity.GetUserId();
            var q = db.Groups.Include(g => g.Members).AsQueryable();
            if (!User.IsInRole("Admin"))
            {
                q = q.Where(g => g.TeamLeaderId == userId);
            }
            return View(q.ToList());
        }

        public ActionResult Details(int? id)
        {
            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            var group = db.Groups.Include(g => g.Members).FirstOrDefault(g => g.Id == id);
            if (group == null) return HttpNotFound();
            if (!User.IsInRole("Admin") && group.TeamLeaderId != User.Identity.GetUserId())
                return new HttpStatusCodeResult(HttpStatusCode.Forbidden);
            return View(group);
        }

        public ActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Group group)
        {
            if (!ModelState.IsValid) return View(group);
            if (!User.IsInRole("Admin")) group.TeamLeaderId = User.Identity.GetUserId();

            db.Groups.Add(group);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        public ActionResult Edit(int? id)
        {
            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            var group = db.Groups.Include(g => g.Members).FirstOrDefault(g => g.Id == id);
            if (group == null) return HttpNotFound();
            if (!User.IsInRole("Admin") && group.TeamLeaderId != User.Identity.GetUserId())
                return new HttpStatusCodeResult(HttpStatusCode.Forbidden);
            return View(group);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Group model)
        {
            var group = db.Groups.Find(model.Id);
            if (group == null) return HttpNotFound();
            if (!User.IsInRole("Admin") && group.TeamLeaderId != User.Identity.GetUserId())
                return new HttpStatusCodeResult(HttpStatusCode.Forbidden);

            if (!ModelState.IsValid) return View(model);

            group.Name = model.Name;
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        public ActionResult Delete(int? id)
        {
            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            var group = db.Groups.Find(id);
            if (group == null) return HttpNotFound();
            if (!User.IsInRole("Admin") && group.TeamLeaderId != User.Identity.GetUserId())
                return new HttpStatusCodeResult(HttpStatusCode.Forbidden);
            return View(group);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            var group = db.Groups.Find(id);
            if (group == null) return HttpNotFound();
            db.Groups.Remove(group);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AddMember(int groupId, string userId)
        {
            var group = db.Groups.Include(g => g.Members).FirstOrDefault(g => g.Id == groupId);
            if (group == null) return HttpNotFound();
            if (!User.IsInRole("Admin") && group.TeamLeaderId != User.Identity.GetUserId())
                return new HttpStatusCodeResult(HttpStatusCode.Forbidden);

            if (!group.Members.Any(m => m.UserId == userId))
            {
                db.GroupMembers.Add(new GroupMember { GroupId = groupId, UserId = userId });
                db.SaveChanges();
            }
            return RedirectToAction("Edit", new { id = groupId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult RemoveMember(int groupId, string userId)
        {
            var gm = db.GroupMembers.FirstOrDefault(m => m.GroupId == groupId && m.UserId == userId);
            if (gm == null) return HttpNotFound();

            var group = db.Groups.Find(groupId);
            if (!User.IsInRole("Admin") && group.TeamLeaderId != User.Identity.GetUserId())
                return new HttpStatusCodeResult(HttpStatusCode.Forbidden);

            db.GroupMembers.Remove(gm);
            db.SaveChanges();
            return RedirectToAction("Edit", new { id = groupId });
        }

        // Kanban-style board
        [Authorize(Roles = "Student,TimLeader,Admin")]
        public ActionResult Board(int id)
        {
            var group = db.Groups.Include(g => g.Members).FirstOrDefault(g => g.Id == id);
            if (group == null) return HttpNotFound();

            if (!User.IsInRole("Admin"))
            {
                var userId = User.Identity.GetUserId();
                var isMember = group.TeamLeaderId == userId || group.Members.Any(m => m.UserId == userId);
                if (!isMember) return new HttpStatusCodeResult(HttpStatusCode.Forbidden);
            }

            var tasks = db.TaskItems.Where(t => t.IsGroupTask && t.GroupId == id).ToList();
            ViewBag.Group = group;
            return View(tasks);
        }
    }
}