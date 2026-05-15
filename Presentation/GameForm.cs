
using PokemonGame.Application;
using PokemonGame.Core.Entities;
using PokemonGame.Core.Interfaces;
using PokemonGame.Core.Services;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;

namespace PokemonGame.Presentation
{
    // ── Estados del juego ─────────────────────────────────────────────────
    public enum GameState
    { Titulo, EntradaNombre, SeleccionStarter, Mundo, Batalla, Inventario, Equipo, Tienda }

    // ── Submenú de batalla ────────────────────────────────────────────────
    public enum BattleMenu { Principal, Movimientos, Items, Pokemon }

    /// <summary>
    /// Formulario principal del juego.
    /// SRP: solo renderiza y procesa input.
    /// DIP: usa IBattleService e IEncounterService, nunca los concretos directamente.
    /// </summary>
    public class GameForm : Form
    {
        // ══════════════════════════════════════════════════════════════════
        //  CONSTANTES DE CONFIGURACIÓN
        // ══════════════════════════════════════════════════════════════════

        private const int ANCHO      = 640;   // píxeles del formulario
        private const int ALTO       = 480;
        private const int TILE       = 32;    // tamaño de cada tile del mapa
        private const int MAP_W      = 20;    // columnas del mapa
        private const int MAP_H      = 15;    // filas del mapa
        private const int FPS        = 30;    // fotogramas por segundo

        // ══════════════════════════════════════════════════════════════════
        //  PALETA DE COLORES DEL JUEGO
        // ══════════════════════════════════════════════════════════════════

        // Colores de interfaz
        private static readonly Color CF       = Color.FromArgb(12,  20,  50);   // fondo oscuro
        private static readonly Color CP       = Color.FromArgb(18,  35,  85);   // panel
        private static readonly Color CPO      = Color.FromArgb(10,  20,  58);   // panel oscuro
        private static readonly Color CB       = Color.FromArgb(80,  140, 255);  // borde
        private static readonly Color CBS      = Color.FromArgb(255, 220, 50);   // borde selección
        private static readonly Color CT       = Color.White;                     // texto
        private static readonly Color CTS      = Color.FromArgb(180, 200, 255);  // texto secundario
        private static readonly Color CAM      = Color.FromArgb(255, 220, 50);   // amarillo
        private static readonly Color CVE      = Color.FromArgb(55,  220, 55);   // verde
        private static readonly Color CRO      = Color.FromArgb(215, 50,  50);   // rojo
        private static readonly Color CGR      = Color.FromArgb(130, 140, 160);  // gris

        // Colores de tiles: 0=camino  1=hierba  2=árbol  3=agua  4=tienda  5=centro
        private static readonly Color[] TILE_COLORS = {
            Color.FromArgb(172, 152, 92),   // 0 camino/arena
            Color.FromArgb(68,  158, 68),   // 1 hierba alta
            Color.FromArgb(28,  98,  28),   // 2 árbol
            Color.FromArgb(52,  112, 200),  // 3 agua/lago
            Color.FromArgb(195, 98,  48),   // 4 tienda
            Color.FromArgb(215, 75,  75),   // 5 centro Pokémon
        };

        // ══════════════════════════════════════════════════════════════════
        //  MAPA DEL MUNDO
        //  0=camino  1=hierba  2=árbol  3=agua  4=tienda  5=centro
        // ══════════════════════════════════════════════════════════════════

        private static readonly int[,] MAPA = {
            {2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2},
            {2,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,2},
            {2,0,0,0,2,2,0,0,1,1,1,1,0,0,2,2,0,0,0,2},
            {2,0,0,0,2,2,0,0,1,1,1,1,0,0,2,2,0,0,0,2},
            {2,0,0,0,0,0,0,0,1,1,1,1,0,0,0,0,0,0,0,2},
            {2,0,0,0,0,0,3,3,3,0,0,0,0,0,0,0,2,2,0,2},
            {2,0,0,2,2,0,3,3,3,0,0,0,0,0,0,0,2,2,0,2},
            {2,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,2},
            {2,0,0,0,0,0,0,0,0,0,1,1,1,1,0,0,0,0,0,2},
            {2,0,0,0,0,0,1,1,1,0,1,1,1,1,0,0,0,0,0,2},
            {2,0,0,0,0,0,1,1,1,0,0,0,0,0,0,0,0,0,0,2},
            {2,0,4,0,0,0,0,0,0,0,0,0,0,0,0,0,0,5,0,2},
            {2,0,0,0,0,0,0,2,2,2,2,2,0,0,0,0,0,0,0,2},
            {2,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,2},
            {2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2},
        };

        // ══════════════════════════════════════════════════════════════════
        //  ESTADO DEL JUEGO
        // ══════════════════════════════════════════════════════════════════

        private GameState  _estado     = GameState.Titulo;
        private BattleMenu _batMenu    = BattleMenu.Principal;
        private ITrainer?  _jugador;

        // ── Animación general ─────────────────────────────────────────────
        private int   _frame    = 0;
        private float _pbY      = 0f;    // posición Y de la Pokéball animada
        private float _pbDY     = -3f;   // velocidad Y de la Pokéball

        // ── Entrada de nombre ─────────────────────────────────────────────
        private string _nombre = "";

        // ── Selección de starter ──────────────────────────────────────────
        private int _starterIdx = 0;
        private static readonly (string Nombre, string Tipo, string Desc)[] STARTERS = {
            ("Bulbasaur",  "Planta  ", "Fuerte en defensa"),
            ("Charmander", "Fuego   ", "Atacante veloz"),
            ("Squirtle",   "Agua    ", "Equilibrado y sólido"),
        };

        // ── Movimiento del jugador en el mapa ─────────────────────────────
        private int   _jugX = 10, _jugY = 7;   // posición actual en grid
        private int   _dirX = 0,  _dirY = 1;   // dirección para el sprite
        private bool  _moviendose  = false;
        private float _movAvance   = 0f;
        private int   _destX, _destY;
        private int   _walkFrame   = 0;         // frame de animación de caminar

        // ── Batalla ───────────────────────────────────────────────────────
        private readonly List<string> _log       = new List<string>();
        private bool _batallaOK                  = false;  // terminó
        private bool _esperandoEvoDecision        = false;  // pregunta evolución
        private int  _batSelMov  = 0;
        private int  _batSelObj  = 0;
        private int  _batSelPoke = 0;

