using System.Linq;
using System.Web.Mvc;
using Connect4.Database;
using System.Data.Entity;

namespace Connect4.Controllers
{
    public class PlayersController : Controller
    {
        private Connect4DBEntities2 db = new Connect4DBEntities2();

        public ActionResult Index()
        {
            var jugadores = db.Players.ToList();
            return View(jugadores);
        }

        // Recalcular estadísticas de todos los jugadores
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
                int score = wins * 5 + draws * 2;

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
