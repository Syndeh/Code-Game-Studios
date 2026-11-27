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

## Review — 2026-11-27 — Verdict: NEEDS REVISION
Review mode: full · Workflow: full
Scope signal: M
Specialists: systems-designer, qa-lead, game-designer, network-programmer, unity-specialist, creative-director (senior)
Blocking items: 6 | Recommended: 4 groups
Summary: Eighth pass after the castle-draw removal and the v5 contract (RelojAgotado, Arremetida block, vidaQuitada, vida_tramo). Under the seventh-pass bar, one closure hole (a recruit face with base p = 0 that got Q from reparto_sin_lugar fails the domain row once recruiting or moving makes it Agotada/SinLugar), one command with two outcomes (a delayed RelojAgotado accepted in turn N+2 of the same player while the text said rejected) and four ACs with unreachable preconditions or wrong oracles (AC-38 x2, AC-62, the 12-vs-13 command count in AC-1/AC-56). The creative director called all of them local text fixes; revised in this session with the user's decisions. Also resolves cross-review W-14, W-15, W-19 (in #1), D-16, F-09, A-10, B-07, B-08.
Prior verdict resolved: Yes (seventh-pass blockers stayed closed; new blockers came from the v5 changes)
Findings:
- [BLOCKING] Core Rules 11 (domain row, face of the turn): now "una de reclutar"; Mover/Reclutar don't touch the face (closure matrix).
- [BLOCKING] Core Rules 3/4 row 9, Edge Cases, AC-50: CerrarPartida(motivo, ganador, turno); RelojAgotado carries numeroTurno, otherwise CierreInvalido (user approved the contract change).
- [BLOCKING] AC-38 (wrong oracle with shared Común calls; ReclutaPerdido unreachable, now a constructor defense), AC-62 (unreachable turn-1 cascade, now turn-2 Distracción), AC-1/AC-56 (13 commands).
- [RECOMMENDED] W-15: the Aceptado that reaches contadorComandos = C_MAX − 1 ends the match in Tablas (rules 4 row 15, 11, 16; AC-43, AC-62); W-14: Tablas = technical no-winner close, no draw offer in MVP (user); OQ on LimitePartida closed.
- [RECOMMENDED] RendirseDisponible query (rule 10, D-16); PartidaTerminada fields (rule 8, F-09); fantasy "la vista previa vale si resolvés a tiempo" (user kept discarding attacks on RelojAgotado); TablaEfectiva signature; limite_normal in rule 6; 123.220 per receptor; vidaQuitada domain-only; AC-40/44/46/53/57 extended; stale PROVISIONAL tags and draw remnants removed; dependents #12/#14 added.
- [RECOMMENDED] Engine (to ADR, appended to the OQ): two csproj, asmdef/csproj file-set equality + noEngineReferences, caller-owned preview buffer, AC-33b method, web legs of AC-24/25, little-endian trailing hash, same overflow option in csc.rsp and csproj.
Note: the GDD is not stamped (its reviewed version was overwritten in the same session); the next check runs a focused verification: domain row of the face, RelojAgotado with turno, AC-1/38/56/62, the W-15 AC, size ≤ 100 KB. Size: 101,216 bytes before, 101,280 after (net +64; the 100 KB ceiling was not met). Disagreements: game-designer rated W-14/W-15 blocking (CD: recommended, they are liveness/meaning, not safety); network/systems/game rated the delayed RelojAgotado recommended (CD: blocking, two outcomes). Pending for neighbors and the registry are listed in the session report.
Reviewed-Content-Hash: design/registry/entities.yaml a1def8387514578df4d215eec6397bb98386ba62
Reviewed-Content-Hash: .claude/skills/design-review/SKILL.md 4dcc8b81162613c757144532f1b72708ab3ba27f
Reviewed-Content-Hash: design/gdd/game-concept.md 43e62d80536c71a39bae754ce48356caf6ee6beb
Reviewed-Content-Hash: design/game-brief.md 1f759b960d5955f6eb96f0f0f6f8c3802b1e2c0c
Reviewed-Content-Hash: design/gdd/gdd-cross-review-2026-11-26.md 4e2bcb868717e3e9c62d8959fed5d77a54b22266
Reviewed-Content-Hash: design/gdd/reclutamiento-y-limites.md e627256e98aea8addef670dfafbbb1add0ace54a
Reviewed-Content-Hash: design/gdd/modificador.md 451e50bd6f8fe9f2aa890968b9cdd59f33bde888
Reviewed-Content-Hash: design/gdd/maquina-de-turno.md 0af0ecd29538b4dbcfba23734b3dc6fb6bf01e38
Reviewed-Content-Hash: design/gdd/timer-de-turno.md a913fc5a6cf779fce954ffa0522e104365587347
Reviewed-Content-Hash: design/gdd/catalogo-de-piezas.md 06dcbb6869c82ccc59b08d260d30c3cb1091667b
Reviewed-Content-Hash: design/gdd/tablero-y-despliegue-inicial.md 7b0dfbacb62401d09968e9167d03214f846cddad
Reviewed-Content-Hash: design/gdd/resolucion-simultanea-del-dano.md 95048c972e2d02528ea282c6363eec956f586004
Reviewed-Content-Hash: design/gdd/ficha-de-pieza-y-hud.md 9d365c37f9a327151b248c7d9e1b489103e657fa

