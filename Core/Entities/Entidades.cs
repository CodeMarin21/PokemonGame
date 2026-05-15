// ╔══════════════════════════════════════════════════════════════════════════╗
// ║  CAPA: Core › Entities                                                  ║
// ║  CLASES: Items, Inventario, EquipoPokemon, Entrenador                   ║
// ║  SOLID:                                                                 ║
// ║    SRP  – Cada clase tiene una sola razón para cambiar                  ║
// ║    OCP  – Objeto es abstracto; nuevos ítems no modifican código actual  ║
// ║    LSP  – Pocion/Pokeball/Revive son intercambiables como IItem         ║
// ╚══════════════════════════════════════════════════════════════════════════╝
using System.Linq;
using PokemonGame.Core.Interfaces;
using System;
using System.Collections.Generic;

namespace PokemonGame.Core.Entities
{
    // ══════════════════════════════════════════════════════════════════════
    //  ITEMS – Jerarquía OCP
    // ══════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Clase abstracta base para todos los ítems del juego.
    /// OCP: abierta a extensión (nuevos ítems), cerrada a modificación.
    /// </summary>
    public abstract class ItemBase : IItem
    {
        public string Nombre { get; }
        public int Precio { get; }

        protected ItemBase(string nombre, int precio)
        { Nombre = nombre; Precio = precio; }

        public abstract bool Usar(IBattleable objetivo);

        public override string ToString() => $"{Nombre} (${Precio})";
    }

    /// <summary>LSP: Pocion es un IItem válido en cualquier contexto.</summary>
    public sealed class Pocion : ItemBase
    {
        public const int CuraHP = 30;
        public Pocion() : base("Poción", 50) { }
        public override bool Usar(IBattleable objetivo)
        {
            if (objetivo.Debilitado) return false;
            return objetivo.Curar(CuraHP) > 0;
        }
    }

    /// <summary>Cura más HP que la Poción normal.</summary>
    public sealed class SuperPocion : ItemBase
    {
        public const int CuraHP = 60;
        public SuperPocion() : base("Super Poción", 100) { }
        public override bool Usar(IBattleable objetivo)
        {
            if (objetivo.Debilitado) return false;
            return objetivo.Curar(CuraHP) > 0;
        }
    }

    /// <summary>Reactiva a un Pokémon debilitado con 50% HP.</summary>
    public sealed class Revive : ItemBase
    {
        public Revive() : base("Revive", 150) { }
        public override bool Usar(IBattleable objetivo)
        {
            if (!objetivo.Debilitado) return false;
            objetivo.Revivir();
            return true;
        }
    }

    /// <summary>Intenta capturar un Pokémon salvaje (probabilidad basada en HP).</summary>
    public sealed class Pokeball : ItemBase
    {
        private const float ProbBase = 0.30f;
        private const float BonusMax = 0.55f;
        private static readonly Random _rng = new();

        public Pokeball() : base("Pokéball", 100) { }

        public override bool Usar(IBattleable objetivo)
        {
            float prob = ProbBase + BonusMax * (1f - objetivo.PorcentajeVida);
            return (float)_rng.NextDouble() < prob;
        }
    }

    /// <summary>Pokéball mejorada con mayor tasa de captura.</summary>
    public sealed class GreatBall : ItemBase
    {
        private const float ProbBase = 0.50f;
        private const float BonusMax = 0.45f;
        private static readonly Random _rng = new();

        public GreatBall() : base("Super Ball", 200) { }

        public override bool Usar(IBattleable objetivo)
        {
            float prob = ProbBase + BonusMax * (1f - objetivo.PorcentajeVida);
            return (float)_rng.NextDouble() < prob;
        }
    }

    // ══════════════════════════════════════════════════════════════════════
    //  INVENTARIO  (SRP: solo gestión de ítems)
    // ══════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Almacena y gestiona los ítems del entrenador.
    /// SRP: única responsabilidad = gestión de colección de IItem.
    /// </summary>
    public sealed class Inventario : IInventoryService
    {
        private readonly List<IItem> _items = new();

