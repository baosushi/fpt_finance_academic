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
        [Display(Name = "Register")] // = new
        Register = 0,
        [Display(Name = "Studying")]  // = in progress
        Studying = 1,
        [Display(Name = "Submitted")]
        Submitted = 2,
        [Display(Name = "Publishable")]
        FirstPublish = 3,
        [Display(Name = "Final Publish")]
        FinalPublish = 4,
        [Display(Name = "Passed")]
        Passed = 5,
        [Display(Name = "Failed")]
        Failed = 6,
        [Display(Name = "Cancel")]
        Cancel = -1,
    }
    public enum CourseStatus
    {
        [Display(Name = "New")]
        New = 0,
        [Display(Name = "In Progress")]
        InProgress = 1,
        [Display(Name = "Submitted")]
        Submitted = 2,
        [Display(Name = "Publishable")]
        FirstPublish = 3,
        [Display(Name = "Final Publish")]
        FinalPublish = 4,
        [Display(Name = "Closed")]
        Closed = 5,
        [Display(Name = "Cancel")]
        Cancel = -1,
    }
    public enum FinalEditStatus
    {
        [Display(Name = "Edit Final")]
        EditFinal = 0,
        [Display(Name = "Edit Retake")]
        EditRetake = 1,
        [Display(Name = "No Edit")]
        NoEdit = 2,
    }

}