
using System;
using PokemonGame.Core.Entities;
using PokemonGame.Core.Interfaces;
using PokemonGame.Core.Starters;
using System.Collections.Generic;

namespace PokemonGame.Application
{
    // ══════════════════════════════════════════════════════════════════════
    //  INICIAR JUEGO
    // ══════════════════════════════════════════════════════════════════════

    /// <summary>
    /// SRP: solo crea y configura al jugador inicial.
    /// </summary>
    public sealed class IniciarJuegoUseCase
    {
        public ITrainer Ejecutar(string nombreJugador, int idxStarter)
        {
            var jugador = new Entrenador(nombreJugador, 500);

            // Starter elegido (LSP: todos los starters son IPokemon válidos)
            IPokemon starter = idxStarter switch
            {
                0 => new BulbasaurLine(5),
                1 => new CharmanderLine(5),
                2 => new SquirtleLine(5),
                _ => new BulbasaurLine(5)
            };

            jugador.CapturarPokemon(starter);

            // Ítems iniciales de regalo
            jugador.Inventario.Agregar(new Pocion());
            jugador.Inventario.Agregar(new Pocion());
            jugador.Inventario.Agregar(new Pokeball());
            jugador.Inventario.Agregar(new Pokeball());
            jugador.Inventario.Agregar(new Pokeball());
            jugador.Inventario.Agregar(new Revive());

            return jugador;
        }
    }

    // ══════════════════════════════════════════════════════════════════════
    //  CAPTURAR POKEMON
    // ══════════════════════════════════════════════════════════════════════

    public sealed record ResultadoCaptura(bool Exitosa, string Mensaje);

    /// <summary>SRP: solo encapsula la lógica de captura con Pokéball.</summary>
    public sealed class CapturarPokemonUseCase
    {
        public ResultadoCaptura Ejecutar(ITrainer jugador, IPokemon objetivo)
        {
            var idxs = jugador.Inventario.IndicesPokeballs();
            if (idxs.Count == 0)
                return new ResultadoCaptura(false, "¡No tienes Pokéballs!");

            var ball = jugador.Inventario.Obtener(idxs[0]);
            bool ok = ball!.Usar(objetivo);
            jugador.Inventario.Remover(idxs[0]);

            if (ok)
            {
                jugador.CapturarPokemon(objetivo);
                return new ResultadoCaptura(true, $"¡{objetivo.Nombre} fue capturado!");
            }
            return new ResultadoCaptura(false, $"¡{objetivo.Nombre} escapó!");
        }
    }

    // ══════════════════════════════════════════════════════════════════════
    //  USAR OBJETO
    // ══════════════════════════════════════════════════════════════════════

    /// <summary>SRP: solo gestiona el uso de objetos del inventario.</summary>
    public sealed class UsarObjetoUseCase
    {
        public bool Ejecutar(ITrainer jugador, int idxObj, int idxPoke) =>
            jugador.UsarObjeto(idxObj, idxPoke);
    }

    // ══════════════════════════════════════════════════════════════════════
    //  GESTIONAR EQUIPO
    // ══════════════════════════════════════════════════════════════════════

    /// <summary>SRP: solo gestiona intercambios en el equipo.</summary>
    public sealed class GestionarEquipoUseCase
    {
        public bool Intercambiar(ITrainer jugador, int idxE, int idxS) =>
            jugador.Equipo.Intercambiar(idxE, idxS);
    }

    // ══════════════════════════════════════════════════════════════════════
    //  COMPRAR EN TIENDA
    // ══════════════════════════════════════════════════════════════════════

    public sealed record ItemTienda(string Nombre, int Precio, Func<IItem> Fabrica);

    /// <summary>SRP: solo gestiona la lógica de compra.</summary>
    public sealed class ComprarItemUseCase
    {
        public static readonly IReadOnlyList<ItemTienda> Catalogo = new List<ItemTienda>
        {
            new("Poción",       50,  () => new Pocion()),
            new("Super Poción",100,  () => new SuperPocion()),
            new("Pokéball",    100,  () => new Pokeball()),
            new("Super Ball",  200,  () => new GreatBall()),
            new("Revive",      150,  () => new Revive()),
        };

        public bool Ejecutar(ITrainer jugador, int idxCatalogo)
        {
            if (idxCatalogo < 0 || idxCatalogo >= Catalogo.Count) return false;
            var item = Catalogo[idxCatalogo];
            if (!jugador.Pagar(item.Precio)) return false;
            jugador.Inventario.Agregar(item.Fabrica());
            return true;
        }
    }
}