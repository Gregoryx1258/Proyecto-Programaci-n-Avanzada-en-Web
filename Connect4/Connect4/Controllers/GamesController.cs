using Connect4.Database;
using System;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using System.Web.Services.Description;

namespace Connect4.Controllers
{
    public class GamesController : Controller
    {
        private readonly Connect4DBEntities db = new Connect4DBEntities();

        public GamesController()
        {
            db.Configuration.ProxyCreationEnabled = false;
        }

        // GET: Games
        //Lista de partidas con el nombre de los jugadores ordenadas por fecha descendente
        public ActionResult Index()
        {
            var games = db.Games
                .Include(g => g.Players1)
                .Include(g => g.Players2)
                .Include(g => g.Players3)
                .OrderByDescending(g => g.CreatedAt)
                .ToList();
            //Visualizar la lista
            return View(games);
        }

        // GET: Games/Create
        //Formulario para crear una nueva partida
        public ActionResult Create()
        {
            var jugadores = db.Players.ToList();
            //Se hace un select del dropdown desde la base de datos
            ViewBag.Player1Id = new SelectList(jugadores, "Id", "Name");
            ViewBag.Player2Id = new SelectList(jugadores, "Id", "Name");
            return View();
        }

        // POST: Games/Create
        //Se procesa la creacion de una nueva partida
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Games game)
        {
            //Validacion del modelo
            if (ModelState.IsValid)
            {
                //GridJson: tablero vacio (en 0)
                game.GridJson = new string('0', 42);
                game.CurrentTurnId = game.Player1Id;
                game.Status = "En progreso";
                game.CreatedAt = DateTime.Now;

                db.Games.Add(game);
                db.SaveChanges();

                return RedirectToAction("Board", new { id = game.Id });
            }

            //Si hay errores de validación, vuelve a mostrar el formulario.
            var jugadores = db.Players.ToList();
            ViewBag.Player1Id = new SelectList(jugadores, "Id", "Name", game.Player1Id);
            ViewBag.Player2Id = new SelectList(jugadores, "Id", "Name", game.Player2Id);
            return View(game);
        }

        // GET: Games/Board/5
        //Muestra el tablero de la partida
        public ActionResult Board(int? id)
        {
            if (!id.HasValue)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var game = db.Games
                .Include(g => g.Players1)
                .Include(g => g.Players2)
                .Include(g => g.Players3)
                .FirstOrDefault(g => g.Id == id.Value);

            if (game == null)
                return HttpNotFound();

            return View(game);
        }

        // POST: Games/Play
        //Acciones del jugador
        [HttpPost]
        public ActionResult Play(int id, int column)
        {
            //Busca la partida actual
            var game = db.Games.Find(id);
            if (game == null || game.Status != "En progreso")
                return HttpNotFound();

            //Seleccionar turnos y la ficha del jugador 
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

            //Columna llena de fichas
            if (!jugadaExitosa)
            {
                TempData["Error"] = "Esta columna está llena. Elige otra.";
                return RedirectToAction("Board", new { id });
            }

            game.GridJson = new string(grid);
            bool hayGanador = CheckWinner(game.GridJson, ficha);
            bool tableroLleno = !game.GridJson.Contains('0');

            //Si hay ganador, marca la partida como finalizada y asigna el ganador.
            if (hayGanador)
            {
                game.Status = "Finalizado";
                game.WinnerId = ficha == '1' ? game.Player1Id : game.Player2Id;
            }
            //Si el tablero está lleno, marca la partida como empate.
            else if (tableroLleno)
            {
                game.Status = "Finalizado";
                game.WinnerId = null;
            }
            else
            //Si no, cambia el turno al otro jugador.
            {
                game.CurrentTurnId = game.CurrentTurnId == game.Player1Id ? game.Player2Id : game.Player1Id;
            }

            db.Entry(game).State = EntityState.Modified;
            //Guarda los cambios y, si la partida terminó, recalcula estadísticas.
            db.SaveChanges();

            if (game.Status == "Finalizado")
            {
                RecalcularEstadisticas();
            }

            //Redirige al tablero actualizado.
            return RedirectToAction("Board", new { id });
        }

        // Lógica para verificar si hay cuatro fichas en línea
        private bool CheckWinner(string grid, char ficha)
        {
            int rows = 6, cols = 7;

            for (int row = 0; row < rows; row++)
            {
                for (int col = 0; col < cols; col++)
                {
                    int idx = row * cols + col;
                    
                    if (col <= cols - 4 &&
                        grid[idx] == ficha && grid[idx + 1] == ficha &&
                        grid[idx + 2] == ficha && grid[idx + 3] == ficha)
                        return true;

                    
                    if (row <= rows - 4 &&
                        grid[idx] == ficha && grid[idx + cols] == ficha &&
                        grid[idx + 2 * cols] == ficha && grid[idx + 3 * cols] == ficha)
                        return true;
                    
                    if (row <= rows - 4 && col <= cols - 4 &&
                        grid[idx] == ficha && grid[idx + cols + 1] == ficha &&
                        grid[idx + 2 * (cols + 1)] == ficha && grid[idx + 3 * (cols + 1)] == ficha)
                        return true;

                    if (row <= rows - 4 && col >= 3 &&
                        grid[idx] == ficha && grid[idx + cols - 1] == ficha &&
                        grid[idx + 2 * (cols - 1)] == ficha && grid[idx + 3 * (cols - 1)] == ficha)
                        return true;
                }
            }
            return false;
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

        [HttpPost]
        //Finaliza la partida actual (si no está finalizada) y crea una nueva partida con los mismos jugadores
        public ActionResult RestartGame(int id)
        {
            //Busca la partida actual
            var partidaActual = db.Games.Find(id);
            if (partidaActual == null)
                return HttpNotFound();

            //•	Si no está finalizada, la marca como finalizada
            if (partidaActual.Status != "Finalizado")
            {
                partidaActual.Status = "Finalizado";
                db.Entry(partidaActual).State = EntityState.Modified;
                db.SaveChanges();
            }

            //Crea una nueva partida con los mismos jugadores y tablero vacío
            var nuevaPartida = new Games
            {
                Player1Id = partidaActual.Player1Id,
                Player2Id = partidaActual.Player2Id,
                GridJson = new string('0', 42),
                CurrentTurnId = partidaActual.Player1Id,
                Status = "En progreso",
                CreatedAt = DateTime.Now
            };

            db.Games.Add(nuevaPartida);
            db.SaveChanges();

            return RedirectToAction("Board", new { id = nuevaPartida.Id });
        }

        [HttpGet]
        //Guarda la nueva partida y redirige al tablero de la nueva partida
        public ActionResult RecalcularEstadisticasManual()
        {
            RecalcularEstadisticas();
            TempData["Success"] = "Estadísticas actualizadas correctamente";
            return RedirectToAction("Index", "Players"); 
        }

        //Medodo que se encarga de liberar correctamente los recursos utilizados por el objeto db  y cerrar la conexión a la base de datos
        protected override void Dispose(bool disposing)
        {
            if (disposing)
                db.Dispose();
            base.Dispose(disposing);
        }
    }
}
