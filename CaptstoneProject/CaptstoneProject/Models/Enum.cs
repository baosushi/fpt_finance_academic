using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace CaptstoneProject.Models
{
    public enum SemesterStatus
    {
        [Display(Name = "Registration")]
        Registration = 0,
        [Display(Name = "Open")]
        Open = 1,
        [Display(Name = "Pending")]
        Pending = 2,
        [Display(Name = "Closed")]
        Closed = 3,
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
        //[Display(Name = "Publishable")]
        //FirstPublish = 3,
        //[Display(Name = "Final Publish")]
        //FinalPublish = 4,
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
        [Display(Name = "Mới")]
        New = 0,
        [Display(Name = "Đã Duyệt")]
        Approve = 1,
        [Display(Name = "Hủy bỏ")]
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
        //[Display(Name = "Normal")]
        //Normal = 1,
        //[Display(Name = "Rollback")]
        //RollBack = 2,

        [Display(Name = "Đóng tiền")]
        AddFunds = 1,
        [Display(Name = "Thanh toán học phí")]
        TuitionPayment = 2,
        [Display(Name = "Hoàn học phí")]
        RefundTuitionFee = 3,
        [Display(Name = "Điều chỉnh tăng tiền")]
        AdjustIncrease = 4,
        [Display(Name = "Điều chỉnh giảm tiền")]
        AdjustDecrease = 5,
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
        [Display(Name = "Bình thường")]
        Normal = 1,
        [Display(Name = "Học bổng 50%")]
        HalflyDiscount = 2,
        [Display(Name = "Học bổng 100%")]
        FullyDiscount = 3,
    }

    public enum TransactionForm
    {
        [Display(Name = "Increase")]
        Increase = 1,
        [Display(Name = "Decrease")]
        Decrease = 2,
    }

    public enum RegistrationType
    {
        [Display(Name = "Curriculum Subject")]
        CurriculumSubject = 1,
        [Display(Name = "Relearn Subject")]
        RelearnSubject = 2,
    }

    public enum JsonResultErrorType
    {
        [Display(Name = "Unauthorized access")]
        Unauthorized = 1,
        [Display(Name = "Exception occured")]
        Exception = 2,
        [Display(Name = "Failed to process request")]
        Failed = 3,
        [Display(Name = "Request Expired")]
        Expired = 4,
    }

    //public enum TransactionFilter
    //{
    //    [Display(Name = "Add Funds")]
    //    AddFunds = 1,
    //    [Display(Name = "Pay for registered")]
    //    PayforRegistered = 2,
    //    [Display(Name = "Rollback Increase")]
    //    RollbackIncrease = 3,
    //    [Display(Name = "Rollback Decrease")]
    //    RollbackDecrease = 4,
    //}
}