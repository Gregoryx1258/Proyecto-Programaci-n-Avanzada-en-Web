using System;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using Connect4.Database;

namespace Connect4.Controllers
{
    public class GamesController : Controller
    {
        private Connect4DBEntities2 db = new Connect4DBEntities2();

        public GamesController()
        {
            db.Configuration.ProxyCreationEnabled = false;
        }

        // GET: Games
        public ActionResult Index()
        {
            var games = db.Games
                .Include(g => g.Players1)
                .Include(g => g.Players2)
                .Include(g => g.Players3)
                .ToList();

            return View(games);
        }

        // GET: Games/Create
        public ActionResult Create()
        {
            ViewBag.Player1Id = new SelectList(db.Players, "Id", "Name");
            ViewBag.Player2Id = new SelectList(db.Players, "Id", "Name");
            return View();
        }

        // POST: Games/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Games game)
        {
            if (ModelState.IsValid)
            {
                game.GridJson = new string('0', 42);
                game.CurrentTurnId = game.Player1Id;
                game.Status = "En progreso";
                game.CreatedAt = DateTime.Now;

                db.Games.Add(game);
                db.SaveChanges();

                return RedirectToAction("Board", new { id = game.Id });
            }

            ViewBag.Player1Id = new SelectList(db.Players, "Id", "Name", game.Player1Id);
            ViewBag.Player2Id = new SelectList(db.Players, "Id", "Name", game.Player2Id);
            return View(game);
        }

        // GET: Games/Board/5
        public ActionResult Board(int id)
        {
            var game = db.Games
                .Include(g => g.Players1)
                .Include(g => g.Players2)
                .Include(g => g.Players3)
                .FirstOrDefault(g => g.Id == id);

            if (game == null)
                return HttpNotFound();

            return View(game);
        }

        // POST: Games/Play
        [HttpPost]
        public ActionResult Play(int id, int column)
        {
            var game = db.Games.Find(id);
            if (game == null || game.Status != "En progreso")
                return HttpNotFound();

            var grid = game.GridJson.ToCharArray();
            char ficha = game.CurrentTurnId == game.Player1Id ? '1' : '2';
            bool jugadaExitosa = false;

            for (int row = 0; row < 6; row++)
            {
                int index = row * 7 + column;
                if (grid[index] == '0')
                {
                    grid[index] = ficha;
                    jugadaExitosa = true;
                    break;
                }
            }

            if (!jugadaExitosa)
            {
                TempData["Error"] = "Esta columna está llena. Elige otra.";
                return RedirectToAction("Board", new { id });
            }

            game.GridJson = new string(grid);

            // Verificar si hay un ganador
            bool hayGanador = CheckWinner(game.GridJson, ficha);
            bool isBoardFull = !game.GridJson.Contains('0');

            if (hayGanador)
            {
                game.Status = "Finalizado";
                game.WinnerId = (ficha == '1') ? game.Player1Id : game.Player2Id;
            }
            else if (isBoardFull) // Tablero lleno = empate
            {
                game.Status = "Finalizado";
                game.WinnerId = null; // Empate
            }
            else
            {
                // Cambiar turno
                game.CurrentTurnId = (game.CurrentTurnId == game.Player1Id) ? game.Player2Id : game.Player1Id;
            }

            
            db.Entry(game).State = EntityState.Modified;
            db.SaveChanges();

            // Recalcular estadísticas
            if (game.Status == "Finalizado")
            {
                RecalcularEstadisticas();
            }

            return RedirectToAction("Board", new { id });
        }

        // Lógica para verificar si hay 4 en línea
        private bool CheckWinner(string grid, char ficha)
        {
            int rows = 6, cols = 7;

            for (int row = 0; row < rows; row++)
            {
                for (int col = 0; col < cols; col++)
                {
                    int idx = row * cols + col;

                    // Horizontal
                    if (col <= cols - 4 &&
                        grid[idx] == ficha &&
                        grid[idx + 1] == ficha &&
                        grid[idx + 2] == ficha &&
                        grid[idx + 3] == ficha)
                        return true;

                    // Vertical
                    if (row <= rows - 4 &&
                        grid[idx] == ficha &&
                        grid[idx + cols] == ficha &&
                        grid[idx + 2 * cols] == ficha &&
                        grid[idx + 3 * cols] == ficha)
                        return true;

                    // Diagonal derecha
                    if (row <= rows - 4 && col <= cols - 4 &&
                        grid[idx] == ficha &&
                        grid[idx + cols + 1] == ficha &&
                        grid[idx + 2 * (cols + 1)] == ficha &&
                        grid[idx + 3 * (cols + 1)] == ficha)
                        return true;

                    // Diagonal izquierda
                    if (row <= rows - 4 && col >= 3 &&
                        grid[idx] == ficha &&
                        grid[idx + cols - 1] == ficha &&
                        grid[idx + 2 * (cols - 1)] == ficha &&
                        grid[idx + 3 * (cols - 1)] == ficha)
                        return true;
                }
            }
            return false;
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
            return RedirectToAction("Index", "Players"); 
        }
    }
}
