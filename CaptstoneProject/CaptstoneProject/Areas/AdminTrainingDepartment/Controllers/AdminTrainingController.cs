using CaptstoneProject.Controllers;
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
    public class AdminTrainingController : MyBaseController
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

        public ActionResult CourseDetails(int courseId)
        {
            ViewBag.CourseId = courseId;
            try
            {

                using (var context = new DB_Finance_AcademicEntities())
                {
                    var course = context.Courses.Find(courseId);

                    if (course != null)
                    {
                        ViewBag.ClassName = course.ClassName;

                        var data = course.StudentInCourses.Select(q => new StudentInCourseViewModel
                        {
                            UserName = q.StudentMajor.LoginName,
                            StudentCode = q.StudentMajor.StudentCode,
                            Average = q.Average != null ? q.Average.ToString() : "-",
                            MarksComponent = q.StudentCourseMarks.ToList(),
                            Status = q.Status == null ? 0 : q.Status.Value
                        }).ToList();

                        //var datatest = course.StudentInCourses.Select(q => new IConvertible[] {
                        //    q.Student.StudentCode,
                        //    q.Student.LoginName,
                        //    //q.StudentCourseMarks.Select(s => s.Mark),
                        //    q.Average,
                        //    q.Status
                        //});

                        var columns = context.CourseMarks.Where(q => q.CourseId == courseId).Select(q => q.ComponentName).ToList();

                        var model = new CourseDetailsViewModel
                        {
                            CourseId = courseId,
                            ComponentNames = columns,
                            StudentInCourse = data,
                            Semester = course.Semester.Title + " " + course.Semester.Year,
                            SubCode = course.Subject.SubjectCode,
                            SubName = course.Subject.SubjectName,
                            //IsEditable = course.Status != (int)CourseStatus.Submitted ? false : true
                            IsPublish = course.Status == (int)CourseStatus.InProgress ?  (int)FinalEditStatus.SubmitComponent :
                            course.Status == (int)CourseStatus.Submitted ? (int)FinalEditStatus.EditFinal : 
                            course.Status == (int)CourseStatus.FirstPublish ? (int)FinalEditStatus.EditRetake : 
                            (int)FinalEditStatus.NoEdit,
                            StatusName = Enum.GetName(typeof(CourseStatus), course.Status == null ? 0 : course.Status.Value),
                        };

                        //return Json(new { success = true, columns = columns, data = data });
                        return View("CourseDetails", model);
                    }
                    else
                    {
                        return Json(new { success = false, message = "Access denied." }, JsonRequestBehavior.AllowGet);
                    }
                }
            }
            catch (Exception e)
            {
                return Json(new { success = false, message = e.Message }, JsonRequestBehavior.AllowGet);
            }
        }


        public ActionResult TestSemesterManagement(int semesterId = -1)
        {
            List<IConvertible[]> courses = new List<IConvertible[]>();
            using (var context = new DB_Finance_AcademicEntities())
            {
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

        //Get result data of SubjectGroup of current and previous semester for chart
        public ActionResult GetTestResultBySubjectGroup(int semesterId = -1)
        {
            using (var context = new DB_Finance_AcademicEntities())
            {
                var semester = semesterId == -1 ? context.Semesters.OrderByDescending(q => q.Year).
                    ThenByDescending(q => q.SemesterInYear).
                    FirstOrDefault() : context.Semesters.Find(semesterId);

                var currentIdList = context.Courses.Where(q => q.SemesterId == semester.Id).
                    Select(q => q.Subject.SubjectGroup.Id).ToList();
                HashSet<int> currentTempList = new HashSet<int>(currentIdList); // take unique subjectGroup Id, remove redundant
                List<int> currentSubjectGroupIds = currentTempList.ToList();// easy to interact


                List<string> subjectGroupNames = new List<string>();
                List<int> sumPassStudents = new List<int>();
                List<int> sumFailStudents = new List<int>();
                for (int i = 0; i < currentSubjectGroupIds.Count(); i++)
                {
                    var subjectGroupId = currentSubjectGroupIds[i];
                    var list = context.Courses.Where(q => q.Id == semester.Id &&
                     q.Subject.SubjectGroup.Id == subjectGroupId).AsEnumerable().Select(q => new IConvertible[]{
                        q.StudentInCourses.Select(a => a.Status == (int)StudentInCourseStatus.Passed).Count(),
                        q.StudentInCourses.Select(a => a.Status == (int)StudentInCourseStatus.Failed).Count(),
                        q.StudentInCourses.Select(a => a.Status == (int)StudentInCourseStatus.Studying).Count(),

                     }).ToList();
                    int totalPassed = 0;
                    int totalFailed = 0;
                    int totalStudying = 0;
                    foreach (var item in list)
                    {
                        totalPassed += (int)item[0];
                        totalFailed += (int)item[1];
                        totalStudying += (int)item[2];
                    }
                    var subjectGroupName = context.SubjectGroups.Where(q => q.Id == subjectGroupId)
                        .Select(q => q.Name).FirstOrDefault();
                    subjectGroupNames.Add(subjectGroupName);
                    sumPassStudents.Add(totalPassed);
                    sumFailStudents.Add(totalFailed);
                }


                return Json(new
                {
                    success = true,
                    subjectGroupNameList = subjectGroupNames,
                    passList = sumPassStudents,
                    failList = sumFailStudents,
                    semesterName = semester.Title + semester.Year
                }, JsonRequestBehavior.AllowGet);
            }

        }

        [HttpPost]
        public ActionResult UnlockStudent(string studentCode ,int courseId = -1)
        {
            if (courseId == -1 || studentCode == null || studentCode.Length == 0)
            {
                return Json(new { success = false, message = "Error! No student found" });

            }
            try
            {
                using (var context = new DB_Finance_AcademicEntities())
                {
                    var studentInCourse = context.StudentInCourses.Where(q => q.CourseId == courseId && q.StudentMajor.StudentCode == studentCode).FirstOrDefault();
                    studentInCourse.Status = (int)StudentInCourseStatus.Issued;
                    context.SaveChanges();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return Json(new { success = false, message = e });
            }
            return RedirectToAction("CourseDetails", new { courseId = courseId});
        }

        public ActionResult GetTestResultCurrentSemester(int semesterId = 3)
        {
            try
            {
                using (var context = new DB_Finance_AcademicEntities())
                {
                    var semester = semesterId == -1 ? context.Semesters.OrderByDescending(q => q.Year).
                        ThenByDescending(q => q.SemesterInYear).
                        FirstOrDefault() : context.Semesters.Find(semesterId);


                    var currentIdList = context.Courses.Where(q => q.SemesterId == semester.Id).Select(q => q.SubjectId).ToList();
                    HashSet<int> tempList = new HashSet<int>(currentIdList); // take unique subjectGroup Id, remove redundant
                    List<int> currentSubjectIds = tempList.ToList();



                    List<string> subjectNames = new List<string>();
                    List<int> sumPassStudents = new List<int>();
                    List<int> sumFailStudents = new List<int>();

                    for (int i = 0; i < currentSubjectIds.Count(); i++)
                    {
                        var subjectId = currentSubjectIds[i];
                        var list = context.Courses.Where(q => q.SubjectId == subjectId && q.SemesterId == semester.Id).
                            AsEnumerable().Select(q => new IConvertible[] {

                           q.StudentInCourses.Where(a => a.Status  == (int)StudentInCourseStatus.Passed).Count(),
                           q.StudentInCourses.Where(a => a.Status  == (int)StudentInCourseStatus.Failed).Count(),
                            q.StudentInCourses.Where(a => a.Status  == (int)StudentInCourseStatus.Studying).Count(),

                        }).ToList();
                        var totalPassed = 0;
                        var totalFailed = 0;
                        var totalStudying = 0;
                        foreach (var item in list)
                        {
                            totalPassed += (int)item[0]; //pass
                            totalFailed += (int)item[1]; //fail
                            totalStudying += (int)item[2];//studying
                        }
                        var subjectName = context.Subjects.Where(q => q.Id == subjectId).Select(q => q.SubjectName).FirstOrDefault().ToString();
                        subjectNames.Add(subjectName);
                        sumPassStudents.Add(totalPassed);
                        sumFailStudents.Add(totalFailed);
                    }


                    return Json(new
                    {
                        success = true,
                        subjectNameList = subjectNames,
                        passList = sumPassStudents,
                        failList = sumFailStudents,
                        semesterName = semester.Title + semester.Year
                    }, JsonRequestBehavior.AllowGet);
                }

            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return Json(new { succcess = false, message = e });
            }

        }



        public class MyData
        {
            public String CourseName { get; set; }
            public String ClassName { get; set; }
            public String SubjectName { get; set; }
        }

        public class CourseTestResult
        {
            public int CourseId { get; set; }
            //public String
        }
    }
}