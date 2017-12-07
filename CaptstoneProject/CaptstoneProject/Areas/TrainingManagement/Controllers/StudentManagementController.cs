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
using System.Transactions;
using OfficeOpenXml;
using System.Threading;
using System.Threading.Tasks;

namespace CaptstoneProject.Areas.TrainingManagement.Controllers
{
    public class StudentManagementController : MyBaseController
    {
        static public double excelRowCompleted = 0;
        static public double excelTotalRow = 0;
        // GET: TrainingManagement/StudentManagement
        public ActionResult Index()
        {
            try
            {
                using (var context = new DB_Finance_AcademicEntities())
                {
                    var blockList = context.Blocks.Where(q => q.Semester.Status != (int)SemesterStatus.Closed).Select(q => new SelectListItem
                    {
                        Value = q.Id.ToString(),
                        Text = q.Semester.Title + q.Semester.Year + " - " + q.Name
                    }).ToList();
                    ViewBag.BlockList = blockList;
                }
            }
            catch (Exception e)
            {
                return Json(new { success = false, message = e.Message });
            }
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
                        a.Id ,
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
                    var studentMajor = context.StudentMajors.Find(id);
                    if (studentMajor == null)
                    {
                        return HttpNotFound();
                    }
                    StudentMajorViewModel model = new StudentMajorViewModel();
                    model.Id = studentMajor.Id;
                    model.StudentName = studentMajor.Student.Name;


                    model.StudentCode = studentMajor.StudentCode;

                    var userManager = HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
                    var currentLoginAccount = userManager.Users.Where(q => q.Email.Contains(studentMajor.LoginName)).FirstOrDefault();
                    if (currentLoginAccount != null)
                        model.Email = currentLoginAccount.Email;

                    model.Account = context.Accounts.Where(q => q.StudentMajorId == studentMajor.Id).FirstOrDefault();

                    //model.TotalRegistered = (from reg in context.Registrations
                    //                         join studentmajor in context.StudentMajors on reg.StudentMajorId equals studentmajor.Id
                    //                         join regDetail in context.RegistrationDetails on reg.Id equals regDetail.RegistrationId
                    //                         where studentmajor.StudentId == student.Id
                    //                         select regDetail).Count();

                    //model.TotalMoneySpent = (from reg in context.Registrations
                    //                         join studentmajor in context.StudentMajors
                    //                         on reg.StudentMajorId equals studentmajor.Id
                    //                         where studentmajor.Id == student.Id
                    //                         select reg.FinalAmount).Sum();

                    var semesters = from course in context.Courses
                                    join studentInCourse in context.StudentInCourses on course.Id equals studentInCourse.CourseId
                                    join semester in context.Semesters on course.SemesterId equals semester.Id
                                    where studentInCourse.StudentId == studentMajor.Id // studentId of StudentInCourse is actually StudentMajorId
                                    select semester;
                    var semesterList = semesters.Select(q => new SelectListItem
                    {
                        Text = q.Title + q.Year,
                        Value = q.Id.ToString()
                    }).ToList();


                    ViewBag.semesterList = semesterList;
                    return View(model);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return HttpNotFound();
            }
        }


