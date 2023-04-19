using Project.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.IO;
using System.Data.Entity.Validation;

namespace Project.Controllers
{
    public class AdminController : Controller
    {
        MonarchyEntities Registerentity = new MonarchyEntities();
        public ActionResult Authenticate()
        {
            string email = Request.Form["email"];
            string password = Request.Form["password"];
            var pass = from p in Registerentity.Admins where p.admin_name == email select p.admin_password;
            if (email == "" || password == "")
            {
                ViewBag.error = "errorAdmin";
                return View("~/Views/Home/Login.cshtml");
            }

            if (pass.Count() == 0)
            {
                ViewBag.error = "errorAdmin";
                return View("~/Views/Home/Login.cshtml");
            }
            else
            {
                foreach (var data in pass)
                {
                    if (password == data)
                    {
                        Session["email"] = email;
                        var details = from x in Registerentity.Admins where x.admin_name == email select x.admin_password;
                        Session["id"] = details.FirstOrDefault();
                        return RedirectToAction("");
                    }
                    else
                    {
                        ViewBag.error = "errorAdmin";
                        return View("~/Views/Home/Login.cshtml");
                    }
                }
            }

            return View("Login");
        }

        [HttpGet]
        public ActionResult Index(string a)
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

            List<object> modelList = new List<object>();
            modelList.Add(Registerentity.Categories.ToList());
            modelList.Add(Registerentity.Developers.ToList());
            modelList.Add(Registerentity.Users.ToList());


            ViewBag.developer = "Add a New Developer";
            return View(modelList);
        }

        [HttpGet]
        public ActionResult Product()
        {
            Session["dev"] = Registerentity.Developers.Select(x => new SelectListItem { Text = x.Developer_name, Value = x.Developer_Id.ToString() }).ToList();
            Session["cat"] = Registerentity.Categories.Select(x => new SelectListItem { Text = x.Category_name, Value = x.Category_Id.ToString() }).ToList();
        
            return View();
        }
        [HttpPost]
        public ActionResult Product(Game game, HttpPostedFileBase fileBase, string Developer_name, string Category_name)
        {
            if (ModelState.IsValid)
            {

                var gameName = Registerentity.Games.Where(e => e.Game_Name == game.Game_Name);
                if (gameName.Count() == 0)
                {
                    if (fileBase != null)
                    {
                        string path = Server.MapPath("~/Content/img/products/");
                        string fileName = Path.GetFileName(fileBase.FileName);
                        if (!Directory.Exists(path))
                        {
                            Directory.CreateDirectory(path);
                        }
                        fileBase.SaveAs(path + fileName);
                        game.Game_Image = ("~/Content/img/products/" + fileName);

                    }

                    game.Developer_Id = int.Parse(Developer_name);
                    game.Category_Id = int.Parse(Category_name);
                    Registerentity.Games.Add(game);
                    Registerentity.SaveChanges();
                    ViewBag.msg = "successfully Added Game!";
                    ModelState.Clear();
                    Session["dev"] = Registerentity.Developers.Select(x => new SelectListItem { Text = x.Developer_name, Value = x.Developer_Id.ToString() }).ToList();
                    Session["cat"] = Registerentity.Categories.Select(x => new SelectListItem { Text = x.Category_name, Value = x.Category_Id.ToString() }).ToList();
                    return View();
                }
                else
                {
                    ViewBag.error = "error";
                    ModelState.Clear();
                    Session["dev"] = Registerentity.Developers.Select(x => new SelectListItem { Text = x.Developer_name, Value = x.Developer_Id.ToString() }).ToList();
                    Session["cat"] = Registerentity.Categories.Select(x => new SelectListItem { Text = x.Category_name, Value = x.Category_Id.ToString() }).ToList();
                    return View();
                }

            }
            else
            {
                Session["dev"] = Registerentity.Developers.Select(x => new SelectListItem { Text = x.Developer_name, Value = x.Developer_Id.ToString() }).ToList();
                Session["cat"] = Registerentity.Categories.Select(x => new SelectListItem { Text = x.Category_name, Value = x.Category_Id.ToString() }).ToList();
                return View();
            }

        }

