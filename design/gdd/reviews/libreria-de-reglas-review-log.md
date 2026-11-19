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

## Review — 2026-11-19 — Verdict: NEEDS REVISION
Review mode: full · Workflow: full
Scope signal: M
Specialists: game-designer, systems-designer, qa-lead, unity-specialist, network-programmer, creative-director (senior)
Blocking items: 6 | Recommended: 9
Summary: Convergence continues (9 blockers to 6); all formulas, examples and the registry were re-verified by hand and are correct. The remaining blockers were local contract holes: Aplicar did not receive the Catalog (implicit input), no definition of a valid loaded/resynced state (a well-formed state with counters at int.MaxValue made every command ErrorInterno, contradicting rule 5), and a mandatory recruit with no available type stalling a timer-less local match; plus three incorrect ACs. The creative director ruled NEEDS REVISION and kept the narrowed bar; if the sixth pass finds only recommendations, it should be APPROVED. All 6 were revised in the same session with the user's decisions: the impossible recruit is lost when the recruit phase starts, C_MAX = 1,000,000 (provisional), CierrePorTiempo mid-turn left in Open Questions.
Prior verdict resolved: Yes (all 9 fourth-pass blockers stayed closed; new blockers found)
Findings:
- [BLOCKING] Core Rules 1/2/11/12: Aplicar(estado, comando) had no Catalog input; now Aplicar(contexto, estado, comando), context hash checked at row 1.
- [BLOCKING] Core Rules 5/11/14, AC-21/AC-43: no valid-state invariants for Cargar/Resincronizar; a hash-correct state could hit ErrorInterno on every command.
- [BLOCKING] Core Rules 17: a pending recruit with no available type counted as a possible action and stalled a match with no timer.
- [BLOCKING] Acceptance Criteria: AC-25 used an attack-phase fixture for Mover (FaseIncorrecta).
- [BLOCKING] Acceptance Criteria/Edge Cases: AC-29 TiradaVieja case lacked a pending roll; edge case said TiradaVieja where it is TiradaNoEsperada.
- [BLOCKING] Acceptance Criteria: AC-58 unreachable (row 14 codes belong to unwritten GDDs; row 1 needs a test constructor).
- [RECOMMENDED] Core Rules 5/AC-43: exception-free checked arithmetic on WebGL; removed "en todas las plataformas por igual" (web leg to ADR).
- [RECOMMENDED] Acceptance Criteria: AC-56 fuzz died at row 10; strengthened (copies, fixed xorshift seed, half-correct counters, CrearPartida/Resincronizar fuzz).
- [RECOMMENDED] Core Rules 14: false claim "una divergencia real nunca se oculta" removed; counter monotonicity left to #28 (it could block correcting a real divergence).
- [RECOMMENDED] Player Fantasy/UI: "no silent rejections" vs internal codes; reworded to a neutral notice for discarded player actions.
- [RECOMMENDED] Game: CierrePorTiempo can land mid-turn with the preview showing MUERE; tie-break invisible (Open Questions, #3/#12).
- [RECOMMENDED] Network: ComandoAdelantado, Resincronizar with an identical state, Rendirse without match id (Open Questions, #28).
- [RECOMMENDED] Rules: config caps for board size, Catalog size, max bonus (Open Questions).
- [RECOMMENDED] QA: AC-44 missing orders, AC-29 not a generated table, AC-45 ambiguous THEN, AC-59 vs provisional golden regeneration (open); AC-47/51/52/53/54 and PROVISIONAL marks (done).
- [RECOMMENDED] Engine: 0-GC on IL2CPP, nightly web runner does not exist yet, AC that both projects compile the same files (ADR).
Note: the hashes below are of the files as reviewed (commit 29d89f3), taken before the same-session revisions, so the next check reads CHANGED and runs a full re-review. Size: 82,251 bytes before, 82,232 after (net-zero met). Deviation from the senior verdict: the creative director asked the sixth pass to focus on the changes; this skill forbids partial re-reviews, so it will be a full review with the same bar. Disagreements: unity-specialist rated the WebGL overflow blocking (CD: ADR); qa-lead rated AC-56 blocking (CD: weak, not wrong); game-designer found no blockers (CD agreed for its findings).
Reviewed-Content-Hash: design/gdd/libreria-de-reglas.md f4eb9a8296f0a47bed2fb53bc63e01433ca7331e
Reviewed-Content-Hash: design/registry/entities.yaml 09f3afc9fa59c98361ab76eaa45980a115eae095
Reviewed-Content-Hash: .claude/skills/design-review/SKILL.md 4dcc8b81162613c757144532f1b72708ab3ba27f
Reviewed-Content-Hash: design/gdd/game-concept.md 590e85748059dfb71f99b94cd04d2b367360de33
Reviewed-Content-Hash: design/game-brief.md 1f759b960d5955f6eb96f0f0f6f8c3802b1e2c0c

## Review — 2026-11-19 — Verdict: NEEDS REVISION
Review mode: full · Workflow: full
Scope signal: M
Specialists: game-designer, systems-designer, qa-lead, unity-specialist, network-programmer, creative-director (senior)
Blocking items: 6 groups (14 findings) | Recommended: 7 groups
Summary: Formulas, examples and registry re-verified by hand and correct; all fifth-pass blockers stayed closed. The remaining blockers are one family the creative director called a method problem, not a design problem: "valid state" was an enumerated list, so each pass found the missing case (a hash-correct state with an unbounded bonus or a dangling attack ID reached ErrorInterno; Aplicar could produce a state past C_MAX that Cargar rejects). Plus four one-line totality/determinism holes and five incorrect ACs. Revised in the same session: a per-field domain table with EsValido as an iff, a closure rule (LimitePartida, row 15), concept-tight caps, and the user's decisions: LimitePartida with room for Rendirse/CerrarPartida, CierrePorTiempo rejected before the castle draw, caps from the concept (24 × 15 board, 61 pieces, bonus 0-10).
Prior verdict resolved: Yes (all 6 fifth-pass blockers stayed closed; new blockers found)
Findings:
- [BLOCKING] Core Rules 5/11: valid-state list incomplete (life, bonuses, types vs Catalog, enums, attack/cover refs, undo stack, walls, castle pair, pending roll); hash-correct states could reach ErrorInterno. Now a domain table.
- [BLOCKING] Core Rules 4/11: Aplicar not closed under the valid-state list (counter past C_MAX accepted, then unloadable). Now LimitePartida (row 15), En curso implies contadorComandos <= C_MAX - 1.
- [BLOCKING] Core Rules 11/16: motive-to-winner mapping undefined; decoder had no per-collection count cap (50,000,000 prefix allocates). Both now in the table/rule 16.
- [BLOCKING] Core Rules 3: CierrePorTiempo while waiting for the castle draw had no Kings to compare (now CierreInvalido).
- [BLOCKING] Core Rules 4: emitter outside {0, 1, authority} had no rejection code (now EmisorInvalido, row 4).
- [BLOCKING] Core Rules 5: checked arithmetic made the serializer's narrowing casts throw on valid values (exception now covers writer, reader and hash, with masks).
- [BLOCKING] Core Rules 1/2: CrearPartida accepted a declared Catalog hash (now PrepararContexto computes it; Cargar signature; Resincronizar checks the context).
- [BLOCKING] Acceptance Criteria: AC-43 (command x counter impossible), AC-56 (uniform int32 never accepted), AC-38 case 2 (unreachable, contradicted rule 17), AC-16 k = 1 (Tirada is TiradaNoEsperada), AC-42/AC-48 (assumed both players attack in one resolution).
- [RECOMMENDED] Text: row 5 visibility for TurnTimeout, TimeoutObsoleto in UI Requirements, timer mode not in state (done).
- [RECOMMENDED] QA: preconditions (AC-2/9/20/37/38), PROVISIONAL marks, missing cases in AC-44/47/50/57, AC-58 helper (done); AC-29 generated, AC-45 mutant, AC-54 insertion order, AC-59 vs provisional goldens (Open Questions).
- [RECOMMENDED] Engine: AC-6 Mono/IL2CPP leg, WebGL exceptionSupport, destination after Rechazado, single-byte hash injectivity, JIT warm-up, embedded web golden, custom analyzer (ADR, Open Questions).
- [RECOMMENDED] Network: EstadoViejo dead end, TurnTimeout/CerrarPartida without counter under reordering, server-assigned unique match id, identical Resincronizar (Open Questions, #28).
- [RECOMMENDED] Game: CierrePorTiempo mid-turn, passive-play tiebreak, invisible auto-recruit, friendly fire on own King without an AC (Open Questions).
- [RECOMMENDED] Network/Game: Resincronizar with a foreign context (CD downgraded from blocking; fixed anyway in rule 14 step 2).
Note: the GDD is deliberately not stamped. Its reviewed version was an uncommitted working copy that the same-session revisions overwrote, so no hash of it exists; leaving it unstamped makes the next check run a full re-review. Size: 82,232 bytes before, about 92,300 after (+10 KB; the net-zero rule of earlier passes was not met, mostly the domain table and new ACs). Disagreements: network-programmer found no incorrect AC (CD sided with qa-lead); unity-specialist rated AC-6 without IL2CPP blocking (CD: ADR); game-designer rated the Resincronizar context blocking (CD: recommended). The creative director keeps the bar for the seventh pass: if the domain table and these fixes hold and no blocker outside this family appears, the verdict should be APPROVED; reviewers should start with a mechanical check that every canonical-form field has a table row and every AC has a reachable precondition.
Reviewed-Content-Hash: design/registry/entities.yaml 09f3afc9fa59c98361ab76eaa45980a115eae095
Reviewed-Content-Hash: .claude/skills/design-review/SKILL.md 4dcc8b81162613c757144532f1b72708ab3ba27f
Reviewed-Content-Hash: design/gdd/game-concept.md 590e85748059dfb71f99b94cd04d2b367360de33

## Review — 2026-11-19 — Verdict: NEEDS REVISION
Review mode: full · Workflow: full
Scope signal: M
Specialists: game-designer, systems-designer, qa-lead, unity-specialist, network-programmer, creative-director (senior)
Blocking items: 8 | Recommended: 5 groups
Summary: The domain table held almost entirely, but two closure cells failed (closing a match during the castle draw left a Terminada state with an empty castle pair that the table rejects; row 15 and AC-43 gave two answers for a match-ending Resolver at C_MAX - 1), plus four incorrect ACs. The creative director called it a method problem: the GDD asserted closure without proving it per transition. Revised in the same session with a closure matrix and a generative closure AC, and the user's decisions: an empty pair is valid in a Terminada state, LimitePartida rejects everything at C_MAX - 1 except Rendirse/CerrarPartida, the saved stream ends with its 8-byte hash, and the state stores the turn's base table.
Prior verdict resolved: Yes (all sixth-pass blockers stayed closed; new blockers found)
Findings:
- [BLOCKING] Core Rules 3/11: Rendirse/CerrarPartida during the castle draw produced a Terminada state with an empty pair outside the domain table (now valid; equivalence pair-empty <=> 0 pieces/walls; AC-50 round trip).
- [BLOCKING] Core Rules 4 row 15/Edge Cases/AC-43: LimitePartida criterion contradicted itself at C_MAX - 1 (now a closed list).
- [BLOCKING] Core Rules 1/11, AC-21: saved stream did not state it carries the hash; a flipped bit in match id or mode loaded as another valid state (now 8-byte trailing hash checked by Cargar).
- [BLOCKING] Acceptance Criteria: AC-49 used an invalid state (1 wall, counter C_MAX while En curso).
- [BLOCKING] Acceptance Criteria/Core Rules 5: AC-12b case 2 unreachable (bonuses are 0-10); negative-bonus text removed.
- [BLOCKING] Acceptance Criteria: AC-56 unachievable quotas and circular oracle (now robustness fuzz with a test-side domain oracle; quotas in new AC-62).
- [BLOCKING] Core Rules 11: numeroTurno bound broken by turn-1 cascades and no start value; Modifier face row unverifiable (now starts at 1, <= contadorComandos + 2; new turn base-table field).
- [BLOCKING] Core Rules 5/11, Open Questions: explicit overflow detection without exceptions; replay log stores the emitter; owners named for prediction/pipelining/relay (#28) and event presentation (#16).
- [RECOMMENDED] QA: AC-29 full pair matrix, AC-44 test double and missing orders, fixture reachability tag, AC-11/12a/17/42/43/50/54/58/59 details (open).
- [RECOMMENDED] Systems: rule 5 bound and Grito/Arremetida L = 1 text (done); call caps without numbers, undo stack 256 vs legal play, cover expiry (open).
- [RECOMMENDED] Network (to #28): pipelining vs contadorEsperado, relay with emitter, counter padding DoS, LimitePartida on authority commands (edge case done), Resincronizar dead ends, total byte cap, roll commitment as an online requirement.
- [RECOMMENDED] Engine (to ADR): WebGL/IL2CPP overflow, Mono/IL2CPP legs of AC-6, dual-compilation options, reference platform for 0-GC and budgets, Math.Abs(int.MinValue).
- [RECOMMENDED] Game (to #28/#16/#3/#12): predicted events shown then rolled back, ComandoViejo cascades, CierrePorTiempo vs preview MUERE, DobleRey favours the resolver, auto-recruit not previewed, "El tablero no hace trampa" overclaims.
Note: the GDD is deliberately not stamped. Its reviewed version was an uncommitted working copy that the same-session revisions overwrote, so the next check runs a full re-review. Size: 92,256 bytes before, 98,468 after (+6 KB; the creative director's near-zero target was not met, mostly the closure matrix and AC-62). Disagreements: game-designer rated its three points blocking (CD: name owners in Open Questions); network-programmer rated pipelining and relay blocking (CD: #28); unity-specialist rated four engine points blocking (CD: ADR, plus one line in rule 5); qa-lead rated AC-29/AC-44 blocking (CD: recommended). Bar for the eighth pass: start with the closure matrix; only (a) an accepted sequence reaching a state outside the table or two outcomes for one command, or (b) an AC with an unreachable precondition or unverifiable oracle, blocks; if both are clean, APPROVED.
Reviewed-Content-Hash: design/registry/entities.yaml 09f3afc9fa59c98361ab76eaa45980a115eae095
Reviewed-Content-Hash: .claude/skills/design-review/SKILL.md 4dcc8b81162613c757144532f1b72708ab3ba27f
Reviewed-Content-Hash: design/gdd/game-concept.md 590e85748059dfb71f99b94cd04d2b367360de33
Reviewed-Content-Hash: design/game-brief.md 1f759b960d5955f6eb96f0f0f6f8c3802b1e2c0c
