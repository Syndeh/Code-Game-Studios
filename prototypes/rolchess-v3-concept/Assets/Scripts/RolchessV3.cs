// PROTOTYPE - NOT FOR PRODUCTION
// Question: With the "Modificador" (a 5-face die whose odds depend on how the match is going,
//           shown before rolling), does the die stop deciding matches while comebacks stay
//           possible? Also: circular range, split move/attack timer, archers on walls.
// Date: 2026-11-17 (built on rolchess-v2-concept, 2026-11-13)
//
// Hotseat 2 players, drawn entirely with IMGUI. Press Play in any scene: it creates itself.

using System.Collections.Generic;
using UnityEngine;

public class RolchessV3 : MonoBehaviour
{
    // ---------- Tuning (hardcoded on purpose: prototype) ----------
    const int MaxMoves = 12;
    const int WallLife = 30;
    const int BoostAmount = 2;            // Grito de guerra: +2 attack for that turn
    const int MaxBoostCharges = 4;
    const int WallArcherShield = 2;       // archer standing on its own wall
    const float SlowdownAfter = 600f;     // after 10 min the turn timer gets shorter
    static readonly int[] MoveTime   = { 60, 40 };   // seconds: before / after SlowdownAfter
    static readonly int[] AttackTime = { 30, 20 };
    const int BreachesToLoseRecruit = 1; // recruiting needs every wall standing
    const float AutoDelay = 2f;
    const float DieOverlayTime = 2.5f;

    enum Kind { Soldado, Arquero, Escudero, Catapulta, Jinete, Demoledor, Rey }

    static readonly string[] Names   = { "Soldado", "Arquero", "Escudero", "Catapulta", "Jinete", "Demoledor", "Rey" };
    static readonly string[] Letters = { "S", "A", "E", "C", "J", "D", "R" };
    static readonly int[] Mov    = { 4, 4, 4, 2, 6, 6, 6 };
    static readonly int[] RMin   = { 1, 1, 1, 2, 1, 1, 1 };
    static readonly int[] RMax   = { 1, 6, 1, 6, 1, 1, 1 };
    static readonly int[] Atk    = { 8, 6, 4, 10, 8, 30, 8 };
    static readonly int[] Shield = { 2, 2, 5, 3, 3, 1, 5 };
    static readonly int[] Life   = { 25, 20, 15, 15, 20, 10, 30 };
    static readonly int[] Limit  = { 12, 16, 12, 8, 8, 4, 1 };

    // The "Modificador": a 5-face die rolled automatically at the start of each turn.
    static readonly string[] FaceNames = { "Distracción", "Grito de guerra", "Llamada al servicio", "Cuerno de guerra", "Arremetida" };
    static readonly string[] FaceText =
    {
        "pierdes el turno",
        "+1 carga: +2 de ataque a todas tus piezas durante el turno en que la uses (hasta 4)",
        "reclutas 4 piezas comunes: soldados, arqueros o escuderos",
        "reclutas especiales: 4 jinetes, 2 catapultas o 1 demoledor",
        "mueves todas las piezas que quieras, sin límite",
    };
    static string FaceLine(int face) => $"{face}. {FaceNames[face - 1]}: {FaceText[face - 1]}";

    // Odds (%) per face 1..5, chosen by comparing your pieces (King included) with the rival's.
    enum Band { Normal, Losing23, Losing13, Winning32, Winning31 }
    static readonly string[] BandNames = { "Normal", "Pierde (2/3 o menos)", "Pierde (1/3 o menos)", "Gana (3/2 o más)", "Gana (3/1 o más)" };
    static readonly int[,] Odds =
    {
        { 20, 20, 20, 20, 20 },   // normal
        { 20, 20, 25, 20, 15 },   // losing with 2/3 of the rival's pieces (revised 2026-11-17)
        { 15, 20, 25, 25, 15 },   // losing with 1/3
        { 20, 25, 15, 20, 20 },   // winning with 3/2
        { 20, 25, 15, 15, 25 },   // winning with 3/1
    };

    class Piece
    {
        public Kind kind;
        public int owner;
        public Vector2Int pos;
        public int life;
        public bool moved;
    }

    class Plan
    {
        public Piece target;
        public bool isWall;
        public bool area;      // catapult: hits `wall` (any cell) and every cell around it
        public Vector2Int wall;
        public bool explode;
    }

    struct MoveRecord
    {
        public Piece piece;
        public Vector2Int from;
        public bool counted;
    }

    enum Phase { Setup, Roll, LostTurn, Recruit, Move, Attack, Over }
    enum KingMode { None, Exit, Enter }
    enum AutoAction { None, EndLost, ToAttack, ResolveAndEnd }

    // ---------- Setup options ----------
    bool hex;
    bool castles = true;
    bool castleRandom;   // castles in a random pair of sectors instead of 4 and 6
    // Allowed castle sectors (blue, red), keypad numbering. Index 0 is the default.
    static readonly int[,] SectorPairs = { { 4, 6 }, { 1, 9 }, { 7, 3 } };   // 2-8 does not fit: 15 rows leave no room between the castles
    int pairIdx, forcedPair = -1;
    readonly Vector2Int[] home = new Vector2Int[2];   // King's starting cell (castle centre)
    readonly Vector2Int[] face = new Vector2Int[2];   // direction towards the rival
    bool useTimer = true;
    const int BoardW = 24, BoardH = 15;   // wide board for a 16:9 screen

    // ---------- State ----------
    int W, H;
    float stageStart;   // when the current move or attack stage started (timer)
    readonly List<Piece> pieces = new List<Piece>();
    readonly Dictionary<Vector2Int, Piece> occ = new Dictionary<Vector2Int, Piece>();
    readonly Dictionary<Vector2Int, int> wallLife = new Dictionary<Vector2Int, int>();
    readonly Dictionary<Vector2Int, int> wallOwner = new Dictionary<Vector2Int, int>();
    readonly HashSet<Vector2Int>[] interior = { new HashSet<Vector2Int>(), new HashSet<Vector2Int>() };
    readonly HashSet<Vector2Int>[] castleCells = { new HashSet<Vector2Int>(), new HashSet<Vector2Int>() };
    readonly int[] breaches = new int[2];
    readonly int[] boostCharges = new int[2];
    readonly bool[] boostOn = new bool[2];   // Grito de guerra in use: +2 attack, only during that player's turn
    Band rollBand;                           // odds table used for the last roll
    bool recruitSpecial;                     // die 3 = basic troops, die 4 = special pieces

    int current;
    Phase phase = Phase.Setup;
    int die;
    float dieShownAt = -100f;
    int movesUsed;
    bool unlimitedMoves;

    Piece selected;
    HashSet<Vector2Int> reach = new HashSet<Vector2Int>();
    KingMode kingMode;
    HashSet<Vector2Int> kingCands = new HashSet<Vector2Int>();
    readonly List<MoveRecord> undo = new List<MoveRecord>();
    readonly Dictionary<Piece, Plan> plans = new Dictionary<Piece, Plan>();

    AutoAction autoAction;
    float autoAt = -1f;
    string message = "";
    readonly List<string> log = new List<string>();
    int winner = -1;

    // ---------- Measurement ----------
    float matchStart, turnStart, matchEnd;
    readonly List<float>[] turnTimes = { new List<float>(), new List<float>() };
    int lostTurns, moveTimeouts, attackTimeouts, turnNumber;
    readonly int[] faceCount = new int[5];   // how often each face came up
    readonly int[] bandCount = new int[5];   // how often each odds table was used
    readonly List<string> rollLog = new List<string>();   // per turn: player, table, face
    int attackTurns, combinedTurns, decisiveTurns;   // turns with any attack / 2+ pieces on one enemy target / where combining decided it

    // ---------- Drawing ----------
    Vector2 origin = new Vector2(10, 10);
    float cs = 30f;
    Texture2D hexTex;
    GUIStyle centerStyle, richStyle, statStyle, badgeStyle;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Boot()
    {
        if (FindAnyObjectByType<RolchessV3>() != null) return;
        var cam = Camera.main;
        if (cam == null) cam = new GameObject("Camera").AddComponent<Camera>();
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = new Color(0.12f, 0.12f, 0.14f);
        new GameObject("RolchessV3").AddComponent<RolchessV3>();
    }

    void Awake()
    {
        hexTex = MakeHexTexture(74, 64);
    }

