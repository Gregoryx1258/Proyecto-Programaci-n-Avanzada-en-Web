# Proyecto-Programaci-n-Avanzada-en-Web
Heymmy Massiel López Juárez
FI22023936

Correo Git: hlopez68900@ufide.ac.cr
Usuario Git: HeymmyLJuarez

Allan Alfredo Rodriguez Estrada
FI22026319

Correo Git: arodriguez70214@ufide.ac.cr
Usuario Git: alrodriufide

Frameworks y herramientas utilizadas
-ASP.NET MVC 5 (C#)
-Entity Framework 6 (Database-First)
-SQL Server (motor de base de datos relacional)
-Visual Studio 2022 (IDE)
-.NET Framework 4.8

Tipo de aplicación: MPA (Multi-Page Application)

Arquitectura utilizada: MVC (Modelo-Vista-Controlador)

Diagrama de la base de datos (Mermaid)

    PLAYERS {
        int Id PK
        int Identification
        string Name
        int? Score
        int? Wins
        int? Losses
        int? Draws
    }
    GAMES {
        int Id PK
        int Player1Id FK
        int Player2Id FK
        string GridJson
        int CurrentTurnId
        string Status
        int? WinnerId FK
        datetime CreatedAt
    }
    PLAYERS ||--o{ GAMES : "Player1Id"
    PLAYERS ||--o{ GAMES : "Player2Id"
    PLAYERS ||--o{ GAMES : "WinnerId"
	
Instructivo de instalación, compilación y ejecución

Crear un proyecto MVC en Visual Studio 2022
1.	Ir a Archivo > Nuevo > Proyecto.
2.	Buscar y seleccionar ASP.NET Web Application
3.	Asignarle un nombre y ubicación al proyecto.
4.	En la siguiente ventana, seleccionar MVC y hacer clic en Crear.
---
Instalar Entity Framework
1.	Hacer clic derecho en el proyecto > Administrar paquetes NuGet.
2.	Buscar EntityFramework
3.	Instálarlo
---
Configurar la base de datos
1.	Hacer clic derecho en la carpeta Models o donde prefieras > Agregar > Nuevo elemento.
2.	Seleccionar ADO.NET Entity Data Model.
4.	Elegir EF Designer from database y seguir el asistente para conectar la base de datos y generar los modelos.
4.	Crear una clase que herede de DbContext.
5.	Configurar la cadena de conexión en Web.config.
---
Compilar el proyecto
Menú: Compilar > Compilar solución (o presionar Ctrl+Shift+B).
---
Ejecutar la aplicación
Presionar F5 o hacer clic en Iniciar depuración.


## PROMPTS IA

====================================================
1) ¿Cómo puedo crear la lógica para un tablero de Connect4 en ASP.NET MVC que permita a los jugadores hacer clic en las columnas y que las fichas caigan en la celda vacía más baja, actualizando la base de datos y la vista en cada turno?
----------------------------------------------------
Respuesta (vista + controlador):

[Board.cshtml] – Botones por columna que envían el índice de la columna al controlador:
----------------------------------------------------
@* Dentro de <main> ... *@
@if (!juegoFinalizado)
{
    using (Html.BeginForm("Play", "Games", FormMethod.Post))
    {
        @Html.Hidden("id", Model.Id)
        <table>
            <thead>
                <tr>
                    @for (int i = 0; i < 7; i++)
                    {
                        <th>
                            <button type="submit" name="column" value="@i">@columnas[i]</button>
                        </th>
                    }
                </tr>
            </thead>
            <tbody>
                @for (int row = 5; row >= 0; row--)
                {
                    <tr>
                        @for (int col = 0; col < 7; col++)
                        {
                            char cell = grid[row * 7 + col];
                            string ficha = cell == '1' ? "🟡" : cell == '2' ? "🔴" : "⬜";
                            <td>@ficha</td>
                        }
                    </tr>
                }
            </tbody>
        </table>
    }
}
----------------------------------------------------

[GamesController.cs] – Acción Play: recibe la columna, deja “caer” la ficha a la celda vacía inferior y persiste:
----------------------------------------------------
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

    bool hayGanador = CheckWinner(game.GridJson, ficha);
    bool tableroLleno = !game.GridJson.Contains('0');

    if (hayGanador)
    {
        game.Status = "Finalizado";
        game.WinnerId = ficha == '1' ? game.Player1Id : game.Player2Id;
    }
    else if (tableroLleno)
    {
        game.Status = "Finalizado";
        game.WinnerId = null;
    }
    else
    {
        game.CurrentTurnId = game.CurrentTurnId == game.Player1Id ? game.Player2Id : game.Player1Id;
    }

    db.Entry(game).State = EntityState.Modified;
    db.SaveChanges();

    if (game.Status == "Finalizado")
    {
        RecalcularEstadisticas();
    }

    return RedirectToAction("Board", new { id });
}
----------------------------------------------------


====================================================
2) ¿Cómo implemento la lógica para detectar si hay un ganador en Connect4 considerando 4 en línea en horizontal, vertical y ambas diagonales, y actualizar la vista en consecuencia?
----------------------------------------------------
Respuesta (método en el controlador + uso al jugar):

[GamesController.cs] – Método CheckWinner con horizontales, verticales, diagonal ↘ y diagonal ↙:
----------------------------------------------------
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
                grid[idx] == ficha && grid[idx + 1] == ficha &&
                grid[idx + 2] == ficha && grid[idx + 3] == ficha)
                return true;

            // Vertical
            if (row <= rows - 4 &&
                grid[idx] == ficha && grid[idx + cols] == ficha &&
                grid[idx + 2 * cols] == ficha && grid[idx + 3 * cols] == ficha)
                return true;

            // Diagonal derecha (↘)
            if (row <= rows - 4 && col <= cols - 4 &&
                grid[idx] == ficha && grid[idx + cols + 1] == ficha &&
                grid[idx + 2 * (cols + 1)] == ficha && grid[idx + 3 * (cols + 1)] == ficha)
                return true;

            // Diagonal izquierda (↙)
            if (row <= rows - 4 && col >= 3 &&
                grid[idx] == ficha && grid[idx + cols - 1] == ficha &&
                grid[idx + 2 * (cols - 1)] == ficha && grid[idx + 3 * (cols - 1)] == ficha)
                return true;
        }
    }
    return false;
}
----------------------------------------------------

