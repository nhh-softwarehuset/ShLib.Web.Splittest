using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using SHLib.Web.SplitTest;

namespace Sample.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            if (SplitTestContainer.Instance.Test("test", "test1", "test2") == "test1")
            {
                ViewBag.Hello = "Hello from test1";
            }
            else
            {
                ViewBag.Hello = "We Say Hello From Test2";
            }
            SplitTestContainer.Instance.ForceSave();;
            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
    }
}