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

    public enum StudentInCourseStatus
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
        [Display(Name = "Issued")]
        Issued = 7,
        [Display(Name = "Cancel")]
        Cancel = -1,

    }
    public enum CourseStatus
    {
        [Display(Name = "New")] //gray
        New = 0,
        [Display(Name = "In Progress")]//green
        InProgress = 1,
        [Display(Name = "Submitted")] //blue
        Submitted = 2,
        [Display(Name = "Publishable")] //yellow
        FirstPublish = 3,
        [Display(Name = "Final Publish")] //orange
        FinalPublish = 4,
        [Display(Name = "Closed")] //red
        Closed = 5,
        [Display(Name = "Cancel")] //black
        Cancel = -1,
    }
    public enum FinalEditStatus
    {
        [Display(Name = "Edit Final")]
        EditFinal = 0,
        [Display(Name = "Edit Retake")]
        EditRetake = 1,
        [Display(Name = "Submit Component")]
        SubmitComponent = 2,
        [Display(Name = "No Edit")]
        NoEdit = 3,
    }

}