        public ActionResult EditProducts(int id)
        {
            Session["dev"] = Registerentity.Developers.Select(x => new SelectListItem { Text = x.Developer_name, Value = x.Developer_Id.ToString() }).ToList();
            Session["cat"] = Registerentity.Categories.Select(x => new SelectListItem { Text = x.Category_name, Value = x.Category_Id.ToString() }).ToList();
            var data = Registerentity.Games.Find(id);
            return View(data);
        }
        [HttpPost]
        public ActionResult EditProducts(int id, Game game)
        {
            Session["dev"] = Registerentity.Developers.Select(x => new SelectListItem { Text = x.Developer_name, Value = x.Developer_Id.ToString() }).ToList();
            Session["cat"] = Registerentity.Categories.Select(x => new SelectListItem { Text = x.Category_name, Value = x.Category_Id.ToString() }).ToList();
            var data = Registerentity.Games.Find(id);
            if (data != null)
            {
                data.Game_Name = game.Game_Name;
                data.Price = game.Price;
                data.RAM = game.RAM;
                data.VGA = game.VGA;
                data.CPU = game.CPU;
                data.Developer = game.Developer;
                data.Category = game.Category;
                data.Disk_Space = game.Disk_Space;
                Registerentity.SaveChanges();
                return RedirectToAction("Tables", "Admin");

            }
            else
            {
                return RedirectToAction("Tables", "Admin");
            }
        }
        public ActionResult DeleteProducts(int id)
        {
            var data = Registerentity.Games.Find(id);
            return View(data);
        }
        [HttpPost]
        public ActionResult DeleteProducts(int id, Game game)
        {
            var data = Registerentity.Games.Find(id);
            if (data != null)
            {
                Registerentity.Games.Remove(data);
                Registerentity.SaveChanges();
                return RedirectToAction("Tables");
            }
            else
            {
                return HttpNotFound();
            }
        }
        public ActionResult Category()
        {
            return View();
        }
        [HttpPost]
        public ActionResult Category(string catname)
        {
            Category cat = new Category();
            cat.Category_name = catname;
            Registerentity.Categories.Add(cat);
            Registerentity.SaveChanges();
            return RedirectToAction("Index", cat);
        }
        [HttpGet]
        public ActionResult Developer()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Developer(string devname)
        {
            Developer dev = new Developer();
            dev.Developer_name = devname;
            Registerentity.Developers.Add(dev);
            Registerentity.SaveChanges();
            return RedirectToAction("Index", dev);
        }
        [HttpGet]
        public ActionResult Tables()
        {
            List<object> modelList = new List<object>();
            modelList.Add(Registerentity.Categories.ToList());
            modelList.Add(Registerentity.Developers.ToList());
            modelList.Add(Registerentity.Games.ToList());
            return View(modelList);
        }
        [HttpGet]
        public ActionResult DeleteDeveloper(int id)
        {
            var data = Registerentity.Developers.Find(id);
            if (data != null)
            {
                Registerentity.Developers.Remove(data);
                Registerentity.SaveChanges();
                return RedirectToAction("Tables");
            }
            else
            {
                return HttpNotFound();
            }
        }

        public ActionResult EditDeveloper(int id)
        {
            var data = Registerentity.Developers.Find(id);
            return View(data);
        }
        [HttpPost]
        public ActionResult EditDeveloper(int id, Developer dev)
        {
            var data = Registerentity.Developers.Find(id);
            if (data != null)
            {
                data.Developer_name = dev.Developer_name;
                Registerentity.SaveChanges();
                return RedirectToAction("Tables", "Admin");

            }
            else
            {
                return RedirectToAction("Tables", "Admin");
            }
        }

        public ActionResult DeveloperDetails(int id)
        {
            var data = Registerentity.Developers.Find(id);
            return View(data);
        }

        public ActionResult DeleteCategory(int id)
        {
            var data = Registerentity.Categories.Find(id);
            if (data != null)
            {
                Registerentity.Categories.Remove(data);
                Registerentity.SaveChanges();
                return RedirectToAction("Tables");
            }
            else
            {
                return HttpNotFound();
            }
        }

        public ActionResult CategoryDetails(int id)
        {
            var data = Registerentity.Categories.Find(id);
            return View(data);
        }

        public ActionResult EditCategory(int id)
        {
            var data = Registerentity.Categories.Find(id);
            return View(data);
        }
        [HttpPost]
        public ActionResult EditCategory(int id, Category cat)
        {
            var data = Registerentity.Categories.Find(id);
            if (data != null)
            {
                data.Category_name = cat.Category_name;
                Registerentity.SaveChanges();
                return RedirectToAction("Tables", "Admin");

            }
            else
            {
                return RedirectToAction("Tables", "Admin");
            }
        }

        public ActionResult CustomerDetails(int id)
        {
            //var data = Registerentity.Users.Find(name);
            var data = Registerentity.Users.FirstOrDefault(x => x.User_id == id);
            return View(data);
        }

        public ActionResult OrderDetails()
        {
            var data = Registerentity.OrderDetails.ToList();
            return View(data);
        }
    }
}