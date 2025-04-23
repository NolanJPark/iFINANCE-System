using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Group12_iFINANCEAPP.Models
{
    //Detail row in a transaction
    public class TransactionLineViewModel
    {
        [Required] public string DebitAccountID { get; set; }
        [Required] public string CreditAccountID { get; set; }
        [Required] public decimal DebitAmount { get; set; }
        [Required] public decimal CreditAmount { get; set; }
        public string Comment { get; set; }
    }

    //Master transaction plus multiple lines
    //this doesn't work.
    public class TransactionViewModel
    {
        public string ID { get; set; }

        [Required, DataType(DataType.Date)]
        public DateTime Date { get; set; }

        public string Description { get; set; }

        [Required]
        public List<TransactionLineViewModel> Lines { get; set; }
    }
}
