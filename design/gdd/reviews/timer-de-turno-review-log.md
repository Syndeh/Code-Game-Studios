# Review log — Timer de turno (#12)

## Review — 2026-11-27 — Verdict: NEEDS REVISION
Review mode: full · Workflow: full
Scope signal: M
Specialists: game-designer, systems-designer, qa-lead, ux-designer, unity-specialist, network-programmer, gameplay-programmer, audio-director; senior: creative-director
Blocking items: 15 | Recommended: 14
Summary: El concepto, la fantasía B («el reloj como recurso») y el reparto con la Máquina y la Librería son sólidos. Los bloqueantes son de precisión del contrato: qué cuenta como tiempo consumido (hora de recepción, detección atrasada, un solo acumulador), una máquina de estados que no cerraba, la vista y los eventos que contradecían la regla del único ámbar de #15, guardas que no probaban lo que decían, y AC imposibles. Cuatro decisiones del jugador: el reloj corre con la página oculta; la placa no cuesta reloj (la etapa espera la placa o el primer comando); `aviso_reloj_s` = 120 s con guarda ≥ `turno_max_s`; guarda `turno_max_s × 8 < reloj_jugador_s`. Además: ninguna pausa activa en el MVP y el salto de reloj se cobra entero (solo `SaltoDetectado`). Revisiones aplicadas en la misma sesión; pendiente de re-revisión.
Prior verdict resolved: First review
Warnings del informe cruzado resueltos acá: W-1 (regla 17), W-2 (ya cerrado; regla 11 completada con el caso sin resolución), W-15 (regla 9), A-20 (`SaltoDetectado`), B-12 (`tope_partida_s` con alias `T_tope`), B-13 (rango válido = seguro, guardas), F-03 (`TramoFinalIniciado` al cerrar el turno), F-09 (`TurnoMedido` parcial), F-11 (regla 8: qué ve el jugador); DW-1, DW-3 y DW-4 quedan con disparadores en Open Questions.
Findings:
- [BLOCKING] Core Rules 2/8: hora de recepción sin definir y sin gancho «antes de aplicar» (AC-12 imposible)
- [BLOCKING] Core Rules 7/D3: ms entre vencimiento y detección sin regla; dos modelos de acumulación; `pausado_ms` con rango falso
- [BLOCKING] Core Rules 9/Edge: carrera `CierreInvalido` inalcanzable con la regla 8; `consumido_ms` podía pasar de 750.000
- [BLOCKING] States: pausa por salto sin salida; faltaba la pausa en `EnEspera`; disparador `PartidaTerminada` mal nombrado
- [BLOCKING] Core Rules 8: orden observable de eventos y reentrada con la Librería síncrona sin definir
- [BLOCKING] Core Rules 12: `TurnoMedido` sin turno parcial al terminar a mitad de turno (F-09)
- [BLOCKING] Core Rules: contrato de Sin límite sin definir
- [BLOCKING] Core Rules 9: `LimitePartida` sobre un `Reclutar` de jugador en Sin límite trababa la partida (W-15)
- [BLOCKING] Edge Cases: «Sin límite solo la corta C_MAX» era falso
- [BLOCKING] Core Rules 13: faltaban `avisoEtapa` y el estado «esperando a la autoridad» en la vista
- [BLOCKING] Visual/Audio: filas de aviso contradecían el único ámbar de #15 (F3); el reloj no tiene forma de anillo
- [BLOCKING] Visual/Audio: el tictac seguía a la etapa aunque limitara el reloj y no tenía condición de corte
- [BLOCKING] Tuning Knobs: rango válido vs seguro confundidos; guarda de `turno_max_s` no cumplía su meta; faltaban guardas
- [BLOCKING] Formulas: la tabla de uso máximo se presentaba como límite de duración siendo cota de tiempo consumido
- [BLOCKING] Acceptance Criteria: AC-17, AC-22, AC-13 y AC-24 imposibles o mal tipados; fuente falsa con pasos > 5 s
- [RECOMMENDED] Core Rules 2/9: códigos `ModoTimerInvalido`/`RelojInvalido` sin dueño de presentación (pasa a `ConfigTimerInvalida`)
- [RECOMMENDED] Core Rules 6/12: `TramoFinalIniciado` llegaba tarde para la placa de la cara
- [RECOMMENDED] Core Rules 13: faltaban `numeroTurno`, `venceEn_s`, `restanteHastaTramo_s`; `jugadorActivo` en la espera
- [RECOMMENDED] Core Rules 12: `TiempoAgotado` antes de la respuesta mostraba «Tiempo» en un timeout obsoleto
- [RECOMMENDED] Core Rules 9/Edge: «TerminarMovimiento cierra el turno» y «TimeoutObsoleto o ComandoViejo» incorrectos
- [RECOMMENDED] Core Rules 10/Edge: pestaña oculta en Web daba tiempo gratis (decisión del jugador: el reloj corre)
- [RECOMMENDED] Core Rules 7: placa de la cara y reloj (W-1; decisión del jugador: la etapa espera la placa)
- [RECOMMENDED] Tuning Knobs: `aviso_reloj_s` = 60 no daba un turno de margen (decisión: 120 s)
- [RECOMMENDED] Formulas: tabla de uso máximo fija en 750; cota general y pared sin decir
- [RECOMMENDED] Formulas D4: niveles de aviso pendientes al armar; doble cruce de umbrales
- [RECOMMENDED] Open Questions: disparadores para RelojAgotado, `vencio_atacar` (DW-4) y desventaja de azul (DW-3); `relojInicio_ms` (DW-1)
- [RECOMMENDED] Visual/Audio: pedidos a Audio (prioridad, nivel 1, fatiga) y al art bible/#15 (forma estática con Reducir efectos)
- [RECOMMENDED] UI Requirements: lector de pantalla (a pedido, qué se anuncia, Web NO VERIFICADO)
- [RECOMMENDED] Acceptance Criteria: AC-1, 2, 4, 12, 14, 15, 16, 19, 20, 25, 26, 27 reescritos; AC nuevos para placa, atraso, parcial, orden, Sin límite, guardas
Reviewed-Content-Hash: design/gdd/timer-de-turno.md 6e71847aa2219f22d202b8e9b0b92460a0dc5bad
Reviewed-Content-Hash: design/registry/entities.yaml 07713dcc52725c7d683477e25e8906d5db2f0c3e
Reviewed-Content-Hash: .claude/skills/design-review/SKILL.md 4dcc8b81162613c757144532f1b72708ab3ba27f
Reviewed-Content-Hash: design/gdd/maquina-de-turno.md bbe86a444be2634587f8b4797c33f2a14bb5a105
Reviewed-Content-Hash: design/gdd/libreria-de-reglas.md 5ac1d08ff674134ee106d55ad965c1784175b26d
Reviewed-Content-Hash: design/gdd/catalogo-de-piezas.md 24c2df9133aaea913698815462cb31d575219d30
Reviewed-Content-Hash: design/gdd/tablero-y-despliegue-inicial.md 9460a55db38e0d1d885bb1e96069d99b1b77e54c
Reviewed-Content-Hash: design/gdd/ficha-de-pieza-y-hud.md 1090b0e1717f0e164d50e925f173f52c4c6f7054
Reviewed-Content-Hash: design/gdd/render-animacion-y-feedback-de-combate.md 84f4bc10496ac137ba64adee2a48ca89c5d58fc2
Reviewed-Content-Hash: design/gdd/metricas-locales-de-partida.md 4b0beb1c2995defdc1e564cdce82fca557ce2a02
Reviewed-Content-Hash: design/gdd/modificador.md 2dae305bc81b22ff28cd83b7ebbd23b096b98d49
Reviewed-Content-Hash: design/gdd/interfaz-del-modificador.md 283c065061e4d5a48c2d383e0b666dd09b3c1084
Reviewed-Content-Hash: design/gdd/game-concept.md 43e62d80536c71a39bae754ce48356caf6ee6beb
Reviewed-Content-Hash: design/gdd/gdd-cross-review-2026-11-26.md 4e2bcb868717e3e9c62d8959fed5d77a54b22266
Reviewed-Content-Hash: design/art/art-bible.md c49b99572be1dffabb48c5974fc7ee54cdb29f28

