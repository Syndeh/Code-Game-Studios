# Movimiento

> **Status**: Designed (pendiente de `/design-review` en una sesión nueva)
> **Author**: Martin Arin + Claude
> **Last Updated**: 2026-11-25
> **Last Verified**: 2026-11-25
> **Implements Pillar**: Pilar 1 (Planificar y cosechar), Pilar 4 (Duelo corto); sostiene el Pilar 2 (la formación que permite combinar)
> **Creative Director Review (CD-GDD-ALIGN)**: CONCERNS (accepted) 2026-11-25 — autoevaluación en la sesión de autoría; el jugador los aceptó como riesgos a medir en el MVP: (1) Formulas F6: 3,1 s por pieza en el tramo final puede pelear con el pilar 1 en táctil; (2) Edge Cases: una muralla de 15 piezas puede alargar la partida (pilar 4); (3) reglas 8 y 10: el Rey sin cupo puede premiar a una pieza sola (pilar 2)

## Summary

Movimiento define cómo se mueven las piezas en la fase `Mover`: en cruz, a un punto por casilla, hasta 12 piezas más el Rey, atravesando aliados y no enemigos ni muros (con excepciones para jinete, Rey y arquero). Para el jugador es donde se arma el plan del turno, probando y deshaciendo sin costo. Existe para que las piezas lleguen juntas al objetivo dentro del reloj.

> **Quick reference** — Layer: `Feature` · Priority: `MVP` · Key deps: `Librería de reglas (#1), Catálogo de piezas (#2), Máquina de turno (#3), Tablero (#4), Castillo y muros (#5)`

## Overview

Movimiento es el conjunto de reglas de la fase `Mover`: decide a qué casillas puede ir cada pieza propia en su turno y cuánto le cuesta llegar. En cada turno el jugador mueve hasta 12 piezas más el Rey (con la Arremetida, sin tope de piezas), en cruz sobre la grilla de 24 × 15 del Tablero, a un punto de `movimiento` por casilla. Las piezas atraviesan a sus aliados pero no a los enemigos (el jinete pasa por encima de ellos sin poder terminar encima), y ninguna cruza un tramo de muro en pie, salvo el Rey y los arqueros del dueño del castillo. Una pieza puede recibir varios `Mover` en el mismo turno mientras le queden puntos; el jugador solo elige el destino y el sistema cobra el camino permitido más corto. Todo lo movido se puede deshacer, del último al primero (la pila es de la Máquina de turno). Este sistema es donde se arma la estrategia (pilar 1): el jugador acomoda el ejército para que el ataque del turno combine (pilar 2), y lo hace con un gesto por pieza para que 13 movimientos entren en el reloj (pilar 4). Sin Movimiento no habría forma de acercar un plan al rival: la Máquina abriría una fase sin reglas, el Tablero tendría casillas que nadie recorre, y el Castillo no tendría quién suba al muro ni quién pase por un hueco.

**Cómo lo vive el jugador.** Es activo y constante: casi todos los turnos empiezan por acá. Elige una pieza, el tablero marca adonde llega, elige el destino y la pieza se desplaza. No hay azar en este sistema; el único azar del turno (el Modificador) ya se resolvió antes de empezar a mover.

