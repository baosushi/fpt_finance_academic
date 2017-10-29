using CaptstoneProject.Controllers;
using CaptstoneProject.Models;
using DataService.Model;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
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
            using (var context = new DB_Finance_AcademicEntities())
            {
                var subjectGroupList = context.SubjectGroups.Select(q => new SelectListItem
                {
                    Text = q.Name,
                    Value = q.Id.ToString()
                }).ToList();
                var semesterList = context.Semesters.OrderByDescending(q => q.Year)
                    .ThenByDescending(q =>q.SemesterInYear).Select(q => new SelectListItem
                {
                    Text = q.Title + " " + q.Year,
                    Value = q.Id.ToString()
                }).ToList();
                ViewBag.SubjectGroups = subjectGroupList;
                ViewBag.SemesterList = semesterList;
            }
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
                            IsPublish = course.Status == (int)CourseStatus.InProgress ? (int)FinalEditStatus.SubmitComponent :
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
            try
            {
                using (var context = new DB_Finance_AcademicEntities())
                {
                    var semester = semesterId == -1 ? context.Semesters.OrderByDescending(q => q.Year).
                        ThenByDescending(q => q.SemesterInYear).
                        FirstOrDefault() : context.Semesters.Find(semesterId);

                    var currentIdList = context.Courses.Where(q => q.SemesterId == semester.Id).
                        Select(q => (int?)q.Subject.SubjectGroup.Id).ToList();
                    HashSet<int?> currentTempList = new HashSet<int?>(currentIdList); // take unique subjectGroup Id, remove redundant
                    List<int?> currentSubjectGroupIds = currentTempList.ToList();// easy to interact


                    List<string> subjectGroupNames = new List<string>();
                    List<int> sumPassStudents = new List<int>();
                    List<int> sumFailStudents = new List<int>();
                    for (int i = 0; i < currentSubjectGroupIds.Count(); i++)
                    {
                        var subjectGroupId = currentSubjectGroupIds[i];
                        if (subjectGroupId != null)
                        {
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
            catch (Exception e)
            {
                Response.StatusCode = (int)HttpStatusCode.BadRequest;
                return Json(new { success = false, message = e });
            }

        }

        [HttpPost]
        public ActionResult UnlockStudent(string studentCode, int courseId = -1)
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
            return RedirectToAction("CourseDetails", new { courseId = courseId });
        }

        public ActionResult GetTestResultCurrentSemester(int semesterId = -1)
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


        //get study statistic of every subject in subjectGroup

        public ActionResult StudyStatisticBySubjectGroup(int? subjectGroupId, int? semesterId = -1)
        {
            try
            {
                using (var context = new DB_Finance_AcademicEntities())
                {
                    var semester = semesterId == -1 ? context.Semesters.OrderByDescending(q => q.Year).
                        ThenByDescending(q => q.SemesterInYear).
                        FirstOrDefault() : context.Semesters.Find(semesterId);


                    List<int> currentSubjectIds = context.Courses.Where(q => q.SemesterId == semester.Id &&
                    q.Subject.SubjectGroup.Id == subjectGroupId).Select(q => q.SubjectId).Distinct().ToList();



                    //List<string> subjectNames = new List<string>();
                    //List<int> totalStudents = new List<int>();
                    //List<int> totalPassStudents = new List<int>();
                    //List<double> passPercentages = new List<double>();
                    //List<double> minMarks = new List<double>();
                    //List<double> maxMarks = new List<double>();

                    List<IConvertible[]> statisticList = new List<IConvertible[]>();

                    for (int i = 0; i < currentSubjectIds.Count(); i++)
                    {
                        var subjectId = currentSubjectIds[i];

                        //could use join table 
                        var list = context.Courses.Where(q => q.SubjectId == subjectId && q.SemesterId == semester.Id).
                            AsEnumerable().Select(q => new IConvertible[] {

                           q.StudentInCourses.Count(),//total
                           q.StudentInCourses.Where(a => a.Status  == (int)StudentInCourseStatus.Passed).Count(),// pass
                           q.StudentInCourses.Max(a => a.Average).Value, //max
                           q.StudentInCourses.Min(a => a.Average).Value //min

                        }).ToList();


                        var total = 0;
                        var totalPassed = 0;
                        double passPercent = 0;
                        double minMark = 0;
                        double maxMark = 0;
                        foreach (var item in list)
                        {
                            total += (int)item[0]; //total
                            totalPassed += (int)item[1]; //pass
                            if ((double)item[2] > maxMark)
                            {
                                maxMark = (double)item[2];
                            }
                            if ((double)item[3] < minMark)
                            {
                                minMark = (double)item[3];
                            }
                        }
                        passPercent = Math.Round((totalPassed * 1.0 / total * 100), 2);
                        var subjectName = context.Subjects.Where(q => q.Id == subjectId).Select(q => q.SubjectName).FirstOrDefault().ToString();
                        //subjectNames.Add(subjectName);
                        //totalStudents.Add(total);
                        //totalPassStudents.Add(totalPassed);
                        //maxMarks.Add(maxMark);
                        //minMarks.Add(minMark);
                        statisticList.Add(new IConvertible[]
                        {
                           subjectName,
                           total,
                           totalPassed,
                           passPercent,
                           maxMark,
                           minMark
                        });
                    }


                    return Json(new
                    {
                        success = true,
                        iTotalRecords = statisticList.Count(),
                        iTotalDisplayRecords = statisticList.Count(),
                        aaData = statisticList
                    }, JsonRequestBehavior.AllowGet);
                }

            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return Json(new { succcess = false, message = e });
            }
        }


        public FileResult ExportAllStudentMarkExcelbySemester(int? semesterId)
        {
            List<StudentMarkInSemester> result = null;
            string fileName = "Export_Mark_";
            var semesterName = "";
            //string semesterName = null;
            using (var context = new DB_Finance_AcademicEntities())
            {
                //semesterName = context.Semesters.Where(q => q.Id == semesterId).Select(q => q.Title).SingleOrDefault();
                result = (from c in context.Courses
                          join sc in context.StudentInCourses
                          on c.Id equals sc.CourseId
                          where c.Id == semesterId
                          select new StudentMarkInSemester
                          {
                              RollNumber = sc.StudentMajor.StudentCode,
                              SubjectCode = c.Subject.SubjectCode,
                              ClassName = c.ClassName,
                              AverageMark = sc.Average,
                              Status = Enum.GetName(typeof(StudentInCourseStatus), sc.Status == null ? 0 : sc.Status.Value)
                          }).OrderBy(q => q.RollNumber).ToList();

                semesterName = context.Semesters.Where(q => q.Id == semesterId).Select(q => q.Title + q.Year).FirstOrDefault();
                fileName += semesterName;
            }
            MemoryStream ms = new MemoryStream();
            ExcelPackage excelPack;
            using (excelPack = new ExcelPackage(ms))
            {

                ExcelWorksheet ws1 = excelPack.Workbook.Worksheets.Add("Sheet1");

                using (var range = ws1.Cells[1, 1, 1, 6])
                {
                    range.Style.Font.Bold = true;
                    range.Style.Font.Size = 11;
                }
                ws1.Cells[1, 1].Value = "SemesterName";
                ws1.Cells[1, 2].Value = "RollNumber";
                ws1.Cells[1, 3].Value = "SubjectCode";
                ws1.Cells[1, 4].Value = "ClassName";
                ws1.Cells[1, 5].Value = "AverageMark";
                ws1.Cells[1, 6].Value = "Status";

                for (int i = 0; i < result.Count; i++)
                {
                    ws1.Cells[i + 2, 1].Value = semesterName;
                    ws1.Cells[i + 2, 2].Value = result[i].RollNumber;
                    ws1.Cells[i + 2, 3].Value = result[i].SubjectCode;
                    ws1.Cells[i + 2, 4].Value = result[i].ClassName;
                    ws1.Cells[i + 2, 5].Value = result[i].AverageMark;
                    ws1.Cells[i + 2, 6].Value = result[i].Status;
                }


                var contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                excelPack.SaveAs(ms);
                ms.Seek(0, SeekOrigin.Begin);
                return File(ms, contentType, semesterName + ".xlsx");
            }
        }

        public class StudentMarkInSemester
        {
            public string SubjectCode { get; set; }
            public string ClassName { get; set; }
            public double? AverageMark { get; set; }
            public string Status { get; set; } // 0:Fail, 1:Success
            public string RollNumber { get; set; } // studentRollNumber

        }



        public class MyData
        {
            public String CourseName { get; set; }
            public String ClassName { get; set; }
            public String SubjectName { get; set; }
        }
        public class SubjectStudyStatistic
        {
            public string SubjectName { get; set; }
            public int TotalStudent { get; set; }
            public int TotalPassedStudent { get; set; }
            public double PassPecentage { get; set; }
            public double MaxMark { get; set; }
            public double MinMark { get; set; }
        }


        public class CourseTestResult
        {
            public int CourseId { get; set; }
            //public String
        }
    }
}