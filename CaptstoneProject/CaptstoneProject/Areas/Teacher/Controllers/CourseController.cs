using DataService.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Entity;
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

                startDate = semester.StartDate.Value;
                endDate = semester.EndDate.Value;

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

        public ActionResult CourseDetails(int courseId, int semesterId)
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
                        var semester = semesterId == -1 ? context.Semesters.OrderByDescending(q => q.Year).ThenByDescending(q => q.SemesterInYear).FirstOrDefault() : context.Semesters.Find(semesterId);
                        var model = new CourseDetailsViewModel
                        {
                            ComponentNames = columns,
                            StudentInCourse = data,
                            Semester = semester.Title + " " + semester.Year,
                            SubCode = course.Subject.SubjectCode,
                            SubName = course.Subject.SubjectName,
                            CourseId = course.Id,
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
            using (var context = new DB_Finance_AcademicEntities())
            {

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
        public JsonResult GetEdit(List<MarkComp> markList, int courseId, int studentId)
        {
            using (var context = new DB_Finance_AcademicEntities())
            {
                var coursePer = context.CourseMarks.Where(q => q.CourseId == courseId).Select(q => new PerComp
                {
                    CompName = q.ComponentName,
                    Id = q.Id,
                    Per = q.Percentage,
                }).ToList();

                var studentMarks = context.StudentCourseMarks.Where(q => q.StudentInCourseId==studentId).ToList();

                var lengh = markList.Count();
                double average=0;
                for (int i = 0; i < lengh; i++)
                {
                    var compId = int.Parse(markList[i].name);
                    var mark = double.Parse(markList[i].value);
                    var compPer = coursePer.Where(q => q.Id == compId).Select(q => q.Per).FirstOrDefault();
                    average += mark * compPer;
                    foreach(var item in studentMarks)
                    {
                        if(item.CourseMarkId == compId)
                        {
                            item.Mark = mark;
                            
                        }
                    }
                }
                average = average / 100;
                var studentInCourse = context.StudentInCourses.Where(q => q.Id == studentId).FirstOrDefault();
                studentInCourse.Average = average;
                context.SaveChanges();
                return null;
            }
        }

        public ActionResult EditMarks(string studentCode, int courseId)
        {
            using (var context = new DB_Finance_AcademicEntities())
            {
                var course = context.Courses.Find(courseId);
                var student = course.StudentInCourses.Where(q => q.Student.StudentCode.Equals(studentCode)).Select(q => new StudentEditViewModel
                {
                    Id=q.Id,
                    Class = course.ClassName,
                    CourseId = courseId,
                    Name = q.Student.LoginName,
                    Code = q.Student.StudentCode,
                    MarksComponent = q.StudentCourseMarks.ToList(),
                    
                }).FirstOrDefault();
                student.ComponentNames = course.CourseMarks.Select(q => q.ComponentName).ToList();
                return View("EditMark", student);

            }

        }
    }

    public class MarkComp
    {
        public string name { get; set; }
        public string value { get; set; }
        public int id { get; set; }
    }
    public class PerComp
    {
        public int Id { get; set; }
        public string CompName { get; set; }
        public double Per { get; set; }
    }

    public class StudentEditViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Code { get; set; }
        public int CourseId { get; set; }
        public string Class { get; set; }
        public List<string> ComponentNames { get; set; }
        public List<StudentCourseMark> MarksComponent { get; set; }
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
        public string SubName { get; set; }
        public string SubCode { get; set; }
        public string Semester { get; set; }
        public int CourseId { get; set; }
    }
}