**Qué no es.** Movimiento no guarda coordenadas ni decide qué casillas existen (Tablero, #4), no define quién bloquea en el muro ni las reglas del arquero sobre el tramo (Castillo y muros, #5, reglas 3 y 6), no es dueño de la pila de deshacer, de las fases ni del timeout (Máquina de turno, #3), no calcula bonos como el Grito de guerra (Modificador, #11, que los escribe en el estado) y no decide dónde aparece una pieza reclutada (Reclutamiento, #10). Aplica lo que esos sistemas definen y es dueño de tres cosas: el paso y su costo, el límite de piezas por turno (`limite_normal`) y la consulta de qué destinos son legales.

**Decisiones de esta sesión (2026-11-25).** El movimiento es en cruz, no en las 8 direcciones (cierra la Open Question del concepto, que había quedado como supuesto del prototipo), y las piezas reclutadas pueden moverse en el turno en que aparecen (cierra la pregunta que la Máquina de turno había dejado para #6 y #10).

→ Cómo se representa la búsqueda del camino y sus buffers sin reservar memoria por comando es un tema de arquitectura, no de este GDD (`/create-architecture` y el ADR de la Librería).

## Player Fantasy

**El tablero responde antes de que muevas.** Tocás una pieza y el campo te contesta: se marcan todas las casillas a las que llega, y vos ya sabés cuál de ellas arma el plan. Mover no es una apuesta ni un trámite; es pensar con las manos. Mandás los escuderos al frente, los arqueros avanzan detrás sin estorbarse, el Rey se corre por el costado, y todo eso se puede probar, mirar y deshacer sin costo hasta que el plan queda como lo querías.

**Momento ancla:** el turno en que los dos ejércitos ya se tocan. El jugador selecciona un escudero, ve de un vistazo que llega a la casilla que cubre a su arquero, lo manda ahí, ve que otro soldado queda a un paso del objetivo y cierra el movimiento sabiendo exactamente qué va a pasar cuando ataque.

**Pilar que sirve:** Pilar 1, *"Al mover armás la estrategia; al atacar ejecutás el plan y ves sus frutos."* Movimiento es la mitad del pilar que se juega con las manos: la otra mitad, la cosecha, es la Resolución. Sostiene además el Pilar 2 (la formación es lo que permite que varias piezas lleguen juntas al mismo objetivo) y el Pilar 4 (un gesto por pieza: 13 movimientos entran en el timer).

**Qué le exige al resto del diseño:**

- **Alcance legible:** al elegir una pieza se marcan las casillas legales (Entrada #13, Render #18). Si el jugador tiene que contar a mano, no hay planificación (anti-pilar de información oculta).
- **Deshacer sin costo ni confirmación:** el plan se arma probando. Es el contrapeso de que el límite de piezas y los puntos se gasten de verdad.
- **Rechazos que dicen la causa:** "ya moviste 12 piezas", "el muro bloquea", "casilla ocupada" (Librería, regla 4: si un movimiento no vale, el juego dice por qué).
- **Que se note quién atraviesa a quién:** aliados se cruzan, enemigos no, el jinete salta. Es la regla que hace que una fila de escuderos se sienta como una muralla y no como un adorno.

**Lo que este encuadre no promete:** que mover sea siempre rápido en pantalla táctil. 13 gestos en 40 a 60 segundos son entre 3 y 4,6 s por pieza (Formulas, F6); es el riesgo que el director técnico marcó para Entrada y selección (#13) y se mide en el MVP.

## Detailed Design

### Core Rules

1. **Qué es.** Las reglas de la fase `Mover` dentro de `Aplicar` (Librería, regla 3; Máquina, regla 7): el paso y su costo, qué casillas se pueden atravesar y en cuáles se puede terminar, el presupuesto por pieza, el límite de piezas por turno, el efecto de cada `Mover` y las consultas `MovimientosLegales` y `hayMovimientoLegal`. La pila de deshacer, las fases y el timeout son de la Máquina; quién bloquea en el muro, del Castillo; la geometría, del Tablero.
2. **El paso (decisión 2026-11-25).** Un paso va a una de las 4 vecinas en cruz (Tablero, regla 2). No hay diagonales. Cada paso cuesta 1 punto, en cualquier casilla: el terreno es decoración y no cambia costos (los terrenos del cuaderno no existen en esta versión). *Por qué cruz:* Tablero, Castillo (una esquina rota no abre el castillo), `techo_movimiento` = 12 y los movimientos del Catálogo ya están calculados para cruz, y las distancias en cruz se cuentan a ojo (pilar 1).
3. **Un comando, un destino.** `Mover(pieza, destino)` lleva la pieza al destino por el camino permitido más corto. El jugador no elige camino. El costo es la longitud de ese camino (F2).
4. **Presupuesto por pieza.** `restante = movimiento_efectivo − usados` (F1). `movimiento_efectivo` sale de `EffectiveStats` (Catálogo más Grito de guerra); `usados` es del estado y empieza cada turno en 0. Una pieza recibe varios `Mover` mientras `costo ≤ restante`. Lo no gastado se pierde al cerrar el turno.
5. **Atravesar una casilla.**

   | Casilla | Soldado, escudero, catapulta, demoledor | Jinete | Arquero | Rey |
   |---|---|---|---|---|
   | Libre (incluye hueco) | Pasa | Pasa | Pasa | Pasa |
   | Pieza aliada | Pasa | Pasa | Pasa | Pasa |
   | Pieza enemiga | Bloquea | Pasa | Bloquea | Bloquea |
   | Tramo propio en pie, sin arquero | Bloquea | Bloquea | Pasa (regla 7) | Pasa |
   | Tramo propio en pie, con arquero | Bloquea | Bloquea | Bloquea | Pasa |
   | Tramo rival en pie | Bloquea | Bloquea | Bloquea | Bloquea |

6. **Terminar en una casilla.** Solo en una casilla sin pieza y sin tramo en pie. Única excepción: un arquero en un tramo propio en pie sin arquero (Castillo, regla 6). Nunca sobre un aliado (no hay intercambio) ni sobre un enemigo (el jinete tampoco). El Rey nunca termina sobre un tramo. Un tramo roto es una casilla libre.
7. **Arquero y muro propio.** Sube a un tramo propio en pie y libre desde el anillo propio, desde otro tramo propio o desde un hueco vecino; camina de tramo en tramo a 1 punto por paso y baja al anillo o a un hueco. Un paso entre un tramo en pie y una casilla del interior (3 × 3) no existe para el arquero; por un hueco abierto llega al interior como cualquier pieza.
8. **Rey.** Usa el `movimiento` del Rey, no cuenta en el límite de piezas, atraviesa sus propios tramos y los de nadie más, y no sube a los muros. Sin regla de entrar o salir del castillo (2026-11-19).
9. **Jinete.** Pasa por encima de piezas enemigas, el Rey incluido, pagando 1 por casilla; no termina sobre ellas; los tramos en pie lo frenan.
10. **Límite de piezas.** Una pieza que no es el Rey y tiene `usados = 0` solo recibe su primer `Mover` si `piezasMovidas < límite_turno` (F4). Si no, `SinMovimientos`. `límite_turno` es `limite_normal` (12) o "sin límite" tras Arremetida (la Máquina lo guarda, regla 5). Cuenta la pieza, no el comando. Una pieza ya movida sigue moviéndose con el cupo lleno. `piezasMovidas` no se guarda: se calcula desde `usados`, así `Deshacer` la corrige sola. Una pieza que va y vuelve sigue contando hasta que se deshaga.
11. **Reclutas (decisión 2026-11-25).** Una pieza reclutada este turno llega a `Mover` con `usados = 0` y se mueve como cualquier otra, gastando cupo si se mueve. Las del recluta automático no llegan a `Mover` (#10, regla 10). `techo_movimiento` (12) impide que llegue al muro rival ese mismo turno.
12. **Efecto de un `Mover` aceptado.** La pieza pasa al destino, `usados += costo`, se apila (pieza, origen, `usados` anterior) en la pila de la Máquina y sale el evento `PiezaMovida(pieza, origen, destino, costo, camino)`. Un rechazo no cambia nada ni emite eventos.
13. **Códigos de rechazo (fila 14 de la Librería), en este orden:** `DestinoFueraDelTablero` (se valida antes de calcular índices), `DestinoIgualOrigen`, `PiezaAgotada` (`restante = 0`), `SinMovimientos` (cupo lleno y pieza sin mover), `DestinoOcupado` (hay una pieza), `DestinoBloqueado` (tramo en pie en el que esa pieza no puede terminar), `FueraDeAlcance` (ningún camino de costo ≤ `restante`).
14. **Camino canónico.** Búsqueda en anchura con vecinos en orden arriba, izquierda, derecha, abajo; cada casilla guarda a quien la descubrió primero. Solo afecta la animación, nunca la legalidad ni el costo.
15. **Consultas puras.** `MovimientosLegales(pieza)` devuelve (casilla, costo) en orden (fila, columna), vacía si no es el turno o la fase, si la pieza está agotada o si el cupo está lleno y no se movió; usa la misma función que `Mover` (no hay dos cálculos). `hayMovimientoLegal(jugador)` corta en el primer destino y es la que usa la Máquina para saltar la fase (regla 10). También expone `piezasMovidas` y el límite vigente para el HUD.
16. **Cierre del turno.** `usados` vuelve a 0 en todas las piezas cuando la Máquina cierra el turno (su regla 11).
17. **Sin azar, todo a la vista.** Ninguna regla de este sistema usa números aleatorios. `limite_normal` viaja por la config (1 a 37); el resto (cruz, costo 1, tabla de la regla 5, orden de vecinos) son constantes del contrato: cambiarlas sube la versión de reglas.

### States and Transitions

Estado de una pieza durante la fase `Mover` de su dueño (se deriva de `usados`, no se guarda aparte):

| Estado | Condición |
|---|---|
| **Descansada** | `usados = 0` |
| **En marcha** | `0 < usados < movimiento_efectivo` |
| **Agotada** | `usados = movimiento_efectivo` |

| De → a | Disparador | Efecto |
|---|---|---|
| Descansada → En marcha o Agotada | `Mover` aceptado | `usados += costo`; sube `piezasMovidas` (salvo el Rey) |
| En marcha → En marcha o Agotada | `Mover` aceptado | `usados += costo` |
| Cualquiera → estado anterior | `Deshacer` (Máquina) | Vuelve la casilla y el `usados` de la entrada; `piezasMovidas` se recalcula |
| Cualquiera → Descansada | Cierre de turno | `usados = 0` |

**Cupo del turno** (derivado): *Libre* (Arremetida), *Abierto* (`piezasMovidas < límite`) y *Lleno* (`piezasMovidas = límite`; solo se mueven el Rey y las piezas ya movidas). Pasa de Abierto a Lleno con el `Mover` de la pieza número 12 y vuelve con `Deshacer`. Fuera de la fase `Mover` o del turno del dueño ninguna pieza se mueve (Máquina, regla 17).

### Interactions with Other Systems

| Sistema | Qué entra | Qué sale | Dueño de la interfaz |
|---|---|---|---|
| #1 Librería | `Mover` validado hasta la fila 13, `EffectiveStats`, `limite_normal` (config) | Aceptado con eventos o rechazo con código de la regla 13; `MovimientosLegales` | Librería (contrato); #6 (reglas y códigos) |
| #2 Catálogo | `movimiento` por tipo, IDs de arquero (1), jinete (4) y Rey (6) | — | Catálogo |
| #3 Máquina de turno | Fase `Mover` abierta, límite del turno, pila | `hayMovimientoLegal`, una entrada de pila por `Mover`, `usados` a 0 al cerrar | Máquina (fases y pila); #6 (qué es legal) |
| #4 Tablero | `dentro`, `ocupante`, `hayTramo`, cruz, `distanciaCruz`, anillo e interior | Casilla nueva de la pieza | Tablero |
| #5 Castillo y muros | Quién bloquea, huecos, subir y bajar (reglas 3 y 6) | `enMuro` se recalcula solo (consulta de #5) | Castillo |
| #8 Cobertura | — | Nada: las coberturas del jugador se borran al abrir su turno (Máquina, regla 4) | — |
| #9 Resolución | — | Los muros y las piezas vivas no cambian durante `Mover`: el daño se resuelve después | — |
| #10 Reclutamiento | Piezas reclutadas con `usados = 0` | — | Reclutamiento (casilla de aparición) |
| #11 Modificador | Grito (+2 de movimiento) por `EffectiveStats`; Arremetida (sin límite); Distracción (no hay fase) | — | Modificador |
| #13, #15, #18, #25 | — | `MovimientosLegales`, `piezasMovidas`, evento `PiezaMovida` con camino | Cada GDD; la IA usa los mismos comandos que un jugador |

**Provisional:** #11 no tiene GDD. Este documento supone que la tirada da una sola cara por turno, así que una recluta nunca coincide con el Grito de guerra, y que el Grito (+2) vale para todas las piezas, Rey incluido (concepto: "a todas tus piezas").

**Factibilidad:** `docs/engine-reference/unity/modules/core.md` (C# 9, .NET Standard 2.1, núcleo sin referencias al motor) no trae obstáculos: enteros y una búsqueda acotada a 360 casillas. El rendimiento en WebGL no se midió; lo cubre AC-25.

## Formulas

Todo en enteros. F1 a F4 corren en el núcleo; F5 y F6 son oráculos de diseño y de tests.

### F1. Movimiento restante

The `movimiento_restante` formula is defined as:

`movimiento_restante = max(0, movimiento_efectivo − usados)`

**Variables:**

| Variable | Type | Range | Source | Description |
|----------|------|-------|--------|-------------|
| movimiento_efectivo | int | 1–1.010 (hoy 2–8) | calculated (`EffectiveStats`: Catálogo 1–1.000 + bono 0–10) | Puntos de la pieza en el turno |
| usados | int | 0–movimiento del tipo + 10 | calculated (estado) | Puntos gastados en el turno |

**Output Range:** 0 a 1.010 (hoy 0 a 8). El `max` solo actúa sobre un estado cargado; jugando, `usados` nunca supera a `movimiento_efectivo`.
**Example:** jinete con Grito de guerra: 6 + 2 = 8; gastó 5, le quedan 3.

### F2. Costo de un movimiento

The `costo_mover` formula is defined as:

`costo_mover(P, destino) = cantidad de pasos del camino más corto desde la casilla de P hasta destino`, donde cada casilla intermedia se puede atravesar (reglas 5 y 7) y en `destino` se puede terminar (regla 6). Sin camino no hay costo: `FueraDeAlcance`.

**Variables:**

| Variable | Type | Range | Source | Description |
|----------|------|-------|--------|-------------|
| P | ID de pieza | pieza viva | estado | La pieza que se mueve |
| destino | casilla | 360 casillas | comando | Adonde va |
| costo_paso | int | 1 | constant | Costo de cada paso |

**Output Range:** 1 a 359. Siempre `costo_mover ≥ distanciaCruz(origen, destino)`, y es igual si el camino directo está libre.
**Example:** soldado en `(7, 5)`, enemigo en `(8, 5)`, destino `(9, 5)`: la distancia en cruz es 2, pero el rodeo `(7,4) (8,4) (9,4) (9,5)` cuesta 4. Con 4 puntos llega; con 3, `FueraDeAlcance`. Un jinete paga 2. Una catapulta (2 puntos) no llega.

### F3. Destinos en campo abierto

The `destinos_abiertos` formula is defined as:

`destinos_abiertos = 2 × r × (r + 1)`

**Variables:**

| Variable | Type | Range | Source | Description |
|----------|------|-------|--------|-------------|
| r | int | 0–1.010 (hoy 2–8) | calculated (F1) | Movimiento restante |

**Output Range:** es una cota superior; nunca se ofrecen más de 359 destinos, aunque la fórmula dé más. Es exacta si no hay bordes ni piezas ni tramos a distancia r o menos.
**Example:** r = 0 → 0; r = 2 → 12; r = 4 → 40; r = 6 → 84; r = 8 → 144.

### F4. Cupo de piezas

The `puede_recibir_mover` formula is defined as:

`puede_recibir_mover = esRey(P) ∨ usados(P) > 0 ∨ limite_turno = sin límite ∨ piezasMovidas < limite_turno`
`piezasMovidas = cantidad de piezas del jugador, sin el Rey, con usados > 0`

**Variables:**

| Variable | Type | Range | Source | Description |
|----------|------|-------|--------|-------------|
| limite_turno | int o sin límite | 1–37 | estado (Máquina) | `limite_normal` salvo Arremetida |
| limite_normal | int | 1–37 (hoy 12) | data file (config) | Límite del turno común |
| piezasMovidas | int | 0–60 | calculated | Cupo gastado |

**Output Range:** verdadero o falso. Con `limite_normal` 1 se mueve una sola pieza más el Rey; con 37 solo frena a ejércitos más grandes.
**Example:** 12 piezas movidas y límite 12: la pieza 13 no puede; una de las 12 con puntos, sí; el Rey, sí.

### F5. Entradas de la pila en un turno

The `entradas_pila_turno` formula is defined as:

`entradas_pila_turno ≤ mov_rey + Σ de los N mayores movimiento_efectivo`, con `N = min(limite_turno, piezas_sin_rey)`

Cada `Mover` aceptado cuesta 1 o más, así que una pieza nunca deja más entradas que puntos tiene. Esto **confirma `pila_max`** (Máquina, D4) como cota exacta sin bonos.

**Variables:**

| Variable | Type | Range | Source | Description |
|----------|------|-------|--------|-------------|
| mov_rey | int | 1–1.010 (hoy 6 u 8) | calculated | Movimiento efectivo del Rey |
| N | int | 0–60 | calculated | Piezas que pueden moverse |
| piezas_sin_rey | int | 0–60 | calculated | Piezas vivas sin el Rey |

**Output Range:** 0 a 254 con los valores vigentes (`pila_tope` = 256).
**Example:** Arremetida, ejército lleno y sin bono: 254. Grito (+2) con límite 12: 8 jinetes y 4 demoledores × 8 = 96, más Rey 8 = **104**. Turno de reclutar, límite 12: 12 × 6 + 6 = 78.

**Dónde se rompe:** F8 del Catálogo no cuenta el bono. Con Grito (+2) y `limite_normal` 37 da 96 + 25 × 6 + 8 = 254; con **38 da 260 y pasa de 256**. **Decisión del jugador (2026-11-25):** `limite_normal` válido de 1 a **37**. PROVISIONAL hasta que #11 fije el bono; se revisa si cambia el Catálogo.

### F6. Tiempo por pieza

The `ms_por_pieza` formula is defined as:

`ms_por_pieza = dur_mover_s × 1000 / (limite_normal + 1)` (división entera hacia abajo)

**Variables:**

| Variable | Type | Range | Source | Description |
|----------|------|-------|--------|-------------|
| dur_mover_s | int | 30–90 (hoy 60 o 40) | data file (Timer, #12) | Etapa de mover |
| limite_normal | int | 1–37 (hoy 12) | data file (config) | Más 1 por el Rey |

**Output Range:** 789 a 45.000 ms en los extremos; 4.615 y 3.076 con los valores vigentes. No se aplica al modo Sin límite. Con Arremetida el divisor real es la cantidad de piezas.
**Example:** 60 s → 60.000 / 13 = 4.615 ms por pieza; tramo final de 40 s → 3.076 ms. Se descuenta además lo que tome reclutar, que comparte la etapa.

**Lazos de retroalimentación:** Movimiento no produce recursos. Arremetida favorece al que tiene más piezas, pero eso es de #11.

## Edge Cases

**Destino**

- **If el destino está fuera de la grilla** (también con los extremos de int32): `DestinoFueraDelTablero`, sin calcular ningún índice.
- **If el destino es la casilla de la pieza:** `DestinoIgualOrigen`; no gasta ni apila.
- **If el destino tiene una pieza, propia o rival:** `DestinoOcupado`, también para el jinete y para un arquero hacia un tramo con arquero. No hay intercambio ni empuje.
- **If el destino es un tramo en pie donde la pieza no puede terminar:** `DestinoBloqueado` (un no arquero, el Rey, o cualquiera hacia un tramo rival).
- **If varias causas valen a la vez:** sale la primera de la regla 13.

**Puntos y cupo**

- **If el costo es igual al restante:** se acepta y la pieza queda Agotada.
- **If la pieza está Agotada y el cupo lleno:** `PiezaAgotada`.
- **If una pieza va y vuelve:** las dos idas se pagan y sigue contando en el cupo; solo `Deshacer` las devuelve.
- **If se deshace el único `Mover` de una pieza:** `usados` vuelve a 0 y libera su cupo.
- **If ya se movieron 12 y se mueve el Rey:** se acepta y `piezasMovidas` sigue en 12.
- **If no se puede partir un movimiento** porque la casilla intermedia tiene un aliado: el `Mover` completo sigue siendo válido; solo no se puede terminar ahí.
- **If la pila llega a 256** (inalcanzable con `limite_normal` ≤ 37 y el Catálogo vigente): `LimitePartida` de la Librería.
- **If un estado cargado trae `usados` mayor que `movimiento_efectivo`:** el restante es 0 (F1); la pieza queda Agotada.

**Bloqueos**

- **If una pieza está rodeada de enemigos, tramos o bordes:** `MovimientosLegales` vacía y todo destino libre da `FueraDeAlcance`. Un jinete rodeado de enemigos puede salir.
- **If ninguna pieza, ni el Rey, tiene destino al abrir la fase:** `FaseSaltada` (Máquina, regla 10).
- **If el jinete tiene enemigos en fila:** los pasa pagando 1 por cada uno, si llega a una casilla libre.
- **If hay un arquero propio sobre un tramo:** el Rey lo atraviesa; otro arquero no, y rodea por el anillo si le alcanza.

**Muro**

- **If se rompe solo una esquina:** se puede pisar, pero no abre el interior. **Es intencional** (cierra la pregunta de Castillo): sale del paso en cruz.
- **If un arquero en un tramo quiere ir al interior:** solo por un hueco, pagando el camino completo.
- **If una pieza rival tapa un hueco:** el hueco bloquea mientras siga ahí, salvo para el jinete.
- **If cambia un muro:** nunca pasa durante `Mover`; los muros solo cambian al resolver.

**Turno**

- **If sale Distracción:** no hay fase; `Mover` da `FaseIncorrecta`.
- **If vence el timer de mover:** lo movido queda y la pila se vacía (Máquina, regla 12).
- **If el turno cierra por cualquier vía** (resolver, timeout o saltos): `usados` vuelve a 0.
- **If llega un `Mover` repetido:** `ComandoViejo` (Librería).

**Estrategias a vigilar**

- **If un jugador tapa una columna entera con 15 piezas:** es legal y solo la cruza el jinete; se rompe atacándola. Se mide (Open Questions).
- **If el Rey se usa como pieza de ataque gratis** (no gasta cupo): es legal; el costo es exponerlo. Se mide.

## Dependencies

**Depende de:**

| Sistema | Tipo | Qué usa |
|---|---|---|
| #1 Librería de reglas | Dura | `Mover`, orden de validación, `EffectiveStats`, `limite_normal` en la config, eventos, estado |
| #2 Catálogo de piezas | Dura | `movimiento` y los IDs de arquero, jinete y Rey |
| #3 Máquina de turno | Dura | Fase `Mover`, límite del turno, pila y `Deshacer`, cierre del turno |
| #4 Tablero | Dura | Grilla, cruz, ocupación, `hayTramo`, anillo e interior |
| #5 Castillo y muros | Dura | Quién bloquea, huecos, subir y bajar |

**Dependen de este sistema:**

| Sistema | Tipo | Qué usa |
|---|---|---|
| #1 Librería | Blanda | `limite_normal` (dueño: #6), códigos de la regla 13 |
| #3 Máquina de turno | Dura | `hayMovimientoLegal` para abrir o saltar la fase |
| #10 Reclutamiento | Blanda | La recluta puede moverse el mismo turno |
| #11 Modificador | Blanda | `limite_normal` decide cuándo Arremetida puede salir |
| #13 Entrada y selección | Dura | `MovimientosLegales`, códigos de rechazo, varios `Mover` por pieza |
| #14 Localización | Blanda | Textos de los códigos visibles |
| #15 Ficha y HUD | Blanda | `piezasMovidas`, límite vigente, restante |
| #18 Render | Dura | `PiezaMovida` con su camino |
| #19 Métricas | Blanda | `PiezaMovida` y deshechos |
| #24 Tutorial, #25 Rival IA | Dura | Las mismas reglas y consultas que un jugador |

#7, #8 y #9 no dependen de este sistema: leen del estado dónde quedaron las piezas.

**Cambios en documentos vecinos** (aplicados el 2026-11-25 al cerrar la tanda 1, salvo Entrada, que sigue en diseño):

- **Máquina de turno:** `usados` a 0 en la regla 11; levantar el PROVISIONAL de D4 y del límite normal; cerrar la pregunta de las reclutas; precisar en AC-7 que el `Mover` es de una pieza sin mover.
- **Librería:** códigos de la regla 13 en la fila 14; `limite_normal` de 1 a 37; `usados` a 0 en la matriz de clausura.
- **Castillo y muros:** levantar el PROVISIONAL de #6; aclarar que tampoco se atraviesa un tramo con arquero; cerrar la pregunta de la esquina.
- **Tablero:** exponer la consulta del interior (regla 11) y sumarla a la fila de #6.
- **Catálogo:** #6 ya tiene GDD.
- **Concepto:** cerrar "cruz o 8 direcciones".
- **Entrada y selección:** responder su pregunta: una pieza puede moverse más de una vez por turno.

## Tuning Knobs

| Perilla | Vigente | Rango seguro | Si es demasiado alto | Si es demasiado bajo | Interactúa con |
|---|---|---|---|---|---|
| `limite_normal` (config) | 12 | 8–16 (propuesto, sin medir); válido 1–37 (el máximo que cabe en la pila con Grito +2) | Turnos largos y 3 s o menos por pieza en el tramo final (F6); Arremetida casi no sale; 38 o más se rechaza en la config (F5) | No alcanza para armar un golpe combinado (pilar 2); con menos de 12, el ejército inicial ya no se mueve entero | Arremetida, `pila_tope`, `dur_mover_s` |

Con 12, el límite coincide con las 12 piezas iniciales sin Rey: recién frena cuando hay reclutas.

**Perillas de otros GDD que lo afectan:** `movimiento` por tipo (Catálogo; techo 12 y F8), bono del Grito (#11), `dur_mover_normal_s` y `dur_mover_final_s` (#12), topes de piezas (Catálogo).

**No son perillas** (constantes del contrato): paso en cruz, costo 1 por paso, tabla de la regla 5 y orden de vecinos. Cambiarlas sube la versión de reglas.

## Visual/Audio Requirements

Fuente: `design/art/art-bible.md` (§3 marcas, §4 daltonismo, §5 movimiento, §7 HUD). El sonido es dirección; lo especifica Audio (#22).

| Evento | Visual | Sonido |
|---|---|---|
| Elegir una pieza | Puntos blancos al 60% en cada destino legal | Apoyo leve |
| `PiezaMovida` | Desplazamiento por código por el camino, ≤ 0,25 s en total, con frenado suave; sin ciclo de caminata | Uno por `Mover`, no por casilla |
| Jinete sobre un enemigo | Se dibuja por encima de la pieza que pasa | El mismo |
| Rechazo | La pieza no se mueve; aparece la causa en texto, no modal | Seco y distinto |
| `Deshacer` | Vuelve a su casilla en ≤ 0,25 s | Corto, distinto del de mover |
| Cupo lleno | El contador cambia de forma además de color | Una sola vez |

**Estilo:** nunca flechas para mover (son de ataque); nada solo por color; el terreno no se anima. Principios que aplican: *Principio 1: El plan se lee antes que el detalle* y *Principio 3: El ritmo del turno manda sobre la animación*.

> 📌 **Asset Spec** — Visual/Audio requirements are defined. After the art bible is approved, run `/asset-spec system:movimiento` to produce per-asset visual descriptions, dimensions, and generation prompts from this section.

## Game Feel

- **Respuesta inmediata:** `Mover` cumple `comando_presupuesto_ms` (1 ms, p95) y los puntos aparecen dentro de un cuadro de 60 fps (16,6 ms) tras elegir la pieza.
- **La animación no bloquea:** el estado cambia al aceptar el comando y el siguiente se puede enviar mientras la pieza se desplaza.
- **Dos toques por pieza** (Entrada, #13), sin confirmación: F6 da de 3,1 a 4,6 s por pieza.
- **Probar es gratis:** un toque deshace un paso.
- **Verificación:** capturas retenidas y playtest.

## UI Requirements

Sin pantalla propia.

- **Destinos:** se marcan al elegir la pieza, también los tramos a los que puede subir un arquero.
- **Contador de piezas movidas** (abajo a la derecha, por ejemplo 5/12): no cuenta al Rey; con Arremetida dice "sin límite". Debe quedar claro que el Rey va aparte.
- **Restante de la pieza elegida**, visible sin pasar el mouse (regla del proyecto).
- **Pieza ya movida:** marca por forma, para saber cuáles gastaron cupo. *Propuesta: no está en el art bible.*
- **Rechazos visibles:** todos los de la regla 13 salvo `DestinoFueraDelTablero`, que es interno.
- **Deshacer:** abajo a la izquierda (art bible §7).

> **📌 UX Flag — Movimiento**: This system has UI requirements. In Phase 4 (Pre-Production), run `/ux-design` to create a UX spec for each screen or HUD element this system contributes to **before** writing epics. Stories that reference UI should cite `design/ux/[screen].md`, not the GDD directly.

## Cross-References

| Documento | Qué se referencia | Cómo |
|---|---|---|
| `design/gdd/libreria-de-reglas.md` | Reglas 2, 3, 4, 8, 10 y 11; AC-24 y AC-25 | Contrato, códigos y presupuesto |
| `design/gdd/catalogo-de-piezas.md` | Reglas 2 a 6, F8 | `movimiento` e IDs de tipo |
| `design/gdd/maquina-de-turno.md` | Reglas 5, 7, 8, 10, 11, 12 y 17; D4; AC-7 | Fase, límite, pila |
| `design/gdd/tablero-y-despliegue-inicial.md` | Reglas 2, 3, 5, 7 y 11; `techo_movimiento` | Geometría y despliegue |
| `design/gdd/castillo-y-muros.md` | Reglas 3, 5, 6, 7 y 11 | Bloqueo, huecos y arquero |
| `design/gdd/timer-de-turno.md` | Duraciones de mover | F6 |
| `design/gdd/entrada-y-seleccion.md` | Dos toques, sin arrastrar | Game Feel |
| `design/gdd/game-concept.md` | Reglas vigentes, pilares, Open Question de la cruz | La cierra |
| `design/art/art-bible.md` | §3, §4, §5 y §7 | Marcas y HUD |
| `design/registry/entities.yaml` | `pila_max`, `pila_tope`, `techo_movimiento`, `comando_presupuesto_ms` | Valores usados sin cambios |

## Acceptance Criteria

Casillas como (columna, fila). Salvo que se diga otra cosa: turno de azul en fase `Mover`, límite 12, Catálogo vigente, muros enteros, Reyes en `(2, 7)` y `(21, 7)`, ninguna otra pieza que las nombradas, `usados` en 0. **Logic** = test unitario; **Integration** = test entre sistemas; **UI** y **Visual** = captura retenida.

**Reglas**

- **AC-1 (Logic, regla 2)** — **GIVEN** un soldado en `(11, 7)` con `usados` 3, **WHEN** va a `(12, 8)`, **THEN** `FueraDeAlcance`; con `usados` 2 se acepta con costo 2.
- **AC-2 (Logic, reglas 3 y 14, F2)** — **GIVEN** un soldado en `(7, 5)` y un enemigo en `(8, 5)`, **WHEN** va a `(9, 5)`, **THEN** costo 4 y camino `(7,4) (8,4) (9,4) (9,5)`; con `usados` 1, `FueraDeAlcance`.
- **AC-3 (Logic, regla 4)** — **GIVEN** un soldado en campo libre, **WHEN** hace un `Mover` de costo 3, otro de costo 2 y otro de costo 1, **THEN** aceptado, `FueraDeAlcance`, aceptado; un cuarto a otra casilla da `PiezaAgotada`.
- **AC-4 (Logic, regla 5)** — **GIVEN** AC-2 con cada tipo, **THEN** el costo es 4 para soldado, arquero, escudero, demoledor y Rey, 2 para jinete (también si el enemigo es el Rey), y la catapulta da `FueraDeAlcance`. **GIVEN** un aliado en `(8, 5)`, **THEN** los siete tipos pagan 2.
- **AC-5 (Logic, regla 6)** — **GIVEN** un aliado o un enemigo en el destino a 1 paso, **THEN** `DestinoOcupado` para los siete tipos.
- **AC-6 (Logic, reglas 5, 6 y 8)** — **GIVEN** muros enteros, **THEN** el Rey va de `(2, 7)` a `(5, 7)` con costo 3 y a `(4, 7)` da `DestinoBloqueado`; con un arquero propio en `(4, 7)` sigue llegando a `(5, 7)` con costo 3; un jinete en `(5, 7)` hacia `(3, 7)` da `FueraDeAlcance`; un soldado en `(5, 7)` hacia `(4, 7)` da `DestinoBloqueado`; el Rey azul en `(18, 7)` hacia `(19, 7)` da `DestinoBloqueado` y hacia `(20, 7)`, `FueraDeAlcance`.
- **AC-7 (Logic, regla 7)** — **GIVEN** un arquero en `(5, 5)`, **THEN** sube a `(4, 5)` con costo 1. **GIVEN** un arquero en el tramo `(4, 6)`, **THEN** ir a `(3, 6)` da `FueraDeAlcance`; con `(4, 7)` roto cuesta 3. **GIVEN** un arquero en `(4, 5)` y otro en `(4, 6)`, **WHEN** el primero va a `(4, 7)`, **THEN** costo 4 por el anillo. **WHEN** un arquero azul en `(18, 7)` va a `(19, 7)`, **THEN** `DestinoBloqueado`.
- **AC-8 (Logic, regla 10, F4)** — **GIVEN** 12 piezas con `usados` > 0, **THEN** una pieza 13 da `SinMovimientos`; una de las 12 con puntos se acepta; el Rey se acepta y `piezasMovidas` sigue en 12; tras deshacer el único `Mover` de una, la 13 se acepta. Con "sin límite" la 13 se acepta. Con `limite_normal` 1, la segunda pieza da `SinMovimientos`.
- **AC-9 (Integration, regla 11)** — **GIVEN** un soldado reclutado este turno, **WHEN** se mueve 2, **THEN** se acepta, `usados` 2 y `piezasMovidas` sube 1.
- **AC-10 (Logic, regla 12)** — **GIVEN** un `Mover` aceptado, **THEN** hay una entrada más en la pila y un `PiezaMovida`; con uno rechazado la huella no cambia y no hay eventos.
- **AC-11 (Logic, regla 13)** — **GIVEN** cada destino inválido, **THEN** sale su código: `(−1, 0)`, `(24, 0)`, `(0, 15)` e int32 mínimo y máximo dan `DestinoFueraDelTablero`; la propia casilla, `DestinoIgualOrigen`; una pieza Agotada con cupo lleno y destino ocupado, `PiezaAgotada`.
- **AC-12 (Logic, regla 15)** — **GIVEN** el despliegue inicial y otros dos tableros guardados, **WHEN** se prueban los 360 destinos de cada pieza, **THEN** se acepta si y solo si está en `MovimientosLegales`, con el mismo costo y en orden (fila, columna). La lista es vacía para piezas del rival, fuera de la fase, Agotadas y con cupo lleno sin mover.
- **AC-13 (Integration, regla 15)** — **GIVEN** azul solo con el Rey en `(0, 0)` y enemigos en `(1, 0)` y `(0, 1)`, **WHEN** se abre `Mover`, **THEN** `FaseSaltada`.
- **AC-14 (Integration, regla 16)** — **GIVEN** piezas con `usados` > 0, **WHEN** el turno cierra por `Resolver`, por timeout y por saltos, **THEN** en los tres casos todas quedan con `usados` 0.
- **AC-15 (Logic, reglas 14 y 17)** — **GIVEN** dos instancias con el mismo estado, **WHEN** aplican los mismos `Mover`, **THEN** sus eventos (camino incluido) y huellas son iguales; cambiar una constante sin subir la versión de reglas hace fallar el test de referencia.

**Fórmulas**

- **AC-16 (Logic, F1)** — **THEN** 4 − 0 = 4; 4 − 4 = 0; 1 − 1 = 0; 8 − 5 = 3; un estado con `usados` 7 y efectivo 6 da 0.
- **AC-17 (Logic, F2)** — **GIVEN** todos los destinos de AC-12, **THEN** `costo ≥ distanciaCruz`; la búsqueda visita como mucho 360 casillas por pieza, también con movimiento 1.000.
- **AC-18 (Logic, F3)** — **GIVEN** una pieza en `(11, 7)`, **THEN** hay 0, 12, 40 y 84 destinos con restante 0, 2, 4 y 6.
- **AC-19 (Logic, F5)** — **THEN** el oráculo da 254 (Arremetida), 104 (Grito, 12), 78 (reclutar, 12), 254 (Grito, 37) y 260 (Grito, 38). **GIVEN** un soldado y 4 `Mover` de un paso, **THEN** la pila tiene 4 entradas.
- **AC-20 (Logic, F6)** — **THEN** 60 s da 4.615 ms y 40 s da 3.076 ms con límite 12; 30 s con límite 37 da 789 ms.

**Entre sistemas**

- **AC-21 (Integration, #11)** — **GIVEN** Grito de guerra, **THEN** un soldado llega a 6 casillas y al turno siguiente a 4. *Provisional hasta #11.*
- **AC-22 (Integration, #5)** — **GIVEN** un arquero que sube a un tramo, **WHEN** se deshace, **THEN** vuelve a su casilla y `enMuro` es falso. **GIVEN** solo `(0, 5)` roto y un soldado en `(0, 4)`, **THEN** pisa `(0, 5)` y no llega a `(1, 6)`; con `(0, 6)` también roto llega con costo 3.
- **AC-23 (Integration, #4)** — **GIVEN** el despliegue inicial, **THEN** el Rey azul llega a `(8, 7)` con costo 6 y a `(9, 7)` da `FueraDeAlcance`.
- **AC-24 (Integration, #3)** — **GIVEN** Distracción, **THEN** `Mover` da `FaseIncorrecta`. **GIVEN** timeout de mover, **THEN** lo movido queda y `Deshacer` da `FaseIncorrecta`.
- **AC-25 (Integration, rendimiento)** — **GIVEN** el tablero de AC-25 de la Librería, **THEN** `Mover` cumple 1 ms p95, y `hayMovimientoLegal` sin ningún destino visita como mucho 61 × 360 casillas.

**Presentación**

- **AC-26 (UI)** — Captura con destinos marcados, contador 5/12 y restante visibles sin pasar el mouse; otra con "sin límite"; otra con un rechazo y su causa.
- **AC-27 (Visual)** — Captura en escala de grises donde los puntos de movimiento y los corchetes de alcance se distinguen; y medición de que la animación dura ≤ 0,25 s y acepta un comando mientras corre.

La regla 1 es de alcance del documento y la 9 la cubren AC-4, AC-5 y AC-6. La fantasía no es automatizable: va a playtest.

## Open Questions

| Pregunta | Dueño | Para cuándo |
|---|---|---|
| ¿Entran 13 gestos en 40 s en pantalla táctil (3,1 s por pieza)? | Entrada (#13), Timer (#12), playtest | MVP |
| ¿Una muralla de 15 piezas traba la partida (pilar 4)? | Métricas (#19), playtest | MVP |
| ¿El Rey sin cupo se vuelve la mejor pieza de ataque? | Métricas (#19), playtest | MVP |
| ¿Frustra que ir y volver gaste cupo? Evitarlo exige guardar la casilla inicial | Playtest | MVP |
| ¿El rango 8–16 de `limite_normal` es correcto? | Métricas (#19) | Tras las primeras 20 partidas online |
| La marca de pieza ya movida | `/art-bible`, HUD (#15) | En el GDD de #15 |
| Cómo se busca el camino sin reservar memoria | ADR de la Librería | `/create-architecture` |
