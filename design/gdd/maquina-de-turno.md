# Máquina de turno y fin de partida

> **Status**: In Design
> **Author**: jugador + Claude (design-system)
> **Last Updated**: 2026-11-20
> **Last Verified**: 2026-11-20
> **Implements Pillar**: 4 Duelo corto (dueña del presupuesto de duración); sirve a 1 Planificar y cosechar
> **Creative Director Review (CD-GDD-ALIGN)**: REVISED 2026-11-20 — la fantasía promete "no te quita lo ya hecho" y declara el límite del timeout en Reclutar; el aviso del cierre por tiempo es requisito (regla 14, AC-30)

## Summary

La **Máquina de turno** ordena cada turno de rolchess (tirada del Modificador, reclutar,
mover, atacar y resolver), guarda la pila de deshacer de los movimientos y decide cuándo
termina la partida. Existe para que el jugador planifique y vea caer sus golpes siempre en
el mismo orden, y para sostener el pilar de duelo corto con un tope de 25 minutos y un
desempate por vida del Rey, piezas vivas y vida quitada. Es parte del núcleo puro de C#: no
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

*Qué le exige al resto del diseño:* que agotar el tiempo nunca te quite lo ya hecho (lo
movido queda, lo asignado se resuelve, el recluta automático no te hace perder el recurso y
un comando que llega tarde se descarta sin ruido), y que el jugador vea siempre en qué
etapa está y cuánto le queda, también hasta el cierre de la partida. **Límite conocido:** si
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
3. **Primer turno (decisión del 2026-11-20).** Lo juega el jugador 0 y no tira el
   Modificador: empieza directo en `Mover`, sin `Reclutar`, porque sin tirada no hay cara de
   reclutar. *(Quién empieza es PROVISIONAL; ver Open Questions.)*
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
   Máquina solo guarda el límite del turno en el estado.
6. **Fase `Reclutar`.** Solo existe con `ReclutarObligatorio`. Si al abrirla ningún tipo
   está disponible → `ReclutaPerdido` y pasa a `Mover`. Si no, solo sale con
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
    del turno. Luego se corre `EvaluateMatchEnd` y, si no hay ganador, pasa el jugador, sube
    `numeroTurno` y se abre el siguiente turno (regla 4).
12. **`TurnTimeout` (definitivo; antes provisional, Librería regla 17).**
    - *Etapa mover:* si hay un `Reclutar` pendiente, la librería recluta el primer tipo
      disponible en el orden del Catálogo (evento con `automatico`); cierra la pila y pasa a
      `Atacar`. Las piezas ya movidas se quedan donde están.
    - *Etapa atacar:* resuelve los ataques ya asignados, igual que `Resolver`. Las
      coberturas valen.
    - Con `EsperandoTirada` se rechaza; un timeout de otro turno o etapa es
      `TimeoutObsoleto`. **Ningún timeout hace perder el plan.**
13. **`EvaluateMatchEnd(estado)`**, función pura. Sin Reyes vivos → `DobleRey`, gana el
    jugador del turno (**confirmado el 2026-11-20**: es quien planeó y vio la vista previa,
    pilar 1). Un solo Rey vivo → `ReyMuerto`, gana su dueño. Un Rey propio muerto por fuego
    amigo cuenta igual: gana el rival.
