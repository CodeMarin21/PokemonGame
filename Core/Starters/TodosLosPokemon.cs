

using PokemonGame.Core.Entities;
using PokemonGame.Core.Interfaces;

namespace PokemonGame.Core.Starters
{
    // ══════════════════════════════════════════════════════════════════════
    //  LÍNEA BULBASAUR  (Planta)
    // ══════════════════════════════════════════════════════════════════════

    /// <summary>
    /// LSP: BulbasaurLine puede sustituir a PokemonBase en cualquier contexto.
    /// Gestiona internamente las 3 etapas evolutivas.
    /// </summary>
    public sealed class BulbasaurLine : PokemonBase
    {
        private int _etapa = 0; // 0=Bulbasaur  1=Ivysaur  2=Venusaur

        private static readonly (string Nombre, int NivelEvo)[] Etapas =
        {
            ("Bulbasaur", 0),
            ("Ivysaur",  6),
            ("Venusaur", 2)
        };

        public BulbasaurLine(int nivel = 8)
            : base("Bulbasaur", "Planta", nivel, 45, 49, 49, 45)
        {
            AgregarMovimiento(new Movimiento("Placaje", 35, "Normal"));
            AgregarMovimiento(new Movimiento("Látigo Cepa", 45, "Planta"));
            AgregarMovimiento(new Movimiento("Drenadoras", 20, "Planta"));
            AgregarMovimiento(new Movimiento("Gruñido", 0, "Normal"));
        }

        public override bool PuedeEvolucionar =>
            _etapa < 2 && Nivel >= Etapas[_etapa + 1].NivelEvo;
        public override string NombreEvolucion =>
            _etapa < 2 ? Etapas[_etapa + 1].Nombre : string.Empty;
        public override int NivelEvolucion =>
            _etapa < 2 ? Etapas[_etapa + 1].NivelEvo : int.MaxValue;

        public override void Evolucionar()
        {
            if (!PuedeEvolucionar) return;
            _etapa++;
            CambiarNombre(Etapas[_etapa].Nombre);
            AplicarBoostEvolucion();
            if (_etapa == 1) AgregarMovimiento(new Movimiento("Rayo Solar", 120, "Planta"));
            if (_etapa == 2) AgregarMovimiento(new Movimiento("Hoja Afilada", 80, "Planta"));
        }
    }

    // ══════════════════════════════════════════════════════════════════════
    //  LÍNEA CHARMANDER  (Fuego)
    // ══════════════════════════════════════════════════════════════════════

    public sealed class CharmanderLine : PokemonBase
    {
        private int _etapa = 0; // 0=Charmander  1=Charmeleon  2=Charizard

        private static readonly (string Nombre, int NivelEvo)[] Etapas =
        {
            ("Charmander", 0),
            ("Charmeleon", 16),
            ("Charizard",  36)
        };

        public CharmanderLine(int nivel = 5)
            : base("Charmander", "Fuego", nivel, 9, 12, 13, 15)
        {
            AgregarMovimiento(new Movimiento("Arañazo", 40, "Normal"));
            AgregarMovimiento(new Movimiento("Ascuas", 40, "Fuego"));
            AgregarMovimiento(new Movimiento("Garra Ignia", 55, "Fuego"));
            AgregarMovimiento(new Movimiento("Gruñido", 0, "Normal"));
        }

        public override bool PuedeEvolucionar =>
            _etapa < 2 && Nivel >= Etapas[_etapa + 1].NivelEvo;
        public override string NombreEvolucion =>
            _etapa < 2 ? Etapas[_etapa + 1].Nombre : string.Empty;
        public override int NivelEvolucion =>
            _etapa < 2 ? Etapas[_etapa + 1].NivelEvo : int.MaxValue;

        public override void Evolucionar()
        {
            if (!PuedeEvolucionar) return;
            _etapa++;
            CambiarNombre(Etapas[_etapa].Nombre);
            AplicarBoostEvolucion(1.4f, 1.40f, 1.30f, 1.30f);
            if (_etapa == 1) AgregarMovimiento(new Movimiento("Llamarada", 95, "Fuego"));
            if (_etapa == 2) AgregarMovimiento(new Movimiento("Lanzallamas", 90, "Fuego"));
        }
    }

    // ══════════════════════════════════════════════════════════════════════
    //  LÍNEA SQUIRTLE  (Agua)
    // ══════════════════════════════════════════════════════════════════════

    public sealed class SquirtleLine : PokemonBase
    {
        private int _etapa = 0; // 0=Squirtle  1=Wartortle  2=Blastoise

        private static readonly (string Nombre, int NivelEvo)[] Etapas =
        {
            ("Squirtle",  0),
            ("Wartortle", 16),
            ("Blastoise", 36)
        };

