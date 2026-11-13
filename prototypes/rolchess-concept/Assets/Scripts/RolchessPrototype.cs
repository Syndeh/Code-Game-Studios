// PROTOTYPE - NOT FOR PRODUCTION
// Question: Does moving up to 12 pieces and combining attacks each turn keep turns brisk
//           (average turn under 90 s, full local match under 40 min)?
// Date: 2026-11-13
//
// Hotseat 2 players. Everything is drawn with IMGUI: no scene, sprites or input setup needed.
// Press Play in any scene; the prototype creates itself.

using System.Collections.Generic;
using UnityEngine;

public class RolchessPrototype : MonoBehaviour
{
    // ---------- Tuning (hardcoded on purpose: prototype) ----------
    const int W = 24, H = 24;
    const int MaxMoves = 12;

    enum Kind { Soldado, Arquero, Escudero, Catapulta, Jinete, Demoledor, Rey }

    static readonly string[] Names   = { "Soldado", "Arquero", "Escudero", "Catapulta", "Jinete", "Demoledor", "Rey" };
    static readonly string[] Letters = { "S", "A", "E", "C", "J", "D", "R" };
    // Big table from page 1 of the notebook (Demoledor attack 30, Catapulta move 2).
    static readonly int[]  Mov    = { 4, 4, 4, 2, 6, 6, 6 };
    static readonly int[]  RMin   = { 1, 1, 1, 2, 1, 1, 1 };
    static readonly int[]  RMax   = { 1, 6, 1, 6, 1, 1, 1 };
    static readonly int[]  Atk    = { 8, 6, 4, 10, 8, 30, 8 };
    static readonly int[]  Shield = { 2, 2, 5, 3, 3, 1, 5 };
    static readonly int[]  Life   = { 25, 20, 15, 15, 20, 10, 30 };
    static readonly bool[] Diag   = { false, true, false, true, false, true, false };
    static readonly int[]  Limit  = { 12, 16, 12, 8, 8, 4, 1 };

    class Piece
    {
        public Kind kind;
        public int owner;
        public Vector2Int pos;
        public int life;
        public bool moved, attacked;
    }

    enum Phase { Roll, LostTurn, Recruit, Move, Attack, Over }

    readonly RectInt[] castle = { new RectInt(2, 9, 5, 5), new RectInt(17, 9, 5, 5) };
    readonly List<Piece> pieces = new List<Piece>();
    readonly int[] archerBonus = new int[2];
    readonly int[] soldierBonus = new int[2];

    int current;
    Phase phase;
    int die;
    int movesUsed;
    bool unlimitedMoves;
    bool archerBonusOn, soldierBonusOn, bonusUsedThisTurn;
    Piece selected;
    HashSet<Vector2Int> reach = new HashSet<Vector2Int>();
    Piece target;
    readonly List<Piece> attackers = new List<Piece>();
    string message = "";
    int winner = -1;

    // ---------- Measurement: the point of this prototype ----------
    float matchStart, turnStart, matchEnd = -1f;
    readonly List<float>[] turnTimes = { new List<float>(), new List<float>() };
    int lostTurns;
    int turnNumber;

    Vector2 boardOrigin = new Vector2(10, 10);
    int cs = 28;
    GUIStyle pieceStyle;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Boot()
    {
        if (FindAnyObjectByType<RolchessPrototype>() != null) return;
        var cam = Camera.main;
        if (cam == null) cam = new GameObject("Camera").AddComponent<Camera>();
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = new Color(0.12f, 0.12f, 0.14f);
        new GameObject("RolchessPrototype").AddComponent<RolchessPrototype>();
    }

    void Start()
    {
        for (int p = 0; p < 2; p++)
        {
            var c = castle[p];
            Add(Kind.Rey, p, new Vector2Int(c.x + 2, c.y + 2));
            int front = p == 0 ? 7 : 16;
            int second = p == 0 ? 8 : 15;
            for (int i = 0; i < 8; i++)
                Add(i % 2 == 0 ? Kind.Soldado : Kind.Arquero, p, new Vector2Int(front, 8 + i));
            for (int i = 0; i < 4; i++)
                Add(Kind.Escudero, p, new Vector2Int(second, 10 + i));
        }
        matchStart = Time.realtimeSinceStartup;
        StartTurn();
    }

