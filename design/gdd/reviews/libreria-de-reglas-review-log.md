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
