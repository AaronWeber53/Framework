namespace DojoManagmentSystem.Migrations
{
    using DojoManagmentSystem.Models;
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Migrations;
    using System.Linq;

    internal sealed class Configuration : DbMigrationsConfiguration<DAL.DojoManagmentContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = false;
        }

        protected override void Seed(DAL.DojoManagmentContext context)
        {
            //  This method will be called after migrating to the latest version.
            Member memberAdmin = new Member() { Id = -1, FirstName = "Admin", LastName = "Admin", IsInstructor = true };
            context.Members.AddOrUpdate(memberAdmin);
            context.SaveChanges();

            User admin = new User() { Id = -1, Username = "Admin", HashPassword = "Password" };
            memberAdmin.User = admin;
            admin.Member = memberAdmin;

            context.Users.AddOrUpdate(admin);
            context.SaveChanges();
            //  You can use the DbSet<T>.AddOrUpdate() helper extension method 
            //  to avoid creating duplicate seed data.
        }
    }
}
