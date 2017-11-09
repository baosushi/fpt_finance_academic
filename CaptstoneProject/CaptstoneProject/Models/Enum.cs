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

    public enum BlockStatus
    {
        [Display(Name = "Registering")]
        Registering = 0,
        [Display(Name = "Closed")]
        Closed = 1
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


    ////////

    public enum TransactionStatus
    {
        [Display(Name = "New")]
        New = 0,
        [Display(Name = "Approve")]
        Approve = 1,
        [Display(Name = "Cancel")]
        Cancel = 2,
    }

    public enum TransactionAmount
    {
        [Display(Name = "100,000 VNĐ")]
        VND100 = 100000,
        [Display(Name = "200,000 VNĐ")]
        VND200 = 200000,
        [Display(Name = "300,000 VNĐ")]
        VND300 = 300000,
        [Display(Name = "500,000 VNĐ")]
        VND500 = 500000
    }

    public enum TransactionTypeEnum
    {
        [Display(Name = "Normal")]
        Normal = 1,
        [Display(Name = "Rollback")]
        RollBack = 2,
    }

    public enum RegistrationStatus
    {
        [Display(Name = "Processing")]
        Processing = 1,
        [Display(Name = "Cancel")]
        Cancel = 2,
        [Display(Name = "Done")]
        Done = 3,
    }

    public enum AccountType
    {
        [Display(Name = "Normal")]
        Normal = 1,
        [Display(Name = "50% schoolarship")]
        HalflyDiscount = 2,
        [Display(Name = "100% schoolarship")]
        FullyDiscount = 3,
    }

    public enum TransactionForm
    {
        [Display(Name = "Increase")]
        Increase = 1,
        [Display(Name = "Decrease")]
        Decrease = 2,
    }

    public enum TransactionFilter
    {
        [Display(Name = "Add Funds")]
        AddFunds = 1,
        [Display(Name = "Pay for registered")]
        PayforRegistered = 2,
        [Display(Name = "Rollback Increase")]
        RollbackIncrease = 3,
        [Display(Name = "Rollback Decrease")]
        RollbackDecrease = 4,
    }
}