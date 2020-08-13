using Business.Migrations;
using Business.Models;
using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.ModelConfiguration.Conventions;
using System.Linq;
using System.Web;

namespace Business.DAL
{
    public class DatabaseContext : IdentityDbContext<ApplicationUser>
    {
        public DatabaseContext()
            : base("DefaultConnection", throwIfV1Schema: false)
        {
            Configuration.LazyLoadingEnabled = false;
        }

        public static DatabaseContext Create()
        {
            return new DatabaseContext();
        }

        public static List<T> GetGenericList<T>() where T : class, IBaseModel<long>
        {
            List<T> myDynamicList;

            using (DatabaseContext db = new DatabaseContext())
            {
                // consider using exception handling here as GetDbSet might get an invalid type
                IQueryable<T> dbSet = db.GetDbSet<T>();
                myDynamicList = dbSet.ToList();

            }

            if (myDynamicList != null && myDynamicList.Count() > 0)
            {
                return myDynamicList;
            }
            return new List<T>();
        }

        public DbSet<T> GetDbSet<T>() where T : class, IBaseModel<long>
        {
            return this.Set<T>();
        }
        public DbSet GetDbSet(Type type)
        {
            return this.Set(type);
        }


        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Conventions.Remove<PluralizingTableNameConvention>();

            // Create a relationship where a member may exist without a user 
            // but a user must exist with a member.
            modelBuilder.Entity<Member>()
                .HasOptional(q => q.User)
                .WithRequired(u => u.Member);
            //modelBuilder.Entity<User>()
            //    .HasRequired(q => q.Member);

            modelBuilder.Entity<IdentityUserClaim>().ToTable("UserClaims", "Security");
            modelBuilder.Entity<IdentityRole>().ToTable("Role", "Security");
            modelBuilder.Entity<IdentityUserRole>().ToTable("UserRoles", "Security");
            modelBuilder.Entity<IdentityUserLogin>().ToTable("UserLogins", "Security");
            modelBuilder.Entity<IdentityUser>().ToTable("Users", "Security");
            modelBuilder.Entity<ApplicationUser>().ToTable("Users", "Security");

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


        public IQueryable<T> GetDBList<T>() where T : class, IBaseModel<long>
        {
            Type type = typeof(DbSet<T>);
            var objectList = this.GetType().GetProperties();
            var newList = objectList.Where(m => m.PropertyType == type).First().GetValue(this);
            return (IQueryable<T>)newList;
        }
    }
}