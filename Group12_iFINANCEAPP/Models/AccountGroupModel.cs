using System.Collections.Generic;

namespace Group12_iFINANCEAPP.Models
{
    //Represents a single group node in the tree
    public class GroupNodeViewModel
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public List<GroupNodeViewModel> Children { get; set; }
    }

    //Represents the category
    public class CategoryTreeViewModel
    {
        public string CategoryId { get; set; }
        public string CategoryName { get; set; }
        public List<GroupNodeViewModel> Groups { get; set; }
    }
}