    static Texture2D MakeHexTexture(int w, int h)
    {
        var t = new Texture2D(w, h, TextureFormat.RGBA32, false) { wrapMode = TextureWrapMode.Clamp, filterMode = FilterMode.Bilinear };
        // Flat-top hexagon: vertices left and right, flat edges up and down.
        float R = w / 2f, half = R * 0.8660254f;
        var px = new Color32[w * h];
        for (int y = 0; y < h; y++)
            for (int x = 0; x < w; x++)
            {
                float dx = Mathf.Abs(x + 0.5f - w / 2f), dy = Mathf.Abs(y + 0.5f - h / 2f);
                bool inside = dy <= half && dx <= R - dy / 1.7320508f;
                px[y * w + x] = inside ? new Color32(255, 255, 255, 255) : new Color32(255, 255, 255, 0);
            }
        t.SetPixels32(px);
        t.Apply();
        return t;
    }

    // ================= Setup =================
    void BuildMatch()
    {
        W = BoardW; H = BoardH;
        pieces.Clear(); wallLife.Clear(); wallOwner.Clear();
        for (int p = 0; p < 2; p++) { interior[p].Clear(); castleCells[p].Clear(); breaches[p] = 0; boostCharges[p] = 0; boostOn[p] = false; }

        // The board is split in 9 sectors numbered like a phone keypad (1 top-left ... 9 bottom-right).
        // Without the random option the players sit in 4 and 6. Same logic with or without castles.
        pairIdx = castleRandom ? (forcedPair >= 0 ? forcedPair : Random.Range(0, SectorPairs.GetLength(0))) : 0;
        for (int p = 0; p < 2; p++)
        {
            int sector = SectorPairs[pairIdx, p] - 1, sc = sector % 3, sr = sector / 3;
            int edge = castles ? 2 : 1;   // castle centre sits 2 cells from the border, a lone King 1
            home[p] = new Vector2Int(sc == 0 ? edge : sc == 1 ? W / 2 : W - 1 - edge, sr == 0 ? edge : sr == 1 ? H / 2 : H - 1 - edge);
        }
        for (int p = 0; p < 2; p++)
        {
            // Face the rival: sideways, up/down (1-7, 3-9, 2-8) or diagonally (1-9, 7-3).
            var d = home[1 - p] - home[p];
            face[p] = new Vector2Int(d.x > 0 ? 1 : d.x < 0 ? -1 : 0, d.y > 0 ? 1 : d.y < 0 ? -1 : 0);
        }
        attackTurns = combinedTurns = decisiveTurns = 0;
        for (int p = 0; p < 2; p++)
        {
            Vector2Int king = home[p];
            if (castles)
                for (int x = 0; x < W; x++)
                    for (int y = 0; y < H; y++)
                    {
                        // Square castle: 5x5 block, the border is the wall. Hex castle: the King's
                        // 6 neighbours are the interior and the ring at distance 2 is the wall.
                        var v = new Vector2Int(x, y);
                        int d = hex ? Dist(v, king) : Mathf.Max(Mathf.Abs(x - king.x), Mathf.Abs(y - king.y));
                        if (d > 2) continue;
                        castleCells[p].Add(v);
                        if (d == 2) { wallLife[v] = WallLife; wallOwner[v] = p; }
                        else interior[p].Add(v);
                    }
            Add(Kind.Rey, p, king);

            // 3 lines of 4, back to front: archers, soldiers, escuderos, facing the rival.
            // They start far apart so there is room to approach: against the castle wall,
            // or near the King (one free line in between) when there are no castles.
            Kind[] ranks = { Kind.Arquero, Kind.Soldado, Kind.Escudero };
            int back = castles ? 3 : 2;
            var f = face[p];
            var side = new Vector2Int(Mathf.Abs(f.y), Mathf.Abs(f.x));   // same rows/columns for both players
            // Keep the 4-wide line on the board when a lone King sits right next to the edge.
            int perp = side.x != 0 ? king.x : king.y, lim = side.x != 0 ? W : H;
            int shift = Mathf.Clamp(perp - 2, 0, lim - 4) - (perp - 2);
            for (int c = 0; c < 3; c++)
                for (int i = 0; i < 4; i++)
                {
                    Vector2Int cell;
                    if (f.x == 0 || f.y == 0) cell = king + f * (back + c) + side * (i - 2 + shift);
                    else if (hex) cell = HexDiagonalCell(king, f, back + c, i - 2);
                    else
                    {
                        // Diagonal lines across the corner, symmetric around the diagonal to the rival.
                        int b = castles ? 1 : 0;
                        cell = king + new Vector2Int(f.x * (b + c + i), f.y * (b + 3 + c - i));
                    }
                    Add(ranks[c], p, cell);
                }
        }
        RebuildOcc();
        current = 0;
        turnNumber = 0; lostTurns = 0; moveTimeouts = 0; attackTimeouts = 0;
        System.Array.Clear(faceCount, 0, 5); System.Array.Clear(bandCount, 0, 5); rollLog.Clear();
        turnTimes[0].Clear(); turnTimes[1].Clear();
        log.Clear();
        winner = -1;
        matchStart = Time.realtimeSinceStartup;
        StartTurn();
    }

    // Hex boards have no 45° line: advance along the hex direction closest to the diagonal
    // and lay each line along the mirrored one, which gives a rhombus pointing at the rival.
    static Vector2Int HexDiagonalCell(Vector2Int king, Vector2Int f, int forward, int along)
    {
        Vector2Int Axis(int sx, int sy) => new Vector2Int(sx, sx > 0 ? (sy > 0 ? 0 : -1) : (sy > 0 ? 1 : 0));
        var fw = Axis(f.x, f.y); var al = Axis(f.x, -f.y);
        int q = king.x + fw.x * forward + al.x * along;
        int r = king.y - (king.x - (king.x & 1)) / 2 + fw.y * forward + al.y * along;
        return new Vector2Int(q, r + (q - (q & 1)) / 2);
    }

    void Add(Kind k, int owner, Vector2Int pos)
    {
        pieces.Add(new Piece { kind = k, owner = owner, pos = pos, life = Life[(int)k] });
    }

    void RebuildOcc()
    {
        occ.Clear();
        foreach (var p in pieces) occ[p.pos] = p;
    }

    // ================= Geometry =================
    bool InBoard(Vector2Int v) => v.x >= 0 && v.y >= 0 && v.x < W && v.y < H;
    bool IsWall(Vector2Int v) => wallLife.ContainsKey(v);
    Piece PieceAt(Vector2Int v) => occ.TryGetValue(v, out var p) ? p : null;

    static Vector3Int Cube(Vector2Int v)
    {
        // Flat-top hexes in vertical columns; odd columns sit half a cell lower.
        int q = v.x;
        int r = v.y - (v.x - (v.x & 1)) / 2;
        return new Vector3Int(q, r, -q - r);
    }

    int Dist(Vector2Int a, Vector2Int b)
    {
        if (!hex) return Mathf.Max(Mathf.Abs(a.x - b.x), Mathf.Abs(a.y - b.y));
        var ca = Cube(a); var cb = Cube(b);
        return Mathf.Max(Mathf.Abs(ca.x - cb.x), Mathf.Max(Mathf.Abs(ca.y - cb.y), Mathf.Abs(ca.z - cb.z)));
    }

    static readonly Vector2Int[] Orth = { new Vector2Int(1, 0), new Vector2Int(-1, 0), new Vector2Int(0, 1), new Vector2Int(0, -1) };
    static readonly Vector2Int[] Eight =
    {
        new Vector2Int(1, 0), new Vector2Int(-1, 0), new Vector2Int(0, 1), new Vector2Int(0, -1),
        new Vector2Int(1, 1), new Vector2Int(1, -1), new Vector2Int(-1, 1), new Vector2Int(-1, -1),
    };
    static readonly Vector2Int[] HexEven = { new Vector2Int(0, -1), new Vector2Int(0, 1), new Vector2Int(1, 0), new Vector2Int(1, -1), new Vector2Int(-1, 0), new Vector2Int(-1, -1) };
    static readonly Vector2Int[] HexOdd  = { new Vector2Int(0, -1), new Vector2Int(0, 1), new Vector2Int(1, 0), new Vector2Int(1, 1), new Vector2Int(-1, 0), new Vector2Int(-1, 1) };

    IEnumerable<Vector2Int> MoveNeighbors(Vector2Int v)
    {
        var dirs = hex ? ((v.x & 1) == 1 ? HexOdd : HexEven) : Orth;
        foreach (var d in dirs) { var n = v + d; if (InBoard(n)) yield return n; }
    }

