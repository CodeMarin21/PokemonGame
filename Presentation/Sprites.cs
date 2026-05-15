// ╔══════════════════════════════════════════════════════════════════════════╗
// ║  CAPA: Presentation  –  Sprites                                         ║
// ║  SRP: única responsabilidad = definir y dibujar pixel art               ║
// ╚══════════════════════════════════════════════════════════════════════════╝

using System.Collections.Generic;
using System.Drawing;

namespace PokemonGame.Presentation
{
    /// <summary>
    /// Repositorio de pixel art del juego.
    /// Cada sprite es un array de strings donde cada carácter = un bloque de color.
    /// '.' = transparente.
    /// </summary>
    public static class Sprites
    {
        // ── Paleta ───────────────────────────────────────────────────────
        private static readonly Dictionary<char, Color> Paleta = new()
        {
            ['.'] = Color.Transparent,
            ['K'] = Color.FromArgb(20, 20, 20),
            ['W'] = Color.FromArgb(240, 240, 240),
            ['w'] = Color.FromArgb(200, 200, 200),
            ['S'] = Color.FromArgb(255, 213, 168),
            ['s'] = Color.FromArgb(200, 160, 110),
            ['H'] = Color.FromArgb(50, 30, 10),
            ['h'] = Color.FromArgb(80, 50, 20),
            ['R'] = Color.FromArgb(220, 40, 40),
            ['r'] = Color.FromArgb(150, 20, 20),
            ['B'] = Color.FromArgb(50, 100, 210),
            ['b'] = Color.FromArgb(30, 60, 160),
            ['G'] = Color.FromArgb(60, 180, 60),
            ['g'] = Color.FromArgb(30, 110, 30),
            ['L'] = Color.FromArgb(140, 230, 100),
            ['l'] = Color.FromArgb(90, 160, 60),
            ['Y'] = Color.FromArgb(250, 220, 50),
            ['y'] = Color.FromArgb(180, 150, 20),
            ['O'] = Color.FromArgb(240, 140, 40),
            ['o'] = Color.FromArgb(180, 90, 20),
            ['P'] = Color.FromArgb(220, 120, 200),
            ['p'] = Color.FromArgb(160, 60, 140),
            ['T'] = Color.FromArgb(70, 210, 210),
            ['t'] = Color.FromArgb(30, 140, 160),
            ['U'] = Color.FromArgb(150, 70, 210),
            ['u'] = Color.FromArgb(90, 30, 150),
            ['N'] = Color.FromArgb(180, 90, 40),
            ['n'] = Color.FromArgb(100, 50, 20),
            ['A'] = Color.FromArgb(170, 170, 170),
            ['a'] = Color.FromArgb(100, 100, 100),
            ['C'] = Color.FromArgb(80, 200, 180),
            ['c'] = Color.FromArgb(40, 130, 120),
            ['M'] = Color.FromArgb(240, 100, 60),
            ['m'] = Color.FromArgb(170, 60, 30),
        };

        // ── Jugador ──────────────────────────────────────────────────────
        public static readonly string[] JugadorFrente =
        {
            "..HhH...",
            ".HhHhH..",
            ".HSsSH..",
            "..SSS...",
            ".RRRRRR.",
            "RRRRRRRR",
            "..B..B..",
            "..B..B..",
        };
        public static readonly string[] JugadorEspalda =
        {
            "..HHH...",
            ".HHHHH..",
            ".HHHhH..",
            "..HHH...",
            ".RRRRRR.",
            "RRRRRRRR",
            "..B..B..",
            "..B..B..",
        };
        public static readonly string[] JugadorIzquierda =
        {
            "..HHh...",
            ".HHHsH..",
            ".HSSsS..",
            "..SSs...",
            ".RRRRRR.",
            "RRRRRRRR",
            "..BB....",
            "..BB....",
        };
        public static readonly string[] JugadorDerecha =
        {
            "...hHH..",
            "..HsHHH.",
            "..SsSHH.",
            "...sSS..",
            ".RRRRRR.",
            "RRRRRRRR",
            "....BB..",
            "....BB..",
        };

