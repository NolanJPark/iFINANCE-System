using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Group12_iFINANCEAPP.Models;

namespace Group12_iFINANCEB.Controllers
{
    //added authorize because apparently it forces the user to be verified
    [Authorize]
    public class HomeController : Controller
    {
        private Group12_iFINANCEDB db = new Group12_iFINANCEDB();
        public ActionResult Index()
        {
            var username = User.Identity.Name;
            var userPass = db.UserPassword.FirstOrDefault(u => u.username == username);

            //Check if the user is an Admin, if they are send them straight to Manage Users
            if (userPass != null && db.Administrator.Any(a => a.ID == userPass.ID))
                return RedirectToAction("Index", "User");

            //If the usre isn't an admin they get sent to the Home dashboard
            return View();
        }

        public ActionResult Contact()
        {
            //idk do something with this later ig
            ViewBag.Message = "Your contact page.";

            return View();
        }
    }
}