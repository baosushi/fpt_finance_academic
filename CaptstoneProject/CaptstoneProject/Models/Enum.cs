using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace CaptstoneProject.Models
{
    public enum SememsterStatus
    {
        [Display(Name = "Open")]
        Open = 0,
        [Display(Name = "Pending")]
        Pending = 1,
        [Display(Name = "Closed")]
        Closed = 2,
    }

    public enum StudentCourseStatus
    {
        [Display(Name = "Register")]
        Register = 0,
        [Display(Name = "Studying")]
        Studying = 1,
        [Display(Name = "Passed")]
        Passed = 2,
        [Display(Name = "Failed")]
        Failed = 3,
    }
    public enum CourseStatus
    {
        [Display(Name = "Open")]
        Open = 0,
        [Display(Name ="LockTeacher")]
        LockTeacher= 1,
        [Display(Name = "LockTM")] //Lock Training Mangement
        LockTM = 2,

    }

}