    void Add(Kind k, int owner, Vector2Int pos)
    {
        pieces.Add(new Piece { kind = k, owner = owner, pos = pos, life = Life[(int)k] });
    }

    // ---------- Turn flow ----------
    void StartTurn()
    {
        turnNumber++;
        phase = Phase.Roll;
        movesUsed = 0;
        unlimitedMoves = false;
        archerBonusOn = soldierBonusOn = bonusUsedThisTurn = false;
        foreach (var p in pieces) { p.moved = false; p.attacked = false; }
        ClearSelection();
        turnStart = Time.realtimeSinceStartup;
    }

    void Roll()
    {
        die = Random.Range(1, 7);
        switch (die)
        {
            case 1: phase = Phase.LostTurn; message = "Salió 1: pierdes el turno."; break;
            case 2: archerBonus[current] = Mathf.Min(4, archerBonus[current] + 1); phase = Phase.Move; message = "Salió 2: +1 carga de ataque para arqueros."; break;
            case 3: soldierBonus[current] = Mathf.Min(4, soldierBonus[current] + 1); phase = Phase.Move; message = "Salió 3: +1 carga de ataque para soldados."; break;
            case 4: phase = Phase.Recruit; message = "Salió 4: elige un tipo para reclutar 4."; break;
            case 5: phase = Phase.Move; message = "Salió 5: la retirada al castillo no está en el prototipo."; break;
            default: unlimitedMoves = true; phase = Phase.Move; message = "Salió 6: este turno mueves todas las piezas que quieras."; break;
        }
    }

    void EndTurn(bool lost)
    {
        float t = Time.realtimeSinceStartup - turnStart;
        if (lost) lostTurns++;
        else turnTimes[current].Add(t);
        Debug.Log($"[Rolchess] Turno {turnNumber} J{current + 1}: {t:0.0}s{(lost ? " (perdido)" : "")}, piezas movidas {movesUsed}");
        current = 1 - current;
        message = "";
        StartTurn();
    }

    void Recruit(Kind k)
    {
        int alive = 0;
        foreach (var p in pieces) if (p.owner == current && p.kind == k) alive++;
        int n = Mathf.Min(4, Limit[(int)k] - alive);
        var c = castle[current];
        int placed = 0;
        for (int x = c.xMin - 1; x <= c.xMax && placed < n; x++)
            for (int y = c.yMin - 1; y <= c.yMax && placed < n; y++)
            {
                var v = new Vector2Int(x, y);
                if (c.Contains(v) || !InBoard(v) || PieceAt(v) != null) continue;
                Add(k, current, v);
                placed++;
            }
        message = $"Reclutaste {placed} {Names[(int)k]}.";
        phase = Phase.Move;
    }

    // ---------- Rules helpers ----------
    static bool InBoard(Vector2Int v) => v.x >= 0 && v.y >= 0 && v.x < W && v.y < H;

    Piece PieceAt(Vector2Int v)
    {
        foreach (var p in pieces) if (p.pos == v) return p;
        return null;
    }

    bool CanEnter(Piece p, Vector2Int v)
    {
        if (!InBoard(v) || PieceAt(v) != null) return false;
        for (int i = 0; i < 2; i++)
            if (castle[i].Contains(v) && !(p.kind == Kind.Rey && p.owner == i)) return false;
        return true;
    }

