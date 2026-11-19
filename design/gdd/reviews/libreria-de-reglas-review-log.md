# Review log — Librería de reglas (contrato)

## Review — 2026-11-19 — Verdict: NEEDS REVISION
Review mode: full · Workflow: full
Scope signal: L
Specialists: game-designer, systems-designer, qa-lead, unity-specialist, network-programmer, creative-director (senior)
Blocking items: 11 | Recommended: 5
Summary: The core design is sound: Aplicar as the only way in, injected chance, integer-only math, and a preview that runs the same code as resolution. The gaps were in the specification, at exactly the points where determinism breaks: integer edge cases, resolution order, the hash and rejection-code precedence. Online sync also bypassed rule 1. All 11 blockers were revised in the same session with the user's decisions: resolver wins when both Kings die (the player whose turn it is, provisional until #3), the authority is trusted openly for MVP, and Rendirse is required before online ships. A re-review decides approval.
Prior verdict resolved: First review
Findings:
- [BLOCKING] Formulas / Core Rules 5: integer width and overflow undefined; p_i overflow ([INT_MAX, INT_MAX, 102] wraps to 100); damage and HP not floored at 0; k not tied to table length; tables validated at roll time.
- [BLOCKING] Core Rules: resolution order (simultaneous? does a dead piece strike?) and the combined-hit flag undefined.
- [BLOCKING] Core Rules 4: no closed list of codes or precedence; TiradaNoEsperada had two meanings; codes not marked player-facing or internal.
- [BLOCKING] Overview / States: "el único azar" contradicted the castle draw; CrearPartida never entered "esperando tirada"; TurnTimeout during a pending roll undefined.
- [BLOCKING] Core Rules 11: no canonical serialization; hash coverage (undo stack, pending roll) undefined.
- [BLOCKING] Core Rules 3: sender never authenticated; local/online mode not in the config.
- [BLOCKING] Edge Cases / AC-26: client adopting the server state bypassed rule 1 (now Resincronizar, rule 14).
- [BLOCKING] Core Rules 9: preview copy cost unspecified (WebGL GC); no allocation AC.
- [BLOCKING] AC-7: banned list too narrow to guarantee determinism.
- [BLOCKING] Acceptance Criteria: AC-5/10/12/18 depended on unwritten GDDs; AC-24/25 environment undefined and "122 piezas" unsourced; AC-26 S1 event undefined; ACs missing for the combined flag, castle config, precedence, hash, QuitarAtaque/Cubrir.
- [BLOCKING] Formulas: "las primeras opciones reciben un número más" false for k≠3; bare literal 100.
- [RECOMMENDED] AC-20 and AC-22: mark PROVISIONAL until #3.
- [RECOMMENDED] Player Fantasy: narrow "El tablero no hace trampa" to what the library can promise; state openly that the authority is trusted.
- [RECOMMENDED] Core Rules 13: tie Rendirse to the online launch, not "Fase 2".
- [RECOMMENDED] Edge Cases: fix the wording of "suma 100 pero toda cara en 0".
- [RECOMMENDED] AC-6: golden files, .NET per PR and WebGL nightly.
Note: the hashes below are of the files as reviewed (commit 7f3fe00), taken before the same-session revisions, so the next check reads CHANGED and runs a full re-review.
Reviewed-Content-Hash: design/gdd/libreria-de-reglas.md ccb50f9a7fcda3ac58aac62fe0bda5ad44da6ef8
Reviewed-Content-Hash: design/registry/entities.yaml f59e0073cef38c0613b174aa10e181a3cbbda913
Reviewed-Content-Hash: .claude/skills/design-review/SKILL.md 4dcc8b81162613c757144532f1b72708ab3ba27f
Reviewed-Content-Hash: design/gdd/game-concept.md 590e85748059dfb71f99b94cd04d2b367360de33
Reviewed-Content-Hash: design/game-brief.md 1f759b960d5955f6eb96f0f0f6f8c3802b1e2c0c