        public SquirtleLine(int nivel = 5)
            : base("Squirtle", "Agua", nivel, 44, 48, 65, 43)
        {
            AgregarMovimiento(new Movimiento("Placaje", 35, "Normal"));
            AgregarMovimiento(new Movimiento("Pistola Agua", 40, "Agua"));
            AgregarMovimiento(new Movimiento("Mordisco", 60, "Siniestro"));
            AgregarMovimiento(new Movimiento("Burbuja", 30, "Agua"));
        }

        public override bool PuedeEvolucionar =>
            _etapa < 2 && Nivel >= Etapas[_etapa + 1].NivelEvo;
        public override string NombreEvolucion =>
            _etapa < 2 ? Etapas[_etapa + 1].Nombre : string.Empty;
        public override int NivelEvolucion =>
            _etapa < 2 ? Etapas[_etapa + 1].NivelEvo : int.MaxValue;

        public override void Evolucionar()
        {
            if (!PuedeEvolucionar) return;
            _etapa++;
            CambiarNombre(Etapas[_etapa].Nombre);
            AplicarBoostEvolucion(1.4f, 1.30f, 1.45f, 1.20f);
            if (_etapa == 1) AgregarMovimiento(new Movimiento("Surf", 95, "Agua"));
            if (_etapa == 2) AgregarMovimiento(new Movimiento("Hidrobomba", 110, "Agua"));
        }
    }

    // ══════════════════════════════════════════════════════════════════════
    //  POKÉMON SALVAJES  (LSP: todos sustituyen PokemonBase)
    // ══════════════════════════════════════════════════════════════════════

    public sealed class Rattata : PokemonBase
    {
        public Rattata() : base("Rattata", "Normal", 3, 30, 56, 35, 72)
        {
            AgregarMovimiento(new Movimiento("Placaje", 35, "Normal"));
            AgregarMovimiento(new Movimiento("Mordisco", 60, "Siniestro"));
            AgregarMovimiento(new Movimiento("Gruñido", 0, "Normal"));
        }
        public override bool PuedeEvolucionar => Nivel >= 20;
        public override string NombreEvolucion => "Raticate";
        public override int NivelEvolucion => 20;
        public override void Evolucionar()
        {
            if (!PuedeEvolucionar) return;
            CambiarNombre("Raticate");
            AplicarBoostEvolucion(1.3f, 1.40f, 1.35f, 1.35f);
            AgregarMovimiento(new Movimiento("Hiper Colmillo", 80, "Normal"));
        }
    }

    public sealed class Pidgey : PokemonBase
    {
        public Pidgey() : base("Pidgey", "Volador", 3, 40, 45, 40, 56)
        {
            AgregarMovimiento(new Movimiento("Placaje", 35, "Normal"));
            AgregarMovimiento(new Movimiento("Tornado", 40, "Volador"));
            AgregarMovimiento(new Movimiento("Ráfaga", 35, "Normal"));
        }
        public override bool PuedeEvolucionar => Nivel >= 18;
        public override string NombreEvolucion => "Pidgeotto";
        public override int NivelEvolucion => 18;
        public override void Evolucionar()
        {
            if (!PuedeEvolucionar) return;
            CambiarNombre("Pidgeotto");
            AplicarBoostEvolucion(1.35f, 1.35f, 1.30f, 1.30f);
            AgregarMovimiento(new Movimiento("Vuelo", 90, "Volador"));
        }
    }

    public sealed class Caterpie : PokemonBase
    {
        public Caterpie() : base("Caterpie", "Bicho", 3, 45, 30, 35, 45)
        {
            AgregarMovimiento(new Movimiento("Placaje", 35, "Normal"));
            AgregarMovimiento(new Movimiento("Paralizador", 20, "Normal"));
        }
        public override bool PuedeEvolucionar => Nivel >= 7;
        public override string NombreEvolucion => "Metapod";
        public override int NivelEvolucion => 7;
        public override void Evolucionar()
        {
            if (!PuedeEvolucionar) return;
            CambiarNombre("Metapod");
            AplicarBoostEvolucion(1.30f, 1.10f, 1.60f, 1.00f);
        }
    }

    public sealed class Weedle : PokemonBase
    {
        public Weedle() : base("Weedle", "Bicho", 3, 40, 35, 30, 50)
        {
            AgregarMovimiento(new Movimiento("Picadura", 35, "Bicho"));
            AgregarMovimiento(new Movimiento("Veneno", 0, "Veneno"));
            AgregarMovimiento(new Movimiento("Placaje", 35, "Normal"));
        }
        public override bool PuedeEvolucionar => Nivel >= 7;
        public override string NombreEvolucion => "Kakuna";
        public override int NivelEvolucion => 7;
        public override void Evolucionar()
        {
            if (!PuedeEvolucionar) return;
            CambiarNombre("Kakuna");
            AplicarBoostEvolucion(1.25f, 1.05f, 1.70f, 1.00f);
        }
    }

