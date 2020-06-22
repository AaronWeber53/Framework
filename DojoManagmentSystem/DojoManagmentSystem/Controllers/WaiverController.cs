using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Business.DAL;
using Business.Models;
using DojoManagmentSystem.ViewModels;

namespace DojoManagmentSystem.Controllers
{
    public class WaiverController : BaseController<Waiver>
    {        
        // GET: Waiver/Create
        public ActionResult Create(int id)
        {
            ViewBag.LastUserIdModifiedBy = new SelectList(db.GetDbSet<User>(), "Id", "Username");

            Waiver waiver = new Waiver { DateSigned = DateTime.Today };

            ViewBag.MemberId = id;
            return PartialView(waiver);
        }

        // POST: Waiver/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,Note,DateSigned,IsSigned,MemberId")] Waiver waiver)
        {
            if (ModelState.IsValid)
            {
                db.GetDbSet<Waiver>().Add(waiver);
                db.SaveChanges();
                return Json(new JsonReturn { RefreshScreen = true });
            }
            
            ViewBag.MemberId = waiver.MemberId;
            return PartialView(waiver);
        }
    }
}
