//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace DataService.Model
{
    using System;
    using System.Collections.Generic;
    
    public partial class StudentMajor
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public StudentMajor()
        {
            this.StudentInCourses = new HashSet<StudentInCourse>();
        }
    
        public int Id { get; set; }
        public string StudentCode { get; set; }
        public string LoginName { get; set; }
        public Nullable<int> StudentId { get; set; }
        public Nullable<int> OldStudentId { get; set; }
    
        public virtual Student Student { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<StudentInCourse> StudentInCourses { get; set; }
    }
}