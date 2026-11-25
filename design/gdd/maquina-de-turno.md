# Máquina de turno y fin de partida

> **Status**: In Design
> **Author**: jugador + Claude (design-system)
> **Last Updated**: 2026-11-25 (reloj de partida por jugador propagado a todo el documento; pendiente de revisión)
> **Last Verified**: 2026-11-20
> **Implements Pillar**: 4 Duelo corto (dueña del presupuesto de duración); sirve a 1 Planificar y cosechar
> **Creative Director Review (CD-GDD-ALIGN)**: REVISED 2026-11-20 — la fantasía promete "no te quita lo ya hecho" y declara el límite del timeout en Reclutar; el aviso de tiempo es requisito (desde el 2026-11-24, los dos relojes a la vista: regla 14, AC-30)

## Summary

La **Máquina de turno** ordena cada turno de rolchess (tirada del Modificador, reclutar,
mover, atacar y resolver), guarda la pila de deshacer de los movimientos y decide cuándo
termina la partida. Existe para que el jugador planifique y vea caer sus golpes siempre en
el mismo orden, y para sostener el pilar de duelo corto con un reloj de
12:30 por jugador (25 minutos en total). Es parte del núcleo puro de C#: no
mide tiempo ni toma azar.

> **Quick reference** — Layer: `Core` · Priority: `MVP` · Key deps: `Librería de reglas (#1), Catálogo de piezas (#2)`

## Overview

La **Máquina de turno** es la parte del núcleo que ordena un turno de rolchess y decide
cuándo termina la partida. Define las fases por las que pasa cada turno (tirada del
Modificador, reclutar, mover, atacar y resolver), qué comandos vale cada una y cuándo se
pasa de una a la siguiente. Es dueña de tres cosas: la **pila de deshacer** de los
movimientos del turno, la función **`EvaluateMatchEnd(estado)`**, que se consulta después
de cada resolución, y el **presupuesto de duración de la partida**, que sostiene el pilar 4
(15–20 minutos, nunca más de 25).

Es una capa técnica: el jugador no la opera, pero la siente en el ritmo de cada turno.
Escribe sus fases *dentro* del contrato de la Librería de reglas (#1): no mide tiempo (solo
recibe `TurnTimeout`), no toma azar (la tirada llega como dato de la autoridad) y no conoce
a ningún sistema por nombre. Trabaja con **resultados tipados** (seguir, reclutar
obligatorio, saltear la fase de movimiento, cambiar el límite de movimientos), y así el
Modificador, el Reclutamiento y el Movimiento se enganchan sin que la Máquina los conozca.

Sin ella, los diez sistemas que juegan un turno no tendrían orden común, y nadie sería
responsable de cuánto dura la partida ni de cómo termina.

*Alternativa descartada:* que cada sistema (Modificador, Reclutamiento, Movimiento,
Resolución) controle su propia fase. Eso repartiría el orden del turno entre cinco GDD y
dejaría sin dueño el presupuesto de duración.

→ Cómo se implementan las fases (tipos, estructura de código) queda para el ADR de la
Librería.

## Player Fantasy

Quedan doce segundos de la etapa de mover. Ya armaste la mitad de la maniobra y todavía
falta la pieza que cierra el golpe. El reloj no te espera: soltás las últimas piezas sobre el
objetivo, pasás a atacar con lo que tenés y resolvés. Quedarte sin tiempo no te quita lo
que ya hiciste: lo movido se queda donde está, lo asignado se resuelve y lo que falta se
decide solo. Es justo lo que te obliga a concentrarte en lo que importa y a decidir. Cuando
cae el golpe, el turno se cierra limpio y la partida sigue hacia un final que se siente
corto y ganado.

La Máquina de turno no tiene cara propia: la fantasía es la de **decidir bajo una presión
buscada**, dentro de un duelo corto donde cada turno cuenta.

*Pilar citado (pilar 4):* "El timer es presión buscada: quedarse sin tiempo para mover o
para atacar no es una falla sino lo que obliga a concentrarse."

*Qué le exige al resto del diseño:* que agotar el tiempo de una etapa nunca te quite lo ya hecho (lo
movido queda, lo asignado se resuelve, el recluta automático no te hace perder el recurso y
un comando que llega tarde se descarta sin ruido), y que el jugador vea siempre en qué
etapa está y cuánto le queda, también en su reloj de partida. El reloj de 12:30 sí
termina la partida: es la única presión que la hace perder. **Límite conocido:** si
el tiempo se agota mientras se elige qué reclutar, el turno pasa directo a atacar y esa fase
de mover se pierde; se mide en playtest (Open Questions).


## Detailed Design

### Core Rules

1. **Qué es.** La Máquina no es un comando: es el conjunto de reglas de fase dentro de
   `Aplicar` (Librería, reglas 1–17). Define las fases, qué comando vale en cada una, las
   transiciones, la pila de deshacer, `EvaluateMatchEnd` y el presupuesto de duración. No
   conoce al Modificador, al Reclutamiento, al Movimiento ni a la Resolución por nombre: les
   da ganchos tipados.
2. **Fases (lista cerrada).** `EsperandoTirada`, `Reclutar`, `Mover`, `Atacar` y el estado
   final `Terminada`. **Resolver no es una fase en reposo:** es la transición que cierra
   `Atacar` y abre el turno siguiente en un solo paso. La **etapa del timer** es *mover*
   para `Reclutar` y `Mover`, y *atacar* para `Atacar`; en `EsperandoTirada` no corre
   ninguna.
