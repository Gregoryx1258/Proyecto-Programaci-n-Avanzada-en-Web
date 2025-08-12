using System.Linq;
using System.Web.Mvc;
using Connect4.Database;
using System.Data.Entity;

namespace Connect4.Controllers
{
    public class PlayersController : Controller
    {
        private Connect4DBEntities db = new Connect4DBEntities();

        public ActionResult Index()
        {
            var jugadores = db.Players.ToList();
            return View(jugadores);
        }

        // Tu código: Crear jugador
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

        // Recalcula las estadísticas de todos los jugadores y las actualiza (ganados, perdidos, empates, puntaje).
        private void RecalcularEstadisticas()
        {
            var jugadores = db.Players.ToList();

            foreach (var jugador in jugadores)
            {
                int wins = db.Games.Count(g => g.WinnerId == jugador.Id && g.Status == "Finalizado");
                int losses = db.Games.Count(g => g.Status == "Finalizado" &&
                                    (g.Player1Id == jugador.Id || g.Player2Id == jugador.Id) &&
                                    g.WinnerId.HasValue && g.WinnerId != jugador.Id);
                int draws = db.Games.Count(g => g.Status == "Finalizado" &&
                                    !g.WinnerId.HasValue &&
                                    (g.Player1Id == jugador.Id || g.Player2Id == jugador.Id));

                //Una unidad positiva (+1) por cada partida ganada.
                //Una unidad negativa(-1) por cada partida perdida.
                //Una unidad nula(0) por cada partida empatada.
                int score = wins * 1 + losses * -1 + draws * 0;

                jugador.Wins = wins;
                jugador.Losses = losses;
                jugador.Draws = draws;
                jugador.Score = score;

                db.Entry(jugador).State = EntityState.Modified;
            }

            db.SaveChanges();
        }

        [HttpGet]
        public ActionResult RecalcularEstadisticasManual()
        {
            RecalcularEstadisticas();
            TempData["Success"] = "Estadísticas actualizadas correctamente";
            return RedirectToAction("Index");
        }

        public ActionResult Ping()
        {
            return Content("funciona");
        }
    }
}