        // ── Paneles laterales ─────────────────────────────────────────────
        private int _invSel    = 0;
        private int _equipSel  = 0;
        private int _tiendaSel = 0;

        // ── Input ─────────────────────────────────────────────────────────
        private readonly HashSet<Keys> _held = new HashSet<Keys>();  // teclas sostenidas
        private readonly HashSet<Keys> _just = new HashSet<Keys>();  // recién presionadas

        // ── Servicios inyectados (DIP) ────────────────────────────────────
        private readonly IBattleService    _battleSvc;
        private readonly IEncounterService _encounterSvc;

        // ── Casos de uso ──────────────────────────────────────────────────
        private readonly IniciarJuegoUseCase   _ucIniciar  = new IniciarJuegoUseCase();
        private readonly UsarObjetoUseCase     _ucObjeto   = new UsarObjetoUseCase();
        private readonly GestionarEquipoUseCase _ucEquipo  = new GestionarEquipoUseCase();
        private readonly ComprarItemUseCase    _ucTienda   = new ComprarItemUseCase();

        // ── Canvas (panel con doble buffer para evitar parpadeo) ──────────
        private readonly Panel _canvas;

        // ── Timer de juego (nombre completo para evitar CS0104) ───────────
        // SOLUCIÓN CS0104: Usar el nombre completo System.Windows.Forms.Timer
        // en lugar de solo "Timer" para evitar ambigüedad.
        private readonly System.Windows.Forms.Timer _gameTimer;

        // ── Cache de fuentes (rendimiento) ────────────────────────────────
        private static readonly Font[] _fonts = new Font[60];

        // ══════════════════════════════════════════════════════════════════
        //  CONSTRUCTOR
        // ══════════════════════════════════════════════════════════════════

        public GameForm()
        {
            // DIP: se inyectan implementaciones concretas AQUÍ.
            // El resto del formulario trabaja solo con las interfaces.
            _battleSvc    = new BattleService();
            _encounterSvc = new EncounterService();

            // Configuración del formulario
            Text            = "Juego Pokémon – SOLID  |  Construcción de Software";
            ClientSize      = new Size(ANCHO, ALTO);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox     = false;
            BackColor       = Color.Black;
            StartPosition   = FormStartPosition.CenterScreen;

            // Panel con doble buffer para evitar parpadeo al renderizar
            _canvas = new Panel { Dock = DockStyle.Fill, BackColor = Color.Black };
            // Activar DoubleBuffered mediante reflexión (protegido en Panel)
            typeof(Panel).GetProperty("DoubleBuffered",
                System.Reflection.BindingFlags.NonPublic |
                System.Reflection.BindingFlags.Instance)
                ?.SetValue(_canvas, true);
            _canvas.Paint += OnPaint;
            Controls.Add(_canvas);

            // Input de teclado
            KeyDown  += (s, e) =>
            {
                if (!_held.Contains(e.KeyCode)) _just.Add(e.KeyCode);
                _held.Add(e.KeyCode);
            };
            KeyUp    += (s, e) => _held.Remove(e.KeyCode);
            KeyPress += OnKeyPress;

            // Timer principal del juego (nombre completo para evitar CS0104)
            _gameTimer = new System.Windows.Forms.Timer { Interval = 1000 / FPS };
            _gameTimer.Tick += OnTick;
            _gameTimer.Start();
        }

        // ══════════════════════════════════════════════════════════════════
        //  BUCLE PRINCIPAL DEL JUEGO
        // ══════════════════════════════════════════════════════════════════

        private void OnTick(object? sender, EventArgs e)
        {
            _frame++;

            // Actualizar según el estado actual
            switch (_estado)
            {
                case GameState.Titulo:            UpdateTitulo();    break;
                case GameState.SeleccionStarter:  UpdateStarter();   break;
                case GameState.Mundo:             UpdateMundo();     break;
                case GameState.Batalla:           UpdateBatalla();   break;
                case GameState.Inventario:        UpdateInventario();break;
                case GameState.Equipo:            UpdateEquipo();    break;
                case GameState.Tienda:            UpdateTienda();    break;
            }

            _canvas.Invalidate();  // solicitar repintado
            _just.Clear();         // limpiar teclas recién presionadas
        }

        // ── Atajo para verificar tecla recién presionada ──────────────────
        private bool JP(Keys k) => _just.Contains(k);

        // ══════════════════════════════════════════════════════════════════
        //  UPDATES POR ESTADO
        // ══════════════════════════════════════════════════════════════════

        private void UpdateTitulo()
        {
            // Animación de rebote de la Pokéball
            _pbY  += _pbDY;
            _pbDY += 0.5f;
            if (_pbY > 20f) { _pbY = 20f; _pbDY = -3f; }

            if (JP(Keys.Enter) || JP(Keys.Space))
                _estado = GameState.EntradaNombre;
        }

        private void UpdateStarter()
        {
            if (JP(Keys.Left)  || JP(Keys.A)) _starterIdx = (_starterIdx + 2) % 3;
            if (JP(Keys.Right) || JP(Keys.D)) _starterIdx = (_starterIdx + 1) % 3;

            if (JP(Keys.Enter))
            {
                _jugador = _ucIniciar.Ejecutar(_nombre, _starterIdx);
                _estado  = GameState.Mundo;
            }
        }

        private void UpdateMundo()
        {
            // Atajos de teclado para paneles
            if (JP(Keys.I))           { _estado = GameState.Inventario; _invSel   = 0; return; }
            if (JP(Keys.E))           { _estado = GameState.Equipo;     _equipSel = 0; return; }
            if (JP(Keys.F))           { IniciarBatallaRival(); return; }
            if (JP(Keys.T))           { AbrirTienda();         return; }

            // Movimiento con animación de interpolación
            if (_moviendose)
            {
                _movAvance += 0.25f;                     // velocidad de paso
                if ((_frame % 8) == 0) _walkFrame ^= 1;  // alternar frame de caminar
                if (_movAvance >= 1f)
                {
                    _jugX       = _destX;
                    _jugY       = _destY;
                    _moviendose = false;
                    _movAvance  = 0f;
                    OnJugadorPasoANuevaCelda();
                }
                return;
            }

            // Leer input de movimiento
            int dx = 0, dy = 0;
            if      (_held.Contains(Keys.W) || _held.Contains(Keys.Up))    dy = -1;
            else if (_held.Contains(Keys.S) || _held.Contains(Keys.Down))  dy =  1;
            else if (_held.Contains(Keys.A) || _held.Contains(Keys.Left))  dx = -1;
            else if (_held.Contains(Keys.D) || _held.Contains(Keys.Right)) dx =  1;

            if (dx != 0 || dy != 0)
            {
                _dirX = dx; _dirY = dy;  // actualizar dirección del sprite
                int nx = _jugX + dx, ny = _jugY + dy;

                // Verificar que la celda destino es transitable
                if (nx >= 0 && nx < MAP_W && ny >= 0 && ny < MAP_H
                    && MAPA[ny, nx] != 2   // árbol = bloqueado
                    && MAPA[ny, nx] != 3)  // agua  = bloqueado
                {
                    _destX = nx; _destY = ny;
                    _moviendose = true;
                }
            }
        }

