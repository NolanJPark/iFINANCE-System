using System;

namespace Group12_iFINANCEAPP.Models
{
    //Model for the Transaction List
    //does this do anything anymore with the new changes?
    //So it would be more aptely named TransactionLineViewModel, but trying to change it broke the wholr project
    //So I'm leaving it like this
    public class TransactionListItemViewModel
    {
        public string ID { get; set; }
        public DateTime Date { get; set; }
        public string Description { get; set; }
        public decimal TotalDebit { get; set; }
        public decimal TotalCredit { get; set; }
    }
}