    IEnumerable<Vector2Int> Adjacent(Vector2Int v)
    {
        var dirs = hex ? ((v.x & 1) == 1 ? HexOdd : HexEven) : Eight;
        foreach (var d in dirs) { var n = v + d; if (InBoard(n)) yield return n; }
    }

    // ================= Rules helpers =================
    HashSet<Vector2Int> ComputeReach(Piece p)
    {
        var res = new HashSet<Vector2Int>();
        var dist = new Dictionary<Vector2Int, int> { [p.pos] = 0 };
        var q = new Queue<Vector2Int>();
        q.Enqueue(p.pos);
        int max = Mov[(int)p.kind];
        while (q.Count > 0)
        {
            var c = q.Dequeue();
            int d = dist[c];
            if (d >= max) continue;
            foreach (var n in MoveNeighbors(c))
            {
                if (dist.ContainsKey(n)) continue;
                if (IsWall(n))
                {
                    // Archers can climb onto a free segment of their own wall; it ends the move
                    // there (a wall is never walked through).
                    if (p.kind == Kind.Arquero && wallOwner[n] == p.owner && PieceAt(n) == null)
                    { dist[n] = d + 1; res.Add(n); }
                    continue;
                }
                var o = PieceAt(n);
                if (o != null && o.owner != p.owner) continue; // enemies block
                dist[n] = d + 1;
                q.Enqueue(n);                                  // allies can be passed through
                if (o == null) res.Add(n);                      // but not stopped on
            }
        }
        return res;
    }

    bool CanSelectToMove(Piece p) =>
        p.owner == current && !p.moved && (p.kind == Kind.Rey || unlimitedMoves || movesUsed < MaxMoves);

    // Cells right outside a castle (the "lado del castillo, a un cuadro").
    List<Vector2Int> CastleRing(int owner)
    {
        var res = new List<Vector2Int>();
        for (int x = 0; x < W; x++)
            for (int y = 0; y < H; y++)
            {
                var v = new Vector2Int(x, y);
                if (castleCells[owner].Contains(v) || IsWall(v)) continue;
                foreach (var n in Adjacent(v))
                    if (castleCells[owner].Contains(n)) { res.Add(v); break; }
            }
        return res;
    }

    // Lower is better: as far towards the rival as possible, then close to the King's line.
    float FrontScore(int owner, Vector2Int v)
    {
        var d = v - home[owner]; var f = face[owner];
        return -(d.x * f.x + d.y * f.y) * 100f + Mathf.Abs(d.x * f.y - d.y * f.x);
    }

    List<Vector2Int> SpawnCells(int owner)
    {
        List<Vector2Int> cand;
        if (castles) cand = CastleRing(owner);
        else
        {
            cand = new List<Vector2Int>();
            var king = pieces.Find(p => p.owner == owner && p.kind == Kind.Rey);
            if (king != null)
                for (int x = 0; x < W; x++)
                    for (int y = 0; y < H; y++)
                    {
                        var v = new Vector2Int(x, y);
                        if (Dist(v, king.pos) <= 2) cand.Add(v);
                    }
        }
        cand.RemoveAll(v => PieceAt(v) != null || IsWall(v));
        cand.Sort((a, b) => FrontScore(owner, a).CompareTo(FrontScore(owner, b)));
        return cand;
    }

    bool CanRecruit(int owner) => !castles || breaches[owner] < BreachesToLoseRecruit;

    // Square board: a round zone. A cell is in range when dx² + dy² <= max² + 1 (with max 6 that
    // is 37: (1,6) and (4,4) are in, (2,6) and (4,5) are out, matching the player's sketch).
    // The minimum works the same way: the catapult (min 2) cannot hit dx² + dy² <= 1² + 1, its
    // 8 neighbours. With range 1 the zone is the 8 neighbours, as before.
    bool InRange(Piece a, Vector2Int t)
    {
        int min = RMin[(int)a.kind], max = RMax[(int)a.kind];
        if (hex)
        {
            int d = Dist(a.pos, t);
            return d >= min && d <= max;
        }
        int dx = t.x - a.pos.x, dy = t.y - a.pos.y, d2 = dx * dx + dy * dy;
        if (d2 == 0 || d2 > max * max + 1) return false;
        return min <= 1 || d2 > (min - 1) * (min - 1) + 1;
    }

    bool HasTargets(Piece a)
    {
        if (a.kind == Kind.Demoledor)
        {
            foreach (var n in Adjacent(a.pos))
            {
                var o = PieceAt(n);
                if (o != null && o.owner != a.owner) return true;
                if (IsWall(n) && wallOwner[n] != a.owner) return true;
            }
            return false;
        }
        if (a.kind == Kind.Catapulta)
        {
            // Worth shooting if some cell in range has an enemy piece or wall inside its area.
            foreach (var p in pieces)
                if (p.owner != a.owner) foreach (var c in AreaCells(p.pos)) if (InRange(a, c)) return true;
            foreach (var kv in wallLife)
                if (wallOwner[kv.Key] != a.owner) foreach (var c in AreaCells(kv.Key)) if (InRange(a, c)) return true;
            return false;
        }
        foreach (var p in pieces) if (p.owner != a.owner && InRange(a, p.pos)) return true;
        foreach (var kv in wallLife) if (wallOwner[kv.Key] != a.owner && InRange(a, kv.Key)) return true;
        return false;
    }

    bool IsValidTarget(Piece a, Vector2Int cell)
    {
        if (a.kind == Kind.Demoledor) return false; // the demolisher explodes instead
        if (a.kind == Kind.Catapulta) return InBoard(cell) && InRange(a, cell); // any cell, even an empty one
        var o = PieceAt(cell);
        if (o != null) return o.owner != a.owner && InRange(a, cell);
        if (IsWall(cell)) return wallOwner[cell] != a.owner && InRange(a, cell);
        return false;
    }

    int AttackValue(Piece a)
    {
        int v = Atk[(int)a.kind];
        if (boostOn[a.owner]) v += BoostAmount;
        return v;
    }

    bool OnWall(Piece p) => IsWall(p.pos);

    int ShieldValue(Piece p) => Shield[(int)p.kind] + (OnWall(p) ? WallArcherShield : 0);

    // Cells hit by a catapult shot: the chosen cell and every cell around it.
    IEnumerable<Vector2Int> AreaCells(Vector2Int c)
    {
        yield return c;
        foreach (var n in Adjacent(c)) yield return n;
    }

    // Sums every planned attack per target. Shield is applied once per target later.
    // A hit on a cell lands on whatever is there: an archer standing on a wall and the wall
    // segment under it both take it (the archer subtracts its shield, the wall has none).
    void IncomingDamage(Dictionary<Piece, int> toPieces, Dictionary<Vector2Int, int> toWalls)
    {
        void HitCell(Vector2Int cell, int v)
        {
            var o = PieceAt(cell);
            if (o != null) Accumulate(toPieces, o, v);
            if (IsWall(cell)) Accumulate(toWalls, cell, v);
        }
        foreach (var kv in plans)
        {
            var a = kv.Key; var pl = kv.Value;
            if (pl.explode)
                foreach (var n in Adjacent(a.pos)) HitCell(n, AttackValue(a));
            else if (pl.area)
                // Full damage on the chosen cell, half (rounded down) on the cells around it.
                foreach (var n in AreaCells(pl.wall)) HitCell(n, n == pl.wall ? AttackValue(a) : AttackValue(a) / 2);
            else HitCell(pl.isWall ? pl.wall : pl.target.pos, AttackValue(a));
        }
    }

    static void Accumulate<T>(Dictionary<T, int> d, T k, int v)
    {
        d.TryGetValue(k, out int cur);
        d[k] = cur + v;
    }

    // ================= Turn flow =================
    void StartTurn()
    {
        turnNumber++;
        phase = Phase.Roll;
        movesUsed = 0;
        unlimitedMoves = false;
        boostOn[0] = boostOn[1] = false; // Grito de guerra only lasts the turn it is used in
        foreach (var p in pieces) p.moved = false;
        undo.Clear();
        plans.Clear();
        ClearSelection();
        autoAction = AutoAction.None; autoAt = -1f;
        turnStart = stageStart = Time.realtimeSinceStartup;
        Roll(); // the die rolls by itself; the overlay shows the result
    }

