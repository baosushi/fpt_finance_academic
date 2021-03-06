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
    
    public partial class AvailableSubject
    {
        public int Id { get; set; }
        public Nullable<bool> IsInProgram { get; set; }
        public Nullable<bool> IsSlowProgress { get; set; }
        public Nullable<bool> IsRelearn { get; set; }
        public int StudentMajorId { get; set; }
        public int SubjectId { get; set; }
        public int BlockId { get; set; }
        public Nullable<bool> IsViolatingRegulation { get; set; }
        public string Document { get; set; }
    
        public virtual Block Block { get; set; }
        public virtual StudentMajor StudentMajor { get; set; }
        public virtual Subject Subject { get; set; }
    }
}