        /// <summary>
        /// Se llama cuando el jugador completa un paso.
        /// Verifica eventos especiales del tile de destino.
        /// </summary>
        private void OnJugadorPasoANuevaCelda()
        {
            int tile = MAPA[_jugY, _jugX];

            if (tile == 4)  // tienda
                AbrirTienda();
            else if (_encounterSvc.VerificarEncuentro(tile))
                IniciarBatallaSalvaje();
        }

        private void AbrirTienda()
        {
            _estado = GameState.Tienda;
            _tiendaSel = 0;
        }

        private void UpdateBatalla()
        {
            if (_batallaOK)
            {
                // La batalla terminó: esperar ENTER para volver al mapa
                if (JP(Keys.Enter) || JP(Keys.Space))
                {
                    _estado    = GameState.Mundo;
                    _batallaOK = false;
                    _log.Clear();
                    _batMenu = BattleMenu.Principal;
                }
                return;
            }

            if (_esperandoEvoDecision)
            {
                // El jugador decide si evolucionarlo o no
                if (JP(Keys.S) || JP(Keys.Enter))
                {
                    _jugador!.Equipo.PrimerActivo()?.Evolucionar();
                    AgregarLog("¡Evolucionó exitosamente!");
                    _esperandoEvoDecision = false;
                }
                else if (JP(Keys.N))
                    _esperandoEvoDecision = false;
                return;
            }

            // Navegar el menú de batalla
            switch (_batMenu)
            {
                case BattleMenu.Principal:   UpdateBatMenuPrincipal();  break;
                case BattleMenu.Movimientos: UpdateBatMenuMovimientos(); break;
                case BattleMenu.Items:       UpdateBatMenuItems();       break;
                case BattleMenu.Pokemon:     UpdateBatMenuPokemon();     break;
            }
        }

        private void UpdateBatMenuPrincipal()
        {
            if (JP(Keys.D1) || JP(Keys.NumPad1)) { _batMenu = BattleMenu.Movimientos; _batSelMov = 0; }
            if (JP(Keys.D2) || JP(Keys.NumPad2)) { _batMenu = BattleMenu.Items;       _batSelObj = 0; }
            if (JP(Keys.D3) || JP(Keys.NumPad3)) { _batMenu = BattleMenu.Pokemon;     _batSelPoke= 0; }
            if (JP(Keys.D4) || JP(Keys.NumPad4)) _battleSvc.IntentarHuida();
            if (JP(Keys.D5) || JP(Keys.NumPad5)) _battleSvc.EjecutarCaptura();
        }

        private void UpdateBatMenuMovimientos()
        {
            var movs = _jugador!.Equipo.PrimerActivo()?.Movimientos;
            if (movs == null || movs.Count == 0) { _batMenu = BattleMenu.Principal; return; }

            if (JP(Keys.Up))    _batSelMov = Math.Max(0, _batSelMov - 1);
            if (JP(Keys.Down))  _batSelMov = Math.Min(movs.Count - 1, _batSelMov + 1);
            if (JP(Keys.Escape)){ _batMenu = BattleMenu.Principal; return; }
            if (JP(Keys.Enter)) { _battleSvc.EjecutarAtaque(_batSelMov); _batMenu = BattleMenu.Principal; }
        }

        private void UpdateBatMenuItems()
        {
            var items = _jugador!.Inventario.Items;
            if (JP(Keys.Escape)) { _batMenu = BattleMenu.Principal; return; }
            if (items.Count == 0){ _batMenu = BattleMenu.Principal; return; }

            if (JP(Keys.Up))   _batSelObj = Math.Max(0, _batSelObj - 1);
            if (JP(Keys.Down)) _batSelObj = Math.Min(items.Count - 1, _batSelObj + 1);
            if (JP(Keys.Enter))
            {
                _battleSvc.EjecutarUsoObjeto(_batSelObj, 0);
                _batMenu = BattleMenu.Principal;
            }
        }

        private void UpdateBatMenuPokemon()
        {
            var eq = _jugador!.Equipo.Equipo;
            if (JP(Keys.Escape)) { _batMenu = BattleMenu.Principal; return; }
            if (JP(Keys.Up))    _batSelPoke = Math.Max(0, _batSelPoke - 1);
            if (JP(Keys.Down))  _batSelPoke = Math.Min(eq.Count - 1, _batSelPoke + 1);
            if (JP(Keys.Enter))
            {
                if (!eq[_batSelPoke].Debilitado)
                    AgregarLog($"¡{eq[_batSelPoke].Nombre} al campo!");
                _batMenu = BattleMenu.Principal;
            }
        }

        private void UpdateInventario()
        {
            var inv = _jugador!.Inventario;
            if (JP(Keys.Escape) || JP(Keys.I))
            { _estado = GameState.Mundo; return; }
            if (!inv.EstaVacio)
            {
                if (JP(Keys.Up))   _invSel = Math.Max(0, _invSel - 1);
                if (JP(Keys.Down)) _invSel = Math.Min(inv.Cantidad - 1, _invSel + 1);
                if (JP(Keys.Enter))
                {
                    var poke = _jugador.Equipo.PrimerActivo();

                    if (poke == null)
                    {
                        poke = _jugador.Equipo.Equipo
                            .FirstOrDefault(p => p.Debilitado);
                    }

                    if (poke != null)
                        inv.Usar(_invSel, poke);

                    _invSel = Math.Min(_invSel, Math.Max(0, inv.Cantidad - 1));
                }
            }
         }
        

        private void UpdateEquipo()
        {
            if (JP(Keys.Escape) || JP(Keys.E)) { _estado = GameState.Mundo; return; }
            var eq = _jugador!.Equipo;
            if (JP(Keys.Up))   _equipSel = Math.Max(0, _equipSel - 1);
            if (JP(Keys.Down)) _equipSel = Math.Min(eq.Equipo.Count - 1, _equipSel + 1);
        }