        public IReadOnlyList<IItem> Items => _items.AsReadOnly();
        public bool EstaVacio => _items.Count == 0;
        public int Cantidad => _items.Count;

        public void Agregar(IItem item) => _items.Add(item);

        public bool Usar(int idx, IBattleable objetivo)
        {
            if (!IndiceValido(idx)) return false;
            bool ok = _items[idx].Usar(objetivo);
            if (ok) _items.RemoveAt(idx);
            return ok;
        }

        public IItem? Obtener(int idx) => IndiceValido(idx) ? _items[idx] : null;
        public IItem? Remover(int idx)
        {
            if (!IndiceValido(idx)) return null;
            var item = _items[idx];
            _items.RemoveAt(idx);
            return item;
        }

        public List<int> IndicesPokeballs() =>
            _items.Select((it, i) => (it, i))
                  .Where(t => t.it is Pokeball || t.it is GreatBall)
                  .Select(t => t.i)
                  .ToList();

        private bool IndiceValido(int i) => i >= 0 && i < _items.Count;
    }

    // ══════════════════════════════════════════════════════════════════════
    //  EQUIPO POKEMON  (SRP: solo gestión de equipo)
    // ══════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Administra el equipo activo (máx. 6) y el almacenamiento ilimitado.
    /// SRP: única responsabilidad = gestionar colecciones de IPokemon.
    /// </summary>
    public sealed class EquipoPokemon : IEquipoPokemon
    {
        public const int CapacidadMaxima = 6;

        private readonly List<IPokemon> _equipo = new();
        private readonly List<IPokemon> _almacenamiento = new();

        public IReadOnlyList<IPokemon> Equipo => _equipo.AsReadOnly();
        public IReadOnlyList<IPokemon> Almacenamiento => _almacenamiento.AsReadOnly();
        public bool EquipoLleno => _equipo.Count >= CapacidadMaxima;
        public bool TodosDebilitados =>
            _equipo.Count > 0 && _equipo.All(p => p.Debilitado);

        public bool Agregar(IPokemon pokemon)
        {
            if (!EquipoLleno) { _equipo.Add(pokemon); return true; }
            _almacenamiento.Add(pokemon); return false;
        }

        public IPokemon? PrimerActivo() =>
            _equipo.FirstOrDefault(p => !p.Debilitado);

        public IPokemon? ObtenerDelEquipo(int i) =>
            (i >= 0 && i < _equipo.Count) ? _equipo[i] : null;

        public bool Intercambiar(int idxE, int idxS)
        {
            if (idxE < 0 || idxE >= _equipo.Count) return false;
            if (idxS < 0 || idxS >= _almacenamiento.Count) return false;
            (_equipo[idxE], _almacenamiento[idxS]) =
                (_almacenamiento[idxS], _equipo[idxE]);
            return true;
        }
    }

    // ══════════════════════════════════════════════════════════════════════
    //  ENTRENADOR  (SRP: actor principal, delega a equipo e inventario)
    // ══════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Actor principal del sistema.
    /// SRP: coordina equipo e inventario, NO implementa lógica de ellos.
    /// DIP: expone IEquipoPokemon e IInventoryService (interfaces, no concretos).
    /// </summary>
    public sealed class Entrenador : ITrainer
    {
        public string Nombre { get; }
        public int Dinero { get; private set; }
        public IEquipoPokemon Equipo { get; }
        public IInventoryService Inventario { get; }

        public Entrenador(string nombre, int dinero = 500)
        {
            Nombre = nombre;
            Dinero = dinero;
            Equipo = new EquipoPokemon();
            Inventario = new Inventario();
        }

        public void CapturarPokemon(IPokemon pokemon) => Equipo.Agregar(pokemon);
        public void GanarDinero(int n) => Dinero += n;

        public bool Pagar(int n)
        {
            if (Dinero < n) return false;
            Dinero -= n; return true;
        }

        public bool UsarObjeto(int idxObj, int idxPoke)
        {
            var pk = Equipo.ObtenerDelEquipo(idxPoke);
            return pk != null && Inventario.Usar(idxObj, pk);
        }

        public override string ToString() =>
            $"Entrenador {Nombre} | ${Dinero}";
    }
}