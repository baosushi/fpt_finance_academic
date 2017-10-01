using DataService.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace CaptstoneProject.Areas.Teacher.Controllers
{
    public class CourseController : Controller
    {
        // GET: Teacher/Course
        public ActionResult Index(int semesterId = -1)
        {
            this.Session["loginName"] = "phuonglhk";
            var loginName = (string)Session["loginName"];
            List<CourseRecordViewModel> courses = new List<CourseRecordViewModel>();
            using (var context = new DB_Finance_AcademicEntities())
            {
                DateTime startDate, endDate;
                var semester = semesterId == -1 ? context.Semesters.OrderByDescending(q => q.Year).ThenByDescending(q => q.SemesterInYear).FirstOrDefault() : context.Semesters.Find(semesterId);

                startDate = semester.SemesterBlocks.Select(q => q.StartDate).Min().Value;
                endDate = semester.SemesterBlocks.Select(q => q.EndDate).Max().Value;

                courses = context.Teachers.Where(q => q.LoginName == loginName).FirstOrDefault()
                    .Courses.Where(q => q.StartDate >= startDate && q.EndDate <= endDate)
                    .Select(q => new CourseRecordViewModel
                    {
                        CourseId = q.Id,
                        Name = q.Subject.SubjectName,
                        Code = q.Subject.SubjectCode,
                        Class = q.ClassName,
                        StartDate = q.StartDate.Value,
                        EndDate = q.EndDate.Value
                    }).ToList();
                var semesters = context.Semesters.OrderByDescending(q => q.Year).ThenByDescending(q => q.SemesterInYear);
                var semesterList = semesters.Select(q => new SelectListItem
                {
                    Text = q.Title + " " + q.Year,
                    Value = q.Id.ToString(),
                }).ToList();
                ViewBag.semList = semesterList;
                ViewBag.selectedSem = semesterId;
            }


            return View(courses);
        }

        //public ActionResult CourseDetails(int courseId)
        //{
        //    ViewBag.CourseId = courseId;
        //    return View();
        //}
        
        public ActionResult CourseDetails(int courseId)
        {
            ViewBag.CourseId = courseId;
            try
            {
                var loginName = (string)Session["loginName"];

                using (var context = new DB_Finance_AcademicEntities())
                {
                    var course = context.Courses.Find(courseId);

                    if (course != null && course.Teacher.LoginName == loginName)
                    {
                        ViewBag.ClassName = course.ClassName;

                        var data = course.StudentInCourses.Select(q => new StudentInCourseViewModel
                        {
                            UserName = q.Student.LoginName,
                            StudentCode = q.Student.StudentCode,
                            Average = q.Average.Value,
                            MarksComponent = q.StudentCourseMarks.ToList(),
                            Status = q.Status
                        }).ToList();

                        //var datatest = course.StudentInCourses.Select(q => new IConvertible[] {
                        //    q.Student.StudentCode,
                        //    q.Student.LoginName,
                        //    //q.StudentCourseMarks.Select(s => s.Mark),
                        //    q.Average,
                        //    q.Status
                        //});

                        var columns = data.ElementAt(0).MarksComponent.Select(q => q.CourseMark.ComponentName).ToList();

                        var model = new CourseDetailsViewModel {
                            ComponentNames = columns,
                            StudentInCourse = data
                        };

                        //return Json(new { success = true, columns = columns, data = data });
                        return View("CourseDetails", model);
                    }
                    else
                    {
                        return Json(new { success = false, message = "Access denied." });
                    }
                }
            }
            catch (Exception e)
            {
                return Json(new { success = false, message = e.Message });
            }
        }

    
        public JsonResult GetSemesters()
        {
            var context = new DB_Finance_AcademicEntities();

            var semesters = context.Semesters.OrderByDescending(q => q.Year).ThenByDescending(q => q.SemesterInYear);
            var semesterList = semesters.Select(q => new
            {
                q.Id,
                q.Title,
                q.Year,
            }).ToList();

            return Json(new
            {
                semList = semesterList,
            });
        }
    }


    public class CourseRecordViewModel
    {
        public int CourseId { get; set; }
        public string Name { get; set; }
        public string Code { get; set; }
        public string Class { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }

    public class StudentInCourseViewModel
    {
        public string UserName { get; set; }
        public string StudentCode { get; set; }
        public double Average { get; set; }
        public List<StudentCourseMark> MarksComponent { get; set; }
        public string Status { get; set; }
    }

    public class CourseDetailsViewModel
    {
        public List<StudentInCourseViewModel> StudentInCourse { get; set; }
        public List<string> ComponentNames { get; set; }
    }
}