# Entrada y selección

> **Status**: Designed (pending review) — sin `/design-review`
> **Author**: Martin Arin + Claude
> **Last Updated**: 2026-11-25
> **Last Verified**: 2026-11-25
> **Creative Director Review (CD-GDD-ALIGN)**: CONCERNS (accepted) 2026-11-25 — Core Rules 12 (reclutar en un toque) debilitaba el Pilar 1; REVISED 2026-11-25 al cerrar la tanda 2: las casillas de aparición se marcan sin toque extra. Player Fantasy REVISED 2026-11-25 (decía "corregir es un toque más" y la regla 8 lo hace en dos). Lente aplicada en esta sesión, sin agente.
> **Implements Pillar**: Pilar 1 (Planificar y cosechar), Pilar 4 (Duelo corto)

## Summary

Entrada y selección es la capa del cliente que convierte un toque o un clic en la selección de una pieza y en los comandos de jugador de la Librería: mover (tocar la pieza, tocar el destino), atacar (tocar al atacante, tocar el objetivo), deshacer, avanzar de fase, reclutar y rendirse. Con una pieza elegida, un toque en lo legal manda el comando; si no, elige la pieza que hay ahí (también las rivales, para ver su alcance); un toque en una casilla vacía no legal muestra el motivo del rechazo. Existe para que el plan del jugador pase al tablero con el menor número de toques posible (pilares 1 y 4), y deja medido el costo en toques y en tamaño de casilla en el teléfono.

> **Quick reference** — Layer: `Foundation` · Priority: `MVP` · Key deps: `Librería de reglas (#1)`, `Máquina de turno (#3)`, `Movimiento (#6)`, `Alcance y asignación (#7)`, `Reclutamiento (#10)`, `Localización (#14)`, `Tablero (#4)`, `Cobertura del escudero (#8)`, `Modificador (#11)`. Layer y Priority tomados del índice (la categoría `Core` del índice está marcada "inferred").

## Overview

Entrada y selección traduce toques y clics del jugador en los comandos cerrados de la Librería (`Mover`, `Deshacer`, `TerminarMovimiento`, `AsignarAtaque`, `QuitarAtaque`, `Cubrir`, `QuitarCobertura`, `Reclutar`, `Resolver`, `Rendirse`) y usa sus consultas puras (`MovimientosLegales`, `ObjetivosValidos`) para resaltar casillas. No decide reglas: solo propone comandos y muestra los códigos de rechazo. Mismo modelo con mouse y con dedo, sin información que aparezca solo al pasar el mouse (art bible, Controles).

## Decisiones tomadas (2026-11-25, jugador)