    int PieceCount(int owner)
    {
        int n = 0;
        foreach (var p in pieces) if (p.owner == owner) n++;
        return n;
    }

    // Compare your pieces (King included, walls not) with the rival's. Integer maths, no floats.
    Band BandFor(int owner)
    {
        int mine = PieceCount(owner), theirs = PieceCount(1 - owner);
        if (mine * 3 <= theirs) return Band.Losing13;
        if (mine * 3 <= theirs * 2) return Band.Losing23;
        if (mine >= theirs * 3) return Band.Winning31;
        if (mine * 2 >= theirs * 3) return Band.Winning32;
        return Band.Normal;
    }

    void Roll()
    {
        rollBand = BandFor(current);
        int r = Random.Range(0, 100), acc = 0;
        die = 5;
        for (int f = 0; f < 5; f++) { acc += Odds[(int)rollBand, f]; if (r < acc) { die = f + 1; break; } }
        faceCount[die - 1]++;
        bandCount[(int)rollBand]++;
        rollLog.Add($"T{turnNumber} {(current == 0 ? "Azul" : "Rojo")} {PieceCount(current)} vs {PieceCount(1 - current)} · {BandNames[(int)rollBand]} → {die} {FaceNames[die - 1]}");
        dieShownAt = Time.realtimeSinceStartup;
        message = FaceLine(die);
        switch (die)
        {
            case 1: phase = Phase.LostTurn; break;
            case 2: boostCharges[current] = Mathf.Min(MaxBoostCharges, boostCharges[current] + 1); phase = Phase.Move; break;
            case 3:
            case 4:
                recruitSpecial = die == 4;
                if (CanRecruit(current)) phase = Phase.Recruit;
                else { phase = Phase.Move; message = $"{die}. {FaceNames[die - 1]}: tu castillo tiene un muro derribado, no puedes reclutar."; }
                break;
            default: unlimitedMoves = true; phase = Phase.Move; break;   // 5: Arremetida
        }
    }

    // Timer: move stage then attack stage, each with its own limit; both get shorter after 10 min.
    int StageLimit()
    {
        int i = Time.realtimeSinceStartup - matchStart >= SlowdownAfter ? 1 : 0;
        return phase == Phase.Attack ? AttackTime[i] : MoveTime[i];
    }

    void EnterAttack(string why = null)
    {
        phase = Phase.Attack;
        stageStart = Time.realtimeSinceStartup;
        ClearSelection();
        if (why != null) message = why;
    }

    // Without castles there are no walls to bring down, so demolishers cannot be recruited.
    bool CanRecruitKind(Kind k) => castles || k != Kind.Demoledor;
    static bool IsSpecial(Kind k) => k == Kind.Jinete || k == Kind.Catapulta || k == Kind.Demoledor;
    static int RecruitCount(Kind k) => k == Kind.Demoledor ? 1 : k == Kind.Catapulta ? 2 : 4;   // demolishers one at a time, catapults in pairs

    void Recruit(Kind k)
    {
        if (!CanRecruitKind(k) || IsSpecial(k) != recruitSpecial) return;
        int alive = 0;
        foreach (var p in pieces) if (p.owner == current && p.kind == k) alive++;
        int n = Mathf.Min(RecruitCount(k), Limit[(int)k] - alive);
        var cells = SpawnCells(current);
        int placed = 0;
        for (int i = 0; i < cells.Count && placed < n; i++) { Add(k, current, cells[i]); placed++; }
        RebuildOcc();
        message = $"Reclutaste {placed} {Names[(int)k]}.";
        phase = Phase.Move;
    }

    void EndTurn(bool lost)
    {
        float t = Time.realtimeSinceStartup - turnStart;
        if (lost) lostTurns++;
        else turnTimes[current].Add(t);
        boostOn[current] = false;
        Debug.Log($"[RolchessV3] Turno {turnNumber} J{current + 1}: {t:0.0}s{(lost ? " (perdido)" : "")}, movidas {movesUsed}");
        current = 1 - current;
        StartTurn();
    }

    void ResolveAll()
    {
        if (plans.Count == 0) return;
        var toPieces = new Dictionary<Piece, int>();
        var toWalls = new Dictionary<Vector2Int, int>();
        IncomingDamage(toPieces, toWalls);
        log.Clear();

        // Measurement. "Combined" = an enemy piece or wall hit by 2+ attackers. "Decisive" is the
        // strict one: the sum kills (or breaks the wall) where no single attacker would, or the
        // sum gets through a defense that every single attacker bounces off.
        var hits = new Dictionary<object, List<int>>();
        foreach (var kv in plans)
        {
            var a = kv.Key; var pl = kv.Value;
            IEnumerable<Vector2Int> cells = pl.explode ? Adjacent(a.pos) : pl.area ? AreaCells(pl.wall)
                                          : new[] { pl.isWall ? pl.wall : pl.target.pos };
            foreach (var c in cells)
            {
                int v = pl.area && c != pl.wall ? AttackValue(a) / 2 : AttackValue(a);
                var o = PieceAt(c);
                object key = o != null && o.owner != a.owner ? o : IsWall(c) && wallOwner[c] != a.owner ? (object)c : null;
                if (key == null) continue;
                if (!hits.TryGetValue(key, out var list)) hits[key] = list = new List<int>();
                list.Add(v);
            }
        }
        attackTurns++;
        bool anyCombined = false, anyDecisive = false;
        foreach (var kv in hits)
        {
            if (kv.Value.Count < 2) continue;
            anyCombined = true;
            int sum = 0, best = 0;
            foreach (int v in kv.Value) { sum += v; best = Mathf.Max(best, v); }
            int shield = kv.Key is Piece tp ? ShieldValue(tp) : 0;
            int life = kv.Key is Piece tp2 ? tp2.life : wallLife[(Vector2Int)kv.Key];
            bool kills = sum - shield >= life && best - shield < life;
            bool pierces = sum > shield && best <= shield;
            if (kills || pierces) anyDecisive = true;
        }
        if (anyCombined) combinedTurns++;
        if (anyDecisive) decisiveTurns++;

        foreach (var kv in toPieces)
        {
            var t = kv.Key;
            int sh = ShieldValue(t);
            int dmg = Mathf.Max(0, kv.Value - sh);
            t.life -= dmg;
            log.Add($"{(t.owner == 0 ? "Azul" : "Rojo")} {Names[(int)t.kind]}: {kv.Value} − esc {sh} = {dmg}{(t.life <= 0 ? " MUERE" : $" (queda {t.life})")}");
        }
        foreach (var kv in toWalls)
        {
            wallLife[kv.Key] -= kv.Value;
            bool broke = wallLife[kv.Key] <= 0;
            log.Add($"Muro {(wallOwner[kv.Key] == 0 ? "azul" : "rojo")}: −{kv.Value}{(broke ? " ¡roto!" : $" (queda {wallLife[kv.Key]})")}");
            if (broke)
            {
                breaches[wallOwner[kv.Key]]++;
                wallLife.Remove(kv.Key);
                wallOwner.Remove(kv.Key);
            }
        }
        foreach (var kv in plans)
            if (kv.Value.explode) kv.Key.life = 0;

        pieces.RemoveAll(p => p.life <= 0);
        plans.Clear();
        RebuildOcc();

        bool k0 = pieces.Exists(p => p.owner == 0 && p.kind == Kind.Rey);
        bool k1 = pieces.Exists(p => p.owner == 1 && p.kind == Kind.Rey);
        if (!k0 || !k1)
        {
            winner = (!k0 && !k1) ? current : (k0 ? 0 : 1);
            phase = Phase.Over;
            matchEnd = Time.realtimeSinceStartup;
            turnTimes[current].Add(matchEnd - turnStart);
            Debug.Log($"[RolchessV3] Ganó J{winner + 1}. {(hex ? "hex" : "cuadrado")}, {(castles ? "con" : "sin")} castillos, {W}x{H}, " +
                      $"timer {(useTimer ? "sí" : "no")}. Partida {Fmt(matchEnd - matchStart)}, turnos {turnNumber}, " +
                      $"promedio J1 {Fmt(Avg(turnTimes[0]))}, J2 {Fmt(Avg(turnTimes[1]))}, perdidos {lostTurns}, " +
                      $"tiempo agotado: mover {moveTimeouts}, atacar {attackTimeouts}, " +
                      $"turnos con ataque {attackTurns}, con ataque combinado {combinedTurns}, combinado decisivo {decisiveTurns}, " +
                      $"caras {string.Join("/", faceCount)}, tablas {string.Join("/", bandCount)}\n" + string.Join("\n", rollLog));
        }
    }

