// ╔══════════════════════════════════════════════════════════════════════════╗
// ║  CAPA: Core › Services                                                  ║
// ║  CLASE: BattleService : IBattleService                                  ║
// ║  SOLID:                                                                 ║
// ║    SRP – Solo responsable del motor de combate por turnos               ║
// ║    DIP – Depende de IPokemon/ITrainer (interfaces), no concretos        ║
// ╚══════════════════════════════════════════════════════════════════════════╝

using PokemonGame.Core.Interfaces;
using System;

namespace PokemonGame.Core.Services
{
    /// <summary>
    /// Motor de combate por turnos.
    /// Reglas:
    ///   - El Pokémon con mayor Velocidad ataca primero.
    ///   - Al derrotar al rival, el jugador gana EXP.
    ///   - Si todos los Pokémon del jugador caen, pierde.
    ///   - Usa eventos C# para notificar a la capa de presentación (Observer).
    /// </summary>
    public sealed class BattleService : IBattleService
    {
        // ── Estado ───────────────────────────────────────────────────────
        public bool EnCurso { get; private set; }
        public bool JugadorGano { get; private set; }
        public ITrainer Jugador { get; private set; } = null!;
        public ITrainer Rival { get; private set; } = null!;

        private static readonly Random _rng = new();

        // ── Eventos (Observer – DIP): la UI se suscribe sin acoplamientos ─
        public event Action<string>? OnLogMensaje;
        public event Action<bool>? OnBatallaTerminada;
        public event Action<IPokemon, bool>? OnExpGanada;
        public event Action<IPokemon>? OnPokemonDebilitado;

        // ── API pública ──────────────────────────────────────────────────

        public bool IniciarBatalla(ITrainer jugador, ITrainer rival)
        {
            if (jugador.Equipo.PrimerActivo() == null) return false;
            if (rival.Equipo.PrimerActivo() == null) return false;

            Jugador = jugador;
            Rival = rival;
            EnCurso = true;
            JugadorGano = false;

            Log($"¡Un {rival.Equipo.PrimerActivo()!.Nombre} apareció!");
            return true;
        }

        public void EjecutarAtaque(int idxMovimiento)
        {
            if (!EnCurso) return;

            var miPoke = Jugador.Equipo.PrimerActivo();
            var suPoke = Rival.Equipo.PrimerActivo();
            if (miPoke == null || suPoke == null) return;

            var movimientos = miPoke.Movimientos;
            if (idxMovimiento < 0 || idxMovimiento >= movimientos.Count) return;

            // Determinar orden por velocidad
            if (miPoke.Velocidad >= suPoke.Velocidad)
            {
                RealizarAtaque(miPoke, suPoke, movimientos[idxMovimiento], Rival);
                if (EnCurso && !suPoke.Debilitado) TurnoIA(suPoke, miPoke);
            }
            else
            {
                TurnoIA(suPoke, miPoke);
                if (EnCurso && !miPoke.Debilitado)
                    RealizarAtaque(miPoke, suPoke, movimientos[idxMovimiento], Rival);
            }
        }

        public bool EjecutarCaptura()
        {
            if (!EnCurso) return false;

            var idxBalls = Jugador.Inventario.IndicesPokeballs();
            if (idxBalls.Count == 0)
            {
                Log("¡No tienes Pokéballs!");
                return false;
            }

            var ball = Jugador.Inventario.Obtener(idxBalls[0]);
            var objetivo = Rival.Equipo.PrimerActivo()!;
            bool ok = ball!.Usar(objetivo);
            Jugador.Inventario.Remover(idxBalls[0]);

            if (ok)
            {
                Log($"¡{objetivo.Nombre} fue capturado!");
                Jugador.CapturarPokemon(objetivo);
                TerminarBatalla(true);
                return true;
            }

            Log($"¡{objetivo.Nombre} rompió la Pokéball!");
            // La IA ataca tras el fallo de captura
            var suPoke = Rival.Equipo.PrimerActivo();
            var miPoke = Jugador.Equipo.PrimerActivo();
            if (suPoke != null && miPoke != null) TurnoIA(suPoke, miPoke);
            return false;
        }

