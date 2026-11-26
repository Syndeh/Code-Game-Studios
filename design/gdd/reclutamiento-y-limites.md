# Reclutamiento y límites

> **Status**: Designed (pendiente de `/design-review` en una sesión nueva)
> **Author**: Martin Arin + Claude (lentes: game-designer, economy-designer, systems-designer, qa-lead, art-director)
> **Last Updated**: 2026-11-25
> **Last Verified**: 2026-11-25
> **Implements Pillar**: Pilar 3 (Siempre hay vuelta, con límites); Pilar 4 (Duelo corto, como restricción)
> **Creative Director Review (CD-GDD-ALIGN)**: CONCERNS (accepted) 2026-11-25 — pilar 4: Δturnos sin valor medido (F6); pilar 3: el límite de 6 Llamadas casi nunca aprieta (F6) y el todo o nada está sin jugar. Las tres están en Open Questions.

## Summary

Reclutamiento y límites define qué piezas nuevas recibe un jugador cuando el Modificador saca una cara de reclutar, cuántas, dónde aparecen y cuántas veces puede pasar en la partida. El jugador elige un tipo y las piezas aparecen alrededor de su castillo, listas para moverse ese mismo turno. Existe para que quien va perdiendo tenga con qué remontar, con límites que eviten un duelo sin fin.

> **Quick reference** — Layer: `Feature` · Priority: `MVP` · Key deps: `Librería de reglas (#1), Catálogo de piezas (#2), Máquina de turno (#3), Tablero y despliegue inicial (#4), Castillo y muros (#5)`

## Overview

