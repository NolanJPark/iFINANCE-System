using System;

namespace Group12_iFINANCEAPP.Models
{
    public class TransactionLineViewModel
    {
        public string ID { get; set; }
        public DateTime Date { get; set; }
        public string Description { get; set; }
        public decimal TotalDebit { get; set; }
        public decimal TotalCredit { get; set; }
    }
}
