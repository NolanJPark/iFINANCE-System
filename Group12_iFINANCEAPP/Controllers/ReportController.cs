using System.Linq;
using System.Web.Mvc;
using Group12_iFINANCEAPP.Models;

namespace Group12_iFINANCEAPP.Controllers
{
    [Authorize]
    public class ReportsController : Controller
    {
        private Group12_iFINANCEDB db = new Group12_iFINANCEDB();

        // GET: Reports
        public ActionResult Index()
        {
            return View();
        }

        // GET: Trial Balance report
        public ActionResult TrialBalance()
        {
            //are these things getting longer???
            //what is c sharp
            var rows = (from ma in db.MasterAccount
                        join g in db.Group on ma.GroupID equals g.ID
                        join ac in db.AccountCategory on g.AccountCategoryID equals ac.ID
                        select new TrialBalanceItemViewModel
                        {
                            AccountName = ma.name,
                            OpeningAmount = ma.openingAmount,
                            ClosingAmount = ma.closingAmount
                        }).ToList();

            return View(rows);
        }

        // GET: Profit Loss report
        public ActionResult ProfitLoss()
        {
            //Sum of Income
            var totalIncome = (from ma in db.MasterAccount
                               join g in db.Group on ma.GroupID equals g.ID
                               join ac in db.AccountCategory on g.AccountCategoryID equals ac.ID
                               where ac.name == "Income"
                               select ma.closingAmount).DefaultIfEmpty(0m).Sum();

            //Sum of Expenses
            var totalExpenses = (from ma in db.MasterAccount
                                 join g in db.Group on ma.GroupID equals g.ID
                                 join ac in db.AccountCategory on g.AccountCategoryID equals ac.ID
                                 where ac.name == "Expenses"
                                 select ma.closingAmount).DefaultIfEmpty(0m).Sum();

            var model = new ProfitLossViewModel
            {
                TotalIncome = totalIncome,
                TotalExpenses = totalExpenses,
                ProfitOrLoss = totalIncome - totalExpenses
            };

            return View(model);
        }

        // GET: Balance Sheet report
        public ActionResult BalanceSheet()
        {
            //Build asset lines
            var assets = (from ma in db.MasterAccount
                          join g in db.Group on ma.GroupID equals g.ID
                          join ac in db.AccountCategory on g.AccountCategoryID equals ac.ID
                          where ac.name == "Assets"
                          select new BalanceSheetItem
                          {
                              AccountName = ma.name,
                              ClosingAmount = ma.closingAmount
                          }).ToList();

            //Build liability lines
            var liabilities = (from ma in db.MasterAccount
                               join g in db.Group on ma.GroupID equals g.ID
                               join ac in db.AccountCategory on g.AccountCategoryID equals ac.ID
                               where ac.name == "Liabilities"
                               select new BalanceSheetItem
                               {
                                   AccountName = ma.name,
                                   ClosingAmount = ma.closingAmount
                               }).ToList();

            //Reuse Profit/Loss we already did
            var totalIncome = (from ma in db.MasterAccount
                               join g in db.Group on ma.GroupID equals g.ID
                               join ac in db.AccountCategory on g.AccountCategoryID equals ac.ID
                               where ac.name == "Income"
                               select ma.closingAmount).DefaultIfEmpty(0m).Sum();

            var totalExpenses = (from ma in db.MasterAccount
                                 join g in db.Group on ma.GroupID equals g.ID
                                 join ac in db.AccountCategory on g.AccountCategoryID equals ac.ID
                                 where ac.name == "Expenses"
                                 select ma.closingAmount).DefaultIfEmpty(0m).Sum();

            var model = new BalanceSheetViewModel
            {
                AssetItems = assets,
                LiabilityItems = liabilities,
                ProfitOrLoss = totalIncome - totalExpenses
            };

            return View(model);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }
    }
}
