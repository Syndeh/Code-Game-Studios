# Máquina de turno y fin de partida

> **Status**: Approved (`/design-review` del 2026-11-27/28: NEEDS REVISION once veces, corregidos todos los bloqueantes; la 12.ª pasada, con el panel completo, dio APPROVED. Los contratos con vecinos no aprobados quedan para la sesión de cierre de la tanda B)
> **Author**: jugador + Claude (design-system)
> **Last Updated**: 2026-11-28 (12.ª pasada `/design-review`: APPROVED; bloqueantes de la 3.ª a la 11.ª pasada aplicados, ver el log)
> **Last Verified**: 2026-11-28 (contra Librería #1, Catálogo #2, Tablero #4, Castillo y muros #5 y Timer #12, todos Approved)
> **Implements Pillar**: 4 Duelo corto (dueña del presupuesto de duración); sirve a 1 Planificar y cosechar
> **Creative Director Review (CD-GDD-ALIGN)**: REVISED 2026-11-20 — la fantasía promete "no te quita lo ya hecho" y declara el límite del timeout en Reclutar; el aviso de tiempo es requisito (desde el 2026-11-24, los dos relojes a la vista: regla 14, AC-30)

## Summary

La **Máquina de turno** ordena cada turno de rolchess (tirada del Modificador, reclutar,
mover, atacar y resolver), guarda la pila de deshacer de los movimientos y decide cuándo
termina la partida. Existe para que el jugador planifique y vea caer sus golpes siempre en
el mismo orden, y para sostener el pilar de duelo corto con un reloj de
12:30 por jugador (25 minutos en total). Es parte del núcleo puro de C#: no
mide tiempo ni toma azar.

> **Quick reference** — Layer: `Core` · Priority: `MVP` · Key deps: `Librería de reglas (#1), Catálogo de piezas (#2), Tablero (#4)`

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
objetivo, pasás a atacar con lo que tenés y resolvés. Que se acabe el tiempo de una etapa
no te quita lo que ya hiciste: lo movido se queda donde está, lo asignado se resuelve y lo
que falta se decide solo. Es justo lo que te obliga a concentrarte en lo que importa y a
decidir. Cuando cae el golpe, el turno se cierra limpio y la partida sigue hacia un final
que se siente corto y ganado. El reloj de partida es otra cosa: cada segundo de tu turno desde que se arma la etapa lo
gasta (la placa de la cara no, regla 14), uses la etapa entera o no (jugar rápido te ahorra
reloj), y si llega a 0 perdés en ese instante (online, al terminar la gracia por latencia),
también con ataques asignados que todavía no resolviste. Por eso el HUD te
avisa cuando lo que vence primero es tu reloj.

La Máquina de turno no tiene cara propia: la fantasía es la de **decidir bajo una presión
buscada**, dentro de un duelo corto donde cada turno cuenta.

*Pilar citado (pilar 4):* "El timer es presión buscada: quedarse sin tiempo para mover o
para atacar no es una falla sino lo que obliga a concentrarse."

*Qué le exige al resto del diseño:* que agotar el tiempo de una etapa nunca te quite lo ya hecho (lo
movido queda, lo asignado se resuelve, el recluta automático no te hace perder el recurso y
un comando que llega tarde se descarta sin error ni castigo, con un aviso neutro), que el
juego nunca se trague una orden válida (la placa de la cara no bloquea: regla 14), y que el jugador vea siempre en qué
etapa está y cuánto le queda, también en su reloj de partida. El reloj de 12:30 sí
termina la partida: es la única presión de tiempo que la hace perder, y con él lo asignado
y no resuelto no se resuelve (regla 14; jugador, 2026-11-27). **Límite conocido:** si
el tiempo se agota mientras se elige qué reclutar, el turno pasa directo a atacar y esa fase
de mover se pierde; se mide en playtest (Open Questions).


## Detailed Design

### Core Rules

1. **Qué es.** La Máquina no es un comando: es el conjunto de reglas de fase dentro de
   `Aplicar` (Librería, reglas 1–17). Define las fases, qué comando vale en cada una, las
   transiciones, la pila de deshacer, `EvaluateMatchEnd` y el presupuesto de duración. No
   conoce al Modificador, al Reclutamiento, al Movimiento ni a la Resolución por nombre: les
   da ganchos tipados.
2. **Fases (lista cerrada de cuatro).** `EsperandoTirada`, `Reclutar`, `Mover` y `Atacar`;
   `Terminada` es un predicado, no una fase (abajo). **Resolver no es una fase en reposo:** es la transición que cierra
   `Atacar` y abre el turno siguiente en un solo paso. La **etapa del timer** es *mover*
   para `Reclutar` y `Mover`, y *atacar* para `Atacar`; en `EsperandoTirada` no corre
   ninguna. La etapa es un dato del estado (la lee el Timer); cuándo empieza a **correr** su
   tiempo lo fija la regla 14 (al terminar la placa de la cara o con el primer comando; en el
   turno 1, la regla 3).
   **`Terminada` no es un valor de la fase:** la partida está Terminada si y solo si hay
   motivo (Librería, regla 11, fila de motivo y ganador), y la fase conserva el valor que
   tenía (regla 16, *Estado al terminar*). **La etapa guardada también se conserva:** es la
   de la fase conservada (Librería, regla 11: etapa mover con `Reclutar` o `Mover`, atacar
   con `Atacar`), y el Timer no la lee porque la partida no está en curso (Timer, regla 2).
   Si la partida terminó desde `EsperandoTirada` (un `Rendirse`, un `Abandono` o Tablas
   esperando la tirada), la fase conservada es `EsperandoTirada` y no hay etapa.
   Las columnas `Terminada` de este GDD leen ese predicado.
3. **Primer turno (decisión del 2026-11-20, confirmada por el jugador el 2026-11-24).** Lo juega el jugador 0 y no tira el
   Modificador: empieza directo en `Mover`, sin `Reclutar`, porque sin tirada no hay cara de
   reclutar. Quién empieza lo decide el Tablero (#4, regla 10): el jugador 0, sin compensación.
   `CrearPartida` no es un comando y **no emite eventos** (Librería, regla 2): quien la llama
   lee del estado inicial que el turno 1 del jugador 0 está en `Mover`. **Los consumidores de
   eventos** (#15, #17, #18, #19) arrancan el turno 1 leyendo ese estado inicial, no un
   `TurnoIniciado` (no hay evento sintético). La etapa de mover del turno 1 **no** se arma
   al crear la partida (no hay placa): se arma cuando la presentación del tablero es visible
   por primera vez, o con el primer comando de fase aceptado de azul si llega antes (Timer,
   reglas 7 y 17: una carga o el despliegue no le cuestan reloj a azul); online, cuando los
   dos clientes confirman que están listos, con un tope (`listo_max_s`, pedido al Servidor,
   #28). La asimetría que esto
   crea se mide antes de cambiarla (Open Questions, DW-3).
4. **Abrir un turno T del jugador J** (el cierre del turno anterior y esta apertura salen
   en un solo comando):
   1. Se borran las coberturas de J, el jugador que entra, con un `CoberturaQuitada(…, Caducada)` por escudero en orden de ID, entre `TurnoTerminado` y `TurnoIniciado` (Cobertura, #8, regla 9).
   2. El estado pasa a `EsperandoTirada` con la tabla efectiva (Librería, regla 6); el
      jugador del estado ya es J, el que entra.
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

   `LimiteMovimientos(sin límite)` tiene **un solo valor** (no lleva número): el nombre se
   conserva porque ya lo usan #11 y #15. El límite normal de movimientos (`limite_normal`,
   hoy 12 piezas más el Rey) es de Movimiento (#6). **El límite del turno no es un campo del
   estado: se deriva** de la cara del turno con la consulta pura
   **`LimiteDelTurno(estado)`**, dueña la Máquina: si el papel de la cara del turno es
   `LimiteMovimientos(sin límite)` (Arremetida), sin límite; si no hay cara o es otra,
   `limite_normal`. El papel de la cara sale de la tabla base guardada en el estado
   (PrepararContexto valida los papeles, Librería regla 6); la firma exacta es del ADR.
   `piezasMovidas` se deriva de los movimientos usados (#6, regla 10).
   Así no necesitan fila propia en la tabla de dominios de la Librería (regla 11) y se
   limpian solos cuando la cara se vacía al cerrar el turno. **Arremetida solo puede salir si
   `piezas_sin_rey > limite_normal`** (hoy, más de 12 piezas sin el Rey; decisión del
   2026-11-24; Librería, regla 6): con menos no cambia nada, y el Modificador (#11) pasa su
   probabilidad entera a Grito de guerra.

   **Gancho del Modificador (lista cerrada y orden).** Al aceptar una `Tirada`: (1) la
   Librería convierte `n` en una cara de la tabla efectiva (`CaraPorTirada`, Librería regla
   6) y escribe en el estado la tabla base y la cara del turno, borra la tirada pendiente y
   suma 1 a `contadorTirada` (Librería, regla 11, fila «Tirada del Modificador»); (2) el
   Modificador (#11) devuelve el resultado tipado de esa cara y, solo con Grito de guerra,
   escribe sus bonos en el estado (0 a 10, Librería regla 11); (3) la Máquina fija fase y
   etapa según el resultado (tabla de arriba) y corre la cascada de la regla 10. El
   resultado tipado y la cara son **la misma información**: el Modificador lo deduce del
   papel de esa cara, y `LimiteDelTurno` y el recluta automático leen la cara. La tabla
   efectiva que usa (1) la calcula la Librería al entrar en `EsperandoTirada` con la regla
   del Modificador (#11, regla 4): la Máquina no la elige. Un resultado fuera de la lista es
   inalcanzable (PrepararContexto valida los papeles de las caras). La firma exacta de la
   función es del ADR de la Librería.
6. **Fase `Reclutar`.** Solo existe con `ReclutarObligatorio`. Si al abrirla ningún tipo
   está disponible (`tipo_disponible`, #10) → `ReclutaPerdido` (nunca `FaseSaltada(Reclutar)`
   ni `FaseIniciada(Reclutar)`) y pasa a abrir `Mover`; es una defensa inalcanzable jugando,
   porque la tabla efectiva usa la misma consulta (#10, regla 6). Si no, la fase queda abierta y solo sale con
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
9. **Fase `Atacar`.** Valen `AsignarAtaque`, `QuitarAtaque`, `Cubrir`, `QuitarCobertura` y `Resolver`. Las
   asignaciones se editan libremente hasta resolver. `Resolver` cierra la fase y llama a
   Resolución (#9).
10. **Fases que se saltan.** Una fase que **empieza** sin acción posible se salta con
    `FaseSaltada` (lista cerrada, Librería regla 17): en reclutar, algún tipo disponible; en
    mover, una pieza con algún movimiento legal; en atacar, una pieza con algún objetivo
    válido o un escudero con un aliado que cubrir. Reclutar sin acción posible emite
    `ReclutaPerdido` en lugar de `FaseSaltada` (regla 6). **Con Distracción la fase `Mover` no
    existe:** no se abre ni se salta, así que no hay `FaseSaltada(Mover)` (lo saltó la cara,
    no la falta de movimientos; #15, Edge Cases). Si se saltan todas, el turno cierra solo
    con `TurnoTerminado`. La cascada para en el siguiente `EsperandoTirada` o en el fin de
    la partida: un comando avanza **como mucho un turno**, porque el turno siguiente espera
    una tirada de la autoridad. Las consultas de "acción posible" (`tipo_disponible`,
    `hayMovimientoLegal`, `ObjetivosValidos`, `AliadosCubribles`) se usan como **"existe
    alguno"**: cortan en el primer resultado (el peor caso es un ejército entero sin acción;
    el costo lo fija el ADR dentro de `comando_presupuesto_ms`).
11. **Cerrar un turno.** Después de resolver (con `Resolver`, con el timeout de atacar o por
    salto automático) se limpian las asignaciones y los movimientos usados de todas las piezas
    vuelven a 0 (Movimiento, #6, regla 16). Luego se corre `EvaluateMatchEnd`. **Si no hay
    ganador**, se limpian los bonos del Grito de guerra, la tabla y la cara del turno (con la
    cara se va el límite del turno, regla 5), pasa el jugador, sube `numeroTurno` y se abre el
    siguiente turno (regla 4). **Si hay ganador**, la partida terminada **conserva la tabla,
    la cara y los bonos del último turno**, igual que tras `Rendirse` o `CerrarPartida`
    (decisión del jugador, 4.ª pasada; Librería, regla 11: la fila «Resolver · `TurnTimeout`
    de atacar» solo los vacía si la partida sigue). En un cierre por saltos no hubo resolución, así que
    `EvaluateMatchEnd` no puede encontrar ganador; se corre igual, sin rama especial.
    **Filas de la matriz de clausura que usa la Máquina** (Librería, regla 11): `CrearPartida`
    (fase `Mover`, etapa mover); Tirada del Modificador (cara, bonos, fase y etapa según el
    resultado tipado); Reclutar · recluta automático; TerminarMovimiento · `TurnTimeout` de
    mover; Resolver · `TurnTimeout` de atacar; Cascada al empezar una fase (incluido el
    cierre del turno); `Rendirse` · `CerrarPartida`; y Todo Aceptado (Tablas automáticas).
    Ninguna transición de la Máquina necesita una fila nueva: el límite del turno y
    `piezasMovidas` se derivan (regla 5).
12. **`TurnTimeout` (definitivo; antes provisional, Librería regla 17).**
    - *Etapa mover:* si hay un `Reclutar` pendiente, la librería recluta el **primer tipo
      disponible de la cara** en el orden del Catálogo (Llamada al servicio: soldado,
      arquero, escudero; Cuerno de guerra: catapulta, jinete, demoledor; #10, regla 10;
      evento `Reclutado` con `automatico`); cierra la pila y abre `Atacar`, que se salta si
      empieza sin acción posible (regla 10). Las piezas ya movidas se quedan donde están.
    - *Etapa atacar:* resuelve los ataques ya asignados, igual que `Resolver`. Las
      coberturas valen.
    - **Identidad del timeout:** el par (`numeroTurno`, etapa), no la fase. La etapa mover
      cubre `Reclutar` y `Mover` sin reiniciarse (Timer, regla 5), así que un
      `TurnTimeout(T, mover)` que llega después de un `Reclutar(tipo)` aceptado se aplica en
      `Mover`: cierra la fase de mover en el acto, como un `TerminarMovimiento`.
    - Con `EsperandoTirada` se rechaza con el código `EsperandoTirada` (fila 5, interno); un
      timeout de otro turno o de otra etapa, sin tirada esperada, es `TimeoutObsoleto` (fila
      8). La autoridad descarta los dos sin aviso (Timer, regla 9). **Ningún timeout de etapa
      hace perder el plan** (el reloj de partida es otra cosa: regla 14).
13. **`EvaluateMatchEnd(estado)`**, función pura. Sin Reyes vivos → `DobleRey`, gana el
    jugador del turno (**confirmado el 2026-11-20**: es quien planeó y vio la vista previa,
    pilar 1). Un solo Rey vivo → `ReyMuerto`, gana su dueño. Un Rey propio muerto por fuego
    amigo cuenta igual: gana el rival.
14. **Reloj de partida por jugador (decisión del 2026-11-24; reemplaza el cierre por tiempo
    del 2026-11-20).** Cada jugador tiene `reloj_jugador_s` = `T_tope` / 2 = **750 s (12:30)**
    para toda la partida. Corre solo durante sus turnos, en `Reclutar`, `Mover` y
    `Atacar`; **no corre en `EsperandoTirada`** (jugador, 2026-11-25): el jugador no paga la
    demora de la autoridad. **Convive con el timer de cada etapa** (mover y atacar).
    - **La placa no cuesta reloj ni bloquea** (decisión del jugador, 2026-11-27, confirmada
      en la 2.ª pasada; cierra W-1 y **reemplaza** el «comandos bloqueados durante la placa»
      de la 1.ª pasada). La regla la define el Timer (#12, regla 17) y la Máquina la cita:
      cuando una `Tirada` aceptada abre una fase (`Reclutar`, `Mover` o `Atacar`, con
      cualquier cara), la primera etapa del turno queda **pendiente** (ni la etapa ni el reloj
      corren) y se arma, **con su duración completa**, en el primero de dos instantes: la hora
      de la `Tirada` más la duración fija de la placa (`t_total_max` de #17, F1: 1,2 s; 1,5 s
      con Distracción), o la hora del **primer comando de fase aceptado** del jugador del turno
      (`Rendirse` y los rechazados no cuentan, salvo con el reloj en 0: *Fin por reloj*,
      abajo). Así no hay tiempo libre para actuar: actuar
      antes arranca el reloj en ese instante. La espera es un número fijo de la config, no una
      señal del cliente: vale igual con «Reducir efectos», online y para el Rival IA (#25). Una
      pausa del Timer (ninguna en el MVP: Timer, regla 10) también detiene esa espera, así que
      la alarga lo que dure.
      **La placa no bloquea la entrada;** los toques que caen dentro del rectángulo de la placa
      mientras tapa el tablero no pasan al tablero (regla de interfaz: Entrada, #13, e
      Interfaz del Modificador, #17). El núcleo no cambia: `Aplicar` acepta el comando como
      en cualquier momento de la fase. El turno 1 no tiene placa (regla 3).
    - **Qué queda fuera de los relojes.** En local, `EsperandoTirada` dura lo que la
      presentación de la resolución anterior (≤ 2 s por turno con ataques: la `Tirada` espera a
      Render, #18 regla 25) o, si el turno se cerró solo, lo que la placa y las leyendas
      pendientes (≤ 1,5 s, #18 Edge Cases); después viene, como mucho, la espera de la placa
      (1,2 s; 1,5 s con Distracción). Como mucho 3,2 s de pared por turno (3,5 s con
      Distracción), fuera de los dos relojes. Online, la `Tirada`
      nunca espera un mensaje de un cliente sin tope: la autoridad la envía como mucho
      `resolucion_max_s` después del cierre (pedido al Servidor, #28). **Online, la latencia de
      subida no queda fuera:** el servidor fecha cada comando al recibirlo (Timer, regla 15),
      así que la etapa y el reloj corren durante toda la subida; la gracia `g` solo cubre el
      borde del vencimiento (*Fin por reloj*, abajo). Cuánta gracia se da es del Servidor (#28)
      y del Timer (#12).
    - **Garantía.** Así ninguna partida con timer pasa de 25:00 de tiempo de turno en local
      (online, con la gracia, como mucho `T_tope` + 2g, una vez por jugador; #28); las
      esperas de arriba son tiempo de pared aparte (D2).
    - **Fin por reloj.** El tiempo lo mide la autoridad (Timer #12 local, Servidor #28
      online); el núcleo no tiene reloj. Cuando el reloj de un jugador llega a 0 en
      `Reclutar`, `Mover` o `Atacar`, la autoridad envía
      `CerrarPartida(RelojAgotado, rival, numeroTurno)` (motivo **RelojAgotado**, Librería,
      reglas 3 y 16, que lo rechaza con `CierreInvalido` si el ganador no es el rival del
      jugador del turno, si `turno` no es `numeroTurno` o si se espera una tirada). Lo ya
      movido queda; **los ataques asignados y no resueltos no se resuelven** (confirmado por
      el jugador el 2026-11-27: el reloj es un presupuesto de toda la partida, y resolver
      después de 0 regalaría una acción). **Toda autoridad cumple este invariante, mientras
      haya una etapa del jugador del turno armada o pendiente** (en `EsperandoTirada` no corre
      ningún reloj: abajo): un comando de jugador nunca se aplica con hora **en o después del
      vencimiento efectivo** del reloj del jugador del turno: el instante en que ese reloj llega a 0 más la gracia
      `g` por latencia (`g` = 0 en local; online la define #28). La hora es **la que la
      autoridad asigna al comando** (en local, la lectura de la fuente al procesarlo, Timer
      regla 2; online, la que define #28), y un comando así llega tarde: el vencimiento se
      envía antes (Timer, regla 8). La gracia es un **corrimiento fijo del vencimiento**, nunca
      una hora fechada hacia atrás, y **no se cobra**: el consumo se cobra hasta 0 y nunca más
      allá (`consumido_ms` no pasa de `reloj_jugador_s × 1000`, Timer regla 7), así que
      durante [vencimiento, vencimiento + g) el reloj se ve en 0 y quieto. **Un reloj que ya
      está en 0 al armarse una etapa vence en el instante del armado, sin gracia nueva**: si
      la etapa se arma dentro de [vencimiento, vencimiento + g) (por ejemplo, un
      `TerminarMovimiento` aceptado en la gracia que abre `Atacar`), su vencimiento efectivo es
      el instante del armado y **corta lo que quedaba de gracia** (decisión del jugador, 8.ª
      pasada; PROVISIONAL #28).
      **Con la etapa pendiente de la placa**, el primer comando de fase del jugador del turno
      arma la etapa a su hora **antes** de evaluar los vencimientos. **Con el reloj del
      jugador del turno ya en 0** (en local, anomalía del Timer, regla 9: un `CierreInvalido` a
      un `RelojAgotado`; online con g > 0, alcanzable jugando: un cierre de turno aceptado
      dentro de la gracia), **cualquier** comando de jugador de cualquiera de los dos (un emisor
      0 o 1 que pase las filas 1 a 4 de la Librería) que llegue con la etapa pendiente,
      también `Rendirse` y fuera de turno, arma la etapa a
      su hora: sale primero `CerrarPartida(RelojAgotado, rival, numeroTurno)` (gana el rival
      del jugador del turno) y el comando recibe `PartidaTerminada` (decisión del jugador,
      3.ª pasada; Timer, regla 17; AC-44). **En `EsperandoTirada` con el reloj del jugador
      del turno ya en 0** (mismos dos orígenes) no hay etapa ni vencimiento que evaluar y
      `CerrarPartida(RelojAgotado)` daría `CierreInvalido` (Librería, regla 3): un `Rendirse`
      de cualquiera de los dos **se aplica como Rendición** (decisión del jugador, 5.ª
      pasada). Si nadie se rinde, el reloj en 0 vence al armarse la etapa después de la
      `Tirada` (arriba). AC-44(iv).
    - **El modo Sin límite no tiene reloj.** **El jugador ve los dos relojes** en todo momento
      y un aviso cuando lo que vence primero es su reloj y no la etapa. **Disparo del aviso:**
      cuando se arma una etapa con `vinculante` = Reloj (Timer, regla 13; `vinculante` no
      cambia mientras la etapa corre). Clave propuesta: `hud.aviso_reloj_vinculante` (#14).
      Forma y lugar: Ficha y HUD (#15; propuesta de la revisión: el botón de avanzar en
      `Atacar` con ataques asignados, «Terminar turno · reloj 0:20», para no competir con el
      aviso de la vista previa; esa propuesta cubre solo `Atacar`: en `Mover` y `Reclutar` el
      aviso necesita otro lugar o texto, pedido a #15).
15. **`vidaQuitada` (estadística).** No hay empate por tiempo que resolver: con el reloj por
    jugador la partida termina por Rey muerto (`ReyMuerto`, o `DobleRey` si mueren los dos), reloj agotado, rendición, abandono o tablas (Tablas es un cierre técnico sin ganador: no hay oferta de empate en el MVP; Librería, regla 3)
    (2026-11-24). El estado guarda `vidaQuitada` por jugador (jugador, 2026-11-25): vida
    **realmente perdida** por piezas y muros del rival, acumulada en la partida; el fuego
    amigo no cuenta y lo que absorbe un escudero al cubrir sí. La escribe Resolución (#9), la
    lee Métricas (#19) y ninguna regla decide con ella. **Cota:** `estadistica_max` (1.000) ×
    (`C_MAX` − 1 = 999.999 piezas posibles) + 16 × `vida_tramo` de los 16 tramos rivales (#5; 16.000 con `vida_tramo` 1.000) = 1.000.015.000 como máximo; no
    desborda. `C_MAX` − 1 es una cota holgada: los IDs de pieza no pasan de `C_MAX` y cada
    pieza rival pierde como mucho su vida (≤ `estadistica_max`) una vez. **Techo de `C_MAX`**
    (hoy PROVISIONAL) para que la cota entre en int32: 1.000 × (`C_MAX` − 1) + 16.000 ≤
    2.147.483.647 ⇒ `C_MAX` ≤ 2.147.468.
16. **Eventos de la Máquina (lista cerrada).** `TurnoIniciado(numeroTurno, jugador)`,
    `FaseIniciada(fase)`, `FaseSaltada(fase)`, `ReclutaPerdido`, `TurnoTerminado` y
    `PartidaTerminada(ganador, motivo, numeroTurno, jugador del turno, fase, ataques asignados sin resolver)` (campos de la Librería, regla 8: un fin a mitad de turno se cierra sin `TurnoTerminado`) y `MovimientoDeshecho(pieza, origen, destino,
    usadosRestaurados)`, el evento de `Deshacer` que exige la Librería (regla 8: todo comando
    aceptado emite al menos uno; nombre propuesto por Métricas, #19, cierre de la tanda 4).

    **Cuándo sale cada uno** (2026-11-27):
    - `FaseIniciada(fase)` sale **una vez por cada fase que queda abierta esperando
      comandos**: `Reclutar`, `Mover` o `Atacar`. Nunca para una fase saltada, nunca para
      `Mover` con Distracción, nunca para `EsperandoTirada` (la abre `TurnoIniciado`), y
      tampoco para el `Mover` del turno 1, que nace abierto con `CrearPartida`, sin eventos (regla 3).
    - `FaseSaltada(fase)` sale solo para `Mover` o `Atacar` (regla 10); un Reclutar vacío
      da `ReclutaPerdido` (regla 6).
    - `CrearPartida` no emite eventos (regla 3).
    - `TurnoTerminado` sale en todo cierre de turno, también con ganador. Un fin **a mitad de
      turno** (`Rendirse`, `CerrarPartida`, Tablas automáticas sin cierre) no emite
      `TurnoTerminado`: lo cierra `PartidaTerminada` con sus campos (Librería, regla 8).

    **Estado al terminar y campos de `PartidaTerminada`** (lista cerrada, 2026-11-27).
    `PartidaTerminada(ganador, motivo, numeroTurno, jugador, fase, ataquesSinResolver)` lleva
    los valores del estado **después** de aplicar el comando entero (con su cascada). El
    estado conserva todo lo que la fila de la matriz de clausura no toca (Librería, regla 11);
    la tirada pendiente se borra siempre. `ataquesSinResolver` son las asignaciones vigentes
    en ese estado: una lista de pares **(`idAtacante`, `casillaObjetivo`)** ordenada por ID de
    atacante (vacía fuera de `Atacar`). El tipo de cada pieza, para mostrar «Arquero → Rey»,
    lo lee la presentación del estado final.

    | Fin | Fase, `numeroTurno`, jugador | Asignaciones, cara, bonos, pila | `ataquesSinResolver` |
    |---|---|---|---|
    | `Resolver` o `TurnTimeout` de atacar con ganador (`ReyMuerto`, `DobleRey`) | `Atacar`, sin cambio (no se abre el turno siguiente) | Asignaciones vacías (se resolvieron; regla 11); cara, tabla y bonos del último turno se conservan (regla 11); pila vacía | `[]` |
    | `Rendirse` o `CerrarPartida` (Abandono, `RelojAgotado`, Tablas) | Los de ese momento | Se conservan (en `Atacar`, las asignaciones siguen: Librería, regla 11) | Las asignaciones vigentes si la fase es `Atacar`; si no, `[]` |
    | Tablas automáticas en una fase abierta | Los de ese momento, tras el comando | Se conservan | Igual que la fila anterior |
    | Tablas automáticas tras una cascada que abrió el turno siguiente | `EsperandoTirada`, `T + 1`, jugador que entra | Las del turno nuevo (vacías) | `[]` |

    **Firma única de `CerrarPartida`:** `CerrarPartida(motivo, ganador, turno)` (Librería,
    regla 3), con `vacío` explícito donde no aplica: `(Abandono, j, vacío)`,
    `(Tablas, vacío, vacío)`, `(RelojAgotado, rival, numeroTurno)`.

    **Orden de los eventos en un Aplicar** (lista cerrada; los de otros sistemas van en el
    lugar que se indica). *Apertura de una fase F* = `FaseIniciada(F)` y la cascada se detiene,
    o `FaseSaltada(F)` y se abre la siguiente. *Cierre del turno* = `TurnoTerminado` → si
    `EvaluateMatchEnd` da ganador, `PartidaTerminada` y nada más; si no,
    `CoberturaQuitada(…, Caducada)` del jugador que entra, por ID de escudero (#8, regla 9) →
    `TurnoIniciado(numeroTurno + 1, jugador)`.

    | Comando aceptado | Eventos, en orden |
    |---|---|
    | `Tirada` | Evento de la tirada (cara y tablas, #11) → según el resultado: `Reclutar` (`FaseIniciada(Reclutar)`, o `ReclutaPerdido` y apertura de `Mover`); `Mover` (apertura de `Mover`); `Atacar` (Distracción: apertura de `Atacar`). Si `Mover` se salta, apertura de `Atacar`; si `Atacar` se salta, cierre del turno |
    | `Reclutar(tipo)` | `Reclutado` (#10) → apertura de `Mover` → (si se salta) apertura de `Atacar` → (si se salta) cierre del turno |
    | `TurnTimeout` de mover en `Reclutar` | `Reclutado` con `automatico` (#10) → apertura de `Atacar` → (si se salta) cierre del turno |
    | `TerminarMovimiento` · `TurnTimeout` de mover en `Mover` | Apertura de `Atacar` → (si se salta) cierre del turno |
    | `Resolver` · `TurnTimeout` de atacar | Eventos de Resolución (#9, regla 14; Librería, regla 8), que incluyen `CoberturaQuitada(…, EscuderoMuerto)` → cierre del turno, con `CoberturaQuitada(…, Caducada)` del jugador que entra: un `Resolver` puede emitir `CoberturaQuitada` en los dos lugares |
    | `Mover` · `Deshacer` | `PiezaMovida` (#6) · `MovimientoDeshecho` |
    | `AsignarAtaque` · `QuitarAtaque` · `Cubrir` · `QuitarCobertura` | Los de #7 y #8 |
    | `Rendirse` · `CerrarPartida` | Solo `PartidaTerminada` (sin `TurnoTerminado`) |
    | Cualquiera que deje `contadorComandos` en C_MAX − 1 con la partida en curso | Sus eventos y, **al final**, `PartidaTerminada(Tablas)`. Si el comando ya terminó la partida, se conserva su motivo y no se emite un segundo `PartidaTerminada` (Librería, regla 16) |
17. **Matriz de comandos por fase.** Sale del orden de validación de la Librería (regla 4).
    **Precondiciones de cada celda:** versión válida (fila 1); **tipo de emisor correcto**: la
    autoridad para los comandos de autoridad y un jugador (0 o 1) para los de jugador (si no,
    fila 4, `SoloAutoridad` o `SoloJugador`: la penúltima fila de esta matriz; un emisor que no
    es 0, 1 ni la autoridad da `EmisorInvalido`, que prueba la Librería); **jugador del turno**
    para los comandos de jugador salvo `Rendirse`, que acepta a cualquiera de los dos (si no,
    fila 11, `NoEsTuTurno`, que va **después** de la 5 y de la 10: la última fila de esta matriz);
    contadores correctos **solo en los comandos que los llevan**
    (`contadorEsperado` en los de jugador salvo `Rendirse`; `contadorTiradaEsperado` en la
    `Tirada`, y su `n` en 0–99: fila 7, `TiradaVieja` o `TiradaInvalida`); la forma de `CerrarPartida` válida para su motivo (regla 16, *Firma única*;
    fila 9); y la pieza, el tipo o el objetivo válidos. Las filas de `TurnTimeout` nombran el
    turno actual. **El turno y la etapa del `TurnTimeout` no son precondición ni contador:**
    un `TurnTimeout` con un turno o una etapa que no son los actuales da `TimeoutObsoleto`
    (fila 8) en una fase abierta, y `EsperandoTirada` en `EsperandoTirada` (la fila 5 va
    antes), sea cual sea su turno (regla 12; AC-39). Con eso, la celda dice
    el resultado de las filas 2, 4, 5, 6, 8, 9, 11 y 13. Las demás filas quedan fuera de la
    matriz por sus precondiciones: la 7 (contador y `n` de la `Tirada`, Librería), la 10
    (`ComandoViejo`, AC-23 con el contador viejo), la 12 (pieza inexistente o del rival, que
    prueba cada sistema dueño), la 15 (`LimitePartida`, AC-42) y `EmisorInvalido` (fila 4,
    Librería). ✓ = aceptado si pasa las reglas del sistema dueño (fila 14). La columna `Terminada` es la partida con motivo (regla 2), sea
    cual sea su fase. **En `Terminada` gana la fila 2:** todo comando da `PartidaTerminada`
    aunque el emisor, el turno, el contador o la forma estén mal, porque la fila 2 va antes que
    la 4, la 9, la 10 y la 11 (Librería, regla 4). **Con contador viejo y fuera de turno a la
    vez**, en una fase abierta gana `ComandoViejo` (la fila 10 va antes que la 11).

    | Comando | `EsperandoTirada` | `Reclutar` | `Mover` | `Atacar` | `Terminada` |
    |---|---|---|---|---|---|
    | `Tirada` (autoridad) | ✓ | `TiradaNoEsperada` | `TiradaNoEsperada` | `TiradaNoEsperada` | `PartidaTerminada` |
    | `Reclutar` | `EsperandoTirada` | ✓ | `FaseIncorrecta` | `FaseIncorrecta` | `PartidaTerminada` |
    | `Mover`, `Deshacer`, `TerminarMovimiento` | `EsperandoTirada` | `FaseIncorrecta` | ✓ | `FaseIncorrecta` | `PartidaTerminada` |
    | `AsignarAtaque`, `QuitarAtaque`, `Cubrir`, `QuitarCobertura`, `Resolver` | `EsperandoTirada` | `FaseIncorrecta` | `FaseIncorrecta` | ✓ | `PartidaTerminada` |
    | `TurnTimeout(numeroTurno, mover)` (autoridad) | `EsperandoTirada` | ✓ | ✓ | `TimeoutObsoleto` | `PartidaTerminada` |
    | `TurnTimeout(numeroTurno, atacar)` (autoridad) | `EsperandoTirada` | `TimeoutObsoleto` | `TimeoutObsoleto` | ✓ | `PartidaTerminada` |
    | `Rendirse` (cualquier jugador; se salta las filas 5 y 10 a 15, Librería regla 4) | ✓ | ✓ | ✓ | ✓ | `PartidaTerminada` |
    | `CerrarPartida(Abandono)` · `CerrarPartida(Tablas)` | ✓ | ✓ | ✓ | ✓ | `PartidaTerminada` |
    | `CerrarPartida(RelojAgotado)` | `CierreInvalido` | ✓ | ✓ | ✓ | `PartidaTerminada` |
    | Emisor incorrecto (fila 4): un jugador envía `Tirada`, `TurnTimeout` o `CerrarPartida` · la autoridad envía un comando de jugador (`Rendirse` incluido) | `SoloAutoridad` · `SoloJugador` | `SoloAutoridad` · `SoloJugador` | `SoloAutoridad` · `SoloJugador` | `SoloAutoridad` · `SoloJugador` | `PartidaTerminada` |
    | Jugador fuera de turno (fila 11): el jugador que no es el del turno envía un comando de jugador salvo `Rendirse` (con contador correcto) | `EsperandoTirada` | `NoEsTuTurno` | `NoEsTuTurno` | `NoEsTuTurno` | `PartidaTerminada` |

### States and Transitions

| Desde | Hacia | Cuándo |
|---|---|---|
| (creación) | `Mover` (turno 1, jugador 0) | `CrearPartida` con castillos fijos (Tablero, #4); sin tirada ni reclutar |
| `EsperandoTirada` (turno ≥ 2) | `Reclutar` | `Tirada` válida → `ReclutarObligatorio` con algún tipo disponible |
| `EsperandoTirada` | `Mover` | `Tirada` → `Seguir` o `LimiteMovimientos(sin límite)`; o `ReclutarObligatorio` sin tipo disponible (`ReclutaPerdido`, sin pasar por `Reclutar`) |
| `EsperandoTirada` | `Atacar` | `Tirada` → `SaltearMovimiento` |
| `Reclutar` | `Mover` | `Reclutar(tipo)` |
| `Reclutar` | `Atacar` | Recluta automático por `TurnTimeout` de mover (regla 12) |
| `Mover` | `Atacar` | `TerminarMovimiento` o `TurnTimeout` de mover (la pila se vacía) |
| `EsperandoTirada` o `Reclutar` | `Atacar` | La fase que abre el comando (`Mover`) empieza sin acción posible: `FaseSaltada(Mover)` y se abre `Atacar` (regla 10) |
| `EsperandoTirada`, `Reclutar` o `Mover` | `EsperandoTirada` (turno + 1, rival) | El comando abre `Atacar` (directo o tras saltar `Mover`) y `Atacar` empieza sin acción posible: `FaseSaltada(Atacar)` y cierre del turno sin resolución. Incluye `EsperandoTirada` → `EsperandoTirada` (Tirada con Distracción y `Atacar` sin acción posible, o con las dos fases saltadas) |
| `Atacar` | `EsperandoTirada` (turno + 1) | `Resolver` o `TurnTimeout` de atacar, sin ganador |
| `Atacar` | `Terminada` (fase `Atacar`) | `Resolver` o `TurnTimeout` con ganador (`ReyMuerto`, `DobleRey`) |
| cualquier estado en curso | `Terminada` (fase conservada) | `Rendirse`, o `CerrarPartida` (Abandono, `RelojAgotado` salvo en `EsperandoTirada`, Tablas) |
| cualquier estado en curso | `Terminada` (Tablas) | Cierre automático: el Aceptado que deja `contadorComandos` en `C_MAX − 1` termina la partida en Tablas en ese mismo Aplicar, después de su cascada (Librería, regla 16; regla 16, *Estado al terminar*) |
| `Terminada` | `Terminada` | Todo comando se rechaza con `PartidaTerminada` |

`Terminada` es la partida con motivo (regla 2); la columna «Hacia» dice qué fase conserva.

### Interactions with Other Systems

| Sistema | La Máquina le da | Recibe | Dueño de la interfaz |
|---|---|---|---|
| 1 Librería | Fases y reglas dentro del contrato | Comandos, códigos, enteros y orden | Librería (contrato) y #3 (fases) |
| 11 Modificador | Pide la tirada y le pasa la cara | Un resultado tipado y los bonos en el estado, en el orden del gancho (regla 5) | Máquina define los tipos y el orden; Modificador los produce |
| 10 Reclutamiento | Abre `Reclutar`, ejecuta el auto-recluta | Consulta "¿tipo disponible?" y `Reclutar` | Reclutamiento |
| 6 Movimiento | Abre `Mover`, guarda la pila | `MovimientosLegales` y el límite del turno | Movimiento |
| 7 Alcance y asignación | Abre `Atacar` | `ObjetivosValidos` | Alcance y asignación |
| 8 Cobertura | Borra coberturas al abrir turno del dueño | Reglas de `Cubrir` | Cobertura |
| 9 Resolución | Llama tras `Resolver` | Estado nuevo, eventos y contador `vidaQuitada` | Resolución |
| 12 Timer | `numeroTurno`, etapa actual y el invariante del reloj (regla 14) | `TurnTimeout`, `CerrarPartida(RelojAgotado)` y la regla de arranque de la etapa tras la placa (Timer, regla 17) | Timer |
| 28 Servidor | Mismo pipeline; los pedidos de la regla 14 (Tirada con tope, etapa armada con la espera fija de la placa o con el primer comando aceptado según el reloj del servidor, gracia como corrimiento del vencimiento, turno 1 armado con los dos listos y con tope `listo_max_s`) | Es la autoridad de los comandos de autoridad | Servidor |
| 15 HUD, 19 Métricas, 25 Rival IA | Eventos de fase, etapa y pila, en el orden de la regla 16 | Nada (IA: mismos comandos que un jugador) | Cada GDD |
| 16 Vista previa | Fase `Atacar`, jugador del turno y el `PartidaTerminada` que daría `Resolver` | Nada | Vista previa |
| 17 Interfaz del Modificador | Fase `EsperandoTirada`, `TurnoIniciado`, `FaseIniciada`, `ReclutaPerdido`; que la placa no bloquea y que los toques dentro de su rectángulo no pasan al tablero (regla 14) | La duración de la placa (`t_total_max`, F1), que fija la espera de la etapa (Timer, regla 17). El `Reclutar` lo manda Entrada. Durante la cosecha local el HUD muestra el estado presentado (#15, regla 1) y no un «esperando la tirada» | Interfaz del Modificador |
| 18 Render | `TurnoTerminado`, `TurnoIniciado`, `FaseIniciada`, `FaseSaltada`, `PartidaTerminada`, `MovimientoDeshecho` | En local, el momento de la `Tirada` (#18, regla 25) | Render (cola y cuándo termina la presentación) |

**Supuestos PROVISIONALES sobre GDD sin escribir:** ninguno sobre #9 (el contador `vidaQuitada` y su cota los confirmó Resolución al cerrar la tanda 3, 2026-11-25). Las piezas reclutadas se mueven ese turno, salvo las
del recluta automático (#6, regla 11; #10, reglas 9 y 10). Con una catapulta o un demoledor
propios vivos, `Atacar` nunca se salta (#7, regla 14).

## Formulas

Hay dos fórmulas de diseño (D1, D2), que **no corren en el núcleo** porque no tiene reloj
(Librería, regla 5). Las usan el Timer (#12), el Servidor (#28) y las Métricas (#19). La
otra (D4) es una regla pura del núcleo; D3 se retiró.

### D1. Duración de la partida y presupuesto de turnos

La fórmula `turnos_presupuesto` se define así (una sola fórmula; decisión del jugador, 4.ª
pasada: reemplaza la de «bolsa común» `⌊T_tope ÷ seg_medio⌋`, que contaba de más). Se
calcula en **milisegundos enteros y sin redondeos intermedios** (5.ª pasada: redondear cada
turno a segundos inflaba `k`):

`duracion_ms_J = Σ consumido_ms_t` (los turnos t **cerrados o cortados por `RelojAgotado`** del jugador J), J = 0 (azul), 1 (rojo)
`k_J = ⌈(reloj_jugador_s × 1.000 × N_J) ÷ max(duracion_ms_J, 1.000 × N_J)⌉ − 1` si N_J ≥ 1;
`k_J = reloj_jugador_s − 1` si N_J = 0
`turnos_presupuesto = min(2 × k_0, 2 × k_1 + 1)`

**Por qué así.** `k_J` son los turnos que J **completa** con su reloj por encima de 0 a su
ritmo medio `m_J = duracion_ms_J ÷ N_J` (con piso de 1.000 ms, el `max`): k turnos caben si
`k × m_J < reloj_jugador_s × 1.000` (un turno que deja el reloj justo en 0 ya venció: Timer,
regla 8 y D3), y el mayor k que lo cumple es `⌈reloj_ms ÷ m_J⌉ − 1`; escrito con `N_J` en el
numerador queda en enteros (`⌈a ÷ b⌉ = (a + b − 1) div b`, en int64). **Qué turnos cuentan**
(decisión del jugador, 7.ª pasada): los cerrados con `TurnoTerminado` **y el cortado por
`RelojAgotado`**, que es un dato completo (el jugador gastó todo su reloj: con él,
`duracion_ms_J` = `reloj_jugador_s × 1.000` y `k_J` = ⌈reloj × N_J ÷ reloj⌉ − 1 = N_J − 1,
exactamente los turnos que completó). **No cuenta** un turno cortado por `Rendirse`,
`CerrarPartida(Abandono)` o Tablas (automáticas o `CerrarPartida(Tablas)`), que es un corte por decisión, parcial, y bajaría el promedio: azul
con 8 turnos y 720.000 ms y un 9.º cortado por `Rendirse` a los 5.000 ms da, contándolo,
`k_0` = ⌈750.000 × 9 ÷ 725.000⌉ − 1 = 9 y el presupuesto 17; sin contarlo, 16. **Excluir el
corte por reloj sobrecuenta** con ritmo variable: azul con 8 turnos y 662.000 ms y el 9.º
cortado por `RelojAgotado` (88.000 ms), rojo con 8 y 720.000 ms → contándolo, `k_0` = 8 y 16
(lo real); excluyéndolo, ⌈6.000.000 ÷ 662.000⌉ − 1 = 9 y 17. Es **exacta con ritmo constante** por
jugador; con un ritmo que cambia (Variable, 90 → 60 s) estima el presupuesto al ritmo
medio.
Azul juega los turnos impares: agota su reloj en su turno k_0 + 1, el turno 2·k_0 + 1 de la
partida, y deja 2·k_0 turnos completos. Rojo lo agota en su turno k_1 + 1, el turno
2·k_1 + 2, y deja 2·k_1 + 1. Gana el que llega antes: el `min`. **Con ritmos iguales**
(`seg_medio_0 = seg_medio_1 = seg_medio`) queda `2 × k` y agota primero azul, que empieza.
Para el informe (solo informativo, no entra en `k_J`) se dan también
`duracion_partida` (la suma de `consumido_ms` de todos los turnos jugados, en s hacia abajo),
`seg_medio = max(1, ⌊duracion_partida ÷ N⌋)` y `seg_medio_J = max(1, ⌊duracion_ms_J ÷
(1.000 × max(1, N_J))⌋)`.

**Solo modos con timer.** En Sin límite no hay `turno_max_s` ni reloj: D1 no se calcula
(el informe de Métricas lo muestra como «sin presupuesto»).

**Variables:**

| Variable | Type | Range | Source | Description |
|----------|------|-------|--------|-------------|
| N | int | 1–C_MAX + 1 (v4: 24 y 34) | calculated (Métricas #19) | Turnos jugados: los `TurnoTerminado` más 1 si la partida terminó a mitad de turno (un turno abierto por la cascada en el que la partida termina antes de armarse su primera etapa no cuenta: no se jugó; por ejemplo, Tablas automáticas al abrirlo, o un `Rendirse` o un `Abandono` durante la cosecha o la placa, que el Timer mide como `TurnoMedido(n + 1, …, 0, 0, …, parcial)`). El turno 1 cuenta aunque la partida termine en él, así que N ≥ 1 |
| N_J | int | 0–C_MAX + 1 | calculated (Métricas #19) | Turnos del jugador J **cerrados** (sus `TurnoTerminado`) **o cortados por `RelojAgotado`**; un turno cortado por `Rendirse`, `CerrarPartida(Abandono)` o Tablas no cuenta. Por ejemplo, 0 para rojo si la partida terminó en el turno 1 |
| consumido_ms_t | int | 0–`turno_max_s` × 1.000 (45.000–93.000, Timer D1 y guarda 1) | calculated (Métricas #19, `TurnoMedido` del Timer, regla 12) | Milisegundos de **reloj** consumidos en el turno t: `ms_mover + ms_atacar` de su `TurnoMedido`; no incluye las esperas fuera de los relojes (regla 14). El 0 se da con un turno sin fases o con la anomalía de `CierreInvalido` (Timer, regla 9). Un `TurnoMedido` con `parcial` verdadero solo puede ser el último turno de la partida y **cuenta solo si `PartidaTerminada.motivo` = `RelojAgotado`** (con `Rendición`, `Abandono` o `Tablas` no cuenta); un último turno cerrado con ganador (`ReyMuerto`, `DobleRey`) tiene `parcial` falso y cuenta |
| duracion_ms_J | int | 0–`reloj_jugador_s` × 1.000 (el techo, inclusive, solo se alcanza con el turno cortado por `RelojAgotado`; con turnos cerrados queda por debajo) | calculated (Métricas #19) | Suma de `consumido_ms_t` de los turnos de J que cuentan (cerrados o cortados por `RelojAgotado`) |
| reloj_jugador_s | int | 645–750 | calculated (D2) | Reloj de cada jugador |
| k_J | int | 8–749 | calculated | Turnos que J completa con reloj. Piso 8: un turno que cuenta consume como mucho `turno_max_s` × 1.000, así que `m_J ≤ turno_max_s × 1.000`, y la guarda 1 del Timer (`turno_max_s × 8 < reloj_jugador_s`) da `reloj_ms ÷ m_J > 8`, así que `⌈…⌉ ≥ 9`. Techo: con `m_J` = 1.000 (piso) y reloj 750, `750 − 1`; con N_J = 0, igual |
| turnos_presupuesto | int | 16–1.498 (2 × 8 a `min(2 × 749, 2 × 749 + 1)`) | calculated | Turnos **completos** que caben antes de que el primero de los dos agote su reloj |

**Output Range:** `duracion_partida` de 0 a menos de `T_tope` s de reloj (D2 lo garantiza;
la pared suma las esperas aparte); `turnos_presupuesto` de 16 a 1.498. Online, con una
gracia g > 0, el reloj puede consumir hasta `T_tope` + 2g (una vez por jugador), y para quien
cerró dentro de la gracia `k_J` = N_J − 1 (PROVISIONAL #28). Meta de diseño:
900–1.200 s. Los ejemplos v4 de abajo son **tiempo de pared** con el timer viejo: sirven
como referencia de ritmo, no como valores del rango.

**Example (partidas v4 del 2026-11-19, repartidas en partes iguales, aproximado):** partida
1: N = 24, 1.381 s → unos 57,5 s por turno → `k` = ⌈750 ÷ 57,5⌉ − 1 = 13 →
`turnos_presupuesto` 26. Partida 2: N = 34, 1.372 s → unos 40,4 s → `k` = ⌈18,6⌉ − 1 = 18 →
36. Las dos usaron menos turnos
que su presupuesto, pero terminaron en ~23 min: **3 minutos por encima de la meta de 15–20.**

| Ritmo medio `m_J` en s (los dos iguales, reloj 750) | 40 | 50 | 57 | 60 | 75 | 90 |
|---|---|---|---|---|---|---|
| `turnos_presupuesto` | 36 | 28 | 26 | 24 | 18 | 16 |

Con 75 s el cociente es exacto (10): el turno 10 de azul deja el reloj en 0, vence y no
es un turno completo; por eso da 18 y no 20. **Sin redondeo:** 10 turnos de 68,5 s dan
⌈750.000 × 10 ÷ 685.000⌉ − 1 = 10 → 20 (redondeando cada turno a 68 s saldría 22, pero
11 × 68,5 = 753,5 s no caben). Para entrar en la meta de 15–20 min hacen falta 16–21 turnos
a 57 s por turno, o 23–30 a 40 s.

**Ritmos distintos.** Ejemplo: azul a 60 s y rojo a 30 s → `k_0` 12, `k_1` 24 →
min(24, 49) = **24** (azul agota su reloj en su turno 13, el turno 25). Con el promedio
común (45 s) la fórmula daría 2 × (⌈750 ÷ 45⌉ − 1) = 32. **Caso espejo:** azul a 30 s y rojo
a 60 s → min(48, 25) = **25** (rojo agota su reloj en su turno 13, el turno 26). **Ritmos
cercanos:** azul a 70 s y rojo a 72 s → `k_0` = ⌈10,71⌉ − 1 = 10 y `k_1` = ⌈10,42⌉ − 1 = 10 →
min(20, 21) = **20**: azul necesita 11 × 70 = 770 s para su turno 11 y agota primero, aunque
es el más rápido. **Uso máximo** (Fijo 90, Timer AC-21): azul cierra 8 turnos de 90 s y se
le corta el 9.º; rojo cierra 8 de 90 s → `k_0` = `k_1` = 8 → min(16, 17) = **16**, el turno
17 cortado, como en el Timer. El informe de Métricas da `turnos_presupuesto` y, aparte,
`seg_medio`.

**Riesgo medido, no supuesto (2026-11-27).** Repartidas en partes iguales, las dos partidas
v4 dan unos 690 s por jugador contra 750: menos de un turno de 90 s de margen. Son datos
de pared con el timer viejo (incluyen esperas y presentación), así que la cifra es
pesimista, pero indica que el reloj puede decidir partidas normales. Lo vigila AC-43.

**Aporte de los demás sistemas.** Resolución (#9), Reclutamiento (#10) y Modificador (#11)
declaran en su GDD cuántos turnos suman por partida (`Δturnos`); el de Resolución es **0 por
definición** (los turnos que hacen falta para matar son la base; Métricas, F4).
`turnos_estimados = N_base + ΔResolución + ΔReclutamiento + ΔModificador`, y
`turnos_estimados ≤ turnos_presupuesto`. `N_base` no se mide directamente (las dos v4
incluyen remontadas y no hay partida sin ellas): se estima con `N_base_estimado` de
Métricas (#19, F4), que es una **cota inferior**. Por eso la comparación solo sirve en un
sentido: si `turnos_estimados` ya supera el presupuesto, el presupuesto se pasa; si no lo
supera, no prueba que se cumpla.

### D2. Reloj de partida por jugador

La fórmula `reloj_jugador_s` se define así:

`reloj_jugador_s = T_tope ÷ 2` (división entera hacia abajo)

**Variables:**

| Variable | Type | Range | Source | Description |
|----------|------|-------|--------|-------------|
| T_tope | int | 1.290–1.500 (valor 1.500) | data file (config de la autoridad; en el Timer, `tope_partida_s`) | Tope de partida: 25 min, pilar 4. El rango es el del Timer (#12, Tuning Knobs) |
| reloj_jugador_s | int | 645–750 (valor 750) | calculated | Segundos de cada jugador para toda la partida |

**Output Range:** 645 a 750 s; 750 s (12:30) con el valor vigente. **Sin límite: exento**, no
hay reloj. **El rango es el del Timer** (decisión del jugador, 4.ª pasada: reemplaza el piso
1.442 de la 2.ª pasada). Lo que protege a los modos de 90 s es la **guarda 1 del Timer**
(`turno_max_s × 8 < reloj_jugador_s`: una partida de 15 turnos con uso máximo no termina por
reloj), que se evalúa por modo: con `T_tope` < 1.442 (`reloj_jugador_s` < 721) Fijo 90 y
Variable dan `ConfigTimerInvalida(guarda_1)` y solo pasan Fijo 60 y Sin límite; online (solo
Variable) eso deja sin modo. Fuera de 1.290–1.500 el Timer rechaza con
`ConfigTimerInvalida(tope_partida_s)` en todos los modos.

**Garantía:** los dos relojes suman como mucho `T_tope`, así que el tiempo de turno jugado no
pasa de 25:00 (en local; online, con gracia, ver regla 14). Quedan afuera, como tiempo de
pared: la espera de la `Tirada` (con tope `resolucion_max_s` online) y la espera de la placa
de la cara (regla 14). Online, la latencia de subida de cada comando **sí** corre sobre la
etapa y el reloj (regla 14). En local, con los valores vigentes, son como mucho 3,2 s por turno (3,5 s con
Distracción): unos 1,5 min en una partida de 26 turnos. **Lo que eso le pide a Métricas (#19):** la compuerta del arte
compara `duracion_pared_s` con `T_tope`, y la pared incluye esas esperas, así que una
partida que usa los dos relojes enteros no pasaría nunca. #19 decide si compara el tiempo
de reloj consumido o la pared menos las esperas.

Con un `T_tope` impar se pierde 1 s (la división redondea hacia abajo) y se acepta: la suma
de los relojes sigue sin pasar de `T_tope` (decisión del jugador, 4.ª pasada: se retira el
rechazo del impar).

**Example:** azul gastó 740 s y abre su turno con 10 s. Aunque la etapa de mover dure 60 s, a
los 10 s la autoridad envía `CerrarPartida(RelojAgotado, rojo, numeroTurno)`.

### D3. Retirada

*(Las fórmulas del cierre anterior, D2 y D3 del 2026-11-20, se retiraron el 2026-11-24. La
cota de `vidaQuitada` pasó a la regla 15.)*

### D4. Capacidad de la pila de deshacer

La fórmula `pila_max` se define así:

`pila_max = Σ_t tope_vivas_t × movimiento_t` (t = los 7 tipos del Catálogo, Rey incluido): el peor caso **sin bono**, con Arremetida (sin tope de piezas).
`cota_grito = (movimiento_rey + bono) + Σ de los n_grito valores mayores de (movimiento_t + bono)`, con `n_grito = min(limite_normal, piezas_sin_rey_max)` y `bono = bono_grito_movimiento`: el peor caso **con el Grito** (la fórmula es la F5 de Movimiento, #6). La suma se toma sobre el **multiconjunto de piezas sin el Rey**: cada tipo t aparece `tope_vivas_t` veces con valor `movimiento_t + bono`, y se suman los `n_grito` mayores (no los `n_grito` tipos mayores).
`pila_peor = max(pila_max, cota_grito)`

**Invariantes (dos, en dos lugares):**
- `pila_max ≤ pila_tope` (256): **F8 del Catálogo**. `PrepararContexto` rechaza el Catálogo
  con `ConfigInvalida(F8)` (Catálogo, regla 6).
- `cota_grito ≤ pila_tope`: lo valida `CrearPartida` con la config, en el orden de la
  Librería (regla 2): la pila con bono va **después de `limite_normal` y `tablaReclutas`**
  (que van después de F9 y de T4 y T5) **y antes de T6**. Rechaza con `ConfigInvalida` y el
  código del campo (Librería, regla 2: «o el del campo»); el nombre propuesto es
  **`PilaGrito`** y lo fija el ADR.

Con los dos, `pila_peor ≤ 256` en cualquier turno. Los dos hacen falta: un Catálogo que pasa
F8 y F9 puede romper la cota con el Grito (por ejemplo, 37 piezas de movimiento 6 y 23 de
movimiento 1 dan `pila_max` 251, pero con Grito y `limite_normal` 37 dan 37 × 8 + 8 = 304).

**Variables:**

| Variable | Type | Range | Source | Description |
|----------|------|-------|--------|-------------|
| tope_vivas_t | int | 1–55 (Rey: 1) | data file (Catálogo, F5 y F6) | Piezas vivas máximas del tipo t |
| movimiento_t | int | 1 a 12 − `bono` (F9; hoy 2–6) | data file (Catálogo) | Puntos de movimiento del tipo t |
| bono | int | 0–10 (hoy 2) | data file (config, Modificador #11) | `bono_grito_movimiento` |
| limite_normal | int | 1–37 (hoy 12) | data file (config, Movimiento #6) | Piezas sin el Rey que se mueven en un turno normal |
| movimiento_rey | int | 1 a 12 − `bono` (hoy 6) | data file (Catálogo) | `movimiento_t` del Rey |
| piezas_sin_rey_max | int | hoy 60 | calculated (Catálogo) | Σ `tope_vivas_t` de los 6 tipos sin el Rey (12 + 16 + 12 + 8 + 8 + 4) |
| n_grito | int | 1–37 | calculated | `min(limite_normal, piezas_sin_rey_max)`: piezas sin el Rey que el Grito deja mover |
| pila_tope | int | 256 | constant (contrato) | Tope de entradas de la pila |
| pila_max | int | 7–256 (Catálogo válido) | calculated | Peor caso sin bono; 7 es la cota teórica (un ejemplar de cada tipo, movimiento 1); el despliegue del Tablero (F5 y F6 del Catálogo) la sube |
| cota_grito | int | 2–256 (config válida) | calculated | Peor caso con el Grito (2 con `limite_normal` 1, bono 0 y todo en movimiento 1) |
| pila_peor | int | 7–256 | calculated | `max(pila_max, cota_grito)` |

**Output Range:** con el Catálogo y la config vigentes, `pila_max` 254 y `cota_grito` 104
(254 con `limite_normal` 37); 2 de margen.

**Example:** soldado 12 × 4 = 48; arquero 16 × 4 = 64; escudero 12 × 4 = 48; catapulta
8 × 2 = 16; jinete 8 × 6 = 48; demoledor 4 × 6 = 24; Rey 1 × 6 = 6 → **254**. El peor caso
es una Arremetida (sin tope de piezas, sin bono de movimiento) moviendo todo un paso por
comando. Con Grito de guerra (12 piezas más el Rey, +2 de movimiento) el máximo es
12 × 8 + 8 = 104. El movimiento efectivo con Grito lo acota la regla F9 del Catálogo
(`movimiento + bono_grito_movimiento ≤ 12`, validada en `CrearPartida`).

**Dónde se rompe:** subir el movimiento del soldado en 1 da 266 > 256. Por eso el
invariante.

**Confirmado por Movimiento (#6, F5):** cada `Mover` cuesta 1 punto o más, así que una pieza
nunca deja más entradas que puntos tiene. **El 37 es el tope duro de la config** de
`limite_normal` (Librería, regla 2; Movimiento, Tuning) y, con el Catálogo vigente, coincide
con lo que deja la cota: con 38, `cota_grito` daría 260. Son dos validaciones distintas: con
un Catálogo más lento, el tope de 37 sigue rechazando 38, y con uno más rápido la cota puede
rechazar valores menores.

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
  disponible **de la cara**, en el orden del Catálogo) y pasa **directo a `Atacar`**, sin
  `Mover`; la pieza reclutada no se mueve ese turno, pero puede atacar o cubrir. Si `Atacar`
  empieza sin acción posible, se salta y el turno cierra en ese mismo comando.
- **Si el timeout de mover llega en `Mover`:** las piezas ya movidas quedan donde están, no
  se recluta y pasa a `Atacar`.
- **Quién gana una carrera.** El orden es **el orden en que la autoridad aplica los
  comandos** (`contadorComandos` solo lo registra: `TurnTimeout` no lleva
  `contadorEsperado`). El que llega segundo se descarta sin efecto. Qué código recibe depende
  de quién llegó segundo:
  - **Un comando de autoridad que llega segundo** (lo descarta la autoridad, sin aviso al
    jugador; Timer, regla 9):
    - `EsperandoTirada` (fila 5, antes que la 8): el comando que ganó **cerró el turno**. Es
      cualquier comando cuya cascada cierre el turno: `Resolver`, `Reclutar(tipo)` o un
      `TerminarMovimiento` con las fases siguientes saltadas (una `Tirada` que salta todas las
      fases también cierra el turno, pero no compite con ningún timeout: no hay etapa armada).
      Online es la carrera más común: `Resolver` contra el timeout de atacar (en local
      la regla 8 del Timer la evita: un comando con hora anterior al vencimiento gana siempre).
    - `TimeoutObsoleto` (fila 8): el timeout nombra un turno o una etapa que no son los
      actuales y no se espera tirada (por ejemplo, el de mover después de un
      `TerminarMovimiento` que abrió `Atacar`, o uno del turno T que llega en el `Mover` del
      turno T + 1).
    - `CierreInvalido` (fila 9): un `CerrarPartida(RelojAgotado)` que llega después de un
      `Resolver` que cerró el turno (Librería, Edge Cases). En local es una anomalía: con la
      regla 8 del Timer no debería pasar, y el Timer la registra (Timer, regla 9).
    - `PartidaTerminada` (fila 2): el comando que ganó terminó la partida.
  - **Un comando de jugador que llega tarde** (`ComandoViejo` es un código interno y el
    jugador lo ve como aviso neutro, Librería *UI Requirements*; `EsperandoTirada` y
    `PartidaTerminada` son visibles con su propio texto, Librería regla 4, filas 5 y 2. Que
    después de un vencimiento se lean como «el turno terminó por tiempo» lo piden Entrada,
    #13, y HUD, #15). `Rendirse` es la excepción: si el timeout cerró el turno y la partida
    sigue, se acepta (Librería, regla 4):
    - `ComandoViejo` (fila 10): el timeout dejó la partida en **otra fase abierta del mismo
      turno** (por ejemplo, `TerminarMovimiento` después del timeout de mover que abrió
      `Atacar`).
    - `EsperandoTirada` (fila 5): el timeout **cerró el turno** (el de atacar, o el de mover
      con `Atacar` saltado). Un `Resolver` después del timeout de atacar da `EsperandoTirada`,
      no `ComandoViejo`; recién después de la tirada siguiente daría `ComandoViejo`.
    - `PartidaTerminada` (fila 2): el vencimiento terminó la partida (`RelojAgotado`).
  Ninguno de estos casos es un error de la partida.
- **Si un `Reclutar(tipo)` se acepta y después llega `TurnTimeout(T, mover)` del mismo
  turno:** se aplica en `Mover` (la etapa mover cubre las dos fases, regla 12) y cierra la
  fase de mover en el acto; la pieza reclutada se queda donde se desplegó.

### Fin de partida y tiempo

- **Si mueren los dos Reyes en la misma resolución** (también con fuego amigo, también en
  un timeout de atacar): `DobleRey`, gana el jugador del turno.
- **Si el Rey propio muere por fuego amigo y el del rival sobrevive:** `ReyMuerto`, gana el
  rival.
- **Si el reloj de un jugador llega a 0 en su turno** (en `Reclutar`, `Mover` o `Atacar`):
  pierde con `RelojAgotado` (en local, en ese instante; online, al terminar la gracia g, o en
  el armado si la etapa se armó con el reloj ya en 0: PROVISIONAL #28). Lo movido queda; los ataques asignados y no resueltos no se
  resuelven, aunque uno hubiera matado al Rey rival (regla 14). La pantalla de resultado los
  lista (vienen en `PartidaTerminada`).
- **Si un comando de jugador llega, con una etapa del jugador del turno armada o pendiente,
  con hora en o después del vencimiento efectivo del reloj del jugador del turno** (en
  local, ese reloj en 0; online, en o después de vencimiento + g, o del armado si la etapa se
  armó con el reloj ya en 0, regla 14): no se aplica; la
  autoridad envía antes `CerrarPartida(RelojAgotado, …)` y el comando recibe
  `PartidaTerminada` (regla 14; Timer, #12, regla 8). Online, uno que llega dentro de la
  gracia sí se aplica.
- **Si el reloj se agota con el diálogo de Rendirse abierto:** vale el primero que aplique
  la autoridad. Con el diálogo abierto y sin confirmar, el reloj (`RelojAgotado`), y el
  diálogo se cierra; un `Rendirse` ya aplicado antes del 0 termina con Rendición.
- **Si el rival envía `Rendirse` con una etapa del jugador del turno armada o pendiente y
  con hora en o después del vencimiento efectivo del reloj del jugador del turno, o con ese reloj ya en 0 y la
  etapa pendiente:** la autoridad envía antes `CerrarPartida(RelojAgotado, …)` (invariante
  de la regla 14) y el `Rendirse` recibe `PartidaTerminada`: el motivo es `RelojAgotado`.
  Online, un `Rendirse` del rival con hora dentro de la gracia ([vencimiento,
  vencimiento + g), reloj en 0 y quieto, la **misma** etapa que estaba armada al llegar a 0,
  es decir el mismo par (`numeroTurno`, etapa))
  se aplica como cualquier comando dentro de la gracia, como Rendición: PROVISIONAL #28. Si dentro de la gracia se armó una etapa nueva, su vencimiento es
  el instante del armado (regla 14) y el `Rendirse` posterior da `RelojAgotado`.
- **Si cualquiera envía `Rendirse` en `EsperandoTirada` con el reloj del jugador del turno
  ya en 0** (en local, anomalía del Timer, regla 9; online, tras un cierre aceptado dentro de
  la gracia): se aplica como Rendición, porque ahí no corre ningún reloj y `RelojAgotado` da
  `CierreInvalido` (Librería, regla 3; decisión del jugador, 5.ª pasada). Sin `Rendirse`, el
  reloj vence al armarse la etapa después de la `Tirada` (regla 14). **Si esa `Tirada` salta
  todas las fases** (Distracción con `Atacar` sin acción, o las dos fases saltadas), no se
  arma ninguna etapa, así que no vence: el turno se cierra, el reloj sigue en 0 y vence al
  armarse la próxima etapa de ese jugador, en su turno siguiente.
- **Durante `EsperandoTirada` y la espera de la placa:** ningún reloj corre (hasta 3,2 s por
  turno en local; 3,5 s con Distracción). La placa no bloquea: el jugador puede mirar o
  actuar, y su primer comando de fase aceptado arma la etapa y el reloj en ese instante
  (regla 14; Timer, regla 17). Con la etapa pendiente, `Rendirse` sigue disponible y no la
  arma, **salvo con el reloj del jugador del turno ya en 0** (en local, anomalía del Timer,
  regla 9; online con g > 0, un cierre de turno aceptado dentro de la gracia):
  ahí cualquier comando de jugador, `Rendirse` de cualquiera de los dos incluido, arma la
  etapa a su hora, sale primero `RelojAgotado` y gana el rival del jugador del turno; el
  `Rendirse` recibe `PartidaTerminada` (regla 14; decisión del jugador, 3.ª pasada).
- **Si un toque cae sobre la placa mientras tapa el tablero:** no llega al tablero (Entrada,
  #13; Interfaz del Modificador, #17); no hay comando ni se arma la etapa.
- **Si `RelojAgotado` llega con el turno ya cerrado o en `EsperandoTirada`:**
  `CierreInvalido`; la partida sigue.
- **Si `Rendirse` llega en cualquier fase, también esperando tirada:** se acepta si la partida
  sigue en curso (en cualquier fase), termina la partida y
  gana el otro jugador, **salvo** con una etapa del jugador del turno armada o pendiente y la
  hora del `Rendirse` en o después del vencimiento efectivo del reloj del jugador del turno (o ese reloj ya en 0
  con la etapa pendiente): ahí sale antes `RelojAgotado` y gana el rival del jugador del
  turno (entradas de arriba; regla 14).
- **Si los dos jugadores solo dejan correr el reloj:** con timers de etapa iguales y **con
  tiradas iguales**, agota primero el que jugó más turnos: azul, que empieza (el Timer lo
  confirma: con uso máximo simétrico pierde siempre azul). Con tiradas es solo probable: cada
  Distracción le ahorra al que la saca la etapa de mover (su duración según el modo y el
  tramo: tabla de etapas del Timer, regla 3), y cada `Atacar` saltado, la de atacar. El
  arreglo preferido, si las mediciones lo confirman, está en Open Questions (DW-3). En Sin
  límite la partida no termina sola: `C_MAX` la corta en Tablas **solo si hay comandos**;
  con jugadores pasivos el contador no sube y no termina (Open Questions).

### Límites del contrato

- **Un comando genera hasta 6 eventos de la Máquina.** Por lugar (regla 16): la apertura
  da como mucho uno por fase (`ReclutaPerdido` o `FaseIniciada(Reclutar)`; `FaseIniciada` o
  `FaseSaltada` de `Mover`; ídem de `Atacar`), y una `FaseIniciada` detiene la cascada; el
  cierre da 2 (`TurnoTerminado` y `TurnoIniciado`, o `TurnoTerminado` y `PartidaTerminada`);
  y las Tablas automáticas, 1. Peor caso: `Tirada` con reclutar perdido, `Mover` y `Atacar`
  saltadas y el turno que cierra en C_MAX − 1: `ReclutaPerdido`, dos `FaseSaltada`,
  `TurnoTerminado`, `TurnoIniciado` y `PartidaTerminada(Tablas)`; solo se arma con el
  constructor. Jugando, sin reclutar perdido (inalcanzable, regla 6), son 5. A eso se suman los de otros sistemas: el evento de
  la tirada (#11), `Reclutado` (#10), los de Resolución (#9) y los
  `CoberturaQuitada(Caducada)` del jugador que entra (#8, hasta `coberturas_max`).
- **Si `contadorComandos` llega a `C_MAX − 1`:** el Aceptado que lo deja ahí termina la
  partida en Tablas en ese mismo Aplicar (Librería, regla 16); ningún estado en curso queda
  en `C_MAX − 1`. La fila 15 (`LimitePartida`) queda solo como defensa.
- **Si la pila llega a 256 entradas:** es alcanzable con un Catálogo en el límite de F8
  (Catálogo, AC-34). Lo inalcanzable es un `Mover` **aceptado** con la pila ya en 256: con los
  dos invariantes de D4, en ese punto toda pieza gastó sus puntos, y la fila 14
  (`SinMovimientos` o `PiezaAgotada`) lo rechaza antes que la fila 15 (`LimitePartida`, que
  queda como defensa).
- **Si el recluta automático llevara el próximo ID por encima de C_MAX:** `LimitePartida`
  (Librería, fila 15; defensa). El Timer responde con `CerrarPartida(Tablas)` (#12, regla 9).
- **Si una `Tirada` devolviera un resultado tipado inexistente:** inalcanzable, porque
  PrepararContexto valida los papeles de las caras al crear la partida.

*Sin resolución, pasa a Open Questions:* que ninguno de los dos jugadores pueda actuar
durante varios turnos seguidos (casi inalcanzable; en Sin límite solo lo corta `C_MAX`).

## Dependencies

**Depende de:**

| Sistema | Tipo | Qué usa |
|---|---|---|
| #1 Librería de reglas | Dura | El contrato: `Aplicar`, comandos, códigos, enteros, orden por ID y forma canónica. Las fases se escriben *dentro* de él. |
| #2 Catálogo de piezas | Dura | `movimiento` y `topeVivas` por tipo para el invariante de la pila (D4, F8: sin él, `PrepararContexto` rechaza el Catálogo), y el orden de tipos para el recluta automático. |
| #4 Tablero y despliegue | Blanda | Quién empieza (jugador 0, sin compensación) y el estado ya desplegado al crear la partida. |
| #5 Castillo y muros | Blanda (solo un dato) | `vida_tramo`, para la cota de `vidaQuitada` (regla 15). |
| #12 Timer | Blanda | `tope_partida_s` (`T_tope`) y la regla 17 (cuándo se arma la etapa tras la placa), que la regla 14 usa; la Máquina no lee el reloj: el Timer es quien envía `TurnTimeout` y `CerrarPartida(RelojAgotado)`. |

Salvo esos dos datos (#5 y #12), la Máquina **no depende por nombre** de Modificador, Reclutamiento, Movimiento, Alcance,
Cobertura, Castillo ni Resolución: los llama por ganchos tipados (índice, *Circular
Dependencies*). Por eso esos sistemas dependen de ella y no al revés. **Interfaces que
consume** (las definen los dueños; la Máquina solo las llama o lee):

| Interfaz | Dueño | Dónde la usa la Máquina |
|---|---|---|
| `tipo_disponible` | Reclutamiento (#10) | Reglas 6, 10 y 12 |
| `hayMovimientoLegal` | Movimiento (#6) | Regla 10 |
| `ObjetivosValidos` | Alcance y asignación (#7) | Regla 10 |
| `AliadosCubribles` | Cobertura (#8) | Regla 10 |
| Resultado tipado y bonos del Grito | Modificador (#11) | Regla 5 |
| Resolución de los ataques y `vidaQuitada` | Resolución (#9) | Reglas 9, 11 y 15 |
| `limite_normal` | Movimiento (#6, config) | Regla 5 (`LimiteDelTurno`), D4 |
| `bono_grito_movimiento` | Modificador (#11, config) | D4 |
| `vida_tramo` | Castillo y muros (#5) | Regla 15 (cota) |
| `tope_partida_s` / `reloj_jugador_s` y la regla 17 | Timer (#12) | Regla 14, D1, D2 |
| `t_total_max` (duración de la placa) | Interfaz del Modificador (#17, F1) | Regla 14 (vía Timer, regla 17) |

**Dependen de este sistema:**

| Sistema | Tipo | Qué usa | Estado del GDD |
|---|---|---|---|
| #6 Movimiento | Dura | Fase `Mover`, pila de deshacer, límite del turno | Existe |
| #7 Alcance y asignación | Dura | Fase `Atacar` y el criterio "acción posible" | Existe |
| #8 Cobertura | Blanda | Momento de borrar las coberturas (al abrir el turno de su dueño) y `AliadosCubribles` para el salto de fase | Existe |
| #5 Castillo y muros | Blanda | Momento del corte de reclutamiento (reclutar es de la fase de mover; la tabla se calcula al abrir el turno), `Deshacer` sobre los `Mover` de subir y bajar del muro y `vidaQuitada` | Existe |
| #9 Resolución | Dura | Transición `Resolver`; escribe `vidaQuitada`; declara `Δturnos` | Existe (escribe `vidaQuitada` con su cota; `Δturnos` todavía no lo declara: queda para Métricas, #19) |
| #10 Reclutamiento | Dura | Fase `Reclutar`, recluta automático; declara `Δturnos` (su F6, sin valor medido) | Existe |
| #11 Modificador | Dura | Resultados tipados, fase de tirada y bonos que se borran al cerrar; declara `Δturnos` de Distracción (F4, sin medir) | Existe |
| #12 Timer | Dura | `numeroTurno`, etapa, `reloj_jugador_s` y el invariante del reloj (regla 14); define el arranque tras la placa (su regla 17); emite `TurnTimeout` y `CerrarPartida(RelojAgotado)` | Existe |
| #13 Entrada | Blanda | Fase, jugador del turno y pila: los botones de avanzar y Deshacer; los toques sobre la placa no pasan al tablero; sin autorrepetición en Avanzar | Existe |
| #14 Localización | Blanda | Nombres de fases y motivos de fin | Existe |
| #15 Ficha y HUD | Dura | Fase, etapa, cuántos movimientos se pueden deshacer y los eventos de la regla 16 (sin fase no hay botón) | Existe (Approved) |
| #16 Vista previa | Dura | Fase `Atacar`, jugador del turno y evento de fin | Existe (Approved) |
| #17 Interfaz del Modificador | Blanda | `EsperandoTirada`, `TurnoIniciado`, `FaseIniciada`, `ReclutaPerdido`; la placa que no bloquea | Existe (Designed) |
| #18 Render | Dura | Eventos de turno y fase en el orden de la regla 16, `PartidaTerminada`, `MovimientoDeshecho` | Existe (Approved) |
| #19 Métricas | Dura | Eventos de fase, `TurnoTerminado`, `PartidaTerminada` (con sus campos, para cerrar un turno a medias), `MovimientoDeshecho`, `vidaQuitada`, `T_tope`; D1 | Existe (tanda 4) |
| #21 Menú principal | Blanda | Inicia la partida y recibe `PartidaTerminada` | Pendiente |
| #25 Rival IA | Dura | Mismas fases y comandos que un jugador | Pendiente |
| #28 Servidor | Dura | Autoridad de `Tirada`, `TurnTimeout` y `CerrarPartida` | Pendiente |

**Bidireccionalidad.** La Librería, el Catálogo (desde el 2026-11-25) y el Tablero ya nombran a
la Máquina, y el índice tiene sus aristas. #15, #16, #18 y #19 la declaran Dura, y esta
tabla ahora coincide (cierra la parte de #3 de W-21, 2026-11-27). El índice quedó alineado al
cerrar la tanda B (2026-11-28): filas 16, 18 y 19 con #3 Dura, y la fila 3 con el #2 Dura.

Los GDD pendientes tienen que nombrar a la Máquina en su Dependencies al escribirse
(marcado para `/design-review` y `/review-all-gdds`).

## Tuning Knobs

**Perillas propias de la Máquina** (viven en la configuración de la autoridad y de las
Métricas, nunca en el código):

| Perilla | Valor | Rango seguro | Si es muy alta | Si es muy baja | Interactúa con | Dónde vive |
|---|---|---|---|---|---|---|
| `T_tope` (tope de partida; en el registro y en el Timer, `tope_partida_s`) | 1.500 s | 1.290–1.500 (el del Timer; un impar pierde 1 s, D2) | Más de 1.500 contradice el pilar 4: hay que revisar el pilar, no solo el número | Menos de 1.442 deja `reloj_jugador_s` < 721 y la guarda 1 del Timer (`turno_max_s × 8 < reloj_jugador_s`) rechaza los modos de 90 s: solo quedan Fijo 60 y Sin límite, y online (solo Variable) ningún modo: online el rango útil es 1.442–1.500 (D2; decisión del jugador, 4.ª pasada) | `reloj_jugador_s` es su mitad. También acota `turnos_presupuesto` | Config de la autoridad (Timer #12 / Servidor #28) |
| Meta de duración | 900–1.200 s | mínimo 600 s; máximo `T_tope` | Una meta pegada al tope hace frecuentes los relojes agotados | Una meta por debajo de 900 s deja las métricas siempre en rojo | `T_tope` (su techo) y `seg_medio` | Config de Métricas (#19) |

**Perillas de otros sistemas que cambian esta GDD** (no se duplican acá, se apuntan):

| Valor | Dueño | Qué cambia aquí |
|---|---|---|
| Etapas del timer (60 + 30, 40 + 20) y minuto de cambio | Timer (#12) | `turno_max_s` (dueño: Timer, D1; 45–93 por su guarda 1): cuántos turnos completos caben en `reloj_jugador_s` (8 de 90 s, 12 de 60 s) |
| Orden de ajuste si AC-43 falla (más del 20% de las partidas terminan por `RelojAgotado`) | Máquina, Timer (#12), Cobertura (#8) | Se tocan en este orden: (1) las etapas del Timer; (2) DW-4 (etapa de atacar corta con solo coberturas); (3) `T_tope`, solo hacia arriba hasta 1.500 (con el valor vigente, 1.500, este paso no se puede aplicar: queda revisar el pilar 4) |
| `movimiento` y `topeVivas` por tipo | Catálogo (#2) | `pila_max` (D4): hoy 254 de 256; subir el movimiento del soldado en 1 lo rompe |
| `bono_grito_movimiento` | Modificador (#11) | `cota_grito` (D4): hoy 104 con `limite_normal` 12 |
| Duración de la placa de la cara (`t_total_max`: 1,2 s; 1,5 s con Distracción) | Interfaz del Modificador (#17, F1) | Cuánto puede esperar como mucho la primera etapa del turno si el jugador no actúa antes (regla 14; Timer, regla 17); es tiempo de pared fuera de los relojes |
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
| `TurnoIniciado` | Ni el banner del turno ni el contorno cambian con este evento, que sale al cerrar el turno anterior, durante la cosecha: el banner (donde lo haya) aparece en el mismo instante que el contorno, que pasa al bando de turno en el instante que fijan las *Restricciones de animación* de abajo (el más tardío entre el fin de la presentación del turno cerrado y el cambio de `jugadorActivo` en la vista del Timer). **En el turno 1 no hay `TurnoIniciado`** (regla 3): el contorno y el banner salen del estado inicial (jugador 0, turno 1) | Definido (§7 *Pantalla y HUD*, §2 *Turno del rival*) |
| `FaseIniciada` (Reclutar, Mover, Atacar) | El botón de avanzar cambia de texto y el ambiente pasa a *Planificar* o *Asignar ataques* | Definido (§2 *Estados*, §7) |
| Distracción (`SaltearMovimiento`) | Placa gris "Distracción" sobre el tablero ≤ 1,0 s; estandarte y ambiente hasta 1,5 s (#17, regla 4). La etapa de atacar arranca a los 1,5 s o con el primer comando aceptado (regla 14) | Definido (§2 *Estados*) |
| `FaseSaltada` (Mover o Atacar) | **Leyenda breve al centro** ("Sin movimientos" o "Sin objetivos"), ≤ 1 s, sin ambiente propio y sin sonido de error: no es una falla, es un turno que se cierra solo. Si cae junto con la placa de la cara, es **un renglón de esa placa** y dura lo que la placa, nunca una segunda placa central | **Falta**: hay que agregarlo a §2 |
| `ReclutaPerdido` | Leyenda breve "Sin reclutas disponibles", ≤ 1 s, en la placa central de leyendas (HUD, #15, regla 19). Como sale con la `Tirada`, cae siempre junto con la placa de la cara: es **un renglón de esa placa**, igual que `FaseSaltada`, nunca una segunda placa central | **Falta** en el art bible; lugar cerrado por #15 |
| Recluta automático (`automatico`) | Aviso transitorio de texto "Elegido por el tiempo · sin mover" en la zona de avisos del HUD, para que el jugador sepa que decidió el timer y que se perdió la fase de mover (pedido a #15, que hoy dice "Elegido por el tiempo"); sin ícono ni marca sobre la pieza ni la ficha (HUD, #15, regla 18; decisión del 2026-11-26) | **Falta** en el art bible; lugar cerrado por #15 |
| `PartidaTerminada` con `ReyMuerto`, Rendición o Abandono | Pantallas espejo de Victoria y Derrota | Definido (§2 *Estados*) |
| `PartidaTerminada` con `DobleRey` | Victoria para el jugador del turno: las dos figuras de Rey caen en el mismo cuadro y después entra la luz de Victoria (Render, #18). El resumen explica la regla en una frase («los dos Reyes cayeron: gana quien atacó») | Definido en #18; falta en el art bible y el texto |
| `PartidaTerminada` con `RelojAgotado` | Victoria o Derrota; el resumen dice a quién se le acabó el reloj, muestra los dos relojes y lista los ataques asignados que no se resolvieron, como pares por tipo («Arquero → Rey») con su conteo | Texto (lo define la Máquina, art bible §7): `fin.RelojAgotado` = «Se acabó el reloj de [bando]» / «[Side] ran out of time» (#14). Falta en el art bible |
| `PartidaTerminada` con `Tablas` | Placa **neutra**, ni Victoria ni Derrota, con la causa en términos del jugador | Texto: `fin.Tablas` = «La partida llegó a su límite» / «The match reached its limit» (#14). **Falta** en el art bible; el dueño de la pantalla está en Open Questions |

**Restricciones de animación:** ningún evento le suma tiempo de reloj al turno (§2 *Reglas de
ritmo*, pilar 4); la placa de la cara corre fuera de los relojes y no bloquea (regla 14). Una
cascada de eventos de la Máquina se presenta en **≤ 1,5 s en total y en una sola placa
central**, también cuando cae junto con la placa de la cara (las leyendas pasan a ser
renglones de esa placa; cómo, lo fijan #15, #17 y #18). En hotseat la leyenda nombra con
texto el bando que perdió la fase, con el nombre de pantalla del bando («Hueso: sin
objetivos»; art bible §4, clave de bando de #15), no solo con el color. El contorno del
tablero pasa al bando siguiente en el **más tardío** de dos instantes: el fin de la
presentación del turno cerrado (la cosecha o su leyenda; estado presentado, #15 regla 1) y
el cambio de `jugadorActivo` en la vista del Timer, que ocurre al aceptarse la `Tirada`
(Timer, regla 13). En local los dos coinciden (la `Tirada` espera a Render, #18 regla 25).
Online, mientras se espera la tirada, el contorno y el emblema del anillo siguen en el
bando que cerró, junto al texto de espera (salvo mientras se presentan la placa y la
leyenda de un turno que se cerró solo: ahí no hay texto de espera; Timer, UI
Requirements). El cue suave de cambio de turno va en ese mismo
instante. **Nunca después de la placa del turno nuevo:** esa placa ya es del bando que
juega.

**Lector de pantalla:** las leyendas de cascada, el aviso de reloj, el recluta automático y
el motivo del fin se anuncian una vez cada uno (alcance de Accesibilidad, #23).

**Audio:** el cambio de turno es un cue suave. `FaseSaltada` y `ReclutaPerdido` no usan
sonidos de alerta. El cue del timer lo definen el Timer (#12) y Audio (#22).

**Principios del art bible que aplican:** *Regla de ambiente* (§2: el ambiente cambia solo
con gradación, viñeta, interfaz y efectos breves) y *Reglas de ritmo* (§2).

📌 **Asset Spec** — Visual/Audio requirements are defined. After the art bible is approved,
run `/asset-spec system:maquina-de-turno` to produce per-asset visual descriptions,
dimensions, and generation prompts from this section.

## Game Feel

- **Respuesta de los comandos:** `Aplicar` de cada comando entra en el presupuesto de 1 ms
  p95 de la Librería (`comando_presupuesto_ms`); la fase siguiente se ve en el primer cuadro
  después de la respuesta (y la transición dura ≤ 0,25 s). Ninguna orden válida se traga:
  la placa de la cara no bloquea (regla 14).
- **Transiciones de fase:** ≤ 0,25 s (art bible, §2 *Reglas de ritmo*).
- **Deshacer:** la pieza vuelve a la casilla de origen **de ese movimiento** en ≤ 0,25 s. El
  contador de piezas movidas baja en el mismo cuadro **solo si la pieza queda sin
  movimientos usados** (`piezasMovidas` se deriva de los usados, regla 5); si le queda algún
  movimiento usado, sigue contando como movida y el contador no cambia.
- **Avanzar de fase sin confirmación** (el botón de avanzar abajo a la derecha): confirmar
  suma tiempo al turno (pilar 4). El riesgo de tocarlo por error se baja con la distancia al
  botón de Deshacer, abajo a la izquierda (art bible, §7), y con un texto que avisa sin
  pedir un toque más: «Terminar sin atacar» cuando algún `ObjetivosValidos` del jugador del
  turno no está vacío y no hay ningún `AsignarAtaque` vigente; las coberturas no cuentan, ni
  como objetivo ni como asignación (si contaran, por DW-4 saldría casi siempre). Pedido a #15
  y al presupuesto de texto de #14 (el botón mide 144 px; EN «End without attacking»).
  Entrada (#13) ignora la autorrepetición de teclas y mandos: sostener Enter
  no cierra dos fases. La decisión final es de Ficha y HUD (#15); los toques por error se
  miden en playtest.
- **Timer sin castigo:** al agotarse la etapa, el cambio de fase tiene que verse como el plan
  que sigue, no como un corte. Observables: sin sonido de error, la marca «Tiempo» dura
  ≤ 1 s y nada de lo ya hecho se pierde (lo movido queda, lo asignado se resuelve). La
  percepción se mide en playtest («Criterios que no se automatizan»).

## UI Requirements

El HUD (#15) lee de la Máquina:

- **Jugador y número del turno**, y el contorno del bando de turno. **Fuente de lo que se
  muestra:** el estado presentado (#15, regla 1) y `jugadorActivo` de la vista del Timer
  (Timer, regla 13), **no** el `jugador` del estado en `EsperandoTirada`, que ya es el nuevo
  mientras se presenta el turno cerrado; el cambio sigue la regla del contorno de
  Visual/Audio (el más tardío de los dos instantes).
- **Fase y etapa:** texto del botón de avanzar ("Atacar" en `Mover`; en `Atacar`, "Terminar
  turno", o «Terminar sin atacar» con la condición de Game Feel; en `Reclutar` el botón está
  desactivado y la fase la dice el encabezado del recuadro de reclutar) y qué etapa del timer
  corre. En `EsperandoTirada` no corre ninguna; durante la espera de la placa la etapa está
  pendiente (`etapaPendiente` del Timer: anillo lleno y quieto, pedido a #15).
  **Durante la cosecha local** el HUD muestra el estado presentado, con el anillo y los
  relojes quietos en los valores del turno que resuelve, y sin un «esperando la tirada» (#15,
  regla 1; cierra W-3 del lado de #3). Ese texto se muestra online, o si la espera sigue
  después del grupo 1 de la cosecha (condición de #15, regla 12), salvo mientras se
  presentan la placa y la leyenda de un turno que se cerró solo (≤ 1,5 s), que se tratan
  como la cosecha (Timer, UI Requirements).
- **Contador de piezas movidas contra el límite** (por ejemplo 5/12); con
  `LimiteMovimientos(sin límite)` no muestra tope.
- **Estado del botón Deshacer:** desactivado con la pila vacía, de modo que
  `NadaQueDeshacer` no salga por la interfaz.
- **Los dos relojes de partida**, siempre visibles salvo en Sin límite, que no tiene reloj (regla 14); en `EsperandoTirada` y en la
  espera de la placa están detenidos y se nota. Cómo se ven y desde cuándo avisan que queda
  poco lo define #15 (umbrales del Timer: 120 s y 10 s). **Aviso de reloj:** se dispara al
  armarse una etapa con `vinculante` = Reloj (Timer, regla 13; regla 14 de este GDD) y dura
  lo que esa etapa. En `Atacar` con ataques asignados dice que hay que resolver antes de que
  se acabe. Clave `hud.aviso_reloj_vinculante`. Lugar y forma: #15 (los relojes de #15 no
  llevan texto y la zona de avisos ya tiene el de la vista previa; propuesta: el botón de
  avanzar, «Terminar turno · reloj 0:20»). AC-30.
- **Recluta automático:** **solo con `vinculante` = Etapa** (Timer, regla 13), el recuadro
  de reclutar marca desde el principio el tipo que elegiría el tiempo (el primero disponible
  de la cara), con una **forma propia** que no se lee como «ya elegido»: un reloj pequeño y
  «Si se acaba el tiempo» en esa tarjeta, con descripción para el lector de pantalla; y
  cuando la vista del Timer marca `avisoEtapa` (el estado, no el evento), el recuadro dice
  que si se acaba se elige solo y no se mueve. **Con `vinculante` = Reloj no hay tipo
  premarcado** (el timeout de la etapa no llega: vence antes el reloj y se pierde sin
  reclutar, regla 14): el recuadro muestra el aviso de reloj desde que se arma la etapa
  hasta su fin, sin depender de `avisoEtapa`. El aviso posterior dice «Elegido por el tiempo
  · sin mover» (textos de #15 y #17). AC-30.
- **Rendirse:** la interfaz lo ofrece cuando `RendirseDisponible(estado, jugador)` es
  verdadero (Librería, regla 10: jugador del turno con sus piezas vivas × 2 ≤ las del rival,
  **contando al Rey** en los dos lados; también en `EsperandoTirada`). El núcleo lo acepta
  siempre, y el Servidor (#28) no debe exigir esa condición.
- **Pantalla de resultado:** motivo del fin; para `RelojAgotado`, los dos relojes y los
  ataques sin resolver (campo `ataquesSinResolver` de `PartidaTerminada`, regla 16; pedido a
  #15, regla 22, que hoy entrega solo motivo y relojes); para `DobleRey`, la frase que explica
  la regla; para Tablas, la placa neutra (Visual/Audio). AC-45.

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
| Orden del recluta automático | `reclutamiento-y-limites.md` | Regla 10 | **Usa:** primer tipo disponible **de la cara** en el orden del Catálogo (Llamada: soldado; Cuerno: catapulta, si están disponibles) |
| La placa no cuesta reloj ni bloquea; la etapa se arma al terminar la placa o con el primer comando aceptado (W-1, decisión del jugador confirmada en la 2.ª pasada) | `timer-de-turno.md` | Regla 17 | **Usa** (la regla es del Timer) |
| Con la etapa pendiente, el comando del jugador arma la etapa a su hora **antes** de evaluar vencimientos; AC del caso | `timer-de-turno.md` | Reglas 8 y 17 | **Pide** (cierre de la tanda B) |
| Rango de `T_tope` 1.290–1.500 y guarda 1 por modo (los modos de 90 s piden ≥ 1.442) | `timer-de-turno.md` | Tuning Knobs (guardas 1 y 5) y D1 | **Usa** (decisión del jugador, 4.ª pasada) |
| La etapa «se arma con ese comando», no «venía corriendo desde que abrió la fase»; el reloj corre desde que termina la placa o desde el primer comando; los toques dentro del rectángulo de la placa no pasan al tablero; `ComandoViejo` (no `FaseIncorrecta`) para un `Reclutar` tardío | `interfaz-del-modificador.md` | AC-5, regla 6 (l.86), regla 4, l.414 | **Pide** |
| Los toques sobre la placa no pasan al tablero; sin autorrepetición en Avanzar | `entrada-y-seleccion.md` | Reglas 12 y 15 | **Pide** |
| Justificación «la placa no se cobra del reloj»: citar Timer regla 17 | `modificador.md` | Visual/Audio | **Pide** |
| «Terminar sin atacar» (condición de Game Feel); «Elegido por el tiempo · sin mover»; tipo premarcado con forma propia; aviso de reloj (disparo de la regla 14, lugar propuesto en el botón de avanzar); `ataquesSinResolver` en la pantalla de resultado; prefijo de bando con nombre de pantalla; placa neutra de Tablas; anillo lleno y quieto con `etapaPendiente`; umbral de reloj 120 s | `ficha-de-pieza-y-hud.md` | Reglas 12, 14, 18, 19, 20, 22; F3; l.271 | **Pide** (re-revisión de #15) |
| El límite del turno se deriva con `LimiteDelTurno(estado)` (no lo «guarda» la Máquina) | `movimiento.md` | Regla 10, F4 | **Pide** |
| `ConfigInvalida(PilaGrito)` en la lista de códigos de config | `libreria-de-reglas.md` | Regla 2 | **Pide** |
| Gracia por latencia como corrimiento del vencimiento; tope `listo_max_s` del turno 1; etapa armada con el reloj del servidor | Servidor (#28, sin GDD) | — | **Pide** (Open Questions) |
| Orden de los eventos y emisión de `FaseIniciada`; estado y campos de `PartidaTerminada` por motivo | `metricas-locales-de-partida.md`, `render-animacion-y-feedback-de-combate.md`, `ficha-de-pieza-y-hud.md` | #19 regla 5 (turno 1 sin `TurnoIniciado`; cierre a medias con `PartidaTerminada`; N según D1); #18 cola de eventos | **Pide** que lo citen (cierre de la tanda B) |
| `EsperandoTirada` como respuesta benigna a un `TurnTimeout`, sin `TiempoAgotado`; un `TurnTimeout` con el turno cerrado da `EsperandoTirada`, no `TimeoutObsoleto` | `timer-de-turno.md` | Regla 9 y Edge Cases (l.284) | **Pide** (cierre de la tanda B) |
| Aristas #3 → #2 y #8, #9, #10 → #3; mitigación del riesgo de duración | `systems-index.md` | Tabla de sistemas y High-Risk | **Agrega** |
| "Nunca pasa de 25 minutos" vale en los modos con timer; Sin límite exento | `game-concept.md` | Pilar 4 y Core Identity (Session Length) | **Cambia** el texto |
| Eventos nuevos que necesitan tratamiento visual | `art-bible.md` | §2 *Estados* | **Agrega** |

## Acceptance Criteria

**Tipos de evidencia** (estándar de pruebas del estudio): **Logic** = prueba automática de
unidad, BLOQUEANTE, en `Assets/Tests/EditMode/MaquinaDeTurno/`. **Integration** = prueba de
integración con otro sistema. **Config/Data** = chequeo de humo. **UI** = captura retenida
en `production/qa/evidence/`. **Playtest** = observación con jugadores, ADVISORY. Los
criterios marcados **PROVISIONAL** dependen de un GDD que todavía no existe y se confirman
cuando exista.

**Convenciones.** Los estados que no salen jugando (un reclutar perdido, contadores cerca de
`C_MAX`, tableros abiertos) se arman con el **constructor de pruebas** de la Librería (AC-43
de #1). El **Modificador de prueba** es un doble que implementa solo la interfaz del gancho
(regla 5) y devuelve el resultado tipado que pide el test; el ensamblado de tests de la
Máquina no referencia al Modificador real. **Cara y resultado coinciden:** el test elige `n`
para que caiga en la cara cuyo papel corresponde al resultado que devuelve el doble
(Arremetida para `LimiteMovimientos`, la cara de Llamada o de Cuerno para
`ReclutarObligatorio(cara)` con esa misma cara, Distracción para `SaltearMovimiento`), porque
`LimiteDelTurno` y el recluta automático leen la cara (regla 5). Si el ADR no da un punto de
inyección del doble, estos AC se rehacen con la config real y `n`. Toda afirmación sobre
eventos habla de **los eventos de la Máquina** (regla 16) salvo que diga otra cosa. **Salvo
que el GIVEN diga otra cosa, el jugador que entra no tiene coberturas**, así que al cerrar
un turno no sale ningún `CoberturaQuitada(…, Caducada)` (regla 4.1). El
orden de los eventos es el de la regla 16. Las precondiciones de la matriz (regla 17) valen en todos los
casos: emisor y contadores correctos. **Acción posible:** todo AC cuyo THEN espere
`FaseIniciada(F)` o un estado en F, o cuyo WHEN ocurra en F, incluye en el GIVEN una acción
posible en F (regla 10): una pieza propia con un objetivo válido para `Atacar`, una pieza con
movimiento legal para `Mover`. Todo `Resolver` o timeout de atacar sin ganador declarado deja
vivos los dos Reyes y fuera de todo daño.

### Reglas de la Máquina (Sección C)

- **AC-1 (Logic, regla 1: determinismo)** — **GIVEN** la lista grabada de comandos de una
  partida de prueba de 30 turnos (con Distracción, Arremetida, un recluta automático y un
  timeout de cada etapa), **WHEN** se aplica dos veces desde `CrearPartida` con el mismo
  contexto, **THEN** las dos corridas devuelven la misma secuencia de eventos y la misma
  huella final.
- **AC-2 (Logic, regla 2)** — **GIVEN** un estado en curso en cada una de las cuatro fases,
  **WHEN** se consulta la etapa guardada, **THEN** `Reclutar` y `Mover` dan etapa mover,
  `Atacar` da etapa atacar y `EsperandoTirada` no da ninguna. **Y GIVEN** una partida
  terminada por `Rendirse` en `Mover` y otra por `ReyMuerto` en `Atacar`, **THEN** la etapa
  guardada es mover y atacar (la de la fase conservada) y el predicado Terminada es
  verdadero. **Y GIVEN** una partida terminada por `Rendirse` en `EsperandoTirada`, **THEN** la
  fase conservada es `EsperandoTirada`, no hay etapa guardada y el predicado Terminada es
  verdadero.
- **AC-3 (Logic, regla 3)** — **GIVEN** la config vigente, **WHEN** se llama `CrearPartida`,
  **THEN** no hay eventos; el estado está en `Mover`, etapa mover, jugador 0, `numeroTurno`
  1, `contadorComandos` 0, `contadorTirada` 0, sin tirada pendiente, sin cara, pila vacía,
  bonos 0, `vidaQuitada` 0 y 0, sin motivo, y `LimiteDelTurno` = `limite_normal` (regla 5).
- **AC-4 (Logic, regla 4)** — **GIVEN** el turno 2 (jugador 1) en `Atacar`, dos escuderos
  del jugador 0 que cubren desde el turno 1 (IDs `e1` < `e2`), un escudero del jugador 1 que
  cubre en este turno y ataques asignados que no matan a ningún Rey ni alcanzan a `e1`, `e2`
  ni a sus aliados, **WHEN** `Resolver`
  cierra el turno, **THEN** después de los eventos de Resolución salen `TurnoTerminado`,
  `CoberturaQuitada(e1, Caducada)`, `CoberturaQuitada(e2, Caducada)` y
  `TurnoIniciado(3, 0)`, en ese orden; el estado queda en `EsperandoTirada` del jugador 0, y
  la cobertura del jugador 1 sigue.
- **AC-5 (Logic, regla 5; cruce con #11)** — Con el Modificador de prueba, cada caso por
  separado: **GIVEN** `EsperandoTirada` y alguna pieza con movimiento legal, **WHEN** llega
  la `Tirada` y el doble devuelve `Seguir`, **THEN** `FaseIniciada(Mover)`; con
  `ReclutarObligatorio` y un tipo disponible, `FaseIniciada(Reclutar)`; con
  `SaltearMovimiento`, `FaseIniciada(Atacar)` y ningún evento de `Mover`; con
  `LimiteMovimientos(sin límite)` y al menos `limite_normal` + 1 piezas sin el Rey con
  movimiento legal, `FaseIniciada(Mover)` y, **WHEN** se movieron `limite_normal` piezas
  distintas, **THEN** se acepta el primer `Mover` de una pieza más (sin contar el Rey).
- **AC-6 (Logic, regla 6)** — **GIVEN** la fase `Reclutar` con tipos disponibles, **WHEN**
  llega un `Mover`, **THEN** `FaseIncorrecta`; **WHEN** llega `Reclutar(tipo válido)` con
  alguna pieza con movimiento legal, **THEN** `Reclutado` y `FaseIniciada(Mover)`. **Y GIVEN**
  (constructor y Modificador de prueba) `EsperandoTirada` con una tabla efectiva guardada que
  da probabilidad > 0 a la cara de Llamada al servicio aunque sus tres tipos no estén
  disponibles (estado que la carga rechazaría), `n` que cae en esa cara, el doble que devuelve
  `ReclutarObligatorio(Llamada)` y alguna pieza con movimiento legal, **WHEN** llega la
  `Tirada`, **THEN** `ReclutaPerdido` sin `FaseIniciada(Reclutar)` ni
  `FaseSaltada(Reclutar)`, y el estado queda en `Mover`.
- **AC-7 (Logic, regla 7)** — **GIVEN** la fase `Mover` con todo el límite usado (y acción posible en `Atacar`), **WHEN**
  llega un `Mover` de una pieza sin mover, **THEN** `SinMovimientos` y la fase sigue en
  `Mover`; **WHEN** llega `TerminarMovimiento`, **THEN** `FaseIniciada(Atacar)` y la pila
  queda vacía.
- **AC-8 (Logic, regla 8)** — **GIVEN** (constructor, sin otras piezas en los caminos) A un
  soldado azul (movimiento 4) en a1 = (10, 3) y B un arquero azul en b1 = (10, 11), con
  a2 = (11, 3), a4 = (13, 3) y b2 = (11, 11), y un turno con tres `Mover` (A de a1 a a2, que cuesta
  1 punto; A de a2 a a4, dos pasos que cuestan 2 puntos; B de b1 a b2, 1 punto; A y B sin
  mover antes, así que A queda con 3 usados), **WHEN** se envían tres `Deshacer`, **THEN**
  salen tres `MovimientoDeshecho(pieza, origen, destino, usadosRestaurados)` en orden
  inverso, con estos valores exactos: (B, b1, b2, 0), (A, a2, a4, 1), (A, a1, a2, 0), donde
  `origen` es la casilla a la que vuelve la pieza y `destino` la que deja (después del
  segundo, A tiene 1 usado, no 2: se restaura el valor guardado, no se resta 1); y la casilla y
  los movimientos usados de cada pieza son los del inicio de `Mover` (la huella no, porque
  `contadorComandos` avanzó); un cuarto `Deshacer` da `NadaQueDeshacer`.
- **AC-9 (Logic, regla 8: qué no entra)** — **GIVEN** un turno donde se reclutó y luego se
  movió una pieza, **WHEN** se envía `Deshacer` dos veces, **THEN** la primera deshace el
  movimiento y la segunda da `NadaQueDeshacer` (el recluta sigue). **Y WHEN** en `Atacar` se
  envía `Deshacer`, **THEN** `FaseIncorrecta`.
- **AC-10 (Logic, regla 9)** — **GIVEN** la fase `Atacar` con dos atacantes con objetivo y un
  escudero con aliado, **WHEN** llegan en este orden `AsignarAtaque(A)`, `Cubrir(E, X)`,
  `AsignarAtaque(B)`, `QuitarAtaque(A)`, `QuitarCobertura(E)` y `Cubrir(E, X)`, **THEN** todos
  se aceptan y quedan el ataque de B y la cobertura de E; **WHEN** llega `Resolver` y los dos
  Reyes sobreviven, **THEN** `TurnoTerminado` y el estado queda en `EsperandoTirada` del
  rival.
- **AC-11 (Logic, regla 10: `Atacar` y Distracción)** — **GIVEN** (Modificador de prueba)
  `SaltearMovimiento`, ni catapultas ni demoledores propios y ninguna pieza propia con
  objetivo válido ni escudero con aliado, **WHEN** se aplica la `Tirada`, **THEN** los
  eventos de la Máquina son exactamente `FaseSaltada(Atacar)`, `TurnoTerminado` y
  `TurnoIniciado(T + 1, rival)`, sin `FaseSaltada(Mover)` ni `FaseIniciada(Mover)`, y el estado queda en
  `EsperandoTirada` del rival.
- **AC-12 (Logic, regla 10: `Mover` y las dos)** — **GIVEN** (constructor) un estado donde
  ninguna pieza propia tiene movimiento legal y alguna tiene objetivo, **WHEN** llega la
  `Tirada` y el doble devuelve `Seguir`, **THEN** `FaseSaltada(Mover)` y `FaseIniciada(Atacar)`.
  **Y GIVEN** que tampoco hay acción en `Atacar`, **THEN** `FaseSaltada(Mover)`,
  `FaseSaltada(Atacar)`, `TurnoTerminado` y `TurnoIniciado(T + 1, rival)`, sin
  `PartidaTerminada`.
- **AC-13 (Logic, regla 7: una fase empezada no termina sola)** — **GIVEN** `Mover` con una
  pieza y un solo movimiento legal (y acción posible en `Atacar`), **WHEN** se usa ese movimiento, **THEN** el único evento
  es `PiezaMovida` y la fase sigue en `Mover`; **WHEN** después llega `TerminarMovimiento`,
  **THEN** `FaseIniciada(Atacar)`.
- **AC-14 (Logic, regla 11)** — Cuatro casos: (a) **GIVEN** un turno con Grito de guerra
  (bonos 2), piezas movidas y dos ataques asignados, **WHEN** `Resolver`, **THEN** bonos 0,
  movimientos usados 0, sin asignaciones, sin cara. (b) **GIVEN** un turno con Arremetida y
  piezas movidas, **WHEN** `TurnTimeout(numeroTurno, atacar)`, **THEN** los movimientos
  usados vuelven a 0, no queda la cara de Arremetida (sin cara hasta la `Tirada` siguiente) y, en el turno siguiente con `Seguir` y al menos `limite_normal` + 1
  piezas sin contar el Rey con movimiento legal, el `Mover` de la pieza número
  `limite_normal` + 1 (sin contar el Rey) da
  `SinMovimientos`. (c) **GIVEN** piezas movidas, **WHEN** llega `TerminarMovimiento` y
  `Atacar` empieza sin acción posible, **THEN** movimientos usados 0 y `TurnoTerminado`.
  (d) **GIVEN** (Modificador de prueba) una `Tirada` con Grito de guerra y ni `Mover` ni
  `Atacar` con acción posible, **WHEN** se aplica, **THEN** al cerrar el turno en ese mismo
  Aplicar los bonos quedan en 0.
- **AC-15 (Logic, regla 12: etapa mover en `Reclutar`)** — **GIVEN** `Reclutar` abierto por
  la cara de Llamada al servicio con soldados y arqueros disponibles y alguna pieza propia con
  objetivo, **WHEN** llega `TurnTimeout(numeroTurno, mover)`, **THEN** se reclutan soldados,
  `Reclutado` lleva `automatico`, el evento siguiente es `FaseIniciada(Atacar)` y no sale
  `FaseIniciada(Mover)`. **Y GIVEN** la misma cara con el soldado sin llamadas
  (`SinLlamadas`) y arqueros disponibles, **THEN** se reclutan arqueros. **Y GIVEN** la cara
  de Cuerno de guerra con soldados, catapultas y jinetes disponibles, **THEN** se reclutan
  catapultas (no soldados: el primero disponible **de la cara**); con la catapulta en su tope
  de piezas vivas, **THEN** jinetes.
- **AC-16 (Logic, regla 12: etapa mover en `Mover`)** — **GIVEN** `Mover` con 5 piezas ya
  movidas (y acción posible en `Atacar`), **WHEN** llega `TurnTimeout(numeroTurno, mover)`, **THEN** las 5 piezas quedan
  donde están, no hay `Reclutado`, la pila queda vacía y sale `FaseIniciada(Atacar)`.
- **AC-17 (Logic, regla 12: etapa atacar)** — **GIVEN** `Atacar` con 2 ataques asignados y
  una cobertura (la acción posible en `Atacar` también vale en los otros dos casos de este AC), **WHEN** llega `TurnTimeout(numeroTurno, atacar)` y, sobre una copia,
  `Resolver`, **THEN** las dos listas de eventos y los dos estados finales son idénticos.
  **Y GIVEN** `Atacar` sin ataques asignados, **WHEN** llega el timeout, **THEN**
  `TurnoTerminado` y la apertura del turno siguiente. **Y GIVEN** `Reclutar` y un
  `Reclutar(tipo)` aceptado (estado en `Mover`), **WHEN** llega `TurnTimeout(mismo turno,
  mover)`, **THEN** se acepta, la pila queda vacía y sale `FaseIniciada(Atacar)` (regla 12,
  identidad del timeout).
- **AC-18 (Logic, regla 12: rechazos)** — **GIVEN** `EsperandoTirada`, **WHEN** la autoridad
  envía un `TurnTimeout`, **THEN** `EsperandoTirada`; **WHEN** lo envía un jugador, **THEN**
  `SoloAutoridad`. **Y GIVEN** `TerminarMovimiento` ya aceptado (estado en `Atacar`), **WHEN**
  llega `TurnTimeout(mismo turno, mover)`, **THEN** `TimeoutObsoleto` y la huella no cambia.
- **AC-19 (Logic, regla 13)** — **GIVEN** seis casos en cinco estados (un Rey muerto, probado por separado
  con el del jugador 0 y con el del jugador 1; los dos muertos con
  el turno del jugador 0; los dos muertos con el turno del jugador 1; el Rey propio muerto
  por fuego amigo con el rival vivo; los dos vivos con vida 1), **WHEN** se consulta
  `EvaluateMatchEnd`, **THEN** `ReyMuerto` ganando el dueño del Rey vivo; `DobleRey` ganando
  el jugador 0; `DobleRey` ganando el jugador 1; `ReyMuerto` ganando el rival; sin ganador.
  El fuego amigo que llega a ese estado se prueba de punta a punta en Resolución (#9, AC-37).
- **AC-20 (Integration, regla 14; cruce con #12; con #28, PROVISIONAL)** — Con una fuente de
  tiempo falsa: **GIVEN** modo Variable, turno de azul con 1 s en su reloj y la etapa ya
  corriendo con más de 1 s por delante, **WHEN** pasa 1 s en `Mover` (y, en otra copia, en
  `Reclutar` y en `Atacar`), **THEN** la autoridad envía
  `CerrarPartida(RelojAgotado, rojo, numeroTurno)` y no envía `TurnTimeout`. **GIVEN** el
  reloj de azul igual al resto de la etapa (empate), **WHEN** llegan a 0 juntos, **THEN**
  `RelojAgotado` y no `TurnTimeout` (gana el reloj, Timer regla 8). **GIVEN**
  `EsperandoTirada`, **WHEN** pasan 60 s, **THEN** ningún reloj cambia. **GIVEN** el turno de
  rojo, **WHEN** pasan 10 s, **THEN** el reloj de azul no cambia. **Borde:** **GIVEN** azul con 1 s en su reloj, **WHEN** pasan 999 ms, **THEN** la autoridad todavía no envía nada (el `RelojAgotado` no sale antes del 0, tampoco en el envío por tick). **GIVEN** Sin límite,
  **WHEN** pasan 800 s de turno, **THEN** nunca lo envía.
- **AC-21 (Logic, regla 14)** — **GIVEN** el turno de azul en `Atacar` con un ataque asignado
  que mataría al Rey rojo, **WHEN** llega `CerrarPartida(RelojAgotado, rojo, numeroTurno)`,
  **THEN** `Terminada`, gana rojo, el Rey rojo sigue vivo y el ataque figura entre los no
  resueltos de `PartidaTerminada`.
- **AC-22 (Logic, regla 16)** — **GIVEN** (constructor y Modificador de prueba)
  `ReclutarObligatorio` sin tipo disponible y sin acciones en `Mover` ni `Atacar`, **WHEN** se
  aplica la `Tirada`, **THEN** los eventos de la Máquina son exactamente `ReclutaPerdido`,
  `FaseSaltada(Mover)`, `FaseSaltada(Atacar)`, `TurnoTerminado`, `TurnoIniciado`, en ese
  orden, después del evento de la tirada.
- **AC-23 (Logic, regla 17)** — **GIVEN** 6 columnas: las 4 fases y dos estados `Terminada`
  (uno en fase `Atacar` terminado por `ReyMuerto` tras `Resolver`, otro en fase `Mover`
  terminado por `Rendirse`), y cada comando o variante de las filas de la matriz expandida
  (17: `Tirada`, `Reclutar`, `Mover`, `Deshacer`, `TerminarMovimiento`, `AsignarAtaque`,
  `QuitarAtaque`, `Cubrir`, `QuitarCobertura`, `Resolver`, `TurnTimeout` mover,
  `TurnTimeout` atacar, `Rendirse` del jugador del turno, `Rendirse` del otro jugador,
  `CerrarPartida` Abandono, Tablas y `RelojAgotado`, cada `CerrarPartida` con la forma válida
  de su motivo), con sus precondiciones (en las celdas ✓, además: `Deshacer` con pila no
  vacía, `QuitarAtaque` y `QuitarCobertura` con algo asignado), **WHEN** se prueba cada
  celda, **THEN** el código devuelto es el de la tabla (17 × 6 = 102 celdas; las dos
  columnas `Terminada` dan `PartidaTerminada`). **Y** la fila de emisor incorrecto: cada uno
  de los 13 tipos de comando con el emisor equivocado (`Tirada`, `TurnTimeout` y
  `CerrarPartida` enviados por el jugador 0; los 10 de jugador, `Rendirse` incluido, enviados
  por la autoridad) en las 6 columnas da `SoloAutoridad` o `SoloJugador` en las 4 fases y
  `PartidaTerminada` en las dos `Terminada` (13 × 6 = 78 celdas). El oráculo es la tabla de
  la regla 17 **escrita a mano** en el test; solo los estados y comandos de entrada se
  generan. **Y** la fila de jugador fuera de turno: cada uno de los 9 comandos de jugador
  salvo `Rendirse`, enviado con contador correcto por el jugador que no es el del turno, en
  las 6 columnas da `EsperandoTirada` en `EsperandoTirada`, `NoEsTuTurno` en `Reclutar`,
  `Mover` y `Atacar` (también con un comando de otra fase: la fila 11 va antes que la 13) y
  `PartidaTerminada` en las dos `Terminada` (9 × 6 = 54 celdas). **Y** el orden de
  validación: un comando de jugador (no `Rendirse`, que no lleva contador) con contador viejo
  da `EsperandoTirada` en `EsperandoTirada`, `ComandoViejo` en `Mover` y `PartidaTerminada` en
  las dos `Terminada`; con contador viejo **y** fuera de turno en `Mover`, `ComandoViejo` (no
  `NoEsTuTurno`: la fila 10 va antes que la 11); el contador viejo se prueba también en
  `Reclutar` y en `Atacar` (`ComandoViejo`). En la fila de fuera de turno, los comandos con
  pieza usan una pieza **del emisor**, así que no sale `NoEsTuPieza` (fila 12, después de la 11).
- **AC-24 (Logic, edge case)** — **GIVEN** la partida en cada una de las cuatro fases,
  **WHEN** llega `Rendirse` de cualquiera de los dos jugadores, **THEN** `Terminada`, gana el
  otro jugador y el motivo es `Rendicion`.

### Fórmulas (Sección D)

- **AC-25 (Logic, D1; cruce con #19)** — Prueba unitaria de la herramienta de informe de
  Métricas (#19, en su carpeta de tests). Con `reloj_jugador_s` 750 salvo que se diga otro;
  «turnos» son los que cuentan en D1 (cerrados o cortados por `RelojAgotado`) y los tiempos
  son `consumido_ms` sumados. **GIVEN** una partida
  con azul y rojo con 12 turnos y 690.000 ms cada uno, **WHEN** se calcula el presupuesto,
  **THEN** `k_J` = 13 y `turnos_presupuesto` = 26. **GIVEN** los dos con 12 turnos de 60.000 ms,
  **THEN** 24 (no 25: el turno 25 es el 13.º de azul). **GIVEN** los dos a 75.000 ms
  (cociente exacto), **THEN** 18 (no 20). **GIVEN** (sin redondeo) los dos con 10 turnos y
  685.000 ms, **THEN** 20 (redondeando a 68 s por turno saldría 22). **GIVEN** (turno
  cortado) azul con 8 turnos cerrados y 720.000 ms más un 9.º cortado por `Rendirse` con
  5.000 ms, y rojo
  con 8 turnos y 720.000 ms, **THEN** 16 (si el turno cortado contara, 17). **GIVEN** (corte
  por reloj, que sí cuenta) azul con 8 turnos cerrados y 662.000 ms más un 9.º cortado por
  `RelojAgotado` con 88.000 ms, y rojo con 8 turnos y 720.000 ms, **THEN** azul cuenta 9
  turnos y 750.000 ms, `k_0` = 8 y 16 (si se excluyera, 17). **GIVEN** (corte por
  `CerrarPartida(Abandono)`, que no cuenta) los mismos números que el caso de `Rendirse`, con
  el 9.º de azul cortado por Abandono a los 5.000 ms, **THEN** 16 (si contara, 17). **GIVEN**
  (último turno con ganador, que cuenta) azul con 8 turnos y 720.000 ms más un 9.º cerrado con
  `TurnoTerminado` y `ReyMuerto` a los 20.000 ms, y rojo con 8 turnos y 720.000 ms, **THEN**
  azul cuenta 9 turnos y 740.000 ms, `k_0` = ⌈6.750.000 ÷ 740.000⌉ − 1 = 9 y 17 (si se
  excluyera, 16). **GIVEN** azul
  con 3 turnos y 2.000 ms y rojo con 2 turnos y 1.000 ms, **THEN** `k_J` = 749 (por el piso
  de 1.000 ms) y 1.498. **GIVEN** los dos con 8 turnos de 93.000 ms, **THEN** 16; con `reloj_jugador_s` 721
  y 8 turnos de 90.000 ms, **THEN** 16. **GIVEN** (Tablas, que no cuenta) los mismos números
  que el caso de `Rendirse`, con el 9.º de azul cortado por Tablas automáticas a los 5.000 ms,
  **THEN** 16. **GIVEN** (int64) azul y rojo con N_J = 1.000.001 turnos sin fases (0 ms cada
  uno), **THEN** `k_J` = 749 y 1.498, sin desborde (750.000 × 1.000.001 no entra en int32). **GIVEN** (ritmos distintos) azul con 12 turnos y 720.000 ms,
  rojo con 12 turnos y 360.000 ms, **THEN** 24; **GIVEN** el espejo, **THEN** 25; **GIVEN**
  ritmos cercanos (azul con 10 turnos y 700.000 ms, rojo con 10 y 720.000 ms), **THEN** 20 (no
  21); **GIVEN** azul con 1 turno de 60.000 ms y rojo con N_J = 0, **THEN** `k_1` = 749, no
  hay división por cero y el presupuesto es 24.
  **GIVEN** una partida Sin límite, **THEN** el
  informe dice «sin presupuesto». El cierre del registro trae `duracion_pared_s` y `pasa`
  (Métricas, F1 y F2); `turnos_presupuesto` lo calcula la herramienta de informe.
- **AC-26 (Logic, D2; cruce con #12)** — **GIVEN** `T_tope` = 1.500 y 1.442 en Fijo 90,
  **WHEN** se calcula `reloj_jugador_s`, **THEN** da 750 y 721 y se aceptan; **GIVEN** 1.441
  (impar) y 1.440 en Fijo 90, **THEN** `ConfigTimerInvalida(guarda_1)` (8 × 90 = 720 no es menor que
  720); **GIVEN** 1.441 y 1.290 en Fijo 60, **THEN** se aceptan con 720 y 645 (el impar
  pierde 1 s); **GIVEN** 1.289 o 1.501 en cualquier modo, **THEN**
  `ConfigTimerInvalida(tope_partida_s)`; **GIVEN** Sin límite, **THEN** no hay
  `reloj_jugador_s` y la guarda 1 no se evalúa. Lo corre la suite del Timer
  (`Assets/Tests/EditMode/TimerDeTurno/`), que es quien calcula el reloj: **amplía** su
  AC-19 (el espejo se agregó al cerrar la tanda B, 2026-11-28).
- **AC-27 (Logic, D4, con valores del límite)** — **GIVEN** el Catálogo vigente, **WHEN**
  `PrepararContexto` calcula `pila_max`, **THEN** da 254 y lo acepta. **Y GIVEN** un Catálogo
  de prueba con el Rey en movimiento 8 (suma 256), **THEN** lo acepta; con el Rey en
  movimiento 9 (suma 257) o el soldado en movimiento 5 (suma 266), **THEN**
  `ConfigInvalida(F8)`. Entre las validaciones del Catálogo que hace `PrepararContexto`,
  cada caso viola solo F8 (Catálogo, convenciones de AC); la cota del Grito (D4) no entra
  acá porque la valida `CrearPartida` con la config, después (AC-41). Lo corre la
  suite del Catálogo (#2), que es donde vive F8.
- **AC-28 (Logic, D4: peor caso real)** — **GIVEN** (constructor) un tablero abierto donde
  cada pieza tiene camino libre para todos sus pasos, una Arremetida y todas las piezas del
  Catálogo vivas, **WHEN** se mueve cada pieza paso a paso hasta agotar su movimiento,
  **THEN** la pila llega a 254 entradas y todas se aceptan. **Y GIVEN** (constructor) una
  pila con 255 entradas y una pieza con puntos y un destino legal, **WHEN** llega su `Mover`,
  **THEN** se acepta y la pila queda en 256. **Y GIVEN** (constructor) una
  pila con 256 entradas y una pieza con puntos y un destino legal (sin la defensa, el `Mover`
  se aceptaría), **WHEN** llega ese `Mover`, **THEN** `LimitePartida` (defensa de la fila 15).

### Contratos entre sistemas

- **AC-29 (Revisión de diseño, D1; cruce con #9, #10 y #11)** — **GIVEN** los GDD de
  Resolución, Reclutamiento y Modificador, **WHEN** se revisa su sección de dependencias,
  **THEN** cada uno nombra a la Máquina; Reclutamiento y Modificador declaran su `Δturnos` y
  el de Resolución es 0 por definición (Métricas, F4). Lo comprueba `/design-review`, no un
  test.
- **AC-30 (UI, regla 14; cruce con #15)** — Con la fuente de tiempo falsa y los valores
  leídos de la vista del Timer (la captura es la evidencia): **GIVEN** un modo con timer y la
  etapa corriendo (fuera de la espera de la placa y de `EsperandoTirada`), **WHEN** la fuente
  avanza 2.000 ms entre dos capturas del HUD, en el turno de cada jugador, **THEN** se ven
  los dos relojes y solo cambió el del jugador del turno. **Y GIVEN** la cosecha local
  (grupo 1 de Render), **WHEN** la fuente avanza ≥ 1.000 ms entre dos capturas (con pausa de
  depuración o grabación de cuadros), **THEN** los relojes están quietos con los valores del
  turno que resuelve, y en las dos capturas el subrayado de activo, el contorno y el emblema
  del anillo siguen en el bando que cerró (no siguen al `jugador` del estado, que ya es el
  nuevo; UI Requirements). **Y GIVEN** la espera de la placa (`Tirada` aceptada en t), **WHEN** la
  fuente avanza de t + 100 a t + 1.100 sin comandos (menos que los 1.200 ms de la placa),
  con una captura en cada extremo, **THEN** en las dos el anillo está lleno y quieto con el
  emblema del bando nuevo, los dos relojes muestran los mismos valores, y el subrayado de
  activo y el contorno están en el bando nuevo. **Aviso de reloj** (etapa de mover de
  60 s, `aviso_reloj_s` 120, así que en los dos casos el nivel 1 del reloj ya está
  prendido): **GIVEN** el reloj del jugador en 50 s (`vinculante` = Reloj), **THEN** el
  aviso de reloj (`hud.aviso_reloj_vinculante`) no se ve durante la espera de la placa y se
  ve desde el instante del armado hasta el `RelojAgotado` (captura `-a` en la placa, `-b`
  en el armado + 100 ms, con el reloj ≥ 49 s: nivel 1 prendido y nivel 2 no; y `-c`, sin
  comandos, con el reloj del jugador entre 11 y 15 s, nivel 1 prendido y nivel 2 no: **el
  aviso de reloj sigue visible**, no es un aviso transitorio que se apaga solo); en una etapa
  de mover abierta en `Reclutar` con el mismo reloj, sin tipo premarcado (UI Requirements).
  **GIVEN** el
  reloj en 100 s (`vinculante` = Etapa; caso negativo), **THEN** el aviso de reloj no
  aparece (captura `-a` al armar, `-b` con el restante de la etapa por debajo de
  `aviso_etapa_s`) y, en `Reclutar`, el tipo premarcado sí. **Y GIVEN** el mismo caso Reloj con ataques asignados en `Atacar`,
  **THEN** el aviso dice que hay que resolver antes de que se acabe el reloj. **Y GIVEN** el reloj
  del jugador en sus últimos 10 s (nivel 2), sin comandos, **THEN** el aviso sigue visible y
  no hay texto de espera. **Y GIVEN** `Reclutar` con la etapa de mover ya armada y un
  `Reclutar(tipo)` aceptado, **THEN** al pasar a `Mover` la etapa sigue siendo la misma (no se
  rearma) y el aviso se mantiene. **Y GIVEN** Sin límite, **THEN** no aparecen relojes y se ve la
  marca de Sin límite en el lugar del anillo (#15). Evidencia:
  `production/qa/evidence/hud-relojes-<caso>-a.png`, `-b.png` y, en el caso Reloj,
  `-c.png` (tres en el caso Reloj; dos en los demás).
- **AC-31 (UI, Rendirse; cruce con #15, AC-20 de #15)** — **GIVEN** el jugador del turno con
  8 piezas vivas contra 16 (contando al Rey en los dos lados), **THEN** la interfaz ofrece
  Rendirse; también durante la espera de la placa y en `EsperandoTirada` (probado con una
  autoridad de prueba que retiene la `Tirada`, porque en local esa espera coincide con la
  cosecha); con 9 contra 16, no; con 8 contra 15, no (sin contar al Rey serían 7 contra 14 y se
  ofrecería: este caso detecta olvidar al Rey en los dos lados); con el turno del otro
  jugador, no. La condición se evalúa para `estado.jugador` (en `EsperandoTirada`, el
  jugador que entra). Evidencia: capturas `rendirse-<caso>.png`
  retenidas en `production/qa/evidence/`.

### Eventos, cierres y carreras (agregados en la revisión del 2026-11-27)

- **AC-32 (Logic, regla 16: campos de `PartidaTerminada`)** — **GIVEN** `Atacar` del jugador
  0 en el turno 7 con los ataques de A sobre la casilla c1 y de B sobre c2 (IDs A < B,
  asignados en el orden B y después A) y un escudero E del jugador 0 que cubre a un aliado, **WHEN** llega `CerrarPartida(Abandono, 1, vacío)`,
  **THEN** el único evento es `PartidaTerminada(1, Abandono, 7, 0, Atacar, [(A, c1), (B, c2)])`
  (ordenados por ID, no por orden de asignación), sin `TurnoTerminado`, y el estado
  conserva las dos asignaciones y la cobertura de E, sin `CoberturaQuitada` (regla 16, *Estado al terminar*). **Y GIVEN** `Mover` del
  jugador 0 en el turno 7, **WHEN** el jugador 0 envía `Rendirse`, **THEN**
  `PartidaTerminada(1, Rendicion, 7, 0, Mover, [])`.
- **AC-33 (Logic, regla 16; Librería, regla 8)** — **GIVEN** un caso aceptado de cada uno de
  los 13 tipos de comando, con sus variantes como casos aparte (`TurnTimeout` de mover en
  `Reclutar`, de mover en `Mover` y de atacar; `CerrarPartida` con `Abandono`, `RelojAgotado` y
  `Tablas`), **WHEN** se aplica, **THEN** devuelve al menos un evento.
- **AC-34 (Logic, regla 16: Tablas automáticas)** — **GIVEN** (constructor)
  `contadorComandos` = C_MAX − 2 en `Mover`, **WHEN** llega un `Mover` válido, **THEN**
  `PiezaMovida` y, al final, `PartidaTerminada(vacío, Tablas, T, jugador, Mover, [])`; el
  estado queda Terminada. **Y GIVEN** C_MAX − 2 en `Atacar` con un ataque que mata al Rey
  rival, **WHEN** `Resolver`, **THEN** un solo `PartidaTerminada`, con `ReyMuerto`. **Y GIVEN**
  C_MAX − 2 en `Mover` del turno T, ninguna pieza propia con objetivo válido, ni catapultas,
  demoledores ni escuderos propios (no hay acción posible en `Atacar`, regla 10) y el rival
  sin coberturas (no sale `CoberturaQuitada`), **WHEN** llega
  `TerminarMovimiento`, **THEN** `FaseSaltada(Atacar)`, `TurnoTerminado`,
  `TurnoIniciado(T + 1, rival)` y, al final, `PartidaTerminada(vacío, Tablas, T + 1, rival,
  EsperandoTirada, [])`. **Y GIVEN** C_MAX − 2 en `Atacar` con el ataque de A sobre c1
  asignado, **WHEN** llega `AsignarAtaque(B, c2)`, **THEN**
  `PartidaTerminada(…, Atacar, [(A, c1), (B, c2)])`.
- **AC-35 (Logic, regla 17: variantes de `CerrarPartida`)** — **GIVEN** cada una de las
  cuatro fases, **WHEN** llega `CerrarPartida(Abandono, 0, vacío)`,
  `CerrarPartida(Abandono, 1, vacío)` o `CerrarPartida(Tablas, vacío, vacío)`, **THEN** se
  acepta. **Y WHEN** llega `CerrarPartida(RelojAgotado, …)` en `EsperandoTirada`, con
  `turno` ≠ `numeroTurno` o con el jugador del turno como ganador, **THEN** `CierreInvalido` y
  la huella no cambia. Los demás `CierreInvalido`, que son chequeos de forma y se prueban en una fase abierta (Abandono sin ganador, Tablas con ganador,
  `turno` no vacío fuera de `RelojAgotado`, motivo desconocido) los prueba la Librería (AC del
  cierre de `CerrarPartida`, #1).
- **AC-36 (Integration con #9, regla 15)** — **GIVEN** un golpe de 30 de daño neto sobre una
  pieza rival de 5 de vida, **WHEN** `Resolver`, **THEN** `vidaQuitada` del atacante suma 5,
  no 30; sobre una pieza propia, suma 0; sobre un tramo de muro rival con 30 de vida y 40 de
  daño neto, suma 30; sobre un escudero rival que cubre y recibe 7 netos, suma 7. (Mientras
  #9 no esté implementado, con estados del constructor.)
- **AC-37 (Logic, reglas 11 y 16: fin con `Resolver` y partida terminada)** — **GIVEN**
  `Atacar` del turno T abierto por Grito de guerra (bonos 2), con un ataque que mata al Rey
  rival y el Rey propio fuera de todo daño, y el rival (el jugador que entraría) con un
  escudero que cubre desde su turno a un aliado, los dos fuera de todo daño (excepción a la
  convención de coberturas), **WHEN** `Resolver`, **THEN** después de los
  eventos de Resolución salen `TurnoTerminado` y
  `PartidaTerminada(jugador del turno, ReyMuerto, T, jugador del turno, Atacar, [])`, sin
  `CoberturaQuitada(…, Caducada)` ni `TurnoIniciado`; el estado queda en fase `Atacar` y
  etapa atacar, sin asignaciones, con los movimientos usados en 0 y con la tabla, la cara de Grito y los bonos 2 del turno
  (regla 11; regla 16, *Estado al terminar*), y la cobertura del rival sigue en el estado
  (con ganador no se abre el turno siguiente: «`PartidaTerminada` y nada más», regla 16).
  **Y WHEN** después llega un comando de cada tipo, **THEN** todos dan `PartidaTerminada` y la huella no cambia.
- **AC-38 (Logic, regla 8: la pila no cruza turnos)** — **GIVEN** un turno del jugador 0 con
  dos `Mover` y `TerminarMovimiento`, **WHEN** en su turno siguiente, ya en `Mover`, envía
  `Deshacer`, **THEN** `NadaQueDeshacer`.
- **AC-39 (Logic, Edge Cases: carreras)** — **GIVEN** `Atacar`, **WHEN** se acepta `Resolver`
  sin ganador y después llega `TurnTimeout(mismo turno, atacar)`, **THEN** `EsperandoTirada`
  y la huella no cambia. **GIVEN** `Atacar`, **WHEN** se acepta `TurnTimeout(numeroTurno,
  atacar)` sin ganador y después llega `Resolver` con el contador anterior, **THEN**
  `EsperandoTirada` (no `ComandoViejo`: la fila 5 va antes que la 10) y la huella no cambia.
  **GIVEN** `Mover` con alguna pieza propia con objetivo válido, **WHEN** se acepta
  `TurnTimeout(numeroTurno, mover)` y después llega `TerminarMovimiento` con el contador
  anterior, **THEN** `ComandoViejo`. **GIVEN** `Mover` del turno T + 1, **WHEN** llega
  `TurnTimeout(T, mover)`, **THEN** `TimeoutObsoleto`. **GIVEN** una partida terminada por
  `Rendirse`, **WHEN** llega un `TurnTimeout`, **THEN** `PartidaTerminada`.
- **AC-40 (Integration con #12 y #17, regla 14: la placa)** — Con una fuente de tiempo
  falsa y la `Tirada` aceptada en t: (a) **GIVEN** cara Grito de guerra y ningún comando,
  **THEN** en t + 1.199 ms `etapaPendiente` es verdadero y el reloj del jugador es el de t; en
  t + 1.200 la etapa de mover se arma con su duración completa, y en t + 2.200 el reloj bajó
  1.000 ms. (b) **GIVEN** cara Grito y un `Mover` válido aceptado en t + 500, **THEN** la
  etapa se arma en t + 500 con su duración completa y el reloj corre desde ahí. (c) **GIVEN**
  cara Llamada al servicio y `Reclutar(tipo)` aceptado en t + 300, **THEN** la etapa de mover
  se arma en t + 300; sin comando, en t + 1.200. (d) **GIVEN** Distracción y `Atacar` con acción posible, **THEN** sin comandos la etapa
  de atacar se arma en t + 1.500; **y GIVEN** el primer comando de fase aceptado en t + 600
  (un `AsignarAtaque` en una copia, un `Cubrir` en otra), **THEN** se arma en t + 600. (e) **GIVEN**
  «Reducir efectos» (placa de 0,9 s), **THEN** igual que (a): se arma en t + 1.200 (los 0,9 s
  son solo visuales: la espera de la autoridad no cambia). (f)
  **GIVEN** `Rendirse` en t + 300, **THEN** `Terminada` y no se arma ninguna etapa. (g)
  **GIVEN** un toque dentro del rectángulo de la placa en t + 400 mientras tapa el tablero,
  **THEN** no sale ningún comando y la etapa sigue pendiente. (h) Turno 1 (regla 3):
  **GIVEN** una partida recién creada en t0 y la presentación del tablero visible por
  primera vez en t0 + 3.000, sin comandos, **THEN** hasta t0 + 2.999 no hay etapa armada y
  el reloj de azul es 750.000 ms; en t0 + 3.000 la etapa de mover se arma con su duración
  completa. **Y GIVEN** un `Mover` válido de azul aceptado en t0 + 1.000, antes de que el
  tablero sea visible, **THEN** la etapa se arma en t0 + 1.000. (Online, con los dos listos o
  `listo_max_s`: PROVISIONAL #28.) **Y** (UI) en el turno 1, el banner y el contorno de azul se
  ven desde el primer cuadro en que el tablero es visible, leídos del estado inicial (no hay
  `TurnoIniciado`); captura retenida `turno1-banner.png` en `production/qa/evidence/`.
- **AC-41 (Logic, D4: `cota_grito`)** — **GIVEN** el Catálogo vigente y la config con
  `bono_grito_movimiento` 2 y `limite_normal` 37, **WHEN** `CrearPartida`, **THEN** se crea
  (`cota_grito` 254). **GIVEN** `bono_grito_movimiento` 3 y `limite_normal` 37 (`cota_grito`
  9 + 72 + 36 + 175 = 292; F9 pasa: 6 + 3 = 9), **THEN** `ConfigInvalida(PilaGrito)`; con
  `limite_normal` 12 (117), **THEN** se crea. **Y** el borde, con `bono_grito_movimiento` 2 y
  `limite_normal` 37: el Catálogo vigente con el Rey en movimiento 8 (F8: 256; F9: 8 + 2 =
  10; `cota_grito` 10 + 96 + 150 = 256) se crea; con la catapulta en movimiento 1 y el Rey en
  movimiento 9 (F8: 249; F9: 9 + 2 = 11; `cota_grito` 11 + 96 + 150 = 257),
  `ConfigInvalida(PilaGrito)`. Cada caso pasa F8 y F9, así que solo lo decide la cota. **Orden
  de validación** (Librería, regla 2): con `limite_normal` 38 y `bono_grito_movimiento` 3 a la
  vez, el rechazo es el de `limite_normal` (va antes que la pila con bono); con un castillo
  inválido y la cota rota a la vez, `ConfigInvalida(T4)`.
- **AC-42 (Logic, regla 12: defensa del recluta automático)** — **GIVEN** (constructor)
  `Reclutar` pendiente con el próximo ID tal que el recluta automático pasaría de C_MAX,
  **WHEN** llega `TurnTimeout(numeroTurno, mover)`, **THEN** `LimitePartida` y la huella no
  cambia.
- **AC-43 (Playtest, ADVISORY; regla 14 y D1; cruce con #19)** — **GIVEN** las partidas con
  timer del MVP contra un humano (al menos 10), **WHEN** Métricas (#19, regla 14) cuenta los
  motivos de fin, **THEN** como mucho el 20% terminan por `RelojAgotado`. **Y** si hay al
  menos 5 partidas con `RelojAgotado` (sumando grupos de playtest si hace falta), en ≥ 70%
  gana el que iba adelante en piezas vivas (con el Rey) al cierre (un empate en piezas no
  cuenta); con menos de 5 se informa sin veredicto. Métricas informa además el % de
  `RelojAgotado` con `ataquesSinResolver` no vacío. Si falla, el orden de ajuste está en
  Tuning Knobs.
- **AC-44 (Integration con #12, regla 14: invariante del reloj; local, g = 0)** — Con la
  fuente de tiempo falsa: **GIVEN** el reloj de azul vence en t, **WHEN** llega un `Mover`
  válido de azul con hora t − 1, **THEN** se aplica con sus eventos y no sale
  `CerrarPartida` antes. **WHEN** llega con hora t (el 0 exacto) y, en otra copia, t + 1,
  **THEN** la autoridad aplica primero `CerrarPartida(RelojAgotado, rojo, numeroTurno)`, el
  comando recibe `PartidaTerminada` y no sale ningún evento del comando. (Online, el mismo
  borde en vencimiento + g − 1 y vencimiento + g: PROVISIONAL #28.) **GIVEN** la etapa pendiente de la placa y el reloj de azul
  ya en 0 (en local solo se llega así por la anomalía de la regla 9 del Timer, armada con la
  autoridad de prueba; así se arman todos los casos de reloj en 0 de este AC), **WHEN** llega
  un `Mover` válido de azul, **THEN** lo mismo (el comando arma la etapa a su hora antes de
  evaluar). **Y WHEN** en ese estado llega un `Mover` de rojo (fuera de turno), **THEN** también
  sale primero `RelojAgotado` y el `Mover` recibe `PartidaTerminada`, no `NoEsTuTurno`. **Rendirse con el reloj en 0** (gana rojo por
  `RelojAgotado` en los casos (i) a (iii)): (i) **GIVEN** la etapa de azul armada y su reloj que
  vence en t, **WHEN** rojo envía `Rendirse` con hora t − 1, **THEN** se acepta y gana azul por
  Rendición; **WHEN** lo envía con hora t, **THEN** sale primero
  `CerrarPartida(RelojAgotado, rojo, numeroTurno)`, el `Rendirse` recibe `PartidaTerminada`
  y el motivo es `RelojAgotado`, no Rendición (con Rendición habría ganado azul); (ii)
  **GIVEN** la etapa pendiente de la placa y el reloj de azul ya en 0 (anomalía de la regla
  9 del Timer, armada con la autoridad de prueba), **WHEN** rojo envía `Rendirse`, **THEN**
  lo mismo: la etapa se arma a la hora del `Rendirse` y gana rojo por `RelojAgotado`; (iii)
  en ese mismo estado, **WHEN** azul envía `Rendirse`, **THEN** motivo `RelojAgotado` (no
  Rendición) y el `Rendirse` recibe `PartidaTerminada`. (iv) **GIVEN** (autoridad de prueba,
  misma anomalía) `EsperandoTirada` del turno de azul con el reloj de azul en 0, **WHEN** rojo
  envía `Rendirse`, **THEN** no sale `CerrarPartida`, el `Rendirse` se acepta y gana azul por
  Rendición; **y en otra copia, WHEN** es azul quien envía `Rendirse`, **THEN** también se acepta y gana
  rojo por Rendición; **y en otra copia, WHEN** llega la `Tirada` en t₁ y no llega ningún
  comando, **THEN** la etapa se arma en t₁ + 1.200 y en ese instante sale
  `CerrarPartida(RelojAgotado, rojo, numeroTurno)`. (v) **GIVEN** la etapa pendiente y el reloj
  de azul en 0, **WHEN** llega un comando de azul que la Librería rechazaría (por ejemplo, un
  `Mover` a una casilla ocupada), **THEN** igual se arma la etapa y sale primero `RelojAgotado`;
  el comando recibe `PartidaTerminada` (con el reloj en 0, el armado no depende de que el
  comando se acepte; Timer, regla 17). El caso de la regla 9 del Timer con la etapa del mismo
  jugador **en curso** (`TurnTimeout` inmediato) lo prueba el Timer, AC-13.
- **AC-45 (UI, pantalla de resultado; cruce con #15)** — **GIVEN** una partida terminada por
  cada motivo (`ReyMuerto`, `DobleRey`, Rendición, Abandono, `RelojAgotado` con cuatro
  ataques sin resolver —dos arqueros sobre el Rey, un arquero sobre un escudero y un jinete
  sobre un escudero—, Tablas), **THEN** una captura por motivo muestra: Victoria o Derrota
  (neutra en Tablas), el texto del motivo (`fin.RelojAgotado` y `fin.Tablas` con los textos
  de Visual/Audio; la frase de `DobleRey`), y en `RelojAgotado` los dos relojes y los pares
  de `ataquesSinResolver` agrupados por el par (tipo del atacante, tipo del objetivo) con su
  conteo: «Arquero → Rey ×2», «Arquero → Escudero» y «Jinete → Escudero» (agrupar solo por
  atacante o solo por objetivo da otra lista; el formato exacto del conteo es de #15); y en `fin.RelojAgotado` el [bando] es el del
  jugador del turno, **el que perdió**, cuyo reloj se ve en 0:00, no el bando ganador. En hotseat cada captura (salvo Tablas, sin
  ganador; la de Abandono, que en hotseat no existe, se toma con la autoridad de prueba)
  muestra además el **nombre de pantalla del bando ganador** («Hueso» o «Ciruela»),
  no solo el color. Evidencia: `production/qa/evidence/fin-*.png` (6 capturas).
- **AC-46 (Logic, regla 13 y regla 16: `DobleRey` por timeout)** — **GIVEN** `Atacar` del
  turno T con ataques asignados que matan a los dos Reyes, **WHEN** llega
  `TurnTimeout(numeroTurno, atacar)`, **THEN** después de los eventos de Resolución salen
  `TurnoTerminado` y `PartidaTerminada(jugador del turno, DobleRey, T, jugador del turno,
  Atacar, [])`, y el estado conserva fase `Atacar` y etapa atacar, sin asignaciones, con los
  movimientos usados en 0 y con la tabla, la cara y los bonos del turno (regla 16, *Estado al
  terminar*); no se abre el turno siguiente.

**Criterios que no se automatizan** (van a playtest): que la presión del timer se sienta
como "presión buscada" y no como castigo (Player Fantasy): ≥ 70% de los playtesters marca
4–5 en «el timer me apuró de forma justa» (escala 1–5) y < 20% lo nombra como castigo en la
pregunta abierta; que la partida dure 15–20 minutos: mediana de la duración de las partidas
de la compuerta entre 900 y 1.200 s, con la medida (pared o reloj consumido) que fije
Métricas (#19, regla 12; D2); la compuerta en sí: 3 partidas seguidas que terminan en el
tablero —Rey muerto, doble Rey o rendición—, con timer y contra un humano, en 25:00 o menos
según esa misma medida; AC-43.

**Huecos conocidos:** "la Máquina no conoce a ningún sistema por nombre" se prueba con el
Modificador de prueba (AC-5); la política de la regla 14 se prueba en #12 y #28 (AC-20,
AC-40, AC-44); `N_base` no se puede fijar (D1); `ReclutaPerdido` y los contadores cerca de `C_MAX`
solo se prueban con el constructor (AC-6, AC-22, AC-34, AC-42); el estancamiento (nadie
puede actuar) no tiene criterio porque quedó como pregunta abierta.

## Open Questions

| Pregunta | Dueño | Para cuándo |
|---|---|---|
| ¿Los relojes se pausan durante una desconexión (online)? En el MVP no (Timer, #12) | Servidor (#28) | En su GDD |
| ~~¿Cómo se ven los dos relojes?~~ **Resuelta en #15 (regla 12).** Los umbrales de aviso son del Timer (#12): **120 s** y 10 s (2026-11-27); propagado a #15 (F3) y al registro al cerrar la tanda B (2026-11-28). Abierto en #15: dónde va el aviso de reloj vinculante (regla 14) | Ficha y HUD (#15) | Re-revisión de #15 |
| ~~¿Las partidas Sin límite cuentan para la compuerta del arte?~~ **Resuelta 2026-11-26:** no (Métricas, regla 12) | — | — |
| ¿El timeout durante `Reclutar` salta `Mover` y se siente como castigo? **Ahora (2026-11-27):** la interfaz marca el tipo que elegiría el tiempo y avisa que no se mueve (UI Requirements; #15 y #17). **Si el playtest lo confirma como castigo:** una ventana propia para reclutar (`reclutar_max_s`, unos 15 s) y después `Mover` con el resto de la etapa (cambia #12 y la regla 17 de la Librería) | Playtest; Timer (#12) | MVP |
| ¿El reloj decide partidas que el tablero no decidió? Medir cuántas terminan por reloj y quién iba adelante (AC-43). La medida existe (Métricas, regla 14) | Métricas (#19) | MVP |
| **DW-3 y desempate del reloj (decisión del jugador, 2026-11-27: medir primero).** Azul no tira en el turno 1 y rojo sí tira en el turno 2; con pasividad pareja pierde azul (Edge Cases; Tablero, #4, Open Questions). Métricas registra el bando ganador y el motivo desde la primera partida. **Arreglo preferido si se confirma:** nadie tira en su primer turno (rojo tampoco en el turno 2) y el turno 1 de azul no gasta su reloj. Descartado: un desempate por material, que contradice la regla 15 y la fantasía | Máquina y playtest (Métricas #19) | MVP, después de las primeras partidas |
| **DW-4: `Atacar` casi nunca se salta**, porque un escudero con un aliado al lado siempre es acción posible (regla 10; hasta +150 s por partida). Sacar `Cubrir` de "acción posible" no sirve: las coberturas se borran al abrir el turno del dueño y el Rey quedaría sin cubrir. **Recomendado:** un comando de un toque `RepetirCoberturas` (comando nuevo: cambio de contrato) y, si solo hay coberturas posibles, una etapa de atacar corta (unos 10 s). Se decide junto con DW-1 (Rey cubierto) en Cobertura (#8). Descartado: la cobertura permanente, que empeora DW-1 | Cobertura (#8), Máquina y Timer (#12) | Antes del ADR de la Librería |
| ¿El reloj premia a quien tiene menos para hacer? El tiempo por turno crece con las piezas activas, así que un ejército chico o encerrado gasta menos reloj. Medir segundos por pieza movida y por ataque asignado | Métricas (#19) | MVP |
| ¿Alcanza 1 s para leer una leyenda de cascada de hasta 3 renglones? | Playtest; Ficha y HUD (#15) | MVP |
| **Pedidos al Servidor (#28), online:** la `Tirada` nunca espera un mensaje de cliente sin tope; la etapa se arma con la espera fija de la placa o con el primer comando de fase aceptado, medidos con el reloj del servidor (Timer, regla 17); el turno 1 se arma cuando los dos clientes están listos, con un tope `listo_max_s` (si vence, no se crea la partida o `CerrarPartida(Abandono, el que sí confirmó, vacío)`); cuánta gracia por latencia se da (calculada por el servidor, nunca informada por el cliente), siempre como corrimiento fijo del vencimiento que no se cobra (el reloj queda en 0 y quieto durante la gracia) y nunca una hora retroactiva (regla 14); los resultados predichos (`PiezaMuerta`, `PartidaTerminada`) se muestran solo confirmados; qué pasa con Sin límite online (sin reloj, un jugador puede trabar la partida) | Servidor (#28) | En su GDD |
| ¿Quién es dueño de la pantalla de resultado de Tablas (#15 o la Máquina) y cuenta para estadísticas? (Librería, Open Question «Juego y presentación») | Ficha y HUD (#15) y Métricas (#19) | MVP |
| ~~`N_base` y `Δturnos` de #9, #10 y #11~~ **Resuelta 2026-11-26:** `N_base` se estima con Métricas F4 (cota inferior); el `Δturnos` de Resolución es 0 por definición | — | — |
| Que ninguno de los dos jugadores pueda actuar durante varios turnos (casi inalcanzable; en Sin límite solo lo corta `C_MAX`, y solo si hay comandos) | Timer (#12) y Librería (ADR) | Antes de lanzar Sin límite |
| ¿Hace falta una guarda corta (~250 ms) contra toques arrastrados del turno anterior en hotseat? Hoy no: entre turnos hay hasta 2 s de presentación. Si el playtest la pide, es una perilla de Entrada (#13) con valor por defecto 0, no una regla de la Máquina | Playtest; Entrada (#13) | MVP |
