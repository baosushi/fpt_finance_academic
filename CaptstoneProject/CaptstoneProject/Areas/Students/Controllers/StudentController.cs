﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using DataService.Model;
using CaptstoneProject.Controllers;
using static CaptstoneProject.Models.AreaViewModel;
using CaptstoneProject.Models;

namespace CaptstoneProject.Areas.Students.Controllers
{
    public class StudentController : MyBaseController
    {
        private DB_Finance_AcademicEntities db = new DB_Finance_AcademicEntities();

        // GET: Student
        public ActionResult Index()
        {
            return View("_Index");
        }

        public ActionResult GetRegistrationSubjects()
        {
            var loginName = (string)this.Session["loginName"];
            using (var context = new DB_Finance_AcademicEntities())
            {
                var studentMajor = context.StudentMajors.Where(q => q.LoginName == loginName).FirstOrDefault();
                var availableSubjects = context.AvailableSubjects.Where(q => q.StudentMajorId == studentMajor.Id);

                var curriculumSubjects = availableSubjects.Where(q => q.IsInProgram.HasValue && q.IsInProgram.Value).Select(q => new
                {
                    SubjectCode = q.Subject.SubjectCode,
                    SubjectName = q.Subject.SubjectName
                }).ToList();
                var relearnSubjects = availableSubjects.Where(q => q.IsRelearn.HasValue && q.IsRelearn.Value).Select(q => new
                {
                    SubjectCode = q.Subject.SubjectCode,
                    SubjectName = q.Subject.SubjectName
                }).ToList();
                var otherSubjects = availableSubjects.Where(q => (!q.IsInProgram.HasValue || (q.IsInProgram.HasValue && !q.IsInProgram.Value)) && (!q.IsRelearn.HasValue || (q.IsRelearn.HasValue && !q.IsRelearn.Value))).Select(q => new
                {
                    SubjectCode = q.Subject.SubjectCode,
                    SubjectName = q.Subject.SubjectName
                }).ToList();

                return Json(new { curriculumSubjects = curriculumSubjects, relearnSubjects = relearnSubjects, otherSubjects = otherSubjects });
            }
        }

        [HttpPost]
        public ActionResult SubmitRegistration(List<string> relearnList, bool curriculumSubject = false, bool relearnSubject = false)
        {
            var loginName = (string)this.Session["loginName"];

            try
            {
                using (var context = new DB_Finance_AcademicEntities())
                {
                    var studentMajor = context.StudentMajors.Where(q => q.LoginName == loginName).FirstOrDefault();
                    var availableSubjects = context.AvailableSubjects.Where(q => q.StudentMajorId == studentMajor.Id);

                    var model = new RegistrationViewModel();
                    model.CurriculumRegistrationDetails = new List<RegistrationDetailViewModel>();
                    model.OtherRegistrationDetails = new List<RegistrationDetailViewModel>();

                    if (curriculumSubject)
                    {
                        var curriculumSubjects = availableSubjects.Where(q => q.IsInProgram.HasValue && q.IsInProgram.Value).Select(q => new RegistrationDetailViewModel
                        {
                            SubjectName = q.Subject.SubjectName,
                            SubjectCode = q.Subject.SubjectCode,
                            CreditValue = q.Subject.CreditValue.HasValue ? q.Subject.CreditValue.Value : 0,
                            RegisteredType = (int)RegistrationType.CurriculumSubject,
                            UnitPrice = 3000000,
                            TotalPrice = q.Subject.CreditValue.HasValue ? q.Subject.CreditValue.Value * 3000000 : 0
                        }).ToList();

                        model.CurriculumRegistrationDetails.InsertRange(model.CurriculumRegistrationDetails.Count, curriculumSubjects);
                        model.CurriculumTotalPrice = 25300000;
                    } else
                    {
                        model.CurriculumTotalPrice = 0;
                    }

                    if (relearnSubject && relearnList != null && relearnList.Count > 0)
                    {
                        var relearnSubjects = availableSubjects.Where(q => q.IsRelearn.HasValue && q.IsRelearn.Value).Select(q => new RegistrationDetailViewModel
                        {
                            SubjectCode = q.Subject.SubjectCode,
                            SubjectName = q.Subject.SubjectName,
                            CreditValue = q.Subject.CreditValue.HasValue ? q.Subject.CreditValue.Value : 0,
                            RegisteredType = (int)RegistrationType.RelearnSubject,
                            UnitPrice = 1500000,
                            TotalPrice = q.Subject.CreditValue.HasValue ? q.Subject.CreditValue.Value * 1500000 : 0
                        }).ToList();

                        model.OtherRegistrationDetails.InsertRange(model.OtherRegistrationDetails.Count, relearnSubjects);
                        model.OtherTotalPrice = model.OtherRegistrationDetails.Sum(q => q.TotalPrice);
                    } else
                    {
                        model.OtherTotalPrice = 0;
                    }

                    //OTHER Subject todo


                    model.StudentAccount = studentMajor.Accounts.FirstOrDefault();
                    model.TotalPrice = model.CurriculumTotalPrice + model.OtherTotalPrice;

                    return View("Payment", model);
                }
            }
            catch (Exception e)
            {
                return View("_Index");
            }
        }

        public ActionResult SubmitPayment()
        {
            return Json(new { success = true });
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

            List<StudentInCourse> studentInCourses = null;

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
