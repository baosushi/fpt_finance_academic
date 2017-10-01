using DataService.Model;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace CaptstoneProject.Areas.Semester.Controllers
{
    public class SemesterController : Controller
    {
        // GET: Semester/Semester
        public ActionResult Index()
        {
            return View();
        }

        public JsonResult getSemester()
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
                return Json(new { semesterList = semesterList });
            }
        }

        public JsonResult exportExcelbySemester(int semesterId)
        {

            ExcelPackage excelPack = new ExcelPackage();
            ExcelWorksheet ws1 = excelPack.Workbook.Worksheets.Add("Sheet1");


            return Json(new { success = true, data =});
        }
    }
}