        private void UpdateTienda()
        {
            var cat = ComprarItemUseCase.Catalogo;
            if (JP(Keys.Escape) || JP(Keys.T)) { _estado = GameState.Mundo; return; }
            if (JP(Keys.Up))   _tiendaSel = Math.Max(0, _tiendaSel - 1);
            if (JP(Keys.Down)) _tiendaSel = Math.Min(cat.Count - 1, _tiendaSel + 1);
            if (JP(Keys.Enter))
            {
                bool ok = _ucTienda.Ejecutar(_jugador!, _tiendaSel);
                // El resultado se verá en el dinero actualizado
            }
        }

        // ── Entrada de nombre (manejada por evento KeyPress para caracteres) ──
        private void OnKeyPress(object? sender, KeyPressEventArgs e)
        {
            if (_estado != GameState.EntradaNombre) return;

            if (e.KeyChar == '\b')       // Backspace
            {
                if (_nombre.Length > 0)
                    _nombre = _nombre.Substring(0, _nombre.Length - 1);
            }
            else if (e.KeyChar == '\r')  // Enter
            {
                if (_nombre.Length == 0) _nombre = "Ash";
                _estado = GameState.SeleccionStarter;
            }
            else if (_nombre.Length < 12 && !char.IsControl(e.KeyChar))
            {
                _nombre += e.KeyChar;
            }
        }

        // ══════════════════════════════════════════════════════════════════
        //  LÓGICA DE BATALLAS (delega a servicios)
        // ══════════════════════════════════════════════════════════════════

        private void IniciarBatallaSalvaje()
        {
            int nv      = _jugador!.Equipo.PrimerActivo()?.Nivel ?? 5;
            var salvaje = _encounterSvc.GenerarPokemonSalvaje(nv);
            var rival   = new Entrenador("Pokémon Salvaje");
            rival.CapturarPokemon(salvaje);
            PrepararBatalla(rival);
        }
        private bool mensajeSinPokemonMostrado = false;
        private void IniciarBatallaRival()
        {
            if (_jugador?.Equipo.PrimerActivo() == null)
            {
                if (!mensajeSinPokemonMostrado)
                {
                    mensajeSinPokemonMostrado = true;
                    MessageBox.Show("Primero debes revivir un Pokémon.");
                }

                return;
            }

            mensajeSinPokemonMostrado = false;

            int nv = _jugador.Equipo.PrimerActivo()!.Nivel;
            var gary = _encounterSvc.CrearRival(nv);
            PrepararBatalla(gary);
        }

        private void PrepararBatalla(ITrainer rival)
        {
            var miPoke = _jugador?.Equipo.PrimerActivo();

            if (miPoke == null)
            {
                MessageBox.Show("No tienes Pokémon disponibles. Ve a la tienda y usa un Revivir.");

                _estado = GameState.Mundo;
                _batallaOK = true;
                _log.Clear();

                return;
            }

            _log.Clear();
            _batallaOK = false;
            _esperandoEvoDecision = false;
            _batMenu = BattleMenu.Principal;

            // Suscribir eventos
            _battleSvc.OnLogMensaje += AgregarLog;
            _battleSvc.OnBatallaTerminada += OnBatallaTerminada;
            _battleSvc.OnExpGanada += OnExpGanada;
            _battleSvc.OnPokemonDebilitado += pk => { };

            _battleSvc.IniciarBatalla(_jugador!, rival);
            _estado = GameState.Batalla;
        }
        private void OnBatallaTerminada(bool gano)
        {
            _batallaOK = true;
            // Desuscribir eventos al terminar (evitar fugas de memoria)
            _battleSvc.OnLogMensaje       -= AgregarLog;
            _battleSvc.OnBatallaTerminada -= OnBatallaTerminada;
            _battleSvc.OnExpGanada        -= OnExpGanada;
        }

        private void OnExpGanada(IPokemon pk, bool subioNivel)
        {
            if (subioNivel && pk.PuedeEvolucionar)
                _esperandoEvoDecision = true;
        }

        private void AgregarLog(string msg)
        {
            _log.Add(msg);
            if (_log.Count > 5) _log.RemoveAt(0);
        }

        // ══════════════════════════════════════════════════════════════════
        //  RENDERIZADO PRINCIPAL
        // ══════════════════════════════════════════════════════════════════

        private void OnPaint(object? sender, PaintEventArgs e)
        {
            var g = e.Graphics;
            // Modo pixel-art: sin anti-aliasing para bordes nítidos
            g.SmoothingMode     = SmoothingMode.None;
            g.InterpolationMode = InterpolationMode.NearestNeighbor;
            g.PixelOffsetMode   = PixelOffsetMode.Half;
            g.Clear(CF);

            switch (_estado)
            {
                case GameState.Titulo:            DrawTitulo(g);    break;
                case GameState.EntradaNombre:     DrawNombre(g);    break;
                case GameState.SeleccionStarter:  DrawStarter(g);   break;
                case GameState.Mundo:             DrawMundo(g);     break;
                case GameState.Batalla:           DrawBatalla(g);   break;
                case GameState.Inventario: DrawMundo(g); DrawInventario(g); break;
                case GameState.Equipo:     DrawMundo(g); DrawEquipo(g);     break;
                case GameState.Tienda:     DrawMundo(g); DrawTienda(g);     break;
            }
        }

        // ── Pantalla de Título ────────────────────────────────────────────
        private void DrawTitulo(Graphics g)
        {
            // Degradado de fondo
            for (int y = 0; y < ALTO; y++)
            {
                float t = (float)y / ALTO;
                using (var br = new SolidBrush(Color.FromArgb(
                    (int)(12 + t * 40), (int)(20 + t * 30), (int)(55 + t * 130))))
                    g.FillRectangle(br, 0, y, ANCHO, 1);
            }

            // Pokéball animada con rebote
            DrawPokeball(g, ANCHO / 2 - 60, (int)(120 + _pbY), 120);

            // Títulos
            DrawSombraTexto(g, "POKÉMON", 52, ANCHO / 2, 270, CAM, true);
            DrawSombraTexto(g, "Construcción de Software", 14, ANCHO / 2, 332, CTS, true);
            DrawSombraTexto(g, "Principios SOLID aplicados", 11, ANCHO / 2, 352, CGR, true);

            // Parpadeo "Presiona ENTER"
            if ((_frame / 20) % 2 == 0)
                DrawTexto(g, "Presiona ENTER para comenzar", 12, ANCHO / 2, 385, CB, true);

            // Panel de controles
            DrawCaja(g, 18, 405, ANCHO - 36, 65, CPO, CB);
            DrawTexto(g, "WASD/Flechas: Mover    I: Inventario    E: Equipo    F: Retar rival    T: Tienda", 9, ANCHO/2, 420, CTS, true);
            DrawTexto(g, "En batalla: 1=Atacar  2=Objetos  3=Pokémon  4=Huir  5=Capturar", 9, ANCHO/2, 437, CTS, true);
            DrawTexto(g, "En submenús: ↑↓ seleccionar · ENTER confirmar · ESC volver", 9, ANCHO/2, 454, CGR, true);
        }