    void ClearSelection()
    {
        selected = null;
        reach.Clear();
        kingMode = KingMode.None;
        kingCands.Clear();
    }

    // ================= Auto actions and timer =================
    void Update()
    {
        if (phase == Phase.Setup || phase == Phase.Over) return;
        float now = Time.realtimeSinceStartup;

        // Move time runs out → attack stage. Attack time runs out → resolve what is assigned
        // and end the turn. Recruiting counts as move time.
        if (useTimer && (phase == Phase.Recruit || phase == Phase.Move || phase == Phase.Attack)
            && now - stageStart >= StageLimit())
        {
            if (phase == Phase.Attack)
            {
                attackTimeouts++;
                message = "Se acabó el tiempo de ataque: se resuelven los ataques asignados.";
                ResolveAll();
                if (phase != Phase.Over) EndTurn(false);
            }
            else
            {
                moveTimeouts++;
                EnterAttack("Se acabó el tiempo de mover: fase de ataque.");
            }
            return;
        }

        AutoAction want = AutoAction.None;
        if (phase == Phase.LostTurn) want = AutoAction.EndLost;
        else if (phase == Phase.Move && !AnyMovable()) want = AutoAction.ToAttack;
        else if (phase == Phase.Attack)
        {
            // Only auto-end when nothing can attack. Assigned attacks wait for the player's
            // "Resolver" button: they may still want to change or drop one.
            bool anyAttack = plans.Count > 0;
            foreach (var p in pieces)
                if (p.owner == current && HasTargets(p)) { anyAttack = true; break; }
            if (!anyAttack) want = AutoAction.ResolveAndEnd;
        }

        if (want != autoAction) { autoAction = want; autoAt = want == AutoAction.None ? -1f : now + AutoDelay; }
        if (autoAction != AutoAction.None && now >= autoAt)
        {
            var a = autoAction;
            autoAction = AutoAction.None; autoAt = -1f;
            switch (a)
            {
                case AutoAction.EndLost: EndTurn(true); break;
                case AutoAction.ToAttack: EnterAttack("Sin más movimientos: fase de ataque."); break;
                case AutoAction.ResolveAndEnd: ResolveAll(); if (phase != Phase.Over) EndTurn(false); break;
            }
        }
    }

    bool AnyMovable()
    {
        foreach (var p in pieces)
        {
            if (!CanSelectToMove(p)) continue;
            if (ComputeReach(p).Count > 0) return true;
            if (p.kind == Kind.Rey && castles && (KingExitCells(p).Count > 0 || KingEnterCells(p).Count > 0)) return true;
        }
        return false;
    }

    HashSet<Vector2Int> KingExitCells(Piece king)
    {
        var res = new HashSet<Vector2Int>();
        if (!castles || !interior[king.owner].Contains(king.pos)) return res;
        foreach (var v in CastleRing(king.owner)) if (PieceAt(v) == null) res.Add(v);
        return res;
    }

    HashSet<Vector2Int> KingEnterCells(Piece king)
    {
        var res = new HashSet<Vector2Int>();
        if (!castles || interior[king.owner].Contains(king.pos)) return res;
        if (!CastleRing(king.owner).Contains(king.pos)) return res;
        foreach (var v in interior[king.owner]) if (PieceAt(v) == null) res.Add(v);
        return res;
    }

    // ================= Input =================
    void OnCellClicked(Vector2Int cell)
    {
        var p = PieceAt(cell);
        if (phase == Phase.Move)
        {
            // King: the cells to leave or enter the castle are shown right away (light blue), no
            // button needed. Doing it ends the King's movement for the turn (notebook rule).
            if (selected != null && kingCands.Contains(cell))
            {
                DoMove(selected, cell, false);
                return;
            }
            if (selected != null && reach.Contains(cell))
            {
                DoMove(selected, cell, selected.kind != Kind.Rey && !unlimitedMoves);
                return;
            }
            if (p != null && CanSelectToMove(p))
            {
                ClearSelection();
                selected = p;
                reach = ComputeReach(p);
                if (p.kind == Kind.Rey && castles)
                {
                    kingCands = KingExitCells(p);
                    kingCands.UnionWith(KingEnterCells(p));
                    kingMode = kingCands.Count == 0 ? KingMode.None : interior[p.owner].Contains(p.pos) ? KingMode.Exit : KingMode.Enter;
                }
            }
            else ClearSelection();
        }
        else if (phase == Phase.Attack)
        {
            if (selected != null)
            {
                if (cell == selected.pos)
                {
                    if (selected.kind == Kind.Demoledor && !plans.ContainsKey(selected)) plans[selected] = new Plan { explode = true };
                    else plans.Remove(selected);
                    ClearSelection();
                    return;
                }
                if (IsValidTarget(selected, cell))
                {
                    plans[selected] = selected.kind == Kind.Catapulta ? new Plan { area = true, wall = cell }
                                    : p != null ? new Plan { target = p } : new Plan { isWall = true, wall = cell };
                    ClearSelection();
                    return;
                }
            }
            if (p != null && p.owner == current && (HasTargets(p) || plans.ContainsKey(p)))
            {
                ClearSelection();
                selected = p;
            }
            else ClearSelection();
        }
    }

    void DoMove(Piece piece, Vector2Int to, bool counted)
    {
        undo.Add(new MoveRecord { piece = piece, from = piece.pos, counted = counted });
        piece.pos = to;
        piece.moved = true;
        if (counted) movesUsed++;
        RebuildOcc();
        ClearSelection();
    }

    void Undo()
    {
        if (undo.Count == 0) return;
        var r = undo[undo.Count - 1];
        undo.RemoveAt(undo.Count - 1);
        r.piece.pos = r.from;
        r.piece.moved = false;
        if (r.counted) movesUsed--;
        RebuildOcc();
        ClearSelection();
    }

