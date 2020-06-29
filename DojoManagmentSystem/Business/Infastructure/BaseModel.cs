using Business.DAL;
using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Linq;

namespace Business
{
    public abstract class BaseModel
    {
        public long Id { get; set; }

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
            db.Entry(this).State = System.Data.Entity.EntityState.Modified;
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
            else if (db.Entry(this).State != EntityState.Added || Id > 0)
            {
                db.Entry(this).State = System.Data.Entity.EntityState.Modified;
            }
            db.SaveChanges();
        }
    }
}