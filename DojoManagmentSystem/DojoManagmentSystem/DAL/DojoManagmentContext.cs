using DojoManagmentSystem.Infastructure;
using DojoManagmentSystem.Migrations;
using DojoManagmentSystem.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.ModelConfiguration.Conventions;
using System.Linq;
using System.Web;

namespace DojoManagmentSystem.DAL
{
    public class DojoManagmentContext : DbContext
    {
        public DojoManagmentContext() : base()
        {
            Database.SetInitializer<DojoManagmentContext>(new MigrateDatabaseToLatestVersion<DojoManagmentContext, Configuration>());
            Configuration.LazyLoadingEnabled = false;
        }

        #region DBSets

        #endregion

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Conventions.Remove<PluralizingTableNameConvention>();

            // Create a relationship where a member may exist without a user 
            // but a user must exist with a member.
            modelBuilder.Entity<Member>()
                .HasOptional(q => q.User);
            modelBuilder.Entity<User>()
                .HasRequired(q => q.Member);
        }

        public override int SaveChanges()
        {
            var changedModelList = ChangeTracker.Entries().Where(x => x.Entity is BaseModel && (x.State == EntityState.Added || x.State == EntityState.Modified));

            foreach (var model in changedModelList)
            {
                ((BaseModel)model.Entity).LastModified = DateTime.Now;
                ((BaseModel)model.Entity).LastUserIdModifiedBy = ApplicationContext.CurrentApplicationContext?.CurrentSession?.UserId ?? null;
            }

            return base.SaveChanges();
        }

        public DbSet<User> Users { get; set; }

        public DbSet<Member> Members { get; set; }

        public DbSet<Payment> Payments { get; set; }

        public DbSet<Session> Sessions { get; set; }

        public DbSet<Contact> Contacts { get; set; }

        public DbSet<MemberPhone> MemberPhones { get; set; }

        public DbSet<MemberEmail> MemberEmail { get; set; }

        public DbSet<MemberAddress> MemberAddresses { get; set; }

        public DbSet<Discipline> Disciplines { get; set; }

        public DbSet<ClassSession> ClassSessions { get; set; }

        public DbSet<DisciplineEnrolledMember> DisciplineEnrolledMembers { get; set; }

        public DbSet<AttendanceSheet> AttendanceSheets { get; set; }

        public DbSet<Waiver> Waivers { get; set; }
    }
}