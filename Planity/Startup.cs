using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.Owin;
using Owin;
using Planity.Models;

[assembly: OwinStartupAttribute(typeof(Planity.Startup))]
namespace Planity
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
            CreateRoles();
        }

        private void CreateRoles()
        {
            using (var context = new ApplicationDbContext())
            {
                var roleManager = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(context));
                var userManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(context));

                string[] roles = { "Admin", "TimLeader", "Student" };
                foreach (var role in roles)
                {
                    if (!roleManager.RoleExists(role))
                    {
                        roleManager.Create(new IdentityRole(role));
                    }
                }

                var adminEmail = "admin@admin.com";
                var adminUser = userManager.FindByEmail(adminEmail);
                if (adminUser == null)
                {
                    adminUser = new ApplicationUser
                    {
                        UserName = adminEmail,
                        Email = adminEmail,
                        FullName = "System Admin"
                    };

                    var result = userManager.Create(adminUser, "admin123");
                    if (result.Succeeded)
                    {
                        userManager.AddToRole(adminUser.Id, "Admin");
                    }
                }
            }
        }
    }
}
