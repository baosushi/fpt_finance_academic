using DataService.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CaptstoneProject.Models
{
    public class AreaViewModel
    {

        public class MarkComp
        {
            public string Name { get; set; }
            public string Value { get; set; }
            public int Id { get; set; }
        }
        public class PerComp
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
            public string Status { get; set; }
        }

        public class CourseDetailsViewModel
        {
            public List<StudentInCourseViewModel> StudentInCourse { get; set; }
            public List<string> ComponentNames { get; set; }
            public string SubName { get; set; }
            public string SubCode { get; set; }
            public string Semester { get; set; }
            public int CourseId { get; set; }
            public bool isEditable { get; set; } // true: able to edit mark, false: vice versa

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
    }
}