using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.Entity;
using DataService.Model;

namespace CaptstoneProject.Areas.Students.Models
{
    public class StudentInCourse
    {
        public int ID { get; set; }
        public string StudentID { get; set; }
        public string Name { get; set; }
        public int CourseID { get; set; }
        public int SubjectID { get; set; }
        public int Average { get; set; }
        

    }
    public class MarkDBContext : DB_Finance_AcademicEntities
    {
        public DbSet<StudentInCourse> StudentsInCourse { get; set; }
    }
}