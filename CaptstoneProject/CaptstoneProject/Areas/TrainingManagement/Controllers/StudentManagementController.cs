using CaptstoneProject.Controllers;
using CaptstoneProject.Models;
using DataService.Model;
using Microsoft.AspNet.Identity.Owin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using static CaptstoneProject.Models.AreaViewModel;

namespace CaptstoneProject.Areas.TrainingManagement.Controllers
{
    public class StudentManagementController : MyBaseController
    {
        // GET: TrainingManagement/StudentManagement
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult GetListStudentAccount(JQueryDataTableParamModel param)
        {
            try
            {
                using (var context = new DB_Finance_AcademicEntities())
                {


                    if (param.sSearch != null)
                        param.sSearch = param.sSearch.Trim();

                    int count = 0;


                    var studentMajor = context.StudentMajors.Where(a => string.IsNullOrEmpty(param.sSearch) ||
                            ((a.Student.Name.Contains(param.sSearch)) || a.StudentCode.Contains(param.sSearch)));

                    int totalRecords = studentMajor.Count();

                    count = param.iDisplayStart + 1;

                    var rs = studentMajor.OrderByDescending(q => q.Id).Skip(param.iDisplayStart).Take(param.iDisplayLength)
                            .ToList()
                            .Select(a => new IConvertible[]
                                {
                        count++,
                        string.IsNullOrEmpty(a.Student.Name) ? "-": a.Student.Name,
                        string.IsNullOrEmpty(a.LoginName) ? "-" : a.LoginName,
                        string.IsNullOrEmpty(a.StudentCode) ? "-" : a.StudentCode,
                        a.Student.Id ,
                                }).ToList();


                    int totalDisplay = studentMajor.Count();

                    return Json(new
                    {
                        sEcho = param.sEcho,
                        iTotalRecords = totalRecords,
                        iTotalDisplayRecords = totalDisplay,
                        aaData = rs
                    }, JsonRequestBehavior.AllowGet);

                }
            }
            catch (Exception e)
            {
                Response.StatusCode = (int)HttpStatusCode.BadRequest;
                return Json(new { success = false, message = e.Message });
            }
        }

        public ActionResult StudentDetail(int id)
        {
            try
            {
                using (var context = new DB_Finance_AcademicEntities())
                {
                    var student = context.Students.Find(id);
                    if (student == null)
                    {
                        return HttpNotFound();
                    }
                    StudentViewModel model = new StudentViewModel();
                    model.Id = student.Id;
                    model.Name = student.Name;
                    var studentMarjorOfCurrentProgram = student.StudentMajors.OrderByDescending(q => q.Id).FirstOrDefault(); //neu hoc sinh chuyen nganh thi account moi nhat se la account duoc tao sau
                    var currentloginName = studentMarjorOfCurrentProgram.LoginName;
                    var currentStudentCode = studentMarjorOfCurrentProgram.StudentCode;

                    model.CurrentStudentCode = currentStudentCode;

                    var userManager = HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
                    var currentLoginAccount = userManager.Users.Where(q => q.Email.Contains(currentloginName)).FirstOrDefault();
                    if (currentLoginAccount != null)
                        model.Email = currentLoginAccount.Email;

                    model.CurrentAccount = context.StudentMajors.Where(q => q.StudentId == student.Id)
                        .OrderByDescending(q => q.Id).FirstOrDefault().Accounts.FirstOrDefault();

                    model.TotalRegistered = (from reg in context.Registrations
                                             join studentmajor in context.StudentMajors on reg.StudentMajorId equals studentmajor.Id
                                             join regDetail in context.RegistrationDetails on reg.Id equals regDetail.RegistrationId
                                             where studentmajor.StudentId == student.Id
                                             select regDetail).Count();

                    model.TotalMoneySpent = (from reg in context.Registrations
                                             join studentmajor in context.StudentMajors
                                             on reg.StudentMajorId equals studentmajor.Id
                                             where studentmajor.Id == student.Id
                                             select reg.FinalAmount).Sum();
                    return View(model);

                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return HttpNotFound();
            }
        }


        public ActionResult LoadOrder(JQueryDataTableParamModel param, int studentId, string startTime, string endTime)
        {
            try
            {
                using (var context = new DB_Finance_AcademicEntities())
                {
                    var startDate = startTime.ToDateTime().GetStartOfDate();
                    var endDate = endTime.ToDateTime().GetEndOfDate();
                    var listOrder = context.Registrations.Where(q => q.RegisteredBy >= startDate
                    && q.RegisteredBy <= endDate && q.Status == (int)RegistrationStatus.Done && q.StudentMajor.StudentId == studentId);



                    //if (!string.IsNullOrWhiteSpace(param.sSearch))
                    //{
                    //    listOrder = listOrder.Where(q => q.InvoiceID.ToLower().Contains(param.sSearch.ToLower()));
                    //}
                    int count = 0;
                    count = param.iDisplayStart + 1;

                    //try
                    //{
                    var result = listOrder
                        .OrderByDescending(q => q.RegisteredBy)
                        .Skip(param.iDisplayStart)
                        .Take(param.iDisplayLength)
                        .ToList();
                    var list = result.Select(a => new object[]
                            {
                        ++count, // 0
                        a.RegistrationDetailTotalQuantity, // 1
                        a.StudentMajor.StudentCode,//2
                        a.FinalAmount, // 3
                        a.RegisteredBy.Value.ToString("dd/MM/yyyy"), // 4
                        a.CheckInPerson, // 5
                            });
                    var totalRecords = listOrder.Count();

                    return Json(new
                    {
                        sEcho = param.sEcho,
                        iTotalRecords = totalRecords,
                        iTotalDisplayRecords = totalRecords,
                        aaData = list
                    }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return Json(new { success = false, message = e.Message });
            }
        }

        public ActionResult loadAllStudentAccount(int studentId)
        {
            try
            {
                var count = 1;
                using (var context = new DB_Finance_AcademicEntities())
                {
                    var accountList = context.Accounts.Where(q => q.StudentMajor.StudentId == studentId)
                        .OrderByDescending(q => q.StartDate).ToList().Select(q => new IConvertible[]
                    {
                        count++, //0
                        q.StudentMajor.StudentCode, //1
                        q.StartDate.Value.ToString("dd/MM/yyyy"),
                        q.Type,
                        q.Balance, //4
                        q.Active
                    }).ToList();

                    return Json(new
                    {
                        success = true,
                        iTotalDisplayRecords = accountList.Count(),
                        iTotalRecord = accountList.Count(),
                        aaData = accountList
                    }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return Json(new { success = false, message = e.Message });
            }
        }
    }
}