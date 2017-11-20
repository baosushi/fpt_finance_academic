using CaptstoneProject.Controllers;
using CaptstoneProject.Models;
using DataService.Model;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
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
using Microsoft.Office.Interop.Excel;
using Microsoft.Ajax.Utilities;
using System.Globalization;
using Microsoft.AspNet.Identity.Owin;

namespace CaptstoneProject.Areas.TrainingManagement.Controllers
{
    [Authorize(Roles = "Training Management")]
    public class ManagementController : MyBaseController
    {
        public ActionResult Index(int semesterId = -1)
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
                        StartDate = q.StartDate.HasValue ? q.StartDate.Value : new DateTime(),
                        EndDate = q.EndDate.HasValue ? q.EndDate.Value : new DateTime(),
                        Status = Enum.GetName(typeof(CourseStatus), q.Status == null ? 0 : q.Status.Value)
                    }).ToList();


                var semesters = context.Semesters.OrderByDescending(q => q.Year).ThenByDescending(q => q.SemesterInYear);

                var semesterList = semesters.Select(q => new SelectListItem
                {
                    Text = q.Title + " " + q.Year,
                    Value = q.Id.ToString(),
                }).ToList();
                ViewBag.semList = semesterList;
                ViewBag.selectedSem = semesterId;
                ViewBag.selectedSemName = semester.Title + "" + semester.Year;
                ViewBag.semesterStatus = semester.Status;
            }

            return View(courses);
        }

        public ActionResult CourseDetails(int courseId, int semesterId)
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
                            StudentName = q.StudentMajor.Student.Name,
                            StudentCode = q.StudentMajor.StudentCode,
                            Average = q.Average != null ? q.Average.ToString() : "-",
                            MarksComponent = q.StudentCourseMarks.ToList(),
                            Status = q.Status,
                            StatusName = Enum.GetName(typeof(StudentInCourseStatus), q.Status == null ? 0 : q.Status.Value)
                        }).ToList();

                        //var datatest = course.StudentInCourses.Select(q => new IConvertible[] {
                        //    q.Student.StudentCode,
                        //    q.Student.LoginName,
                        //    //q.StudentCourseMarks.Select(s => s.Mark),
                        //    q.Average,
                        //    q.Status
                        //});

                        var columns = context.CourseMarks.Where(q => q.CourseId == courseId).Select(q => q.ComponentName).ToList();
                        var components = context.CourseMarks.Where(q => q.CourseId == courseId).ToList();
                        List<int> finalColumns = new List<int>();
                        int i = 3;
                        foreach (var com in components)
                        {
                            if (com.IsFinal == true)
                            {
                                finalColumns.Add(i);
                            }
                            i++;
                        }
                        var semester = semesterId == -1 ? context.Semesters.OrderByDescending(q => q.Year).ThenByDescending(q => q.SemesterInYear).FirstOrDefault() : context.Semesters.Find(semesterId);
                        var model = new CourseDetailsViewModel
                        {
                            CourseId = courseId,
                            ComponentNames = columns,
                            StudentInCourse = data,
                            FinalCol = finalColumns,
                            Semester = semester.Title + " " + semester.Year,
                            SubCode = course.Subject.SubjectCode,
                            SubName = course.Subject.SubjectName,
                            //IsEditable = course.Status != (int)CourseStatus.Submitted ? false : true
                            IsPublish = course.Status == (int)CourseStatus.InProgress ? (int)FinalEditStatus.SubmitComponent : course.Status == (int)CourseStatus.Submitted ? (int)FinalEditStatus.EditFinal : course.Status == (int)CourseStatus.FirstPublish ? (int)FinalEditStatus.EditRetake : (int)FinalEditStatus.NoEdit,
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

        [HttpPost]
        public ActionResult LockAllCourseForTeacher(int semesterId, string returnUrl)
        {
            using (var context = new DB_Finance_AcademicEntities())
            {
                var semester = context.Semesters.Where(q => q.Id == semesterId).SingleOrDefault();
                if (semester.Status != (int)SememsterStatus.Closed)
                {
                    var courseList = context.Courses.Where(q => q.SemesterId == semesterId).ToList();
                    foreach (var course in courseList)
                    {
                        course.Status = (int)CourseStatus.Submitted;
                    }
                    context.SaveChanges();
                }

            }

            return RedirectToAction("Index");

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
                    //ws.Cells[0, 0].Style.WrapText = true;
                    //Image img = CaptstoneProject.Properties.Resources.img_logo_fe;
                    //ExcelPicture pic = ws.Drawings.AddPicture("FPTLogo", img);
                    //pic.From.Column = 0;
                    //pic.From.Row = 0;
                    //pic.SetSize(320, 240);
                    ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = "No";
                    ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = "StudentMajor ID";
                    ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = "StudentMajor login";
                    ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = "Name";

                    foreach (var component in course.CourseMarks)
                    {
                        if (component.IsFinal == null || component.IsFinal != true)
                        {
                            ws.Cells["" + (StartHeaderChar) + (StartHeaderNumber + 1)].Value = component.Percentage / 100;
                            ws.Cells["" + (StartHeaderChar) + (StartHeaderNumber + 1)].Style.Numberformat.Format = "0%";
                            ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = component.ComponentName;
                        }
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
                    foreach (var StudentMajor in course.StudentInCourses)
                    {
                        ws.Cells["" + (StartHeaderChar++) + (++StartHeaderNumber)].Value = count++;
                        ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = StudentMajor.StudentMajor.StudentCode;
                        ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = StudentMajor.StudentMajor.LoginName;
                        ws.Cells["" + (StartHeaderChar) + (StartHeaderNumber)].Value = StudentMajor.StudentMajor.LoginName;
                        foreach (var mark in StudentMajor.StudentCourseMarks)
                        {
                            if (!mark.CourseMark.IsFinal.HasValue || mark.CourseMark.IsFinal != true)
                            {
                                ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = mark.Mark.HasValue && mark.Mark.Value != -1 ? mark.Mark.Value.ToString() : "";
                            }
                        }
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
        public ActionResult DownloadFinalTemplate(int courseId, int isPublish)
        {
            MemoryStream ms = new MemoryStream();

            using (var context = new DB_Finance_AcademicEntities())
            {
                var course = context.Courses.Find(courseId);
                string fileName;
                if (isPublish == 0)
                {
                    fileName = "Final Exam " + course.Subject.SubjectCode + " - " + course.ClassName + " - " + course.CourseName;
                }
                else
                {
                    fileName = "Retake Exam " + course.Subject.SubjectCode + " - " + course.ClassName + " - " + course.CourseName;
                }
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

                    if (isPublish == 0)
                    {
                        foreach (var component in course.CourseMarks)
                        {
                            if (component.IsFinal != null && component.IsFinal == true && !component.ComponentName.Contains("2nd"))
                            {
                                ws.Cells["" + (StartHeaderChar) + (StartHeaderNumber + 1)].Value = component.Percentage / 100;
                                ws.Cells["" + (StartHeaderChar) + (StartHeaderNumber + 1)].Style.Numberformat.Format = "0%";
                                ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = component.ComponentName;
                            }
                        }
                    }
                    else
                    {
                        foreach (var component in course.CourseMarks)
                        {
                            if (component.IsFinal != null && component.IsFinal == true && component.ComponentName.Contains("2nd"))
                            {
                                ws.Cells["" + (StartHeaderChar) + (StartHeaderNumber + 1)].Value = component.Percentage / 100;
                                ws.Cells["" + (StartHeaderChar) + (StartHeaderNumber + 1)].Style.Numberformat.Format = "0%";
                                ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = component.ComponentName;
                            }
                        }
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
                        ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = student.StudentMajor.StudentCode;
                        ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = student.StudentMajor.LoginName;
                        ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = student.StudentMajor.Student.Name;
                        foreach (var mark in student.StudentCourseMarks)
                        {
                            if (!mark.CourseMark.IsFinal.HasValue && mark.CourseMark.IsFinal != true)
                            {
                                ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = mark.Mark.HasValue && mark.Mark.Value != -1 ? mark.Mark.Value.ToString("0.00") : "";
                            }
                        }
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
        [HttpPost]
        public ActionResult GetEdit(List<MarkComp> markList, int courseId, int studentId, string note)
        {
            using (var context = new DB_Finance_AcademicEntities())
            {
                var course = context.Courses.Find(courseId);
                var coursePer = course.CourseMarks.Select(q => new ComponentPercentage
                {
                    CompName = q.ComponentName,
                    Id = q.Id,
                    Per = q.Percentage,
                }).ToList();

                var studentInCourse = context.StudentInCourses.Find(studentId);
                var studentMarks = studentInCourse.StudentCourseMarks.ToList();

                double? average = 0;
                double mark = 0;
                foreach (var record in markList)
                {
                    var componentId = int.Parse(record.Name);
                    if (record.Value != null)
                    {
                        mark = double.Parse(record.Value);
                    }
                    else
                    {
                        mark = -1;
                    }
                    //var componentPercentage = coursePer.Where(q => q.Id == componentId).Select(q => q.Per).FirstOrDefault();
                    //average += mark * componentPercentage/100;

                    //if (course.Status.HasValue && (course.Status.Value == (int)CourseStatus.FirstPublish || course.Status.Value == (int)CourseStatus.FinalPublish))
                    //{
                    //    if (studentInCourse.Status.HasValue && studentInCourse.Status.Value == (int)StudentInCourseStatus.Issued)
                    //    {
                    //        var studentMark = studentMarks.Where(q => q.CourseMarkId == componentId).FirstOrDefault();
                    //        studentMark.EdittedMark = mark;
                    //        studentMark.Note = note;
                    //        studentInCourse.Status = course.Status.Value == (int)CourseStatus.FirstPublish ? (int)StudentInCourseStatus.FirstPublish : (int)StudentInCourseStatus.FinalPublish;
                    //    }
                    //}
                    //else if (course.Status.HasValue && course.Status.Value == (int)CourseStatus.InProgress)
                    //{
                    //    var studentMark = studentMarks.Where(q => q.CourseMarkId == componentId).FirstOrDefault();
                    //    studentMark.Mark = mark;
                    //}
                    if (studentInCourse.Status.HasValue && studentInCourse.Status.Value == (int)StudentInCourseStatus.Issued)
                    {
                        var studentMark = studentMarks.Where(q => q.CourseMarkId == componentId).FirstOrDefault();
                        studentMark.EdittedMark = mark;
                        studentMark.Note = note;
                        studentInCourse.Status = course.Status.Value == (int)CourseStatus.FirstPublish ? (int)StudentInCourseStatus.FirstPublish : (int)StudentInCourseStatus.FinalPublish;
                    }

                    else
                    {
                        var studentMark = studentMarks.Where(q => q.CourseMarkId == componentId).FirstOrDefault();
                        studentMark.Mark = mark;
                    }

                }
                context.SaveChanges();
                foreach (var item in studentInCourse.StudentCourseMarks)
                {
                    if (item.CourseMark.IsFinal == null || item.CourseMark.IsFinal != true)
                    {
                        if (item.EdittedMark == null)
                        {
                            average += item.Mark != -1 ? item.Mark * item.CourseMark.Percentage / 100 : 0 * item.CourseMark.Percentage;
                        }
                        else
                        {
                            average += item.EdittedMark != -1 ? item.EdittedMark * item.CourseMark.Percentage / 100 : 0 * item.CourseMark.Percentage;
                        }
                    }
                }
                if (studentInCourse.HasRetake == true)
                {
                    var retake = studentInCourse.StudentCourseMarks.Where(q => q.CourseMark.IsFinal == true && q.CourseMark.ComponentName.Contains("2nd")).Sum(q => q.Mark != -1 ? q.Mark * q.CourseMark.Percentage / 100 : 0);
                    average += retake;
                }
                else
                {
                    var final = studentInCourse.StudentCourseMarks.Where(q => q.CourseMark.IsFinal == true && !q.CourseMark.ComponentName.Contains("2nd")).Sum(q => q.Mark != -1 ? q.Mark * q.CourseMark.Percentage / 100 : 0);
                    average += final;
                }

                studentInCourse.Average = average;
                context.SaveChanges();
                return Json(new { success = true });
            }
        }

        public ActionResult EditMarks(string studentCode, int courseId)
        {
            using (var context = new DB_Finance_AcademicEntities())
            {
                var course = context.Courses.Find(courseId);
                var status = course.Status;
                //if (status != (int)CourseStatus.InProgress)
                //{
                //    return RedirectToAction("Index", "Home");
                //}
                var student = course.StudentInCourses.Where(q => q.StudentMajor.StudentCode.Equals(studentCode)).Select(q => new StudentEditViewModel
                {
                    SemesterId = course.Semester.Id,
                    Id = q.Id,
                    Class = course.ClassName,
                    CourseId = courseId,
                    Name = q.StudentMajor.Student.Name,
                    Code = q.StudentMajor.StudentCode,
                    Average = q.Average != null ? q.Average.ToString() : "-",
                    MarksComponent = q.StudentCourseMarks.ToList(),
                    CourseStatus = course.Status.Value,
                    StudentInCourseStatus = Enum.GetName(typeof(StudentInCourseStatus), q.Status != null ? q.Status : 0),

                }).FirstOrDefault();
                student.ComponentNames = course.CourseMarks.Select(q => q.ComponentName).ToList();
                return View("EditMark", student);

            }
        }

        [HttpPost]
        public ActionResult UploadExcel(int courseId)
        {
            var failRecordCount = 0;

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
                            if (course.Status != (int)CourseStatus.InProgress)
                            {
                                return Json(new { success = false, message = "Unauthorized." });
                            }
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

                                    if (totalCol - course.CourseMarks.Where(q => !q.IsFinal.HasValue).Count() != 4 || totalRow - course.StudentInCourses.Count != 2)
                                    {
                                        return Json(new { success = false, message = "Invalid template for this course." });
                                    }

                                    int tempNo = 0;
                                    for (int i = firstRecordRow; int.TryParse(ws.Cells[i, 1].Text.Trim(), out tempNo); i++)
                                    {
                                        var studentCode = ws.Cells[i, studentCodeCol].Text.Trim().ToUpper();
                                        var studentInCourse = context.StudentInCourses.Where(q => q.StudentMajor.StudentCode.ToUpper().Equals(studentCode) && q.CourseId == courseId).FirstOrDefault();

                                        if (studentInCourse != null)
                                        {
                                            double? average = 0;
                                            for (var j = 5; j <= totalCol; j++)
                                            {

                                                double value = 0;
                                                if (double.TryParse(ws.Cells[i, j].Text.Trim(), out value) && !ws.Cells[titleRow, j].Text.Trim().Contains("Final") && !ws.Cells[titleRow, j].Text.Trim().Contains("FE"))
                                                {
                                                    StudentCourseMark studentCourseMark = null;

                                                    var component = course.CourseMarks.Where(q => q.ComponentName.Contains(ws.Cells[titleRow, j].Text.Trim())).FirstOrDefault();

                                                    studentCourseMark = context.StudentCourseMarks.Where(q => q.StudentInCourseId.Equals(studentInCourse.Id) && q.CourseMarkId.Equals(component.Id)).FirstOrDefault();

                                                    if (studentCourseMark == null)
                                                    {
                                                        failRecordCount++;
                                                    }

                                                    studentCourseMark.Mark = value;
                                                    studentCourseMark.StudentInCourseId = studentInCourse.Id;
                                                    if (component != null)
                                                    {
                                                        average += studentCourseMark.Mark.Value * component.Percentage / 100;
                                                        studentCourseMark.CourseMarkId = component.Id;
                                                    }
                                                    else
                                                    {
                                                        return Json(new { success = false, message = "Invalid template for this course." });
                                                    }

                                                }
                                                else
                                                {
                                                    StudentCourseMark studentCourseMark = null;
                                                    var component = course.CourseMarks.Where(q => q.ComponentName.Contains(ws.Cells[titleRow, j].Text.Trim())).FirstOrDefault();

                                                    studentCourseMark = context.StudentCourseMarks.Where(q => q.StudentInCourseId.Equals(studentInCourse.Id) && q.CourseMarkId.Equals(component.Id)).FirstOrDefault();

                                                    if (studentCourseMark == null)
                                                    {
                                                        failRecordCount++;
                                                    }

                                                    studentCourseMark.Mark = -1;
                                                    studentCourseMark.StudentInCourseId = studentInCourse.Id;
                                                    if (component != null)
                                                    {
                                                        studentCourseMark.CourseMarkId = component.Id;
                                                    }
                                                    else
                                                    {
                                                        return Json(new { success = false, message = "Invalid template for this course." });
                                                    }
                                                }
                                            }
                                            //var final = studentInCourse.StudentCourseMarks.Where(q => q.CourseMark.ComponentName.Contains("(2nd)") && q.CourseMark.IsFinal.HasValue && q.CourseMark.IsFinal.Value).Sum(q => q.Mark.Value == -1 ? 0 : q.Mark.Value * q.CourseMark.Percentage / 100);
                                            //average += final > 0 ? final : studentInCourse.StudentCourseMarks.Where(q => !q.CourseMark.ComponentName.Contains("(2nd)") && q.CourseMark.IsFinal.HasValue && q.CourseMark.IsFinal.Value).Sum(q => q.Mark.Value == -1 ? 0 : q.Mark.Value * q.CourseMark.Percentage / 100);
                                            if (studentInCourse.HasRetake == true)
                                            {
                                                var retake = studentInCourse.StudentCourseMarks.Where(q => q.CourseMark.IsFinal == true && q.CourseMark.ComponentName.Contains("2nd")).Sum(q => q.Mark != -1 ? q.Mark * q.CourseMark.Percentage / 100 : 0);
                                                average += retake;
                                            }
                                            else
                                            {
                                                var final = studentInCourse.StudentCourseMarks.Where(q => q.CourseMark.IsFinal == true && !q.CourseMark.ComponentName.Contains("2nd")).Sum(q => q.Mark != -1 ? q.Mark * q.CourseMark.Percentage / 100 : 0);
                                                average += final;
                                            }

                                            studentInCourse.Average = average;
                                            //studentInCourse.Status = 1;
                                        }
                                        else
                                        {
                                            failRecordCount++;
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
                return Json(new { error = e.Message, message = "Errors in uploaded file. Please recheck" });
            }

            return Json(new { success = true, message = "File uploaded successfully", failRecordCount = failRecordCount });
        }

        public ActionResult UploadFinalExamExcel(int courseId, int isPublish)
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
                            //if (course.Status != (int)CourseStatus.InProgress)
                            //{
                            //    return RedirectToAction("Index", "Home");
                            //}
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
                                    var firstRow = 3;
                                    int tempNo = 0;
                                    double FinalAverage = 0;
                                    double FinalPercentage = 0;
                                    List<MarkPoint> FEList = new List<MarkPoint>();
                                    List<MarkPoint> REList = new List<MarkPoint>();
                                    for (int i = firstRow; int.TryParse(ws.Cells[i, 1].Text.Trim(), out tempNo); i++)
                                    {
                                        var studentCode = ws.Cells[i, studentCodeCol].Text.Trim().ToUpper();
                                        var studentInCourse = context.StudentInCourses.Where(q => q.StudentMajor.StudentCode.ToUpper().Equals(studentCode) && q.CourseId == courseId).FirstOrDefault();

                                        if (studentInCourse != null)
                                        {
                                            if (isPublish == 1)
                                            {
                                                studentInCourse.HasRetake = null;
                                            }
                                            double? average = 0;
                                            foreach (var item in studentInCourse.StudentCourseMarks)
                                            {
                                                if (item.EdittedMark == null)
                                                {
                                                    average += item.Mark != -1 ? item.Mark * item.CourseMark.Percentage / 100 : 0 * item.CourseMark.Percentage;
                                                }
                                                else
                                                {
                                                    average += item.EdittedMark != -1 ? item.EdittedMark * item.CourseMark.Percentage / 100 : 0 * item.CourseMark.Percentage;
                                                }
                                            }
                                            for (var j = 5; j <= totalCol; j++)
                                            {
                                                double value = 0;
                                                if (double.TryParse(ws.Cells[i, j].Text.Trim(), out value))
                                                {
                                                    //if (ws.Cells[titleRow, j].Text.Trim().Contains("2nd"))
                                                    //{
                                                    //    retaked = true;
                                                    //}
                                                    if (isPublish == 1 && ws.Cells[titleRow, j].Text.Trim().Contains("2nd") && !studentInCourse.HasRetake.HasValue)
                                                    {
                                                        studentInCourse.HasRetake = true;
                                                    }
                                                    if (isPublish == 0 && ws.Cells[titleRow, j].Text.Trim().Contains("2nd"))
                                                    {
                                                        return null;
                                                    }
                                                    if (isPublish == 1 && !ws.Cells[titleRow, j].Text.Trim().Contains("2nd"))
                                                    {
                                                        return null;
                                                    }
                                                    var FE = course.CourseMarks.Where(q => q.ComponentName.Equals(ws.Cells[titleRow, j].Text.Trim())).FirstOrDefault();
                                                    var studentCourseMarkFE = context.StudentCourseMarks.
                                                    Where(q => q.StudentInCourseId.Equals(studentInCourse.Id) && q.CourseMarkId.Equals(FE.Id)).FirstOrDefault();
                                                    var recordExistedFE = true;
                                                    if (studentCourseMarkFE == null)
                                                    {
                                                        studentCourseMarkFE = context.StudentCourseMarks.Create();
                                                        recordExistedFE = false;
                                                    }
                                                    studentCourseMarkFE.StudentInCourseId = studentInCourse.Id;
                                                    studentCourseMarkFE.CourseMarkId = FE.Id;
                                                    studentCourseMarkFE.Mark = value;
                                                    if (!recordExistedFE)
                                                    {
                                                        studentCourseMarkFE.CourseMarkId = FE.Id;
                                                        context.StudentCourseMarks.Add(studentCourseMarkFE);
                                                    }
                                                    //if (!studentInCourse.HasRetake.HasValue)
                                                    //{
                                                    //    MarkPoint FMark = new MarkPoint();
                                                    //    FMark.Value = value;
                                                    //    FMark.Per = FE.Percentage;
                                                    //    FEList.Add(FMark);
                                                    //}
                                                    //else
                                                    //{
                                                    //    MarkPoint FMark = new MarkPoint();
                                                    //    FMark.Value = value;
                                                    //    FMark.Per = FE.Percentage;
                                                    //    REList.Add(FMark);
                                                    //}
                                                }
                                                else
                                                {
                                                    var FE = course.CourseMarks.Where(q => q.ComponentName.Equals(ws.Cells[titleRow, j].Text.Trim())).FirstOrDefault();
                                                    var studentCourseMarkFE = context.StudentCourseMarks.
                                                    Where(q => q.StudentInCourseId.Equals(studentInCourse.Id) && q.CourseMarkId.Equals(FE.Id)).FirstOrDefault();
                                                    var recordExistedFE = true;
                                                    if (studentCourseMarkFE == null)
                                                    {
                                                        studentCourseMarkFE = context.StudentCourseMarks.Create();
                                                        recordExistedFE = false;
                                                    }
                                                    studentCourseMarkFE.StudentInCourseId = studentInCourse.Id;
                                                    studentCourseMarkFE.CourseMarkId = FE.Id;
                                                    studentCourseMarkFE.Mark = -1;
                                                    if (!recordExistedFE)
                                                    {
                                                        studentCourseMarkFE.CourseMarkId = FE.Id;
                                                        context.StudentCourseMarks.Add(studentCourseMarkFE);
                                                    }
                                                }

                                            }
                                            if (studentInCourse.HasRetake == true)
                                            {
                                                var retake = studentInCourse.StudentCourseMarks.Where(q => q.CourseMark.IsFinal == true && q.CourseMark.ComponentName.Contains("2nd")).Sum(q => q.Mark != -1 ? q.Mark * q.CourseMark.Percentage / 100 : 0);
                                                average += retake;

                                                //foreach (var item in REList)
                                                //{
                                                //    FinalPercentage += item.Per;
                                                //}
                                                //foreach (var item in REList)
                                                //{
                                                //    average += item.Value * item.Per / 100;
                                                //    FinalAverage += item.Value * item.Per / FinalPercentage;//Average for finals only
                                                //}
                                            }
                                            else
                                            {
                                                var final = studentInCourse.StudentCourseMarks.Where(q => q.CourseMark.IsFinal == true && !q.CourseMark.ComponentName.Contains("2nd")).Sum(q => q.Mark != -1 ? q.Mark * q.CourseMark.Percentage / 100 : 0);
                                                average += final;

                                                //foreach (var item in FEList)
                                                //{
                                                //    FinalPercentage += item.Per;
                                                //}
                                                //foreach (var item in FEList)
                                                //{
                                                //    average += item.Value * item.Per / 100;
                                                //    FinalAverage += item.Value * item.Per / FinalPercentage;
                                                //}
                                            }

                                            studentInCourse.Average = average;
                                            FEList.Clear();
                                            REList.Clear();
                                            //if (FinalAverage >= 4)
                                            //{
                                            //    if (average >= 5)
                                            //    {
                                            //        studentInCourse.Status = 2; //Student pass
                                            //    }
                                            //    else
                                            //    {
                                            //        studentInCourse.Status = 3; //Student fail
                                            //    }
                                            //}
                                            //else
                                            //{
                                            //    studentInCourse.Status = 3; //Student fail
                                            //}
                                            context.SaveChanges();
                                        }
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


        public ActionResult ChangeToPublish(int courseId)
        {
            using (var context = new DB_Finance_AcademicEntities())
            {
                var course = context.Courses.Find(courseId);
                course.Status = (int)CourseStatus.FirstPublish;
                var students = course.StudentInCourses;
                foreach (var item in students)
                {
                    item.Status = (int)StudentInCourseStatus.FirstPublish;
                }
                context.SaveChanges();
            }
            return Json(new { success = true, message = "Successully submitted!" });
        }

        public ActionResult ChangeToSubmit(int courseId)
        {
            using (var context = new DB_Finance_AcademicEntities())
            {
                var course = context.Courses.Find(courseId);
                course.Status = (int)CourseStatus.Submitted;
                var students = course.StudentInCourses;
                foreach (var item in students)
                {
                    item.Status = (int)StudentInCourseStatus.Submitted;
                }
                context.SaveChanges();
            }
            return Json(new { success = true, message = "Successully submitted!" });
        }

        public ActionResult ChangeToFinalPublish(int courseId)
        {
            using (var context = new DB_Finance_AcademicEntities())
            {
                var course = context.Courses.Find(courseId);
                course.Status = (int)CourseStatus.FinalPublish;
                var students = course.StudentInCourses;
                foreach (var item in students)
                {
                    item.Status = (int)StudentInCourseStatus.FinalPublish;
                }
                context.SaveChanges();
            }
            return Json(new { success = true, message = "Successully submitted!" });
        }

        public ActionResult CloseCourse(int courseId)
        {
            using (var context = new DB_Finance_AcademicEntities())
            {
                var course = context.Courses.Find(courseId);
                course.Status = (int)CourseStatus.Closed;
                var students = course.StudentInCourses;
                foreach (var item in students)
                {
                    item.Status = (int)StudentInCourseStatus.FinalPublish;
                }
                context.SaveChanges();
            }
            return Json(new { success = true, message = "Successully submitted!" });
        }

        [AllowAnonymous]
        [HttpPost]
        public ActionResult UploadSubject()
        {
            List<String> emptyFileName = new List<String>();
            try
            {
                if (Request.Files.Count == 0)
                {
                    return Json(new { success = false, message = "No file has been uploaded" });
                }
                foreach (string file in Request.Files)
                {
                    var fileContent = Request.Files[file];
                    if (fileContent != null && fileContent.ContentLength > 0)
                    {
                        string[] segments = fileContent.FileName.Split('.');
                        var fileExt = segments[segments.Length - 1];
                        Stream stream = null;
                        string path = Server.MapPath("~/UploadFile/");
                        var savePath = path + fileContent.FileName;

                        if (fileExt.Equals("xls"))
                        {
                            fileContent.SaveAs(savePath);

                            var app = new Microsoft.Office.Interop.Excel.Application(); //Interop not receive stream
                            var wb = app.Workbooks.Open(savePath);
                            wb.SaveAs(savePath + "x", FileFormat: Microsoft.Office.Interop.Excel.XlFileFormat.xlOpenXMLWorkbook);
                            wb.Close();
                            app.Quit();

                            stream = new FileStream(savePath + "x", FileMode.Open);
                        }
                        else
                        {

                            stream = fileContent.InputStream;
                        }


                        using (ExcelPackage excelPackage = new ExcelPackage(stream))
                        {
                            var wsList = excelPackage.Workbook.Worksheets.ToList();
                            if (wsList.Count == 0)
                            {
                                emptyFileName.Add(fileContent.FileName);
                            }
                            using (var context = new DB_Finance_AcademicEntities())
                            {
                                foreach (var ws in wsList)
                                {
                                    var totalRow = ws.Dimension.Rows;

                                    //Cell[Row, Col]. [4,2] -> Subject Code; [4,4] -> SUBJECTNAME
                                    if (ws.Cells[4, 2].Text.Trim().ToUpper().Equals("SUBJECT CODE")
                                        & ws.Cells[4, 4].Text.Trim().ToUpper().Equals("SUBJECT NAME"))
                                    {

                                        for (int i = 5; i <= totalRow; i++) //data start from row 5 in template
                                        {
                                            var subCode = ws.Cells[i, 2].Text.Trim();
                                            var subName = ws.Cells[i, 4].Text.Trim();
                                            var subjectCredit = -1;
                                            int.TryParse(ws.Cells[i, 5].Text.Trim(), out subjectCredit);
                                            var existList = context.Subjects.Where(q => q.SubjectCode.Equals(subCode)).ToList();
                                            if (existList.Count == 0)
                                                context.Subjects.Add(new Subject { SubjectCode = subCode, SubjectName = subName, CreditValue = subjectCredit });

                                            context.SaveChanges();
                                        }
                                    }

                                }
                            }
                        }
                        stream.Close();
                        if (System.IO.File.Exists(@savePath))
                        {
                            System.IO.File.Delete(@savePath);
                            if (System.IO.File.Exists(@savePath + "x"))
                            {
                                System.IO.File.Delete(@savePath + "x");
                            }

                        }
                    }
                }
            }
            catch (Exception e)
            {
                Response.StatusCode = (int)HttpStatusCode.BadRequest;
                return Json(new { success = false, message = e.Message });
            }
            if (emptyFileName.Count > 0)
            {
                string empFileName = "";
                foreach (var item in emptyFileName)
                {
                    empFileName += item + " ,";
                }
                return Json(new { success = false, message = "Upload subjects successed! But " + empFileName + " file are empty" });
            }

            return Json(new { success = true, message = "Upload Subjects successed" });
        }

        [AllowAnonymous]
        public ActionResult SubjectManagement()
        {
            return View();
        }

        [HttpPost]
        public ActionResult UploadStudent()
        {
            try
            {
                if (Request.Files.Count == 0)
                {
                    return Json(new { success = false, message = "No file has been uploaded" });
                }
                String savePath = "";
                String savePath2 = "";

                foreach (string file in Request.Files)
                {
                    var fileContent = Request.Files[file];
                    if (fileContent != null && fileContent.ContentLength > 0)
                    {
                        //old code get file Extension
                        //string path = Server.MapPath(fileContent.FileName);
                        //string fileName = Path.GetFileName(path);
                        //string fileExt = Path.GetExtension(fileName);

                        string[] segments = fileContent.FileName.Split('.');
                        var fileExt = segments[segments.Length - 1];
                        var fileNameWithoutExt = segments[0];

                        string myPath = Server.MapPath("~/UploadFile/");

                        if (fileExt.Equals("xls"))
                        {
                            fileExt += "x";
                        }
                        savePath = myPath + fileContent.FileName;
                        savePath2 = myPath + fileNameWithoutExt + "_2" + "." + fileExt;

                        if (System.IO.File.Exists(@savePath))
                        {
                            System.IO.File.Delete(@savePath);
                        }
                        if (System.IO.File.Exists(@savePath2))
                        {
                            System.IO.File.Delete(@savePath2);
                        }
                        fileContent.SaveAs(savePath);

                        var app = new Microsoft.Office.Interop.Excel.Application(); //Interop not receive stream
                        var wb = app.Workbooks.Open(savePath, null);
                        wb.SaveAs(savePath2, FileFormat: Microsoft.Office.Interop.Excel.XlFileFormat.xlOpenXMLWorkbook);
                        wb.Close();
                        app.Quit();

                        var stream = new FileStream(savePath2, FileMode.Open);

                        #region
                        //problem: html tag contain in excel file
                        //string myPath = Server.MapPath("~/UploadFile/");
                        //fileContent.SaveAs(myPath + fileName);

                        //var srcApp = new Application();
                        //var destApp = new Application();

                        //var srcWb = srcApp.Workbooks.Open(myPath + fileName);
                        //var destWb = srcApp.Workbooks.Add(Type.Missing);

                        //Worksheet srcSheet = srcWb.Worksheets[1];
                        //Worksheet destSheet = destWb.Worksheets[1];
                        //var srcRange = srcSheet.UsedRange;
                        //srcRange.Copy(Type.Missing);
                        //destSheet.Cells[1, 1].Select();
                        //destSheet.Paste(Type.Missing, Type.Missing);
                        //destWb.SaveAs(Server.MapPath("~/UploadFile/")+path + "2");
                        //destWb.Close();
                        //destApp.Quit();
                        //srcWb.Close();
                        //srcApp.Quit();
                        #endregion

                        ISheet sheet;
                        if (fileExt.Equals("xls"))
                        {
                            HSSFWorkbook hssfwb = new HSSFWorkbook(stream);
                            sheet = hssfwb.GetSheetAt(0);//only 1 sheet
                        }
                        else
                        {
                            XSSFWorkbook xssfwb = new XSSFWorkbook(stream);
                            sheet = xssfwb.GetSheetAt(0);//only 1 sheet
                        }


                        IEnumerator rows = sheet.GetRowEnumerator();
                        var t = sheet.GetEnumerator();
                        int? dataRow = null;
                        int dataCol = 0; //default sheet
                        using (var context = new DB_Finance_AcademicEntities())
                        {
                            while (rows.MoveNext())
                            {
                                IRow row;
                                if (fileExt.Equals(".xls"))
                                {
                                    row = (HSSFRow)rows.Current;
                                }
                                else
                                {
                                    row = (XSSFRow)rows.Current;

                                }
                                var tempCell = row.GetCell(0);
                                if (tempCell != null && tempCell.ToString().Trim().ToUpper().Equals("LOGIN")
                                    && dataRow == null)
                                {
                                    dataRow = tempCell.RowIndex + 2; // plus percentage Mark row
                                    dataCol = tempCell.ColumnIndex;
                                }
                                if (dataRow != null && tempCell.RowIndex >= dataRow)
                                {
                                    var loginName = row.GetCell(dataCol + 0).ToString().Trim();
                                    var studentCode = row.GetCell(dataCol + 1).ToString().Trim();
                                    var name = row.GetCell(dataCol + 2).ToString().Trim();
                                    Student student;

                                    var stuMajorExist = context.StudentMajors.Where(q => q.StudentCode == studentCode).FirstOrDefault();
                                    if (stuMajorExist == null)
                                    {
                                        student = context.Students.Add(new Student { Name = name });
                                        var studentMajor = context.StudentMajors.Add(new StudentMajor { LoginName = loginName, StudentCode = studentCode });
                                        studentMajor.StudentId = student.Id;
                                    }
                                    context.SaveChanges();
                                }
                            }
                        }


                        if (dataRow == null)
                        {
                            return Json(new { success = false, message = "File uploaded was empty" });
                        }


                    }
                }
                if (System.IO.File.Exists(@savePath))
                {
                    System.IO.File.Delete(@savePath);
                }
                if (System.IO.File.Exists(@savePath2))
                {
                    System.IO.File.Delete(@savePath2);

                }
            }
            catch (Exception e)
            {
                Response.StatusCode = (int)HttpStatusCode.BadRequest;
                return Json(new { success = false, message = e.Message });
            }

            return Json(new { success = true, message = "Upload Student Success" });
        }

        public ActionResult ManageStudent()
        {
            return View();
        }

        public ActionResult ArrangeCourse()
        {
            var smallRoom = System.Web.Configuration.WebConfigurationManager.AppSettings["SmallRoom"];
            var largeRoom = System.Web.Configuration.WebConfigurationManager.AppSettings["LargeRoom"];

            List<dynamic> result = new List<dynamic>();
            using (var context = new DB_Finance_AcademicEntities())
            {
                var block = context.Semesters.OrderBy(q => q.Year).ThenBy(q => q.SemesterInYear).LastOrDefault().Blocks.Where(q => q.Status == (int)BlockStatus.Registering).FirstOrDefault();

                var availableSubjects = block.AvailableSubjects.GroupBy(q => q.SubjectId).Select(q => new
                {
                    Count = q.Count(),
                    SubjectId = q.Key
                });

                foreach (var registeredSubject in availableSubjects)
                {
                    var subject = context.Subjects.Find(registeredSubject.SubjectId);

                    if (subject != null)
                    {
                        var registrationCount = registeredSubject.Count;

                    }
                    else
                    {
                        //TODO
                    }
                }
            }

            return null;
        }

        public ActionResult GetSubject4DataTable(JQueryDataTableParamModel param)
        {
            try
            {

                using (var context = new DB_Finance_AcademicEntities())
                {
                    ////use for serverside DataTable
                    //int count = 0;
                    //count = param.iDisplayStart;
                    //var search = context.Subjects.Where(q => String.IsNullOrEmpty(param.sSearch) || q.SubjectName.ToUpper().Contains(param.sSearch.ToUpper()));
                    //var subjectList = search
                    //  .Skip(param.iDisplayStart)
                    //  .Take(param.iDisplayLength)
                    //  .ToList()
                    //  .Select(q => new IConvertible[]{
                    //++count,
                    //q.SubjectCode,
                    //q.SubjectName,
                    //q.Id
                    //  });

                    //var totalRecords = subjectList.Count();

                    //return Json(new
                    //{
                    //    success = true,
                    //    sEcho = param.sEcho,
                    //    iTotalRecords = totalRecords,
                    //    iTotalDisplayRecords = search.Count(),
                    //    aaData = subjectList
                    //}, JsonRequestBehavior.AllowGet);

                    int count = 1;
                    var subjectList = context.Subjects.OrderBy(q => q.Id).AsEnumerable().Select(q => new IConvertible[]
                     {
                        count++,
                        q.SubjectCode,
                        q.SubjectName,
                        q.Id
                     }).ToList();
                    return Json(new
                    {
                        success = true,
                        iTotalRecords = subjectList.Count(),
                        iTotalDisplayRecords = subjectList.Count(),
                        aaData = subjectList
                    }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Response.StatusCode = (int)HttpStatusCode.BadRequest;
                return Json(new { success = true, message = e.Message });
            }
        }

        public ActionResult SubjectDetails(int subjectId = -1)
        {
            if (subjectId == -1)
            {
                return Json(new { success = false, message = "Error! Invalid Subject" });
            }
            try
            {
                using (var context = new DB_Finance_AcademicEntities())
                {
                    var count = 0;
                    var model = context.SubjectMarks.Where(q => q.SubjectId == subjectId)
                        .OrderByDescending(q => q.EffectivenessDate).AsEnumerable()
                        .Select(q => new SubjectMarkViewModel
                        {
                            Index = count++,
                            Id = q.Id,
                            ComponentName = q.ComponentName,
                            Percentage = q.Percentage,
                            EffectivenessDate = q.EffectivenessDate == null ? "-" :
                            DateTime.Parse(q.EffectivenessDate.Value.ToString())
                            .ToString("dd/MM/yyyy hh:mm:ss", CultureInfo.InvariantCulture)
                        })
                        .ToList();
                    ViewBag.SubjectId = subjectId;
                    return View(model);
                }
            }
            catch (Exception)
            {

                throw;
            }
        }

        [HttpPost]
        public ActionResult AddSubjectComponent(List<SubjectMarkModel> subjectMarkList, int subjectId)
        {
            try
            {
                using (var context = new DB_Finance_AcademicEntities())
                {
                    foreach (var item in subjectMarkList)
                    {
                        if (item != null && item.SubjectComponentName != null)
                        {
                            context.SubjectMarks.Add(new SubjectMark
                            {
                                ComponentName = item.SubjectComponentName,
                                Percentage = item.Percentage,
                                EffectivenessDate = DateTime.Now,
                                SubjectId = subjectId

                                ///còn thiếu CurrentSyllabus
                            });
                        }
                    }
                    context.SaveChanges();
                }

                return Json(new { success = true, message = "Add Subject Component Successed!" });
            }
            catch (Exception e)
            {
                Response.StatusCode = (int)HttpStatusCode.BadRequest;
                return Json(new { success = false, message = e.Message });
            }

        }

        public bool NotifyTeacherForgotInputMark(string senderPassword)
        {
            try
            {
                var userManager = HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
                using (var context = new DB_Finance_AcademicEntities())
                {
                    var teacherList = context.Courses.Where(q => q.Status != (int)CourseStatus.Submitted)
                          .Select(q => new TeacherMail
                          {
                              EduMail = q.Teacher.EduEmail,
                              FeMail = q.Teacher.FeEmail
                          }).ToList();

                    var message = "";

                    ///not done yet!

                    foreach (var teacher in teacherList)
                    {
                        
                    }
                }


            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return false;
            }


            return true;
        }






    }

}