        // ── Pokémon ──────────────────────────────────────────────────────
        public static readonly string[] Bulbasaur =
        {
            "....GG..",
            "...lGGG.",
            ".GlGLGGG",
            "GGlWWWGG",
            "GGW..WGG",
            ".GGGGGGG",
            "..G.G.G.",
            "..G.G.G.",
        };
        public static readonly string[] Ivysaur =
        {
            "...GGG..",
            "..LlGGG.",
            ".GLlGGGG",
            "GGGWWwGG",
            "GGGw.wGG",
            "GGGGGGGG",
            "..GG.GG.",
            "..G...G.",
        };
        public static readonly string[] Venusaur =
        {
            "..GGGGl.",
            ".GLLlGGG",
            "GGGLLGGG",
            "GGGwwwGG",
            "GGGw.wGG",
            "GGGGGGGG",
            ".GG..GG.",
            ".G....G.",
        };
        public static readonly string[] Charmander =
        {
            "...OOO..",
            "..SSOOO.",
            ".SSSSO..",
            ".SWwSO..",
            ".SW.WSO.",
            "..SSSOO.",
            "..SS.OY.",
            "..SS..Y.",
        };
        public static readonly string[] Charmeleon =
        {
            "..MOOOO.",
            ".MMSOOOO",
            ".MSSSOOM",
            ".SWwSOOM",
            ".SW.WSOM",
            "..SSSOOO",
            "...SS.YY",
            "...S..YY",
        };
        public static readonly string[] Charizard =
        {
            ".MMOOOOO",
            "MMMSOOOO",
            "MMSSOOOO",
            "MSWwSooo",
            "MSW.Wsoo",
            "MMSSSooo",
            ".MSSYYYY",
            ".MS.YYYY",
        };
        public static readonly string[] Squirtle =
        {
            "..TTTT..",
            ".TTTTTT.",
            "TTWWWWTT",
            "TTW..WTT",
            "NNNttNNN",
            ".NTttTN.",
            "..NN.NN.",
            ".TT...TT",
        };
        public static readonly string[] Wartortle =
        {
            "..CCTT..",
            ".CTTTTC.",
            "CTTWWWTC",
            "CTW..WTC",
            "NNCttCNN",
            ".NCttCN.",
            ".CN..NC.",
            ".TT..TT.",
        };
        public static readonly string[] Blastoise =
        {
            ".CTTTTC.",
            "CTTTTTCC",
            "CTTWWWCC",
            "CTW..WCC",
            "NNNttCCC",
            ".NTttCCC",
            ".NN..NN.",
            ".TT..TT.",
        };
        public static readonly string[] Rattata =
        {
            "......P.",
            "....PPP.",
            "...PWWP.",
            ".PPW.WP.",
            "PPPPPppp",
            ".P.P.PP.",
            "..P..P..",
            "...PPP..",
        };
        public static readonly string[] Pidgey =
        {
            "...YNN..",
            "..yNNNY.",
            ".yNNNNYY",
            ".NWwNNNy",
            ".NW.wNNy",
            ".NNNNNN.",
            "..NY.YN.",
            "..NY.YN.",
        };
        public static readonly string[] Caterpie =
        {
            "...GGG..",
            "..GGWWG.",
            ".GGW.WG.",
            "GGGlGGGG",
            ".GGGGlG.",
            "..GGGlG.",
            "...GGG..",
            "....G...",
        };
        public static readonly string[] Weedle =
        {
            "...Y....",
            "..YGG...",
            ".YGWWGG.",
            ".GGW.WGG",
            ".GGGGGGG",
            "..GGGGg.",
            "..GG.GG.",
            "..G...G.",
        };
        public static readonly string[] Zubat =
        {
            "P...P...",
            "PP..PP..",
            "PPPPPPPP",
            ".PPWWPP.",
            ".PPW.WP.",
            "..PPPP..",
            "...PP...",
            "....P...",
        };
        public static readonly string[] Geodude =
        {
            ".AAAAAA.",
            "AAAAAAAA",
            "AAwwwwAA",
            "AAw..wAA",
            "AAAAAAAA",
            "AAaAAAAA",
            "..AA.AA.",
            ".A.....A",
        };
        public static readonly string[] Ekans =
        {
            "..UUUU..",
            ".UUuUUU.",
            "UUuUuuUU",
            "UUWwuuUU",
            "UUW.wuUU",
            ".UUUUuU.",
            "..UUuU..",
            "...UuU..",
        };
        public static readonly string[] Psyduck =
        {
            "..YYYY..",
            ".YYYYYY.",
            "YYWWWWYY",
            "YYW..WYY",
            ".YYYYYY.",
            "..YYYY..",
            ".YY..YY.",
            ".YY..YY.",
        };
        public static readonly string[] Jigglypuff =
        {
            "..PPPP..",
            ".PPPPPP.",
            "PPWWWWPP",
            "PPW..WPP",
            ".PPPPPP.",
            "..PPPP..",
            ".PP..PP.",
            ".PP..PP.",
        };
        public static readonly string[] Meowth =
        {
            "..YYY...",
            ".YYYYAA.",
            ".YWWYAA.",
            ".YW.WAA.",
            ".YYYYAA.",
            ".YY.YAA.",
            ".Y...Y..",
            "..Y.Y...",
        };

