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
    }
}
