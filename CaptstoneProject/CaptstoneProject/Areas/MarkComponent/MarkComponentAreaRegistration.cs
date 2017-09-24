using System.Web.Mvc;

namespace CaptstoneProject.Areas.MarkComponent
{
    public class MarkComponentAreaRegistration : AreaRegistration 
    {
        public override string AreaName 
        {
            get 
            {
                return "MarkComponent";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context) 
        {
            context.MapRoute(
                "MarkComponent_default",
                "MarkComponent/{controller}/{action}/{id}",
                new { action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}