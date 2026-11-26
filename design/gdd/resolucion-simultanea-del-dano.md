# Resolución simultánea del daño

> **Status**: Designed (pendiente de `/design-review` en una sesión nueva)
> **Author**: Martin Arin + Claude
> **Last Updated**: 2026-11-25
> **Last Verified**: 2026-11-25
> **Implements Pillar**: Pilar 2 (La fuerza está en combinar) y Pilar 1 (Planificar y cosechar); Pilar 4 (Duelo corto) como restricción de presentación
> **Creative Director Review (CD-GDD-ALIGN)**: CONCERNS (accepted) 2026-11-25 — pilares 2 y 3 (reglas 7 y 8: un solo demoledor corta el reclutamiento rival, excepción aprobada por el jugador el 2026-11-25; el argumento de Castillo F4 «cortar cuesta más que matar al Rey» ya no vale para esa pieza), pilares 2 y 4 (regla 7: el exceso sobre el escudero se pierde y habilita la tortuga alrededor del Rey) y pilar 4 (el límite de ~2 s depende solo de la presentación y es una propuesta del art bible sin validar). Los tres están en Open Questions con su medición en el MVP. El gate lo aplicó la sesión misma, sin agentes.

## Summary

Resolución simultánea del daño calcula, de una sola vez y sobre una foto del estado de antes, todo el daño del turno: suma los golpes sobre cada casilla, resta el escudo una sola vez, manda al escudero lo que iba al aliado cubierto, rompe tramos de muro y retira a los muertos. Hace que combinar varias piezas sobre un objetivo valga más que la suma de golpes sueltos (pilar 2) y que el resultado sea exactamente el que anunció la vista previa (pilar 1). No tiene estado ni perillas propias: usa los números del Catálogo, de Castillo y del Modificador, y entrega eventos para que Render, Audio y Métricas lo muestren.

> **Quick reference** — Layer: `Feature` · Priority: `MVP` · Key deps: `Librería de reglas (#1), Catálogo de piezas (#2), Máquina de turno (#3), Tablero (#4), Castillo y muros (#5), Alcance y asignación (#7), Cobertura del escudero (#8), Modificador (#11, blanda)`

## Overview