## Review — 2026-11-27 — Verdict: NEEDS REVISION
Review mode: full · Workflow: full
Scope signal: M
Specialists: systems-designer, qa-lead, game-designer, network-programmer, creative-director (senior)
Blocking items: 4 | Recommended: 6 groups
Summary: Ninth pass, the focused check of the eighth-pass fixes. Five of the six eighth-pass blockers stayed closed (domain row of the face, CerrarPartida with turno, AC-38 oracle, AC-1/AC-56 at 13 commands); AC-62's replacement cascade (turn-2 Distracción) is still unreachable, because red's four adjacent squires can always cover. QA found three more AC defects of the same kind (AC-42 unreachable IDs, AC-56 and AC-21 wrong oracles). The creative director called all of them local text fixes and downgraded the closure-matrix cascade gap to recommended, asking to fix it in this revision; revised in this session.
Prior verdict resolved: Partly (5 of 6; AC-62 reopened and fixed again)
Findings:
- [BLOCKING] AC-62: turn-2 Distracción without targets unreachable (squires cover; Modificador r13); now a constructor-built Tirada that rolls Distracción for a player without targets or a covering squire.
- [BLOCKING] AC-42: catapult with ID 3 unreachable (deployment IDs 1–26); now A = deployed soldier ID 3, B = recruited catapult ID > 26, A before B.
- [BLOCKING] AC-56: load/resync oracle "EstadoInvalido exactly out of domain" wrong; now the first code of the rule 1 / rule 14 order that each mutation triggers.
- [BLOCKING] AC-21: the version/Catalog-hash domain row gives VersionNoSoportada, now stated.
- [RECOMMENDED] Applied: closure matrix row "Cascada al empezar una fase", Tirada row changes fase/etapa, "Todo Aceptado" changes motivo/ganador/tirada with automatic Tablas; rule 16: automatic Tablas evaluated after the full cascade, PartidaTerminada last, the command's own end motive wins; AC-50 isolates "not while waiting" and uses three arguments; AC-38 defense gets its WHEN; AC-43 includes the atacar TurnTimeout; Edge Case for the RelojAgotado vs Resolver/TerminarMovimiento race; three-argument CerrarPartida in Edge Cases and AC-29; "two formulas" wording.
- [RECOMMENDED] Not applied (open): TablaEfectiva combined expression and input contract in Formulas; RendirseDisponible variable table and AC (13 vs 26 true, 14 vs 26 false), live vs turn-start, whether the AI respects it; AC-57 name the opening command; row 15 next-ID cases in AC-43; AC-43 one constructor state per phase; AC-44 two-version fixture; invariant "En curso ⇒ (fase tirada ⇔ tirada pendiente)".
- [RECOMMENDED] Not applied (game-designer, CD endorsed): move the RelojAgotado caveat into the first fantasy bullet; UI requirement warning that assigned attacks won't resolve when the match clock binds, end screen listing them; OQ on gated surrender pushing rage-quits to Abandono; owner of the Tablas result screen.
- [NIT] numeroTurno ≤ contadorComandos + 1 would hold (the +2 is slack); C_MAX still PROVISIONAL; Terminada at C_MAX is in-domain but unreachable (AC-49 uses it on purpose).
Note: the GDD is not stamped (revised in the same session after review). Size: 101,280 bytes before, 102,696 after (the 100 KB ceiling is still not met; the eighth-pass suggestion stands: move the closure matrix and AC-62 to an annex if it grows again). Next: tenth pass in a separate session, a short check of AC-21/42/56/62, the cascade row and rule 16. Disagreement: systems-designer rated the matrix cascade gap borderline blocking; CD recommended (no invalid state, no two outcomes). Propagation (neighbors, not this GDD): timer-de-turno.md (l.69, 192, 236, AC-10, AC-11) and maquina-de-turno.md (l.288, 626) still send two-argument CerrarPartida(RelojAgotado, rival), which would now be CierreInvalido and the match would never end; Timer l.120, 242, 444 and AC-13 still mark Tablas PROVISIONAL; Timer AC-13 expects ComandoViejo for a TurnTimeout (row 10 is player-only).
Reviewed-Content-Hash: design/registry/entities.yaml a1def8387514578df4d215eec6397bb98386ba62
Reviewed-Content-Hash: .claude/skills/design-review/SKILL.md 4dcc8b81162613c757144532f1b72708ab3ba27f
Reviewed-Content-Hash: design/gdd/game-concept.md 43e62d80536c71a39bae754ce48356caf6ee6beb
Reviewed-Content-Hash: design/game-brief.md 1f759b960d5955f6eb96f0f0f6f8c3802b1e2c0c
Reviewed-Content-Hash: design/gdd/tablero-y-despliegue-inicial.md 6f268f045f27e1b8aa5d75012c30838a49ebee89
Reviewed-Content-Hash: design/gdd/modificador.md 451e50bd6f8fe9f2aa890968b9cdd59f33bde888
Reviewed-Content-Hash: design/gdd/cobertura-del-escudero.md ccc66ea4cf0e8419870a77b09dcdfcbe82a209f0
Reviewed-Content-Hash: design/gdd/reclutamiento-y-limites.md e627256e98aea8addef670dfafbbb1add0ace54a
Reviewed-Content-Hash: design/gdd/catalogo-de-piezas.md 06dcbb6869c82ccc59b08d260d30c3cb1091667b
Reviewed-Content-Hash: design/gdd/timer-de-turno.md a913fc5a6cf779fce954ffa0522e104365587347
Reviewed-Content-Hash: design/gdd/maquina-de-turno.md 0af0ecd29538b4dbcfba23734b3dc6fb6bf01e38

