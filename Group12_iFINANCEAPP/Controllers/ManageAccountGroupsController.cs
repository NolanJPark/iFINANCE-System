using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Group12_iFINANCEAPP.Models;

namespace Group12_iFINANCEB.Controllers
{
    [Authorize]
    public class ManageAccountGroupsController : Controller
    {
        private Group12_iFINANCEDB db = new Group12_iFINANCEDB();

        // GET: ManageAccountGroups
        public ActionResult Index()
        {
            //Load categories and build group tree in memory
            var cats = db.AccountCategory.ToList();
            var all = db.Group.ToList();

            var vm = cats.Select(cat => new CategoryTreeViewModel
            {
                CategoryId = cat.ID,
                CategoryName = cat.name,
                Groups = BuildTree(cat.ID, null, all)
            }).ToList();

            return View(vm);
        }

        //recursive helper function for loading the tree
        private List<GroupNodeViewModel> BuildTree(string catId, string parentId, List<Group> all)
        {
            return all.Where(g => g.AccountCategoryID == catId && ((parentId == null && g.ParentGroupID == null) || g.ParentGroupID == parentId))
                .Select(g => new GroupNodeViewModel
                {
                    Id = g.ID,
                    Name = g.Name,
                    Children = BuildTree(catId, g.ID, all)
                }).ToList();
        }

        // POST: Rename group
        [HttpPost]
        public JsonResult Rename(string id, string newName)
        {
            var g = db.Group.Find(id);
            if (g == null)
                return Json(new { success = false, message = "Not found." });
            g.Name = newName;

            db.SaveChanges();

            return Json(new { success = true });
        }

        // POST: Add subgroup
        [HttpPost]
        public JsonResult Add(string parentId, string categoryId, string name)
        {
            var newId = Guid.NewGuid().ToString();
            var grp = new Group
            {
                ID = newId,
                Name = name,
                AccountCategoryID = categoryId,
                ParentGroupID = string.IsNullOrEmpty(parentId) ? null : parentId
            };

            db.Group.Add(grp);
            db.SaveChanges();

            return Json(new { success = true, id = newId, name });
        }

        // recursive helper function deleteing a tree
        private void DeleteRecursive(string id)
        {
            var children = db.Group.Where(x => x.ParentGroupID == id).ToList();
            foreach (var c in children)
                DeleteRecursive(c.ID);

            var g = db.Group.Find(id);
            db.Group.Remove(g);
        }

        //Recursively collect this group and all subgroups for checking
        private void CollectSubtreeIds(string id, List<string> bucket)
        {
            bucket.Add(id);
            var children = db.Group.Where(g => g.ParentGroupID == id).Select(g => g.ID).ToList();
            foreach (var childId in children)
                CollectSubtreeIds(childId, bucket);
        }

        // POST: Delete group + group subtree
        [HttpPost]
        public JsonResult Delete(string id)
        {
            var g = db.Group.Find(id);
            if (g == null)
                return Json(new { success = false, message = "Group not found." });

            //Build list of all IDs that would be removed to check against the Master Accounts
            var toDelete = new List<string>();
            CollectSubtreeIds(id, toDelete);

            //Check if a MasterAccount references any of those IDs
            bool inUse = db.MasterAccount.Any(ma => toDelete.Contains(ma.GroupID));
            if (inUse)
            {
                return Json(new
                {
                    success = false,
                    message = "Cannot delete this group (or its sub-groups) because one or more master accounts belong to it. Please reassign or delete those master accounts first."
                });
            }

            DeleteRecursive(id);
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