[GamesController.cs] – Uso del método dentro de Play (ver bloque anterior), donde si hay ganador se marca Status = "Finalizado" y se asigna WinnerId.


====================================================
3) ¿Cómo puedo mostrar en la vista si una partida está en curso o empatada, de forma diferenciada, sin que aparezca "Empate o en curso", sino exactamente uno u otro según el estado real del juego?
----------------------------------------------------
Respuesta (vista Board y vista Index de Games):

[Board.cshtml] – En el encabezado de la partida mostramos: si Status == "Finalizado" y WinnerId es null => Empate; si WinnerId tiene valor => ganador; si no está finalizada => “Turno de …”
----------------------------------------------------
@if (juegoFinalizado)
{
    string ganador = Model.WinnerId.HasValue
        ? (Model.WinnerId == Model.Player1Id ? jugador1 :
           Model.WinnerId == Model.Player2Id ? jugador2 : "Empate")
        : "Empate";

    if (ganador == "Empate")
    {
        <h3>Resultado: Empate</h3>
    }
    else
    {
        <h3><i class="bi bi-trophy-fill trophy-icon"></i>El ganador es: @ganador</h3>
    }
}
else
{
    string turno = Model.CurrentTurnId == Model.Player1Id ? jugador1 : jugador2;
    <h3>Turno de: @turno</h3>
}
----------------------------------------------------

[Views/Games/Index.cshtml] – En la tabla, calculamos el texto del estado (Empate, En curso o nombre del ganador) sin usar “Empate o en curso”
----------------------------------------------------
@foreach (var game in Model)
{
    string jugador1 = game.Players1?.Name ?? "Jugador 1";
    string jugador2 = game.Players2?.Name ?? "Jugador 2";
    string ganador;

    if (game.Status == "Finalizado" && game.WinnerId.HasValue)
    {
        ganador = game.WinnerId == game.Player1Id ? jugador1 :
                  game.WinnerId == game.Player2Id ? jugador2 : "Desconocido";
    }
    else if (game.Status == "Finalizado" && !game.WinnerId.HasValue)
    {
        ganador = "Empate";
    }
    else
    {
        ganador = "En curso";
    }

    <tr>
        <td>@jugador1</td>
        <td>@jugador2</td>
        <td>@game.Status</td>
        <td>@ganador</td>
        <td>
            @Html.ActionLink("Ver", "Board", new { id = game.Id }, new { @class = "btn-view" })
        </td>
    </tr>
}
----------------------------------------------------


