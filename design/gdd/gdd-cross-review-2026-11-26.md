# Cross-GDD Review Report

Date: 2026-11-26
GDDs Reviewed: 19 of 19 present (más `game-concept.md` y `systems-index.md` como contexto; `design/registry/entities.yaml`, 152 entradas, como línea base)
Not read: ninguno. Cobertura parcial por sección: ver «Alcance real de la revisión» al final.
Focus: full (consistencia + teoría de diseño + escenarios)
Systems Covered: #1 Librería de reglas, #2 Catálogo de piezas, #3 Máquina de turno, #4 Tablero y despliegue inicial, #5 Castillo y muros, #6 Movimiento, #7 Alcance y asignación, #8 Cobertura del escudero, #9 Resolución simultánea del daño, #10 Reclutamiento y límites, #11 Modificador, #12 Timer de turno, #13 Entrada y selección, #14 Localización, #15 Ficha de pieza y HUD, #16 Vista previa de ataques, #17 Interfaz del Modificador, #18 Render, animación y feedback de combate, #19 Métricas locales de partida.

Método: seis revisores en paralelo (dependencias y referencias cruzadas; núcleo #1–#4, #6, #10–#12; combate #2, #5, #7–#9, #16; cliente #13–#15, #17–#19; teoría de diseño; escenarios). Los informes por revisor, con todas las citas, quedaron en la carpeta de notas de la sesión (`review-all-gdds/A…F`). Los tres hallazgos marcados Blocking por los revisores se verificaron a mano contra los GDD antes de fijar su severidad. Los ID entre corchetes son los de esos informes; los duplicados entre revisores se fusionaron.

**Resumen de lo que está bien:** las reglas del daño no se contradicen entre #1, #2, #5, #7, #8, #9 y #11 (suma por casilla, escudo una sola vez, desvío al escudero sin cadenas, el tramo no se redirige, área y explosión con fuego amigo, ROTO, +2/+2 del arquero, corte con 2 tramos, Grito sin área ni explosión). Vista previa y resolución coinciden en todos los subcasos de combate recorridos (escudero que muere en la misma resolución, fuego amigo, exceso perdido, `TurnTimeout` igual que `Resolver`). DobleRey, ReyMuerto, RelojAgotado, la pila de deshacer (no cruza el reclutamiento) y el recluta automático cierran. Las decisiones de la tanda 4 están casi todas propagadas: la Tirada local que espera a la presentación (#18 r25 → #3 r14, #11 r9, #12, #17 r3, #16), la compuerta del arte (#19 → índice, concepto, #3), `MovimientoDeshecho` (#3 r16, #6, #19), la Divergencia en #32 (Librería r14), Exportar métricas (#15 r22) y la placa sobre todo receptor (#15 r10, States, AC-12). Ninguna perilla tiene dos dueños declarados. Las 19 fantasías citan su pilar y ninguna viola un anti-pilar.

---

### Consistency Issues

#### Blocking (must resolve before architecture begins)

🔴 **BL-1. El número flotante de la cosecha «igual al de la placa» no se puede cumplir** [C-01 = F-06; incluye C-05] — ✅ **Resuelto el 2026-11-26: opción (c).** El número flotante es `vidaPerdida` y no aparece sobre MUERE o ROTO; corregidos #16 r19 y AC-23, #18 r21 y su tabla de feedback, #9 Restricciones y AC-43, y #5 Visual l.371.
- #16 r7 (l.81-82): la placa de pieza muestra `vidaAntes → vidaDespues` y `bruto · escudo`; no muestra el `neto`. #16 r9 (l.87): la placa de tramo es de un solo renglón, `vidaAntes → vidaDespues`; con ROTO, sin número.
- #16 r19 (l.130) y AC-23 (l.531): «cada número flotante de la cosecha es igual al de la placa … (`neto` en piezas, `bruto` en tramos)». #18 r21 (l.174): «Es el mismo que mostró la vista previa … ningún número puede ser mayor que lo que el jugador pudo calcular». #9 Visual (l.482) y AC-43 dicen lo mismo.
- #5 Edge Cases (l.262): «ROTO sin mostrar el exceso». #5 Visual (l.371) y UI (l.409) todavía hablan de «el daño previsto sobre el tramo como número».
- Caso concreto: un tramo de 30 que recibe 32 muestra la placa ROTO sin número y el flotante muestra «32», un número que el jugador nunca vio. En piezas, el neto se deduce (25 → 12 ⇒ 13), pero no está en la placa; con MUERE, el neto puede pasar de `vidaAntes` y tampoco se deduce. #16 AC-23 y #9 AC-43 fallan tal como están escritos.
- **Decisión del jugador:** (a) el flotante es la vida perdida (`vidaAntes − vidaDespues`), lo que la placa ya muestra (los revisores lo recomiendan, al menos para el tramo); o (b) se mantienen neto y bruto y se reescriben #16 r19 y AC-23, #18 r21, #9 Visual y AC-43 como «se deduce de la placa», aceptando que el bruto del tramo puede superar lo mostrado. Con cualquiera de las dos, alinear #5 Visual l.371 y UI l.409.

🔴 **BL-2. El HUD lee el estado final, pero Render le pide cambiar en el cuadro de impacto** [D-01] — ✅ **Resuelto el 2026-11-26.** #15 muestra el estado presentado durante la cosecha y toma a #18 como dependencia Dura de tiempos (regla 1, Edge Case l.392, Dependencies, AC-37); #18 marca a #15 como Dura en las dos tablas.
- #15 r1 (l.69): el HUD «Lee, y solo lee» las consultas de la Librería, la Máquina, Movimiento, el Timer y el Modificador. Su fila de #18 (l.191, l.465) solo toma `casillaEnPantalla`.
- #15 Edge Cases (l.392), Visual (l.503, l.517): «el número de vida cambia de una vez, en el cuadro del destello de resolución». AC-27 (l.601): la ficha desaparece «en el mismo cuadro que su figura».
- #18 r19-20 (l.159-166): el grupo 2 (banner, emblema y contorno del turno nuevo, «de #15») no empieza antes del fin del grupo 1; en `t_impacto` «la vida de cada ficha cambia de una vez». #18 Interactions (l.286) le entrega a #15 el cuadro de impacto y el fin del grupo 1.
- Cuando llegan los eventos, el estado del núcleo ya es el final. Un HUD que solo lee estado mostraría la vida final, el emblema del rival y «Esperando la tirada» en el cuadro 0 de la cosecha, antes del impacto. Las reglas de #15 y la línea de tiempo de #18 no pueden cumplirse a la vez, y la arquitectura heredaría el hueco (¿el HUD lee el núcleo o un estado de presentación?).
- **Cambio:** #15 debe tomar a #18 como fuente Dura de tiempos (impacto, `t_caida`, fin del grupo 1) y declarar que, durante la presentación, muestra el estado «presentado», no el del núcleo. Es una decisión de frontera para `/create-architecture` (una cola de presentación a la que se suscriben Render y HUD).

#### Warnings (should resolve, but won't block)

⚠️ **W-1. ¿La placa del Modificador cuesta reloj?** [B-02 = D-05]. #11 Visual/Audio (l.396): «La tirada y su placa no se cobran del reloj del jugador». #12 r5 y r7 (l.65, l.67) arman la etapa con la `Tirada` aceptada; #17 r4 y r6 (l.72, l.86) y #3 Visual (l.460) muestran la placa con la fase abierta y el reloj corriendo (1,2 s, o 1,5 s con Distracción). **Decisión del jugador:** corregir #11, o que #12 arme la etapa al terminar la placa.

⚠️ **W-2. La regla 11 del Timer y la pregunta abierta 11 del HUD quedaron viejas** [B-03 = D-03]. #12 r11 (l.75): «ni las animaciones de resolución … frenan el reloj»; Tuning (l.325): «≤ 1,5 s». #15 OQ 11 (l.652): el reloj del siguiente corre durante la animación. #18 r25, #3 r14 y #12 OQ (l.445) dicen que en local no corre y que el techo es 2,0 s. → Reescribir #12 r11 y su tabla, y cerrar la OQ 11 de #15.

⚠️ **W-3. «Esperando la tirada» en local** [D-02]. #15 r12 (l.111) lo muestra en `EsperandoTirada`; #17 r5 (l.79) y su estado `Esperando` (l.204) lo marcan «solo online». Con #18 r25, el `EsperandoTirada` local dura hasta 2 s por turno. → Decidir qué muestran el anillo y la etiqueta durante la cosecha local y alinear #15, #17 y #3.

⚠️ **W-4. Avanzar durante la placa de Distracción** [D-06 = F-02]. #15 Edge (l.422): Avanzar pasa a `Terminar turno` «cuando termina la placa (≤ 1,5 s)». #17 r4 (l.72): «La placa no bloquea nada»; #15 r14 bloquea solo 300 ms. → Unificar (los revisores sugieren el bloqueo de 300 ms).

⚠️ **W-5. El toque que salta la resolución se come los controles del HUD** [D-07 + F-04]. #18 r24 (l.189): «cualquier toque o clic sobre la pantalla … se consume». #13 r3 (l.60) deja los controles al HUD; el «?» está visible en `EsperandoTirada` (#17 r7) y `Rendirse` vale ahí (#3, matriz l.184). → Definir si el salto cuenta solo sobre el tablero o en toda la pantalla, y nombrarlo en #15 y #17.

⚠️ **W-6. Render nombra un evento `Deshacer` que no existe** [D-04 + F-13]. #18 r15 (l.151), estados (l.252), Interactions (l.277) y Edge (l.461) dicen «`PiezaMovida` o `Deshacer`». #3 r16 (l.172) define `MovimientoDeshecho(pieza, origen, destino, usadosRestaurados)`, sin camino, y #18 promete volver «por el mismo camino». → Renombrar en #18 y decidir si el evento lleva el camino o la vuelta es directa.

⚠️ **W-7. Conversión pantalla → casilla con dos dueños y sin fórmula inversa** [D-08]. #18 r27 (l.212): «Nadie más convierte coordenadas», pero F3 solo va de casilla a rectángulo. #13 r3 (l.60) convierte el punto con bordes fijos de 60/144 px, sin `escala_pantalla`. → Agregar a #18 F3 la inversa (`casillaEnPunto`, con escala y letterbox) y que #13 apunte a ella.

⚠️ **W-8. #15 AC-24 no puede pasar** [D-09]. AC-24 (l.590) y r20 (l.133) permiten solo `ui.hud_*`, `pieza.*`, `fase.*`, `cara.*` y `fin.*`. El HUD muestra también `ui.metricas_*` (r22), `codigo.*`, `sistema.aviso_neutro` (r18) y claves de #17 (`ui.reclutar_auto`, `ui.reclutar_perdido`, `ui.modif_etiqueta_apertura`). → Ampliar la lista o reformular como «sin literales».

⚠️ **W-9. Un mismo código de rechazo con dos textos** [D-10]. #17 r6 (l.89) usa `codigo.<Código>`; r10 y r13 (l.126-129, l.172) usan `ui.modif_motivo_<Código>`. #14 r5 (l.62) pide un solo texto por código. → Documentar la excepción en #14 o reusar `codigo.*`.

⚠️ **W-10. Métricas se contradice sobre cuándo escribe** [D-11 = F-10]. #19 r7 (l.74): «nunca durante una animación». #19 F6 (l.294), Tuning (l.410) y Game Feel (l.430): la escritura cae «cuando empieza la animación de resolución», que es cuando llega `TurnoTerminado`. → Diferir la escritura al fin del grupo 1 de #18; en local cae en `EsperandoTirada`, sin reloj.

⚠️ **W-11. La duración de pared y la espera de la tirada** [D-12]. #19 F1 (l.183) dice que la espera de la tirada en local es «casi 0»; con #18 r25 y #3 r14 es de hasta 2 s por turno con ataques (≈ 56 s por partida). → Corregir el texto y decidir si la cosecha cuenta para `T_tope` en la compuerta (saltarla acorta la duración de pared).

⚠️ **W-12. Los techos de `ataque` del Catálogo no cuentan el Grito** [B-01]. #2 Tuning (l.265-266) y AC-22 (l.344): techo seguro 10 («con 11 o más mata sola al demoledor») y catapulta 16. Con el +2 del Grito (#11 r7, F3, l.98-100 y l.216), un soldado de 9 da 11 y una catapulta de 15 da 17. **Decisión del jugador:** si los invariantes I1/I2 se evalúan sobre la base o sobre base + bono; en el segundo caso los techos bajan a 8 y 14 y cambian AC-19 y AC-22.

⚠️ **W-13. El techo de `bono_grito_movimiento` se justifica con un caso imposible** [B-04]. #11 Tuning (l.371) y #4 OQ (l.403) usan el golpe del turno 1, pero en el turno 1 no se tira el dado (#3 r3, #11 r2). → Recalcular el margen para el turno 2 o retirar la justificación.

⚠️ **W-14. `Tablas` tiene dos significados y ninguno tiene flujo** [B-05]. #1 r3 (l.153): «un empate acordado (Tablas)». #12 r9 (l.73) y #19 (l.345): Tablas es el cierre por `LimitePartida`. #3 r15 (l.164) la cuenta como fin. Ningún GDD define cómo se propone un empate. **Decisión del jugador:** Tablas solo como cierre técnico (se corrigen #1 y #3) o un flujo de empate acordado (comando nuevo).

⚠️ **W-15. Un `LimitePartida` puede dejar la partida trabada** [B-06]. #1 Edge (l.602-605): solo un `LimitePartida` sobre un comando de autoridad obliga a cerrar. #12 r9 reacciona solo a sus `TurnTimeout`, y en Sin límite no hay ninguno (#12 Edge, l.268). Casi inalcanzable (C_MAX = 1.000.000), pero indefinido. → La autoridad cierra ante cualquier `LimitePartida`.

⚠️ **W-16. El invariante del «99+» se propagó con otra forma y otra cifra** [C-02 = A-03]. #16 F2 (l.241-255) y AC-11 (l.537): forma global, «hoy 30 + 5 = 35». #2 Dependencies (l.231) y el registro (`vida_escudo_max`): por tipo, «Rey, 30 + 7 = 37»; el 37 está mal porque el Rey no recibe el +2 del muro. → Elegir una forma (los revisores recomiendan por tipo, con el bono del muro solo para el arquero) y escribirla igual en #16, #2 y el registro.

⚠️ **W-17. Los avisos de corte no saltan con la explosión** [C-03 = F-07]. #16 r14, prioridades 3 y 5 (l.118, l.120): «llega a `tramos_rotos_corte` (2)». Por #9 r8, AC-11 y AC-12, una explosión deja el contador en 3. Leído literal, el demoledor de asedio no avisa. → «pasa a ser ≥ corte desde menos», más un caso de explosión en #16 AC-16.

⚠️ **W-18. #9 copia rangos de perillas ajenas, y no coinciden** [C-04]. #9 Tuning (l.421) dice «no se duplican», pero da `bono_grito_ataque` 0–10, `BONO_ESCUDO_MURO` 0–10 y `danoArea` 1–10; los dueños dicen 1–3 (#11 l.370), 1 a 4 (#5 l.346) y piso 0 (#2 l.272). → Citar el rango del dueño o borrar la columna.

⚠️ **W-19. Perillas de presupuesto sin nombre en su dueño** [A-01]. #16 Formulas (l.287): «`vista_previa_presupuesto_ms` = 5; Librería, Tuning Knobs». #1 Tuning (l.692) dice «Constantes del contrato (no son perillas)», sin nombre. #13 AC-36 (l.439) se la atribuye a Alcance, y #19 (l.409) usa `comando_presupuesto_ms`. → Que #1 las nombre y diga si son constante o dato; alinear #16 y #13.

⚠️ **W-20. Rango de `presupuesto_placa`** [A-02]. #16 Tuning (l.385): 4–6 («con 7 la placa pasa de 64 px»). #14 Tuning (l.261): 5–8. → Bajar el techo de #14 a 6.

⚠️ **W-21. Asimetrías de dependencias (índice y tablas)** [A-04, A-05, A-07, A-08, A-09, A-14, A-15].
- Fila 16 del índice (l.265): faltan #3 (Dura en #16 l.359), #11, #2 y #4 (blandas).
- Fila 18 (l.267): faltan #1 y #3, y Castillo y Alcance figuran como blandas, aunque #18 (l.489-496) las declara Duras. Castillo (l.322) y Tablero (l.256) listan a #18 como blanda.
- #19 y el índice (l.268) declaran Duras a #2, #3, #9 y #12, pero esos cuatro listan a #19 como Blanda: Catálogo l.250, Máquina l.415, Resolución l.409 y Timer l.289.
- A la tabla de dependientes del Catálogo le faltan #16 y #17. A la de la Máquina le faltan #16, #17 y #18. A la del Tablero, #13 y #17. A la de Castillo, #17.
- → Unificar la fuerza de cada arista en los tres lugares y completar las tablas.

⚠️ **W-22. Estados vencidos en #18 §Dependencies** [A-06]. l.499 y l.517 dicen «#11 … no nombra a #18», pero #11 lo nombra en l.149 y l.350. l.503, l.510 y l.511 dicen «#16 / #19 en paralelo». → Pasar a «Existe (tanda 4)».

---

### Game Design Issues

#### Blocking

Ninguno después de la verificación. E-01 pasó a Warning; ver DW-1.

#### Warnings

⚠️ **DW-1. Tortuga del Rey cubierto + reloj = victoria por tiempo** [E-01, E-10; marcado Blocking por el revisor, bajado a Warning]. Hechos verificados: el Rey se puede cubrir por decisión del jugador del 2026-11-25 (#8 r2 l.51, Edge l.239). Mientras dure la cobertura, recibe 0 (#8 AC-21, AC-25). Romper la cobertura cuesta 25 de bruto por escudero, y hay 4 al empezar y hasta 12 con reclutas (#8 Edge l.271). El Rey es 6/8/5/30 y supera al soldado (4/8/3/25) en todo (#2 l.70, l.76). El riesgo estaba aceptado en CD-GDD-ALIGN (#8 l.8) con una medición prevista (#8 OQ l.458). **Lo nuevo de esta revisión** es el mecanismo del reloj: el que va ganando cubre y cierra el turno en segundos, y el que ataca gasta 60-90 s por turno. Así la partida tiende a terminar por `RelojAgotado`, que no cuenta para la compuerta del arte (#19). Por eso no es Blocking: la regla está decidida, es coherente y se va a medir. Pero el pilar 2 («ninguna pieza gana sola»), el 4 y la fantasía de #19 («el reloj nunca tuvo que decidir») quedan expuestos. **Decisión del jugador:** (a) mantener y medir; (b) el exceso sobre el escudero pasa al Rey; (c) un escudero no puede cubrir al mismo aliado dos turnos seguidos; (d) excluir al Rey (#8 Tuning l.331). El revisor recomienda (b) o (c).

⚠️ **DW-2. El castillo deja al Rey más expuesto que el campo abierto** [E-02]. #5 r6 (l.58): con el muro entero, en el interior de 3 × 3 solo entra el Rey, así que ningún escudero lo puede cubrir. Desde afuera lo matan 6 arqueros (36 − 5 = 31 ≥ 30), o 5 con Grito. En el campo, cubierto, es inmune (DW-1). Esto contradice la fantasía de #5 y su F4 (l.185, «proteger al Rey»). **Decisión del jugador:** que un escudero cubra desde el tramo o desde el interior, o corregir F4 y la fantasía.

⚠️ **DW-3. Posible ventaja del segundo jugador y punto muerto en la apertura** [E-03, B-16]. Rojo tira en su primer turno (turno 2), con tabla 20/40/20/20/0, y azul no tira en el turno 1 (#4 r10 l.61, #11 r2 y F2). Si azul avanza, rojo pega primero con unos 40 de bruto. Si azul espera, nadie entra en alcance, y #4 Edge (l.231) ya acepta que eso lo cierre el reloj. Las desventajas de azul se suman sin compensación (#4 r10, #3 Edge l.368, #12 AC-21). → Registrar el lado ganador en #19 desde la primera partida. **Decisión del jugador** si se confirma: que azul también tire en el turno 1, o que rojo no tire en su primer turno.

⚠️ **DW-4. La fase de atacar supera el presupuesto de atención** [E-05, E-11]. Hay 6 decisiones activas en 30 s (20 s en el tramo final): ataca cada pieza viva, sin el límite de 12 del movimiento, y las coberturas se rehacen cada turno (#7 r1, #8 r7 y r13, #12). En la v4 el timeout de atacar saltó en el 79% y el 32% de los turnos (`production/session-state/active.md`). Además, un escudero con un aliado al lado siempre es «acción posible», así que `Atacar` casi nunca se salta con Distracción (hasta +150 s por partida). → Pasar a pasivo lo repetitivo: la cobertura sigue vigente mientras el escudero siga pegado (refuerza DW-1, así que hay que decidirlos juntos), un comando «repetir ataques», o una etapa de atacar que crezca con las piezas.

⚠️ **DW-5. La goma elástica puede pasar el umbral del pilar 3c** [E-06]. El ritmo de reclutas va de 1,2 a 2,0 piezas por turno según la tabla (#11 F5 l.275, #10 F6 l.232). Eso da hasta ≈ 11 piezas de diferencia en 14 turnos, casi un ejército inicial. En el único dato de la v4, el líder a 11-5 perdió. → Medir W y las piezas reclutadas por lado; si W < 2/3, achicar la diferencia de Llamada y Cuerno entre las tablas extremas.

⚠️ **DW-6. El corte de reclutamiento es permanente y lo provoca un solo demoledor** [E-07]. Un demoledor (vida 10) llega al muro en 2 turnos, rompe 3 tramos y apaga para siempre las caras de reclutar del rival (#5 F3 l.169, r10; #10 F3). Con 12 piezas o menos, Arremetida vale 0 y solo queda el Grito, contra «siempre hay vuelta». **Decisión del jugador:** mantener y medir, subir `vida_tramo` a 31, o que con 2 tramos rotos las caras de reclutar bajen a la mitad en vez de a 0.

⚠️ **DW-7. El pilar 3a se cumple en las tablas base y se invierte en las efectivas** [E-08]. Con 12 piezas o menos y 2 tramos rotos, el que pierde tiene Grito 85 y el que gana, 80 (#11 r12 l.108, F2). La diferencia sale de que Distracción vale 15 en «Pierde» y 20 en «Gana», no de una cara de reclutar inusable; la enmienda del 3a dice «nunca por ir perdiendo» (game-concept l.230). Además, la tabla cuenta piezas y no su valor. **Decisión del jugador** (toca la enmienda del pilar): aplicar la prueba a las tablas efectivas o igualar Distracción entre tablas.

⚠️ **DW-8. Anclas de fantasía que no cierran con las fórmulas** [E-04, E-09].
- #17 Player Fantasy (l.35, l.37): «Grito 45 · Arremetida 40» con 6 piezas contra 13. Pero #11 F2 (l.206) da 15/85/0/0/0 para ese caso (5 sin el Rey); 45/40 es la fila de 13 o más.
- #15 Player Fantasy (l.30): 2 arqueros y un soldado con Grito suman 26, menos que los 30 de un tramo entero.
- → Corregir las dos anclas (son ediciones de texto).

---

### Cross-System Scenario Issues

Scenarios walked: 5
- S1 Inicio de turno (presentación → Tirada → reclutar → reloj → HUD y placa)
- S2 Ataque combinado sobre una pieza cubierta junto a un tramo, con Grito y área con fuego amigo
- S3 Muerte del Rey con eventos simultáneos (DobleRey, reloj en 0, Rendición durante la animación)
- S4 Deshacer después de reclutar y de subir un arquero al muro
- S5 Cambio de idioma con un rechazo y el panel «?» abiertos, en el tramo final

#### Blockers
Ninguno propio. El problema más grave de S2 es BL-1 (el número flotante distinto de la placa).

#### Warnings
⚠️ **S1 — #18, #17, #15, #3** [F-01]. Cuando un turno se cierra solo (Distracción sin objetivos o cascada de `FaseSaltada`), la `Tirada` local siguiente «sale enseguida» (#18 Edge l.453). Eso pisa la placa de Distracción (1,5 s, #17 r4) y la leyenda «Sin objetivos» (#15 r19), y el jugador no ve que perdió el turno. → Que la `Tirada` local también espere a la placa y a las leyendas (≤ 1,5 s, fuera de los relojes).

⚠️ **S1/S5 — #12, #15, #17, #18** [F-03]. Si el turno empieza en el tramo final, `TramoFinalIniciado` (#12 r6) cae a la vez que la placa de la cara. #15 r12 pone el banner «Tramo final» al centro, y #15 r13 promete una sola placa central. → Fijar el orden o la prioridad.

⚠️ **S3 — #15, #18, #19** [F-08]. #15 Edge (l.430) muestra la pantalla de resultado apenas el estado pasa a `Terminada`. #18 States: «primero termina (o se salta) la resolución». La placa de fin, con `Exportar métricas`, tapa la caída de los Reyes, y un toque en Exportar se consume como salto. → La placa de fin espera al fin de la cosecha.

⚠️ **S3 — #19, #12, #3** [F-09]. Si la partida termina por `RelojAgotado`, `Rendicion` o `Tablas` a mitad de turno, no hay `TurnoTerminado`: no se escribe `ResumenTurno` (#19 r5) y quizá tampoco `TurnoMedido` (#12 r12). Se pierde justo el turno decisivo para la pregunta «¿el reloj decide partidas?». → Un cierre parcial del turno en `PartidaTerminada`.

⚠️ **S5 — #17, #15** [F-14]. #17 r7 enumera qué sigue activo con el panel «?» abierto y no nombra ES/EN, que está en el margen derecho (#15 r21). → Nombrarlo en #17 r7: o cambia el idioma, o solo cierra el panel.

#### Info
ℹ️ **S1 — #7** [F-05]. #7 r14 dice que con catapulta «`Atacar` nunca se salta», y su Edge (l.221) dice que una catapulta sin casillas en alcance «no cuenta como acción».
ℹ️ **S3 — #16, #12, #13** [F-11]. Un `Resolver` en el mismo milisegundo en que el reloj llega a 0 pierde por reloj (es la regla), pero falta fijar si al jugador se le muestra el rechazo `PartidaTerminada`.
ℹ️ **S3 — #13, #1, #15** [F-12]. En hotseat no está definido quién figura como emisor de `Rendirse` (al abrir o al confirmar el diálogo), ni que el diálogo es modal.
ℹ️ **S5 — #17, #13, #15, #16** [F-15]. No se dice qué pasa con el aviso persistente de la vista previa bajo el velo del panel.

---

### Info (consistencia, ediciones menores)

- [A-10] Tabla «Dependen de ella» de #1 (l.649-662): faltan #12 y #14. #1 depende del Catálogo como blanda (l.643), pero la fila 1 del índice (l.250) dice «—».
- [A-11] #1 §Cross-References (l.720-723) es un placeholder: «se verifican cuando cada GDD se escriba». Para #1 solo se aplicó el chequeo de prosa.
- [A-12] Párrafo de bidireccionalidad de #17 (l.473) vencido: #1 ya nombra a #17, y #15, #18 y #19 existen.
- [A-13] #2 §Cross-References (l.311) dice que faltan aristas en el índice, y su propia l.257 dice que ya están.
- [A-16, C-06] Restos de «placa solo sobre el objetivo»: #15 l.22, l.393, la nota de QA l.636 (todavía lista AC-12 y AC-33 como PROVISIONAL y dice que #16 no existe) y el art bible l.113.
- [A-17] #19 l.464: «Ninguno está aplicado» bajo un título que dice «aplicados al cerrar la tanda 4». Por muestreo, sí están aplicados.
- [A-20] El pedido de #19 al Timer, punto 2 (¿un salto de reloj mayor que `salto_reloj_max_ms` emite `RelojPausado`/`RelojReanudado`?), no tiene rastro en #12.
- [B-07] #1 r2 (l.118): «empieza directo en reclutar/mover». El turno 1 empieza en `Mover` (#3 r3, AC-3).
- [B-08] #1 States (l.474): «la autoridad la envía en cuanto se pide». En local, la tirada espera a Render (#18 r25).
- [B-09] game-concept l.89: «el turno también termina solo si no quedan acciones». Lo que pasa es que se salta una fase que empieza sin acciones (#3 r7 y r10).
- [B-10] Rangos de L y Q: 0–100 en #1 (l.540) y #10 F5 (l.201), 0–50 en #11 F2 (l.191-192).
- [B-11] #2 Dependencies (l.233) ubica el bono de turno en la Librería; #1 Tuning (l.689) dice que es del Modificador.
- [B-12] La misma perilla tiene dos nombres: `T_tope` (#3) y `tope_partida_s` (#12 l.323; registro l.1514). Hay que elegir uno canónico, con alias en el registro.
- [B-13] #12: el rango válido de D1 (15–90) se confunde con los rangos seguros de Tuning (30–90). La guarda de `aviso_etapa_s` rechaza 15, que está dentro de su rango seguro. #13 E2 (l.178) también pone 15–90 [D-18].
- [B-14] La precondición de #3 AC-4 (l.556), una cobertura «desde su turno anterior» en el turno 1, no se puede dar jugando.
- [B-15] #11 F4 (l.246) toma como N los `turnos_presupuesto` (26–37), no los turnos jugados. #10 F6 usa «14 tiradas por jugador». Hay que fijar una sola base.
- [B-17] game-concept l.88 conserva «(sin castillos, los demoledores no cuentan)», de la variante descartada.
- [C-07, C-08] Cotas de bruto y neto distintas entre #16 F2/F3 (61.610), #9 F2/F3 (123.220) y #5 F1/F6 (24.999.999 «sobre la casilla», que en realidad es la cota de una resolución entera).
- [C-09] #8 Visual (l.345): la flecha «se quiebra» en el aliado. #8 UI (l.375) y #16 r10: «terminan en el escudero».
- [C-10] `tramos_rotos_corte` (#16 l.396) y `bono_alcance_muro` (#7 l.266) se tratan como perillas, pero en #5 r14 son constantes del contrato.
- [C-11] Open Questions ya resueltas que siguen abiertas: #7 l.375, #8 l.457 y #11 l.145.
- [C-12] #16 Tuning (l.397) dice que `vida_tramo` con 100 o más se abrevia; F1 (l.232) no lo abrevia.
- [C-13] #8 Edge (l.256): «AliadoMuerto solo por fuego amigo» olvida al demoledor cubierto que muere al explotar.
- [C-14] #15 r9 (l.98): la tarjeta del tramo pone «/ 30» fijo; debe usar `vida_tramo`.
- [D-13] Falta `Tablas` en la transición `Libre → Final` de #18 (l.243).
- [D-14] #18 Tuning (l.541): `leyenda_ms` ubica el banner del grupo 2 en el centro, que desde el turno 2 ocupa la placa de #17.
- [D-15] Índice l.75: frase sin verbo. Next Steps (l.541) sigue diciendo solo «25 min o menos».
- [D-16] La consulta `rendirseDisponible` (#15 r14, #19 r5) no existe en #1. #1 r3 dice que ese límite es de la interfaz, así que hoy hay dos copias.
- [D-17] #13 Dependencies (l.299) dice que el Timer no lo nombra, y sí lo nombra. #13 Edge (l.251) deja abierto el doble clic de Avanzar, ya resuelto por `bloqueo_avance_ms`.
- [D-19] #18 r7 y Edge (l.480) mandan `SkinRechazada` y `ResolucionSaltada` a Métricas, pero el esquema de #19 no los tiene (OQ l.590).

---

### GDDs Flagged for Revision

| GDD | Reason | Type | Priority |
|-----|--------|------|----------|
| vista-previa-de-ataques.md (#16) | BL-1 (r19, AC-23); W-16, W-17, W-19, W-20 | Consistency | Blocking |
| render-animacion-y-feedback-de-combate.md (#18) | BL-1 (r21), BL-2 (contrato con #15); W-5, W-6, W-7, W-22; F-01, F-08 | Consistency | Blocking |
| resolucion-simultanea-del-dano.md (#9) | BL-1 (Visual l.482, AC-43); W-18 | Consistency | Blocking |
| ficha-de-pieza-y-hud.md (#15) | BL-2 (r1, fuentes de tiempo); W-2, W-3, W-4, W-8; F-03, F-08 | Consistency | Blocking |
| castillo-y-muros.md (#5) | BL-1 (Visual l.371, UI l.409); DW-2, DW-6 | Consistency / Design | Warning |
| modificador.md (#11) | W-1, W-13; DW-7 | Consistency / Design | Warning |
| timer-de-turno.md (#12) | W-1, W-2; A-20 | Consistency | Warning |
| interfaz-del-modificador.md (#17) | W-3, W-9; F-14; DW-8 | Consistency | Warning |
| metricas-locales-de-partida.md (#19) | W-10, W-11; F-09 | Consistency | Warning |
| catalogo-de-piezas.md (#2) | W-12, W-16 | Consistency | Warning |
| libreria-de-reglas.md (#1) | W-14, W-15, W-19 | Consistency | Warning |
| maquina-de-turno.md (#3) | W-14; W-21 (tabla de dependientes) | Consistency | Warning |
| cobertura-del-escudero.md (#8) | DW-1, DW-4 | Design Theory | Warning |
| entrada-y-seleccion.md (#13) | W-7 | Consistency | Warning |
| localizacion.md (#14) | W-9, W-20 | Consistency | Warning |
| tablero-y-despliegue-inicial.md (#4) | W-13; DW-3 | Consistency / Design | Warning |
| systems-index.md | W-21 (filas 16, 18, 19) | Consistency | Warning |

#6 Movimiento, #7 Alcance y #10 Reclutamiento solo tienen ítems Info.

---

### Verdict: FAIL (superado: ver «Revisión de cambios 2026-11-26» al final; veredicto vigente CONCERNS)

FAIL: dos problemas bloqueantes de consistencia (BL-1 y BL-2) que la arquitectura heredaría. Los dos están en la frontera entre el núcleo y la presentación del combate (#9, #15, #16, #18). Las reglas de juego en sí son coherentes. Además hay 22 Warnings de consistencia y 8 de diseño.

### If FAIL — required actions before re-running

1. **BL-1:** que el jugador decida qué número flota en la cosecha (vida perdida, o neto/bruto deducido). Después, alinear #16 r19 y AC-23, #18 r21, #9 Visual (l.482) y AC-43, y #5 Visual (l.371) y UI (l.409).
2. **BL-2:** en #15 r1 y Dependencies, sumar a #18 como fuente Dura de tiempos (impacto, `t_caida`, fin del grupo 1) y declarar que, durante la presentación, el HUD muestra el estado presentado y no el del núcleo. Reflejarlo en #18 Interactions (l.286).
3. Recomendado antes de `/create-architecture`, porque son decisiones de tiempo que tocan el contrato: W-1 (la placa y el reloj), W-2, W-6, W-10, W-14 y F-08.
4. Volver a correr `/review-all-gdds since-last-review` (o `consistency`) después de las ediciones.

### Decisiones que son del jugador

- BL-1: qué número flota en la cosecha.
- W-1: si la placa del Modificador cuesta reloj.
- W-3: qué se ve durante la cosecha local.
- W-12: si los techos del Catálogo cuentan el Grito.
- W-14: qué es Tablas.
- DW-1: la tortuga del Rey (mantener y medir, o una regla nueva).
- DW-2: si se puede cubrir al Rey dentro del castillo.
- DW-3: la tirada del primer turno de rojo (después de medir).
- DW-4: si la cobertura se vuelve pasiva (junto con DW-1).
- DW-6: si el corte de reclutamiento es total o parcial.
- DW-7: la prueba del pilar 3a sobre las tablas efectivas.

---

### Alcance real de la revisión

Todas las fases corrieron. Cada revisor leyó por secciones y dejó áreas sin verificar; no hay que tomarlas como limpias:

- **Dependencias y referencias:** las tablas de Dependencies completas de #6 a #15 se vieron solo por grep, y las Cross-References de #1 a #15 no se revisaron elemento por elemento. Tampoco se cotejaron las líneas Quick reference. De los pedidos de #18, quedaron sin verificar #9 AC-43, #5 AC-31 y la flecha quebrada en #8. El pedido de #16 a #8 (l.345) tampoco se verificó.
- **Núcleo:** los AC de #1 (l.725-1205) y sus reglas 8-14; las Open Questions de #10 y #6.
- **Combate:** #18 completo (solo grep), los AC de #1, #3, #6, #10 y #13 en detalle, y los AC numéricos uno por uno (se verificó una muestra de unos 25). No hay datos de balance (`assets/data/`, `design/balance/`), así que la comparación de datos de `/balance-check` no se hizo.
- **Cliente:** el contenido completo de #16 más allá del grep, las fórmulas F1-F7 de #15 y F1-F5 de #17, los presupuestos de texto en píxeles y los AC de #13 y #17. Los eventos no están en `entities.yaml`, así que no se pudieron cotejar contra el registro.
- **Teoría de diseño:** sin partidas del MVP no hay win rate por lado, W, duración por motivo de fin ni bajas por turno. No se leyó el Detailed Design de #6, #12 y #9 (solo lo que citan los demás GDD), ni los REPORT de los prototipos v1 y v2.
- **Escenarios:** no se leyó `orden_aparicion` de #4, solo la regla 3 de #1, las secciones Visual/Audio y UI de casi todos los GDD, ni F4/F5 de #14. La pantalla de resultado (#21) no existe todavía.

---

## Revisión de cambios 2026-11-26

**Alcance.** `review-scope.sh` dio «ningún GDD cambió», porque el informe se commiteó junto con los arreglos de BL-1 (`928898a`). Por eso se revisó a mano el diff `49627ba..HEAD`, con estas búsquedas en todos los GDD y en el registro: «flotante» junto con «neto» o «bruto», «destello», y las filas de dependencias de #15 y #18. Los W, DW y F del informe original no se revisaron de nuevo; ninguno tocaba las líneas cambiadas.

**Commits revisados**
- `928898a`, BL-1: el número flotante es `vidaPerdida`, sin número sobre MUERE o ROTO. Toca #16, #18, #9 y #5.
- `7c99dc5`, BL-2: #15 muestra el estado presentado durante la cosecha, y #15 y #18 pasan a ser dependencias Duras entre sí. Toca #15 y #18, y agrega AC-37.

**Restos encontrados y corregidos en este mismo commit**
- **#9:** la tabla Visual en l.467 y l.472, y «Por pieza» y «Por tramo» en l.502-503, todavía decían «neto» y «bruto». Ahora dicen vida perdida y que MUERE o ROTO van sin número. Con esto cierra BL-1.
- **#16:** l.377 (supuestos sobre #18) y l.504 (cambios pedidos) dicen la regla nueva y dejan nota de la anterior.
- **#15:** Visual l.505 y l.519 pasan de «cuadro del destello» a `t_impacto` de Render. El Quick reference nombra Render.
- **#18 Quick reference y `systems-index.md` filas 15 y 18:** #15 y #18 figuran como Duras entre sí. Esto resuelve la parte de W-21 que tocaba estas filas.

**Estado**
- BL-1 y BL-2: **resueltos**.
- Sin hallazgos nuevos.

### Verdict vigente: CONCERNS

No queda ningún bloqueante. Siguen abiertos los 22 Warnings de consistencia (W-21 solo en parte), los 8 Warnings de diseño (DW-1 a DW-8) y los Warnings de escenario. Ninguno bloquea `/create-architecture`, pero conviene repartirlos entre las `/design-review` de cada GDD.
