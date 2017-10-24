using System.Web.Mvc;

namespace CaptstoneProject.Areas.AdminTrainingDepartment
{
    public class AdminTrainingDepartmentAreaRegistration : AreaRegistration 
    {
        public override string AreaName 
        {
            get 
            {
                return "AdminTrainingDepartment";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context) 
        {
            context.MapRoute(
                "AdminTrainingDepartment_default",
                "AdminTrainingDepartment/{controller}/{action}/{id}",
                new { action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}