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

        
    }
}