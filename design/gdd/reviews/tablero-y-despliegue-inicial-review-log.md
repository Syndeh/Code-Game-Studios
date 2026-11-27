# Review log — Tablero y despliegue inicial (#4)

## Review — 2026-11-27 — Verdict: NEEDS REVISION
Review mode: full · Workflow: full
Scope signal: S
Specialists: game-designer, systems-designer, level-designer, qa-lead, creative-director (senior)
Blocking items: 5 | Recommended: 7
Summary: no hace falta rediseñar nada. La geometría cierra, las cuentas verificadas a mano son correctas y no hay conflictos con el registro. Lo que falla es la parte escrita del GDD: falta el turno 2 con Grito (raíz de W-13 y DW-3), hay rangos de Tuning imposibles, siete AC no se pueden probar, la fantasía no tiene con qué cumplirse sin una capa de amenaza y el punto muerto aparece como neutro cuando no lo es. Ninguna decisión ya tomada se reabre.
Prior verdict resolved: First review
Findings:
- [BLOCKING] Formulas / Open Questions: falta `margen_primer_golpe` para el turno 2 con Grito. W-13: el tope del Grito se justificaba con el turno 1, que no tiene tirada. La OQ 1 tenía dos errores: decía «8 y 9 seguras para azul» y que la v5 «quita» la ventaja de abrir reclutando, cuando en realidad se la pasa a rojo.
- [BLOCKING] Tuning Knobs / Formulas: el rango de `RADIO_MURO` (1 a 3, hasta 5) contradecía la precondición `cc − r ≥ 0`; el interior de `casilla_de_castillo` estaba definido como `radio ≤ 1`; el rango seguro de `COLUMNAS` 22–24 y el del margen al borde ignoraban el turno 2; `techo_movimiento` no tenía en cuenta el alcance.
- [BLOCKING] Acceptance Criteria: AC-26 no es un test viable; AC-29 dependía de un ADR que no existe y Game Feel lo citaba para las consultas; regla 6, Edge Cases y AC-6 no coincidían sobre el Rey; AC-21 no detectaba la envoltura; faltaba el AC del arquero sobre tramo; AC-23 no podía construir su GIVEN; AC-7 era ambiguo y leía un archivo.
- [BLOCKING] Player Fantasy / Edge Cases: no había pedido de una capa de amenaza para contar los pasos con y sin Grito; el punto muerto se describía como neutro (DW-3).
- [BLOCKING] Dependencies / Cross-References: W-21 (#13 y #17 ausentes, #18 como Blanda); §7 del art bible contradictorio; Status «In Design»; bidireccionalidad vencida; prototipo v4 citado como fuente de datos.
- [RECOMMENDED] Core Rules 3: `hayTramo` como «vida > 0», dueño de cada tramo y regla de escritura sobre una casilla llena (`ErrorInterno`).
- [RECOMMENDED] Formulas: `turnos_hasta_contacto` sin `distancia_frente` en la tabla de variables y con el ejemplo del Grito mal; faltaban precondiciones en `casilla_inicial`.
- [RECOMMENDED] Core Rules 7: asimetría del primer recluta (13 casillas libres en el anillo) aceptada como rasgo, con su AC.
- [RECOMMENDED] Tuning Knobs: la perilla del margen al borde no listaba sus efectos aguas abajo (anillo de 24, #10 F4, #17).
- [RECOMMENDED] Acceptance Criteria: AC-4 con los 81 pares, lista exacta del anillo, AC-9 como biyección, vida inicial de tramos, precedencia de errores, atomicidad y AC-12 por reflexión.
- [RECOMMENDED] Interactions: supuestos de los oráculos (carril recto y libre) sin declarar.
- [RECOMMENDED] Tuning Knobs: «16:9» atribuido a la grilla (24 × 15 es 1,6).

Disagreements: game-designer marcó la capa de amenaza como bloqueante y el creative-director lo resolvió así: lo que bloquea es el pedido escrito en #4, no la capa, que es de #7, #15 y #16. Sobre el punto muerto, game-designer lo veía cerca de crítico y level-designer como recomendado; quedó que no bloquea el diseño, pero sí bloquea que el GDD lo subestime.

Decisiones del jugador en esta revisión:
- Disparador de DW-3: rojo gana 14 o más de 20 partidas, o rojo da el primer daño en 15 o más de 20.
- Respuesta si salta el disparador: quedan abiertas las dos, sin preferencia: (a) azul también tira en el turno 1; (b) rojo no tira en su primer turno.
- Asimetría del primer recluta: se acepta y se documenta (regla 7, AC-33).
- Regla nueva: una escritura que rompe un invariante del tablero da `ErrorInterno` (regla 3, AC-35).

Revisions applied 2026-11-27, mismo día, aprobadas por el jugador; falta la re-revisión. Bloqueante → arreglo:
- Turno 2 con Grito → `margen_primer_golpe` nueva (3 / 1; con bono 3 da 0), AC-32, AC-36 (propiedad), OQ 1 y OQ 2 reescritas, fila #11 de Dependencies y fila del Grito en Tuning.
- Rangos de Tuning → `COLUMNAS` solo 24 (tabla con la columna del turno 2), margen al borde solo 2, `RADIO_MURO` con `df = 17 − 4r` (seguro 2), interior `radio < RADIO_MURO`, techo con alcance `m + R ≤ 13`.
- AC → AC-3 a AC-7, AC-9, AC-11, AC-12, AC-15, AC-17 a AC-27, AC-29 y AC-30 reescritos; nuevos AC-31 a AC-36.
- Fantasía y punto muerto → momento ancla «con y sin Grito», pedido de la capa de amenaza (Player Fantasy y OQ nueva), Edge Case y OQ 3 con la cadena completa.
- W-21 y limpieza → Interactions y Dependencies con #11, #13, #15, #16, #17 y #18; bidireccionalidad al 2026-11-27; Cross-References (§7, v5, cross-review); Status In Review.

Reviewed-Content-Hash: design/gdd/tablero-y-despliegue-inicial.md 55416e65dbdc2dacf7367d7df3c1d0ee09ab055a
Reviewed-Content-Hash: design/registry/entities.yaml a1def8387514578df4d215eec6397bb98386ba62
Reviewed-Content-Hash: .claude/skills/design-review/SKILL.md 4dcc8b81162613c757144532f1b72708ab3ba27f
Reviewed-Content-Hash: design/gdd/libreria-de-reglas.md 7f8922710484111ebf7ae82406627649fe16e04f
Reviewed-Content-Hash: design/gdd/catalogo-de-piezas.md 06dcbb6869c82ccc59b08d260d30c3cb1091667b
Reviewed-Content-Hash: design/gdd/castillo-y-muros.md 2bf9b1977a323e2ba287a5965d3986bff874ea71
Reviewed-Content-Hash: design/gdd/modificador.md 451e50bd6f8fe9f2aa890968b9cdd59f33bde888
Reviewed-Content-Hash: design/gdd/maquina-de-turno.md 0af0ecd29538b4dbcfba23734b3dc6fb6bf01e38
Reviewed-Content-Hash: design/gdd/metricas-locales-de-partida.md f537fae3cdaaea30d3ee110d24eb796063abb71a
Reviewed-Content-Hash: design/gdd/reclutamiento-y-limites.md e627256e98aea8addef670dfafbbb1add0ace54a
Reviewed-Content-Hash: design/gdd/entrada-y-seleccion.md 3fd6b9293ba7a64dd7203b971f6fa697d7e5bfe7
Reviewed-Content-Hash: design/gdd/render-animacion-y-feedback-de-combate.md 84f4bc10496ac137ba64adee2a48ca89c5d58fc2
Reviewed-Content-Hash: design/gdd/interfaz-del-modificador.md 661f75e3fe5c891e0a0ad5abe008327470826479
Reviewed-Content-Hash: design/gdd/gdd-cross-review-2026-11-26.md 4e2bcb868717e3e9c62d8959fed5d77a54b22266
Reviewed-Content-Hash: prototypes/rolchess-v5-concept/README.md 0976e24738eb78704795aa4aaa8d9201e30979e0

## Review — 2026-11-27 — Verdict: NEEDS REVISION
Review mode: full · Workflow: full
Scope signal: S
Specialists: game-designer, systems-designer, level-designer, qa-lead, creative-director (senior)
Blocking items: 6 | Recommended: 23
Summary: re-revisión. Cuatro de los cinco bloqueantes anteriores quedaron resueltos. El de fantasía y punto muerto se resolvió solo en parte: ya está el pedido de la capa de amenaza y el punto muerto ya no figura como neutro, pero los porcentajes de exposición del turno 2 siguen subestimados. Las cuentas cierran y no hay conflictos de valor con el registro. Los bloqueantes nuevos son de texto: afirmaciones falsas sobre el equilibrio de la apertura y un invariante que no cubre al recluta del turno 2. No se reabrió ninguna decisión tomada.
Prior verdict resolved: Partial (4 de 5; el de Player Fantasy / Edge Cases, en parte)
Findings:
- [BLOCKING] Core Rules 10 / Open Questions: «80% de Modificador activo» omite que Distracción (20%) quita la fase de mover; hay que escribir 20/40/40 con la zona de cada cara (columnas 14, 10 y 8).
- [BLOCKING] Edge Cases / Open Questions: la exposición de azul está subestimada. Las columnas 8 y 9 están expuestas en el 40% de los casos (solo con Grito); de la 10 en adelante, en el 80% (cuando rojo mueve).
- [BLOCKING] Formulas / Edge Cases / Open Questions: el arquero «sobre su tramo sin moverse» es falso, porque empieza en el anillo; además, la columna 10 del arquero azul se usa para argumentar la ventaja de rojo.
- [BLOCKING] Formulas / AC-36: el dominio `k` de `margen_primer_golpe` no cubre al recluta del turno 2 de rojo.
- [BLOCKING] Tuning Knobs: la nota de `piezas_para_matar` ignora el Grito (4 soldados con Grito matan al Rey).
- [BLOCKING] Cabecera: estado de la revisión.
- [RECOMMENDED] Formulas: rango de `bono_t` 0-10, mínimo −26; la precondición de `FILAS` par sobra; el Rey queda fuera de `k`.
- [RECOMMENDED] Tuning Knobs: topes por tipo de `m + R`; «Solo 24» es falso; «la única perilla es m»; «margen 2» es ambiguo; RADIO_MURO 3 deja golpear también al arquero.
- [RECOMMENDED] Acceptance Criteria: AC-26 tautológico; AC-12 debería usar lista blanca; AC-35 sin la excepción del arquero; AC-24 sin el caso de vida de tramo ni el conteo de 32; AC-32 Logic con Catálogo de prueba; AC-2 sin bordes; AC-36 sin chequeo de la casilla de llegada; NITs en AC-4, AC-10, AC-16, AC-19, AC-23, AC-27, AC-28, AC-29 y AC-33.
- [RECOMMENDED] Core Rules 7 / Edge Cases: el «frente» del anillo tiene 5 o 7 casillas según dónde se lea; «13 pasos del rival» es la distancia al muro; el jinete en (5,9) depende del anillo inicial.
- [RECOMMENDED] UI Requirements: el recuadro de #17 tapa la zona de decisión del turno 2.
- [RECOMMENDED] Player Fantasy / Open Questions: momento ancla con tres casos; capa de amenaza calculada antes de la tirada; #13 como disparador; «débilmente dominante» pasa a ser hipótesis.

Disagreements: systems-designer marcó como BLOCKING que AC-36 no detectaría arqueros tapados por sus aliados (S1). Se descartó porque Movimiento, regla 5, deja atravesar aliados (`movimiento.md` l.12 y l.58), y el creative-director lo confirmó. Queda como recomendado chequear la casilla de llegada. Q1 (el recluta del turno 2): qa-lead lo marcó BLOCKING y game-designer y systems-designer, RECOMMENDED; el creative-director lo dejó BLOCKING porque el invariante promete cubrir un camino que las reglas permiten.

Reviewed-Content-Hash: design/gdd/tablero-y-despliegue-inicial.md 7b0dfbacb62401d09968e9167d03214f846cddad
Reviewed-Content-Hash: design/registry/entities.yaml a1def8387514578df4d215eec6397bb98386ba62
Reviewed-Content-Hash: .claude/skills/design-review/SKILL.md 4dcc8b81162613c757144532f1b72708ab3ba27f
Reviewed-Content-Hash: design/gdd/modificador.md 451e50bd6f8fe9f2aa890968b9cdd59f33bde888
Reviewed-Content-Hash: design/gdd/movimiento.md 679bca4fed148d8ebd3f599a15a6acbe6ce44bda
Reviewed-Content-Hash: design/gdd/reclutamiento-y-limites.md e627256e98aea8addef670dfafbbb1add0ace54a
Reviewed-Content-Hash: design/gdd/alcance-y-asignacion-de-ataques.md e7011b26918d7a0a5a7b7da793d0c803e9b13a43

Revisions applied 2026-11-27, el mismo día. El jugador eligió «Revisar ahora», bloqueantes y recomendados. Bloqueante → arreglo:
- Reparto 20/40/40 → regla 10 y OQ de ventaja: Distracción hasta la columna 14, reclutar hasta la 10 y Grito hasta la 8. Momento ancla con tres casos.
- Franjas de exposición → Edge Case, OQ de ventaja y OQ del punto muerto: columnas 8-9 en el 40% (solo Grito), 10 en adelante en el 80%.
- Arquero sobre el tramo → Formulas, Edge Case y OQ del Grito: un paso desde el anillo (azul llega a 9 → 10; rojo a 14 → 13). Se atribuyó bien cada columna.
- Recluta del turno 2 → dominio `k` de `margen_primer_golpe` (recluta en d = 11 y sin bono; el Rey queda fuera, con margen 5), AC-32 en dos partes (Logic con Catálogo de prueba e Integration) y AC-36 con los casos Grito y reclutar.
- `piezas_para_matar` → con Grito, 4 soldados matan al Rey (ceil(35/10) = 4).
- Cabecera → estado de la re-revisión.
Recomendados aplicados: `bono_t` 0-10 con mínimo −26; topes por tipo de `m + R` en Tuning (arquero 8, soldado 7, escudero 6, reclutable 10); «Seguro 24» con la nota sobre 25 o más; perillas de Tuning; RADIO_MURO 3 (golpean escudero y arquero) aclarado frente a AC-17; precondición de `FILAS` par quitada; oráculos de Formulas; frente del anillo de 7 casillas; «13 pasos del muro rival» y primeras casillas de recluta rojo; nota del jinete con el anillo inicial; carril libre citando Movimiento, regla 5; pedido a #17 de no tapar las filas 4-10 entre las columnas 5 y 18 en el turno 2; #13 como gesto de la capa (Interactions, OQ y pendientes); capa calculada antes de la tirada; «hipótesis» en lugar de «débilmente dominante»; ejemplo de daño con la casilla (8, 6) y los IDs 18-20 y 23. AC-2 (bordes), AC-4 (sectores 0 y 10), AC-10 (En curso), AC-12 (lista blanca), AC-16, AC-17, AC-19, AC-23 (factory), AC-24 (32 tramos y vida), AC-26 (huella contra la tabla, alineada con Edge Cases), AC-27, AC-28 y AC-35 (huella de estado), AC-29 (máquina y 100 lotes), AC-33 (consulta) y AC-35 (excepción del arquero al escribir).
Regla nueva derivada (Edge Cases, estado cargado): un estado con una cantidad de tramos distinta de 32, o con un tramo de vida fuera de 0 a `vida_tramo`, da `EstadoInvalido`. Sale de la regla 3, que ya decía que los tramos rotos siguen existiendo como dato.

## Review — 2026-11-27 — Verdict: NEEDS REVISION
Review mode: full · Workflow: full
Scope signal: S
Specialists: game-designer, systems-designer, level-designer, qa-lead, creative-director (senior)
Blocking items: 3 | Recommended: 8
Summary: tercera pasada. Los 6 bloqueantes de la segunda quedaron resueltos: los cuatro especialistas y la revisión principal rehicieron todas las cuentas y cierran. No se reabrió ninguna decisión tomada y no hay conflictos de valor con el registro. Los tres bloqueantes nuevos son de coherencia y testabilidad frente al Catálogo aprobado: la F9 que valida `techo_movimiento` en `CrearPartida`, los `ConfigInvalida` sin código de regla y el caso del Rey distinto de 1, que ya corta la F5.
Prior verdict resolved: Yes (6 de 6)
Findings:
- [BLOCKING] Formulas / Dependencies / Edge Cases: la F9 del Catálogo valida `movimiento + bono ≤ techo_movimiento` en `CrearPartida`, pero el GDD decía «ninguno entra al núcleo» y «`movimiento` solo en los oráculos»; además, la F9 suma un bono que el recluta no tiene e ignora el alcance.
- [BLOCKING] Core Rules 4-6 / AC-4, AC-6, AC-27, AC-34: los `ConfigInvalida` del Tablero no tenían código; el AC-30 del Catálogo espera uno.
- [BLOCKING] Core Rules 6 / AC-6: el `topeVivas` del Rey distinto de 1 es inalcanzable en `CrearPartida`, porque la F5 lo corta en `PrepararContexto`.
- [RECOMMENDED] AC-32: la firma con una vista de solo lectura (un Catálogo de 4 tipos no pasa F1); AC-34 con una lista fija; AC-36 con la casilla de origen y la columna máxima por tipo.
- [RECOMMENDED] Core Rules 2 / AC-2: la cruz y el contorno de una casilla fuera de la grilla, sin definir.
- [RECOMMENDED] Acceptance Criteria: la tabla de apertura de la regla 10, sin criterio ni remisión.
- [RECOMMENDED] Tuning: el tope del bono (2) es de rango seguro y no tiene guarda en ejecución; AC-32 Integration no nombraba el 2.
- [RECOMMENDED] Formulas: mínimo de −26 inalcanzable (R sin techo), rango de `m` (el real es 10), «el tope que aprieta» (8/7/6 para el ejército, ≤ 10 para los reclutables), `bono_t` por caso de la cara.
- [RECOMMENDED] Edge Cases / Open Questions: «azul está obligado» depende de los timers de etapa (Máquina l.368-369); se mezclaban dos configuraciones excluyentes (arquero en el tramo y reclutas con los arqueros en el anillo).
- [RECOMMENDED] UI Requirements: el pedido a #17 dejaba sin cubrir piezas azules amenazadas fuera de las filas 4-10.
- [RECOMMENDED] Formulas: «el recluta» en singular (Llamada da hasta 4); el daño del caso reclutar no está calculado.
- NITs: franjas en las filas del frente y «la 10 y la 11»; «13 pasos o más»; «con bono 2» en la tabla de COLUMNAS; fila de composición frente a la constante 4; `vida_tramo` en el GIVEN de AC-24; nota sobre la numeración de los AC; AC-12 citando el AC de la Librería.

Disagreements: qa-lead dejó la F9 como RECOMMENDED; game-designer y systems-designer, como BLOCKING. El creative-director la dejó BLOCKING: el GDD afirmaba algo que un documento aprobado contradice, y quien implemente leyendo solo el Tablero no validaría F9. game-designer dejó el caso del Rey como RECOMMENDED y qa-lead como BLOCKING; el creative-director, BLOCKING (es un AC que no se puede construir y va unido a la elección de códigos). systems-designer propuso cambiar la F9 a `m + R ≤ 13`: queda fuera del alcance de esta sesión y pasa a Open Questions, con dueño el Catálogo y el jugador. level-designer no encontró bloqueantes.

Decisiones del jugador en esta revisión:
- Códigos por regla: `ConfigInvalida(T4)` (par de sectores), `(T5)` (`vida_tramo`) y `(T6)` (ejército), evaluados en el orden T4 → T5 → T6; frente a F9 manda el orden de la Librería.
- La cruz y el contorno de una casilla fuera de la grilla son la lista vacía, sin error.
- Un Rey distinto de 1 en `CrearPartida` es `ErrorInterno` (guarda defensiva); el rechazo real es F5.

Revisions applied 2026-11-27, en la misma sesión, aprobadas por el jugador (bloqueantes y recomendados). Bloqueante → arreglo:
- F9 → la introducción de Formulas dice que `techo_movimiento` entra al núcleo por la F9; párrafo «Lo que valida el núcleo y lo que no» en `techo_movimiento`; Edge Case de F9; fila #2 de Dependencies; AC-19 con los dos invariantes; AC-34 con un caso de F9; Open Question nueva F9 frente a `m + R ≤ 13`.
- Códigos → reglas 4, 5 y 6 con T4, T5 y T6 y su orden; Edge Cases (incluido el de varias violaciones); AC-4, AC-6, AC-27 y AC-34 con su código; AC-37 nuevo (orden); convención en la introducción de los AC; Interactions, Tuning y Cross-References.
- Rey → regla 6 y Edge Case: F5 en `PrepararContexto`, `ErrorInterno` como guarda; AC-6 espera `ConfigInvalida(F5)` y pasa por `PrepararContexto`.
Recomendados y NITs aplicados: todos los de la lista.
Receipt: el hash del GDD no se estampa porque las revisiones se aplicaron después de la pasada; así la próxima la trata como un documento cambiado y hace la revisión completa.

Reviewed-Content-Hash: design/registry/entities.yaml a1def8387514578df4d215eec6397bb98386ba62
Reviewed-Content-Hash: .claude/skills/design-review/SKILL.md 4dcc8b81162613c757144532f1b72708ab3ba27f
Reviewed-Content-Hash: design/gdd/catalogo-de-piezas.md 299af94496afef9ccf01ba6718cd0514993fa5bb
Reviewed-Content-Hash: design/gdd/modificador.md f4dd0097846c4bec69bb73bc4561b6ffc643dbea
Reviewed-Content-Hash: design/gdd/reclutamiento-y-limites.md e627256e98aea8addef670dfafbbb1add0ace54a
Reviewed-Content-Hash: design/gdd/movimiento.md 679bca4fed148d8ebd3f599a15a6acbe6ce44bda
Reviewed-Content-Hash: design/gdd/maquina-de-turno.md 0af0ecd29538b4dbcfba23734b3dc6fb6bf01e38

## Review — 2026-11-27 — Verdict: APPROVED
Review mode: full · Workflow: full
Scope signal: S
Specialists: creative-director (senior). Fue una verificación corta y focalizada (4.ª pasada), sin el panel completo, por decisión del jugador. Lo que quede entre GDD lo revisa el `/consistency-check` del cierre de la tanda A.
Blocking items: 0 | Recommended: 0
Summary: se confirmó que los 3 bloqueantes de la 3.ª pasada están resueltos. F9: `techo_movimiento` entra al núcleo; están el párrafo de qué valida el núcleo, el Edge Case, la fila de Dependencies, AC-19, el caso de F9 en AC-34 (no se cruza con T6) y la OQ con dueño. Códigos: T4, T5 y T6 aparecen en las reglas, los Edge Cases, los AC y la convención. Rey: lo corta F5 en `PrepararContexto` y `ErrorInterno` queda como guarda; AC-6 se puede construir. Los recomendados de la 3.ª pasada están aplicados. La alineación con los dos Approved coincide. La Librería, regla 2, valida en este orden: F1→F8, bonos, F9, T4, T5, `limite_normal`/`tablaReclutas`/pila y T6; el T4→T5→T6 del Tablero respeta ese orden relativo. El Catálogo pone F1–F8 en `PrepararContexto` (devuelve la primera que falla) y F9 en `CrearPartida`, igual que el Tablero. El registro no cambió.
Prior verdict resolved: Yes (3 de 3)
Findings:
- [NIT] Core Rules 6 / Edge Cases: cambiar «lo fija la Librería» por «Librería, regla 2». Aplicado.
- [NIT] Cabecera: la línea Design Review seguía pidiendo una re-revisión. Actualizada.
Fuera de alcance (lo propaga la sesión de cierre): el AC-30 del Catálogo todavía no nombra `T6`; la fila 4 de `systems-index.md` pasa a Approved.

Reviewed-Content-Hash: design/gdd/tablero-y-despliegue-inicial.md 9460a55db38e0d1d885bb1e96069d99b1b77e54c
Reviewed-Content-Hash: design/registry/entities.yaml a1def8387514578df4d215eec6397bb98386ba62
Reviewed-Content-Hash: .claude/skills/design-review/SKILL.md 4dcc8b81162613c757144532f1b72708ab3ba27f
Reviewed-Content-Hash: design/gdd/libreria-de-reglas.md 5ac1d08ff674134ee106d55ad965c1784175b26d
Reviewed-Content-Hash: design/gdd/catalogo-de-piezas.md 299af94496afef9ccf01ba6718cd0514993fa5bb