## Review — 2026-11-19 — Verdict: NEEDS REVISION
Review mode: full · Workflow: full
Scope signal: L
Specialists: game-designer, systems-designer, qa-lead, unity-specialist, network-programmer, creative-director (senior)
Blocking items: 11 | Recommended: 7
Summary: The 11 prior blockers were closed. The remaining gaps appeared where rules reviewed separately meet: TurnTimeout had no turn/phase identity, so a stale timeout could resolve attacks before they were assigned. NoEsTuTurno rejected the authority's own commands. The hash field list was incomplete and could collide. Runtime Modificador tables were unvalidated. Rules 3 and 14 contradicted each other on the online model. Several ACs could not run in the declared test root. The core (purity, verified formulas, precedence) is sound. All 11 were revised in the same session with the user's decisions: a single rounding over the total, auto-recruit on timeout, EditMode + dotnet test root, and combinado counting rival attackers only. A re-review decides approval.
Prior verdict resolved: Yes (all 11 prior blockers closed; new blockers found)
Findings:
- [BLOCKING] Core Rules 3/Edge Cases: TurnTimeout without turn/phase; stale timeout accepted; FaseIncorrecta race code unreachable; timeout semantics (pending recruit, covers, 0 attacks, auto-advance) undefined.
- [BLOCKING] Core Rules 4: NoEsTuTurno (row 8) rejected authority Tirada/TurnTimeout.
- [BLOCKING] Core Rules 11: hash field list incomplete; canonical form without length prefixes/presence flags.
- [BLOCKING] Core Rules 6/Formulas: runtime-derived Modificador tables had no owner, rounding or validation.
- [BLOCKING] Core Rules 5/7: initial and multi-recruit ID order undefined; sort stability not required.
- [BLOCKING] Core Rules 3/14: online model contradictory; Resincronizar without match id, old-state check, Terminada rule or reason.
- [BLOCKING] Core Rules 2: castle options validated only for the drawn one (ambiguous).
- [BLOCKING] Edge Cases/Rule 9: preview with no attacks returned an empty list, contradicting rules 8/9 and AC-11.
- [BLOCKING] Core Rules 1/5/9: overflow behaviour undefined; new-state-per-command contradicted zero-allocation preview.
- [BLOCKING] Core Rules 16: no end-reason vocabulary (DobleRey, Rendicion, reserved CierrePorTiempo/Tablas), no Rendirse.
- [BLOCKING] Acceptance Criteria: test root undefined; AC-24/25/33 gated on an undefined reference; AC-25 measured the rejection path; AC-32 untestable at library level; precedence, rule 15 and Resincronizar ACs missing; AC-7 evadable grep.
- [RECOMMENDED] Rule 8: combinado counted friendly fire.
- [RECOMMENDED] Rule 5: bruto per hit vs summed ambiguous.
- [RECOMMENDED] AC-15: missing empty, all-zero, k=1, 10-face and end-zero tables (still open).
- [RECOMMENDED] Registry: table-length cap 10 and k_sorteo rename (still open).
- [RECOMMENDED] Undo-stack bounds under Distracción/Arremetida (still open).
- [RECOMMENDED] Player Fantasy: the authority also controls timeouts.
- [RECOMMENDED] AC-21 byte flip, AC-1 aliasing, AC-6 circular golden file, AC-22 both players (AC-22 done; rest open).
Note: the hashes below are of the files as reviewed (commit e69e80a), taken before the same-session revisions, so the next check reads CHANGED and runs a full re-review. Deviation from the senior verdict: Resincronizar into Terminada is allowed (out of Terminada is rejected).
Reviewed-Content-Hash: design/gdd/libreria-de-reglas.md d85bad196eda28dbbee46ff4945ad29cde767b7a
Reviewed-Content-Hash: design/registry/entities.yaml c81cedfdafab4dac2dfed253c99798ca4d1e7173
Reviewed-Content-Hash: .claude/skills/design-review/SKILL.md 4dcc8b81162613c757144532f1b72708ab3ba27f
Reviewed-Content-Hash: design/gdd/game-concept.md 590e85748059dfb71f99b94cd04d2b367360de33
Reviewed-Content-Hash: design/game-brief.md 1f759b960d5955f6eb96f0f0f6f8c3802b1e2c0c

