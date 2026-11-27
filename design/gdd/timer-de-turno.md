# Timer de turno

> **Status**: Designed (pendiente de `/design-review` en una sesión nueva)
> **Author**: jugador + Claude (design-system)
> **Last Updated**: 2026-11-25
> **Last Verified**: 2026-11-25
> **Implements Pillar**: 4 Duelo corto (ejecuta el reloj que la Máquina de turno define); sirve a 1 Planificar y cosechar (el aviso a tiempo protege la jugada)
> **Creative Director Review (CD-GDD-ALIGN)**: CONCERNS (accepted) 2026-11-25 (autoevaluación; el jugador las aceptó como riesgos a medir en el MVP): (1) el tramo final de 40 s para mover puede pelear con el pilar 1 en pantalla táctil; (2) el reloj por jugador puede decidir una partida que el pilar 3 (siempre hay vuelta) habría salvado. Las dos se miden en playtest (Open Questions).

## Summary

El **Timer de turno** mide el tiempo de una partida de rolchess y lo traduce a las dos órdenes que el núcleo entiende: `TurnTimeout` (se acabó la etapa de mover o de atacar) y `CerrarPartida(RelojAgotado)` (a un jugador se le acabó su reloj de 12:30). Es el dueño de los cuatro modos de timer, de la duración de cada etapa, del tramo final y de lo que el jugador ve del tiempo. Existe porque el núcleo no tiene reloj: sin el Timer, el pilar de duelo corto no tendría quién lo haga cumplir.

> **Quick reference** — Layer: `Core` · Priority: `MVP` · Key deps: `Máquina de turno (#3), Librería de reglas (#1)`

## Overview

El **Timer de turno** es el servicio, **fuera del núcleo puro de C#**, que sabe qué hora es. Corre en el cliente en las partidas locales y en el servidor en las online (Servidor, #28). Después de cada comando aceptado mira en qué turno, jugador y etapa está la partida, y con eso hace tres cosas: **arma el timer de la etapa** (mover, que incluye reclutar, o atacar), **descuenta el reloj de partida** del jugador del turno (Máquina de turno, regla 14) y, cuando algo vence, **envía el comando de autoridad** que corresponde. También mantiene la **vista de relojes** que el HUD (#15) dibuja y los avisos de "queda poco".

