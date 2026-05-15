
using System;
using System.Collections.Generic;

namespace PokemonGame.Core.Interfaces
{
    // ─────────────────────────────────────────────────────────────────────
    //  DATO: Movimiento (SRP – solo transporta datos de un ataque)
    // ─────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Inmutable. Representa los datos de un ataque.
    /// </summary>
    public sealed record Movimiento(
        string Nombre,
        int Poder,
        string Tipo);

    // ─────────────────────────────────────────────────────────────────────
    //  ISP: Interfaces pequeñas y enfocadas
    // ─────────────────────────────────────────────────────────────────────

    /// <summary>
    /// IBattleable – solo lo relacionado con recibir/recuperar daño.
    /// </summary>
    public interface IBattleable
    {
        int VidaActual { get; }
        int VidaMax { get; }
        bool Debilitado { get; }
        float PorcentajeVida { get; }

        void AplicarDano(int dano);
        int Curar(int cantidad);   // retorna HP real recuperado
        void Revivir();
    }

    /// <summary>
    /// ILevelable – solo progresión de experiencia y nivel.
    /// </summary>
    public interface ILevelable
    {
        int Nivel { get; }
        int Exp { get; }
        int ExpParaSiguienteNivel { get; }

        /// <summary>Retorna true si subió de nivel.</summary>
        bool GanarExp(int cantidad);
    }

    /// <summary>
    /// IEvolvable – solo evolución.
    /// </summary>
    public interface IEvolvable
    {
        bool PuedeEvolucionar { get; }
        string NombreEvolucion { get; }
        int NivelEvolucion { get; }

        void Evolucionar();
    }

    /// <summary>
    /// IAttacker – estadísticas de combate y lista de movimientos.
    /// </summary>
    public interface IAttacker
    {
        int Ataque { get; }
        int Defensa { get; }
        int Velocidad { get; }

        IReadOnlyList<Movimiento> Movimientos { get; }
    }

    /// <summary>
    /// IPokemon – combina todas las interfaces.
    /// LSP: cualquier subclase puede sustituir a IPokemon sin romper el sistema.
    /// </summary>
    public interface IPokemon : IBattleable, ILevelable, IEvolvable, IAttacker
    {
        string Nombre { get; }
        string Tipo { get; }
    }

    // ─────────────────────────────────────────────────────────────────────
    //  Interfaces de dominio (Inventario, Items, Equipo, Entrenador)
    // ─────────────────────────────────────────────────────────────────────

    /// <summary>
    /// IItem – contrato de cualquier objeto del inventario.
    /// OCP: nuevos ítems extienden IItem sin modificar código existente.
    /// </summary>
    public interface IItem
    {
        string Nombre { get; }
        int Precio { get; }

        /// <summary>Aplica el efecto. Retorna true si se usó con éxito.</summary>
        bool Usar(IBattleable objetivo);
    }

    /// <summary>
    /// IInventoryService – gestión de ítems.
    /// </summary>
    public interface IInventoryService
    {
        IReadOnlyList<IItem> Items { get; }
        bool EstaVacio { get; }
        int Cantidad { get; }

        void Agregar(IItem item);
        bool Usar(int idx, IBattleable objetivo);
        IItem? Obtener(int idx);
        IItem? Remover(int idx);
        List<int> IndicesPokeballs();
    }

    /// <summary>
    /// IEquipoPokemon – gestión del equipo activo y almacenamiento.
    /// </summary>
    public interface IEquipoPokemon
    {
        IReadOnlyList<IPokemon> Equipo { get; }
        IReadOnlyList<IPokemon> Almacenamiento { get; }
        bool EquipoLleno { get; }
        bool TodosDebilitados { get; }

        bool Agregar(IPokemon pokemon);
        IPokemon? PrimerActivo();
        IPokemon? ObtenerDelEquipo(int idx);
        bool Intercambiar(int idxEquipo, int idxStorage);
    }

    /// <summary>
    /// ITrainer – actor principal del sistema.
    /// </summary>
    public interface ITrainer
    {
        string Nombre { get; }
        int Dinero { get; }
        IEquipoPokemon Equipo { get; }
        IInventoryService Inventario { get; }

        void CapturarPokemon(IPokemon pokemon);
        void GanarDinero(int cantidad);
        bool Pagar(int cantidad);
        bool UsarObjeto(int idxObj, int idxPoke);
    }

    // ─────────────────────────────────────────────────────────────────────
    //  Interfaces de Servicios (DIP – depender de abstracciones)
    // ─────────────────────────────────────────────────────────────────────

    /// <summary>
    /// IBattleService – motor de combate por turnos.
    /// DIP: GameForm depende de esta interfaz, no de BattleService concreto.
    /// </summary>
    public interface IBattleService
    {
        // Observer pattern mediante eventos C#
        event Action<string> OnLogMensaje;
        event Action<bool> OnBatallaTerminada;  // true = jugador ganó
        event Action<IPokemon, bool> OnExpGanada;         // pokemon, subioNivel
        event Action<IPokemon> OnPokemonDebilitado;

        bool EnCurso { get; }
        bool JugadorGano { get; }
        ITrainer Jugador { get; }
        ITrainer Rival { get; }

        bool IniciarBatalla(ITrainer jugador, ITrainer rival);
        void EjecutarAtaque(int idxMovimiento);
        bool EjecutarCaptura();
        void EjecutarUsoObjeto(int idxObjeto, int idxPokemon);
        bool IntentarHuida();
    }

    /// <summary>
    /// IEncounterService – generación de encuentros y rivales.
    /// DIP: GameForm depende de esta interfaz.
    /// </summary>
    public interface IEncounterService
    {
        IPokemon GenerarPokemonSalvaje(int nivelReferencia);
        bool VerificarEncuentro(int tipoTile);
        ITrainer CrearRival(int nivelReferencia);
    }
}