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
            catch (Exception)
            {

                throw;
            }
        }

        [HttpPost]
        public ActionResult CreateTransaction(string studentCode, decimal amount, int? form, int type, string description)
        {
            try
            {
                using (var context = new DB_Finance_AcademicEntities())
                {

                    var account = context.Accounts.Where(q => q.StudentMajor.StudentCode.ToUpper().Equals(studentCode.Trim().ToUpper()))
                        .FirstOrDefault();
                    var formTrasaction = true;
                    if (form != null)
                    {
                        if (form == 1)
                        {
                            formTrasaction = true;
                        }
                        else
                        {
                            formTrasaction = false;
                        }
                    }
                    else
                    {
                        return Json(new { success = false, message = "Giao dịch thất bại!" });
                    }

                    context.Transactions.Add(new Transaction
                    {
                        AccountId = account.Id,
                        Amount = amount,
                        Date = DateTime.Now,
                        IsIncreaseTransaction = formTrasaction,
                        TransactionType = type,
                        Notes = description,
                        Status = (int)TransactionStatus.New
                    });

                    return Json(new { success = true, message = "Giao dịch thành công!" });
                }
            }
            catch (Exception)
            {
                return Json(new { success = false, message = "Có lỗi xảy ra, xin vui lòng liện hệ admin" });
            }
        }

        public ActionResult CheckMembershipCardCode(string studentCode)
        {
            try
            {
                using (var context = new DB_Finance_AcademicEntities())
                {

                    var account = context.Accounts.Where(q => q.StudentMajor.StudentCode.ToUpper().Equals(studentCode.Trim().ToUpper())).FirstOrDefault();

                    if (account != null)
                    {
                        var student = context.StudentMajors.Where(q => q.StudentCode.ToUpper().Equals(studentCode.Trim().ToUpper())).FirstOrDefault();
                        return Json(new { success = true, AccountName = account.Name, Customer = new { Name = student.Student.Name, Phone = "none" } }, JsonRequestBehavior.AllowGet);
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

        public ActionResult GetAllTransactionsByDateRange(JQueryDataTableParamModel param, string startTime, string endTime, int transactionStatus, int transactionType, int transactionMode)
        {
            try
            {
                using (var context = new DB_Finance_AcademicEntities())
                {
                    DateTime sTime = startTime.ToDateTime().GetStartOfDate();
                    DateTime eTime = endTime.ToDateTime().GetEndOfDate();
                    var result = context.Transactions.Where(q => q.Date >= sTime && q.Date <= eTime);
                    var count = param.iDisplayStart + 1;

                    if (transactionStatus != -1)
                    {
                        result = result.Where(a => a.Status == transactionStatus).OrderBy(q => q.Date);
                    }

                    if (transactionType != -1)
                    {
                        result = result.Where(a => a.IsIncreaseTransaction == (transactionType == 0)).OrderBy(q => q.Date);
                    }

                    if (transactionMode != -1)
                    {
                        result = result.Where(a => a.TransactionType == transactionMode).OrderBy(q => q.Date);
                    }

                    IQueryable<Transaction> filteredResult;

                    filteredResult = result
                    .OrderByDescending(q => q.Date)
                    .Where(a => string.IsNullOrEmpty(param.sSearch) || a.Account.StudentMajor.StudentCode.ToUpper().Contains(param.sSearch.Trim().ToUpper()));


                    var list = filteredResult.Skip(param.iDisplayStart)
                        .Take(param.iDisplayLength)
                        .ToList()
                        .Select(q => new IConvertible[] {
                        count++,
                        q.Account.Name,
                        context.StudentMajors.Where(a => a.StudentCode.Equals(q.Account.StudentMajor.StudentCode)).FirstOrDefault().Student.Name,
                        q.Amount,
                        q.Date.Value.ToString("dd/MM/yyyy HH:mm:ss"),
                        String.IsNullOrEmpty(q.Notes) ? "-" : q.Notes,
                        (q.UserName==null) ? "-" : q.UserName, //User tạo ra transaction này
                        q.Status,
                        q.Id, //transaction ID
                        q.IsIncreaseTransaction,
                        q.AccountId,
                        });
                    var list2 = result
                        .Where(a => string.IsNullOrEmpty(param.sSearch) || a.Account.StudentMajor.StudentCode.ToLower().Contains(param.sSearch.Trim().ToLower()))
                        .ToList()
                        .Select(q => new IConvertible[] {
                        q.Amount,
                        q.Status,
                        q.IsIncreaseTransaction
                        });

                    var totalRecords = result.Count();
                    var displayRecords = filteredResult.Count();

                    return Json(new
                    {
                        sEcho = param.sEcho,
                        iTotalRecords = totalRecords,
                        iTotalDisplayRecords = displayRecords,
                        aaData = list,
                        totalData = list2,
                    }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return Json(new { success = false, message = "Có lỗi xảy ra, xin vui lòng liện hệ admin" }, JsonRequestBehavior.AllowGet);
            }
        }

        public JsonResult LoadTotalTransaction(string startDate, string endDate)
        {
            try
            {
                using (var context = new DB_Finance_AcademicEntities())
                {

                    var startTime = startDate.ToDateTime().GetStartOfDate();
                    var endTime = endDate.ToDateTime().GetEndOfDate();
                    var transaction = context.Transactions.Where(q => q.Date >= startTime && q.Date <= endTime);

                    var numberIncrease = 0;
                    var numberIncreaseOptimize = 0;
                    var numberDecreaseOptimize = 0;
                    var numberDecrease = 0;
                    var numberIncreaseRollback = 0;
                    var numberDecreaseRollback = 0;
                    var numberActiveCard = 0;
                    var numberDecreaseCancel = 0;
                    var numberIncreaseCancel = 0;
                    decimal revenueIncrease = 0;
                    decimal revenueDecrease = 0;
                    decimal revenueIncreaseRollback = 0;
                    decimal revenueDecreaseRollback = 0;
                    decimal revenueActiveCard = 0;
                    decimal revenueIncreaseOptimize = 0;
                    decimal revenueDecreaseOptimize = 0;
                    decimal revenueDecreaseCancel = 0;
                    decimal revenueIncreaseCancel = 0;

                    foreach (var item in transaction)
                    {
                        if (item.IsIncreaseTransaction != null && item.IsIncreaseTransaction == true)
                        {
                            if (item.Status == (int)TransactionStatus.Approve)
                            {
                                numberIncrease++;
                                revenueIncrease += item.Amount == null ? 0 : item.Amount.Value;
                                numberIncreaseOptimize++;
                                revenueIncreaseOptimize += item.Amount == null ? 0 : item.Amount.Value;
                                if (item.TransactionType == (int)TransactionTypeEnum.RollBack)
                                {
                                    numberIncreaseRollback++;
                                    revenueIncreaseRollback += item.Amount == null ? 0 : item.Amount.Value;
                                }
                                else
                                {
                                    if (item.TransactionType == (int)TransactionTypeEnum.ActiveCard)
                                    {
                                        numberActiveCard++;
                                        revenueActiveCard += item.Amount == null ? 0 : item.Amount.Value;
                                    }
                                }
                            }
                            else
                            {
                                if (item.Status == (int)TransactionStatus.New)
                                {
                                    numberIncreaseOptimize++;
                                    revenueIncreaseOptimize += item.Amount == null ? 0 : item.Amount.Value;
                                }
                                else
                                {
                                    if (item.Status == (int)TransactionStatus.Cancel)
                                    {
                                        numberIncreaseCancel++;
                                        revenueIncreaseCancel += item.Amount == null ? 0 : item.Amount.Value;
                                    }
                                }
                            }
                        }
                        else
                        {
                            if (item.Status == (int)TransactionStatus.Approve)
                            {
                                numberDecrease++;
                                revenueDecrease += item.Amount == null ? 0 : item.Amount.Value;
                                numberDecreaseOptimize++;
                                revenueDecreaseOptimize += item.Amount == null ? 0 : item.Amount.Value;
                                if (item.TransactionType == (int)TransactionTypeEnum.RollBack)
                                {
                                    numberDecreaseRollback++;
                                    revenueDecreaseRollback += item.Amount == null ? 0 : item.Amount.Value;
                                }
                            }
                            else
                            {
                                if (item.Status == (int)TransactionStatus.New)
                                {
                                    numberDecreaseOptimize++;
                                    revenueDecreaseOptimize += item.Amount == null ? 0 : item.Amount.Value;
                                }
                                else
                                {
                                    if (item.Status == (int)TransactionStatus.Cancel)
                                    {
                                        numberDecreaseCancel++;
                                        revenueDecreaseCancel += item.Amount == null ? 0 : item.Amount.Value;
                                    }
                                }
                            }
                        }

                    }

                    return Json(new
                    {
                        numberIncrease = numberIncrease,
                        numberIncreaseOptimize = numberIncreaseOptimize,
                        numberDecreaseOptimize = numberDecreaseOptimize,
                        numberDecrease = numberDecrease,
                        numberIncreaseRollback = numberIncreaseRollback,
                        numberDecreaseRollback = numberDecreaseRollback,
                        numberActiveCard = numberActiveCard,
                        numberDecreaseCancel = numberDecreaseCancel,
                        numberIncreaseCancel = numberIncreaseCancel,
                        revenueIncrease = revenueIncrease,
                        revenueDecrease = revenueDecrease,
                        revenueIncreaseRollback = revenueIncreaseRollback,
                        revenueDecreaseRollback = revenueDecreaseRollback,
                        revenueActiveCard = revenueActiveCard,
                        revenueIncreaseOptimize = revenueIncreaseOptimize,
                        revenueDecreaseOptimize = revenueDecreaseOptimize,
                        revenueDecreaseCancel = revenueDecreaseCancel,
                        revenueIncreaseCancel = revenueIncreaseCancel,
                    }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return Json(new { success = false, message = e.Message });
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
                    var account =  context.Accounts.Find(model.AccountId);
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

                return Json(new { success = false, message = e.Message});
            }
        }




        public class TransactionEnumViewModel
        {
            public dynamic Value { get; set; }
            public string Name { get; set; }
        }




    }
}