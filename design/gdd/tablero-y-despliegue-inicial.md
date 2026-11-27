# Tablero y despliegue inicial

> **Status**: Approved
> **Author**: Martin Arin + Claude
> **Last Updated**: 2026-11-27
> **Last Verified**: 2026-11-27
> **Design Review**: APPROVED 2026-11-27, en la 4.ª pasada: una verificación focalizada, por decisión del jugador. Confirmó que los 3 bloqueantes de la 3.ª pasada (F9, códigos de `ConfigInvalida`, Rey cortado por F5) quedaron resueltos y que los códigos T4 a T6 están alineados con la Librería (regla 2) y el Catálogo (regla 6), los dos aprobados. Lo que quede entre GDD lo revisa el `/consistency-check` del cierre de la tanda A (`design/gdd/reviews/tablero-y-despliegue-inicial-review-log.md`). Resuelve W-13, DW-3 y la parte de W-21 que toca al Tablero (`gdd-cross-review-2026-11-26.md`)
> **Implements Pillar**: Pilar 1 (Planificar y cosechar), Pilar 4 (Duelo corto)
> **Creative Director Review (CD-GDD-ALIGN)**: REVISED 2026-11-25 — texto del sorteo corregido; riesgo de pilar 4 (punto muerto) y legibilidad de distancias (#6/#7/#23) aceptados y registrados en Open Questions

## Summary

El Tablero es la grilla de 24 × 15 sobre la que se juega la partida y el despliegue inicial que la llena: dos castillos de 5 × 5 en los sectores 4 y 6, el muro de 16 tramos de cada uno, 12 piezas por jugador en tres líneas de cuatro y el Rey en el centro de su castillo. El jugador lo ve como el campo donde planifica y mide la distancia al rival; para el resto del juego es la única fuente de casillas, ocupación y distancias, y fija en qué turno llega el primer contacto (pilar 4). Existe para que cliente y servidor arranquen idénticos con la misma configuración, sin sorteo y con el jugador 0 moviendo primero.

> **Quick reference** — Layer: `Core` · Priority: `MVP` · Key deps: `Librería de reglas (#1), Catálogo de piezas (#2), Castillo y muros (#5, solo un dato)`

## Overview

El Tablero es la grilla de 24 columnas por 15 filas sobre la que se juega toda la partida, y el despliegue inicial que la llena. Al crear la partida coloca los dos castillos en los sectores 4 (azul, a la izquierda) y 6 (rojo, a la derecha), levanta el muro perimetral de 5 × 5 de cada bando con sus 16 tramos, ubica el ejército inicial (4 soldados, 4 arqueros y 4 escuderos en tres líneas de cuatro, los arqueros atrás y los escuderos al frente, mirando al rival) y pone al Rey en el centro de su castillo. No hay sorteo: todas las partidas empiezan igual y mueve primero el jugador 0 (azul). Terminado el despliegue, el Tablero no vuelve a decidir nada: queda como la capa de casillas, coordenadas y ocupación que leen Movimiento, Alcance, Resolución y Reclutamiento. Además fija la distancia entre castillos, de la que dependen el tiempo hasta el primer contacto (pilar 4) y el techo de `movimiento` que el Catálogo dejó sin verificar. El jugador no lo manipula: lo ve como el campo donde planifica (pilar 1) y lo siente como la distancia al rival. Sin este sistema no existiría un estado inicial único y reproducible: cliente y servidor, con la misma configuración, tendrían que arrancar idénticos, y ningún otro sistema sabría dónde está cada cosa.

**Qué no es.** Las reglas de los muros después del despliegue (daño, rotura, huecos, arqueros sobre el tramo) son de Castillo y muros (#5); quién puede moverse adonde, de Movimiento (#6); y cuántas piezas puede haber vivas, del Catálogo (#2). El Tablero no usa azar: el sorteo de castillos se retiró de la Librería el 2026-11-25.

**Ciclo con #5: cerrado.** Decisión del 2026-11-24 y esquema fijado el 2026-11-25 (Castillo y muros, regla 14). El Tablero coloca también los 16 tramos de muro; su vida inicial es el campo `vida_tramo` de la config (1 a 1.000, hoy 30), que valida la Librería. La arista de datos de #4 hacia #5 está en el índice.

## Player Fantasy

**El campo del general.** Antes de mover la primera pieza ya leíste el campo: sabés dónde está cada una, a cuántos pasos queda el rival y por dónde va a llegar tu plan. El tablero no esconde nada; es la mesa sobre la que el general piensa.

**Momento ancla:** el primer turno, con los dos ejércitos recién desplegados y el campo vacío entre los castillos. El jugador está contando cuántos pasos hay hasta el primer golpe en los tres casos posibles: **con el Grito de guerra del rival, sin Grito o sin que el rival mueva** (Distracción). Rojo tira el dado en su primer turno: Grito en el 40% de esas tiradas, reclutar en el 40% y Distracción en el 20% (Modificador, #11).

**Pilar que sirve:** Pilar 1, *"Al mover armás la estrategia; al atacar ejecutás el plan y ves sus frutos."* La fantasía central del juego (el plan que sale) empieza acá: sin un campo legible no hay plan que armar.

**Qué le exige al resto del diseño:**

- Una grilla legible de un vistazo y distancias que se puedan contar (Movimiento #6 y Alcance #7 miden en casillas; la Ficha y el HUD #15 y la Vista previa #16 las muestran).
- Ninguna información oculta sobre el campo: respeta el anti-pilar *"NOT información oculta al resolver"*.
- Un despliegue inicial que el jugador pueda reconocer sin texto: mismo ejército, mismas posiciones relativas, "mirando al rival".
- **Una capa de amenaza del turno siguiente del rival** (pedido de esta revisión, 2026-11-27). Contar «a cuántos pasos está el primer golpe» exige sumar el movimiento del rival (en cruz), su alcance (en círculo) y un posible Grito. Hoy, al tocar una pieza rival solo se ve su alcance desde donde está (Entrada, #13, AC-07). Se pide: las casillas que el rival puede golpear en su próximo turno, con movimiento + alcance, calculadas **antes de su tirada**. La franja base es la de sin bono, que ya contiene la de Distracción; la franja que agrega el Grito va aparte. El cálculo es de Alcance (#7). Lo muestran la Vista previa (#16) y la Ficha y HUD (#15), y el gesto que la activa es de Entrada (#13). Sin esa capa, la fantasía depende de una cuenta que el propio GDD hizo mal en su primera versión (Open Questions). Queda como dependencia abierta.

**Lo que este encuadre no promete** (queda como pregunta abierta): que quien mueve primero no tenga ventaja (el despliegue es parejo, pero el dado no: ver regla 10 y Open Questions) y que la distancia entre castillos entre en el presupuesto de 25 minutos (pilar 4). Esto último lo acotan `turnos_hasta_contacto` y `margen_primer_golpe` en Formulas y se mide en el MVP.

## Detailed Design

### Core Rules

1. **La grilla.** 24 columnas (0–23) × 15 filas (0–14), 360 casillas. Una casilla es `(columna, fila)`: la columna crece hacia la derecha y la fila hacia abajo. El índice plano es `fila × 24 + columna`, y el orden canónico de recorrido es fila y después columna. Las dimensiones son constantes del contrato: cambiarlas sube la versión de reglas.
2. **Vecindades.** El Tablero define las dos que usan los demás: la **cruz** (4 vecinas: arriba, abajo, izquierda y derecha; la usa Movimiento) y el **contorno** (las 8 vecinas; lo usan Alcance, Cobertura y la explosión del demoledor). La distancia en cruz es `|Δcolumna| + |Δfila|`. Una casilla fuera de la grilla no es vecina de nada, y pedir la cruz o el contorno **de** una casilla fuera de la grilla devuelve la lista vacía, sin error (como `ocupante`, decisión del 2026-11-27).
3. **Un ocupante por casilla.** Una casilla tiene como mucho una pieza, salvo un arquero sobre un tramo de muro de su propio castillo (regla de la Librería). Un tramo ocupa su casilla como muro. Cada casilla está en uno de cuatro estados: libre, con pieza, con tramo, o con tramo y arquero. **Cada tramo tiene dueño** (el jugador de su castillo) y **`hayTramo(casilla)` es verdadero solo si hay un tramo con vida > 0**. Un tramo roto (vida 0) sigue existiendo como dato de Castillo y muros (#5), que lo cuenta para el corte, pero para la ocupación su casilla es libre o "con pieza". **El Tablero hace cumplir estos invariantes:** una escritura que los rompa (poner una pieza en una casilla ocupada sin ser el arquero sobre su tramo, o fuera de la grilla) se rechaza con `ErrorInterno` y no cambia nada. Nunca llega por un comando de jugador, porque Movimiento (#6) y Reclutamiento (#10) validan antes (decisión del 2026-11-27).
4. **Despliegue fijo, sin sorteo.** En esta versión de reglas la config trae los castillos fijos en el par de sectores **4 y 6**: azul (jugador 0) a la izquierda, rojo (jugador 1) a la derecha. **Convención de nombres (decisión del jugador, 2026-11-26):** «azul» y «rojo» son los nombres internos de los jugadores 0 y 1 en reglas, fórmulas y tests de todos los GDD; en pantalla los bandos se pintan **Hueso (jugador 0, azul) y Ciruela (jugador 1, rojo)** (art bible §4). Ningún texto visible dice «azul» ni «rojo». Cualquier otro par se rechaza con `ConfigInvalida(T4)`. *Por qué solo ese par:* el 2–8 no entra (15 filas no dejan espacio entre castillos) y los diagonales (1–9 y 7–3) quedan a 11 casillas de frente a frente contra 9, un turno más de contacto. Decisión del 2026-11-24: la partida sin sorteo evita medir una diferencia que nadie pidió.
5. **Los castillos.** Cada castillo es un bloque de 5 × 5 centrado en su Rey: azul en el centro `(2, 7)`, columnas 0–4 y filas 5–9; rojo en `(21, 7)`, columnas 19–23 y filas 5–9. El **muro** son las 16 casillas del perímetro del bloque; el **interior** son las 9 de dentro (3 × 3). El muro queda pegado al borde lateral del tablero (el izquierdo para azul, el derecho para rojo). El Tablero coloca los 16 tramos, identificados por su casilla; su vida inicial es el campo `vida_tramo` de la config (1 a 1.000, hoy 30; dueño: Castillo y muros, #5, regla 14). Un valor fuera de ese rango da `ConfigInvalida(T5)`.
6. **El ejército inicial.** Cada jugador empieza con 4 soldados, 4 arqueros, 4 escuderos y el Rey (13 piezas). Las cantidades son constantes de la versión de reglas. Si el Catálogo permite menos de 4 de alguno de esos tres tipos, `CrearPartida` devuelve `ConfigInvalida(T6)` (cierra la pendiente que dejó el Catálogo, AC-30). Un `topeVivas` del Rey distinto de 1 no llega acá: lo rechaza antes `PrepararContexto` con `ConfigInvalida(F5)` (Catálogo, F5). Si igual llegara, es `ErrorInterno` (guarda defensiva).
   **Códigos y orden (decisión del 2026-11-27).** Los rechazos propios del Tablero en `CrearPartida` llevan código de regla, como los F1–F9 del Catálogo: **T4** (par de sectores), **T5** (`vida_tramo`) y **T6** (ejército inicial). Entre ellos se evalúan en el orden T4 → T5 → T6 y se devuelve el primero que falla. Dónde van frente a F9 y a los demás rechazos de `CrearPartida` lo fija la Librería (regla 2).
7. **Dónde empiezan las piezas.** Tres líneas de 4, en las filas 5 a 8 (las cuatro filas que van de 2 antes del Rey a 1 después). El Rey en el centro del castillo. Arqueros pegados al muro, escuderos al frente:

| Pieza | Azul (mira a la derecha) | Rojo (mira a la izquierda) |
|---|---|---|
| Rey | (2, 7) | (21, 7) |
| Arqueros | columna 5, filas 5–8 | columna 18, filas 5–8 |
| Soldados | columna 6, filas 5–8 | columna 17, filas 5–8 |
| Escuderos | columna 7, filas 5–8 | columna 16, filas 5–8 |

   **Dos consecuencias aceptadas como rasgo** (decisión del 2026-11-27):
   - **Los arqueros empiezan sobre el anillo.** La columna 5 (18 para rojo) es anillo, así que el despliegue ocupa 4 de sus 17 casillas y deja **13 libres**. El **frente del anillo** es la columna 5 entera (filas 4 a 10, 7 casillas, como lo define Reclutamiento, #10, F4), y le quedan 3 libres: (5, 9), (5, 4) y (5, 10). Con el orden de aparición de Reclutamiento (#10, F4), los 4 primeros reclutas con el anillo inicial salen en **(5, 9), (5, 4), (5, 10) y (4, 4)**, y para rojo en (18, 9), (18, 4), (18, 10) y (19, 4). Es **asimétrico en el tiempo**: rojo recluta por primera vez en el turno 2, antes de `Mover`, sin haber podido despejar sus arqueros; azul recluta por primera vez en el turno 3 y pudo moverlos en el turno 1. Se suma a la pregunta de ventaja de turno (Open Questions).
   - **El ejército no es simétrico arriba y abajo.** Cuatro piezas no se centran en las 5 filas del castillo: quedan 5 filas libres arriba (0–4) y 6 abajo (9–14), y la fila 9 del muro no tiene pieza delante. Los dos bandos quedan corridos igual (la simetría que importa es la de la regla 9) y el espejo horizontal no se testea.

8. **IDs.** Siguen la regla 7 de la Librería: azul primero; dentro de cada jugador, por orden de tipo del Catálogo y después por casilla (fila, columna). Azul: soldados 1–4, arqueros 5–8, escuderos 9–12, Rey 13. Rojo: soldados 14–17, arqueros 18–21, escuderos 22–25, Rey 26. El próximo ID es 27.
9. **Simetría de espejo.** El despliegue es simétrico respecto del eje vertical entre las columnas 11 y 12: a cada pieza azul de tipo `t` en `(c, f)` le corresponde una roja de tipo `t` en `(23 − c, f)`, y lo mismo vale para los tramos. Es una propiedad que se testea.
10. **Quién empieza.** La partida nace **En curso**, en el turno 1, fase Mover, del jugador 0 (azul). Sin tirada del Modificador y sin compensación. **Rojo sí tira en su primer turno** (turno 2), con la tabla de apertura 20/40/20/20/0 (Modificador, #11, AC-29). Con **Distracción (20%)** no mueve y solo ataca desde donde está, así que llega hasta la columna 14. Con **Grito de guerra (40%)** llega hasta la columna 8. Con **reclutar (40%: Llamada 20 y Cuerno 20)** mueve con su movimiento base y llega hasta la 10, también con los reclutas (Llamada da hasta 4 piezas y Cuerno hasta 4 jinetes). En los tres casos rojo planifica sabiendo qué cara le tocó, mientras azul movió a ciegas. Es la asimetría que se vigila: Métricas locales (#19) la mide y Open Questions fija el disparador para revisarla.
11. **Consultas que expone (solo lectura).** `dentro(casilla)`, `ocupante(casilla)`, `hayTramo(casilla)`, `distanciaCruz(a, b)` (precondición: las dos casillas dentro de la grilla; si no, `ErrorInterno`), las dos vecindades, el **anillo** de un castillo (las casillas de afuera pegadas a su muro en contorno de 8, o sea `radio = RADIO_MURO + 1`: incluye las esquinas (5, 4) y (5, 10), que tocan el muro solo en diagonal), su **interior** (las 9 de dentro) y `ladoDelRival(jugador)` (+1 para azul, −1 para rojo). Nadie más guarda coordenadas del tablero.
12. **Lo que el jugador no puede hacer.** Elegir lado, mover un castillo o armar su despliegue. Todas las partidas empiezan igual (pilar 1: ninguna información oculta).

### States and Transitions

| Estado | Cuándo | Quién escribe |
|---|---|---|
| **SinDesplegar** | Antes de `CrearPartida`; no hay casillas ni piezas | — |
| **Desplegado** | Al terminar `CrearPartida` válida: 26 piezas y 32 tramos | El despliegue del Tablero |
| **En juego** | Desde el primer comando aceptado | Movimiento, Reclutamiento y Resolución cambian la ocupación; el Tablero hace cumplir los invariantes |

| De → a | Disparador | Efecto |
|---|---|---|
| SinDesplegar → Desplegado | `CrearPartida` con config válida | Despliegue completo en una sola operación |
| SinDesplegar → (sin partida) | Config inválida | No se crea tablero ni partida |
| Desplegado → En juego | Primer comando aceptado | Ninguno propio |

No hay transición que "borre" el tablero: una partida Terminada conserva la ocupación final. Si algún día vuelve el sorteo, habría que reabrir la Librería (estado de espera y fórmula) y este GDD, con cambio de versión de reglas.

### Interactions with Other Systems

| Sistema | Qué entra al Tablero | Qué sale | Dueño de la interfaz |
|---|---|---|---|
| #1 Librería | La config (castillos fijos) en `CrearPartida` | Estado inicial: piezas, muros, IDs 1–26 | Librería (contrato); Tablero (el diseño del despliegue) |
| #2 Catálogo | Orden de tipos y `topeVivas` | `ConfigInvalida(T6)` si el ejército inicial no cabe | Catálogo |
| #3 Máquina de turno | — | "Empieza el jugador 0, fase Mover" | Máquina de turno |
| #5 Castillo y muros | Vida inicial del tramo (`vida_tramo`, campo de la config) | Casillas del muro, del interior y del anillo | Castillo y muros |
| #6 Movimiento | — | `dentro`, `ocupante`, `hayTramo`, cruz, `distanciaCruz`, anillo e interior | Tablero |
| #7 Alcance | — | `dentro`, `ocupante`, `hayTramo`, contorno de 8 y límites de la grilla | Tablero (grilla); Alcance (círculo) |
| #8, #9 Cobertura y Resolución | — | `ocupante`, contorno | Tablero |
| #10 Reclutamiento | — | Anillo entero, `ocupante` y `ladoDelRival` | Tablero |
| #11 Modificador | — | `margen_primer_golpe` (oráculo: justifica el tope de `bono_grito_movimiento`) | Modificador (el bono); Tablero (el margen) |
| #13 Entrada y selección | — | Geometría 24 × 15, `ocupante`, tramos; el gesto que activa la capa de amenaza pedida (Player Fantasy) | Tablero (geometría); Entrada (el toque y el gesto) |
| #15 Ficha y HUD, #16 Vista previa | — | Dimensiones y lado de cada castillo; la capa de amenaza pedida (Player Fantasy) | Tablero (geometría); #7, #15 y #16 (la capa) |
| #17 Interfaz del Modificador | — | Anillo, casillas libres y columnas del frente | Tablero (geometría); #17 (cómo se marcan) |
| #18 Render | — | Grilla, castillos, `ladoDelRival` y despliegue | Tablero (geometría); Render (pantalla) |
| #19 Métricas | — | Constantes de la partida (el par de sectores es fijo) | Métricas |

**Suposiciones provisionales de esta sección:**

- *`ladoDelRival`* es una constante (+1 / −1) porque hay un solo par; con otros pares habría que volver a una orientación calculada.
- *Azul siempre a la izquierda y siempre el primero:* no se puede separar el efecto de "lado" del de "turno" en las métricas. Queda para Open Questions (la revancha del menú, #21, podría alternar lados).
- *Los oráculos de Formulas suponen el carril recto y libre:* las dos líneas ocupan las mismas filas (5 a 8), no hay bloqueos (las piezas atraviesan a sus aliados, Movimiento, #6, regla 5, y la casilla de llegada está libre) y se avanza en línea recta, que es lo que más columnas gana (en diagonal, la cruz del Movimiento y el círculo del Alcance, #7, ganan menos columnas). Por eso el margen calculado es el peor caso, y por eso AC-36 los contrasta con Movimiento y Alcance reales.

## Formulas

El Tablero no tiene balance propio: su geometría es una constante del contrato y las fórmulas de abajo son cuentas de enteros sobre ella. No hay ciclos de retroalimentación (la geometría es estática). `turnos_hasta_contacto` y `margen_primer_golpe` son **oráculos para tests de balance**, y `espejo` lo es del test de simetría: no entran al núcleo. **`techo_movimiento` sí entra:** su valor (12) es la constante que valida la regla F9 del Catálogo en `CrearPartida` (`movimiento_t + bono_grito_movimiento ≤ 12`, `ConfigInvalida(F9)`; Catálogo, regla 6). Su variante con alcance, `techo_movimiento(k)`, sigue siendo un oráculo (AC-19).

**Regla común a todas:** `dentro(columna, fila)` se verifica **siempre antes** de calcular un índice. El chequeo de límites de arreglos de IL2CPP (activo por defecto) atrapa un índice menor que 0 o mayor que 359, pero **no** una columna 24 que se "envuelve" a la columna 0 de la fila siguiente (`(24, 3)` daría 96, que es `(0, 4)`).

The `indice_casilla` formula is defined as:

`indice = fila × COLUMNAS + columna` (inversa: `columna = indice mod COLUMNAS`, `fila = indice div COLUMNAS`). **Precondición: `dentro(columna, fila)`.**

| Variable | Type | Range | Source | Description |
|----------|------|-------|--------|-------------|
| columna | int | 0–23 | calculated | Coordenada horizontal |
| fila | int | 0–14 | calculated | Coordenada vertical |
| COLUMNAS | int | 24 | constant (contrato) | Ancho de la grilla |
| FILAS | int | 15 | constant (contrato) | Alto de la grilla |

**Output Range:** 0 a 359. Fuera de la precondición no se define.
**Example:** el Rey rojo en `(21, 7)`: 7 × 24 + 21 = **189**; y 189 mod 24 = 21, 189 div 24 = 7.

The `distancia_cruz` formula is defined as:

`distancia_cruz(a, b) = |columna_b − columna_a| + |fila_b − fila_a|`

| Variable | Type | Range | Source | Description |
|----------|------|-------|--------|-------------|
| a, b | casilla | dentro de la grilla | calculated | Dos casillas |

**Output Range:** 0 (la misma casilla) a 37 (23 + 14, esquina contra esquina). Cuenta de enteros sin desborde.
**Example:** escudero azul `(7, 5)` y escudero rojo `(16, 8)`: 9 + 3 = **12**.

The `casilla_de_castillo` formula is defined as:

`radio(c, f) = max(|c − cc|, |f − cf|)`, con el centro `(cc, cf)` del castillo. **Interior** si `radio < RADIO_MURO`; **muro** si `radio = RADIO_MURO`; **anillo** si `radio = RADIO_MURO + 1` (y la casilla está dentro de la grilla). `tramos = 8 × RADIO_MURO`; `interior = (2 × RADIO_MURO − 1)²`.

| Variable | Type | Range | Source | Description |
|----------|------|-------|--------|-------------|
| cc, cf | int | azul (2, 7); rojo (21, 7) | constant (contrato) | Centro del castillo (casilla del Rey) |
| RADIO_MURO | int | 2 | constant (contrato) | Distancia del centro al muro: bloque de 5 × 5 |

**Output Range:** `radio` 0 a 21 (`max(|23 − 2|, |0 − 7|)`). Con `RADIO_MURO` = 2: **16 tramos** y **9 casillas de interior**. **Precondiciones:** el muro va pegado al borde lateral (regla 5), así que `cc_azul = RADIO_MURO` y `cc_rojo = COLUMNAS − 1 − RADIO_MURO` (con 2 y 24: centros 2 y 21); `cf ± RADIO_MURO` dentro de 0 a `FILAS − 1`. La no superposición de los castillos queda absorbida por el invariante `distancia_frente ≥ 2`. Con `RADIO_MURO` = 1 el castillo sería de 3 × 3: 8 tramos y solo el Rey adentro (interior 1).
**Example:** `(4, 5)` para el castillo azul: |4 − 2| = 2 y |5 − 7| = 2, radio 2, **muro** (la esquina). `(5, 6)`: |5 − 2| = 3, radio 3, **anillo**.

The `casilla_inicial` formula is defined as:

`columna = cc_j + lado_j × (RADIO_MURO + 1 + linea_tipo)` y `fila = cf_j − 2 + i`, con `i = 0..3`.

| Variable | Type | Range | Source | Description |
|----------|------|-------|--------|-------------|
| j | int | 0 (azul), 1 (rojo) | constant | Jugador |
| lado_j | int | +1 azul, −1 rojo | constant (contrato) | Hacia dónde mira |
| linea_tipo | int | arquero 0, soldado 1, escudero 2 | constant (contrato) | Línea del tipo, de atrás hacia adelante |
| i | int | 0–3 | calculated | Posición dentro de la línea de 4 |

**Output Range:** columnas 5–7 (azul) y 16–18 (rojo); filas 5–8. La fila de menor índice es `cf − 2` = 5 y la de mayor índice `cf + 1` = 8, ambas dentro de 0–14. **Precondiciones:** `cf ≥ 2` y `cf + 1 ≤ FILAS − 1`. Solo vale con los centros de esta versión: con otros habría que revalidar. La línea de arqueros (`linea_tipo` 0, columna `cc + RADIO_MURO + 1`) **cae sobre el anillo** (regla 7).
**Example:** escudero rojo con `i = 3`: columna = 21 − (2 + 1 + 2) = 16, fila = 7 − 2 + 3 = 8, **(16, 8)**. Arquero azul con `i = 0`: columna = 2 + 3 = 5, fila 5, **(5, 5)**.

The `distancia_frente` formula is defined as:

`distancia_frente = (cc_rojo − cc_azul) − 2 × (RADIO_MURO + 1 + linea_max)`

| Variable | Type | Range | Source | Description |
|----------|------|-------|--------|-------------|
| cc_azul, cc_rojo | int | 2 y 21 | constant (contrato) | Columna del centro de cada castillo |
| RADIO_MURO | int | 2 | constant (contrato) | Distancia del centro al muro |
| linea_max | int | 2 | constant (contrato) | Línea más adelantada del ejército (escuderos) |

**Output Range:** 19 − 2 × 5 = **9** (8 columnas libres entre las líneas de escuderos). **Invariante:** `distancia_frente ≥ 2`; con 1 los ejércitos empiezan pegados y con menos se superponen. Con el muro pegado al borde (precondición de `casilla_de_castillo`) la fórmula queda `distancia_frente = COLUMNAS − 7 − 4 × RADIO_MURO` (con `linea_max` 2): 24 y 2 dan 9; con `COLUMNAS` 24, `RADIO_MURO` 1 da 13 y 3 da 5.
**Example:** azul en la columna 7, rojo en la 16: 16 − 7 = 9.

The `turnos_hasta_contacto` formula is defined as:

`turnos_hasta_contacto = ceil((distancia_frente − 1) / m)`

| Variable | Type | Range | Source | Description |
|----------|------|-------|--------|-------------|
| distancia_frente | int | ≥ 2 (invariante) | calculated (`distancia_frente`) | Columnas entre las dos líneas de frente |
| m | int | 1–12 (piso del Catálogo F2, techo `techo_movimiento`; con F9 y el bono vigente de 2, el `movimiento` base llega como mucho a 10) | data file (Catálogo, `movimiento`) | Movimiento de las piezas de frente, por turno de un jugador |

**Output Range:** 1 a `distancia_frente − 1` (1 a 8 con el valor vigente); da 1 para todo `m ≥ distancia_frente − 1`. No hay división por cero porque el Catálogo exige `m ≥ 1`, y el invariante `distancia_frente ≥ 2` evita el 0. **Supuesto:** cuenta turnos de partida en los que **los dos bandos avanzan `m`** hacia el otro en línea recta y sin bloqueos; mide **contacto** (quedar pegados), no golpe: el golpe a distancia lo mide `margen_primer_golpe`. Es una **cota inferior** y un oráculo para los tests de balance, no una predicción. Si avanza uno solo, el contacto llega en el turno de partida `2 × ceil((distancia_frente − 1) / m) − 1` del que avanza (azul) o `2 × ceil(...)` (rojo): con `m` = 4, en el turno 3 o 4.
**Example:** `m` = 4 (escuderos): ceil(8 / 4) = **2** (azul avanza 4 en el turno 1 y rojo cierra con 4 en el turno 2). Si rojo saca Grito de guerra en el turno 2: azul 4 sin Grito (no hay tirada en el turno 1) más rojo 6 da 10 ≥ 8, sigue en **2**. Con `m` = 2: 4.

The `margen_primer_golpe` formula is defined as (agregada el 2026-11-27; resuelve W-13 de la revisión cruzada):

`margen(t, k) = d_k − (m_k + bono_t + R_k)` y `margen_primer_golpe(t) = min sobre k de margen(t, k)`, con `d_k = distancia_frente + linea_max − linea_k`.

| Variable | Type | Range | Source | Description |
|----------|------|-------|--------|-------------|
| t | int | 1 o 2 | constant | Turno del primer golpe posible: 1 = azul, 2 = rojo |
| k | tipo | ejército inicial: soldado, arquero, escudero; en `t` = 2, también cada tipo reclutable | constant (contrato) | Tipos que pueden golpear en el turno `t`: el ejército inicial del atacante (regla 6) y, en el turno 2, el recluta de rojo, que aparece en su anillo antes de `Mover` (regla 7) |
| linea_k | int | arquero 0, soldado 1, escudero 2 | constant (contrato) | Línea del tipo (la de `casilla_inicial`); el recluta parte de la columna del anillo, que es la de la línea 0 |
| d_k | int | ≥ 2 | calculated | Columnas desde donde parte el tipo `k` hasta la línea de frente defensora, sin mover: arquero 11, soldado 10, escudero 9; recluta 11 (columna 18 contra la 7) |
| m_k | int | 1–10 (F9 del Catálogo con el bono vigente de 2) | data file (Catálogo, `movimiento`) | Movimiento base del tipo |
| R_k | int | ≥ 1 (hoy 1–6; el esquema del Catálogo no lo acota por debajo de 1.000) | data file (Catálogo, `alcance_max`) | Alcance máximo del tipo |
| bono_t | int | 0–10 | config (Modificador, `bono_grito_movimiento`; dominio válido de la Librería) | 0 si `t` = 1 (en el turno 1 no se tira el Modificador, regla 10), sea cual sea el bono. Si `t` = 2, depende del caso de la cara (abajo): `bono_grito_movimiento` en el caso Grito; 0 en el caso reclutar, porque reclutar y Grito son caras distintas y sale una sola (Modificador, regla 2) |

`margen_primer_golpe(1) = min_k margen(1, k)` sobre el ejército inicial. `margen_primer_golpe(2) = min(caso Grito, caso reclutar)`: **caso Grito**, el mínimo sobre el ejército inicial con `bono_t` = `bono_grito_movimiento`; **caso reclutar**, el mínimo sobre el ejército inicial y cada tipo reclutable, con `bono_t` = 0. Con Distracción rojo no mueve y no entra en el mínimo (su zona, la columna 14, está dentro de la del caso reclutar). El Rey no entra en `k`: parte del centro del castillo, a 14 columnas del frente azul, y aun con Grito su margen es 14 − (6 + 2 + 1) = 5.

**Output Range:** sin cota inferior: `alcance_max` no tiene un techo de diseño (el esquema del Catálogo admite hasta 1.000), así que el margen puede ser tan negativo como el alcance lo permita; un negativo es información para el test. **Margen > 0: no llega; margen ≤ 0: golpea.** **Invariante:** `margen_primer_golpe(1) ≥ 1` y `margen_primer_golpe(2) ≥ 1` (el turno 2 contra un **azul que no se movió**: si azul avanzó, el golpe del turno 2 es una elección de azul, no del despliegue). Supone el carril recto y libre (supuestos de Interactions). Es un oráculo para los tests de balance; no entra al núcleo.
**Example:** turno 1: arquero 11 − (4 + 0 + 4) = 3, soldado 10 − (4 + 0 + 1) = 5, escudero 9 − (4 + 0 + 2) = 3; **mínimo 3**. Turno 2 con Grito (`bono` 2): arquero 11 − 10 = 1, soldado 10 − 7 = 3, escudero 9 − 8 = 1; mínimo 1. Turno 2 con reclutar: el ejército da lo mismo que en el turno 1 (3), y el peor recluta (arquero o catapulta, `m + R` = 8) da 11 − 8 = 3; mínimo 3. **`margen_primer_golpe(2)` = 1.** Con un bono de 3 el mínimo sería 0: rojo golpearía en el turno 2 a un azul quieto. De ahí sale el tope del bono: `bono_grito_movimiento ≤ min(d_k − m_k − R_k) − 1 = 3 − 1 = 2` (la justificación del Modificador, #11, era la del turno 1, que no puede tener Grito). **El arquero sobre su tramo:** empieza en el anillo (columna 5 o 18), y desde ahí alcanza hasta la 9 (azul) o la 14 (rojo). Si da un paso a su tramo (columna 4 o 19, +2 de alcance, Castillo y muros) alcanza 6 columnas: azul hasta la 10 y rojo hasta la 13. El tramo le suma una sola columna.
**Lo que no cuenta:** el daño. Con Grito (+2 de ataque) y el escudo restado una sola vez sobre la suma (Resolución, #9), el escudero azul ID 10, que avanzó de (7, 6) a (8, 6), puede morir en el turno 2. Con Grito, los arqueros rojos 18, 19 y 20 (filas 5 a 7) avanzan 6 hasta la columna 12 y lo alcanzan: con el círculo de Alcance (#7, `dx² + dy² ≤ R² + 1`), 16 + 1 ≤ 17. El 21, en la fila 8, no llega: 16 + 4 > 17. El escudero rojo 23 lo alcanza desde (10, 6). Suman 3 × 8 + 6 = 30, menos 5 de escudo, 25 contra 20 de vida. Es un dato para la pregunta de ventaja de turno (Open Questions), no un invariante. Tampoco está calculado el daño del caso reclutar: con Llamada aparecen hasta 4 piezas que, sumadas al ejército, pueden concentrar golpes sobre una pieza azul que avanzó hasta la franja de la columna 10.

The `techo_movimiento` formula is defined as:

`techo_movimiento = d_anillo_muro − 1`, con `d_anillo_muro = (cc_rojo − RADIO_MURO − 1) − (cc_azul + RADIO_MURO + 1)`.

| Variable | Type | Range | Source | Description |
|----------|------|-------|--------|-------------|
| cc_azul, cc_rojo | int | 2 y 21 | constant (contrato) | Columna del centro de cada castillo |
| RADIO_MURO | int | 2 | constant (contrato) | Distancia del centro al muro |
| d_anillo_muro | int | 13 | calculated | Pasos desde el anillo de un castillo hasta quedar pegado al muro del otro |

**Output Range:** d_anillo_muro = 18 − 5 = 13, **techo = 12**. Responde la pendiente del Catálogo ("una pieza llega al castillo rival el mismo turno en que aparece"): una pieza reclutada en el anillo azul necesita 13 pasos para quedar pegada al muro rojo, así que con `movimiento` ≤ 12 no llega ese mismo turno. Supone el carril libre; con piezas en el camino el techo real es menor.
**Con alcance (atacar el castillo el turno en que aparece):** desde el anillo (columna 5) hasta el muro rojo (columna 19) hay 14 columnas, así que la condición segura por tipo es `m_k + R_max(k) ≤ d_anillo_muro` = 13, es decir `techo_movimiento(k) = 13 − R_max(k)`. Con R = 1 vuelve a dar 12. Valores vigentes: arquero 4 + 4 = 8, catapulta 2 + 6 = 8, soldado 4 + 1 = 5, escudero 4 + 2 = 6, jinete 6 + 1 = 7, demoledor 6 + 1 = 7: todos por debajo del techo del castillo. Los topes que de verdad aprietan son los de `margen_primer_golpe(2) ≥ 1`: para el ejército inicial con Grito, `m + R` ≤ 8 (arquero), ≤ 7 (soldado) y ≤ 6 (escudero); para cualquier tipo reclutable, `m + R ≤ 10`. Ver Tuning. **El Grito no se suma** en el turno en que la pieza aparece: reclutar y Grito son caras distintas del mismo dado y sale una sola por turno (Modificador, regla 2); el recluta mueve ese turno con su `movimiento` base.
**Lo que valida el núcleo y lo que no.** La F9 del Catálogo valida en `CrearPartida` `movimiento_t + bono_grito_movimiento ≤ techo_movimiento` (12). Suma el bono aunque el recluta no pueda tener Grito el turno en que aparece, así que es más estricta que esta fórmula en el movimiento (deja el base en 10, no en 12), y **no mira el alcance**: un tipo de `movimiento` 8 y `alcance_max` 6 pasa F9 (8 + 2 = 10) y rompe `m + R ≤ 13`. Esa condición con alcance, y los topes de `margen_primer_golpe`, solo los cubren los tests de balance (AC-19, AC-32, AC-36). La diferencia queda en Open Questions.
**Example:** jinete reclutado en `(5, 9)` (la primera casilla libre del anillo inicial, con los arqueros todavía en él, regla 7; con el anillo libre la primera es la (5, 7)) mueve 6 y llega a la columna 11, a 8 columnas del muro rojo; aunque tuviera Grito (8), llegaría a la 13.

The `espejo` formula is defined as:

`espejo(columna, fila) = (COLUMNAS − 1 − columna, fila)`

| Variable | Type | Range | Source | Description |
|----------|------|-------|--------|-------------|
| columna | int | 0–23 | calculated | Coordenada horizontal |
| fila | int | 0–14 | calculated | Coordenada vertical |
| COLUMNAS | int | 24 | constant (contrato) | Ancho de la grilla |

**Output Range:** columna 0–23, fila igual. **Example:** el Rey azul `(2, 7)` ↔ el Rey rojo `(21, 7)`; el arquero azul `(5, 5)` ↔ `(18, 5)`. Es el oráculo del test de simetría (regla 9).

## Edge Cases

**Configuración y versión**

- **If la config trae un par de castillos que no es (4, 6)** (otro par, o el mismo par invertido (6, 4)): `ConfigInvalida(T4)`; no se crea la partida. El invertido también se rechaza porque azul va siempre a la izquierda.
- **If el Catálogo permite menos de 4 soldados, arqueros o escuderos**: `ConfigInvalida(T6)`; no se crea tablero (regla 6). **If el `topeVivas` del Rey es distinto de 1**: lo rechaza `PrepararContexto` con `ConfigInvalida(F5)` y no se llega a `CrearPartida` (Catálogo, F5); si llegara, `ErrorInterno`.
- **If Castillo y muros (#5) entrega una vida inicial de tramo menor o igual a 0 o fuera de la tabla de dominios**: `ConfigInvalida(T5)` (`vida_tramo` de 1 a 1.000; ciclo con #5 cerrado el 2026-11-25).
- **If la config viola varias reglas del Tablero a la vez** (por ejemplo, el par (6, 4) y un Catálogo con 3 soldados): se devuelve la primera en el orden T4 → T5 → T6 (regla 6). Frente a F9 y a los demás rechazos de `CrearPartida`, manda el orden de la Librería (regla 2).
- **If un tipo tiene `movimiento` + `bono_grito_movimiento` > 12** (`techo_movimiento`): `CrearPartida` devuelve `ConfigInvalida(F9)` (Catálogo, regla 6). La regla es del Catálogo; el Tablero aporta la constante.
- **If un estado cargado o una repetición declara otras dimensiones de tablero o otro radio de muro**: `VersionNoSoportada`. Las dimensiones son constantes de la versión de reglas.
- **If se cambia una constante del Tablero** (dimensiones, radio del muro, centros, líneas del ejército o composición inicial) **sin subir la versión de reglas**: falla el test de la huella de geometría (AC-26), que compara la huella calculada con la registrada para esa versión. Si el cambio mueve piezas o tramos, también falla el test del despliegue de referencia (AC-7: las 26 piezas y los 32 tramos en sus casillas, con sus IDs). Es el mismo mecanismo que el Catálogo usa con su huella.

**Consultas y geometría**

- **If una consulta recibe una casilla fuera de la grilla** (coordenada negativa, columna 24 o fila 15): `dentro` devuelve falso, `ocupante` responde "ninguno", la cruz y el contorno de esa casilla son la lista vacía (regla 2) y **nunca se calcula un índice** (regla común de Formulas). Si llegó por un comando de jugador, la Librería lo rechazó antes con su código.
- **If se pide el anillo de un castillo pegado al borde lateral**: se excluyen las casillas fuera de la grilla. El castillo azul tiene 17 casillas de anillo, no 24, y no tiene anillo a su izquierda. El anillo es geometría pura: incluye casillas ocupadas; quien lo usa (Reclutamiento, #10) filtra las libres.
- **If un tramo de muro se rompe**: la clasificación de la casilla (muro, interior, anillo) **no cambia**, porque es geometría fija. El tablero la sigue reconociendo como casilla de muro; `hayTramo` pasa a falso. Quien decide quién puede pasar por el hueco es Castillo y muros (#5).
- **If un arquero está sobre un tramo y el tramo se rompe**: el arquero queda en esa casilla, que pasa a ser "con pieza". El Tablero no mueve a nadie.
- **If `ladoDelRival` recibe un jugador distinto de 0 o 1**: es un error de programación, `ErrorInterno` de la Librería; nunca llega por un comando de jugador.
- **If un sistema intenta poner una pieza en una casilla ocupada** (salvo el arquero sobre un tramo propio con vida > 0) **o fuera de la grilla, durante el juego**: `ErrorInterno` y el tablero no cambia (regla 3). Es un error de programación de quien escribe (Movimiento, Reclutamiento o Resolución).
- **If `distanciaCruz` recibe una casilla fuera de la grilla**: `ErrorInterno` (precondición de la regla 11); no se calcula.
- **If la config es inválida**: `CrearPartida` no devuelve un estado parcial (ni tablero ni piezas): el despliegue es una sola operación (States and Transitions).

**Estado cargado**

- **If un estado cargado tiene dos piezas en una casilla** (salvo un arquero sobre un tramo propio), **una pieza fuera de la grilla, un tramo fuera del perímetro de su castillo, una cantidad de tramos distinta de 32 (los rotos siguen existiendo como dato, regla 3) o un tramo con vida fuera de 0 a `vida_tramo`**: `EstadoInvalido` (los invariantes de las reglas 1 a 3). Un tramo con vida 0 es válido: es un tramo roto.

**Estrategia degenerada**

- **If nadie cruza las 8 columnas libres** (8 a 15): no es un error del Tablero y no se resuelve acá, pero **no es un punto muerto neutro**. Termina por el reloj de 12:30 por jugador (Máquina de turno, regla 14). Si los dos agotan sus timers de etapa, agota primero el que jugó más turnos: **azul** (Máquina, Edge Cases). Azul puede evitarlo cerrando sus turnos rápido, pero la pasividad pareja lo castiga, así que queda **presionado** a acercarse primero, y si se acerca queda expuesto en el turno 2. En las filas del frente y en carril recto, lo golpean en las **columnas 8 y 9 con Grito (40%)** y **en la 10 y la 11 (lo más lejos que llega azul en el turno 1) siempre que rojo mueva (Grito o reclutar, 80%)**; ahí solo la Distracción (20%) lo deja a salvo (`margen_primer_golpe`; Open Questions). Además, quien espera tiene ventajas de posición, **una u otra según dónde deje a sus arqueros**:
  - Si un arquero rojo da un paso a su tramo, alcanza 6 columnas desde el muro: desde la columna 19, hasta la 13 (desde el anillo ya llegaba a la 14, así que el tramo suma una columna). Al subir libera su casilla de la columna 18, que vuelve al frente del anillo, y los reclutas salen ahí primero (por ejemplo, en (18, 7)).
  - Si los arqueros siguen en el anillo, los reclutas del defensor salen en las esquinas y en un costado: (18, 9), (18, 4), (18, 10) y (19, 4).
  
  En los dos casos los reclutas aparecen a 13 pasos o más del muro rival. **Hipótesis: esperar favorece a rojo.** En el modo Sin límite no hay reloj y la partida no termina sola. *PROVISIONAL: es la hipótesis que acordamos medir; se vigila con las Métricas (#19) y el disparador está en Open Questions.*

## Dependencies

**Depende de:**

| Sistema | Tipo | Qué usa |
|---|---|---|
| #1 Librería de reglas | Dura | `CrearPartida` (config con castillos fijos), IDs estables (regla 7), códigos de error (`ConfigInvalida`, `VersionNoSoportada`, `EstadoInvalido`) |
| #2 Catálogo de piezas | Dura | Tipos 0 a 6 y su orden, `topeVivas` (valida el ejército 4/4/4 con `ConfigInvalida(T6)`; el Rey lo valida el Catálogo, F5), `movimiento` y `alcance_max` (en los oráculos de Formulas). En sentido inverso, la F9 del Catálogo lee `techo_movimiento` (12) en `CrearPartida` |
| #5 Castillo y muros | Blanda, solo un dato | La vida inicial del tramo, que llega por la config (hoy 30) |

**El ciclo con #5.** #5 depende de #4 para la geometría (dura); #4 depende de #5 solo para un número que llega por la config y que la Librería valida, igual que la tabla del Catálogo. En el código no hay ciclo (ninguno llama al otro). En el índice queda una arista de datos en sentido contrario, anotada como tal. *Decisión del 2026-11-24:* el valor es de #5 porque el Catálogo ya lo asume ("la vida de los tramos (Castillo, #5)") y porque el balance vive en el GDD que lo define.

**Dependen de este sistema:**

| Sistema | Tipo | Qué usa |
|---|---|---|
| #3 Máquina de turno | Blanda | "Empieza el jugador 0, fase Mover, sin compensación" (cierra su Open Question) |
| #5 Castillo y muros | Dura | Grilla, geometría del castillo (muro, interior, anillo) y tramos colocados |
| #6 Movimiento | Dura | `dentro`, `ocupante`, `hayTramo`, cruz, `distanciaCruz`, anillo e interior |
| #7 Alcance y asignación | Dura | `dentro`, `ocupante`, `hayTramo`, contorno de 8 y límites de la grilla |
| #8 Cobertura, #9 Resolución | Dura | `ocupante` y contorno |
| #10 Reclutamiento | Dura | Anillo entero del castillo, `ocupante` y `ladoDelRival` |
| #11 Modificador | Blanda | `margen_primer_golpe` del **turno 2** (la primera tirada, de rojo): cada punto de `bono_grito_movimiento` lo achica; con +2 queda en 1 |
| #13 Entrada y selección | Dura | Geometría 24 × 15, casilla de 68 px, `ocupante` y tramos |
| #15 Ficha y HUD | Blanda | Medidas, lado de cada castillo; muestra la capa de amenaza pedida (Player Fantasy) |
| #16 Vista previa | Blanda | Geometría de la casilla; muestra la capa de amenaza pedida |
| #17 Interfaz del Modificador | Blanda | Anillo, casillas libres y columnas del frente |
| #18 Render | Dura | Grilla, castillos, `ladoDelRival` y despliegue |
| #19 Métricas | Blanda | Constantes de la partida (el par de sectores es fijo); mide la ventaja de turno (Open Questions) |
| #26 Registro | Blanda | La config con los castillos fijos |

**Bidireccionalidad: estado de los documentos vecinos** (al 2026-11-27):

- **Librería, Catálogo, Castillo, Movimiento, Alcance, Cobertura, Resolución, Reclutamiento, Máquina:** nombran al Tablero con la misma fuerza (verificado el 2026-11-27).
- **Entrada (#13), Render (#18):** lo nombran como Dura; esta tabla los sumaba como Blanda o no los tenía (W-21), corregido el 2026-11-27.
- **Interfaz del Modificador (#17), Ficha y HUD (#15), Vista previa (#16), Métricas (#19):** lo nombran como Blanda.
- **Pendiente en vecinos** (lo propaga la sesión de cierre de la tanda): el Modificador (#11) no nombra al Tablero en Dependencies y su justificación del tope de `bono_grito_movimiento` habla del turno 1; Métricas (#19) tiene que sumar las medidas de la ventaja de turno; Alcance (#7), #15 y #16 tienen que recibir el pedido de la capa de amenaza, y Entrada (#13) el del gesto que la activa; la Interfaz del Modificador (#17), el pedido de no tapar la zona central en el turno 2 (UI Requirements); el índice, la fila 16 sin el Tablero (blanda); el Catálogo (#2), los códigos T4/T5/T6 para su AC-30 y la pregunta de F9 frente a `m + R ≤ 13` (Open Questions).

## Tuning Knobs

**El Tablero no tiene perillas de balance propias.** Las perillas que entran en sus fórmulas son `movimiento` y `alcance_max` del Catálogo (data file) y `bono_grito_movimiento` del Modificador (config). Lo que un ajuste de balance tocaría vive en otro GDD:

| Valor | Dónde se ajusta | Nota |
|---|---|---|
| `movimiento` y `alcance_max` de cada tipo | Catálogo (#2), archivo de datos | Entran en `turnos_hasta_contacto`, `margen_primer_golpe` y `techo_movimiento`. **Topes por tipo** de `m + R`, para que `margen_primer_golpe(2) ≥ 1` con el bono vigente de 2: arquero ≤ 8, soldado ≤ 7, escudero ≤ 6 (`d_k − 1 − bono`). Cualquier tipo reclutable, ≤ 10 (`11 − 1`, sin bono). Además, `techo_movimiento(k)`: ≤ 13. **Arquero (8) y escudero (6) ya están en su tope**: un +1 a cualquiera de los dos rompe el invariante |
| `topeVivas` | Catálogo (#2) | Tiene que cubrir la composición inicial |
| Vida inicial del tramo | Castillo y muros (#5), llega por la config | Hoy 30 |
| +2 de movimiento del Grito de guerra (`bono_grito_movimiento`) | Modificador (#11) | Cambia `m` durante un turno. **Tope 2** por `margen_primer_golpe(2) ≥ 1`: cada punto resta uno al margen del turno 2 (con +2 queda en 1, con +3 en 0 y rojo golpea a un azul quieto). Es una restricción del **rango seguro**, no de la validación: la Librería acepta el dominio 0 a 10 y `CrearPartida` no rechaza un 3 por esta regla (sí puede rechazarlo F9 o la cota de la pila). Lo vigila la parte Integration de AC-32 |

**Dónde viven las constantes.** Son **constantes con nombre, en una sola definición, versionadas** (decisión del 2026-11-24, igual que las constantes del contrato de la Librería): nunca hay un número suelto en el código (`24`, `15`, `2`). Cambiar una es tocar esa definición, subir la versión de reglas y actualizar el test del despliegue de referencia. No son perillas de un archivo de datos, aunque funcionan como palancas de diseño para el pilar 4.

**Constantes del contrato que funcionan como palancas de diseño:**

| Constante | Vigente | Rango seguro | Si es demasiado alto | Si es demasiado bajo |
|---|---|---|---|---|
| `COLUMNAS` (con el centro azul en 2, distancia entre centros = `COLUMNAS − 5`) | 24 | **Seguro: 24**, el mínimo que mantiene `margen_primer_golpe ≥ 1` en los dos turnos (margen del turno 2 con Grito, con el bono vigente de 2, = `COLUMNAS − 23`). 25 o más también lo cumple, pero atrasa el contacto al turno 3 (pilar 4). 22 y 23 son válidos y nadie golpea en el turno 1, pero rojo con Grito golpea a un azul quieto en el turno 2. Piso duro 17 (`distancia_frente ≥ 2`) | 25 o más: contacto en el turno 3 o después, partida más lenta (pilar 4). La grilla es 24 × 15 = 1,6; lo que es 16:9 es la pantalla, que deja los márgenes del HUD (UI Requirements) | 21 o menos: arqueros y escuderos golpean en el turno 1; 20 o menos: contacto en el turno 1. Ventaja de golpe para quien mueve primero |
| `FILAS` | 15 | Piso duro 5 (entra el castillo); piso útil sin medir, 7 para dejar una fila de flanqueo a cada lado (con 7 esa fila es la del propio anillo). Cambiarla obliga a mover `cf` = `(FILAS − 1) / 2`; con `FILAS` par el castillo no se centra | Tablero más ralo y más rodeos, sin medir | Sin flanqueo: se pierden opciones de planificar (pilar 1) |
| Margen del castillo al borde (= `cc_azul`, la columna del centro; el muro queda a 0 del borde) | 2 | **Solo 2** (= `RADIO_MURO`). Con `cc` > `RADIO_MURO` el castillo se despega del borde: cada +1 baja la distancia entre centros en 2 (`distancia_frente` = 13 − 2 × `cc`: 3 da 7, con golpe en el turno 2 con Grito), el anillo pasa de 17 a 24 casillas y cambian AC-11, AC-22, AC-33, el orden F4 de Reclutamiento (#10) y las casillas libres de la Interfaz del Modificador (#17) | Contacto antes y espalda libre para rodear el castillo | Piso duro `RADIO_MURO`: el bloque tiene que caber |
| `RADIO_MURO` (con el muro pegado al borde: `cc` = `RADIO_MURO`, `distancia_frente` = 17 − 4 × `RADIO_MURO`) | 2 | **Seguro 2.** Válidos 1 a 3: con 1 el castillo es de 3 × 3, sin interior para nadie más que el Rey (`distancia_frente` 13, otro juego); con 3, `distancia_frente` 5 y el escudero y el arquero azules golpean en el turno 1 (margen −1 los dos). Con 3 el centro pasa a `cc` = 3, por eso cabe; AC-17 prueba otro caso, `cc` = 2 con radio 3, que no cabe. Con 4 o más, `distancia_frente` < 2: inválido | Más tramos (`8 × radio`) y menos distancia de frente | Con 1 el castillo es de 3 × 3: 8 tramos y solo el Rey adentro |
| Composición inicial (soldados / arqueros / escuderos) | 4 / 4 / 4 | Constante de la versión de reglas (regla 6): cambiarla sube la versión y obliga a revalidar `casilla_inicial` (`i = 0..3`), la regla 7 y AC-7. Como palanca, de 1 hasta el `topeVivas` de cada tipo | Ejército más ancho y partida más larga | Pocas piezas para combinar (pilar 2) |

**Relación entre `COLUMNAS` y el turno de contacto** (con `movimiento` 4 y alcance 4 / 1 / 2 en arquero, soldado y escudero, margen al borde 2 y `RADIO_MURO` 2):

| `COLUMNAS` | Distancia entre centros | `distancia_frente` | Contacto posible en el turno | `margen_primer_golpe` turno 1 / turno 2 con Grito (bono 2) |
|---|---|---|---|---|
| 24 | 19 | 9 | **2** (lo probado en los prototipos) | 3 / **1** |
| 22 y 23 | 17 y 18 | 7 y 8 | **2** | 1 y 2 / −1 y 0: rojo con Grito golpea a un azul quieto |
| 21 | 16 | 6 | **2**, pero arqueros y escuderos golpean en el turno 1 | 0 / −2 |
| 17 a 20 | 12 a 15 | 2 a 5 | **1** (azul golpea antes de que rojo mueva) | negativo |
| 25 o más | 20 o más | 10 o más | 3 o más | 4 o más / 2 o más |

**Perillas que interactúan:**

- `COLUMNAS`, el margen, `RADIO_MURO`, el `movimiento` y el `alcance_max` del Catálogo y `bono_grito_movimiento` (#11) se miran juntos: los tres primeros fijan la `distancia_frente`, y los otros deciden en qué turno llega el contacto (`turnos_hasta_contacto`) y el primer golpe (`margen_primer_golpe`).
- **Soldados iniciales y `piezas_para_matar` del Catálogo:** `ceil((escudo + vida del Rey) / ataque del soldado) = ceil(35 / 8) = 5`. **Sin Grito**, 4 soldados solos no matan al Rey con un golpe combinado, y 5 o más sí. **Con Grito** (+2 de ataque) alcanzan 4: `ceil(35 / 10) = 4`, porque 4 × 10 − 5 = 35 ≥ 30. El Rey y el ataque del soldado no cambiaron en la v5. Subir los soldados iniciales exige revisar eso con Resolución (#9).
- **Composición y `topeVivas`:** tienen que cumplir `composición ≤ topeVivas` (si no, `ConfigInvalida(T6)`).

## Visual/Audio Requirements

El Tablero no dibuja ni suena: define la geometría que Render (#18) pinta, y el sonido es de Audio (#22). Requisitos que impone, según el art bible (`design/art/art-bible.md`):

- **El despliegue aparece de golpe** al crear la partida, sin animación de entrada: el tablero tiene que leerse completo en el turno 1 (pilar 4: no se gasta tiempo en una introducción). El único sonido de inicio es de Audio (#22).
- **El terreno sigue un poco más allá de las 24 × 15 casillas** y se oscurece hacia los bordes (§6, densidad de elementos). Los muros laterales están pegados a ese borde: el muro tiene que leerse entero, sin que el HUD lo tape (los botones viven en los 144 px laterales, fuera del tablero).
- **El castillo es un bloque de 5 × 5 con 16 tramos rectos de una casilla** (§3, "Castillo y muros"); el interior es mínimo, solo empedrado (§6).
- **Los dos bandos se distinguen por color y emblema, nunca por posición** (§4, colores de bando): como el despliegue es simétrico, la posición no alcanza para saber de quién es una pieza, y nada puede depender solo del color.
- **Sin cuadrícula dibujada por defecto** (§3, terreno). Cómo se cuentan las distancias: ver UI Requirements.
- **Art bible §7:** corregido el 2026-11-25; los castillos quedan pegados a los bordes laterales, en las filas 5 a 9.

> 📌 **Asset Spec** — Visual/Audio requirements are defined. After the art bible is approved, run `/asset-spec system:tablero-y-despliegue-inicial` to produce per-asset visual descriptions, dimensions, and generation prompts from this section.

## Game Feel

No aplica: el Tablero no responde a la entrada del jugador. La sensación de seleccionar y mover es de Entrada (#13) y Movimiento (#6). Lo único que aporta es que `CrearPartida` y sus consultas son baratas (AC-29: 1 ms para crear la partida y un presupuesto por lote para las consultas).

## UI Requirements

Sin pantalla propia: el tablero ocupa toda la pantalla (decisión del jugador, 2026-11-17; art bible §7).

- **Medidas:** casillas de 68 px a 1080p; 24 × 68 = 1632 px de ancho y 15 × 68 = 1020 px de alto, con 60 px arriba y 144 px a cada lado para el HUD. Mínimo 1080p.
- **Las coordenadas son internas:** el jugador no ve "columna 5, fila 7" (salvo en modo de depuración).
- **Contar distancias sin cuadrícula dibujada** (decisión del 2026-11-25): por defecto no hay cuadrícula, como pide el art bible. Al elegir una pieza se marcan las casillas alcanzables (Movimiento, #6) y el alcance (Alcance, #7). Una opción **"Mostrar cuadrícula"**, apagada por defecto, queda en Accesibilidad (#23) y Ajustes (#20).
- **Lo que consume:** Ficha y HUD (#15) y Vista previa (#16) leen las dimensiones; Render (#18) pinta el terreno y el castillo.
- **Castillos y anillos despejados:** ningún recuadro de la Interfaz del Modificador (#17, F5) tapa las columnas 0 a 5 ni 18 a 23 (castillos y anillos de reclutamiento). **Pedido a #17 (2026-11-27):** en el turno 2, cuando rojo decide qué hacer con su cara, el recuadro tampoco tapa las filas 4 a 10 entre las columnas 5 y 18, donde están los dos ejércitos y las franjas de amenaza (columnas 8 a 14), **ni ninguna casilla con una pieza azul o dentro del alcance rojo de ese turno** (azul puede terminar el turno 1 en las filas 1 a 13, y los reclutas de (18, 4) y (18, 10) amenazan fuera del carril; el alcance lo calcula #7). Va en las filas 0–3 u 11–14, en la parte que quede libre de todo eso. El HUD (#15) no tapa ninguna casilla salvo las leyendas centrales de ≤ 1 s.

> **📌 UX Flag — Tablero y despliegue inicial**: This system has UI requirements. In Phase 4 (Pre-Production), run `/ux-design` to create a UX spec for each screen or HUD element this system contributes to **before** writing epics. Stories that reference UI should cite `design/ux/[screen].md`, not the GDD directly.

## Cross-References

| Documento | Qué se referencia | Cómo |
|---|---|---|
| `design/gdd/libreria-de-reglas.md` | Regla 2 (`CrearPartida`, castillos fijos), regla 7 (IDs), tabla de dominios (tablero 24 × 15), códigos `ConfigInvalida`, `VersionNoSoportada`, `EstadoInvalido` y `ErrorInterno` | Este GDD adopta el tope 24 × 15 como constante propia; la Librería retiró el sorteo (2026-11-25) y levantó la marca PROVISIONAL del tablero |
| `design/gdd/catalogo-de-piezas.md` | Tipos 0 a 6 y su orden, `topeVivas`, `movimiento`, `piezas_para_matar`, pendientes PROVISIONAL (AC-2, AC-30) | Este GDD cierra los provisionales y calcula el techo de `movimiento` (12), que lee la F9 del Catálogo. Para su AC-30 fija el código `ConfigInvalida(T6)`; el Rey distinto de 1 lo corta su F5 (2026-11-27) |
| `design/gdd/maquina-de-turno.md` | Estados de creación, AC-3, Open Question de quién empieza | Sorteo retirado de la tabla de estados y de AC-3 (2026-11-25); pregunta cerrada (jugador 0, sin compensación) y #4 en su Dependencies |
| `design/gdd/game-concept.md` | "Reglas vigentes" (tablero, ejército, castillo, Rey), pilares 1, 3 y 4, anti-pilares, línea 348 (Procedural Systems) | Línea 348 corregida el 2026-11-25 (castillos en posición fija, sin sorteo) |
| `design/art/art-bible.md` | §3 (terreno sin cuadrícula, castillo y muros), §4 (colores de bando), §6 (densidad y borde), §7 (pantalla, 68 px) | Fuente de los requisitos visuales; §7 corregido el 2026-11-25 (castillos pegados a los bordes laterales, filas 5 a 9) |
| `design/gdd/systems-index.md` | Sistema #4, su texto ("sectores al azar") y las aristas | Texto del #4 corregido y arista de datos #4 → #5 anotada (2026-11-25) |
| `design/registry/entities.yaml` | `tablero_columnas` 24, `tablero_filas` 15, `radio_muro` 2, `distancia_frente`, `turnos_hasta_contacto`, `techo_movimiento`; `opcion_por_sorteo` (ahora deprecated) | Registradas el 2026-11-25 |
| `prototypes/rolchess-v5-concept/README.md` | «Qué cambió desde la v4» (castillos siempre en 4 y 6, turno 1 sin modificador) | Fuente de los datos vigentes (v5) |
| `prototypes/rolchess-v4-concept/` | Despliegue y sectores (`RolchessV4.cs:262-320`), README de partidas | Fuente del código del despliegue; el código no se migra |
| `design/gdd/modificador.md`, `design/gdd/metricas-locales-de-partida.md` | Regla 2 y tabla de apertura (AC-29) del Modificador; regla 14 de Métricas | `margen_primer_golpe`, regla 10 y la pregunta de ventaja de turno |
| `design/gdd/gdd-cross-review-2026-11-26.md` | W-13, DW-3, W-21 | Resueltos en la revisión del 2026-11-27 |

## Acceptance Criteria

Todos son automatizables en Edit Mode (lógica pura, sin pantalla); el dibujo del tablero es de Render (#18) y tendrá sus propias capturas. Tipos: **Logic** (prueba unitaria) e **Integration** (con otro sistema). **Los números de AC son estables y no siguen el orden de la página:** los que se agregaron en revisiones quedaron en la sección que prueban. Todo `ConfigInvalida` esperado se afirma **con su código** (T4, T5, T6 del Tablero o F1–F9 del Catálogo), como en el Catálogo.

**Reglas (Core Rules)**

- **AC-1 (regla 1, grilla)** — **GIVEN** un tablero recién creado, **WHEN** se consulta `dentro` para `(0,0)`, `(23,14)`, `(24,0)`, `(0,15)` y `(−1,0)`, **THEN** devuelve verdadero, verdadero, falso, falso y falso, y el tablero tiene 360 casillas. *(Logic)*
- **AC-2 (regla 2, vecindades)** — **GIVEN** el tablero vacío, **WHEN** se piden las vecindades, **THEN** la cruz de `(0,0)` tiene 2 casillas y la de `(5,5)` tiene 4; el contorno de `(0,0)` tiene 3 y el de `(5,5)` tiene 8; la cruz de `(23,14)` tiene 2 y su contorno 3; la cruz de `(0,7)` tiene 3 y su contorno 5; ninguna casilla fuera de la grilla aparece como vecina; **y** la cruz y el contorno de `(−1,0)` y de `(24,7)` son la lista vacía, sin error. *(Logic)*
- **AC-3 (regla 3, un ocupante)** — **GIVEN** el despliegue inicial, **WHEN** se recorren las 360 casillas, **THEN** los 26 IDs están en 26 casillas distintas, las otras 334 dan `ocupante` = «ninguno» y ninguna pieza inicial está sobre un tramo. *(Logic)*
- **AC-4 (regla 4, despliegue fijo)** — **GIVEN** cada par ordenado `(azul, rojo)` de sectores 1 a 9 (81 pares, incluidos (4,4), (2,8), (7,3) y (6,4)), **WHEN** se llama `CrearPartida`, **THEN** solo (4, 6) crea la partida; los otros 80 dan `ConfigInvalida(T4)` y no devuelven estado. **Y** los pares con un sector fuera de 1 a 9, (0, 6) y (4, 10), también dan `ConfigInvalida(T4)`. *(Logic)*
- **AC-5 (regla 5, castillos)** — **GIVEN** el despliegue inicial, **THEN** el castillo azul tiene 16 tramos, de dueño azul, en las filas 5 y 9 (columnas 0 a 4) y en las columnas 0 y 4 (filas 6 a 8), 9 casillas de interior y el Rey en `(2,7)`; el rojo tiene 16 tramos de dueño rojo en las filas 5 y 9 (columnas 19 a 23) y en las columnas 19 y 23 (filas 6 a 8), 9 de interior y el Rey en `(21,7)`. *(Logic)*
- **AC-6 (regla 6, ejército)** — **GIVEN** el Catálogo vigente con un solo cambio por caso, preparado con `PrepararContexto` (pasa F1 a F8: bajar un tope no rompe F5, F6 ni F8), y la config vigente, **WHEN** se llama `CrearPartida`, **THEN** devuelve `ConfigInvalida(T6)` y ningún estado con `topeVivas` de soldados = 3, de arqueros = 3 o de escuderos = 3; **y** crea la partida con 4 / 4 / 4 (frontera exacta). **Y GIVEN** el `topeVivas` del Rey en 0 o en 2, **WHEN** se llama `PrepararContexto`, **THEN** `ConfigInvalida(F5)` y no hay contexto con qué llamar a `CrearPartida` (Catálogo, AC-9). *(Logic)* **Y GIVEN** el Catálogo vigente, **THEN** cada jugador tiene 4 soldados, 4 arqueros, 4 escuderos y 1 Rey (13 piezas). *(Integration con Catálogo)*
- **AC-7 (reglas 5, 7 y 8, posiciones)** — **GIVEN** un fixture escrito a mano como literal C# (26 entradas `ID, jugador, tipo, casilla` y 32 entradas `casilla, dueño`, copiadas de las reglas 5, 7 y 8, no generadas por el código ni leídas de un archivo), **WHEN** se compara con el estado que devuelve `CrearPartida`, **THEN** coinciden entrada por entrada y no sobra ni falta ninguna. *(Logic)*
- **AC-8 (regla 8, IDs)** — **GIVEN** el despliegue inicial, **WHEN** se leen los IDs, **THEN** azul tiene soldados 1 a 4, arqueros 5 a 8, escuderos 9 a 12 y Rey 13; rojo tiene 14 a 17, 18 a 21, 22 a 25 y Rey 26; y el próximo ID es 27. *(Integration con Librería)*
- **AC-9 (regla 9, simetría)** — **GIVEN** el despliegue inicial, **THEN** para cada pieza `(ID, tipo, casilla)` azul existe exactamente una pieza roja `(ID + 13, tipo, espejo(casilla))`, para cada tramo existe exactamente un tramo del otro dueño en `espejo(casilla)`, y los conteos coinciden (13 ↔ 13 piezas, 16 ↔ 16 tramos). *(Logic)*
- **AC-10 (regla 10, quién empieza)** — **GIVEN** una partida recién creada, **WHEN** se lee el estado, **THEN** la partida está En curso, en el turno 1, del jugador 0, en la fase Mover, sin tirada pendiente ni evento de reclutar. *(Integration con Máquina de turno)*
- **AC-11 (regla 11, consultas)** — **GIVEN** el despliegue inicial, **THEN** `ocupante(7,5)` es el ID 9, `ocupante(2,7)` el 13 y `ocupante(21,7)` el 26; `hayTramo(0,5)` es verdadero y `hayTramo(2,7)` falso; `ladoDelRival(0)` es +1 y `ladoDelRival(1)` es −1. *(Logic; el anillo se prueba en AC-22)*
- **AC-12 (regla 12, jugador no puede)** — **GIVEN** los comandos de jugador de la Librería (todos menos `CrearPartida`), **WHEN** un test los recorre por reflexión, **THEN** el conjunto de comandos públicos (nombre y firma) es exactamente la lista fija escrita en el test, copiada del contrato de la Librería (si la Librería tiene un AC propio con esa lista, este test usa la misma y la cita, para que no haya dos fuentes): un comando nuevo, por ejemplo `ElegirLado(int)`, hace fallar el test hasta que alguien lo revise y lo agregue a la lista. Y `CrearPartida` solo acepta el par (4, 6) (AC-4). *(Logic)*

**Fórmulas**

- **AC-13 (`indice_casilla`)** — **GIVEN** el tablero, **THEN** `(0,0)` da 0, `(23,14)` da 359 y `(21,7)` da 189, y la inversa de 189 es `(21,7)`; **y** una consulta con `(24,5)` da "fuera de la grilla" y **no** devuelve lo que hay en `(0,6)` (un tramo): `hayTramo(24,5)` es falso. *(Logic, límite)*
- **AC-14 (`distancia_cruz`)** — **THEN** la distancia de una casilla a sí misma es 0, de `(0,0)` a `(23,14)` es 37 y de `(7,5)` a `(16,8)` es 12. *(Logic, límite)*
- **AC-15 (`casilla_de_castillo`)** — **GIVEN** el castillo azul, **THEN** `(4,5)` es muro, `(5,6)` y la esquina `(5,4)` son anillo, `(3,6)` y `(2,7)` son interior, `(12,7)` no es nada, y el castillo tiene 16 casillas de muro y 9 de interior; el rojo da lo mismo en `espejo` de esas casillas. **Y GIVEN** la función con `RADIO_MURO` = 1 como parámetro, **THEN** el interior tiene 1 casilla y el muro 8 (ninguna casilla es interior y muro a la vez). *(Logic, límite)*
- **AC-16 (`casilla_inicial`)** — **THEN** el escudero rojo con `i = 3` está en `(16,8)`, el arquero azul con `i = 0` en `(5,5)`, y las 24 piezas que no son el Rey coinciden con la tabla de la regla 7. *(Logic, límite)*
- **AC-17 (`distancia_frente`)** — **THEN** `distancia_frente(cc_azul 2, cc_rojo 21, RADIO_MURO 2, linea_max 2)` = 9. **Y** `invarianteFrente(cc_azul, cc_rojo, radio, linea_max)` pasa con `(2, 14, 2, 2)` (da 2; equivale a `COLUMNAS` = 17) y falla con `(2, 13, 2, 2)` (da 1). **Y** `castilloCabe(cc, cf, radio, columnas, filas)` pasa con `(2, 7, 2, 24, 15)` y falla con `(2, 7, 3, 24, 15)` (con `cc` = 2 el muro cae en la columna −1; con `cc` = 3 cabe, ver Tuning). Cada invariante se prueba por separado. *(Logic, límite)*
- **AC-18 (`turnos_hasta_contacto`)** — **THEN** con `distancia_frente` 9: `m` = 4 da 2, `m` = 6 da 2, `m` = 2 da 4, `m` = 1 da 8, `m` = 8 da 1 y `m` = 12 da 1; con `distancia_frente` 2 y `m` = 1 da 1. *(Logic, límite)*
- **AC-19 (`techo_movimiento`)** — **THEN** con las constantes vigentes da 12 y `techo_movimiento(k)` = 13 − `alcance_max(k)`. **Y GIVEN** el Catálogo vigente, **WHEN** se recorren los 7 tipos, **THEN** para cada uno `movimiento_k + alcance_max_k ≤ 13`, leyendo los dos valores del Catálogo (sin literales en el test). **Y GIVEN** un Catálogo de prueba con una catapulta de `movimiento` 8 y `alcance_max` 6 declarado, **THEN** el chequeo falla (8 + 6 = 14), aunque ese tipo pase F9 (8 + 2 = 10 ≤ 12). Son dos invariantes distintos: `m + R ≤ 13` es este oráculo de test; `m + bono ≤ 12` lo valida F9 en `CrearPartida` (Catálogo, AC-35), que lee la constante `techo_movimiento` exportada por el Tablero, no un literal. *(Integration con Catálogo; el segundo, Logic con dobles)*
- **AC-20 (`espejo`)** — **THEN** `espejo(2,7)` da `(21,7)` y `espejo(5,5)` da `(18,5)`; aplicarlo dos veces devuelve la casilla original para las 360 casillas, y ninguna casilla es su propio espejo (24 columnas es par). *(Logic)*
- **AC-32 (`margen_primer_golpe`)** — Firma única: `margen_primer_golpe(t, bono, vista)`, donde `vista` es una vista de solo lectura con `movimiento` y `alcance_max` por tipo del ejército inicial y la lista de tipos reclutables (no un Catálogo validado: así el test la arma sin pasar F1; la firma la fija el ADR).
  - **Parte Logic** *(Logic, límite)*. **GIVEN** las constantes vigentes y una vista armada en el test: arquero, soldado y escudero con `movimiento` 4 y `alcance_max` 4 / 1 / 2, y como único tipo reclutable una catapulta de `movimiento` 2 y alcance 6. **THEN**:
    - con `t` = 1 y bono 0 da 3;
    - con `t` = 1 y bono 2 sigue en 3, porque en el turno 1 el bono se ignora;
    - con `t` = 2 y bono 2 da 1;
    - con `t` = 2 y bono 3 da 0 (golpe);
    - con `t` = 2 y bono 0 da 3 (el caso reclutar: ejército sin bono y recluta 11 − 8);
    - si el reclutable pasa a `movimiento` 6 y alcance 5, con `t` = 2 y bono 0 da 0 (11 − 11).
    
    Las distancias `d_k` salen de `distancia_frente` + `linea_max` − `linea_k` y de la columna del anillo, no de un literal.
  - **Parte Integration** *(Integration con Catálogo y Modificador: es el test que justifica el tope del bono)*. **GIVEN** el Catálogo vigente (su vista) y el tope del **rango seguro** de `bono_grito_movimiento` del Modificador (#11), hoy **2** (no el dominio válido de la Librería, que llega a 10), **THEN** `margen_primer_golpe(1, ·)` ≥ 1 y `margen_primer_golpe(2, 2)` ≥ 1.

**Casos de Edge Cases que podrían salir rotos**

- **AC-21 (casilla fuera de la grilla)** — **GIVEN** el despliegue inicial, **WHEN** se llama `ocupante`, `hayTramo` y `dentro` con centinelas cuyo índice ingenuo cae sobre algo: `(−3,8)` (índice 189, el Rey rojo), `(26,6)` (índice 170, `(2,7)`, el Rey azul) y `(24,5)` (índice 144, `(0,6)`, un tramo); y además con `(−1,0)`, `(0,15)` y `(5,−1)`, **THEN** `ocupante` responde «ninguno», `hayTramo` y `dentro` responden falso, sin excepción. *(Logic, límite: con el error de envoltura el test devolvería un Rey o un tramo)*
- **AC-22 (anillo recortado)** — **GIVEN** cada castillo, **THEN** `anillo(0)` es exactamente {fila 4, columnas 0–5} ∪ {fila 10, columnas 0–5} ∪ {columna 5, filas 5–9} (17 casillas, ninguna fuera de la grilla) y `anillo(1)` es el `espejo` de esas casillas; las dos listas incluyen las casillas ocupadas por los arqueros iniciales, `(5,5)`–`(5,8)` y `(18,5)`–`(18,8)`. *(Logic)*
- **AC-23 (tramo roto)** — **GIVEN** un estado cargado con el tramo `(0,5)` en vida 0, **THEN** `(0,5)` sigue clasificada como muro, `hayTramo(0,5)` es falso y `ocupante(0,5)` es «ninguno». *(Logic)* **Y GIVEN** el estado de AC-31 (arquero ID 5 en `(4,6)` sobre su tramo), armado con la misma factory de pruebas que usa AC-31 para que ningún test dependa del orden de ejecución, **WHEN** Castillo y muros (#5) baja la vida del tramo `(4,6)` a 0, **THEN** `ocupante(4,6)` sigue siendo 5, `hayTramo(4,6)` es falso y la casilla sigue clasificada como muro. *(Integration con #5)*
- **AC-24 (estado cargado inválido)** — **GIVEN** la config vigente (`vida_tramo` 30) y un estado con dos piezas en `(7,5)`, o una pieza en `(24,3)`, o un tramo en `(10,10)`, o un tramo en `(3,6)` (interior), o un tramo azul en el perímetro rojo, o dos tramos en la misma casilla, o 31 tramos, o un tramo con vida −1 o con vida `vida_tramo` + 1, **WHEN** se carga, **THEN** `EstadoInvalido`. **Y** un tramo con vida 0 carga, porque es un tramo roto (AC-23). *(Logic)*
- **AC-25 (versión)** — **GIVEN** un estado que declara un tablero de 25 × 15, o de 24 × 16, o `RADIO_MURO` 3, **WHEN** se carga, **THEN** `VersionNoSoportada`; **y** un estado de 25 × 15 con una pieza en `(24,3)` da `VersionNoSoportada`, no `EstadoInvalido` (la versión se valida primero). *(Logic)*
- **AC-26 (constante sin versión)** — **GIVEN** la función `HuellaGeometria(columnas, filas, radioMuro, centros, lineas, composicion)`, la tabla fija del repositorio `versionDeReglas → huella` (un literal escrito a mano) y las constantes con nombre del código, **WHEN** el test calcula `HuellaGeometria` con esas constantes, **THEN** el resultado es igual a `tabla[versionDeReglas vigente]`, y si la versión vigente no tiene fila en la tabla el test falla. Así, cambiar una constante sin subir la versión y sin registrar la huella nueva hace fallar el test (Edge Cases). *(Logic; el mismo mecanismo que la huella del Catálogo)*
- **AC-27 (vida de tramo)** — **GIVEN** una config con `vida_tramo` 0, −1 o 1001, **WHEN** se crea la partida, **THEN** `ConfigInvalida(T5)`. **Y GIVEN** `vida_tramo` 1 y 1000, **THEN** la partida se crea y los 32 tramos tienen exactamente esa vida. *(Integration con #5)*
- **AC-30 (`ladoDelRival` inválido)** — **GIVEN** el tablero, **WHEN** se llama `ladoDelRival(2)` o `ladoDelRival(−1)`, **THEN** `ErrorInterno` (el código de resultado de la Librería, no una excepción). *(Logic)*
- **AC-31 (arquero sobre tramo, regla 3)** — **GIVEN** un estado cargado con el arquero azul ID 5 en `(4,6)` sobre un tramo azul con vida > 0, **THEN** carga, `ocupante(4,6)` = 5 y `hayTramo(4,6)` es verdadero. **Y GIVEN** el arquero rojo ID 18 sobre ese tramo azul, o el soldado azul ID 1 sobre `(4,6)`, o el arquero ID 5 y otra pieza en `(4,6)`, **THEN** `EstadoInvalido`. *(Logic)*
- **AC-33 (anillo al empezar, regla 7)** — **GIVEN** el despliegue inicial, **THEN** el anillo azul tiene 13 casillas libres y el rojo 13. **Y WHEN** se consulta `orden_aparicion` de Reclutamiento (#10, F4) sobre ese estado (es una consulta, no un recluta real: en el turno 1 nadie recluta), **THEN** los 4 primeros reclutas azules irían a `(5,9)`, `(5,4)`, `(5,10)` y `(4,4)` y los rojos a `(18,9)`, `(18,4)`, `(18,10)` y `(19,4)`. *(Integration con Reclutamiento)*
- **AC-34 (config inválida, sin estado parcial)** — **GIVEN** una lista fija escrita en el test con un caso por código: el par (6, 4) (T4), `vida_tramo` 0 (T5), soldados con `topeVivas` 3 (T6) y el `movimiento` del Jinete en 11 con los topes de AC-35 del Catálogo (F9), **WHEN** se llama `CrearPartida` con cada uno, **THEN** devuelve solo `ConfigInvalida` con ese código, sin tablero ni piezas. *(Logic)*
- **AC-37 (orden de los códigos del Tablero, regla 6)** — **GIVEN** una config con el par (6, 4) y un Catálogo con soldados en `topeVivas` 3, **WHEN** se llama `CrearPartida`, **THEN** `ConfigInvalida(T4)`. **GIVEN** `vida_tramo` 0 y soldados en 3, **THEN** `ConfigInvalida(T5)`. *(Logic; el orden frente a F9 lo prueba la Librería)*
- **AC-35 (invariante de ocupación, regla 3)** — **GIVEN** el despliegue inicial, **WHEN** se intenta escribir una pieza en `(7,5)` (ocupada por el ID 9) o en `(24,3)`, **THEN** `ErrorInterno` y el estado no cambia (misma huella de estado de la Librería). **Y GIVEN** el arquero azul ID 5 en una casilla libre y el tramo azul `(4,6)` con vida > 0, **WHEN** se escribe el arquero 5 sobre `(4,6)`, **THEN** se acepta; **y WHEN** se escribe ahí el arquero rojo 18 o el soldado azul 1, **THEN** `ErrorInterno` y la huella de estado no cambia. **Y** escribir una pieza sobre un tramo con vida 0 se acepta: la casilla está libre. **Y WHEN** se llama `distanciaCruz((−1,0), (0,0))`, **THEN** `ErrorInterno`. *(Logic)*

**Cruce con otros sistemas y rendimiento**

- **AC-28 (determinismo)** — **GIVEN** la misma config, **WHEN** se llama `CrearPartida` dos veces, **THEN** los dos estados son idénticos (misma huella de estado de la Librería). *(Integration con Librería)*
- **AC-29 (rendimiento)** — **GIVEN** Unity Performance Testing en Edit Mode, 10 calentamientos y 1000 mediciones en la máquina de referencia provisional (la de desarrollo, con su modelo de CPU anotado en el informe del test), **WHEN** se mide `CrearPartida` con la config vigente, **THEN** el percentil 95, calculado por el test a partir de las muestras, es ≤ 1 ms. **Y WHEN** se miden 100 lotes de 10 000 llamadas de `ocupante` + `anillo` + contorno, **THEN** el percentil 95 por lote es ≤ 1 ms. *(Performance; ADVISORY, como el AC-25 de la Librería: en Edit Mode corre en Mono y no representa IL2CPP. Pasa a BLOCKING, con la máquina de referencia del ADR de la Librería, cuando el ADR la fije)*
- **AC-36 (sin golpe del despliegue, propiedad)** — **GIVEN** el despliegue inicial y Movimiento (#6) y Alcance (#7) reales, **WHEN** se enumeran todos los destinos de todas las piezas azules en el turno 1 (sin bono) y, para cada uno, sus casillas en alcance, **THEN** ninguna contiene una pieza o un tramo rojo. Los destinos incluyen la casilla de origen (0 pasos), que es el caso Distracción. El test comprueba también que cada destino con la columna máxima de su tipo esté libre: es la casilla de llegada que suponen los oráculos. Movimiento deja atravesar aliados (regla 5), así que nadie queda tapado. Para rojo en el turno 2, con el despliegue azul sin mover, hay dos casos:
  - **Grito:** lo mismo con las piezas rojas y `bono_grito_movimiento` aplicado. **THEN** ninguna casilla en alcance contiene una pieza o un tramo azul.
  - **Reclutar:** sin bono, con cada tipo reclutable del Catálogo puesto en cada una de las 13 casillas libres del anillo rojo, se enumeran sus destinos con su movimiento base, y los de las 13 piezas rojas sin bono. **THEN** ninguna casilla en alcance contiene una pieza o un tramo azul.
  
  *(Integration con Movimiento, Alcance y Catálogo; falla si cambian el Catálogo, el bono o la geometría. Por ejemplo, un tipo reclutable con `movimiento` 6 y alcance 5 lo hace fallar.)*

**Sin criterio, a propósito:**

- *"Nadie más guarda coordenadas del tablero"* (regla 11) es una regla de arquitectura, no un comportamiento: va al ADR y al manual de control, y se verifica con un test de referencias entre assemblies.
- *"Se vigila con las Métricas"* (regla 10) y la *tierra de nadie* (Edge Cases) son preguntas de playtest: van a Open Questions.
- *"Rojo tira en el turno 2 con la tabla 20/40/20/20/0"* (regla 10) es del Modificador: lo prueba su AC-29.

## Open Questions

| Pregunta | Dueño | Para cuándo |
|---|---|---|
| **Ventaja de turno (DW-3 de la revisión cruzada; reescrita el 2026-11-27).** El despliegue es parejo, el dado no. En el turno 1 azul no tira. En el turno 2 rojo tira con la apertura 20/40/20/20/0 y planifica sabiendo su cara. **Zonas del turno 2**, es decir, hasta qué columna golpea rojo: con Distracción (20%) no mueve y llega a la 14; con reclutar (40%) llega a la 10, también con el recluta; con Grito (40%) llega a la 8 (`margen_primer_golpe`). Para un avance azul desde la columna 7, en las filas del frente: **las columnas 8 y 9 quedan expuestas en el 40% de los casos (solo con Grito)** y **la 10 y la 11, lo más lejos que llega azul en el turno 1, en el 80% (siempre que rojo mueva)**. El avance completo de un escudero (4, hasta la 11), que es el del ejemplo de `turnos_hasta_contacto`, cae en la franja del 80%. Con Grito (+2 de ataque, escudo restado una vez sobre la suma), un escudero azul en (8, 6) puede morir (Formulas). Las zonas de los turnos siguientes dependen de cómo se movió cada uno y no se publican como constantes. **Corrección:** la v4 mostró ventaja de abrir reclutando (azul lo hizo en 7 de 11 partidas y ganó 5); la v5 **no la quita: se la pasa a rojo**, que es el primero que tira. Sumado al punto muerto (abajo) y al primer recluta de rojo con el anillo tapado (regla 7), las desventajas de azul se acumulan sin compensación. **Disparador (decisión del jugador, 2026-11-27):** en la muestra de 20 partidas de Métricas, **rojo gana 14 o más**, o **rojo da el primer daño en 15 o más**. Si salta, se decide entre (a) azul también tira en el turno 1 o (b) rojo no tira en su primer turno; **las dos quedan abiertas**, sin preferencia. **Medida pedida a #19:** por partida, quién da el primer daño, en qué turno y con qué cara, y si esa partida la gana; quién cruza primero la columna 11/12; y las victorias por color (esta última ya existe, regla 14). | Métricas (#19), playtest; la decisión es del jugador | MVP, al completar 20 partidas |
| **Tope del Grito (W-13; reescrita el 2026-11-27).** El margen contra el primer golpe es **3 en el turno 1** (arquero a 11 que amenaza 4 + 4; escudero a 9 que amenaza 4 + 2; con la v4 era 1) y **1 en el turno 2 con Grito**, contra un azul quieto. La versión anterior de esta pregunta decía «con Grito queda en 1» para el turno 1, que no tiene tirada: el número era bueno, el turno no. El tope de `bono_grito_movimiento` (2) se justifica con el turno 2 (`margen_primer_golpe`, AC-32 y AC-36). El arquero que da un paso a su tramo (+2 de alcance) suma una sola columna respecto del anillo: el azul llega a la 10 y el rojo a la 13. ¿El Grito en el 40% de las aperturas de rojo acelera demasiado los primeros turnos? | Modificador (#11), playtest | En la revisión del Modificador y en el MVP |
| **Punto muerto (reescrita el 2026-11-27).** Si nadie cruza las 8 columnas libres, decide el reloj de 12:30 por jugador, y si los dos agotan sus timers de etapa pierde **azul** (jugó más turnos; puede evitarlo cerrando rápido sus turnos). Si azul avanza, queda expuesto en el turno 2: con Grito (40%) en las columnas 8 y 9, y siempre que rojo mueva (80%) en la 10 y la 11. Hipótesis: esperar favorece a rojo (Edge Cases). En Sin límite la partida no termina sola. ¿Hace falta un desempate del reloj que no castigue a azul (por ejemplo por `vidaQuitada`)? | Máquina de turno (#3), playtest | MVP |
| **Capa de amenaza (2026-11-27).** La fantasía de contar los pasos hasta el primer golpe necesita ver lo que el rival puede golpear en su próximo turno: movimiento + alcance, calculado antes de su tirada, con la franja base y aparte la del Grito (Player Fantasy). El cálculo es de Alcance (#7). ¿Cómo se muestra sin tapar el tablero y con qué gesto se activa? | Vista previa (#16), Ficha y HUD (#15), Entrada (#13); Alcance (#7) para el cálculo | En sus revisiones, antes de `/ux-design` |
| Azul siempre a la izquierda y siempre el primero: no se separa el efecto de lado del de turno. ¿La revancha alterna lados? La medida existe (Métricas, #19, regla 14). | Menú (#21), Métricas (#19) | Fase 1 |
| **F9 del Catálogo frente a `m + R ≤ 13` (2026-11-27).** La F9 valida `movimiento + bono_grito_movimiento ≤ 12` en `CrearPartida`: suma un bono que el recluta no puede tener el turno en que aparece (Formulas, `techo_movimiento`) y no mira el alcance, así que un tipo de `movimiento` 8 y `alcance_max` 6 la pasa y puede golpear el muro rival el turno en que aparece. La condición con alcance (`m + R ≤ 13`) y los topes de `margen_primer_golpe` (8/7/6 y ≤ 10) solo los cubren tests de balance (AC-19, AC-32, AC-36). ¿La F9 pasa a `m + R ≤ 13`, se complementa con ella (por ejemplo `m + bono + R ≤ 13`, si se quiere cubrir una pieza que espera en el anillo y saca Grito después), o queda como está y la condición con alcance sigue siendo de test? | Catálogo (#2); la decisión es del jugador | Sesión de cierre de la tanda A |
| ~~Esquema de config para la vida inicial del tramo (arista de datos #4 → #5)~~ **Resuelta el 2026-11-25:** campo `vida_tramo` en la config (1 a 1.000), validado por la Librería (Castillo y muros, regla 14) | Castillo y muros (#5) | Cerrada |
| Pisos útiles de `FILAS` (7) y de la composición inicial: sin medir. La medida existe (Métricas, #19, regla 14). | Métricas (#19), playtest | Compuerta de duración |
| ¿El jugador percibe el despliegue como parejo? ¿La fila 9 del muro sin pieza delante (regla 7) se lee como un hueco? No automatizable | Playtest | MVP |
| Sin cuadrícula dibujada, ¿se cumple "contar distancias" (pilar 1)? Depende del resaltado de Movimiento (#6) y Alcance (#7), de la capa de amenaza (arriba) y de la opción de Accesibilidad (#23). La cuadrícula ayuda a contar casillas, no círculos ni bonos | #6, #7, #23, playtest | En sus GDD y en el MVP |
