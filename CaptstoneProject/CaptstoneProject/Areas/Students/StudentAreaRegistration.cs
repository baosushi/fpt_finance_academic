using System.Web.Mvc;

namespace CaptstoneProject.Areas.Students
{
    public class StudentAreaRegistration : AreaRegistration 
    {
        public override string AreaName 
        {
            get 
            {
                return "Students";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context) 
        {
            context.MapRoute(
                "Students_default",
                "Students/{controller}/{action}/{id}",
                new { action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}