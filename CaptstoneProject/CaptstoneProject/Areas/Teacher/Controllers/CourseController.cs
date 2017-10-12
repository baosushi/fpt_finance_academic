using CaptstoneProject.Controllers;
using CaptstoneProject.Models;
using DataService.Model;
using OfficeOpenXml;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using static CaptstoneProject.Models.AreaViewModel;

namespace CaptstoneProject.Areas.Teacher.Controllers
{
    [Authorize(Roles = "Teacher")]
    public class CourseController : MyBaseController
    {
        // GET: Teacher/Course
        public ActionResult Index(int semesterId = -1)
        {
            this.Session["loginName"] = "phuonglhk";
            var loginName = (string)Session["loginName"];
            List<CourseRecordViewModel> courses = new List<CourseRecordViewModel>();
            using (var context = new DB_Finance_AcademicEntities())
            {
                //DateTime startDate, endDate;
                var semester = semesterId == -1 ? context.Semesters.OrderByDescending(q => q.Year).ThenByDescending(q => q.SemesterInYear).FirstOrDefault() : context.Semesters.Find(semesterId);

                //startDate = semester.StartDate.Value;
                //endDate = semester.EndDate.Value;

                courses = context.Teachers.Where(q => q.LoginName == loginName).FirstOrDefault()
                    .Courses.Where(q => q.SemesterId == semester.Id)
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
                ViewBag.selectedSem = semester.Id;
            }


            return View(courses);
        }

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
                            Average = q.Average != null ? q.Average.ToString() : "N/A",
                            MarksComponent = q.StudentCourseMarks.ToList(),
                            Status = Enum.GetName(typeof(StudentCourseStatus),q.Status.Value)
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
                            CourseId = courseId,
                            ComponentNames = columns,
                            StudentInCourse = data,
                            Semester = semester.Title + " " + semester.Year,
                            SubCode = course.Subject.SubjectCode,
                            SubName = course.Subject.SubjectName,
                            isEditable = course.Status == (int)CourseStatus.Open? true: false
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

        [HttpPost]
        public ActionResult DownloadTemplate(int courseId)
        {
            MemoryStream ms = new MemoryStream();

            using (var context = new DB_Finance_AcademicEntities())
            {
                var course = context.Courses.Find(courseId);
                var fileName = course.ClassName + " - " + course.CourseName;

                using (ExcelPackage package = new ExcelPackage(ms))
                {
                    #region Excel format
                    ExcelWorksheet ws = package.Workbook.Worksheets.Add("Sheet1");
                    char StartHeaderChar = 'A';
                    int StartHeaderNumber = 1;
                    #region Headers
                    ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = "No";
                    ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = "Student ID";
                    ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = "Student login";
                    ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = "Name";

                    foreach (var component in course.CourseMarks)
                    {
                        ws.Cells["" + (StartHeaderChar) + (StartHeaderNumber + 1)].Value = component.Percentage / 100;
                        ws.Cells["" + (StartHeaderChar) + (StartHeaderNumber + 1)].Style.Numberformat.Format = "0%";
                        ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = component.ComponentName;
                    }

                    var EndHeaderChar = --StartHeaderChar;
                    var EndHeaderNumber = StartHeaderNumber;
                    StartHeaderChar = 'A';
                    StartHeaderNumber = 1;
                    #endregion
                    #region Header styling
                    ws.Cells["" + StartHeaderChar + StartHeaderNumber.ToString() +
                    ":" + EndHeaderChar + EndHeaderNumber.ToString()].Style.Font.Bold = true;
                    //ws.Cells["" + StartHeaderChar + StartHeaderNumber.ToString() +
                    //    ":" + EndHeaderChar + EndHeaderNumber.ToString()].AutoFitColumns();
                    //ws.Cells["" + StartHeaderChar + StartHeaderNumber.ToString() +
                    //    ":" + EndHeaderChar + EndHeaderNumber.ToString()]
                    //    .Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                    //ws.Cells["" + StartHeaderChar + StartHeaderNumber.ToString() +
                    //    ":" + EndHeaderChar + EndHeaderNumber.ToString()]
                    //    .Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.GreenYellow);
                    //ws.View.FreezePanes(2, 1);

                    StartHeaderNumber++;
                    #endregion
                    #region Set values for available fields
                    var count = 1;
                    foreach (var student in course.StudentInCourses)
                    {
                        ws.Cells["" + (StartHeaderChar++) + (++StartHeaderNumber)].Value = count++;
                        ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = student.Student.StudentCode;
                        ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = student.Student.LoginName;
                        ws.Cells["" + (StartHeaderChar) + (StartHeaderNumber)].Value = student.Student.LoginName;
                        //foreach(var mark in student.StudentCourseMarks)
                        //{
                        //    ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = mark.Mark.HasValue ? mark.Mark.Value : -1;
                        //}
                        StartHeaderChar = 'A';
                    }
                    fileName += ".xlsx";

                    StartHeaderNumber = 1;
                    ws.Cells.AutoFitColumns();
                    //ws.Cells["" + StartHeaderChar + StartHeaderNumber.ToString() +
                    //":" + EndHeaderChar + EndHeaderNumber.ToString()].Style.Border.BorderAround(OfficeOpenXml.Style.ExcelBorderStyle.Thin);
                    #endregion

                    #endregion

                    package.SaveAs(ms);
                    ms.Seek(0, SeekOrigin.Begin);
                    var contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                    return this.File(ms, contentType, fileName);
                }
            }
        }