        // ── Pokéball decorativa ───────────────────────────────────────────
        public static readonly string[] PokeballGrande =
        {
            "..RRRR..",
            ".RRRRRR.",
            "RRRRRRRR",
            "RRKKKKKR",
            "WWKKKKKW",
            "WWWWWWWW",
            ".WWWWWW.",
            "..WWWW..",
        };

        // ══════════════════════════════════════════════════════════════════
        //  MÉTODO DE DIBUJO
        // ══════════════════════════════════════════════════════════════════

        /// <summary>
        /// Dibuja un sprite en las coordenadas dadas.
        /// blockSize = tamaño en píxeles de cada bloque.
        /// espejear = voltear horizontalmente.
        /// </summary>
        public static void Dibujar(Graphics g, string[] sprite,
            int x, int y, int blockSize = 4, bool espejear = false)
        {
            for (int row = 0; row < sprite.Length; row++)
            {
                string linea = sprite[row];
                for (int col = 0; col < linea.Length; col++)
                {
                    char c = espejear ? linea[linea.Length - 1 - col] : linea[col];
                    if (!Paleta.TryGetValue(c, out Color color)) continue;
                    if (color == Color.Transparent) continue;

                    using var br = new SolidBrush(color);
                    g.FillRectangle(br,
                        x + col * blockSize,
                        y + row * blockSize,
                        blockSize, blockSize);
                }
            }
        }

        /// <summary>Obtiene el sprite de un Pokémon por su nombre.</summary>
        public static string[] ObtenerSprite(string nombre) => nombre switch
        {
            "Bulbasaur" => Bulbasaur,
            "Ivysaur" => Ivysaur,
            "Venusaur" => Venusaur,
            "Charmander" => Charmander,
            "Charmeleon" => Charmeleon,
            "Charizard" => Charizard,
            "Squirtle" => Squirtle,
            "Wartortle" => Wartortle,
            "Blastoise" => Blastoise,
            "Rattata" or "Raticate" => Rattata,
            "Pidgey" or "Pidgeotto" => Pidgey,
            "Caterpie" or "Metapod" => Caterpie,
            "Weedle" or "Kakuna" => Weedle,
            "Zubat" or "Golbat" => Zubat,
            "Geodude" or "Graveler" => Geodude,
            "Ekans" or "Arbok" => Ekans,
            "Psyduck" or "Golduck" => Psyduck,
            "Jigglypuff" or "Wigglytuff" => Jigglypuff,
            "Meowth" or "Persian" => Meowth,
            _ => Rattata,
        };
    }
}