using System;
using System.Linq;
using System.Web.Mvc;
using Group12_iFINANCEAPP.Models;
using System.Collections.Generic;

namespace Group12_iFINANCEAPP.Controllers
{
    [Authorize]
    public class ChartOfAccountsController : Controller
    {
        private Group12_iFINANCEDB db = new Group12_iFINANCEDB();

        // GET: ChartOfAccounts
        public ActionResult Index()
        {
            //Load all Groups for the dropdown
            var groups = db.Group.Select(g => new { g.ID, g.Name }).ToList();
            ViewBag.Groups = new SelectList(groups, "ID", "Name");

            //Load all MasterAccounts
            var accounts = db.MasterAccount.Select(a => new MasterAccountViewModel
            {
                ID = a.ID,
                Name = a.name,
                OpeningAmount = a.openingAmount,
                ClosingAmount = a.closingAmount,
                GroupID = a.GroupID
            }).ToList();

            return View(accounts);
        }

        // POST: inline update (Name, OpeningAmount, ClosingAmount, or GroupID)
        [HttpPost]
        public JsonResult Update(string id, string field, string value)
        {
            var acct = db.MasterAccount.Find(id);
            if (acct == null)
                return Json(new { success = false, message = "Account not found." });

            try
            {
                switch (field)
                {
                    case "name":
                        acct.name = value;
                        break;
                    case "opening":
                        acct.openingAmount = decimal.Parse(value);
                        break;
                    case "closing":
                        acct.closingAmount = decimal.Parse(value);
                        break;
                    case "group":
                        acct.GroupID = value;
                        break;
                    default:
                        return Json(new { success = false, message = "Invalid field." });
                }
                db.SaveChanges();
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // POST: create a new MasterAccount
        [HttpPost]
        public JsonResult Add(string name, decimal opening, decimal closing, string groupId)
        {
            var newId = Guid.NewGuid().ToString();
            var acct = new MasterAccount
            {
                ID = newId,
                name = name,
                openingAmount = opening,
                closingAmount = closing,
                GroupID = groupId
            };

            db.MasterAccount.Add(acct);
            db.SaveChanges();

            return Json(new
            {
                success = true,
                id = newId,
                name = name,
                opening = opening,
                closing = closing,
                groupId = groupId
            });
        }

        // POST: delete a MasterAccount
        [HttpPost]
        public JsonResult Delete(string id)
        {
            var acct = db.MasterAccount.Find(id);
            if (acct == null)
                return Json(new { success = false, message = "Account not found." });

            db.MasterAccount.Remove(acct);
            db.SaveChanges();

            return Json(new { success = true });
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }
    }
}
