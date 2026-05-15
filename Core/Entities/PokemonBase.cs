// ╔══════════════════════════════════════════════════════════════════════════╗
// ║  CAPA: Core › Entities                                                  ║
// ║  CLASE: PokemonBase (abstract)                                          ║
// ║  SOLID:                                                                 ║
// ║    SRP – Solo maneja estado y comportamiento de un Pokémon              ║
// ║    OCP – Abierta a extensión (subclases), cerrada a modificación        ║
// ║    LSP – Toda subclase puede sustituir a PokemonBase sin romper nada   ║
// ╚══════════════════════════════════════════════════════════════════════════╝

using PokemonGame.Core.Interfaces;
using System;
using System.Collections.Generic;

namespace PokemonGame.Core.Entities
{
    /// <summary>
    /// Clase abstracta base para todos los Pokémon.
    /// Implementa la lógica común (daño, curación, EXP, nivel).
    /// Las subclases solo deben implementar: evolución y comportamiento propio.
    /// </summary>
    public abstract class PokemonBase : IPokemon
    {
        // ── IPokemon ─────────────────────────────────────────────────────
        public string Nombre { get; protected set; }
        public string Tipo { get; protected set; }

        // ── IBattleable ──────────────────────────────────────────────────
        public int VidaActual { get; protected set; }
        public int VidaMax { get; protected set; }
        public bool Debilitado => VidaActual == 0;
        public float PorcentajeVida => VidaMax > 0 ? (float)VidaActual / VidaMax : 0f;

        // ── ILevelable ───────────────────────────────────────────────────
        public int Nivel { get; protected set; }
        public int Exp { get; protected set; }
        public int ExpParaSiguienteNivel => Nivel * 12;

        // ── IAttacker ────────────────────────────────────────────────────
        public int Ataque { get; protected set; }
        public int Defensa { get; protected set; }
        public int Velocidad { get; protected set; }

        private readonly List<Movimiento> _movimientos = new();
        public IReadOnlyList<Movimiento> Movimientos => _movimientos.AsReadOnly();

        // ── IEvolvable (abstract – cada subclase define su propia evolución)
        public abstract bool PuedeEvolucionar { get; }
        public abstract string NombreEvolucion { get; }
        public abstract int NivelEvolucion { get; }
        public abstract void Evolucionar();

        // ── Random compartido ────────────────────────────────────────────
        private static readonly Random _rng = new();

        // ── Constructor ──────────────────────────────────────────────────
        protected PokemonBase(
            string nombre, string tipo, int nivel,
            int vida, int ataque, int defensa, int velocidad)
        {
            Nombre = nombre;
            Tipo = tipo;
            Nivel = nivel;
            VidaMax = vida;
            VidaActual = vida;
            Ataque = ataque;
            Defensa = defensa;
            Velocidad = velocidad;
        }

        // ── IBattleable ──────────────────────────────────────────────────

        /// <summary>Recibe daño. Marca debilitado si llega a 0.</summary>
        public void AplicarDano(int dano)
        {
            VidaActual = Math.Max(0, VidaActual - dano);
        }

        /// <summary>Recupera HP. No funciona en debilitados. Retorna HP curado.</summary>
        public int Curar(int cantidad)
        {
            if (Debilitado) return 0;
            int antes = VidaActual;
            VidaActual = Math.Min(VidaMax, VidaActual + cantidad);
            return VidaActual - antes;
        }

        /// <summary>Reactiva con el 50% del HP máximo.</summary>
        public void Revivir()
        {
            VidaActual = VidaMax / 2;
        }

        // ── ILevelable ───────────────────────────────────────────────────

        /// <summary>Acumula EXP. Retorna true si subió de nivel.</summary>
        public bool GanarExp(int cantidad)
        {
            Exp += cantidad;
            if (Exp >= ExpParaSiguienteNivel)
            {
                Exp -= ExpParaSiguienteNivel;
                SubirNivel();
                return true;
            }
            return false;
        }

        /// <summary>
        /// Sube de nivel y mejora todas las estadísticas.
        /// Virtual: las subclases pueden añadir movimientos nuevos al subir de nivel.
        /// </summary>
        protected virtual void SubirNivel()
        {
            Nivel++;
            int bonusVida = Math.Max(1, (int)(VidaMax * 0.08f));
            VidaMax += bonusVida;
            VidaActual = VidaMax;          // cura completa al subir nivel
            Ataque = (int)(Ataque * 1.07f);
            Defensa = (int)(Defensa * 1.07f);
            Velocidad = (int)(Velocidad * 1.05f);
        }

        // ── IAttacker ────────────────────────────────────────────────────

        /// <summary>
        /// Calcula el daño a infligir al objetivo y lo aplica.
        /// Fórmula: (Ataque * Poder) / (Defensa_objetivo * 2) * varianza 0.85-1.15
        /// </summary>
        public int Atacar(Movimiento mov, IPokemon objetivo)
        {
            if (Debilitado) return 0;
            float base_ = (float)(Ataque * mov.Poder) / (objetivo.Defensa * 2f);
            float var_ = 0.85f + (float)_rng.NextDouble() * 0.30f;
            int dano = Math.Max(1, (int)(base_ * var_));
            objetivo.AplicarDano(dano);
            return dano;
        }

        // ── Helpers internos ─────────────────────────────────────────────

        /// <summary>Usado por la fábrica para ajustar nivel sin EXP.</summary>
        internal void AjustarNivel(int nivel)
        {
            if (nivel <= 0) return;
            // Escalar stats proporcionalmente al nivel
            float factor = 1f + (nivel - Nivel) * 0.07f;
            Nivel = nivel;
            VidaMax = (int)(VidaMax * factor);
            VidaActual = VidaMax;
            Ataque = (int)(Ataque * factor);
            Defensa = (int)(Defensa * factor);
            Velocidad = (int)(Velocidad * factor);
        }

        /// <summary>Agregar un movimiento a la lista interna (solo desde subclases/factory).</summary>
        protected void AgregarMovimiento(Movimiento mov) => _movimientos.Add(mov);

        /// <summary>Cambiar nombre al evolucionar.</summary>
        protected void CambiarNombre(string nuevoNombre) => Nombre = nuevoNombre;

        /// <summary>Boost de stats al evolucionar.</summary>
        protected void AplicarBoostEvolucion(float factorVida = 1.4f, float factorAtk = 1.35f,
            float factorDef = 1.35f, float factorVel = 1.20f)
        {
            VidaMax = (int)(VidaMax * factorVida);
            VidaActual = VidaMax;
            Ataque = (int)(Ataque * factorAtk);
            Defensa = (int)(Defensa * factorDef);
            Velocidad = (int)(Velocidad * factorVel);
        }

        public override string ToString() =>
            $"{Nombre} Nv.{Nivel} HP:{VidaActual}/{VidaMax} [{Tipo}]";
    }
}