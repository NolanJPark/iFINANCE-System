using System.ComponentModel.DataAnnotations;

namespace Group12_iFINANCEAPP.Models
{
    //Model for the master account screen
    public class MasterAccountViewModel
    {
        public string ID { get; set; }

        [Required] public string Name { get; set; }

        [Display(Name = "Opening Amount"), Range(0, double.MaxValue)]
        public decimal OpeningAmount { get; set; }

        [Display(Name = "Closing Amount"), Range(0, double.MaxValue)]
        public decimal ClosingAmount { get; set; }

        [Display(Name = "Group")] public string GroupID { get; set; }
    }
}
