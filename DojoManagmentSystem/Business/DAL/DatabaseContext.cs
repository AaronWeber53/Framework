using Business.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.ModelConfiguration.Conventions;
using System.Linq;
using System.Web;

namespace Business.DAL
{
    public class DatabaseContext : DbContext
    {
        public DatabaseContext() : base()
        {
            Database.SetInitializer<DatabaseContext>(new MigrateDatabaseToLatestVersion<DatabaseContext, Configuration>());
            Configuration.LazyLoadingEnabled = false;
        }

        public static List<T> GetGenericList<T>() where T : BaseModel
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

        public DbSet<T> GetDbSet<T>() where T : BaseModel
        {
            return this.Set<T>();
        }
        public DbSet GetDbSet(Type type)
        {
            return this.Set(type);
        }


        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Conventions.Remove<PluralizingTableNameConvention>();

            // Create a relationship where a member may exist without a user 
            // but a user must exist with a member.
            modelBuilder.Entity<Member>()
                .HasOptional(q => q.User)
                .WithRequired(u => u.Member);
            //modelBuilder.Entity<User>()
            //    .HasRequired(q => q.Member);
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


        public IQueryable<T> GetDBList<T>() where T : BaseModel
        {
            Type type = typeof(DbSet<T>);
            var objectList = this.GetType().GetProperties();
            var newList = objectList.Where(m => m.PropertyType == type).First().GetValue(this);
            return (IQueryable<T>)newList;
        }
    }
}