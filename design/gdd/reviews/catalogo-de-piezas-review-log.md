# Review log — Catálogo de piezas (#2)

## Review — 2026-11-27 — Verdict: NEEDS REVISION
Review mode: full · Workflow: full
Scope signal: M
Specialists: game-designer, systems-designer, qa-lead, creative-director (senior)
Blocking items: 9 | Recommended: 9
Summary: La estructura y los datos están bien. Hay 8/8 secciones, los valores coinciden con el prototipo v5 (`RolchessV5.cs` l.46-52) y con el registro, y tres revisores verificaron la matriz 7×7. Fallaban los chequeos I y los AC que los prueban: no veían el Grito (W-12) ni la explosión del demoledor, y varios AC no se podían ejecutar (AC-18, AC-22, AC-32). En la misma sesión se revisaron los 9 bloqueantes con las decisiones del jugador: D1, explosión en 30 con el chequeo I4; D2, I1 e I2 evaluados con el bono del Grito; D3, Rey sin cambios, se decide con DW-1; D4, manda el GDD sobre el prototipo. La aprobación la decide un re-review, después de una nueva pasada de CD-GDD-ALIGN sobre I4.
Prior verdict resolved: First review
Cross-review 2026-11-26 resuelto acá: W-12 (D2), W-16 (ya estaba resuelto, verificado), W-21 (la parte del Catálogo: #16, #17 y #19 Dura), A-13, B-11. Sin cambios en el Catálogo: DW-1 (va a #8; D3 queda en Open Questions), DW-6 (#5), W-18 (#9 debe citar el piso 0 de `danoArea`).
Findings:
- [BLOCKING] Formulas: I3 no veía la explosión del demoledor (30 a las 8 vecinas, `danoArea` 0). Se agregó I4: excepción para varias muertes y regla dura para el Rey. La justificación del contrajuego se reescribió para la v5.
- [BLOCKING] Tuning Knobs / Formulas: W-12, los techos no contaban el Grito. Se agregó `piezasParaMatarEf`. Techos de ataque 8 y 14; pisos de escudo + vida 13 y 11.
- [BLOCKING] Core Rules 6: F8 no cubría el +2 de movimiento y `techo_movimiento` era solo un oráculo de test. Se agregó F9.
- [BLOCKING] Core Rules 6 / AC: `ConfigInvalida` no traía el código de la regla. Ahora lo trae (pedido a la Librería). Se anotaron los techos de F2 y F5 que no se alcanzan.
- [BLOCKING] AC-18: no se podía armar en la fila del Rey y le faltaban precondiciones. Reescrito.
- [BLOCKING] AC-32: era ambiguo y una de sus lecturas rompía F6. Reescrito.
- [BLOCKING] AC-22: no se podía ejecutar. Pasó a una lista cerrada de casos.
- [BLOCKING] Player Fantasy: decía que los bonos eran "iguales para los dos". Ahora dice "la misma regla, no el mismo acceso".
- [BLOCKING] Header: el estado no coincidía con el índice. Encabezado en In Review; el índice queda pendiente para la sesión de cierre.
- [RECOMMENDED] AC: se agregaron AC-34 (F8), AC-35 (F9), AC-36 (I4), AC-37 (≤99 y ≤100) y AC-25b.
- [RECOMMENDED] AC: cambios de qa en AC-4, 12, 13, 15, 23, 24, 28 y 30, y `EvaluarI` y `VerificarRegistro` afirmables.
- [RECOMMENDED] Tuning Knobs: "sin techo duro" pasó a techo de presentación (#15, #16).
- [RECOMMENDED] Core Rules 6: máximos reales con el despliegue (49, 46 y 16). El revisor había dado 52, y se corrigió.
- [RECOMMENDED] Formulas: I3 no suma varias catapultas (el combo es buscado).
- [RECOMMENDED] Dependencies: B-11 (rango del bono de la Librería, valor de #11); W-21 (#16, #17; #19 Dura).
- [RECOMMENDED] Cross-References: A-13 (aristas del índice).
- [RECOMMENDED] Open Questions: D3 (Rey con movimiento 4, junto con DW-1), divergencia del prototipo v5, daño por tipo en #19.
- [RECOMMENDED] Core Rules 5: nota de la divergencia del prototipo v5 (Grito sobre el área y la explosión); manda el GDD.
Specialist disagreements: I1 con o sin bono (game-designer: solo I2; systems-designer, qa-lead y CD: los dos). Lo decidió el jugador (D2: los dos). La divergencia del prototipo (systems-designer: bloqueante; CD: nota en el Catálogo, bloquea la compuerta de duración). Lo decidió el jugador (D4: manda el GDD).
Note: el hash del GDD es el de la versión revisada (HEAD 23c5a83), tomado antes de las revisiones de esta sesión. Así el próximo check lee CHANGED y corre un re-review completo. Los demás hashes son de los archivos tal como se leyeron.
Reviewed-Content-Hash: design/gdd/catalogo-de-piezas.md 7dccd7606bda963cd3b35bff4f3a06a4b967a881
Reviewed-Content-Hash: design/registry/entities.yaml a1def8387514578df4d215eec6397bb98386ba62
Reviewed-Content-Hash: .claude/skills/design-review/SKILL.md 4dcc8b81162613c757144532f1b72708ab3ba27f
Reviewed-Content-Hash: design/gdd/game-concept.md 43e62d80536c71a39bae754ce48356caf6ee6beb
Reviewed-Content-Hash: design/gdd/gdd-cross-review-2026-11-26.md 4e2bcb868717e3e9c62d8959fed5d77a54b22266
Reviewed-Content-Hash: design/gdd/modificador.md 451e50bd6f8fe9f2aa890968b9cdd59f33bde888
Reviewed-Content-Hash: design/gdd/maquina-de-turno.md 0af0ecd29538b4dbcfba23734b3dc6fb6bf01e38
Reviewed-Content-Hash: design/gdd/vista-previa-de-ataques.md 0e7008f9b267943f86ab2c7be6ab26f1c02c3397
Reviewed-Content-Hash: design/gdd/resolucion-simultanea-del-dano.md 95048c972e2d02528ea282c6363eec956f586004
Reviewed-Content-Hash: design/gdd/tablero-y-despliegue-inicial.md 7b0dfbacb62401d09968e9167d03214f846cddad
Reviewed-Content-Hash: design/gdd/interfaz-del-modificador.md 661f75e3fe5c891e0a0ad5abe008327470826479

## Review — 2026-11-27 — Verdict: APPROVED
Review mode: full · Workflow: full
Scope signal: M
Specialists: game-designer, systems-designer, qa-lead, creative-director (senior, CD-GDD-ALIGN y verificación puntual)
Blocking items: 5 (resueltos en la sesión) | Recommended: 12 (aplicados)
Summary: Re-review. Los 9 bloqueantes de la revisión anterior quedaron resueltos (aritmética verificada por tres revisores) y los valores siguen iguales al prototipo v5 (`RolchessV5.cs` l.46-52) y al registro. El re-review encontró 5 bloqueantes nuevos, todos de texto o de contrato, sin cambios de cifras. Se corrigieron en la misma sesión con decisiones del jugador: D5, F9 se valida en CrearPartida; D6, se devuelve la primera regla F que falla, en orden F1 → F8; D7, I4 sin cota, se mide. El veredicto inicial fue NEEDS REVISION y pasó a APPROVED tras la verificación puntual. CD-GDD-ALIGN dio CONCERNS y luego APPROVE: I4 es compatible con el pilar 2 y el contrajuego ahora es posicional.
Prior verdict resolved: Yes
Findings:
- [BLOCKING] Core Rules 6 / States / Edge / AC-24 / AC-35: F9 se validaba en `PrepararContexto(catálogo)`, que no recibe la config. Pasa a CrearPartida (D5).
- [BLOCKING] Core Rules 6 / AC-5, 6, 9, 10, 35: no había un orden de los códigos F. Ahora se devuelve la primera que falla, F1 → F8 (D6); los casos violan una sola regla; AC-38 nuevo.
- [BLOCKING] AC-37: no era un criterio de este GDD y no discriminaba nada. Pasó a Config/Data, como inclusión de #15 AC-4 y #16 AC-11 en el job de CI.
- [BLOCKING] Formulas I1: el contrajuego del demoledor era un conteo de golpes. Reescrito como separación (principal) e intercepción (radio 7, o 9 con Grito, contra alcance de respuesta), y se dice dónde no hay respuesta (a 9 o más, o con cobertura).
- [BLOCKING] Formulas I4 / Player Fantasy / encabezado: I4 reemplaza la aprobación del 2026-11-20, acepta hasta 8 muertes sin cota (D7), vale con cobertura y sin ella y se mide en #19. Salvedad en l.33 y encabezado CD actualizado.
- [RECOMMENDED] Dependencies / AC-39: techo cruzado `bono_grito_ataque ≤ 2` hacia #11.
- [RECOMMENDED] Tuning Knobs: el piso de escudo + vida es el ataque efectivo de la catapulta + 1 (perillas que interactúan); "fallaría con 11 o más".
- [RECOMMENDED] AC: AC-36 (`Coberturas()` vacío, esquinas no vecinas, caso con cobertura); lista informativa, `ValidarF2` y entradas de `EvaluarI` definidas en las Convenciones; AC-18 con bono; AC-4, 12, 21, 22 y 30 precisados; AC-19 con el oráculo de los tramos.
- [RECOMMENDED] Core Rules 5 / Open Questions: valores no calibrados bajo la regla del GDD; l.406 actualizada (ataque 3 con Grito, explosión más arquero); OQ nueva sobre la cota de muertes por explosión.
Specialist disagreements: si el escudero que cubre al demoledor lo vuelve intocable (game-designer: sí; CD: verificar en #8). Se verificó en #8, reglas 1 y 6: los golpes van al escudero. Gravedad del techo de `bono_grito_ataque` (systems-designer: problema serio; CD: recomendado). Quedó como recomendado, con AC-39.
Note: el hash del GDD es el de la versión aprobada, con todas las revisiones de esta sesión. libreria-de-reglas.md y tablero-y-despliegue-inicial.md tienen cambios sin commit de sus propias re-revisiones paralelas: se leyeron tal como estaban el 2026-11-27.
Reviewed-Content-Hash: design/gdd/catalogo-de-piezas.md 299af94496afef9ccf01ba6718cd0514993fa5bb
Reviewed-Content-Hash: design/registry/entities.yaml a1def8387514578df4d215eec6397bb98386ba62
Reviewed-Content-Hash: .claude/skills/design-review/SKILL.md 4dcc8b81162613c757144532f1b72708ab3ba27f
Reviewed-Content-Hash: design/gdd/game-concept.md 43e62d80536c71a39bae754ce48356caf6ee6beb
Reviewed-Content-Hash: design/gdd/libreria-de-reglas.md b829f31f36e56a6d603bb35f3c42c5ad46f98c97
Reviewed-Content-Hash: design/gdd/cobertura-del-escudero.md ccc66ea4cf0e8419870a77b09dcdfcbe82a209f0
Reviewed-Content-Hash: design/gdd/resolucion-simultanea-del-dano.md 95048c972e2d02528ea282c6363eec956f586004
Reviewed-Content-Hash: design/gdd/tablero-y-despliegue-inicial.md 6f268f045f27e1b8aa5d75012c30838a49ebee89
Reviewed-Content-Hash: design/gdd/modificador.md 451e50bd6f8fe9f2aa890968b9cdd59f33bde888