## Review — 2026-11-19 — Verdict: NEEDS REVISION
Review mode: full · Workflow: full
Scope signal: M
Specialists: game-designer, systems-designer, qa-lead, unity-specialist, network-programmer, creative-director (senior)
Blocking items: 11 | Recommended: 6
Summary: The rules core has converged: every formula and AC number was verified by hand and the registry matches. The gaps moved to the online contract (replay of old or duplicate commands, abandonment and the 25-minute cap with no command, Resincronizar unimplementable and unable to fix a wrongly finished client), totality (face roles unvalidated, TurnTimeout stalling when auto-recruit is impossible), and test design (circular golden files, untestable AC-43, a web leg with no runner). The creative director ruled NEEDS REVISION over systems-designer's MAJOR. The document grows by absorbing ADR, #28 and #3/#12 content, so that detail was moved to Open Questions. All 11 were revised in the same session with the user's decisions: CerrarPartida carries the winner, an impossible recruit is lost and the turn advances, anti-replay lives in the library (contadorEsperado/ComandoViejo), and the GDD was trimmed.
Prior verdict resolved: Yes (all 11 prior blockers closed; new blockers found)
Findings:
- [BLOCKING] Core Rules 3/4: no anti-replay; a duplicate Deshacer undoes twice and old-turn commands become valid again.
- [BLOCKING] Core Rules 3/16: no command produces Abandono/CierrePorTiempo/Tablas; pillar 4 and abandonment unenforceable.
- [BLOCKING] Core Rules 14: Resincronizar has no declared hash to check, unclear which state each step reads, Divergencia undefined under prediction, a wrongly Terminada client cannot be corrected.
- [BLOCKING] Core Rules 2/Formulas: face roles unvalidated; reparto_liberado can have nowhere to put L, reaching the "unreachable" ErrorInterno.
- [BLOCKING] Core Rules 17: TurnTimeout(mover) not total when auto-recruit is impossible; the turn can stall.
- [BLOCKING] Core Rules 11: field list missing face roles, castle options, the chosen base table; pending roll in a terminal state; "same hash by different paths" false.
- [BLOCKING] Core Rules 6/10: ProbabilidadesSiguienteTirada only defined while a roll is pending (pillar 3b).
- [BLOCKING] Acceptance Criteria: circular golden files (AC-6/30/41), no event canonical form, no hand-written oracles or hash vectors.
- [BLOCKING] Acceptance Criteria: AC-1 covered 4 of 11 commands; AC-43 uncompilable; AC-21 vague; AC-37 hidden assumption.
- [BLOCKING] Acceptance Criteria: unmarked PROVISIONAL (AC-12b/24/25/40); AC-33 split; web legs have no Unity runner; test asmdef wording contradictory.
- [BLOCKING] Dependencies/States/Rule 4: Catalog schema dependency, states table vs AC-34, row 1 command version.
- [RECOMMENDED] Network: server epoch, integrity vs authenticity, emisor in the log, commit-reveal, pending queue and liveness to #28 (moved to Open Questions).
- [RECOMMENDED] Game feel: automatico recruit flag and combinado count (done); preview "tu Rey también muere", UI confirm for Rendirse and empty Resolver (open).
- [RECOMMENDED] Rules: row 12 second ID, phase-to-stage mapping, damage and shield >= 0, k = 1 (done); AC-42 source of the rival's damage, first turn after the castle roll (open).
- [RECOMMENDED] ADR: exceptionSupport, Math.BigMul, .csproj gate for AC-7, unchecked hash module, wider ban list (moved to Open Questions).
- [RECOMMENDED] QA: remaining Q6 ACs (undo bounds, immutability, wall IDs, prediction rollback), perf p95 robustness, AC-30/AC-45 overlap (partly done).
- [RECOMMENDED] Out of this GDD: Grito de guerra +2 stats contradicts pillar 3a; owner creative-director and #11 (in Open Questions).
Note: the hashes below are of the files as reviewed (commit 0e70864), taken before the same-session revisions, so the next check reads CHANGED and runs a full re-review. Deviation from the senior verdict: the creative director suggested a delta-only fourth pass; this skill forbids partial re-reviews, so the next pass is a full review.
Reviewed-Content-Hash: design/gdd/libreria-de-reglas.md db7882c18affc710736bc3bbde49f8f426ed84b3
Reviewed-Content-Hash: design/registry/entities.yaml 09bf6ff61cb8635fe619cda7c0825d9898cb8aac
Reviewed-Content-Hash: .claude/skills/design-review/SKILL.md 4dcc8b81162613c757144532f1b72708ab3ba27f
Reviewed-Content-Hash: design/gdd/game-concept.md 590e85748059dfb71f99b94cd04d2b367360de33
Reviewed-Content-Hash: design/game-brief.md 1f759b960d5955f6eb96f0f0f6f8c3802b1e2c0c