    // Orthogonal steps through empty squares, up to the piece's movement points.
    HashSet<Vector2Int> ComputeReach(Piece p)
    {
        var result = new HashSet<Vector2Int>();
        var dist = new Dictionary<Vector2Int, int> { [p.pos] = 0 };
        var queue = new Queue<Vector2Int>();
        queue.Enqueue(p.pos);
        Vector2Int[] dirs = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };
        while (queue.Count > 0)
        {
            var cur = queue.Dequeue();
            int d = dist[cur];
            if (d >= Mov[(int)p.kind]) continue;
            foreach (var dir in dirs)
            {
                var n = cur + dir;
                if (dist.ContainsKey(n) || !CanEnter(p, n)) continue;
                dist[n] = d + 1;
                result.Add(n);
                queue.Enqueue(n);
            }
        }
        return result;
    }

    static bool InRange(Piece a, Piece t)
    {
        int dx = t.pos.x - a.pos.x, dy = t.pos.y - a.pos.y;
        int ax = Mathf.Abs(dx), ay = Mathf.Abs(dy);
        bool straight = dx == 0 || dy == 0;
        bool diagonal = ax == ay;
        if (!(straight || (Diag[(int)a.kind] && diagonal))) return false;
        int d = Mathf.Max(ax, ay);
        return d >= RMin[(int)a.kind] && d <= RMax[(int)a.kind];
    }

    int AttackValue(Piece a)
    {
        int v = Atk[(int)a.kind];
        if (a.kind == Kind.Arquero && archerBonusOn) v++;
        if (a.kind == Kind.Soldado && soldierBonusOn) v++;
        return v;
    }

    bool CanStillAttack(Piece enemy)
    {
        foreach (var p in pieces)
            if (p.owner == current && !p.attacked && InRange(p, enemy)) return true;
        return false;
    }

    int AttackSum()
    {
        int s = 0;
        foreach (var a in attackers) s += AttackValue(a);
        return s;
    }

    void ResolveAttack()
    {
        int sum = AttackSum();
        int dmg = Mathf.Max(0, sum - Shield[(int)target.kind]);
        target.life -= dmg;
        foreach (var a in attackers) a.attacked = true;
        message = $"{attackers.Count} atacante(s): {sum} - escudo {Shield[(int)target.kind]} = {dmg} de daño a {Names[(int)target.kind]}.";
        if (target.life <= 0)
        {
            pieces.Remove(target);
            message += " ¡Eliminado!";
            if (target.kind == Kind.Rey)
            {
                winner = current;
                phase = Phase.Over;
                matchEnd = Time.realtimeSinceStartup;
                turnTimes[current].Add(matchEnd - turnStart);
                Debug.Log($"[Rolchess] Ganó J{winner + 1}. Partida {Fmt(matchEnd - matchStart)}, turnos {turnNumber}, " +
                          $"promedio J1 {Fmt(Avg(turnTimes[0]))}, J2 {Fmt(Avg(turnTimes[1]))}, turnos perdidos {lostTurns}");
            }
        }
        target = null;
        attackers.Clear();
    }

    void ClearSelection()
    {
        selected = null;
        reach.Clear();
        target = null;
        attackers.Clear();
    }

    // ---------- Input ----------
    void OnCellClicked(Vector2Int cell)
    {
        var p = PieceAt(cell);
        if (phase == Phase.Move)
        {
            if (selected != null && reach.Contains(cell))
            {
                selected.pos = cell;
                selected.moved = true;
                if (selected.kind != Kind.Rey && !unlimitedMoves) movesUsed++;
                selected = null;
                reach.Clear();
                return;
            }
            if (p != null && p.owner == current && !p.moved &&
                (p.kind == Kind.Rey || unlimitedMoves || movesUsed < MaxMoves))
            {
                selected = p;
                reach = ComputeReach(p);
            }
            else
            {
                selected = null;
                reach.Clear();
            }
        }
        else if (phase == Phase.Attack)
        {
            if (p == null) return;
            if (p.owner != current)
            {
                if (CanStillAttack(p)) { target = p; attackers.Clear(); }
            }
            else if (target != null && !p.attacked && InRange(p, target))
            {
                if (attackers.Contains(p)) attackers.Remove(p);
                else attackers.Add(p);
            }
        }
    }

    // ---------- Drawing ----------
    void OnGUI()
    {
        if (pieceStyle == null)
        {
            pieceStyle = new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter, fontStyle = FontStyle.Bold };
            pieceStyle.normal.textColor = Color.white;
        }
        cs = Mathf.Max(12, Mathf.Min((Screen.width - 300) / W, (Screen.height - 20) / H));
        pieceStyle.fontSize = Mathf.Max(8, cs / 3);
        var boardRect = new Rect(boardOrigin.x, boardOrigin.y, W * cs, H * cs);

        var e = Event.current;
        if (e.type == EventType.MouseDown && e.button == 0 && boardRect.Contains(e.mousePosition) && phase != Phase.Over)
        {
            var cell = new Vector2Int((int)((e.mousePosition.x - boardOrigin.x) / cs), (int)((e.mousePosition.y - boardOrigin.y) / cs));
            OnCellClicked(cell);
            e.Use();
        }

        DrawBoard();
        DrawPanel(new Rect(boardRect.xMax + 12, 10, 270, Screen.height - 20));
    }

    Rect CellRect(Vector2Int v) => new Rect(boardOrigin.x + v.x * cs, boardOrigin.y + v.y * cs, cs, cs);

    void Fill(Rect r, Color c)
    {
        var old = GUI.color;
        GUI.color = c;
        GUI.DrawTexture(r, Texture2D.whiteTexture);
        GUI.color = old;
    }

    void DrawBoard()
    {
        for (int x = 0; x < W; x++)
            for (int y = 0; y < H; y++)
            {
                var v = new Vector2Int(x, y);
                Color c = (x + y) % 2 == 0 ? new Color(0.30f, 0.30f, 0.32f) : new Color(0.26f, 0.26f, 0.28f);
                if (castle[0].Contains(v)) c = new Color(0.20f, 0.28f, 0.45f);
                if (castle[1].Contains(v)) c = new Color(0.45f, 0.22f, 0.22f);
                if (reach.Contains(v)) c = new Color(0.25f, 0.55f, 0.30f);
                Fill(CellRect(v), c);
            }

        foreach (var p in pieces)
        {
            var r = CellRect(p.pos);
            var inner = new Rect(r.x + 2, r.y + 2, r.width - 4, r.height - 4);
            Color baseCol = p.owner == 0 ? new Color(0.25f, 0.45f, 0.95f) : new Color(0.90f, 0.30f, 0.30f);
            bool spent = p.owner == current && ((phase == Phase.Move && p.moved) || (phase == Phase.Attack && p.attacked));
            if (spent) baseCol *= 0.55f;

            if (phase == Phase.Attack && p.owner != current && CanStillAttack(p))
                Fill(r, new Color(1f, 0.85f, 0.2f, 0.6f));
            if (phase == Phase.Attack && target != null && p.owner == current && !p.attacked && InRange(p, target))
                Fill(r, new Color(1f, 1f, 1f, 0.5f));
            if (p == selected || p == target || attackers.Contains(p))
                Fill(r, new Color(1f, 0.6f, 0.1f));

            baseCol.a = 1f;
            Fill(inner, baseCol);
            GUI.Label(new Rect(inner.x, inner.y - inner.height * 0.15f, inner.width, inner.height), Letters[(int)p.kind], pieceStyle);
            int oldSize = pieceStyle.fontSize;
            pieceStyle.fontSize = Mathf.Max(7, cs / 4);
            GUI.Label(new Rect(inner.x, inner.y + inner.height * 0.3f, inner.width, inner.height * 0.7f), p.life.ToString(), pieceStyle);
            pieceStyle.fontSize = oldSize;
        }
    }

    void DrawPanel(Rect area)
    {
        GUILayout.BeginArea(area);
        string who = current == 0 ? "Jugador 1 (azul)" : "Jugador 2 (rojo)";

        if (phase == Phase.Over)
        {
            GUILayout.Label($"<b>Ganó el {(winner == 0 ? "Jugador 1 (azul)" : "Jugador 2 (rojo)")}</b>", Rich());
            DrawStats(true);
            GUILayout.EndArea();
            return;
        }

        GUILayout.Label($"<b>Turno {turnNumber}: {who}</b>", Rich());
        GUILayout.Label($"Turno actual: {Fmt(Time.realtimeSinceStartup - turnStart)}");
        GUILayout.Space(6);

        switch (phase)
        {
            case Phase.Roll:
                if (GUILayout.Button("Tirar dado", GUILayout.Height(36))) Roll();
                break;
            case Phase.LostTurn:
                GUILayout.Label($"Dado: {die}");
                if (GUILayout.Button("Pasar el turno", GUILayout.Height(36))) EndTurn(true);
                break;
            case Phase.Recruit:
                GUILayout.Label($"Dado: {die}. ¿Qué reclutas?");
                for (int k = 0; k < 6; k++)
                    if (GUILayout.Button(Names[k])) Recruit((Kind)k);
                if (GUILayout.Button("No reclutar")) phase = Phase.Move;
                break;
            case Phase.Move:
                GUILayout.Label($"Dado: {die} · MOVER");
                GUILayout.Label(unlimitedMoves ? "Piezas movidas: sin límite" : $"Piezas movidas: {movesUsed}/{MaxMoves} (+ Rey)");
                GUILayout.Label("Clic en tu pieza y luego en una casilla verde.");
                if (GUILayout.Button("Terminar movimiento → Atacar", GUILayout.Height(32)))
                {
                    phase = Phase.Attack;
                    ClearSelection();
                }
                break;
            case Phase.Attack:
                GUILayout.Label($"Dado: {die} · ATACAR");
                GUILayout.Label("1) Clic en un enemigo marcado en amarillo.\n2) Clic en tus piezas resaltadas para sumarlas.\n3) Resolver.");
                DrawBonus();
                if (target != null)
                {
                    int sum = AttackSum();
                    int sh = Shield[(int)target.kind];
                    GUILayout.Label($"Objetivo: {Names[(int)target.kind]} (vida {target.life}, escudo {sh})");
                    GUILayout.Label($"Atacantes: {attackers.Count} · total {sum} → daño {Mathf.Max(0, sum - sh)}");
                    GUI.enabled = attackers.Count > 0;
                    if (GUILayout.Button("Resolver ataque", GUILayout.Height(30))) ResolveAttack();
                    GUI.enabled = true;
                }
                if (GUILayout.Button("Terminar turno", GUILayout.Height(32))) EndTurn(false);
                break;
        }

        GUILayout.Space(6);
        GUILayout.Label($"Cargas J1: arqueros {archerBonus[0]}, soldados {soldierBonus[0]}");
        GUILayout.Label($"Cargas J2: arqueros {archerBonus[1]}, soldados {soldierBonus[1]}");
        if (!string.IsNullOrEmpty(message)) GUILayout.Label(message);
        if (selected != null)
        {
            int k = (int)selected.kind;
            GUILayout.Label($"{Names[k]} · mov {Mov[k]} · alcance {RMin[k]}-{RMax[k]} · atq {Atk[k]} · esc {Shield[k]} · vida {selected.life}");
        }
        GUILayout.Space(6);
        DrawStats(false);
        GUILayout.Label("S Soldado · A Arquero · E Escudero · C Catapulta · J Jinete · D Demoledor · R Rey");
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
        {
            archerBonus[current]--; archerBonusOn = true; bonusUsedThisTurn = true;
        }
        if (soldierBonus[current] > 0 && GUILayout.Button($"Usar +1 soldados ({soldierBonus[current]})"))
        {
            soldierBonus[current]--; soldierBonusOn = true; bonusUsedThisTurn = true;
        }
    }

    void DrawStats(bool final)
    {
        float match = (final ? matchEnd : Time.realtimeSinceStartup) - matchStart;
        GUILayout.Label("<b>Medición</b>", Rich());
        GUILayout.Label($"Partida: {Fmt(match)} (meta: menos de 40:00)");
        GUILayout.Label($"Promedio de turno J1: {Fmt(Avg(turnTimes[0]))} ({turnTimes[0].Count} turnos)");
        GUILayout.Label($"Promedio de turno J2: {Fmt(Avg(turnTimes[1]))} ({turnTimes[1].Count} turnos)");
        var all = new List<float>(turnTimes[0]);
        all.AddRange(turnTimes[1]);
        GUILayout.Label($"Promedio general: {Fmt(Avg(all))} (meta: menos de 1:30)");
        float longest = 0f;
        foreach (var t in all) longest = Mathf.Max(longest, t);
        GUILayout.Label($"Turno más largo: {Fmt(longest)} · turnos perdidos: {lostTurns}");
    }

    GUIStyle Rich() => new GUIStyle(GUI.skin.label) { richText = true };

    static float Avg(List<float> l)
    {
        if (l.Count == 0) return 0f;
        float s = 0f;
        foreach (var t in l) s += t;
        return s / l.Count;
    }

    static string Fmt(float s) => $"{(int)(s / 60)}:{(int)(s % 60):00}";
}
