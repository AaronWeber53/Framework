using Business.DAL;
using System;
using System.Data.Entity.Core.Objects.DataClasses;

namespace Business
{
    public interface IBaseModel
    {
        long Id { get; set; }
        DateTime? LastModified { get; set; }
        long? LastUserIdModifiedBy { get; set; }
        bool IsArchived { get; set; }

        /// <summary>
        /// Override this method to add deletion rules onto a model but always call base after you set what you need.
        /// </summary>
        /// <param name="db">Database being used to access the data.</param>
        void Delete(DatabaseContext db);

        void Save(DatabaseContext db);
    }
}