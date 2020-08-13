using Business.DAL;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Business.Models
{
    public class ApplicationUser : IdentityUser, IBaseModel<string>
    {
        public DateTime? LastModified { get; set; }

        [Index("ModifiedById")]
        public long? LastUserIdModifiedBy { get; set; }

        public bool IsArchived { get; set; } = false;

        /// <summary>
        /// Override this method to add deletion rules onto a model but always call base after you set what you need.
        /// </summary>
        /// <param name="db">Database being used to access the data.</param>
        public virtual void Delete(DatabaseContext db)
        {
            // Archive the object.
            this.IsArchived = true;

            // Make sure entity framework recognizes that this object has been modified.
            db.Entry(this).State = EntityState.Modified;
            db.SaveChanges();
        }

        public virtual void Save(DatabaseContext db)
        {
            var state = db.Entry(this).State;

            Type type = this.GetType();


            if (state == EntityState.Detached)
            {
                db.GetDbSet(type).Add(this);
            }
            else if (db.Entry(this).State != EntityState.Added || Id != "")
            {
                db.Entry(this).State = System.Data.Entity.EntityState.Modified;
            }
            db.SaveChanges();
        }

        public async Task<ClaimsIdentity> GenerateUserIdentityAsync(UserManager<ApplicationUser> manager)
        {
            // Note the authenticationType must match the one defined in CookieAuthenticationOptions.AuthenticationType
            var userIdentity = await manager.CreateIdentityAsync(this, DefaultAuthenticationTypes.ApplicationCookie);
            // Add custom user claims here
            return userIdentity;
        }
    }
}