## Review — 2026-11-27 — Verdict: NEEDS REVISION
Review mode: full · Workflow: full
Scope signal: M
Specialists: game-designer, systems-designer, qa-lead, ux-designer, unity-specialist, network-programmer, gameplay-programmer, audio-director; senior: creative-director
Blocking items: 26 (7 temas) | Recommended: 30
Summary: Re-revisión del contenido revisado en la primera pasada (hash 6e71847: el receipt anterior se estampó después de los arreglos). De los 15 bloqueantes previos, 9 quedaron resueltos y 6 parciales. Los arreglos no rompieron el diseño, pero abrieron huecos de contrato en tres temas: instantes de evaluación (fin de la placa, comandos del propio Timer, SaltoDetectado en el orden), la vista que consumen HUD y Audio (`corriendo`, `tictac`, jugador activo en la placa, ámbar de nivel 1 sin forma) y los AC. Choque nuevo con la Máquina (W-1): las dos sesiones registraron versiones opuestas de la placa. Decisiones del jugador: **la placa bloquea los comandos de la fase** (vale la versión de la Máquina; cambia #17, #15 y #13) y **guarda 4 estricta** (`aviso_reloj_s > turno_max_s`, rango 91–240). Revisiones aplicadas en la misma sesión; sigue otra pasada.
Prior verdict resolved: Partial (9 de 15)
Findings:
- [BLOCKING] Core Rules 17/18 y States: el fin de la espera de la placa no era un instante evaluable; detectado tarde regalaba tiempo
- [BLOCKING] Core Rules 2/8: los comandos del propio Timer pasaban por la evaluación previa (recursión) y no llevaban la hora del vencimiento
- [BLOCKING] Core Rules 8 y AC-29/AC-32: `SaltoDetectado` sin lugar en el orden; los AC con saltos > 5 s lo omitían
- [BLOCKING] Core Rules 2/7: «nunca la hora del sistema» impedía compensar el sueño del equipo en Web
- [BLOCKING] Core Rules 8/AC-12b: el comando tardío da `ComandoViejo` (fila 10 antes que 13), no `FaseIncorrecta`
- [BLOCKING] Core Rules 9: faltaban `EsperandoTirada` a un TurnTimeout, «cualquier otro código» y el `Mover` con pila 256 de la fila 15
- [BLOCKING] Core Rules 17/Dependencies: el Timer no podía saber que hay Distracción (pasa a recibir `espera_placa_ms`)
- [BLOCKING] Core Rules 17 vs Máquina r14: placa que bloquea vs que no bloquea (decisión: bloquea)
- [BLOCKING] Core Rules 12: «Rey muerto» contado como fin a mitad de turno; `parcial` = sin `TurnoTerminado`
- [BLOCKING] Core Rules 13: `jugadorActivo` del turno cerrado durante la placa; faltaban `corriendo`, `tictac` y los valores quietos de la cosecha
- [BLOCKING] Visual/Audio: el ámbar de nivel 1 que limita perdía la forma (solo color)
- [BLOCKING] Visual/Audio: «estado de tictac» inexistente; el tictac sonaba en el paso a 0
- [BLOCKING] Visual/Audio: `TramoFinalIniciado` caía sobre la cosecha
- [BLOCKING] Player Fantasy: «nunca vence por sorpresa» falso con la página oculta
- [BLOCKING] Tuning Knobs: guarda 4 aceptaba margen cero (decisión: estricta); guardas «para cada modo» ambiguas; faltaba la tabla modo → perillas
- [BLOCKING] Acceptance Criteria: sin config de pruebas (AC-15, 20, 24); AC-8 y AC-22c medían el reloj equivocado; AC-34 vacuo; AC-19 con 1.441 impar; AC-27 interrumpía el juego; sin AC de audio
- [RECOMMENDED] Core Rules 18: «ningún comando puede llegar en el medio» era falso; `delta_efectivo`; varios instantes por evaluación
- [RECOMMENDED] Core Rules 15/17: online (hora al recibir, cola única, ancla de la predicción, gracia de latencia) marcado PROVISIONAL
- [RECOMMENDED] Core Rules 5/12: hora de armado tras un comando; turno sin etapa al terminar la partida
- [RECOMMENDED] Core Rules 7: la `Tirada` local solo con la presentación visible (resultado igual en todas las plataformas)
- [RECOMMENDED] Core Rules 9: `CierreInvalido` con la etapa en curso del mismo jugador trababa la partida
- [RECOMMENDED] Core Rules 4: 64 bits para delta, salto y medición; delta contra la mayor marca vista
- [RECOMMENDED] Formulas D1/Tuning: `dur_mover_normal_s` 79–90 inusable (rango 30–78); guardas 3, 4a, 4c, 5 marcadas como defensa; campo `guarda_N`
- [RECOMMENDED] Formulas D3: salidas sin variables; «hacia abajo»; anomalía en D2
- [RECOMMENDED] UI Requirements: lector de pantalla (los dos relojes a pedido, nivel 1), anticipación del tramo, turno sin fases, Sin límite
- [RECOMMENDED] Visual/Audio: frontera con #22, ráfaga tras atraso, cue de cambio de fase, «apagar el sonido» con Reducir efectos, online/IA
- [RECOMMENDED] Acceptance Criteria: AC-1 sin huella, AC-1b, AC-4, AC-14b/14c, AC-24 en CI y harness, AC-26, AC-35, ubicaciones y compuertas
- [RECOMMENDED] Open Questions: disparadores de ventaja de rojo, punto muerto y congelamiento propio; nivel 1 saliente; aviso de vencimientos atrasados
Reviewed-Content-Hash: design/gdd/timer-de-turno.md 6e71847aa2219f22d202b8e9b0b92460a0dc5bad
Reviewed-Content-Hash: design/registry/entities.yaml 07713dcc52725c7d683477e25e8906d5db2f0c3e
Reviewed-Content-Hash: .claude/skills/design-review/SKILL.md 4dcc8b81162613c757144532f1b72708ab3ba27f
Reviewed-Content-Hash: design/gdd/maquina-de-turno.md bbe86a444be2634587f8b4797c33f2a14bb5a105
Reviewed-Content-Hash: design/gdd/libreria-de-reglas.md 5ac1d08ff674134ee106d55ad965c1784175b26d
Reviewed-Content-Hash: design/gdd/catalogo-de-piezas.md 24c2df9133aaea913698815462cb31d575219d30
Reviewed-Content-Hash: design/gdd/tablero-y-despliegue-inicial.md 9460a55db38e0d1d885bb1e96069d99b1b77e54c
Reviewed-Content-Hash: design/gdd/ficha-de-pieza-y-hud.md 1090b0e1717f0e164d50e925f173f52c4c6f7054
Reviewed-Content-Hash: design/gdd/render-animacion-y-feedback-de-combate.md 84f4bc10496ac137ba64adee2a48ca89c5d58fc2
Reviewed-Content-Hash: design/gdd/metricas-locales-de-partida.md 4b0beb1c2995defdc1e564cdce82fca557ce2a02
Reviewed-Content-Hash: design/gdd/modificador.md 2dae305bc81b22ff28cd83b7ebbd23b096b98d49
Reviewed-Content-Hash: design/gdd/interfaz-del-modificador.md 283c065061e4d5a48c2d383e0b666dd09b3c1084
Reviewed-Content-Hash: design/gdd/entrada-y-seleccion.md 27d538e355bf54759ebc019bc2fc80e667abb0b0
Reviewed-Content-Hash: design/gdd/game-concept.md 43e62d80536c71a39bae754ce48356caf6ee6beb
Reviewed-Content-Hash: design/gdd/gdd-cross-review-2026-11-26.md 4e2bcb868717e3e9c62d8959fed5d77a54b22266
Reviewed-Content-Hash: design/art/art-bible.md c49b99572be1dffabb48c5974fc7ee54cdb29f28

## Review — 2026-11-27 — Verdict: NEEDS REVISION
Review mode: full · Workflow: full
Scope signal: M
Specialists: game-designer, systems-designer, qa-lead, ux-designer, unity-specialist, network-programmer, gameplay-programmer, audio-director; senior: creative-director
Blocking items: 12 | Recommended: ~35
Summary: 3.ª pasada. Casi todos los bloqueantes de la re-revisión quedaron resueltos; el documento converge y no hay problema estructural (creative-director). Los nuevos son de contrato y vienen sobre todo de «la placa bloquea»: el descarte de comandos durante la placa no tenía lista cerrada, respuesta ni regla de registro; controles de #15 sin declarar; turno sin fases; a qué turno pertenece la espera; bucle de la anomalía `CierreInvalido`; validación por modo; config de pruebas; Cambia al Tablero; «visible» y AC-24 en CI; atardecer vs renglón; AC-36 sobre golpes. Decisiones del jugador: `TurnoMedido(n + 1, …, 0, 0, …, parcial)` si la partida termina antes de armar la etapa; «visible» = no minimizada (tapada cuenta como visible; el foco no importa). Revisiones aplicadas en la misma sesión; sigue otra pasada.
Prior verdict resolved: Yes (con PARCIALES menores)
Findings:
- [BLOCKING] Core Rules 17: descarte durante la placa sin lista cerrada, sin respuesta (`PlacaEnCurso`) y sin excluirlo del registro (rompía AC-23)
- [BLOCKING] Cross-References #15: controles del HUD durante la placa sin declarar como Cambia
- [BLOCKING] Core Rules 13: el turno sin fases mostraba el sobrante del rival
- [BLOCKING] Core Rules 12: a qué turno pertenece `EsperandoTirada` en la medición
- [BLOCKING] Core Rules 9/D3: anomalía `CierreInvalido` con la etapa del mismo jugador (bucle de delta 0, 0 con tiempo para actuar)
- [BLOCKING] Formulas D1/Edge: qué se valida en cada modo
- [BLOCKING] Acceptance Criteria: config de pruebas sin modo de guardas ni presencia en el build de desarrollo (AC-19, 24, 27, 35)
- [BLOCKING] Cross-References: Cambia al Tablero (el reloj no cierra un punto muerto rápido)
- [BLOCKING] Core Rules 7: «visible» sin definición por plataforma
- [BLOCKING] Acceptance Criteria: AC-24 no ejecutable en game-ci tal como estaba
- [BLOCKING] Visual/Audio y Cross-References #18: atardecer (grupo 2) vs renglón (placa)
- [BLOCKING] Acceptance Criteria: AC-36 afirmaba golpes que el Timer no produce
- [RECOMMENDED] Core Rules 2/8/15: hora max(lectura, mayor marca), hora de `CerrarPartida(Tablas)`, umbral «en o antes», `espera_placa_ms` validado, online con `etapaPendiente`
- [RECOMMENDED] Core Rules 7/17: turno 1 al tablero visible; pausa extiende la espera
- [RECOMMENDED] Acceptance Criteria: AC-35 válido, AC-27 en 300 s con «me alcanzó», AC-26, AC-28 fronteras, AC-22c ambos relojes, AC-37, AC-38, Cobertura completa
- [RECOMMENDED] Visual/Audio: nivel 2 reemplaza el golpe del 10, un golpe por cuadro, descartes en silencio, eventos de una evaluación en un cuadro, cues con ventana oculta
- [RECOMMENDED] Open Questions: lector de pantalla y placa, Wake Lock, congelamiento al arrastrar la ventana
Reviewed-Content-Hash: design/gdd/timer-de-turno.md bc48b883e5d36b1c0b1af54e48aecd92356d6e65
Reviewed-Content-Hash: design/registry/entities.yaml 07713dcc52725c7d683477e25e8906d5db2f0c3e
Reviewed-Content-Hash: .claude/skills/design-review/SKILL.md 4dcc8b81162613c757144532f1b72708ab3ba27f
Reviewed-Content-Hash: design/gdd/maquina-de-turno.md 63a3d623846f6aca22f53d59c659def352627b8e
Reviewed-Content-Hash: design/gdd/libreria-de-reglas.md 5ac1d08ff674134ee106d55ad965c1784175b26d
Reviewed-Content-Hash: design/gdd/catalogo-de-piezas.md 24c2df9133aaea913698815462cb31d575219d30
Reviewed-Content-Hash: design/gdd/tablero-y-despliegue-inicial.md 9460a55db38e0d1d885bb1e96069d99b1b77e54c
Reviewed-Content-Hash: design/gdd/ficha-de-pieza-y-hud.md 1090b0e1717f0e164d50e925f173f52c4c6f7054
Reviewed-Content-Hash: design/gdd/render-animacion-y-feedback-de-combate.md 84f4bc10496ac137ba64adee2a48ca89c5d58fc2
Reviewed-Content-Hash: design/gdd/metricas-locales-de-partida.md 4b0beb1c2995defdc1e564cdce82fca557ce2a02
Reviewed-Content-Hash: design/gdd/modificador.md 2dae305bc81b22ff28cd83b7ebbd23b096b98d49
Reviewed-Content-Hash: design/gdd/interfaz-del-modificador.md 283c065061e4d5a48c2d383e0b666dd09b3c1084
Reviewed-Content-Hash: design/gdd/entrada-y-seleccion.md 27d538e355bf54759ebc019bc2fc80e667abb0b0
Reviewed-Content-Hash: design/gdd/game-concept.md 43e62d80536c71a39bae754ce48356caf6ee6beb
Reviewed-Content-Hash: design/art/art-bible.md c49b99572be1dffabb48c5974fc7ee54cdb29f28

## Review — 2026-11-27 — Verdict: NEEDS REVISION
Review mode: full · Workflow: full
Scope signal: M
Specialists: game-designer, systems-designer, qa-lead, ux-designer, unity-specialist, network-programmer, gameplay-programmer, audio-director (sin senior: el veredicto lo fijaron el choque con la Máquina y una decisión del jugador; el creative-director había anticipado aprobar sin contradicciones nuevas)
Blocking items: 5 | Recommended: ~30
Summary: 4.ª pasada. Los bloqueantes de la 3.ª quedaron resueltos; game-designer, audio-director y unity-specialist no encontraron bloqueantes. Contradicción nueva con la Máquina: su working copy (re-revisión paralela, 16:47) pasó a «la placa no cuesta reloj ni bloquea; la etapa se arma al terminar la placa o con el primer comando de fase», mientras el Timer decía que bloqueaba. **Decisión del jugador: la placa NO bloquea** (vale para los dos): el Timer revierte la regla 17 (sin `PlacaEnCurso` ni bloqueo de botones; AC-38 retirado) y queda alineado con la Máquina regla 14/AC-44 y con #15 y #17 sin cambiarles eso. Otros bloqueantes de texto: AC-19 («con 30 se acepta» fallaba por la guarda 2 en Variable), `tramo` en `EsperandoTirada`, Cambia §2 del art bible (banner), lista de capturas de AC-25. Revisiones aplicadas en la misma sesión; sigue otra pasada.
Prior verdict resolved: Yes
Findings:
- [BLOCKING] Core Rules 17 / Cross-References: contradicción con la Máquina en disco sobre la placa (decisión: no bloquea; el Timer se alinea)
- [BLOCKING] Acceptance Criteria AC-19: «con 30, se acepta» rompía la guarda 2 en Variable
- [BLOCKING] Core Rules 13: `tramo` y `restanteHastaTramo_s` en `EsperandoTirada` sin definir
- [BLOCKING] Cross-References art bible: §2 sigue con el banner «Tramo final» sin Cambia
- [BLOCKING] Acceptance Criteria AC-25: lista de capturas distinta de la pedida en Cross-References
- [RECOMMENDED] Core Rules 8: avisos en orden cronológico de cruce y al final de la evaluación
- [RECOMMENDED] Core Rules 9: hora y medición del `TurnTimeout` inmediato de la anomalía
- [RECOMMENDED] Core Rules 12: `relojInicio_ms` = 0; `LimitePartida` a un `TurnTimeout` cuenta como vencida
- [RECOMMENDED] Core Rules 7/Edge: compensación del sueño `max(0, Δpared − Δmonotónica)`; «tapada» según la plataforma; App Nap/minimizada NO VERIFICADO
- [RECOMMENDED] Visual/Audio: primer golpe al armar con tictac ya verdadero; prioridad del vencimiento; AC de golpes pedido a #22
- [RECOMMENDED] Acceptance Criteria: AC-1b (analizador filtrado), AC-12/12b, AC-14b, AC-16, AC-24 (validez, guion, timeout), AC-26, AC-27 (Fijo 90), AC-33, AC-35, AC-36, AC-37
- [RECOMMENDED] Open Questions/Edge: disparador del punto muerto con `vidaQuitada` acumulada; cuenta de 40 min a 2,5 h
Reviewed-Content-Hash note: el hash del contenido revisado en esta pasada no se capturó antes de aplicar los arreglos; se omite la línea del GDD a propósito para que la próxima pasada no lo dé por «sin cambios».
Reviewed-Content-Hash: design/registry/entities.yaml 07713dcc52725c7d683477e25e8906d5db2f0c3e
Reviewed-Content-Hash: .claude/skills/design-review/SKILL.md 4dcc8b81162613c757144532f1b72708ab3ba27f
Reviewed-Content-Hash: design/gdd/maquina-de-turno.md f5477a252f24cfbaeda8c7588f57131ed1bd15a3
Reviewed-Content-Hash: design/gdd/libreria-de-reglas.md 5ac1d08ff674134ee106d55ad965c1784175b26d
Reviewed-Content-Hash: design/gdd/catalogo-de-piezas.md 24c2df9133aaea913698815462cb31d575219d30
Reviewed-Content-Hash: design/gdd/tablero-y-despliegue-inicial.md 9460a55db38e0d1d885bb1e96069d99b1b77e54c
Reviewed-Content-Hash: design/gdd/ficha-de-pieza-y-hud.md 1090b0e1717f0e164d50e925f173f52c4c6f7054
Reviewed-Content-Hash: design/gdd/render-animacion-y-feedback-de-combate.md 84f4bc10496ac137ba64adee2a48ca89c5d58fc2
Reviewed-Content-Hash: design/gdd/metricas-locales-de-partida.md 4b0beb1c2995defdc1e564cdce82fca557ce2a02
Reviewed-Content-Hash: design/gdd/modificador.md 2dae305bc81b22ff28cd83b7ebbd23b096b98d49
Reviewed-Content-Hash: design/gdd/interfaz-del-modificador.md 283c065061e4d5a48c2d383e0b666dd09b3c1084
Reviewed-Content-Hash: design/gdd/entrada-y-seleccion.md 27d538e355bf54759ebc019bc2fc80e667abb0b0
Reviewed-Content-Hash: design/gdd/game-concept.md 43e62d80536c71a39bae754ce48356caf6ee6beb
Reviewed-Content-Hash: design/art/art-bible.md c49b99572be1dffabb48c5974fc7ee54cdb29f28

## Review — 2026-11-27 — Verdict: NEEDS REVISION
Review mode: full · Workflow: full
Scope signal: M
Specialists: game-designer, systems-designer, qa-lead, ux-designer, unity-specialist, network-programmer, gameplay-programmer, audio-director (sin senior; los bloqueantes eran de texto y coincidentes)
Blocking items: 3 | Recommended: ~20
Summary: 5.ª pasada, sobre la reversión de W-1 (la placa no bloquea). Sin restos del modelo con bloqueo que cambien el comportamiento; coincide con la Máquina (regla 14, AC-44). game-designer, audio-director y unity-specialist sin bloqueantes. Quedaban tres huecos del armado por comando: cuándo salen `EtapaIniciada` y los avisos si el comando se acepta o se rechaza; `Rendirse` durante la placa con el reloj del turno en 0 (rompía el invariante de la Máquina); y el texto que daba por hecho «placa = etapa quieta» (tictac, F3 de #15). Revisiones aplicadas; el jugador eligió una pasada final enfocada (systems, qa, gameplay, ux + creative-director).
Prior verdict resolved: Yes
Findings:
- [BLOCKING] Core Rules 8/12/17: eventos del armado por comando (solo con el Aceptado; nada con un rechazo; regla 5 si cambia el par)
- [BLOCKING] Core Rules 17: con el reloj del turno en 0 y la etapa pendiente, cualquier comando (también `Rendirse`) dispara primero `RelojAgotado`
- [BLOCKING] Visual/Audio y Cross-References #15: tictac y ámbar atados a `etapaPendiente`, no a «placa en pantalla»
- [RECOMMENDED] Core Rules 17: «comando de fase» definido; emisor en las entradas; turno 1 armado por comando
- [RECOMMENDED] Cross-References: #17 reglas 2 y 4, l.580 y AC-5 (toques sobre la placa) y #13 reglas 12 y 15 como Cambia; Máquina AC-40(h); fila del `TurnTimeout` de la anomalía como Usa
- [RECOMMENDED] Acceptance Criteria: AC-28 (eventos aceptado/rechazado, TerminarMovimiento, reloj en 0, Rendirse, pausa, t + 1.700, turno 1), AC-29 caso 3 completo, AC-36 en la placa, Cobertura
- [RECOMMENDED] Edge Cases: cuenta del punto muerto con un solo supuesto
Reviewed-Content-Hash: design/gdd/timer-de-turno.md 2dcdc3c15f7aee9ebb74b4fcd1e1d59c7bbb4d86
Reviewed-Content-Hash: design/registry/entities.yaml 07713dcc52725c7d683477e25e8906d5db2f0c3e
Reviewed-Content-Hash: .claude/skills/design-review/SKILL.md 4dcc8b81162613c757144532f1b72708ab3ba27f
Reviewed-Content-Hash: design/gdd/maquina-de-turno.md f5477a252f24cfbaeda8c7588f57131ed1bd15a3
Reviewed-Content-Hash: design/gdd/libreria-de-reglas.md 5ac1d08ff674134ee106d55ad965c1784175b26d
Reviewed-Content-Hash: design/gdd/catalogo-de-piezas.md 24c2df9133aaea913698815462cb31d575219d30
Reviewed-Content-Hash: design/gdd/tablero-y-despliegue-inicial.md 9460a55db38e0d1d885bb1e96069d99b1b77e54c
Reviewed-Content-Hash: design/gdd/ficha-de-pieza-y-hud.md 1090b0e1717f0e164d50e925f173f52c4c6f7054
Reviewed-Content-Hash: design/gdd/render-animacion-y-feedback-de-combate.md 84f4bc10496ac137ba64adee2a48ca89c5d58fc2
Reviewed-Content-Hash: design/gdd/metricas-locales-de-partida.md 4b0beb1c2995defdc1e564cdce82fca557ce2a02
Reviewed-Content-Hash: design/gdd/modificador.md 2dae305bc81b22ff28cd83b7ebbd23b096b98d49
Reviewed-Content-Hash: design/gdd/interfaz-del-modificador.md 283c065061e4d5a48c2d383e0b666dd09b3c1084
Reviewed-Content-Hash: design/gdd/entrada-y-seleccion.md 27d538e355bf54759ebc019bc2fc80e667abb0b0
Reviewed-Content-Hash: design/gdd/game-concept.md 43e62d80536c71a39bae754ce48356caf6ee6beb
Reviewed-Content-Hash: design/art/art-bible.md c49b99572be1dffabb48c5974fc7ee54cdb29f28

## Review — 2026-11-27 — Verdict: APPROVED
Review mode: full · Workflow: full
Scope signal: M
Specialists: systems-designer, qa-lead, gameplay-programmer, ux-designer (pasada final enfocada, elegida por el jugador); senior: creative-director
Blocking items: 0 | Recommended: ~12 (aplicados antes del veredicto)
Summary: Pasada final. Los cuatro especialistas verificaron como resueltos los 3 bloqueantes de la 5.ª pasada (eventos del armado por comando, comandos con el reloj en 0 durante la placa, tictac/ámbar atados a `etapaPendiente`) y no encontraron bloqueantes nuevos. Criterio de veredicto del usuario: frenan solo los defectos propios o un choque con un Approved (#1, #2, #4); los contratos con vecinos no aprobados van a la nota de cierre. El creative-director confirmó que no rompe a la Librería (#1) y que el choque con el Tablero (#4, punto muerto) está declarado como Cambia. Se aplicaron aclaraciones menores (`EtapaIniciada` y avisos con el reloj en 0, AC-20/28/36, Cobertura, States, D3).
Prior verdict resolved: Yes
Findings:
- [RECOMMENDED] Core Rules 9/12/17 y AC-20: sin `EtapaIniciada` ni avisos cuando la etapa se cierra al armarse con el reloj en 0 (aplicado)
- [RECOMMENDED] States: la fila `EnEspera → Corriendo` nombra cualquier comando con el reloj en 0 (aplicado)
- [RECOMMENDED] Acceptance Criteria: AC-28 (PartidaTerminada, Rendirse propio, TerminarMovimiento con Atacar saltado), AC-36, Cobertura (aplicado)
- [RECOMMENDED] Formulas D3: vista vs política para `vinculante` y `vence_en_ms` con la etapa pendiente (aplicado)
- Para el cierre (no frenan): Máquina regla 14 «cualquier comando de jugador» con el reloj en 0 y AC-44 acotado a la fase abierta; Tablero Edge/OQ del punto muerto; #17, #13, #15, #18, #19, art bible y game-concept (ver nota de pendientes); ADR del Timer (orden de eventos de un mismo Aceptado, cola sin reentrada, visibilidad, adaptador de la fuente, harness, analizador).
Reviewed-Content-Hash: design/gdd/timer-de-turno.md 4c19f61f268d86dfdbce590a684e22b41acdc60d
Reviewed-Content-Hash: design/registry/entities.yaml 07713dcc52725c7d683477e25e8906d5db2f0c3e
Reviewed-Content-Hash: .claude/skills/design-review/SKILL.md 4dcc8b81162613c757144532f1b72708ab3ba27f
Reviewed-Content-Hash: design/gdd/maquina-de-turno.md f5477a252f24cfbaeda8c7588f57131ed1bd15a3
Reviewed-Content-Hash: design/gdd/libreria-de-reglas.md 5ac1d08ff674134ee106d55ad965c1784175b26d
Reviewed-Content-Hash: design/gdd/catalogo-de-piezas.md 24c2df9133aaea913698815462cb31d575219d30
Reviewed-Content-Hash: design/gdd/tablero-y-despliegue-inicial.md 9460a55db38e0d1d885bb1e96069d99b1b77e54c
Reviewed-Content-Hash: design/gdd/ficha-de-pieza-y-hud.md 1090b0e1717f0e164d50e925f173f52c4c6f7054
Reviewed-Content-Hash: design/gdd/render-animacion-y-feedback-de-combate.md 84f4bc10496ac137ba64adee2a48ca89c5d58fc2
Reviewed-Content-Hash: design/gdd/metricas-locales-de-partida.md 4b0beb1c2995defdc1e564cdce82fca557ce2a02
Reviewed-Content-Hash: design/gdd/modificador.md 2dae305bc81b22ff28cd83b7ebbd23b096b98d49
Reviewed-Content-Hash: design/gdd/interfaz-del-modificador.md 283c065061e4d5a48c2d383e0b666dd09b3c1084
Reviewed-Content-Hash: design/gdd/entrada-y-seleccion.md 27d538e355bf54759ebc019bc2fc80e667abb0b0
Reviewed-Content-Hash: design/gdd/game-concept.md 43e62d80536c71a39bae754ce48356caf6ee6beb
Reviewed-Content-Hash: design/art/art-bible.md c49b99572be1dffabb48c5974fc7ee54cdb29f28

## Cierre de la tanda B — 2026-11-28 (no es una pasada de revisión)
La sesión de cierre aplicó la redacción que pidió la Máquina: reglas 9 y 17 (el acote del invariante del reloj), l.215, Visual/Audio («Elegido por el tiempo · sin mover», `fin.RelojAgotado`), dos filas de Cross-References (`EsperandoTirada` benigna; la Máquina se alinea al rango 1.290–1.500) y el espejo de la Máquina AC-26 en AC-19. No se aplicaron, porque los anuló la decisión del jugador sobre `T_tope`: el piso 1.442, «pares» y el rechazo de impares. El Status sigue Approved.
