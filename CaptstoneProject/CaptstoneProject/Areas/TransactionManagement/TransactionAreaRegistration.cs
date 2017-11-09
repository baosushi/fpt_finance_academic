using System.Web.Mvc;

namespace CaptstoneProject.Areas.TransactionManagement
{
    public class TransactionAreaRegistration : AreaRegistration 
    {
        public override string AreaName 
        {
            get 
            {
                return "TransactionManagement";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context) 
        {
            context.MapRoute(
                "TransactionManagement_default",
                "TransactionManagement/{controller}/{action}/{id}",
                new { action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}