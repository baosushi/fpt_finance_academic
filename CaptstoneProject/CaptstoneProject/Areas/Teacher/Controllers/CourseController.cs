using CaptstoneProject.Controllers;
using CaptstoneProject.Models;
using DataService.Model;
using OfficeOpenXml;
using OfficeOpenXml.Drawing;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Entity;
using System.Drawing;
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
        public ActionResult Index(bool all = false, int subjectId = -1, int semesterId = -1)
        {
            var loginName = (string)Session["loginName"];
            List<CourseRecordViewModel> courses = new List<CourseRecordViewModel>();
            using (var context = new DB_Finance_AcademicEntities())
            {
                //DateTime startDate, endDate;
                var semester = semesterId == -1 ? context.Semesters.OrderByDescending(q => q.Year).ThenByDescending(q => q.SemesterInYear).FirstOrDefault() : context.Semesters.Find(semesterId);

                var teacher = context.Teachers.Where(q => q.LoginName == loginName).FirstOrDefault();
                //var subjects = context.TeacherSubjects.Where(q => q.Id == teacher.Id && q.SubjectId == subjectId).ToList();
                subjectId = subjectId == -1 ? context.TeacherSubjects.Where(q => q.TeacherId == teacher.Id).FirstOrDefault().SubjectId : subjectId;
                //startDate = semester.StartDate.Value;
                //endDate = semester.EndDate.Value;
                if (all)
                {
                    courses = context.Teachers.Where(q => q.LoginName == loginName).FirstOrDefault()
                        .Courses.Where(q => q.SemesterId == semester.Id && q.SubjectId == subjectId)
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
                }
                else
                {
                    courses = context.Teachers.Where(q => q.LoginName == loginName).FirstOrDefault()
                      .Courses.Where(q => q.SemesterId == semester.Id && q.Status != (int)CourseStatus.Closed && q.Status != (int)CourseStatus.Cancel)
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
                }
                var semesters = context.Semesters.OrderByDescending(q => q.Year).ThenByDescending(q => q.SemesterInYear);
                var semesterList = semesters.Select(q => new SelectListItem
                {
                    Text = q.Title + " " + q.Year,
                    Value = q.Id.ToString(),
                }).ToList();
                var subjects = context.TeacherSubjects.Where(q => q.TeacherId == teacher.Id).Select(q => new SelectListItem
                {
                    Value = q.Subject.Id.ToString(),
                    Text = q.Subject.SubjectName,
                }).ToList();
                ViewBag.semList = semesterList;
                ViewBag.selectedSem = semester.Id;
                ViewBag.showAll = all;
                ViewBag.subjectList = subjects;
                ViewBag.selectedSub = subjectId;
            }
            return View(courses);
        }

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
                        var componentsEdit = course.CourseMarks.Where(q => q.IsFinal != true || !q.IsFinal.HasValue).Select(q => new MarkComp
                        {
                            Name = q.ComponentName,
                            Id = q.Id,
                        });
                        ViewBag.Components = componentsEdit;
                        ViewBag.ClassName = course.ClassName;

                        var data = course.StudentInCourses.Select(q => new StudentInCourseViewModel
                        {
                            UserName = q.StudentMajor.LoginName,
                            StudentName = q.StudentMajor.Student.Name,
                            StudentCode = q.StudentMajor.StudentCode,
                            Average = q.Average != null ? q.Average.ToString() : "-",
                            MarksComponent = q.StudentCourseMarks.ToList(),
                            Status = q.Status,
                            StatusName = q.Status == null ? null : Enum.GetName(typeof(StudentInCourseStatus), q.Status.Value)
                        }).ToList();

                        //var datatest = course.StudentInCourses.Select(q => new IConvertible[] {
                        //    q.StudentMajor.StudentCode,
                        //    q.StudentMajor.LoginName,
                        //    //q.StudentCourseMarks.Select(s => s.Mark),
                        //    q.Average,
                        //    q.Status
                        //});
                        DateTime currentDate = DateTime.Now;
                        bool readySub = false;
                        if ((course.EndDate == null ? currentDate : course.EndDate.Value).Subtract(currentDate).Days <= 14)
                        {
                            readySub = true;
                        }
                        var columns = course.CourseMarks.Select(q => q.ComponentName).ToList();
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
                        var semester = course.Semester;
                        var model = new CourseDetailsViewModel
                        {
                            CourseId = courseId,
                            ComponentNames = columns,
                            StudentInCourse = data,
                            Semester = semester.Title + " " + semester.Year,
                            SubCode = course.Subject.SubjectCode,
                            FinalCol = finalColumns,
                            ReadySubmit = readySub,
                            SubName = course.Subject.SubjectName,
                            IsEditable = course.Status == (int)CourseStatus.InProgress ? true : false,
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
                        ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = StudentMajor.StudentMajor.Student.Name;
                        foreach (var mark in StudentMajor.StudentCourseMarks)
                        {
                            if (!mark.CourseMark.IsFinal.HasValue && mark.CourseMark.IsFinal != true)
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

        //public JsonResult GetSubjectsByTeacher()
        //{
        //    this.Session["loginName"] = "phuonglhk";
        //    var loginName = (string)Session["loginName"];
        //    using (var context = new DB_Finance_AcademicEntities())
        //    {
        //        var teacher = context.Teachers.Where(q => q.LoginName == loginName).FirstOrDefault();
        //        var subjects = context.TeacherSubjects.Where(q => q.TeacherId == teacher.Id).Select(q => new SelectListItem{
        //            Value=q.Subject.Id.ToString(),
        //            Text=q.Subject.SubjectName,
        //        });


        //        return Json(new
        //        {
        //            subjectList = subjects,
        //        });
        //    }
        //}

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

                    if (course.Status.HasValue && (course.Status.Value == (int)CourseStatus.FirstPublish || course.Status.Value == (int)CourseStatus.FinalPublish))
                    {
                        if (studentInCourse.Status.HasValue && studentInCourse.Status.Value == (int)StudentInCourseStatus.Issued)
                        {
                            var studentMark = studentMarks.Where(q => q.CourseMarkId == componentId).FirstOrDefault();
                            studentMark.EdittedMark = mark;
                            studentMark.Note = note;
                            studentInCourse.Status = course.Status.Value == (int)CourseStatus.FirstPublish ? (int)StudentInCourseStatus.FirstPublish : (int)StudentInCourseStatus.FinalPublish;
                        }
                    }
                    else if (course.Status.HasValue && course.Status.Value == (int)CourseStatus.InProgress)
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
                    StudentInCourseStatus = Enum.GetName(typeof(StudentInCourseStatus), q.Status != null ? q.Status : 0),

                }).FirstOrDefault();
                student.ComponentNames = course.CourseMarks.Select(q => q.ComponentName).ToList();
                return View("EditMark", student);

            }
        }

        [HttpGet]
        public ActionResult EditSingleCourseComponent(int courseId, int componentId)
        {
            using (var context = new DB_Finance_AcademicEntities())
            {
                var course = context.Courses.Find(courseId);
                var component = context.CourseMarks.Find(componentId);

                var list = course.StudentInCourses.Select(q => new StudentComponent
                {
                    StudentCode = q.StudentMajor.StudentCode,
                    StudentName = q.StudentMajor.Student.Name,
                    ComponentMark = q.StudentCourseMarks.Where(z => z.CourseMarkId == componentId).FirstOrDefault().Mark ?? -1
                }).ToList();

                var model = new EditCourseSingleComponentModel
                {
                    CourseId = courseId,
                    ComponentName = component.ComponentName,
                    StudentComponents = list
                };

                return View("EditCourseMark", model);
            }
        }

        [HttpPost]
        public ActionResult EditSingleCourseComponent(EditCourseSingleComponentModel model)
        {
            using (var context = new DB_Finance_AcademicEntities())
            {
                var course = context.Courses.Find(model.CourseId);

                if (course != null)
                {
                    foreach (var record in model.StudentComponents)
                    {
                        var studentCourseMark = context.StudentCourseMarks.Where(q => q.StudentInCourse.StudentMajor.StudentCode == record.StudentCode.ToUpper().Trim() && q.CourseMark.ComponentName.Equals(model.ComponentName.Trim())).FirstOrDefault();
                        var student = context.StudentMajors.Where(q => q.StudentCode == record.StudentCode).FirstOrDefault();
                        var studentInCourse = context.StudentInCourses.Where(q => q.StudentId == student.Id && q.CourseId == model.CourseId).FirstOrDefault();
                        double? average = 0;

                        if (studentCourseMark == null)
                        {
                            studentCourseMark = context.StudentCourseMarks.Create();
                            //var student = context.StudentMajors.Where(q => q.StudentCode == record.StudentCode).FirstOrDefault();

                            if (student == null)
                            {
                                return Json(new { success = false, message = "Student is null" });
                            }

                            //var studentInCourse = context.StudentInCourses.Where(q => q.StudentId == student.Id).FirstOrDefault();

                            if (studentInCourse == null)
                            {
                                studentInCourse = context.StudentInCourses.Create();
                                studentInCourse.CourseId = model.CourseId;
                                studentInCourse.StudentId = student.Id;
                                studentInCourse.Average = -1;
                                context.StudentInCourses.Add(studentInCourse);

                                context.SaveChanges();
                            }

                            var courseMark = context.CourseMarks.Where(q => q.ComponentName.Equals(model.ComponentName.Trim()) && q.CourseId == model.CourseId).FirstOrDefault();

                            if (courseMark == null)
                            {
                                return Json(new { success = false, message = "CourseMark is null" });
                            }

                            studentCourseMark.StudentInCourseId = studentInCourse.Id;
                            studentCourseMark.CourseMarkId = courseMark.Id;
                            studentCourseMark.Mark = -1;
                            context.StudentCourseMarks.Add(studentCourseMark);

                            context.SaveChanges();
                        }
                        if (record.ComponentMark == null)
                        {
                            studentCourseMark.Mark = -1;
                        }
                        else
                        {
                            studentCourseMark.Mark = record.ComponentMark;
                        }

                        //studentCourseMark.StudentInCourse.Average -= studentCourseMark.Mark * studentCourseMark.CourseMark.Percentage / 100;

                        //average += record.ComponentMark * studentCourseMark.CourseMark.Percentage / 100;
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
                        //studentInCourse.Status = 1;

                        context.SaveChanges();
                    }

                    return Json(new { success = true, courseId = course.Id, semesterId = course.SemesterId });
                }
            }

            return Json(new { success = false, message = "Course is null" });
        }
    }
}