using System;

namespace Group12_iFINANCEAPP.Models
{
    //Model for the Transaction List
    //does this do anything anymore with the new changes?
    public class TransactionListItemViewModel
    {
        public string ID { get; set; }
        public DateTime Date { get; set; }
        public string Description { get; set; }
        public decimal TotalDebit { get; set; }
        public decimal TotalCredit { get; set; }
    }
}