3. **Primer turno (decisión del 2026-11-20, confirmada por el jugador el 2026-11-24).** Lo juega el jugador 0 y no tira el
   Modificador: empieza directo en `Mover`, sin `Reclutar`, porque sin tirada no hay cara de
   reclutar. Quién empieza lo decide el Tablero (#4, regla 10): el jugador 0, sin compensación.
4. **Abrir un turno T del jugador J** (el cierre del turno anterior y esta apertura salen
   en un solo comando):
   1. Se borran las coberturas de J (Cobertura, #8).
   2. El estado pasa a `EsperandoTirada` con la tabla efectiva (Librería, regla 6).
   3. Al llegar `Tirada(n)`, la Librería convierte `n` en una cara y el **Modificador
      (#11)** devuelve un **resultado tipado**.
5. **Resultados tipados (lista cerrada).** El índice listaba tres; se suma
   `ReclutarObligatorio` porque abre una fase:

   | Resultado | La Máquina… | Lo produce hoy |
   |---|---|---|
   | `Seguir` | abre `Mover` | Grito de guerra (escribe sus bonos en el estado) |
   | `ReclutarObligatorio(cara)` | abre `Reclutar` | Llamada al servicio, Cuerno de guerra |
   | `SaltearMovimiento` | abre `Atacar` directo | Distracción |
   | `LimiteMovimientos(sin límite)` | abre `Mover` sin tope | Arremetida |

   El límite normal de movimientos (hoy 12 piezas más el Rey) es de Movimiento (#6); la
   Máquina solo guarda el límite del turno en el estado. **Arremetida solo puede salir si el
   jugador tiene más de 12 piezas sin contar el Rey** (decisión del 2026-11-24): con 12 o menos
   no cambia nada, y el Modificador (#11) pasa su probabilidad entera a Grito de guerra.
6. **Fase `Reclutar`.** Solo existe con `ReclutarObligatorio`. Si al abrirla ningún tipo
   está disponible (`tipo_disponible`, #10) → `ReclutaPerdido` y pasa a `Mover`; es una defensa, porque la tabla usa la misma consulta. Si no, solo sale con
   `Reclutar(tipo)` o con el timeout (recluta automático). No hay "no reclutar".
7. **Fase `Mover`.** Valen `Mover`, `Deshacer` y `TerminarMovimiento`. Una fase ya empezada
   nunca termina sola: aunque se haya usado todo el límite, la cierra `TerminarMovimiento`
   o el timeout. Al salir, la pila se vacía y lo movido queda firme.
8. **Pila de deshacer (decisión del 2026-11-20: solo movimientos).** Cada entrada guarda
   pieza, casilla de origen y movimientos usados antes. `Deshacer` saca la última y la
   devuelve con su movimiento usado. Ni el reclutamiento ni los ataques entran: reclutar se
   compromete al elegir y los ataques ya se corrigen con `QuitarAtaque` y `Cubrir`. Tope de
   256 entradas (el dueño ahora es la Máquina; se justifica en Formulas, D4, que además
   exige que el Catálogo no lo supere).
9. **Fase `Atacar`.** Valen `AsignarAtaque`, `QuitarAtaque`, `Cubrir` y `Resolver`. Las
   asignaciones se editan libremente hasta resolver. `Resolver` cierra la fase y llama a
   Resolución (#9).
10. **Fases que se saltan.** Una fase que **empieza** sin acción posible se salta con
    `FaseSaltada` (lista cerrada, Librería regla 17): en reclutar, algún tipo disponible; en
    mover, una pieza con algún movimiento legal; en atacar, una pieza con algún objetivo
    válido o un escudero con un aliado que cubrir. Si se saltan todas, el turno cierra solo
    con `TurnoTerminado`. La cascada para en el siguiente `EsperandoTirada` o en el fin de
    la partida.
11. **Cerrar un turno.** Después de resolver (con `Resolver`, con el timeout de atacar o por
    salto automático) se limpian los bonos del Grito de guerra, las asignaciones y el límite
    del turno, y los movimientos usados de todas las piezas vuelven a 0 (Movimiento, #6, regla 16). Luego se corre `EvaluateMatchEnd` y, si no hay ganador, pasa el jugador, sube
    `numeroTurno` y se abre el siguiente turno (regla 4).
12. **`TurnTimeout` (definitivo; antes provisional, Librería regla 17).**
    - *Etapa mover:* si hay un `Reclutar` pendiente, la librería recluta el primer tipo
      disponible en el orden del Catálogo (evento con `automatico`); cierra la pila y pasa a
      `Atacar`. Las piezas ya movidas se quedan donde están.
    - *Etapa atacar:* resuelve los ataques ya asignados, igual que `Resolver`. Las
      coberturas valen.
    - Con `EsperandoTirada` se rechaza; un timeout de otro turno o etapa es
      `TimeoutObsoleto`. **Ningún timeout de etapa hace perder el
      plan** (el reloj de partida es otra cosa: regla 14).
13. **`EvaluateMatchEnd(estado)`**, función pura. Sin Reyes vivos → `DobleRey`, gana el
    jugador del turno (**confirmado el 2026-11-20**: es quien planeó y vio la vista previa,
    pilar 1). Un solo Rey vivo → `ReyMuerto`, gana su dueño. Un Rey propio muerto por fuego
    amigo cuenta igual: gana el rival.
14. **Reloj de partida por jugador (decisión del 2026-11-24; reemplaza el cierre por tiempo
    del 2026-11-20).** Cada jugador tiene `reloj_jugador_s` = `T_tope` / 2 = **750 s (12:30)**
    para toda la partida. Corre solo durante sus turnos, en `Reclutar`, `Mover` y
    `Atacar`; **no corre en `EsperandoTirada`** (jugador, 2026-11-25): el jugador no paga la
    demora de la autoridad. **Convive con el timer de cada etapa** (mover y atacar). Así ninguna partida con
    timer pasa de 25:00, sin contar las esperas de la tirada. El tiempo lo mide la autoridad (Timer #12 local, Servidor #28
    online); el núcleo no tiene reloj. Cuando el reloj de un jugador llega a 0, en cualquier
    fase, la autoridad envía `CerrarPartida` con ganador = el rival (motivo **RelojAgotado**,
    Librería, reglas 3 y 16, que lo rechaza con `CierreInvalido` si el ganador no es el rival
    del jugador del turno o si se espera una tirada). Lo ya movido queda; los ataques asignados y no resueltos no se
    resuelven. **El modo Sin límite no tiene reloj.** **El jugador ve los dos relojes** en todo
    momento (diseño: Ficha y HUD, #15).
15. **`vidaQuitada` (estadística).** No hay empate por tiempo que resolver: con el reloj por
    jugador la partida termina por Rey muerto, reloj agotado, rendición, abandono o tablas
    (2026-11-24). El estado guarda `vidaQuitada` por jugador (jugador, 2026-11-25): vida
    **realmente perdida** por piezas y muros del rival, acumulada en la partida; el fuego
    amigo no cuenta y lo que absorbe un escudero al cubrir sí. La escribe Resolución (#9), la
    lee Métricas (#19) y ninguna regla decide con ella. **Cota:** `estadistica_max` (1.000) ×
    (`C_MAX` − 1 = 999.999 piezas posibles) + 16 × `vida_tramo` de los 16 tramos rivales (#5; 16.000 con `vida_tramo` 1.000) = 1.000.015.000 como máximo; no
    desborda.
16. **Eventos de la Máquina (lista cerrada).** `TurnoIniciado(numeroTurno, jugador)`,
    `FaseIniciada(fase)`, `FaseSaltada(fase)`, `ReclutaPerdido`, `TurnoTerminado` y
    `PartidaTerminada(ganador, motivo)`.
17. **Matriz de comandos por fase.** Sale del orden de validación de la Librería (regla 4):

    | Comando | `EsperandoTirada` | `Reclutar` | `Mover` | `Atacar` |
    |---|---|---|---|---|
    | `Tirada` (autoridad) | ✓ | `TiradaNoEsperada` | `TiradaNoEsperada` | `TiradaNoEsperada` |
    | `Reclutar` | `EsperandoTirada` | ✓ | `FaseIncorrecta` | `FaseIncorrecta` |
    | `Mover`, `Deshacer`, `TerminarMovimiento` | `EsperandoTirada` | `FaseIncorrecta` | ✓ | `FaseIncorrecta` |
    | `AsignarAtaque`, `QuitarAtaque`, `Cubrir`, `Resolver` | `EsperandoTirada` | `FaseIncorrecta` | `FaseIncorrecta` | ✓ |
    | `TurnTimeout` (autoridad) | `EsperandoTirada` | ✓ etapa mover | ✓ etapa mover | ✓ etapa atacar |
    | `Rendirse`, `CerrarPartida` | ✓ | ✓ | ✓ | ✓ |

### States and Transitions

| Desde | Hacia | Cuándo |
|---|---|---|
| (creación) | `Mover` (turno 1, jugador 0) | `CrearPartida` con castillos fijos (Tablero, #4); sin tirada ni reclutar |
| `EsperandoTirada` (turno ≥ 2) | `Reclutar` | `Tirada` válida → `ReclutarObligatorio` |
| `EsperandoTirada` | `Mover` | `Tirada` → `Seguir` o `LimiteMovimientos(sin límite)` |
| `EsperandoTirada` | `Atacar` | `Tirada` → `SaltearMovimiento` |
| `Reclutar` | `Mover` | `Reclutar(tipo)`, o `ReclutaPerdido` al abrirla |
| `Reclutar` | `Atacar` | Recluta automático por `TurnTimeout` de mover (regla 12) |
| `Mover` | `Atacar` | `TerminarMovimiento` o `TurnTimeout` de mover (la pila se vacía) |
| `Mover` o `Atacar` | siguiente fase / cierre | Salto automático (`FaseSaltada`) al abrirla sin acción posible |
| `Atacar` | `EsperandoTirada` (turno + 1) | `Resolver` o `TurnTimeout` de atacar, sin ganador |
| `Atacar` | `Terminada` | `Resolver` o `TurnTimeout` con ganador (`ReyMuerto`, `DobleRey`) |
| cualquier estado en curso | `Terminada` | `Rendirse`, o `CerrarPartida` (Abandono, `RelojAgotado` salvo en `EsperandoTirada`, Tablas) |
| `Terminada` | `Terminada` | Todo comando se rechaza con `PartidaTerminada` |

### Interactions with Other Systems

| Sistema | La Máquina le da | Recibe | Dueño de la interfaz |
|---|---|---|---|
| 1 Librería | Fases y reglas dentro del contrato | Comandos, códigos, enteros y orden | Librería (contrato) y #3 (fases) |
| 11 Modificador | Pide la tirada y le pasa la cara | Un resultado tipado y los bonos en el estado | Máquina define los tipos; Modificador los produce |
| 10 Reclutamiento | Abre `Reclutar`, ejecuta el auto-recluta | Consulta "¿tipo disponible?" y `Reclutar` | Reclutamiento |
| 6 Movimiento | Abre `Mover`, guarda la pila | `MovimientosLegales` y el límite del turno | Movimiento |
| 7 Alcance y asignación | Abre `Atacar` | `ObjetivosValidos` | Alcance y asignación |
| 8 Cobertura | Borra coberturas al abrir turno del dueño | Reglas de `Cubrir` | Cobertura |
| 9 Resolución | Llama tras `Resolver` | Estado nuevo, eventos y contador `vidaQuitada` | Resolución |
| 12 Timer | `numeroTurno` y etapa actual | `TurnTimeout` y `CerrarPartida(RelojAgotado)` | Timer |
| 28 Servidor | Mismo pipeline | Es la autoridad de los comandos de autoridad | Servidor |
| 15 HUD, 19 Métricas, 25 Rival IA | Eventos de fase, etapa y pila | Nada (IA: mismos comandos que un jugador) | Cada GDD |

**Supuestos PROVISIONALES sobre GDD sin escribir:** el resultado `ReclutarObligatorio`
(#11) y el contador `vidaQuitada` (#9). Las piezas reclutadas se mueven ese turno, salvo las
del recluta automático (#6, regla 11; #10, reglas 9 y 10). Con una catapulta o un demoledor
propios vivos, `Atacar` nunca se salta (#7, regla 14).

## Formulas

Hay dos fórmulas de diseño (D1, D2), que **no corren en el núcleo** porque no tiene reloj
(Librería, regla 5). Las usan el Timer (#12), el Servidor (#28) y las Métricas (#19). La
otra (D4) es una regla pura del núcleo; D3 se retiró.

### D1. Duración de la partida y presupuesto de turnos

La fórmula `duracion_partida` se define así:

`duracion_partida = Σ seg_turno_t` (t = 1 … N)
`seg_medio = ⌊duracion_partida ÷ N⌋`
`turnos_presupuesto = ⌊T_tope ÷ seg_medio⌋`

**Variables:**

| Variable | Type | Range | Source | Description |
|----------|------|-------|--------|-------------|
| N | int | 1–∞ (v4: 24 y 34) | calculated (Métricas #19) | Turnos jugados |
| seg_turno_t | int | 0–`turno_max_s` | calculated (Métricas #19) | Segundos del turno t, medidos por el cliente |
| T_tope | int | 1290–1500 (valor 1500) | data file (config de la autoridad) | Tope de partida: 25 min, pilar 4 |
| seg_medio | int | 1–`turno_max_s` | calculated | Promedio por turno; mínimo 1 para no dividir por cero |

**Output Range:** `duracion_partida` de 0 a 1.500 s en los modos con timer (D2 lo
garantiza); sin cota en Sin límite. Meta de diseño: 900–1.200 s.

**Example (partidas v4 del 2026-11-19):** partida 1: N = 24, 1.381 s → `seg_medio` 57 →
`turnos_presupuesto` 26. Partida 2: N = 34, 1.372 s → `seg_medio` 40 → `turnos_presupuesto`
37. Las dos usaron menos turnos que su presupuesto, pero terminaron en ~23 min: **3 minutos
por encima de la meta de 15–20.**

| `seg_medio` | 40 | 50 | 60 | 75 | 90 |
|---|---|---|---|---|---|
| `turnos_presupuesto` (tope 1.500 s) | 37 | 30 | 25 | 20 | 16 |

Para entrar en la meta de 15–20 min hacen falta 16–21 turnos a 57 s por turno, o 23–30 a
40 s.

**Aporte de los demás sistemas.** Resolución (#9), Reclutamiento (#10) y Modificador (#11)
declaran en su GDD cuántos turnos suman por partida (`Δturnos`).
`turnos_estimados = N_base + ΔResolución + ΔReclutamiento + ΔModificador`, y
`turnos_estimados ≤ turnos_presupuesto`. **NOT SOURCEABLE — `N_base` no está medido:** las
dos v4 incluyen remontadas y no hay partida sin ellas. Se fija con las Métricas (#19) en el
MVP.

### D2. Reloj de partida por jugador

La fórmula `reloj_jugador_s` se define así:

`reloj_jugador_s = T_tope ÷ 2` (división entera hacia abajo)

**Variables:**

| Variable | Type | Range | Source | Description |
|----------|------|-------|--------|-------------|
| T_tope | int | 1290–1500 (valor 1500) | data file (config de la autoridad) | Igual que en D1 |
| reloj_jugador_s | int | 645–750 (valor 750) | calculated | Segundos de cada jugador para toda la partida |

**Output Range:** 750 s (12:30) con el valor vigente. **Sin límite: exento**, no hay reloj.

**Garantía:** los dos relojes suman como mucho `T_tope`, así que el tiempo de turno jugado no
pasa de 25:00. Las esperas de la tirada y la red quedan afuera.

**Example:** azul gastó 740 s y abre su turno con 10 s. Aunque la etapa de mover dure 60 s, a
los 10 s la autoridad envía `CerrarPartida(RelojAgotado, rojo)`.

### D3. Retirada

*(Las fórmulas del cierre anterior, D2 y D3 del 2026-11-20, se retiraron el 2026-11-24. La
cota de `vidaQuitada` pasó a la regla 15.)*

### D4. Capacidad de la pila de deshacer

La fórmula `pila_max` se define así:

`pila_max = Σ_t tope_vivas_t × movimiento_t` (t = los 7 tipos del Catálogo, Rey incluido)
**Invariante:** `pila_max ≤ pila_tope` (256). PrepararContexto rechaza con `ConfigInvalida`
un Catálogo que lo viole.

**Variables:**

| Variable | Type | Range | Source | Description |
|----------|------|-------|--------|-------------|
| tope_vivas_t | int | 1–61 | data file (Catálogo) | Piezas vivas máximas del tipo t |
| movimiento_t | int | 1–1.000 (hoy 2–6) | data file (Catálogo) | Puntos de movimiento del tipo t |
| pila_tope | int | 256 | constant (contrato) | Tope de entradas de la pila |
| pila_max | int | 254 hoy | calculated | Peor caso de entradas en un turno |

**Output Range:** 254 con el Catálogo vigente; 2 de margen.

**Example:** soldado 12 × 4 = 48; arquero 16 × 4 = 64; escudero 12 × 4 = 48; catapulta
8 × 2 = 16; jinete 8 × 6 = 48; demoledor 4 × 6 = 24; Rey 1 × 6 = 6 → **254**. El peor caso
es una Arremetida (sin tope de piezas, sin bono de movimiento) moviendo todo un paso por
comando. Con Grito de guerra (12 piezas más el Rey, +2 de movimiento) el máximo es
12 × 8 + 8 = 104.

**Dónde se rompe:** subir el movimiento del soldado en 1 da 266 > 256. Por eso el
invariante.

**Confirmado por Movimiento (#6, F5):** cada `Mover` cuesta 1 punto o más. La fórmula no cuenta
el bono del Grito; por eso `limite_normal` llega como mucho a 37 (con 38 daría 260).

## Edge Cases

### Flujo del turno

- **Si Distracción sale y ninguna pieza propia tiene objetivo válido** (ni un escudero con
  aliado que cubrir): `Atacar` se salta con `FaseSaltada` y el turno cierra solo. Evita los
  30 s vacíos del prototipo.
- **Si `Reclutar` se abre y ningún tipo está disponible** (llamadas agotadas, tope de piezas
  vivas o sin lugar para todas sus piezas; inalcanzable, #10, regla 6): `ReclutaPerdido` y pasa a `Mover`.
- **Si ya se usó todo el límite de movimientos:** la fase `Mover` sigue abierta hasta
  `TerminarMovimiento` o el timeout; un `Mover` de una pieza sin mover se rechaza con `SinMovimientos`; las ya movidas y el Rey siguen (#6, regla 10).
- **Si `Mover` empieza sin ningún movimiento legal:** `FaseSaltada` y pasa a `Atacar`.
- **Si se saltan `Mover` y `Atacar` en el mismo turno:** `TurnoTerminado` sin `Resolver`;
  `EvaluateMatchEnd` corre igual.
- **Si `TerminarMovimiento` llega con la pila vacía:** es válido y pasa a `Atacar`.
- **Si `Deshacer` llega con la pila vacía:** `NadaQueDeshacer`.
- **Si `Resolver` llega sin ataques asignados:** es válido y emite `TurnoTerminado`.

### Timeouts y carreras

- **Si el timeout de mover llega en la fase `Reclutar`:** recluta automático (primer tipo
  disponible en el orden del Catálogo) y pasa **directo a `Atacar`**, sin `Mover`; la pieza
  reclutada no se mueve ese turno.
- **Si el timeout de mover llega en `Mover`:** las piezas ya movidas quedan donde están, no
  se recluta y pasa a `Atacar`.
- **Si el timeout llega con otro turno u otra etapa** (la carrera normal con
  `TerminarMovimiento`): `TimeoutObsoleto`, sin efecto.
- **Si llegan a la vez `TerminarMovimiento` y el timeout de mover:** el orden lo fija
  `contadorComandos`; el segundo falla con `ComandoViejo` o `TimeoutObsoleto`.

### Fin de partida y tiempo

- **Si mueren los dos Reyes en la misma resolución** (también con fuego amigo, también en
  un timeout de atacar): `DobleRey`, gana el jugador del turno.
- **Si el Rey propio muere por fuego amigo y el del rival sobrevive:** `ReyMuerto`, gana el
  rival.
- **Si el reloj de un jugador llega a 0 en su turno:** pierde con `RelojAgotado`, en cualquier
  fase. Lo movido queda; los ataques asignados y no resueltos no se resuelven.
- **Si `RelojAgotado` llega con el turno ya cerrado o en `EsperandoTirada`:**
  `CierreInvalido`; la partida sigue.
- **Si `Rendirse` llega en cualquier fase, también esperando tirada:** termina la partida y
  gana el otro jugador.
- **Si los dos jugadores solo dejan correr el reloj:** con timers de etapa iguales, agota
  primero el que jugó más turnos. Azul empieza, así que con pasividad total pierde azul. En
  Sin límite la partida no termina sola.

### Límites del contrato

- **Un comando genera hasta 5 eventos de la Máquina** (peor caso: `Tirada` con reclutar
  perdido y las fases `Mover` y `Atacar` saltadas: `ReclutaPerdido`, dos `FaseSaltada`,
  `TurnoTerminado` y `TurnoIniciado`), más los de las demás reglas.
- **Si `contadorComandos` llega a `C_MAX − 1`:** solo pasan `Rendirse` y `CerrarPartida`
  (Librería, fila 15).
- **Si la pila llegara a 256:** `LimitePartida`; con el invariante de D4 es inalcanzable.
- **Si una `Tirada` devolviera un resultado tipado inexistente:** inalcanzable, porque
  PrepararContexto valida los papeles de las caras al crear la partida.

*Sin resolución, pasa a Open Questions:* que ninguno de los dos jugadores pueda actuar
durante varios turnos seguidos (casi inalcanzable; en Sin límite solo lo corta `C_MAX`).

## Dependencies

**Depende de:**

| Sistema | Tipo | Qué usa |
|---|---|---|
| #1 Librería de reglas | Dura | El contrato: `Aplicar`, comandos, códigos, enteros, orden por ID y forma canónica. Las fases se escriben *dentro* de él. |
| #2 Catálogo de piezas | Blanda | `movimiento` y `topeVivas` por tipo para el invariante de la pila (D4), y el orden de tipos para el recluta automático. |
| #4 Tablero y despliegue | Blanda | Quién empieza (jugador 0, sin compensación) y el estado ya desplegado al crear la partida. |

La Máquina **no depende** de Modificador, Reclutamiento, Movimiento, Alcance, Cobertura ni
Resolución: los llama por ganchos tipados sin conocerlos por nombre (índice, *Circular
Dependencies*). Por eso esos sistemas dependen de ella y no al revés.

**Dependen de este sistema:**

| Sistema | Tipo | Qué usa | Estado del GDD |
|---|---|---|---|
| #6 Movimiento | Dura | Fase `Mover`, pila de deshacer, límite del turno | Existe |
| #7 Alcance y asignación | Dura | Fase `Atacar` y el criterio "acción posible" | Existe |
| #8 Cobertura | Blanda | Momento de borrar las coberturas (al abrir el turno de su dueño) | Pendiente |
| #5 Castillo y muros | Blanda | Momento del corte de reclutamiento (reclutar es de la fase de mover; la tabla se calcula al abrir el turno), `Deshacer` sobre los `Mover` de subir y bajar del muro y `vidaQuitada` | Existe |
| #9 Resolución | Dura | Transición `Resolver`; escribe `vidaQuitada`; declara `Δturnos` | Pendiente |
| #10 Reclutamiento | Dura | Fase `Reclutar`, recluta automático; declara `Δturnos` (su F6, sin valor medido) | Existe |
| #11 Modificador | Dura | Resultados tipados y fase de tirada; declara `Δturnos` | Pendiente |
| #12 Timer | Dura | `numeroTurno`, etapa y `reloj_jugador_s`; emite `TurnTimeout` y `CerrarPartida(RelojAgotado)` | Existe |
| #14 Localización | Blanda | Nombres de fases y motivos de fin | Existe |
| #15 Ficha y HUD | Blanda | Fase, etapa y cuántos movimientos se pueden deshacer | Pendiente |
| #19 Métricas | Blanda | Eventos de fase, `TurnoTerminado`, `PartidaTerminada` | Pendiente |
| #21 Menú principal | Blanda | Inicia la partida y recibe `PartidaTerminada` | Pendiente |
| #25 Rival IA | Dura | Mismas fases y comandos que un jugador | Pendiente |
| #28 Servidor | Dura | Autoridad de `Tirada`, `TurnTimeout` y `CerrarPartida` | Pendiente |

**Bidireccionalidad.** La Librería, el Catálogo (desde el 2026-11-25) y el Tablero ya nombran a
la Máquina, y el índice tiene sus aristas.

Los GDD pendientes tienen que nombrar a la Máquina en su Dependencies al escribirse
(marcado para `/design-review` y `/review-all-gdds`).

## Tuning Knobs

**Perillas propias de la Máquina** (viven en la configuración de la autoridad y de las
Métricas, nunca en el código):

| Perilla | Valor | Rango seguro | Si es muy alta | Si es muy baja | Interactúa con | Dónde vive |
|---|---|---|---|---|---|---|
| `T_tope` (tope de partida) | 1.500 s | 1.290–1.500 | Más de 1.500 contradice el pilar 4: hay que revisar el pilar, no solo el número | Menos de 1.290 deja a cada jugador con menos de 10:45 (piso heredado de la fórmula anterior, sin volver a fundamentar) | `reloj_jugador_s` es su mitad. También acota `turnos_presupuesto` | Config de la autoridad (Timer #12 / Servidor #28) |
| Meta de duración | 900–1.200 s | mínimo 600 s; máximo `T_tope` | Una meta pegada al tope hace frecuentes los relojes agotados | Una meta por debajo de 900 s deja las métricas siempre en rojo | `T_tope` (su techo) y `seg_medio` | Config de Métricas (#19) |

**Perillas de otros sistemas que cambian esta GDD** (no se duplican acá, se apuntan):

| Valor | Dueño | Qué cambia aquí |
|---|---|---|
| Etapas del timer (60 + 30, 40 + 20) y minuto de cambio | Timer (#12) | `turno_max_s` (dueño: Timer, D1): cuántos turnos completos caben en `reloj_jugador_s` (8 de 90 s, 12 de 60 s) |
| `movimiento` y `topeVivas` por tipo | Catálogo (#2) | `pila_max` (D4): hoy 254 de 256; subir el movimiento del soldado en 1 lo rompe |
| Límite normal de movimientos (12 piezas más el Rey) | Movimiento (#6) | Cuántas entradas llega a tener la pila en un turno normal |
| `Δturnos` de Resolución, Reclutamiento y Modificador | #9, #10, #11 | `turnos_estimados` contra `turnos_presupuesto` (D1) |
| Orden de tipos (soldado, arquero, escudero, catapulta, jinete, demoledor, Rey) | Catálogo (#2) | Qué tipo recluta el timeout (primer tipo disponible) |

**Constantes de la Máquina (no son perillas):** `pila_tope` = 256 (cambiarlo es un cambio de
contrato y sube la versión de reglas); la lista cerrada de fases, de resultados tipados y de eventos.

## Visual/Audio Requirements

La Máquina no dibuja ni suena: emite eventos y la presentación (Render #18, Audio #22) los
traduce. Fuente: `design/art/art-bible.md`, secciones 2 y 7. Los estados que el art bible ya
define no se repiten acá; solo se listan los eventos que **ya tienen** tratamiento y los que
**le faltan**.

| Evento | Qué ve y oye el jugador | Estado en el art bible |
|---|---|---|
| `TurnoIniciado` | El contorno del tablero toma el color del bando de turno y aparece el banner del turno | Definido (§7 *Pantalla y HUD*, §2 *Turno del rival*) |
| `FaseIniciada` (Reclutar, Mover, Atacar) | El botón de avanzar cambia de texto y el ambiente pasa a *Planificar* o *Asignar ataques* | Definido (§2 *Estados*, §7) |
| Distracción (`SaltearMovimiento`) | Placa gris "Distracción", ≤ 1,5 s, y pasa a atacar | Definido (§2 *Estados*) |
| `FaseSaltada` (Mover o Atacar) | **Leyenda breve al centro** ("Sin movimientos" o "Sin objetivos"), ≤ 1 s, sin ambiente propio y sin sonido de error: no es una falla, es un turno que se cierra solo | **Falta**: hay que agregarlo a §2 |
| `ReclutaPerdido` | Leyenda breve "Sin reclutas disponibles", ≤ 1 s | **Falta** |
| Recluta automático (`automatico`) | La pieza aparece con una marca breve "Elegido por el tiempo", para que el jugador sepa que decidió el timer | **Falta** |
| `PartidaTerminada` con `ReyMuerto`, Rendición o Abandono | Pantallas espejo de Victoria y Derrota | Definido (§2 *Estados*) |
| `PartidaTerminada` con `DobleRey` | Victoria para el jugador del turno, con los dos Reyes cayendo | **Falta** el detalle de los dos Reyes |
| `PartidaTerminada` con `RelojAgotado` | Victoria o Derrota; el resumen dice a quién se le acabó el reloj | **Falta** el texto en el resumen |

**Restricciones de animación:** ningún evento le suma tiempo al turno (§2 *Reglas de ritmo*,
pilar 4). Una cascada de hasta 5 eventos de la Máquina se presenta en **≤ 1,5 s en total**,
igual que el banner de Distracción.

**Audio:** el cambio de turno es un cue suave. `FaseSaltada` y `ReclutaPerdido` no usan
sonidos de alerta. El cue del timer lo definen el Timer (#12) y Audio (#22).

**Principios del art bible que aplican:** *Regla de ambiente* (§2: el ambiente cambia solo
con gradación, viñeta, interfaz y efectos breves) y *Reglas de ritmo* (§2).

📌 **Asset Spec** — Visual/Audio requirements are defined. After the art bible is approved,
run `/asset-spec system:maquina-de-turno` to produce per-asset visual descriptions,
dimensions, and generation prompts from this section.

## Game Feel

- **Respuesta de los comandos:** `Aplicar` de cada comando entra en el presupuesto de 1 ms
  p95 de la Librería (`comando_presupuesto_ms`); la fase siguiente se ve en la misma pantalla
  en que se envió el comando.
- **Transiciones de fase:** ≤ 0,25 s (art bible, §2 *Reglas de ritmo*).
- **Deshacer:** la pieza vuelve a su casilla de origen en ≤ 0,25 s y el contador de piezas
  movidas baja en el mismo cuadro.
- **Avanzar de fase sin confirmación** (el botón de avanzar abajo a la derecha): confirmar
  suma tiempo al turno (pilar 4). El riesgo de tocarlo por error se baja con la distancia al
  botón de Deshacer, abajo a la izquierda (art bible, §7). La decisión final es de Ficha y
  HUD (#15).
- **Timer sin castigo:** al agotarse la etapa, el cambio de fase tiene que verse como el plan
  que sigue, no como un corte. Se verifica en playtest (Player Fantasy).

## UI Requirements

El HUD (#15) lee de la Máquina:

- **Jugador y número del turno**, y el contorno del bando de turno.
- **Fase y etapa:** texto del botón de avanzar ("Atacar" en `Mover`, "Terminar turno" en
  `Atacar`) y qué etapa del timer corre. En `EsperandoTirada` no corre ninguna: el HUD lo
  dice, sobre todo online.
- **Contador de piezas movidas contra el límite** (por ejemplo 5/12); con
  `LimiteMovimientos(sin límite)` no muestra tope.
- **Estado del botón Deshacer:** desactivado con la pila vacía, de modo que
  `NadaQueDeshacer` no salga por la interfaz.
- **Los dos relojes de partida**, siempre visibles (regla 14); en `EsperandoTirada` están
  detenidos y se nota. Cómo se ven y desde cuándo avisan que queda poco lo define #15.
- **Rendirse:** la interfaz lo ofrece solo al jugador del turno con la mitad de las piezas del
  rival o menos (concepto, «Rendirse»); el núcleo lo acepta siempre.
- **Pantalla de resultado:** motivo del fin y, para `RelojAgotado`, los dos relojes.

📌 **UX Flag — Máquina de turno y fin de partida**: This system has UI requirements. In
Phase 4 (Pre-Production), run `/ux-design` to create a UX spec for each screen or HUD
element this system contributes to **before** writing epics. Stories that reference UI
should cite `design/ux/[screen].md`, not the GDD directly.

## Cross-References

| Este GDD dice | Documento | Elemento exacto | Naturaleza |
|---|---|---|---|
| `RelojAgotado` como motivo de fin | `libreria-de-reglas.md` | Reglas 3, 4 y 16; AC-50 | Hecho el 2026-11-25 |
| Campo `vidaQuitada` por jugador, versión de reglas | `libreria-de-reglas.md` | Regla 11 (tabla de dominios, matriz de clausura de `Resolver` y `TurnTimeout` de atacar), regla 12 | Hecho el 2026-11-25 (solo estadística) |
| `Resolver` no es fase en reposo; sin etapa en `EsperandoTirada` | `libreria-de-reglas.md` | Regla 11, fila de fase y etapa | Hecho el 2026-11-25 |
| Pila de 256 pasa a ser de la Máquina, con invariante de catálogo | `libreria-de-reglas.md` | Regla 11, fila de pila y Tuning Knobs | Hecho el 2026-11-25 |
| `DobleRey`, `TurnTimeout` y primer turno sin tirada quedan definitivos | `libreria-de-reglas.md` | Reglas 2, 16 y 17; AC-20, AC-22, AC-31, AC-34, AC-38 | Hecho el 2026-11-25 |
| Se cierran cuatro preguntas abiertas (tope de 25 min, `TurnTimeout`, `DobleRey`, pila) | `libreria-de-reglas.md` | Open Questions | Hecho el 2026-11-25 |
| El Catálogo tiene a la Máquina como consumidora; invariante `pila_max ≤ 256` | `catalogo-de-piezas.md` | Dependencies y F8 (el AC queda en AC-27) | Hecho el 2026-11-25 |
| Orden del recluta automático del Cuerno de guerra | `catalogo-de-piezas.md` | Open Questions | **Resuelve por defecto:** primer tipo disponible en el orden del Catálogo, o sea la catapulta; lo confirma #10 |
| Aristas #3 → #2 y #8, #9, #10 → #3; mitigación del riesgo de duración | `systems-index.md` | Tabla de sistemas y High-Risk | **Agrega** |
| "Nunca pasa de 25 minutos" vale en los modos con timer; Sin límite exento | `game-concept.md` | Pilar 4 y Core Identity (Session Length) | **Cambia** el texto |
| Eventos nuevos que necesitan tratamiento visual | `art-bible.md` | §2 *Estados* | **Agrega** |

## Acceptance Criteria

**Tipos de evidencia** (estándar de pruebas del estudio): **Logic** = prueba automática de
unidad, BLOQUEANTE, en `Assets/Tests/EditMode/MaquinaDeTurno/`. **Integration** = prueba de
integración con otro sistema. **Config/Data** = chequeo de humo. Los criterios marcados
**PROVISIONAL** dependen de un GDD que todavía no existe y se confirman cuando exista.

### Reglas de la Máquina (Sección C)

- **AC-1 (Logic, regla 1: determinismo)** — **GIVEN** la lista grabada de comandos de una
  partida de prueba de 30 turnos (con Distracción, Arremetida, un reclutar perdido y un
  timeout de cada etapa), **WHEN** se aplica dos veces desde `CrearPartida` con el mismo
  contexto, **THEN** las dos corridas devuelven la misma secuencia de eventos de la Máquina
  y la misma huella final.
- **AC-2 (Logic, regla 2)** — **GIVEN** un estado en cada una de las cuatro fases,
  **WHEN** se consulta la etapa del timer, **THEN** `Reclutar` y `Mover` dan etapa mover,
  `Atacar` da etapa atacar y `EsperandoTirada` no da ninguna.
- **AC-3 (Logic, regla 3)** — **GIVEN** `CrearPartida` con castillos fijos, **WHEN** se
  crea, **THEN** el estado está en `Mover`, jugador 0, `numeroTurno` 1, sin tirada pendiente
  y sin `FaseIniciada(Reclutar)`; el primer turno no pide tirada del Modificador.
- **AC-4 (Logic, regla 4)** — **GIVEN** el turno 1 del jugador 0 en `Atacar` y un escudero
  del jugador 1 cubriendo a un aliado desde su turno anterior, **WHEN** `Resolver` cierra el
  turno sin ganador, **THEN** el estado queda en `EsperandoTirada` del jugador 1,
  `numeroTurno` 2, sin coberturas del jugador 1, y los eventos traen `TurnoTerminado` y
  `TurnoIniciado(2, 1)` en ese orden.
- **AC-5 (Logic, regla 5; cruce con #11, PROVISIONAL)** — **GIVEN** un Modificador de prueba
  que devuelve cada resultado tipado, **WHEN** llega la `Tirada` correspondiente, **THEN**
  `Seguir` abre `Mover`; `ReclutarObligatorio` abre `Reclutar`; `SaltearMovimiento` abre
  `Atacar`; y con `LimiteMovimientos(sin límite)` se abre `Mover` y se acepta el `Mover` de
  una pieza número 13 (sin contar el Rey). La Máquina pasa esto sin conocer al Modificador
  real.
- **AC-6 (Logic, regla 6)** — **GIVEN** la fase `Reclutar` con tipos disponibles, **WHEN**
  llega un `Mover`, **THEN** `FaseIncorrecta`; **WHEN** llega `Reclutar(tipo válido)`,
  **THEN** pasa a `Mover`. **Y GIVEN** que al abrirla ningún tipo está disponible, **THEN**
  se emite `ReclutaPerdido` y el estado queda en `Mover`.
- **AC-7 (Logic, regla 7)** — **GIVEN** la fase `Mover` con todo el límite usado, **WHEN**
  llega un `Mover` de una pieza sin mover, **THEN** `SinMovimientos` y la fase sigue en `Mover`; **WHEN** llega
  `TerminarMovimiento`, **THEN** pasa a `Atacar` y la pila queda vacía.
- **AC-8 (Logic, regla 8)** — **GIVEN** un turno con tres `Mover` (piezas A, B, A),
  **WHEN** se envían tres `Deshacer`, **THEN** las piezas y sus movimientos usados vuelven
  en orden inverso y la huella es igual a la del inicio de `Mover`; un cuarto `Deshacer` da
  `NadaQueDeshacer`.
- **AC-9 (Logic, regla 8: qué no entra)** — **GIVEN** un turno donde se reclutó y luego se
  movió una pieza, **WHEN** se envía `Deshacer` dos veces, **THEN** la primera deshace el
  movimiento y la segunda da `NadaQueDeshacer` (el recluta sigue). **Y WHEN** en `Atacar` se
  envía `Deshacer`, **THEN** `FaseIncorrecta`.
- **AC-10 (Logic, regla 9)** — **GIVEN** la fase `Atacar`, **WHEN** se hacen
  `AsignarAtaque`, `QuitarAtaque` y `Cubrir` en cualquier orden, **THEN** todos se aceptan;
  **WHEN** llega `Resolver`, **THEN** el turno cierra y el estado queda en `EsperandoTirada`
  del rival.
- **AC-11 (Logic, regla 10: `Atacar`)** — **GIVEN** una tirada de Distracción, ni catapultas ni demoledores propios y ninguna
  pieza propia con objetivo válido ni escudero con aliado, **WHEN** se aplica la `Tirada`,
  **THEN** los eventos son `FaseSaltada(Atacar)`, `TurnoTerminado` y `TurnoIniciado`, y el
  estado queda en `EsperandoTirada` del rival sin esperar ningún timer.
- **AC-12 (Logic, regla 10: `Mover` y las dos)** — **GIVEN** un estado donde ninguna pieza
  tiene movimiento legal, **WHEN** se abre `Mover`, **THEN** `FaseSaltada(Mover)` y pasa a
  `Atacar`. **Y GIVEN** que también `Atacar` queda sin acción, **THEN** el turno cierra con
  `TurnoTerminado` sin `Resolver` y `EvaluateMatchEnd` se consulta igual.
- **AC-13 (Logic, regla 10: una fase empezada no termina sola)** — **GIVEN** `Mover` con una
  pieza y un solo movimiento legal, **WHEN** se usa ese movimiento, **THEN** la fase sigue en
  `Mover` hasta `TerminarMovimiento` o timeout.
- **AC-14 (Logic, regla 11)** — **GIVEN** un turno con Grito de guerra (bonos 2), ataques
  asignados, límite alterado y piezas movidas, **WHEN** el turno cierra por `Resolver`, por timeout o por saltos, **THEN** bonos 0, movimientos usados 0, asignaciones
  vacías, límite normal y `EvaluateMatchEnd` consultado.
- **AC-15 (Logic, regla 12: etapa mover en `Reclutar`)** — **GIVEN** `Reclutar` con soldados
  y arqueros disponibles, **WHEN** llega `TurnTimeout(turno, mover)`, **THEN** se recluta el
  primer tipo disponible en el orden del Catálogo, el evento lleva `automatico`, y el estado
  pasa **directo a `Atacar`** sin pasar por `Mover`.
- **AC-16 (Logic, regla 12: etapa mover en `Mover`)** — **GIVEN** `Mover` con 5 piezas ya
  movidas, **WHEN** llega `TurnTimeout(turno, mover)`, **THEN** las 5 piezas quedan donde
  están, no hay recluta, la pila queda vacía y pasa a `Atacar`.
- **AC-17 (Logic, regla 12: etapa atacar)** — **GIVEN** `Atacar` con 2 ataques asignados y
  una cobertura, **WHEN** llega `TurnTimeout(turno, atacar)`, **THEN** la lista de eventos
  de daño es idéntica a la de `Resolver` sobre el mismo estado. Sin ataques,
  `TurnoTerminado`.
- **AC-18 (Logic, regla 12: rechazos y carrera)** — **GIVEN** `EsperandoTirada`, **WHEN**
  llega un `TurnTimeout`, **THEN** `EsperandoTirada`. **Y GIVEN** `TerminarMovimiento` ya
  aceptado, **WHEN** llega `TurnTimeout(mismo turno, mover)`, **THEN** `TimeoutObsoleto` y la
  huella no cambia.
- **AC-19 (Logic, regla 13)** — **GIVEN** cuatro estados (un Rey muerto; los dos muertos con
  el turno del jugador 0; el Rey propio muerto por fuego amigo con el rival vivo; los dos
  vivos con vida 1), **WHEN** se consulta `EvaluateMatchEnd`, **THEN** `ReyMuerto` ganando el
  dueño del Rey vivo; `DobleRey` ganando el jugador 0; `ReyMuerto` ganando el rival; sin
  ganador.
- **AC-20 (Integration, regla 14; cruce con #12; con #28, PROVISIONAL)** — **GIVEN** modo
  Variable, turno de azul con 1 s en su reloj, **WHEN** pasa 1 s en `Mover` (y, en otra copia,
  en `Reclutar` y en `Atacar`), **THEN** la autoridad envía `CerrarPartida(RelojAgotado,
  rojo)`. **GIVEN** `EsperandoTirada`, **WHEN** pasan 60 s, **THEN** ningún reloj cambia.
  **GIVEN** el turno de rojo, **THEN** el reloj de azul no cambia. En Sin límite nunca lo envía.
- **AC-21 (Logic, reglas 14 y 15)** — **GIVEN** el turno de azul en `Atacar` con un ataque
  asignado, **WHEN** llega `CerrarPartida(RelojAgotado, rojo)`, **THEN** `Terminada`, gana
  rojo y ese ataque no se resuelve; **WHEN** llega con azul como ganador, **THEN**
  `CierreInvalido`. **Y GIVEN** un golpe de 30 de daño sobre una pieza rival de 5 de vida,
  **THEN** suma 5 a `vidaQuitada`, no 30; sobre una pieza propia, suma 0.
- **AC-22 (Logic, regla 16)** — **GIVEN** `Tirada` con reclutar perdido y sin acciones en
  `Mover` ni `Atacar`, **WHEN** se aplica, **THEN** los eventos de la Máquina son exactamente
  `ReclutaPerdido`, `FaseSaltada(Mover)`, `FaseSaltada(Atacar)`, `TurnoTerminado`,
  `TurnoIniciado`, en ese orden; y todo comando aceptado devuelve al menos un evento.
- **AC-23 (Logic, regla 17)** — **GIVEN** las 4 fases y los 6 grupos de comandos de la
  matriz, **WHEN** se prueba cada celda, **THEN** el código devuelto es el de la tabla (24
  celdas, generadas desde el enum de fases y comandos, no escritas a mano).
- **AC-24 (Logic, edge case)** — **GIVEN** la partida en cada una de las cuatro fases,
  **WHEN** llega `Rendirse` de cualquiera de los dos jugadores, **THEN** `Terminada`, gana el
  otro jugador y el motivo es Rendición.

### Fórmulas (Sección D)

- **AC-25 (Config/Data, D1; cruce con #19, PROVISIONAL)** — **GIVEN** el archivo de métricas
  de una partida (N = 24, 1.381 s), **WHEN** se calcula el presupuesto, **THEN** `seg_medio`
  = 57 y `turnos_presupuesto` = 26; con `seg_medio` = 0 se usa 1 y da 1.500. El archivo marca
  si la partida entró en 1.500 s.
- **AC-26 (Logic, D2; cruce con #12)** — **GIVEN** `T_tope` = 1.500, 1.291 y
  1.290, **WHEN** se calcula `reloj_jugador_s`, **THEN** da 750, 645 y 645; Sin límite no da
  reloj.
- **AC-27 (Logic, D4, con valores del límite)** — **GIVEN** el Catálogo vigente, **WHEN**
  `PrepararContexto` calcula `pila_max`, **THEN** da 254 y lo acepta. **Y GIVEN** un Catálogo
  de prueba con el Rey en movimiento 8 (suma 256), **THEN** lo acepta; con el Rey en
  movimiento 9 (suma 257) o el soldado en movimiento 5 (suma 266), **THEN**
  `ConfigInvalida`.
- **AC-28 (Logic, D4: peor caso real)** — **GIVEN** un estado de prueba con una Arremetida y
  todas las piezas del Catálogo vivas, **WHEN** se mueve cada pieza paso a paso hasta agotar
  su movimiento, **THEN** la pila llega a 254 entradas y todas se aceptan; ningún `Mover`
  da `LimitePartida`.

### Contratos entre sistemas

- **AC-29 (Config/Data, D1; cruce con #9, #10 y #11, PROVISIONAL)** — **GIVEN** los GDD de
  Resolución, Reclutamiento y Modificador ya escritos, **WHEN** se revisa su sección de
  dependencias, **THEN** cada uno nombra a la Máquina y declara su `Δturnos`.

- **AC-30 (UI, regla 14; cruce con #15, PROVISIONAL)** — **GIVEN** un modo con timer,
  **WHEN** se muestra el HUD en el turno de cada jugador, **THEN** se ven los dos relojes y
  corre solo el del jugador del turno; en `EsperandoTirada` no corre ninguno. En Sin límite no
  aparecen. Evidencia: captura retenida en `production/qa/evidence/`.
- **AC-31 (UI, Rendirse; cruce con #15, PROVISIONAL)** — **GIVEN** el jugador del turno con 8
  piezas contra 16, **THEN** la interfaz ofrece Rendirse; con 9 contra 16, no; al rival nunca
  se le ofrece en ese turno. Evidencia: captura retenida en `production/qa/evidence/`.

**Criterios que no se automatizan** (van a playtest): que la presión del timer se sienta
como "presión buscada" y no como castigo (Player Fantasy); que la partida dure 15–20
minutos (lo mide la compuerta de 3 partidas seguidas ≤ 25 min con las Métricas, #19).

**Huecos conocidos:** "la Máquina no conoce a ningún sistema por nombre" se prueba con un
Modificador de prueba (AC-5); la política de la regla 14 se prueba en #12 y #28 (AC-20);
`N_base` no se puede fijar (D1); la parte de `vidaQuitada` de AC-21 usa estados armados con el constructor de pruebas
hasta que Resolución (#9) escriba `vidaQuitada`; el estancamiento (nadie puede actuar) no
tiene criterio porque quedó como pregunta abierta.

## Open Questions

| Pregunta | Dueño | Para cuándo |
|---|---|---|
| ¿Los relojes se pausan durante una desconexión (online)? En el MVP no (Timer, #12) | Servidor (#28) | En su GDD |
| ¿Cómo se ven los dos relojes? (Los umbrales de aviso son del Timer, #12: 60 s y 10 s) | Ficha y HUD (#15) | En su GDD |
| ¿Las partidas Sin límite cuentan para la compuerta del arte (3 partidas seguidas ≤ 25 min)? Propuesta: no | Métricas (#19) | En su GDD |
| ¿El timeout durante `Reclutar` salta `Mover` y se siente como castigo? | Playtest | MVP |
| ¿El reloj decide partidas que el tablero no decidió? Medir cuántas terminan por reloj y quién iba adelante. Con pasividad total pierde azul, que empieza | Métricas (#19) | MVP |
| `N_base` y `Δturnos` de #9, #10 y #11 | Métricas (#19), #9, #10, #11 | MVP |
| Que ninguno de los dos jugadores pueda actuar durante varios turnos (casi inalcanzable; en Sin límite solo lo corta `C_MAX`) | Timer (#12) y Librería (ADR) | Antes de lanzar Sin límite |
