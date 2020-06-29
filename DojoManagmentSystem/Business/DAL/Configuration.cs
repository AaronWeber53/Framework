namespace Business.DAL
{
    using Business.Models;
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Migrations;
    using System.Linq;

    internal sealed class Configuration : DbMigrationsConfiguration<DAL.DatabaseContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = true;
            AutomaticMigrationDataLossAllowed = true;
        }

        protected override void Seed(DAL.DatabaseContext context)
        {
            //  This method will be called after migrating to the latest version.
            if (!context.GetDbSet<Member>().Any(a => a.User.Username == "Admin"))
            {
                Member memberAdmin = new Member() { Id = -1, FirstName = "Admin", LastName = "Admin", IsInstructor = true };
                context.GetDbSet<Member>().AddOrUpdate(memberAdmin);
                context.SaveChanges();

                User admin = new User() { Id = -1, Username = "Admin", HashPassword = "Password" };
                memberAdmin.User = admin;
                admin.Member = memberAdmin;

                context.GetDbSet<Member>().AddOrUpdate(memberAdmin);
                context.GetDbSet<User>().AddOrUpdate(admin);
                context.SaveChanges();
            }
            //  You can use the DbSet<T>.AddOrUpdate() helper extension method 
            //  to avoid creating duplicate seed data.
        }
    }
}
