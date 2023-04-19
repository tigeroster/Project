using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Project.Models;
using System.Web.Security;
using System.IO;

namespace Project.Controllers
{
    public class HomeController : Controller
    {
        // GET: Home
        MonarchyEntities Registerentity = new MonarchyEntities();
        public ActionResult Index()
        {
            var data = Registerentity.Games.ToList();

            var countUser = (from countUsers in Registerentity.Users select countUsers).Count();
            ViewBag.countUser = countUser;

            var countGame = (from countGames in Registerentity.Games select countGames).Count();
            ViewBag.countGame = countGame;

            var countDeveloper = (from countDevelopers in Registerentity.Games select countDevelopers).Count();
            ViewBag.countDeveloper = countDeveloper;

            var countCatagory = (from countCatagories in Registerentity.Games select countCatagories).Count();
            ViewBag.countCatagory = countCatagory;

            return View(data);
        }

        public ActionResult UserRegister()
        {
            return View();
        }

        [HttpPost]
        public ActionResult UserRegister(User user, HttpPostedFileBase fileBase)
        {
            if (ModelState.IsValid)
            {

                var email = Registerentity.Users.Where(e => e.Email == user.Email);
                if (email.Count() == 0)
                {
                    if(fileBase != null)
                    {
                        string path = Server.MapPath("~/Content/img/Profile/");
                        string fileName = Path.GetFileName(fileBase.FileName);
                        if (!Directory.Exists(path))
                        {
                            Directory.CreateDirectory(path);
                        }
                        fileBase.SaveAs(path + fileName);
                        user.Profile_pic = ("~/Content/img/Profile/" + fileName);
                        
                    }
                    Registerentity.Users.Add(user);
                    Registerentity.SaveChanges();
                    ViewBag.msg = "success";
                    ModelState.Clear();
                    return View();
                }
                else
                {
                    ViewBag.error = "error";
                    ModelState.Clear();
                    return View();
                }

            }
            else
            {
                return View();
            }

        }
     
        public ActionResult Login()
        {
            return View();
        }
        
        public ActionResult Authenticate()
        {
            string email = Request.Form["email"];
            string password = Request.Form["password"];
            var pass = from p in Registerentity.Users where p.Email == email select p.Password;
            if (email == "" || password == "")
            {
                ViewBag.error = "error";
                return View("Login");
            }

            if (pass.Count() == 0)
            {
                ViewBag.error = "error";
                return View("Login");
            }
            else
            {
                foreach (var data in pass)
                {
                    if (password == data)
                    {
                        Session["email"] = email;
                        var details = from x in Registerentity.Users where x.Email == email select x.User_id;
                        Session["id"] = details.FirstOrDefault();
                        return RedirectToAction("ProductCatalogue");
                    }
                    else
                    {
                        ViewBag.error = "error";
                        return View("Login");
                    }
                }
            }

            return View("Login");
        }

        public ActionResult ProductCatalogue()
        {
            string mail = Convert.ToString(Session["email"]);
            var fName = from f in Registerentity.Users where f.Email == mail select f.First_Name;
            var lName = from l in Registerentity.Users where l.Email == mail select l.Last_name;
            Session["name"] = fName.FirstOrDefault() +" " + lName.FirstOrDefault();

            List<object> gameModel = new List<object>();

            gameModel.Add(Registerentity.Games.ToList());
            gameModel.Add(Registerentity.Categories.ToList());
            gameModel.Add(Registerentity.Developers.ToList());

            return View(gameModel);
        }
        [HttpPost]
        public ActionResult Search(string searchProduct)
        {
            var data = from product in Registerentity.Games where product.Game_Name == searchProduct select product;
            List<object> gameModel = new List<object>();

            if (data.Count() > 0)
            {
                gameModel.Add(data.ToList());
                System.Web.HttpContext.Current.Session["noData"] = "";
                
            }
            else
            {
                System.Web.HttpContext.Current.Session["noData"] = "No Results Found!";
                gameModel.Add(Registerentity.Games.ToList());
                
            }

            gameModel.Add(Registerentity.Categories.ToList());
            gameModel.Add(Registerentity.Developers.ToList());
            return View(gameModel);

        }
        public ActionResult Filter(string name)
        {
            if(name != null)
            {
                List<object> gameModel = new List<object>();

                var data = from f in Registerentity.Games where f.Category.Category_name == name select f;
                if (data.Count() > 0)
                {
                    gameModel.Add(data.ToList());
                    System.Web.HttpContext.Current.Session["noData"] = "";
                }
                else
                {
                    System.Web.HttpContext.Current.Session["noData"] = "No Results Found!";
                    gameModel.Add(Registerentity.Games.ToList());
                }
                gameModel.Add(Registerentity.Categories.ToList());
                gameModel.Add(Registerentity.Developers.ToList());
                return View(gameModel);
            }
            return RedirectToAction("ProductCatalogue");
            
        }
        public ActionResult Update()
        {
            int id = Convert.ToInt32(Session["id"]);
            var data = Registerentity.Users.FirstOrDefault(x => x.User_id == id);
            return View(data);
        }
        [HttpPost]
        public ActionResult Update(User user, HttpPostedFileBase fileBase)
        {
            int id = Convert.ToInt32(Session["id"]);
            var data = Registerentity.Users.FirstOrDefault(x => x.User_id == id);
            
            if(data != null)
            {
                if (ModelState.IsValid)
                {
                    
                    if (fileBase != null)
                    {
                        string path = Server.MapPath("~/Content/img/Profile/");
                        string fileName = Path.GetFileName(fileBase.FileName);
                        if (!Directory.Exists(path))
                        {
                            Directory.CreateDirectory(path);
                        }
                        fileBase.SaveAs(path + fileName);
                        user.Profile_pic = ("~/Content/img/Profile/" + fileName);
                        data.Profile_pic = user.Profile_pic;

                    }
                    data.First_Name = user.First_Name;
                    data.Last_name = user.Last_name;
                    data.Email = user.Email;
                    data.DoB = user.DoB;
                    data.Country = user.Country;
                    data.Password = user.Password;
                    data.ConfirmPassword = user.ConfirmPassword;
                   
                    Registerentity.SaveChanges();
                    ViewBag.msg = "success";
                }
                else
                {
                    return View(data);
                }
            }
            else
            {
                return RedirectToAction("Update");
            }
            return View(data);
        }
        public ActionResult Product(string name)
        {
            var data = from g in Registerentity.Games where g.Game_Name == name select g;
            List<object> gameModel = new List<object>();

            gameModel.Add(data.ToList());
            gameModel.Add(Registerentity.Categories.ToList());
            gameModel.Add(Registerentity.Developers.ToList());

            return View(gameModel);

        }
     
    }
}