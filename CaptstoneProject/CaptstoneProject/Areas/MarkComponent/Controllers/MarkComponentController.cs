using CaptstoneProject.Controllers;
using DataService.Model;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace CaptstoneProject.Areas.MarkComponent.Controllers
{
    public class MarkComponentController : MyBaseController
    {
        // GET: MarkComponent/MarkComponent
        public ActionResult Index()
        {
            return View("ImportExcel");
        }

        [HttpPost]
        public ActionResult UploadExcel()
        {
            try
            {
                if (Request.Files.Count > 0)
                {
                    foreach (string file in Request.Files)
                    {
                        //List<KeyValuePair<string, List<double>>> data = new List<KeyValuePair<string, List<double>>>();

                        var fileContent = Request.Files[file];
                        var subjectCode = fileContent.FileName.Split(new char[] { '_' })[0];
                        var className = fileContent.FileName.Split(new char[] { '_' })[1];
                        if (fileContent != null && fileContent.ContentLength > 0)
                        {
                            var stream = fileContent.InputStream;

                            using (ExcelPackage package = new ExcelPackage(stream))
                            {
                                var ws = package.Workbook.Worksheets.First();
                                var totalCol = ws.Dimension.Columns;
                                var totalRow = ws.Dimension.Rows;
                                var studentCodeCol = 0;
                                var titleRow = 8;
                                for (int i = 1; i <= totalCol && studentCodeCol == 0; i++)
                                {
                                    if (ws.Cells[8, i].Text.ToUpper().Trim() == "STUDENT ID")
                                    {
                                        studentCodeCol = i;
                                    }
                                }

                                int tempNo = 0;
                                for (int i = 10; int.TryParse(ws.Cells[i, 1].Text.Trim(), out tempNo) /*totalRow*/; i++)
                                {
                                    var studentCode = ws.Cells[i, studentCodeCol].Text.Trim().ToUpper();

                                    //data.Add(new KeyValuePair<string, List<double>>(studentCode, new List<double>()));
                                    using (var context = new DB_Finance_AcademicEntities())
                                    {
                                        var studentInCourse = context.StudentInCourses.Where(q => q.StudentMajor.StudentCode.ToUpper().Equals(studentCode)).FirstOrDefault();
                                        var course = context.Courses.Where(q => q.Subject.SubjectCode == subjectCode).FirstOrDefault();

                                        if (studentInCourse != null && course != null)
                                        {
                                            double average = 0;
                                            for (var j = 5; j <= totalCol; j++)
                                            {

                                                double value = 0;
                                                if (double.TryParse(ws.Cells[i, j].Text.Trim(), out value))
                                                {

                                                    StudentCourseMark studentCourseMark = null;

                                                    var component = course.CourseMarks.Where(q => q.ComponentName.Contains(ws.Cells[titleRow, j].Text.Trim())).FirstOrDefault();

                                                    var stuCourseMarkExist = context.StudentCourseMarks.
                                                        Where(q => q.StudentInCourseId.Equals(studentInCourse.Id) && q.CourseMarkId.Equals(component.Id)).FirstOrDefault();
                                                    //check if exist
                                                    if (stuCourseMarkExist != null)
                                                    {
                                                        studentCourseMark = stuCourseMarkExist;
                                                    }
                                                    else //create new if none exist
                                                    {
                                                        studentCourseMark = context.StudentCourseMarks.Create();
                                                    }


                                                    studentCourseMark.Mark = value;
                                                    studentCourseMark.StudentInCourseId = studentInCourse.Id;
                                                    if (component != null)
                                                    {
                                                        average += studentCourseMark.Mark.Value * component.Percentage / 100;
                                                        studentCourseMark.CourseMarkId = component.Id;

                                                        context.StudentCourseMarks.Add(studentCourseMark);
                                                    }
                                                }
                                            }

                                            studentInCourse.Average = average;
                                        }

                                        context.SaveChanges();
                                    }
                                }
                            }

                            //var fileName = Path.GetFileName(file);
                            //var path = Path.Combine(Server.MapPath("~/App_Data/Images"), fileName);
                            //using (var fileStream = File.Create(path))
                            //{
                            //    stream.CopyTo(fileStream);
                            //}
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
        public ActionResult GetAverageBySubjectCode(string subjectCode)
        {
            using (var context = new DB_Finance_AcademicEntities())
            {
                var course = context.Courses.Where(q => q.Subject.SubjectCode.Equals(subjectCode.ToUpper())).LastOrDefault();
                if (course != null)
                {
                    int count = 1;
                    var result = course.StudentInCourses.Select(q => new IConvertible[] {
                        count++,
                        q.StudentMajor.StudentCode,
                        "Voodoo Login",
                        "Voodoo Name",
                        q.Average.Value.ToString("0.##")
                    });

                    if (result.Count() > 0)
                    {
                        return Json(new { success = true, data = result });
                    }
                    else
                    {
                        return Json(new { success = false });
                    }
                }
                else
                {
                    return Json(new { success = false });
                }
            }
        }
    }
}



















