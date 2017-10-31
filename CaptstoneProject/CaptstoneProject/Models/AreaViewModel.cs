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
            public DateTime StartDate { get; set; }
            public DateTime EndDate { get; set; }
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

            public class CourseRecordViewModel
            {
                public int CourseId { get; set; }
                public string Name { get; set; }
                public string Code { get; set; }
                public string Class { get; set; }
                public DateTime StartDate { get; set; }
                public DateTime EndDate { get; set; }
            }
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
    }
}