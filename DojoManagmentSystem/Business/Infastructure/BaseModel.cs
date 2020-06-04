using Business.DAL;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Business
{
    public abstract class BaseModel
    {
        public int Id { get; set; }

        public DateTime? LastModified { get; set; }

        [Index("ModifiedById")]
        public int? LastUserIdModifiedBy { get; set; }

        public bool IsArchived { get; set; } = false;

        /// <summary>
        /// Override this method to add deletion rules onto a model but always call base after you set what you need.
        /// </summary>
        /// <param name="db">Database being used to access the data.</param>
        public virtual void Delete(DojoManagmentContext db)
        {
            // Archive the object.
            this.IsArchived = true;
            
            // Make sure entity framework recognizes that this object has been modified.
            db.Entry(this).State = System.Data.Entity.EntityState.Modified;
            db.SaveChanges();
        }
    }
}