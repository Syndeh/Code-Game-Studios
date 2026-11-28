# Review log — Castillo y muros (#5)

## Review — 2026-11-27 — Verdict: NEEDS REVISION
Review mode: full · Workflow: full
Scope signal: L
Specialists: game-designer, systems-designer, level-designer, qa-lead, ux-designer, gameplay-programmer; creative-director (senior)
Blocking items: 16 | Recommended: ~35
Summary: Estructura completa (8/8) y casi todas las cifras cuadran con el registro y los GDD Approved. Dos decisiones de pilar abiertas (DW-6, el demoledor corta solo; DW-2, el Rey dentro del castillo no se puede cubrir) y unos 14 bloqueantes de texto, rangos y AC. CD-GDD-ALIGN: CONCERNS (pilares 2 y 3). Decisiones del jugador: mantener `vida_tramo` 30 y medir (DW-6); reescribir F4 y la fantasía, sin cobertura dentro del castillo (DW-2); medir el corte propio por fuego amigo. Correcciones aplicadas en la misma sesión; re-review a continuación.
Prior verdict resolved: First review
Findings:
- [BLOCKING] Formulas F3/Player Fantasy: el demoledor rompe 3 tramos solo y corta el reclutamiento (DW-6, pilar 2).
- [BLOCKING] Formulas F4/Player Fantasy: el castillo no protege al Rey contra la distancia y nadie lo cubre dentro (DW-2).
- [BLOCKING] Formulas F4: dice «cota inferior» y es superior (demoledor: 2 contra 1 real); l.250 «60 de vida».
- [BLOCKING] Formulas F1/F5/F6, AC-18: `bruto` 0–24.999.999 contra 0–61.610 de #9 F1; escudo efectivo con dos rangos.
- [BLOCKING] Core Rules 14/Tuning/AC-22/AC-24: constantes del contrato tratadas como perillas.
- [BLOCKING] Visual/UI: el aviso de corte «en 2» no salta con una explosión 0→3 (W-17 análogo); sin AC.
- [BLOCKING] Player Fantasy/Visual/UI: contador «(0, 1, 2)», «0, 1, 2 o más» y «1 de 2»; dos dueños con #15; hover que #15 no tiene.
- [BLOCKING] UI: el defensor no puede leer la vida de un tramo con arquero encima.
- [BLOCKING] Core Rules 1/Edge Cases: falta el invariante de ocupación del tramo; no cita `EstadoInvalido` ni `ConfigInvalida(T5)` del Tablero; Edge y OQ de `vida_tramo` en registros vencidas.
- [BLOCKING] Core Rules 3: no dice si un arquero atraviesa un tramo con otro arquero (pedido de #6).
- [BLOCKING] AC-27: «huella igual» contradice Librería AC-30; falta el escenario S4.
- [BLOCKING] AC-17: duplica Tablero AC-27 y Librería AC-46 con otro código.
- [BLOCKING] AC-16/regla 13: «(fila, columna)» invierte la notación; orden de #9; tipo Integration.
- [BLOCKING] Acceptance Criteria/Revisión de QA: cobertura «reglas 1–14» falsa (reglas 11, 13, 14; Edge de hueco y de dos castillos cortados; demoledor en esquina).
- [BLOCKING] AC-5/momento ancla: «4 soldados sobre (4,7)» imposible (un tramo recto tiene 3 casillas de afuera en su contorno).
- [BLOCKING] Dependencies/header: «#9 no tiene GDD» vencido; #18 es Dura; falta #17; CD-GDD-ALIGN pendiente.
- [RECOMMENDED] Edge Cases: lado del borde (las esquinas sí tienen acceso), «nadie queda atrapado» falso, esquinas que cortan sin abrir paso; cerrar la OQ de la esquina.
- [RECOMMENDED] Tuning Knobs: piso de `vida_tramo` con un solo criterio y bordes 6/10/12; techo de `BONO_ESCUDO_MURO` 3 (inmunidad contra un arquero suelto con 4); «zona en disputa 10–13» inexistente; `TRAMOS_ROTOS_CORTE` 3 no frena al demoledor; `vida_tramo` 31 por config apaga la excepción sin subir versión.
- [RECOMMENDED] Formulas/Lazos: con 12 piezas o menos lo liberado va casi entero al Grito; incentivo al corte propio.
- [RECOMMENDED] AC: códigos de rechazo, un escenario por AC, sin «10/5 turnos», fixtures armables (AC-26, AC-28, AC-29), AC-13/14 sin depender del azar; AC-1 con 12 sin el Rey.
- [RECOMMENDED] Core Rules: eventos `DanoATramo`/`TramoRoto`, firmas de las consultas, `estado_cara` → Agotada como dueño del 0, pieza enemiga en un hueco es objetivo, atravesar un tramo cuesta 1, regla 6 como arista del grafo.
- [RECOMMENDED] Visual/UI/Game Feel: requisitos de otros dueños copiados (colores, escala, ~2 s, 1 ms p95); valores fijos en vez de `EffectiveStats`; nombre único del estado; marca propia para subir al muro; escombros de rotos contra dañados; art bible §9 l.723.
- [RECOMMENDED] Open Questions: metas medibles; arquero en el muro (30–70%); muertes del Rey en el interior.
- Receipt: el hash del GDD no se estampa en esta entrada porque las correcciones se aplicaron antes de escribir el log; el de la re-review siguiente sí lo estampa.
Reviewed-Content-Hash: design/registry/entities.yaml 07713dcc52725c7d683477e25e8906d5db2f0c3e
Reviewed-Content-Hash: .claude/skills/design-review/SKILL.md 4dcc8b81162613c757144532f1b72708ab3ba27f
Reviewed-Content-Hash: design/gdd/libreria-de-reglas.md 5ac1d08ff674134ee106d55ad965c1784175b26d
Reviewed-Content-Hash: design/gdd/catalogo-de-piezas.md 24c2df9133aaea913698815462cb31d575219d30
Reviewed-Content-Hash: design/gdd/tablero-y-despliegue-inicial.md 9460a55db38e0d1d885bb1e96069d99b1b77e54c
Reviewed-Content-Hash: design/gdd/resolucion-simultanea-del-dano.md 52eb62677e85d84ecae345b930e37d123b9a4673
Reviewed-Content-Hash: design/gdd/movimiento.md 679bca4fed148d8ebd3f599a15a6acbe6ce44bda
Reviewed-Content-Hash: design/gdd/alcance-y-asignacion-de-ataques.md a81635dd4ffeb409a8522a47ed8dd20d7e414b75
Reviewed-Content-Hash: design/gdd/cobertura-del-escudero.md 1398f6027b8caa40188f5f9264d8fd5c25be6b81
Reviewed-Content-Hash: design/gdd/reclutamiento-y-limites.md b0a1c65376fa3da8052670ef3e24d224c2d30ad9
Reviewed-Content-Hash: design/gdd/modificador.md 2dae305bc81b22ff28cd83b7ebbd23b096b98d49
Reviewed-Content-Hash: design/gdd/maquina-de-turno.md fd6370c54fb0fd22a6b53074fa2a83b132ebf942
Reviewed-Content-Hash: design/gdd/ficha-de-pieza-y-hud.md 1090b0e1717f0e164d50e925f173f52c4c6f7054
Reviewed-Content-Hash: design/gdd/vista-previa-de-ataques.md 9f2b8c1e1276a36f2d00c39eeddc5149a7e341c9
Reviewed-Content-Hash: design/gdd/render-animacion-y-feedback-de-combate.md 84f4bc10496ac137ba64adee2a48ca89c5d58fc2
Reviewed-Content-Hash: design/gdd/interfaz-del-modificador.md 283c065061e4d5a48c2d383e0b666dd09b3c1084
Reviewed-Content-Hash: design/gdd/metricas-locales-de-partida.md 4b0beb1c2995defdc1e564cdce82fca557ce2a02
Reviewed-Content-Hash: design/gdd/game-concept.md 43e62d80536c71a39bae754ce48356caf6ee6beb
Reviewed-Content-Hash: design/art/art-bible.md c49b99572be1dffabb48c5974fc7ee54cdb29f28
Reviewed-Content-Hash: design/gdd/gdd-cross-review-2026-11-26.md 4e2bcb868717e3e9c62d8959fed5d77a54b22266

## Review — 2026-11-27 — Verdict: NEEDS REVISION
Review mode: full · Workflow: full
Scope signal: L
Specialists: game-designer, systems-designer, level-designer, qa-lead, ux-designer, gameplay-programmer (agentes nuevos, contexto limpio); creative-director (senior)
Blocking items: 9 | Recommended: ~17
Summary: Re-revisión en la misma sesión. Casi todos los 16 bloqueantes de la primera pasada quedaron resueltos; los nuevos son de fidelidad del texto (cifras falsas, promesas que el juego no cumple, fixtures no armables), sin cambio de reglas ni de las decisiones del jugador. CD-GDD-ALIGN: CONCERNS (accepted), con dos ajustes al header (demoledor hasta 5 desde el interior; defensa a distancia según el tramo y sin cubrir la catapulta). Decisiones del jugador: **dos términos** («Sin reclutamiento» = estado, «Castillo cortado» = causa); quitar la vista anticipada del alcance al subir; piso seguro de `vida_tramo` 21. Correcciones aplicadas en la misma sesión; el jugador pidió la re-revisión siguiente en una sesión nueva.
Prior verdict resolved: Partial (16 bloqueantes previos resueltos salvo DW-2 en el texto, aviso de corte y pedido a #15, cobertura de AC y «encerrados», que pasaron a esta lista)
Findings:
- [BLOCKING] Player Fantasy/F3/F4/Tuning/Edge/OQ: el demoledor rompe hasta 5 tramos desde una esquina del interior (#9, F6), no 3; Edge de `(0, 7)` «ninguna explosión llega» falso; «la palanca es 4 o más» falso.
- [BLOCKING] Player Fantasy/F4: «protegen tus arqueros en el muro» promete de más: solo desde `(4, 7)` cubre todas las casillas de un arquero rival, nunca las de la catapulta, y el rival puede pegar en un turno.
- [BLOCKING] Lazos/Edge/Player Fantasy: el Grito queda en 80% (85% solo con la tabla *Pierde 1/2*).
- [BLOCKING] UI Requirements: nombre del estado contradice a #14, #16 y #17; el cruce del corte se daba por cubierto en #15; el pedido de la vida del tramo con arquero ya existe en #15 r8 (el pedido real es `/vida_tramo`).
- [BLOCKING] AC-32: no verificable con captura; panel «?» es de #17.
- [BLOCKING] Acceptance Criteria: convención de fixtures sin Reyes (AC-15 fallaba, AC-7b/14/14c sin turno); AC-16 no armable; AC-14b 3→16 no armable; AC-3d contradictorio.
- [BLOCKING] Edge Cases: «encerrados» contradecía a Movimiento (los propios se atraviesan; el jinete pasa enemigos).
- [BLOCKING] Revisión de QA: cobertura falsa (regla 6 límite, regla 11 hueco rival, jinete rival frenado).
- [BLOCKING] Open Questions: metas no medibles (sin proporción, tipo de turno, techo, ni denominador).
- [RECOMMENDED] AC-1b duplicaba Tablero AC-31 y citaba AC-24 por error; registro con 4 valores viejos; piso de `vida_tramo` con Grito (21); aserción en compilación no existe en C# (AC-17b con tabla versión → constantes); dueños de `motivo_bloqueo` (#10) y `motivo_cara` (#17); firma `enMuro(estado, pieza)` y regla 10 de la Librería; duplicados con Movimiento/Alcance/Render; casillas faltantes en AC-5b/6/12/13/14/27b; AC-16b sin observador; audio atado al cruce; aviso previo de #16 r14; renglón Debilitado; «armas distintas»; corte propio en la fantasía; criterio del techo del bono de escudo; headers de vecinos.
- Applied fixes: todos los bloqueantes y las recomendadas que son de este GDD; los cambios en otros documentos quedan listados para el cierre de la tanda B (`side-session-notes/.../castillo-pendientes-cierre-tanda-B.md`). Las metas nuevas de las Open Questions (por ejemplo 15–50% de turnos-arquero en el muro, ≤ 25% de muertes del Rey en el interior) son propuestas de esta revisión: la re-revisión siguiente y el jugador pueden ajustarlas.
- Receipt: sin hash del GDD (se corrigió después de revisarlo). `maquina-de-turno.md` cambió entre las dos pasadas (la edita la sesión paralela de la tanda B); `localizacion.md` se suma a los archivos leídos.
Reviewed-Content-Hash: design/registry/entities.yaml 07713dcc52725c7d683477e25e8906d5db2f0c3e
Reviewed-Content-Hash: .claude/skills/design-review/SKILL.md 4dcc8b81162613c757144532f1b72708ab3ba27f
Reviewed-Content-Hash: design/gdd/libreria-de-reglas.md 5ac1d08ff674134ee106d55ad965c1784175b26d
Reviewed-Content-Hash: design/gdd/catalogo-de-piezas.md 24c2df9133aaea913698815462cb31d575219d30
Reviewed-Content-Hash: design/gdd/tablero-y-despliegue-inicial.md 9460a55db38e0d1d885bb1e96069d99b1b77e54c
Reviewed-Content-Hash: design/gdd/resolucion-simultanea-del-dano.md 52eb62677e85d84ecae345b930e37d123b9a4673
Reviewed-Content-Hash: design/gdd/movimiento.md 679bca4fed148d8ebd3f599a15a6acbe6ce44bda
Reviewed-Content-Hash: design/gdd/alcance-y-asignacion-de-ataques.md a81635dd4ffeb409a8522a47ed8dd20d7e414b75
Reviewed-Content-Hash: design/gdd/cobertura-del-escudero.md 1398f6027b8caa40188f5f9264d8fd5c25be6b81
Reviewed-Content-Hash: design/gdd/reclutamiento-y-limites.md b0a1c65376fa3da8052670ef3e24d224c2d30ad9
Reviewed-Content-Hash: design/gdd/modificador.md 2dae305bc81b22ff28cd83b7ebbd23b096b98d49
Reviewed-Content-Hash: design/gdd/maquina-de-turno.md bbe86a444be2634587f8b4797c33f2a14bb5a105
Reviewed-Content-Hash: design/gdd/ficha-de-pieza-y-hud.md 1090b0e1717f0e164d50e925f173f52c4c6f7054
Reviewed-Content-Hash: design/gdd/vista-previa-de-ataques.md 9f2b8c1e1276a36f2d00c39eeddc5149a7e341c9
Reviewed-Content-Hash: design/gdd/render-animacion-y-feedback-de-combate.md 84f4bc10496ac137ba64adee2a48ca89c5d58fc2
Reviewed-Content-Hash: design/gdd/interfaz-del-modificador.md 283c065061e4d5a48c2d383e0b666dd09b3c1084
Reviewed-Content-Hash: design/gdd/metricas-locales-de-partida.md 4b0beb1c2995defdc1e564cdce82fca557ce2a02
Reviewed-Content-Hash: design/gdd/localizacion.md 3e81fd308be0094faddd9b519a8008e1f0b1fb79
Reviewed-Content-Hash: design/gdd/game-concept.md 43e62d80536c71a39bae754ce48356caf6ee6beb
Reviewed-Content-Hash: design/art/art-bible.md c49b99572be1dffabb48c5974fc7ee54cdb29f28
Reviewed-Content-Hash: design/gdd/gdd-cross-review-2026-11-26.md 4e2bcb868717e3e9c62d8959fed5d77a54b22266

## Review — 2026-11-27 — Verdict: NEEDS REVISION
Review mode: full · Workflow: full
Scope signal: L
Specialists: game-designer, systems-designer, level-designer, qa-lead, ux-designer, gameplay-programmer (sesión nueva, contexto limpio); creative-director (senior)
Blocking items: 5 | Recommended: ~25
Summary: Tercera pasada, en una sesión nueva. Los bloqueantes de las dos pasadas anteriores están resueltos y verificados (demoledor 3/2/5 y AC-30, Grito 80/85%, bruto 0–61.610, piso 21, techo 3, Reyes en los fixtures, orden de AC-16, «encerrados», aviso por cruce, dos términos), salvo dos que quedaron resueltos a medias: la promesa de «qué protege el muro» y la cobertura de QA. Sin contradicciones nuevas con el registro ni con #1, #2, #4, #9, #15, #16 o #18 (solo las 4 discrepancias del registro ya anotadas). Todo lo que falla es de texto y de AC; no cambia ninguna regla. Decisiones del jugador: metas de DW-6 (cortes causados solo por una explosión ≤ 40%; demoledores muertos antes de explotar junto al muro ≥ 30%), corte propio clasificado por señal ≤ 5%, y las demás metas nuevas de Open Questions. Correcciones aplicadas en la misma sesión; el jugador pidió la re-revisión siguiente en otra sesión.
Prior verdict resolved: Yes (los 9 bloqueantes de la segunda pasada), con los dos restos de arriba pasados a esta lista
Findings:
- [BLOCKING] Player Fantasy l.30 / header (4): «desde otros tramos, no todas» era falso: seis tramos cubren a los arqueros rivales; el ejemplo de la catapulta (8, 7) estaba mal y «uno de la fila 5 o de la 9» no alcanza (hace falta un par, (4, 6) + (4, 8), para el tiro directo; contra el área, ni eso). Desacuerdo resuelto con la cuenta: 2 arqueros, no 3 (game-designer).
- [BLOCKING] AC-12: «bruto de 11» contra 8 + 6 = 14.
- [BLOCKING] Core Rules 14: `ErrorInterno` de F5 y F7 sin canal en las consultas puras; sin AC fuera del dominio.
- [BLOCKING] Revisión de QA: regla 3 (el arquero atraviesa un tramo propio) sin AC; AC-26 no cubre ataques; Edge Cases sin AC; «Sin duplicar» no coincidía con Movimiento AC-6 ni con Alcance AC-9.
- [BLOCKING] Open Questions: la meta de DW-6 se cumplía sola; la del corte propio tenía un razonamiento falso; la del pilar 3 leía el umbral al revés; las metas de playtest no tenían N ni proporción.
- [RECOMMENDED] Player Fantasy: «sentencia para el ejército, no para la partida»; momento ancla del defensor; «4 soldados» solo sin Grito.
- [RECOMMENDED] Tuning Knobs: el demoledor con TRAMOS_ROTOS_CORTE 3 / 4–5 / 6 o más; «por más turnos».
- [RECOMMENDED] Core Rules 7 y 10: cuándo se evalúa `enMuro`; fase `Reclutar`, no «fase de mover»; precondición de `enMuro`; dominio `8 × RADIO_MURO`.
- [RECOMMENDED] UI Requirements: lectura persistente de «Sin reclutamiento»; intención de *Debilitado*; aviso de corte suprimido con `PartidaTerminada`; tutorial con daño compartido y «2 tramos cualesquiera»; accesibilidad de #13 como pedido; OQ de anticipación online (#16 OQ 1).
- [RECOMMENDED] Acceptance Criteria: cláusula de reemplazo del Rey; AC-8f armable; AC-14 con control; casillas en AC-10/11/14b/6b; AC-30 y AC-32 partidos; Mellado fuera de AC-2; AC-13 «las dos caras»; AC-29 con ganador; AC-27b «1 arquero»; AC-17b redacción.
- Applied fixes: los 5 bloqueantes y las recomendadas de este GDD. Los arrastres nuevos a otros documentos están en `side-session-notes/.../castillo-pendientes-cierre-tanda-B.md`.
- Receipt: sin hash del GDD (se corrigió después de revisarlo). `maquina-de-turno.md` cambió otra vez (la edita la sesión paralela de la tanda B); `entrada-y-seleccion.md` se suma a los archivos leídos; `gdd-cross-review-2026-11-26.md` no se leyó en esta pasada.
Reviewed-Content-Hash: design/registry/entities.yaml 07713dcc52725c7d683477e25e8906d5db2f0c3e
Reviewed-Content-Hash: .claude/skills/design-review/SKILL.md 4dcc8b81162613c757144532f1b72708ab3ba27f
Reviewed-Content-Hash: design/gdd/libreria-de-reglas.md 5ac1d08ff674134ee106d55ad965c1784175b26d
Reviewed-Content-Hash: design/gdd/catalogo-de-piezas.md 24c2df9133aaea913698815462cb31d575219d30
Reviewed-Content-Hash: design/gdd/tablero-y-despliegue-inicial.md 9460a55db38e0d1d885bb1e96069d99b1b77e54c
Reviewed-Content-Hash: design/gdd/resolucion-simultanea-del-dano.md 52eb62677e85d84ecae345b930e37d123b9a4673
Reviewed-Content-Hash: design/gdd/movimiento.md 679bca4fed148d8ebd3f599a15a6acbe6ce44bda
Reviewed-Content-Hash: design/gdd/alcance-y-asignacion-de-ataques.md a81635dd4ffeb409a8522a47ed8dd20d7e414b75
Reviewed-Content-Hash: design/gdd/cobertura-del-escudero.md 1398f6027b8caa40188f5f9264d8fd5c25be6b81
Reviewed-Content-Hash: design/gdd/reclutamiento-y-limites.md b0a1c65376fa3da8052670ef3e24d224c2d30ad9
Reviewed-Content-Hash: design/gdd/modificador.md 2dae305bc81b22ff28cd83b7ebbd23b096b98d49
Reviewed-Content-Hash: design/gdd/maquina-de-turno.md 2614e052c742e1039b515e7306823f8dbaed9b9c
Reviewed-Content-Hash: design/gdd/ficha-de-pieza-y-hud.md 1090b0e1717f0e164d50e925f173f52c4c6f7054
Reviewed-Content-Hash: design/gdd/vista-previa-de-ataques.md 9f2b8c1e1276a36f2d00c39eeddc5149a7e341c9
Reviewed-Content-Hash: design/gdd/render-animacion-y-feedback-de-combate.md 84f4bc10496ac137ba64adee2a48ca89c5d58fc2
Reviewed-Content-Hash: design/gdd/interfaz-del-modificador.md 283c065061e4d5a48c2d383e0b666dd09b3c1084
Reviewed-Content-Hash: design/gdd/localizacion.md 3e81fd308be0094faddd9b519a8008e1f0b1fb79
Reviewed-Content-Hash: design/gdd/entrada-y-seleccion.md 27d538e355bf54759ebc019bc2fc80e667abb0b0
Reviewed-Content-Hash: design/gdd/game-concept.md 43e62d80536c71a39bae754ce48356caf6ee6beb
Reviewed-Content-Hash: design/art/art-bible.md c49b99572be1dffabb48c5974fc7ee54cdb29f28

## Review — 2026-11-27 — Verdict: NEEDS REVISION
Review mode: full · Workflow: full
Scope signal: L
Specialists: game-designer, systems-designer, level-designer, qa-lead, ux-designer, gameplay-programmer (sesión nueva, contexto limpio); creative-director (senior)
Blocking items: 7 | Recommended: ~30
Summary: Cuarta pasada, en una sesión nueva. Los 5 bloqueantes de la tercera están resueltos y verificados (la geometría de «qué protege el muro», recalculada por script en el tablero 24 × 15: seis tramos contra arqueros, el par (4, 6) + (4, 8) contra el tiro directo, ningún par contra el área; AC-12 con bruto 14; regla 14 con AC-22b y AC-24b; AC-3g y AC-26b; metas con N). Sin contradicciones nuevas con el registro (solo las 4 discrepancias ya anotadas). Los 7 bloqueantes nuevos son de texto, de contrato y de metas; ninguno cambia una regla de juego. Level-designer y ux-designer: APPROVED. Decisiones del jugador: corte propio como táctica legítima con techo (5%); la fantasía pasa de «sostener» a «hacer pagar» cada brecha; «iba perdiendo» se mide al terminar el turno 8 de cada jugador. Correcciones aplicadas en la misma sesión; el jugador pidió la re-revisión siguiente en otra sesión.
Prior verdict resolved: Yes (los 5 bloqueantes de la tercera pasada)
Findings:
- [BLOCKING] Formulas F3 (variables): `ataque_a` le sumaba el Grito al demoledor, contra el Catálogo regla 5 (Approved); con `vida_tramo` 31 y Grito la fórmula daba 1 golpe en vez de 2.
- [BLOCKING] Core Rules 7: la precondición de `enMuro` remitía a una de `EffectiveStats` que la Librería no define; ahora es total, y un jugador inválido en `tramosRotos`/`puedeReclutar` da `ErrorInterno`.
- [BLOCKING] Core Rules 1: atribuía a la Librería un invariante de ocupación que su tabla de dominios no tiene; queda como pedido pendiente.
- [BLOCKING] Open Questions (DW-6, meta b): el demoledor explota el mismo turno en que llega (Catálogo I1), así que la meta fallaba sola; denominador nuevo (los que terminan un turno a ≤ 7 casillas, 9 con Grito, de un tramo en pie).
- [BLOCKING] Open Questions (pilar 3): «iba perdiendo» sin momento de medida; ahora al terminar el turno 8 de cada jugador, con N ≥ 30 partidas con corte.
- [BLOCKING] Acceptance Criteria: la explosión propia junto al muro propio no tenía AC; se agregó AC-30e.
- [BLOCKING] Tuning Knobs (`TRAMOS_ROTOS_CORTE`): «con 6 o más, ninguna explosión sola corta» contradecía a AC-30d (6); con 4 también corta desde un lado del interior.
- [RECOMMENDED] Core Rules 14: variantes `Try…` dentro de `Aplicar`; las consultas públicas sin canal de error (comportamiento ante la falla en el ADR); `enMuro` y `tramosRotos` fuera de la lista; AC-22b con 8 casos, AC-22b/24b esperan `false`, AC-24 con umbral 16.
- [RECOMMENDED] States: umbrales con `TRAMOS_ROTOS_CORTE`, no 2 fijo. Regla 13: cómo se deriva el contador anterior para el cruce.
- [RECOMMENDED] Acceptance Criteria: AC-4 y AC-5b duplicaban Alcance AC-4 y AC-6; AC-3d coincide solo en parte con Movimiento AC-7; AC-5a sin el hueco rival; AC-7b, AC-14c y AC-27b no armables; AC-16b sin «sin reservar memoria»; AC-1b sin el Rey rojo; cita a Render AC-28; Edge de Deshacer contra AC-27a; Edge Cases sin AC ni exclusión.
- [RECOMMENDED] Player Fantasy/F3/F4/Edge: «4 soldados no rompen un tramo recto» y «ningún escudero cubre al Rey» valen solo con el muro entero; encierro simétrico; «más barato» por «más rápido»; «sostener» la muralla sin una acción que lo respalde.
- [RECOMMENDED] Open Questions: meta (a) de DW-6 no ataba la explosión al corte; muertes del Rey sin N ni tramo de partida; encuadre del corte propio (legítimo contra «explotación»); amenaza del rival que #13 no muestra; meta de lectura trivial. La tabla del game-designer sobre el bono de escudo (arquero 4 → 8) era errónea (restaba el escudo por golpe); la premisa original era cierta y se dejó con la tabla correcta (arquero 3 → 4).
- [RECOMMENDED] UI Requirements: la marca persistente reemplaza el borde grueso de #15; contador tocable (pedido a #13 y #15); aviso previo tapable por prioridades de #16; riesgo de no ver los tramos débiles; lector de pantalla (#23); filas faltantes en Cross-References; AC-32a/b con resincronización, 1 → 2 y captura en grises.
- Applied fixes: los 7 bloqueantes y las recomendadas que son de este GDD. Los pedidos nuevos a otros documentos están en `side-session-notes/.../castillo-pendientes-cierre-tanda-B.md` («Agregados de la cuarta pasada»).
- Receipt: sin hash del GDD (se corrigió después de revisarlo). `maquina-de-turno.md` cambió otra vez (la edita la sesión paralela de la tanda B); `art-bible.md` y `gdd-cross-review-2026-11-26.md` no se leyeron en esta pasada.
Reviewed-Content-Hash: design/registry/entities.yaml 07713dcc52725c7d683477e25e8906d5db2f0c3e
Reviewed-Content-Hash: .claude/skills/design-review/SKILL.md 4dcc8b81162613c757144532f1b72708ab3ba27f
Reviewed-Content-Hash: design/gdd/libreria-de-reglas.md 5ac1d08ff674134ee106d55ad965c1784175b26d
Reviewed-Content-Hash: design/gdd/catalogo-de-piezas.md 24c2df9133aaea913698815462cb31d575219d30
Reviewed-Content-Hash: design/gdd/tablero-y-despliegue-inicial.md 9460a55db38e0d1d885bb1e96069d99b1b77e54c
Reviewed-Content-Hash: design/gdd/resolucion-simultanea-del-dano.md 52eb62677e85d84ecae345b930e37d123b9a4673
Reviewed-Content-Hash: design/gdd/movimiento.md 679bca4fed148d8ebd3f599a15a6acbe6ce44bda
Reviewed-Content-Hash: design/gdd/alcance-y-asignacion-de-ataques.md a81635dd4ffeb409a8522a47ed8dd20d7e414b75
Reviewed-Content-Hash: design/gdd/cobertura-del-escudero.md 1398f6027b8caa40188f5f9264d8fd5c25be6b81
Reviewed-Content-Hash: design/gdd/reclutamiento-y-limites.md b0a1c65376fa3da8052670ef3e24d224c2d30ad9
Reviewed-Content-Hash: design/gdd/modificador.md 2dae305bc81b22ff28cd83b7ebbd23b096b98d49
Reviewed-Content-Hash: design/gdd/maquina-de-turno.md f5477a252f24cfbaeda8c7588f57131ed1bd15a3
Reviewed-Content-Hash: design/gdd/ficha-de-pieza-y-hud.md 1090b0e1717f0e164d50e925f173f52c4c6f7054
Reviewed-Content-Hash: design/gdd/vista-previa-de-ataques.md 9f2b8c1e1276a36f2d00c39eeddc5149a7e341c9
Reviewed-Content-Hash: design/gdd/render-animacion-y-feedback-de-combate.md 84f4bc10496ac137ba64adee2a48ca89c5d58fc2
Reviewed-Content-Hash: design/gdd/interfaz-del-modificador.md 283c065061e4d5a48c2d383e0b666dd09b3c1084
Reviewed-Content-Hash: design/gdd/localizacion.md 3e81fd308be0094faddd9b519a8008e1f0b1fb79
Reviewed-Content-Hash: design/gdd/entrada-y-seleccion.md 27d538e355bf54759ebc019bc2fc80e667abb0b0
Reviewed-Content-Hash: design/gdd/game-concept.md 43e62d80536c71a39bae754ce48356caf6ee6beb

## Review — 2026-11-27 — Verdict: NEEDS REVISION
Review mode: full · Workflow: full
Scope signal: L
Specialists: game-designer, systems-designer, level-designer, qa-lead, ux-designer, gameplay-programmer (sesión nueva); creative-director (senior)
Blocking items: 13 | Recommended: ~35
Summary: Quinta pasada, con el criterio nuevo del jugador (bloqueante = contradicción interna, regla ambigua, cifra mal, AC imposible o que no discrimina, o choque con un GDD Approved #1, #2, #4; contrato con un vecino no aprobado = pendiente del cierre, no frena). Las cifras geométricas centrales se re-verificaron por script y por cinco especialistas (seis tramos, par (4,6)+(4,8), demoledor 3/2/2/5, F3/F4, bordes de vida_tramo, AC-10 a AC-30e). Los 13 bloqueantes son de texto y ninguno cambia una regla ni una decisión del jugador; dos los introdujeron correcciones de la 4.ª pasada (B1, B7). Decisión del jugador: resultado tipado para el jugador inválido (B2). Correcciones aplicadas en la misma sesión.
Prior verdict resolved: Yes (los 7 de la 4.ª), salvo la fila de TRAMOS_ROTOS_CORTE, mal corregida (B1)
Criterios fijados (decisión del jugador, 2026-11-27, para esta pasada y las siguientes):
- Una meta de Open Questions que respalda una decisión del jugador ya tomada (DW-6, DW-2, lectura) se juzga como un AC: con N mínimo, escenario y algo que discrimine.
- Un número o una geometría en la prosa (Player Fantasy, Edge Cases) cuenta como cifra; la retórica es recomendado.
- Contrato con un vecino no aprobado = pendiente del cierre; AC duplicado con un vecino = se remite.
- Las cifras de posición viven en Formulas, *Hechos geométricos* (H1–H5); el resto las cita. Tras corregir, se busca cada cifra tocada en todo el GDD.
- La 6.ª pasada es de alcance cerrado: verifica los 13 arreglos y los párrafos tocados; fuera del diff solo entra un bloqueante con contraejemplo concreto (casilla, cifra o AC).
Findings:
- [BLOCKING] Tuning Knobs (TRAMOS_ROTOS_CORTE): «con 4 a 6, solo desde el interior» falso para 4 (k = 1 y demoledor en (5,6) → 4); reescrito por k rotos previos (H2b). [systems, level, qa]
- [BLOCKING] Core Rules 7 y 14: jugador inválido con ErrorInterno contra «sin canal de error»; ahora resultado tipado (decisión del jugador) y AC-13b. [gameplay-programmer, qa, systems]
- [BLOCKING] AC-16b: «la vida de un tramo» sin consulta definida; la regla 10 define `vidaTramo`, total. [gameplay-programmer]
- [BLOCKING] AC-27b: «1 arquero» (la Llamada da 4) y pila vacía si el recluta llega puesto; ahora aplica `Reclutar(Arquero)` y tiene un tramo roto. [gameplay-programmer, qa]
- [BLOCKING] AC-7b: tirada sin cara fijada y THEN que pasaba con todo rechazado; cara fija, `Aceptado` por comando, Atacar del rojo saltada. [gameplay-programmer, qa, systems]
- [BLOCKING] AC-26: movimiento sobrescrito a 12 viola Catálogo F8/F9 (Approved); ahora 6 y 8. [qa]
- [BLOCKING] Open Questions, DW-6 (b): Grito del turno equivocado (Catálogo I1, Approved) y distancia a un tramo no definida; ahora recorrido ≤ 8 hasta una casilla vecina del tramo, con N mínimo. [systems, qa]
- [BLOCKING] Player Fantasy: «desde la columna 9» falso (el par la cubre); ahora las 8 casillas reales (H4). [level; creative-director contra systems, que lo daba recomendado]
- [BLOCKING] Game Feel y Visual/Audio: alcance «en el mismo cuadro» sin la condición de UI Requirements. [ux]
- [BLOCKING] Open Questions, meta de lectura: «tocar» una captura; (b) sin tutorial ni reposo. [ux]
- [BLOCKING] AC-32a: resincronización que pasaba en vacío; ahora 1 → 2 por resync, con control. [ux]
- [BLOCKING] Open Questions, hueco de esquina: se cumplía sola; ahora escenario guionado. [qa]
- [BLOCKING] Edge Cases: «caben 4 golpes» contra las 5 casillas de la misma frase. [level, game-designer; subido por el creative-director]
- [RECOMMENDED] Player Fantasy/Overview: «solas no harían nada» (el tramo no tiene escudo: combinar rinde tempo); bajado de bloqueante por el creative-director, corregido igual. [game-designer]
- [RECOMMENDED] Corregidos en los mismos párrafos: arqueros (2 dejan en 5 de vida, 3 matan); área de la catapulta en cifras; Overview «perímetro del 5 × 5»; F3 frase reordenada; vida_tramo ≤ 6, área hasta 5; AC-21 THEN; convenciones de fixtures (sin bonos, turno ≥ 2, `n` fijada, `Aceptado`); rodeo inexistente en el lado del borde; señal sonora que sigue al aviso; Dependencies con #13, #14, #23, #24; rama Try → ErrorInterno declarada defensiva.
- [RECOMMENDED] Sin aplicar (siguen como recomendados): oráculo de turnos además de piezas (F4/AC-21); tapar el anillo como verbo del defensor; AC-9b (estado vivo); `GC.GetAllocatedBytesForCurrentThread` en Mono (verificar); metas sin N mínimo en las demás preguntas; denominadores de DW-6 (a), DW-2 y pilar 3; firmas de F5/F7 y formato del tramo (ADR); AC-17b append-only; *Debilitado* persistente; AC-31/AC-32b en grises con juez; dos castillos que cruzan a la vez; hueco del medio del borde como entrada del dueño (AC-26); `BONO_ESCUDO_MURO` × techo de presentación (#16 F2).
- Applied fixes: los 13 bloqueantes y las recomendadas listadas como corregidas. Los contratos nuevos con vecinos están en `side-session-notes/.../castillo-pendientes-cierre-tanda-B.md` («Agregados de la quinta pasada»).
- Receipt: sin hash del GDD (se corrigió después de revisarlo).
Reviewed-Content-Hash: design/registry/entities.yaml 07713dcc52725c7d683477e25e8906d5db2f0c3e
Reviewed-Content-Hash: .claude/skills/design-review/SKILL.md 4dcc8b81162613c757144532f1b72708ab3ba27f
Reviewed-Content-Hash: design/gdd/libreria-de-reglas.md 5ac1d08ff674134ee106d55ad965c1784175b26d
Reviewed-Content-Hash: design/gdd/catalogo-de-piezas.md 24c2df9133aaea913698815462cb31d575219d30
Reviewed-Content-Hash: design/gdd/tablero-y-despliegue-inicial.md 9460a55db38e0d1d885bb1e96069d99b1b77e54c
Reviewed-Content-Hash: design/gdd/resolucion-simultanea-del-dano.md 52eb62677e85d84ecae345b930e37d123b9a4673
Reviewed-Content-Hash: design/gdd/movimiento.md 679bca4fed148d8ebd3f599a15a6acbe6ce44bda
Reviewed-Content-Hash: design/gdd/alcance-y-asignacion-de-ataques.md a81635dd4ffeb409a8522a47ed8dd20d7e414b75
Reviewed-Content-Hash: design/gdd/reclutamiento-y-limites.md b0a1c65376fa3da8052670ef3e24d224c2d30ad9
Reviewed-Content-Hash: design/gdd/modificador.md 2dae305bc81b22ff28cd83b7ebbd23b096b98d49
Reviewed-Content-Hash: design/gdd/maquina-de-turno.md f5477a252f24cfbaeda8c7588f57131ed1bd15a3
Reviewed-Content-Hash: design/gdd/ficha-de-pieza-y-hud.md 1090b0e1717f0e164d50e925f173f52c4c6f7054
Reviewed-Content-Hash: design/gdd/vista-previa-de-ataques.md 9f2b8c1e1276a36f2d00c39eeddc5149a7e341c9
Reviewed-Content-Hash: design/gdd/render-animacion-y-feedback-de-combate.md 84f4bc10496ac137ba64adee2a48ca89c5d58fc2
Reviewed-Content-Hash: design/gdd/interfaz-del-modificador.md 283c065061e4d5a48c2d383e0b666dd09b3c1084
Reviewed-Content-Hash: design/gdd/entrada-y-seleccion.md 27d538e355bf54759ebc019bc2fc80e667abb0b0
Reviewed-Content-Hash: design/gdd/metricas-locales-de-partida.md 4b0beb1c2995defdc1e564cdce82fca557ce2a02
Reviewed-Content-Hash: design/art/art-bible.md c49b99572be1dffabb48c5974fc7ee54cdb29f28

## Review — 2026-11-27 — Verdict: APPROVED
Review mode: full · Workflow: full
Scope signal: L
Specialists: game-designer, systems-designer, level-designer, qa-lead, ux-designer, gameplay-programmer (alcance cerrado); creative-director (senior)
Blocking items: 1 (resuelto en la misma pasada) | Recommended: ~15 (aplicados)
Summary: Sexta pasada, de alcance cerrado (decisión del jugador): verificó los 13 arreglos de la quinta y los párrafos tocados. Cinco especialistas dieron APPROVED; el systems-designer encontró un bloqueante en la tabla nueva H3 (centrada en un tramo recto pegado a una esquina el área toca 4 tramos, no 3; la fila vida_tramo lo repetía), confirmado por script, corregido y verificado a mano por el creative-director junto con la fila de Tuning Knobs. Los recomendados de los cinco que aprobaron quedaron aplicados; también las dos precisiones opcionales de H3 (tramo de esquina 3, diagonal de afuera 2; «hasta 5»).
Prior verdict resolved: Yes (los 13 bloqueantes de la quinta)
Findings:
- [BLOCKING] Formulas, Hechos geométricos H3 y Tuning Knobs (vida_tramo): «centrada en un tramo recto: 3» falso para (4,6), (4,8), (1,5), (3,5), (1,9), (3,9), (0,6), (0,8) (4 tramos); corregido a 2/3/4/5 por posición. [systems-designer]
- [RECOMMENDED] Player Fantasy: «6 − 2 = 4» seguido de «lo dejás en 5» (el escudo se resta una vez a la suma); espejos de H4 listados ((0,13), no «fila 14»); «un soldado solo tardaría cuatro turnos». Aplicados. [game-designer, ux-designer, level-designer, systems-designer]
- [RECOMMENDED] AC-24: el umbral por parámetro es de F7, no de `puedeReclutar`; AC-26: el paréntesis sobre F9 era falso (el tope es F8, 2 de margen); AC-13b control con jugadores 0 y 1; AC-27b «salvo el segundo Deshacer»; Tipos sin solapar AC-13b; regla 14: `puedeReclutar` reusa su ErrorInterno. Aplicados. [gameplay-programmer, qa-lead]
- [RECOMMENDED] Meta de lectura: 5 s desde que se formula cada pregunta; vida 15 a propósito encima de Mellado; toques acotados a tramos y contador. H2b y Tuning «hasta 3 + k», «por sí solo». Aplicados. [ux-designer, qa-lead, level-designer]
- Status: Approved en la cabecera del GDD. No se tocó systems-index.md (regla de la tanda B: lo propaga la sesión de cierre). Contratos con vecinos en `side-session-notes/.../castillo-pendientes-cierre-tanda-B.md`.
Reviewed-Content-Hash: design/gdd/castillo-y-muros.md e2109fc867b4b33f496a4266dd581ca14de70b1f
Reviewed-Content-Hash: design/registry/entities.yaml 07713dcc52725c7d683477e25e8906d5db2f0c3e
Reviewed-Content-Hash: .claude/skills/design-review/SKILL.md 4dcc8b81162613c757144532f1b72708ab3ba27f
Reviewed-Content-Hash: design/gdd/libreria-de-reglas.md 5ac1d08ff674134ee106d55ad965c1784175b26d
Reviewed-Content-Hash: design/gdd/catalogo-de-piezas.md 24c2df9133aaea913698815462cb31d575219d30
Reviewed-Content-Hash: design/gdd/tablero-y-despliegue-inicial.md 9460a55db38e0d1d885bb1e96069d99b1b77e54c
Reviewed-Content-Hash: design/gdd/modificador.md 2dae305bc81b22ff28cd83b7ebbd23b096b98d49
Reviewed-Content-Hash: design/gdd/maquina-de-turno.md f5477a252f24cfbaeda8c7588f57131ed1bd15a3
Reviewed-Content-Hash: design/gdd/reclutamiento-y-limites.md b0a1c65376fa3da8052670ef3e24d224c2d30ad9

## Cierre de la tanda B — 2026-11-28 (no es una pasada de revisión)
La sesión de cierre aplicó los pedidos de `castillo-pendientes-cierre-tanda-B.md` a los vecinos y al registro. En este GDD solo cambió texto: UI Requirements («Cortás tu propio reclutamiento» para el corte propio; inspección del tramo sin pieza propia elegida), regla 13 (`tramosRotosAntes`, agregado por #9) y las menciones a §9 del art bible, ya corregido. El Status sigue Approved.