====================================================
4) ¿Cómo puedo crear una funcionalidad que reinicie la partida actual en ASP.NET MVC, conservando los mismos jugadores pero generando un nuevo registro de partida con una cuadrícula vacía?
----------------------------------------------------
Respuesta (controlador + vista):

[GamesController.cs] – Acción RestartGame:
----------------------------------------------------
[HttpPost]
public ActionResult RestartGame(int id)
{
    var partidaActual = db.Games.Find(id);
    if (partidaActual == null)
        return HttpNotFound();

    if (partidaActual.Status != "Finalizado")
    {
        partidaActual.Status = "Finalizado";
        db.Entry(partidaActual).State = EntityState.Modified;
        db.SaveChanges();
    }

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
----------------------------------------------------

[Board.cshtml] – Botón que llama a RestartGame:
----------------------------------------------------
<form method="post" action="@Url.Action("RestartGame", "Games", new { id = Model.Id })">
    <button type="submit" class="btn btn-warning">
        @(juegoFinalizado ? "Nueva partida" : "Reiniciar partida")
    </button>
</form>
----------------------------------------------------


====================================================
5) ¿Cómo puedo recalcular y actualizar las estadísticas de cada jugador (ganadas, perdidas, empatadas y marcador) cada vez que una partida termina en ASP.NET MVC con Entity Framework?
----------------------------------------------------
Respuesta (método en el controlador y llamado al finalizar):

[GamesController.cs] – RecalcularEstadisticas y llamado al finalizar una partida:
----------------------------------------------------
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
----------------------------------------------------

[GamesController.cs] – Dentro de Play, tras guardar, si terminó la partida, recalcular:
----------------------------------------------------
db.Entry(game).State = EntityState.Modified;
db.SaveChanges();

if (game.Status == "Finalizado")
{
    RecalcularEstadisticas();
}
----------------------------------------------------


====================================================
6) ¿Cómo puedo mostrar las fichas de Connect4 como íconos circulares (amarillo, rojo y celda vacía) en una tabla HTML y aplicar estilos CSS modernos con colores y sombreado personalizados?
----------------------------------------------------
Respuesta (vista Board + CSS local):

[Board.cshtml] – Construcción visual con emojis/íconos + estilos de tabla con sombra y bordes redondeados:
----------------------------------------------------
<table>
    <thead>
        <tr>
            @for (int i = 0; i < 7; i++)
            {
                <th>
                    <button type="submit" name="column" value="@i">@columnas[i]</button>
                </th>
            }
        </tr>
    </thead>
    <tbody>
        @for (int row = 5; row >= 0; row--)
        {
            <tr>
                @for (int col = 0; col < 7; col++)
                {
                    char cell = grid[row * 7 + col];
                    string ficha = cell == '1' ? "🟡" : cell == '2' ? "🔴" : "⬜";
                    <td>@ficha</td>
                }
            </tr>
        }
    </tbody>
</table>

<style>
table {
    margin: auto;
    background: #fff;
    border-collapse: collapse;
    color: black;
    border-radius: 10px;
    overflow: hidden;
    box-shadow: 0 4px 10px rgba(0, 0, 0, 0.3);
}
th, td {
    width: 60px;
    height: 60px;
    font-size: 32px;
    text-align: center;
    border: 1px solid #ddd;
}
</style>
----------------------------------------------------


====================================================
7) ¿Cómo puedo alternar correctamente el turno entre jugador 1 y jugador 2 después de cada movimiento?
----------------------------------------------------
Respuesta (controlador Play: toggle de CurrentTurnId si no hay ganador ni empate):
----------------------------------------------------
else
{
    game.CurrentTurnId = game.CurrentTurnId == game.Player1Id ? game.Player2Id : game.Player1Id;
}
----------------------------------------------------


====================================================
8) ¿Cómo puedo verificar si un jugador ha ganado en Connect4 utilizando lógica en el controlador?
----------------------------------------------------
Respuesta (llamada a CheckWinner dentro de Play y actualización de estado/ganador):
----------------------------------------------------
game.GridJson = new string(grid);
bool hayGanador = CheckWinner(game.GridJson, ficha);
bool tableroLleno = !game.GridJson.Contains('0');

if (hayGanador)
{
    game.Status = "Finalizado";
    game.WinnerId = ficha == '1' ? game.Player1Id : game.Player2Id;
}
else if (tableroLleno)
{
    game.Status = "Finalizado";
    game.WinnerId = null;
}
else
{
    game.CurrentTurnId = game.CurrentTurnId == game.Player1Id ? game.Player2Id : game.Player1Id;
}
----------------------------------------------------
