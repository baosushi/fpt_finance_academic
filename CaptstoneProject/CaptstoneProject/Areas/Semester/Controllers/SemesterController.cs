using CaptstoneProject.Controllers;
using CaptstoneProject.Models;
using DataService.Model;
using OfficeOpenXml;
using System;

using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace CaptstoneProject.Areas.Semester.Controllers
{
    public class SemesterController : MyBaseController
    {
        // GET: Semester/Semester
        public ActionResult Index()
        {
            return View("SemesterMarkExport");
        }


        public JsonResult GetSemester()
        {
            using (var context = new DB_Finance_AcademicEntities())
            {
                var semesters = context.Semesters.OrderByDescending(q => q.Year).ThenByDescending(q => q.SemesterInYear);
                var semesterList = semesters.Select(q => new
                {
                    q.Id,
                    q.Title,
                    q.Year
                }).ToList();
                return Json(new { success = true, semesterList = semesterList });
            }
        }

        public FileResult ExportExcelbySemester(int? semesterId, string semesterName)
        {
            List<StudentMarkInSemester> result = null;
            //string semesterName = null;
            using ( var context = new DB_Finance_AcademicEntities())
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
                              Status = ((StudentInCourseStatus)sc.Status).ToString()
                          }).OrderBy(q => q.RollNumber).ToList();
            }
            string fileName = "Export_Mark_" + semesterName;
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
    }
}