## Review — 2026-11-27 — Verdict: APPROVED
Review mode: full · Workflow: full
Scope signal: M
Specialists: qa-lead, systems-designer, game-designer, creative-director (senior); qa-lead again for the short re-check
Blocking items: 2 (fixed in session) | Recommended: 9
Summary: Tenth pass, focused check of the ninth-pass fixes. AC-62, AC-42 (IDs) and AC-21 stayed closed, as did every eighth-pass item. Two blockers remained. First, AC-56's resync leg could not be computed: with no `confirmado` named, a local base gives SoloOnline in every case. Second, a contract clash with the approved Catálogo: `ConfigInvalida` had no rule code, F9 was not placed in CrearPartida's order, and `EvaluarI`/`ValidarF2` were missing. The creative director called both local text fixes and set the re-check scope: if those hold, APPROVED. The fixes were applied with the user's order decision. The re-check found two more problems (the Tablero's T4 → T5 → T6 order, and F9 summing the bonuses before their range check, which could overflow), both fixed. A second re-check came back HOLDS on every item.
Prior verdict resolved: Yes (AC-62, AC-42 and AC-21 closed; AC-56 closed in this revision)
Findings:
- [BLOCKING] AC-56 / AC conventions: resync of a mutated state uses `confirmado` = the unmutated state with the base fixture in online mode, and `huellaDeclarada` = Huella(mutated); mutations that change nothing are excluded; SoloOnline and PartidaTerminada are excluded from the oracle (AC-44 covers them). The same applies to AC-21.
- [BLOCKING] Rule 2 / rule 4 / rule 10 / AC-3 (user decision, CD order). CrearPartida checks in this order: tables → PrepararContexto F1→F8 (first failure) → Grito bonuses (0 to 10, not both 0) → F9 → T4 → T5 → limite_normal, tablaReclutas with recruit faces, pila con bono → T6. `ConfigInvalida` carries F1–F9, T4–T6 or a field code. CrearPartida accepts an already-prepared context (it skips F1–F8 but still checks F9). `EvaluarI(catalogo, config)` and `ValidarF2(fila)` are now listed. ProbabilidadesInvalidas comes only from CrearPartida.
- [RECOMMENDED, applied] Closure matrix cascade row: the incoming player's coverages expire (`Caducada`, #8 rule 9); `ReclutaPerdido` is noted as an event, not a field. CrearPartida row: next ID 27, first move phase non-empty. AC-62's Distracción case expires the rival's coverages. Constructor states satisfy EsValido unless the AC says otherwise. AC-42: A has 3 life, area 6 − 3, E takes 8 − 3 = 5. AC-21 has one case per field. Rule 11 points to the domain table instead of repeating the field list. The RelojAgotado caveat moved into the headline fantasy.
- [RECOMMENDED, deferred to Open Questions, presentation, CD]: warning that assigned attacks are not resolved under RelojAgotado, plus the end-screen list; showing `FaseSaltada` and a turn emptied by a cascade; the owner of the Tablas result screen; gated Rendirse pushing players to Abandono.
- [RECOMMENDED, outside this GDD] Register C_MAX = 1.000.000 (PROVISIONAL) in the registry; tirada_rango referenced_by lacks modificador.md.
- [NIT] "Lo ya movido queda" (rule 3) reads as RelojAgotado only.
Note: the GDD was revised in the same session; the stamp below is the approved version. Size is 102,696 bytes before and 103,202 after (+506 net, partly offset by removing duplicated field lists and edge cases). CD condition: if the next pass is still over 100 KB, splitting out an annex becomes mandatory. Disagreement: systems-designer rated the cascade-coverages gap blocking; the CD rated it recommended (no invalid state, since #8/#3 already expire them). The T4–T6 codes come from the Tablero's uncommitted revision, which another session is re-reviewing; if they change, this rule 2 must follow. The first specialist spawns failed on a usage limit and were rerun.
Reviewed-Content-Hash: design/gdd/libreria-de-reglas.md 5ac1d08ff674134ee106d55ad965c1784175b26d
Reviewed-Content-Hash: design/registry/entities.yaml a1def8387514578df4d215eec6397bb98386ba62
Reviewed-Content-Hash: .claude/skills/design-review/SKILL.md 4dcc8b81162613c757144532f1b72708ab3ba27f
Reviewed-Content-Hash: design/gdd/catalogo-de-piezas.md 299af94496afef9ccf01ba6718cd0514993fa5bb
Reviewed-Content-Hash: design/gdd/tablero-y-despliegue-inicial.md 04cb6fe46f22a9330e4d276b8ae676404554dfbe
Reviewed-Content-Hash: design/gdd/cobertura-del-escudero.md ccc66ea4cf0e8419870a77b09dcdfcbe82a209f0
Reviewed-Content-Hash: design/gdd/resolucion-simultanea-del-dano.md 95048c972e2d02528ea282c6363eec956f586004