14. **Tope de 25 minutos (decisión del 2026-11-20: solo entre turnos, con umbral
    anticipado).** El tiempo lo mide la autoridad (Timer #12 local, Servidor #28 online); el
    núcleo no tiene reloj. Cuando `t_partida` llega al `umbral_cierre_s` (Formulas, D2:
    25:00 menos el turno máximo del modo), la autoridad envía
    `CerrarPartida(CierrePorTiempo)` **en el primer momento en que la partida está entre
    turnos** (en `EsperandoTirada` de un turno ≥ 2), nunca a mitad de un plan. Así ninguna
    partida con timer pasa de 25:00. **El modo Sin límite está exento** (no hay umbral). La
    Librería acepta el cierre en cualquier momento; esta restricción es política de quien
    envía y se verifica en las pruebas del Timer y del Servidor. **El jugador ve cuánto
    falta:** el aviso del cierre está visible desde `umbral_cierre_s − turno_max_s`
    (Variable: 1.380 s, 23:00), así que el último turno completo antes del cierre ya lo
    muestra y el corte nunca sorprende. El diseño del aviso es de Ficha y HUD (#15).
15. **Desempate de `CierrePorTiempo` (decisión del 2026-11-20, cambia la Librería).** Gana
    quien tenga: (1) más vida en el Rey; si empatan, (2) más piezas vivas (Rey incluido,
    muros no); si empatan, (3) más **vida realmente quitada al rival** (piezas y muros
    rivales, acumulada en la partida; el fuego amigo no cuenta; el daño que absorbe un
    escudero al cubrir cuenta); si también empatan, nadie. Resolución (#9) escribe el
    contador `vidaQuitada` por jugador. Es un campo nuevo del estado, así que sube la versión
    de reglas.
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
| (creación, castillos fijos o k = 1) | `Mover` (turno 1, jugador 0) | `CrearPartida`; sin tirada ni reclutar |
| (creación, k ≥ 2) | `EsperandoTirada` (sorteo) → `Mover` (turno 1) | `Tirada` del sorteo coloca castillos y piezas |
| `EsperandoTirada` (turno ≥ 2) | `Reclutar` | `Tirada` válida → `ReclutarObligatorio` |
| `EsperandoTirada` | `Mover` | `Tirada` → `Seguir` o `LimiteMovimientos(sin límite)` |
| `EsperandoTirada` | `Atacar` | `Tirada` → `SaltearMovimiento` |
| `Reclutar` | `Mover` | `Reclutar(tipo)`, recluta automático por timeout, o `ReclutaPerdido` al abrirla |
| `Mover` | `Atacar` | `TerminarMovimiento` o `TurnTimeout` de mover (la pila se vacía) |
| `Mover` o `Atacar` | siguiente fase / cierre | Salto automático (`FaseSaltada`) al abrirla sin acción posible |
| `Atacar` | `EsperandoTirada` (turno + 1) | `Resolver` o `TurnTimeout` de atacar, sin ganador |
| `Atacar` | `Terminada` | `Resolver` o `TurnTimeout` con ganador (`ReyMuerto`, `DobleRey`) |
| cualquier estado en curso | `Terminada` | `Rendirse`, o `CerrarPartida` (Abandono, `CierrePorTiempo` entre turnos, Tablas) |
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
| 12 Timer | `numeroTurno` y etapa actual | `TurnTimeout` y `CierrePorTiempo` entre turnos | Timer |
| 28 Servidor | Mismo pipeline | Es la autoridad de los comandos de autoridad | Servidor |
| 15 HUD, 19 Métricas, 25 Rival IA | Eventos de fase, etapa y pila | Nada (IA: mismos comandos que un jugador) | Cada GDD |

**Supuestos PROVISIONALES sobre GDD sin escribir:** el resultado `ReclutarObligatorio`
(#11 y #10), el límite normal de movimientos (#6), el contador `vidaQuitada` (#9) y que el
tope de 25 minutos aplica en los cuatro modos de timer (#12, abierta). Si las piezas
reclutadas pueden moverse ese turno queda para #10 y #6.

## Formulas

Hay dos fórmulas de diseño (D1, D2), que **no corren en el núcleo** porque no tiene reloj
(Librería, regla 5). Las usan el Timer (#12), el Servidor (#28) y las Métricas (#19). Las
otras dos (D3, D4) son reglas puras del núcleo.

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

### D2. Umbral de cierre por tiempo

La fórmula `umbral_cierre_s` se define así:

`umbral_cierre_s = T_tope − turno_max_s`

**Variables:**

| Variable | Type | Range | Source | Description |
|----------|------|-------|--------|-------------|
| T_tope | int | 1290–1500 (valor 1500) | data file (config de la autoridad) | Igual que en D1 |
| turno_max_s | int | 60–90 | data file (Timer #12) | Etapa de mover + etapa de atacar en vigor: Variable tras el minuto 10 = 60, Fijo 90 = 90, Fijo 60 = 60 |
| t_partida | int | 0–∞ | calculated (reloj de la autoridad) | Segundos de partida |
| umbral_cierre_s | int | 1410–1440 | calculated | Desde aquí, la autoridad cierra en el primer entre-turnos |

**Output Range:** Variable 1.440 s (24:00); Fijo 90 1.410 s (23:30); Fijo 60 1.440 s. **Sin
límite: exento**, no hay umbral (decisión del 2026-11-20).

**Garantía:** el último turno que puede abrirse empieza antes del umbral y dura como mucho
`turno_max_s`, así que termina antes de `T_tope`. Salvo la espera de la tirada y la latencia
de red, que mide el reloj de la autoridad y no el del turno.

**Example (Variable):** un turno empieza en 1.439 s y dura el máximo de 60 s → termina en
1.499 s. En el entre-turnos siguiente `t_partida ≥ 1.440`, la autoridad envía
`CierrePorTiempo`: la partida cierra en 24:59, bajo 25:00. Con el umbral exacto de 25:00, el
mismo turno cerraría hasta en 26:00.

**Límite del rango:** `T_tope` no baja de 1.290 s (1.200 de la meta de 20 min más 90 de
turno máximo del modo Fijo 90): por debajo, el umbral cae bajo 20:00 y corta partidas que
están dentro de la meta.

### D3. Desempate de `CierrePorTiempo`

La fórmula `ganador_tiempo` se define así:

`clave_j = (vidaRey_j, piezasVivas_j, vidaQuitada_j)`
gana el jugador de **mayor clave en orden lexicográfico**; si las claves son iguales, nadie.

**Variables:**

| Variable | Type | Range | Source | Description |
|----------|------|-------|--------|-------------|
| vidaRey_j | int | 1–30 | calculated (estado; `rey.vida` = 30 en el registro) | Vida del Rey de j (en partida en curso los dos Reyes viven) |
| piezasVivas_j | int | 1–61 | calculated (estado; `tope_vivas_por_jugador` = 61) | Piezas vivas de j, Rey incluido, muros no |
| vidaQuitada_j | int | 0–999.999.480 | calculated (estado; la escribe Resolución #9) | Vida **realmente perdida** por piezas y muros del rival por ataques de j, en toda la partida; sin fuego amigo; lo que absorbe un escudero al cubrir cuenta |

**Output Range:** un jugador (0 o 1) o vacío. **Cota de `vidaQuitada`:** como mucho
`estadistica_max` (1.000) × (`C_MAX` − 1 = 999.999 piezas posibles) + 480 de vida de los 16
tramos de muro rival = 999.999.480, muy por debajo de 2.147.483.647. No desborda.

**Example:** Reyes 30 y 30, piezas 11 y 11, `vidaQuitada` 412 (azul) y 388 (rojo) → gana
azul por el tercer criterio. Con Reyes 30 y 25 gana el del 30 sin mirar el resto. Con las
tres cifras iguales (pasividad total, 0 y 0) → nadie.

**Bucle positivo, nombrado:** quien va adelante gana más con que corra el reloj. Lo
amortiguan el timer de cada turno y que el rezagado puede atacar para cambiar la clave. No
se resuelve con una fórmula: lo vigilan las Métricas (Open Questions).

### D4. Capacidad de la pila de deshacer

La fórmula `pila_max` se define así:

`pila_max = Σ_t tope_vivas_t × movimiento_t` (t = los 7 tipos del Catálogo, Rey incluido)
**Invariante:** `pila_max ≤ pila_tope` (256). PrepararContexto rechaza con `ConfigInvalida`
un Catálogo que lo viole.

**Variables:**

| Variable | Type | Range | Source | Description |
|----------|------|-------|--------|-------------|
| tope_vivas_t | int | 1–61 | data file (Catálogo) | Piezas vivas máximas del tipo t |
| movimiento_t | int | 0–1.000 (hoy 2–6) | data file (Catálogo) | Puntos de movimiento del tipo t |
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

**PROVISIONAL hasta Movimiento (#6):** el peor caso supone que cada `Mover` gasta un solo
punto de movimiento. Si #6 define otra cosa, el máximo baja.

## Edge Cases

### Flujo del turno

- **Si Distracción sale y ninguna pieza propia tiene objetivo válido** (ni un escudero con
  aliado que cubrir): `Atacar` se salta con `FaseSaltada` y el turno cierra solo. Evita los
  30 s vacíos del prototipo.
- **Si `Reclutar` se abre y ningún tipo está disponible** (llamadas agotadas o sin lugar
  para todas sus piezas): `ReclutaPerdido` y pasa a `Mover`.
- **Si ya se usó todo el límite de movimientos:** la fase `Mover` sigue abierta hasta
  `TerminarMovimiento` o el timeout; un `Mover` más se rechaza con `SinMovimientos`.
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
- **Si `t_partida ≥ umbral_cierre_s` cuando termina un turno:** la autoridad comprueba el
  umbral **antes de enviar cada `Tirada`** y, en lugar de la tirada, envía
  `CierrePorTiempo`. Nunca se abre un turno pasado el umbral.
- **Si `CierrePorTiempo` llega mientras se espera el sorteo de castillos:**
  `CierreInvalido`, porque todavía no hay Reyes.
- **Si `Rendirse` llega en cualquier fase, también esperando tirada:** termina la partida y
  gana el otro jugador.
- **Si los dos jugadores solo dejan correr el reloj** (Reyes 30 y 30, mismas piezas,
  `vidaQuitada` 0 y 0): cada turno agota el timer, la partida llega al umbral y cierra por
  `CierrePorTiempo` sin ganador.

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

La Máquina **no depende** de Modificador, Reclutamiento, Movimiento, Alcance, Cobertura ni
Resolución: los llama por ganchos tipados sin conocerlos por nombre (índice, *Circular
Dependencies*). Por eso esos sistemas dependen de ella y no al revés.

**Dependen de este sistema:**

| Sistema | Tipo | Qué usa | Estado del GDD |
|---|---|---|---|
| #6 Movimiento | Dura | Fase `Mover`, pila de deshacer, límite del turno | Pendiente |
| #7 Alcance y asignación | Dura | Fase `Atacar` y el criterio "acción posible" | Pendiente |
| #8 Cobertura | Blanda | Momento de borrar las coberturas (al abrir el turno de su dueño) | Pendiente |
| #9 Resolución | Dura | Transición `Resolver`; escribe `vidaQuitada`; declara `Δturnos` | Pendiente |
| #10 Reclutamiento | Dura | Fase `Reclutar`, recluta automático; declara `Δturnos` | Pendiente |
| #11 Modificador | Dura | Resultados tipados y fase de tirada; declara `Δturnos` | Pendiente |
| #12 Timer | Dura | `numeroTurno`, etapa y `umbral_cierre_s`; emite `TurnTimeout` y `CierrePorTiempo` | Pendiente |
| #15 Ficha y HUD | Blanda | Fase, etapa y cuántos movimientos se pueden deshacer | Pendiente |
| #19 Métricas | Blanda | Eventos de fase, `TurnoTerminado`, `PartidaTerminada` | Pendiente |
| #21 Menú principal | Blanda | Inicia la partida y recibe `PartidaTerminada` | Pendiente |
| #25 Rival IA | Dura | Mismas fases y comandos que un jugador | Pendiente |
| #28 Servidor | Dura | Autoridad de `Tirada`, `TurnTimeout` y `CerrarPartida` | Pendiente |

**Bidireccionalidad.** La Librería ya lista a la Máquina como dependiente dura. Estos dos
puntos no están al día:

- El **Catálogo** no nombra a la Máquina como consumidora.
- El **índice** no tiene las aristas #3 → Catálogo ni #8, #9, #10 → Máquina.

Los GDD pendientes tienen que nombrar a la Máquina en su Dependencies al escribirse
(marcado para `/design-review` y `/review-all-gdds`).

## Tuning Knobs

**Perillas propias de la Máquina** (viven en la configuración de la autoridad y de las
Métricas, nunca en el código):

| Perilla | Valor | Rango seguro | Si es muy alta | Si es muy baja | Interactúa con | Dónde vive |
|---|---|---|---|---|---|---|
| `T_tope` (tope de partida) | 1.500 s | 1.290–1.500 | Más de 1.500 contradice el pilar 4: hay que revisar el pilar, no solo el número | Menos de 1.290 deja el umbral por debajo de 20:00 y corta partidas que están dentro de la meta | `turno_max_s`: el umbral es `T_tope − turno_max_s`, así que cada segundo que se baja `T_tope` adelanta el cierre un segundo. También acota `turnos_presupuesto` | Config de la autoridad (Timer #12 / Servidor #28) |
| Meta de duración | 900–1.200 s | mínimo 600 s; máximo `T_tope − turno_max_s` | Una meta pegada al umbral hace frecuentes los cierres por tiempo | Una meta por debajo de 900 s deja las métricas siempre en rojo | `T_tope` (su techo) y `seg_medio` | Config de Métricas (#19) |

**Perillas de otros sistemas que cambian esta GDD** (no se duplican acá, se apuntan):

| Valor | Dueño | Qué cambia aquí |
|---|---|---|
| Etapas del timer (60 + 30, 40 + 20) y minuto de cambio | Timer (#12) | `turno_max_s` y por lo tanto `umbral_cierre_s` (D2): Fijo 90 adelanta el umbral 30 s respecto de Fijo 60 |
| `movimiento` y `topeVivas` por tipo | Catálogo (#2) | `pila_max` (D4): hoy 254 de 256; subir el movimiento del soldado en 1 lo rompe |
| Límite normal de movimientos (12 piezas más el Rey) | Movimiento (#6) | Cuántas entradas llega a tener la pila en un turno normal |
| `Δturnos` de Resolución, Reclutamiento y Modificador | #9, #10, #11 | `turnos_estimados` contra `turnos_presupuesto` (D1) |
| Orden de tipos (soldado, arquero, escudero, catapulta, jinete, demoledor, Rey) | Catálogo (#2) | Qué tipo recluta el timeout (primer tipo disponible) |

**Constantes de la Máquina (no son perillas):** `pila_tope` = 256 (cambiarlo es un cambio de
contrato y sube la versión de reglas); el orden del desempate (vida del Rey, piezas vivas,
`vidaQuitada`); la lista cerrada de fases, de resultados tipados y de eventos.

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
| `PartidaTerminada` con `CierrePorTiempo` con ganador | Victoria o Derrota, y el resumen muestra la clave del desempate (vida del Rey, piezas vivas, vida quitada) para que se entienda por qué | **Falta** la clave en el resumen |
| `CierrePorTiempo` sin ganador | Pantalla neutral, luz base sin gradación, "Empate por tiempo" | **Falta** el estado |

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
- **Aviso del cierre por tiempo:** visible desde un turno completo antes del umbral (regla
  14), sin estado de ambiente propio que compita con la ficha (art bible, §2 *Regla de
  ambiente*). Cuándo aparece y cómo se ve lo define #15.
- **Pantalla de resultado:** motivo del fin y, para `CierrePorTiempo`, la clave del
  desempate.

📌 **UX Flag — Máquina de turno y fin de partida**: This system has UI requirements. In
Phase 4 (Pre-Production), run `/ux-design` to create a UX spec for each screen or HUD
element this system contributes to **before** writing epics. Stories that reference UI
should cite `design/ux/[screen].md`, not the GDD directly.

## Cross-References

| Este GDD dice | Documento | Elemento exacto | Naturaleza |
|---|---|---|---|
| Desempate: vida del Rey, piezas vivas, **vida quitada**, nadie | `libreria-de-reglas.md` | Reglas 3 y 16 | **Cambia:** agrega el tercer criterio |
| Campo `vidaQuitada` por jugador, versión de reglas | `libreria-de-reglas.md` | Regla 11 (tabla de dominios, matriz de clausura de `Resolver` y `TurnTimeout` de atacar), regla 12 | **Agrega** (cambio de contrato) |
| `Resolver` no es fase en reposo; sin etapa en `EsperandoTirada` | `libreria-de-reglas.md` | Regla 11, fila de fase y etapa | **Cambia** |
| Pila de 256 pasa a ser de la Máquina, con invariante de catálogo | `libreria-de-reglas.md` | Regla 11, fila de pila y Tuning Knobs | **Cambia** el dueño |
| `DobleRey`, `TurnTimeout` y primer turno sin tirada quedan definitivos | `libreria-de-reglas.md` | Reglas 2, 16 y 17; AC-20, AC-22, AC-31, AC-34, AC-38 | **Confirma**: se quitan las marcas PROVISIONAL |
| Se cierran cuatro preguntas abiertas (tope de 25 min, `TurnTimeout`, `DobleRey`, pila) | `libreria-de-reglas.md` | Open Questions | **Cierra** |
| El Catálogo tiene a la Máquina como consumidora; invariante `pila_max ≤ 256` | `catalogo-de-piezas.md` | Dependencies, Formulas (chequeo nuevo) y AC | **Agrega** |
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
  y sin `FaseIniciada(Reclutar)`. **Y GIVEN** castillos sorteados con k = 3, **WHEN** llega
  la `Tirada` del sorteo, **THEN** pasa a `Mover` del turno 1 sin pedir tirada del
  Modificador.
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
  llega un `Mover`, **THEN** `SinMovimientos` y la fase sigue en `Mover`; **WHEN** llega
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
- **AC-11 (Logic, regla 10: `Atacar`)** — **GIVEN** una tirada de Distracción y ninguna
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
  asignados y límite alterado, **WHEN** el turno cierra, **THEN** bonos 0, asignaciones
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
- **AC-20 (Integration, regla 14; cruce con #12 y #28, PROVISIONAL)** — **GIVEN** modo
  Variable, estado `EsperandoTirada` del turno 8 y `t_partida` = 1.440 s, **WHEN** la
  autoridad decide su próximo comando, **THEN** envía `CerrarPartida(CierrePorTiempo)` y no
  una `Tirada`; con `t_partida` = 1.439 s envía `Tirada`. En Sin límite nunca envía el
  cierre. Nunca lo envía con el estado en `Reclutar`, `Mover` o `Atacar`.
- **AC-21 (Logic, regla 15 y D3)** — **GIVEN** cinco estados con las claves (30, 11, 412)
  contra (30, 11, 388); (30, 12, 0) contra (30, 11, 900); (30, 11, 0) contra (25, 30, 900);
  (30, 11, 0) contra (30, 11, 0); y un golpe de 30 de daño sobre una pieza de 5 de vida,
  **WHEN** se aplica `CerrarPartida(CierrePorTiempo)`, **THEN** gana el primero; gana el
  primero; gana el primero; sin ganador; y ese golpe suma 5 a `vidaQuitada`, no 30.
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
- **AC-26 (Logic, D2; cruce con #12, PROVISIONAL)** — **GIVEN** `T_tope` = 1.500, **WHEN** se
  calcula `umbral_cierre_s`, **THEN** Variable (tras el minuto 10) da 1.440, Fijo 90 da
  1.410, Fijo 60 da 1.440, y Sin límite no da umbral.
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

- **AC-30 (UI, regla 14; cruce con #15, PROVISIONAL)** — **GIVEN** modo Variable con
  `t_partida` = 1.380 s, **WHEN** se muestra el HUD, **THEN** el aviso del cierre es
  visible; con `t_partida` = 1.379 s no lo es. En Sin límite nunca aparece. Evidencia:
  captura retenida en `production/qa/evidence/`.

**Criterios que no se automatizan** (van a playtest): que la presión del timer se sienta
como "presión buscada" y no como castigo (Player Fantasy); que la partida dure 15–20
minutos (lo mide la compuerta de 3 partidas seguidas ≤ 25 min con las Métricas, #19).

**Huecos conocidos:** "la Máquina no conoce a ningún sistema por nombre" se prueba con un
Modificador de prueba (AC-5); la política de la regla 14 se prueba en #12 y #28 (AC-20);
`N_base` no se puede fijar (D1); AC-21 usa estados armados con el constructor de pruebas
hasta que Resolución (#9) escriba `vidaQuitada`; el estancamiento (nadie puede actuar) no
tiene criterio porque quedó como pregunta abierta.

## Open Questions

| Pregunta | Dueño | Para cuándo |
|---|---|---|
| ¿Quién empieza (hoy el jugador 0) y hay compensación por mover primero? | Tablero (#4) | En su GDD |
| ¿Las piezas reclutadas pueden moverse ese turno? | Reclutamiento (#10) y Movimiento (#6) | En sus GDD |
| ¿`t_partida` se pausa cuando se espera la tirada o hay una desconexión (online)? | Timer (#12) y Servidor (#28) | En sus GDD |
| ¿Cómo se ve y cuándo aparece el aviso del cierre por tiempo (forma, sonido)? El mínimo ya está fijado: visible un turno completo antes del umbral | Ficha y HUD (#15) y Timer (#12) | En sus GDD |
| ¿Las partidas Sin límite cuentan para la compuerta del arte (3 partidas seguidas ≤ 25 min)? Propuesta: no | Métricas (#19) | En su GDD |
| ¿El timeout durante `Reclutar` salta `Mover` y se siente como castigo? | Playtest | MVP |
| ¿El desempate por vida quitada evita premiar el juego pasivo? Medir la proporción de cierres por tiempo ganados por quien iba adelante a mitad de partida | Métricas (#19) | MVP |
| `N_base` y `Δturnos` de #9, #10 y #11 | Métricas (#19), #9, #10, #11 | MVP |
| Que ninguno de los dos jugadores pueda actuar durante varios turnos (casi inalcanzable; en Sin límite solo lo corta `C_MAX`) | Timer (#12) y Librería (ADR) | Antes de lanzar Sin límite |
| Estado visual de "Empate por tiempo" | Render (#18) y art bible | En su GDD |
