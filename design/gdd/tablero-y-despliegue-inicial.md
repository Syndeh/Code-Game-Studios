# Tablero y despliegue inicial

> **Status**: In Design
> **Author**: Martin Arin + Claude
> **Last Updated**: 2026-11-25
> **Last Verified**: 2026-11-25
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

**Momento ancla:** el primer turno, con los dos ejércitos recién desplegados y el campo vacío entre los castillos. El jugador está contando cuántos pasos hay hasta el primer golpe.

**Pilar que sirve:** Pilar 1, *"Al mover armás la estrategia; al atacar ejecutás el plan y ves sus frutos."* La fantasía central del juego (el plan que sale) empieza acá: sin un campo legible no hay plan que armar.

**Qué le exige al resto del diseño:**

- Una grilla legible de un vistazo y distancias que se puedan contar (Movimiento #6 y Alcance #7 miden en casillas; la Ficha y el HUD #15 y la Vista previa #16 las muestran).
- Ninguna información oculta sobre el campo: respeta el anti-pilar *"NOT información oculta al resolver"*.
- Un despliegue inicial que el jugador pueda reconocer sin texto: mismo ejército, mismas posiciones relativas, "mirando al rival".

**Lo que este encuadre no promete** (queda como pregunta abierta): que quien mueve primero no tenga ventaja (el despliegue es parejo, pero ver Open Questions) y que la distancia entre castillos entre en el presupuesto de 25 minutos (pilar 4). Esto último lo acota `turnos_hasta_contacto` en Formulas y se mide en el MVP.

## Detailed Design

### Core Rules

1. **La grilla.** 24 columnas (0–23) × 15 filas (0–14), 360 casillas. Una casilla es `(columna, fila)`: la columna crece hacia la derecha y la fila hacia abajo. El índice plano es `fila × 24 + columna`, y el orden canónico de recorrido es fila y después columna. Las dimensiones son constantes del contrato: cambiarlas sube la versión de reglas.
2. **Vecindades.** El Tablero define las dos que usan los demás: la **cruz** (4 vecinas: arriba, abajo, izquierda y derecha; la usa Movimiento) y el **contorno** (las 8 vecinas; lo usan Alcance, Cobertura y la explosión del demoledor). La distancia en cruz es `|Δcolumna| + |Δfila|`. Una casilla fuera de la grilla no es vecina de nada.
3. **Un ocupante por casilla.** Una casilla tiene como mucho una pieza, salvo un arquero sobre un tramo de muro de su propio castillo (regla de la Librería). Un tramo ocupa su casilla como muro. Cada casilla está en uno de cuatro estados: libre, con pieza, con tramo, o con tramo y arquero.
4. **Despliegue fijo, sin sorteo.** En esta versión de reglas la config trae los castillos fijos en el par de sectores **4 y 6**: azul (jugador 0) a la izquierda, rojo (jugador 1) a la derecha. **Convención de nombres (decisión del jugador, 2026-11-26):** «azul» y «rojo» son los nombres internos de los jugadores 0 y 1 en reglas, fórmulas y tests de todos los GDD; en pantalla los bandos se pintan **Hueso (jugador 0, azul) y Ciruela (jugador 1, rojo)** (art bible §4). Ningún texto visible dice «azul» ni «rojo». Cualquier otro par se rechaza con `ConfigInvalida`. *Por qué solo ese par:* el 2–8 no entra (15 filas no dejan espacio entre castillos) y los diagonales (1–9 y 7–3) quedan a 11 casillas de frente a frente contra 9, un turno más de contacto. Decisión del 2026-11-24: la partida sin sorteo evita medir una diferencia que nadie pidió.
5. **Los castillos.** Cada castillo es un bloque de 5 × 5 centrado en su Rey: azul en el centro `(2, 7)`, columnas 0–4 y filas 5–9; rojo en `(21, 7)`, columnas 19–23 y filas 5–9. El **muro** son las 16 casillas del perímetro del bloque; el **interior** son las 9 de dentro (3 × 3). El muro queda pegado al borde lateral del tablero (el izquierdo para azul, el derecho para rojo). El Tablero coloca los 16 tramos, identificados por su casilla; su vida inicial es el campo `vida_tramo` de la config (1 a 1.000, hoy 30; dueño: Castillo y muros, #5, regla 14).
6. **El ejército inicial.** Cada jugador empieza con 4 soldados, 4 arqueros, 4 escuderos y el Rey (13 piezas). Las cantidades son constantes de la versión de reglas. Si el Catálogo permite menos de 4 de alguno de esos tres tipos, o más de 1 Rey, `CrearPartida` devuelve `ConfigInvalida` (cierra la pendiente que dejó el Catálogo).
7. **Dónde empiezan las piezas.** Tres líneas de 4, en las filas 5 a 8 (las cuatro filas que van de 2 antes del Rey a 1 después). El Rey en el centro del castillo. Arqueros pegados al muro, escuderos al frente:

| Pieza | Azul (mira a la derecha) | Rojo (mira a la izquierda) |
|---|---|---|
| Rey | (2, 7) | (21, 7) |
| Arqueros | columna 5, filas 5–8 | columna 18, filas 5–8 |
| Soldados | columna 6, filas 5–8 | columna 17, filas 5–8 |
| Escuderos | columna 7, filas 5–8 | columna 16, filas 5–8 |

8. **IDs.** Siguen la regla 7 de la Librería: azul primero; dentro de cada jugador, por orden de tipo del Catálogo y después por casilla (fila, columna). Azul: soldados 1–4, arqueros 5–8, escuderos 9–12, Rey 13. Rojo: soldados 14–17, arqueros 18–21, escuderos 22–25, Rey 26. El próximo ID es 27.
9. **Simetría de espejo.** El despliegue es simétrico respecto del eje vertical entre las columnas 11 y 12: a cada pieza azul de tipo `t` en `(c, f)` le corresponde una roja de tipo `t` en `(23 − c, f)`, y lo mismo vale para los tramos. Es una propiedad que se testea.
10. **Quién empieza.** La partida nace **En curso**, en el turno 1, fase Mover, del jugador 0 (azul). Sin tirada del Modificador y sin compensación. Se vigila con las Métricas locales (#19) y se revisa con las partidas del MVP.
11. **Consultas que expone (solo lectura).** `dentro(casilla)`, `ocupante(casilla)`, `hayTramo(casilla)`, `distanciaCruz(a, b)`, las dos vecindades, el **anillo** de un castillo (las casillas de afuera pegadas a su muro), su **interior** (las 9 de dentro) y `ladoDelRival(jugador)` (+1 para azul, −1 para rojo). Nadie más guarda coordenadas del tablero.
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
| #2 Catálogo | Orden de tipos y `topeVivas` | `ConfigInvalida` si el ejército inicial no cabe | Catálogo |
| #3 Máquina de turno | — | "Empieza el jugador 0, fase Mover" | Máquina de turno |
| #5 Castillo y muros | Vida inicial del tramo (`vida_tramo`, campo de la config) | Casillas del muro, del interior y del anillo | Castillo y muros |
| #6 Movimiento | — | `dentro`, `ocupante`, `hayTramo`, cruz, `distanciaCruz`, anillo e interior | Tablero |
| #7 Alcance | — | `dentro`, `ocupante`, `hayTramo`, contorno de 8 y límites de la grilla | Tablero (grilla); Alcance (círculo) |
| #8, #9 Cobertura y Resolución | — | `ocupante`, contorno | Tablero |
| #10 Reclutamiento | — | Anillo entero, `ocupante` y `ladoDelRival` | Tablero |
| #15, #16, #18 Cliente | — | Dimensiones, casillas, despliegue (solo lectura) | Tablero |
| #19 Métricas | — | Constantes de la partida (el par de sectores es fijo) | Métricas |

**Suposiciones provisionales de esta sección:**

- *`ladoDelRival`* es una constante (+1 / −1) porque hay un solo par; con otros pares habría que volver a una orientación calculada.
- *Azul siempre a la izquierda y siempre el primero:* no se puede separar el efecto de "lado" del de "turno" en las métricas. Queda para Open Questions (la revancha del menú, #21, podría alternar lados).

## Formulas

El Tablero no tiene balance propio: su geometría es una constante del contrato y las fórmulas de abajo son cuentas de enteros sobre ella. No hay ciclos de retroalimentación (la geometría es estática). Las dos últimas, `turnos_hasta_contacto` y `techo_movimiento`, son **oráculos para tests de balance**: no entran al núcleo.

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

`radio(c, f) = max(|c − cc|, |f − cf|)`, con el centro `(cc, cf)` del castillo. **Interior** si `radio ≤ 1`; **muro** si `radio = RADIO_MURO`; **anillo** si `radio = RADIO_MURO + 1`. `tramos = 8 × RADIO_MURO`.

| Variable | Type | Range | Source | Description |
|----------|------|-------|--------|-------------|
| cc, cf | int | azul (2, 7); rojo (21, 7) | constant (contrato) | Centro del castillo (casilla del Rey) |
| RADIO_MURO | int | 2 | constant (contrato) | Distancia del centro al muro: bloque de 5 × 5 |

**Output Range:** `radio` 0 a 23. Con `RADIO_MURO` = 2: **16 tramos** y **9 casillas de interior**. Para que el castillo quepa: `cc − RADIO_MURO ≥ 0`, `cc + RADIO_MURO ≤ 23`, `cf ± RADIO_MURO` dentro de 0–14 (tope 7 en vertical) y `cc_rojo − cc_azul > 2 × RADIO_MURO`. Con 1 el castillo sería de 3 × 3: 8 tramos y solo el Rey adentro.
**Example:** `(4, 5)` para el castillo azul: |4 − 2| = 2 y |5 − 7| = 2, radio 2, **muro** (la esquina). `(5, 6)`: |5 − 2| = 3, radio 3, **anillo**.

The `casilla_inicial` formula is defined as:

`columna = cc_j + lado_j × (RADIO_MURO + 1 + linea_tipo)` y `fila = cf_j − 2 + i`, con `i = 0..3`.

| Variable | Type | Range | Source | Description |
|----------|------|-------|--------|-------------|
| j | int | 0 (azul), 1 (rojo) | constant | Jugador |
| lado_j | int | +1 azul, −1 rojo | constant (contrato) | Hacia dónde mira |
| linea_tipo | int | arquero 0, soldado 1, escudero 2 | constant (contrato) | Línea del tipo, de atrás hacia adelante |
| i | int | 0–3 | calculated | Posición dentro de la línea de 4 |

**Output Range:** columnas 5–7 (azul) y 16–18 (rojo); filas 5–8. La fila más baja es `cf − 2` = 5 y la más alta `cf + 1` = 8, ambas dentro de 0–14. Solo vale con los centros de esta versión: con otros habría que revalidar.
**Example:** escudero rojo con `i = 3`: columna = 21 − (2 + 1 + 2) = 16, fila = 7 − 2 + 3 = 8, **(16, 8)**. Arquero azul con `i = 0`: columna = 2 + 3 = 5, fila 5, **(5, 5)**.

The `distancia_frente` formula is defined as:

`distancia_frente = (cc_rojo − cc_azul) − 2 × (RADIO_MURO + 1 + linea_max)`

| Variable | Type | Range | Source | Description |
|----------|------|-------|--------|-------------|
| cc_azul, cc_rojo | int | 2 y 21 | constant (contrato) | Columna del centro de cada castillo |
| RADIO_MURO | int | 2 | constant (contrato) | Distancia del centro al muro |
| linea_max | int | 2 | constant (contrato) | Línea más adelantada del ejército (escuderos) |

**Output Range:** 19 − 2 × 5 = **9** (8 columnas libres entre las líneas de escuderos). **Invariante:** `distancia_frente ≥ 2`; con 1 los ejércitos empiezan pegados y con menos se superponen.
**Example:** azul en la columna 7, rojo en la 16: 16 − 7 = 9.

The `turnos_hasta_contacto` formula is defined as:

`turnos_hasta_contacto = ceil((distancia_frente − 1) / m)`

| Variable | Type | Range | Source | Description |
|----------|------|-------|--------|-------------|
| m | int | ≥ 1 | data file (Catálogo, `movimiento`; F2 exige ≥ 1) | Movimiento de las piezas de frente, por turno de un jugador |

**Output Range:** 1 a 8 con `m` entre 1 y 8; no hay división por cero porque el Catálogo exige `m ≥ 1`. Es una **cota inferior** (un oráculo para los tests de balance, no una predicción: ignora bloqueos y supone que quien mueve avanza en línea recta).
**Example:** `m` = 4 (escuderos): ceil(8 / 4) = **2** (el contacto se vuelve posible en el turno 2, el de rojo). Con Grito de guerra (`m` = 6): ceil(8 / 6) = 2. Con la catapulta (`m` = 2): 4.

The `techo_movimiento` formula is defined as:

`techo_movimiento = d_anillo_muro − 1`, con `d_anillo_muro = (cc_rojo − RADIO_MURO − 1) − (cc_azul + RADIO_MURO + 1)`.

| Variable | Type | Range | Source | Description |
|----------|------|-------|--------|-------------|
| cc_azul, cc_rojo | int | 2 y 21 | constant (contrato) | Columna del centro de cada castillo |
| RADIO_MURO | int | 2 | constant (contrato) | Distancia del centro al muro |
| d_anillo_muro | int | 13 | calculated | Pasos desde el anillo de un castillo hasta quedar pegado al muro del otro |

**Output Range:** d_anillo_muro = 18 − 5 = 13, **techo = 12**. Responde la pendiente del Catálogo ("una pieza llega al castillo rival el mismo turno en que aparece"): una pieza reclutada en el anillo azul necesita 13 pasos para quedar pegada al muro rojo, así que con `movimiento` efectivo ≤ 12 no llega ese mismo turno. Con los valores vigentes el jinete tiene 6 y 8 con Grito de guerra: queda con margen. Supone el carril libre; con piezas en el camino el techo real es menor.
**Example:** jinete reclutado en `(5, 6)` con 8 de movimiento llega a la columna 13, a 5 columnas del muro rojo.

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

- **If la config trae un par de castillos que no es (4, 6)** (otro par, o el mismo par invertido (6, 4)): `ConfigInvalida`; no se crea la partida. El invertido también se rechaza porque azul va siempre a la izquierda.
- **If el Catálogo permite menos de 4 soldados, arqueros o escuderos, o un Rey distinto de 1**: `ConfigInvalida`; no se crea tablero (regla 6).
- **If Castillo y muros (#5) entrega una vida inicial de tramo menor o igual a 0 o fuera de la tabla de dominios**: `ConfigInvalida` (`vida_tramo` de 1 a 1.000; ciclo con #5 cerrado el 2026-11-25).
- **If un estado cargado o una repetición declara otras dimensiones de tablero o otro radio de muro**: `VersionNoSoportada`. Las dimensiones son constantes de la versión de reglas.
- **If se cambia una constante del Tablero** (dimensiones, radio del muro, centros, líneas del ejército o composición inicial) **sin subir la versión de reglas**: falla el test del despliegue de referencia (las 26 piezas y los 32 tramos en sus casillas, con sus IDs). Es el mismo mecanismo que el Catálogo usa con su huella.

**Consultas y geometría**

- **If una consulta recibe una casilla fuera de la grilla** (coordenada negativa, columna 24 o fila 15): `dentro` devuelve falso, `ocupante` responde "ninguno" y **nunca se calcula un índice** (regla común de Formulas). Si llegó por un comando de jugador, la Librería lo rechazó antes con su código.
- **If se pide el anillo de un castillo pegado al borde lateral**: se excluyen las casillas fuera de la grilla. El castillo azul tiene 17 casillas de anillo, no 24, y no tiene anillo a su izquierda. El anillo es geometría pura: incluye casillas ocupadas; quien lo usa (Reclutamiento, #10) filtra las libres.
- **If un tramo de muro se rompe**: la clasificación de la casilla (muro, interior, anillo) **no cambia**, porque es geometría fija. El tablero la sigue reconociendo como casilla de muro; `hayTramo` pasa a falso. Quien decide quién puede pasar por el hueco es Castillo y muros (#5).
- **If un arquero está sobre un tramo y el tramo se rompe**: el arquero queda en esa casilla, que pasa a ser "con pieza". El Tablero no mueve a nadie.
- **If `ladoDelRival` recibe un jugador distinto de 0 o 1**: es un error de programación, `ErrorInterno` de la Librería; nunca llega por un comando de jugador.

**Estado cargado**

- **If un estado cargado tiene dos piezas en una casilla** (salvo un arquero sobre un tramo propio), **una pieza fuera de la grilla, o un tramo fuera del perímetro de su castillo**: `EstadoInvalido` (los invariantes de las reglas 1 a 3).

**Estrategia degenerada**

- **If nadie cruza las 8 columnas libres** (8 a 15): no es un error del Tablero y no se resuelve acá. Termina por el reloj de 12:30 por jugador (Máquina de turno, regla 14): pierde quien lo agota primero. En el modo Sin límite no hay reloj y la partida no termina sola. *PROVISIONAL: es la hipótesis que acordamos medir; se vigila con las Métricas y se lleva a Open Questions.*

## Dependencies

**Depende de:**

| Sistema | Tipo | Qué usa |
|---|---|---|
| #1 Librería de reglas | Dura | `CrearPartida` (config con castillos fijos), IDs estables (regla 7), códigos de error (`ConfigInvalida`, `VersionNoSoportada`, `EstadoInvalido`) |
| #2 Catálogo de piezas | Dura | Tipos 0 a 6 y su orden, `topeVivas` (valida el ejército 4/4/4/1) y `movimiento` (solo en los oráculos de Formulas) |
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
| #11 Modificador | Blanda | Margen del golpe del turno 1: cada punto del bono de movimiento del Grito lo achica |
| #15, #16, #18 Cliente | Blanda | Dimensiones, casillas y despliegue (solo lectura) |
| #19 Métricas | Blanda | Constantes de la partida (el par de sectores es fijo) |
| #26 Registro | Blanda | La config con los castillos fijos |

**Bidireccionalidad: estado de los documentos vecinos** (al 2026-11-25):

- **Librería:** hecho el 2026-11-25: se retiró el sorteo (fila del #4, Tuning Knobs, `opcion_por_sorteo`, AC-16, AC-36, AC-46, AC-54). La marca PROVISIONAL del tablero 24 × 15 también se levantó.
- **Máquina de turno:** hecho el 2026-11-25: se quitaron las filas y los casos del sorteo (AC-3 incluido). Ya nombra a #4 y cerró su Open Question de quién empieza.
- **Catálogo:** hecho el 2026-11-25: sin PROVISIONAL que dependan de #4 y con el techo de `movimiento` en 12 (`techo_movimiento`).
- **systems-index:** hecho: fila, texto del #4 y arista de datos hacia #5.

## Tuning Knobs

**El Tablero no tiene perillas de balance propias.** Toda variable `data file` de Formulas es una perilla, y la única es `m` (el `movimiento` del Catálogo). Lo que un ajuste de balance tocaría vive en otro GDD:

| Valor | Dónde se ajusta | Nota |
|---|---|---|
| `movimiento` de cada tipo | Catálogo (#2), archivo de datos | Entra en `turnos_hasta_contacto` y `techo_movimiento` |
| `topeVivas` | Catálogo (#2) | Tiene que cubrir la composición inicial |
| Vida inicial del tramo | Castillo y muros (#5), llega por la config | Hoy 30 |
| +2 de movimiento del Grito de guerra | Modificador (#11) | Cambia `m` durante un turno |

**Dónde viven las constantes.** Son **constantes con nombre, en una sola definición, versionadas** (decisión del 2026-11-24, igual que las constantes del contrato de la Librería): nunca hay un número suelto en el código (`24`, `15`, `2`). Cambiar una es tocar esa definición, subir la versión de reglas y actualizar el test del despliegue de referencia. No son perillas de un archivo de datos, aunque funcionan como palancas de diseño para el pilar 4.

**Constantes del contrato que funcionan como palancas de diseño:**

| Constante | Vigente | Rango seguro | Si es demasiado alto | Si es demasiado bajo |
|---|---|---|---|---|
| `COLUMNAS` (con el margen de 2, distancia entre centros = `COLUMNAS − 5`) | 24 | 22 a 24: contacto en el turno 2 y nadie golpea en el turno 1 (v5); piso duro 17 (`distancia_frente ≥ 2`) | 25 o más: contacto en el turno 3 o después, partida más lenta (pilar 4); se pierde el 16:9 (24 × 15 = 1,6) | 21 o menos: arqueros y escuderos golpean en el turno 1; 20 o menos: contacto en el turno 1. Ventaja de golpe para quien mueve primero (sin medir) |
| `FILAS` | 15 | Piso duro 5 (entra el castillo); piso útil sin medir, 7 para dejar una fila de flanqueo a cada lado | Tablero más ralo y más rodeos, sin medir | Sin flanqueo: se pierden opciones de planificar (pilar 1) |
| Margen del castillo al borde | 2 | 2 a 3 sin golpe en el turno 1 (`distancia_frente ≥ 7`); hasta 5 es válido (cada +1 baja la distancia entre centros en 2) | Contacto antes y espalda libre para rodear el castillo | Piso duro 2: el bloque de 5 × 5 tiene que caber |
| `RADIO_MURO` | 2 | 1 a 3 sin golpe en el turno 1 (`distancia_frente ≥ 7`); hasta 5 es válido (con 6 la distancia de frente queda en menos de 2) | Más tramos (`8 × radio`) y menos distancia de frente | Con 1 el castillo es de 3 × 3: 8 tramos y solo el Rey adentro |
| Composición inicial (soldados / arqueros / escuderos) | 4 / 4 / 4 | De 1 hasta el `topeVivas` de cada tipo | Ejército más ancho y partida más larga | Pocas piezas para combinar (pilar 2) |

**Relación entre `COLUMNAS` y el turno de contacto** (con `movimiento` 4, margen 2 y `RADIO_MURO` 2):

| `COLUMNAS` | Distancia entre centros | `distancia_frente` | Contacto posible en el turno |
|---|---|---|---|
| 22 a 24 | 17 a 19 | 7 a 9 | **2** (24 es lo probado en los prototipos) |
| 21 | 16 | 6 | **2**, pero arqueros y escuderos golpean en el turno 1 |
| 17 a 20 | 12 a 15 | 2 a 5 | **1** (azul golpea antes de que rojo mueva) |
| 25 o más | 20 o más | 10 o más | 3 o más |

**Perillas que interactúan:**

- `COLUMNAS`, el margen, `RADIO_MURO` y el `movimiento` del Catálogo se miran juntos: los tres primeros fijan la `distancia_frente` y el último decide en qué turno llega el contacto.
- **Soldados iniciales y `piezas_para_matar` del Catálogo:** `ceil((escudo + vida del Rey) / ataque del soldado) = ceil(35 / 8) = 5`. Con 4 soldados un grupo de solo soldados no puede matar al Rey con un golpe combinado; con 5 o más sí (el Rey y el ataque del soldado no cambiaron en la v5). Subir los soldados iniciales exige revisar eso con Resolución (#9).
- **Composición y `topeVivas`:** tienen que cumplir `composición ≤ topeVivas` (si no, `ConfigInvalida`).

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

No aplica: el Tablero no responde a la entrada del jugador. La sensación de seleccionar y mover es de Entrada (#13) y Movimiento (#6). Lo único que aporta es que sus consultas son baratas (AC-29, 1 ms).

## UI Requirements

Sin pantalla propia: el tablero ocupa toda la pantalla (decisión del jugador, 2026-11-17; art bible §7).

- **Medidas:** casillas de 68 px a 1080p; 24 × 68 = 1632 px de ancho y 15 × 68 = 1020 px de alto, con 60 px arriba y 144 px a cada lado para el HUD. Mínimo 1080p.
- **Las coordenadas son internas:** el jugador no ve "columna 5, fila 7" (salvo en modo de depuración).
- **Contar distancias sin cuadrícula dibujada** (decisión del 2026-11-25): por defecto no hay cuadrícula, como pide el art bible. Al elegir una pieza se marcan las casillas alcanzables (Movimiento, #6) y el alcance (Alcance, #7). Una opción **"Mostrar cuadrícula"**, apagada por defecto, queda en Accesibilidad (#23) y Ajustes (#20).
- **Lo que consume:** Ficha y HUD (#15) y Vista previa (#16) leen las dimensiones; Render (#18) pinta el terreno y el castillo.
- **Castillos y anillos despejados:** ningún recuadro de la Interfaz del Modificador (#17, F5) tapa las columnas 0 a 5 ni 18 a 23 (castillos y anillos de reclutamiento); el HUD (#15) no tapa ninguna casilla salvo las leyendas centrales de ≤ 1 s.

> **📌 UX Flag — Tablero y despliegue inicial**: This system has UI requirements. In Phase 4 (Pre-Production), run `/ux-design` to create a UX spec for each screen or HUD element this system contributes to **before** writing epics. Stories that reference UI should cite `design/ux/[screen].md`, not the GDD directly.

## Cross-References

| Documento | Qué se referencia | Cómo |
|---|---|---|
| `design/gdd/libreria-de-reglas.md` | Regla 2 (`CrearPartida`, castillos fijos), regla 7 (IDs), tabla de dominios (tablero 24 × 15), códigos `ConfigInvalida`, `VersionNoSoportada`, `EstadoInvalido` y `ErrorInterno` | Este GDD adopta el tope 24 × 15 como constante propia; la Librería retiró el sorteo (2026-11-25) y levantó la marca PROVISIONAL del tablero |
| `design/gdd/catalogo-de-piezas.md` | Tipos 0 a 6 y su orden, `topeVivas`, `movimiento`, `piezas_para_matar`, pendientes PROVISIONAL (AC-2, AC-30) | Este GDD cierra los provisionales y calcula el techo de `movimiento` (12) |
| `design/gdd/maquina-de-turno.md` | Estados de creación, AC-3, Open Question de quién empieza | Sorteo retirado de la tabla de estados y de AC-3 (2026-11-25); pregunta cerrada (jugador 0, sin compensación) y #4 en su Dependencies |
| `design/gdd/game-concept.md` | "Reglas vigentes" (tablero, ejército, castillo, Rey), pilares 1, 3 y 4, anti-pilares, línea 348 (Procedural Systems) | Línea 348 corregida el 2026-11-25 (castillos en posición fija, sin sorteo) |
| `design/art/art-bible.md` | §3 (terreno sin cuadrícula, castillo y muros), §4 (colores de bando), §6 (densidad y borde), §7 (pantalla, 68 px) | Fuente de los requisitos visuales; §7 menciona castillos en esquinas |
| `design/gdd/systems-index.md` | Sistema #4, su texto ("sectores al azar") y las aristas | Texto del #4 corregido y arista de datos #4 → #5 anotada (2026-11-25) |
| `design/registry/entities.yaml` | `tablero_columnas` 24, `tablero_filas` 15, `radio_muro` 2, `distancia_frente`, `turnos_hasta_contacto`, `techo_movimiento`; `opcion_por_sorteo` (ahora deprecated) | Registradas el 2026-11-25 |
| `prototypes/rolchess-v4-concept/` | Despliegue y sectores (`RolchessV4.cs:262-320`), README de partidas | Fuente del despliegue; el código no se migra |

## Acceptance Criteria

Todos son automatizables en Edit Mode (lógica pura, sin pantalla); el dibujo del tablero es de Render (#18) y tendrá sus propias capturas. Tipos: **Logic** (prueba unitaria) e **Integration** (con otro sistema).

**Reglas (Core Rules)**

- **AC-1 (regla 1, grilla)** — **GIVEN** un tablero recién creado, **WHEN** se consulta `dentro` para `(0,0)`, `(23,14)`, `(24,0)`, `(0,15)` y `(−1,0)`, **THEN** devuelve verdadero, verdadero, falso, falso y falso, y el tablero tiene 360 casillas. *(Logic)*
- **AC-2 (regla 2, vecindades)** — **GIVEN** el tablero vacío, **WHEN** se piden las vecindades, **THEN** la cruz de `(0,0)` tiene 2 casillas y la de `(5,5)` tiene 4; el contorno de `(0,0)` tiene 3 y el de `(5,5)` tiene 8; ninguna casilla fuera de la grilla aparece como vecina. *(Logic)*
- **AC-3 (regla 3, un ocupante)** — **GIVEN** el despliegue inicial, **WHEN** se recorren las 360 casillas, **THEN** ninguna tiene más de una pieza, y las 26 piezas ocupan 26 casillas distintas. *(Logic)*
- **AC-4 (regla 4, despliegue fijo)** — **GIVEN** una config con castillos en el par (4, 6), **WHEN** se llama `CrearPartida`, **THEN** la partida nace En curso. **Y GIVEN** el par (6, 4) o el par (1, 9), **THEN** `ConfigInvalida` y no se crea partida. *(Logic)*
- **AC-5 (regla 5, castillos)** — **GIVEN** el despliegue inicial, **WHEN** se cuentan las casillas de cada castillo, **THEN** el azul tiene 16 tramos en las filas 5 y 9 (columnas 0 a 4) y en las columnas 0 y 4 (filas 6 a 8), 9 casillas de interior y el Rey en `(2,7)`; el rojo tiene lo mismo en las columnas 19 a 23 con el Rey en `(21,7)`. *(Logic)*
- **AC-6 (regla 6, ejército)** — **GIVEN** el Catálogo vigente, **WHEN** se crea la partida, **THEN** cada jugador tiene 4 soldados, 4 arqueros, 4 escuderos y 1 Rey (13 piezas). **Y GIVEN** el Catálogo con `topeVivas` de soldados en 3 (o el Rey en 2), **THEN** `ConfigInvalida`. *(Integration con Catálogo)*
- **AC-7 (regla 7, posiciones)** — **GIVEN** el despliegue inicial, **WHEN** se compara con el despliegue de referencia guardado en el repositorio, **THEN** las 26 piezas y los 32 tramos están exactamente en las casillas de la tabla de la regla 7. *(Logic)*
- **AC-8 (regla 8, IDs)** — **GIVEN** el despliegue inicial, **WHEN** se leen los IDs, **THEN** azul tiene soldados 1 a 4, arqueros 5 a 8, escuderos 9 a 12 y Rey 13; rojo tiene 14 a 17, 18 a 21, 22 a 25 y Rey 26; y el próximo ID es 27. *(Integration con Librería)*
- **AC-9 (regla 9, simetría)** — **GIVEN** el despliegue inicial, **WHEN** se refleja cada pieza y cada tramo con `espejo`, **THEN** cae sobre una pieza (o tramo) del otro jugador del mismo tipo. *(Logic)*
- **AC-10 (regla 10, quién empieza)** — **GIVEN** una partida recién creada, **WHEN** se lee el estado, **THEN** es el turno 1, del jugador 0, en la fase Mover, sin tirada pendiente ni evento de reclutar. *(Integration con Máquina de turno)*
- **AC-11 (regla 11, consultas)** — **GIVEN** el despliegue inicial, **THEN** `ocupante(7,5)` es el ID 9, `ocupante(2,7)` el 13 y `ocupante(21,7)` el 26; `hayTramo(0,5)` es verdadero y `hayTramo(2,7)` falso; `ladoDelRival(0)` es +1 y `ladoDelRival(1)` es −1; y el anillo de cada castillo tiene 17 casillas. *(Logic)*
- **AC-12 (regla 12, jugador no puede)** — **GIVEN** la lista de comandos públicos de la Librería, **WHEN** se revisan sus parámetros, **THEN** ninguno permite elegir lado, mover un castillo ni colocar piezas del despliegue. *(Logic, revisión de API)*

**Fórmulas**

- **AC-13 (`indice_casilla`)** — **GIVEN** el tablero, **THEN** `(0,0)` da 0, `(23,14)` da 359 y `(21,7)` da 189, y la inversa de 189 es `(21,7)`; **y** una consulta con `(24,5)` da "fuera de la grilla" y **no** devuelve lo que hay en `(0,6)` (un tramo): `hayTramo(24,5)` es falso. *(Logic, límite)*
- **AC-14 (`distancia_cruz`)** — **THEN** la distancia de una casilla a sí misma es 0, de `(0,0)` a `(23,14)` es 37 y de `(7,5)` a `(16,8)` es 12. *(Logic, límite)*
- **AC-15 (`casilla_de_castillo`)** — **GIVEN** el castillo azul, **THEN** `(4,5)` es muro, `(5,6)` es anillo, `(3,6)` y `(2,7)` son interior, y el castillo tiene 16 casillas de muro y 9 de interior. *(Logic)*
- **AC-16 (`casilla_inicial`)** — **THEN** el escudero rojo con `i = 3` está en `(16,8)`, el arquero azul con `i = 0` en `(5,5)`, y las 24 piezas que no son el Rey coinciden con la tabla de la regla 7 (con `i` en 0 y en 3 para los dos jugadores). *(Logic, límite)*
- **AC-17 (`distancia_frente`)** — **THEN** con las constantes vigentes da 9. **Y GIVEN** la función del invariante, que recibe las constantes como parámetros, **WHEN** se le pasa `RADIO_MURO` = 6 (o `COLUMNAS` = 16), **THEN** falla (`distancia_frente` menor que 2); con `COLUMNAS` = 17 pasa. *(Logic, límite)*
- **AC-18 (`turnos_hasta_contacto`)** — **THEN** con `distancia_frente` 9: `m` = 4 da 2, `m` = 6 da 2, `m` = 2 da 4, `m` = 1 da 8 y `m` = 8 da 1. *(Logic, límite)*
- **AC-19 (`techo_movimiento`)** — **THEN** con las constantes vigentes da 12; **y GIVEN** el Catálogo vigente, el `movimiento` efectivo más alto (jinete 6 + Grito de guerra 2 = 8) es menor que 12. *(Integration con Catálogo)*
- **AC-20 (`espejo`)** — **THEN** `espejo(2,7)` da `(21,7)` y `espejo(5,5)` da `(18,5)`, y aplicarlo dos veces devuelve la casilla original para las 360 casillas. *(Logic)*

**Casos de Edge Cases que podrían salir rotos**

- **AC-21 (casilla fuera de la grilla)** — **GIVEN** el despliegue inicial, **WHEN** se llama `ocupante` con `(−1,0)`, `(24,3)` y `(0,15)`, **THEN** responde "ninguno" sin excepción y sin leer el arreglo. *(Logic)*
- **AC-22 (anillo recortado)** — **GIVEN** cada castillo, **THEN** su anillo no incluye ninguna casilla fuera de la grilla: tiene 17 casillas, no 24. *(Logic)*
- **AC-23 (tramo roto)** — **GIVEN** el despliegue con el tramo `(0,5)` destruido, **THEN** la casilla sigue clasificada como muro, `hayTramo(0,5)` es falso y `ocupante` no cambia. **Y GIVEN** un arquero sobre un tramo que se rompe, **THEN** queda en su casilla. *(Logic; el segundo, Integration con #5)*
- **AC-24 (estado cargado inválido)** — **GIVEN** un estado con dos piezas en `(7,5)`, o una pieza en `(24,3)`, o un tramo en `(10,10)`, **WHEN** se carga, **THEN** `EstadoInvalido`. *(Logic)*
- **AC-25 (versión)** — **GIVEN** un estado que declara un tablero de 25 × 15, **WHEN** se carga, **THEN** `VersionNoSoportada`. *(Logic)*
- **AC-26 (constante sin versión)** — **GIVEN** `RADIO_MURO` cambiado a 3 sin subir la versión de reglas, **WHEN** corre el test del despliegue de referencia (AC-7), **THEN** falla; al restaurar el valor, pasa. *(Logic; se rompe a propósito para comprobar que el test falla)*
- **AC-27 (vida de tramo inválida)** — **GIVEN** una config con `vida_tramo` 0 o 1.001, **WHEN** se crea la partida, **THEN** `ConfigInvalida`. *(Integration con #5)*
- **AC-30 (`ladoDelRival` inválido)** — **GIVEN** el tablero, **WHEN** se llama `ladoDelRival(2)` o `ladoDelRival(−1)`, **THEN** `ErrorInterno`. *(Logic)*

**Cruce con otros sistemas y rendimiento**

- **AC-28 (determinismo)** — **GIVEN** la misma config, **WHEN** se llama `CrearPartida` dos veces, **THEN** los dos estados son idénticos (misma huella). *(Integration con Librería)*
- **AC-29 (rendimiento)** — **GIVEN** la referencia que fije el ADR de la Librería, **WHEN** se mide `CrearPartida` con el despliegue fijo, **THEN** el percentil 95 es menor o igual que 1 ms. *(Performance; ADVISORY, como el AC-25 de la Librería, hasta que el ADR nombre la referencia)*

**Sin criterio, a propósito:**

- *"Nadie más guarda coordenadas del tablero"* (regla 11) es una regla de arquitectura, no un comportamiento: va al ADR y al manual de control, y se verifica con un test de referencias entre assemblies.
- *"Se vigila con las Métricas"* (regla 10) y la *tierra de nadie* (Edge Cases) son preguntas de playtest: van a Open Questions.

## Open Questions

| Pregunta | Dueño | Para cuándo |
|---|---|---|
| Quien se acerca primero recibe el primer golpe: con los valores v5 azul amenaza hasta la columna 13 y rojo desde la 10, así que 8 y 9 son seguras para azul, 14 y 15 para rojo, y 10 a 13 están en disputa. ¿Mover primero o segundo da ventaja? Sin medir: el análisis de las 11 partidas v4 señala la ventaja de abrir reclutando (azul lo hizo en 7 y ganó 5), que la v5 quita al no tirar el Modificador en el turno 1 La medida existe (Métricas, #19, regla 14). | Métricas (#19), playtest | MVP; medir quién da el primer daño y si esa partida la gana |
| Margen de 3 casillas contra el golpe del turno 1 (v5; con la v4 era 1): el arquero está a 11 de la línea enemiga y amenaza 4 + 4 = 8; el escudero está a 9 y amenaza 4 + 2 = 6. Con Grito de guerra queda en 1. El arquero que se queda en su muro (+2 de alcance) llega solo a la columna 10 | Catálogo (#2), Modificador (#11) | Al revisar el Catálogo y en el GDD del Modificador |
| Si nadie cruza las 8 columnas libres, decide el reloj de 12:30 por jugador, y en Sin límite la partida no termina sola. ¿Es aceptable? | Máquina de turno (#3), playtest | MVP |
| Azul siempre a la izquierda y siempre el primero: no se separa el efecto de lado del de turno. ¿La revancha alterna lados? La medida existe (Métricas, #19, regla 14). | Menú (#21), Métricas (#19) | Fase 1 |
| ~~Esquema de config para la vida inicial del tramo (arista de datos #4 → #5)~~ **Resuelta el 2026-11-25:** campo `vida_tramo` en la config (1 a 1.000), validado por la Librería (Castillo y muros, regla 14) | Castillo y muros (#5) | Cerrada |
| Pisos útiles de `FILAS` (7) y de la composición inicial: sin medir La medida existe (Métricas, #19, regla 14). | Métricas (#19), playtest | Compuerta de duración |
| ¿El jugador percibe el despliegue como parejo? No automatizable | Playtest | MVP |
| Sin cuadrícula dibujada, ¿se cumple "contar distancias" (pilar 1)? Depende del resaltado de Movimiento (#6) y Alcance (#7) y de la opción de Accesibilidad (#23) | #6, #7, #23, playtest | En sus GDD y en el MVP |
