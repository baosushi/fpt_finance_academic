using CaptstoneProject.Controllers;
using CaptstoneProject.Models;
using DataService.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using static CaptstoneProject.Models.AreaViewModel;

namespace CaptstoneProject.Areas.TransactionManagement.Models
{
    public class TransactionController : MyBaseController
    {
        // GET: TransactionManagement/Transaction
        public ActionResult Index()
        {
            try
            {

                var transactionStatusList = new List<TransactionEnumViewModel>();
                var allTransactionStatus = Enum.GetValues(typeof(TransactionStatus));
                foreach (var status in allTransactionStatus)
                {
                    transactionStatusList.Add(new TransactionEnumViewModel
                    {
                        Name = Enum.GetName(typeof(TransactionStatus), status),
                        Value = (int)status
                    });
                }

                var transactionAmountList = new List<TransactionEnumViewModel>();
                var allTransactionAmount = Enum.GetValues(typeof(TransactionAmount));
                foreach (var amount in allTransactionAmount)
                {
                    transactionAmountList.Add(new TransactionEnumViewModel
                    {
                        Name = Enum.GetName(typeof(TransactionAmount), amount),
                        Value = (int)amount
                    });
                }

                ViewBag.TransactionStatusList = transactionStatusList;
                ViewBag.TransactionAmountList = transactionAmountList;
                return View();
            }
            catch (Exception e)
            {
                throw;
            }
        }

        [HttpPost]
        public ActionResult CreateTransaction(int studentMajorId, decimal amount,int transactionType, string description)
        {
            int form = -1;
            
            try
            {
                using (var context = new DB_Finance_AcademicEntities())
                {

                    var account = context.Accounts.Where(q => q.StudentMajorId == studentMajorId)
                        .FirstOrDefault();
                    if (account == null)
                    {
                        return Json(new { success = false, message = "Create transaction failed!" });
                    }
                    switch (transactionType)
                    {
                        case (int)TransactionTypeEnum.AddFunds:
                            form = (int)TransactionForm.Increase;
                            break;
                        case (int)TransactionTypeEnum.TuitionPayment:
                            form = (int)TransactionForm.Decrease;
                            break;
                        case (int)TransactionTypeEnum.RefundTuitionFee:
                            form = (int)TransactionForm.Decrease;
                            break;
                        case (int)TransactionTypeEnum.RollbackIncrease:
                            form = (int)TransactionForm.Increase;
                            break;
                        case (int)TransactionTypeEnum.RollbackDecrease:
                            form = (int)TransactionForm.Decrease;
                            break;

                    }
                    if(form == -1 || transactionType == -1)
                    {
                        return Json(new { success = false, message = "Create transaction Failed!" });
                    }
                    var formTransaction = (form == (int)TransactionForm.Increase ? true : false);
                    context.Transactions.Add(new Transaction
                    {
                        AccountId = account.Id,
                        Amount = amount,
                        Date = DateTime.Now,
                        IsIncreaseTransaction = formTransaction,
                        TransactionType = transactionType,
                        Notes = description,
                        Status = (int)TransactionStatus.New
                    });

                    context.SaveChanges();
                    return Json(new { success = true, message = "Create transaction successed!" });
                }
            }
            catch (Exception e)
            {
                return Json(new { success = false, message = e.Message });
            }
        }


        public ActionResult GetAccountInformation(int studentMajorId = -1)
        {
            try
            {
                if (studentMajorId != -1)
                    using (var context = new DB_Finance_AcademicEntities())
                    {
                        var account = context.Accounts.Where(q => q.StudentMajorId == studentMajorId).FirstOrDefault();
                        var accountName = account.Name;
                        var studentName = account.StudentMajor.Student.Name;
                        return Json(new { success = true, accountName = accountName, studentName = studentName });
                    }
                else
                    return Json(new { success = false, message = "Error! Account not found" });

            }
            catch (Exception e)
            {

                return Json(new { success = false, message = e.Message });
            }
        }

