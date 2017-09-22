using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(CaptstoneProject.Startup))]
namespace CaptstoneProject
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