        public ActionResult LoadTransaction(JQueryDataTableParamModel param, int studentMajorId, string startTime, string endTime, int transactionStatus, int transactionType)
        {
            try
            {
                using (var context = new DB_Finance_AcademicEntities())
                {
                    var startDate = startTime.ToDateTime().GetStartOfDate();
                    var endDate = endTime.ToDateTime().GetEndOfDate();
                    var listTransaction = context.Transactions.Where(q => q.Date >= startDate
                    && q.Date <= endDate && q.Account.StudentMajorId == studentMajorId);

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
                            transactionForm = (int)TransactionForm.Increase;
                            break;
                        case (int)TransactionTypeEnum.AdjustIncrease:
                            transactionForm = (int)TransactionForm.Increase;
                            break;
                        case (int)TransactionTypeEnum.AdjustDecrease:
                            transactionForm = (int)TransactionForm.Decrease;
                            break;
                        default:
                            break;
                    }

                    // -1 : get all
                    if (transactionStatus != -1)
                    {
                        listTransaction = listTransaction.Where(a => a.Status == transactionStatus);
                    }

                    if (transactionForm != -1)
                    {
                        listTransaction = listTransaction.Where(a => a.IsIncreaseTransaction == (transactionForm == (int)TransactionForm.Increase));
                    }

                    if (transactionType != -1)
                    {
                        listTransaction = listTransaction.Where(a => a.TransactionType == transactionType);
                    }

                    int count = 0;
                    count = param.iDisplayStart + 1;

                    listTransaction.Where(q => string.IsNullOrEmpty(param.sSearch)
                    || q.Account.StudentMajor.StudentCode.ToUpper().Contains(param.sSearch.Trim().ToUpper()));

                    var result = listTransaction
                        .OrderByDescending(q => q.Date)
                        .Skip(param.iDisplayStart)
                        .Take(param.iDisplayLength)
                        .AsEnumerable();
                    var list = result.Select(a => new IConvertible[]
                            {
                        count++, // 0
                        a.Account.StudentMajor.StudentCode,//1
                        a.Amount, // 2
                        a.Date.Value.ToString("dd/MM/yyyy"), // 3
                        a.Status, //4
                        a.TransactionType, //5
                        a.UserName == null? "-": a.UserName, // 6 User who created this transaction
                        a.Id, //7 TransactionId
                        a.IsIncreaseTransaction //8
                            }).ToList();
                    var totalRecords = listTransaction.Count();

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


        public ActionResult CreateAllStudentAccount()
        {
            try
            {
                using (var transactionScope = new TransactionScope())
                {

                    DB_Finance_AcademicEntities context;
                    using (context = new DB_Finance_AcademicEntities())
                    {
                        var studentMajorList = context.StudentMajors
                            .Select(q => new StudentMajorModel
                            {
                                Id = q.Id,
                                Account = q.Accounts.FirstOrDefault(),
                                LoginName = q.LoginName,
                                StudentCode = q.StudentCode
                            }).ToList();

                        //only create account for student dont have account yet
                        var count = 0;
                        foreach (var studentMajor in studentMajorList)
                        {
                            if (studentMajor.Account == null)
                            {
                                ++count;
                                var account = new Account
                                {
                                    Name = "account_" + studentMajor.LoginName,
                                    StartDate = DateTime.Now,
                                    Type = (int)AccountType.Normal,
                                    Balance = 0,
                                    StudentMajorId = studentMajor.Id,
                                    Active = true
                                };
                                context = context.BulkInsert(account, count, 100);
                            }
                        }
                        context.SaveChanges();

                    }
                    transactionScope.Complete();
                    return Json(new { success = true, message = "Create accounts successed!" });
                }
            }
            catch (Exception e)
            {
                Response.StatusCode = (int)HttpStatusCode.BadRequest;
                return Json(new { success = false, message = e.Message });
            }
        }


        public ActionResult GetHistoryCourseforStudent(JQueryDataTableParamModel param, int studentMajorId, int? semesterId = -1)
        {
            try
            {
                using (var context = new DB_Finance_AcademicEntities())
                {

                    var joinResult = from course in context.Courses
                                     join studentInCourse in context.StudentInCourses on course.Id equals studentInCourse.CourseId
                                     join semester in context.Semesters on course.SemesterId equals semester.Id
                                     where semester.Id == semesterId
                                     && studentInCourse.StudentId == studentMajorId //studentId of studentInCourse is actually StudentMajorId
                                     && (course.Status != null ? course.Status.Value : 0) == (int)CourseStatus.Closed
                                     select new { Semester = semester, Course = course, StudentInCourse = studentInCourse };

                    var courseHistoryList = joinResult.Where(q => string.IsNullOrEmpty(param.sSearch)
                    || q.Course.Subject.SubjectCode.ToUpper().Contains(param.sSearch.Trim().ToUpper())
                    || q.Course.Subject.SubjectName.ToUpper().Contains(param.sSearch.Trim().ToUpper())
                    || q.Course.CourseName.ToUpper().Contains(param.sSearch.Trim().ToUpper()));

                    var count = 0;
                    count = param.iDisplayStart + 1;
                    var result = courseHistoryList
                        //.OrderByDescending(q => q.Course.StartDate) //Date still null , uncomment this code when Date available
                        .OrderByDescending(q => q.Course.Id) //temporary fix cause Date still null
                        .Skip(param.iDisplayStart).Take(param.iDisplayLength).AsEnumerable()
                        .Select(q => new IConvertible[]
                        {
                            count++,
                            q.Course.CourseName,
                            q.Course.Subject.SubjectCode,
                            q.Course.Subject.SubjectName,
                            //(q.Course.StartDate != null? q.Course.StartDate.Value.ToString("dd/MM/yyyy"): "-")
                            //    + " - " +
                            //    (q.Course.EndDate != null? q.Course.EndDate.Value.ToString("dd/MM/yyyy"): "-"),

                            q.Semester.Title + q.Semester.Year,
                            Enum.GetName(typeof(StudentInCourseStatus), (q.StudentInCourse.Status.Value))
                        }).ToList();

                    var totalRecords = courseHistoryList.Count();
                    var totalDisplay = result.Count();
                    return Json(new
                    {
                        success = true,
                        sEcho = param.sEcho,
                        iTotalRecords = totalRecords,
                        iTotalDisplayRecords = totalDisplay,
                        aaData = result
                    }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception e)
            {
                Response.StatusCode = (int)HttpStatusCode.BadRequest;
                return Json(new { success = false, message = e.Message });
            }
        }

        public ActionResult GetAvailableSubjectforStudent(JQueryDataTableParamModel param, int studentMajorId = -1)
        {
            try
            {
                if (studentMajorId == -1)
                {
                    return Json(new { success = false, message = "Error! Student not found" });
                }
                using (var context = new DB_Finance_AcademicEntities())
                {
                    var count = 0;
                    count = param.iDisplayStart + 1;

                    var currentDate = DateTime.Now;
                    var currentSemester = context.Semesters.Where(q => q.StartDate <= currentDate && q.EndDate >= currentDate).FirstOrDefault();
                    var nextSemester = context.Semesters.OrderBy(q => q.Id).Where(q => q.Id > currentSemester.Id).FirstOrDefault();
                    var result = context.AvailableSubjects.Where(q => q.StudentMajorId == studentMajorId && q.Block.SemesterId == nextSemester.Id).AsEnumerable()
                        .Select(q => new IConvertible[]{
                            count++,
                            q.Subject.SubjectCode,
                            q.Subject.SubjectName,
                            "-" // get IsInProgram or IsRelearn to declare status

                        }).ToList();

                    return Json(new
                    {
                        success = true,
                        sEcho = param.sEcho,
                        iTotalRecords = result.Count(),
                        iTotalDisplayRecords = result.Count(),
                        aaData = result
                    }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception e)
            {
                Response.StatusCode = (int)HttpStatusCode.BadRequest;
                return Json(new { success = false, message = e.Message });
            }
        }

        [HttpPost]
        public ActionResult ImportAvailableSubjectForRegister(int blockId)
        {
            try
            {
                if (Request.Files.Count < 0)
                {
                    Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    return Json(new { success = false, message = "No file found!" });
                }
                var fileContent = Request.Files[0];
                if (fileContent != null && fileContent.ContentLength > 0)
                {
                    var stream = fileContent.InputStream;
                    DB_Finance_AcademicEntities context;
                    using (var excelPackage = new ExcelPackage(stream))
                    {
                        var ws = excelPackage.Workbook.Worksheets.FirstOrDefault();
                        var totalCol = ws.Dimension.Columns;
                        var totalRow = ws.Dimension.Rows;

                        excelTotalRow = totalRow;
                        excelRowCompleted = 0;

                        var studentCodeCell = (from cell in ws.Cells
                                               where cell.Value.ToString().Trim().ToUpper()
                                               .Equals("MSSV")
                                               select cell).FirstOrDefault();

                        var failedSubjectCell = (from cell in ws.Cells
                                                 where cell.Value.ToString().Trim().ToUpper()
                                                 .Equals("MÔN ĐANG NỢ")
                                                 select cell).FirstOrDefault();
                        var nextSubjectCell = (from cell in ws.Cells
                                               where cell.Value.ToString().Trim().ToUpper()
                                               .Equals("MÔN TIẾP THEO")
                                               select cell).FirstOrDefault();

                        var slowProgressSubjectCell = (from cell in ws.Cells
                                                       where cell.Value.ToString().Trim().ToUpper()
                                                       .Equals("DANH SÁCH MÔN CHẬM TIẾN ĐỘ")
                                                       select cell).FirstOrDefault();



                        var headerRow = studentCodeCell.Start.Row; //Headers are all in the same row

                        var studentCodeCol = studentCodeCell.Start.Column;

                        var failedSubjectCol = failedSubjectCell.Start.Column;

                        var nextSubjectCol = nextSubjectCell.Start.Column;

                        var slowProgressSubjectCol = slowProgressSubjectCell.Start.Column;

                        using (context = new DB_Finance_AcademicEntities())
                        {
                            var count = 0;
                            //row from EPPLUS start from 1
                            for (int i = headerRow + 1; i <= totalRow; i++)
                            {
                                var studentCode = ws.Cells[i, studentCodeCol].Value.ToString().Trim().ToUpper();
                                var failedSubjects = ws.Cells[i, failedSubjectCol].Value.ToString().Trim();
                                var nextSubjects = ws.Cells[i, nextSubjectCol].Value.ToString().Trim();
                                var slowProgressSubjects = ws.Cells[i, slowProgressSubjectCol].Value.ToString().Trim();

                                if (failedSubjects[failedSubjects.Length - 1].Equals(','))
                                {
                                    failedSubjects = failedSubjects.Substring(0, failedSubjects.Length - 1);
                                }
                                if (nextSubjects[nextSubjects.Length - 1].Equals(','))
                                {
                                    nextSubjects = nextSubjects.Substring(0, nextSubjects.Length - 1);
                                }
                                if (slowProgressSubjects[slowProgressSubjects.Length - 1].Equals(','))
                                {
                                    slowProgressSubjects = slowProgressSubjects.Substring(0, slowProgressSubjects.Length - 1);
                                }

                                string[] failedSubjectList = new string[0];
                                string[] nextSubjectList = new string[0];
                                string[] slowProgressSubjectList = new string[0];

                                if (!failedSubjects.Contains("N/A"))
                                {
                                    failedSubjectList = failedSubjects.Split(',');
                                }
                                if (!nextSubjects.Contains("N/A"))
                                {
                                    nextSubjectList = nextSubjects.Split(',');
                                }
                                if (!slowProgressSubjects.Contains("N/A"))
                                {
                                    slowProgressSubjectList = slowProgressSubjects.Split(',');
                                }


                                var studentMajor = context.StudentMajors
                                    .Where(q => q.StudentCode.ToUpper().Equals(studentCode)).FirstOrDefault();


                                if (studentMajor != null)
                                {
                                    var oldAvailableSubjectList = context.AvailableSubjects.Where(q => q.BlockId == blockId
                                    && q.StudentMajorId == studentMajor.Id)
                                    .ToList();

                                    if (oldAvailableSubjectList.Count > 0)
                                    {
                                        context.AvailableSubjects.RemoveRange(oldAvailableSubjectList);
                                        context.SaveChanges();
                                    }

                                    //môn nợ
                                    foreach (var failedSubject in failedSubjectList)
                                    {
                                        var subject = context.Subjects
                                            .Where(q => q.SubjectCode.ToUpper().Equals(failedSubject.Trim().ToUpper())).FirstOrDefault();



                                        if (subject != null)
                                        {
                                            ++count;
                                            AvailableSubject availableSubject = new AvailableSubject()
                                            {

                                                IsRelearn = true,
                                                StudentMajorId = studentMajor.Id,
                                                SubjectId = subject.Id,
                                                BlockId = blockId
                                            };
                                            context.BulkInsert(availableSubject, count, 100);
                                        }
                                    }

                                    //môn tiếp theo trong học kì tới
                                    foreach (var nextSubject in nextSubjectList)
                                    {
                                        var subject = context.Subjects
                                            .Where(q => q.SubjectCode.ToUpper().Equals(nextSubject.Trim().ToUpper())).FirstOrDefault();
                                        if (subject != null)
                                        {
                                            AvailableSubject availableSubject = new AvailableSubject()
                                            {

                                                IsInProgram = true,
                                                StudentMajorId = studentMajor.Id,
                                                SubjectId = subject.Id,
                                                BlockId = blockId
                                            };
                                            context.BulkInsert(availableSubject, count, 100);
                                        }
                                    }

                                    //môn chậm tiến độ
                                    foreach (var slowProgressSubject in slowProgressSubjectList)
                                    {
                                        var subject = context.Subjects
                                            .Where(q => q.SubjectCode.ToUpper().Equals(slowProgressSubject.Trim().ToUpper())).FirstOrDefault();
                                        if (subject != null)
                                        {
                                            AvailableSubject availableSubject = new AvailableSubject()
                                            {

                                                IsSlowProgress = true,
                                                StudentMajorId = studentMajor.Id,
                                                SubjectId = subject.Id,
                                                BlockId = blockId
                                            };
                                            context.BulkInsert(availableSubject, count, 100);
                                        }
                                    }
                                }//end of if

                                ++excelRowCompleted;
                            } //end of for()
                            context.SaveChanges();
                        }// end of using context

                    }
                }
                else
                {
                    return Json(new { success = false, message = "Error! Empty file" });
                }


                return Json(new { success = true, message = "Importing Available Subject Done" });
            }
            catch (Exception e)
            {
                Response.StatusCode = (int)HttpStatusCode.BadRequest;
                return Json(new { success = false, message = e.Message });
            }

        }

        [HttpPost]
        public ActionResult TestPercent(int blockId)
        {

            //Session["excelTotalRow"] = 20;
            excelTotalRow = 20;
            excelRowCompleted = 0;
            for (int i = 0; i < 20; i++)
            {
                //Session["excelRowCompleted"] = ++excelRowCompleted;
                ++excelRowCompleted;
                Thread.Sleep(1000);
            }
            return Json(new { success = true, message = "Done" });
        }



        public ActionResult GetPercentageOfImportingAvailableSubject()
        {
            try
            {
                double mypercent = 0;


                if (excelRowCompleted > 0)
                {
                    mypercent = Math.Round(excelRowCompleted / excelTotalRow * 100, 1);

                }
                if (excelRowCompleted == excelTotalRow)
                {
                    excelRowCompleted = 0;
                }

                return Json(new { success = true, percent = mypercent });
            }
            catch (Exception e)
            {
                Response.StatusCode = (int)HttpStatusCode.BadRequest;
                return Json(new { success = false, message = e.Message });
            }
        }

    }
}