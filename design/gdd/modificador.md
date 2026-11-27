# Modificador (dado adaptativo)

> **Status**: Designed (pendiente de `/design-review` en una sesión nueva)
> **Author**: Martin Arin + Claude (lentes: game-designer, systems-designer, ai-programmer, art-director, qa-lead)
> **Last Updated**: 2026-11-25
> **Last Verified**: 2026-11-25
> **Implements Pillar**: Pilar 3 Siempre hay vuelta, con límites; respeta 1 Planificar y cosechar y 4 Duelo corto
> **Creative Director Review (CD-GDD-ALIGN)**: CONCERNS (accepted) 2026-11-25. (1) Pilar 3a: la enmienda de la regla 12 se aplicó en `game-concept.md` el 2026-11-25; el reparto hacia el Grito (hasta 85% para el que pierde) se mide en el MVP. (2) REVISED 2026-11-25: Overview y Player Fantasy decían que el dado «no toca el daño», contra el +2 de ataque del Grito; corregidos para apoyarse en que el bono se conoce antes de planificar y ya está en la vista previa. (3) Pilar 4: el aporte de Distracción (F4) y `W` (F5) están declarados pero sin medir.

## Summary

El **Modificador** es el dado adaptativo de rolchess: al empezar cada turno, desde el segundo, tira una de cinco caras (Distracción, Grito de guerra, Llamada al servicio, Cuerno de guerra o Arremetida) que le da al jugador un recurso antes de planificar. Sus probabilidades se adaptan a cuántas piezas tiene cada jugador, para que el que va perdiendo tenga más chances de reclutar, y ninguna cara cae sin sentido: las que no pueden salir se reparten. Existe para sostener el pilar de remontada sin que el dado decida golpes, con las probabilidades de los dos jugadores siempre consultables.

> **Quick reference** — Layer: `Feature` · Priority: `MVP` · Key deps: `Librería de reglas (#1), Máquina de turno (#3), Reclutamiento y límites (#10), Castillo y muros (#5), Alcance y asignación de ataques (#7)`

## Overview