        [HttpPost]
        public ActionResult UploadExcel(int courseId)
        {
            try
            {
                if (Request.Files.Count > 0)
                {
                    using (var context = new DB_Finance_AcademicEntities())
                    {
                        foreach (string file in Request.Files)
                        {
                            var fileContent = Request.Files[file];

                            var course = context.Courses.Find(courseId);

                            var subjectCode = course.Subject.SubjectCode;
                            var className = course.ClassName;
                            if (fileContent != null && fileContent.ContentLength > 0)
                            {
                                var stream = fileContent.InputStream;

                                using (ExcelPackage package = new ExcelPackage(stream))
                                {
                                    var ws = package.Workbook.Worksheets.First();
                                    var totalCol = ws.Dimension.Columns;
                                    var totalRow = ws.Dimension.Rows;
                                    var studentCodeCol = 2;
                                    var titleRow = 1;
                                    var firstRecordRow = 3;

                                    int tempNo = 0;
                                    for (int i = firstRecordRow; int.TryParse(ws.Cells[i, 1].Text.Trim(), out tempNo); i++)
                                    {
                                        var studentCode = ws.Cells[i, studentCodeCol].Text.Trim().ToUpper();
                                        var studentInCourse = context.StudentInCourses.Where(q => q.Student.StudentCode.ToUpper().Equals(studentCode)).FirstOrDefault();

                                        if (studentInCourse != null)
                                        {
                                            double average = 0;
                                            for (var j = 5; j <= totalCol; j++)
                                            {

                                                double value = 0;
                                                if (double.TryParse(ws.Cells[i, j].Text.Trim(), out value))
                                                {

                                                    StudentCourseMark studentCourseMark = null;

                                                    var component = course.CourseMarks.Where(q => q.ComponentName.Contains(ws.Cells[titleRow, j].Text.Trim())).FirstOrDefault();

                                                    studentCourseMark = context.StudentCourseMarks.
                                                        Where(q => q.StudentInCourseId.Equals(studentInCourse.Id) && q.CourseMarkId.Equals(component.Id)).FirstOrDefault();

                                                    var recordExisted = true;
                                                    //remake null check
                                                    if (studentCourseMark == null)
                                                    {
                                                        studentCourseMark = context.StudentCourseMarks.Create();
                                                        recordExisted = false;
                                                    }


                                                    studentCourseMark.Mark = value;
                                                    studentCourseMark.StudentInCourseId = studentInCourse.Id;
                                                    if (component != null)
                                                    {
                                                        average += studentCourseMark.Mark.Value * component.Percentage / 100;
                                                        studentCourseMark.CourseMarkId = component.Id;

                                                        if(!recordExisted)
                                                        {
                                                            context.StudentCourseMarks.Add(studentCourseMark);
                                                        }
                                                    }
                                                    var FE = course.CourseMarks.Where(q => q.ComponentName.Equals("FE")).FirstOrDefault();
                                                    var RE = course.CourseMarks.Where(q => q.ComponentName.Equals("RE")).FirstOrDefault();
                                                    var studentCourseMarkFE = context.StudentCourseMarks.
                                                        Where(q => q.StudentInCourseId.Equals(studentInCourse.Id) && q.CourseMarkId.Equals(FE.Id)).FirstOrDefault();
                                                    var studentCourseMarkRE = context.StudentCourseMarks.
                                                        Where(q => q.StudentInCourseId.Equals(studentInCourse.Id) && q.CourseMarkId.Equals(RE.Id)).FirstOrDefault();
                                                    //remake null check

                                                    if (studentCourseMarkFE == null)
                                                    {
                                                        studentCourseMarkFE = context.StudentCourseMarks.Create();
                                                        context.StudentCourseMarks.Add(studentCourseMarkFE);
                                                    }
                                                    if (studentCourseMarkRE == null)
                                                    {
                                                        studentCourseMarkRE = context.StudentCourseMarks.Create();
                                                        context.StudentCourseMarks.Add(studentCourseMarkRE);
                                                    }
                                                    studentCourseMarkFE.StudentInCourseId = studentInCourse.Id;
                                                    studentCourseMarkRE.StudentInCourseId = studentInCourse.Id;
                                                    studentCourseMarkFE.CourseMarkId = FE.Id;
                                                    studentCourseMarkRE.CourseMarkId = RE.Id;
                                                    studentCourseMarkRE.Mark = null;
                                                    studentCourseMarkFE.Mark = null;
                                                }
                                            }

                                            studentInCourse.Average = average;
                                            studentInCourse.Status = 1;
                                        }

                                        context.SaveChanges();
                                    }
                                }
                            }
                        }
                    }
                }
                else
                {
                    return Json(new { success = false, message = "No file has been uploaded" });
                }
            }
            catch (Exception e)
            {
                Response.StatusCode = (int)HttpStatusCode.BadRequest;
                return Json(new { success = false, message = e.Message });
            }

            return Json(new { success = true, message = "File uploaded successfully" });
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

                var studentMarks = context.StudentCourseMarks.Where(q => q.StudentInCourseId == studentId).ToList();

                var lengh = markList.Count();
                double average = 0;
                for (int i = 0; i < lengh; i++)
                {
                    var compId = int.Parse(markList[i].Name);
                    var mark = double.Parse(markList[i].Value);
                    var compPer = coursePer.Where(q => q.Id == compId).Select(q => q.Per).FirstOrDefault();
                    average += mark * compPer;
                    foreach (var item in studentMarks)
                    {
                        if (item.CourseMarkId == compId)
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
            if(String.IsNullOrEmpty(studentCode))
            {
                return RedirectToAction("Index");
            }
            using (var context = new DB_Finance_AcademicEntities())
            {
                var course = context.Courses.Find(courseId);
                var status = course.Status;
                if (status != (int)CourseStatus.Open)
                {
                    return RedirectToAction("Index", "Home");
                }
                var student = course.StudentInCourses.Where(q => q.Student.StudentCode.Equals(studentCode)).Select(q => new StudentEditViewModel
                {
                    Id = q.Id,
                    Class = course.ClassName,
                    CourseId = courseId,
                    Name = q.Student.LoginName,
                    Code = q.Student.StudentCode,
                    Average = q.Average != null ? q.Average.ToString() : "N/A",
                    MarksComponent = q.StudentCourseMarks.ToList(),

                }).FirstOrDefault();
                student.ComponentNames = course.CourseMarks.Select(q => q.ComponentName).ToList();
                return View("EditMark", student);

            }

        }
    }
	
	
}