﻿using CaptstoneProject.Controllers;
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
                        StartDate = q.StartDate.Value,
                        EndDate = q.EndDate.Value,
                        Status = Enum.GetName(typeof(CourseStatus), q.Status.Value)
                    }).ToList();
                var randomCourse = context.Courses.Where(q => q.SemesterId == semester.Id).FirstOrDefault();
                if (randomCourse != null)
                {
                    status = randomCourse.Status;
                }
                var semesters = context.Semesters.OrderByDescending(q => q.Year).ThenByDescending(q => q.SemesterInYear);
                var semesterList = semesters.Select(q => new SelectListItem
                {
                    Text = q.Title + " " + q.Year,
                    Value = q.Id.ToString(),
                }).ToList();
                ViewBag.semList = semesterList;
                ViewBag.selectedSem = semesterId;
                ViewBag.selectedSemName = semester.Title + "" + semester.Year;
                ViewBag.courseStatus = status;
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
                            StudentCode = q.StudentMajor.StudentCode,
                            Average = q.Average != null ? q.Average.ToString() : "N/A",
                            MarksComponent = q.StudentCourseMarks.ToList(),
                            Status = Enum.GetName(typeof(StudentCourseStatus), q.Status == null ? 0 : q.Status.Value)
                        }).ToList();

                        //var datatest = course.StudentInCourses.Select(q => new IConvertible[] {
                        //    q.Student.StudentCode,
                        //    q.Student.LoginName,
                        //    //q.StudentCourseMarks.Select(s => s.Mark),
                        //    q.Average,
                        //    q.Status
                        //});

                        var columns = context.CourseMarks.Where(q => q.CourseId == courseId).Select(q => q.ComponentName).ToList();
                        var semester = semesterId == -1 ? context.Semesters.OrderByDescending(q => q.Year).ThenByDescending(q => q.SemesterInYear).FirstOrDefault() : context.Semesters.Find(semesterId);
                        var model = new CourseDetailsViewModel
                        {
                            CourseId = courseId,
                            ComponentNames = columns,
                            StudentInCourse = data,
                            Semester = semester.Title + " " + semester.Year,
                            SubCode = course.Subject.SubjectCode,
                            SubName = course.Subject.SubjectName,
                            isEditable = course.Status != (int)CourseStatus.LockTM ? true : false
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
                        course.Status = (int)CourseStatus.LockTeacher;
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
                        course.Status = (int)CourseStatus.LockTM;
                    }
                    semester.Status = (int)SememsterStatus.Closed;
                    context.SaveChanges();
                }
            }
            return RedirectToAction("Index");
        }


        [HttpPost]
        public ActionResult DownloadTemplate(int courseId)
        {
            MemoryStream ms = new MemoryStream();

            using (var context = new DB_Finance_AcademicEntities())
            {
                var course = context.Courses.Find(courseId);
                var fileName = "Final Exam " + course.Subject.SubjectCode + " - " + course.ClassName + " - " + course.CourseName;

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
                        if (component.IsFinal != null && component.IsFinal == true)
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
                    foreach (var student in course.StudentInCourses)
                    {
                        ws.Cells["" + (StartHeaderChar++) + (++StartHeaderNumber)].Value = count++;
                        ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = student.StudentMajor.StudentCode;
                        ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = student.StudentMajor.LoginName;
                        ws.Cells["" + (StartHeaderChar) + (StartHeaderNumber)].Value = student.StudentMajor.LoginName;
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
            using (var context = new DB_Finance_AcademicEntities())
            {
                var course = context.Courses.Find(courseId);
                var status = course.Status;
                if (status == (int)CourseStatus.LockTM)
                {
                    return RedirectToAction("Index", "Home");
                }
                var student = course.StudentInCourses.Where(q => q.StudentMajor.StudentCode.Equals(studentCode)).Select(q => new StudentEditViewModel
                {
                    SemesterId = course.Semester.Id,
                    Id = q.Id,
                    Class = course.ClassName,
                    CourseId = courseId,
                    Name = q.StudentMajor.LoginName,
                    Code = q.StudentMajor.StudentCode,
                    Average = q.Average != null ? q.Average.ToString() : "N/A",
                    MarksComponent = q.StudentCourseMarks.ToList(),

                }).FirstOrDefault();
                student.ComponentNames = course.CourseMarks.Select(q => q.ComponentName).ToList();
                return View("EditMark", student);

            }

        }

        public ActionResult UploadFinalExamExcel(int courseId)
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
                            if (course.Status == (int)CourseStatus.LockTM)
                            {
                                return RedirectToAction("Index", "Home");
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
                                    var firstRow = 3;
                                    var j = 5;
                                    int tempNo = 0;
                                    for (int i = firstRow; int.TryParse(ws.Cells[i, 1].Text.Trim(), out tempNo); i++)
                                    {
                                        var studentCode = ws.Cells[i, studentCodeCol].Text.Trim().ToUpper();
                                        var studentInCourse = context.StudentInCourses.Where(q => q.StudentMajor.StudentCode.ToUpper().Equals(studentCode) && q.CourseId == courseId).FirstOrDefault();

                                        if (studentInCourse != null)
                                        {
                                            double? average = 0;
                                            foreach (var item in studentInCourse.StudentCourseMarks)
                                            {
                                                if (item.CourseMark.IsFinal == null || item.CourseMark.IsFinal != true)
                                                {
                                                    average += item.Mark * item.CourseMark.Percentage / 100;
                                                }
                                            }
                                            double value = 0;
                                            double TotalFinal = 0; //for FE with component 
                                            if (double.TryParse(ws.Cells[i, j].Text.Trim(), out value) || ws.Cells[i, j].Text.Trim().Equals('/'))
                                            {
                                                #region FE with component
                                                if (ws.Cells[titleRow, j].Text.Trim().Contains("FE"))
                                                {
                                                    if (!ws.Cells[titleRow, j].Text.Trim().Contains("2nd"))
                                                    {
                                                        if (!ws.Cells[i, j].Text.Trim().Equals('/'))
                                                        {
                                                            var FE = course.CourseMarks.Where(q => q.ComponentName.Equals(ws.Cells[titleRow, j].Text.Trim())).FirstOrDefault();
                                                            var studentCourseMarkFE = context.StudentCourseMarks.
                                                            Where(q => q.StudentInCourseId.Equals(studentInCourse.Id) && q.CourseMarkId.Equals(FE.Id)).FirstOrDefault();
                                                        }
                                                        else
                                                        {
                                                            var RE = course.CourseMarks.Where(q => q.ComponentName.Equals(ws.Cells[titleRow, j].Text.Trim())).FirstOrDefault();
                                                            var studentCourseMarkFE = context.StudentCourseMarks.
                                                            Where(q => q.StudentInCourseId.Equals(studentInCourse.Id) && q.CourseMarkId.Equals(RE.Id)).FirstOrDefault();

                                                        }
                                                    }
                                                }
                                                #endregion
                                                #region Normal FE
                                                else
                                                {
                                                    if (!ws.Cells[i, j].Text.Trim().Equals('/'))
                                                    {
                                                        var FE = course.CourseMarks.Where(q => q.ComponentName.Equals(ws.Cells[titleRow, j].Text.Trim())).FirstOrDefault();
                                                        var RE = course.CourseMarks.Where(q => q.ComponentName.Equals(ws.Cells[titleRow, j + 1].Text.Trim())).FirstOrDefault();
                                                        var studentCourseMarkFE = context.StudentCourseMarks.
                                                            Where(q => q.StudentInCourseId.Equals(studentInCourse.Id) && q.CourseMarkId.Equals(FE.Id)).FirstOrDefault();
                                                        var studentCourseMarkRE = context.StudentCourseMarks.
                                                            Where(q => q.StudentInCourseId.Equals(studentInCourse.Id) && q.CourseMarkId.Equals(RE.Id)).FirstOrDefault();
                                                        var recordExistedFE = true;
                                                        var recordExistedRE = true;
                                                        //remake null check
                                                        if (studentCourseMarkFE == null)
                                                        {
                                                            studentCourseMarkFE = context.StudentCourseMarks.Create();
                                                            recordExistedFE = false;
                                                        }
                                                        if (studentCourseMarkRE == null)
                                                        {
                                                            studentCourseMarkRE = context.StudentCourseMarks.Create();
                                                            recordExistedRE = false;
                                                        }
                                                        studentCourseMarkFE.Mark = value;
                                                        double FEMark = 0;
                                                        if (double.TryParse(ws.Cells[i, j].Text.Trim(), out FEMark))
                                                        {
                                                            studentCourseMarkFE.Mark = FEMark;
                                                        }
                                                        else
                                                        {
                                                            studentCourseMarkFE.Mark = null;

                                                        }

                                                        double REMark = 0;
                                                        if (double.TryParse(ws.Cells[i, j + 1].Text.Trim(), out REMark))
                                                        {

                                                            studentCourseMarkRE.Mark = REMark;
                                                        }
                                                        else
                                                        {
                                                            studentCourseMarkRE.Mark = null;

                                                        }
                                                        studentCourseMarkFE.StudentInCourseId = studentInCourse.Id;
                                                        studentCourseMarkRE.StudentInCourseId = studentInCourse.Id;
                                                        if (studentCourseMarkFE.Mark != null && studentCourseMarkRE.Mark == null)
                                                        {
                                                            //average = studentInCourse.Average.Value;
                                                            average += studentCourseMarkFE.Mark.Value * FE.Percentage / 100;
                                                        }
                                                        else if (studentCourseMarkRE.Mark != null)
                                                        {
                                                            //average = studentInCourse.Average.Value;
                                                            average += studentCourseMarkRE.Mark.Value * RE.Percentage / 100;
                                                        }
                                                        if (!recordExistedFE)
                                                        {
                                                            studentCourseMarkFE.CourseMarkId = FE.Id;
                                                            context.StudentCourseMarks.Add(studentCourseMarkFE);
                                                        }
                                                        if (!recordExistedRE)
                                                        {
                                                            studentCourseMarkRE.CourseMarkId = RE.Id;
                                                            context.StudentCourseMarks.Add(studentCourseMarkRE);
                                                        }

                                                        studentInCourse.Average = average;
                                                        if (studentCourseMarkFE == null || studentCourseMarkFE.Mark < 4)
                                                        {
                                                            if (studentCourseMarkRE == null || studentCourseMarkRE.Mark < 4)
                                                            {
                                                                studentInCourse.Status = 3;
                                                            }
                                                            else if (average < 5)
                                                            {
                                                                studentInCourse.Status = 3;
                                                            }
                                                            else
                                                            {
                                                                studentInCourse.Status = 2;
                                                            }
                                                        }
                                                        else
                                                        {
                                                            if (average < 5)
                                                            {
                                                                studentInCourse.Status = 3;
                                                            }
                                                            else
                                                            {
                                                                studentInCourse.Status = 2;
                                                            }
                                                        }
                                                    }

                                                    else
                                                    {
                                                        var FE = course.CourseMarks.Where(q => q.ComponentName.Equals(ws.Cells[titleRow, j].Text.Trim())).FirstOrDefault();
                                                        var RE = course.CourseMarks.Where(q => q.ComponentName.Equals(ws.Cells[titleRow, j + 1].Text.Trim())).FirstOrDefault();
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

                                                        studentInCourse.Average = null;
                                                        studentInCourse.Status = 3;
                                                    }
                                                }
                                                #endregion

                                            }
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
                    HttpPostedFileBase fileContent = Request.Files[file];
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
                                            var existList = context.Subjects.Where(q => q.SubjectCode.Equals(subCode)).ToList();
                                            if (existList.Count == 0)
                                                context.Subjects.Add(new Subject { SubjectCode = subCode, SubjectName = subName });

                                        }
                                    }

                                }
                                context.SaveChanges();
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
                return Json(new { success = false, message = "Upload Students successed! But " + empFileName + " file are empty" });
            }

            return Json(new { success = true, message = "Upload Students successed" });
        }



        public ActionResult ImportSubject()
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
    }

}