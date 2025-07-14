using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Connect4.Database;

namespace Connect4.Controllers
{
    public class HomeController : Controller
    {
        private Connect4DBEntities2 db = new Connect4DBEntities2();

        public ActionResult Index()
        {
            ViewBag.TotalJugadores = db.Players.Count();
            ViewBag.TotalPartidas = db.Games.Count();

            return View();
        }

  
    }
}
