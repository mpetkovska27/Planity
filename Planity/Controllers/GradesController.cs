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
    public class GradesController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: Grades
        public ActionResult Index()
        {
            string currentUserId = User.Identity.GetUserId();
            var grades = db.Grades
                .Where(g => g.UserId == currentUserId)
                .Include(g => g.Subject)
                .OrderByDescending(g => g.Date)
                .ToList();
            return View(grades);
        }

        // GET: Grades/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Grade grade = db.Grades.Include(g => g.Subject).FirstOrDefault(g => g.Id == id);
            if (grade == null || grade.UserId != User.Identity.GetUserId())
            {
                return HttpNotFound();
            }
            return View(grade);
        }

        // GET: Grades/Create
        public ActionResult Create()
        {
            string currentUserId = User.Identity.GetUserId();
            ViewBag.SubjectId = new SelectList(db.Subjects.Where(s => s.UserId == currentUserId), "Id", "Name");
            return View();
        }

        // POST: Grades/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,Value,Date,Type,SubjectId")] Grade grade)
        {
            grade.UserId = User.Identity.GetUserId();

            if (ModelState.IsValid)
            {
                db.Grades.Add(grade);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.SubjectId = new SelectList(db.Subjects.Where(s => s.UserId == grade.UserId), "Id", "Name", grade.SubjectId);
            return View(grade);
        }

        // GET: Grades/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Grade grade = db.Grades.Find(id);
            if (grade == null || grade.UserId != User.Identity.GetUserId())
            {
                return HttpNotFound();
            }
            ViewBag.SubjectId = new SelectList(db.Subjects.Where(s => s.UserId == grade.UserId), "Id", "Name", grade.SubjectId);
            return View(grade);
        }

        // POST: Grades/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,Value,Date,Type,SubjectId,UserId")] Grade grade)
        {
            grade.UserId = User.Identity.GetUserId();

            if (ModelState.IsValid)
            {
                db.Entry(grade).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.SubjectId = new SelectList(db.Subjects.Where(s => s.UserId == grade.UserId), "Id", "Name", grade.SubjectId);
            return View(grade);
        }

        // GET: Grades/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Grade grade = db.Grades.Include(g => g.Subject).FirstOrDefault(g => g.Id == id);
            if (grade == null || grade.UserId != User.Identity.GetUserId())
            {
                return HttpNotFound();
            }
            return View(grade);
        }

        // POST: Grades/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Grade grade = db.Grades.Find(id);
            if (grade.UserId == User.Identity.GetUserId())
            {
                db.Grades.Remove(grade);
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
