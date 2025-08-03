using System.Linq;
using System.Web.Mvc;
using Connect4.Database;

namespace Connect4.Controllers
{
    public class PlayersController : Controller
    {
        private Connect4DBEntities3 db = new Connect4DBEntities3();

        public ActionResult Index()
        {
            var jugadores = db.Players.ToList();
            return View(jugadores);
        }

        public ActionResult CreatePlayer()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreatePlayer(Players player)
        {
            if (ModelState.IsValid)
            {
                player.Score = 0;
                player.Wins = 0;
                player.Losses = 0;
                player.Draws = 0;
                db.Players.Add(player);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(player);
        }



    }

}
