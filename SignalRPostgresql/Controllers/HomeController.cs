using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace SignalRPostgresql.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Tickets()
        {
            return View();
        }

        public ActionResult Information()
        {
            return View();
        }
    }
}