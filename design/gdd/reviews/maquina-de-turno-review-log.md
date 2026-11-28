# Review log — Máquina de turno y fin de partida (`design/gdd/maquina-de-turno.md`)

## Review — 2026-11-27 — Verdict: NEEDS REVISION
Review mode: full · Workflow: full
Scope signal: L
Specialists: game-designer, systems-designer, qa-lead, ux-designer, network-programmer, lead-programmer, creative-director (senior)
Blocking items: 7 | Recommended: 12
Summary: El diseño es sano (fases, dos relojes, cascada y clausura encajan con los pilares) y no cambia la identidad del juego, pero la especificación no era implementable ni testeable: faltaba el orden canónico de los eventos dentro de un Aplicar, D1 y D4 se rompían en los bordes, la carrera Resolver/timeout estaba mal descrita, varios AC tenían GIVEN imposibles y la fantasía contradecía la regla 14. Todo se corrigió dentro del GDD en la misma sesión.
Prior verdict resolved: First review
Findings:
- [BLOCKING] Core Rules r16: sin orden de eventos por comando ni regla de emisión de `FaseIniciada`; "hasta 5 eventos" mal contado (real: 6).
- [BLOCKING] Player Fantasy vs r14: prometía "lo asignado se resuelve" y `RelojAgotado` descarta los ataques asignados.
- [BLOCKING] Formulas D1: `seg_medio` podía ser 0 (división por cero); sin sentido en Sin límite; rangos mal.
- [BLOCKING] Formulas D4: invariante y tope de 37 válidos solo para el Catálogo v5; faltaba la cota con el Grito que valida CrearPartida; `ConfigInvalida` sin código (F8).
- [BLOCKING] Edge Cases (carreras): "el orden lo fija contadorComandos" falso; faltaba Resolver vs `TurnTimeout` de atacar → `EsperandoTirada`.
- [BLOCKING] Contrato: recluta automático sin "de la cara"; límite del turno sin fila en la tabla de dominios; gancho del Modificador sin orden; filas de clausura no listadas; dependientes sin #16/#17/#18 y #15/#19 con fuerza equivocada (W-21).
- [BLOCKING] Acceptance Criteria: AC-4, AC-8, AC-14 y AC-23 imposibles; AC-1/6/22 con `ReclutaPerdido` inalcanzable; AC-15 con orden equivocado; AC-30 no verificable con una captura; sin AC de campos de `PartidaTerminada`, Tablas automáticas, `CerrarPartida`, `Terminada`.
- [RECOMMENDED] Core Rules r14: invariante "ningún comando de jugador con reloj ≤ 0" para toda autoridad; latencia y Tirada online con tope (#28).
- [RECOMMENDED] Core Rules r5: Arremetida con `piezas_sin_rey > limite_normal`; r6/r10: Reclutar vacío no emite `FaseSaltada`; Distracción no emite `FaseSaltada(Mover)`.
- [RECOMMENDED] Core Rules r17: matriz sin precondiciones, sin columna Terminada, sin excepción de RelojAgotado en EsperandoTirada.
- [RECOMMENDED] Formulas D1: jugadores asimétricos; `N_base` es cota inferior (comparación en un solo sentido).
- [RECOMMENDED] Formulas D2: la compuerta de #19 compara pared con `T_tope` y la pared incluye esperas.
- [RECOMMENDED] Tuning Knobs: alias `T_tope` = `tope_partida_s`; piso 1.290 sin fundamento.
- [RECOMMENDED] Visual/Audio: filas de Tablas (placa neutra) y DobleRey (explicación); una sola placa central por cascada.
- [RECOMMENDED] UI Requirements: W-3 (estado presentado en la cosecha local), aviso de reloj vinculante, recluta automático con tipo marcado, `RendirseDisponible` con el Rey.
- [RECOMMENDED] Edge Cases: "pierde azul" solo con tiradas iguales; pila en 256 alcanzable (lo inalcanzable es un Mover aceptado ahí).
- [RECOMMENDED] Game Feel: "Terminar sin atacar"; Entrada ignora autorrepetición.
- [RECOMMENDED] Acceptance Criteria: AC experiencial del reloj (≤ 20% por RelojAgotado; GD-B1, bajado de bloqueante por el creative-director).
- [RECOMMENDED] Open Questions: DW-3, DW-4, timeout en Reclutar, pedidos a #28, reloj que premia a quien hace menos, lectura de leyendas.

Decisiones del jugador (2026-11-27):
- RelojAgotado con ataques asignados: se mantiene la regla (no se resuelven); se corrige la fantasía, aviso en el HUD y lista en la pantalla final.
- W-1: la etapa y el reloj arrancan al terminar la placa de la cara; comandos de la fase bloqueados durante la placa (contradice hoy a #15 Edge Cases y #17 r4/r6: cambio para el cierre de la tanda).
- DW-3: medir primero; arreglo preferido anotado (nadie tira en su primer turno y el turno 1 de azul no gasta reloj).
- Desacuerdos resueltos por el creative-director: Avanzar sin confirmación con etiqueta "Terminar sin atacar"; timeout en Reclutar por UI ahora, ventana propia tras playtest; GD-B1 no bloqueante.

Revisión aplicada en la misma sesión (2026-11-27, con OK del jugador): todos los bloqueantes y recomendados del changeset. Status del GDD → In Review. Re-revisión: nueva sesión.

Receipt note: el GDD objetivo **no se estampa** porque se modificó después de la revisión; la próxima corrida lo ve como NEW y hace la revisión completa.
Reviewed-Content-Hash: design/registry/entities.yaml 07713dcc52725c7d683477e25e8906d5db2f0c3e
Reviewed-Content-Hash: .claude/skills/design-review/SKILL.md 4dcc8b81162613c757144532f1b72708ab3ba27f
Reviewed-Content-Hash: design/gdd/libreria-de-reglas.md 5ac1d08ff674134ee106d55ad965c1784175b26d
Reviewed-Content-Hash: design/gdd/catalogo-de-piezas.md 24c2df9133aaea913698815462cb31d575219d30
Reviewed-Content-Hash: design/gdd/tablero-y-despliegue-inicial.md 9460a55db38e0d1d885bb1e96069d99b1b77e54c
Reviewed-Content-Hash: design/gdd/movimiento.md 679bca4fed148d8ebd3f599a15a6acbe6ce44bda
Reviewed-Content-Hash: design/gdd/reclutamiento-y-limites.md b0a1c65376fa3da8052670ef3e24d224c2d30ad9
Reviewed-Content-Hash: design/gdd/resolucion-simultanea-del-dano.md 52eb62677e85d84ecae345b930e37d123b9a4673
Reviewed-Content-Hash: design/gdd/ficha-de-pieza-y-hud.md 1090b0e1717f0e164d50e925f173f52c4c6f7054
Reviewed-Content-Hash: design/gdd/vista-previa-de-ataques.md 9f2b8c1e1276a36f2d00c39eeddc5149a7e341c9
Reviewed-Content-Hash: design/gdd/render-animacion-y-feedback-de-combate.md 84f4bc10496ac137ba64adee2a48ca89c5d58fc2
Reviewed-Content-Hash: design/gdd/timer-de-turno.md 12990c1a2f5696888720320b5a89a6ee5b8c0db9
Reviewed-Content-Hash: design/gdd/metricas-locales-de-partida.md 4b0beb1c2995defdc1e564cdce82fca557ce2a02
Reviewed-Content-Hash: design/gdd/modificador.md 2dae305bc81b22ff28cd83b7ebbd23b096b98d49
Reviewed-Content-Hash: design/gdd/interfaz-del-modificador.md 283c065061e4d5a48c2d383e0b666dd09b3c1084
Reviewed-Content-Hash: design/gdd/cobertura-del-escudero.md 1398f6027b8caa40188f5f9264d8fd5c25be6b81
Reviewed-Content-Hash: design/gdd/game-concept.md 43e62d80536c71a39bae754ce48356caf6ee6beb
Reviewed-Content-Hash: design/gdd/gdd-cross-review-2026-11-26.md 4e2bcb868717e3e9c62d8959fed5d77a54b22266

## Review — 2026-11-27 — Verdict: NEEDS REVISION
Review mode: full · Workflow: full
Scope signal: L
Specialists: game-designer, systems-designer, qa-lead, ux-designer, network-programmer, lead-programmer, creative-director (senior)
Blocking items: 9 | Recommended: 30
Summary: Re-revisión (2.ª pasada). Los 7 bloqueantes de la 1.ª pasada se sostienen (orden de eventos, D4, contrato, matriz, AC), pero el Timer revisado en paralelo dejó desfasados la placa de la cara (dos registros de W-1 opuestos con la misma fecha), el piso de `T_tope` y los rangos de D1; además faltaban el estado en `Terminada`, las dos listas de carreras y la convención del doble del Modificador. Panel unánime NEEDS REVISION; el creative-director confirma, sin revisión mayor. Todo se corrigió dentro del GDD en la misma sesión.
Prior verdict resolved: Yes (los 7 de la 1.ª pasada; D1 y carreras se reabrieron por los desfasajes y quedaron corregidos)
Findings:
- [BLOCKING] Core Rules r14: placa bloqueaba comandos; contradice Timer r17, #13, #15, #17; el núcleo no puede hacerlo cumplir; faltaba la espera en caras de reclutar; AC-40 no probaba nada.
- [BLOCKING] Core Rules r14: hueco del invariante del reloj con la etapa pendiente (comando aplicado con reloj en 0); «hora de recepción».
- [BLOCKING] Tuning Knobs / D1 / D2 / AC-25 / AC-26: piso de `T_tope` 1.290 inválido frente a la guarda 1 del Timer (≥ 1.442); rangos con `turno_max_s` 45–125.
- [BLOCKING] Formulas D1: «ritmos distintos» sin definir y con división por cero (N_J = 0); ejemplo 24 vs «unos 25».
- [BLOCKING] Core Rules r16: estado y campos de `PartidaTerminada` en `Terminada` sin definir por motivo.
- [BLOCKING] Edge Cases (carreras): «llega tarde → ComandoViejo» falso en la carrera más común; mezcla de descartes de autoridad y rechazos del jugador.
- [BLOCKING] Acceptance Criteria (convenciones): el doble del Modificador y la cara daban dos fuentes de verdad (AC-5/6/15/22/28).
- [BLOCKING] UI Requirements: aviso de reloj sin disparo, clave ni AC.
- [BLOCKING] Player Fantasy / r14: «cada etapa que usás entera lo gasta» falso; 3,5 s vs 3,2 s.
- [RECOMMENDED] Core Rules r5: `LimiteDelTurno` sin nombre (Movimiento dice que se guarda); gancho sin efectos de la Tirada; turno 1 sin `TurnoIniciado` para consumidores.
- [RECOMMENDED] States and Transitions: filas de salto con origen equivocado y faltantes.
- [RECOMMENDED] Dependencies: interfaces consumidas sin listar; #2 debía ser Dura.
- [RECOMMENDED] Formulas D4: variables faltantes, suma sobre multiconjunto, código de `cota_grito`, 37 como tope duro; r15 techo de `C_MAX`; `T_tope` impar.
- [RECOMMENDED] Edge Cases: Rendirse vs reloj, Reclutar seguido de timeout de mover, Sin límite y `C_MAX` solo con comandos.
- [RECOMMENDED] Visual/Audio / UI: bandos Hueso/Ciruela, «Terminar sin atacar», premarcado del recluta, textos `fin.*`, lector de pantalla, Distracción 1,0/1,5 s.
- [RECOMMENDED] Cross-References: pedidos de la 1.ª pasada sin fila «Pide».
- [RECOMMENDED] Acceptance Criteria: AC-4, 5, 8, 13, 14, 15, 17, 19, 20, 23 (80 celdas), 30, 31, 34, 35, 36, 37, 39, 41, 43; faltaban comando con reloj en 0, pantalla de resultado y DobleRey por timeout.
- [RECOMMENDED] Open Questions: umbral 60 → 120 s; pedidos a #28 (gracia, `listo_max_s`).

Decisiones del jugador (2026-11-27, 2.ª pasada):
- **Placa (W-1): vale el modelo del Timer (regla 17)** — la placa no bloquea; la etapa y el reloj se arman al terminar la placa o con el primer comando de fase aceptado, lo que pase antes; los toques dentro del rectángulo de la placa no pasan al tablero. **Reemplaza** el «comandos de la fase bloqueados durante la placa» que registró la entrada anterior de este log.
- **Piso de `T_tope` = 1.442** (rango 1.442–1.500, pares).
- Guarda de ~250 ms en hotseat: no como regla (creative-director); queda como Open Question de #13.

Revisión aplicada en la misma sesión (2026-11-27, con OK del jugador): los 9 bloqueantes y los recomendados y menores del changeset; agregados AC-44, AC-45 y AC-46. Status del GDD: In Review. Re-revisión (3.ª pasada): sesión nueva. Pendientes para otros archivos: notas compartidas de la tanda B (`maquina-2a-pasada-pendientes.md`).

Receipt note: el GDD objetivo **no se estampa** porque se modificó después de la revisión. `timer-de-turno.md` cambió después de su propia entrada de log (sigue en revisión paralela): la 3.ª pasada debe volver a cotejar la regla 17 y las guardas.
Reviewed-Content-Hash: design/registry/entities.yaml 07713dcc52725c7d683477e25e8906d5db2f0c3e
Reviewed-Content-Hash: .claude/skills/design-review/SKILL.md 4dcc8b81162613c757144532f1b72708ab3ba27f
Reviewed-Content-Hash: design/gdd/libreria-de-reglas.md 5ac1d08ff674134ee106d55ad965c1784175b26d
Reviewed-Content-Hash: design/gdd/catalogo-de-piezas.md 24c2df9133aaea913698815462cb31d575219d30
Reviewed-Content-Hash: design/gdd/tablero-y-despliegue-inicial.md 9460a55db38e0d1d885bb1e96069d99b1b77e54c
Reviewed-Content-Hash: design/gdd/movimiento.md 679bca4fed148d8ebd3f599a15a6acbe6ce44bda
Reviewed-Content-Hash: design/gdd/reclutamiento-y-limites.md b0a1c65376fa3da8052670ef3e24d224c2d30ad9
Reviewed-Content-Hash: design/gdd/cobertura-del-escudero.md 1398f6027b8caa40188f5f9264d8fd5c25be6b81
Reviewed-Content-Hash: design/gdd/modificador.md 2dae305bc81b22ff28cd83b7ebbd23b096b98d49
Reviewed-Content-Hash: design/gdd/resolucion-simultanea-del-dano.md 52eb62677e85d84ecae345b930e37d123b9a4673
Reviewed-Content-Hash: design/gdd/ficha-de-pieza-y-hud.md 1090b0e1717f0e164d50e925f173f52c4c6f7054
Reviewed-Content-Hash: design/gdd/vista-previa-de-ataques.md 9f2b8c1e1276a36f2d00c39eeddc5149a7e341c9
Reviewed-Content-Hash: design/gdd/interfaz-del-modificador.md 283c065061e4d5a48c2d383e0b666dd09b3c1084
Reviewed-Content-Hash: design/gdd/render-animacion-y-feedback-de-combate.md 84f4bc10496ac137ba64adee2a48ca89c5d58fc2
Reviewed-Content-Hash: design/gdd/entrada-y-seleccion.md 27d538e355bf54759ebc019bc2fc80e667abb0b0
Reviewed-Content-Hash: design/gdd/timer-de-turno.md 90950e71355be8361f9a8dd5c8261a3818a566c3
Reviewed-Content-Hash: design/gdd/metricas-locales-de-partida.md 4b0beb1c2995defdc1e564cdce82fca557ce2a02
Reviewed-Content-Hash: design/gdd/game-concept.md 43e62d80536c71a39bae754ce48356caf6ee6beb
Reviewed-Content-Hash: design/art/art-bible.md c49b99572be1dffabb48c5974fc7ee54cdb29f28

## Review — 2026-11-27 — Verdict: NEEDS REVISION
Review mode: full · Workflow: full
Scope signal: L
Specialists: game-designer, systems-designer, qa-lead, ux-designer, network-programmer, lead-programmer, creative-director (senior)
Blocking items: 14 | Recommended: ~20 | Contratos con vecinos no aprobados: a pendientes del cierre
Summary: 3.ª pasada con el criterio de dos clases del jugador (bloqueante propio frente a contrato con un vecino no aprobado). Los bloqueantes de la 2.ª pasada se sostienen. Los nuevos son de texto y de AC. El Timer revisado en paralelo cambió el armado del turno 1 y el de los comandos con el reloj en 0. D1 asimétrica tiene dos cifras mal. A la matriz le falta la forma válida de `CerrarPartida`, y cuatro AC no discriminan. El creative-director: NEEDS REVISION acotado, sin rediseño, sin revisión mayor. El jugador decidió parar acá y revisar en otra sesión.
Prior verdict resolved: Yes (los 9 de la 2.ª pasada)
Findings:
- [BLOCKING] Core Rules r3 / AC-40(h): la etapa del turno 1 «se arma al crear la partida» contradice al Timer r7/r17 (tablero visible por primera vez o primer comando de fase de azul; online, los dos listos o `listo_max_s`) y a la regla 2.
- [BLOCKING] Core Rules r14 / Edge l.654 / AC-44: con el reloj en 0 y la etapa pendiente, cualquier comando de jugador (Rendirse de cualquiera, también fuera de turno) arma la etapa y sale `RelojAgotado`; hoy los dos textos se contradicen y el ganador se invierte. AC-44 necesita el caso pendiente con Rendirse del rival y el caso armada.
- [BLOCKING] Formulas D1 asimétrica: el mínimo es 16, no 14 (2×⌊721÷90⌋ = 2×⌊750÷93⌋).
- [BLOCKING] Formulas D1 asimétrica: le falta un turno cuando el más lento es rojo → `2k + [el más lento es el jugador 1]` (azul 30, rojo 60 → 25); ejemplo y caso espejo en AC-25.
- [BLOCKING] Core Rules r17 (matriz y convención l.948): falta la forma válida de `CerrarPartida` (fila 9); `contadorEsperado` solo en los comandos que lo llevan; Rendirse de cualquier emisor; en `Terminada` gana la fila 2; falta la fila 4 (SoloAutoridad/SoloJugador).
- [BLOCKING] AC-23: Rendirse desde los dos emisores; `Terminada` en Atacar (ReyMuerto) y en Mover (Rendirse); contador viejo en `Terminada`; `SoloAutoridad`.
- [BLOCKING] AC-8: `usadosRestaurados` con valores (B,b1,b2,0), (A,a2,a3,1), (A,a1,a2,0), suponiendo que un paso cuesta 1.
- [BLOCKING] AC-41: fixtures del borde: 256 = Rey en movimiento 8 (F8 256, F9 10, cota 256); 257 = catapulta en 1 y Rey en 9 (F8 249, F9 11, cota 257).
- [BLOCKING] Formulas D4: el orden de validación está mal citado frente a la Librería r2 (Approved): la pila con bono va después de `limite_normal` y `tablaReclutas` y antes de T6; `PilaGrito` es un nombre propuesto del campo (lo fija el ADR).
- [BLOCKING] Core Rules r14 (online): invariante sobre el vencimiento efectivo (vencimiento + g, g = 0 en local), consumo cobrado hasta 0; «no paga la latencia» → «paga la latencia de subida que exceda g».
- [BLOCKING] Visual/Audio l.795: en el turno 1, el contorno y el banner salen del estado inicial (no hay `TurnoIniciado`).
- [BLOCKING] Visual/Audio l.812–814: el contorno pasa al aceptarse la `Tirada` o al terminar la cosecha o la leyenda del turno cerrado, nunca después de la placa del turno nuevo.
- [BLOCKING] AC-30: caso negativo (con vinculante = Etapa no aparece el aviso de reloj).
- [BLOCKING] AC-45: en hotseat, el nombre del bando ganador en cada captura.
- [RECOMMENDED] Summary/Overview «25 minutos» → «25:00 de reloj»; carpeta de Integration y tipos de AC-29/AC-43; premisa de D4 (Grito y Arremetida nunca juntos; 376); r15 `n_tramos`; N de D1 (Rendirse frente a Tablas en `EsperandoTirada`); etapa en `Terminada`; `etapaPendiente` es del Timer; el gancho del Modificador recibe la cara; AC-33 con eventos enumerados; AC-36 Logic; Edge del reloj en 0 con un turno sin fases; «ninguna orden válida se traga» → «ningún comando aceptable se rechaza»; decisiones abiertas del game-designer (timeout en Reclutar, umbral de AC-43, N de DW-3) a Open Questions.

Decisiones del jugador (2026-11-27, 3.ª pasada):
- **Rendirse con el reloj en 0 y la etapa pendiente: seguir al Timer (r17).** Cualquier comando de jugador, `Rendirse` de cualquiera incluido, arma la etapa a su hora y el vencimiento va antes: gana el rival por `RelojAgotado`.
- Parar acá: los 14 bloqueantes se aplican en una sesión nueva y después va una re-revisión completa (la skill no admite una parcial, aunque el creative-director propuso una enfocada).

Desacuerdos resueltos: `PilaGrito` es válido (Librería r2: «o el del campo»); #4 Blanda no choca (Tablero l.293 declara a #3 Blanda).
Pendientes para otros archivos: nota compartida `maquina-pendientes-cierre-tanda-B.md`.

Reviewed-Content-Hash: design/gdd/maquina-de-turno.md f5477a252f24cfbaeda8c7588f57131ed1bd15a3
Reviewed-Content-Hash: design/registry/entities.yaml 07713dcc52725c7d683477e25e8906d5db2f0c3e
Reviewed-Content-Hash: .claude/skills/design-review/SKILL.md 4dcc8b81162613c757144532f1b72708ab3ba27f
Reviewed-Content-Hash: design/gdd/libreria-de-reglas.md 5ac1d08ff674134ee106d55ad965c1784175b26d
Reviewed-Content-Hash: design/gdd/catalogo-de-piezas.md 24c2df9133aaea913698815462cb31d575219d30
Reviewed-Content-Hash: design/gdd/tablero-y-despliegue-inicial.md 9460a55db38e0d1d885bb1e96069d99b1b77e54c
Reviewed-Content-Hash: design/gdd/timer-de-turno.md 4c19f61f268d86dfdbce590a684e22b41acdc60d
Reviewed-Content-Hash: design/gdd/movimiento.md 679bca4fed148d8ebd3f599a15a6acbe6ce44bda
Reviewed-Content-Hash: design/gdd/reclutamiento-y-limites.md b0a1c65376fa3da8052670ef3e24d224c2d30ad9
Reviewed-Content-Hash: design/gdd/cobertura-del-escudero.md 1398f6027b8caa40188f5f9264d8fd5c25be6b81
Reviewed-Content-Hash: design/gdd/modificador.md 2dae305bc81b22ff28cd83b7ebbd23b096b98d49
Reviewed-Content-Hash: design/gdd/interfaz-del-modificador.md 283c065061e4d5a48c2d383e0b666dd09b3c1084
Reviewed-Content-Hash: design/gdd/ficha-de-pieza-y-hud.md 1090b0e1717f0e164d50e925f173f52c4c6f7054
Reviewed-Content-Hash: design/gdd/entrada-y-seleccion.md 27d538e355bf54759ebc019bc2fc80e667abb0b0
Reviewed-Content-Hash: design/gdd/metricas-locales-de-partida.md 4b0beb1c2995defdc1e564cdce82fca557ce2a02
Reviewed-Content-Hash: design/gdd/render-animacion-y-feedback-de-combate.md 84f4bc10496ac137ba64adee2a48ca89c5d58fc2
Reviewed-Content-Hash: design/gdd/castillo-y-muros.md a1f9f5217874a1c9a47ea748ebb6e8e685f8daa3
Reviewed-Content-Hash: design/gdd/game-concept.md 43e62d80536c71a39bae754ce48356caf6ee6beb
Reviewed-Content-Hash: design/art/art-bible.md c49b99572be1dffabb48c5974fc7ee54cdb29f28

## Review — 2026-11-27 — Verdict: NEEDS REVISION
Review mode: full · Workflow: full
Scope signal: L
Specialists: game-designer, systems-designer, qa-lead, ux-designer, network-programmer, lead-programmer, creative-director (senior)
Blocking items: 13 | Recommended: ~30 | Contratos con vecinos no aprobados: a pendientes del cierre
Summary: 4.ª pasada, después de aplicar los 14 bloqueantes de la 3.ª (todos se sostienen). Los seis especialistas y el creative-director dan NEEDS REVISION acotado, sin rediseño. Dos fórmulas cuentan mal (D1 con ritmos cercanos y empates exactos). Hay dos choques con GDD Approved: el piso de `T_tope` contra el Timer y el estado al cerrar con ganador contra la Librería. Además la gracia online se contradice entre D2 y la regla 14, y cinco AC no discriminan o se contradicen.
Prior verdict resolved: Yes (los 14 de la 3.ª pasada)
Findings:
- [BLOCKING] Formulas D2 / Tuning / AC-26: piso 1.442 y rechazo del impar contra el Timer Approved (l.414 1.290–1.500; AC-19 acepta 1.440 con Fijo 60); AC-26 llama «impar» a 1.440.
- [BLOCKING] Formulas D1: la asimétrica falla con ritmos cercanos (70/72 da 21, real 20); el empate exacto cuenta como completo un turno que termina en `RelojAgotado`; la simétrica sobrecuenta (60 s → 25, Timer 24); l.464 falsa.
- [BLOCKING] Core Rules r11 / r16 / AC-37: limpia tabla, cara y bonos con ganador; la matriz de clausura de la Librería (Approved) solo los vacía «si sigue».
- [BLOCKING] Core Rules r2 / AC-2: lista cerrada con `Terminada` contra «no es un valor de la fase»; AC-2 exige «ninguna etapa» en Terminada, y la Librería guarda la etapa de la fase conservada.
- [BLOCKING] Core Rules r16 / AC-32 / AC-45: forma de `ataquesSinResolver` ambigua (solo IDs o pares atacante–objetivo).
- [BLOCKING] AC-8: con todos los pasos de costo 1, no separa «restaurar usados» de «usados − 1».
- [BLOCKING] Game Feel: «el contador de piezas movidas baja» al deshacer es falso si la pieza conserva movimientos usados.
- [BLOCKING] AC-30 / UI Requirements: el aviso de reloj no discrimina frente al nivel 1 del reloj; el caso de la placa no tiene WHEN; el recluta premarcado promete el recluta automático con vinculante = Reloj.
- [BLOCKING] Visual/Audio: el contorno puede no coincidir con `jugadorActivo` de la vista del Timer (Approved, r13).
- [BLOCKING] AC-45: no prueba el conteo de los pares.
- [BLOCKING] Core Rules r14 / D2 / Edge Cases / AC-44 / Open Questions: la gracia `g` se contradice (red fuera de los relojes contra cobrada; «se cobra» contra «hasta 0»); con g > 0 la anomalía del reloj en 0 se alcanza; un reloj en 0 al armar debe vencer en el armado; Edge «reloj en 0: no se aplica»; AC-44 sin t − 1 aceptado.
- [BLOCKING] Edge Cases (carreras): atribuye el aviso neutro a la fila 5 de la Librería, que es visible («esperando la tirada»).
- [BLOCKING] AC-27: «cada caso viola solo F8» no es cierto con la cota del Grito (aclarar que `PrepararContexto` no la valida).
- [RECOMMENDED] Fantasía («lo que falta se decide solo», «nunca se traga una orden válida»); «3,2 s de pared» solo con la presentación visible; AC-43 laxo y estrategia de agotar el reloj del rival; Dependencies sin #12 y #5; nota en la matriz para `EmisorInvalido` y `ComandoDesconocido`; AC-1 con archivo dorado; AC-14(b), AC-17, AC-23 (filas 10/11), AC-34, AC-40(b, d, f), carpetas de Integration; semántica de `MovimientoDeshecho` en la regla; `piezas_sin_rey_max` sin rango; aviso de reloj durante la placa (pregunta abierta).

Decisiones del jugador (2026-11-27, 4.ª pasada):
- **`T_tope`: alinear al Timer.** 1.290–1.500 (reloj 645–750); por debajo de 1.442 solo pasan Fijo 60 y Sin límite, por la guarda 1 del Timer. Se retira el rechazo del impar. **Reemplaza** el piso 1.442 de la 2.ª pasada.
- **D1: una sola fórmula exacta** `k_J = ⌈reloj ÷ seg_medio_J⌉ − 1`, `min(2·k_0, 2·k_1 + 1)`.
- **Fin con ganador: se conservan tabla, cara y bonos** del último turno, igual que con Rendirse y CerrarPartida.

Desacuerdos resueltos por el creative-director: el armado del turno 1 online con el primer comando de azul pasa a pendientes (#28 y el Timer en el cierre); el aviso de reloj durante la placa queda como pregunta abierta.
Pendientes para otros archivos: nota compartida `maquina-pendientes-cierre-tanda-B.md` (bloque «4.ª pasada»).

Revisión aplicada en la misma sesión con OK del jugador; la 5.ª pasada corre en esta misma sesión.

Reviewed-Content-Hash: design/gdd/maquina-de-turno.md f896e48a3b02ca789a8dbbe998c3b1695c6c1db3
Reviewed-Content-Hash: design/registry/entities.yaml 07713dcc52725c7d683477e25e8906d5db2f0c3e
Reviewed-Content-Hash: .claude/skills/design-review/SKILL.md 4dcc8b81162613c757144532f1b72708ab3ba27f
Reviewed-Content-Hash: design/gdd/libreria-de-reglas.md 5ac1d08ff674134ee106d55ad965c1784175b26d
Reviewed-Content-Hash: design/gdd/catalogo-de-piezas.md 24c2df9133aaea913698815462cb31d575219d30
Reviewed-Content-Hash: design/gdd/tablero-y-despliegue-inicial.md 9460a55db38e0d1d885bb1e96069d99b1b77e54c
Reviewed-Content-Hash: design/gdd/timer-de-turno.md 4c19f61f268d86dfdbce590a684e22b41acdc60d
Reviewed-Content-Hash: design/gdd/castillo-y-muros.md e2109fc867b4b33f496a4266dd581ca14de70b1f
Reviewed-Content-Hash: design/gdd/movimiento.md 679bca4fed148d8ebd3f599a15a6acbe6ce44bda
Reviewed-Content-Hash: design/gdd/ficha-de-pieza-y-hud.md 1090b0e1717f0e164d50e925f173f52c4c6f7054
Reviewed-Content-Hash: design/gdd/interfaz-del-modificador.md 283c065061e4d5a48c2d383e0b666dd09b3c1084
Reviewed-Content-Hash: design/gdd/render-animacion-y-feedback-de-combate.md 84f4bc10496ac137ba64adee2a48ca89c5d58fc2
Reviewed-Content-Hash: design/gdd/metricas-locales-de-partida.md 4b0beb1c2995defdc1e564cdce82fca557ce2a02
Reviewed-Content-Hash: design/gdd/game-concept.md 43e62d80536c71a39bae754ce48356caf6ee6beb
Reviewed-Content-Hash: design/art/art-bible.md c49b99572be1dffabb48c5974fc7ee54cdb29f28

## Review — 2026-11-27 — Verdict: NEEDS REVISION
Review mode: full · Workflow: full
Scope signal: L
Specialists: game-designer, systems-designer, qa-lead, ux-designer, network-programmer, lead-programmer, creative-director (senior)
Blocking items: 8 | Recommended: ~15 | Contratos con vecinos no aprobados: a pendientes del cierre
Summary: 5.ª pasada. Los 13 bloqueantes de la 4.ª y las tres decisiones del jugador se sostienen; el qa-lead aprueba los AC. Quedan 8 bloqueantes acotados, sin rediseño: Rendirse en `EsperandoTirada` con el reloj en 0 no tiene salida definida; las entradas de D1 (turno cortado y redondeo) inflan el presupuesto; AC-26, AC-34 y AC-45 no discriminan; el contorno se contradice entre la tabla y la prosa; y la precondición de turno del `TurnTimeout` en la regla 17 es ambigua.
Prior verdict resolved: Yes (los 13 de la 4.ª pasada)
Findings:
- [BLOCKING] Core Rules r14 / Edge Cases / AC-44: el invariante y la Edge de Rendirse con el reloj en 0 no se acotan a una etapa armada o pendiente; en `EsperandoTirada`, RelojAgotado da `CierreInvalido` (Librería r3), así que no hay salida definida.
- [BLOCKING] Formulas D1: N_J y duracion_J cuentan el turno cortado (uso máximo del Timer AC-21: da 17/25/21 contra 16/24/20 reales).
- [BLOCKING] Formulas D1: el redondeo hacia abajo de `seg_turno_t` y `seg_medio_J` infla k (10 × 68,5 s da 22 en vez de 20); «exacta» es falso.
- [BLOCKING] AC-26: el modo es ambiguo (1.442 se prueba solo en Fijo 60) y «coincide con su AC-19» es falso.
- [BLOCKING] Core Rules r17: «el turno del TurnTimeout es su numeroTurno» aparece entre los contadores; un turno distinto es `TimeoutObsoleto` (fila 8).
- [BLOCKING] AC-34: el tercer caso no excluye un escudero con aliado ni fija las coberturas del rival.
- [BLOCKING] Visual/Audio / UI Requirements: la fila `TurnoIniciado` contradice la prosa del contorno (el más tardío de dos instantes); falta decir de qué fuente sale lo que muestra el HUD.
- [BLOCKING] AC-45: el fixture no discrimina agrupar solo por atacante o solo por objetivo.
- [RECOMMENDED] AC-30 (caso de 50 s: «hasta el RelojAgotado»), AC-37 (usados en 0), AC-46 (estado conservado), AC-2 (Terminada en `EsperandoTirada`), AC-8 (tipo de A y casillas), AC-28, AC-44 (cómo se arma el reloj en 0), Tuning paso (3), Edge «Tirada sin fases», Player Fantasy «perdés en ese instante», garantía online T_tope + 2g, «el servidor fecha al recibir» PROVISIONAL #28, D2 «con los valores vigentes», propuesta del botón solo en Atacar.

Decisiones del jugador (2026-11-27, 5.ª pasada):
- **Rendirse en `EsperandoTirada` con el reloj del jugador del turno en 0: vale como Rendición.** No corre ningún reloj; el reloj en 0 vence al armarse la etapa después de la Tirada. El invariante del reloj se acota a «mientras haya una etapa del jugador del turno armada o pendiente».

Revisión aplicada en la misma sesión con OK del jugador; la 6.ª pasada corre en esta misma sesión.

Reviewed-Content-Hash: design/gdd/maquina-de-turno.md 2a6394ea05dfbfd227c8d4737681c59e88a06928
Reviewed-Content-Hash: design/registry/entities.yaml 07713dcc52725c7d683477e25e8906d5db2f0c3e
Reviewed-Content-Hash: .claude/skills/design-review/SKILL.md 4dcc8b81162613c757144532f1b72708ab3ba27f
Reviewed-Content-Hash: design/gdd/libreria-de-reglas.md 5ac1d08ff674134ee106d55ad965c1784175b26d
Reviewed-Content-Hash: design/gdd/catalogo-de-piezas.md 24c2df9133aaea913698815462cb31d575219d30
Reviewed-Content-Hash: design/gdd/tablero-y-despliegue-inicial.md 9460a55db38e0d1d885bb1e96069d99b1b77e54c
Reviewed-Content-Hash: design/gdd/timer-de-turno.md 4c19f61f268d86dfdbce590a684e22b41acdc60d
Reviewed-Content-Hash: design/gdd/castillo-y-muros.md e2109fc867b4b33f496a4266dd581ca14de70b1f
Reviewed-Content-Hash: design/gdd/movimiento.md 679bca4fed148d8ebd3f599a15a6acbe6ce44bda
Reviewed-Content-Hash: design/art/art-bible.md c49b99572be1dffabb48c5974fc7ee54cdb29f28

## Review — 2026-11-27 — Verdict: NEEDS REVISION
Review mode: full · Workflow: full
Scope signal: L
Specialists: game-designer, systems-designer, qa-lead, ux-designer, network-programmer, lead-programmer, creative-director (senior)
Blocking items: 6 | Recommended: ~15
Summary: 6.ª pasada. Los 8 bloqueantes de la 5.ª se sostienen y el lead-programmer aprueba. Quedan seis correcciones de una línea, sin rediseño: la justificación del turno cortado en D1 es falsa con la fórmula en ms (un corte por reloj es neutro) y el caso de AC-25 no discrimina; dos Edge del reloj en 0 no tienen el alcance nuevo; el banner sigue atado a `TurnoIniciado`; la captura de Abandono no existe en hotseat; la captura `-b` de AC-30 no tiene instante.
Prior verdict resolved: Yes (los 8 de la 5.ª pasada)
Findings:
- [BLOCKING] Formulas D1 / AC-25: «17, 25 y 21» es falso (un corte por RelojAgotado consume justo el resto del reloj y deja k igual); el caso del turno cortado da 16 lo cuente o no.
- [BLOCKING] Edge Cases (Fin de partida y tiempo): «comando con hora en o después del vencimiento» sin el alcance de etapa armada o pendiente, y con «su reloj» en lugar del reloj del jugador del turno.
- [BLOCKING] Edge Cases: «Rendirse en cualquier fase… gana el otro» sin la salvedad del reloj en 0.
- [BLOCKING] Visual/Audio: el banner aparece con `TurnoIniciado`, que sale durante la cosecha (Timer: «nunca durante la cosecha»).
- [BLOCKING] AC-45: la captura de Abandono en hotseat es imposible.
- [BLOCKING] AC-30: el instante de la captura `-b` del caso positivo no está fijado.
- [RECOMMENDED] AC-44 (rótulo de (i) a (iii), hora del armado en (iv), Rendirse de azul, caso (v) con comando rechazado); «jugador del turno» en EsperandoTirada; Tirada con todas las fases saltadas y reloj en 0; AC-25 con N fijo; garantía T_tope + 2g; precondiciones de la regla 17 (filas 7, 11 y 15, EmisorInvalido); D1 online con g > 0; objetivo sin pieza en la pantalla de resultado (#15).

Revisión aplicada en la misma sesión con OK del jugador; la 7.ª pasada corre en esta misma sesión.

Reviewed-Content-Hash: design/gdd/maquina-de-turno.md c3284cc963c8b8682e266c805e483bafcbb8d65a
Reviewed-Content-Hash: design/registry/entities.yaml 07713dcc52725c7d683477e25e8906d5db2f0c3e
Reviewed-Content-Hash: .claude/skills/design-review/SKILL.md 4dcc8b81162613c757144532f1b72708ab3ba27f
Reviewed-Content-Hash: design/gdd/libreria-de-reglas.md 5ac1d08ff674134ee106d55ad965c1784175b26d
Reviewed-Content-Hash: design/gdd/catalogo-de-piezas.md 24c2df9133aaea913698815462cb31d575219d30
Reviewed-Content-Hash: design/gdd/tablero-y-despliegue-inicial.md 9460a55db38e0d1d885bb1e96069d99b1b77e54c
Reviewed-Content-Hash: design/gdd/timer-de-turno.md 4c19f61f268d86dfdbce590a684e22b41acdc60d
Reviewed-Content-Hash: design/gdd/castillo-y-muros.md e2109fc867b4b33f496a4266dd581ca14de70b1f
Reviewed-Content-Hash: design/gdd/movimiento.md 679bca4fed148d8ebd3f599a15a6acbe6ce44bda
Reviewed-Content-Hash: design/art/art-bible.md c49b99572be1dffabb48c5974fc7ee54cdb29f28

## Review — 2026-11-27 — Verdict: NEEDS REVISION
Review mode: full · Workflow: full
Scope signal: L
Specialists: game-designer, systems-designer, qa-lead, ux-designer, network-programmer, lead-programmer, creative-director (senior)
Blocking items: 6 | Recommended: ~10
Summary: 7.ª pasada. Las 6 correcciones de la 6.ª se sostienen salvo una: el «corte por reloj neutro» de D1 es falso con ritmo variable, porque excluirlo sobrecuenta (8 turnos con 662 s más un corte por reloj: excluido da 17, contado 16, real 16). Además quedan cinco correcciones de una línea: la gracia online en dos Edge, el texto de espera online contra la UI del Timer, AC-30 con el bando viejo durante la cosecha, la convención de coberturas en AC-11 y AC-22, y AC-14(b) sin el Rey.
Prior verdict resolved: Yes (5 de 6; la del turno cortado se reabrió y se corrigió)
Findings:
- [BLOCKING] Formulas D1 / AC-25: un corte por RelojAgotado excluido da `⌈R·n ÷ dur_n⌉ − 1 ≥ n` y sobrecuenta; AC-25 no tiene ningún caso de corte por reloj.
- [BLOCKING] Edge Cases (Fin de partida y tiempo): «reloj en 0 con etapa armada» choca online con la gracia ([V, V+g)), y el mismo Rendirse recibe dos respuestas.
- [BLOCKING] Visual/Audio / UI Requirements: «texto de espera online» sin la excepción de la placa y la leyenda de un turno que se cerró solo (Timer, UI Requirements, Approved).
- [BLOCKING] AC-30: el caso de la cosecha no detecta un HUD que sigue a `estado.jugador`.
- [BLOCKING] AC-11 / AC-22: «exactamente» sin fijar las coberturas del jugador que entra (regla 4.1), así que una implementación correcta puede fallar.
- [BLOCKING] AC-14(b): «limite_normal + 1 piezas» sin excluir al Rey.
- [RECOMMENDED] AC-26 (paréntesis de la guarda), AC-23 (contador viejo sin Rendirse), AC-40(d) (Cubrir), ReclutaPerdido como renglón de la placa, «salvo Sin límite» en UI, regla 4.1 «J que entra», Distracción con 0,5 s sin placa, Timer r9/r17 sin el acote del invariante, `duracion_ms_J` hasta reloj − 1.

Decisión del jugador (2026-11-27, 7.ª pasada):
- **D1: el turno cortado por RelojAgotado cuenta en N_J y duracion_ms_J** (es un dato completo: el jugador gastó todo su reloj). Se excluyen solo los cortes por Rendirse, CerrarPartida y Tablas. **Reemplaza** la exclusión de todo turno cortado de la 5.ª pasada.

Revisión aplicada en la misma sesión con OK del jugador; la 8.ª pasada corre en esta misma sesión.

Reviewed-Content-Hash: design/gdd/maquina-de-turno.md b52fdba82ce963b6c18ad83eef1f1b0dc955b733
Reviewed-Content-Hash: design/registry/entities.yaml 07713dcc52725c7d683477e25e8906d5db2f0c3e
Reviewed-Content-Hash: .claude/skills/design-review/SKILL.md 4dcc8b81162613c757144532f1b72708ab3ba27f
Reviewed-Content-Hash: design/gdd/libreria-de-reglas.md 5ac1d08ff674134ee106d55ad965c1784175b26d
Reviewed-Content-Hash: design/gdd/catalogo-de-piezas.md 24c2df9133aaea913698815462cb31d575219d30
Reviewed-Content-Hash: design/gdd/tablero-y-despliegue-inicial.md 9460a55db38e0d1d885bb1e96069d99b1b77e54c
Reviewed-Content-Hash: design/gdd/timer-de-turno.md 4c19f61f268d86dfdbce590a684e22b41acdc60d
Reviewed-Content-Hash: design/gdd/castillo-y-muros.md e2109fc867b4b33f496a4266dd581ca14de70b1f
Reviewed-Content-Hash: design/gdd/movimiento.md 679bca4fed148d8ebd3f599a15a6acbe6ce44bda
Reviewed-Content-Hash: design/art/art-bible.md c49b99572be1dffabb48c5974fc7ee54cdb29f28

## Review — 2026-11-27 — Verdict: NEEDS REVISION
Review mode: full · Workflow: full
Scope signal: L
Specialists: game-designer, systems-designer, qa-lead, ux-designer, network-programmer, lead-programmer, creative-director (senior)
Blocking items: 6 | Recommended: ~10
Summary: 8.ª pasada. El game-designer aprueba y las correcciones de la 7.ª se sostienen. Quedan seis líneas: online, el vencimiento de una etapa armada dentro de la gracia con el reloj en 0 no coincide entre la regla 14 y dos Edge; AC-45 no detecta el bando equivocado en `fin.RelojAgotado`; a AC-25 le faltan el caso del último turno con ganador y el del Abandono; AC-37 perdió su prueba de coberturas con la convención nueva; D1 no dice de dónde sale `consumido_ms_t`; y «CerrarPartida» excluido contradice el corte por RelojAgotado. El creative-director lo ve aprobable con estas líneas.
Prior verdict resolved: Yes (las 6 de la 7.ª)
Findings:
- [BLOCKING] Core Rules r14 / Edge Cases: con una etapa armada dentro de [V, V+g) y el reloj en 0, «vencimiento + g» choca con «vence en el armado».
- [BLOCKING] AC-45: [bando] de `fin.RelojAgotado` sin fijar (el que perdió).
- [BLOCKING] AC-25: falta el último turno cerrado con ReyMuerto (cuenta: 17, excluido 16) y el corte por CerrarPartida(Abandono).
- [BLOCKING] AC-37: «sin CoberturaQuitada» ya no discrimina con la convención de coberturas.
- [BLOCKING] Formulas D1: la fuente de `consumido_ms_t` es `TurnoMedido`, que no tiene ese campo, y `parcial` no dice el motivo.
- [BLOCKING] Formulas D1: «Rendirse, CerrarPartida o Tablas» excluye también el RelojAgotado, que llega como CerrarPartida.
- [RECOMMENDED] Fila k_J («un turno que cuenta»), AC-14(b) (cara vieja), AC-32 (coberturas), AC-7/13/16/17 (acción posible en Atacar, ya en la convención), N_J «por ejemplo», Dependencies #5/#12, regla 15 con DobleRey, AC-30 «sin comandos» y «sin texto de espera», AC-44(v) del Timer r9, Edge l.688 «anomalía en local».

Decisión del jugador (2026-11-27, 8.ª pasada):
- **Online, una etapa que se arma con el reloj ya en 0, también dentro de la gracia, vence en el instante del armado: corta lo que quedaba de gracia.** PROVISIONAL #28.

Revisión aplicada en la misma sesión con OK del jugador; la 9.ª pasada corre en esta misma sesión.

Reviewed-Content-Hash: design/gdd/maquina-de-turno.md e59bc5236e5e75cd160a3489cb19c4972cb14c2f
Reviewed-Content-Hash: design/registry/entities.yaml 07713dcc52725c7d683477e25e8906d5db2f0c3e
Reviewed-Content-Hash: .claude/skills/design-review/SKILL.md 4dcc8b81162613c757144532f1b72708ab3ba27f
Reviewed-Content-Hash: design/gdd/libreria-de-reglas.md 5ac1d08ff674134ee106d55ad965c1784175b26d
Reviewed-Content-Hash: design/gdd/catalogo-de-piezas.md 24c2df9133aaea913698815462cb31d575219d30
Reviewed-Content-Hash: design/gdd/tablero-y-despliegue-inicial.md 9460a55db38e0d1d885bb1e96069d99b1b77e54c
Reviewed-Content-Hash: design/gdd/timer-de-turno.md 4c19f61f268d86dfdbce590a684e22b41acdc60d
Reviewed-Content-Hash: design/gdd/castillo-y-muros.md e2109fc867b4b33f496a4266dd581ca14de70b1f
Reviewed-Content-Hash: design/gdd/movimiento.md 679bca4fed148d8ebd3f599a15a6acbe6ce44bda
Reviewed-Content-Hash: design/art/art-bible.md c49b99572be1dffabb48c5974fc7ee54cdb29f28

## Review — 2026-11-27 — Verdict: NEEDS REVISION
Review mode: full · Workflow: full
Scope signal: L
Specialists: game-designer, systems-designer, qa-lead, ux-designer, network-programmer, lead-programmer, creative-director (senior)
Blocking items: 1 | Recommended: ~15
Summary: 9.ª pasada. Cinco especialistas aprueban y las seis correcciones de la 8.ª se sostienen; volvieron a calcular todas las cifras de D1, AC-25, D2, D4 y la regla 15 y no hay choques con GDD Approved. Único bloqueante (ux, confirmado por el creative-director): AC-30 no comprueba que el aviso de reloj dure toda la etapa. El creative-director lo da por APPROVED con esa línea; el jugador eligió una 10.ª pasada completa.
Prior verdict resolved: Yes (las 6 de la 8.ª)
Findings:
- [BLOCKING] AC-30: el aviso de reloj con vinculante = Reloj no se mira después del armado + 100 ms; un aviso transitorio pasa. Se agrega la captura -c (reloj entre 11 y 15 s) en la que sigue visible.
- [RECOMMENDED] AC-30 con ataques asignados y marca de Sin límite; Player Fantasy «cada segundo de tu turno» frente a la placa; D1 Output Range online con g; números de rojo en el ejemplo de Rendirse; Edge «con la misma etapa armada» y «misma etapa» = par (numeroTurno, etapa); AC-19 (cuenta de estados); AC-37 «con la cara Grito»; AC-26 Sin límite con GIVEN/THEN; turno 1 sin presentación (ADR del Timer); T_tope online 1.442–1.500 en «Rango seguro»; turno de 0 ms por la anomalía en D1; casos de Tablas e int64 en AC-25; fila N con Rendirse durante la cosecha.

Revisión aplicada en la misma sesión con OK del jugador (y limpieza de «a decidir con el jugador», ya decidido en la 8.ª); la 10.ª pasada corre en esta misma sesión.

Receipt note: el GDD objetivo no se estampa en esta entrada porque se le aplicó la corrección antes de registrarla; la 10.ª pasada lo ve como CHANGED.
Reviewed-Content-Hash: design/registry/entities.yaml 07713dcc52725c7d683477e25e8906d5db2f0c3e
Reviewed-Content-Hash: .claude/skills/design-review/SKILL.md 4dcc8b81162613c757144532f1b72708ab3ba27f
Reviewed-Content-Hash: design/gdd/libreria-de-reglas.md 5ac1d08ff674134ee106d55ad965c1784175b26d
Reviewed-Content-Hash: design/gdd/catalogo-de-piezas.md 24c2df9133aaea913698815462cb31d575219d30
Reviewed-Content-Hash: design/gdd/tablero-y-despliegue-inicial.md 9460a55db38e0d1d885bb1e96069d99b1b77e54c
Reviewed-Content-Hash: design/gdd/timer-de-turno.md 4c19f61f268d86dfdbce590a684e22b41acdc60d
Reviewed-Content-Hash: design/gdd/castillo-y-muros.md e2109fc867b4b33f496a4266dd581ca14de70b1f
Reviewed-Content-Hash: design/gdd/movimiento.md 679bca4fed148d8ebd3f599a15a6acbe6ce44bda
Reviewed-Content-Hash: design/art/art-bible.md c49b99572be1dffabb48c5974fc7ee54cdb29f28

## Review — 2026-11-27 — Verdict: NEEDS REVISION
Review mode: full · Workflow: full
Scope signal: L
Specialists: game-designer, systems-designer, qa-lead, ux-designer, network-programmer, lead-programmer, creative-director (senior)
Blocking items: 1 | Recommended: ~15
Summary: 10.ª pasada. Cinco especialistas aprueban, con todas las cifras recalculadas y sin choques con GDD Approved. Único bloqueante (lead-programmer, confirmado por el creative-director contra la Librería r4): la regla 17 atribuye el emisor fuera de turno a la fila 4, cuando es la fila 11 (`NoEsTuTurno`), que va después de la 5 y de la 10. `NoEsTuTurno` no aparece en el GDD y AC-23 no prueba el caso. El creative-director lo da por APPROVED con las cuatro ediciones; el jugador eligió una 11.ª pasada completa.
Prior verdict resolved: Yes (la de AC-30 de la 9.ª)
Findings:
- [BLOCKING] Core Rules r17 / AC-23: jugador fuera de turno atribuido a la fila 4 (Librería: fila 11, `NoEsTuTurno`); falta la fila en la matriz, falta la fila 11 en «la celda dice el resultado de las filas…» y AC-23 no lo prueba ni prueba el orden ComandoViejo → NoEsTuTurno.
- [RECOMMENDED] AC-30 (últimos 10 s y Reclutar → Mover); AC-31 (nombres de capturas); AC-40(d) (hora fija y acción posible); AC-44 (Rendirse del rival en t − 1); Edge «llega a 0 → pierde» online; regla 14 (pausa en la espera de la placa); regla 2 (etapa en Terminada desde EsperandoTirada); D1 «vence y no cuenta» → «no es completo»; D2 «unos 1,5 min»; Rendicion/Rendición en el enum; AC-35 (caso bien formado, ya en la Librería AC-50); AC-41 (orden de validación); AC-28 (destino legal).

Revisión aplicada en la misma sesión con OK del jugador; la 11.ª pasada corre en esta misma sesión.

Reviewed-Content-Hash: design/gdd/maquina-de-turno.md 871896e812445881eae4afee55c3b238babcc7fe
Reviewed-Content-Hash: design/registry/entities.yaml 07713dcc52725c7d683477e25e8906d5db2f0c3e
Reviewed-Content-Hash: .claude/skills/design-review/SKILL.md 4dcc8b81162613c757144532f1b72708ab3ba27f
Reviewed-Content-Hash: design/gdd/libreria-de-reglas.md 5ac1d08ff674134ee106d55ad965c1784175b26d
Reviewed-Content-Hash: design/gdd/catalogo-de-piezas.md 24c2df9133aaea913698815462cb31d575219d30
Reviewed-Content-Hash: design/gdd/tablero-y-despliegue-inicial.md 9460a55db38e0d1d885bb1e96069d99b1b77e54c
Reviewed-Content-Hash: design/gdd/timer-de-turno.md 4c19f61f268d86dfdbce590a684e22b41acdc60d
Reviewed-Content-Hash: design/gdd/castillo-y-muros.md e2109fc867b4b33f496a4266dd581ca14de70b1f
Reviewed-Content-Hash: design/gdd/movimiento.md 679bca4fed148d8ebd3f599a15a6acbe6ce44bda
Reviewed-Content-Hash: design/art/art-bible.md c49b99572be1dffabb48c5974fc7ee54cdb29f28

## Review — 2026-11-28 — Verdict: NEEDS REVISION
Review mode: full · Workflow: full
Scope signal: L
Specialists: game-designer, systems-designer, qa-lead, ux-designer, network-programmer, lead-programmer, creative-director (senior)
Blocking items: 1 | Recommended: ~12
Summary: 11.ª pasada. Cinco especialistas aprueban, con las cuatro ediciones de la regla 17 y AC-23 verificadas celda por celda contra la Librería r4. Único bloqueante (ux, confirmado por el creative-director): AC-31 no detecta una interfaz que olvide al Rey en los dos lados (8/16 y 9/16 dan igual con y sin Rey). Se suma «con 8 contra 15, no». El creative-director lo da por APPROVED con esa línea; el jugador eligió una 12.ª pasada completa.
Prior verdict resolved: Yes (la de la regla 17 de la 10.ª)
Findings:
- [BLOCKING] AC-31: el borde de RendirseDisponible no discrimina olvidar al Rey; falta el caso 8 contra 15 (no se ofrece; sin el Rey se ofrecería).
- [RECOMMENDED] Regla 14 «cualquier comando de jugador de cualquiera de los dos» y acotarlo a emisor 0/1 que pasa las filas 1–4; AC-44 con un Mover de rojo con el reloj en 0; Edge «Rendirse se acepta si la partida sigue» en cualquier fase; AC-23 con la pieza del emisor en la fila fuera de turno y contador viejo en otra columna; «penúltima fila de esta matriz»; n en 0–99 de la Tirada en las precondiciones; States «Distracción y Atacar sin acción»; AC-40(e) con los 0,9 s solo visuales; Edge con referencia cruzada al Rendirse del propio jugador del turno.

Revisión aplicada en la misma sesión con OK del jugador; la 12.ª pasada corre en esta misma sesión.

Reviewed-Content-Hash: design/gdd/maquina-de-turno.md 9f1cacd36492633214d693fe6377d8e7499be2ca
Reviewed-Content-Hash: design/registry/entities.yaml 07713dcc52725c7d683477e25e8906d5db2f0c3e
Reviewed-Content-Hash: .claude/skills/design-review/SKILL.md 4dcc8b81162613c757144532f1b72708ab3ba27f
Reviewed-Content-Hash: design/gdd/libreria-de-reglas.md 5ac1d08ff674134ee106d55ad965c1784175b26d
Reviewed-Content-Hash: design/gdd/catalogo-de-piezas.md 24c2df9133aaea913698815462cb31d575219d30
Reviewed-Content-Hash: design/gdd/tablero-y-despliegue-inicial.md 9460a55db38e0d1d885bb1e96069d99b1b77e54c
Reviewed-Content-Hash: design/gdd/timer-de-turno.md 4c19f61f268d86dfdbce590a684e22b41acdc60d
Reviewed-Content-Hash: design/gdd/castillo-y-muros.md e2109fc867b4b33f496a4266dd581ca14de70b1f
Reviewed-Content-Hash: design/gdd/movimiento.md 679bca4fed148d8ebd3f599a15a6acbe6ce44bda
Reviewed-Content-Hash: design/art/art-bible.md c49b99572be1dffabb48c5974fc7ee54cdb29f28

## Review — 2026-11-28 — Verdict: APPROVED
Review mode: full · Workflow: full
Scope signal: L
Specialists: game-designer, systems-designer, qa-lead, ux-designer, network-programmer, lead-programmer, creative-director (senior)
Blocking items: 0 | Recommended: 13
Summary: 12.ª pasada: APPROVED. Los seis especialistas aprueban sin bloqueantes. AC-31, con 8/16, 9/16 y 8/15, ya detecta olvidar al Rey en uno o en los dos lados. D1, D2, D4 y las reglas 14 a 17 cuadran con la Librería, el Timer y el Catálogo aprobados, sin choques con #1, #2, #4, #5 ni #12 (creative-director). Los 13 recomendados quedan como pulido opcional.
Prior verdict resolved: Yes (la de AC-31 de la 11.ª)
Findings:
- [RECOMMENDED] Dependencies: frase vieja sobre «el #2 Dura de esta tabla» (la tabla ya dice Dura).
- [RECOMMENDED] AC-20: borde t + 999 (el RelojAgotado no sale antes del 0 en el envío por tick).
- [RECOMMENDED] D1, fila consumido_ms_t: «solo puede ser el último turno».
- [RECOMMENDED] AC-35: «en una fase abierta» para los chequeos de forma.
- [RECOMMENDED] Edge Cases: «la carrera más común» es online; «su reloj» → «del reloj del jugador del turno».
- [RECOMMENDED] Regla 17: lista de filas (10, 12; la 15 se prueba en AC-42); n en 0–99.
- [RECOMMENDED] AC-40(d): «primer comando de fase aceptado».
- [RECOMMENDED] Regla 16: la pantalla de resultado no es «animar, sonar o medir»; CoberturaQuitada en los dos lugares de un Resolver; FaseIniciada salvo el Mover del turno 1.
- [RECOMMENDED] AC-33: variantes de TurnTimeout y de CerrarPartida.
- [RECOMMENDED] Header: fecha y estado (resuelto al pasar el Status a Approved).

Status del GDD → Approved (cabecera actualizada con OK del jugador antes de estampar este receipt; el cambio es solo de cabecera).
Pendientes para otros archivos: nota compartida `maquina-pendientes-cierre-tanda-B.md` (bloques de la 3.ª a la 11.ª pasada y de la 12.ª).

Reviewed-Content-Hash: design/gdd/maquina-de-turno.md 1e786e248c90bb65255b8c73ec6be6d6a1d5e6ba
Reviewed-Content-Hash: design/registry/entities.yaml 07713dcc52725c7d683477e25e8906d5db2f0c3e
Reviewed-Content-Hash: .claude/skills/design-review/SKILL.md 4dcc8b81162613c757144532f1b72708ab3ba27f
Reviewed-Content-Hash: design/gdd/libreria-de-reglas.md 5ac1d08ff674134ee106d55ad965c1784175b26d
Reviewed-Content-Hash: design/gdd/catalogo-de-piezas.md 24c2df9133aaea913698815462cb31d575219d30
Reviewed-Content-Hash: design/gdd/tablero-y-despliegue-inicial.md 9460a55db38e0d1d885bb1e96069d99b1b77e54c
Reviewed-Content-Hash: design/gdd/timer-de-turno.md 4c19f61f268d86dfdbce590a684e22b41acdc60d
Reviewed-Content-Hash: design/gdd/castillo-y-muros.md e2109fc867b4b33f496a4266dd581ca14de70b1f
Reviewed-Content-Hash: design/gdd/movimiento.md 679bca4fed148d8ebd3f599a15a6acbe6ce44bda
Reviewed-Content-Hash: design/art/art-bible.md c49b99572be1dffabb48c5974fc7ee54cdb29f28

## Cierre de la tanda B — 2026-11-28 (no es una pasada de revisión)
Por decisión del jugador, la sesión de cierre aplicó como pulido los recomendados abiertos de la 5.ª a la 12.ª pasada, sin cambiar reglas: Dependencies (#5 y #12 blandas, estados al día), reglas 2, 4, 14 (gracia `T_tope` + 2g, emisor 0/1, pausa), 15 (`DobleRey`), 16 (`FaseIniciada` del turno 1, `CoberturaQuitada` en un `Resolver`) y 17 (filas fuera de la matriz, `n` en 0–99); D1 (filas N, N_J, consumido_ms_t, duracion_ms_J, k_J; Output Range online), D2, Tuning, Edge Cases, UI, Player Fantasy; AC-2, 7, 8, 13, 14, 16, 17, 19, 20, 23, 25, 26, 28, 30, 31, 32, 33, 35, 37, 40, 41, 44 y 46. Quedan sin aplicar dos propuestas de diseño que van a #15 y #17: el aviso de reloj solo en `Atacar` y el premarcado durante la placa. El Status sigue Approved; una re-revisión, si hace falta, la decide el jugador.
