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
            }

            return View(courses);
        }

        public ActionResult CourseDetails(int courseId)
        {
            ViewBag.CourseId = courseId;
            return View();
        }

        [HttpPost]
        public async Task<ActionResult> GetCourseDetails(int courseId)
        {
            try
            {
                var loginName = (string)Session["loginName"];

                using (var context = new DB_Finance_AcademicEntities())
                {
                    var course = await context.Courses.FindAsync(courseId);

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
                        });

                        var datatest = course.StudentInCourses.Select(q => new IConvertible[] {
                            q.Student.StudentCode,
                            q.Student.LoginName,
                            //q.StudentCourseMarks.Select(s => s.Mark),
                            q.Average,
                            q.Status
                        });

                        var columns = new List<string[]>();
                        foreach(var component in data.ElementAt(0).MarksComponent)
                        {
                            columns.Add(new string[] { "title", component.CourseMark.ComponentName });
                        }

                        return Json(new { success = true, columns = columns, data = datatest });
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
}