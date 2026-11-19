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
