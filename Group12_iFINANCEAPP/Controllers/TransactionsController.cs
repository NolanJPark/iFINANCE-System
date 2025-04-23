using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Group12_iFINANCEAPP.Models;

namespace Group12_iFINANCEAPP.Controllers
{
    public class TransactionsController : Controller
    {
        private Group12_iFINANCEDB db = new Group12_iFINANCEDB();

        // GET: List all the transactions
        public ActionResult Index()
        {
            var list = db.Transaction
                .Select(t => new TransactionListItemViewModel
                {
                    ID = t.ID,
                    Date = t.date,
                    Description = t.description,
                    TotalDebit = t.TransactionLine.Sum(l => l.debitedAmount),
                    TotalCredit = t.TransactionLine.Sum(l => l.creditedAmount)
                }).ToList();

            return View(list);
        }

        // GET: Create a transaction
        public ActionResult Create()
        {
            ViewBag.Accounts = new SelectList(db.MasterAccount, "ID", "name");
            var vm = new TransactionViewModel
            {
                Date = DateTime.Today,
                Lines = new List<TransactionLineViewModel>
                {
                    new TransactionLineViewModel()
                }
            };
            return View(vm);
        }

        // POST: Create a transaction
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(TransactionViewModel vm)
        {
            //Ensure at least one detail line exists
            if (vm.Lines == null || !vm.Lines.Any())
                ModelState.AddModelError(nameof(vm.Lines),"At least one transaction line is required.");

            //Make sure total debits == total credits
            //This language is odd
            var totalDr = vm.Lines?.Sum(l => l.DebitAmount) ?? 0m;
            var totalCr = vm.Lines?.Sum(l => l.CreditAmount) ?? 0m;
            if (totalDr != totalCr)
                ModelState.AddModelError("", "Total debits must equal total credits.");

            if (!ModelState.IsValid)
            {
                ViewBag.Accounts = new SelectList(db.MasterAccount, "ID", "name");
                return View(vm);
            }

            //Create transaction header
            var currentUserName = User.Identity.Name;
            var userPass = db.UserPassword.FirstOrDefault(up => up.username == currentUserName);
            if (userPass == null)
                return new HttpStatusCodeResult(403, "Unknown user");

            var txn = new Transaction
            {
                ID = Guid.NewGuid().ToString(),
                date = vm.Date,
                description = vm.Description,
                NonAdminUserID = userPass.ID
            };
            db.Transaction.Add(txn);

            //Create each line of the transaction
            foreach (var line in vm.Lines)
            {
                var tl = new TransactionLine
                {
                    ID = Guid.NewGuid().ToString(),
                    TransactionID = txn.ID,
                    FirstMasterAccountID = line.DebitAccountID,
                    SecondMasterAccountID = line.CreditAccountID,
                    debitedAmount = line.DebitAmount,
                    creditedAmount = line.CreditAmount,
                    comment = line.Comment
                };
                db.TransactionLine.Add(tl);
            }

            db.SaveChanges();

            //Adjust balances properly based on what type of account it is
            foreach (var line in vm.Lines)
            {
                //Debit calculations
                var drAcct = db.MasterAccount.Include(a => a.Group.AccountCategory).First(a => a.ID == line.DebitAccountID);
                var drType = drAcct.Group.AccountCategory.type;
                if (drType == "Debit")
                    drAcct.closingAmount += line.DebitAmount;
                else
                    drAcct.closingAmount -= line.DebitAmount;

                //Credit calculations
                var crAcct = db.MasterAccount.Include(a => a.Group.AccountCategory).First(a => a.ID == line.CreditAccountID);
                var crType = crAcct.Group.AccountCategory.type;
                if (crType == "Credit")
                    crAcct.closingAmount += line.CreditAmount;
                else
                    crAcct.closingAmount -= line.CreditAmount;
            }

            db.SaveChanges();

            return RedirectToAction("Index");
        }

        // GET: Delete transaction
        public ActionResult Delete(string id)
        {
            if (id == null) return new HttpStatusCodeResult(400);
            var txn = db.Transaction.Find(id);
            if (txn == null) return HttpNotFound();
            return View(txn);
        }

        // POST: Delete transaction
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(string id)
        {
            //Remove lines first
            var lines = db.TransactionLine.Where(l => l.TransactionID == id);
            db.TransactionLine.RemoveRange(lines);

            //Remove header
            var txn = db.Transaction.Find(id);
            db.Transaction.Remove(txn);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