Resolución es lo que ocurre cuando el jugador del turno envía `Resolver` (o llega `TurnTimeout` de atacar): toma la lista de ataques que Alcance (#7) guardó, la lee contra una **foto** del estado de antes y calcula, en un solo paso, todo el daño del turno. Cada ataque se vuelve golpes sobre casillas: uno para el ataque directo; uno en el centro y ocho alrededor para el área de la catapulta; ocho alrededor para la explosión del demoledor. Los golpes que caen sobre una misma casilla se suman; el escudo del receptor se resta **una sola vez**; la cobertura (#8) manda al escudero lo que iba al aliado cubierto; y el tramo de muro (#5) recibe el golpe completo, sin escudo. Recién entonces se aplica el resultado: baja la vida, se rompen tramos, mueren piezas, se borran las coberturas huérfanas y se suma `vidaQuitada`.

Nadie actúa antes que nadie. Una pieza que muere en la resolución igual golpea, un tramo que se rompe igual dio su +2 de escudo al arquero que tenía encima y un escudero que muere igual desvió los golpes que le tocaban. Esa simultaneidad es lo que hace verdad el pilar 2: un golpe suelto rebota contra un escudo y varios juntos lo atraviesan, porque el escudo se descuenta una vez sobre la suma y no una vez por golpe.

El jugador no maneja este sistema: lo prepara en `Atacar` y lo cosecha. Lo ve primero en la vista previa (#16), que es **esta misma operación sobre una copia** (Librería, regla 9), y después en los ≤ 2 s en que Render (#18) anima el resultado. No tiene estado propio, no tiene perillas propias (todo número viene del Catálogo, de Castillo o de la config) y no hay azar: mismo estado y misma lista dan siempre los mismos eventos.

Este GDD **cierra los supuestos PROVISIONAL** que le dejaron los GDD vecinos: la explosión del demoledor sobre muros (Castillo, #5), el reparto del daño con cobertura (Cobertura, #8), el formato de la lista y la zona de explosión (Alcance, #7), el bono evaluado al empezar la resolución (Castillo, #5, y Modificador, #11) y el contador `vidaQuitada` (Máquina, #3, y Librería, #1). Los cierra **confirmando** lo que ellos ya habían escrito, no reabriéndolo; lo único nuevo es lo que ninguno fijó: el orden y la forma de los eventos de la resolución, las cotas del `bruto` y la regla de «combinado» para la explosión.

*Alternativas descartadas:* (1) resolver ataque por ataque en orden de ID contra el estado ya modificado: la pieza que muere antes no golpea, el escudo se resta por golpe y la vista previa tendría que mostrar un orden que el jugador no controla (rompe los pilares 1 y 2 y la regla 15 de la Librería); (2) que el exceso de daño sobre el escudero pase al aliado cubierto: contradice la regla del jugador («todos los ataques enemigos a ese aliado los recibe el escudero») y volvería la cobertura una pérdida de tiempo contra cualquier combo.

## Player Fantasy

**La cosecha.** Alcance te dio la mesa de guerra: trazás flechas, las sumás, ves MUERE y todavía no pasó nada. Resolución es el momento en que pasa, y pasa **exactamente** lo que viste. No hay sorpresa ni tirada entre el plan y el golpe: las flechas caen en el mismo cuadro, las piezas que tenían que caer caen juntas y el número flotante coincide con el de la vista previa.

**Momento ancla:** el soldado rival tiene escudo 3 y vida 25. Tres soldados tuyos y un arquero lo apuntan, y la vista previa ya dijo MUERE (8 + 8 + 8 + 6 = 30, menos 3 = 27). Tocás Resolver y las cuatro flechas se funden en un solo impacto, más grande que la suma de los chicos; el soldado cae. Lo que se anunció, pasó.

**Dos momentos más que la sostienen.** El **contra-combo**: tu catapulta cae sobre el arquero que el rival cubrió y el área llega al escudero pegado; 16 sumados, menos 5, y el escudero queda con 9 mientras el arquero ni se entera. Y el **sacrificio**: un demoledor de 10 de vida llega pegado al muro, explota, abre tres huecos de un golpe y deja de existir; vale lo que cuesta, y el costo también se vio antes.

**Pilares que sirve:**

- Pilar 1, *"Al mover armás la estrategia; al atacar ejecutás el plan y ves sus frutos."* Resolución es la mitad que se cosecha: el daño es fijo, se ve antes y nunca depende del azar.
- Pilar 2, *"Ninguna pieza gana sola: el poder sale de juntar varias sobre un mismo objetivo."* Suma por objetivo y escudo restado una vez son la regla que lo hace cierto; la cobertura y el área son las dos respuestas al combo y también pasan por esta suma.

**Qué le exige al resto del diseño:** que la vista previa sea literalmente esta operación (#16); que todos los impactos se vean en el mismo cuadro y que el golpe combinado se sienta distinto de varios sueltos (#18, #22, art bible, principios 2 y 3); que el fuego amigo se vea siempre **antes**, nunca por sorpresa (Alcance); y que la resolución completa entre en ~2 s (pilar 4). El riesgo propio de esta fantasía es el opuesto: que un resultado correcto pero ilegible se sienta como azar. Por eso cada evento trae el desglose (bruto, escudo, neto, qué se desvió).

*Encuadres considerados:* «el golpe que suma» (solo el combo) y «el costo de la jugada» (solo lo que se pierde: fuego amigo, sacrificio). Se escribió la cosecha con el combo como cima y el costo como cláusula de riesgo, igual que hizo Alcance con su mezcla. Elegido en `guided` por recomendación; cambiable si preferís otro encuadre.

## Detailed Design

### Core Rules

1. **Qué es y cuándo corre.** Resolución es la transición que ocurre dentro de `Resolver` y de `TurnTimeout` de la etapa atacar (Máquina, #3, reglas 11 y 12; las dos hacen exactamente lo mismo). No es un comando ni una fase en reposo, no rechaza nada y no tiene estado propio: calcula sobre el estado de entrada y devuelve el estado nuevo y una lista de eventos. Los rechazos del comando (`NoEsTuTurno`, `FaseIncorrecta`, `EsperandoTirada`, `PartidaTerminada`) son de la Librería y salen antes. Sea **J** el jugador del turno y **K** su rival: en toda resolución, **todos los golpes tienen origen en una pieza de J**.
2. **Entrada.** (a) La lista de asignaciones de Alcance (#7, regla 16): `(pieza, objetivo, modo)` por ID de pieza ascendente, con a lo sumo una por pieza; (b) la **foto** de la regla 3. Resolución **no revalida** las asignaciones (Alcance, regla 11: nada de lo que define su validez cambia durante `Atacar`) ni las coberturas (#8, regla 14). Un estado que las viole ya fue rechazado al cargarlo (`EstadoInvalido`); si llegara hasta acá, es `ErrorInterno` (Librería, regla 1). **Con la lista vacía** no hay golpes: no cambia ninguna vida, no hay eventos de Resolución, y la Máquina cierra el turno igual (`TurnoTerminado`).
3. **La foto (cierra el supuesto «el bono rige al empezar la resolución»).** Antes de aplicar nada se leen **una sola vez** y quedan congelados hasta el final de la resolución: de cada pieza viva su casilla, su dueño, su tipo, su vida, su `ataque_ef` y su `escudo_ef` (`EffectiveStats`: Catálogo, más el Grito en el ataque salvo en el modo Explosión, más el +2 de escudo del arquero sobre un tramo propio en pie), y el `danoArea` de su tipo; de cada tramo su vida; y `Coberturas()`. **Nada que ocurra dentro de la resolución cambia la foto.** Consecuencias, todas ya prometidas por los GDD vecinos y ahora cerradas:
   - Una pieza que muere en esta resolución **igual golpea**, con el `ataque_ef` del turno (Librería, regla 15; Modificador, Edge Cases).
   - Un tramo que se rompe en esta resolución **igual dio el +2 de escudo** al arquero que tiene encima (Castillo, regla 7): el escudo de ese arquero es el de la foto.
   - Un escudero que muere en esta resolución **igual desvía** todo lo que le tocaba (Cobertura, regla 10). Sin cascadas.
   - Un tramo ya roto en la foto no recibe ni se vuelve a romper; un tramo en pie recibe el golpe completo aunque otro golpe de la misma resolución lo rompa: no hay «antes» ni «después».
4. **Golpes.** Cada asignación se convierte en **golpes**, cada uno `(casilla, daño, origen)`. El modo sale del Catálogo, no del jugador (Alcance, regla 5):

   | Modo | Golpes que genera | Daño de cada golpe |
   |---|---|---|
   | **Directo** | 1, en la casilla objetivo | `ataque_ef(origen)` |
   | **Área** (`danoArea` > 0; hoy la catapulta) | 1 en la casilla objetivo y 1 en cada una de sus 8 vecinas **dentro del tablero** | Centro: `ataque_ef(origen)` (con Grito, 10 pasa a 12). Vecinas: `danoArea` del tipo (6), **sin bono** (Modificador, regla 7) |
   | **Explosión** (el demoledor, ID de tipo 5) | 1 en cada una de las 8 vecinas **de su propia casilla** dentro del tablero. Su casilla no recibe golpe | `ataque_ef(origen)`, que es su ataque de Catálogo (30): el Grito no se le suma |

   Las vecinas son el **contorno de 8** del Tablero (#4, regla 2). Los golpes que caerían fuera de la grilla se descartan sin evento. Un golpe cae sobre **la casilla, sin mirar qué hay**: casilla vacía, hueco, pieza propia o enemiga, tramo propio o rival; no hay línea de visión ni distinción de bando (fuego amigo). Un `danoArea` mayor que 0 en un tipo que no es la catapulta se aplica igual que a ella (Catálogo, Edge Cases); un demoledor con `danoArea` distinto de 0 sigue explotando (Alcance, regla 5). Una asignación genera como mucho un golpe por casilla, así que una pieza nunca golpea dos veces la misma casilla.
5. **Qué recibe cada casilla.** Se suman los daños de todos los golpes de la casilla `c`: `bruto_casilla(c)` (F1). Después:
   - **Si `c` tiene un tramo en pie en la foto**, el tramo recibe `bruto_casilla(c)` **entero**, sin escudo y sin redirección (regla 8).
   - **Si `c` tiene una pieza en la foto**, cada golpe de `c` va a `receptor(pieza, origen)` (Cobertura, F2): la propia pieza, o su escudero si está cubierta y el origen es del rival del escudero (regla 7).
   - **Si `c` no tiene ni lo uno ni lo otro** (vacía o hueco sin pieza), el golpe se pierde: sin evento.

   Las dos primeras pueden valer a la vez: un **arquero sobre su tramo** recibe como pieza y su tramo recibe como muro, los dos del mismo golpe (Castillo, regla 8). El atacante no puede elegir uno solo.
6. **Pieza: bruto, escudo, neto, vida.** El `bruto_pieza(q)` de un receptor `q` es la suma de los golpes cuyo receptor es `q` (F2). Su daño neto es `max(0, bruto_pieza − escudo_ef)` y su vida nueva `max(0, vida − neto)` (F3). **El escudo se resta una sola vez por receptor**, sobre la suma, nunca por golpe ni por casilla: es la regla que hace que tres golpes que solos rebotan, juntos pasen (pilar 2). Un receptor con `neto` 0 no pierde vida; con `neto` mayor o igual que su vida, muere (regla 9).
7. **Cobertura: el reparto (cierra el supuesto de Cobertura, #8).** Con las coberturas de la foto, para cada golpe sobre la casilla de una pieza `p`:
   - Si `p` tiene un escudero `e` en `Coberturas()` y el dueño del origen **no** es el dueño de `e`, el golpe es de `e`: se suma al `bruto_pieza(e)`, junto a todo lo que ya caía sobre la casilla de `e`. Eso vale para el ataque directo, el centro y el área de la catapulta y la explosión.
   - El escudero resta **su** escudo **una sola vez** sobre esa suma y pierde vida con su propia vida: `neto = max(0, bruto − escudo_e)`.
   - Si el neto supera su vida, **muere y el exceso se pierde**: no pasa al aliado ni a otra pieza. El aliado sale sin daño de esa resolución (Cobertura, C1).
   - El aliado cubierto recibe **solo lo que no se desvía**: los golpes de su propio bando (fuego amigo), que `receptor` deja en él. Su escudo se resta una vez sobre eso.
   - **El tramo no se redirige** (regla 5): si el cubierto es un arquero sobre su tramo, el tramo recibe `bruto_casilla` completo.
   - **La cobertura de J no desvía nada en la resolución de J**: todos los golpes son de J, así que `receptor` es la identidad. Las coberturas de K, asignadas en su turno anterior, son las que desvían (Cobertura, regla 7).
8. **Tramos y explosión sobre muros (cierra el supuesto de Castillo, #5).**
   - Un tramo en pie con `bruto_casilla(c)` queda con `max(0, vida − bruto_casilla)` (Castillo, F1). En 0 pasa a **Roto** para siempre; `tramosRotos` de su dueño sube y `puedeReclutar` rige desde la tabla del siguiente turno de ese dueño (Castillo, regla 10).
   - Cuentan **todos** los golpes: directo, centro y área de la catapulta y explosión, de **cualquier bando** (fuego amigo, propio o rival). El tramo no tiene escudo ni se redirige.
   - **La explosión sobre muros queda confirmada tal como la aprobó el jugador (2026-11-25):** 30 de daño a cada tramo en pie vecino al demoledor, sin Grito. Con `vida_tramo` ≤ 30 rompe cada tramo de un golpe: **hasta 3** si está pegado a un tramo recto (2 junto a una esquina), así que una sola pieza de 10 de vida puede cortar el reclutamiento rival. Es un borde exacto: con `vida_tramo` 31 deja cada tramo en 1 y no corta nada. El demoledor muere al explotar (regla 9); se mide en el MVP (Open Questions).
   - Un demoledor pegado a **su propio** muro lo rompe igual (fuego amigo): puede cortar su propio reclutamiento. Es legal, no suma a `vidaQuitada` y la vista previa lo muestra.
   - El golpe sobre un hueco solo daña a la pieza que haya en él.
9. **Muerte.** Una pieza con vida nueva 0 **sale del estado al terminar el cálculo**, no durante: hasta entonces ocupa su casilla y cuenta en la foto. Quedan sin ocupante las casillas de las piezas muertas. **Un demoledor con explosión asignada sale siempre**, haya o no alguien alrededor, y aunque también reciba daño en esa resolución: su salida es la orden de explotar, no una consecuencia de su vida (motivo `Explosion`). Si recibe golpes, igual se emite su `DanoRecibido`. Un arquero cuyo tramo se rompe pero sobrevive queda en la casilla, ahora hueco, como arquero común (Castillo, regla 9); si muere, la casilla queda libre. La muerte del Rey la evalúa la Máquina después (`EvaluateMatchEnd`, regla 13); Resolución no corta el cálculo por ella.
10. **Coberturas después de resolver.** Al terminar el cálculo y después de los eventos de muerte, cada cobertura `(e, a)` cuyo escudero o aliado murió se borra con `CoberturaQuitada(e, a, motivo)` de Cobertura (regla 9): `EscuderoMuerto` si murió `e` (también si murieron los dos), `AliadoMuerto` si solo murió `a`. Salen por ID de escudero ascendente. Las demás coberturas siguen: las de K caducan al abrirse su siguiente turno y las de J valen en la resolución de K (Máquina, regla 4.1).
11. **`vidaQuitada` (cierra el supuesto de la Máquina, #3, regla 15).** Al terminar el cálculo, `vidaQuitada[J]` suma la **vida realmente perdida** (`vidaAntes − vidaDespues`) por todos los receptores cuyo dueño es **K**: sus piezas y los tramos de su castillo (F4). Se escribe una sola vez por resolución. Precisiones:
    - Cuenta lo perdido, no lo golpeado: una pieza con 6 de vida que recibe 10 de neto suma 6; el exceso perdido sobre un escudero y lo que el escudo restó no suman.
    - Lo que absorbe un escudero al cubrir cuenta como vida del escudero (Máquina, regla 15).
    - **El fuego amigo no cuenta**: receptores de J (piezas o tramos propios) y la salida del demoledor al explotar no suman.
    - En la casilla de un arquero sobre su tramo, la pieza y el tramo se cuentan **por separado**: un golpe de 8 con escudo efectivo 4 suma 4 + 8.
    - **Es solo estadística**: ninguna regla la lee (jugador, 2026-11-25). La cota de la Máquina (`estadistica_max × (C_MAX − 1) + 16 × vida_tramo`) se confirma (F4).
12. **Golpe combinado.** Para cada receptor (pieza o tramo), `atacantesRivales` es la cantidad de **piezas atacantes distintas, del rival del dueño del receptor**, cuyos golpes llegaron a él: los de la casilla del tramo, o los que `receptor` mandó a la pieza (también el directo, el centro o el área de la catapulta y la explosión). `combinado` es verdadero con 2 o más. El fuego amigo nunca cuenta (decisión del 2026-11-19): un receptor de J golpeado por J trae 0. Una misma pieza atacante que llega dos veces al mismo receptor (el centro desviado y el área sobre el escudero) cuenta **una**. El escudo que absorbe todo no cambia la marca: dos atacantes con neto 0 siguen siendo combinado. El daño que recibe un escudero al cubrir cuenta como golpe al escudero.
13. **Qué cambia y qué no.** Cambian: vida de piezas y tramos, piezas muertas (salen), `vidaQuitada[J]` y coberturas huérfanas. **No cambian**: posiciones (no hay empujes ni retrocesos), ataques asignados y bonos del Grito (los borra la Máquina al cerrar el turno, regla 11), movimientos usados, fase, tabla o tirada del Modificador, llamadas de reclutamiento. Resolución no usa azar ni reloj.
14. **Eventos (lista cerrada de Resolución) y orden.** Todos los eventos de una resolución salen en este orden de tipos y, dentro de cada tipo, en el orden que se indica; después la Máquina emite los suyos (`TurnoTerminado` y, según haya o no ganador, `PartidaTerminada` o la caducidad de coberturas y `TurnoIniciado`; su orden es de la Máquina):

    | Orden | Evento | Uno por | Dentro del tipo | Campos |
    |---|---|---|---|---|
    | 1 | `AtaqueEjecutado` | asignación | ID de la pieza atacante ascendente | `pieza`, `modo`, `casilla` (el objetivo; para la explosión, la propia) |
    | 2 | `DanoRecibido` | pieza con `bruto_pieza` > 0 | ID de pieza ascendente | `pieza`, `dueno`, `casilla`, `bruto`, `brutoDesviado`, `escudo`, `neto`, `vidaAntes`, `vidaDespues`, `vidaPerdida`, `combinado`, `atacantesRivales` |
    | 3 | `DanoATramo` | tramo en pie con `bruto_casilla` > 0 | casilla (fila, columna) | `casilla`, `dueno`, `bruto`, `vidaAntes`, `vidaDespues`, `vidaPerdida`, `combinado`, `atacantesRivales` |
    | 4 | `TramoRoto` | tramo que quedó en 0 | casilla (fila, columna) | `casilla`, `dueno`, `tramosRotosDelDueno` (el contador del dueño **al terminar** la resolución) |
    | 5 | `PiezaMuerta` | pieza que sale | ID de pieza ascendente | `pieza`, `dueno`, `tipo`, `casilla`, `motivo` (`Dano` o `Explosion`) |
    | 6 | `CoberturaQuitada` | cobertura huérfana | ID de escudero ascendente | los de Cobertura, regla 15 |

    `brutoDesviado` es la parte del `bruto` que vino de golpes dirigidos a la casilla del aliado cubierto (0 si la pieza no es un escudero con cobertura vigente); sirve para dibujar el desvío. `escudo` es el `escudo_ef` de la foto. Los eventos traen lo necesario para animar, sonar o medir sin leer el estado (Librería, regla 8); su forma canónica en bytes es del ADR. Una pieza con `bruto_pieza` 0 (el aliado cubierto cuyo daño se desvió entero) **no** tiene `DanoRecibido`.
15. **Determinismo, vista previa y memoria.** El resultado **no depende del orden** de cálculo: la foto es inmutable y la suma entera es conmutativa; el orden de atacantes (ID) y de receptores (ID o casilla) fija solo el orden de los eventos (Librería, regla 15). Todo es aritmética entera de 32 bits, sin `float`, y las cotas de F1 a F4 garantizan que no hay desborde. La vista previa (#16) llama a esta misma operación sobre una **copia** y devuelve exactamente la misma lista de eventos (Librería, regla 9): no existe un segundo cálculo. En régimen estable no reserva memoria: los golpes de una resolución son como mucho 61 × 9 = 549 y los eventos como mucho 393 (61 + 122 + 32 + 32 + 122 + 24), cotas para los buffers por partida (cómo, lo fija el ADR). El costo es O(golpes + piezas + tramos), dentro de `comando_presupuesto_ms` y `vista_previa_presupuesto_ms`.
16. **Lo que el jugador no puede hacer.** Elegir el orden de los golpes, dirigir un golpe solo al tramo o solo a la pieza de una casilla, frenar o desviar una explosión propia, esquivar o mover una pieza en medio de la resolución, hacer que el exceso sobre un escudero llegue al aliado, ni resolver los ataques de a uno.

### States and Transitions

Resolución **no guarda estado**: no hay nada que persista entre dos resoluciones ni que entre a la huella. Lo que tiene es un **pipeline** de pasos, que ocurre completo dentro de un solo comando aceptado (no se puede interrumpir ni consultar a medias):

| Paso | Qué hace | Lee | Produce |
|---|---|---|---|
| 1. Foto | Congela las estadísticas efectivas, la vida, las casillas y las coberturas (regla 3) | Estado | La foto |
| 2. Golpes | Convierte cada asignación en golpes `(casilla, daño, origen)` (regla 4) | Lista de #7, foto | Golpes (temporales) |
| 3. Suma | `bruto_casilla` por casilla (F1) | Golpes | Bruto por casilla |
| 4. Reparto | Decide el receptor de cada golpe sobre una pieza y arma el `bruto_pieza` de cada receptor (reglas 5 y 7) | Foto, `Coberturas()` | Bruto por pieza y por tramo, atacantes distintos |
| 5. Neto | Resta el escudo una vez y calcula vida nueva y vida perdida (F3, F4) | Foto | Resultado por receptor |
| 6. Aplicar | Escribe vida, rompe tramos, retira a los muertos y a los demoledores que explotaron, suma `vidaQuitada` (reglas 8, 9 y 11) | Resultado | Estado nuevo |
| 7. Limpiar | Borra las coberturas con una pieza muerta (regla 10) | Estado nuevo | Estado nuevo |
| 8. Eventos | Arma la lista en el orden de la regla 14 | Resultado | Eventos |

Los pasos 2 a 5 no escriben en el estado: es lo que permite que la vista previa los corra sobre una copia y que el orden de cálculo no importe.

**Estado de los receptores durante una resolución** (a efectos de eventos y de `vidaQuitada`):

| Entidad | De → a | Disparador | Efecto |
|---|---|---|---|
| Pieza | Viva → Viva (sin cambio) | `neto` = 0 (el escudo absorbió todo) o `bruto_pieza` = 0 | Sin pérdida de vida; con `bruto_pieza` > 0 igual hay `DanoRecibido` |
| Pieza | Viva → Viva (herida) | 0 < `neto` < vida | Baja la vida; `vidaPerdida` = `neto` |
| Pieza | Viva → Muerta | `neto` ≥ vida | Sale del estado al terminar; `vidaPerdida` = vida; `PiezaMuerta` motivo `Dano` |
| Demoledor | Vivo → Muerto | Tiene una explosión asignada | Sale siempre; `PiezaMuerta` motivo `Explosion` |
| Tramo | En pie → En pie | 0 < `bruto_casilla` < vida | Baja la vida; `DanoATramo` |
| Tramo | En pie → Roto | `bruto_casilla` ≥ vida | Vida 0, permanente; `DanoATramo` y `TramoRoto`; la casilla pasa a ser hueco |
| Tramo | Roto → cualquiera | **No existe** | Un hueco no recibe daño ni se repara (Castillo, regla 5) |
| Arquero | En el muro → Común | Su tramo se rompe y él sobrevive | Queda en la casilla, ahora hueco, sin el +2 (Castillo, regla 9) |
| Cobertura | Vigente → Borrada | Muere el escudero o el aliado | `CoberturaQuitada` (`EscuderoMuerto` o `AliadoMuerto`), después de las muertes |
| Cobertura | Vigente → Vigente | Ninguno de los dos muere | Sin evento; caduca al abrirse el siguiente turno de su dueño (Máquina) |

### Interactions with Other Systems

| Sistema | Qué entra | Qué sale | Dueño de la interfaz |
|---|---|---|---|
| #1 Librería | `Resolver` y `TurnTimeout` de atacar como puertas; determinismo, enteros, orden por ID; vista previa = misma operación | Estado nuevo y eventos; la forma canónica y los topes de buffers | Librería (contrato); este GDD (reglas y eventos) |
| #2 Catálogo | `ataque`, `escudo`, `vida`, `danoArea` de cada tipo; IDs 2 (escudero) y 5 (demoledor) | — | Catálogo |
| #3 Máquina | La llamada tras `Resolver`/`TurnTimeout` de atacar | Estado nuevo, eventos y `vidaQuitada`; la Máquina corre `EvaluateMatchEnd`, limpia bonos y asignaciones y abre el turno siguiente | Máquina (fases y fin); este GDD (el cálculo) |
| #4 Tablero | `dentro`, `ocupante`, contorno de 8 | — | Tablero |
| #5 Castillo | Vida de cada tramo; `enMuro` y el bono de escudo vía `EffectiveStats`; reglas 4, 5, 7, 8 y 9 | Vida nueva, tramos rotos, arqueros que pierden el bono, muertes | Castillo (reglas del muro); este GDD (aplicación) |
| #7 Alcance | Lista `(pieza, objetivo, modo)` por ID ascendente | — | Alcance (lista y validez); este GDD (cálculo) |
| #8 Cobertura | `Coberturas()` y `receptor(pieza, origen)` | `CoberturaQuitada` con motivo `EscuderoMuerto` o `AliadoMuerto` | Cobertura (quién); este GDD (cuánto) |
| #11 Modificador | `ataque_ef` con el Grito, salvo en la explosión, vía `EffectiveStats` | — | Modificador; Resolución no suma bonos por su cuenta |
| #16 Vista previa | — | La misma operación sobre una copia; los eventos como contenido de las placas MUERE y ROTO y de los números flotantes | Vista previa lee; este GDD produce |
| #18 Render, #22 Audio | — | Eventos de la regla 14, con `combinado` y `atacantesRivales` | Presentación, solo lectura |
| #19 Métricas | — | `vidaQuitada`, eventos de daño y de muerte | Métricas, solo lectura |
| #24 Tutorial, #25 Rival IA | — | La misma operación sobre una copia (IA: evaluar jugadas) | Cada GDD, solo lectura |

**Supuestos provisionales que quedan:** ninguno sobre los GDD vecinos (todos existen). Quedan sin GDD los lectores de los eventos (#16, #18, #19, #22, #24, #25): la forma de cada evento es de este GDD y ellos la confirman al escribirse.

## Formulas

Resolución **no define ningún número de juego propio**: ataque, escudo, vida y área son del Catálogo (#2), la vida del tramo es de Castillo (#5) y los bonos son de Castillo y del Modificador (#11). Lo que define es **cómo se combinan** (suma por casilla, reparto por cobertura, una sola resta de escudo, qué se cuenta como vida quitada). F1 a F5 son el cálculo real del núcleo; **F6 es un oráculo de tests de balance**: no entra al núcleo ni a la vista previa.

### F1. `bruto_casilla`

La fórmula `bruto_casilla` se define así:

`bruto_casilla(c) = Σ dano(g)` sobre todos los golpes `g` de la resolución que caen en la casilla `c`, donde
`dano(g) = ataque_ef(origen)` para el directo, el centro del área y la explosión, y `dano(g) = danoArea(tipo(origen))` para las vecinas del área.

**Variables:**

| Variable | Type | Range | Source | Description |
|---|---|---|---|---|
| `c` | casilla | columna 0–23, fila 0–14 | calculated (golpe) | Casilla que recibe los golpes. Los golpes fuera de la grilla no existen (regla 4) |
| `g` | golpe | 0 a 549 por resolución | calculated (regla 4) | Una asignación genera de 1 a 9 golpes, como mucho uno por casilla |
| `ataque_ef(origen)` | int | 1 – 1.010 | calculated (`EffectiveStats`) | `ataque` del Catálogo (1–1.000) más `bono_grito_ataque` (0–10) salvo en el modo Explosión (Modificador, F3) |
| `danoArea(tipo)` | int | 1 – 1.000 | data file (Catálogo, regla 6, F4 del formato) | Solo para el modo Área, que exige `danoArea` > 0. Sin bono |

**Output Range:** 0 a 61 × 1.010 = **61.610** por casilla (cada una de las 61 piezas de J golpea una casilla a lo sumo una vez, con un daño de hasta 1.010). Está dentro del dominio que Castillo asume para `bruto` (hasta 24.999.999) y lejos de 2.147.483.647. Con una casilla sin golpes es 0.
**Example:** 3 soldados (8) y 1 arquero (6) sobre la misma casilla: 30. Una catapulta con Grito centrada en esa casilla suma 12 al centro y 6 a cada vecina. Un demoledor con Grito suma 30, no 32.

### F2. `bruto_pieza`

La fórmula `bruto_pieza` se define así:

`bruto_pieza(q) = bruto_propio(q) + bruto_desviado(q)`
`bruto_propio(q) = Σ dano(g)` de los golpes sobre la casilla de `q` con `receptor(q, origen(g)) = q`
`bruto_desviado(q) = Σ dano(g)` de los golpes sobre la casilla del aliado `a` que `q` cubre, con `receptor(a, origen(g)) = q` (0 si `q` no cubre a nadie)

**Variables:**

| Variable | Type | Range | Source | Description |
|---|---|---|---|---|
| `q` | ID de pieza viva en la foto | 1 – próximo ID − 1 | calculated (estado) | Receptor |
| `dano(g)`, `g` | int | como en F1 | calculated (F1) | Daño de cada golpe |
| `receptor(pieza, origen)` | ID de pieza | `pieza` o su escudero | calculated (Cobertura, F2) | Quién recibe un golpe sobre `pieza` con ese origen. Es la identidad sin cobertura, con fuego amigo o con la cobertura del propio jugador del turno |
| `a` | ID de pieza viva | — | calculated (`Coberturas()`) | Aliado que `q` cubre; como mucho uno (Cobertura, regla 3) |

**Output Range:** 0 a 2 × 61.610 = **123.220** (la casilla propia del escudero más la de su aliado; cada atacante llega a cada una a lo sumo una vez). `bruto_desviado` es 0 salvo en un escudero con cobertura vigente. Un aliado cubierto tiene `bruto_propio` solo por fuego amigo, que `receptor` no desvía.
**Example:** el arquero A, cubierto por el escudero E, recibe 3 soldados rivales (24): `bruto_pieza(A)` = 0 y `bruto_pieza(E)` = 24 (todo desviado). Si además 1 soldado apunta a E: `bruto_pieza(E)` = 8 + 24 = 32. Una catapulta rival con el centro (10) en A y el área (6) en E: `bruto_propio(E)` = 6, `bruto_desviado(E)` = 10, total 16.

### F3. `dano_neto_pieza`

La fórmula `dano_neto_pieza` se define así:

`neto(q) = max(0, bruto_pieza(q) − escudo_ef(q))`
`vida_nueva(q) = max(0, vida(q) − neto(q))`
`vida_perdida(q) = vida(q) − vida_nueva(q) = min(vida(q), neto(q))`

**Variables:**

| Variable | Type | Range | Source | Description |
|---|---|---|---|---|
| `bruto_pieza(q)` | int | 0 – 123.220 | calculated (F2) | Suma que llega a `q` |
| `escudo_ef(q)` | int | 0 – 1.010 | calculated (`EffectiveStats`, foto) | `escudo` del Catálogo (0–1.000) más `BONO_ESCUDO_MURO` (0–10) si `q` es un arquero sobre un tramo propio en pie al empezar. El Grito no lo cambia |
| `vida(q)` | int | 1 – 1.000 | calculated (estado, foto) | Vida antes de resolver. Una pieza viva tiene al menos 1 |

**Output Range:** `neto` 0 a 123.220; `vida_nueva` 0 a `vida(q)`; `vida_perdida` 0 a 1.000. Nunca negativos y nunca más que la vida que había. Un `neto` ≥ `vida` mata y **el exceso se pierde**: `vida_perdida` es la vida que había, no el `neto`. Todo cabe en 32 bits con margen.
**Example:** T (vida 10, escudo 1) recibe 3 y 2: bruto 5, neto 4, vida 6 (Librería, AC-10). El escudero (escudo 5, vida 20) con bruto 24: neto 19, vida 1. Con bruto 40: neto 35, vida 0 y `vida_perdida` 20. El soldado (escudo 3, vida 25) con 3 soldados y 1 arquero (30): neto 27 ≥ 25, muere.
**Relación con el oráculo del Catálogo:** con N golpes de ataque `a` sobre un objetivo sin cobertura, F3 mata si y solo si `N × a ≥ escudo + vida`, que es `piezasParaMatar` (Catálogo, Formulas). El oráculo y el cálculo real tienen que coincidir (AC-33).

### F4. `vida_quitada_resolucion`

La fórmula `vida_quitada_resolucion` se define así:

`quitada(J) = Σ vida_perdida(q)` sobre las piezas `q` de K `+ Σ min(vida(t), bruto_casilla(t))` sobre los tramos `t` del castillo de K
`vidaQuitada'[J] = vidaQuitada[J] + quitada(J)`

**Variables:**

| Variable | Type | Range | Source | Description |
|---|---|---|---|---|
| `J`, `K` | jugador | 0 o 1 | calculated (estado) | Jugador del turno y su rival |
| `vida_perdida(q)` | int | 0 – 1.000 | calculated (F3) | Vida realmente perdida; incluye al escudero que cubre |
| `vida(t)`, `bruto_casilla(t)` | int | 0 – `vida_tramo`, 0 – 61.610 | calculated (estado, F1) | Vida del tramo antes de resolver y daño de su casilla (Castillo, F2) |
| `vida_tramo` | int | 1 – 1.000 | data file (config, Castillo) | Tope de cada uno de los 16 tramos |
| `vidaQuitada[J]` | int | 0 – 1.000.015.000 | calculated (estado) | Acumulado de la partida (Máquina, regla 15) |

**Output Range:** `quitada` de 0 a 61 × 1.000 + 16 × 1.000 = **77.000** por resolución. El acumulado no pasa de `estadistica_max × (C_MAX − 1) + 16 × vida_tramo` = 1.000.015.000: ninguna vida se recupera y un tramo roto no se repara, así que cada pieza y cada tramo aportan a lo sumo su vida inicial **una vez** en toda la partida. **La cota de la Máquina (regla 15) y de la Librería (regla 11) queda confirmada**; sigue por debajo de 2.147.483.647. El fuego amigo, el exceso perdido, lo que restó el escudo y el demoledor que explota no aportan.
**Example:** 3 soldados sobre un arquero cubierto: 19 (el escudero), 0 el arquero. Un golpe de 8 sobre un arquero en el muro (escudo efectivo 4): 4 + 8 = 12. Un tramo con vida 5 que recibe 32: 5. 40 de bruto sobre un escudero (escudo 5, vida 20): 20. Un golpe sobre un receptor de J: 0.

### F5. `atacantes_rivales` (marca de combinado)

La fórmula `atacantes_rivales` se define así:

`atacantesRivales(r) = | { a : a es una pieza atacante, dueño(a) ≠ dueño(r) y algún golpe de a llega a r } |` y `combinado(r) = atacantesRivales(r) ≥ 2`

**Variables:**

| Variable | Type | Range | Source | Description |
|---|---|---|---|---|
| `r` | pieza o tramo | — | calculated (F2, F1) | Receptor. Para una pieza, «llega» es el receptor tras la cobertura; para un tramo, cualquier golpe sobre su casilla |
| `a` | ID de pieza atacante | 1 – 61 piezas de J | calculated (estado) | Se cuenta una vez aunque lo golpee por dos vías |
| `dueño(x)` | jugador | 0 o 1 | calculated (estado) | Dueño de la pieza o del castillo del tramo |

**Output Range:** `atacantesRivales` de 0 a 61; `combinado` es booleano. Es 0 para todo receptor de J (fuego amigo), porque todo atacante es de J.
**Example:** dos soldados sobre T: 2, combinado. Una catapulta cuyo centro cae sobre un arquero cubierto y cuya área cae sobre su escudero: 1 (la misma pieza dos veces), no combinado. Dos atacantes contra un escudo que absorbe todo: 2 y neto 0, igual combinado. Un demoledor y un soldado sobre el mismo vecino: 2.

### F6. `tramos_en_explosion` (oráculo de tests de balance)

La fórmula `tramos_en_explosion` se define así:

`tramos_en_explosion(c) = número de tramos en pie entre las 8 vecinas de c` y `tramos_rotos_por_explosion(c) = número de esos tramos con vida ≤ ataque_ef(demoledor)`

Es el oráculo de la excepción aprobada de Castillo (F3 y F4): sirve para comprobar, con la config vigente, **cuántos tramos rompe una explosión según dónde esté**. **No entra al núcleo ni a la vista previa**: la cuenta real es F1 aplicada a las vecinas.

**Variables:**

| Variable | Type | Range | Source | Description |
|---|---|---|---|---|
| `c` | casilla | columna 0–23, fila 0–14 | calculated (estado) | Casilla del demoledor |
| `ataque_ef(demoledor)` | int | 1 – 1.000 | data file (Catálogo) | 30. Sin Grito (Modificador, regla 7) |
| `vida(t)` | int | 1 – `vida_tramo` | calculated (estado) | Vida de cada tramo vecino |

**Output Range:** `tramos_en_explosion` de 0 a 5. Pegado por fuera a un tramo recto es **3**; junto a una esquina, **2**; en una esquina exterior en diagonal, 1; por **dentro** del castillo (con un hueco ya abierto), 3 contra un lado y hasta 5 en una esquina del interior. `tramos_rotos_por_explosion` es igual a ese número con `vida_tramo` ≤ 30 y vale 0 con `vida_tramo` 31 y tramos enteros.
**Example:** demoledor rojo en `(5, 7)` junto al muro azul de la columna 4: vecinas `(4, 6)`, `(4, 7)`, `(4, 8)`; con `vida_tramo` 30, 3 rotos; con 31, 0 (cada tramo queda en 1). **Cruce con Castillo F4:** `piezas_para_cortar` da 2 demoledores (2 × 1) porque cuenta una pieza contra un solo tramo; con la explosión, **un** demoledor pegado a un tramo recto ya corta el reclutamiento (3 ≥ `TRAMOS_ROTOS_CORTE`). Esa es la excepción aprobada, medida en el MVP.

### Límites de variables

Cada variable de F1 a F5 puesta en su mínimo, su máximo y en 0:

| Variable | Mínimo | Máximo | 0 / fuera de rango |
|---|---|---|---|
| `ataque_ef` | 1 | 1.010 | 0 no existe: el Catálogo exige `ataque` ≥ 1 (regla 6, F2 del formato) |
| `danoArea` en modo Área | 1 | 1.000 | 0 vuelve la pieza un atacante directo (Alcance, regla 5), no un área de 0 |
| `bruto_casilla` | 0 → sin golpe, sin evento | 61.610 | Se acota en vida 0 del tramo y en vida 0 de la pieza; sin desborde |
| `bruto_pieza` | 0 → sin evento (el aliado cubierto cuyo daño se desvió) | 123.220 | — |
| `escudo_ef` | 0 → `neto` = `bruto` | 1.010 | Un `escudo_ef` ≥ `bruto` da `neto` 0, nunca negativo: es la defensa del escudo contra un golpe suelto (pilar 2), no una falla |
| `vida` de pieza | 1 → cualquier `neto` ≥ 1 la mata | 1.000 | 0 no existe en una pieza viva (Librería, regla 11) |
| `vida_tramo` | 1 → todo golpe rompe | 1.000 | 0 es `ConfigInvalida` (Castillo) |
| `bono_grito_ataque` | 0 → el Grito no suma daño | 10 → 1.010 | Sin efecto sobre el demoledor ni sobre `danoArea` |
| `vidaQuitada` | 0 | 1.000.015.000 | — |

No se encontró ninguna entrada que produzca un valor inutilizable (división por cero, daño negativo o resultado sin cota). El único valor «sin uso» es `neto` 0: un golpe que el escudo absorbe entero (por ejemplo un escudero, ataque 4, contra el Rey, escudo 5). Es intencional y se ve antes en la vista previa.

### Lazos de retroalimentación

- **Positivo (nombrado, no eliminado):** matar piezas deja al rival con menos piezas y menos ataques el turno siguiente, y a quien tiene más piezas lo ayuda a seguir matando. Es la tensión con el pilar 3. Lo amortigua el Modificador (reparto liberado y tablas «Pierde», más reclutas), el costo de cada combo (hay que juntar `escudo + vida` sobre cada objetivo) y que el exceso se pierde. Se mide con el umbral 3c (2 de 3, Modificador F5).
- **Positivo, del muro:** romper tramos corta el reclutamiento y deja al rival con menos piezas (Castillo, lazos). La explosión sobre muros lo acerca, y es la excepción aprobada.
- **Negativo:** cada golpe que va a un tramo o a un escudero cubierto no va al ejército o al Rey (costo de oportunidad); el demoledor se paga con una pieza; el fuego amigo lo paga el que dispara.

## Edge Cases

Valores del Catálogo v5: soldado ataque 8, escudo 3, vida 25 · arquero 6, 2, 15 · escudero 4, 5, 20 · catapulta 10 (área 6), 2, 15 · jinete 8, 3, 20 · demoledor 30, 1, 10 · Rey 8, 5, 30.

### Golpes y geometría

- **If el centro del área cae en un borde (no esquina):** hay 6 golpes (centro y 5 vecinas); **en una esquina**, 4 (centro y 3). Los que caerían fuera del tablero se descartan sin evento (regla 4).
- **If el centro del área cae en una casilla vacía o en un hueco:** las vecinas golpean igual; sale `AtaqueEjecutado` y no hay evento de daño en el centro.
- **If el centro o las vecinas del área caen sobre una pieza o un tramo propios:** reciben el golpe como fuego amigo, sin excepción. La vista previa lo muestra antes (Alcance, Player Fantasy).
- **If dos áreas, o un área y un ataque directo, coinciden en una casilla:** `bruto_casilla` suma todo y el escudo del receptor se resta **una vez** sobre la suma, no una por ataque (regla 6).
- **If el área de una catapulta caería sobre la propia catapulta:** con el Catálogo vigente es imposible: la zona ciega le impide apuntar a una vecina y el área llega solo a distancia 1 del centro. Con un Catálogo cuyo `alcanceMin` fuera 1, la catapulta se golpearía como cualquier pieza propia, sin caso especial.
- **If el demoledor está en un borde o una esquina:** golpea solo las vecinas que están dentro del tablero (5 o 3) y muere igual.
- **If el demoledor no tiene a nadie alrededor:** sale `AtaqueEjecutado`, ningún `DanoRecibido` y `PiezaMuerta` con motivo `Explosion`. Es un sacrificio vacío, legal (Alcance, regla 7).
- **If hay aliados, escuderos o el Rey junto al demoledor:** cada uno recibe 30 menos su escudo (fuego amigo). El Rey (escudo 5, vida 30) pierde 25. La vista previa lo muestra antes.
- **If dos demoledores propios están pegados y los dos explotan:** cada uno golpea al otro (30 − 1 = 29 ≥ 10) y los dos salen por su explosión; **no hay cascada**: una pieza que no esté en las vecinas de ninguno de los dos no recibe nada, aunque la explosión del otro haya matado a un vecino. Los dos tienen su `DanoRecibido` (bruto 30, escudo 1, neto 29, vida perdida 10) y su `PiezaMuerta` con motivo `Explosion`.
- **If una explosión o un área mata a una pieza propia con ataque asignado:** esa pieza golpea igual (regla 3).
- **If un ataque directo apunta a una casilla con un arquero sobre su tramo en pie:** el golpe va a la pieza (menos su escudo efectivo) y al tramo (completo) a la vez (regla 5).
- **If un golpe cae sobre un hueco sin pieza, o sobre una casilla vacía:** se pierde, sin evento.

### Cobertura

- **If 5 soldados rivales (40) apuntan al aliado cubierto:** el escudero recibe 40 − 5 = 35 ≥ 20, muere y el exceso (15) se pierde; el aliado conserva toda su vida (Cobertura, C1). `vidaQuitada` suma 20.
- **If la catapulta rival tiene el centro (10) en el cubierto y el área (6) sobre su escudero vecino:** el escudero recibe 10 desviados + 6 propios = 16, menos 5 de escudo = 11. Es el contra-combo (Cobertura, Edge Cases).
- **If la catapulta rival tiene el centro en el escudero y el área toca al cubierto:** el mismo total: los 10 caen sobre el escudero y los 6 sobre el cubierto se desvían a él (16). Un solo escudo restado.
- **If un demoledor rival explota junto al cubierto y al escudero:** los 30 del cubierto se desvían a su escudero, que ya recibía sus propios 30: 60 − 5 = 55 lo matan y el aliado no pierde nada. El demoledor solo mata al escudero, no al aliado.
- **If K tiene dos coberturas y un área toca a los dos aliados y a los dos escuderos:** cada golpe sobre un aliado va a **su** escudero, y cada escudero resta su escudo una vez. No hay cadena: ningún escudero está cubierto (Cobertura, regla 4).
- **If el cubierto es un arquero sobre su tramo:** la parte de la pieza va al escudero y el tramo recibe `bruto_casilla` completo (regla 5). El tramo puede romperse sin que el arquero ni el escudero pierdan nada.
- **If el cubierto es el Rey:** sale sin daño mientras la cobertura esté vigente; si el escudero muere, el exceso se pierde y el Rey sigue intacto en esa resolución. La cobertura se borra después y el Rey queda expuesto en el turno siguiente del rival si su dueño no lo cubre de nuevo (Cobertura, Edge Cases).
- **If J tiene una cobertura asignada ese turno y dispara contra el aliado cubierto o contra su escudero:** no se desvía nada, porque `receptor` es la identidad con fuego amigo: cada uno recibe lo que cae en su casilla, con su propio escudo.
- **If el aliado cubierto muere en esta resolución:** solo puede ser por fuego amigo de J en su propio turno (el daño del rival se desvía entero). Se borra la cobertura con motivo `AliadoMuerto`.
- **If mueren a la vez el escudero y el aliado:** un solo `CoberturaQuitada` con `EscuderoMuerto` (Cobertura, regla 9).

### Tramos y arquero

- **If `bruto_casilla` es exactamente la vida del tramo:** se rompe (≥). Con `vida − 1` queda en 1 y sigue en pie: es el borde exacto de la excepción del demoledor (30 contra `vida_tramo` 31).
- **If el bruto supera la vida del tramo:** se rompe, `vidaPerdida` es su vida y el exceso se pierde.
- **If el tramo bajo un arquero se rompe y el arquero sobrevive:** la pieza usó el +2 de escudo en esta resolución (foto); queda en la casilla, ahora hueco, como arquero común (Castillo, regla 9). Un arquero de 15 de vida sobre un tramo de 10 con bruto 12: tramo 0, arquero 15 − (12 − 4) = 7.
- **If el arquero muere y el tramo no se rompe:** la casilla queda libre y el tramo sigue en pie, vacío.
- **If el fuego amigo rompe un tramo propio:** se rompe como cualquier otro, `tramosRotos` del dueño sube (puede cortar su propio reclutamiento) y `vidaQuitada` no cambia (Castillo, AC-28).
- **If una explosión está pegada a un tramo y a una pieza propia a la vez:** golpea las dos casillas; el tramo recibe 30 sin escudo y la pieza 30 menos su escudo.
- **If la resolución rompe el segundo tramo de un castillo y mata a su Rey:** la partida termina; el estado final conserva los tramos rotos (Castillo, AC-29).
- **If el golpe cae sobre un hueco donde hay una pieza enemiga:** solo recibe la pieza; no hay tramo.
- **If `vida_tramo` es 1:** cualquier golpe rompe el tramo; una explosión rompe todos los vecinos. **Si es 1.000:** la explosión de 30 no rompe nada de un golpe.

### Muertes y fin de partida

- **If mueren los dos Reyes en la misma resolución:** `DobleRey`, gana J (Máquina, regla 13). Resolución emite todos sus eventos antes de `PartidaTerminada`.
- **If muere solo el Rey de J por fuego amigo:** gana K; `vidaQuitada` de J no cambia.
- **If una pieza muere pero tenía un ataque asignado:** golpeó igual (regla 3); sus golpes ya están en `bruto_casilla`.
- **If la pieza muerta cubría o estaba cubierta:** la cobertura se borra al final, después de las muertes (regla 10).
- **If la resolución termina la partida:** la Máquina no abre el turno siguiente ni borra coberturas por caducidad; el estado `Terminada` conserva todo lo que dejó Resolución.
- **If todos los golpes caen sobre casillas vacías:** solo salen los `AtaqueEjecutado`; no cambia ninguna vida.

### Estadística

- **If el neto supera la vida del receptor:** `vidaPerdida` es su vida y suma esa; el exceso no suma.
- **If todo el daño es fuego amigo:** `quitada` es 0.
- **If un escudero absorbe y muere:** suma su vida restante; el aliado no suma nada.
- **If el receptor es un arquero en el muro:** suma la pieza y el tramo por separado (golpe de 8: 4 + 8 = 12).
- **If un golpe cae sobre un tramo ya roto:** no suma nada.
- **If el demoledor se inmola junto a un enemigo y un aliado:** suma solo lo que perdió el enemigo; el aliado y el demoledor no suman.

### Determinismo y carga

- **If las asignaciones se entregan en otro orden** (solo en tests; la Librería las guarda por ID): el estado final es el mismo, porque la suma es conmutativa y la foto no cambia.
- **If se resuelve dos veces el mismo estado** (vista previa y después `Resolver`): las dos listas de eventos son idénticas y la vista previa no cambia la huella.
- **If llega `TurnTimeout` de atacar a mitad de un plan:** se resuelve lo asignado hasta ese momento, igual que `Resolver` (Alcance, Edge Cases).
- **If se agota el reloj de partida o llega `Rendirse` durante `Atacar`:** no se llama a Resolución y los ataques asignados no se resuelven (Máquina, regla 14).
- **If hay Distracción, Grito o Arremetida:** mismas reglas; el Grito solo cambia `ataque_ef` (Modificador, regla 7), nunca el área, la explosión ni el escudo.
- **If la lista de asignaciones está vacía:** regla 2.
- **If el estado de entrada viola una invariante** (asignación inválida, cobertura imposible): no es un caso de juego. La carga lo rechaza con `EstadoInvalido`; dentro de `Aplicar` sería `ErrorInterno`.

### Estrategias degeneradas revisadas

- *Cubrir al Rey cada turno (tortuga):* con el exceso perdido, el Rey no pierde vida mientras el escudero absorba 25 de bruto; no es inmunidad, porque cada cobertura rota cuesta una pieza, la catapulta castiga la cobertura (centro y área caen juntos) y un demoledor mata al escudero solo. Es la primera cosa a medir (Open Questions).
- *Demoledor de asedio:* una pieza de 10 de vida puede cortar el reclutamiento rival (excepción aprobada). Lo contienen su movimiento de 6, su vida 10 y que muere al explotar. Se mide la tasa de cortes y cuántos demoledores llegan al muro (Open Questions).
- *Fuego amigo deliberado* (explotar demoledores propios para bajar la cuenta de piezas o disparar al muro propio para abrirse paso): legal y siempre a la vista en la vista previa; cuesta piezas o muros permanentes. Vigilado en Alcance y Modificador.
- *Juntar más de 25 de bruto sobre un cubierto:* inútil, el exceso se pierde. La vista previa lo muestra como exceso para que el jugador no lo malgaste.
- *Resolver sin asignar nada para pasar el turno:* legal; no hay daño y el costo es del reloj propio.

## Dependencies

**Depende de:**

| Sistema | Tipo | Interfaz | ¿El otro lo nombra? |
|---|---|---|---|
| #1 Librería de reglas | Dura | `Resolver` y `TurnTimeout` de atacar como puertas; enteros de 32 bits, orden por ID, vista previa = misma operación, forma canónica de los eventos | Sí (reglas 5, 8, 9 y 15) |
| #2 Catálogo de piezas | Dura | `ataque`, `escudo`, `vida`, `danoArea`; IDs de tipo 2 (escudero) y 5 (demoledor) | Sí (fila #9, «Pendiente» hasta este GDD) |
| #3 Máquina de turno | Dura | Llama a Resolución tras `Resolver` o `TurnTimeout`; recibe estado, eventos y `vidaQuitada`; es dueña de `EvaluateMatchEnd` y del cierre | Sí (Interactions, fila 9; reglas 11, 13 y 15) |
| #4 Tablero y despliegue inicial | Dura | `dentro`, `ocupante`, contorno de 8 casillas y límites de la grilla | Sí (fila «#8, #9») |
| #5 Castillo y muros | Dura | Vida de cada tramo, `enMuro` y su bono vía `EffectiveStats`, reglas 4, 5, 7, 8 y 9, `vida_tramo` | Sí (reglas 7 y 8, Interactions) |
| #7 Alcance y asignación | Dura | Lista `(pieza, objetivo, modo)` por ID ascendente; el modo Explosión y el Área | Sí (fila #9; reglas 5, 7 y 16) |
| #8 Cobertura del escudero | Dura | `Coberturas()` y `receptor(pieza, origen)` (F2); motivos `EscuderoMuerto` y `AliadoMuerto` de `CoberturaQuitada` | Sí (reglas 6, 9, 10 y 16) |
| #11 Modificador | Blanda | El Grito suma `bono_grito_ataque` a `ataque_ef` salvo en la explosión y el área, vía `EffectiveStats`: Resolución no suma bonos por su cuenta | Sí (regla 7 y Edge Cases) |

**Dependen de este sistema:**

| Sistema | Tipo | Qué toma | ¿Está nombrado en su GDD? |
|---|---|---|---|
| #3 Máquina de turno | Dura | Estado nuevo, eventos y `vidaQuitada` para correr `EvaluateMatchEnd` y cerrar el turno (ciclo ya cortado: la Máquina no sabe cómo se calcula el daño) | Sí |
| #8 Cobertura | Dura | El reparto numérico y el momento de la muerte de la cobertura | Sí |
| #5 Castillo | Dura | Aplicación del daño al tramo, tramos rotos y muertes | Sí |
| #7 Alcance | Dura | Consumo de la lista y la zona de explosión | Sí |
| #16 Vista previa | Dura | La misma operación sobre una copia; los eventos como contenido de MUERE, ROTO y números flotantes | Pendiente (sin GDD) |
| #18 Render, animación y feedback | Dura | Los eventos, con `combinado` y `atacantesRivales`, para animar | Pendiente (sin GDD) |
| #19 Métricas locales | Blanda | `vidaQuitada` y los eventos de daño y de muerte | Pendiente (sin GDD) |
| #22 Audio y SFX | Blanda | Los eventos y la marca de combinado | Pendiente (sin GDD) |
| #24 Tutorial, #25 Rival IA | Dura | Resolver sobre una copia para evaluar jugadas | Pendiente (sin GDD) |

**Ciclo cortado:** Máquina ↔ Resolución (fin de partida por muerte del Rey) se corta por contrato: la Máquina es dueña de `EvaluateMatchEnd(estado)` y lo consulta **después** de resolver, sin saber cómo se calcula el daño; Resolución no sabe que la partida puede terminar y no corta el cálculo por la muerte de un Rey (regla 9).

**No dependen ni dependen de Resolución:** Movimiento (#6, lee dónde quedaron las piezas), Reclutamiento (#10, lee `puedeReclutar` de Castillo, no de acá), Timer (#12, solo manda `TurnTimeout`), Localización (#14: Resolución no tiene códigos de rechazo ni texto propio; los textos de MUERE y ROTO son de #16 y #14) y Entrada (#13).

**Supuestos provisionales:** ninguno sobre un GDD vecino: los ocho existen y ya nombran a #9. Los lectores de los eventos (#16, #18, #19, #22, #24, #25) no tienen GDD: la forma de los eventos de la regla 14 es de este GDD y ellos la confirman al escribirse.

## Tuning Knobs

**Resolución no tiene perillas propias.** Toda variable `data file` de sus fórmulas pertenece a otro GDD, que es la fuente de verdad; acá se señala qué hace cada una en la resolución y con cuáles interactúa (no se duplican). Sí hay **reglas fijas** que no son perillas: cambiarlas sube la versión de reglas de la Librería (regla 12) y reabre los chequeos de balance que dependen de ellas.

### Perillas ajenas que cambian lo que produce Resolución

| Perilla (dueño) | Valor hoy | Rango seguro | Qué cambia en la resolución | Si sube mucho | Si baja mucho |
|---|---|---|---|---|---|
| `ataque` por tipo (Catálogo) | 8 · 6 · 4 · 10 · 8 · 30 · 8 | Lo fijan I1 a I3 del Catálogo | Daño de cada golpe directo y del centro del área; con él `piezasParaMatar` | Una pieza mata sola (rompe I1 o I2) | Un golpe suelto queda por debajo del escudo y no hace nada (neto 0): escudero (4) contra Rey (5) |
| `escudo` por tipo (Catálogo) | 3 · 2 · 5 · 2 · 3 · 1 · 5 | 0–1.000, con I1 a I3 | Se resta **una vez** sobre la suma | Un tipo se vuelve inmune a golpes sueltos (pilar 2: es lo buscado hasta que ningún combo razonable lo pasa) | 0 deja pasar todo; el combo deja de ser necesario |
| `vida` por tipo (Catálogo) | 25 · 15 · 20 · 15 · 20 · 10 · 30 | 1–1.000, con I1 a I3 | Cuánto neto mata | Partidas largas (pilar 4) | Muertes de un golpe en todo el tablero |
| `danoArea` (Catálogo) | 6 | 1–10 (I3: `danoArea − escudo < vida` del demoledor) | Daño de las 8 vecinas del área | 11 o más mata solo al demoledor (rompe I3) y el área reemplaza al combo | 0 convierte a la catapulta en atacante directo |
| `vida_tramo` (Castillo, config) | 30 | 18–40, sin medir (registro) | Cuántos golpes rompen un tramo y si la explosión lo rompe de uno | **31 o más:** la explosión de 30 deja cada tramo en 1 y el demoledor deja de cortar solo | 1: cualquier golpe rompe; una explosión abre todos los vecinos |
| `bono_grito_ataque` (Modificador, config) | 2 | 0–10 | Suma al directo y al centro del área, nunca a la explosión ni al área | 10: cada soldado pega 18; la catapulta 20 contra el demoledor | 0: el Grito no suma daño |
| `BONO_ESCUDO_MURO` (Castillo, constante) | 2 | 0–10 | Escudo efectivo del arquero en el muro, congelado en la foto | Un arquero en el muro casi no recibe neto | 0: subir al muro no protege |

### Interacciones entre perillas

- **`vida_tramo` × `ataque` del demoledor (30):** `vida_tramo` ≤ 30 y el demoledor rompe cada tramo de un golpe; con 31 pasa a no romper ninguno de uno. Es el borde exacto de la excepción aprobada (regla 8; F6). Mover cualquiera de los dos cambia si una pieza corta el reclutamiento rival.
- **`danoArea` × (`escudo` + `vida`) del demoledor (11):** el techo del área lo fija el demoledor, la pieza más frágil (I3). Con el Grito, el centro de la catapulta (12) ya mata solo al demoledor: es la excepción ya aprobada y dura un turno.
- **`escudo` + `vida` del escudero (25) × `ataque` de cada tipo:** es el bruto que el rival tiene que juntar para matar a un escudero y quitar su cobertura (Cobertura, F3). Si algún tipo (salvo el demoledor) llegara a `piezasParaMatar` 1 contra el escudero, una sola pieza anularía la cobertura: es una regresión (C2).
- **`bono_grito_ataque` × `escudo`:** con el Grito, tres golpes de 8 pasan a 30; el neto contra escudo 3 sube de 21 a 27. Cambia cuántas piezas hacen falta para matar sin tocar el Catálogo.

### Reglas fijas (no son perillas)

| Regla | Dónde vive | Qué rompería cambiarla |
|---|---|---|
| El escudo se resta **una vez** por receptor sobre la suma | Reglas 6 y 7 | Los golpes sueltos pasarían igual y el pilar 2 pierde su fundamento |
| El exceso sobre el escudero **se pierde** | Regla 7 | Si pasara al aliado, la cobertura dejaría de proteger entera y se reabrirían C1 y los criterios de Cobertura que cuentan el exceso (AC-9, AC-10, AC-21 y AC-25) |
| La foto no cambia dentro de la resolución | Regla 3 | El orden de los golpes importaría y la vista previa no podría anunciarlo |
| La explosión golpea las 8 vecinas con el `ataque_ef` del demoledor | Regla 4 | Cambiaría Castillo F3 y F4 (oráculos) y la excepción aprobada |
| El tramo recibe el golpe completo y no se redirige | Reglas 5 y 8 | Cambiaría Castillo, regla 8 y F6 |

`golpes` ≤ 549 y `eventos` ≤ 393 por resolución son **cotas derivadas** de los topes del Catálogo (61 piezas por jugador, 9 casillas por área, 24 coberturas como máximo): no se tocan; se recalculan si cambia un tope.

## Visual/Audio Requirements

La resolución es el estado «Resolución / cosecha» del art bible (Sección 2, *Estados*): *simultáneo, contundente, rápido, concluyente*, en ≤ ~2 s, con un pulso de luz blanca cálida (~4000 K) desde cada objetivo de ~0,3 s. Resolución **no dibuja nada**: entrega eventos y Render (#18) y Audio (#22) los traducen. Lo que se exige acá es qué tiene que poder leerse de cada evento.

**Principios del art bible que más aplican** (Sección 1): **2, «El combo se ve como un solo golpe»** (pilar 2): los atacantes sobre un objetivo se dibujan con trazos que convergen y el efecto crece con `atacantesRivales`, en la capa del sistema, que ninguna skin toca; **3, «El ritmo del turno manda sobre la animación»** (pilar 4): todos los ataques se animan a la vez y la resolución completa no pasa de ~2 s. Sección 3 (*Castillo y muros*): el daño se ve en la forma, no solo en un número. Sección 6 (*El terreno cuenta la batalla*).

### Feedback por evento

| Evento | Feedback visual (dirección) | Feedback sonoro (dirección; la especificación es de #22) |
|---|---|---|
| `AtaqueEjecutado` (directo) | El gesto corto de ataque de la pieza (anticipación, golpe, vuelta), sincronizado con todos los demás. La flecha de la vista previa pasa de «plan» a «golpe» | Ataque propio del tipo; todos en el mismo cuadro |
| `AtaqueEjecutado` (área) | La catapulta dispara; el centro y las 8 vecinas reciben el pulso en el mismo cuadro. Deja una **mancha de tierra removida** en el centro | Disparo y caída, más grave que un golpe directo |
| `AtaqueEjecutado` (explosión) | El demoledor estalla sobre sus 8 vecinas, incluidos aliados y muros, y **no cae como una pieza común**: desaparece en la explosión. Deja una **marca chamuscada** | El golpe más fuerte de la resolución; no se confunde con un área |
| `DanoRecibido` (`atacantesRivales` = 1) | Pulso de luz blanca cálida ~0,3 s desde el objetivo y número flotante con el **neto** | Golpe simple |
| `DanoRecibido` (`combinado`) | Un impacto **más grande que la suma de los chicos**: los trazos de los atacantes convergen y el efecto escala con `atacantesRivales` (principio 2) | Un solo golpe más pleno, no varios apilados; escala con la cantidad |
| `DanoRecibido` con `neto` 0 | Rebote contra el escudo: ícono de escudo y «0», **sin pulso de daño**; se distingue del daño por la forma y el ícono, no por el color | Golpe amortiguado (escudo contra acero) |
| `DanoRecibido` con `brutoDesviado` > 0 | Una línea del aliado cubierto al escudero; el escudero muestra el total desviado con el ícono de escudo; el cubierto no muestra nada. Se lee **dónde fue el golpe** | Golpe sobre el escudero, igual que uno directo |
| Fuego amigo (el receptor es de J) | Mismo pulso, pero con una señal de **forma** propia (contorno discontinuo en la figura) y nunca por color solo (art bible, Sección 4, daltonismo) | Igual que el golpe, sin cue extra: la sorpresa ya se evitó en la vista previa |
| `DanoATramo` | El tramo muestra su estado por la forma: entero con borde recto, **mellado** si `vidaDespues × 2 < vida_tramo`, número flotante con el **bruto** | Golpe sobre piedra |
| `TramoRoto` | El tramo se vuelve **hueco con escombro bajo y borde quebrado**, la placa **ROTO** (grieta en zigzag) y el contador del castillo se actualiza (Castillo, UI). Deja escombros (cuantos más tramos rotos, más escombros) | Derrumbe de piedra; más fuerte con el segundo tramo roto (corte de reclutamiento) |
| `PiezaMuerta` (`Dano`) | La pieza se **desatura y cae**, en el mismo momento que las demás muertes (art bible, Sección 5, *Golpe y muerte*), con la placa **MUERE** | Caída, sin diferencia entre bandos |
| `PiezaMuerta` (`Explosion`) | Sin caída: la pieza desaparece dentro de la explosión | Parte del estallido |
| `CoberturaQuitada` | La línea de cobertura se corta en el mismo cuadro | Cue breve de escudo roto, solo si el escudero murió |

### Restricciones

- **Todo en el mismo cuadro.** Los impactos de todos los eventos de una resolución se muestran simultáneos; las muertes caen juntas. La resolución completa entra en ~2 s (art bible, principio 3, *propuesta a validar en prototipo*). La presentación puede acelerarse u omitirse sin cambiar el resultado: el estado ya es el final cuando salen los eventos.
- **Nada por color solo.** MUERE, ROTO, el escudo que absorbe y el fuego amigo se distinguen por forma, ícono o texto (art bible, Sección 4, *Daltonismo*).
- **El número flotante es el neto** para una pieza y el **bruto** para un tramo (sin escudo): coincide con lo que mostró la vista previa. Un número no puede ser mayor que lo que el jugador pudo calcular.
- **Una pieza con `bruto_pieza` 0** (el cubierto cuyo daño se desvió) no muestra nada.
- **Audio:** los golpes simultáneos no se apilan hasta saturar: el combinado suena como **un** golpe que crece con `atacantesRivales`, y los cues de MUERE y ROTO tienen prioridad de mezcla sobre los golpes sueltos (pilar 2: *un golpe combinado se siente distinto de varios golpes sueltos*).

> 📌 **Asset Spec** — Visual/Audio requirements are defined. After the art bible is approved, run `/asset-spec system:resolucion-simultanea-del-dano` to produce per-asset visual descriptions, dimensions, and generation prompts from this section.

## Game Feel

- **Respuesta:** `Resolver` entra en `comando_presupuesto_ms` (1 ms p95) y el estado ya es el final cuando salen los eventos; lo que tarda es la animación, no el cálculo. La vista previa de una jugada completa corre dentro de `vista_previa_presupuesto_ms` (5 ms p95).
- **La cosecha es simultánea (pilares 1 y 2):** todos los impactos en el mismo cuadro, las muertes caen juntas y la resolución completa dura ≤ ~2 s (art bible, principio 3; a validar en prototipo). Nada de impactos en fila, en orden de ID.
- **Peso del combo:** el golpe combinado se siente **más grande que la suma de los chicos**, en imagen y en sonido, y crece con `atacantesRivales`. Tres arqueros contra un escudo se leen como una jugada, no como tres.
- **Coincidencia total con la vista previa:** ningún número, MUERE o ROTO de la resolución difiere de lo que mostró la vista previa. Es lo que hace que el daño fijo se perciba como fijo y no como azar (pilar 1).
- **Se puede saltar:** el jugador puede acelerar u omitir la animación; el resultado no cambia (el estado ya está aplicado). Ninguna entrada bloquea el turno siguiente más allá de ~2 s.
- **El costo se siente:** una explosión propia que golpea aliados, o un muro propio que cae, se anima con el mismo peso que un golpe al rival: nada se suaviza porque sea fuego amigo.
- **Verificación:** capturas retenidas de los casos de AC-43 y playtest con dos preguntas: *«¿algún resultado te pareció azar o no coincidió con la previa?»* y *«¿entendiste adónde fue el golpe del aliado cubierto?»*.

## UI Requirements

Resolución **no tiene pantalla propia**: todo lo que el jugador ve de ella lo muestran la vista previa (#16) y Render (#18) a partir de los eventos de la regla 14. Lo que esos sistemas necesitan poder mostrar:

- **Por pieza:** el **neto** como número flotante y la placa **MUERE** cuando `vidaDespues` es 0. Con `neto` 0, el rebote contra el escudo.
- **Por tramo:** el **bruto** (sin escudo) y la placa **ROTO** cuando `vidaDespues` es 0; el contador de tramos rotos del castillo se actualiza con `tramosRotosDelDueno` (Castillo, UI).
- **Exceso:** cuando un escudero muere con `neto` mayor que su vida, la vista previa marca el **exceso perdido** (`neto − vidaPerdida`) para que el jugador no junte daño inútil sobre un cubierto.
- **Desvío:** la línea del aliado cubierto a su escudero, con el total que absorbe el escudero; el cubierto no muestra daño.
- **Fuego amigo:** todo lo propio que recibe daño, y de forma destacada si el **Rey propio** muere (Alcance, UI). Es lo que separa una jugada osada de una sorpresa.
- **Zonas:** al asignar una explosión, las 8 vecinas (muros incluidos); al apuntar un área, el centro y su anillo; con las casillas fuera del tablero omitidas.
- **Accesibilidad (#23):** cada marca se distingue por forma, ícono o texto, nunca por color solo.
- **Localización (#14):** Resolución no emite códigos de rechazo ni textos propios; los textos de MUERE, ROTO y del exceso son de #16 y #14.

📌 **UX Flag — Resolución simultánea del daño**: This system has UI requirements. In Phase 4 (Pre-Production), run `/ux-design` to create a UX spec for each screen or HUD element this system contributes to **before** writing epics. Stories that reference UI should cite `design/ux/[screen].md`, not the GDD directly.

## Cross-References

| Qué | Documento | Dónde | Relación |
|---|---|---|---|
| `Resolver`, `TurnTimeout`, regla 15 (simultánea), eventos, vista previa, `vidaQuitada` | `design/gdd/libreria-de-reglas.md` | Reglas 5, 8, 9, 11, 15 y 17; AC-10, AC-27, AC-42, AC-48 | Completa; propone la lista de eventos y ajustes de AC (abajo) |
| Cierre del turno, `EvaluateMatchEnd`, `vidaQuitada` y su cota | `design/gdd/maquina-de-turno.md` | Reglas 4.1, 11 a 15; Interactions; AC-10 | Respeta; confirma la cota |
| `ataque`, `escudo`, `vida`, `danoArea`, `piezasParaMatar`, I1 a I3 | `design/gdd/catalogo-de-piezas.md` | Reglas 5 y 6, Formulas, I1 a I3, AC-18, AC-33 | Lee; cierra la regla real que el oráculo comprueba |
| Grilla, contorno de 8, `ocupante` | `design/gdd/tablero-y-despliegue-inicial.md` | Reglas 2 y 3 | Lee |
| Tramo, bono del arquero, daño compartido, rotura | `design/gdd/castillo-y-muros.md` | Reglas 4, 5, 7, 8, 9, 10; F1, F2, F5, F6; AC-5, 6, 10 a 12, 28, 30 | Confirma el contrato PROVISIONAL; corrige AC-6 (abajo) |
| Lista de asignaciones, modos, zona de explosión | `design/gdd/alcance-y-asignacion-de-ataques.md` | Reglas 5, 7, 11 y 16; AC-19 | Confirma |
| Reparto de la cobertura, `receptor`, muertes de coberturas | `design/gdd/cobertura-del-escudero.md` | Reglas 6, 9, 10, 15, 16; F2, F3; C1, C2; AC-8 a AC-14, AC-21, AC-25 | Confirma; fija el orden de eventos que dejó abierto |
| El Grito en `ataque_ef`, sin explosión ni área | `design/gdd/modificador.md` | Regla 7, F3, Edge Cases | Confirma; no suma bonos por su cuenta |
| Reglas vigentes de catapulta, demoledor, escudero; pilares 1 y 2 | `design/gdd/game-concept.md` | Reglas vigentes; Game Pillars | Detalla |
| Estados de la resolución, tramos, golpe y muerte, terreno | `design/art/art-bible.md` | Secciones 1 (principios 2 y 3), 2, 3, 5 y 6 | Aplica |

### Arrastre a otros GDD (aplicado al cerrar la tanda 3, 2026-11-25)

Esta sesión corrió en paralelo con otras y no editó GDD vecinos; los cambios de abajo se aplicaron al cerrar la tanda. Por documento:

| GDD | Cambio |
|---|---|
| **Castillo (#5)** | Quitar *PROVISIONAL* de la regla 7, de la fila #9 de Interactions y su nota, de la «Excepción aprobada» del demoledor (F3), de las Open Questions («Confirmar el contrato PROVISIONAL de #9») y de AC-5, 6, 10, 11, 12, 28 y 30. **Corregir AC-6**: con el centro en el tramo rival `(19, 7)` el área también cae sobre `(19, 6)` y `(19, 8)` (6 cada uno), así que los vecinos quedan en 24 y `vidaQuitada` del azul suma 10 + 6 + 6 = 22, no 10. En la primera parte, el área sobre `(4, 7)` también toca `(4, 6)` y `(4, 8)` |
| **Cobertura (#8)** | Quitar *PROVISIONAL* de la regla 6 (último párrafo), la fila #9 de Interactions, los Edge Cases de «lo que cae sobre el escudero supera su vida», los supuestos y AC-8 a AC-11, 13, 14, 21 y 25; en la regla 15, citar que el lugar de las muertes en la resolución es después de `PiezaMuerta` (regla 14 de #9) |
| **Alcance (#7)** | Fila #9 de Interactions: «Este GDD; confirmado por #9»; supuestos: quitar «el formato de la lista y la zona de explosión»; AC-19 sin PROVISIONAL |
| **Modificador (#11)** | Supuestos provisionales y Edge Case «una pieza muere en la misma resolución en que golpea» sin PROVISIONAL; Bidireccionalidad: Resolución ya tiene GDD; AC de #9 sin PROVISIONAL |
| **Máquina de turno (#3)** | Supuestos PROVISIONALES: el contador `vidaQuitada` queda confirmado, con su cota; AC-10 sin PROVISIONAL; en Interactions, la fila 9 apunta a este GDD |
| **Librería (#1)** | Regla 8: agregar la lista cerrada de eventos de Resolución (regla 14 de #9) y que el «combinado» incluye la explosión; regla 11, fila `vidaQuitada`: quitar PROVISIONAL; regla 5: afinar la cota de `bruto` por casilla (≤ 61.610); AC-10 y AC-42 sin PROVISIONAL, y en AC-42 precisar «`AtaqueEjecutado` en orden de ID ascendente (B antes que A)»; AC-48: «golpea dos veces el mismo objetivo» solo ocurre por cobertura (centro desviado y área sobre el escudero) |
| **Catálogo (#2)** | AC-18 y AC-33 sin PROVISIONAL; la observación de la explosión («supuesto del prototipo») pasa a «confirmada por #9»; I1 sin PROVISIONAL para los tramos; fila #9 de Dependencies: «Existe y ya nombra al Catálogo» |
| **Localización (#14)** | La fila de reglas dice «#9, pendiente»: pasa a «#9: sin códigos de rechazo ni textos propios» |
| **Índice de sistemas** | Fila 9: estado *Designed*, enlace a este GDD, dependencias con Librería, Tablero y Modificador (blanda); corregir «área de catapulta (12/8)» a **10/6** (línea de la descripción de #9); contadores de progreso |
| **Registro (`entities.yaml`)** | Entradas nuevas y `referenced_by` en la lista de la sesión |

## Acceptance Criteria

Las piezas se nombran por tipo y casilla `(columna, fila)`. Valores del Catálogo v5 (soldado 8 de ataque, escudo 3, vida 25 · arquero 6, 2, 15 · escudero 4, 5, 20 · catapulta 10, área 6, escudo 2, vida 15 · jinete 8, 3, 20 · demoledor 30, 1, 10 · Rey 8, 5, 30), `vida_tramo` 30, castillo azul con muro en la columna 4 y filas 5 a 9. «Azul» es J salvo que se diga otra cosa. Los criterios Logic e Integration son **tests automáticos de EditMode** (`Assets/Tests/EditMode/Resolucion/`); AC-43 es Visual y pide captura retenida en `production/qa/evidence/`. Un valor marcado «(fixture)» se arma con el constructor interno de pruebas de la Librería (AC-43 de la Librería).

### Reglas de Resolución (Sección C)

- **AC-1 (reglas 1 y 2, lista vacía)** *(Integration #3)* — **GIVEN** `Atacar` del azul sin asignaciones y todas las piezas y tramos con vida completa, **WHEN** `Resolver`, **THEN** no sale ningún evento de los tipos 1 a 6 de la regla 14, la vida de toda pieza y tramo no cambia, `vidaQuitada` de los dos no cambia y la Máquina emite `TurnoTerminado`.
- **AC-2 (regla 3, la pieza que muere igual golpea)** *(Integration #1)* — **GIVEN** un soldado azul A (ID 7, vida 3 fixture) en `(8, 7)` con ataque directo a un soldado rojo T (ID 9, vida 3 fixture) en `(8, 6)`, y una catapulta azul B (ID 3) en `(8, 10)` con el centro en `(8, 8)` (el área cae sobre A y no sobre T), **WHEN** `Resolver`, **THEN** A recibe 6 − 3 = 3 y muere, **y T igual recibe el golpe de A** (8 − 3 = 5 ≥ 3) y muere con `vidaPerdida` 3; los eventos `AtaqueEjecutado` salen B antes que A; `PiezaMuerta` sale 7 y después 9; `vidaQuitada` del azul suma 3.
- **AC-3 (regla 3, el tramo roto igual dio el +2)** *(Integration #5)* — **GIVEN** un arquero azul (vida 15) sobre el tramo azul `(4, 5)` con vida 18 y tres arqueros rojos con ataque directo sobre esa casilla (bruto 18), **WHEN** `Resolver` del rojo, **THEN** el tramo queda en 0, el arquero queda en **1** (neto 18 − 4 = 14; con escudo 2 habría muerto con 16 ≥ 15), sale `TramoRoto(4, 5)` y después de resolver `enMuro` es falso.
- **AC-4 (regla 3, el escudero que muere igual desvía)** *(Integration #8)* — **GIVEN** un arquero azul (escudo 2, vida 15) cubierto por un escudero azul (escudo 5, vida 20) pegado, y el rojo con 2 soldados sobre el escudero y 2 sobre el arquero (cuatro golpes de 8), **WHEN** `Resolver` del rojo, **THEN** el escudero recibe 32, neto 27 ≥ 20 y muere; **el arquero conserva sus 15** (los 16 sobre él se desviaron todos, aunque el escudero muera); salen, en este orden, `DanoRecibido(escudero)` con bruto 32 y `brutoDesviado` 16, `PiezaMuerta(escudero)` y `CoberturaQuitada(escudero, arquero, EscuderoMuerto)`.
- **AC-5 (regla 4, directo)** — **GIVEN** un soldado azul con ataque directo a un soldado rojo (escudo 3, vida 25), **WHEN** `Resolver`, **THEN** `DanoRecibido` trae bruto 8, `brutoDesviado` 0, escudo 3, neto 5, vida antes 25, vida después 20, vida perdida 5, `combinado` falso y `atacantesRivales` 1; `vidaQuitada` del azul suma 5.
- **AC-6 (regla 4, área)** — **GIVEN** una catapulta azul con el centro en `(10, 7)` sobre un soldado rojo, otro soldado rojo en `(11, 7)`, un soldado azul en `(9, 7)` y las demás casillas del área vacías, **WHEN** `Resolver`, **THEN** el rojo del centro recibe 10 − 3 = 7 (vida 18), el rojo de `(11, 7)` recibe 6 − 3 = 3 (vida 22), el azul de `(9, 7)` recibe 3 (vida 22) con `atacantesRivales` 0 y `combinado` falso, las casillas vacías no generan evento, y `vidaQuitada` del azul suma 10.
- **AC-7 (regla 4, borde y esquina)** — **GIVEN** una catapulta azul con el centro en `(0, 0)` y todo libre, **WHEN** se arma la suma, **THEN** hay golpes solo en `(0, 0)`, `(1, 0)`, `(0, 1)` y `(1, 1)` (suma total 10 + 3 × 6 = 28), sin error por las casillas fuera. **Y GIVEN** el centro en `(0, 7)`, **THEN** son 6 casillas (suma 10 + 5 × 6 = 40).
- **AC-8 (regla 4, Grito)** — **GIVEN** Grito de guerra activo para el azul (bono de ataque 2), **WHEN** `Resolver`, **THEN** (a) una catapulta con el centro sobre un demoledor rojo (escudo 1, vida 10) hace 12 − 1 = 11 ≥ 10 y lo mata, mientras sus vecinas siguen en 6; (b) un soldado azul (10) contra un soldado rojo (escudo 3, vida 25) hace neto 7; (c) un demoledor azul con un Rey rojo (escudo 5, vida 30) vecino lo deja en **5** (30 − 5 = 25, no 32).
- **AC-9 (reglas 4 y 9, explosión sobre vecinos)** — **GIVEN** un demoledor azul en `(10, 7)` con un soldado rojo en `(11, 7)`, un escudero azul (escudo 5, vida 20) en `(9, 7)` y el Rey azul (escudo 5, vida 30) en `(10, 6)`, el resto libre, **WHEN** `Resolver`, **THEN** el soldado rojo recibe 30 − 3 = 27 y muere (vida perdida 25), el escudero azul recibe 25 y muere (vida perdida 20), el Rey azul recibe 25 y queda en 5, el demoledor sale con `PiezaMuerta` motivo `Explosion` y su casilla no recibe golpe; `vidaQuitada` del azul suma **25** (solo el soldado rojo).
- **AC-10 (regla 9, explosión sin nadie)** — **GIVEN** un demoledor azul solo en `(10, 7)` con la explosión asignada, **WHEN** `Resolver`, **THEN** salen exactamente `AtaqueEjecutado(demoledor, Explosión, (10, 7))` y `PiezaMuerta(demoledor, Explosion)`, el demoledor ya no está en el estado y `vidaQuitada` no cambia.
- **AC-11 (regla 8, explosión sobre muros; cierra Castillo AC-30)** *(Integration #5)* — **GIVEN** `vida_tramo` 30 con los tramos azules `(4, 6)`, `(4, 7)` y `(4, 8)` en 30 y un demoledor rojo en `(5, 7)` con la explosión asignada, **WHEN** `Resolver` del rojo, **THEN** los tres quedan en 0, `tramosRotos(azul)` es 3, `puedeReclutar(azul)` es falso, salen 3 `DanoATramo` (bruto 30, vida perdida 30) y 3 `TramoRoto` por casilla `(4, 6)`, `(4, 7)`, `(4, 8)` cada uno con `tramosRotosDelDueno` 3, y `vidaQuitada` del rojo suma 90. **Y GIVEN** `vida_tramo` 31, **THEN** los tres quedan en 1, `tramosRotos(azul)` es 0, no sale `TramoRoto` y `vidaQuitada` suma 90.
- **AC-12 (regla 8, esquina y fuego amigo)** *(Integration #5)* — **GIVEN** `vida_tramo` 30 y un demoledor rojo en `(5, 5)`, **WHEN** explota, **THEN** `(4, 5)` y `(4, 6)` quedan en 0 y `puedeReclutar(azul)` es falso. **Y GIVEN** el demoledor en `(5, 4)`, **THEN** solo `(4, 5)` se rompe y `puedeReclutar(azul)` sigue verdadero. **Y GIVEN** un demoledor **azul** en `(5, 7)` que explota junto a su propio muro, **THEN** rompe los tres tramos azules, `puedeReclutar(azul)` es falso y `vidaQuitada` de los dos no cambia.
- **AC-13 (regla 5, arquero sobre el tramo)** *(Integration #5)* — **GIVEN** un arquero azul (vida 15, escudo efectivo 4) sobre su tramo `(4, 5)` (vida 30), **WHEN** 1 soldado rojo (8) lo ataca con ataque directo, **THEN** `DanoRecibido` trae bruto 8, escudo 4, neto 4 (vida 11) y `DanoATramo` trae bruto 8 y vida 22. **Y WHEN** lo atacan 4 soldados rojos (32), **THEN** el arquero muere (neto 28), el tramo queda en 0 y la casilla queda libre y sin tramo.
- **AC-14 (regla 5, hueco y casilla vacía)** — **GIVEN** el tramo azul `(4, 7)` roto con un soldado rojo parado en él, **WHEN** el azul le asigna 2 soldados con ataque directo, **THEN** el soldado rojo recibe 16 − 3 = 13 y **no sale ningún `DanoATramo`**. **Y GIVEN** un área del azul que cae en parte sobre un hueco vacío y en parte sobre casillas vacías, **THEN** esas casillas no generan evento ni cambio.
- **AC-15 (regla 6, el escudo se resta una vez)** — **GIVEN** 2 arqueros azules (6 y 6) con ataque directo sobre un soldado rojo (escudo 3, vida 25), **WHEN** `Resolver`, **THEN** bruto 12, neto **9** (no 6), vida 16, `combinado` verdadero y `atacantesRivales` 2.
- **AC-16 (reglas 6 y 12, momento ancla)** — **GIVEN** 3 soldados azules y 1 arquero azul sobre un soldado rojo (escudo 3, vida 25), **WHEN** `Resolver`, **THEN** bruto 30, neto 27 ≥ 25, el soldado muere con vida perdida 25, `combinado` verdadero con `atacantesRivales` 4, y `vidaQuitada` suma 25. **Y GIVEN** solo los 3 soldados, **THEN** bruto 24, neto 21 y el soldado queda en 4.
- **AC-17 (regla 7, reparto y exceso; cierra Cobertura AC-8 y AC-9)** *(Integration #8)* — **GIVEN** un arquero rojo (escudo 2, vida 15) cubierto por un escudero rojo (escudo 5, vida 20) vecino y 3 soldados azules sobre el arquero, **WHEN** `Resolver`, **THEN** el arquero queda en 15, el escudero en **1** (24 − 5 = 19), `DanoRecibido(escudero)` trae bruto 24, `brutoDesviado` 24 y neto 19, el arquero no tiene `DanoRecibido` y `vidaQuitada` suma 19. **Y GIVEN** 2 soldados sobre el arquero y 1 sobre el escudero, **THEN** el escudero queda en 1 (bruto 24, **un solo** escudo; con dos descuentos quedaría en 6). **Y GIVEN** 5 soldados (40) sobre el arquero, **THEN** el escudero muere con vida perdida 20, el exceso se pierde, el arquero conserva sus 15 y `vidaQuitada` suma 20. **Y GIVEN** el mismo caso de 3 soldados sin cobertura, **THEN** el arquero muere (24 − 2 = 22 ≥ 15).
- **AC-18 (regla 7, área y explosión sobre el cubierto; cierra Cobertura AC-10)** *(Integration #8)* — **GIVEN** el mismo par y una catapulta azul con el centro (10) sobre el arquero y el área (6) sobre el escudero vecino, **WHEN** `Resolver`, **THEN** el escudero recibe 10 desviados + 6 propios = 16, neto 11, vida 9; el arquero conserva 15. **Y GIVEN** un demoledor azul pegado a los dos, **THEN** el escudero recibe 30 + 30 = 60, neto 55, muere, y el arquero conserva 15.
- **AC-19 (regla 7, la cobertura propia no desvía)** *(Integration #8)* — **GIVEN** el azul con un arquero (escudo 2, vida 15) y su escudero (escudo 5, vida 20) vecino con la cobertura asignada en este turno, y una catapulta azul con el centro sobre su propio arquero, **WHEN** `Resolver`, **THEN** el arquero recibe 10 − 2 = 8 (vida 7) y el escudero recibe el área, 6 − 5 = 1 (vida 19); `brutoDesviado` es 0 en los dos; `combinado` falso con `atacantesRivales` 0; `vidaQuitada` no cambia.
- **AC-20 (regla 7, el tramo no se redirige; cierra Castillo AC-11 y Cobertura AC-11)** *(Integration #5 y #8)* — **GIVEN** un arquero azul (vida 15) sobre su tramo `(4, 5)` (vida 30) cubierto por un escudero azul (escudo 5, vida 20), **WHEN** 1 soldado rojo lo ataca, **THEN** el arquero queda en 15, el escudero en 17 (8 − 5 = 3) con `brutoDesviado` 8, y el tramo en **22** (golpe completo).
- **AC-21 (reglas 8 y 9, tramo roto bajo un arquero; cierra Castillo AC-12)** *(Integration #5)* — **GIVEN** un arquero azul (vida 15) sobre un tramo con vida 10 y 2 arqueros rojos con ataque directo (bruto 12), **WHEN** `Resolver`, **THEN** el tramo queda en 0, el arquero en 7 (12 − 4 = 8) sobre la casilla, ahora hueco, y `enMuro` es falso. **Y WHEN** el bruto es 20 (soldado y 2 arqueros), **THEN** el arquero muere (neto 16 ≥ 15), la casilla queda libre y `hayTramo` es falso.
- **AC-22 (regla 10, coberturas después de resolver)** *(Integration #8)* — **GIVEN** un escudero azul que cubre a su arquero azul vecino y un demoledor azul pegado a los dos que explota, **WHEN** mueren los dos (25 ≥ 20 y 28 ≥ 15), **THEN** sale **un solo** `CoberturaQuitada` con `EscuderoMuerto`, después de las dos `PiezaMuerta`. **Y GIVEN** el demoledor pegado solo al arquero (el escudero a distancia 2 del demoledor), **THEN** sale `AliadoMuerto`. **Y GIVEN** una cobertura cuyas dos piezas sobreviven, **THEN** no sale evento y la cobertura sigue en el estado; las coberturas del rival no se tocan.
- **AC-23 (regla 11, `vidaQuitada`)** — **GIVEN** cada fixture del azul como J: (a) un golpe de 8 sobre un soldado rojo con vida 3 (fixture): suma **3**, no 5; (b) la catapulta azul sobre su propio soldado: suma 0; (c) 3 soldados sobre un arquero rojo cubierto: suma **19**, y con 5 soldados (40): suma 20; (d) un demoledor azul que explota solo: suma 0; (e) un golpe de 8 sobre un arquero rojo en el muro (escudo efectivo 4): suma **12** (4 de la pieza y 8 del tramo); (f) un tramo rojo con vida 5 que recibe 32: suma 5; un tramo ya roto: suma 0; un tramo azul: suma 0 (Castillo AC-6 y AC-28). **THEN** cada `vidaQuitada` coincide con el valor indicado y la del rojo no cambia en ningún caso.
- **AC-24 (regla 12, combinado)** — **GIVEN** cada fixture, **THEN**: dos atacantes sobre un objetivo → `combinado` verdadero y `atacantesRivales` 2; uno → falso; dos atacantes sobre dos objetivos distintos → ninguno es combinado; dos atacantes cuyo daño absorbe entero un escudo (objetivo con escudo 20, fixture) → neto 0 y combinado; el área de una catapulta propia y otra pieza propia sobre una pieza propia → 0 atacantes rivales y falso; **el centro desviado y el área de la misma catapulta sobre un escudero** → `atacantesRivales` 1 y falso (una pieza cuenta una vez); un demoledor y un soldado sobre el mismo vecino → 2 y verdadero; dos atacantes sobre un tramo → `DanoATramo` combinado.
- **AC-25 (regla 13, lo que no cambia)** — **GIVEN** una resolución con muertes y tramos rotos, **WHEN** termina el cálculo de Resolución, **THEN** las casillas de las piezas vivas, sus movimientos usados, las llamadas de reclutamiento, la cara, los bonos y la tirada del Modificador y la fase son los mismos que antes (la Máquina los cierra después), y no sale ningún evento de movimiento.
- **AC-26 (regla 14, orden y forma de los eventos)** — **GIVEN** un fixture que produce los seis tipos (dos atacantes con IDs 3 y 7, un cubierto con su escudero, un tramo roto con tramos de dos dueños, una muerte por daño y una por explosión), **WHEN** `Resolver`, **THEN** los tipos salen en el orden 1 a 6 de la regla 14; dentro de cada tipo, por ID ascendente (`AtaqueEjecutado`, `DanoRecibido`, `PiezaMuerta`, `CoberturaQuitada`) o por casilla (fila, columna) (`DanoATramo`, `TramoRoto`); cada `TramoRoto` de un dueño trae el contador final de ese dueño; `PiezaMuerta.motivo` es `Dano` o `Explosion` según el caso; y repetir el fixture da una lista idéntica.
- **AC-27 (regla 15, determinismo)** — **GIVEN** un estado con ataques mezclados, **WHEN** se resuelve 100 veces, **THEN** la huella y la lista de eventos son idénticas. **Y WHEN** las asignaciones se entregan a Resolución en otro orden (solo en el test), **THEN** el estado final es el mismo.
- **AC-28 (regla 15, vista previa = resolución)** *(Integration #1)* — **GIVEN** ataques asignados con una cobertura, un área y una explosión, **WHEN** se pide la vista previa y después se envía `Resolver`, **THEN** las dos listas de eventos son idénticas y la vista previa no cambió la huella; **Y GIVEN** 0 asignaciones, **THEN** las dos son iguales y sin eventos de Resolución.
- **AC-29 (regla 15, cotas y memoria)** *(PROVISIONAL hasta el ADR de la Librería)* — **GIVEN** un Catálogo de pruebas válido donde todo tipo tiene `danoArea` > 0 y 61 piezas del azul con 61 asignaciones de área, **WHEN** `Resolver`, **THEN** hay como mucho 549 golpes y 393 eventos, sin desborde de buffers. **Y WHEN** tras calentar se resuelven 1.000 turnos seguidos, **THEN** no hay asignaciones de memoria.

### Fórmulas (Sección D)

- **AC-30 (F1, `bruto_casilla`)** *(Logic)* — **GIVEN** 0 golpes, 3 soldados y 1 arquero sobre una casilla, y 61 atacantes de ataque 1.000 con Grito (+10) sobre la misma casilla, **THEN** `bruto_casilla` es 0, 30 y **61.610** sin desborde. **Y GIVEN** una catapulta, **THEN** aporta 10 al centro y 6 a cada vecina, nunca 16 a una casilla (con Grito, 12 y 6); un demoledor con Grito aporta 30.
- **AC-31 (F2, `bruto_pieza`)** *(Logic)* — **GIVEN** un cubierto A y su escudero E, **THEN** 3 soldados sobre A dan `bruto_pieza(A)` 0 y `bruto_pieza(E)` 24; con 1 soldado más sobre E, 32; una catapulta con el centro en A y el área en E da `bruto_propio(E)` 6 y `bruto_desviado(E)` 10. **Y GIVEN** una pieza sin cobertura, **THEN** `bruto_desviado` es 0. **Y GIVEN** fuego amigo sobre A, **THEN** `bruto_pieza(A)` no es 0.
- **AC-32 (F3, límites)** *(Logic)* — **GIVEN** escudo 5 y vida 20, **WHEN** el bruto es 0, 4, 5, 6, 24 y 25, **THEN** el neto es 0, 0, 0, 1, 19 y 20; la vida nueva es 20, 20, 20, 19, 1 y 0. **Y GIVEN** el bruto 1.000.000, **THEN** vida perdida 20. **Y GIVEN** escudo 0 y vida 1, **WHEN** el bruto es 1, **THEN** muere. **Y GIVEN** el bruto 123.220, **THEN** sin desborde.
- **AC-33 (F3, contra el oráculo del Catálogo; cierra Catálogo AC-18)** *(Integration #2)* — **GIVEN** cada par (atacante, objetivo) de la matriz de 7 × 7 del Catálogo con N = `piezasParaMatar`, sin cobertura ni Grito, **WHEN** se resuelven N ataques del tipo atacante sobre el objetivo y, en otra copia, N − 1, **THEN** con N el objetivo muere y con N − 1 no.
- **AC-34 (F4, `vida_quitada_resolucion` y cota)** *(Logic)* — **GIVEN** el rojo con 61 piezas de vida 1.000 y escudo 0 y 16 tramos de 1.000 (fixture), **WHEN** una resolución las mata y las rompe todas, **THEN** `quitada` es **77.000**. **Y GIVEN** `vidaQuitada` del azul en 1.000.015.000 − 77.000 (fixture), **THEN** esa resolución la deja en exactamente 1.000.015.000 sin `ErrorInterno`.
- **AC-35 (F5, `atacantes_rivales`)** *(Logic)* — **GIVEN** 0, 1, 2 y 61 atacantes rivales distintos sobre un receptor, **THEN** `atacantesRivales` es 0, 1, 2 y 61 y `combinado` es falso, falso, verdadero y verdadero. **Y GIVEN** un receptor de J, **THEN** 0.
- **AC-36 (F6, oráculo de la explosión)** *(Logic; test de balance, no entra al núcleo)* — **GIVEN** `vida_tramo` 30 y el muro azul entero, **THEN** `tramos_en_explosion` es 3 en `(5, 7)`, 2 en `(5, 5)`, 1 en `(5, 4)`, 3 en `(3, 7)` (interior, junto a un lado) y 5 en `(3, 6)` (esquina del interior); `tramos_rotos_por_explosion` es igual con 30 y **0** con `vida_tramo` 31.

### Casos borde y cruces entre sistemas

- **AC-37 (fin de partida; Máquina, regla 13)** *(Integration #3)* — **GIVEN** 5 soldados azules sobre el Rey rojo (escudo 5, vida 30), **WHEN** `Resolver`, **THEN** el Rey muere y, después de los eventos de Resolución, sale `PartidaTerminada(ReyMuerto, azul)` sin `TurnoIniciado`. **Y GIVEN** además dos demoledores azules pegados al Rey azul que explotan (60 − 5 = 55 ≥ 30), **THEN** mueren los dos Reyes y sale `DobleRey` con ganador azul. **Y GIVEN** solo ese fuego amigo sobre el Rey azul, **THEN** gana el rojo y `vidaQuitada` del azul no cambia.
- **AC-38 (corte de reclutamiento; Castillo AC-13 y AC-14)** *(Integration #10 y #11)* — **GIVEN** el castillo azul con 1 tramo roto y una resolución del rojo que rompe el segundo, **WHEN** termina, **THEN** el `TramoRoto` trae `tramosRotosDelDueno` 2, y al abrir el turno del azul su tabla efectiva ya tiene las caras de reclutar en 0 (con la tabla base «Pierde con 1/2 o menos» y más de 12 piezas sin el Rey: 15/45/0/0/40).
- **AC-39 (área genérica; Catálogo AC-33)** *(Integration #2)* — **GIVEN** un Catálogo con `danoArea` 8 en el arquero, **WHEN** un arquero azul ataca una casilla en alcance, **THEN** el centro recibe 6 y las 8 casillas alrededor reciben 8 antes de escudo.
- **AC-40 (sin cascada)** — **GIVEN** dos demoledores azules en `(10, 7)` y `(11, 7)` con la explosión asignada, un soldado rojo en `(12, 7)` y otro en `(13, 7)`, **WHEN** `Resolver`, **THEN** cada demoledor recibe 30 − 1 = 29 y muere con motivo `Explosion`; el soldado de `(12, 7)` recibe 27 y muere; el de `(13, 7)` no recibe nada.
- **AC-41 (`TurnTimeout` de atacar)** *(Integration #3)* — **GIVEN** ataques y una cobertura asignados, **WHEN** llega `TurnTimeout` de la etapa atacar y, sobre una copia, `Resolver`, **THEN** el estado final y los eventos de Resolución son idénticos y la cobertura asignada se conserva.
- **AC-42 (neto 0)** — **GIVEN** un escudero azul (ataque 4) con ataque directo sobre el Rey rojo (escudo 5, vida 30), **WHEN** `Resolver`, **THEN** `DanoRecibido` trae bruto 4, escudo 5, neto 0 y vida perdida 0; el Rey conserva 30 y `vidaQuitada` no cambia.

### Visual

- **AC-43 (capturas de la cosecha)** *(Visual, dueño #18, PROVISIONAL hasta que exista)* — **GIVEN** Render, **WHEN** se resuelven (a) un combo de 4 atacantes, (b) un golpe absorbido por el escudo (neto 0), (c) una cobertura que desvía, (d) una explosión de demoledor sobre un muro de 3 tramos, (e) un tramo roto con el arquero sobreviviente, (f) un fuego amigo, **THEN** se guarda una captura de cada una en `production/qa/evidence/` donde se ven todos los impactos en el mismo cuadro, el número flotante igual al de la vista previa, las placas MUERE y ROTO, la línea de desvío y la marca de fuego amigo por forma.

### Revisión de QA

- **Cobertura:** regla 1 → AC-1, AC-41 · 2 → AC-1 · 3 → AC-2, 3, 4 · 4 → AC-5 a AC-8 · 5 → AC-13, 14 · 6 → AC-15, 16 · 7 → AC-17 a AC-20 · 8 → AC-11, 12, 21 · 9 → AC-9, 10, 21 · 10 → AC-22 · 11 → AC-23, 34 · 12 → AC-16, 24, 35 · 13 → AC-25 · 14 → AC-26 · 15 → AC-27 a AC-29 · 16 → AC-13 (no se puede dirigir a uno solo), AC-17 (el exceso no pasa al aliado) y AC-27 (el orden no importa). F1 a F6 → AC-30 a AC-36. Ninguna regla ni fórmula queda sin criterio.
- **Cruces:** hay criterios con Máquina (AC-1, 37, 41), Castillo (AC-3, 11, 12, 13, 20, 21), Cobertura (AC-4, 17 a 20, 22), Catálogo (AC-33, 39), Reclutamiento y Modificador (AC-38) y Librería (AC-2, 28, 29).
- **Rendimiento:** el tiempo no es un criterio de test (las pruebas no pueden depender del reloj): AC-29 solo verifica cotas y ausencia de asignaciones de memoria. El presupuesto en milisegundos se mide con `/perf-profile`, no en EditMode.
- **No automatizable (a Open Questions):** que la resolución completa entre en ~2 s, que el desvío se entienda y que ningún resultado se perciba como azar son preguntas de playtest.
- **Tipos:** AC-5 a AC-10, 14 a 16, 23 a 27 y 30 a 32, 34 a 36, 40 y 42 son Logic; AC-1 a 4, 11 a 13, 17 a 22, 28, 33, 37 a 39 y 41 son Integration; AC-29 es Logic PROVISIONAL hasta el ADR; AC-43 es Visual con captura retenida.

## Open Questions

| Pregunta | Dueño | Cuándo |
|---|---|---|
| ¿La tortuga (cubrir al Rey cada turno, con el exceso perdido) alarga la partida más allá del pilar 4 o vuelve inútil juntar daño sobre el Rey? Medir cuántos turnos el Rey no pierde vida y cuántas coberturas se rompen | Cobertura (#8) y Métricas (#19); playtest | MVP |
| ¿El demoledor de asedio corta el reclutamiento rival demasiado barato? Medir cuántos demoledores llegan al muro antes de morir y cuántas partidas tienen un corte por explosión (borde exacto: `vida_tramo` 31) | Castillo (#5) y Métricas (#19); playtest | MVP |
| ¿La resolución completa entra en ~2 s con 12 ataques, un área y una explosión a la vez? | Render (#18), prototipo | Cuando exista #18 |
| ¿Render necesita un evento por golpe individual (no agregado por receptor) para dibujar cada trazo que converge? Hoy basta `AtaqueEjecutado` más la lista de asignaciones de la vista previa; si no alcanza, se agrega a `DanoRecibido` la lista de atacantes sin cambiar el cálculo | Render (#18) | Al escribir #18 |
| ¿`vidaQuitada` con la pieza y el tramo del arquero en el muro por separado es la medida que Métricas necesita, o debería contar un golpe una sola vez? Hoy ninguna regla la lee | Métricas (#19) | Al escribir #19 |
| ¿El jugador entiende adónde fue el golpe del aliado cubierto (línea y número del escudero) y el exceso perdido sin leer el texto? | Vista previa (#16); playtest | Al escribir #16 |
| ¿La forma canónica de los eventos de la regla 14 (campos y orden) es la que el ADR de la Librería necesita para repeticiones y para la autoridad online? | Librería (#1) y ADR | En el ADR de la Librería |
