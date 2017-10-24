using CaptstoneProject.Models;
using DataService.Model;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using static CaptstoneProject.Models.AreaViewModel;

namespace CaptstoneProject.Areas.AdminTrainingDepartment.Controllers
{
    public class AdminTrainingController : Controller
    {
        // GET: AdminTrainingDepartment/AdminTraining
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult SemesterManagement(int semesterId = -1)
        {
            IEnumerable<CourseRecordViewModel> courses = new List<CourseRecordViewModel>();
            using (var context = new DB_Finance_AcademicEntities())
            {
                int? status = null;
                //DateTime startDate, endDate;
                var semester = semesterId == -1 ? context.Semesters.OrderByDescending(q => q.Year).ThenByDescending(q => q.SemesterInYear).FirstOrDefault() : context.Semesters.Find(semesterId);

                //startDate = semester.StartDate.Value;
                //endDate = semester.EndDate.Value;

                courses = context.Courses.Where(q => q.SemesterId == semester.Id).AsEnumerable()
                    .Select(q => new CourseRecordViewModel
                    {
                        CourseId = q.Id,
                        Name = q.Subject.SubjectName,
                        Code = q.Subject.SubjectCode,
                        Class = q.ClassName,
                        StartDate = q.StartDate.Value,
                        EndDate = q.EndDate.Value,
                        Status = Enum.GetName(typeof(CourseStatus), q.Status == null ? 0 : q.Status.Value)
                    }).ToList();

                var semesters = context.Semesters.OrderByDescending(q => q.Year).ThenByDescending(q => q.SemesterInYear);
                var semesterList = semesters.Select(q => new SelectListItem
                {
                    Text = q.Title + " " + q.Year,
                    Value = q.Id.ToString(),
                }).ToList();

                var subjectList = context.Subjects.Select(q => new SelectListItem
                {
                    Text = q.SubjectName + " - " + q.SubjectCode,
                    Value = q.Id.ToString()
                }).ToList();

                var teacherList = context.Teachers.Select(q => new SelectListItem
                {
                    Text = q.Name,
                    Value = q.Id.ToString()
                }).ToList();

                ViewBag.semList = semesterList;
                ViewBag.selectedSem = semesterId;
                ViewBag.selectedSemName = semester.Title + "" + semester.Year;
                ViewBag.semesterStatus = semester.Status;
                ViewBag.subjectList = subjectList;
                ViewBag.teacherList = teacherList;
            }

            return View(courses);
        }

        public ActionResult TestSemesterManagement(int semesterId = -1)
        {
            List<IConvertible[]> courses = new List<IConvertible[]>();
            using (var context = new DB_Finance_AcademicEntities())
            {
                int? status = null;
                //DateTime startDate, endDate;
                var semester = semesterId == -1 ? context.Semesters.OrderByDescending(q => q.Year).ThenByDescending(q => q.SemesterInYear).FirstOrDefault() : context.Semesters.Find(semesterId);

                //startDate = semester.StartDate.Value;
                //endDate = semester.EndDate.Value;

                courses = context.Courses.Where(q => q.SemesterId == semester.Id).AsEnumerable()
                    .Select(q => new IConvertible[]
                    {
                         q.Id,
                         q.Subject.SubjectName,
                         q.Subject.SubjectCode,
                         q.ClassName,
                         q.StartDate.Value,
                        q.EndDate.Value,
                        Enum.GetName(typeof(CourseStatus), q.Status == null ? 0 : q.Status.Value)
                    }).ToList();


            }

            return Json(new
            {
                success = true,
                iTotalRecords = courses.Count(),
                iTotalDisplayRecords = courses.Count(),
                aaData = courses
            }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult UnlockSemester(int semesterId)
        {
            using (var context = new DB_Finance_AcademicEntities())
            {
                var semester = context.Semesters.Where(q => q.Id == semesterId).SingleOrDefault();
                var courseList = context.Courses.Where(q => q.SemesterId == semesterId).ToList();
                //foreach (var course in courseList)
                //{
                //    course.Status = (int)CourseStatus.Open;
                //}
                semester.Status = (int)SememsterStatus.Open;
                context.SaveChanges();
            }

            return Json("Index");

        }


        private bool CreateCourse(string courseName,
                string className, DateTime startDate, DateTime endDate,
                int subjectId, int teacherId, int semesterId, int status)
        {
            try
            {

                using (var context = new DB_Finance_AcademicEntities())
                {
                    context.Courses.Add(new Course()
                    {
                        CourseName = courseName,
                        ClassName = className,
                        StartDate = startDate,
                        EndDate = endDate,
                        SubjectId = subjectId,
                        TeacherId = teacherId,
                        SemesterId = semesterId,
                        Status = status
                    });
                    context.SaveChanges();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return false;
            }

            return true;
        }

        [HttpPost]
        public ActionResult CreateCourse(string courseName,
            string className, string startDate, string endDate,
            int subjectId, int teacherId, int semesterId)
        {
            var startD = DateTime.ParseExact(startDate, "dd/MM/yyyy", CultureInfo.InvariantCulture);
            var endD = DateTime.ParseExact(endDate, "dd/MM/yyyy", CultureInfo.InvariantCulture);


            CreateCourse(courseName.Trim(), className.Trim(), startD, endD, subjectId, teacherId, semesterId, (int)CourseStatus.New);




            return RedirectToAction("Index", new { semesterId = semesterId });
        }

        [HttpPost]
        public ActionResult LockAllCourseForTrainingMangement(int semesterId, string returnUrl)
        {
            using (var context = new DB_Finance_AcademicEntities())
            {
                var semester = context.Semesters.Where(q => q.Id == semesterId).SingleOrDefault();
                if (semester.Status != (int)SememsterStatus.Closed)
                {
                    var courseList = context.Courses.Where(q => q.SemesterId == semesterId).ToList();
                    foreach (var course in courseList)
                    {
                        course.Status = (int)CourseStatus.Closed;
                    }
                    semester.Status = (int)SememsterStatus.Closed;
                    context.SaveChanges();
                }
            }
            return RedirectToAction("Index");
        }

        public ActionResult GetMyData()
        {
            using (var context = new DB_Finance_AcademicEntities())
            {
                var list = context.Courses.Select(q => new MyData
                {
                    CourseName = q.CourseName,
                    ClassName = q.ClassName,
                    SubjectName = q.Subject.SubjectName
                }).ToList();
                return View(list);
            }
        }


        public class MyData
        {
            public String CourseName { get; set; }
            public String ClassName { get; set; }
            public String SubjectName { get; set; }
        }
    }
}