El **Modificador** es el dado adaptativo de rolchess. Al empezar cada turno, salvo el primero de la partida, la autoridad tira un dado de cinco caras (**Distracción**, **Grito de guerra**, **Llamada al servicio**, **Cuerno de guerra** y **Arremetida**) y la cara que sale decide qué recurso recibe el jugador del turno *antes* de que planifique: sin fase de movimiento pero con ataque (Distracción), +2 de ataque y de movimiento a todas sus piezas durante ese turno (Grito), cuatro piezas comunes o un refuerzo especial a elegir (Llamada, Cuerno), o mover todas las piezas que quiera (Arremetida). El jugador no opera el dado: la tirada es automática y en pantalla se ve solo el nombre de la cara. Lo único que elige, cuando sale una cara de reclutar, es el tipo de pieza (eso es de Reclutamiento, #10).

Lo que lo hace *adaptativo* es que sus probabilidades no son fijas. Al empezar el turno se comparan las piezas vivas de cada jugador (Rey incluido, muros no) y se elige una de cinco tablas base: normal, o inclinada hacia el que va perdiendo o hacia el que va ganando. Después se anulan las caras de reclutar que no pueden salir (castillo cortado, llamadas agotadas, sin tope o sin lugar) y su parte se reparte entre las caras que sí pueden. Las tablas de los dos jugadores se pueden consultar en cualquier momento. Este sistema es dueño de las cinco tablas base y de la regla que elige entre ellas, de lo que hace cada cara (el resultado tipado que le devuelve a la Máquina de turno y los bonos que escribe en el estado) y de las cifras de esos bonos. Las demás piezas del mecanismo ya tienen dueño y este documento las usa tal cual, sin redefinirlas: la conversión de la tirada en una cara y el armado de la tabla efectiva (Librería, #1), el estado de cada cara de reclutar (Reclutamiento, #10, F3), las fases del turno (Máquina, #3) y el corte por muros (Castillo, #5).

Sin el Modificador no habría remontada (pilar 3), el turno sería siempre igual y el recurso de reclutar no tendría de dónde salir. Con probabilidades fijas, la v2 perdía el turno en el 20% de las tiradas y el dado inclinaba partidas; por eso las probabilidades se adaptan, se consultan y ninguna cara cae sin sentido.

**Qué no es.** No decide si un ataque acierta, y el daño de cada golpe sigue siendo fijo y visible antes de resolver, con el bono del Grito ya incluido: da recursos y un bono de un turno, nunca un golpe que se resuelva con azar (anti-pilar "NOT un dado que decide golpes", cuya única excepción vigente es Distracción). No mide tiempo ni genera azar: la tirada la inyecta la autoridad (en local el cliente; online solo el servidor) y el núcleo la convierte en cara con enteros. No abre ni cierra fases, no recluta, no reparte probabilidad liberada y no calcula la tabla efectiva.

→ Cómo se codifica la elección de tabla y la escritura de bonos en el estado queda para el ADR de la Librería.

## Player Fantasy

**El dado te sirve la mesa; el golpe lo armás vos.** Empieza tu turno y antes de pensar nada ya sabés con qué contás: moverlo todo, un refuerzo para elegir, un +2 en todo el ejército o, con Distracción, atacar con lo que ya dejaste armado. El dado nunca decide si un golpe acierta ni cambia el daño después de que planificaste: lo más que hace es subir 2 el ataque del turno, y eso ya está en la vista previa antes de que asignes. Quién cae lo decide el plan que armás después, a la vista.

**Momento ancla:** tenés 14 piezas y sale **Arremetida**. Movés todas las que quieras, cerrás cuatro soldados sobre un mismo tramo del muro (30 de vida, sin escudo), asignás las flechas, la vista previa marca el tramo ROTO (4 × 8 = 32) y resolvés. Cae exactamente lo que viste. El dado te dio el tablero; el golpe fue tuyo.

**Pilar que sirve:** Pilar 1, *«El azar se resuelve antes de planificar, nunca entre el plan y la cosecha.»* El Modificador es esa frase hecha regla: la tirada va primero, se ve entera y no vuelve a entrar en el turno. Sirve además al pilar 3 (ver Tuning Knobs y Formulas) pero su fantasía no es la remontada: eso es la fantasía de Reclutamiento (#10, «Llegan los refuerzos»).

**Qué le exige al resto del diseño:**

- La tirada se resuelve y se muestra *antes* de que el jugador pueda mover (fase `EsperandoTirada`, Máquina #3). Ningún efecto del dado aparece entre asignar los ataques y resolverlos.
- El bono del Grito de guerra tiene que estar incluido en la vista previa de ataques (#16) y en las estadísticas que ve el jugador (#15): si el +2 no se ve antes de resolver, el dado estaría decidiendo en la cosecha.
- El panel «?» con las probabilidades de los dos jugadores está siempre disponible (pilar 3b) y muestra la tabla que *realmente* se usará, la efectiva y no la base.
- Ninguna cara cae sin sentido: una cara que no puede hacer nada se redistribuye antes de tirar, no después.

**Lo que este encuadre no promete:** que cada turno sea parejo (hay caras mejores que otras según el tablero) ni que el dado no pese: promete que *ningún golpe se resuelve con azar*: lo que el dado da se sabe antes de planificar y ya está en la vista previa. **Excepción declarada:** Distracción quita la fase de movimiento (el jugador sigue atacando con lo que ya tiene posicionado); es la excepción vigente al anti-pilar «NOT un dado que decide golpes» y el director creativo la marcó en contra de los pilares 1 y 3 cuando hacía perder el turno (hoy conserva el ataque).

*Descartados:* «Siempre hay una mano» (la remontada vive en Reclutamiento y en el panel de probabilidades, no es la identidad de este sistema) y «Plan B al instante» (celebra justo la cara que choca con el anti-pilar).

## Detailed Design

### Core Rules

**Vistas por rol.** *game-designer:* el jugador no decide la cara; decide si consulta el «?», qué tipo recluta y cómo usa el bono. Ninguna cara decide un golpe. *systems-designer:* una sola tabla efectiva por turno, calculada al abrirlo y congelada hasta la tirada; las tablas, los dos repartos y los lazos están en Formulas. *ai-programmer:* el Rival IA (#25) usa la misma consulta pura que el panel y no ve la tirada antes que el jugador. *art-director:* cada cara necesita una señal propia que no dependa del color ni del texto (Visual/Audio).

1. **Cinco caras y su papel (dato de la config).** El orden de la tabla es fijo y los papeles los exige la Librería (regla 2: exactamente una cara de Grito, una de Arremetida, distintas de las de reclutar; como mucho dos de reclutar):

   | Índice | Cara | Papel |
   |---|---|---|
   | 1 | Distracción | otra |
   | 2 | Grito de guerra | Grito |
   | 3 | Llamada al servicio | reclutar |
   | 4 | Cuerno de guerra | reclutar |
   | 5 | Arremetida | Arremetida |

   Los nombres que ve el jugador salen de Localización (#14), por ID de cara.
2. **Cuándo se tira.** Al abrir cada turno desde el segundo (Máquina, regla 4), con una sola cara por turno. **El primer turno de la partida no tira** (Máquina, regla 3): empieza en `Mover`, sin reclutar y sin bono. Como sale una sola cara por turno, el Grito nunca coincide con un reclutamiento.
3. **Qué hace cada cara.** El Modificador devuelve a la Máquina un resultado tipado (Máquina, regla 5) y, solo el Grito, escribe bonos en el estado:

   | Cara | Resultado tipado | Escribe en el estado | Efecto en el turno |
   |---|---|---|---|
   | Distracción | `SaltearMovimiento` | nada | No hay fase de mover: se pasa directo a `Atacar` (regla 8) |
   | Grito de guerra | `Seguir` | `bonoAtaque` = 2, `bonoMovimiento` = 2 | +2 de ataque (menos el demoledor) y de movimiento (regla 7) |
   | Llamada al servicio | `ReclutarObligatorio(Llamada)` | nada | Reclutar 4 soldados, arqueros o escuderos (#10) |
   | Cuerno de guerra | `ReclutarObligatorio(Cuerno)` | nada | Reclutar 4 jinetes, 2 catapultas o 1 demoledor (#10) |
   | Arremetida | `LimiteMovimientos(sin límite)` | límite del turno sin tope | Se mueven todas las piezas que se quiera; el movimiento de cada pieza no cambia |

4. **Elegir la tabla base.** Al entrar en `EsperandoTirada`, para el jugador J del turno, `mias` son sus piezas vivas **con el Rey y sin muros** y `rival` las del rival. Se evalúan las condiciones en este orden y gana la primera que se cumple. Todo es aritmética entera (productos cruzados, sin fracciones):

   | Orden | Tabla base | Se elige si | Distracción | Grito | Llamada | Cuerno | Arremetida |
   |---|---|---|---|---|---|---|---|
   | 1 | Pierde con 1/2 o menos | `mias × 2 ≤ rival` | 15 | 20 | 25 | 25 | 15 |
   | 2 | Pierde con 3/4 o menos | `mias × 4 ≤ rival × 3` | 20 | 20 | 25 | 20 | 15 |
   | 3 | Gana 2 a 1 o más | `mias ≥ rival × 2` | 20 | 25 | 15 | 15 | 25 |
   | 4 | Gana 4 a 3 o más | `mias × 3 ≥ rival × 4` | 20 | 25 | 15 | 20 | 20 |
   | 5 | Normal | ninguna de las anteriores | 20 | 20 | 20 | 20 | 20 |

   Cada tabla base tiene un **id fijo en la config** (en el orden de la tabla: `pierde_medio`, `pierde_tres_cuartos`, `gana_dos_a_uno`, `gana_cuatro_a_tres`, `normal`); la Librería lo devuelve con `ProbabilidadesSiguienteTirada` y la Interfaz (#17) lo muestra con un nombre neutro. Las cinco filas suman 100. El orden importa solo dentro de cada lado (1 implica 2 y 3 implica 4); los dos lados no pueden cumplirse a la vez. Las cifras son **PROVISIONALES**: vienen de la v3 y se ajustan con partidas del MVP (Tuning Knobs).
5. **Tabla efectiva: `estado_cara` y los dos repartos.** Sobre la tabla base elegida, la Librería consulta `estado_cara(J, c)` (Reclutamiento, F3) para cada cara de reclutar `c`, con el tablero tal como está al abrir el turno, y aplica según el resultado:

   | `estado_cara` | Qué le pasa a su probabilidad | Reparto que se usa |
   |---|---|---|
   | Disponible | Queda como en la tabla base | Ninguno |
   | SinLugar (tope de piezas vivas o casillas del anillo) | Pasa a 0 | `reparto_sin_lugar` (Reclutamiento, F5): `Q` se reparte por igual entre Grito, Arremetida y la otra cara de reclutar si está Disponible |
   | Agotada (2 tramos rotos o sin llamadas) | Pasa a 0 | `reparto_liberado` (Librería): `L` va mitad a Grito y el resto, con el punto impar, a Arremetida |

   Los dos repartos se calculan desde la tabla base y se suman, así que el orden no importa. **Distracción nunca recibe nada** (Reclutamiento, decisión 5). Como último paso, si el jugador tiene `piezas_sin_rey ≤ limite_normal` (hoy 12), Arremetida no le agrega nada y su probabilidad entera pasa a Grito (jugador, 2026-11-24). La tabla efectiva suma 100, se valida como en `CrearPartida` y es la única que se usa para tirar. Este GDD no redefine los dos repartos: solo fija **cuál se aplica a cada estado** y con qué datos.
6. **Cuándo se calcula y cuándo se congela.** La tabla efectiva se calcula una sola vez al entrar en `EsperandoTirada` y queda guardada hasta la `Tirada`: en ese lapso no se mueve ninguna pieza ni se rompe ningún muro. `ProbabilidadesSiguienteTirada(estado, jugador)` (Librería, regla 6) devuelve la guardada para el jugador del turno y, para el otro, la que tendría si su turno empezara con el tablero tal como está.
7. **Grito de guerra: bonos (confirmado por el jugador, 2026-11-25).**
   - **+2 de ataque y +2 de movimiento** (`bono_grito_ataque`, `bono_grito_movimiento`: campos de la config, Librería, regla 2).
   - Alcanza a **todas** las piezas vivas del jugador del turno, **el Rey incluido** y las que están sobre el muro, al consultar `EffectiveStats`.
   - **`movimiento`** efectivo = base + 2 para todas, sin excepción (la catapulta y el demoledor también).
   - **`ataque`** efectivo = base + 2 para todas **menos el demoledor** (modo Explosión; Alcance, #7, regla 5). La **catapulta** lo recibe en la casilla de impacto (10 → 12), pero su `danoArea` sigue en 6: ni el área ni la explosión reciben bono (jugador, 2026-11-25). Se mantiene el invariante I3 del Catálogo (pilar 2) y Resolución (#9) no necesita un caso especial: lee `EffectiveStats`.
   - No cambia escudo, vida ni alcance (Alcance, regla 3).
   - Dura **solo ese turno**: la Máquina borra los bonos al cerrarlo (Máquina, regla 11). No se acumula: hay una cara por turno, y volver a escribirlo deja 2, no 4.
   - Suma también al daño contra muros (Castillo, F3).
8. **Distracción.** `SaltearMovimiento`: se abre `Atacar` directo. No hay `Mover` ni `Deshacer` ese turno, y la etapa del timer es solo la de atacar. El jugador ataca, asigna y cubre con lo que ya tiene posicionado. Si no hay acción posible en `Atacar`, la Máquina salta la fase y cierra el turno (Máquina, regla 10); con una catapulta o un demoledor propios vivos `Atacar` nunca se salta (Alcance, regla 14). Es la excepción vigente al anti-pilar «NOT un dado que decide golpes» y se mantiene por decisión del jugador.
9. **La tirada.** La autoridad envía `Tirada(n, contadorTiradaEsperado)` con `n` de 0 a 99 (en local el cliente, **cuando termina la presentación de la resolución del turno anterior**, que no se salta, o, si el turno se cerró sin resolución, cuando terminan la placa y la leyenda pendientes; Render #18, reglas 24 y 25 (decisión del 2026-11-26); online solo el servidor). La Librería aplica `cara_por_tirada` sobre la tabla efectiva guardada: las caras con 0% nunca salen. El evento de la tirada lleva `n`, la cara y la tabla usada (Librería, AC-8) y **además el nombre de la tabla base elegida** (Librería, regla 8). Una tirada no se repite ni se deshace.
10. **Panel de probabilidades («?», pilar 3b).** Los dos jugadores pueden consultar en cualquier momento la tabla efectiva de ambos. Debe mostrar la efectiva y no la base, y el motivo de cada cara en 0% o redistribuida (sin muros, sin llamadas, sin tope, sin lugar, 12 piezas o menos). La disposición es de la Interfaz del Modificador (#17).
11. **Rendirse comparte el umbral.** El jugador del turno ve `Rendirse` mientras cumple la condición 1 de la regla 4 (`mias × 2 ≤ rival`), evaluada con el estado actual. Hay una sola definición de «la mitad de las piezas del rival o menos» y es esta.
12. **Enmienda del pilar 3a (decisión del jugador, 2026-11-25).** El Grito da estadísticas y la redacción vigente del 3a dice *«nunca estadísticas»*. El jugador decidió **enmendar el pilar**, no ampliar la excepción. Texto propuesto para el test 3a: *«(a) da opciones (más chances de reclutar) y nunca estadísticas permanentes ni escudo. El único bono de daño admitido es el Grito de guerra: +2 de ataque (sin daño de área ni explosión) y de movimiento durante un turno, igual para los dos jugadores, visible en el «?» y en la vista previa; el que va perdiendo solo lo recibe más seguido como parte de la probabilidad que una cara de reclutar o Arremetida no puede usar, nunca por ir perdiendo.»* **Aplicada en `game-concept.md` el 2026-11-25**, al cerrar la tanda 2. **Prueba operativa:** en las cinco tablas base, el Grito de las tablas «Pierde» es menor o igual que el de las tablas «Gana» (hoy 20 contra 25).
13. **El Modificador no toca** piezas, muros, topes ni llamadas, ni mueve nada del tablero: lee estado y escribe solo la cara del turno y los bonos.

**El jugador no puede:** elegir ni relanzar la cara, rechazarla, guardarla para otro turno, ver la tirada antes de que se aplique ni cambiar la tabla de su turno una vez abierto.

### States and Transitions

El Modificador no guarda estado propio entre turnos: lo que existe en cada turno es la tabla base elegida, la tabla efectiva, la cara del turno y los bonos (Librería, campos del estado).

| Estado | Cuándo | Qué existe |
|---|---|---|
| Sin tirada | Primer turno de la partida | Nada: no hay cara ni bonos |
| Esperando tirada | Turno desde el segundo, al abrirlo (Máquina, `EsperandoTirada`) | Tabla base y tabla efectiva guardadas; cara vacía |
| Resuelta | `Tirada` válida | Cara del turno, resultado tipado entregado a la Máquina y, con Grito, `bonoAtaque` y `bonoMovimiento` |
| Cerrada | Cierre del turno (Máquina, regla 11) | Cara y bonos se limpian; la tabla del turno siguiente se calcula al abrirlo |

| De | A | Cuándo |
|---|---|---|
| (creación) | Sin tirada | `CrearPartida` |
| Cerrada o Sin tirada | Esperando tirada | Se abre el turno T ≥ 2 |
| Esperando tirada | Resuelta | `Tirada` con `n` de 0 a 99 y contador esperado |
| Esperando tirada | Esperando tirada | `Tirada` rechazada (`TiradaInvalida`, contador viejo): nada cambia |
| Resuelta | Cerrada | Cierra el turno (`Resolver`, timeout o salto automático) |
| Cualquiera | Terminada | `Rendirse` o `CerrarPartida`; en `EsperandoTirada` se rechaza `RelojAgotado` (Máquina, regla 14) |

No hay transición Resuelta → Esperando: una tirada nunca se repite ni se pide en medio de `Resolver`.

### Interactions with Other Systems

| Sistema | Entra al Modificador | Sale del Modificador | Dueño de la interfaz |
|---|---|---|---|
| #1 Librería | Config (tablas base con papeles, bonos del Grito), `cara_por_tirada`, tabla efectiva, comando `Tirada` | Las 5 tablas, el orden de elección y las cifras de los bonos | Librería (contrato); #11 (datos y regla de elección) |
| #3 Máquina | Abre `EsperandoTirada`, pide la tirada, borra los bonos al cerrar | Resultado tipado y bonos en el estado | Máquina define los tipos; #11 los produce |
| #10 Reclutamiento | `estado_cara(j, c)`, `llamadas_restantes(j, t)`, `reparto_sin_lugar` | Nada: el Modificador solo lee | Reclutamiento |
| #5 Castillo y muros | `puedeReclutar`, vía `estado_cara` (Agotada con 2 tramos rotos) | — | Castillo |
| #6 Movimiento | Lee `movimiento` efectivo y el límite del turno | `bonoMovimiento`, límite sin tope | Movimiento |
| #7 Alcance y asignación | Modo de ataque por tipo (regla 5): decide quién recibe el +2 de ataque | `bonoAtaque`; el Grito no cambia alcance | Alcance |
| #9 Resolución | Lee `ataque` efectivo vía `EffectiveStats` | `bonoAtaque` para todo modo menos Explosión | Resolución (pendiente) |
| #2 Catálogo | `ataque`, `movimiento` base | — | Catálogo |
| #12 Timer | Etapa actual del turno | Con Distracción no hay etapa de mover | Timer |
| #15 HUD, #16 Vista previa, #17 Interfaz del Modificador | — | Cara, bonos, tabla efectiva de los dos jugadores y motivo de cada 0% (solo lectura) | Cada GDD |
| #18 Render | — | Evento `Tirada` para el ambiente; en local, la `Tirada` sale al terminar su presentación | Render |
| #19 Métricas, #22 Audio, #24 Tutorial, #25 Rival IA, #28 Servidor | — | Evento de la tirada, consulta pura, tablas | Cada GDD |

**Supuestos sobre GDD vecinos:** confirmados al cerrar la tanda 3 (2026-11-25): Resolución (#9) lee el ataque solo por `EffectiveStats` y no suma bonos por su cuenta; la Interfaz del Modificador (#17) muestra el motivo de cada cara en 0%.

## Formulas

Todo lo que corre en el núcleo es aritmética entera (Librería, contrato: sin `float`). Las fórmulas F1 a F3 corren en el núcleo; F4 y F5 son de diseño para Métricas (#19) y no corren en él. La conversión de la tirada en una cara (`cara_por_tirada`), `reparto_liberado`, `reparto_sin_lugar` y `estado_cara` ya tienen dueño (Librería y Reclutamiento) y se **usan sin redefinirlas**.

### F1. Tabla base

The `tabla_base` formula is defined as:

`tabla_base(mias, rival) = la primera de {Pierde 1/2, Pierde 3/4, Gana 2 a 1, Gana 4 a 3} cuya condición se cumple; si ninguna, Normal`

con las condiciones `mias × 2 ≤ rival` · `mias × 4 ≤ rival × 3` · `mias ≥ rival × 2` · `mias × 3 ≥ rival × 4`, en ese orden (regla 4).

**Variables:**

| Variable | Type | Range | Source | Description |
|----------|------|-------|--------|-------------|
| `mias` | int | 1–61 | calculated (estado) | Piezas vivas del jugador del turno, con el Rey y sin muros |
| `rival` | int | 1–61 | calculated (estado) | Ídem, del rival |
| `p_cara,tabla` | int | 0–100 | data file (config) | Probabilidad de cada cara en cada una de las 5 tablas (regla 4); cada tabla suma 100 |
| 61 | int | — | constant (Catálogo, `tope_total`) | Máximo de piezas vivas por jugador, Rey incluido |

**Output Range:** una de las cinco tablas. **Precondición:** `mias ≥ 1` y `rival ≥ 1` (los dos Reyes están vivos al abrir cada turno; si no, `EvaluateMatchEnd` ya cerró la partida, Máquina regla 13). Si no se cumple es `ErrorInterno` (inalcanzable). El producto máximo es 61 × 4 = 244: no desborda.
**Límites:** (1, 1) → Normal. (1, 2) → Pierde 1/2. (2, 1) → Gana 2 a 1. (61, 1) → Gana 2 a 1. (1, 61) → Pierde 1/2.
**Example:** 6 contra 13: 12 ≤ 13 → **Pierde 1/2**. 7 contra 13: 14 ≤ 13 no, 28 ≤ 39 sí → **Pierde 3/4**. 9 contra 12: 36 ≤ 36 → Pierde 3/4; 10 contra 12: Normal. 13 contra 6: 13 ≥ 12 → **Gana 2 a 1**. 4 contra 3: 12 ≥ 12 → Gana 4 a 3.

### F2. Tabla efectiva (composición de los dos repartos)

The `tabla_efectiva` formula is defined as:

`tabla_efectiva = tabla_base, con cada cara de reclutar c ≠ Disponible puesta en 0; L = Σ p_c de las Agotadas; Q = Σ p_c de las SinLugar; R = [Grito, otra cara de reclutar Disponible (si hay), Arremetida]; m = |R|; Grito += ⌊L/2⌋; Arremetida += L − ⌊L/2⌋; cada cara de R += ⌊Q/m⌋ y las primeras Q mod m de R, en ese orden, +1; al final, si piezas_sin_rey ≤ limite_normal: Grito += Arremetida y Arremetida = 0`

**Variables:**

| Variable | Type | Range | Source | Description |
|----------|------|-------|--------|-------------|
| `p_c` | int | 0–100 | data file (tabla base de F1) | Probabilidad base de la cara de reclutar `c` |
| `estado_cara(J, c)` | enum | Agotada / SinLugar / Disponible | calculated (#10, F3) | Estado de la cara al abrir el turno |
| `L` | int | 0–50 (con los datos vigentes; 0–100 por contrato de la Librería) | calculated | Porcentaje liberado por caras Agotadas (`reparto_liberado`, dentro de `TablaEfectiva` de la Librería) |
| `Q` | int | 0–50 (con los datos vigentes; 0–100 por contrato) | calculated | Porcentaje liberado por caras SinLugar (`reparto_sin_lugar`, #10, F5) |
| `m` | int | 2–3 | calculated | Caras que reciben `Q`: siempre Grito y Arremetida, más la otra de reclutar si está Disponible |
| `piezas_sin_rey` | int | 0–60 | calculated (estado) | Piezas vivas del jugador sin el Rey y sin muros |
| `limite_normal` | int | 1–37 (hoy 12) | data file (config; dueño #6) | Límite normal de piezas que se mueven por turno, sin el Rey |

**Output Range:** una tabla de 5 caras que suma exactamente 100: todo `p_c` anulado se suma entero a `L` o a `Q` (una cara tiene un solo estado), `Q` se reparte completo (`m × ⌊Q/m⌋ + Q mod m = Q`) y `L` también. `m ≥ 2` evita la división por cero. **Distracción nunca cambia** (15 o 20 según la tabla base). Con las cifras vigentes: **Grito** entre 20 y 85 (85 solo con Pierde 1/2, 12 piezas o menos y las dos caras de reclutar bloqueadas); **Arremetida** entre 0 y 40 (0 con 12 piezas o menos; 40 como máximo con 13 o más).
**Example** (Distracción / Grito / Llamada / Cuerno / Arremetida):

| Caso | Tabla efectiva |
|---|---|
| Pierde 1/2, 13 sin el Rey, todo Disponible | 15/20/25/25/15 |
| Normal, 12 sin el Rey (apertura), todo Disponible | 20/40/20/20/0 |
| Gana 4 a 3, 13 sin el Rey, Llamada Agotada (`L` = 15 → 7 y 8) | 20/32/0/20/28 |
| Pierde 1/2, 13 sin el Rey, 2 muros rotos (`L` = 50) | 15/45/0/0/40 |
| Pierde 1/2, 12 sin el Rey, 2 muros rotos | 15/85/0/0/0 |
| Pierde 3/4, 13 sin el Rey, Llamada SinLugar y Cuerno Disponible (`Q` = 25, `m` = 3 → 9, 8, 8) | 20/29/0/28/23 |
| Pierde 1/2, 13 sin el Rey, Llamada Agotada y Cuerno SinLugar (12 y 13 de `L`; 13 y 12 de `Q`) | 15/45/0/0/40 |

Las filas 2 a 5 coinciden con los ejemplos de la Librería (AC-40) y la 7 con Reclutamiento (F5). La 1 es la base sin cambios y la 6 es propia de este GDD (F5 no tiene ejemplo con «Pierde 3/4»).

### F3. Estadísticas efectivas con el Grito

The `estadisticas_con_grito` formula is defined as:

`movimiento_ef(p) = movimiento_t + [cara = Grito] × bono_grito_movimiento` · `ataque_ef(p) = ataque_t + [cara = Grito Y modo(p) ≠ Explosión] × bono_grito_ataque`

El resto de `EffectiveStats` (bonos de posición del arquero sobre el muro, Castillo, F5) se suma aparte y no cambia.

**Variables:**

| Variable | Type | Range | Source | Description |
|----------|------|-------|--------|-------------|
| `movimiento_t`, `ataque_t` | int | 1–1.000 | data file (Catálogo) | Valores base del tipo `t` de la pieza `p` |
| `[cara = Grito]` | 0 o 1 | — | calculated (estado) | 1 solo durante el turno de Grito del jugador de la pieza |
| `modo(p)` | enum | Directo / Área / Explosión | data file (Catálogo, vía Alcance #7, regla 5) | Modo de ataque; el de Explosión no recibe el bono de ataque |
| `bono_grito_movimiento` | int | 0–10 (hoy 2) | data file (config) | Bono de movimiento |
| `bono_grito_ataque` | int | 0–10 (hoy 2) | data file (config) | Bono de ataque |

**Output Range:** `movimiento_ef` y `ataque_ef` de 1 a 1.010 (Catálogo, AC-29; `movimiento_restante` hasta 1.010 en Movimiento). Con bono 0 no cambia nada. `danoArea`, escudo, vida y alcance no entran: no cambian nunca.
**Example:** arquero sobre el muro: ataque 6 → 8 y movimiento 4 → 6 (Castillo, AC-9). Jinete: movimiento 6 → 8 (Movimiento). Catapulta: ataque 10 → **12** en el impacto, `danoArea` 6 → **6**, movimiento 2 → 4. Demoledor: ataque 30 → **30** (Explosión), movimiento 6 → 8. Rey: movimiento 6 → 8.
**Cota de la pila de deshacer (Movimiento, F5):** con `limite_normal` 37 y bono de movimiento 2 la pila llega a 254 de 256, así que `bono_grito_movimiento` **no puede subir** mientras `limite_normal` sea 37; con 12 el peor caso es 104 (con el bono máximo de 10, 208). La Librería rechaza con `ConfigInvalida` la config cuya cota con bono pase de 256 (regla 2, 2026-11-25).

### F4. Aporte de Distracción a la duración (Δturnos, pilar 4)

The `aporte_distraccion` formula is defined as:

`E_D = ⌊N × p_D / 100⌋` · `Δturnos_D ≤ E_D` · `−E_D × (t_turno − t_atacar) ≤ Δs_D ≤ E_D × t_atacar`

*(fórmula de diseño para Métricas; no corre en el núcleo)*

**Variables:**

| Variable | Type | Range | Source | Description |
|----------|------|-------|--------|-------------|
| `N` | int | 26–37 | calculated (Máquina, D1) | Turnos de la partida entre los dos jugadores, a 57 y a 40 s por turno |
| `p_D` | int | 15–20 | data file (tablas base) | Probabilidad de Distracción (nunca cambia por reparto) |
| `E_D` | int | 3–7 | calculated | Turnos con Distracción esperados en una partida |
| `Δturnos_D` | int | 0–`E_D` | calculated (Métricas, #19) | Turnos que Distracción agrega a la partida: cota superior = un turno por cada una |
| `t_turno` | int (s) | 40–57 | calculated (Métricas, #19) | Duración media de un turno con movimiento |
| `t_atacar` | int (s) | 1–30 | data file (Timer, #12; tope de la etapa de atacar) | Duración media de un turno sin fase de mover |

**Output Range:** `Δturnos_D` de 0 a 7 y `Δs_D` ilustrativo de −135 s a +210 s (con `t_atacar` = 30 s), contra los 1.500 s del techo. `E_D` es una **esperanza**, no un máximo: una partida puede tener más Distracciones, y con `t_atacar` menor el extremo inferior baja más. **NOT SOURCEABLE — `t_atacar` y `Δturnos_D` no están medidos:** este GDD declara la fórmula y la cota, no el valor; con `t_atacar` en su tope de 30 s la cota superior se calcula, no se mide.
**Example (ilustrativo, sin medir):** N = 26 (57 s por turno), `p_D` 20 → `E_D` = 5; cota superior +5 × 30 = **+150 s** (10% del techo); inferior −5 × (57 − 30) = −135 s. N = 37 (40 s), `p_D` 20 → `E_D` = 7 → +210 s (14%). La cota superior no rompe el techo de 25 minutos si la partida sin Distracción promedia 21,5 minutos o menos.
**Quién la sufre más:** la tabla «Pierde con 1/2» tiene Distracción en 15% y «Gana 2 a 1» en 20%: el que va ganando la saca 5 puntos más seguido, un freno suave y no buscado.

### F5. Ritmo de remontada y umbral del pilar 3c

The `ritmo_remontada` formula is defined as:

`ritmo_x100(tabla) = n_max × (p_Llamada + p_Cuerno)` · `brecha_x100 = ritmo_x100(tabla del que pierde) − ritmo_x100(tabla del que gana)` · `W = victorias del líder inicial / partidas con líder inicial ≥ 2/3`

*(fórmula de diseño; no corre en el núcleo)*

**Variables:**

| Variable | Type | Range | Source | Description |
|----------|------|-------|--------|-------------|
| `n_max` | int | 1–17 (hoy 4) | data file (config, #10) | Piezas por llamada, tomando el máximo; cota superior |
| `p_Llamada`, `p_Cuerno` | int | 0–100 | calculated (F1) | Probabilidad de las caras de reclutar en la tabla base |
| `ritmo_x100` | int | 0–1.700 | calculated | Piezas nuevas esperadas por turno × 100, como cota superior |
| `líder inicial` | jugador | — | calculated (Métricas, #19) | Quien cumple primero la condición «Gana 4 a 3 o más» (F1) al abrir su turno, o el rival de quien primero cumple «Pierde 3/4 o menos»; se mide con el primer evento de la tirada cuya tabla base no es Normal (Métricas, F3) |
| `W` | int (%) | 0–100 | calculated (Métricas, #19) | Parte de las partidas terminadas que gana el líder inicial |

**Output Range:** con las tablas vigentes `ritmo_x100` vale 120 (Gana 2 a 1), 140 (Gana 4 a 3), 160 (Normal), 180 (Pierde 3/4) y 200 (Pierde 1/2). La brecha entre extremos es de 80 (0,8 piezas por turno, hasta 11 en 14 turnos como cota; el tope de 48 reclutas por jugador manda, Reclutamiento F6). **Umbral del pilar 3c (jugador, 2026-11-25): `W` ≥ 2/3.** **NOT SOURCEABLE — `W` no está medida:** con 20 partidas el error estándar es de unos 10,5 puntos, así que las primeras 20 orientan y no deciden.
**Example:** 13 de 20 partidas ganadas por el líder inicial → W = 65%, bajo el umbral: seguir midiendo antes de tocar tablas.

### Lazos de retroalimentación

- **Negativo (buscado):** perder piezas inclina la tabla hacia reclutar, lo que devuelve piezas y vuelve a una tabla normal. Ganar baja la chance de reclutar. Cruzar un umbral mueve cada cara **5 puntos como máximo**, unas 0,2 piezas esperadas por turno a cambio de la pieza; como el efecto se repite mientras dure la tabla, que no rinda está sin medir. **Excepción:** bajar de 13 a 12 piezas sin el Rey pasa toda Arremetida (15 a 40 puntos) a Grito de una vez.
- **Positivo, amortiguado:** cortar el propio castillo o llenar el propio anillo sube Grito y Arremetida a cambio de perder reclutas (Open Questions). Lo frena el costo: el castillo queda expuesto y la cara que se pierde era la remontada.
- **Positivo, no amortiguado por el Modificador:** el que va ganando recibe menos reclutas, pero también un +5 de Grito. Se mide con `W`.

### Arrastre a otros GDD

Fijar el bono en +2 con exclusión del área y la explosión levanta provisionales en Movimiento (F5, cota de pila, AC-21), la Librería (`limite_normal`, `C_MAX`, bono, AC-12b, AC-40, preguntas sobre el 85% y sobre el pilar 3a), el Catálogo (pregunta del bono sobre `danoArea`), Castillo (F3) y Reclutamiento (Open Questions). La lista completa está en Cross-References.

## Edge Cases

### Tirada y fases

- **If llega `Tirada` con `n` fuera de 0–99:** `TiradaInvalida`; el estado no cambia y sigue esperando (Librería, regla 6).
- **If llega `Tirada` con un contador distinto del esperado:** se rechaza sin cambiar el estado (Librería, regla 3); no se aplica dos veces la misma tirada.
- **If llega `Tirada` fuera de `EsperandoTirada`:** `TiradaNoEsperada` (Máquina, regla 17).
- **If `n` = 0 y la primera cara tiene 0%:** sale la primera cara con probabilidad mayor que 0 (`cara_por_tirada`). Con `n` = 99 pasa lo mismo hacia el otro lado: una cara con 0% **nunca** sale.
- **If es el primer turno de la partida:** no hay tirada ni bono (regla 2). El panel «?» igual muestra las dos tablas hipotéticas, calculadas con el tablero inicial.
- **If el jugador usa `Rendirse` o llega `CerrarPartida` durante `EsperandoTirada`:** la partida termina sin tirada. `RelojAgotado` se rechaza en esa fase: el jugador no paga la demora de la autoridad (Máquina, regla 14).
- **If la autoridad no envía la tirada (online):** la partida queda en `EsperandoTirada`; el reloj del jugador no corre y no hay etapa del timer. Qué pasa con una autoridad caída lo decide el Servidor (#28); queda como pregunta suya (Open Questions).

### Tablas y estado

- **If las dos caras de reclutar están bloqueadas y el jugador tiene 12 piezas o menos:** el Grito llega a 85% con «Pierde 1/2» y a 80% con las otras tablas. Distracción conserva su 15 o 20% porque nunca recibe lo liberado, así que el Grito nunca llega a 100%.
- **If los dos jugadores tienen la misma cantidad de piezas:** tabla Normal. Con 13 contra 13 (12 sin el Rey) la apertura es 20/40/20/20/0.
- **If el jugador tiene solo el Rey y el rival 2 o más piezas:** «Pierde 1/2» y `Rendirse` visible. Con las dos caras de reclutar Disponibles la tabla efectiva es 15/35/25/25/0.
- **If una pieza propia muere por fuego amigo o se rompe un tramo durante el turno propio:** la tabla de ese turno ya estaba congelada y la tirada ya salió; no cambia. El turno siguiente del jugador calcula con el tablero nuevo (Castillo: el corte rige desde su siguiente turno).
- **If los tramos se rompen en el turno del rival:** la tabla del dueño se calcula al abrir su turno y ya incluye el corte (Castillo).
- **If el jugador recluta durante su turno:** su tabla del turno no cambia, pero la tabla *hipotética* del rival que muestra el panel sí, porque `mias` cambió.
- **If el jugador sacrifica piezas (fuego amigo, explotar demoledores propios) para cruzar un umbral de tabla:** es legal. Cruzar un umbral mueve cada cara 5 puntos como máximo (unas 0,2 piezas esperadas por turno), a cambio de al menos una pieza. Bajar de 13 a 12 sin el Rey, en cambio, pasa de 15 a 40 puntos de Arremetida a Grito. Si rinde está sin medir. Alcance (#7) ya lo marcó para los demoledores; se vigila con Métricas (Open Questions).

### Caras

- **If sale Grito de guerra y ninguna pieza puede moverse ni atacar:** el bono no tiene efecto; la Máquina salta las fases sin acción y cierra el turno (Máquina, regla 10).
- **If hay timeout de la etapa de mover con el Grito activo:** los bonos siguen vigentes hasta cerrar el turno (Máquina, regla 11), así que valen en `Atacar`.
- **If hay Grito con una catapulta o un demoledor propios:** los dos reciben +2 de movimiento; la catapulta pega 12 en la casilla de impacto y 6 alrededor; el demoledor explota con 30 (regla 7). La vista previa muestra el ataque efectivo.
- **If sale Arremetida con más de 12 piezas:** no hay tope de piezas que se mueven; la pila de deshacer llega a 254 de 256 como máximo, con el ejército lleno y sin bono (Movimiento, F5). Con 12 piezas o menos no sale nunca (0%).
- **If sale Distracción y ninguna pieza tiene un objetivo posible** (sin catapulta ni demoledor propio vivos y sin escudero que pueda cubrir): `Atacar` se salta y el turno se cierra vacío (`FaseSaltada` y `TurnoTerminado`, Máquina, regla 10). La cara se gasta y no se vuelve a tirar: es el rincón donde Distracción sigue haciendo perder el turno. Se acepta y se mide (Open Questions).
- **If la config trae los dos bonos del Grito en 0:** `ConfigInvalida` en `CrearPartida`: sería una cara que no hace nada (Librería, regla 2). Un solo bono en 0 es válido.
- **If la config trae un bono de movimiento que hace pasar la pila de 256:** `ConfigInvalida` (Librería, regla 2).
- **If una tabla base tiene 100 en una cara o una Distracción alta:** es válida para la Librería (cada cara de 0 a 100 y suma 100) pero queda fuera del rango seguro de diseño (Tuning Knobs).
- **If `limite_normal` es alto (37):** Arremetida solo puede salir con más de 37 piezas y casi nunca sale: su porcentaje pasa a Grito en casi toda partida. La cara sigue en la tabla (Tuning Knobs).

### Determinismo y autoridad

- **If se repite el mismo estado con la misma `n`:** sale la misma cara con los mismos bonos, sin excepción (Librería, determinismo).
- **If el cliente online recibe la cara:** puede verificar que corresponde a la tabla pública y a `n`; el compromiso verificable de las tiradas es del Servidor (#28, Librería, Open Questions).
- **If una pieza muere en la misma resolución en que golpea:** golpea igual y con el `ataque_ef` del turno (Librería, regla 15). Confirmado por Resolución (#9).

## Dependencies

**Depende de:**

| Sistema | Tipo | Qué usa |
|---|---|---|
| #1 Librería de reglas | Dura | Config (tablas base con papeles, bonos), `cara_por_tirada`, tabla efectiva, `reparto_liberado`, `ProbabilidadesSiguienteTirada`, comando `Tirada` |
| #3 Máquina de turno | Dura | Fase `EsperandoTirada`, resultados tipados, borrado de bonos al cerrar el turno |
| #10 Reclutamiento y límites | Dura | `estado_cara`, `llamadas_restantes`, `reparto_sin_lugar` (F5) |
| #5 Castillo y muros | Dura | `puedeReclutar` (a través de `estado_cara`: 2 tramos rotos = cara Agotada) |
| #7 Alcance y asignación | Dura | Modo de ataque (regla 5): decide qué piezas reciben el +2 de ataque |
| #2 Catálogo de piezas | Blanda | `ataque` y `movimiento` base, `topeVivas` (indirecto); techo `bono_grito_ataque ≤ 2` (I1, AC-39) y F9 |
| #4 Tablero y despliegue | Blanda | `margen_primer_golpe`: fija el tope 2 de `bono_grito_movimiento` (margen del turno 2) |

**Dependen de este sistema:**

| Sistema | Tipo | Qué usa |
|---|---|---|
| #6 Movimiento | Dura | `bonoMovimiento` y el límite del turno, vía `EffectiveStats` |
| #9 Resolución simultánea | Dura | `ataque` efectivo, vía `EffectiveStats` (GDD desde la tanda 3) |
| #15 Ficha de pieza y HUD | Dura | Cara del turno, bonos activos y estadística efectiva |
| #16 Vista previa de ataques | Dura | Ataque efectivo con el bono del Grito |
| #17 Interfaz del Modificador | Dura | Tablas de los dos jugadores, motivos de cada 0% y las elecciones |
| #18 Render, animación y feedback | Blanda | `Tirada` (ambiente); el envío local de la `Tirada` espera a Render |
| #19 Métricas locales | Dura | Evento de la tirada, tabla base y efectiva (F4, F5); existe desde la tanda 4 |
| #24 Tutorial, #25 Rival IA | Dura | Consulta pura de probabilidades y resultado tipado |
| #28 Servidor autoritativo | Dura | Envío de la tirada y compromiso verificable |
| #22 Audio y SFX | Blanda | Una señal sonora por cara |
| #14 Localización | Blanda | IDs de las cinco caras y motivos de cada 0% |
| #12 Timer | Blanda | Sin cambios si lee la etapa de la Máquina (Distracción no abre la etapa de mover) |

**Bidireccionalidad.** Ya nombran al Modificador en sus tablas: la Librería, la Máquina, Reclutamiento, Castillo, Movimiento y Localización. Alcance, Catálogo, Tablero y Timer lo nombran desde el 2026-11-25. Resolución (#9), HUD (#15) e Interfaz del Modificador (#17) tienen GDD desde la tanda 3 y nombran al Modificador. Vista previa (#16), Render (#18) y Métricas (#19) tienen GDD desde la tanda 4; la arista Dura con #19 queda confirmada.

## Tuning Knobs

Todas las perillas de datos son `data file` (config) y entran a la huella. Los rangos seguros son de diseño y **no están medidos**; las cifras vigentes vienen de la v3 y se ajustan con partidas del MVP.

| Perilla | Valor | Rango seguro | Si es muy alta | Si es muy baja | Interactúa con |
|---|---|---|---|---|---|
| Probabilidad de Distracción (tablas) | 15 a 20 | 10–25 | Más turnos sin mover: partida más larga (F4) y más cerca de un dado que decide | La excepción vigente queda decorativa | F4, Grito y Arremetida (reciben lo liberado) |
| Probabilidad de cada cara de reclutar (tablas) | 15 a 25 | 10–30 | Remontada demasiado fuerte: el que va ganando deja de ganar (3c) y partidas más largas (pilar 4) | Sin remontada (pilar 3); el tope de llamadas nunca aprieta | F5, `llamadas_max` (#10), `topeVivas` (#2) |
| Probabilidad de Grito (tablas) | 20 a 25 | 15–30, con Grito de «Pierde» ≤ Grito de «Gana» | Más turnos con +2 en todo el ejército | El reparto de lo liberado lo sube igual | Prueba operativa del 3a (regla 12), `reparto_liberado` |
| Probabilidad de Arremetida (tablas) | 15 a 25 | 10–30 | La pila de deshacer crece; casi sin efecto si `limite_normal` es alto | Cara casi ausente | `limite_normal` (#6), pila de 256 |
| `bono_grito_ataque` | 2 | 1–2 (la Librería admite 0–10; los dos en 0 se rechazan). Con 3, soldado, jinete y Rey pegan 11 y matan solos al demoledor: rompe I1 del Catálogo (AC-39, 2026-11-27) | El Grito decide golpes (anti-pilar); mueve la tabla F3 de Castillo; con 3 o más rompe I1 del Catálogo | 0 con el otro bono en 0: `ConfigInvalida` | Castillo F3, Catálogo (I2 no aplica al demoledor) |
| `bono_grito_movimiento` | 2 | 1–2 con `limite_normal` 37; no pasar de 2 por el margen del turno 2 (primera tirada, de rojo: `margen_primer_golpe` del Tablero). Es una restricción del rango seguro, sin guarda en ejecución: la Librería acepta 0–10 y F9 del Catálogo (`movimiento + bono ≤ 12`) deja el máximo en 6 con el Catálogo vigente | La pila pasa de 256 (`ConfigInvalida`); cada punto resta uno al margen del turno 2 del Tablero (con +2 queda en 1, con +3 en 0) | 0 con el otro bono en 0: `ConfigInvalida` | `limite_normal` (#6), Tablero (#4), Movimiento F5 |
| Umbrales de F1 (1/2, 3/4, 4 a 3, 2 a 1) | constantes de la regla, no config | No ajustables hoy | Cerca de 1: casi siempre una tabla inclinada | Lejos de 1: la adaptación casi nunca actúa (pilar 3) | F5, `Rendirse` (comparte el 1/2) |
| Umbral del pilar 3c (`W`) | 2/3 | Objetivo de diseño, no valor de juego | — | — | F5, Métricas (#19) |
| Muestra para decidir | 20 partidas | 20–60 | — | Con 20, error estándar de unos 10,5 puntos | Métricas (#19) |

**Perillas que interactúan:** `limite_normal` alto vuelve inerte la probabilidad de Arremetida (su parte va a Grito con 37, salvo con más de 37 piezas). El tope de llamadas de Reclutamiento y la probabilidad de reclutar se calibran juntos (F6 de Reclutamiento: con 14 tiradas por jugador, el tope de 6 Llamadas se alcanza el 4% de las veces al 20% y el 11% al 25%; **se mantiene en 6** como seguro contra partidas largas y se mide en Métricas). Pasar los umbrales de F1 a la config exige un cambio de la Librería (huella y validación).

**De otro GDD (no se duplican):** `llamadas_max` y `n_t` (#10), `topeVivas` (#2), `TRAMOS_ROTOS_CORTE` (#5), `limite_normal` (#6), tiempos de etapa y reloj de partida (#12 y #3).

## Visual/Audio Requirements

*Lente del director de arte, con el art bible (`design/art/art-bible.md`, secciones 1, 2, 4 y 7; las secciones 3 y 5 no se leyeron).*

**1. Feedback visual por evento**

| Evento | Qué se ve | Fuente |
|---|---|---|
| Tirada resuelta | Una placa con el **nombre escrito y un ícono por cara** cae al centro del tablero y se asienta con un golpe seco. Dura ≤ 1,2 s y no tapa una casilla con pieza más de 1 s | Art bible, sección 2 (estado «Tirada del Modificador») y sección 4 |
| Distracción | El estandarte propio del HUD se dobla y cae; placa gris con «Distracción»; terreno desaturado un 40%; ≤ 1,5 s; el turno pasa enseguida a atacar | Art bible, sección 2 |
| Grito de guerra | **Marca de forma sobre cada pieza afectada** (propuesta: un banderín pequeño junto a la ficha), en la capa del sistema: ninguna skin la toca y no depende del color. La ficha muestra el valor efectivo y, si difiere, también la base (Catálogo, requisito de la ficha) | **Falta en el art bible** |
| Grito con catapulta o demoledor | La ficha de la catapulta muestra ataque 12 y área 6; la del demoledor **no** muestra ataque aumentado; las dos muestran el movimiento | Regla 7 |
| Llamada al servicio, Cuerno de guerra | Placa de la cara y después el recuadro central de elecciones (#10, #17) y el resplandor del lado del castillo que mira al rival | Art bible, secciones 2 y 7 |
| Arremetida | Placa de la cara; el contador de piezas movidas deja de mostrar `x/12` y muestra que no hay tope | **Falta en el art bible** |
| Panel de probabilidades | Se abre con el ícono de signo de pregunta, arriba a la derecha (transición ≤ 0,25 s); muestra la tabla efectiva de los dos jugadores | Art bible, sección 7 |

**2. Restricciones de animación y estilo.** La tirada y su placa no se cobran del reloj del jugador (la Máquina no corre ningún reloj en `EsperandoTirada`), pero siguen el pilar 4: ninguna animación del Modificador suma tiempo al turno más allá de los 1,2 s (1,5 s para Distracción). Los marcadores del Grito se animan con el resto de los efectos de la fase, nunca en serie. Cada una de las cinco placas se distingue por **forma o texto, no solo por color** (AC-30).

**3. Principios del art bible que aplican.** *Regla visual* («lo que decide la jugada se lee primero»): el +2 del Grito cambia el daño y tiene que verse antes de resolver. *Principio 1, el plan se lee antes que el detalle:* cada ícono de cara se reconoce en grises. *Principio 3, el ritmo del turno manda sobre la animación.* *Principio 2, el combo se ve como un solo golpe:* el marcador del Grito no compite con la convergencia de flechas.

**Falta en el art bible (a pedir al director de arte):** los cinco íconos de cara con su forma, el marcador del Grito sobre la pieza, la placa de Arremetida y el contador sin tope.

**Audio (para #22, propuesta sin validar con el director de audio).** Una señal sonora por cara que se distinga sin mirar la pantalla, con el golpe seco de la placa como base común. Distracción con un tono caído y breve. Ninguna señal pasa de 1,2 s.

> 📌 **Asset Spec** — Visual/Audio requirements are defined. After the art bible is approved, run `/asset-spec system:modificador` to produce per-asset visual descriptions, dimensions, and generation prompts from this section.

## Game Feel

El jugador no opera el dado, así que la sensación es de **peso y ritmo**, no de respuesta a una entrada.

**Referencia:** el art bible describe la tirada como «breve, central, legible, ceremonial» (energía 3 de 5): un golpe seco que asienta la cara y deja pasar al plan.

| Evento | Qué debe sentirse | Objetivo medible |
|---|---|---|
| Tirada resuelta | Un golpe seco, sin angustia: el azar ya pasó | Placa ≤ 1,2 s; el primer comando válido se acepta apenas la fase abre, sin esperas extra |
| Distracción | Fastidio corto que se cierra rápido, no castigo | ≤ 1,5 s hasta `Atacar` |
| Grito de guerra | El ejército se siente más rápido y con más pegada | El +2 se ve en la ficha y en la vista previa antes de asignar (AC-31) |
| Abrir el «?» | Una consulta inmediata, sin menú | ≤ 0,25 s (transición del art bible) |
| Tirada online | La cara aparece cuando el servidor la decide | Sin predicción de la cara en el cliente; el reloj del jugador no corre mientras espera |

**Lo que no se puede automatizar y va a playtest:** que el dado «no se sienta decisivo» (en la primera partida de la v3, Distracción salió en el 19% de las tiradas y nadie sintió que el dado decidiera) y que el Grito no se perciba como una ventaja injusta del que ya va ganando.

## UI Requirements

El Modificador no tiene una pantalla propia: su interfaz es la placa central, el panel «?» y las marcas sobre las fichas. Las disposiciones son de la Interfaz del Modificador (#17), el HUD (#15) y la Vista previa (#16); este GDD fija **qué datos tienen que mostrar**.

- **Nombre de la cara del turno**, siempre visible durante el turno (texto por Localización, #14; no solo ícono ni color).
- **Panel «?» (pilar 3b):** tabla efectiva de los dos jugadores, cada cara con su porcentaje, y el **motivo** de cada 0% o de cada reparto (sin muros, sin llamadas, sin tope, sin lugar, 12 piezas o menos). Muestra la efectiva, no la base. Para el rival, es la tabla hipotética con el tablero actual.
- **Estadística efectiva en la ficha** (ataque y movimiento con Grito) y la base cuando difiere; la catapulta muestra el ataque aumentado pero no el área, y el demoledor no muestra ataque aumentado.
- **Vista previa con el bono incluido** (#16): el daño que se ve antes de resolver ya suma el +2.
- **Contador de piezas movidas:** con Arremetida muestra que no hay tope.
- **`Rendirse`:** visible solo para el jugador del turno que cumple `mias × 2 ≤ rival`.
- **Accesibilidad:** nada solo por color; textos con espacio para localización; el «?» operable con mouse y táctil.

> **📌 UX Flag — Modificador**: This system has UI requirements. In Phase 4 (Pre-Production), run `/ux-design` to create a UX spec for each screen or HUD element this system contributes to **before** writing epics. Stories that reference UI should cite `design/ux/[screen].md`, not the GDD directly.

## Cross-References

| Documento | Elemento referenciado | Qué toma este GDD |
|---|---|---|
| `libreria-de-reglas.md` | Reglas 2, 3, 6, 10; `cara_por_tirada`, `reparto_liberado`, `ProbabilidadesSiguienteTirada`, AC-8, AC-40 | Config con papeles, tirada inyectada, tabla efectiva, `EffectiveStats` y las dos fórmulas de reparto |
| `maquina-de-turno.md` | Reglas 3, 4, 5, 10, 11, 14 y 17 | Fase `EsperandoTirada`, resultados tipados, borrado de bonos y reloj que no corre esperando la tirada |
| `reclutamiento-y-limites.md` | F3 `estado_cara`, F5 `reparto_sin_lugar`, decisión 5, F6 | Estado de cada cara, reparto de lo bloqueado, tope de 6 Llamadas |
| `castillo-y-muros.md` | F7, F3, AC-9, edge cases de reclutamiento | Corte por 2 tramos rotos; bono del Grito sobre el arquero del muro |
| `movimiento.md` | F5 (cota de pila), regla 11, AC-21 | Bono de movimiento y `limite_normal` |
| `alcance-y-asignacion-de-ataques.md` | Reglas 3, 5 y 14 | Modos de ataque (Directo, Área, Explosión) y que el Grito no da alcance |
| `catalogo-de-piezas.md` | Regla 5, invariante I3, AC-29 | Catálogo estático; el área no recibe el bono |
| `tablero-y-despliegue-inicial.md` | `margen_primer_golpe` (Formulas), margen del turno 2 | Cada punto del bono de movimiento resta uno al margen del turno 2 (primera tirada, de rojo); el turno 1 no tiene tirada |
| `game-concept.md` | Pilares 1 y 3, anti-pilar del dado, filas «Modificador» y «Probabilidades» | Reglas vigentes de las caras y el texto del test 3a a enmendar |
| `design/art/art-bible.md` | Secciones 1, 2, 4 y 7 | Placa del Modificador, banner de Distracción, ícono «?» |
| `systems-index.md` | Fila 11, ciclos y riesgos | Descripción del sistema y la fila de riesgo del Modificador |

**Cambios en GDD vecinos que este documento (aplicados el 2026-11-25 al cerrar la tanda 2):**

- **Librería:** evento de la tirada con el nombre de la tabla base (regla 6 y AC-8); campos de config `bono_grito_ataque` y `bono_grito_movimiento` (0 a 10), con `ConfigInvalida` si los dos son 0 o si la cota de la pila con bono pasa de 256 (regla 2); `EffectiveStats` suma el ataque salvo en modo Explosión (regla 10); levantar los PROVISIONAL de `limite_normal`, `C_MAX`, bono y tablas base (AC-12b, AC-40) y cerrar las dos preguntas abiertas sobre el 85% y el pilar 3a.
- **Movimiento:** confirmar el Grito de +2 con el Rey incluido y levantar el provisional de F5 y de AC-21.
- **Catálogo:** cerrar la pregunta del bono sobre `danoArea` (no lo recibe, ni la explosión; el impacto de la catapulta sí, 10 → 12).
- **Castillo:** nota en F3: el demoledor no recibe el +2 y la catapulta sí (las cifras no cambian: 1 y 3); cerrar la pregunta de la estrategia de cortar el propio reclutamiento hacia este GDD.
- **Reclutamiento:** cerrar las preguntas sobre el tope de 6 Llamadas, el 85% de Grito y los dos repartos; confirmar la arista del #11.
- **Alcance, Catálogo, Tablero y Timer:** agregar el Modificador a su tabla de dependencias (hoy no lo nombran).
- **Máquina:** levantar el provisional de `ReclutarObligatorio` en «Supuestos PROVISIONALES».
- **`game-concept.md`:** aplicar la enmienda del test 3a (regla 12), previa revisión del director creativo, y cerrar «umbral a fijar» del 3c con 2 de cada 3.
- **`systems-index.md`:** estado «Designed» de la fila 11 y la descripción con caras «sin lugar».

## Acceptance Criteria

Tipos de evidencia (`coding-standards.md`): **Logic** e **Integration** son tests automáticos en `tests/unit/` y `tests/integration/` del núcleo (en Unity, `Assets/Tests/EditMode/`); **UI** y **Visual/Feel** exigen una captura retenida en `production/qa/evidence/`. Los criterios marcados *PROVISIONAL* dependen de un GDD sin escribir y se actualizan cuando exista.

### Reglas del Modificador (Sección C)

- **AC-1 (regla 1, papeles)** *(Logic)* — **GIVEN** una config con las cinco caras en el orden de la regla 1, **WHEN** se llama `CrearPartida`, **THEN** se crea. **GIVEN** una config con dos caras de Grito, o sin cara de Arremetida, o con tres caras de reclutar, **THEN** `ProbabilidadesInvalidas`.
- **AC-2 (regla 2, primer turno)** *(Integration, #3)* — **GIVEN** una partida recién creada, **THEN** el jugador 0 está en `Mover`, sin tirada pendiente y con bonos en 0. **WHEN** cierra su turno, **THEN** el jugador 1 queda en `EsperandoTirada` con la tabla base y la efectiva guardadas y sin cara.
- **AC-3 (regla 3, resultado de cada cara)** *(Integration, #3)* — **GIVEN** `EsperandoTirada` con una tirada que da cada cara por separado, **WHEN** llega `Tirada`, **THEN**: Distracción → fase `Atacar`; Grito → fase `Mover`, `bonoAtaque` 2 y `bonoMovimiento` 2; Llamada → fase `Reclutar` de la cara Llamada; Cuerno → fase `Reclutar` de la cara Cuerno; Arremetida → fase `Mover` sin tope de piezas. Solo el Grito deja un bono distinto de 0.
- **AC-4 (regla 4 y F1, elección)** *(Logic)* — **GIVEN** (`mias`, `rival`) = (6, 13), (7, 13), (9, 12), (10, 12), (13, 6), (4, 3), (1, 1), (61, 1) y (1, 61), **THEN** `tabla_base` da Pierde 1/2, Pierde 3/4, Pierde 3/4, Normal, Gana 2 a 1, Gana 4 a 3, Normal, Gana 2 a 1 y Pierde 1/2. **GIVEN** dos jugadores con 13 piezas y uno con 3 tramos rotos, **THEN** Normal (los muros no cuentan). **GIVEN** un jugador con solo el Rey contra uno con 2 piezas, **THEN** Pierde 1/2 (el Rey cuenta).
- **AC-5 (F1, exhaustivo)** *(Logic)* — **GIVEN** cada par (`mias`, `rival`) de 1 a 61, **THEN** se elige exactamente una tabla y las condiciones de «Pierde» y de «Gana» nunca se cumplen a la vez. **GIVEN** cada una de las cinco tablas base de la config, **THEN** suma 100. **GIVEN** (`mias` = 0) o (`rival` = 0), **THEN** `ErrorInterno`.
- **AC-6 (regla 5 y F2, ejemplos)** *(Logic)* — **GIVEN** las siete filas de la tabla de ejemplos de F2 (base, estados de las caras y piezas sin el Rey), **THEN** la tabla efectiva es, en el mismo orden, 15/20/25/25/15, 20/40/20/20/0, 20/32/0/20/28, 15/45/0/0/40, 15/85/0/0/0, 20/29/0/28/23 y 15/45/0/0/40.
- **AC-7 (F2, exhaustivo y límites)** *(Logic)* — **GIVEN** cada tabla base, cada combinación de estados de las dos caras de reclutar (3 × 3) y 12 y 13 piezas sin el Rey, **THEN** la tabla efectiva suma 100, Distracción no cambia, `Tirada(99)` da siempre una cara con probabilidad mayor que 0, el Grito no pasa de 85, Arremetida vale 0 con 12 o menos y no pasa de 40 con 13 o más.
- **AC-8 (regla 5, muros)** *(Integration, #5 y #10)* — **GIVEN** un castillo del jugador J con 2 tramos rotos, **WHEN** se abre su turno, **THEN** Llamada y Cuerno valen 0 en su tabla efectiva y su parte va por `reparto_liberado`. **GIVEN** 1 tramo roto, **THEN** no cambia nada.
- **AC-9 (regla 5, llamadas y lugar)** *(Integration, #10)* — **GIVEN** J con 6 llamadas de Llamada gastadas, **THEN** esa cara es Agotada y vale 0 (`reparto_liberado`). **GIVEN** J con 9 soldados, 13 arqueros y 9 escuderos vivos y llamadas disponibles, **THEN** esa cara es SinLugar y vale 0 (`reparto_sin_lugar`, y Cuerno recibe una parte si está Disponible). **GIVEN** 8, 12 y 8 (el borde: cada llamada llena justo el tope), **THEN** Disponible, con al menos 4 casillas libres en el anillo.
- **AC-10 (regla 6, congelada)** *(Logic)* — **GIVEN** un estado `EsperandoTirada` con tabla `T`, **WHEN** se consulta `ProbabilidadesSiguienteTirada` del jugador del turno antes y después de consultar la del rival, **THEN** las dos devuelven `T` sin cambios y el estado no se modifica. **GIVEN** la consulta del rival, **THEN** da la tabla que él tendría con el tablero actual.
- **AC-11 (regla 7, Grito en piezas directas)** *(Logic)* — **GIVEN** Grito activo para J, **WHEN** se llama `EffectiveStats` de un arquero sobre el muro, un jinete y el Rey de J, **THEN** ataque y movimiento suben 2 (arquero: 6 → 8 y 4 → 6; jinete: movimiento 6 → 8; Rey: movimiento 6 → 8) y escudo, vida y alcance no cambian.
- **AC-12 (regla 7, sin bono en el área ni en la explosión)** *(Logic)* — **GIVEN** Grito activo, **WHEN** se llama `EffectiveStats` de una catapulta y un demoledor de J, **THEN** ataque 12 y 30, `danoArea` 6 y 0, y movimiento +2 (2 → 4 y 6 → 8).
- **AC-13 (regla 7, solo el jugador del turno)** *(Logic)* — **GIVEN** Grito de J, **WHEN** se llama `EffectiveStats` de cualquier pieza del rival, **THEN** sin bono.
- **AC-14 (regla 7, duración)** *(Integration, #3)* — **GIVEN** Grito activo, **WHEN** se cierra el turno, **THEN** los dos bonos son 0 y `EffectiveStats` devuelve la base. **GIVEN** un `TurnTimeout` de mover con Grito activo, **THEN** los bonos siguen vigentes en `Atacar`. **GIVEN** un estado de prueba con bono 2 y se vuelve a escribir el Grito, **THEN** el bono queda en 2 y no en 4.
- **AC-15 (regla 7 y F3, config)** *(Logic)* — **GIVEN** `bono_grito_ataque` 10 y un `ataque` base de 1.000, **THEN** `ataque_ef` 1.010 sin error. **GIVEN** un bono de 11, **THEN** `ConfigInvalida`. **GIVEN** los dos bonos en 0, **THEN** `ConfigInvalida`; con uno solo en 0, se crea. **GIVEN** `bono_grito_movimiento` 3 con `limite_normal` 37 y el Catálogo vigente, **THEN** `ConfigInvalida` (la pila pasa de 256;).
- **AC-16 (regla 8, Distracción)** *(Integration, #3 y #7)* — **GIVEN** Distracción, **WHEN** llega `Mover` o `Deshacer`, **THEN** `FaseIncorrecta`, y la etapa del timer es la de atacar. **GIVEN** que ninguna pieza de J tiene un objetivo posible, no hay catapulta ni demoledor propio vivo y ningún escudero puede cubrir, **THEN** `FaseSaltada(Atacar)` y `TurnoTerminado`, y el rival queda en `EsperandoTirada`. **GIVEN** una catapulta propia viva, **THEN** `Atacar` se abre y no se salta.
- **AC-17 (regla 9, tirada)** *(Logic)* — **GIVEN** la tabla 20/20/25/20/15, **THEN** `Tirada(0)` da la cara 1, `Tirada(47)` la 3 y `Tirada(99)` la 5. **GIVEN** `Tirada(100)`, **THEN** `TiradaInvalida`. **GIVEN** un contador distinto del esperado, **THEN** se rechaza sin cambiar el estado. **GIVEN** una `Tirada` fuera de `EsperandoTirada`, **THEN** `TiradaNoEsperada`. El evento trae `n`, la cara, la tabla efectiva y el nombre de la tabla base.
- **AC-18 (regla 9, determinismo)** *(Logic)* — **GIVEN** el mismo estado y la misma `n`, **WHEN** se aplica `Tirada` dos veces en partidas independientes, **THEN** la cara, los bonos y la huella del estado son idénticos.
- **AC-19 (regla 10, panel)** *(UI, #17; la cubre AC-16 de #17, misma evidencia)* — **GIVEN** una partida con un castillo cortado y 12 piezas, **WHEN** se abre el «?», **THEN** la captura muestra la tabla *efectiva* de los dos jugadores (no la base), cada cara con su porcentaje y el motivo de cada 0% (sin muros, sin llamadas, sin tope, sin lugar, 12 piezas o menos). Evidencia: `production/qa/evidence/modificador-panel-probabilidades.png`.
- **AC-20 (regla 11, `Rendirse`)** *(Integration, #13 y #15)* — **GIVEN** 6 contra 13 piezas, **THEN** el jugador del turno ve `Rendirse`. **GIVEN** 7 contra 13, **THEN** no. **GIVEN** el jugador que va ganando, **THEN** nunca lo ve.
- **AC-21 (regla 12, prueba operativa del 3a)** *(Logic, test de datos)* — **GIVEN** las cinco tablas base vigentes, **THEN** Grito de «Pierde 1/2» (20) ≤ Grito de «Gana 2 a 1» (25) y Grito de «Pierde 3/4» (20) ≤ Grito de «Gana 4 a 3» (25), y ninguna tabla «Pierde» supera a «Normal» (20). **GIVEN** una tabla «Pierde» con Grito 26, **THEN** el test falla y nombra la tabla.
- **AC-22 (regla 13, no toca el tablero)** *(Logic)* — **GIVEN** un estado `EsperandoTirada`, **WHEN** se aplica cada una de las cinco caras, **THEN** piezas, muros, contadores de llamadas y casillas quedan idénticos; solo cambian la cara, los bonos y la fase.

### Fórmulas (Sección D)

- **AC-23 (F3, ejemplos)** *(Logic)* — cubierto por AC-11 y AC-12. **GIVEN** bono 0 en los dos, **THEN** `EffectiveStats` es igual a la base *(rechazado por AC-15 en una partida, válido como función)*.
- **AC-24 (F4, Δ de duración)** *(Logic, test de balance; no entra al núcleo)* — **GIVEN** N = 26 y `p_D` 20, **THEN** `E_D` = 5 y la cota superior es 150 s. **GIVEN** N = 37, **THEN** `E_D` = 7 y 210 s. **GIVEN** `p_D` 0, **THEN** `E_D` = 0 y las dos cotas son 0.
- **AC-25 (F5, ritmo de remontada)** *(Logic, test de balance)* — **GIVEN** las cinco tablas vigentes, **THEN** `ritmo_x100` es 120, 140, 160, 180 y 200 (de «Gana 2 a 1» a «Pierde 1/2») y la brecha entre extremos es 80.
- **AC-26 (F5, umbral `W`)** *(Logic, Métricas #19)* — **GIVEN** 20 partidas con líder inicial y 13 ganadas por él, **THEN** el informe da `W` = 65% y «bajo el umbral». **GIVEN** 14, **THEN** 70% y «cumple». **GIVEN** menos de 20 partidas, **THEN** el informe dice «muestra insuficiente» y no da veredicto.

### Casos borde (Sección E)

- **AC-27 (rendirse o cerrar sin tirada)** *(Integration, #3)* — **GIVEN** `EsperandoTirada`, **WHEN** el jugador usa `Rendirse`, **THEN** la partida termina sin tirada. **WHEN** llega `CerrarPartida(RelojAgotado)`, **THEN** `CierreInvalido`.
- **AC-28 (Grito sin acciones)** *(Integration, #3)* — **GIVEN** Grito y que ninguna pieza de J puede moverse ni atacar, **THEN** `FaseSaltada(Mover)`, `FaseSaltada(Atacar)` y `TurnoTerminado`, y el rival queda en `EsperandoTirada`.
- **AC-29 (apertura)** *(Logic)* — **GIVEN** 13 contra 13 piezas con el Catálogo vigente, **THEN** la tabla efectiva del jugador 1 es 20/40/20/20/0 (Arremetida bloqueada por `limite_normal` 12).

### Presentación (Visual/Audio, Game Feel, UI)

- **AC-30 (las cinco caras son distinguibles)** *(Visual/Feel)* — **GIVEN** cada una de las cinco caras en pantalla, **WHEN** se captura en escala de grises, **THEN** las cinco se distinguen por forma o texto, sin depender del color. Evidencia: `production/qa/evidence/modificador-caras-gris.png`.
- **AC-31 (el bono del Grito se ve antes de resolver)** *(UI, #15 y #16)* — **GIVEN** Grito activo, **WHEN** se asigna un ataque de un arquero, **THEN** la ficha muestra ataque 8 con la base 6, la vista previa usa 8, la catapulta muestra 12 de impacto y 6 de área, y el demoledor no muestra cambio de ataque. Evidencia: `production/qa/evidence/modificador-grito-ficha-vista-previa.png`.
- **AC-32 (la tirada va antes de mover)** *(Visual/Feel)* — **GIVEN** un turno desde el segundo, **WHEN** empieza, **THEN** el nombre de la cara es visible antes de que el jugador pueda mover, y no hay efecto del dado entre asignar y resolver. Evidencia: grabación o secuencia de capturas en `production/qa/evidence/`.

### Huecos que quedan (lente de QA)

- **Sin criterio automático:** que el «?» esté «siempre disponible» (se verifica con la captura de AC-19) y que el dado «no se sienta decisivo» (pregunta de playtest, no criterio).
- **PROVISIONAL:** ninguno: AC-31 dejó de serlo al escribirse #16 (tanda 4). AC-19 (cubierta por AC-16 de #17; AC-30 comparte su captura en grises) y los de Resolución (#9) dejaron de serlo al cerrar la tanda 3.
- **El umbral 3c** (`W` ≥ 2/3) no es un criterio de aceptación sino una medición del MVP con 20 partidas o más (Open Questions).
- **Sin presupuesto propio de rendimiento:** la consulta recorre 5 caras y hereda el de la Librería (ADR).

## Open Questions

| Pregunta | Dueño | Cuándo |
|---|---|---|
| ¿El reparto de caras bloqueadas le da demasiado Grito al que pierde (hasta 85%, +60 puntos frente a un líder en 25%)? Medir cuántos turnos del que pierde tienen Grito en 60% o más La medida existe (Métricas, #19, regla 14). | Métricas (#19) | Playtests del MVP |
| ¿`W` cumple 2 de cada 3? Con 20 partidas el error estándar es de unos 10,5 puntos: las primeras 20 orientan y no deciden Se mide con Métricas (#19, F3). | Métricas (#19) | MVP; propuesta de cambios después de 20 partidas online |
| Valor de `Δturnos_D` y de `t_atacar` (F4) Se mide con Métricas (#19, F4). | Métricas (#19) | Primeras 3 partidas del MVP |
| ¿Distracción sin objetivos posibles (turno vacío) se siente como el castigo que el director creativo marcó? | Playtest | MVP |
| El Grito es el 40% de las tiradas de la apertura (tabla Normal 20/40/20/20/0) y deja el margen del turno 2 (`margen_primer_golpe` del Tablero) en 1: ¿acelera demasiado los primeros turnos? | Playtest y Tablero (#4) | MVP |
| Estrategias degeneradas: cortar el propio castillo, llenar el propio anillo o sacrificar piezas para cruzar un umbral, en especial de 13 a 12 sin el Rey (15 a 40 puntos de Arremetida pasan a Grito) La medida existe (Métricas, #19, regla 14). | Métricas (#19), Reclutamiento (#10) | Playtests del MVP |
| ¿Los dos repartos (liberado y sin lugar) se entienden en el panel «?»? #17 los muestra con el cambio contra la base y el motivo de cada cara (F2, F3); queda medirlo | Playtest | MVP |
| ¿Pasar los umbrales de F1 (1/2, 3/4, 4 a 3, 2 a 1) a la config? | Este GDD y Librería | Después de 20 partidas |
| ¿Qué pasa si la autoridad no envía la tirada (online)? ¿Compromiso verificable de las tiradas? | Servidor (#28) | Antes de lanzar el online |
| ~~Confirmar que Resolución lee el ataque solo por `EffectiveStats`~~ **Resuelta 2026-11-25:** #9 lo confirma | — | — |
| Con `limite_normal` alto Arremetida casi no sale: ¿se revisa su rol? | Movimiento (#6) | Cuando se mida `limite_normal` |
| Íconos de las cinco caras, marcador del Grito, placa de Arremetida y contador sin tope (la forma de los íconos de cara ya la describe #17, Visual/Audio; falta el art bible) | Director de arte | Antes de `/asset-spec` |

**Resuelto en este GDD (2026-11-25):** el bono del Grito (+2 de ataque y +2 de movimiento, Rey incluido, también el impacto de la catapulta; no el área ni la explosión); el uso de `estado_cara` con los dos repartos; el choque con el pilar 3a (enmienda propuesta, regla 12); el umbral 3c (2 de cada 3); el tope de 6 Llamadas (se mantiene en 6 y se mide).