    public sealed class Zubat : PokemonBase
    {
        public Zubat() : base("Zubat", "Veneno", 5, 40, 45, 35, 55)
        {
            AgregarMovimiento(new Movimiento("Absorbe", 20, "Veneno"));
            AgregarMovimiento(new Movimiento("Mordisco", 60, "Siniestro"));
            AgregarMovimiento(new Movimiento("Supersónico", 0, "Normal"));
        }
        public override bool PuedeEvolucionar => Nivel >= 22;
        public override string NombreEvolucion => "Golbat";
        public override int NivelEvolucion => 22;
        public override void Evolucionar()
        {
            if (!PuedeEvolucionar) return;
            CambiarNombre("Golbat");
            AplicarBoostEvolucion(1.45f, 1.35f, 1.30f, 1.40f);
            AgregarMovimiento(new Movimiento("Vuelo", 90, "Volador"));
        }
    }

    public sealed class Geodude : PokemonBase
    {
        public Geodude() : base("Geodude", "Roca", 4, 40, 80, 100, 20)
        {
            AgregarMovimiento(new Movimiento("Lanzarrocas", 50, "Roca"));
            AgregarMovimiento(new Movimiento("Placaje", 35, "Normal"));
            AgregarMovimiento(new Movimiento("Avalancha", 75, "Roca"));
        }
        public override bool PuedeEvolucionar => Nivel >= 25;
        public override string NombreEvolucion => "Graveler";
        public override int NivelEvolucion => 25;
        public override void Evolucionar()
        {
            if (!PuedeEvolucionar) return;
            CambiarNombre("Graveler");
            AplicarBoostEvolucion(1.35f, 1.35f, 1.35f, 1.15f);
            AgregarMovimiento(new Movimiento("Terremoto", 100, "Tierra"));
        }
    }

    public sealed class Ekans : PokemonBase
    {
        public Ekans() : base("Ekans", "Veneno", 5, 35, 60, 44, 55)
        {
            AgregarMovimiento(new Movimiento("Ácido", 40, "Veneno"));
            AgregarMovimiento(new Movimiento("Mordisco", 60, "Siniestro"));
            AgregarMovimiento(new Movimiento("Constricción", 15, "Normal"));
        }
        public override bool PuedeEvolucionar => Nivel >= 22;
        public override string NombreEvolucion => "Arbok";
        public override int NivelEvolucion => 22;
        public override void Evolucionar()
        {
            if (!PuedeEvolucionar) return;
            CambiarNombre("Arbok");
            AplicarBoostEvolucion(1.40f, 1.35f, 1.30f, 1.30f);
            AgregarMovimiento(new Movimiento("Colmillo Veneno", 65, "Veneno"));
        }
    }

    public sealed class Psyduck : PokemonBase
    {
        public Psyduck() : base("Psyduck", "Agua", 5, 50, 52, 48, 55)
        {
            AgregarMovimiento(new Movimiento("Pistola Agua", 40, "Agua"));
            AgregarMovimiento(new Movimiento("Confusión", 50, "Psíquico"));
            AgregarMovimiento(new Movimiento("Placaje", 35, "Normal"));
        }
        public override bool PuedeEvolucionar => Nivel >= 33;
        public override string NombreEvolucion => "Golduck";
        public override int NivelEvolucion => 33;
        public override void Evolucionar()
        {
            if (!PuedeEvolucionar) return;
            CambiarNombre("Golduck");
            AplicarBoostEvolucion(1.40f, 1.35f, 1.35f, 1.30f);
            AgregarMovimiento(new Movimiento("Surf", 95, "Agua"));
        }
    }

    public sealed class Jigglypuff : PokemonBase
    {
        public Jigglypuff() : base("Jigglypuff", "Normal", 4, 115, 45, 20, 20)
        {
            AgregarMovimiento(new Movimiento("Placaje", 35, "Normal"));
            AgregarMovimiento(new Movimiento("Canto", 0, "Normal"));
            AgregarMovimiento(new Movimiento("Golpe Doble", 15, "Normal"));
        }
        public override bool PuedeEvolucionar => false;
        public override string NombreEvolucion => string.Empty;
        public override int NivelEvolucion => int.MaxValue;
        public override void Evolucionar() { /* sin evolución */ }
    }

    public sealed class Meowth : PokemonBase
    {
        public Meowth() : base("Meowth", "Normal", 5, 40, 45, 35, 90)
        {
            AgregarMovimiento(new Movimiento("Arañazo", 40, "Normal"));
            AgregarMovimiento(new Movimiento("Mordisco", 60, "Siniestro"));
            AgregarMovimiento(new Movimiento("Gruñido", 0, "Normal"));
        }
        public override bool PuedeEvolucionar => Nivel >= 28;
        public override string NombreEvolucion => "Persian";
        public override int NivelEvolucion => 28;
        public override void Evolucionar()
        {
            if (!PuedeEvolucionar) return;
            CambiarNombre("Persian");
            AplicarBoostEvolucion(1.35f, 1.30f, 1.30f, 1.45f);
            AgregarMovimiento(new Movimiento("Golpe Furia", 18, "Normal"));
        }
    }
}
