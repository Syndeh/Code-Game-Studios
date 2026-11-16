// PROTOTYPE - NOT FOR PRODUCTION
// Question: With the v1 playtest fixes (planned attacks, pass-through allies, zone range,
//           castle walls with life, turn timer), do turns stay brisk and does the combined
//           attack become a real decision? Square vs hex, castles vs no castles.
// Date: 2026-11-13
//
// Hotseat 2 players, drawn entirely with IMGUI. Press Play in any scene: it creates itself.

using System.Collections.Generic;
using UnityEngine;

public class RolchessV2 : MonoBehaviour
{
    // ---------- Tuning (hardcoded on purpose: prototype) ----------
    const int MaxMoves = 12;
    const int WallLife = 30;
    const int BreachesToLoseRecruit = 4;
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

    static readonly string[] DieFaces =
    {
        "1: pierdes el turno",
        "2: +1 carga de ataque para arqueros (hasta 4)",
        "3: +1 carga de ataque para soldados (hasta 4)",
        "4: reclutas 4 de un tipo",
        "5: retirada al castillo (no está en el prototipo)",
        "6: mueves todas las piezas que quieras",
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
    int sizeIdx = 1, timerIdx = 1;
    static readonly int[] Sizes = { 16, 20, 24 };
    static readonly int[] Timers = { 40, 60, 90, 0 };
    static readonly string[] TimerLabels = { "40 s", "60 s", "90 s", "Sin límite" };

    // ---------- State ----------
    int W, H, timerLimit;
    readonly List<Piece> pieces = new List<Piece>();
    readonly Dictionary<Vector2Int, Piece> occ = new Dictionary<Vector2Int, Piece>();
    readonly Dictionary<Vector2Int, int> wallLife = new Dictionary<Vector2Int, int>();
    readonly Dictionary<Vector2Int, int> wallOwner = new Dictionary<Vector2Int, int>();
    readonly HashSet<Vector2Int>[] interior = { new HashSet<Vector2Int>(), new HashSet<Vector2Int>() };
    readonly HashSet<Vector2Int>[] castleCells = { new HashSet<Vector2Int>(), new HashSet<Vector2Int>() };
    readonly int[] breaches = new int[2];
    readonly int[] archerBonus = new int[2];
    readonly int[] soldierBonus = new int[2];

    int current;
    Phase phase = Phase.Setup;
    int die;
    float dieShownAt = -100f;
    int movesUsed;
    bool unlimitedMoves;
    bool archerBonusOn, soldierBonusOn, bonusUsedThisTurn;

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
    int lostTurns, timeouts, turnNumber;

    // ---------- Drawing ----------
    Vector2 origin = new Vector2(10, 10);
    float cs = 30f;
    Texture2D hexTex;
    GUIStyle centerStyle, richStyle, statStyle;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Boot()
    {
        if (FindAnyObjectByType<RolchessV2>() != null) return;
        var cam = Camera.main;
        if (cam == null) cam = new GameObject("Camera").AddComponent<Camera>();
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = new Color(0.12f, 0.12f, 0.14f);
        new GameObject("RolchessV2").AddComponent<RolchessV2>();
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
        W = H = Sizes[sizeIdx];
        timerLimit = Timers[timerIdx];
        pieces.Clear(); wallLife.Clear(); wallOwner.Clear();
        for (int p = 0; p < 2; p++) { interior[p].Clear(); castleCells[p].Clear(); breaches[p] = 0; archerBonus[p] = soldierBonus[p] = 0; }

        int cy = H / 2;
        for (int p = 0; p < 2; p++)
        {
            int x0 = p == 0 ? 0 : W - 5;
            Vector2Int king;
            if (castles)
            {
                for (int x = x0; x < x0 + 5; x++)
                    for (int y = cy - 2; y <= cy + 2; y++)
                    {
                        var v = new Vector2Int(x, y);
                        castleCells[p].Add(v);
                        bool border = x == x0 || x == x0 + 4 || y == cy - 2 || y == cy + 2;
                        if (border) { wallLife[v] = WallLife; wallOwner[v] = p; }
                        else interior[p].Add(v);
                    }
                king = new Vector2Int(x0 + 2, cy);
            }
            else king = new Vector2Int(p == 0 ? 1 : W - 2, cy);
            Add(Kind.Rey, p, king);

            int col1 = p == 0 ? 6 : W - 7;   // soldiers and archers
            int col2 = p == 0 ? 7 : W - 8;   // escuderos in front
            for (int i = 0; i < 8; i++) Add(i % 2 == 0 ? Kind.Soldado : Kind.Arquero, p, new Vector2Int(col1, cy - 4 + i));
            for (int i = 0; i < 4; i++) Add(Kind.Escudero, p, new Vector2Int(col2, cy - 2 + i));
        }
        RebuildOcc();
        current = 0;
        turnNumber = 0; lostTurns = 0; timeouts = 0;
        turnTimes[0].Clear(); turnTimes[1].Clear();
        log.Clear();
        winner = -1;
        matchStart = Time.realtimeSinceStartup;
        StartTurn();
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
                if (dist.ContainsKey(n) || IsWall(n)) continue;
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

    float FrontScore(int owner, Vector2Int v) => (owner == 0 ? -v.x : v.x) * 100f + Mathf.Abs(v.y - H / 2);

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

    bool InRange(Piece a, Vector2Int t)
    {
        int d = Dist(a.pos, t);
        return d >= RMin[(int)a.kind] && d <= RMax[(int)a.kind];
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
        foreach (var p in pieces) if (p.owner != a.owner && InRange(a, p.pos)) return true;
        foreach (var kv in wallLife) if (wallOwner[kv.Key] != a.owner && InRange(a, kv.Key)) return true;
        return false;
    }

    bool IsValidTarget(Piece a, Vector2Int cell)
    {
        if (a.kind == Kind.Demoledor) return false; // the demolisher explodes instead
        var o = PieceAt(cell);
        if (o != null) return o.owner != a.owner && InRange(a, cell);
        if (IsWall(cell)) return wallOwner[cell] != a.owner && InRange(a, cell);
        return false;
    }

    int AttackValue(Piece a)
    {
        int v = Atk[(int)a.kind];
        if (a.kind == Kind.Arquero && archerBonusOn) v++;
        if (a.kind == Kind.Soldado && soldierBonusOn) v++;
        return v;
    }

    // Sums every planned attack per target. Shield is applied once per target later.
    void IncomingDamage(Dictionary<Piece, int> toPieces, Dictionary<Vector2Int, int> toWalls)
    {
        foreach (var kv in plans)
        {
            var a = kv.Key; var pl = kv.Value;
            if (pl.explode)
            {
                foreach (var n in Adjacent(a.pos))
                {
                    var o = PieceAt(n);
                    if (o != null) Accumulate(toPieces, o, AttackValue(a));
                    if (IsWall(n)) Accumulate(toWalls, n, AttackValue(a));
                }
            }
            else if (pl.isWall) Accumulate(toWalls, pl.wall, AttackValue(a));
            else Accumulate(toPieces, pl.target, AttackValue(a));
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
        archerBonusOn = soldierBonusOn = bonusUsedThisTurn = false;
        foreach (var p in pieces) p.moved = false;
        undo.Clear();
        plans.Clear();
        ClearSelection();
        autoAction = AutoAction.None; autoAt = -1f;
        turnStart = Time.realtimeSinceStartup;
    }

    void Roll()
    {
        die = Random.Range(1, 7);
        dieShownAt = Time.realtimeSinceStartup;
        message = DieFaces[die - 1];
        switch (die)
        {
            case 1: phase = Phase.LostTurn; break;
            case 2: archerBonus[current] = Mathf.Min(4, archerBonus[current] + 1); phase = Phase.Move; break;
            case 3: soldierBonus[current] = Mathf.Min(4, soldierBonus[current] + 1); phase = Phase.Move; break;
            case 4:
                if (CanRecruit(current)) phase = Phase.Recruit;
                else { phase = Phase.Move; message = "4: tu castillo está destruido, no puedes reclutar."; }
                break;
            case 5: phase = Phase.Move; break;
            default: unlimitedMoves = true; phase = Phase.Move; break;
        }
    }

    void Recruit(Kind k)
    {
        int alive = 0;
        foreach (var p in pieces) if (p.owner == current && p.kind == k) alive++;
        int n = Mathf.Min(4, Limit[(int)k] - alive);
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
        Debug.Log($"[RolchessV2] Turno {turnNumber} J{current + 1}: {t:0.0}s{(lost ? " (perdido)" : "")}, movidas {movesUsed}");
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

        foreach (var kv in toPieces)
        {
            var t = kv.Key;
            int sh = Shield[(int)t.kind];
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
            Debug.Log($"[RolchessV2] Ganó J{winner + 1}. {(hex ? "hex" : "cuadrado")}, {(castles ? "con" : "sin")} castillos, {W}x{H}, " +
                      $"timer {timerLimit}s. Partida {Fmt(matchEnd - matchStart)}, turnos {turnNumber}, " +
                      $"promedio J1 {Fmt(Avg(turnTimes[0]))}, J2 {Fmt(Avg(turnTimes[1]))}, perdidos {lostTurns}, por tiempo {timeouts}");
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

        if (timerLimit > 0 && now - turnStart >= timerLimit)
        {
            timeouts++;
            message = "Se acabó el tiempo: se resuelven los ataques asignados.";
            bool lost = phase == Phase.LostTurn;
            ResolveAll();
            if (phase != Phase.Over) EndTurn(lost);
            return;
        }

        AutoAction want = AutoAction.None;
        if (phase == Phase.LostTurn) want = AutoAction.EndLost;
        else if (phase == Phase.Move && !AnyMovable()) want = AutoAction.ToAttack;
        else if (phase == Phase.Attack)
        {
            int potential = 0, assigned = 0;
            foreach (var p in pieces)
                if (p.owner == current && HasTargets(p)) { potential++; if (plans.ContainsKey(p)) assigned++; }
            if (potential == 0 || assigned == potential) want = AutoAction.ResolveAndEnd;
        }

        if (want != autoAction) { autoAction = want; autoAt = want == AutoAction.None ? -1f : now + AutoDelay; }
        if (autoAction != AutoAction.None && now >= autoAt)
        {
            var a = autoAction;
            autoAction = AutoAction.None; autoAt = -1f;
            switch (a)
            {
                case AutoAction.EndLost: EndTurn(true); break;
                case AutoAction.ToAttack: phase = Phase.Attack; ClearSelection(); message = "Sin más movimientos: fase de ataque."; break;
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
            if (selected != null && kingMode != KingMode.None && kingCands.Contains(cell))
            {
                DoMove(selected, cell, false);
                return;
            }
            if (selected != null && kingMode == KingMode.None && reach.Contains(cell))
            {
                DoMove(selected, cell, selected.kind != Kind.Rey && !unlimitedMoves);
                return;
            }
            if (p != null && CanSelectToMove(p))
            {
                ClearSelection();
                selected = p;
                reach = ComputeReach(p);
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
                    plans[selected] = p != null ? new Plan { target = p } : new Plan { isWall = true, wall = cell };
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
        DrawPanel(new Rect(BoardRect().xMax + 14, 10, 300, Screen.height - 20));
    }

    void DrawSetup()
    {
        GUILayout.BeginArea(new Rect(Screen.width / 2f - 200, 60, 400, 460), GUI.skin.box);
        GUILayout.Label("<size=22><b>Rolchess v2: prototipo</b></size>", richStyle);
        GUILayout.Space(8);
        GUILayout.Label("Tablero");
        hex = GUILayout.Toolbar(hex ? 1 : 0, new[] { "Cuadrado", "Hexagonal" }) == 1;
        GUILayout.Label("Castillos");
        castles = GUILayout.Toolbar(castles ? 0 : 1, new[] { "Con castillos", "Sin castillos (Rey atrás)" }) == 0;
        GUILayout.Label("Tamaño");
        sizeIdx = GUILayout.Toolbar(sizeIdx, new[] { "16", "20", "24" });
        GUILayout.Label("Tiempo por turno");
        timerIdx = GUILayout.Toolbar(timerIdx, TimerLabels);
        GUILayout.Space(16);
        if (GUILayout.Button("Empezar", GUILayout.Height(40))) BuildMatch();
        GUILayout.EndArea();
    }

    void ComputeCellSize()
    {
        float availW = Screen.width - 340f, availH = Screen.height - 20f;
        if (!hex) cs = Mathf.Max(14f, Mathf.Min(availW / W, availH / H));
        else cs = Mathf.Max(14f, Mathf.Min(availW / (((W - 1) * 0.75f + 1f) * 1.1547f), availH / (H + 0.5f)));
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

        foreach (var p in pieces)
        {
            var r = CellRect(p.pos);
            if (p == selected) Tile(p.pos, new Color(1f, 0.85f, 0.2f));
            else if (plans.ContainsKey(p)) Tile(p.pos, new Color(1f, 0.55f, 0.1f));
            else if (phase == Phase.Attack && p.owner == current && HasTargets(p)) Tile(p.pos, new Color(1f, 1f, 1f, 0.55f));

            float ins = cs * (hex ? 0.07f : 0.12f), insX = hex ? ins * 1.1547f : ins;
            var inner = new Rect(r.x + insX, r.y + ins, r.width - insX * 2, r.height - ins * 2);
            Color col = p.owner == 0 ? new Color(0.25f, 0.45f, 0.95f) : new Color(0.90f, 0.30f, 0.30f);
            if (phase == Phase.Move && p.owner == current && p.moved) col *= 0.55f;
            col.a = 1f;
            Fill(inner, col, hex ? hexTex : null);
            // Big letter on the left, Vida / Ataque / Defensa stacked on the right.
            var c = r.center;
            Text(new Rect(c.x - cs * 0.40f, c.y - cs * 0.30f, cs * 0.40f, cs * 0.60f), $"<size={FontSize(2.3f)}>{Letters[(int)p.kind]}</size>");
            string[] stats = { $"V{p.life}", $"A{AttackValue(p)}", $"D{Shield[(int)p.kind]}" };
            for (int i = 0; i < 3; i++)
                GUI.Label(new Rect(c.x + cs * 0.01f, c.y - cs * 0.315f + i * cs * 0.21f, cs * 0.40f, cs * 0.21f), $"<size={FontSize(4.6f)}>{stats[i]}</size>", statStyle);
        }

        foreach (var kv in plans)
        {
            var a = kv.Key; var pl = kv.Value;
            var color = a.owner == 0 ? new Color(0.5f, 0.8f, 1f) : new Color(1f, 0.6f, 0.5f);
            if (pl.explode)
            {
                foreach (var n in Adjacent(a.pos)) Line(Center(a.pos), Center(n), new Color(1f, 0.6f, 0.1f, 0.8f), 2f);
            }
            else Arrow(Center(a.pos), pl.isWall ? Center(pl.wall) : Center(pl.target.pos), color);
        }

        // Damage preview on every planned target.
        if (phase == Phase.Attack && plans.Count > 0)
        {
            var toPieces = new Dictionary<Piece, int>();
            var toWalls = new Dictionary<Vector2Int, int>();
            IncomingDamage(toPieces, toWalls);
            foreach (var kv in toPieces)
            {
                int dmg = Mathf.Max(0, kv.Value - Shield[(int)kv.Key.kind]);
                Badge(kv.Key.pos, dmg >= kv.Key.life ? $"−{dmg} MUERE" : $"−{dmg}");
            }
            foreach (var kv in toWalls) Badge(kv.Key, $"−{kv.Value}");
            // The demolisher dies with its own explosion: say so before resolving.
            foreach (var kv in plans) if (kv.Value.explode) Badge(kv.Key.pos, "MUERE");
        }
    }

    void Badge(Vector2Int v, string s)
    {
        var r = CellRect(v);
        var b = new Rect(r.x - cs * 0.1f, r.y - cs * 0.35f, cs * 1.2f, cs * 0.4f);
        Fill(b, new Color(0f, 0f, 0f, 0.75f));
        var old = centerStyle.normal.textColor;
        centerStyle.normal.textColor = new Color(1f, 0.85f, 0.2f);
        GUI.Label(b, $"<size={FontSize(3.6f)}>{s}</size>", centerStyle);
        centerStyle.normal.textColor = old;
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
        GUI.Label(new Rect(box.x, box.y + 10, box.width, 100), $"<size=80><b>{die}</b></size>", centerStyle);
        GUI.Label(new Rect(box.x + 10, box.y + 110, box.width - 20, 60), $"<size=18>{DieFaces[die - 1]}</size>", centerStyle);
    }

    void DrawPanel(Rect area)
    {
        GUILayout.BeginArea(area);
        string who = current == 0 ? "<color=#6fa0ff>Jugador 1 (azul)</color>" : "<color=#ff7a7a>Jugador 2 (rojo)</color>";

        if (phase == Phase.Over)
        {
            GUILayout.Label($"<size=20><b>Ganó el {(winner == 0 ? "Jugador 1 (azul)" : "Jugador 2 (rojo)")}</b></size>", richStyle);
            foreach (var l in log) GUILayout.Label(l, richStyle);
            DrawStats(true);
            if (GUILayout.Button("Nueva partida", GUILayout.Height(32))) phase = Phase.Setup;
            GUILayout.EndArea();
            return;
        }

        GUILayout.Label($"<size=16><b>Turno {turnNumber}: {who}</b></size>", richStyle);
        float elapsed = Time.realtimeSinceStartup - turnStart;
        if (timerLimit > 0)
        {
            float left = Mathf.Max(0f, timerLimit - elapsed);
            string col = left <= 10f ? "#ff5555" : "#ffffff";
            GUILayout.Label($"<size=26><b><color={col}>{Fmt(left)}</color></b></size>", richStyle);
        }
        else GUILayout.Label($"Turno: {Fmt(elapsed)}");

        if (phase != Phase.Roll)
            GUILayout.Label($"<size=22><b>Dado: {die}</b></size>  {DieFaces[die - 1]}", richStyle);

        if (autoAction != AutoAction.None)
        {
            string what = autoAction == AutoAction.EndLost ? "Pasando el turno"
                        : autoAction == AutoAction.ToAttack ? "Pasando a ataque"
                        : "Resolviendo y terminando el turno";
            GUILayout.Label($"<color=#ffd24d>{what} en {Mathf.Max(0f, autoAt - Time.realtimeSinceStartup):0.0} s…</color>", richStyle);
        }
        GUILayout.Space(4);

        switch (phase)
        {
            case Phase.Roll:
                if (GUILayout.Button("Tirar dado", GUILayout.Height(40))) Roll();
                break;
            case Phase.LostTurn:
                if (GUILayout.Button("Pasar el turno ahora", GUILayout.Height(32))) EndTurn(true);
                break;
            case Phase.Recruit:
                GUILayout.Label("¿Qué reclutas? (aparecen del lado que mira al rival)");
                for (int k = 0; k < 6; k++)
                {
                    int alive = 0;
                    foreach (var p in pieces) if (p.owner == current && p.kind == (Kind)k) alive++;
                    GUI.enabled = alive < Limit[k];
                    if (GUILayout.Button($"{Names[k]} ({alive}/{Limit[k]})")) Recruit((Kind)k);
                    GUI.enabled = true;
                }
                if (GUILayout.Button("No reclutar")) phase = Phase.Move;
                break;
            case Phase.Move:
                GUILayout.Label("<b>MOVER</b>", richStyle);
                GUILayout.Label(unlimitedMoves ? "Piezas movidas: sin límite" : $"Piezas movidas: {movesUsed}/{MaxMoves} (+ Rey)");
                GUILayout.Label("Clic en tu pieza y luego en una casilla verde. Puedes atravesar a tus compañeros.", richStyle);
                if (selected != null && selected.kind == Kind.Rey && castles)
                {
                    if (KingExitCells(selected).Count > 0 && GUILayout.Button("Sacar al Rey del castillo"))
                    { kingMode = KingMode.Exit; kingCands = KingExitCells(selected); reach.Clear(); }
                    if (KingEnterCells(selected).Count > 0 && GUILayout.Button("Meter al Rey al castillo"))
                    { kingMode = KingMode.Enter; kingCands = KingEnterCells(selected); reach.Clear(); }
                    if (kingMode != KingMode.None) GUILayout.Label("Elige la casilla celeste. El Rey no se mueve más este turno.", richStyle);
                }
                GUI.enabled = undo.Count > 0;
                if (GUILayout.Button("Deshacer último movimiento")) Undo();
                GUI.enabled = true;
                if (GUILayout.Button("Terminar movimiento → Atacar", GUILayout.Height(32))) { phase = Phase.Attack; ClearSelection(); }
                break;
            case Phase.Attack:
                GUILayout.Label("<b>ATACAR</b>: asigna todos los ataques y se resuelven juntos.", richStyle);
                GUILayout.Label("Clic en tu pieza (resaltada) → clic en su objetivo naranja. Clic de nuevo en la pieza para quitar su flecha. Demoledor: clic en él mismo para que explote (muere al atacar).", richStyle);
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
        GUILayout.Label($"Cargas J1: arq {archerBonus[0]}, sold {soldierBonus[0]} · J2: arq {archerBonus[1]}, sold {soldierBonus[1]}");
        if (castles) GUILayout.Label($"Muros rotos: azul {breaches[0]}, rojo {breaches[1]} (con {BreachesToLoseRecruit} no se recluta)");
        DrawStats(false);
        GUILayout.Label("<b>Dado</b>", richStyle);
        foreach (var f in DieFaces) GUILayout.Label($"<size=11>{f}</size>", richStyle);
        GUILayout.Label("<size=11>Fichas: letra + V vida, A ataque, D defensa. S Soldado, A Arquero, E Escudero, C Catapulta, J Jinete, D Demoledor, R Rey</size>", richStyle);
        GUILayout.EndArea();
    }

    void DrawBonus()
    {
        if (bonusUsedThisTurn)
        {
            GUILayout.Label(archerBonusOn ? "Bonus activo: +1 arqueros" : "Bonus activo: +1 soldados");
            return;
        }
        if (archerBonus[current] > 0 && GUILayout.Button($"Usar +1 arqueros ({archerBonus[current]})"))
        { archerBonus[current]--; archerBonusOn = true; bonusUsedThisTurn = true; }
        if (soldierBonus[current] > 0 && GUILayout.Button($"Usar +1 soldados ({soldierBonus[current]})"))
        { soldierBonus[current]--; soldierBonusOn = true; bonusUsedThisTurn = true; }
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
        GUILayout.Label($"Prom. general {Fmt(Avg(all))} · más largo {Fmt(longest)} · perdidos {lostTurns} · por tiempo {timeouts}", richStyle);
        GUILayout.Label($"<size=11>{(hex ? "Hexagonal" : "Cuadrado")} {W}×{H}, {(castles ? "con" : "sin")} castillos, timer {(timerLimit > 0 ? timerLimit + " s" : "sin límite")}</size>", richStyle);
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