        private void DrawPokeball(Graphics g, int x, int y, int size)
        {
            int h = size / 2, r = Math.Max(4, size / 8);
            // Mitad roja
            using (var br = new SolidBrush(Color.FromArgb(210, 35, 35)))
                g.FillEllipse(br, x, y, size, size);
            // Mitad blanca
            using (var br = new SolidBrush(Color.White))
                g.FillRectangle(br, x, y + h, size, h + 2);
            // Línea central
            using (var p = new Pen(Color.Black, 3))
                g.DrawLine(p, x, y + h, x + size, y + h);
            // Botón central
            using (var br = new SolidBrush(Color.Black))
                g.FillEllipse(br, x + h - r, y + h - r, r * 2, r * 2);
            using (var br = new SolidBrush(Color.White))
                g.FillEllipse(br, x + h - r + 3, y + h - r + 3, r - 3, r - 3);
            // Borde
            using (var p = new Pen(Color.Black, 2))
                g.DrawEllipse(p, x, y, size, size);
        }

        // ── Pantalla de Nombre ────────────────────────────────────────────
        private void DrawNombre(Graphics g)
        {
            for (int y = 0; y < ALTO; y++)
            {
                float t = (float)y / ALTO;
                using (var br = new SolidBrush(Color.FromArgb(
                    (int)(12+t*30),(int)(20+t*25),(int)(55+t*100))))
                    g.FillRectangle(br, 0, y, ANCHO, 1);
            }

            DrawSombraTexto(g, "¿CÓMO TE LLAMAS?", 24, ANCHO / 2, 145, CAM, true);
            DrawCaja(g, ANCHO/2 - 165, 205, 330, 55, CP, CB, 2);
            string cursor = (_frame / 15) % 2 == 0 ? "█" : " ";
            DrawTexto(g, _nombre + cursor, 20, ANCHO / 2, 228, Color.White, true);
            DrawTexto(g, "Escribe tu nombre y presiona ENTER", 11, ANCHO / 2, 285, CGR, true);

            // Mostrar los 3 starters pequeños como preview
            Sprites.Dibujar(g, Sprites.Bulbasaur,  155, 340, 5);
            Sprites.Dibujar(g, Sprites.Charmander, 290, 340, 5);
            Sprites.Dibujar(g, Sprites.Squirtle,   425, 340, 5);
            DrawTexto(g, "Tu aventura está a punto de comenzar...", 10, ANCHO/2, 420, CGR, true);
        }

        // ── Selección de Starter ──────────────────────────────────────────
        private void DrawStarter(Graphics g)
        {
            for (int y = 0; y < ALTO; y++)
            {
                float t = (float)y / ALTO;
                using (var br = new SolidBrush(Color.FromArgb(
                    (int)(12+t*30),(int)(25+t*30),(int)(65+t*120))))
                    g.FillRectangle(br, 0, y, ANCHO, 1);
            }

            DrawSombraTexto(g, "¡ELIGE TU POKÉMON!", 22, ANCHO / 2, 28, CAM, true);
            DrawTexto(g, "← → para elegir   ENTER para confirmar", 11, ANCHO / 2, 62, CTS, true);

            string[][] spriteStarters = { Sprites.Bulbasaur, Sprites.Charmander, Sprites.Squirtle };

            for (int i = 0; i < 3; i++)
            {
                bool sel = (i == _starterIdx);
                int  cx  = 65 + i * 180;
                DrawCaja(g, cx, 88, 162, 285, sel ? Color.FromArgb(40,70,165) : CP,
                         sel ? CBS : CB, sel ? 3 : 1);

                // Animación suave del starter seleccionado (rebota levemente)
                int offsetY = (sel && (_frame / 15) % 2 == 0) ? -3 : 0;
                Sprites.Dibujar(g, spriteStarters[i], cx + 30, 105 + offsetY, 8);

                DrawSombraTexto(g, STARTERS[i].Nombre, 14, cx + 81, 267,
                    sel ? CAM : CT, true);
                DrawTexto(g, "Tipo: " + STARTERS[i].Tipo, 11, cx + 81, 286, CTS, true);
                DrawTexto(g, STARTERS[i].Desc, 10, cx + 81, 303, CGR, true);

                if (sel)
                {
                    DrawTexto(g, "▲", 14, cx + 81, 92,  CAM, true);
                    DrawTexto(g, "▼", 14, cx + 81, 368, CAM, true);
                }
            }
        }

