// ╔══════════════════════════════════════════════════════════════════════════╗
// ║  CAPA: Core › Services                                                  ║
// ║  CLASE: EncounterService : IEncounterService                            ║
// ║  SOLID: SRP – solo genera encuentros; DIP – retorna IPokemon/ITrainer   ║
// ╚══════════════════════════════════════════════════════════════════════════╝

using PokemonGame.Core.Entities;
using PokemonGame.Core.Interfaces;
using PokemonGame.Core.Starters;
using System;

namespace PokemonGame.Core.Services
{
    /// <summary>
    /// Genera Pokémon salvajes y rivales según el nivel de referencia.
    /// OCP: agregar nuevos Pokémon salvajes no modifica esta clase
    /// (solo añadir al pool correspondiente).
    /// </summary>
    public sealed class EncounterService : IEncounterService
    {
        // Probabilidad de encuentro por paso en hierba (1/10)
        private const int ChanceBaja = 10;

        private static readonly Random _rng = new();

        // Pools de Pokémon por rango de nivel
        private static readonly Func<PokemonBase>[] PoolBajo =
        {
            () => new Rattata(),
            () => new Pidgey(),
            () => new Caterpie(),
            () => new Weedle(),
        };

        private static readonly Func<PokemonBase>[] PoolMedio =
        {
            () => new Rattata(),
            () => new Pidgey(),
            () => new Zubat(),
            () => new Ekans(),
            () => new Geodude(),
            () => new Meowth(),
        };

        private static readonly Func<PokemonBase>[] PoolAlto =
        {
            () => new Geodude(),
            () => new Zubat(),
            () => new Psyduck(),
            () => new Jigglypuff(),
            () => new Meowth(),
        };

        /// <summary>
        /// Genera un Pokémon salvaje aleatorio con nivel próximo al de referencia.
        /// DIP: retorna IPokemon, no un tipo concreto.
        /// </summary>
        public IPokemon GenerarPokemonSalvaje(int nivelReferencia)
        {
            Func<PokemonBase>[] pool =
                nivelReferencia <= 3 ? PoolBajo :
                nivelReferencia <= 5 ? PoolMedio : PoolAlto;

            var pokemon = pool[_rng.Next(pool.Length)]();
            int nivel = Math.Max(2, nivelReferencia + _rng.Next(-2, 3));
            pokemon.AjustarNivel(nivel);
            return pokemon;
        }

        /// <summary>
        /// Verifica si ocurre un encuentro según el tipo de tile.
        /// Tile 1 = hierba alta → probabilidad de encuentro.
        /// </summary>
        public bool VerificarEncuentro(int tipoTile)
        {
            if (tipoTile != 1) return false;
            return _rng.Next(ChanceBaja) == 0;
        }

        /// <summary>
        /// Crea al rival Gary Oak con equipo escalado al nivel del jugador.
        /// DIP: retorna ITrainer.
        /// </summary>
        public ITrainer CrearRival(int nivelReferencia)
        {
            var gary = new Entrenador("Gary Oak", 1000);

            // Gary tiene 3 Pokémon variados y más fuertes
            gary.CapturarPokemon(GenerarPokemonSalvaje(nivelReferencia + 3));
            gary.CapturarPokemon(GenerarPokemonSalvaje(nivelReferencia + 2));
            gary.CapturarPokemon(GenerarPokemonSalvaje(nivelReferencia + 1));

            // Gary tiene ítems
            gary.Inventario.Agregar(new Pocion());
            gary.Inventario.Agregar(new SuperPocion());

            return gary;
        }
    }
}