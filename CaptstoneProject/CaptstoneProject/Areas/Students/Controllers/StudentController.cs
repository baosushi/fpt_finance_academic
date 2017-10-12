using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using DataService.Model;
using CaptstoneProject.Controllers;

namespace CaptstoneProject.Areas.Students.Controllers
{
    public class StudentController : MyBaseController
    {
        private DB_Finance_AcademicEntities db = new DB_Finance_AcademicEntities();

        // GET: Student
        public ActionResult Index()
        {
            var studentInCourses = db.StudentInCourses.Include(s => s.Course).Include(s => s.StudentMajor);
            return View(studentInCourses.ToList());
        }

        // GET: Student/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            StudentInCourse studentInCourse = db.StudentInCourses.Find(id);
            if (studentInCourse == null)
            {
                return HttpNotFound();
            }
            return View(studentInCourse);
        }

        // GET: Student/Create
        public ActionResult Create()
        {
            ViewBag.CourseId = new SelectList(db.Courses, "Id", "Id");
            ViewBag.StudentId = new SelectList(db.Students, "Id", "StudentCode");
            return View();
        }

        // POST: Student/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,CourseId,StudentId,Average")] StudentInCourse studentInCourse)
        {
            if (ModelState.IsValid)
            {
                db.StudentInCourses.Add(studentInCourse);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.CourseId = new SelectList(db.Courses, "Id", "Id", studentInCourse.CourseId);
            ViewBag.StudentId = new SelectList(db.Students, "Id", "StudentCode", studentInCourse.StudentId);
            return View(studentInCourse);
        }

        // GET: Student/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            StudentInCourse studentInCourse = db.StudentInCourses.Find(id);
            if (studentInCourse == null)
            {
                return HttpNotFound();
            }
            ViewBag.CourseId = new SelectList(db.Courses, "Id", "Id", studentInCourse.CourseId);
            ViewBag.StudentId = new SelectList(db.Students, "Id", "StudentCode", studentInCourse.StudentId);
            return View(studentInCourse);
        }

        // POST: Student/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,CourseId,StudentId,Average")] StudentInCourse studentInCourse)
        {
            if (ModelState.IsValid)
            {
                db.Entry(studentInCourse).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.CourseId = new SelectList(db.Courses, "Id", "Id", studentInCourse.CourseId);
            ViewBag.StudentId = new SelectList(db.Students, "Id", "StudentCode", studentInCourse.StudentId);
            return View(studentInCourse);
        }

        // GET: Student/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            StudentInCourse studentInCourse = db.StudentInCourses.Find(id);
            if (studentInCourse == null)
            {
                return HttpNotFound();
            }
            return View(studentInCourse);
        }

        // POST: Student/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            StudentInCourse studentInCourse = db.StudentInCourses.Find(id);
            db.StudentInCourses.Remove(studentInCourse);
            db.SaveChanges();
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
        public JsonResult GetStudentsList(jQueryDataTableParamModel param)
        {

            int count = 0;

            List<StudentInCourse> studentInCourses=null;

            if (!string.IsNullOrEmpty(param.sSearch))
            {
                studentInCourses = db.StudentInCourses.Include(s => s.Course).Include(s => s.StudentMajor).Where(s => s.StudentMajor.StudentCode.Contains(param.sSearch)).ToList();
            }
            else
            {
                studentInCourses = db.StudentInCourses.Include(s => s.Course).Include(s => s.StudentMajor).ToList();
            }
            
            int totalRecords = studentInCourses.Count();

            count = param.iDisplayStart + 1;


            var rs = studentInCourses
                    //searchResult
                    .OrderBy(a => a.StudentMajor.StudentCode)
                    .Skip(param.iDisplayStart)
                    .Take(param.iDisplayLength)
                    .Select(a => new IConvertible[]
                        {
                        //count++,
                        a.StudentMajor.StudentCode,
                        a.StudentId,
                        a.Course.SubjectId,
                        a.CourseId,
                        a.Average,
                        });


            int totalDisplay = studentInCourses.Count();

            return Json(new
            {
                sEcho = param.sEcho,
                iTotalRecords = totalRecords,
                iTotalDisplayRecords = totalDisplay,
                aaData = rs
            }, JsonRequestBehavior.AllowGet);

        }
    }
}