        // ── Mapa del Mundo ────────────────────────────────────────────────
        private void DrawMundo(Graphics g)
        {
            // Dibujar tiles
            for (int ty = 0; ty < MAP_H; ty++)
            for (int tx = 0; tx < MAP_W; tx++)
            {
                int t = MAPA[ty, tx];
                Color baseColor = (t >= 0 && t < TILE_COLORS.Length)
                    ? TILE_COLORS[t] : TILE_COLORS[0];

                using (var br = new SolidBrush(baseColor))
                    g.FillRectangle(br, tx * TILE, ty * TILE, TILE, TILE);

                // Detalles visuales según tipo de tile
                switch (t)
                {
                    case 1: // hierba alta – brillo de hojas
                        using (var br2 = new SolidBrush(Color.FromArgb(95, 195, 58)))
                        {
                            g.FillRectangle(br2, tx*TILE+2,  ty*TILE+2,  3, 9);
                            g.FillRectangle(br2, tx*TILE+10, ty*TILE+5,  3, 8);
                            g.FillRectangle(br2, tx*TILE+20, ty*TILE+2,  3, 9);
                            g.FillRectangle(br2, tx*TILE+27, ty*TILE+7,  2, 7);
                        }
                        break;
                    case 2: // árbol – sombra interna + brillo
                        using (var br2 = new SolidBrush(Color.FromArgb(18, 78, 18)))
                            g.FillRectangle(br2, tx*TILE+5, ty*TILE+5, TILE-10, TILE-10);
                        using (var br3 = new SolidBrush(Color.FromArgb(75, 165, 48)))
                        {
                            g.FillRectangle(br3, tx*TILE+3,  ty*TILE+3,  7, 7);
                            g.FillRectangle(br3, tx*TILE+19, ty*TILE+17, 6, 6);
                        }
                        break;
                    case 3: // agua – ondas animadas
                        if ((_frame / 14) % 2 == (tx + ty) % 2)
                            using (var br2 = new SolidBrush(Color.FromArgb(75, 138, 225)))
                            {
                                g.FillRectangle(br2, tx*TILE+3,  ty*TILE+8,  11, 3);
                                g.FillRectangle(br2, tx*TILE+17, ty*TILE+20, 11, 3);
                            }
                        break;
                    case 4: // tienda
                        DrawTexto(g, "TIENDA", 7, tx*TILE+TILE/2, ty*TILE+TILE/2, Color.White, true);
                        break;
                    case 5: // centro Pokémon
                        DrawTexto(g, "CENTRO", 7, tx*TILE+TILE/2, ty*TILE+TILE/2, Color.White, true);
                        break;
                }
            }

            // Dibujar jugador con interpolación de movimiento suave
            float px = _moviendose ? _jugX + (_destX - _jugX) * _movAvance : _jugX;
            float py = _moviendose ? _jugY + (_destY - _jugY) * _movAvance : _jugY;

            // Seleccionar sprite según dirección
            string[] sprJug = (_dirX == -1) ? Sprites.JugadorIzquierda :
                   (_dirX == 1) ? Sprites.JugadorDerecha :
                   (_dirY == -1) ? Sprites.JugadorEspalda :
                                   Sprites.JugadorFrente;

            Sprites.Dibujar(g, sprJug,
                (int)(px * TILE + 4),
                (int)(py * TILE - 2), 4);

            // HUD del mundo
            DrawHUDMundo(g);
        }

        private void DrawHUDMundo(Graphics g)
        {
            // Barra superior semitransparente
            using (var br = new SolidBrush(Color.FromArgb(190, 10, 18, 58)))
                g.FillRectangle(br, 0, 0, ANCHO, 30);
            using (var p = new Pen(CB, 1))
                g.DrawLine(p, 0, 30, ANCHO, 30);

            if (_jugador != null)
            {
                DrawTexto(g, $"★ {_jugador.Nombre}", 12, 8, 8, CAM);
                DrawTexto(g, $"${_jugador.Dinero}",  12, 168, 8, CVE);

                var pk = _jugador.Equipo.PrimerActivo();
                if (pk != null)
                {
                    DrawTexto(g, $"{pk.Nombre} Nv.{pk.Nivel}", 12, 295, 8, CT);
                    DrawBarraHP(g, 450, 10, 120, 11, pk.PorcentajeVida);
                    DrawTexto(g, $"{pk.VidaActual}/{pk.VidaMax}", 9, 578, 11, CTS);
                }
            }

            // Barra inferior con controles
            using (var br = new SolidBrush(Color.FromArgb(190, 10, 18, 58)))
                g.FillRectangle(br, 0, ALTO - 22, ANCHO, 22);
            using (var p = new Pen(CB, 1))
                g.DrawLine(p, 0, ALTO - 22, ANCHO, ALTO - 22);
            DrawTexto(g, "WASD=Mover  I=Inventario  E=Equipo  F=Retar rival  T=Tienda", 9, ANCHO/2, ALTO - 14, CGR, true);
        }

        // ── Pantalla de Batalla ───────────────────────────────────────────
        private void DrawBatalla(Graphics g)
        {
            // Cielo con degradado
            using (var br = new LinearGradientBrush(
                new Rectangle(0, 0, ANCHO, ALTO / 2 + 30),
                Color.FromArgb(78, 138, 218), Color.FromArgb(158, 198, 238), 90f))
                g.FillRectangle(br, 0, 0, ANCHO, ALTO / 2 + 30);

            // Suelo
            using (var br = new SolidBrush(Color.FromArgb(115, 188, 68)))
                g.FillRectangle(br, 0, ALTO / 2, ANCHO, ALTO / 2);
            using (var br = new SolidBrush(Color.FromArgb(85, 158, 48)))
                g.FillRectangle(br, 0, ALTO / 2, ANCHO, 16);

            var rPoke = _battleSvc.Rival?.Equipo.PrimerActivo();
            var jPoke = _jugador?.Equipo.PrimerActivo();

            // ── Info del rival (arriba izquierda) ─────────────────────────
            if (rPoke != null)
            {
                DrawCaja(g, 8, 8, 298, 82, Color.FromArgb(195, 18, 35, 85), CB);
                DrawSombraTexto(g, rPoke.Nombre, 16, 18, 18, CT);
                DrawTexto(g, $"Nv.{rPoke.Nivel}  [{rPoke.Tipo}]", 10, 18, 38, CTS);
                DrawBarraHP(g, 18, 56, 225, 14, rPoke.PorcentajeVida);
                DrawTexto(g, $"HP {rPoke.VidaActual}/{rPoke.VidaMax}", 9, 252, 58, CTS);

                // Sprite del rival (grande, arriba derecha)
                var sprR = Sprites.ObtenerSprite(rPoke.Nombre);
                Sprites.Dibujar(g, sprR, 388, 48, 10, true);
            }

            // ── Info del jugador (abajo derecha) ─────────────────────────
            if (jPoke != null)
            {
                DrawCaja(g, 338, 228, 298, 95, Color.FromArgb(195, 18, 35, 85), CB);
                DrawSombraTexto(g, jPoke.Nombre, 16, 348, 238, CAM);
                DrawTexto(g, $"Nv.{jPoke.Nivel}  [{jPoke.Tipo}]", 10, 348, 258, CTS);
                DrawBarraHP(g, 348, 278, 225, 14, jPoke.PorcentajeVida);
                DrawTexto(g, $"HP {jPoke.VidaActual}/{jPoke.VidaMax}", 9, 582, 278, CTS);

                // Barra de EXP
                float expPct = jPoke.ExpParaSiguienteNivel > 0
                    ? (float)jPoke.Exp / jPoke.ExpParaSiguienteNivel : 0f;
                using (var br = new SolidBrush(Color.FromArgb(45, 45, 45)))
                    g.FillRectangle(br, 348, 300, 225, 7);
                using (var br = new SolidBrush(Color.FromArgb(45, 95, 220)))
                    g.FillRectangle(br, 348, 300, (int)(225 * expPct), 7);
                DrawTexto(g, "EXP", 8, 348, 310, CGR);

                // Sprite del jugador (izquierda del campo)
                var sprJ = Sprites.ObtenerSprite(jPoke.Nombre);
                Sprites.Dibujar(g, sprJ, 52, 195, 10);
            }

            // ── Log de batalla ────────────────────────────────────────────
            DrawCaja(g, 8, 328, 448, 115, Color.FromArgb(205, CPO.R, CPO.G, CPO.B), CB);
            for (int i = 0; i < _log.Count; i++)
                DrawTexto(g, _log[i], 10, 16, 336 + i * 20, CT);

            // ── Menú de acción ────────────────────────────────────────────
            DrawMenuBatalla(g, jPoke);
        }