- **Mover:** tocar la pieza, tocar el destino. Sin arrastrar. Se resaltan los destinos legales; la pieza sigue seleccionada si le quedan movimientos.
- **Atacar:** tocar al atacante, tocar el objetivo. Tocar de nuevo el mismo objetivo (o la flecha) la quita (`QuitarAtaque`).
- **Deshacer:** solo botón, un paso por pulsación, desactivado con la pila vacía (lo pide la Máquina de turno, UI Requirements). Sin atajo de teclado ni "mantener para varios".
- **Regla de toque: "lo legal gana".** Con una pieza elegida, un toque en una casilla que es destino legal u objetivo válido manda el comando; si no lo es y hay una pieza, la elige (propia o rival, para ver su alcance).
- **Toque en una casilla vacía no legal:** manda el comando igual y se muestra el texto del rechazo; la pieza sigue elegida.
- **Reclutar: un solo paso, con las casillas marcadas** (mouse y dedo). Un toque en el tipo recluta; mientras el recuadro está abierto, el tablero marca numeradas las casillas de aparición, sin hover ni toque extra (decisión del jugador al cerrar la tanda 2: cumple a #10 y al pilar 1).
- **Fantasía de jugador:** "Mi mano sigue a mi cabeza" (Player Fantasy, candidato A).

## Hechos de los GDD vecinos que este sistema debe respetar

- Comandos y códigos: Librería regla 3 y tabla de validación (filas 10–15); `ComandoViejo` obliga a llevar `contadorEsperado`.
- Pila de deshacer hasta 256, cerrada al terminar el movimiento (`FaseIncorrecta`).
- Tablero 24×15, casilla 68 px a 1080p, ~26 pt en móvil (bajo los ~44 pt táctiles): el tamaño táctil se decide en #41/#23.
- Sin cuadrícula por defecto; al elegir pieza se marcan alcanzables y alcance (Tablero, UI Requirements).
- Riesgo del TD: 12–13 piezas en 60–90 s en táctil (índice, riesgos de diseño).

## Player Fantasy

**Mi mano sigue a mi cabeza.** El jugador no siente que "maneja una interfaz": tiene el plan armado y las piezas lo siguen. Mover una pieza son dos toques (la pieza, el destino) y la pieza sigue elegida mientras le queden pasos; atacar es lo mismo (el atacante, el objetivo) y corregir es repetir el gesto. Entre pensar y hacer no hay menús, confirmaciones ni arrastres.

*Momento ancla:* turno de doce piezas con la etapa de mover en 60 s. El jugador ya sabe que quiere abrir una brecha por el flanco: toca el jinete, toca el destino, toca la catapulta, toca el destino. Cada toque responde en el mismo cuadro, y si se equivoca, Deshacer devuelve la pieza sin costo. Al terminar no recuerda haber tocado nada: recuerda el plan.

*Pilares citados:* Pilar 1, «Al mover armás la estrategia; al atacar ejecutás el plan y ves sus frutos»; y Pilar 4, «si una mecánica alarga la partida, pierde».

*Lo que le exige al resto del diseño:* ningún toque de más (cada paso de confirmación se paga del presupuesto de E1 y E2); resaltados que aparecen en el mismo cuadro del toque; y rechazos que explican sin reprender (Reclutamiento, Game Feel: "se explica, no se reprende").

*Lo que no es:* un desafío de destreza. La precisión del dedo nunca es parte del juego. Por eso el tamaño de la casilla en el teléfono (E3) es un riesgo de diseño y no una dificultad buscada.

> Lente `creative-director` aplicada en esta sesión (modo `full`, sin agente). Se presentaron tres candidatos (A: mi mano sigue a mi cabeza; B: probar es gratis; C: veo todo lo que puede pasar) y el jugador eligió A el 2026-11-25. B y C quedan cubiertos por reglas (Deshacer, regla 11; inspección, regla 5), no por la fantasía.

## Detailed Design

### Core Rules

**Vistas por rol (Section C, modo `full`, sin agentes).** *game-designer:* el presupuesto de toques sale del pilar 4 (E1, E2); inspeccionar rivales es gratis (pilar 1). *ux-designer:* cada rechazo visible llega al jugador; nada depende del hover; el gesto de "elegir" y el de "mandar" no pueden confundirse (por eso la regla 6 tiene un orden fijo). *gameplay-programmer:* la regla 6 es una función pura `ResolverToque(estado, selección, casilla) → acción` que solo consulta a la Librería; no hay una segunda copia de las reglas en el cliente y la IA (#25) no pasa por acá.

1. **Qué es.** La capa del cliente que traduce un puntero (mouse o dedo) en selección local y en los comandos de jugador de la Librería (regla 3): `Mover`, `Deshacer`, `TerminarMovimiento`, `AsignarAtaque`, `QuitarAtaque`, `Cubrir`, `QuitarCobertura`, `Reclutar`, `Resolver` y `Rendirse`. No decide reglas, no calcula daño ni alcance, y no escribe en el estado de la partida: la selección, el resaltado y los avisos son del cliente y no entran en la forma canónica ni en la huella. Todo lo que muestra sale de las consultas puras (`MovimientosLegales`, `ObjetivosValidos`, `CasillasEnAlcance`), de los eventos y de los códigos de rechazo.
2. **Un toque.** Mouse y dedo producen el mismo gesto: pulsar y soltar sobre **la misma casilla**. El gesto se resuelve al soltar. Si la casilla al soltar no es la de pulsar, o el puntero salió del tablero, o la ventana perdió el foco, el gesto se descarta sin efecto. No hay arrastre (decisión del jugador). Solo cuenta el primer puntero: un segundo dedo o botón mientras hay uno activo se ignora. Clic derecho, rueda y teclado no hacen nada en el MVP.
3. **Qué casilla.** El tablero (24 × 15, Tablero #4) convierte un punto de pantalla en `(columna, fila)`. Un punto fuera del tablero (el borde de 60 px de arriba, los 144 px de cada lado) no es una casilla: un toque ahí no hace nada, salvo que caiga sobre un control del HUD, que lo atiende. Una pulsación que empieza en un control y termina en el tablero (o al revés) no activa ninguno. Un tramo roto es una casilla vacía; una pieza sobre un tramo cuenta como pieza. **Un tramo en pie sin pieza se puede inspeccionar** (pedido por #15, regla 9; cierre de la tanda 3): sin pieza elegida, o cuando la regla 6 no da un comando, la pulsación lo deja en inspección (sin comandos) y el HUD muestra su tarjeta con la vida; con mouse, pasar sobre él es solo un atajo.
4. **Cuándo hay comandos de tablero.** Los toques sobre el tablero mandan comandos solo si la partida está en curso, la fase es `Mover` o `Atacar` y el jugador del turno es el que controla este cliente (en local, siempre; online, el propio). En cualquier otro estado (`EsperandoTirada`, `Reclutar`, turno rival online, `Terminada`) el tablero es **solo de inspección**: se puede elegir cualquier pieza para ver su alcance, pero ningún toque manda un comando. **Con el panel «?» abierto** (#17) no hay comandos de tablero ni de HUD: los toques los atiende el panel, y al cerrarlo el tablero ignora toques durante `guarda_cierre_panel_ms` (250 ms, perilla de #17) para que un doble toque sobre el velo no mande un comando.
5. **Selección e inspección.** Hay como mucho **una** pieza elegida. Elegir es local: no manda comandos, no pausa el timer y no cambia el estado. Cualquier pieza se puede elegir en cualquier fase y turno, propia o rival (todo es público, Alcance #7, regla 13). Al elegir una pieza se marca, sin depender del hover:
   - su **alcance** (`CasillasEnAlcance`), siempre, de cualquier bando (Alcance, UI Requirements);
   - si es del jugador del turno con comandos habilitados y la fase es `Mover`: sus **destinos** (`MovimientosLegales`, con el costo) y el **restante** de movimiento;
   - si es del jugador del turno con comandos habilitados y la fase es `Atacar`: sus **objetivos válidos** (`ObjetivosValidos`) y, si es un escudero, los aliados que puede cubrir (`AliadosCubribles`, #8), más su flecha si ya tiene un ataque asignado;
   - una pieza rival, o propia sin comandos habilitados, muestra solo el alcance y queda marcada como **en inspección** (forma distinta de la elegida, no solo color).

   Los resaltados se recalculan desde el estado después de cada comando aceptado. La selección se suelta sola cuando la pieza muere, cuando cambia la fase o el turno, con `TurnTimeout`, y al terminar la partida.
6. **Resolución de un toque (decisión del jugador, 2026-11-25: "lo legal gana").** Con comandos de tablero habilitados y una pieza propia **P** elegida, un toque en la casilla **X** se resuelve con la **primera** fila que se cumpla:

   | Orden | Condición | Efecto |
   |---|---|---|
   | 1 | X es destino legal (`Mover`) u objetivo válido de P (`Atacar`; en un demoledor incluye su propia casilla); o X es un aliado que un escudero puede cubrir | Manda el comando (reglas 7, 8, 9). Si X ya es el objetivo asignado de P, manda `QuitarAtaque(P)` en lugar de repetir la asignación; si P es un escudero y X es el aliado que ya cubre, manda `QuitarCobertura(P)` |
   | 2 | X es la casilla de P | Suelta la selección. No manda nada |
   | 3 | X tiene una pieza (propia o rival) | Elige esa pieza (reemplaza la selección). No manda nada |
   | 4 | X está vacía o tiene un tramo, y P no es un demoledor en `Atacar` | Manda igual el comando de la fase (`Mover(P, X)` o `AsignarAtaque(P, X)`) para que la Librería devuelva el código; se muestra su texto (regla 14). La selección sigue; el estado no cambia |
   | 5 | X está vacía o tiene un tramo, y P es un demoledor en `Atacar` | Suelta la selección. No manda nada (el único objetivo válido del demoledor es su casilla; `ObjetivoInvalido` es interno y no debe salir por la interfaz) |

   Sin pieza elegida (o con una en inspección): un toque en una pieza la elige (con comandos habilitados, si es propia queda elegida como comandable); un toque en una casilla vacía suelta la selección en inspección y no hace nada si no había.

   Consecuencias que se aceptan: (a) una pieza propia o rival que no es objetivo válido **se elige** en vez de producir `DestinoOcupado`, `ObjetivoPropio` o `FueraDeAlcance`, así que con la fila 3 esos códigos no salen por esta interfaz para un toque sobre una pieza (siguen existiendo para la IA y el online); (b) una pieza propia **sí** es objetivo válido de un ataque de área (catapulta, Alcance #7, regla 6): con una catapulta elegida, tocar un aliado en alcance le asigna el golpe (fuego amigo, avisado por la vista previa #16), y para elegir ese aliado hay que soltar antes la catapulta (fila 2).
7. **Mover (decisión del jugador: tocar la pieza, tocar el destino).** Fila 1 en `Mover` manda `Mover(P, X)`. El jugador no elige camino (Movimiento, regla 3). Si después del comando aceptado a P le queda movimiento (`MovimientosLegales(P)` no vacío), **P sigue elegida** y sus destinos se recalculan desde la casilla nueva; si no le queda, la selección se suelta. No hay confirmación ni arrastre. El Rey se mueve igual que cualquier pieza.
8. **Atacar (decisión del jugador: tocar al atacante, tocar el objetivo).** Fila 1 en `Atacar` manda `AsignarAtaque(P, X)`; sobre una pieza que ya tenía ataque lo reemplaza en un solo comando (Alcance, regla 9). Tocar de nuevo el mismo objetivo manda `QuitarAtaque(P)`; "la flecha" se toca en su **punta**, que es la casilla objetivo (la línea de la flecha no recibe toques, porque cruza casillas que podrían ser otra cosa). Elegir una pieza que ya tiene un ataque asignado resalta su flecha y no la quita. Tras un comando aceptado en `Atacar` (asignar, reemplazar, quitar, cubrir) **la selección se suelta**: así un toque siguiente sobre un aliado en alcance de una catapulta nunca le asigna un golpe por accidente (regla 6, consecuencia b). Una asignación es un comando suelto: nunca se mandan varias juntas.
9. **Escudero y demoledor.** *Escudero:* tocar un aliado cubrible manda `Cubrir(escudero, aliado)` (fila 1); tocar un enemigo en alcance manda `AsignarAtaque` y borra la cobertura (Alcance, regla 9). *Demoledor:* se elige con un toque y se asigna la explosión tocándolo **otra vez** (su casilla es su único objetivo válido, fila 1); como asignar suelta la selección (regla 8), quitarla cuesta otros dos toques: uno lo vuelve a elegir y el siguiente manda `QuitarAtaque` (fila 1, su casilla ya es su objetivo asignado). *Quitar una cobertura (decisión del jugador, 2026-11-25):* con el escudero elegido, tocar al aliado que ya cubre manda `QuitarCobertura(escudero)` (Cobertura, #8, regla 8), igual que quitar un ataque tocando su objetivo.
10. **Avanzar de fase.** El botón de abajo a la derecha (art bible, §7) manda `TerminarMovimiento` en `Mover` y `Resolver` en `Atacar`. Sin diálogo de confirmación (pilar 4; Máquina de turno, Game Feel, decisión final de #15). El botón vive lejos de Deshacer para que no se toquen por error.
11. **Deshacer (decisión del jugador).** El botón de abajo a la izquierda manda `Deshacer`: un paso por pulsación, solo en `Mover`, desactivado con la pila vacía (así `NadaQueDeshacer` no sale por la interfaz). Sin atajo de teclado ni "mantener para varios". Deshacer no cambia la selección: los resaltados de la pieza elegida se recalculan.
12. **Reclutar (decisión del jugador, 2026-11-25: un solo paso).** En `Reclutar`, un toque o clic sobre un tipo del recuadro central manda `Reclutar(tipo)`, igual con mouse y con dedo. Mientras el recuadro está abierto, el tablero marca numeradas las primeras casillas libres de aparición (`orden_aparicion`, #10, F4; las de 1 o 2 piezas son las primeras de las de 4), sin hover ni toque extra (decisión del jugador, 2026-11-25). Un tipo bloqueado se puede tocar: manda el comando y se muestra el motivo traducido (`CastilloSinReclutar`, `SinLlamadas`, `TopeDePiezas`, `SinLugarParaReclutar`); no hay una segunda lógica de disponibilidad en el cliente. Durante `Reclutar` el tablero es solo de inspección. La disposición del recuadro y qué tarjeta es el blanco de cada toque las fija la Interfaz del Modificador (#17); Entrada solo manda `Reclutar(tipo)`. La placa de la cara no recibe toques.
13. **Rendirse.** El control lo ofrece el HUD (#15) solo al jugador del turno con la mitad de las piezas del rival o menos (Librería, regla 3). Como termina la partida y no se deshace, Entrada exige **una confirmación** (diálogo de dos botones) antes de mandar `Rendirse`. Es la única confirmación de esta capa.
14. **Rechazos y avisos.** Todo comando rechazado deja el estado igual y muestra un aviso breve, un solo aviso a la vez (uno nuevo reemplaza al anterior): un código marcado para el jugador muestra `codigo.<Nombre>` (Localización, regla 5); un código interno que descartó una acción del jugador (`ComandoViejo`, `LimitePartida`; por ejemplo un comando que salió justo antes de un `TurnTimeout` llega con `ComandoViejo`, porque el contador cambió) muestra `sistema.aviso_neutro`; `TimeoutObsoleto` no muestra nada. El aviso no bloquea la entrada y dura `aviso_duracion_ms`. Con `ComandoViejo` el cliente vuelve a su último estado confirmado (en local no ocurre: hay un solo proceso).
15. **El estado manda, la animación decora.** Cada comando lleva el `contadorEsperado` del estado que el jugador ve (Librería, regla 3). El estado cambia al aceptar el comando; la animación (≤ 0,25 s) no bloquea: un toque durante ella se resuelve contra el estado nuevo (la pieza ya está en su destino) y no contra la posición animada (Movimiento, Game Feel).
16. **Ventana antidoble.** Un segundo toque sobre **la misma casilla** que el anterior, dentro de `ventana_antidoble_ms`, se ignora: evita que un doble clic mande `Mover` y, con la pieza ya en destino, la suelte por la fila 2. Toques sobre casillas distintas nunca se filtran.
17. **Lo que el jugador no puede hacer.** Arrastrar piezas o flechas; elegir el camino de un `Mover`; elegir casillas de aparición; mandar comandos fuera de `Mover`/`Atacar` (salvo Reclutar y Rendirse); atacar a un objetivo no válido (el rechazo lo muestra); mandar comandos de autoridad (`Tirada`, `TurnTimeout`, `CerrarPartida`); y usar teclado o mando (art bible, §7: llegan con las consolas, fuera del MVP).

### States and Transitions

Estado del cliente (no entra al estado de la partida ni a la huella). La habilitación de comandos (regla 4) se deriva de la partida y no es un estado propio.

| Estado | Qué significa |
|---|---|
| **Sin selección** | Ninguna pieza elegida |
| **Elegida (comandable)** | Pieza propia elegida, con comandos de tablero habilitados (regla 4) |
| **En inspección** | Pieza rival, o propia sin comandos habilitados, elegida: solo muestra el alcance |

| De → a | Disparador | Efecto |
|---|---|---|
| Sin selección → Elegida (comandable) | Toque en una pieza propia con comandos habilitados | Resalta destinos o objetivos y alcance (regla 5) |
| Sin selección → En inspección | Toque en una pieza rival, o propia sin comandos habilitados | Resalta solo el alcance |
| Sin selección → Sin selección | Toque en casilla vacía, tramo o fuera del tablero | Nada |
| Elegida → Elegida (otra pieza) | Regla 6, fila 3 | Reemplaza la selección; nada se manda |
| Elegida → Elegida (misma pieza) | `Mover` aceptado con movimiento restante; rechazo por regla 6, fila 4 | Recalcula resaltados; en el rechazo, muestra el aviso |
| Elegida → Sin selección | Regla 6, filas 2 y 5; `Mover` aceptado sin movimiento restante; cualquier comando aceptado en `Atacar`; muerte de la pieza; cambio de fase o de turno; `TurnTimeout`; fin de partida | Suelta la selección |
| En inspección → En inspección (otra pieza) | Toque en otra pieza | Reemplaza la selección |
| En inspección → Sin selección | Toque en casilla vacía o en la misma pieza; los mismos cambios de fase, turno y fin que arriba | Suelta la selección |
| En inspección → Elegida | No existe como transición directa: un cambio de fase o de turno suelta la selección, y la pieza solo pasa a comandable con un toque nuevo | — |

Un gesto es un par pulsar-soltar (regla 2); si se descarta, ningún estado cambia.

### Interactions with Other Systems

| Sistema | Entra a Entrada | Sale de Entrada | Dueño de la interfaz |
|---|---|---|---|
| #1 Librería | Consultas puras, códigos de rechazo, eventos, estado público (`contadorComandos`, tamaño de la pila) | Los diez comandos de jugador con su `contadorEsperado` (`Rendirse` sin él); el emisor lo pone el cliente | Librería (contrato); este GDD (cuándo se manda cada uno) |
| #3 Máquina de turno | Fase actual, jugador del turno, pila de deshacer | `TerminarMovimiento`, `Resolver`, `Deshacer` | Máquina (qué vale en cada fase); este GDD (los botones y gestos) |
| #6 Movimiento | `MovimientosLegales`, `Mover` y sus códigos, piezas movidas | `Mover(pieza, destino)` | Movimiento (reglas); este GDD (el gesto) |
| #7 Alcance y asignación | `CasillasEnAlcance`, `ObjetivosValidos`, códigos, eventos de flecha | `AsignarAtaque`, `QuitarAtaque` | Alcance (reglas); este GDD (el gesto, incluida la explosión del demoledor) |
| #8 Cobertura | `AliadosCubribles`, `Cubre` | `Cubrir(escudero, aliado)`, `QuitarCobertura(escudero)` | Cobertura (reglas); este GDD (el gesto) |
| #10 Reclutamiento | Disponibilidad por tipo, códigos | `Reclutar(tipo)` | Reclutamiento (reglas y recuadro); este GDD (el toque y las casillas marcadas) |
| #12 Timer | `TurnTimeout` (indirecto, por el cambio de fase) | Nada | Timer |
| #14 Localización | Texto por clave (`codigo.*`, `sistema.aviso_neutro`) | Código de rechazo | Localización la clave; este GDD si, dónde y cuánto se muestra |
| #15 Ficha y HUD | Botones, contador de piezas movidas, relojes | Selección actual (qué pieza se resalta) | HUD (cómo se ve); este GDD (qué hace cada botón) |
| #16 Vista previa | — | Cada asignación aceptada dispara su recálculo (vía eventos) | Vista previa |
| #17 Interfaz del Modificador | — | Los toques en el recuadro central y el "?" los atiende #17; Entrada no los interpreta | #17 |
| #18 Render | — | Selección, destinos, alcance, objetivos, flechas e inspección (qué dibujar) | Render (cómo se dibuja) |
| #23 Accesibilidad, #41 Adaptación móvil | — | Tamaño de casilla, escala del tablero | Cada GDD |
| #25 Rival IA | — | Nada: usa los comandos de la Librería directamente, no esta capa | Rival IA |

**Cobertura (#8):** `Cubrir` y `QuitarCobertura` se eligen tocando al aliado; `AliadosCubribles` da los aliados resaltados.

**Factibilidad:** `docs/engine-reference/unity/modules/input.md` (Input System, `Mouse.current`, `Touchscreen`, `EnhancedTouch`) y `ui.md` (UI Toolkit para los controles del HUD) no traen obstáculos para el gesto de pulsar y soltar sobre una casilla. **NOT SOURCEABLE:** ninguno de los dos cubre WebGL táctil, unificar mouse y dedo en un solo puntero, ni UI Toolkit sobre un tablero dibujado fuera de él. Factibilidad no verificada para esos tres puntos; se verifica con el prototipo en el teléfono real (riesgo del índice).

## Formulas

Entrada no define reglas de juego, pero sí tres cuentas que deciden si el pilar 4 se puede jugar con este gesto: cuánto tiempo hay por toque en cada etapa (E1, E2) y cuánto hay que acercar el tablero en el teléfono (E3). Las ejecuta el diseño y las comprueban los tests y el playtest; ninguna corre en el núcleo. `dur_mover_s` y `dur_atacar_s` son del Timer (#12), `limite_normal` de Movimiento (#6) y el tamaño de casilla del Tablero (#4): acá se **usan**, no se redefinen. Lente `systems-designer` aplicada (modo `full`, sin agente): cada fórmula se evaluó en su mínimo, su máximo y en 0.

### E1. Tiempo por toque en la etapa de mover

La fórmula `ms_por_toque_mover` se define así:

`ms_por_toque_mover = dur_mover_s × 1000 / ((limite_normal + 1) × toques_por_pieza + toques_recluta)`

**Variables:**

| Variable | Type | Range | Source | Description |
|----------|------|-------|--------|-------------|
| dur_mover_s | int | 30–90 (hoy 60 o 40) | data file (Timer, #12) | Segundos de la etapa de mover; comparte con `Reclutar` |
| limite_normal | int | 1–37 (hoy 12) | data file (config, Movimiento #6) | Piezas que se mueven por turno, sin el Rey; el Rey suma 1 |
| toques_por_pieza | int | 2 | constant | Elegir la pieza y tocar el destino, con un `Mover` por pieza. Con k movimientos de la misma pieza son 1 + k toques, así que 2 es la cota por movimiento |
| toques_recluta | int | 0 o 1 | constant | 1 en un turno con `Reclutar` (un solo paso, regla 12), 0 si no |

**Output Range:** 394 a 22.500 ms en los extremos del rango; **2.307 ms** con 60 s y límite 12, **1.538 ms** con 40 s (tramo final); con recluta, 2.222 y 1.481. No se aplica con Arremetida (sin límite: el jugador decide cuántas piezas mueve) ni en el modo Sin límite del Timer.
**Example:** 60 s, `limite_normal` 12, sin recluta: 60.000 / (13 × 2 + 0) = 2.307 ms (división entera hacia abajo). Con recluta: 60.000 / 27 = 2.222 ms.

**Frontera:** el diseño pide `ms_por_toque_mover ≥ ms_toque_objetivo` (knob, PROVISIONAL en 1.000 ms). Eso se cumple con `limite_normal ≤ dur_mover_s / 2 − 1` (29 con 60 s; 19 con 40 s; 14 con 30 s), o `≤ (dur_mover_s − 1) / 2 − 1` en un turno con recluta (28, 18 y 13). `limite_normal` 37 con 30 s da 394 ms: el gesto no alcanza para mover toda la tropa. **Se pone a decisión de #6 y del Timer**, no se corrige acá: Movimiento (F6, Tuning Knobs) ya advierte de "3 s o menos por pieza en el tramo final" y propone 8–16 sin medir, pero ni #6 ni el Timer tienen una guarda numérica entre `limite_normal` y `dur_mover_s`.

### E2. Tiempo por toque en la etapa de atacar

La fórmula `ms_por_toque_atacar` se define así:

`ms_por_toque_atacar = dur_atacar_s × 1000 / (2 × n_ataques + 1)`

`n_ataques_comodos = floor((dur_atacar_s × 1000 / ms_toque_objetivo − 1) / 2)`

**Variables:**

| Variable | Type | Range | Source | Description |
|----------|------|-------|--------|-------------|
| dur_atacar_s | int | 15–90 (hoy 30 o 20) | data file (Timer, #12) | Segundos de la etapa de atacar |
| n_ataques | int | 0–61 | calculated | Piezas con un ataque asignado: como mucho una por pieza viva (Alcance, regla 9; 60 sin el Rey más el Rey, Movimiento F5) |
| ms_toque_objetivo | int | 600–1.500 (PROVISIONAL: 1.000) | data file (Tuning Knobs) | Tiempo realista entre dos toques consecutivos; **sin fuente**, se mide en el playtest |

**Output Range:** 2 × n + 1 toques (el `+ 1` es "Terminar turno"). Con n = 0: 30.000 ms. **1.111 ms** con 30 s y 13 ataques; **740 ms** con 20 s. `n_ataques_comodos` da **14** con 30 s y **9** con 20 s (7 con 15 s). Con n = 61 daría 243 ms: no es un error, el jugador no asigna todo y la vista previa muestra lo que importa; la fórmula acota cuánto del turno se puede planear con comodidad.
**Example:** 30 s, `ms_toque_objetivo` 1.000: (30.000 / 1.000 − 1) / 2 = 14,5, hacia abajo 14 ataques.

**Dónde se rompe:** con el tramo final (20 s) un jugador con 13 piezas que quiere atacar con todas (26 toques) necesita 740 ms por toque, por debajo del objetivo. No es un defecto del gesto: es el pilar 4 ("el timer es presión buscada") actuando sobre un ejército grande. Se registra como riesgo y se mide (AC-33).

### E3. Escala del tablero en el teléfono

La fórmula `escala_zoom_min` se define así:

`escala_zoom_min = max(1, ceil(objetivo_tactil_pt / casilla_pt × 100) / 100)`

`columnas_visibles = floor(columnas / escala_zoom_min)` · `filas_visibles = floor(filas / escala_zoom_min)`

**Variables:**

| Variable | Type | Range | Source | Description |
|----------|------|-------|--------|-------------|
| objetivo_tactil_pt | int | 44 | constant (citado en Tablero, Hechos de los vecinos) | Lado mínimo cómodo de un blanco táctil |
| casilla_pt | number | ≥ 2 (hoy ≈ 26) | calculated (Tablero #4 / #41) | Lado de una casilla en el teléfono: el ancho del tablero en puntos dividido 24 |
| columnas | int | 24 | constant (Tablero #4) | Columnas del tablero |
| filas | int | 15 | constant (Tablero #4) | Filas del tablero |

**Output Range:** 1,00 (casilla ≥ 44 pt, tablero entero a la vista) a 22 (casilla de 2 pt). Con la casilla de ≈ 26 pt: **1,70**, es decir **14 columnas y 8 filas** visibles de 24 × 15. Con `casilla_pt` < 2 no queda ni una columna visible: fuera de dominio.
**Example:** 44 / 26 = 1,692 → 1,70; 24 / 1,70 = 14,1 → 14 columnas; 15 / 1,70 = 8,8 → 8 filas.

**Qué implica:** con el castillo pegado a cada borde lateral (Tablero, regla de despliegue) no se pueden ver los dos castillos a la vez sin perder precisión de toque. La decisión (zoom y desplazamiento, otra vista, o aceptar casillas de 26 pt) es de **#41 Adaptación móvil** y #23; esta fórmula solo pone el costo. PROVISIONAL: `casilla_pt` es una aproximación de Tablero.

**Lazos de retroalimentación:** ninguno. Entrada no produce recursos ni modifica el estado.

## Edge Cases

Lente `systems-designer` aplicada a las secciones C y D (modo `full`, sin agente). En modo `guided` se incluyen todos los casos encontrados; el jugador puede quitar los que no quiera.

### Gesto y puntero

- **Si el puntero se pulsa en una casilla y se suelta en otra:** el gesto se descarta, sin comando ni cambio de selección (regla 2). Un dedo que se desliza no mueve nada.
- **Si hay un puntero activo y llega un segundo (otro dedo, otro botón):** el segundo se ignora hasta que el primero suelte (regla 2).
- **Si la pulsación empieza en un botón del HUD y se suelta sobre el tablero (o al revés):** no se activa ninguno (regla 3).
- **Si la ventana pierde el foco con el puntero pulsado:** el gesto se descarta.
- **Si el jugador hace doble clic sobre un destino:** el primer clic manda `Mover`; el segundo, dentro de `ventana_antidoble_ms` y sobre la misma casilla, se ignora (regla 16). Pasado ese tiempo se resuelve contra el estado nuevo: la casilla es la de la pieza, así que la suelta (regla 6, fila 2).
- **Si un toque cae en un punto fuera del tablero y fuera de un control** (los bordes del HUD): no hace nada y no suelta la selección.

### Selección

- **Si la pieza elegida muere en la resolución** (propia por fuego amigo, o rival en inspección): la selección se suelta con el evento de muerte; los resaltados desaparecen en el mismo cuadro.
- **Si la fase o el turno cambian con una pieza elegida** (incluido un `TurnTimeout`): se suelta. Un gesto a medio hacer se descarta; un comando que ya salió antes del cambio llega rechazado con `ComandoViejo` (el contador cambió con el timeout; la fila 10 de la Librería se revisa antes que `FaseIncorrecta`) y muestra `sistema.aviso_neutro` (regla 14).
- **Si el jugador elige una pieza rival durante su turno:** queda en inspección (solo alcance); ningún toque siguiente la usa como origen de un comando.
- **Si elige una pieza propia agotada en `Mover`:** queda elegida como comandable con sus destinos vacíos y el restante en 0 visible. Tocar una casilla vacía muestra `PiezaAgotada` (regla 6, fila 4).
- **Si elige una pieza propia con el cupo de piezas lleno y sin moverse:** igual; el rechazo es `SinMovimientos`.
- **Si una pieza recién reclutada aparece mientras hay otra elegida:** la selección no cambia; la recluta es una pieza más para elegir.

### Resolución del toque

- **Si con un soldado elegido en `Atacar` el jugador toca un aliado:** lo elige (fila 3); no sale `ObjetivoPropio`. **Si toca un tramo propio en pie:** sale `ObjetivoPropio` (fila 4), porque un tramo no es una pieza que elegir.
- **Si con una catapulta elegida el jugador toca un aliado en alcance:** le asigna el golpe de área (fila 1), no lo elige. La selección se suelta tras asignar (regla 8), así un toque siguiente no repite la asignación.
- **Si con una catapulta elegida el jugador toca una casilla en alcance con un rival o vacía:** asigna (fila 1). **Si la casilla está fuera de alcance y vacía:** `FueraDeAlcance` (fila 4); **si tiene una pieza:** la elige (fila 3).
- **Si con una pieza de ataque directo elegida el jugador toca un rival fuera de alcance:** lo elige para ver su alcance (fila 3); no sale `FueraDeAlcance`. El alcance de P deja de verse porque la selección cambió.
- **Si toca una casilla vacía en alcance con una pieza de ataque directo:** sale `SinObjetivo` (fila 4).
- **Si el demoledor está elegido y el jugador toca cualquier otra casilla vacía:** suelta la selección (fila 5); no se manda `ObjetivoInvalido`.
- **Si el demoledor tiene la explosión asignada y no está elegido:** un toque lo elige (resalta su flecha); un segundo toque sobre él manda `QuitarAtaque` y suelta la selección (regla 9).
- **Si un ataque asignado tiene un objetivo que luego deja de ser válido:** no puede ocurrir durante `Atacar` (Alcance, regla 11); Entrada no revalida.
- **Si el jugador reasigna con otro objetivo:** un solo `AsignarAtaque`; la flecha anterior desaparece en el mismo cuadro (Alcance, regla 9).
- **Si el escudero tiene cobertura y el jugador le asigna un ataque:** la cobertura desaparece a la vista (Alcance, UI Requirements). El camino inverso es tocar de nuevo al aliado cubierto (`QuitarCobertura`, regla 9).

### Comandos y estado

- **Si el jugador toca Deshacer con la pila vacía:** el botón está desactivado; no se manda nada y no sale `NadaQueDeshacer`.
- **Si el jugador pulsa Deshacer varias veces seguidas:** un `Deshacer` por pulsación; con mouse, dos clics rápidos son dos pasos (no hay ventana antidoble para botones). Cada uno lleva el `contadorEsperado` del estado ya actualizado.
- **Si toca el botón de avanzar dos veces:** el primero cierra la fase; el segundo llega a la fase siguiente y manda su comando (`Resolver` tras `TerminarMovimiento`). *Riesgo:* un doble clic cierra el turno. El HUD (#15) decide si el botón se desactiva un instante al cambiar de fase; se anota en Open Questions.
- **Si un comando llega rechazado con `LimitePartida` o `ComandoViejo`:** `sistema.aviso_neutro`; el cliente vuelve a su último estado confirmado (regla 14).
- **Si un comando rechazado deja un aviso en pantalla y el jugador cambia de idioma:** el aviso cambia de idioma y su tiempo restante no se reinicia (Localización, regla 12).
- **Si el jugador toca una opción bloqueada del recuadro de reclutar:** manda `Reclutar(tipo)`, la Librería rechaza y se muestra el motivo; el gasto de llamadas no cambia (Reclutamiento, regla 3).
- **Si el jugador toca un tipo disponible por error:** recluta sin vuelta atrás (Reclutamiento, regla 11). Efecto de la decisión de un solo paso; ver Open Questions.
- **Si el jugador cancela el diálogo de `Rendirse`:** no se manda nada. **Si lo confirma con la partida ya terminada (por tiempo, por ejemplo):** `PartidaTerminada`, sin efecto.

### Tiempo y reloj

- **Si el timer de la etapa vence con una animación en curso:** el estado ya cambió; la animación termina sola y la fase nueva se ve de inmediato (Máquina, Game Feel).
- **Si el jugador toca durante `EsperandoTirada`:** el tablero es de inspección; no sale ningún comando (regla 4).
- **Si el reloj de partida de un jugador llega a 0 con un gesto a medias:** llega `CerrarPartida` y el gesto se descarta.

### Plataforma

- **Si el dispositivo tiene mouse y pantalla táctil:** los dos punteros producen el mismo gesto (regla 2); no hay modos separados.
- **Si la casilla mide menos de `objetivo_tactil_pt` en el teléfono:** el diseño no cambia el gesto; E3 pone el costo y #41 decide el zoom. Hasta entonces la selección puede errar de casilla; el error es siempre recuperable (soltar la pieza, Deshacer), salvo `Reclutar` (un solo paso) y `Resolver`.

## Dependencies

**Entrada y selección depende de** (de qué no puede funcionar sin ello: *dura*; de qué mejora pero funciona: *blanda*):

| Sistema | Tipo | Qué usa | Estado del GDD |
|---|---|---|---|
| #1 Librería de reglas | Dura | Los diez comandos de jugador, las consultas puras, los códigos de rechazo, el `contadorEsperado` y los eventos | Designed |
| #3 Máquina de turno | Dura | Fases (`Mover`, `Atacar`, `Reclutar`, `EsperandoTirada`), jugador del turno, pila de deshacer | Designed |
| #6 Movimiento | Dura | `MovimientosLegales`, `Mover`, sus códigos, el restante de la pieza | Designed |
| #7 Alcance y asignación | Dura | `CasillasEnAlcance`, `ObjetivosValidos`, `AsignarAtaque`, `QuitarAtaque`, sus códigos | Designed |
| #10 Reclutamiento | Dura | `Reclutar(tipo)`, disponibilidad por tipo, sus códigos | Designed |
| #14 Localización | Dura | `codigo.<Nombre>`, `sistema.aviso_neutro`, `ui.*` de los botones | Designed |
| #4 Tablero | Dura | Geometría 24 × 15, casilla de 68 px, ocupante y tramos | Designed |
| #8 Cobertura del escudero | Dura | `Cubrir`, `QuitarCobertura`, `AliadosCubribles` | Designed |
| #12 Timer de turno | Blanda | Solo por el cambio de fase y `TurnTimeout`; no pide nada a Entrada | Designed |
| Art bible, §7 | Referencia | Posición de los botones, tamaños, "nada solo por color" | Aprobado |

**Dependen de Entrada y selección:**

| Sistema | Qué le exige | Estado |
|---|---|---|
| #15 Ficha y HUD | Botones de avanzar, Deshacer y el control de Rendirse; contador de piezas movidas; lee qué pieza está elegida | Sin GDD |
| #16 Vista previa de ataques | Se recalcula con cada asignación aceptada | Existe (tanda 3) |
| #17 Interfaz del Modificador | Atiende sus propios toques (recuadro central y "?"); estado «panel abierto» sin comandos (regla 4) | Existe (tanda 3) |
| #18 Render | Dibuja selección, destinos, alcance, objetivos, flechas e inspección | Sin GDD |
| #23 Accesibilidad | Alternativas a color y a pulso; lector de pantalla; cuadrícula opcional | Sin GDD |
| #24 Tutorial | Enseña los gestos (elegir, mover, atacar, deshacer) | Sin GDD |
| #41 Adaptación móvil | Decide el tamaño táctil y el zoom (E3) | Sin GDD |

**Bidireccionalidad (verificada con grep sobre los GDD del 2026-11-25).** Nombran a Entrada (#13): Librería (Interactions), Movimiento (Game Feel, "dos toques por pieza"), Alcance (UI Requirements, gesto del demoledor, y su tabla de interacciones), Reclutamiento (Dependencies como dependencia *blanda*, y Game Feel: **inconsistente** con la regla 12) y Localización (Interactions); el Tablero la nombra en Game Feel. **No la nombran:** Máquina de turno (su UI Requirements habla de #15, pero no de Entrada: manda los botones de avanzar y Deshacer, que esta capa convierte en comandos), Timer (#12), Castillo y muros (#5) y Catálogo (#2). Los dos primeros son los que conviene actualizar (Cross-References).

## Tuning Knobs

Todos son datos del cliente (no entran al estado ni a la huella). Se derivan de las variables `data file` de la sección Formulas.

| Perilla | Vigente | Rango seguro | Si es demasiado alto | Si es demasiado bajo | Interactúa con |
|---|---|---|---|---|---|
| `ventana_antidoble_ms` | 250 (PROVISIONAL, sin fuente) | 0–500 | Frena una cadena legítima de toques sobre la misma casilla (por ejemplo elegir y asignar al demoledor rápido) | Un doble clic manda `Mover` y suelta la pieza por la fila 2 | Regla 16 |
| `aviso_duracion_ms` | 2.500 | 1.500–5.000 | El aviso tapa el tablero y compite con el siguiente rechazo | El jugador no alcanza a leer una frase de `codigo.*` mientras mira el tablero | Largo de los textos (Localización, F4) |
| `ms_toque_objetivo` | 1.000 (PROVISIONAL, sin fuente: se mide) | 600–1.500 | Marca como imposibles turnos que sí se juegan; `n_ataques_comodos` y la guarda de `limite_normal` quedan demasiado estrictos | Se acepta un turno que en la práctica no se puede tocar en el tiempo | E1, E2 (`dur_mover_s`, `dur_atacar_s`, `limite_normal`) |

**Perillas de otros GDD que afectan a Entrada** (se señalan, no se duplican): `dur_mover_normal_s`, `dur_mover_final_s`, `dur_atacar_normal_s`, `dur_atacar_final_s` (Timer, #12); `limite_normal` (Movimiento, #6); tamaño de casilla (Tablero #4 y #41); "Mostrar cuadrícula" y "Reducir efectos" (Accesibilidad #23 / Ajustes #20).

**No son perillas** (constantes del contrato de esta capa): gesto de pulsar y soltar sobre la misma casilla, un solo puntero, la prioridad de la tabla de la regla 6, soltar la selección tras un comando en `Atacar` y `objetivo_tactil_pt` (44, de Tablero). Cambiarlas cambia el diseño del gesto, no un número.

**Interacción entre perillas:** si `ms_toque_objetivo` sube, `n_ataques_comodos` baja y la guarda entre `limite_normal` y `dur_mover_s` se estrecha. `ventana_antidoble_ms` en 0 la apaga.

## Visual/Audio Requirements

Lente `art-director` aplicada (modo `full`, sin agente) a partir del art bible, §7 (*UI/HUD Visual Direction*). Los eventos que Entrada hace visibles y sonoros son elegir, mandar, rechazar y soltar; cómo se dibuja cada marca es de Render (#18) y HUD (#15), y el sonido de Audio (#22).

**Marcas en el tablero** (cada una distinguible por forma, no solo por color: art bible §7, *Usabilidad y accesibilidad*):

- **Pieza elegida (comandable):** contorno grueso alrededor de la figura. **En inspección:** contorno distinto en forma (por ejemplo discontinuo), para no confundir "voy a mover esto" con "estoy mirando esto".
- **Destinos:** una marca por casilla alcanzable con el **costo** en número (≥ 14 px, negrita, borde Tinta, como los números de la ficha). Un destino legal nunca tiene pieza encima, así que la marca no compite con una ficha.
- **Alcance:** relleno tenue por casilla; la **zona ciega** se distingue por forma (Alcance #7, UI Requirements). Los **objetivos válidos** dentro del alcance llevan una marca más fuerte y distinta del alcance.
- **Flechas:** una por ataque asignado, con la punta sobre la casilla objetivo; esa casilla es el blanco táctil de la punta (regla 8). Siempre visibles en `Atacar`.
- **Pieza ya movida:** marca por forma para saber qué piezas gastaron cupo (Movimiento, UI Requirements; *propuesta de #6, no está en el art bible*).
- **Estado pulsado:** la casilla bajo el puntero se marca mientras está pulsada, para que el jugador sepa que el toque se registró antes de soltar.
- **Aviso de rechazo:** en la zona de avisos del HUD, la banda superior a la derecha del timer (x 1170 a 1840; #15, regla 18, cierre de la tanda 3), donde no tapa ninguna casilla, texto de 16 px, con un ícono además del texto; entra con la animación de interfaz (≤ 0,25 s, frenado suave). No usa el rojo como único indicador.

**Principios del art bible que aplican:** *Lo importante, lo más visible* (en `Atacar`, la vista previa y las placas MUERE/ROTO ganan a las marcas de selección); *Nada solo por color*; *Animación de la interfaz ≤ 0,25 s*; *Controles: un clic o un toque, sin información que aparezca solo al pasar el mouse*; *Opción Reducir efectos* (apaga el pulso de la marca de elegida).

**Audio (a decidir por #22):** un sonido corto al elegir una pieza y otro al mandar un comando; el rechazo suena **neutro**, nunca punitivo (Reclutamiento, Game Feel: "se explica, no se reprende"); inspeccionar rivales no suena distinto de elegir una pieza propia más allá del timbre del bando.

> 📌 **Asset Spec** — Visual/Audio requirements are defined. After the art bible is approved, run `/asset-spec system:entrada-y-seleccion` to produce per-asset visual descriptions, dimensions, and generation prompts from this section.

## Game Feel

- **Feedback de la pulsación:** la casilla pulsada se marca en el mismo cuadro de la pulsación (≤ 16,6 ms a 60 fps), antes de soltar.
- **Elegir:** los resaltados (destinos, alcance, objetivos) aparecen en el mismo cuadro del soltar (Movimiento, Game Feel: "los puntos aparecen dentro de un cuadro de 60 fps").
- **Mandar:** el estado cambia al aceptar el comando; la animación (≤ 0,25 s) no bloquea y el siguiente toque se acepta mientras ocurre (regla 15).
- **Corregir no cuesta:** soltar la pieza, reasignar un ataque, quitarlo y Deshacer no piden confirmación. Las únicas confirmaciones de esta capa son `Rendirse` (regla 13) y, por la decisión de un solo paso, ninguna en `Reclutar`.
- **Presupuesto de toques:** 2.307 ms por toque en `Mover` con 60 s y límite 12; 1.538 ms en el tramo final; 1.111 ms con 13 ataques en 30 s (E1, E2). Un toque no puede costar más que eso sin romper el pilar 4.
- **Los rechazos no frenan:** el aviso no bloquea la entrada y se reemplaza con el siguiente.
- **Verificación:** capturas retenidas de cada marca (AC-32) y playtest en un teléfono real y con mouse (AC-33, AC-34).

## UI Requirements

Sin pantalla propia: Entrada actúa sobre el tablero y sobre los controles del HUD (#15).

- **Botones** (art bible §7): Deshacer abajo a la izquierda; avanzar abajo a la derecha ("Atacar" en `Mover`, "Terminar turno" en `Atacar`), con el contador de piezas movidas encima; Rendirse con confirmación (regla 13), dónde vive lo define #15.
- **Blanco de cada toque:** una casilla (68 px a 1080p). La punta de la flecha es la casilla objetivo. Ningún control del HUD tapa una casilla (Tablero, UI Requirements).
- **Recuadro de reclutar** (de #10): cada tipo es un blanco táctil independiente, con cantidad, vivas / tope y llamadas restantes, y las bloqueadas con su motivo traducido. Mientras está abierto, las casillas de aparición se marcan numeradas en el tablero (regla 12).
- **Resaltados y selección:** los de la sección Visual/Audio, visibles sin pasar el mouse.
- **Aviso de rechazo:** zona de avisos del HUD (banda superior, a la derecha del timer; #15, regla 18); texto por código desde Localización (#14).
- **Accesibilidad (#23):** todo se hace con un toque o un clic, sin depender del hover ni de mantener pulsado; nada solo por color; el texto de avisos escala con el tamaño de texto; un lector de pantalla anuncia la pieza elegida (tipo, bando, vida, movimiento restante) a pedido y los avisos de rechazo. Teclado y mando: fuera del MVP (art bible, §7).
- **Móvil (#41):** tamaño táctil y zoom según E3; Entrada no define el zoom.

> **📌 UX Flag — Entrada y selección**: This system has UI requirements. In Phase 4 (Pre-Production), run `/ux-design` to create a UX spec for each screen or HUD element this system contributes to **before** writing epics. Stories that reference UI should cite `design/ux/[screen].md`, not the GDD directly.

## Cross-References

| Documento | Qué se referencia | Estado |
|---|---|---|
| Librería de reglas (#1), regla 3 y tabla de validación | Lista cerrada de comandos, `contadorEsperado`, orden de códigos | Consistente |
| Librería de reglas (#1), regla 10 | `MovimientosLegales`, `ObjetivosValidos`, `CasillasEnAlcance` | Consistente |
| Máquina de turno (#3), reglas 7, 8, 9 y 17 | Qué comando vale en cada fase; pila de deshacer | Consistente; la Máquina no nombra a Entrada |
| Máquina de turno (#3), Game Feel y UI Requirements | Botón de avanzar sin confirmación, Deshacer abajo a la izquierda | Consistente; la decisión final es de #15 |
| Movimiento (#6), reglas 3, 10, 13 y 15; F6; UI Requirements | Un destino por comando; restante visible; "dos toques por pieza"; marca de pieza ya movida | Consistente |
| Alcance y asignación (#7), reglas 2, 9, 10, 12, 13; UI Requirements | Objetivo = casilla; reemplazo; quitar; demoledor "tocándolo otra vez"; el gesto final lo fija este GDD | Consistente; cierra la deuda de #7 |
| Reclutamiento (#10), Game Feel y UI Requirements | Un clic con mouse, dos toques en táctil; vista previa al pasar el mouse | Consistente desde el 2026-11-25: un toque y casillas marcadas sin hover |
| Cobertura (#8), reglas 8, 12 y 14 | `Cubrir`, `QuitarCobertura`, `AliadosCubribles` | Consistente |
| Localización (#14), reglas 5, 11, 12 y 15 | Texto por código; aviso neutro; el cambio de idioma no toca la selección | Consistente |
| Tablero (#4), UI Requirements y Hechos de los vecinos | Casilla de 68 px; ≈ 26 pt en móvil; sin cuadrícula por defecto | Consistente (E3 usa 26 pt) |
| Timer (#12), D1 y Tuning Knobs | `dur_mover_s`, `dur_atacar_s` | Consistente; el Timer no nombra a Entrada |
| Art bible, §7 | Posición de botones, tamaños, "nada solo por color", controles | Consistente |
| Game concept, Pilares 1 y 4 | Fantasía y presupuesto de toques | Consistente; la decisión de reclutar toca el pilar 1 |

## Acceptance Criteria

Lente `qa-lead` aplicada al final (modo `full`, sin agente). Tipo de evidencia entre corchetes: **[Logic]** prueba automática de `ResolverToque` (función pura, sin Unity, `tests/unit/` del código de cliente), **[Integration]** prueba con la Librería real, **[UI]** captura retenida en `production/qa/evidence/`, **[Playtest]** sesión con jugadores. Cada GIVEN trae su preparación.

### Gesto y puntero (reglas 2 y 3)

- **AC-01 [Logic]** — **GIVEN** un puntero pulsado sobre la casilla (5, 7), **WHEN** se suelta sobre la (6, 7), **THEN** no se manda ningún comando y la selección queda igual.
- **AC-02 [Logic]** — **GIVEN** un puntero activo, **WHEN** llega un segundo puntero (otro dedo) y suelta, **THEN** el segundo se ignora y no se manda nada.
- **AC-03 [Logic]** — **GIVEN** una pieza elegida, **WHEN** se toca un punto de los 144 px laterales fuera de un control, **THEN** no hay casilla, no se manda nada y la selección no cambia.
- **AC-04 [Logic]** — **GIVEN** una pulsación que empieza sobre el botón Deshacer, **WHEN** se suelta sobre el tablero, **THEN** no se manda `Deshacer` ni se elige nada.

### Habilitación y selección (reglas 4 y 5)

- **AC-05 [Integration]** — **GIVEN** la partida en `Reclutar`, `EsperandoTirada` o `Terminada`, **WHEN** se toca una pieza propia y luego una casilla vacía, **THEN** la pieza queda en inspección (solo alcance) y se manda 0 comandos.
- **AC-06 [Integration]** — **GIVEN** el soldado propio en `Mover` con el turno propio, **WHEN** se lo toca, **THEN** se marcan `MovimientosLegales` con su costo y `CasillasEnAlcance`, no se manda ningún comando y el timer de la etapa no se detiene.
- **AC-07 [Integration]** — **GIVEN** cualquier fase, **WHEN** se toca una pieza rival, **THEN** se marca solo su `CasillasEnAlcance` (sin destinos ni objetivos) y se manda 0 comandos.
- **AC-08 [Logic]** — **GIVEN** una pieza elegida, **WHEN** la pieza muere por un `Resolver` o cambia la fase, **THEN** la selección queda vacía en ese mismo estado.

### Resolución del toque (regla 6)

- **AC-09 [Integration]** — **GIVEN** un soldado elegido en `Mover` y un destino legal X, **WHEN** se toca X, **THEN** se manda exactamente un `Mover(soldado, X)` con el `contadorEsperado` del estado mostrado.
- **AC-10 [Logic]** — **GIVEN** una pieza elegida que no es el demoledor, **WHEN** se toca su propia casilla, **THEN** se suelta la selección y se manda 0 comandos.
- **AC-11 [Integration]** — **GIVEN** un soldado elegido en `Mover` y un rival en una casilla vecina, **WHEN** se toca al rival, **THEN** la selección pasa al rival (inspección) y no se manda `Mover` (no sale `DestinoOcupado`).
- **AC-12 [Integration]** — **GIVEN** una catapulta elegida en `Atacar` con un soldado propio en alcance, **WHEN** se toca ese soldado, **THEN** se manda `AsignarAtaque(catapulta, casilla del soldado)` y la selección se suelta.
- **AC-13 [Integration]** — **GIVEN** un soldado elegido en `Atacar` y un aliado en la casilla vecina, **WHEN** se toca al aliado, **THEN** el aliado queda elegido y no se manda comando (no sale `ObjetivoPropio`); **y GIVEN** un tramo propio en pie en alcance, **WHEN** se lo toca, **THEN** sale `ObjetivoPropio` y la selección sigue.
- **AC-14 [Integration]** — **GIVEN** un soldado elegido en `Mover` y una casilla vacía fuera de su alcance, **WHEN** se toca, **THEN** se manda `Mover`, la Librería devuelve `FueraDeAlcance`, se muestra `codigo.FueraDeAlcance`, el estado queda igual y la pieza sigue elegida.
- **AC-15 [Logic]** — **GIVEN** un demoledor elegido en `Atacar`, **WHEN** se toca una casilla vacía que no es la suya, **THEN** se suelta la selección y se manda 0 comandos (no sale `ObjetivoInvalido`).
- **AC-16 [Integration]** — **GIVEN** un soldado con ataque asignado a X y elegido, **WHEN** se toca X, **THEN** se manda `QuitarAtaque(soldado)` y la flecha desaparece; **y WHEN** se vuelve a elegir y se toca X otra vez, **THEN** se manda `AsignarAtaque`.

### Mover y atacar (reglas 7, 8 y 9)

- **AC-17 [Integration]** — **GIVEN** un jinete con 4 de movimiento restante, **WHEN** se mueve 3 casillas, **THEN** sigue elegido con 1 de restante y sus destinos se recalculan desde la casilla nueva; **WHEN** se mueve la última, **THEN** la selección se suelta.
- **AC-18 [Integration]** — **GIVEN** un arquero con ataque asignado, **WHEN** se lo elige y se toca otra casilla en alcance, **THEN** se manda un solo `AsignarAtaque` que reemplaza al anterior y la selección se suelta.
- **AC-19 [Integration]** — **GIVEN** un demoledor, **WHEN** se lo toca y se lo toca otra vez, **THEN** se manda `AsignarAtaque(demoledor, su casilla)` y la selección se suelta; **y WHEN** se lo toca y se lo toca otra vez, **THEN** se manda `QuitarAtaque(demoledor)`.
- **AC-20 [Integration]** — **GIVEN** un escudero elegido en `Atacar` y un aliado cubrible, **WHEN** se toca al aliado, **THEN** se manda `Cubrir(escudero, aliado)`; **y GIVEN** el mismo escudero, **WHEN** se toca un rival en alcance, **THEN** se manda `AsignarAtaque` y la cobertura desaparece; **y GIVEN** el escudero que ya cubre a ese aliado, **WHEN** se lo elige y se toca al aliado, **THEN** se manda `QuitarCobertura(escudero)`.

### Botones y comandos de jugador (reglas 10 a 15)

- **AC-21 [Integration]** — **GIVEN** la fase `Mover`, **WHEN** se toca el botón de avanzar, **THEN** se manda `TerminarMovimiento` sin diálogo de confirmación; **y GIVEN** la fase `Atacar`, **THEN** manda `Resolver` sin diálogo.
- **AC-22 [Integration]** — **GIVEN** la pila de deshacer vacía, **THEN** el botón Deshacer está desactivado; **GIVEN** 3 movimientos hechos, **WHEN** se pulsa 2 veces, **THEN** se mandan 2 `Deshacer` y la pila queda con 1 entrada; la selección no cambia.
- **AC-23 [Integration]** — **GIVEN** la fase `Reclutar` con una cara disponible, **WHEN** se toca un tipo disponible, **THEN** se manda `Reclutar(tipo)` en un solo toque, con mouse y con dedo, y las casillas de aparición ya estaban marcadas; **y WHEN** se toca un tipo bloqueado, **THEN** se muestra su motivo traducido y el estado no cambia.
- **AC-24 [Integration]** — **GIVEN** el control de Rendirse visible, **WHEN** se lo toca y se cancela el diálogo, **THEN** no se manda nada; **WHEN** se lo confirma, **THEN** se manda `Rendirse`.
- **AC-25 [Logic]** — **GIVEN** cada código de rechazo, **THEN** un código marcado "Sí" muestra `codigo.<Nombre>`; `ComandoViejo` y `LimitePartida` muestran `sistema.aviso_neutro`; `TimeoutObsoleto` no muestra nada; y nunca se muestra una clave sin traducir en un build publicado (Localización, regla 14).
- **AC-26 [Integration]** — **GIVEN** un `Mover` aceptado cuya animación dura 0,25 s, **WHEN** el jugador toca otro destino a los 0,1 s, **THEN** el toque se resuelve contra el estado nuevo y el segundo `Mover` se acepta si es legal.
- **AC-27 [Logic]** — **GIVEN** un toque sobre la casilla X, **WHEN** llega otro sobre X a 249 ms, **THEN** se ignora; **WHEN** llega a 251 ms, **THEN** se resuelve (con `ventana_antidoble_ms` = 250); con la perilla en 0 no se ignora ninguno.
- **AC-28 [Integration]** — **GIVEN** una pieza elegida y un gesto a medias, **WHEN** llega un `TurnTimeout` de mover, **THEN** el gesto se descarta, la selección se suelta y no se manda ningún comando.
- **AC-29 [Integration]** — **GIVEN** un aviso en pantalla, **WHEN** se cambia de idioma, **THEN** el aviso cambia de idioma y su tiempo restante no se reinicia; la selección, las flechas y la pila no cambian.

### Fórmulas (Sección D)

- **AC-30 [Logic]** — **GIVEN** `dur_mover_s` 60 y `limite_normal` 12, **THEN** `ms_por_toque_mover` = 2.307 (sin recluta) y 2.222 (con recluta); con 40 s, 1.538 y 1.481; con 30 s y `limite_normal` 37, 394; con 90 s y `limite_normal` 1, 22.500.
- **AC-31 [Logic]** — **GIVEN** `dur_atacar_s` 30, 20 y 15 con `ms_toque_objetivo` 1.000, **THEN** `n_ataques_comodos` es 14, 9 y 7; y `ms_por_toque_atacar` con 13 ataques en 30 s es 1.111 y en 20 s, 740; con n = 0, 30.000.
- **AC-31b [Logic]** — **GIVEN** `casilla_pt` 26, **THEN** `escala_zoom_min` = 1,70, 14 columnas y 8 filas visibles; con `casilla_pt` 44 o más, 1,00, 24 y 15; con `casilla_pt` 2, 22.

### Presentación y juego

- **AC-32 [UI]** — **GIVEN** una pieza propia elegida en `Mover`, una en `Atacar` con flecha, una pieza rival en inspección y un aviso de rechazo, **THEN** hay una captura retenida de cada uno en `production/qa/evidence/` donde elegida e inspección se distinguen por forma, los destinos muestran su costo y el aviso está entre los dos botones sin tapar casillas.
- **AC-33 [Playtest]** — **GIVEN** un teléfono real y el tablero con la casilla más grande que permita #41, **WHEN** un jugador mueve 12 piezas y ataca con todas en el modo Variable, **THEN** se registran el tiempo por toque y los toques errados por turno, contra `ms_toque_objetivo`; el riesgo del índice se cierra solo si la mediana está por debajo del objetivo y no hay un comando enviado por error en la sesión.
- **AC-34 [Playtest]** — **GIVEN** una partida local con mouse, **THEN** ningún jugador de la sesión reporta haber atacado a una pieza propia con una catapulta sin quererlo, y ninguno reclutó el tipo que no quería; el resultado decide si se reabre la regla de un solo paso (regla 12).
- **AC-35 [Logic]** — **GIVEN** una prueba automática que solo mueve el puntero sobre el tablero y los botones sin pulsar, **THEN** no aparece ninguna información que no aparezca también tras elegir con un toque (sin información solo por hover).
- **AC-36 [Integration]** — **GIVEN** un ataque asignado, **THEN** la vista previa (#16) se recalcula dentro de `vista_previa_presupuesto_ms` (5 ms p95, Alcance) y la flecha aparece en el mismo cuadro.

**Gaps y líneas no verificables encontrados por la lente `qa-lead`:** (1) AC-33 y AC-34 son de playtest, no automáticos, por decisión: son los riesgos del índice; (2) no hay criterio para el aviso "se registra una vez por clave e idioma" de Localización, que es de #14; (3) el comportamiento del botón de avanzar tras un doble clic (Edge Cases) queda sin criterio hasta que #15 lo defina.

## Open Questions

| # | Pregunta | Dueño | Se resuelve |
|---|---|---|---|
| 3 | ~~**Doble clic en el botón de avanzar**~~ **Resuelta por #15 (regla 14):** Avanzar se bloquea `bloqueo_avance_ms` (300 ms) tras cada cambio de fase | — | — |
| 4 | **Tamaño táctil y zoom** (E3): con casillas de ≈ 26 pt se ven 14 de 24 columnas a 44 pt. ¿Zoom y desplazamiento, otra vista, o aceptar casillas pequeñas? | #41, #23 | Antes de lanzar en móvil; el índice pide probar en un teléfono real antes de cerrar las reglas |
| 5 | **Guarda numérica entre `limite_normal` y `dur_mover_s`** (E1): hoy 37 piezas con 30 s dan 394 ms por toque. ¿Se agrega una guarda al estilo de la del Timer (`turno_max_s ≤ reloj_jugador_s ÷ 6`)? | #6, #12 | Al revisar #6 y #12 |
| 6 | ~~**Marca de "pieza ya movida"**~~ **Resuelta por #15 (regla 5):** ficha atenuada con gris y marco interior (confirmado por el jugador el 2026-11-25); falta sumarla al art bible | — | — |
| 7 | ~~**Control de Rendirse**~~ **Resuelta por #15 (regla 14):** arriba a la izquierda, bandera blanca y texto, visible solo si se cumple la condición | — | — |
| 8 | **`ms_toque_objetivo` (1.000 ms) y `ventana_antidoble_ms` (250 ms) no tienen fuente**: son objetivos de diseño provisionales | Este GDD | Playtest (AC-33) |
| 9 | **Teclado y mando** fuera del MVP (art bible, §7: llegan con las consolas, hoy descartadas) | #23 | Si se reabren las consolas |
| 10 | **NOT SOURCEABLE:** `docs/engine-reference/unity/modules/input.md` y `ui.md` no cubren WebGL táctil, unificar mouse y dedo en un solo puntero, ni UI Toolkit sobre un tablero dibujado fuera de él. La factibilidad de esos tres puntos no está verificada contra el motor fijado | Este GDD / ADR de entrada | `/architecture-decision` y el prototipo en el teléfono |