Cuando el Modificador (#11) saca **Llamada al servicio** o **Cuerno de guerra**, la Máquina de turno (#3) abre la fase `Reclutar` y el jugador elige un tipo de pieza. Llamada al servicio da 4 soldados, 4 arqueros o 4 escuderos. Cuerno de guerra da 4 jinetes, 2 catapultas o 1 demoledor. Las piezas aparecen solas en el anillo del castillo propio, primero por el frente que mira al rival, y pueden moverse en ese turno. Reclutar es obligatorio. Una opción solo se puede elegir si entran **todas** sus piezas.

Este sistema es dueño de cuatro cosas: los límites de llamadas por jugador y por partida, la cantidad por llamada, el orden de las casillas de aparición y la consulta que le dice al Modificador si cada cara de reclutar puede salir. Sin él, las bajas serían irreversibles y el primero en perder un intercambio perdería la partida.

**Qué no es.** No tira el dado ni es dueño de las probabilidades base (Modificador, #11), no fija el tope de piezas vivas (Catálogo, #2), no define el anillo (Tablero, #4), no decide el corte por muros (Castillo, #5), ni abre o cierra fases (Máquina, #3).

**Decisiones de esta sesión (jugador, 2026-11-25):**

1. Aparición automática y determinista; `Reclutar(tipo)` no lleva casillas. Se usa el anillo entero, con el frente primero, como en el prototipo v5.
2. Los reclutas se mueven el mismo turno, salvo los del recluta automático, que aparecen cuando el tiempo de mover ya se agotó.
3. Todo o nada, también por tope de piezas vivas. Reemplaza el recluta parcial del cuaderno ("hasta completar el límite") y del prototipo v5.
4. Una cara sin ninguna opción disponible baja a 0 en la tabla del Modificador. La tabla mira muros, llamadas, tope y casillas.
5. El porcentaje de una cara sin tope o lugar se reparte por igual entre Grito de guerra, Arremetida y la otra cara de reclutar si está disponible; nunca a Distracción. Muros rotos y llamadas agotadas siguen con `reparto_liberado`.

## Player Fantasy

**Llegan los refuerzos.** Vas perdiendo, el tablero se te achicó y de pronto suena el cuerno: elegís, y cuatro piezas tuyas aparecen frente al castillo, listas para moverse ya. El turno que ibas a usar para aguantar pasa a ser el turno en que volvés.

**Momento ancla:** elegís "4 jinetes", aparecen mirando al rival y en ese mismo turno salen al galope.

**Pilar que sirve:** Pilar 3, *"El que va perdiendo tiene con qué remontar."* Cumple su prueba: la ayuda son opciones (piezas), no estadísticas, y las probabilidades se consultan antes de tirar.

**Qué le exige al resto del diseño:**

- El momento se ve y se oye (#18, #22) y no pide clics de colocación (#17).
- Las llamadas restantes de los dos jugadores son públicas: un límite que no se ve se siente injusto.
- Ninguna opción bloqueada se queda sin motivo escrito.

**Lo que este encuadre no promete:** que reclutar alcance para ganar. Las piezas nuevas compiten por el mismo límite de 12 movimientos y, con el castillo cortado, no llegan más. Tampoco promete que se muevan si el tiempo eligió por vos.

*Descartados: "el general que administra reservas" (más cerebral; los topes casi no aprietan, ver F6) y "la pieza que faltaba" (el tipo lo limita la cara que salió).*

## Detailed Design

### Core Rules

**Vistas por rol.** *game-designer:* la decisión es el tipo; si la colocación también pidiera elegir, costaría tiempo del turno (pilar 4). *economy-designer:* la única fuente es el dado; los límites son llamadas, tope y muros; no hay un costo por elegir, así que el tipo dominante se vigila con Métricas. *systems-designer:* la tabla y la fase deben usar la misma consulta, o saldrían caras imposibles. *ai-programmer / gameplay-programmer:* consulta pura, sin búsqueda.

1. **Tabla de reclutas (dato de la config).** Cada tipo reclutable pertenece a una sola cara:

   | Cara | Tipo (ID) | Piezas por llamada | Llamadas por partida y jugador |
   |---|---|---|---|
   | Llamada al servicio | Soldado (0), Arquero (1), Escudero (2) | 4 | **6 en total**, compartidas por los tres |
   | Cuerno de guerra | Catapulta (3) | 2 | 4 |
   | Cuerno de guerra | Jinete (4) | 4 | 3 |
   | Cuerno de guerra | Demoledor (5) | 1 | 4 |

   El Rey (6) no se recluta. Las llamadas son por jugador y por partida.
2. **Contadores.** El estado guarda por jugador 4 contadores de llamadas usadas: uno de Llamada al servicio y tres de Cuerno. Empiezan en 0, nunca bajan (que mueran los reclutas no devuelve la llamada) y son públicos.
3. **Cuándo se gasta una llamada.** Solo cuando un reclutamiento ocurre (elegido o automático). Un comando rechazado o un `ReclutaPerdido` no gasta nada. *(El prototipo v5 contaba la Llamada al salir la cara; con la regla 6 las dos cuentas coinciden.)*
4. **Opción disponible.** Un tipo está disponible si se cumplen las cuatro condiciones: el castillo puede reclutar (`puedeReclutar`, #5), le quedan llamadas, sus vivas más las piezas de la llamada no pasan de `topeVivas` (#2), y el anillo tiene libres tantas casillas como piezas (F1). **Todo o nada:** nunca se recluta menos.
5. **Estado de cada cara.** *Agotada*: el castillo no puede reclutar o ningún tipo de la cara tiene llamadas. *Sin lugar*: no está agotada, pero ningún tipo está disponible (por tope o casillas). *Disponible*: al menos un tipo disponible. Solo una cara *Disponible* puede tener probabilidad mayor que 0 en la tabla efectiva.
6. **La misma consulta para la tabla y la fase.** La tabla se calcula al entrar en `EsperandoTirada` y hasta la `Tirada` no cambia ninguna pieza ni muro. Si salió una cara de reclutar, hay una opción disponible al abrir `Reclutar`. `ReclutaPerdido` queda como defensa.
7. **Elegir.** En `Reclutar` el jugador envía `Reclutar(tipo)`. No existe "no reclutar". Los códigos se revisan en este orden:

   | Orden | Código | Cuándo | Lo ve el jugador |
   |---|---|---|---|
   | 1 | `TipoNoReclutable` | El tipo no existe, es el Rey o no es de la cara que salió | Interno |
   | 2 | `CastilloSinReclutar` | `puedeReclutar` es falso (defensa, regla 6) | Sí |
   | 3 | `SinLlamadas` | El tipo no tiene llamadas | Sí |
   | 4 | `TopeDePiezas` | Vivas + piezas de la llamada > `topeVivas` | Sí |
   | 5 | `SinLugarParaReclutar` | Casillas libres del anillo < piezas de la llamada | Sí |

   **Consulta pura `motivo_bloqueo(jugador, tipo)`** (pedida por #17, F2; cierre de la tanda 3): devuelve el primer código de la tabla, en este orden, que rechazaría hoy un `Reclutar(tipo)` de ese jugador, sin contar `TipoNoReclutable`, o `Ninguno`. No depende de la cara que salió ni de la fase, así que el panel «?» y el recuadro la piden en cualquier momento. Es la misma comprobación que `Reclutar`, sin efectos.

8. **Dónde aparecen.** En las primeras casillas libres del anillo propio según `orden_aparicion` (F4): frente primero (columna que mira al rival, desde la fila del Rey hacia afuera) y después los costados, del frente hacia atrás. Una casilla ocupada por cualquier pieza se salta. Nunca aparecen en un tramo, un hueco ni el interior. Los IDs se asignan por (fila, columna) (Librería, regla 7).
9. **Cómo aparecen.** Con la vida completa del Catálogo y `usados = 0`. Se mueven ese turno como cualquier otra y gastan cupo del límite (Movimiento, regla 11). Pueden atacar o cubrir (#7, #8).
10. **Recluta automático.** Si llega el timeout de mover en `Reclutar`, se recluta el primer tipo disponible de la cara en el orden del Catálogo (Llamada: soldado, arquero, escudero; Cuerno: catapulta, jinete, demoledor), con marca `automatico`. Se pasa directo a `Atacar`: esas piezas no se mueven ese turno, pero pueden atacar o cubrir.
11. **No se deshace.** El reclutamiento se compromete al elegir.
12. **Evento.** `Reclutado(jugador, cara, tipo, ids, casillas, llamadasRestantes, automatico)`.

**El jugador no puede:** elegir casillas, rechazar la recluta, reclutar menos, guardar una llamada ni despejar el anillo antes de reclutar (la fase `Mover` viene después).

### States and Transitions

Los estados son por cara y por jugador, y se derivan; solo se guardan los contadores.

| De | A | Cuándo |
|---|---|---|
| Disponible | Sin lugar | Ningún tipo con llamadas entra por tope o casillas |
| Sin lugar | Disponible | Muere o se mueve lo necesario (reversible) |
| Disponible o Sin lugar | Agotada | Se gasta la última llamada de la cara, o se rompe el 2.º tramo (permanente) |
| Agotada | — | No vuelve |

### Interactions with Other Systems

| Sistema | Entra a #10 | Sale de #10 | Dueño de la interfaz |
|---|---|---|---|
| #1 Librería | Comando y config | Códigos, evento, contadores, campo `tablaReclutas` de la config | Librería (contrato) |
| #2 Catálogo | `topeVivas`, orden de tipos, vida | — | Catálogo |
| #3 Máquina | Abre `Reclutar`, timeout | `tipo_disponible`, recluta automático, Δturnos (F6) | Máquina |
| #4 Tablero | Anillo, `ocupante`, `ladoDelRival` | Piezas nuevas en casillas | Tablero |
| #5 Castillo | `puedeReclutar` | — | Castillo |
| #6 Movimiento | — | Piezas con `usados = 0` | Reclutamiento |
| #11 Modificador | Cara que salió | `estado_cara(j, cara)`, `llamadas_restantes(j, tipo)` | Reclutamiento; el Modificador solo lee |
| #15, #17, #18, #19, #25 | — | Consultas (`motivo_bloqueo` incluida) y evento (solo lectura) | Reclutamiento (reglas y contenido); #17 es dueño de la disposición y de los toques del recuadro |

## Formulas

Todo es aritmética entera. Los valores de piezas salen del Catálogo.

### F1. Disponibilidad de un tipo

The `tipo_disponible` formula is defined as:

`tipo_disponible(j, t) = puedeReclutar(j) Y llamadas_restantes(j, t) > 0 Y vivas(j, t) + n_t ≤ topeVivas_t Y libres(j) ≥ n_t`

**Variables:**

| Variable | Type | Range | Source | Description |
|----------|------|-------|--------|-------------|
| `puedeReclutar(j)` | bool | — | calculated (#5, F7) | Menos de 2 tramos rotos |
| `llamadas_restantes(j, t)` | int | 0–20 (hoy 0–6) | calculated (F2) | Llamadas sin usar |
| `vivas(j, t)` | int | 0–`topeVivas_t` | calculated (estado) | Piezas vivas del tipo |
| `n_t` | int | 1–17 (hoy 4, 2 o 1) | data file (config) | Piezas por llamada |
| `topeVivas_t` | int | 4–16 (reclutables, valores vigentes) | data file (#2) | Tope de vivas |
| `libres(j)` | int | 0–17 | calculated (#4) | Casillas del anillo sin pieza |

**Output Range:** booleano. Un tipo se bloquea por tope con más de 8 soldados vivos, 12 arqueros, 8 escuderos, 6 catapultas, 4 jinetes o 3 demoledores.
**Example:** 5 jinetes vivos: 5 + 4 = 9 > 8 → bloqueado. Con 4: 8 ≤ 8 → disponible.

### F2. Llamadas restantes

The `llamadas_restantes` formula is defined as:

`llamadas_restantes(j, t) = llamadas_max(g) − usadas(j, g)`, con `g` el contador del tipo `t`

**Variables:**

| Variable | Type | Range | Source | Description |
|----------|------|-------|--------|-------------|
| `g` | id | Común (tipos 0–2), Catapulta, Jinete, Demoledor | data file (config) | Contador al que pertenece `t` |
| `llamadas_max(g)` | int | 0–20 (hoy 6, 4, 3, 4) | data file (config) | Tope de llamadas |
| `usadas(j, g)` | int | 0–`llamadas_max(g)` | calculated (estado) | Llamadas gastadas |

**Output Range:** 0 a `llamadas_max(g)`. Nunca negativo: F1 exige > 0 antes de sumar 1.
**Example:** 2 llamadas de soldados y 1 de arqueros → `usadas(Común)` = 3 → quedan 3 para los tres tipos.

### F3. Estado de la cara

The `estado_cara` formula is defined as:

`estado_cara(j, c) = Agotada si NO puedeReclutar(j) o ningún t de c con llamadas_restantes(j, t) > 0; si no, SinLugar si ningún t de c con tipo_disponible(j, t); si no, Disponible`

**Variables:**

| Variable | Type | Range | Source | Description |
|----------|------|-------|--------|-------------|
| `c` | id | Llamada, Cuerno | data file (papeles de cara) | Cara de reclutar |
| `tipo_disponible` | bool | — | calculated (F1) | Opción elegible |
| `llamadas_restantes` | int | 0–20 | calculated (F2) | Llamadas sin usar |

**Output Range:** uno de tres valores.
**Example:** Cuerno con jinetes sin llamadas, 7 catapultas y 4 demoledores vivos, con llamadas de ambos → SinLugar. Si muere una catapulta (6 + 2 = 8) → Disponible.

### F4. Orden de aparición

The `orden_aparicion` formula is defined as:

`clave(c, f) = (−(c − cc) × lado, |f − cf|, f)`, en orden ascendente, sobre las casillas libres del anillo

**Variables:**

| Variable | Type | Range | Source | Description |
|----------|------|-------|--------|-------------|
| `c`, `f` | int | 0–23, 0–14 | calculated (#4) | Casilla del anillo |
| `cc`, `cf` | int | 2 o 21, 7 | constant (#4) | Centro del castillo |
| `lado` | int | −1, +1 | calculated (`ladoDelRival`, #4) | Dirección al rival |

**Output Range:** un orden total sin empates de 17 casillas.
**Example (azul):** (5,7), (5,6), (5,8), (5,5), (5,9), (5,4), (5,10), (4,4), (4,10), (3,4), (3,10), (2,4), (2,10), (1,4), (1,10), (0,4), (0,10). Rojo es el espejo `(23 − c, f)`. Cuatro reclutas con el anillo libre ocupan (5,5), (5,6), (5,7) y (5,8), y sus IDs crecen en ese orden.

### F5. Reparto de una cara sin lugar

The `reparto_sin_lugar` formula is defined as:

`Q = Σ p_i de las caras SinLugar` (pasan a 0) · `R = {Grito, la otra cara de reclutar si está Disponible, Arremetida}` · `m = |R|` · cada cara de `R` suma `Q / m` (división entera hacia abajo) y las primeras `Q mod m`, en ese orden, suman 1 más

**Variables:**

| Variable | Type | Range | Source | Description |
|----------|------|-------|--------|-------------|
| `p_i` | int | 0–100 | data file (tabla base, #11) | Probabilidad base |
| `Q` | int | 0–100 | calculated | Porcentaje liberado por tope o lugar |
| `m` | int | 2–3 | calculated | Caras que reciben |

**Composición con `reparto_liberado`** (Librería): las caras Agotadas liberan `L`, que va mitad a Grito y mitad a Arremetida (el punto impar a Arremetida); las SinLugar liberan `Q`. Los dos se calculan **desde la tabla base** y se suman, así que el orden no importa. Al final, con `piezas_sin_rey ≤ limite_normal`, toda Arremetida pasa a Grito. Distracción no cambia nunca.

**Output Range:** tabla que suma exactamente 100 (`m ≥ 2`, sin división por cero). Con `Q = 0` no cambia nada.
**Example** (base 15/20/25/25/15: Distracción/Grito/Llamada/Cuerno/Arremetida, más de 12 piezas salvo la última fila):

| Caso | Tabla efectiva |
|---|---|
| Llamada SinLugar, Cuerno Disponible: Q = 25, m = 3 → 9, 8, 8 | 15/29/0/33/23 |
| Las dos SinLugar: Q = 50, m = 2 → 25, 25 | 15/45/0/0/40 |
| Llamada Agotada, Cuerno SinLugar: L = 25 → 12 y 13; Q = 25 → 13 y 12 | 15/45/0/0/40 |
| Llamada SinLugar con 12 piezas o menos | 15/52/0/33/0 |

### F6. Aporte a la duración (Δturnos)

The `delta_reclutamiento` formula is defined as:

`delta_reclutamiento = ⌈piezas_reclutadas / bajas_por_turno⌉` (fórmula de diseño para Métricas; no corre en el núcleo)

**Variables:**

| Variable | Type | Range | Source | Description |
|----------|------|-------|--------|-------------|
| `piezas_reclutadas` | int | 0–96 (48 por jugador) | calculated (Métricas, #19) | Piezas sumadas por los dos |
| `bajas_por_turno` | int | ≥ 1 | calculated (Métricas, #19) | Media redondeada, mínimo 1 |

**Output Range:** 0 a 96. **NOT SOURCEABLE — `bajas_por_turno` no está medido:** este GDD declara la fórmula y la cota, no el valor. Es una cota superior: supone que toda pieza reclutada debe morir.
**Example (ilustrativo, sin medir):** 24 piezas y 3 bajas por turno → 8 turnos.

**Cuánto aprietan las llamadas.** Con 14 tiradas por jugador y 20% de probabilidad se esperan 2,8 Llamadas; la chance de que salga 6 veces o más es 4%. Con 25%, 3,5 y 11%. El tope de 6 casi nunca se alcanza: es un seguro contra partidas largas. Elegir reclutar no agrega un tiempo propio: comparte la etapa de mover.

**Lazos.** Negativo (buscado): perder piezas → más probabilidad de reclutar (#11) y más margen bajo el tope. Positivo, amortiguado: más piezas → más golpes combinados; lo frenan el límite de 12 movimientos, el tope y el corte por muros.

## Edge Cases

**Disponibilidad**

- **If quedan llamadas pero el tipo no entra por tope**: bloqueado con `TopeDePiezas`; la llamada se conserva y vuelve a servir si mueren piezas.
- **If quedan 3 casillas libres en el anillo**: los tipos de 4 se bloquean (`SinLugarParaReclutar`); catapulta (2) y demoledor (1) siguen. Llamada queda SinLugar.
- **If el anillo está lleno por piezas propias al empezar el turno**: las dos caras bajan a 0 ese turno. El jugador no puede despejarlo antes: `Mover` viene después.
- **If el rival ocupa 14 o más casillas del anillo**: bloquea las llamadas de 4. Es legal; exige casi todo un ejército.
- **If el jugador llena su propio anillo para subir Grito o Arremetida**: legal y reversible. *Posible estrategia degenerada; se mide (Open Questions).*
- **If un tipo se queda sin llamadas y los otros de su cara siguen**: la cara sigue Disponible si alguno entra.
- **If las dos caras no están Disponibles por causas distintas**: se aplican los dos repartos (F5).

**Momento**

- **If el 2.º tramo se rompe en el turno del rival**: la tabla del dueño ya lo refleja; las dos caras salen Agotadas.
- **If una pieza recién reclutada muere**: libera tope para la tabla siguiente, sin devolver la llamada.
- **If es el primer turno de la partida**: no hay tirada ni reclutamiento.
- **If hay un hueco en el muro propio**: las piezas no aparecen en él; el orden del anillo no cambia.
- **If la fase `Reclutar` se abriera sin opción**: `ReclutaPerdido`, sin gastar llamada (defensa, regla 6).

**Timeout y fin**

- **If vence la etapa de mover en `Reclutar`**: recluta automático, pasa a `Atacar`; las piezas no se mueven.
- **If la fase `Atacar` que sigue no tiene acción posible**: `FaseSaltada` (Librería, regla 17).
- **If vence el reloj de partida en `Reclutar`**: pierde su dueño; no se recluta.
- **If el jugador se rinde en `Reclutar`**: termina la partida; la llamada no se gasta.
- **If el próximo ID superara C_MAX**: `LimitePartida` (Librería). Inalcanzable con los valores vigentes: 26 + 96 = 122.

**Config**

- **If `n_t` es menor que 1, mayor que 17 o mayor que `topeVivas_t`; `llamadas_max` es menor que 0 o mayor que 20; un tipo está en dos caras, o el Rey figura**: `ConfigInvalida`, no se crea la partida.
- **If `llamadas_max` de todos los tipos de una cara es 0**: válido; la cara nace Agotada.

## Dependencies

**Depende de:**

| Sistema | Tipo | Qué usa |
|---|---|---|
| #1 Librería de reglas | Dura | `Reclutar(tipo)`, config, IDs, todo o nada, tabla efectiva |
| #2 Catálogo de piezas | Dura | `topeVivas`, orden de tipos, vida |
| #3 Máquina de turno | Dura | Fase `Reclutar`, momento del recluta automático |
| #4 Tablero y despliegue | Dura | Anillo, `ocupante`, `ladoDelRival` |
| #5 Castillo y muros | Dura | `puedeReclutar` |

**Dependen de este sistema:**

| Sistema | Tipo | Qué usa |
|---|---|---|
| #1 Librería | Dura | Contadores, `tablaReclutas`, códigos, `estado_cara` y F5 para la tabla efectiva |
| #3 Máquina de turno | Dura | `tipo_disponible`, Δturnos |
| #6 Movimiento | Blanda | Piezas nuevas con `usados = 0` |
| #11 Modificador | Dura | `estado_cara`, `llamadas_restantes` |
| #7 Alcance | Blanda | Un recluta puede atacar ese turno |
| #8 Cobertura | Blanda | Un recluta puede cubrir o ser cubierto ese turno |
| #13 Entrada y selección | Blanda | Códigos y comando |
| #14 Localización | Blanda | Textos de los códigos visibles |
| #15 HUD, #18 Render | Blanda | Evento y contadores |
| #17 Interfaz del Modificador | Dura | Opciones, motivos, llamadas |
| #19 Métricas | Dura | Evento, para F6 |
| #24 Tutorial, #25 Rival IA | Dura | Consultas y comando |
| #26 Registro | Blanda | `tablaReclutas` en la config |

**Bidireccionalidad:** los cambios en los vecinos se aplicaron el 2026-11-25 al cerrar la tanda 1 (Librería, Catálogo, Máquina, Tablero, Castillo, Movimiento y concepto). #11 (tanda 2), #17 (tanda 3), #18 y #19 (tanda 4) ya tienen GDD y nombran a Reclutamiento; la arista Dura con #19 queda confirmada.

## Tuning Knobs

| Perilla | Valor | Rango seguro | Si es muy alta | Si es muy baja | Interactúa con |
|---|---|---|---|---|---|
| `llamadas_max` Común | 6 | 3–8 | Partidas largas (pilar 4) | La remontada se corta (pilar 3) | Por encima de ~6 casi no tiene efecto a 14 tiradas (F6) |
| `llamadas_max` Catapulta | 4 | 2–5 | Más área en juego | Pieza casi ausente | `topeVivas` 8 |
| `llamadas_max` Jinete | 3 | 2–4 | Presión constante | Cara sin atractivo | Con tope 8 cada llamada exige ≤ 4 vivos |
| `llamadas_max` Demoledor | 4 | 2–5 | Demasiadas explosiones | Muros muy seguros | `topeVivas` 4 |
| `n_t` común | 4 | 3–5 | Se bloquea antes por tope | Refuerzo sin peso | `topeVivas`; casillas ocupadas para bloquear = 18 − `n_t` |
| `n_t` Catapulta / Jinete / Demoledor | 2 / 4 / 1 | 1–3 / 2–4 / 1–2 | Se bloquea antes por tope | Refuerzo sin peso | `topeVivas` |

Todas son `data file` y entran a la huella. Los rangos seguros son de diseño, sin medir. **De otro GDD (no se duplican):** `topeVivas` (#2), `TRAMOS_ROTOS_CORTE` (#5), probabilidades (#11), `limite_normal` (#6), tiempos (#12). El Catálogo (#2) recoge la regla 4 desde el 2026-11-25.

## Visual/Audio Requirements

Lente art-director, sobre la regla visual del art bible (§1: ficha, figura, bando, terreno) y su zona "Centro del tablero" para los recuadros de reclutar.

| Evento | Visual | Audio |
|---|---|---|
| Cara de reclutar sale | Recuadro central con las opciones de la cara | Llamada y cuerno, distinguibles entre sí |
| `Reclutado` | Las piezas aparecen a la vez en sus casillas, con el bando marcado, ≤ 0,5 s | Un golpe de llegada por tanda, no por pieza |
| `automatico` | Aviso de texto "Elegido por el tiempo" en la zona de avisos del HUD, sin marca sobre la pieza ni la ficha (#15, regla 18; decisión del 2026-11-26) | Sin sonido de error |
| `ReclutaPerdido` | Leyenda "Sin reclutas disponibles", ≤ 1 s (Máquina, #3) | Ninguno |
| Opción bloqueada | Atenuada con ícono y motivo escrito, no solo color | Ninguno |

La animación no bloquea la entrada. El significado nunca depende solo del color.

📌 **Asset Spec**: con el art bible aprobado, `/asset-spec system:reclutamiento-y-limites`.

## Game Feel

- **Respuesta:** la pieza queda seleccionable a más tardar 0,5 s después de elegir; la entrada no espera la animación.
- **Peso:** la tanda entera llega como un solo golpe, acorde con "Llegan los refuerzos".
- **Clics:** uno para elegir el tipo, con mouse y en táctil (Entrada, #13, regla 12). Cero para colocar.
- **Sin penalidad sensorial** por opción bloqueada: se explica, no se reprende.

## UI Requirements

- Recuadro central: hasta 3 opciones con cantidad, vivas / tope y llamadas restantes; las bloqueadas, con el motivo traducido desde su código (#14) y **sin** la línea de casillas. La disposición y los toques los fija la Interfaz del Modificador (#17); este GDD fija las reglas y el contenido.
- Las casillas de aparición se marcan numeradas en el tablero mientras el recuadro está abierto, sin hover ni toque extra (Entrada, #13, regla 12; decisión del jugador, 2026-11-25): como salen en `orden_aparicion` (F4), las de 1 o 2 piezas son las primeras de las de 4. El sistema decide y se ve antes de elegir (pilar 1).
- Panel "?" (#17): llamadas restantes de los dos jugadores y, si una cara está en 0, por qué (muros, llamadas, tope o lugar).

📌 **UX Flag — Reclutamiento**: correr `/ux-design` para el recuadro de reclutar antes de escribir épicas.

## Cross-References

| Este documento dice | Fuente | Elemento |
|---|---|---|
| Corte con 2 tramos rotos | `design/gdd/castillo-y-muros.md` | F7 |
| Anillo de 17 casillas, `ladoDelRival` | `design/gdd/tablero-y-despliegue-inicial.md` | Regla 11, AC-11 |
| Topes y orden de tipos | `design/gdd/catalogo-de-piezas.md` | `topeVivas`, orden de tipos |
| Fase `Reclutar`, timeout | `design/gdd/maquina-de-turno.md` | Regla 6, Edge Cases (Timeouts), D1 |
| `reparto_liberado`, códigos, IDs | `design/gdd/libreria-de-reglas.md` | Reglas 6, 7, 17; Formulas |
| Reclutas con `usados = 0` | `design/gdd/movimiento.md` | Regla 11 |
| El timer no se reinicia al pasar a `Mover` | `design/gdd/timer-de-turno.md` | Regla 5 |
| Llamadas 6 / 3 / 4 / 4 y caras | `design/gdd/game-concept.md` | Reglas vigentes |

## Acceptance Criteria

Todos automatizados (EditMode) salvo donde se indica.

- **AC-1 (regla 1)** — **GIVEN** salió Llamada, **WHEN** se envía `Reclutar(Arquero)`, **THEN** hay 4 arqueros más. Con Cuerno: 4 jinetes, 2 catapultas o 1 demoledor.
- **AC-2 (reglas 1 y 2, F2)** — **GIVEN** 2 llamadas de soldados y 4 de arqueros usadas, **THEN** quedan 0 para los tres tipos y `estado_cara(Llamada)` es Agotada; el otro jugador conserva 6.
- **AC-3 (regla 2)** — **GIVEN** una llamada usada, **WHEN** mueren sus piezas, **THEN** el contador no cambia.
- **AC-4 (regla 3)** — **GIVEN** la fase `Reclutar`, **WHEN** se rechaza un `Reclutar`, **THEN** contadores, piezas y huella no cambian y hay 0 eventos.
- **AC-5 (regla 4, F1, jinetes)** — **GIVEN** 4 jinetes vivos, **THEN** disponible. **GIVEN** 5, **THEN** `TopeDePiezas` y 0 piezas nuevas.
- **AC-6 (F1, límites por tipo)** — **GIVEN** vivas en 8/9 soldados, 12/13 arqueros, 8/9 escuderos, 6/7 catapultas, 3/4 demoledores, **THEN** el primer valor de cada par está disponible y el segundo no.
- **AC-7 (F1, casillas)** — **GIVEN** 4 casillas libres, **THEN** `Reclutar(Soldado)` se acepta. **GIVEN** 3, **THEN** `SinLugarParaReclutar` y catapulta sigue disponible con Cuerno. **GIVEN** 0, **THEN** ningún tipo está disponible.
- **AC-8 (regla 7, orden de códigos)** — **GIVEN** un tipo de la cara sin llamadas, con tope lleno y sin lugar, **THEN** `SinLlamadas`. **GIVEN** salió Llamada, **WHEN** `Reclutar(Jinete)` o `Reclutar(Rey)`, **THEN** `TipoNoReclutable`.
- **AC-9 (regla 5, F3)** — **GIVEN** 2 tramos rotos, **THEN** las dos caras Agotada. **GIVEN** Cuerno con jinetes sin llamadas, 7 catapultas y 4 demoledores vivos (con llamadas), **THEN** SinLugar; con 6 catapultas, Disponible.
- **AC-10 (regla 6)** — **GIVEN** 1.000 estados generados con semilla fija, **WHEN** se entra en `EsperandoTirada` y sale cualquier cara de reclutar con probabilidad mayor que 0, **THEN** al abrir `Reclutar` hay al menos un tipo disponible y nunca `ReclutaPerdido`.
- **AC-11 (F4)** — **GIVEN** el anillo libre, **THEN** el orden azul es la lista de F4 completa y el rojo su espejo.
- **AC-12 (F4, ocupadas e IDs)** — **GIVEN** (5,7) ocupada, **WHEN** azul recluta 4, **THEN** aparecen en (5,5), (5,6), (5,8) y (5,9), y los IDs crecen por fila y columna.
- **AC-13 (regla 8)** — **GIVEN** el tramo (4,7) roto, **THEN** ningún recluta aparece en (4,7).
- **AC-14 (regla 9, con #6)** — **GIVEN** un recluta elegido y el frente despejado, **WHEN** se abre `Mover`, **THEN** tiene `usados = 0`, vida completa y movimientos legales, y moverlo gasta 1 de cupo.
- **AC-15 (regla 10, con #3 y #12)** — **GIVEN** salió Cuerno y la catapulta está bloqueada, **WHEN** vence mover, **THEN** se reclutan 4 jinetes con `automatico`, la fase es `Atacar` (si hay acción posible) y ninguno se movió.
- **AC-16 (regla 11)** — **GIVEN** un reclutamiento, **WHEN** se envía `Deshacer` sin movimientos, **THEN** `NadaQueDeshacer` y las piezas siguen.
- **AC-17 (regla 12)** — **GIVEN** un reclutamiento, **THEN** el evento trae jugador, cara, tipo, ids, casillas, llamadas restantes y `automatico`.
- **AC-18 (F5, con #1 y #11)** — **GIVEN** la base 15/20/25/25/15 y 13 piezas sin el Rey, **THEN** las cuatro filas del ejemplo de F5 dan exactamente esas tablas (la cuarta, con 12 piezas).
- **AC-19 (F5, propiedad)** — **GIVEN** cada tabla base y cada combinación de estados de las dos caras, **THEN** la tabla suma 100, ningún valor es negativo, Distracción no cambia y las caras no Disponibles valen 0.
- **AC-20 (config)** — **GIVEN** `n_t` 0, 18 o mayor que `topeVivas_t`; `llamadas_max` −1 o 21; un tipo en dos caras; o el Rey en la tabla, **THEN** `ConfigInvalida`. `llamadas_max` 0 se acepta.
- **AC-21 (F6, Métricas)** — **GIVEN** 24 piezas reclutadas y 3 bajas por turno, **THEN** `delta_reclutamiento` = 8; con 25, 9; con 0, 0.
- **AC-22 (reloj y rendición)** — **GIVEN** la fase `Reclutar`, **WHEN** llega `CerrarPartida(RelojAgotado)` o `Rendirse`, **THEN** la partida termina sin piezas nuevas ni llamada gastada.
- **AC-23 (UI, captura)** — **GIVEN** el recuadro con una opción bloqueada, **THEN** la captura en `production/qa/evidence/` muestra cantidad, vivas / tope, llamadas y el motivo, legible en grises.

## Open Questions

| Pregunta | Dueño | Cuándo |
|---|---|---|
| ¿Llenar el propio anillo para subir Grito o Arremetida es una estrategia dominante? La medida existe (Métricas, #19, regla 14). | Métricas (#19) | Playtests del MVP |
| Valor de `bajas_por_turno` para cerrar Δturnos (F6) Se mide con Métricas (#19, F4). | Métricas (#19) | Primeras 3 partidas del MVP |
| ¿El tope de 6 Llamadas sirve si casi nunca se alcanza (4–11%)? Se mantiene en 6 como seguro (Modificador, Tuning Knobs) La medida existe (Métricas, #19, regla 14). | Métricas (#19) | Playtest |
| Con todo o nada, ¿los jinetes bloqueados con 5 a 8 vivos se sienten injustos? El prototipo v5 reclutaba parcial: esta regla está sin jugar | Playtest | MVP |
| Dos repartos según la causa (F5 y `reparto_liberado`): ¿se entienden en el panel "?"? #17 los muestra con el cambio contra la base y el motivo; queda medirlo | Playtest | MVP |
| Con 12 piezas o menos, Grito absorbe hasta 85%; el pilar 3a se enmendó para admitirlo (Modificador, regla 12): ¿se siente injusto? La medida existe (Métricas, #19, regla 14). | Métricas (#19) | Playtest |
