using DataService.Model;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Google.Apis.Drive.v3.Data;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Web;
using System.Web.Mvc;

namespace CaptstoneProject.Controllers
{
    public class DataImportController : Controller
    {
        // If modifying these scopes, delete your previously saved credentials
        // at ~/.credentials/drive-dotnet-quickstart.json
        static string[] Scopes = { DriveService.Scope.DriveReadonly };
        static string ApplicationName = "Drive API .NET Quickstart";

        public void DriveImport()
        {
            UserCredential credential;
            var path = Server.MapPath("~");
            using (var stream = new FileStream(path + "client_secret.json", FileMode.Open, FileAccess.Read))
            {
                string credPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal);
                credPath = Path.Combine(credPath, ".credentials/drive-dotnet-quickstart.json");

                credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
                    GoogleClientSecrets.Load(stream).Secrets,
                    Scopes,
                    "user",
                    CancellationToken.None,
                    new FileDataStore(credPath, true)).Result;
                Debug.WriteLine("Credential file saved to: " + credPath);
            }

            // Create Drive API service.
            var service = new DriveService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = ApplicationName,
            });

            // Define parameters of request.
            FilesResource.ListRequest listRequest = service.Files.List();
            listRequest.PageSize = 10;
            listRequest.Fields = "nextPageToken, files(id, name)";

            // List files.
            IList<Google.Apis.Drive.v3.Data.File> files = listRequest.Execute().Files;
            Debug.WriteLine("Files:");
            if (files != null && files.Count > 0)
            {
                foreach (var file in files)
                {
                    Debug.WriteLine("{0} ({1})", file.Name, file.Id);
                }
            }
            else
            {
                Debug.WriteLine("No files found.");
            }


        }

        public void MarkImport()
        {
            string searchPattern = "*";
            var path = Server.MapPath("~");

            DirectoryInfo di = new DirectoryInfo(path + "Khao thi/New files/");

            FileInfo[] files =
                di.GetFiles(searchPattern, SearchOption.TopDirectoryOnly);

            try
            {
                //Microsoft.Office.Interop.Excel.Application app = new Microsoft.Office.Interop.Excel.Application();
                using (var context = new DB_Finance_AcademicEntities())
                {
                    foreach (var excel in files)
                    {
                        HSSFWorkbook hssfwb;
                        using (FileStream file = excel.OpenRead())
                        {
                            hssfwb = new HSSFWorkbook(file);
                        }

                        ISheet component = hssfwb.GetSheetAt(0);

                        var markCompRow = component.GetRow(8);
                        var titleRow = 7;
                        var subjectCode = excel.Name.Split(new char[] { '_' })[0].Trim().ToUpper();
                        var semesterString = excel.Name.Split(new char[] { '_', '.' })[1].Trim();
                        var tempTitleRow = component.GetRow(titleRow);
                        var tempPercentageRow = component.GetRow(titleRow + 1);

                        //Microsoft.Office.Interop.Excel.Workbook xls;
                        //xls = app.Workbooks.Open(excel.FullName);
                        //xls.SaveAs(path + "Khao thi/New files/" + excel.Name, Microsoft.Office.Interop.Excel.XlFileFormat.xlExcel8);

                        for (int i = 9; i <= component.LastRowNum; i++)
                        {
                            var row = component.GetRow(i);
                            if (row != null) //null is when the row only contains empty cells
                            {
                                var className = row.Cells[3].ToString().Trim().ToUpper();
                                var course = context.Courses.Where(q => q.ClassName.Equals(className) && q.Subject.SubjectCode == subjectCode).FirstOrDefault();

                                if (course == null)
                                {
                                    course = context.Courses.Create();
                                    course.TeacherId = 1;
                                    course.ClassName = className;
                                    course.StartDate = DateTime.ParseExact(component.GetRow(5).GetCell(0).ToString().Split(new char[] { ' ' })[2], "dd/MM/yyyy", null);
                                    course.EndDate = DateTime.ParseExact(component.GetRow(5).GetCell(0).ToString().Split(new char[] { ' ' })[4], "dd/MM/yyyy", null);

                                    var subject = context.Subjects.Where(q => q.SubjectCode == subjectCode).FirstOrDefault();

                                    if (subject == null)
                                    {
                                        subject = context.Subjects.Create();
                                        subject.SubjectCode = subjectCode;
                                        subject.SubjectName = component.GetRow(3).Cells[0].ToString().Split(new char[] { ':' })[1].Trim();
                                        context.Subjects.Add(subject);

                                        context.SaveChanges();
                                    }

                                    course.CourseName = subject.SubjectName;
                                    course.SubjectId = subject.Id;

                                    var semester = context.Semesters.Where(q => semesterString == q.Title + q.Year).FirstOrDefault();
                                    course.SemesterId = semester.Id;

                                    context.Courses.Add(course);
                                    context.SaveChanges();

                                    for (int j = 4; j <= row.LastCellNum && tempTitleRow.GetCell(j, MissingCellPolicy.RETURN_BLANK_AS_NULL).ToString().ToLower() != "total"; j++)
                                    {
                                        if (tempTitleRow.GetCell(j, MissingCellPolicy.RETURN_BLANK_AS_NULL) != null)
                                        {
                                            var value = tempTitleRow.GetCell(j, MissingCellPolicy.RETURN_BLANK_AS_NULL).ToString().Trim();
                                            var prev = tempTitleRow.GetCell(j - 1, MissingCellPolicy.RETURN_BLANK_AS_NULL).ToString().Trim();

                                            if (Regex.IsMatch(value, "\\d+$") || value == prev)
                                            {
                                                var courseMark = context.CourseMarks.Create();
                                                courseMark.CourseId = course.Id;
                                                courseMark.ComponentName = value;
                                                courseMark.Percentage = double.Parse(tempPercentageRow.GetCell(j, MissingCellPolicy.RETURN_BLANK_AS_NULL).ToString().Replace("%", ""));
                                                context.CourseMarks.Add(courseMark);
                                            }
                                            else if ((value.Contains("Final") || value.Contains("FE")) && !tempTitleRow.GetCell(j + 1, MissingCellPolicy.RETURN_BLANK_AS_NULL).ToString().ToLower().Contains("total"))
                                            {
                                                var courseMark = context.CourseMarks.Create();
                                                courseMark.CourseId = course.Id;
                                                courseMark.ComponentName = value;
                                                courseMark.Percentage = double.Parse(tempPercentageRow.GetCell(j, MissingCellPolicy.RETURN_BLANK_AS_NULL).ToString().Replace("%", ""));
                                                context.CourseMarks.Add(courseMark);
                                            }
                                        }
                                    }

                                    context.SaveChanges();
                                }

                                var studentCode = row.Cells[1].ToString().Trim().ToUpper();
                                var studentInCourse = context.StudentInCourses.Where(q => q.StudentMajor.StudentCode.ToUpper().Equals(studentCode) && q.CourseId == course.Id).FirstOrDefault();

                                if (studentInCourse == null)
                                {
                                    var studentMajor = context.StudentMajors.Where(q => q.StudentCode.ToUpper().Equals(studentCode)).FirstOrDefault();
                                    if (studentMajor == null)
                                    {
                                        var student = context.Students.Create();
                                        student.Name = row.GetCell(2, MissingCellPolicy.RETURN_BLANK_AS_NULL).ToString().Trim();
                                        context.Students.Add(student);
                                        context.SaveChanges();

                                        studentMajor = context.StudentMajors.Create();
                                        studentMajor.StudentCode = studentCode;
                                        studentMajor.LoginName = row.GetCell(0, MissingCellPolicy.RETURN_BLANK_AS_NULL).ToString().Trim();
                                        studentMajor.StudentId = student.Id;
                                        context.StudentMajors.Add(studentMajor);
                                        context.SaveChanges();
                                    }

                                    studentInCourse = context.StudentInCourses.Create();
                                    studentInCourse.StudentId = studentMajor.Id;
                                    studentInCourse.CourseId = course.Id;
                                    context.StudentInCourses.Add(studentInCourse);

                                    context.SaveChanges();
                                }

                                for (int j = 4; j < row.LastCellNum; j++)
                                {
                                    if (row.GetCell(j, MissingCellPolicy.RETURN_NULL_AND_BLANK) != null)
                                    {
                                        if (tempTitleRow.GetCell(j, MissingCellPolicy.RETURN_BLANK_AS_NULL) != null)
                                        {
                                            var value = tempTitleRow.GetCell(j, MissingCellPolicy.RETURN_BLANK_AS_NULL).ToString().Trim();
                                            var prev = tempTitleRow.GetCell(j - 1, MissingCellPolicy.RETURN_BLANK_AS_NULL).ToString().Trim();

                                            if (Regex.IsMatch(value, "\\d+$") || value == prev)
                                            {
                                                var markComponent = context.CourseMarks.Where(q => q.CourseId == course.Id && q.ComponentName == value).FirstOrDefault();
                                                var studentCourseMark = context.StudentCourseMarks.Where(q => q.StudentInCourseId == studentInCourse.Id && q.CourseMarkId == markComponent.Id).FirstOrDefault();
                                                if (studentCourseMark == null)
                                                {
                                                    studentCourseMark = context.StudentCourseMarks.Create();
                                                    studentCourseMark.CourseMarkId = markComponent.Id;
                                                    studentCourseMark.StudentInCourseId = studentInCourse.Id;
                                                    context.StudentCourseMarks.Add(studentCourseMark);

                                                    context.SaveChanges();
                                                }

                                                var mark = row.GetCell(j, MissingCellPolicy.RETURN_BLANK_AS_NULL);
                                                studentCourseMark.Mark = mark == null ? -1 : mark.NumericCellValue;
                                            }
                                            else if ((value.Contains("Final") || value.Contains("FE")) && !tempTitleRow.GetCell(j + 1, MissingCellPolicy.RETURN_BLANK_AS_NULL).ToString().ToLower().Contains("total"))
                                            {
                                                var markComponent = context.CourseMarks.Where(q => q.CourseId == course.Id && q.ComponentName == value).FirstOrDefault();
                                                var studentCourseMark = context.StudentCourseMarks.Where(q => q.StudentInCourseId == studentInCourse.Id && q.CourseMarkId == markComponent.Id).FirstOrDefault();
                                                if (studentCourseMark == null)
                                                {
                                                    studentCourseMark = context.StudentCourseMarks.Create();
                                                    studentCourseMark.CourseMarkId = markComponent.Id;
                                                    studentCourseMark.StudentInCourseId = studentInCourse.Id;
                                                    context.StudentCourseMarks.Add(studentCourseMark);

                                                    context.SaveChanges();
                                                }

                                                var mark = row.GetCell(j, MissingCellPolicy.RETURN_BLANK_AS_NULL);
                                                studentCourseMark.Mark = mark == null ? -1 : mark.NumericCellValue;
                                            }
                                            else if (tempTitleRow.GetCell(j, MissingCellPolicy.RETURN_BLANK_AS_NULL).ToString().ToLower() == "total")
                                            {
                                                var mark = row.GetCell(j, MissingCellPolicy.RETURN_BLANK_AS_NULL);
                                                studentInCourse.Average = mark == null ? -1 : mark.NumericCellValue;
                                            }
                                            else if (tempTitleRow.GetCell(j, MissingCellPolicy.RETURN_BLANK_AS_NULL).ToString().ToLower() == "status")
                                            {
                                                var status = row.GetCell(j, MissingCellPolicy.RETURN_BLANK_AS_NULL);
                                                if (status == null)
                                                {
                                                    studentInCourse.Status = 1;
                                                }
                                                else if (status.ToString().ToLower() == "đạt")
                                                {
                                                    studentInCourse.Status = 2;
                                                }
                                                else
                                                {
                                                    studentInCourse.Status = 3;
                                                }
                                            }
                                        }
                                    }
                                }

                                context.SaveChanges();
                            }
                        }
                    }
                    //app.Workbooks.Close();
                    //app.Quit();
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
            }
        }

        [HttpGet]
        public ActionResult TeacherImport()
        {
            string searchPattern = "*";
            var path = Server.MapPath("~");

            DirectoryInfo di = new DirectoryInfo(path + "DSGV/");

            FileInfo[] files =
                di.GetFiles(searchPattern, SearchOption.TopDirectoryOnly);

            try
            {
                //Microsoft.Office.Interop.Excel.Application app = new Microsoft.Office.Interop.Excel.Application();
                using (var context = new DB_Finance_AcademicEntities())
                {
                    foreach (var excel in files)
                    {
                        XSSFWorkbook xssfwb;
                        using (FileStream file = excel.OpenRead())
                        {
                            xssfwb = new XSSFWorkbook(file);
                        }

                        ISheet component = xssfwb.GetSheetAt(0);

                        var titleRow = 0;
                        var tempTitleRow = component.GetRow(titleRow);

                        for (int i = 1; i <= component.LastRowNum; i++)
                        {
                            var row = component.GetRow(i);
                            if (row != null) //null is when the row only contains empty cells
                            {
                                var eduEmail = row.Cells[7].ToString().Trim();
                                var feEmail = row.Cells[8].ToString().Trim();

                                var teacher = context.Teachers.Where(q => q.EduEmail.ToUpper() == eduEmail.ToUpper() || q.FeEmail.ToUpper() == feEmail.ToUpper()).FirstOrDefault();

                                if(teacher == null)
                                {
                                    teacher = context.Teachers.Create();
                                }

                                teacher.EduEmail = eduEmail;
                                teacher.FeEmail = feEmail;

                                teacher.TeacherCode = row.Cells[2].ToString().Trim();
                                teacher.LoginName = eduEmail.Contains("@fpt.") ? eduEmail.Split(new char[] { '@' })[0].ToLower() : feEmail.Split(new char[] { '@' })[0].ToLower();
                                teacher.Name = row.Cells[3].ToString().Trim();
                                teacher.Gender = row.Cells[4].ToString().Trim() == "Nam" ? true : false;
                                teacher.ContractName = row.Cells[5].ToString().Trim();
                                teacher.Position = row.Cells[6].ToString().Trim();

                                if (teacher.Id == 0)
                                {
                                    context.Teachers.Add(teacher);
                                }

                                context.SaveChanges();
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                return Json(new { message = "exception" }, JsonRequestBehavior.AllowGet);
            }

            return Json(new { message = "success" }, JsonRequestBehavior.AllowGet);
        }
    }
}