        private void DrawMenuBatalla(Graphics g, IPokemon? jPoke)
        {
            if (_batallaOK)
            {
                DrawCaja(g, 460, 440, 175, 35, CP, CBS);
                DrawTexto(g, "ENTER = Continuar", 10, 547, 456, CAM, true);
                return;
            }
            if (_esperandoEvoDecision)
            {
                DrawCaja(g, 455, 330, 182, 88, CP, CBS, 2);
                DrawTexto(g, "¡Puede evolucionar!", 11, 546, 342, CAM, true);
                DrawTexto(g, "[S/ENTER] Evolucionar", 11, 546, 362, CVE, true);
                DrawTexto(g, "[N] No evolucionar",    11, 546, 382, CGR, true);
                return;
            }

            switch (_batMenu)
            {
                case BattleMenu.Principal:
                    DrawCaja(g, 460, 330, 175, 112, CP, CB);
                    string[] ops = { "1. ATACAR", "2. OBJETOS", "3. POKÉMON", "4. HUIR", "5. CAPTURAR" };
                    for (int i = 0; i < ops.Length; i++)
                        DrawTexto(g, ops[i], 12, 470, 340 + i * 20, CT);
                    break;

                case BattleMenu.Movimientos:
                    if (jPoke == null) return;
                    var movs = jPoke.Movimientos;
                    DrawCaja(g, 455, 295, 182, 155, CP, CBS, 2);
                    DrawTexto(g, "► Movimientos:", 12, 463, 300, CAM);
                    for (int i = 0; i < movs.Count; i++)
                    {
                        bool s = (i == _batSelMov);
                        DrawTexto(g, (s ? "► " : "  ") + movs[i].Nombre, 11,
                            463, 320 + i * 22, s ? CAM : CT);
                        DrawTexto(g, $"P{movs[i].Poder}", 9, 598, 320 + i * 22, CGR);
                    }
                    DrawTexto(g, "↑↓ · ENTER · ESC", 9, 463, 438, CGR);
                    break;

                case BattleMenu.Items:
                    var items = _jugador!.Inventario.Items;
                    DrawCaja(g, 455, 265, 182, 185, CP, CBS, 2);
                    DrawTexto(g, "► Inventario:", 12, 463, 270, CAM);
                    if (items.Count == 0)
                        DrawTexto(g, "(vacío)", 11, 463, 295, CGR);
                    else
                        for (int i = 0; i < items.Count && i < 7; i++)
                        {
                            bool s = (i == _batSelObj);
                            DrawTexto(g, (s?"►":" ") + items[i].Nombre, 11,
                                463, 292 + i * 20, s ? CAM : CT);
                        }
                    DrawTexto(g, "↑↓ · ENTER · ESC", 9, 463, 440, CGR);
                    break;

                case BattleMenu.Pokemon:
                    var eq = _jugador!.Equipo.Equipo;
                    DrawCaja(g, 455, 265, 182, 185, CP, CBS, 2);
                    DrawTexto(g, "► Equipo:", 12, 463, 270, CAM);
                    for (int i = 0; i < eq.Count; i++)
                    {
                        bool s = (i == _batSelPoke);
                        Color c = eq[i].Debilitado ? CGR : (s ? CAM : CT);
                        DrawTexto(g, (s?"►":" ") + eq[i].Nombre, 11, 463, 292+i*22, c);
                        if (!eq[i].Debilitado)
                            DrawBarraHP(g, 570, 294+i*22, 60, 8, eq[i].PorcentajeVida);
                    }
                    DrawTexto(g, "↑↓ · ENTER · ESC", 9, 463, 440, CGR);
                    break;
            }
        }

        // ── Inventario (panel superpuesto al mapa) ────────────────────────
        private void DrawInventario(Graphics g)
        {
            DrawCaja(g, 68, 42, 508, 398, Color.FromArgb(228, 15, 28, 78), CB, 2);
            DrawSombraTexto(g, "INVENTARIO", 20, ANCHO / 2, 55, CAM, true);
            DrawTexto(g, "↑↓ seleccionar   ENTER usar en Pokémon activo   ESC cerrar", 10, ANCHO/2, 80, CGR, true);

            var inv = _jugador!.Inventario;
            if (inv.EstaVacio)
            {
                DrawTexto(g, "(Inventario vacío – ve a la tienda)", 14, ANCHO / 2, 210, CGR, true);
                return;
            }
            for (int i = 0; i < inv.Cantidad; i++)
            {
                bool sel = (i == _invSel);
                DrawCaja(g, 88, 98 + i * 52, 468, 46,
                    sel ? Color.FromArgb(48, 78, 178) : Color.FromArgb(20, 35, 102),
                    sel ? CBS : CB);
                var item = inv.Items[i];
                DrawTexto(g, (sel ? "► " : "   ") + item.Nombre, 14, 102, 112+i*52,
                    sel ? CAM : CT);
                DrawTexto(g, $"${item.Precio}", 12, 498, 112+i*52, CGR);
            }
        }

