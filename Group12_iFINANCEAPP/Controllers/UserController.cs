using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Helpers;
using System.Web.Mvc;
using System.Web.Security;
using System.Xml.Linq;
using Group12_iFINANCEAPP.Models;

namespace Group12_iFINANCEAPP.Controllers
{
    public class UserController : Controller
    {
        private Group12_iFINANCEDB db = new Group12_iFINANCEDB();

        // GET: User
        public ActionResult Index()
        {
            var nonAdmins = db.NonAdminUser.Include("Administrator").Include("UserPassword");
            return View(nonAdmins.ToList());
        }

        // GET: Produce details about user
        public ActionResult Details(string id)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            NonAdminUser nonAdminUser = db.NonAdminUser.Find(id);

            if (nonAdminUser == null)
                return HttpNotFound();
            return View(nonAdminUser);
        }

        // GET: Create user
        [Authorize]
        public ActionResult Create()
        {
            return View(new RegisterViewModel());
        }

        // POST: Create user
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            //Generate a new ID for this user
            var newId = Guid.NewGuid().ToString();

            //Create the password record
            var up = new UserPassword
            {
                ID = newId,
                username = model.Username,
                encryptedPassword = model.Password,
                passwordExpiryTime = 30,
                userAccountExpiryDate = DateTime.Now.AddYears(1)
            };
            db.UserPassword.Add(up);

            //Handle the creation of either an Admin or a Non-Admin
            if (model.IsAdministrator)
            {
                var admin = new Administrator
                {
                    ID = newId,
                    name = model.Name,
                    dateHired = DateTime.Now,
                    dateFinished = null
                };
                db.Administrator.Add(admin);
            }
            else
            {
                //Find the logged in admin's ID for the NonAdminUser
                var currentUsername = User.Identity.Name;
                var currentUp = db.UserPassword.FirstOrDefault(u => u.username == currentUsername);
                if (currentUp == null || !db.Administrator.Any(a => a.ID == currentUp.ID))
                {
                    ModelState.AddModelError("", "Could not determine your administrator ID.");
                    return View(model);
                }
                var adminId = currentUp.ID;

                var user = new NonAdminUser
                {
                    ID = newId,
                    name = model.Name,
                    address = model.Address,
                    email = model.Email,
                    AdministratorID = adminId
                };
                db.NonAdminUser.Add(user);
            }

            db.SaveChanges();

            return RedirectToAction("Index");
        }

        // GET: Edit user
        public ActionResult Edit(string id)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            NonAdminUser nonAdminUser = db.NonAdminUser.Find(id);
            if (nonAdminUser == null)
                return HttpNotFound();

            ViewBag.AdministratorID = new SelectList(db.Administrator, "ID", "name", nonAdminUser.AdministratorID);
            ViewBag.ID = new SelectList(db.UserPassword, "ID", "username", nonAdminUser.ID);
            return View(nonAdminUser);
        }

        // POST: Edit user
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "ID,name,address,email,AdministratorID")] NonAdminUser nonAdminUser)
        {
            if (ModelState.IsValid)
            {
                db.Entry(nonAdminUser).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.AdministratorID = new SelectList(db.Administrator, "ID", "name", nonAdminUser.AdministratorID);
            ViewBag.ID = new SelectList(db.UserPassword, "ID", "username", nonAdminUser.ID);
            return View(nonAdminUser);
        }

        // GET: Delete user
        public ActionResult Delete(string id)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            NonAdminUser nonAdminUser = db.NonAdminUser.Find(id);
            if (nonAdminUser == null)
                return HttpNotFound();
            return View(nonAdminUser);
        }

        // POST: Delete user
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(string id)
        {
            //Find user
            var user = db.NonAdminUser.Find(id);
            if (user == null)
                return HttpNotFound();

            //Delete all that user’s transactions
            var txns = db.Transaction.Include("TransactionLine").Where(t => t.NonAdminUserID == id).ToList();
            foreach (var txn in txns)
            {
                //Delete each line and the header
                foreach (var line in txn.TransactionLine.ToList())
                    db.TransactionLine.Remove(line);
                db.Transaction.Remove(txn);
            }

            //Remove users password record too
            var pw = db.UserPassword.Find(id);
            if (pw != null)
                db.UserPassword.Remove(pw);

            db.NonAdminUser.Remove(user);

            db.SaveChanges();

            return RedirectToAction("Index");
        }

        // GET: User attempts to login
        public ActionResult Login()
        {
            return View();
        }

        // POST: User attempts to login
        [HttpPost]
        [ValidateAntiForgeryToken]
        [AllowAnonymous]
        public ActionResult Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = db.UserPassword.FirstOrDefault(u => u.username.Trim().ToLower() == model.Username.Trim().ToLower() && u.encryptedPassword == model.Password.Trim());
            if (user == null)
            {
                ModelState.AddModelError("", "Invalid username or password.");
                return View(model);
            }

            FormsAuthentication.SetAuthCookie(user.username, model.RememberMe);

            //If this user is in the Admin table send them to Manage Users not the dashboard
            bool isAdmin = db.Administrator.Any(a => a.ID == user.ID);
            if (isAdmin)
                return RedirectToAction("Index", "User");

            //If the user is a Non Admin send them to the dashboard
            return RedirectToAction("Index", "Home");
        }

        // GET: User logs out of iFINANCE
        public ActionResult Logout()
        {
            FormsAuthentication.SignOut();
            return RedirectToAction("Login", "User");
        }

        // GET: User changes their password
        [Authorize]
        public ActionResult ChangePassword()
        {
            //Get the user who's logged into the system
            var username = User.Identity.Name;
            ViewBag.Username = username;

            //Check if they're an admin
            var up = db.UserPassword.FirstOrDefault(u => u.username == username);
            ViewBag.IsAdmin = (up != null) && db.Administrator.Any(a => a.ID == up.ID);

            return View();
        }

        // POST: User changes their password
        [HttpPost, ValidateAntiForgeryToken, Authorize]
        public ActionResult ChangePassword(ChangePasswordViewModel model)
        {
            //Re-fill the same ViewBag for redisplay
            var username = User.Identity.Name;
            ViewBag.Username = username;
            ViewBag.IsAdmin = db.Administrator.Any(a => a.ID == db.UserPassword
            .Where(u => u.username == username)
            .Select(u => u.ID)
            .FirstOrDefault());

            //If validation fails redisplay
            if (!ModelState.IsValid)
                return View(model);

            //Lookup UserPassword record
            var userPass = db.UserPassword.FirstOrDefault(u => u.username == username);
            if (userPass == null)
            {
                ModelState.AddModelError("", "User record not found.");
                return View(model);
            }

            //Verify current password
            if (userPass.encryptedPassword != model.OldPassword)
            {
                ModelState.AddModelError(nameof(model.OldPassword), "Current password is incorrect.");
                return View(model);
            }

            userPass.encryptedPassword = model.NewPassword;
            db.SaveChanges();

            //Cool little confirmation
            TempData["Success"] = "Your password has been changed.";
            return RedirectToAction("ChangePassword");
        }


        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
