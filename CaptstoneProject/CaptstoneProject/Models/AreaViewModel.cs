using DataService.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CaptstoneProject.Models
{
    public class AreaViewModel
    {
        public class EditCourseSingleComponentModel
        {
            public int CourseId { get; set; }
            public string ComponentName { get; set; }
            public List<StudentComponent> StudentComponents { get; set; }
        }

        public class StudentComponent
        {
            public string StudentCode { get; set; }
            public string StudentName { get; set; }
            public double? ComponentMark { get; set; }
        }

        public class MarkComp
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public string Value { get; set; }
            public string Note { get; set; }
        }
        public class MarkPoint
        {
            public string Name { get; set; }
            public double Value { get; set; }
            public double Per { get; set; }
        }
        public class ComponentPercentage
        {
            public int Id { get; set; }
            public string CompName { get; set; }
            public double Per { get; set; }
        }

        public class StudentEditViewModel
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public string Code { get; set; }
            public int CourseId { get; set; }
            public string Class { get; set; }
            public List<string> ComponentNames { get; set; }
            public List<StudentCourseMark> MarksComponent { get; set; }
            public string Average { get; set; }
            public int SemesterId { get; set; }
            public int CourseStatus { get; set; }
            public string StudentInCourseStatus { get; set; }
        }

        public class CourseRecordViewModel
        {
            public int CourseId { get; set; }
            public string Name { get; set; }
            public string Code { get; set; }
            public string Class { get; set; }
            public DateTime? StartDate { get; set; }
            public DateTime? EndDate { get; set; }
            public string Status { get; set; }
        }

        public class StudentInCourseViewModel
        {
            public string UserName { get; set; }
            public string StudentName { get; set; }
            public string StudentCode { get; set; }
            public string Average { get; set; }
            public List<StudentCourseMark> MarksComponent { get; set; }
            public string StatusName { get; set; }
            public Nullable<int> Status { get; set; }
        }

        public class CourseDetailsViewModel
        {
            public List<StudentInCourseViewModel> StudentInCourse { get; set; }
            public List<string> ComponentNames { get; set; }
            public string SubName { get; set; }
            public string SubCode { get; set; }
            public string Semester { get; set; }
            public int CourseId { get; set; }
            public bool IsEditable { get; set; } // true: able to edit mark, false: vice versa
            public int IsPublish { get; set; }
            public string StatusName { get; set; }
            public List<int> FinalCol { get; set; }
            public bool ReadySubmit { get; set; }
        }




        public class JQueryDataTableParamModel
        {
            public string sEcho { get; set; }
            public string sSearch { get; set; }
            public int iDisplayLength { get; set; }
            public int iDisplayStart { get; set; }
            public int iColumns { get; set; }
            public int iSortingCols { get; set; }
            public string sColumns { get; set; }
        }

        public class RoleViewModel
        {
            public int Index { get; set; }
            public string Name { get; set; }
        }

        public class CourseRegisterInfoModel
        {
            public string SubjectCode { get; set; }
            public int PreferredSlot { get; set; }
            public DateTime RegisteredBy { get; set; }
            public int AssignedSlot { get; set; }
        }

        public class StudentRegisterModel
        {
            public string Name { get; set; }
            public string StudentCode { get; set; }
            public List<CourseRegisterInfoModel> RegisteredCourses { get; set; }
        }

        public class SubjectMarkViewModel
        {
            public int Index { get; set; }
            public int Id { get; set; }
            public string ComponentName { get; set; }
            public double Percentage { get; set; }
            public string EffectivenessDate { get; set; }
        }

        /////

        public class TransactionEditViewModel
        {
            public int Id { get; set; }
            public int AccountId { get; set; }
            public float Amount { get; set; }
            public DateTime Date { get; set; }
            public string Notes { get; set; }
            public bool IsIncreaseTransaction { get; set; }
            public int Status { get; set; }
            public string UserId { get; set; }
            public int TransactionType { get; set; }
        }

        public class StudentMajorViewModel
        {
            public int Id { get; set; }
            public string StudentName { get; set; }
            public string Email { get; set; }
            public string LoginName { get; set; }
            public Account Account { get; set; }
            public Nullable<int> TotalRegistered { get; set; }
            public Nullable<double> TotalMoneySpent { get; set; }
            public string StudentCode { get; set; }
        }

        public class StudentMajorModel
        {
            public int Id { get; set; }
            public String LoginName { get; set; }
            public Account Account { get; set; }
            public string StudentCode { get; set; }
        }

        public class SubjectMarkModel
        {
            public string SubjectComponentName { get; set; }
            public float Percentage { get; set; }
            public bool IsFinal { get; set; }
        }

        public class RegistrationViewModel
        {
            public Account StudentAccount { get; set; }
            public List<RegistrationDetailViewModel> CurriculumRegistrationDetails { get; set; }
            public List<RegistrationDetailViewModel> OtherRegistrationDetails { get; set; }
            public double CurriculumTotalPrice { get; set; }
            public double OtherTotalPrice { get; set; }
            public double TotalPrice { get; set; }
        }

        public class RegistrationDetailViewModel
        {
            public string SubjectCode { get; set; }
            public string SubjectName { get; set; }
            public int CreditValue { get; set; }
            public int RegisteredType { get; set; }
            public double UnitPrice { get; set; }
            public double TotalPrice { get; set; }
        }

        public class TeacherMail
        {
            public String EduMail { get; set; }
            public String FeMail { get; set; }
        }
    }
}