## Review — 2026-11-19 — Verdict: NEEDS REVISION
Review mode: full · Workflow: full
Scope signal: M
Specialists: game-designer, systems-designer, qa-lead, unity-specialist, network-programmer, creative-director (senior)
Blocking items: 9 | Recommended: 5
Summary: The core has converged (formulas, registry and examples re-verified by hand); what remained were local contract closures: anti-replay half-applied (Tirada carried no counter), totality against hostile input and unbounded Catalog values, Resincronizar trusting a caller-supplied motive and not checking immutable fields, an undefined "possible action" allowing invisible mid-phase cascades, and concrete AC errors. The creative director ruled NEEDS REVISION (not MAJOR) and set a net-zero size rule plus a narrowed blocking bar for the fifth pass (totality, determinism, anti-replay, or an incorrect AC). All 9 were revised in the same session with the user's decisions: the library computes the CierrePorTiempo winner (King life, then live pieces, then no winner), turn 1 does not roll the Modificador, and phases end automatically only when they start with no possible action.
Prior verdict resolved: Yes (all 11 third-pass blockers stayed closed; new blockers found)
Findings:
- [BLOCKING] Core Rules 3/6: Tirada had no counter; a resent old roll was accepted as the next turn's (now contadorTiradaEsperado / TiradaVieja).
- [BLOCKING] Core Rules 13/3: replaying predicted commands contradicted contadorEsperado; contadorComandos scope unstated.
- [BLOCKING] Core Rules 14: Resincronizar motive declared by the caller; immutable fields and well-formedness unchecked.
- [BLOCKING] Core Rules 4/5: hostile int32 fields and unbounded Catalog values could reach ErrorInterno.
- [BLOCKING] Core Rules 17: "possible action" undefined; mid-phase auto-end cascades invisible to the preview.
- [BLOCKING] Core Rules 2/6, AC-3/36/54: whether turn 1 rolls the Modificador was undefined.
- [BLOCKING] Formulas: table size documented 1-10 while rule 2 forces at least 2.
- [BLOCKING] Acceptance Criteria: AC-5, AC-21, AC-24, AC-29, AC-37 incorrect.
- [BLOCKING] Acceptance Criteria: unmarked PROVISIONAL ACs; no coverage or golden-file version gate.
- [RECOMMENDED] ADR: exception-free checked arithmetic (WebGL exceptionSupport), IL2CPP/WebGL perf and 0-GC measurement, ban Dictionary/HashSet, .csproj naming, strict decoder, state and undo-stack caps (moved to Open Questions).
- [RECOMMENDED] Server #28: epoch after server restart (EstadoViejo dead end), ErrorInterno policy online, canonical replay log (Open Questions).
- [RECOMMENDED] Rules: face-to-cause mapping for recruit faces; preview does not show the rival's probability change.
- [RECOMMENDED] QA: numeric rewrites for AC-53/25/45/54/15; web Integration ADVISORY vs standard.
- [RECOMMENDED] Game: DobleRey favours the player on turn (provisional, #3).
Note: the GDD is deliberately not stamped. Its reviewed version was an uncommitted working copy that the same-session revisions overwrote, so no hash of it exists; leaving it unstamped makes the next check run a full re-review. The registry hash is taken after the cara_por_tirada note fix (k 2-10). Size: 82,113 bytes before, 82,251 after (+138; net-zero target missed by 138 bytes). Disagreements left to the user: game-designer wanted Reconexion done by replaying commands (CD: #28's choice) and DobleRey decided now (CD: #3).
Reviewed-Content-Hash: design/registry/entities.yaml 09f3afc9fa59c98361ab76eaa45980a115eae095
Reviewed-Content-Hash: .claude/skills/design-review/SKILL.md 4dcc8b81162613c757144532f1b72708ab3ba27f
Reviewed-Content-Hash: design/gdd/game-concept.md 590e85748059dfb71f99b94cd04d2b367360de33