        public void EjecutarUsoObjeto(int idxObjeto, int idxPokemon)
        {
            if (!EnCurso) return;

            bool ok = Jugador.UsarObjeto(idxObjeto, idxPokemon);
            Log(ok ? "¡Objeto usado con éxito!" : "No se pudo usar el objeto.");

            // La IA ataca igualmente
            var suPoke = Rival.Equipo.PrimerActivo();
            var miPoke = Jugador.Equipo.PrimerActivo();
            if (suPoke != null && miPoke != null) TurnoIA(suPoke, miPoke);
        }

        public bool IntentarHuida()
        {
            if (!EnCurso) return false;
            bool exito = _rng.Next(2) == 0;
            if (exito)
            {
                Log("¡Huiste con éxito!");
                TerminarBatalla(false);
            }
            else
            {
                Log("¡No pudiste huir!");
                var su = Rival.Equipo.PrimerActivo();
                var yo = Jugador.Equipo.PrimerActivo();
                if (su != null && yo != null) TurnoIA(su, yo);
            }
            return exito;
        }

        // ── Lógica interna ───────────────────────────────────────────────

        private void RealizarAtaque(IPokemon atacante, IPokemon objetivo,
            Movimiento mov, ITrainer dueñoObjetivo)
        {
            if (atacante.Debilitado || objetivo.Debilitado) return;

            int dano = CalcularDano(atacante, mov, objetivo);
            objetivo.AplicarDano(dano);
            Log($"¡{atacante.Nombre} usó {mov.Nombre}!  → {dano} de daño.");

            if (objetivo.Debilitado)
                ManejarKO(objetivo, atacante, dueñoObjetivo);
        }

        private void TurnoIA(IPokemon atacante, IPokemon objetivo)
        {
            if (atacante.Debilitado || objetivo.Debilitado) return;
            if (atacante.Movimientos.Count == 0) return;

            var mov = atacante.Movimientos[_rng.Next(atacante.Movimientos.Count)];
            int dano = CalcularDano(atacante, mov, objetivo);
            objetivo.AplicarDano(dano);
            Log($"¡{atacante.Nombre} usó {mov.Nombre}!  → {dano} de daño.");

            if (objetivo.Debilitado)
                ManejarKO(objetivo, atacante, Jugador);
        }

        private void ManejarKO(IPokemon ko, IPokemon ganador, ITrainer dueñoKO)
        {
            OnPokemonDebilitado?.Invoke(ko);
            Log($"✗ ¡{ko.Nombre} fue debilitado!");

            // EXP solo cuando el rival cae
            if (dueñoKO == Rival)
            {
                int exp = ko.Nivel * 6 + 10;
                bool subioNivel = ganador.GanarExp(exp);
                Log($"★ {ganador.Nombre} ganó {exp} EXP." +
                    (subioNivel ? $" ¡Subió al nivel {ganador.Nivel}!" : ""));
                OnExpGanada?.Invoke(ganador, subioNivel);
            }

            // ¿Acabó la batalla?
            if (Jugador.Equipo.TodosDebilitados)
            {
                TerminarBatalla(false);
            }
            else if (Rival.Equipo.TodosDebilitados)
            {
                int premio = 80 + _rng.Next(120);
                Jugador.GanarDinero(premio);
                Log($"★ ¡Ganaste! Recibiste ${premio}.");
                TerminarBatalla(true);
            }
        }

        private void TerminarBatalla(bool jugadorGano)
        {
            EnCurso = false;
            JugadorGano = jugadorGano;
            OnBatallaTerminada?.Invoke(jugadorGano);
        }

        /// <summary>
        /// Fórmula de daño:
        ///   base = (Ataque * Poder) / (Defensa_rival * 2)
        ///   varianza = 0.85 – 1.15 aleatoria
        ///   mínimo = 1
        /// </summary>
        private static int CalcularDano(IPokemon atk, Movimiento mov, IPokemon def)
        {
            if (mov.Poder == 0) return 0;
            float base_ = (float)(atk.Ataque * mov.Poder) / (def.Defensa * 2f);
            float var_ = 0.85f + (float)_rng.NextDouble() * 0.30f;
            return Math.Max(1, (int)(base_ * var_));
        }

        private void Log(string msg) => OnLogMensaje?.Invoke(msg);
    }
}