        public ActionResult CheckMembershipCardCode(string studentCode)
        {
            try
            {
                using (var context = new DB_Finance_AcademicEntities())
                {

                    var account = context.Accounts.Where(q => q.StudentMajor.StudentCode
                    .ToUpper().Equals(studentCode.Trim().ToUpper()) && q.Active == true).FirstOrDefault();

                    if (account != null)
                    {
                        var student = account.StudentMajor.Student;
                        return Json(new
                        {
                            success = true,
                            AccountName = account.Name,
                            Student = new { Name = student.Name, StudentCode = account.StudentMajor.StudentCode }
                        }, JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        return Json(new { success = false, message = "Không tồn tại tài khoản thanh toán!" }, JsonRequestBehavior.AllowGet);
                    }

                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return Json(new { success = false, message = "Có lỗi xảy ra, xin vui lòng liện hệ admin" }, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult CheckStudentCode(string studentCode)
        {
            try
            {
                using (var context = new DB_Finance_AcademicEntities())
                {

                    var accountList = context.Accounts.Where(q => q.StudentMajor.StudentCode
                    .ToUpper().Contains(studentCode.Trim().ToUpper()) && q.Active == true).Take(10)
                    .Select(q => new StudentSearchOptions
                    {
                        AccountName = q.Name,
                        StudentCode = q.StudentMajor.StudentCode,
                        StudentMarjorId = q.StudentMajor.Id,
                        StudentName = q.StudentMajor.Student.Name
                    })
                    .ToList();

                    return Json(new
                    {
                        success = true,
                        list = accountList
                    }, JsonRequestBehavior.AllowGet);


                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return Json(new { success = false, message = "Có lỗi xảy ra, xin vui lòng liện hệ admin" }, JsonRequestBehavior.AllowGet);
            }
        }


        public ActionResult GetAllTransactionsByDateRange(JQueryDataTableParamModel param, string startTime, string endTime, int transactionStatus, int transactionType)
        {
            try
            {
                using (var context = new DB_Finance_AcademicEntities())
                {
                    DateTime sTime = startTime.ToDateTime().GetStartOfDate();
                    DateTime eTime = endTime.ToDateTime().GetEndOfDate();
                    var result = context.Transactions.Where(q => q.Date >= sTime && q.Date <= eTime);
                    var count = param.iDisplayStart + 1;

                    int transactionForm = -1;

                    switch (transactionType)
                    {
                        case (int)TransactionTypeEnum.AddFunds:
                            transactionForm = (int)TransactionForm.Increase;
                            break;
                        case (int)TransactionTypeEnum.TuitionPayment:
                            transactionForm = (int)TransactionForm.Decrease;
                            break;
                        case (int)TransactionTypeEnum.RefundTuitionFee:
                            transactionForm = (int)TransactionForm.Decrease;
                            break;
                        case (int)TransactionTypeEnum.RollbackIncrease:
                            transactionForm = (int)TransactionForm.Increase;
                            break;
                        case (int)TransactionTypeEnum.RollbackDecrease:
                            transactionForm = (int)TransactionForm.Decrease;
                            break;
                        default:
                            break;

                    }


                    if (transactionStatus != -1)
                    {
                        result = result.Where(a => a.Status == transactionStatus);
                    }

                    if (transactionForm != -1)
                    {
                        result = result.Where(a => a.IsIncreaseTransaction == (transactionForm == (int)TransactionForm.Increase));
                    }

                    if (transactionType != -1)
                    {
                        result = result.Where(a => a.TransactionType == transactionType);
                    }

                    IQueryable<Transaction> filteredResult;

                    filteredResult = result
                    .OrderByDescending(q => q.Date)
                    .Where(a => string.IsNullOrEmpty(param.sSearch) || a.Account.StudentMajor.StudentCode.ToUpper().Contains(param.sSearch.Trim().ToUpper()));


                    var list = filteredResult.Skip(param.iDisplayStart)
                        .Take(param.iDisplayLength)
                        .AsEnumerable()
                        .Select(q => new IConvertible[] {
                        count++, //0
                        q.Account.StudentMajor.StudentCode,
                        q.Account.StudentMajor.Student.Name,
                        q.Amount, // 3
                        q.Date.Value.ToString("dd/MM/yyyy HH:mm:ss"),
                        q.TransactionType, //5
                        (q.UserName==null) ? "-" : q.UserName, //User tạo ra transaction này //6
                        q.Status, //7
                        q.IsIncreaseTransaction,//8
                        q.Id, //transaction ID //9
                        q.AccountId, //10
                        q.Account.StudentMajor.StudentId //11
                        }).ToList();

                    var totalRecords = result.Count();
                    var displayRecords = filteredResult.Count();

                    return Json(new
                    {
                        sEcho = param.sEcho,
                        iTotalRecords = totalRecords,
                        iTotalDisplayRecords = displayRecords,
                        aaData = list,
                    }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return Json(new { success = false, message = "Có lỗi xảy ra, xin vui lòng liện hệ admin" }, JsonRequestBehavior.AllowGet);
            }
        }

       


        public ActionResult Edit(int Id)
        {

            var model = new TransactionEditViewModel();
            model.Id = Id;
            return View(model);
        }

        [HttpPost]
        public ActionResult Approve(int transactionId, bool isApproved)
        {
            try
            {
                using (var context = new DB_Finance_AcademicEntities())
                {


                    var model = context.Transactions.Find(transactionId);
                    if (isApproved)
                    {
                        model.Status = (int)TransactionStatus.Approve;
                        var account = context.Accounts.Find(model.AccountId);
                        if (account != null)
                        {
                            if (model.IsIncreaseTransaction != null && model.IsIncreaseTransaction == true)
                            {
                                account.Balance += model.Amount;
                            }
                            else
                            {
                                account.Balance -= model.Amount;
                            }
                        }
                    }
                    else
                    {
                        model.Status = (int)TransactionStatus.Cancel;
                    }
                    context.SaveChanges();
                    return Json(new { success = true });
                }
            }
            catch (System.Exception e)
            {
                Console.WriteLine(e);
                return Json(new { success = false });
            }
        }

        [HttpPost]
        public ActionResult Edit(TransactionEditViewModel model)
        {
            if (!this.ModelState.IsValid)
            {
                return View(model);
            }
            try
            {
                using (var context = new DB_Finance_AcademicEntities())
                {
                    var account = context.Accounts.Find(model.AccountId);
                    var transaction = context.Transactions.Find(model.Id);
                    if (model.IsIncreaseTransaction == false && account.Balance < model.Amount)
                    {
                        //return RedirectToAction("CustomerDetail", "Customer", new { Id = account.CustomerID });
                        return RedirectToAction("Index", "Transaction");
                    }

                    transaction.Amount = model.Amount;
                    transaction.Date = model.Date;
                    transaction.Notes = model.Notes;
                    transaction.IsIncreaseTransaction = model.IsIncreaseTransaction;

                    context.SaveChanges();

                    //return RedirectToAction("CustomerDetail", "Customer", new { Id = account.CustomerID });
                    return RedirectToAction("Index", "Transaction");

                }
            }
            catch (Exception e)
            {

                return Json(new { success = false, message = e.Message });
            }
        }




        public class TransactionEnumViewModel
        {
            public dynamic Value { get; set; }
            public string Name { get; set; }
        }

        public class StudentSearchOptions
        {
            public string StudentName { get; set; }
            public string StudentCode { get; set; }
            public int StudentMarjorId { get; set; }
            public string AccountName { get; set; }
        }


    }
}