Es dueño de: los cuatro modos (Variable 90 → 60, Fijo 90, Fijo 60, Sin límite), las duraciones de etapa, el umbral del tramo final, las reglas de pausa, los umbrales de aviso y la medición de cuánto dura cada turno (para las Métricas, #19). **No** es dueño de qué pasa cuando algo vence: eso es de la Máquina de turno (reglas 12 y 14) y de la Librería (reglas 3, 16 y 17), que el Timer solo invoca. Tampoco decide el tope de 25 minutos ni el reloj de 12:30: los recibe de la Máquina (`tope_partida_s`, `reloj_jugador_s`).

Es una capa técnica, pero el jugador la siente más que a ninguna otra: es el anillo que se cierra, el reloj que baja y el aviso que llega o no llega a tiempo.

*Alternativa descartada:* que el núcleo mida el tiempo. Una repetición ya no reproduciría la partida (el reloj no es determinista), y la Librería prohíbe el reloj (regla 13). El núcleo solo recibe `TurnTimeout` y `CerrarPartida`, así que una repetición reproduce el timeout sin reloj.

→ Cómo se implementa (fuente de tiempo inyectada, estructura de clases, ciclo de ticks del servidor) queda para el ADR del Timer.

## Player Fantasy

Te quedan 18 segundos de la etapa de mover y 3:40 de tu reloj. El rival tiene 5:10. Sabés exactamente cuánto es cada número porque los dos relojes están a la vista todo el tiempo. Decidís: soltás la catapulta donde cierra el golpe y dejás las otras dos piezas para el próximo turno. El anillo se pone ámbar a los 10 segundos y pulsa una vez por segundo, pero ya tenías la jugada armada. Pasás a atacar con tiempo de sobra.

Más adelante se acaban los 10 minutos de partida: aparece una sola vez el banner "Tramo final", la luz baja al atardecer y los turnos pasan a 40 + 20. Lo notás, lo esperabas y te alcanza para jugar apurado pero no a ciegas.

La fantasía es **administrar el tiempo como un recurso propio, bajo una presión que se siente justa**: el reloj es igual para los dos, nunca vence por sorpresa, siempre sabés cuánto te queda, y quedarte sin tiempo te hace decidir, no te borra lo que ya hiciste (eso lo garantiza la Máquina de turno).

*Pilar citado (pilar 4):* "El timer es presión buscada: quedarse sin tiempo para mover o para atacar no es una falla sino lo que obliga a concentrarse."

*Encuadres considerados (lente del director creativo):*

| Encuadre | Momento | Qué le exige al resto del diseño |
|---|---|---|
| A. El anillo que aprieta | Los últimos 10 s de mover, con el anillo ámbar pulsando | Aviso preciso, un solo anillo, sin destellos en el tablero |
| **B. El reloj como recurso (elegido)** | Con 3:40 en tu reloj y 5:10 en el del rival, elegís jugar rápido este turno | Los dos relojes siempre visibles y legibles; un reloj que no se pierda en esperas de red |
| C. El atardecer | El banner "Tramo final" y el cambio de luz | Anuncio único y una transición de luz, nada más |

Se eligió B porque es la única que le da al jugador una decisión sobre el tiempo y no solo una reacción; A y C quedan como la forma en que se siente, no como la fantasía. **Límite conocido:** un tramo final de 40 s para mover hasta 12 piezas puede ser poco en pantalla táctil (concepto, riesgos); se mide en playtest.

## Detailed Design

### Core Rules

1. **Qué es y dónde corre.** El Timer no es un comando ni parte del núcleo: es un servicio que **lee** el estado de la partida y **envía** comandos de autoridad. Una instancia por partida. En partida **local**, el cliente ejerce de autoridad (la Librería acepta el emisor autoridad) y el Timer envía los comandos. **Online**, la política corre en el Servidor (#28); el Timer del cliente es solo vista y **nunca envía comandos** (regla 15).
2. **Entradas y salidas.** *Entradas:* la config de la partida (el modo), una **fuente de tiempo monotónica en milisegundos enteros** (inyectada, nunca la hora del sistema) y, después de cada comando aceptado, el estado: `numeroTurno`, jugador del turno, **etapa del timer** (Máquina de turno, regla 2: mover para `Reclutar` y `Mover`, atacar para `Atacar`, ninguna en `EsperandoTirada`) y si la partida sigue en curso. *Salidas:* `TurnTimeout(turno, etapa)`, `CerrarPartida(RelojAgotado, rival, numeroTurno)` y, para `LimitePartida`, `CerrarPartida(Tablas, vacío, vacío)` (regla 9); los eventos del Timer (regla 12) y la vista de relojes (regla 13). No escribe el estado, no lee el Catálogo y no conoce las fases por nombre: solo la etapa.
3. **Modos (lista cerrada).**

   | Modo | Mover / atacar normal | Mover / atacar en tramo final | Reloj de partida | Dónde se elige |
   |---|---|---|---|---|
   | **Variable 90 → 60** | 60 s / 30 s | 40 s / 20 s | 750 s por jugador | Local y online (online solo este) |
   | **Fijo 90** | 60 s / 30 s | igual que el normal | 750 s por jugador | Solo local |
   | **Fijo 60** | 40 s / 20 s | igual que el normal | 750 s por jugador | Solo local |
   | **Sin límite** | sin tope | sin tope | **no hay reloj** | Solo local |

   El modo es config de la partida: lo elige el Menú (#21) al crearla, Ajustes (#20) recuerda la última elección y la Librería lo recibe como dato informativo que no entra al estado. Los dos jugadores usan el mismo modo: no hay compensaciones ni ventajas por jugador. **Un modo ausente o desconocido no se reemplaza por otro:** el Timer rechaza crear la partida con `ModoTimerInvalido`. Online con un modo distinto de Variable también es `ModoTimerInvalido`.
4. **Fuente de tiempo y unidades.** Todo se mide en **milisegundos enteros** de un reloj monotónico inyectado; no hay coma flotante en ningún cálculo del Timer, aunque el Timer esté fuera del núcleo. El tiempo se **acumula por diferencia de marcas de tiempo**, nunca contando cuadros, así que la tasa de cuadros no produce deriva.
5. **Identidad de la etapa.** Una etapa es el par **(`numeroTurno`, etapa)**. Después de cada comando aceptado el Timer compara el par con el que tenía armado; si cambió, cierra la etapa anterior y arma la nueva. Esto cubre sin casos especiales: `Reclutar` → `Mover` (misma etapa, **no reinicia**: reclutar gasta tiempo de mover), `Mover` → `Atacar` (etapa nueva, timer completo), el salto de `Mover` por Distracción (no se arma mover) y el turno donde se saltan todas las fases (no se arma nada). **El tiempo sobrante de una etapa no pasa a la siguiente.**
6. **Duración de la etapa y tramo.** Al armar la **primera** etapa del turno, el Timer fija el **tramo del turno** (normal o final, Fórmulas, D2) y lo mantiene para las dos etapas de ese turno. La duración sale de la tabla del modo (regla 3) y **no cambia una vez armada**: un turno que cruza el umbral a la mitad termina con las duraciones que empezó.
7. **Reloj de partida.** El reloj del jugador del turno **corre** mientras haya una etapa armada, la partida esté en curso y no esté en pausa; se descuenta del reloj de ese jugador. **No corre en `EsperandoTirada`** (el jugador no paga la demora de la autoridad; Máquina, regla 14). Los dos relojes empiezan en `reloj_jugador_s` (750 s). En Sin límite no existen.
8. **Vencimientos y su orden.** La autoridad evalúa vencimientos **con la hora de recepción de cada comando, antes de aplicarlo**, y también en cada tick. Un instante vence cuando `restante ≤ 0` (el 0 exacto ya venció). En orden:
   1. Si el reloj del jugador del turno venció → `CerrarPartida(RelojAgotado, rival, numeroTurno)`, con el `numeroTurno` en que venció (Librería, regla 3: sin él da `CierreInvalido`). Si en el mismo instante también venció la etapa, **vale el reloj** y no se envía `TurnTimeout`.
   2. Si no, y venció la etapa → `TurnTimeout(numeroTurno, etapa)`, **una sola vez por etapa**.
   
   Un comando de un jugador que llega **en o después** del instante de vencimiento llega tarde: el vencimiento se envía primero y el comando se rechaza (`ComandoViejo`, `TimeoutObsoleto`, `PartidaTerminada`, según el caso) sin ruido. Nada se aplica de forma retroactiva.
9. **Respuesta de la Librería a lo que envía el Timer.** Aceptado: el estado cambió y la regla 5 arma lo que sigue. `TimeoutObsoleto`: se descarta sin aviso, ya cambió el turno o la etapa (`ComandoViejo` no aplica a `TurnTimeout`: la fila 10 de la Librería es solo de comandos de jugador). `PartidaTerminada`: se detiene el Timer. `LimitePartida` es solo una defensa (Librería, fila 15: el recluta automático llevaría el próximo ID por encima de C_MAX): el Timer envía `CerrarPartida(Tablas, vacío, vacío)`. Tablas es el cierre técnico sin ganador de la Librería (regla 3, decisión del 2026-11-27). El tope de `contadorComandos` ya no da `LimitePartida`: el Aceptado que lo deja en C_MAX − 1 termina la partida en Tablas por sí solo (Librería, regla 16), y el Timer lo ve como `PartidaTerminada`. `CierreInvalido` a un RelojAgotado es una **carrera esperable**, no un error: si Resolver o TerminarMovimiento cerraron antes el turno, su `turno` ya no es `numeroTurno` (Librería, Edge Cases); el Timer lo descarta sin reportarlo y reevalúa con el estado nuevo.
10. **Pausa.** `Pausar(motivo)` y `Reanudar(motivo)`, con motivos de lista cerrada. Pausado no se acumula tiempo, no hay vencimientos y la vista lo dice. Motivos: `SuspensionLocal` (el sistema suspende la aplicación, solo en local; **perder el foco no pausa**, decisión del jugador 2026-11-25, para que cambiar de ventana no dé tiempo gratis) y `Desconexion` (reservado para online; **PROVISIONAL**, el Servidor decide si lo usa; en el MVP no se usa). **No hay botón de pausa manual en el MVP.** Abrir el panel «?» del Modificador (#17) **no** es un motivo de pausa: consultar las probabilidades cuesta tiempo real (cierre de la tanda 3). En local, además, si entre dos marcas de tiempo pasan más de `salto_reloj_max_ms` (5.000 ms) la diferencia **no se cuenta** (equipo dormido, congelamiento largo): el Timer lo trata como una suspensión que no avisó. Online esto no aplica: el tiempo lo mide el servidor.
11. **Qué cuenta el jugador.** El Timer es el único dueño de estas decisiones, y las toman todos igual: el tramo se evalúa por **tiempo consumido** y no por hora del reloj de pared (Fórmulas, D2); el sobrante no se acumula (regla 5); la pausa no descuenta (regla 10); los eventos de la Máquina no frenan el reloj (Máquina, Visual/Audio: ninguno suma tiempo al turno); y la presentación de la resolución no corre sobre ningún reloj en local: la `Tirada` espera al fin de la cosecha (Render, #18, regla 25; Máquina, regla 14), el reloj no corre en `EsperandoTirada` y el techo es `resolucion_max_s` = 2,0 s. Online sigue abierto (#28) (decisión del 2026-11-26; cierra W-2).
12. **Eventos del Timer (lista cerrada, de presentación, no entran al núcleo).** `EtapaIniciada(numeroTurno, jugador, etapa, duracion_s)`; `AvisoEtapa` (la etapa llega a `aviso_etapa_s`, una vez por etapa); `AvisoReloj(jugador, nivel)` (nivel 1 a `aviso_reloj_s`, nivel 2 a `aviso_reloj_critico_s`, una vez por jugador y nivel); `TramoFinalIniciado` (una sola vez por partida); `TiempoAgotado(etapa | reloj)` (se detectó el vencimiento, antes de ver la respuesta, para que la interfaz reaccione de inmediato); `RelojPausado(motivo)` y `RelojReanudado`; y, para las Métricas, `TurnoMedido(numeroTurno, jugador, ms_mover, ms_atacar, vencio_mover, vencio_atacar)` al cerrar cada turno y `RelojesAlCierre(restante_ms[2])` al terminar la partida.
13. **Vista de relojes (solo lectura).** Una instantánea que el HUD (#15) lee en cada cuadro: `modo`, `jugadorActivo`, `etapa` (o ninguna), `restanteEtapa_s` y `restanteReloj_s[2]` (ya redondeados, Fórmulas, D4), `tramo`, `vinculante` (qué vence antes: `Reloj` o `Etapa`), `nivelAvisoReloj[2]` (0, 1 o 2), `pausado` (con el motivo) y `enEsperaDeTirada`. En Sin límite: `modo` = Sin límite y sin tiempos. El HUD **decide cómo se ve**; el Timer decide qué números y cuándo avisa.
14. **Medición en todos los modos.** El Timer mide cuánto dura cada turno aunque no haya límite (`TurnoMedido` se emite también en Sin límite). Si esas partidas cuentan para la compuerta del arte lo decide Métricas (#19).
15. **Online.** La política (reglas 5 a 9) la ejecuta el Servidor. El Timer del cliente muestra una **predicción** anclada en lo que el servidor confirma: cada vez que el servidor acepta un comando manda los restantes de etapa y de reloj y el cliente sigue contando con su fuente monotónica. Si la predicción llega a 0 antes de que el servidor decida, la vista se queda en 0 con `enEsperaDeTirada` falso y un estado "esperando a la autoridad"; la decisión es siempre del servidor. *PROVISIONAL: el formato del mensaje y la corrección de la predicción son del Servidor (#28).*
16. **Rival IA.** El Rival IA (#25) emite los mismos comandos que un jugador. Si sus turnos descuentan del reloj, y si la partida tutorial usa timers, lo decide su GDD (Open Questions).

### States and Transitions

Estados del Timer (no del núcleo):

| Estado | Qué significa | Corre algún reloj |
|---|---|---|
| `SinPartida` | Todavía no hay partida creada, o la config fue rechazada | No |
| `EnEspera` | Partida en curso en `EsperandoTirada`, o turno con todas las fases saltadas | No |
| `Corriendo` | Hay una etapa armada y no hay pausa | Etapa y reloj del jugador del turno (en Sin límite, solo la medición) |
| `Pausado` | Hay etapa armada pero pausada | No |
| `Terminado` | La partida terminó por cualquier motivo | No |

| Desde | Hacia | Cuándo |
|---|---|---|
| `SinPartida` | `Corriendo` | Partida creada y el estado tiene etapa (turno 1: entra directo a `Mover`) |
| `SinPartida` | `SinPartida` | Config rechazada con `ModoTimerInvalido` |
| `EnEspera` | `Corriendo` | Un comando aceptado deja una etapa nueva (la `Tirada` abrió `Reclutar`, `Mover` o `Atacar`) |
| `Corriendo` | `Corriendo` | Cambia el par (turno, etapa): `Mover` → `Atacar` (se cierra una etapa y se arma otra) |
| `Corriendo` | `EnEspera` | El turno cerró (`Resolver`, `TurnTimeout` de atacar, salto automático) y la etapa pasó a ninguna |
| `Corriendo` | `Pausado` | `Pausar(motivo)` o salto de reloj mayor a `salto_reloj_max_ms` |
| `Pausado` | `Corriendo` | `Reanudar(motivo)` |
| `Pausado` | `Terminado` | `Rendirse`, `CerrarPartida` de otra autoridad o fin por Rey, mientras estaba en pausa |
| `Corriendo` o `EnEspera` | `Terminado` | `PartidaTerminada` (Rey muerto, rendición, abandono, tablas, `RelojAgotado`) |
| `Terminado` | `Terminado` | Todo vencimiento posterior se descarta |

### Interactions with Other Systems

| Sistema | El Timer le da | Recibe | Dueño de la interfaz |
|---|---|---|---|
| 3 Máquina de turno | `TurnTimeout`, `CerrarPartida(RelojAgotado)` | `numeroTurno`, jugador del turno, etapa, partida en curso, `reloj_jugador_s`, `tope_partida_s` | Máquina (qué hacen los comandos y los valores del reloj); Timer (cuándo se envían) |
| 1 Librería de reglas | Los comandos de autoridad | Códigos de rechazo (`TimeoutObsoleto`, `PartidaTerminada`, `LimitePartida`, `CierreInvalido`) | Librería |
| 28 Servidor | La política (reglas 5 a 9) como biblioteca; el formato de la vista online | Hora autoritativa, desconexión | Servidor (transporte y mensajes), Timer (política) |
| 15 Ficha y HUD | La vista de relojes y los eventos | Nada | HUD (cómo se ve); Timer (qué se ve y cuándo avisa) |
| 19 Métricas | `TurnoMedido`, `RelojesAlCierre`, `TiempoAgotado` | Nada | Métricas |
| 21 Menú y 20 Ajustes | El conjunto de modos y su validación | El modo elegido; Ajustes guarda el último | Timer (la lista de modos); Menú (la pantalla); Ajustes (la memoria) |
| 22 Audio y 18 Render | Eventos de aviso y de tramo final | Nada | Audio y Render (cómo suena y se ve) |
| 25 Rival IA | Nada propio: se aplica lo mismo que a un jugador | Nada | Rival IA |

**Supuestos PROVISIONALES sobre GDD sin escribir:** el formato de los restantes online y la pausa por desconexión (#28); si el Rival IA descuenta reloj (#25).

## Formulas

Hay cuatro fórmulas. Ninguna corre en el núcleo (no tiene reloj, Librería regla 13): las ejecuta el Timer en el cliente o en el servidor. `T_tope` y `reloj_jugador_s` son de la Máquina de turno (D1, D2) y acá se **usan**, no se redefinen.

### D1. Duración de la etapa y del turno

La fórmula `dur_etapa_ms` se define así:

`dur_etapa_ms = dur_s[modo][tramo][etapa] × 1000`
`turno_max_s[modo][tramo] = dur_s[modo][tramo][mover] + dur_s[modo][tramo][atacar]`

**Variables:**

| Variable | Type | Range | Source | Description |
|----------|------|-------|--------|-------------|
| modo | enum | Variable, Fijo 90, Fijo 60, Sin límite | data file (config de la partida) | Modo elegido |
| tramo | enum | normal, final | calculated (D2) | Tramo del turno |
| etapa | enum | mover, atacar | calculated (estado de la Máquina) | Etapa armada |
| dur_s | int | 15–90 (valores 60, 30, 40, 20; cada perilla tiene su rango propio en Tuning Knobs) | data file (config del Timer: `dur_mover_normal_s`, `dur_atacar_normal_s`, `dur_mover_final_s`, `dur_atacar_final_s`) | Segundos de la etapa según modo y tramo |
| turno_max_s | int | 45–125 (valores 90 y 60; 125 es el tope de la guarda de Tuning Knobs) | calculated | Lo máximo que puede durar un turno completo; Sin límite no tiene |

**Output Range:** `dur_etapa_ms` de 15.000 a 90.000 ms. En Sin límite no hay duración: la fórmula no se evalúa.

**Example:** modo Variable, tramo normal, etapa mover: 60 × 1000 = 60.000 ms; `turno_max_s` = 60 + 30 = 90. Modo Fijo 60: 40.000 ms para mover y 20.000 ms para atacar; `turno_max_s` = 60.

**Chequeo de frontera:** con la suma de etapas en su máximo del rango (90 + 45 = 135 s) caben 5 turnos completos en 750 s, lo que no deja jugar una partida de 15 turnos; por eso la guarda de la sección Tuning Knobs: `turno_max_s ≤ reloj_jugador_s ÷ 6` (125 s con 750).

### D2. Tiempo consumido y tramo

La fórmula `tramo` se define así:

`consumido_total_ms = consumido_ms[0] + consumido_ms[1]`
`tramo = final  si  modo = Variable  y  consumido_total_ms ≥ umbral_tramo_final_s × 1000;  normal  si no`

**Variables:**

| Variable | Type | Range | Source | Description |
|----------|------|-------|--------|-------------|
| consumido_ms[j] | int | 0 – `reloj_jugador_s` × 1000 (750.000) | calculated | Milisegundos que corrió el reloj del jugador j: solo durante sus etapas y sin pausa |
| umbral_tramo_final_s | int | 300–1.100 (valor 600) | data file (config del Timer) | Segundos consumidos (suma de los dos) en que arranca el tramo final |

**Output Range:** `consumido_total_ms` de 0 a 1.500.000 (`tope_partida_s` × 1000). El tramo se evalúa una vez por turno (regla 6) y solo sube: no vuelve a normal.

**Equivalencia útil:** `consumido_total_ms = 2 × reloj_jugador_s × 1000 − restante_ms[0] − restante_ms[1]`; la suma de los dos restantes sirve de control cruzado.

**Example:** azul consumió 360 s y rojo 270 s: `consumido_total_ms` = 630.000 ≥ 600.000 → el turno que abre es de tramo final (40 + 20). Con 599.999 ms sigue normal.

**Chequeo de frontera:** con `umbral_tramo_final_s` = 0 el primer turno ya es final (el modo Variable se vuelve Fijo 60: la perilla pierde sentido); con un valor mayor o igual a `tope_partida_s` el tramo final no llega nunca (se vuelve Fijo 90). De ahí el rango seguro 300–1.100.

### D3. Tiempo restante y qué vence primero

La fórmula `vence_en_ms` se define así:

`restante_etapa_ms = max(0, dur_etapa_ms − (ahora_ms − inicio_etapa_ms − pausado_ms))`
`restante_reloj_ms[j] = max(0, reloj_jugador_s × 1000 − consumido_ms[j])`
`vence_en_ms = min(restante_etapa_ms, restante_reloj_ms[jugadorActivo])`
`vinculante = Reloj  si  restante_reloj_ms[jugadorActivo] ≤ restante_etapa_ms;  Etapa  si no`

**Variables:**

| Variable | Type | Range | Source | Description |
|----------|------|-------|--------|-------------|
| ahora_ms | int | 0–∞ | calculated (fuente monotónica inyectada) | Marca de tiempo actual |
| inicio_etapa_ms | int | 0–`ahora_ms` | calculated | Marca en que se armó la etapa |
| pausado_ms | int | 0–`dur_etapa_ms` | calculated | Tiempo en pausa o descartado por salto dentro de la etapa |
| dur_etapa_ms | int | 15.000–90.000 | calculated (D1) | Duración de la etapa |
| reloj_jugador_s | int | 645–750 (valor 750) | data file (Máquina, D2) | Segundos de reloj de cada jugador |

**Output Range:** todos los restantes van de 0 a su máximo; `max(0, …)` evita negativos si una marca llega tarde. `vence_en_ms` ≤ `dur_etapa_ms`. Con restante en 0 se vence (regla 8). Empates: vale `Reloj` (la regla 8, orden 1).

**Example:** modo Variable, tramo normal. Azul abre su turno con 25 s de reloj (`restante_reloj_ms` = 25.000) y la etapa de mover dura 60 s: `vence_en_ms` = 25.000, `vinculante` = Reloj. A los 25 s de mover la autoridad envía `CerrarPartida(RelojAgotado, rojo, numeroTurno)`; el timer de etapa **nunca** llega a vencer. Segundo caso: el reloj de azul tiene 400 s y la etapa 60 s → `vinculante` = Etapa.

**Chequeo de frontera:** con `restante_reloj_ms` = 0 al armar una etapa el cierre es inmediato (partida terminada). Con `pausado_ms` igual a la duración de la etapa, `restante_etapa_ms` = `dur_etapa_ms` (no corrió nada).

### D4. Número mostrado y umbrales de aviso

La fórmula `mostrado_s` se define así:

`mostrado_s = ⌈restante_ms ÷ 1000⌉`, en enteros: `(restante_ms + 999) ÷ 1000`
`AvisoEtapa  si  restante_etapa_ms ≤ aviso_etapa_s × 1000`
`AvisoReloj nivel 1  si  restante_reloj_ms[j] ≤ aviso_reloj_s × 1000;  nivel 2  si  ≤ aviso_reloj_critico_s × 1000`

**Variables:**

| Variable | Type | Range | Source | Description |
|----------|------|-------|--------|-------------|
| restante_ms | int | 0–750.000 | calculated (D3) | Restante de etapa o de reloj |
| mostrado_s | int | 0–750 | calculated | Entero que dibuja el HUD (los relojes, en m:ss) |
| aviso_etapa_s | int | 5–15 (valor 10) | data file (config del Timer; art bible §2, "Timer en los últimos 10 s") | Umbral de aviso de la etapa |
| aviso_reloj_s | int | 30–120 (valor 60) | data file (config del Timer) | Umbral de nivel 1 del reloj de partida |
| aviso_reloj_critico_s | int | 5–15 (valor 10) | data file (config del Timer) | Umbral de nivel 2 del reloj de partida |

**Output Range:** `mostrado_s` es 0 **solo** con `restante_ms` = 0, es decir cuando ya venció: el jugador nunca ve "0" con tiempo para actuar. Los avisos suben de nivel solo una vez cada uno.

**Example:** `restante_ms` = 59.001 → `mostrado_s` = 60; = 1 → 1; = 0 → 0. Un reloj con 60.000 ms dispara el nivel 1 y uno con 10.000 ms el nivel 2.

**Chequeo de frontera:** con `aviso_reloj_s` ≤ `aviso_reloj_critico_s` el nivel 1 nunca se vería antes del 2; por eso `aviso_reloj_s` debe ser mayor (guarda en Tuning Knobs).

### Duración de una partida de uso máximo (derivada, no es otra fórmula)

Control de la regla de la Máquina "ninguna partida con timer pasa de 25:00": si **los dos** agotan siempre la etapa de mover y la de atacar (el caso más largo), azul, que empieza, se queda sin reloj primero:

| Modo | Turnos completos de cada uno | Termina en | Consumido total | Gana |
|---|---|---|---|---|
| Fijo 90 | 8 (720 s) | turno 17, a los 30 s | 16 × 90 + 30 = 1.470 s | rojo |
| Fijo 60 | 12 (720 s) | turno 25, a los 30 s | 24 × 60 + 30 = 1.470 s | rojo |
| Variable 90 → 60 | 4 turnos de 90 s y 6 de 60 s de azul (720 s) | turno 21, a los 30 s | 630 + 13 × 60 + 30 = 1.440 s | rojo |

En los tres, `consumido_total_ms` ≤ 1.500.000. Este es el límite superior de duración con timer; la meta de 15–20 min (900–1.200 s) la sostiene que casi nadie agote cada etapa (Máquina, D1).

## Edge Cases

### Vencimientos y carreras

- **Si el reloj de partida y el timer de etapa vencen en el mismo instante:** vale el reloj: se envía `CerrarPartida(RelojAgotado, rival, numeroTurno)` y no hay `TurnTimeout` (regla 8). Cierra la pregunta abierta de la Máquina.
- **Si el reloj del jugador vence antes que el timer de etapa:** la etapa nunca vence; la partida termina por reloj a mitad de la etapa, en cualquier fase. Lo ya movido queda; los ataques asignados y no resueltos no se resuelven (Máquina, regla 14).
- **Si un comando del jugador llega en el mismo milisegundo en que vence su etapa o su reloj, o después:** se evalúa primero el vencimiento con la hora de recepción y el comando falla. No hay gracia.
- **Si el Timer envía `TurnTimeout` y el turno ya cambió** (carrera con `TerminarMovimiento` o `Resolver`): la Librería lo rechaza con `TimeoutObsoleto` o `ComandoViejo` y el Timer lo descarta sin aviso.
- **Si el Timer envía el timeout de mover y la fase es `Reclutar`:** lo resuelve la Máquina (recluta automático y pasa a atacar). El Timer no distingue fases.
- **Si llega un vencimiento con la partida ya terminada:** `PartidaTerminada`, se descarta. El Timer pasa a `Terminado` y no vuelve a evaluar nada.
- **Si `TurnTimeout` recibe `LimitePartida`:** el Timer envía `CerrarPartida(Tablas, vacío, vacío)` (regla 9). Es una defensa: los datos válidos no la alcanzan.
- **Si `CerrarPartida(RelojAgotado)` recibe `CierreInvalido`:** es la carrera con Resolver o TerminarMovimiento (el turno ya cerró): no es un error del Timer ni un S1; se descarta y se reevalúa con el estado nuevo. Si el reloj del mismo jugador sigue en 0 en su turno siguiente, sale ahí (Open Questions: política de la carrera).

### Etapas y turnos

- **Si el turno no tiene ninguna etapa** (se saltan reclutar, mover y atacar): no se arma nada, el reloj no corre y `TurnoMedido` sale con 0 ms.
- **Si Distracción saltea el movimiento:** nunca se arma la etapa de mover; la de atacar usa el tramo fijado en el turno (regla 6).
- **Si se termina de mover con tiempo de sobra:** el sobrante no se acumula; atacar arranca con su duración completa.
- **Si la etapa de mover pasa de `Reclutar` a `Mover`:** el timer no se reinicia; lo gastado en elegir reclutar cuenta.
- **Si el consumo total cruza el umbral a mitad de un turno:** ese turno sigue con sus duraciones; el tramo final empieza en el turno siguiente, con un solo `TramoFinalIniciado`.
- **Si el modo es Fijo 90 o Fijo 60:** no hay tramo final ni `TramoFinalIniciado`; el umbral no se evalúa.

### Pausas, saltos y plataformas

- **Si la aplicación pierde el foco en local:** el reloj sigue corriendo.
- **Si el sistema suspende la aplicación en local:** `Pausar(SuspensionLocal)` apenas llega el aviso del sistema; al volver, `Reanudar`. Lo que pasó en pausa no se descuenta del reloj ni de la etapa.
- **Si el sistema no avisó la suspensión y la fuente de tiempo salta más de `salto_reloj_max_ms`:** la diferencia no se cuenta (regla 10). Un congelamiento corto (menos de 5 s) sí se cuenta: en local no hay rival al que beneficie.
- **Si el equipo se queda sin batería o el proceso muere:** la partida local no se reanuda desde el Timer; qué hace un reinicio es del Registro de partida (#26) y no entra al MVP.
- **Si el servidor pierde la conexión con un jugador:** el Timer **sigue contando** (no se usa la pausa `Desconexion` en el MVP); si el jugador no vuelve, el Servidor cierra con `CerrarPartida(Abandono)`, y gana el comando que la autoridad ordene primero. *PROVISIONAL, #28.*
- **Si el cliente online llega a 0 antes que el servidor:** la vista se queda en 0 y espera; nunca decide (regla 15).
- **Si la latencia online le quita al jugador un comando que estaba en camino al vencer:** el comando se pierde; no hay compensación de latencia en el Timer. Si el Servidor quiere una gracia, es suya y se define en su GDD.

### Configuración y límites

- **Si el modo falta, es desconocido, o online no es Variable:** `ModoTimerInvalido` y no se crea la partida; no hay modo por defecto silencioso (regla 3).
- **Si `T_tope` o `reloj_jugador_s` salen del rango seguro de la Máquina:** el Timer rechaza la config con `RelojInvalido`; no recorta el valor.
- **Si dos jugadores no hacen nada en Sin límite:** nadie vence ni se cierra la partida; solo la corta el tope de comandos de la Librería (`C_MAX`). Es la pregunta abierta de la Máquina (casi inalcanzable).
- **Si la tasa de cuadros cae:** el tiempo se acumula por marcas de tiempo, así que el vencimiento llega con retraso de a lo sumo un cuadro; ni el reloj ni el timer se atrasan.
- **Si el Rival IA emite un comando en el mismo instante en que vence su etapa:** se aplica la misma regla 8 que a un humano.

## Dependencies

**Depende de:**

| Sistema | Tipo | Qué usa |
|---|---|---|
| #3 Máquina de turno | Dura | `numeroTurno`, jugador del turno, etapa del timer, partida en curso; `reloj_jugador_s` y `tope_partida_s`; qué hacen `TurnTimeout` y `CerrarPartida(RelojAgotado)` |
| #1 Librería de reglas | Dura | El contrato de los comandos de autoridad y sus códigos de rechazo; el dato informativo del modo de timer |
| #11 Modificador | Blanda | Con Distracción no hay etapa de mover; el Timer lo sabe por la etapa de la Máquina, no lee la cara |

El índice solo nombra la Máquina; la Librería es una dependencia dura real porque define a quién se le envían los comandos y qué se responde.

**Dependen de este sistema:**

| Sistema | Tipo | Qué usa | Estado del GDD |
|---|---|---|---|
| #15 Ficha y HUD | Dura | La vista de relojes y los eventos de aviso | Existe (tanda 3) |
| #19 Métricas | Blanda | `TurnoMedido`, `RelojesAlCierre`, `TiempoAgotado` | Existe (tanda 4) |
| #21 Menú principal | Blanda | La lista de modos para la pantalla de partida local | Pendiente |
| #20 Ajustes | Blanda | El identificador del modo para recordar la última elección | Pendiente |
| #22 Audio | Blanda | Los eventos que llevan sonido | Pendiente |
| #18 Render | Blanda | `TramoFinalIniciado` para el cambio de luz | Existe (tanda 4) |
| #28 Servidor | Dura | La política de vencimientos como biblioteca y el formato online | Pendiente |
| #25 Rival IA | Blanda | Si sus turnos descuentan reloj | Pendiente |
| #14 Localización | Blanda | Nombres de los cuatro modos | Existe |
| #13 Entrada | Blanda | El cambio de fase por `TurnTimeout` suelta la selección | Existe |

**Bidireccionalidad.** La Máquina de turno ya nombra al Timer (Dependencies, Interactions y Cross-References) y la Librería también (Interactions). Los demás GDD pendientes tienen que nombrar al Timer en su Dependencies al escribirse (marcado para `/design-review` y `/review-all-gdds`).

## Tuning Knobs

**Perillas propias del Timer** (viven en la config del Timer o de la autoridad, nunca en el código):

| Perilla | Valor | Rango seguro | Si es muy alta | Si es muy baja | Interactúa con | Dónde vive |
|---|---|---|---|---|---|---|
| `dur_mover_normal_s` | 60 s | 30–90 | El turno se alarga y el reloj de 12:30 alcanza para pocos turnos; la partida se estira | Mover 12 piezas en pantalla táctil no entra | `dur_atacar_normal_s`; el cociente `reloj_jugador_s` ÷ `turno_max_s` | Config del Timer |
| `dur_atacar_normal_s` | 30 s | 15–45 | Más espera sin decisión | Asignar varios ataques combinados no alcanza | `dur_mover_normal_s` | Config del Timer |
| `dur_mover_final_s` | 40 s | 30–60 | El tramo final casi no aprieta | Pelea con el pilar 1 (concepto, riesgo táctil) | `umbral_tramo_final_s` | Config del Timer |
| `dur_atacar_final_s` | 20 s | 15–30 | Igual que `dur_mover_final_s` | Igual que `dur_atacar_normal_s` | `umbral_tramo_final_s` | Config del Timer |
| `umbral_tramo_final_s` | 600 s | 300–1.100 | Con valores cercanos a `tope_partida_s` el tramo final casi no se juega (se vuelve Fijo 90) | Con 0 el modo Variable es Fijo 60 | `dur_*_final_s`, `tope_partida_s` | Config del Timer |
| `aviso_etapa_s` | 10 s | 5–15 | El aviso llega tan pronto que el anillo ámbar es ruido | No da tiempo de reaccionar | art bible §2 (fija "los últimos 10 s"): cambiarlo exige tocar el art bible | Config del Timer |
| `aviso_reloj_s` | 60 s | 30–120 | Se avisa demasiado pronto, con el reloj casi entero | El jugador no puede ajustar su ritmo | `aviso_reloj_critico_s`: debe ser mayor | Config del Timer |
| `aviso_reloj_critico_s` | 10 s | 5–15 | Pisa al nivel 1 | Llega sin margen | `aviso_reloj_s` | Config del Timer |
| `salto_reloj_max_ms` | 5.000 ms | 1.000–30.000 | Perdona congelamientos largos que sí son tiempo del jugador | Perdona pausas legítimas del sistema (un cuadro lento) | Solo local | Config del cliente |

**Guardas entre perillas** (la config se rechaza si no se cumplen): `turno_max_s ≤ reloj_jugador_s ÷ 6` (125 s con 750: al menos 6 turnos completos por reloj, **guarda propuesta acá, a confirmar en playtest**); `aviso_reloj_s > aviso_reloj_critico_s`; `aviso_etapa_s` menor que la menor etapa (15 s).

**Perillas de otros sistemas que cambian este GDD** (no se duplican, se apuntan):

| Valor | Dueño | Qué cambia aquí |
|---|---|---|
| `tope_partida_s` (1.500 s, rango 1.290–1.500) | Máquina de turno (#3) | `reloj_jugador_s` y el consumo total máximo (D2, D3) |
| `reloj_jugador_s` (750 s) | Máquina de turno (#3) | D2, D3 y la tabla de uso máximo |
| Eventos de la Máquina (leyendas ≤ 1,5 s por cascada) | Máquina de turno (#3) | Que no se descuenten (regla 11) |
| Presentación de la resolución (≤ `resolucion_max_s` = 2,0 s) | Render (#18, regla 25) | Que en local pase con el reloj quieto, en `EsperandoTirada` (regla 11; decisión del 2026-11-26, cierra W-2) |

**Constantes del Timer (no son perillas):** unidad de milisegundos enteros, lista cerrada de modos (4), de eventos (regla 12) y de motivos de pausa (2).

## Visual/Audio Requirements

El Timer no dibuja ni suena: emite eventos y la presentación (HUD #15, Render #18, Audio #22) los traduce. Fuente: `design/art/art-bible.md`, secciones 2 y 7 (lente del director de arte).

| Evento | Qué ve y oye el jugador | Estado en el art bible |
|---|---|---|
| `EtapaIniciada` | El anillo del timer, arriba al centro con el emblema del bando, se llena completo y empieza a cerrarse | Definido (§7, *Pantalla y HUD*: timer arriba al centro, siempre visible) |
| `AvisoEtapa` (últimos 10 s) | El anillo pasa a ámbar `#E89A2C` y pulsa una vez por segundo; el tablero no destella | Definido (§2, *Timer en los últimos 10 s*) |
| `TramoFinalIniciado` | Banner "Tramo final" una sola vez (si `TramoFinalIniciado` coincide con la placa de la cara o con el banner del turno, `Tramo final` es un renglón de esa placa y no hay banner propio; #15, regla 12; decisión del 2026-11-26), atardecer permanente y anillo de **trazo grueso permanente de color base**; el ámbar queda solo para los avisos (HUD, #15, regla 12; decisión del jugador, 2026-11-25) | Definido (§2, *Tramo final*, corregido en la tanda 3) |
| `AvisoReloj` nivel 1 y 2 | El reloj de partida del jugador pasa a ámbar (nivel 1) y pulsa (nivel 2), **con la misma forma de anillo y en su lugar fijo del HUD** | Definido (§7 *Timer y relojes de partida*, agregado 2026-11-26): el ámbar solo lo lleva lo que limita, nunca dos a la vez (resuelto por #15, reglas 12 y 13) |
| `TiempoAgotado` | Marca breve "Tiempo" junto al anillo, ≤ 1 s, sin sonido de error; el recluta automático tiene su aviso de texto ("Elegido por el tiempo") en la zona de avisos, sin marca sobre la pieza ni la ficha (HUD, #15, regla 18; decisión del 2026-11-26) | Definido (§7 *Timer y relojes de partida*, agregado 2026-11-26) |
| `RelojPausado` | Los dos relojes quedan quietos con un indicador de pausa | Definido (§7 *Timer y relojes de partida*, agregado 2026-11-26; ícono de pausa en §7 *Íconos*) |
| `PartidaTerminada` con `RelojAgotado` | Pantalla de Victoria o Derrota con los dos relojes en el resumen | Lugar definido (§7 *Timer y relojes de partida*, agregado 2026-11-26); texto del resumen: **Falta** (Máquina, Visual/Audio) |

**Restricciones:** el ámbar solo aparece en el timer, con forma de anillo y en una posición fija (art bible §2 y §7); ningún aviso se transmite solo por color: siempre lleva forma (anillo más grueso), número y, en nivel 2, pulso. Los pulsos (1 por segundo) cuentan en el presupuesto de destellos del art bible.

**Audio.** El Timer fija qué eventos llevan cue y Audio (#22) decide cómo suena: tictac suave en `AvisoEtapa` (una vez por segundo), un solo cue de `TramoFinalIniciado`, un cue distinto para `AvisoReloj` nivel 2, y silencio en `TiempoAgotado`. Cada cue tiene su equivalente visual, así que desactivar el sonido no esconde ningún aviso. Ningún cue pasa por encima del impacto de la resolución en la mezcla.

📌 **Asset Spec** — Visual/Audio requirements are defined. After the art bible is approved, run `/asset-spec system:timer-de-turno` to produce per-asset visual descriptions, dimensions, and generation prompts from this section.

## Game Feel

*(Opcional para `Core`; se incluye porque el reloj es lo que el jugador siente más.)*

- **Precisión:** un vencimiento se detecta a lo sumo **100 ms** después de su instante, local y servidor (`precision_vencimiento_ms`, constante de verificación, no perilla).
- **Un número que no miente:** el HUD muestra `⌈restante ÷ 1000⌉`, así que "0" solo aparece cuando venció; la cuenta nunca salta hacia atrás (salvo la corrección del servidor online, **PROVISIONAL**).
- **Anillo continuo:** el anillo se cierra de forma continua entre un entero y el siguiente; no avanza a saltos de un segundo.
- **Vencer sin castigo:** al agotarse la etapa, el paso a la fase siguiente tiene que verse como "el plan que sigue", no como un corte (Máquina, Game Feel). Se verifica en playtest.
- **Aviso útil:** el aviso de los 10 s da tiempo de terminar el movimiento en curso; si el jugador no lo ve, no es un buen aviso (se mide en playtest, AC-26).
- **Sin sorpresas de reloj:** no hay vencimientos retroactivos ni descuentos por esperas de la autoridad.

## UI Requirements

El HUD (#15) lee de la vista de relojes (regla 13) y tiene que mostrar:

- **Anillo de la etapa**, arriba al centro, con el emblema del bando de turno (art bible §7), con el número `mostrado_s` adentro.
- **Los dos relojes de partida** en m:ss, siempre visibles, con el del jugador activo marcado; detenidos y claramente quietos en `enEsperaDeTirada` y en pausa.
- **Cuál vence primero** (`vinculante`): cuando el reloj es menor que la etapa, el jugador tiene que poder ver que lo que lo limita es su reloj de partida.
- **Avisos** de etapa, de reloj nivel 1 y 2 y el banner del tramo final, con los tratamientos de Visual/Audio.
- **Sin límite:** sin anillo ni relojes; el HUD lo dice en una marca y no deja un hueco vacío.
- **Estado "esperando a la autoridad"** online cuando la predicción llegó a 0.
- **Accesibilidad:** nada solo por color (anillo más grueso, número y pulso); los tiempos se leen con tamaño de texto escalable (art bible: timer de 28–36 px); un lector de pantalla anuncia el tiempo solo a pedido o en los avisos de nivel 2, no cada segundo.

📌 **UX Flag — Timer de turno**: This system has UI requirements. In Phase 4 (Pre-Production), run `/ux-design` to create a UX spec for each screen or HUD element this system contributes to **before** writing epics. Stories that reference UI should cite `design/ux/[screen].md`, not the GDD directly.

## Cross-References

| Este GDD dice | Documento | Elemento exacto | Naturaleza |
|---|---|---|---|
| Reloj por jugador de 750 s que no corre en `EsperandoTirada`, `RelojAgotado` y gana el reloj | `maquina-de-turno.md` | Regla 14, D2, Edge Cases (tiempo) | **Usa** (valores de la Máquina) |
| El Timer decide cuándo se envían `TurnTimeout` y `CerrarPartida` | `maquina-de-turno.md` | Reglas 12 y 14, Interactions (fila #12) | **Usa** |
| Los restantes se redondean hacia arriba y "0" solo sale al vencer; empate de reloj y etapa: vale el reloj | `maquina-de-turno.md` | Open Questions (empate, aviso de reloj, desconexión) | **Cierra** dos preguntas y acota una |
| Etapa del timer es un dato del estado | `maquina-de-turno.md` | Regla 2, AC-2 | **Usa** |
| `turno_max_s` pasa a ser del Timer (45–125) | `maquina-de-turno.md` | D1, Tuning Knobs | **Define** el valor que la Máquina nombra |
| Comandos de autoridad, rechazos y modo de timer informativo | `libreria-de-reglas.md` | Reglas 3, 5, 13 y 17; fila `LimitePartida`; Edge Cases | **Usa**; `CerrarPartida(motivo, ganador, turno)` con tres argumentos; Tablas como cierre técnico (regla 3) y Tablas automáticas en C_MAX − 1 (regla 16) |
| Los cuatro modos, el tramo final y la partida de 15–20 min | `game-concept.md` | Tabla de Timer, Session Length, pilar 4 | **Cambia** el texto: "desde el minuto 10" es tiempo consumido, no hora de pared |
| Anillo ámbar, últimos 10 s, banner "Tramo final" | `art-bible.md` | §2 (*Tramo final*, *Timer en los últimos 10 s*), §7 | **Usa**; **Agrega** avisos del reloj de partida, "Tiempo" y pausa |
| Los dos relojes de partida y los umbrales de aviso | `systems-index.md` | Fila #12, Dependencias (Librería) | **Agrega** |

## Acceptance Criteria

**Tipos de evidencia:** **Logic** = prueba automática de unidad con fuente de tiempo falsa, BLOQUEANTE, en `Assets/Tests/EditMode/TimerDeTurno/`. **Integration** = prueba con la Máquina y la Librería reales. **UI/Visual** = captura retenida en `production/qa/evidence/` (el HUD las cumple). **Playtest** = observación con jugadores. Los criterios marcados **PROVISIONAL** dependen de un GDD que no existe.

### Reglas del Timer (Sección C)

- **AC-1 (Logic, regla 1 y 2)** — **GIVEN** un Timer con fuente de tiempo falsa y la config Variable, **WHEN** se recorre un turno completo, **THEN** el Timer solo envía comandos de autoridad (`TurnTimeout`, `CerrarPartida`), no cambia el estado y no usa la hora del sistema (el código del Timer no referencia `DateTime`, `Stopwatch` ni el tiempo de Unity fuera de la interfaz inyectada).
- **AC-2 (Config/Data, regla 3)** — **GIVEN** cada uno de los cuatro modos, **WHEN** se piden las duraciones, **THEN**: Variable da 60/30 en normal y 40/20 en final; Fijo 90 da 60/30; Fijo 60 da 40/20; Sin límite no da duraciones; y los tres con timer dan 750 s de reloj.
- **AC-3 (Logic, regla 3)** — **GIVEN** una config con modo ausente, otra con un modo inexistente y otra online con Fijo 60, **WHEN** se crea la partida, **THEN** las tres fallan con `ModoTimerInvalido` y no queda ninguna partida creada.
- **AC-4 (Logic, regla 4)** — **GIVEN** la misma secuencia de marcas de tiempo simulada a 30, 60 y 144 cuadros por segundo, **WHEN** transcurren 59.000 ms de una etapa de mover de 60 s, **THEN** el restante es 1.000 ms en las tres corridas.
- **AC-5 (Logic, regla 5)** — **GIVEN** un turno que va de `Reclutar` a `Mover`, **WHEN** el jugador tarda 20 s en reclutar, **THEN** el timer de mover no se reinicia: a esa altura le quedan 40 s. **WHEN** pasa a `Atacar`, **THEN** el timer de atacar arranca completo (30 s) y el sobrante de mover no se suma.
- **AC-6 (Logic, regla 5)** — **GIVEN** un turno con Distracción y otro donde se saltan todas las fases, **WHEN** se abren, **THEN** el primero arma solo la etapa de atacar y el segundo no arma ninguna, no corre ningún reloj y `TurnoMedido` sale con 0 ms.
- **AC-7 (Logic, regla 6 y D2)** — **GIVEN** el modo Variable con 599.999 ms de consumo total al abrir el turno, **WHEN** se arma la etapa, **THEN** el turno es normal (60/30); **GIVEN** 600.000 ms, **THEN** es final (40/20) y se emite `TramoFinalIniciado` una vez; **GIVEN** que se cruza el umbral a mitad de un turno, **THEN** ese turno conserva sus duraciones. En Fijo 90 y Fijo 60 el tramo nunca cambia.
- **AC-8 (Logic, regla 7)** — **GIVEN** el turno de azul en `EsperandoTirada` durante 5 s de la fuente de tiempo, **WHEN** llega la tirada y se arma `Mover`, **THEN** el reloj de azul es el mismo que antes de esperar. **GIVEN** el modo Sin límite, **THEN** no existen relojes.
- **AC-9 (Logic, regla 8 y D3)** — **GIVEN** una etapa de mover de 60 s, **WHEN** la fuente marca 59.999 ms, **THEN** no se envía nada; **WHEN** marca 60.000 ms, **THEN** se envía `TurnTimeout(numeroTurno, mover)` exactamente una vez, aunque se siga avanzando el tiempo.
- **AC-10 (Logic, regla 8)** — **GIVEN** que a azul le quedan 25 s de reloj y su etapa de mover es de 60 s, **WHEN** pasan 25.000 ms, **THEN** se envía `CerrarPartida(RelojAgotado, rojo, numeroTurno)` y nunca un `TurnTimeout` (D3: `vinculante` = Reloj).
- **AC-11 (Logic, regla 8)** — **GIVEN** que el reloj de azul y su etapa vencen en el mismo instante, **WHEN** el Timer evalúa, **THEN** envía solo `CerrarPartida(RelojAgotado, rojo, numeroTurno)`, con el `numeroTurno` de ese turno.
- **AC-12 (Logic, regla 8)** — **GIVEN** un `TerminarMovimiento` de azul con hora de recepción igual al instante en que vence su etapa, **WHEN** la autoridad lo procesa, **THEN** el `TurnTimeout` se envía primero y el comando se rechaza; con una hora de recepción 1 ms anterior, el comando se acepta y el timeout nunca se envía.
- **AC-13 (Logic, regla 9)** — **GIVEN** un `TurnTimeout` rechazado con `TimeoutObsoleto`, **THEN** el Timer lo descarta sin reintentar ni emitir eventos. **GIVEN** un rechazo `LimitePartida` (defensa, con un fixture), **THEN** envía `CerrarPartida(Tablas, vacío, vacío)`. **GIVEN** un `CerrarPartida(RelojAgotado)` rechazado con `CierreInvalido` porque el turno ya cerró, **THEN** lo descarta sin registrar error. **GIVEN** una respuesta `PartidaTerminada`, **THEN** pasa a `Terminado`.
- **AC-14 (Logic, regla 10)** — **GIVEN** una etapa con 40 s restantes, **WHEN** se pausa 10 min con `SuspensionLocal` y se reanuda, **THEN** el restante sigue siendo 40 s y el reloj del jugador no cambió. **GIVEN** que la aplicación pierde el foco 4 s sin suspenderse, **THEN** se descuentan. **GIVEN** un salto de la fuente de tiempo de 8.000 ms sin pausa (local), **THEN** esa diferencia no se descuenta; **GIVEN** un salto de 4.999 ms, **THEN** se descuenta completo.
- **AC-15 (Logic, regla 12 y D4)** — **GIVEN** `restante_ms` de 59.001, 1 y 0, **WHEN** se calcula `mostrado_s`, **THEN** da 60, 1 y 0. **GIVEN** una etapa que baja de 10.001 a 10.000 ms, **THEN** se emite `AvisoEtapa` una vez. **GIVEN** un reloj que baja a 60.000 y a 10.000 ms, **THEN** se emiten `AvisoReloj` nivel 1 y nivel 2, una vez cada uno por jugador.
- **AC-16 (Logic, regla 13)** — **GIVEN** el reloj de azul con 30 s y una etapa de 60 s, **THEN** la vista dice `vinculante` = Reloj; **GIVEN** 400 s y 60 s, `vinculante` = Etapa. **GIVEN** `EsperandoTirada`, la vista dice `enEsperaDeTirada` y no hay etapa.
- **AC-17 (Logic, regla 12 y 14)** — **GIVEN** un turno de azul con 20 s en mover y 12 s en atacar, el último vencido, **WHEN** cierra, **THEN** se emite `TurnoMedido(n, azul, 20.000, 12.000, falso, verdadero)`. **GIVEN** el modo Sin límite, **THEN** `TurnoMedido` se emite igual. **GIVEN** que termina la partida, **THEN** se emite `RelojesAlCierre` con los dos restantes.
- **AC-18 (Logic, regla 15)** — **GIVEN** un Timer de cliente online, **WHEN** pasa cualquier cantidad de tiempo, **THEN** no envía ningún comando; **GIVEN** una predicción que llega a 0, **THEN** la vista queda en 0 con el estado "esperando a la autoridad" (**PROVISIONAL**, #28).

### Fórmulas (Sección D)

- **AC-19 (Logic, D1)** — **GIVEN** cada modo y tramo, **THEN** `dur_etapa_ms` y `turno_max_s` valen: Variable normal 60.000/30.000 y 90; Variable final 40.000/20.000 y 60; Fijo 90 igual que normal; Fijo 60 igual que final. **GIVEN** una config con `dur_mover_normal_s` = 14, **THEN** se rechaza (fuera del rango) con `RelojInvalido`.
- **AC-20 (Logic, D2 y D3, frontera)** — **GIVEN** `reloj_jugador_s` = 750 y `consumido_ms` de azul = 750.000, **THEN** `restante_reloj_ms` = 0 y el cierre es inmediato al armar la etapa. **GIVEN** `consumido_total_ms`, **THEN** es igual a `2 × 750.000 − restante[0] − restante[1]` en todos los instantes de una partida simulada.
- **AC-21 (Logic, duración máxima)** — **GIVEN** que los dos jugadores agotan siempre las dos etapas, **WHEN** se simula cada modo, **THEN**: Fijo 90 termina en el turno 17 a los 30 s con 1.470 s consumidos; Fijo 60 en el turno 25 a los 30 s con 1.470 s; Variable en el turno 21 a los 30 s con 1.440 s; en los tres gana rojo con `RelojAgotado` y el consumo total es ≤ 1.500.000 ms.

### Cruces con otros sistemas

- **AC-22 (Integration, con Máquina y Librería reales)** — **GIVEN** una partida real con el Timer y fuente de tiempo falsa, **WHEN** vence la etapa de mover en `Reclutar`, **THEN** la Librería acepta el `TurnTimeout`, recluta automático y pasa a `Atacar`, y el Timer arma la etapa de atacar. **WHEN** vence el reloj de azul, **THEN** la Librería acepta `CerrarPartida` y la partida termina con `RelojAgotado` y ganador rojo. **WHEN** vence el reloj mientras se espera una tirada, **THEN** no se envía nada (el reloj no corre).
- **AC-23 (Integration, repeticiones)** — **GIVEN** la lista de comandos de una partida con un `TurnTimeout` de cada etapa y un `CerrarPartida(RelojAgotado)`, **WHEN** se reproduce sin Timer, **THEN** la huella final es idéntica a la de la partida jugada.
- **AC-24 (Config/Data, precisión)** — **GIVEN** el Timer corriendo 60 s en local y en el servidor, **WHEN** se mide el retraso entre el instante de vencimiento y el envío del comando, **THEN** es ≤ 100 ms en el percentil 99 de 1.000 vencimientos.

### Presentación y juego (Visual/UI/Playtest)

- **AC-25 (UI, retenida en `production/qa/evidence/`)** — Una captura por estado del HUD: anillo normal, `AvisoEtapa` ámbar con pulso, tramo final con banner, `AvisoReloj` nivel 1 y nivel 2, partida pausada y Sin límite; en cada una el ámbar aparece solo en el anillo y en su posición fija. **BLOQUEANTE** (lo cumple #15 con este GDD de referencia).
- **AC-26 (Playtest)** — **GIVEN** 5 jugadores en una partida con timers, **WHEN** se les pregunta en un momento al azar cuánto le queda a cada uno, **THEN** al menos 4 de 5 dicen los dos relojes con error de ±5 s en menos de 3 s, sin mirar el menú.
- **AC-27 (Playtest)** — **GIVEN** 10 partidas con timers, **WHEN** se les pregunta después de cada vencimiento si se sintió justo, **THEN** menos de 1 de cada 5 vencimientos se reporta como injusto y ninguno como sorpresa de reloj.

**Cobertura:** cada regla tiene criterio (reglas 1–2: AC-1; 3: AC-2 y AC-3; 4: AC-4; 5: AC-5 y AC-6; 6: AC-7; 7: AC-8; 8: AC-9 a AC-12; 9: AC-13; 10: AC-14; 11: AC-4, AC-14 y AC-22; 12: AC-15 y AC-17; 13: AC-16; 14: AC-17; 15: AC-18; 16: sin criterio, depende del GDD del Rival IA), y cada fórmula también (D1: AC-19; D2: AC-7 y AC-20; D3: AC-9, AC-10 y AC-20; D4: AC-15). **Gap:** la regla 16 queda sin criterio hasta el GDD del Rival IA.

## Open Questions

| Pregunta | Dueño | Para cuándo |
|---|---|---|
| ¿El tramo final de 40 s para mover alcanza en pantalla táctil con 12 piezas? Choca con el pilar 1 | Playtest en táctil | MVP |
| ¿El reloj por jugador decide partidas que el pilar 3 (siempre hay vuelta) habría salvado? La medida existe (Métricas, #19, regla 14). | Métricas (#19) | MVP |
| ¿Hace falta un botón de pausa manual en local (un llamado, una interrupción)? | Menú (#21) y playtest | MVP |
| ~~¿Cómo conviven el anillo de la etapa y el reloj de partida sin dos ámbar compitiendo?~~ **Resuelta por #15 (reglas 12 y 13, F3):** el ámbar solo lo lleva el timer que de verdad limita; el tramo final es un anillo grueso de color base | — | — |
| ¿Se pausan los relojes online por desconexión, y hay gracia por latencia? Hoy: no se pausa y no hay gracia | Servidor (#28) | En su GDD |
| ¿El formato del restante online y la corrección de la predicción? | Servidor (#28) | En su GDD |
| ~~¿`LimitePartida` cierra con `Tablas`?~~ **Resuelta por la Librería (2026-11-27):** sí; Tablas es el cierre técnico sin ganador (regla 3) y el tope de `contadorComandos` cierra solo en Tablas (regla 16) | — | — |
| Si Resolver le gana la carrera al reloj, RelojAgotado recién sale en el turno siguiente del jugador agotado, y en el medio el rival puede ganar por ReyMuerto. ¿Política? (por ejemplo, cobrar el reloj al recibir el comando y no contar un Resolver que llega con el reloj en 0). Un cliente no muestra la pantalla de fin desde un estado predicho, solo desde uno confirmado | Servidor (#28) y Timer | Antes del online |
| ¿Los turnos del Rival IA descuentan reloj? ¿La partida tutorial usa timers? | Rival IA (#25) y Tutorial (#24) | En sus GDD |
| ~~¿La animación de resolución corre sobre el reloj del jugador siguiente?~~ **Resuelta para el modo local (2026-11-26):** no; la `Tirada` espera a la presentación (Render, #18, regla 25, ≤ 2,0 s) y el reloj no corre en `EsperandoTirada`. Online sigue abierta | Servidor (#28) | Antes del online |
| ¿Las repeticiones muestran los relojes? El núcleo no guarda `consumido_ms` | Registro de partida (#26) | En su GDD |
| ~~¿Las partidas Sin límite cuentan para la compuerta del arte?~~ **Resuelta 2026-11-26:** no (Métricas, regla 12) | — | — |
| ¿Se distingue en web perder el foco de una suspensión? **NOT SOURCEABLE** en la referencia del motor. Si el navegador congela una pestaña oculta más de 5 s, hoy ese tiempo no se cuenta y vuelve la pausa gratis | ADR del Timer | Antes de implementar |
| ¿Se mantiene `salto_reloj_max_ms` de 5 s o se mide una forma mejor de detectar la suspensión? | Playtest local | MVP |
| Fijo 90 con uso máximo termina en el turno 17: ¿aceptable? La medida existe (Métricas, #19, regla 14). | Métricas (#19) | MVP |
