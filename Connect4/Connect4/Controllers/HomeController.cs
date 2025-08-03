using System;
using System.Linq;
using System.Web.Mvc;
using Connect4.Database;

namespace Connect4.Controllers
{
    public class HomeController : Controller
    {
        private readonly Connect4DBEntities3 db = new Connect4DBEntities3();

        public ActionResult Index()
        {
            try
            {
                ViewBag.TotalJugadores = db.Players.Count();
                ViewBag.TotalPartidas = db.Games.Count();
            }
            catch (Exception ex)
            {
                // En producción puedes registrar este error en logs
                ViewBag.TotalJugadores = 0;
                ViewBag.TotalPartidas = 0;
                ViewBag.Error = "Ocurrió un error al cargar los datos: " + ex.Message;
            }

            return View();
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
