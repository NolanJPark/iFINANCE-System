using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace Group12_iFINANCEAPP.Models
{
    //Model for the Trial Balance report
    public class TrialBalanceItemViewModel
    {
        public string AccountName { get; set; }
        public decimal OpeningAmount { get; set; }
        public decimal ClosingAmount { get; set; }
    }

    //Model for the Profit Loss report
    public class ProfitLossViewModel
    {
        [Display(Name = "Total Income")]
        public decimal TotalIncome { get; set; }

        [Display(Name = "Total Expenses")]
        public decimal TotalExpenses { get; set; }

        [Display(Name = "Profit (Loss)")]
        public decimal ProfitOrLoss { get; set; }
    }

    //Model for the Balance Sheet rows
    public class BalanceSheetItem
    {
        public string AccountName { get; set; }
        public decimal ClosingAmount { get; set; }
    }

    //Model for the Balance Sheet rows full report
    public class BalanceSheetViewModel
    {
        public List<BalanceSheetItem> AssetItems { get; set; }
        public List<BalanceSheetItem> LiabilityItems { get; set; }
        public decimal TotalAssets => AssetItems.Sum(i => i.ClosingAmount);
        public decimal TotalLiabilities => LiabilityItems.Sum(i => i.ClosingAmount);

        //Reused ProfitLoss calculation
        public decimal ProfitOrLoss { get; set; }
        public decimal LiabilitiesAndEquityTotal => TotalLiabilities + ProfitOrLoss;
    }
}
