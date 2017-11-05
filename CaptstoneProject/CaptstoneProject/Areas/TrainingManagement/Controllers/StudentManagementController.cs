using DataService.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using static CaptstoneProject.Models.AreaViewModel;

namespace CaptstoneProject.Areas.TrainingManagement.Controllers
{
    public class StudentManagementController : Controller
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
                    var studentCodes = student.StudentMajors.Select(q => q.StudentCode).ToList();
                    model.Account = context.Accounts.Where(q => studentCodes.Contains(q.StudentCode)).FirstOrDefault();
                    return View(model);

                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return HttpNotFound();
            }
        }


        //public JsonResult LoadOrder(JQueryDataTableParamModel param, int brandId, int customID, int selectedStoreID, string startTime, string endTime)
        //{

        //    var orderApi = new OrderApi();
        //    IQueryable<Order> listOrder = null;
        //    var startDate = startTime.ToDateTime().GetStartOfDate();
        //    var endDate = endTime.ToDateTime().GetEndOfDate();
        //    var Orders = orderApi.GetAllFinishedOrdersByDateAndCustomer(startDate, endDate, brandId, customID);

        //    if (selectedStoreID == 0)
        //    {
        //        listOrder = Orders;
        //    }
        //    else
        //    {
        //        listOrder = Orders.Where(a => a.StoreID == selectedStoreID);
        //    }

        //    if (!string.IsNullOrWhiteSpace(param.sSearch))
        //    {
        //        listOrder = listOrder.Where(q => q.InvoiceID.ToLower().Contains(param.sSearch.ToLower()));
        //    }
        //    int count = 0;
        //    count = param.iDisplayStart + 1;

        //    //try
        //    //{
        //    var result = listOrder
        //        .OrderByDescending(q => q.CheckInDate)
        //        .Skip(param.iDisplayStart)
        //        .Take(param.iDisplayLength)
        //        .ToList();
        //    var list = result.Select(a => new object[]
        //            {
        //                ++count, // 0
        //                string.IsNullOrEmpty(a.InvoiceID) ? "N/A" : a.InvoiceID, // 1
        //                a.OrderDetailsTotalQuantity, // 2
        //                a.FinalAmount, // 3
        //                a.CheckInDate.Value.ToString("dd/MM/yyyy HH:mm:ss"), // 4
        //                a.OrderType, // 5
        //                a.CheckInPerson, // 6
        //                a.RentID, // 7
        //                a.Store == null ? "N/A" : a.Store.Name, // 8
        //                a.Customer!=null ? a.Customer.Name : "N/A", // 9
        //                //string.IsNullOrEmpty(a.DeliveryAddress) ? "N/A" : a.DeliveryAddress, //10
        //                //a.Customer!=null ? a.Customer.Phone : "N/A", //11
        //                //a.Notes !=null? a.Notes : "", //12
        //                //a.TotalAmount,// 12
        //                //(a.Discount + a.DiscountOrderDetail), // 13
        //                //a.Store.Name // 14
        //            });
        //    var totalRecords = listOrder.Count();

        //    return Json(new
        //    {
        //        sEcho = param.sEcho,
        //        iTotalRecords = totalRecords,
        //        iTotalDisplayRecords = totalRecords,
        //        aaData = list
        //    }, JsonRequestBehavior.AllowGet);
        //}



    }
}