    // ================= Drawing =================
    void OnGUI()
    {
        if (centerStyle == null)
        {
            centerStyle = new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter, fontStyle = FontStyle.Bold, richText = true };
            centerStyle.normal.textColor = Color.white;
            richStyle = new GUIStyle(GUI.skin.label) { richText = true, wordWrap = true };
            statStyle = new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleLeft, fontStyle = FontStyle.Bold, richText = true, wordWrap = false, clipping = TextClipping.Overflow, padding = new RectOffset(0, 0, 0, 0), margin = new RectOffset(0, 0, 0, 0) };
            statStyle.normal.textColor = Color.white;
            badgeStyle = new GUIStyle(statStyle) { alignment = TextAnchor.MiddleCenter };
        }

        if (phase == Phase.Setup) { DrawSetup(); return; }

        ComputeCellSize();
        var e = Event.current;
        if (e.type == EventType.MouseDown && e.button == 0 && phase != Phase.Over && BoardRect().Contains(e.mousePosition))
        {
            if (HitTest(e.mousePosition, out var cell)) OnCellClicked(cell);
            e.Use();
        }

        DrawBoard();
        DrawDieOverlay();

        // Right strip, split in two: blue gets the actions in the top half, red in the bottom
        // half; the other half shows the reference info.
        var b = BoardRect();
        float px = b.xMax, pw = Screen.width - px, half = Screen.height / 2f;
        var top = new Rect(px, 0, pw, half);
        var bottom = new Rect(px, half, pw, Screen.height - half);
        var mine = current == 0 ? top : bottom;
        Fill(mine, current == 0 ? new Color(0.13f, 0.30f, 0.78f, 0.20f) : new Color(0.74f, 0.17f, 0.17f, 0.20f));
        DrawPanel(Pad(mine));
        if (phase != Phase.Over) DrawInfo(Pad(current == 0 ? bottom : top));
    }

    void DrawSetup()
    {
        GUILayout.BeginArea(new Rect(Screen.width / 2f - 200, 60, 400, 460), GUI.skin.box);
        GUILayout.Label("<size=22><b>Rolchess v3: prototipo</b></size>", richStyle);
        GUILayout.Space(8);
        hex = false; // hexagonal board parked for now: the code stays, the option is hidden
        GUILayout.Label("Castillos");
        castles = GUILayout.Toolbar(castles ? 0 : 1, new[] { "Con castillos", "Sin castillos (Rey atrás)" }) == 0;
        GUILayout.Label("Posición inicial (castillos o Reyes)");
        castleRandom = GUILayout.Toolbar(castleRandom ? 1 : 0, new[] { "En el medio (4 y 6)", "Al azar (4 y 6, 1 y 9, 7 y 3)" }) == 1;
        GUILayout.Label($"Tablero: {BoardW} × {BoardH}");
        GUILayout.Label("Tiempo por turno");
        useTimer = GUILayout.Toolbar(useTimer ? 0 : 1, new[] { "60 s mover + 30 s atacar (40 + 20 desde el min 10)", "Sin límite" }) == 0;
        GUILayout.Space(16);
        if (GUILayout.Button("Empezar", GUILayout.Height(40))) BuildMatch();
        GUILayout.EndArea();
    }

    const float PanelWidth = 340f;
    Vector2 panelScroll, infoScroll;

    static Rect Pad(Rect r) => new Rect(r.x + 12, r.y + 10, r.width - 24, r.height - 20);

    // The board fills the screen to the left of the panel strip, centred vertically.
    void ComputeCellSize()
    {
        float availW = Screen.width - PanelWidth, availH = Screen.height - 4f;
        if (!hex) cs = Mathf.Max(14f, Mathf.Min(availW / W, availH / H));
        else cs = Mathf.Max(14f, Mathf.Min(availW / (((W - 1) * 0.75f + 1f) * 1.1547f), availH / (H + 0.5f)));
        origin = Vector2.zero;
        var b = BoardRect();
        origin = new Vector2(0f, (Screen.height - b.height) / 2f);
    }

    Rect BoardRect()
    {
        if (!hex) return new Rect(origin.x, origin.y, W * cs, H * cs);
        float w = cs * 1.1547f;
        return new Rect(origin.x, origin.y, (W - 1) * 0.75f * w + w, (H + 0.5f) * cs);
    }

    Rect CellRect(Vector2Int v)
    {
        if (!hex) return new Rect(origin.x + v.x * cs, origin.y + v.y * cs, cs, cs);
        float w = cs * 1.1547f;
        return new Rect(origin.x + v.x * w * 0.75f, origin.y + v.y * cs + ((v.x & 1) == 1 ? cs / 2f : 0f), w, cs);
    }

    Vector2 Center(Vector2Int v) => CellRect(v).center;

    bool HitTest(Vector2 m, out Vector2Int cell)
    {
        cell = default;
        float best = float.MaxValue;
        for (int x = 0; x < W; x++)
            for (int y = 0; y < H; y++)
            {
                var v = new Vector2Int(x, y);
                float d = (Center(v) - m).sqrMagnitude;
                if (d < best) { best = d; cell = v; }
            }
        return best < cs * cs * 0.36f;
    }

    void Fill(Rect r, Color c, Texture tex = null)
    {
        var old = GUI.color;
        GUI.color = c;
        GUI.DrawTexture(r, tex != null ? tex : Texture2D.whiteTexture);
        GUI.color = old;
    }

    void Tile(Vector2Int v, Color c, float inset = 0f)
    {
        var r = CellRect(v);
        r = new Rect(r.x + inset, r.y + inset, r.width - inset * 2, r.height - inset * 2);
        Fill(r, c, hex ? hexTex : null);
    }

    void Line(Vector2 a, Vector2 b, Color c, float width)
    {
        var d = b - a;
        float len = d.magnitude;
        if (len < 1f) return;
        float ang = Mathf.Atan2(d.y, d.x) * Mathf.Rad2Deg;
        var m = GUI.matrix;
        GUIUtility.RotateAroundPivot(ang, a);
        Fill(new Rect(a.x, a.y - width / 2f, len, width), c);
        GUI.matrix = m;
    }

    void Arrow(Vector2 a, Vector2 b, Color c)
    {
        var dir = (b - a).normalized;
        var tip = b - dir * cs * 0.3f;
        Line(a, tip, c, 3f);
        float head = cs * 0.3f;
        Vector2 l = Rotate(-dir, 30f) * head, r = Rotate(-dir, -30f) * head;
        Line(tip, tip + l, c, 3f);
        Line(tip, tip + r, c, 3f);
    }

    static Vector2 Rotate(Vector2 v, float deg)
    {
        float rad = deg * Mathf.Deg2Rad, cos = Mathf.Cos(rad), sin = Mathf.Sin(rad);
        return new Vector2(v.x * cos - v.y * sin, v.x * sin + v.y * cos);
    }

    void DrawBoard()
    {
        Fill(BoardRect(), new Color(0.08f, 0.08f, 0.09f));
        var targetCells = new HashSet<Vector2Int>();
        if (phase == Phase.Attack && selected != null)
        {
            foreach (var p in pieces) if (IsValidTarget(selected, p.pos)) targetCells.Add(p.pos);
            foreach (var kv in wallLife) if (IsValidTarget(selected, kv.Key)) targetCells.Add(kv.Key);
            if (selected.kind == Kind.Catapulta)
                for (int x = 0; x < W; x++)
                    for (int y = 0; y < H; y++)
                        if (IsValidTarget(selected, new Vector2Int(x, y))) targetCells.Add(new Vector2Int(x, y));
            if (selected.kind == Kind.Demoledor) foreach (var n in Adjacent(selected.pos)) targetCells.Add(n);
        }

        for (int x = 0; x < W; x++)
            for (int y = 0; y < H; y++)
            {
                var v = new Vector2Int(x, y);
                Color c = (x + y) % 2 == 0 || hex ? new Color(0.30f, 0.30f, 0.32f) : new Color(0.26f, 0.26f, 0.28f);
                if (interior[0].Contains(v)) c = new Color(0.22f, 0.30f, 0.45f);
                if (interior[1].Contains(v)) c = new Color(0.45f, 0.24f, 0.24f);
                if (IsWall(v)) c = wallOwner[v] == 0 ? new Color(0.30f, 0.35f, 0.55f) : new Color(0.55f, 0.30f, 0.28f);
                if (reach.Contains(v)) c = new Color(0.25f, 0.55f, 0.30f);
                if (kingCands.Contains(v)) c = new Color(0.20f, 0.60f, 0.65f);
                if (targetCells.Contains(v)) c = new Color(0.75f, 0.35f, 0.15f);
                Tile(v, c, hex ? 0.5f : 1f);
                if (IsWall(v))
                {
                    var r = CellRect(v);
                    Fill(new Rect(r.x + r.width * 0.2f, r.y + r.height * 0.2f, r.width * 0.6f, r.height * 0.6f), new Color(0.15f, 0.12f, 0.1f, 0.8f));
                    Text(r, $"<size={FontSize(4.5f)}>{wallLife[v]}</size>");
                }
            }

        // Attack range of the selected piece, or of whichever piece is under the mouse.
        Piece ranged = selected;
        if (ranged == null && HitTest(Event.current.mousePosition, out var hover)) ranged = PieceAt(hover);
        if (ranged != null)
            for (int x = 0; x < W; x++)
                for (int y = 0; y < H; y++)
                {
                    var v = new Vector2Int(x, y);
                    // Green move cells keep their colour so the two overlays stay readable.
                    if (InRange(ranged, v) && !reach.Contains(v) && !kingCands.Contains(v)) Tile(v, new Color(1f, 0.25f, 0.2f, 0.30f), hex ? 0.5f : 1f);
                }

        foreach (var p in pieces)
        {
            var r = CellRect(p.pos);
            if (p == selected) Tile(p.pos, new Color(1f, 0.85f, 0.2f));
            else if (plans.ContainsKey(p)) Tile(p.pos, new Color(1f, 0.55f, 0.1f));
            else if (phase == Phase.Attack && p.owner == current && HasTargets(p)) Tile(p.pos, new Color(1f, 1f, 1f, 0.55f));

            float ins = cs * (hex ? 0.05f : 0.06f), insX = hex ? ins * 1.1547f : ins;
            var inner = new Rect(r.x + insX, r.y + ins, r.width - insX * 2, r.height - ins * 2);
            Color col = p.owner == 0 ? new Color(0.13f, 0.30f, 0.78f) : new Color(0.74f, 0.17f, 0.17f);
            if (phase == Phase.Move && p.owner == current && p.moved) col *= 0.55f;
            col.a = 1f;
            // Archer on a wall: stone frame around the piece, wall life in a strip under it.
            if (IsWall(p.pos)) Fill(r, new Color(0.62f, 0.58f, 0.50f), hex ? hexTex : null);
            Fill(inner, col, hex ? hexTex : null);
            if (IsWall(p.pos))
            {
                var strip = new Rect(r.x, r.yMax - cs * 0.20f, r.width, cs * 0.20f);
                Fill(strip, new Color(0.15f, 0.12f, 0.1f, 0.9f));
                Text(strip, $"<size={FontSize(6f)}>muro {wallLife[p.pos]}</size>");
            }
            // Big letter on the left, Vida / Ataque / Defensa stacked on the right.
            var c = r.center;
            float lx = hex ? 0.52f : 0.47f, sx = hex ? 0.17f : 0.07f, step = 0.27f;
            Text(new Rect(c.x - cs * lx, c.y - cs * 0.30f, cs * 0.33f, cs * 0.60f), $"<size={FontSize(3.0f)}>{Letters[(int)p.kind]}</size>");
            string[] stats = { $"V{p.life}", $"A{AttackValue(p)}", $"D{ShieldValue(p)}" };
            for (int i = 0; i < 3; i++)
            {
                // Bigger numbers with a dark shadow so they read on both team colours.
                var sr = new Rect(c.x - cs * sx, c.y - cs * step * 1.5f + i * cs * step, cs * 0.56f, cs * step);
                string txt = $"<size={FontSize(4.0f)}>{stats[i]}</size>";
                var oldCol = GUI.contentColor;
                GUI.contentColor = new Color(0f, 0f, 0f, 0.9f);
                GUI.Label(new Rect(sr.x + 1f, sr.y + 1f, sr.width, sr.height), txt, statStyle);
                GUI.contentColor = oldCol;
                GUI.Label(sr, txt, statStyle);
            }
        }

        foreach (var kv in plans)
        {
            var a = kv.Key; var pl = kv.Value;
            var color = a.owner == 0 ? new Color(0.5f, 0.8f, 1f) : new Color(1f, 0.6f, 0.5f);
            if (pl.explode)
            {
                foreach (var n in Adjacent(a.pos)) Line(Center(a.pos), Center(n), new Color(1f, 0.6f, 0.1f, 0.8f), 2f);
            }
            else
            {
                if (pl.area) foreach (var n in AreaCells(pl.wall)) if (InBoard(n)) Tile(n, new Color(1f, 0.45f, 0.1f, 0.45f), hex ? 0.5f : 1f);
                Arrow(Center(a.pos), pl.isWall || pl.area ? Center(pl.wall) : Center(pl.target.pos), color);
            }
        }

        // Damage preview on every planned target.
        if (phase == Phase.Attack && plans.Count > 0)
        {
            var toPieces = new Dictionary<Piece, int>();
            var toWalls = new Dictionary<Vector2Int, int>();
            IncomingDamage(toPieces, toWalls);
            foreach (var kv in toPieces)
            {
                int dmg = Mathf.Max(0, kv.Value - ShieldValue(kv.Key));
                if (dmg >= kv.Key.life) Badge(kv.Key.pos, "MUERE", true); else Badge(kv.Key.pos, $"−{dmg}");
            }
            // A wall with an archer on it gets its preview under the cell, so both stay readable.
            foreach (var kv in toWalls)
            {
                bool low = PieceAt(kv.Key) != null;
                if (kv.Value >= wallLife[kv.Key]) Badge(kv.Key, low ? "MURO ROTO" : "ROTO", !low, low);
                else Badge(kv.Key, low ? $"muro −{kv.Value}" : $"−{kv.Value}", false, low);
            }
            // The demolisher dies with its own explosion: say so before resolving.
            foreach (var kv in plans) if (kv.Value.explode) Badge(kv.Key.pos, "MUERE", true);
        }
    }

    // Damage preview. A lethal hit shows only the word, as a black band across the piece.
    void Badge(Vector2Int v, string s, bool lethal = false, bool low = false)
    {
        var r = CellRect(v);
        var b = lethal ? new Rect(r.center.x - cs * 0.5f, r.center.y - cs * 0.19f, cs, cs * 0.38f)
              : low    ? new Rect(r.x - cs * 0.1f, r.yMax - cs * 0.05f, cs * 1.2f, cs * 0.4f)
                       : new Rect(r.x - cs * 0.1f, r.y - cs * 0.35f, cs * 1.2f, cs * 0.4f);
        Fill(b, lethal ? new Color(0f, 0f, 0f, 0.92f) : new Color(0f, 0f, 0f, 0.75f));
        badgeStyle.normal.textColor = lethal ? Color.white : new Color(1f, 0.85f, 0.2f);
        GUI.Label(b, $"<size={FontSize(lethal ? 4.4f : 3.6f)}>{s}</size>", badgeStyle);
    }

    int FontSize(float div) => Mathf.Max(8, Mathf.RoundToInt(cs / div));

    void Text(Rect r, string s) => GUI.Label(r, s, centerStyle);

    void DrawDieOverlay()
    {
        float since = Time.realtimeSinceStartup - dieShownAt;
        if (since > DieOverlayTime || phase == Phase.Roll) return;
        var b = BoardRect();
        var box = new Rect(b.center.x - 190, b.center.y - 90, 380, 180);
        Fill(box, new Color(0f, 0f, 0f, 0.85f));
        GUI.Label(new Rect(box.x, box.y + 4, box.width, 90), $"<size=72><b>{die}</b></size>", centerStyle);
        GUI.Label(new Rect(box.x + 10, box.y + 92, box.width - 20, 30), $"<size=22><b>{FaceNames[die - 1]}</b></size>", centerStyle);
        GUI.Label(new Rect(box.x + 10, box.y + 122, box.width - 20, 54), $"<size=14>{FaceText[die - 1]}  ·  tabla: {BandNames[(int)rollBand]}</size>", centerStyle);
    }

    void DrawPanel(Rect area)
    {
        GUILayout.BeginArea(area);
        panelScroll = GUILayout.BeginScrollView(panelScroll);
        string who = current == 0 ? "<color=#6fa0ff>Jugador 1 (azul)</color>" : "<color=#ff7a7a>Jugador 2 (rojo)</color>";

        if (phase == Phase.Over)
        {
            GUILayout.Label($"<size=20><b>Ganó el {(winner == 0 ? "Jugador 1 (azul)" : "Jugador 2 (rojo)")}</b></size>", richStyle);
            foreach (var l in log) GUILayout.Label(l, richStyle);
            DrawStats(true);
            if (GUILayout.Button("Nueva partida", GUILayout.Height(32))) phase = Phase.Setup;
            GUILayout.EndScrollView();
            GUILayout.EndArea();
            return;
        }

        GUILayout.Label($"<size=16><b>Turno {turnNumber}: {who}</b></size>", richStyle);
        float elapsed = Time.realtimeSinceStartup - turnStart;
        if (useTimer && (phase == Phase.Recruit || phase == Phase.Move || phase == Phase.Attack))
        {
            int limit = StageLimit();
            float left = Mathf.Max(0f, limit - (Time.realtimeSinceStartup - stageStart));
            string col = left <= 10f ? "#ff5555" : "#ffffff";
            string stage = phase == Phase.Attack ? "atacar" : "mover";
            GUILayout.Label($"<size=26><b><color={col}>{Fmt(left)}</color></b></size>  para {stage} ({limit} s)", richStyle);
        }
        else GUILayout.Label($"Turno: {Fmt(elapsed)}");

        if (phase != Phase.Roll)
            GUILayout.Label($"<size=20><b>Modificador {die}: {FaceNames[die - 1]}</b></size>\n{FaceText[die - 1]}", richStyle);

        if (autoAction != AutoAction.None)
        {
            string what = autoAction == AutoAction.EndLost ? "Pasando el turno"
                        : autoAction == AutoAction.ToAttack ? "Pasando a ataque"
                        : "Sin ataques posibles: terminando el turno";
            GUILayout.Label($"<color=#ffd24d>{what} en {Mathf.Max(0f, autoAt - Time.realtimeSinceStartup):0.0} s…</color>", richStyle);
        }
        GUILayout.Space(4);

        switch (phase)
        {
            case Phase.Roll:
                GUILayout.Label("Tirando el dado…");
                break;
            case Phase.LostTurn:
                if (GUILayout.Button("Pasar el turno ahora", GUILayout.Height(32))) EndTurn(true);
                break;
            case Phase.Recruit:
                GUILayout.Label("¿Qué reclutas? (aparecen del lado que mira al rival)");
                for (int k = 0; k < 6; k++)
                {
                    if (IsSpecial((Kind)k) != recruitSpecial) continue;
                    int alive = 0;
                    foreach (var p in pieces) if (p.owner == current && p.kind == (Kind)k) alive++;
                    bool allowed = CanRecruitKind((Kind)k);
                    GUI.enabled = allowed && alive < Limit[k];
                    if (GUILayout.Button(allowed ? $"{RecruitCount((Kind)k)} {Names[k]} ({alive}/{Limit[k]})" : $"{Names[k]} (solo con castillos)")) Recruit((Kind)k);
                    GUI.enabled = true;
                }
                if (GUILayout.Button("No reclutar")) phase = Phase.Move;
                break;
            case Phase.Move:
                GUILayout.Label("<b>MOVER</b>", richStyle);
                GUILayout.Label(unlimitedMoves ? "Piezas movidas: sin límite" : $"Piezas movidas: {movesUsed}/{MaxMoves} (+ Rey)");
                GUILayout.Label("<size=11>Clic en tu pieza y luego en una casilla verde. Puedes atravesar a tus compañeros.</size>", richStyle);
                GUILayout.Label("<size=11>Arqueros: pueden subir a un tramo libre de su propio muro (+2 de defensa). El daño a esa casilla lo reciben el arquero y el muro.</size>", richStyle);
                if (selected != null && selected.kind == Kind.Rey && kingMode != KingMode.None)
                    GUILayout.Label(kingMode == KingMode.Exit
                        ? "Rey: clic en una casilla celeste para salir del castillo. Ese turno no se mueve más."
                        : "Rey: clic en una casilla celeste para entrar al castillo. Ese turno no se mueve más.", richStyle);
                GUI.enabled = undo.Count > 0;
                if (GUILayout.Button("Deshacer último movimiento")) Undo();
                GUI.enabled = true;
                if (GUILayout.Button("Terminar movimiento → Atacar", GUILayout.Height(32))) EnterAttack();
                break;
            case Phase.Attack:
                GUILayout.Label("<b>ATACAR</b>: asigna los ataques; se resuelven juntos cuando pulses Resolver.", richStyle);
                GUILayout.Label("<size=11>Clic en tu pieza (resaltada) → clic en su objetivo naranja. Clic de nuevo en la pieza para quitar su flecha. Demoledor: clic en él mismo para que explote (muere al atacar). Catapulta: clic en cualquier casilla a su alcance; daño completo en esa casilla y la mitad en todas las de alrededor, aliados incluidos.</size>", richStyle);
                DrawBonus();
                GUILayout.Label($"Ataques asignados: {plans.Count}");
                if (GUILayout.Button("Quitar todas las flechas")) { plans.Clear(); ClearSelection(); }
                if (GUILayout.Button("Resolver ataques y terminar turno", GUILayout.Height(34)))
                {
                    ResolveAll();
                    if (phase != Phase.Over) EndTurn(false);
                }
                break;
        }

        if (!string.IsNullOrEmpty(message) && phase != Phase.Roll) GUILayout.Label($"<i>{message}</i>", richStyle);
        if (log.Count > 0)
        {
            GUILayout.Label("<b>Último combate</b>", richStyle);
            foreach (var l in log) GUILayout.Label(l, richStyle);
        }
        GUILayout.Space(4);
        GUILayout.Label($"Cargas de Grito de guerra: J1 {boostCharges[0]}{(boostOn[0] ? " (activo)" : "")} · J2 {boostCharges[1]}{(boostOn[1] ? " (activo)" : "")}");
        GUILayout.EndScrollView();
        GUILayout.EndArea();
    }

    void DrawInfo(Rect area)
    {
        GUILayout.BeginArea(area);
        infoScroll = GUILayout.BeginScrollView(infoScroll);
        if (castles) GUILayout.Label($"Muros rotos: azul {breaches[0]}, rojo {breaches[1]} (con un muro derribado no se recluta)");
        DrawOdds();
        DrawStats(false);
        GUILayout.Label("<size=11>Fichas: letra + V vida, A ataque, D defensa. S Soldado, A Arquero, E Escudero, C Catapulta, J Jinete, D Demoledor, R Rey. Arquero sobre su muro: +2 defensa; el número chico abajo es la vida del muro. Casillas rojizas: alcance de ataque de la pieza seleccionada o bajo el cursor.</size>", richStyle);
        GUILayout.EndScrollView();
        GUILayout.EndArea();
    }

    void DrawBonus()
    {
        if (boostOn[current])
        {
            GUILayout.Label($"Grito de guerra activo: +{BoostAmount} de ataque a todas tus piezas en este turno");
            return;
        }
        if (boostCharges[current] > 0 && GUILayout.Button($"Usar Grito de guerra: +{BoostAmount} ataque este turno ({boostCharges[current]} cargas)"))
        { boostCharges[current]--; boostOn[current] = true; }
    }

    // The odds each player rolls with next, so both know them before the die is thrown.
    void DrawOdds()
    {
        GUILayout.Label("<b>Modificador: probabilidades de la próxima tirada</b>", richStyle);
        for (int p = 0; p < 2; p++)
        {
            var band = BandFor(p);
            string who = p == 0 ? "<color=#6fa0ff>Azul</color>" : "<color=#ff7a7a>Rojo</color>";
            var parts = new List<string>();
            for (int f = 0; f < 5; f++) parts.Add($"{f + 1}: {Odds[(int)band, f]}%");
            GUILayout.Label($"<size=12>{who} ({PieceCount(p)} piezas vs {PieceCount(1 - p)}) · {BandNames[(int)band]}\n{string.Join(" · ", parts)}</size>", richStyle);
        }
        for (int f = 1; f <= 5; f++) GUILayout.Label($"<size=11>{FaceLine(f)}</size>", richStyle);
    }

    void DrawStats(bool final)
    {
        float match = (final ? matchEnd : Time.realtimeSinceStartup) - matchStart;
        var all = new List<float>(turnTimes[0]);
        all.AddRange(turnTimes[1]);
        float longest = 0f;
        foreach (var t in all) longest = Mathf.Max(longest, t);
        GUILayout.Label("<b>Medición</b>", richStyle);
        GUILayout.Label($"Partida {Fmt(match)} · prom. J1 {Fmt(Avg(turnTimes[0]))} ({turnTimes[0].Count}) · J2 {Fmt(Avg(turnTimes[1]))} ({turnTimes[1].Count})", richStyle);
        int rolls = 0; foreach (int c in faceCount) rolls += c;
        GUILayout.Label($"Prom. general {Fmt(Avg(all))} · más largo {Fmt(longest)} · tiempo agotado: mover {moveTimeouts}, atacar {attackTimeouts}", richStyle);
        GUILayout.Label($"Turnos perdidos (Distracción): {lostTurns} de {rolls} tiradas{(rolls > 0 ? $" ({lostTurns * 100 / rolls}%)" : "")}", richStyle);
        var fc = new List<string>();
        for (int f = 0; f < 5; f++) fc.Add($"{f + 1}: {faceCount[f]}");
        GUILayout.Label($"Caras: {string.Join(" · ", fc)}", richStyle);
        var bc = new List<string>();
        for (int b = 0; b < 5; b++) if (bandCount[b] > 0) bc.Add($"{BandNames[b]} {bandCount[b]}");
        GUILayout.Label($"Tablas usadas: {string.Join(" · ", bc)}", richStyle);
        int played = all.Count;
        string pct = played > 0 ? $" ({decisiveTurns * 100 / played}%)" : "";
        GUILayout.Label($"Ataque combinado decisivo (sumando matan o rompen una defensa que por separado no): {decisiveTurns} de {played} turnos jugados{pct}", richStyle);
        GUILayout.Label($"2+ piezas a un mismo objetivo: {combinedTurns} turnos · turnos con ataque: {attackTurns}", richStyle);
        GUILayout.Label($"<size=11>{(hex ? "Hexagonal" : "Tablero")} {W}×{H}, {(castles ? "castillos" : "sin castillos, Reyes")} en {SectorPairs[pairIdx, 0]} y {SectorPairs[pairIdx, 1]}, timer {(useTimer ? "60+30 → 40+20" : "sin límite")}</size>", richStyle);
    }

    static float Avg(List<float> l)
    {
        if (l.Count == 0) return 0f;
        float s = 0f;
        foreach (var t in l) s += t;
        return s / l.Count;
    }

    static string Fmt(float s) => $"{(int)(s / 60)}:{(int)(s % 60):00}";
}
