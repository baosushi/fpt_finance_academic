﻿using CaptstoneProject.Models;
using DataService.Model;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using static CaptstoneProject.Models.AreaViewModel;
using Microsoft.AspNet.Identity.Owin;
using System.Net;
using Microsoft.AspNet.Identity;
using System.Threading.Tasks;

namespace CaptstoneProject.Areas.Admin.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        // GET: Admin/Admin
        public ActionResult Index()
        {
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

        public ActionResult AccountManagement()
        {
            return View();
        }

        [HttpPost]
        public ActionResult DownloadAccountTemplate()
        {
            MemoryStream ms = new MemoryStream();
            using (var excelpackage = new ExcelPackage(ms))
            {
                var ws = excelpackage.Workbook.Worksheets.Add("Sheet1");
                char startHeaderChar = 'A';
                int startHeaderRow = 1;
                var fileName = "Template Import Google Account " + ".xlsx";

                List<String> roleList;
                using (var context = new DB_Finance_AcademicEntities())
                {
                    roleList = context.AspNetRoles.Select(q => q.Name).ToList();
                }

                //title
                ws.Cells["" + (startHeaderChar) + startHeaderRow].Value = "List of Google Account";

                //header
                ws.Cells["" + startHeaderChar + (++startHeaderRow)].Value = "Email";
                ws.Cells["" + startHeaderChar + startHeaderRow].Style.Font.Bold = true;

                ws.Cells["" + (++startHeaderChar) + startHeaderRow].Value = "Student Full Name";
                ws.Cells["" + startHeaderChar + startHeaderRow].Style.Font.Bold = true;

                ws.Cells["" + (++startHeaderChar) + startHeaderRow].Value = "Role";
                ws.Cells["" + startHeaderChar + startHeaderRow].Style.Font.Bold = true;

                startHeaderChar = 'A';
                ws.Cells["" + startHeaderChar + (++startHeaderRow)].Value = "cuongse60022@fpt.edu.vn";
                ws.Cells["" + (++startHeaderChar) + startHeaderRow].Value = "Tô Chí Cường";
                //ws.Cells["" + (++startHeaderChar) + startHeaderRow].DataValidation.AddListDataValidation();

                startHeaderChar = 'A';
                ws.Cells["" + startHeaderChar + (++startHeaderRow)].Value = "danhse60032@fpt.edu.vn";
                ws.Cells["" + (++startHeaderChar) + startHeaderRow].Value = "Đổng Công Danh";


                startHeaderChar++;
                var range = ExcelRange.GetAddress(3, 3, ExcelPackage.MaxRows, 3);
                var val = ws.DataValidations.AddListValidation(range);
                val.ShowErrorMessage = true;
                val.ErrorTitle = "An invalid item was entered";
                val.Error = "Please choose role from drop down list only";
                val.ShowInputMessage = true;
                val.Prompt = "Choose one role from drop down list";

                foreach (var item in roleList)
                {
                    val.Formula.Values.Add(item);
                }

                //contentType for .xlsx
                var contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                excelpackage.SaveAs(ms);
                ms.Seek(0, SeekOrigin.Begin);
                return File(ms, contentType, fileName);
            }
        }

        [HttpPost]
        public async Task<ActionResult> ImportGoogleAccount()
        {
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

                        if (System.IO.File.Exists(@savePath))
                        {
                            System.IO.File.Delete(@savePath);
                        }
                        if (System.IO.File.Exists(@savePath + "x"))
                        {
                            System.IO.File.Delete(@savePath + "x");
                        }

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
                            var ws = excelPackage.Workbook.Worksheets.FirstOrDefault();



                            var queryCell = from cell in ws.Cells where cell.Value.ToString().Trim().ToUpper().Equals("EMAIL") select cell;
                            var targetCell = queryCell.FirstOrDefault();
                            var headerRow = targetCell.Start.Row;
                            var headerColumn = targetCell.Start.Column;

                            var userManager = HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();

                            for (int i = headerRow + 1; i <= ws.Dimension.End.Row; i++)
                            {
                               
                               
                                    var email = ws.Cells[i, headerColumn].Value.ToString();
                                    var fullname = ws.Cells[i, headerColumn+1].Value.ToString();
                                    var role = ws.Cells[i, headerColumn+2].Value.ToString();

                               var userExist = userManager.FindByEmail(email);

                                if(userExist == null)
                                {
                                    // using email instead of providerKey for automatic import google account
                                    UserLoginInfo userInfo = new UserLoginInfo("Google", email);
                                    ApplicationUser user = new ApplicationUser() { UserName = email, Email = email, FullName = fullname };

                                    await userManager.CreateAsync(user);
                                    await userManager.AddLoginAsync(user.Id, userInfo);

                                    userManager.AddToRole(user.Id, role);
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

            return Json(new { success = true, message = "Import Account successed" });
        }
    }
}