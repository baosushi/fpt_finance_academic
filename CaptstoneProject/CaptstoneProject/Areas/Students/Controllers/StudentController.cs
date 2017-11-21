using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using DataService.Model;
using CaptstoneProject.Controllers;
using static CaptstoneProject.Models.AreaViewModel;
using CaptstoneProject.Models;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.AspNet.Identity;

namespace CaptstoneProject.Areas.Students.Controllers
{
    [Authorize(Roles = "Student")]
    public class StudentController : MyBaseController
    {
        private DB_Finance_AcademicEntities db = new DB_Finance_AcademicEntities();
        private static object mutex = new object();

        // GET: Student
        public ActionResult Index()
        {
            try
            {
                var userManager = HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
                var userId = User.Identity.GetUserId();
                var user = userManager.FindById(userId);
                var loginName = user.Email.Split('@')[0];
                using (var context = new DB_Finance_AcademicEntities())
                {
                    var studentMajorId = context.StudentMajors.Where(q => q.LoginName.Equals(loginName)).Select(q => q.Id).FirstOrDefault();
                    var semesters = from course in context.Courses
                                    join studentInCourse in context.StudentInCourses on course.Id equals studentInCourse.CourseId
                                    join semester in context.Semesters on course.SemesterId equals semester.Id
                                    where studentInCourse.StudentId == studentMajorId // studentId of StudentInCourse is actually StudentMajorId
                                    select semester;
                    var semesterList = semesters.Select(q => new SelectListItem
                    {
                        Text = q.Title + q.Year,
                        Value = q.Id.ToString()
                    }).ToList();

                    ViewBag.semesterList = semesterList;
                    ViewBag.studentMajorId = studentMajorId;
                    return View("_Index");
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return Json(new { success = false, message = e.Message });
            }
        }

        public ActionResult GetRegistrationSubjects()
        {
            var loginName = (string)this.Session["loginName"];
            using (var context = new DB_Finance_AcademicEntities())
            {
                var semester = context.Semesters.Where(q => q.Status == (int)SememsterStatus.Registration).FirstOrDefault();
                if (semester != null)
                {
                    var studentMajor = context.StudentMajors.Where(q => q.LoginName == loginName).FirstOrDefault();
                    var availableSubjects = context.AvailableSubjects.Where(q => q.StudentMajorId == studentMajor.Id && q.Block.SemesterId == semester.Id);

                    var curriculumSubjects = availableSubjects.Where(q => q.IsInProgram.HasValue && q.IsInProgram.Value).Select(q => new
                    {
                        SubjectCode = q.Subject.SubjectCode,
                        SubjectName = q.Subject.SubjectName
                    }).ToList();
                    var relearnSubjects = availableSubjects.Where(q => q.IsRelearn.HasValue && q.IsRelearn.Value).Select(q => new
                    {
                        SubjectCode = q.Subject.SubjectCode,
                        SubjectName = q.Subject.SubjectName
                    }).ToList();
                    var otherSubjects = availableSubjects.Where(q => (!q.IsInProgram.HasValue || (q.IsInProgram.HasValue && !q.IsInProgram.Value)) && (!q.IsRelearn.HasValue || (q.IsRelearn.HasValue && !q.IsRelearn.Value))).Select(q => new
                    {
                        SubjectCode = q.Subject.SubjectCode,
                        SubjectName = q.Subject.SubjectName
                    }).ToList();

                    return Json(new { curriculumSubjects = curriculumSubjects, relearnSubjects = relearnSubjects, otherSubjects = otherSubjects });
                }
                else
                {
                    return Json(new { success = false, message = "Registration is not available at the moment." });
                }
            }
        }

        [HttpPost]
        public ActionResult SubmitRegistration(List<string> relearnList, bool curriculumSubject = false, bool relearnSubject = false)
        {
            var loginName = (string)this.Session["loginName"];

            try
            {
                using (var context = new DB_Finance_AcademicEntities())
                {
                    var semester = context.Semesters.Where(q => q.Status == (int)SememsterStatus.Registration).FirstOrDefault();
                    var studentMajor = context.StudentMajors.Where(q => q.LoginName == loginName).FirstOrDefault();
                    var availableSubjects = context.AvailableSubjects.Where(q => q.StudentMajorId == studentMajor.Id && q.Block.SemesterId == semester.Id);

                    var model = new RegistrationViewModel();
                    model.CurriculumRegistrationDetails = new List<RegistrationDetailViewModel>();
                    model.OtherRegistrationDetails = new List<RegistrationDetailViewModel>();

                    if (curriculumSubject)
                    {
                        var curriculumSubjects = availableSubjects.Where(q => q.IsInProgram.HasValue && q.IsInProgram.Value).Select(q => new RegistrationDetailViewModel
                        {
                            SubjectName = q.Subject.SubjectName,
                            SubjectCode = q.Subject.SubjectCode,
                            CreditValue = q.Subject.CreditValue.HasValue ? q.Subject.CreditValue.Value : 0,
                            RegisteredType = (int)RegistrationType.CurriculumSubject,
                            UnitPrice = 3000000,
                            TotalPrice = q.Subject.CreditValue.HasValue ? q.Subject.CreditValue.Value * 3000000 : 0
                        }).ToList();

                        model.CurriculumRegistrationDetails.InsertRange(model.CurriculumRegistrationDetails.Count, curriculumSubjects);
                        model.CurriculumTotalPrice = 25300000;
                    }
                    else
                    {
                        model.CurriculumTotalPrice = 0;
                    }

                    if (relearnSubject && relearnList != null && relearnList.Count > 0)
                    {
                        var relearnSubjects = availableSubjects.Where(q => relearnList.Contains(q.Subject.SubjectCode)).Select(q => new RegistrationDetailViewModel
                        {
                            SubjectCode = q.Subject.SubjectCode,
                            SubjectName = q.Subject.SubjectName,
                            CreditValue = q.Subject.CreditValue.HasValue ? q.Subject.CreditValue.Value : 0,
                            RegisteredType = (int)RegistrationType.RelearnSubject,
                            UnitPrice = 1500000,
                            TotalPrice = q.Subject.CreditValue.HasValue ? q.Subject.CreditValue.Value * 1500000 : 0
                        }).ToList();

                        model.OtherRegistrationDetails.InsertRange(model.OtherRegistrationDetails.Count, relearnSubjects);
                        model.OtherTotalPrice = model.OtherRegistrationDetails.Sum(q => q.TotalPrice);
                    }
                    else
                    {
                        model.OtherTotalPrice = 0;
                    }

                    //OTHER Subject todo


                    model.StudentAccount = studentMajor.Accounts.FirstOrDefault();
                    model.TotalPrice = model.CurriculumTotalPrice + model.OtherTotalPrice;

                    this.Session["cart"] = model;

                    return View("Payment", model);
                }
            }
            catch (Exception e)
            {
                return View("_Index");
            }
        }

        [HttpPost]
        public async Task<ActionResult> SubmitPayment()
        {
            var loginName = (string)this.Session["loginName"];

            try
            {
                var cart = (RegistrationViewModel)this.Session["cart"];

                using (var context = new DB_Finance_AcademicEntities())
                {
                    var studentAccount = context.Accounts.Find(cart.StudentAccount.Id);
                    if (studentAccount == null || studentAccount.StudentMajor.LoginName != loginName)
                    {
                        return Json(new { success = false, errorType = (int)JsonResultErrorType.Unauthorized, message = JsonResultErrorType.Unauthorized.GetEnumDisplayName() });
                    }
                    else if (cart.TotalPrice > studentAccount.Balance)
                    {
                        return Json(new { success = false, errorType = (int)JsonResultErrorType.Failed, message = "Your balance is not enough for this transaction." });
                    }
                    else
                    {
                        var user = context.AspNetUsers.Where(q => q.UserName == loginName || (q.Email != null && q.Email.Contains(loginName))).FirstOrDefault();

                        if (user != null && user.Email != null)
                        {
                            var recipient = user.Email;

                            var senderEmail = System.Web.Configuration.WebConfigurationManager.AppSettings["DefaultEmail"];
                            var senderPassword = System.Web.Configuration.WebConfigurationManager.AppSettings["DefaultPassword"];

                            var pool = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
                            var random = new Random();
                            var code = new string(Enumerable.Range(0, 6).Select(x => pool[random.Next(0, pool.Length)]).ToArray());

                            this.Session["code"] = code;
                            this.Session["attempt"] = 0;
                            this.Session["startTime"] = DateTime.Now;

                            var message = $"Dear {user.FullName},<br><br>We've received your course registration payment request. Please use this confirmation code to complete your transaction (case-sensitive).<br><h2 style=\"color: blue;\">Confirmation code: {code}</h2><br><b>Please complete this verification steps in 5 minutes.</b><br><br>Thank you,<br>FPT University Dummy";

                            await Utils.SendEmail(senderEmail, recipient, senderPassword, message);

                            return Json(new { success = true });
                        }
                        else
                        {
                            return Json(new { success = false, errorType = (int)JsonResultErrorType.Failed, message = "Invalid account." });
                        }
                    }
                }
            }
            catch (Exception e)
            {
                return Json(new { success = false, errorType = (int)JsonResultErrorType.Exception, message = e.Message });
            }
        }

        [HttpPost]
        public ActionResult ConfirmPayment(string code)
        {
            lock (mutex)
            {
                var correctCode = (string)this.Session["code"];
                int attempt = (int)this.Session["attempt"];
                var loginName = (string)this.Session["loginName"];
                var startTime = (DateTime)this.Session["startTime"];

                if (correctCode == code && attempt < 5 && (DateTime.Now - startTime) <= (new TimeSpan(0, 5, 0)))
                {
                    try
                    {
                        var cart = (RegistrationViewModel)this.Session["cart"];
                        using (var context = new DB_Finance_AcademicEntities())
                        {
                            var studentAccount = context.Accounts.Find(cart.StudentAccount.Id);
                            if (studentAccount == null || studentAccount.StudentMajor.LoginName != loginName)
                            {
                                return Json(new { success = false, errorType = (int)JsonResultErrorType.Unauthorized, message = JsonResultErrorType.Unauthorized.GetEnumDisplayName() });
                            }
                            else if (cart.TotalPrice > studentAccount.Balance)
                            {
                                return Json(new { success = false, errorType = (int)JsonResultErrorType.Failed, message = "Your balance is not enough for this transaction." });
                            }
                            else
                            {
                                var semester = context.Semesters.Where(q => q.Status == (int)SememsterStatus.Registration).FirstOrDefault();
                                studentAccount.Balance -= cart.TotalPrice;

                                context.Transactions.Add(new Transaction
                                {
                                    AccountId = studentAccount.Id,
                                    Amount = cart.TotalPrice,
                                    Date = DateTime.Now,
                                    IsIncreaseTransaction = false,
                                    TransactionType = (int)TransactionTypeEnum.TuitionPayment,
                                    Status = (int)TransactionStatus.Approve
                                });

                                var registration = new Registration()
                                {
                                    RegisteredBy = DateTime.Now,
                                    StudentMajorId = studentAccount.StudentMajorId.Value,
                                    FinalAmount = cart.TotalPrice,
                                    RegistrationDetailTotalQuantity = cart.CurriculumRegistrationDetails.Count + cart.OtherRegistrationDetails.Count
                                };

                                context.Registrations.Add(registration);

                                context.SaveChanges();

                                foreach (var detail in cart.CurriculumRegistrationDetails.Concat(cart.OtherRegistrationDetails))
                                {
                                    var subject = context.Subjects.Where(q => q.SubjectCode == detail.SubjectCode).FirstOrDefault();
                                    var abstractCourse = context.Courses.Where(q => q.ClassName == null && q.SemesterId == semester.Id && q.SubjectId == subject.Id).FirstOrDefault();

                                    if (abstractCourse == null)
                                    {
                                        abstractCourse = new Course()
                                        {
                                            SubjectId = subject.Id,
                                            SemesterId = semester.Id
                                        };

                                        context.Courses.Add(abstractCourse);
                                        context.SaveChanges();
                                    }

                                    context.RegistrationDetails.Add(new RegistrationDetail()
                                    {
                                        RegistrationId = registration.Id,
                                        CourseId = abstractCourse.Id,
                                    });
                                }

                                context.SaveChanges();

                                return Json(new { success = true, message = "Transaction is being processed." });
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        return Json(new { success = false, errorType = (int)JsonResultErrorType.Exception, message = e.Message });
                    }
                }
                else if ((DateTime.Now - startTime) > (new TimeSpan(0, 5, 0)))
                {
                    return Json(new { success = false, errorType = (int)JsonResultErrorType.Expired, message = "Invalid attempt. Verification code has expired." });
                }
                else
                {
                    this.Session["attempt"] = ++attempt;

                    return Json(new { success = false, errorType = (int)JsonResultErrorType.Failed, message = "Invalid attempt. " + (5 - attempt) + "/5 attempt(s) left." });
                }
            }
        }

        // GET: Student/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            StudentInCourse studentInCourse = db.StudentInCourses.Find(id);
            if (studentInCourse == null)
            {
                return HttpNotFound();
            }
            return View(studentInCourse);
        }

        // GET: Student/Create
        public ActionResult Create()
        {
            ViewBag.CourseId = new SelectList(db.Courses, "Id", "Id");
            ViewBag.StudentId = new SelectList(db.Students, "Id", "StudentCode");
            return View();
        }

        // POST: Student/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,CourseId,StudentId,Average")] StudentInCourse studentInCourse)
        {
            if (ModelState.IsValid)
            {
                db.StudentInCourses.Add(studentInCourse);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.CourseId = new SelectList(db.Courses, "Id", "Id", studentInCourse.CourseId);
            ViewBag.StudentId = new SelectList(db.Students, "Id", "StudentCode", studentInCourse.StudentId);
            return View(studentInCourse);
        }

        // GET: Student/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            StudentInCourse studentInCourse = db.StudentInCourses.Find(id);
            if (studentInCourse == null)
            {
                return HttpNotFound();
            }
            ViewBag.CourseId = new SelectList(db.Courses, "Id", "Id", studentInCourse.CourseId);
            ViewBag.StudentId = new SelectList(db.Students, "Id", "StudentCode", studentInCourse.StudentId);
            return View(studentInCourse);
        }

        // POST: Student/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,CourseId,StudentId,Average")] StudentInCourse studentInCourse)
        {
            if (ModelState.IsValid)
            {
                db.Entry(studentInCourse).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.CourseId = new SelectList(db.Courses, "Id", "Id", studentInCourse.CourseId);
            ViewBag.StudentId = new SelectList(db.Students, "Id", "StudentCode", studentInCourse.StudentId);
            return View(studentInCourse);
        }

        // GET: Student/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            StudentInCourse studentInCourse = db.StudentInCourses.Find(id);
            if (studentInCourse == null)
            {
                return HttpNotFound();
            }
            return View(studentInCourse);
        }

        // POST: Student/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            StudentInCourse studentInCourse = db.StudentInCourses.Find(id);
            db.StudentInCourses.Remove(studentInCourse);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
        public JsonResult GetStudentsList(jQueryDataTableParamModel param)
        {

            int count = 0;

            List<StudentInCourse> studentInCourses = null;

            if (!string.IsNullOrEmpty(param.sSearch))
            {
                studentInCourses = db.StudentInCourses.Include(s => s.Course).Include(s => s.StudentMajor).Where(s => s.StudentMajor.StudentCode.Contains(param.sSearch)).ToList();
            }
            else
            {
                studentInCourses = db.StudentInCourses.Include(s => s.Course).Include(s => s.StudentMajor).ToList();
            }

            int totalRecords = studentInCourses.Count();

            count = param.iDisplayStart + 1;


            var rs = studentInCourses
                    //searchResult
                    .OrderBy(a => a.StudentMajor.StudentCode)
                    .Skip(param.iDisplayStart)
                    .Take(param.iDisplayLength)
                    .Select(a => new IConvertible[]
                        {
                        //count++,
                        a.StudentMajor.StudentCode,
                        a.StudentId,
                        a.Course.SubjectId,
                        a.CourseId,
                        a.Average,
                        });


            int totalDisplay = studentInCourses.Count();

            return Json(new
            {
                sEcho = param.sEcho,
                iTotalRecords = totalRecords,
                iTotalDisplayRecords = totalDisplay,
                aaData = rs
            }, JsonRequestBehavior.AllowGet);

        }

        public ActionResult GetHistoryCourseforStudent(JQueryDataTableParamModel param, int studentMajorId, int semesterId)
        {
            try
            {
                using (var context = new DB_Finance_AcademicEntities())
                {

                    var joinResult = from course in context.Courses
                                     join studentInCourse in context.StudentInCourses on course.Id equals studentInCourse.CourseId
                                     join semester in context.Semesters on course.SemesterId equals semester.Id
                                     where (semesterId != -1 ? semester.Id == semesterId : true) //get by semester or all
                                     && studentInCourse.StudentId == studentMajorId //studentId of studentInCourse is actually StudentMajorId
                                     //&& (course.Status != null ? course.Status.Value : 0) == (int)CourseStatus.Closed
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
                            (q.StudentInCourse.Average != -1? q.StudentInCourse.Average.Value.ToString() : "-"),
                            Enum.GetName(typeof(StudentInCourseStatus), (q.StudentInCourse.Status.Value)),
                            q.StudentInCourse.Id
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

        public ActionResult GetMarkComponen4StudentbySubject(int studentInCourseId)
        {
            try
            {

                using (var context = new DB_Finance_AcademicEntities())
                {
                    var result = context.StudentCourseMarks.Where(q => q.StudentInCourseId == studentInCourseId).AsEnumerable()
                         .Select(q => new IConvertible[] {
                            q.CourseMark.ComponentName,
                            (q.Mark != -1? q.Mark.Value.ToString() : "-")
                         }).ToList();

                    return Json(new
                    {
                        success = true,
                        iTotalRecords = result.Count(),
                        iTotalDisplayRecords = result.Count(),
                        aaData = result
                    }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception e)
            {
                return Json(new { success = false, message = e.Message });
                throw;
            }
        }

    }
}