        // ── Equipo (panel superpuesto al mapa) ────────────────────────────
        private void DrawEquipo(Graphics g)
        {
            DrawCaja(g, 48, 32, 548, 418, Color.FromArgb(228, 15, 28, 78), CB, 2);
            DrawSombraTexto(g, "EQUIPO POKÉMON", 20, ANCHO / 2, 45, CAM, true);
            DrawTexto(g, "↑↓ ver detalles   ESC cerrar", 10, ANCHO/2, 70, CGR, true);

            var eq = _jugador!.Equipo.Equipo;
            for (int i = 0; i < eq.Count; i++)
            {
                bool sel = (i == _equipSel);
                var  pk  = eq[i];
                DrawCaja(g, 66, 92 + i * 58, 512, 52,
                    sel ? Color.FromArgb(48, 78, 162) : Color.FromArgb(20, 35, 92),
                    pk.Debilitado ? CRO : (sel ? CBS : CB), sel ? 2 : 1);

                // Mini sprite del Pokémon
                var spr = Sprites.ObtenerSprite(pk.Nombre);
                Sprites.Dibujar(g, spr, 72, 96 + i * 58, 3);

                DrawTexto(g, pk.Nombre, 13, 130, 98 + i * 58, sel ? CAM : CT);
                DrawTexto(g, $"Tipo:{pk.Tipo}  Nv.{pk.Nivel}", 10, 130, 114+i*58, CTS);

                if (pk.Debilitado)
                    DrawTexto(g, "DEBILITADO", 10, 325, 98+i*58, CRO);
                else
                {
                    DrawBarraHP(g, 325, 102+i*58, 165, 12, pk.PorcentajeVida);
                    DrawTexto(g, $"{pk.VidaActual}/{pk.VidaMax}", 9, 498, 102+i*58, CGR);
                }

                DrawTexto(g, $"Atk:{pk.Ataque} Def:{pk.Defensa} Vel:{pk.Velocidad}",
                    9, 520, 98+i*58, CGR);

                if (pk.PuedeEvolucionar)
                    DrawTexto(g, $"★ Evoluciona a Nv.{pk.NivelEvolucion}: {pk.NombreEvolucion}",
                        9, 130, 128+i*58, CVE);
            }
        }

        // ── Tienda (panel superpuesto al mapa) ────────────────────────────
        private void DrawTienda(Graphics g)
        {
            DrawCaja(g, 68, 42, 508, 398, Color.FromArgb(228, 15, 28, 78), CB, 2);
            DrawSombraTexto(g, "TIENDA POKÉMON", 20, ANCHO / 2, 55, CAM, true);
            DrawTexto(g, $"Tu dinero: ${_jugador!.Dinero}", 12, ANCHO/2, 80, CVE, true);
            DrawTexto(g, "↑↓ seleccionar   ENTER comprar   ESC cerrar", 10, ANCHO/2, 98, CGR, true);

            var cat = ComprarItemUseCase.Catalogo;
            for (int i = 0; i < cat.Count; i++)
            {
                bool sel = (i == _tiendaSel);
                DrawCaja(g, 88, 110 + i * 52, 468, 46,
                    sel ? Color.FromArgb(48, 78, 178) : Color.FromArgb(20, 35, 102),
                    sel ? CBS : CB);
                DrawTexto(g, (sel ? "► " : "   ") + cat[i].Nombre, 14, 102, 124+i*52,
                    sel ? CAM : CT);
                DrawTexto(g, $"${cat[i].Precio}", 13, 498, 124+i*52, CVE);

                if (sel && _jugador.Dinero < cat[i].Precio)
                    DrawTexto(g, "¡Sin dinero suficiente!", 9, 102, 140+i*52, CRO);
            }
        }

        // ══════════════════════════════════════════════════════════════════
        //  HELPERS DE RENDERIZADO  (SRP: solo ayudan a dibujar)
        // ══════════════════════════════════════════════════════════════════

        private static Font GetFont(int size)
        {
            if (size < 1 || size >= _fonts.Length) size = 12;
            if (_fonts[size] == null)
                _fonts[size] = new Font("Consolas", size, FontStyle.Bold, GraphicsUnit.Pixel);
            return _fonts[size];
        }

        private static void DrawTexto(Graphics g, string txt, int size,
            int x, int y, Color color, bool centrado = false)
        {
            var f = GetFont(size);
            using (var br = new SolidBrush(color))
            {
                if (centrado)
                {
                    var sf = new StringFormat { Alignment = StringAlignment.Center };
                    g.DrawString(txt, f, br, x, y, sf);
                }
                else g.DrawString(txt, f, br, x, y);
            }
        }

        private static void DrawSombraTexto(Graphics g, string txt, int size,
            int x, int y, Color color, bool centrado = false)
        {
            // Sombra desplazada 2 píxeles
            using (var s = new SolidBrush(Color.FromArgb(85, 0, 0, 0)))
            {
                var f = GetFont(size);
                if (centrado)
                {
                    var sf = new StringFormat { Alignment = StringAlignment.Center };
                    g.DrawString(txt, f, s, x + 2, y + 2, sf);
                }
                else g.DrawString(txt, f, s, x + 2, y + 2);
            }
            DrawTexto(g, txt, size, x, y, color, centrado);
        }

        private static void DrawCaja(Graphics g, int x, int y, int w, int h,
            Color fondo, Color borde, int grosor = 1)
        {
            using (var br = new SolidBrush(fondo))
                g.FillRectangle(br, x, y, w, h);
            if (grosor > 0)
                using (var p = new Pen(borde, grosor))
                    g.DrawRectangle(p, x, y, w, h);
        }

        private static void DrawBarraHP(Graphics g, int x, int y, int w, int h,
            float porcentaje)
        {
            // Fondo oscuro
            using (var br = new SolidBrush(Color.FromArgb(55, 55, 55)))
                g.FillRectangle(br, x, y, w, h);

            // Barra coloreada según HP restante
            Color barColor = porcentaje > 0.5f ? CVE :
                             porcentaje > 0.2f ? CAM : CRO;
            int bw = Math.Max(0, (int)(w * porcentaje));
            if (bw > 0)
                using (var br = new SolidBrush(barColor))
                    g.FillRectangle(br, x, y, bw, h);

            // Borde
            using (var p = new Pen(Color.FromArgb(40, 40, 40), 1))
                g.DrawRectangle(p, x, y, w, h);
        }

        // ══════════════════════════════════════════════════════════════════
        //  DISPOSE
        // ══════════════════════════════════════════════════════════════════
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _gameTimer.Stop();
                _gameTimer.Dispose();
                // Liberar fuentes en cache
                foreach (var f in _fonts)
                    f?.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
