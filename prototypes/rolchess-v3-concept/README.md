<!-- PROTOTYPE - NOT FOR PRODUCTION -->
<!-- Question: with the Modificador (state-dependent die odds shown before rolling), does the die stop deciding matches while comebacks stay possible? -->
<!-- Date: 2026-11-17 -->

# rolchess-v3-concept

**Status:** concluded — PROCEED (2026-11-17). Reglas del Modificador cerradas; porcentajes y
umbrales se ajustan en el MVP. Concepto: `design/gdd/game-concept.md`

Copia de `prototypes/rolchess-v2-concept/` (que queda intacta) con los cambios del jugador del
2026-11-17. Informe de la v2: `prototypes/rolchess-v2-concept/REPORT.md`.

## Hypothesis

Con el **Modificador** (un dado de 5 caras cuyas probabilidades cambian según cómo va la
partida y se ven antes de tirar), el dado deja de decidir partidas y la remontada sigue siendo
posible. Se cumple si, en partidas de corrido con al menos un rival externo:
- los turnos perdidos (Distracción) quedan **bien por debajo del 20%** de las tiradas;
- **nadie dice que la ganó el dado**, y las probabilidades se perciben como justas y visibles;
- sigue habiendo remontadas, pero **quien tiene más piezas gana la mayoría** (pilar 3: 2 de
  cada 3 como propuesta);
- la partida dura **15 min o menos** (pilar 4) y el timer por etapas no corta turnos seguido;
- el **ataque combinado decisivo** aparece en buena parte de los turnos (primera medición).

Comparar también: con y sin castillos (pendiente desde la v2).

## Cómo correrlo

1. Unity Hub → **Add → Add project from disk** → `prototypes/rolchess-v3-concept/`
   (editor 6000.3.28f1).
2. Pulsa **Play** en cualquier escena. En la pantalla de opciones elegí castillos, posición y
   timer. **Empezar**.
3. Partida en la misma PC, turnándose con el ratón. Al terminar, la consola de Unity muestra
   un resumen con cada tirada (jugador, piezas, tabla y cara).

## Qué cambió desde la v2

| Tema | En la v3 |
|---|---|
| Dado → **Modificador** | 5 caras: 1 Distracción (pierdes el turno) · 2 Grito de guerra (+2 de ataque y +2 de movimiento a todas tus piezas en ese mismo turno; no se acumula) · 3 Llamada al servicio (reclutas 4 soldados, arqueros o escuderos) · 4 Cuerno de guerra (4 jinetes, 2 catapultas o 1 demoledor) · 5 Arremetida (mueves todas las piezas) |
| Probabilidades | Según tus piezas comparadas con las del rival, Rey incluido y muros no: Normal 20/20/20/20/20 · Pierde con 3/4 o menos 20/20/25/20/15 · Pierde con 1/2 o menos 15/20/25/25/15 · Gana 4 a 3 o más 20/25/15/20/20 · Gana 2 a 1 o más 20/25/15/15/25. Se evalúa para cada jugador al empezar su turno, antes de tirar. Con un muro roto no puede reclutar: Llamada al servicio y Cuerno de guerra bajan a 0% y su parte se reparte entre Grito de guerra (la mitad, redondeada hacia abajo) y Arremetida (el resto). Las de los dos jugadores se ven siempre en el panel de información. Probabilidades y umbrales revisados después de la partida 1 (antes: 2/3, 1/3, 3/2 y 3/1) |
| Pantalla | Los números de las caras son internos: el juego muestra solo el nombre del modificador. Las elecciones del modificador (qué reclutar, pasar el turno) aparecen en un recuadro en el centro del tablero |
| Grito de guerra | Se aplica solo en el turno en que sale: +2 de ataque y +2 de movimiento, sin cargas ni botón; el ataque de cada ficha se ve en verde (en la v2: carga de hasta 4, +2 ataque y +2 defensa hasta el turno siguiente). Cambiado después de la partida 1 |
| Timer | 60 s para mover + 30 s para atacar; desde el minuto 10 de partida, 40 + 20. Se acaba el de mover → pasa a atacar. Se acaba el de atacar → se resuelven los ataques asignados y termina el turno. Reclutar cuenta como tiempo de mover. Opción "Sin límite" |
| Alcance circular | Una casilla está en alcance si dx² + dy² ≤ alcance² + 1 (con 6, hasta 37). La catapulta no alcanza las 8 vecinas. Las piezas de alcance 1 siguen igual |
| Rey | Al seleccionarlo se marcan en celeste las casillas para salir del castillo (o entrar), sin botón. Ese turno no se mueve más |
| Arqueros en el muro | Un arquero puede moverse a un tramo libre de su propio muro (un arquero por tramo; el muro no se atraviesa). Arriba tiene +2 de defensa. El daño a esa casilla lo reciben el arquero (menos su defensa) y el muro (completo). Si el muro se rompe, el arquero queda en el hueco |
| Medición | Turnos perdidos sobre tiradas, conteo de cada cara y de cada tabla, tiempo agotado al mover y al atacar, y registro de cada tirada en la consola |

**Supuestos del prototipo** (no los dijo el jugador): para elegir la tabla se cuentan todas las
piezas vivas, Rey incluido; el arquero en el muro resta su defensa y el muro recibe el daño
completo; las cinco tablas suman 100%.
De la v2 siguen: movimiento en cruz, solo se atacan muros enemigos.

**Fuera:** escudero que cubre de flechas, torres, tirada para ver quién empieza, tablero
hexagonal (oculto).

## Findings

En curso. Partidas jugadas:

| Fecha | Reglas | Config | Duración | Turno prom. (J1 / J2) | Más largo | Perdidos / tiradas | Tablas usadas | Caras 1·2·3·4·5 | Tiempo agotado (mover / atacar) | Combinado decisivo | Ganó |
|---|---|---|---|---|---|---|---|---|---|---|---|
| 2026-11-17 | Probabilidades viejas, Grito con cargas | 24×15, castillos en 4 y 6, timer 60+30 → 40+20 | 32:45 | 0:57 (0:57 en 18 / 0:57 en 16) | 1:30 | 8 / 42 (19%) | Normal 33 · pierde 2/3: 5 · pierde 1/3: 1 · gana 3/2: 3 | 8 · 10 · 7 · 11 · 6 | 16 / 29 de 34 turnos | 21 de 34 (61%); 2+ piezas a un objetivo en 27 de 31 turnos con ataque | Rojo |

**Partida 1 (2026-11-17), jugadores nuevos.** Se jugó antes de cambiar las probabilidades y el
Grito de guerra.
- **Duración:** 32:45, el doble de la meta. Hubo mucho ida y vuelta porque salieron muchas
  caras de reclutar (3 y 4: 18 de 42 tiradas). Al principio los jugadores tenían dudas; cuando
  entendieron el juego se entusiasmaron y no se dieron cuenta de que había pasado media hora.
- **Tiempo agotado:** el de atacar se agotó en 29 de 34 turnos. Por ser nuevos no llegaban a
  planear los ataques y a veces esperaban a que se resolvieran solos. Para el jugador no es
  grave: con el timer va a pasar y acorta la partida.
- **Dado:** esta vez nadie sintió que lo ganó el dado; fue más parejo. Los turnos perdidos
  quedaron en 19%, por debajo del 20%, y para el jugador no son un problema.
- **Remontada:** el que estuvo acorralado al principio terminó ganando. Puede deberse a que,
  por ser nuevos, no sabían aprovechar la ventaja.
- **Modificador:** casi no actuó, porque se usó la tabla Normal en 33 de 42 tiradas. Con
  piezas parejas no se llega a 2/3 ni a 3/2 (con 15 piezas del rival, de 11 a 22 propias es
  Normal). La tabla se evalúa para cada jugador al empezar su turno, antes de tirar.
- **Combinado decisivo:** primera medición, 61%. Ahora distingue: no marca todos los turnos
  con ataque.

Cambios después de la partida 1: probabilidades revisadas y umbrales más cercanos (4 a 3 y 2 a
1; 3/4 y 1/2), sin caras de reclutar con un muro roto, Grito de guerra que se aplica en el mismo
turno con +2 de movimiento y el ataque en verde, elecciones del modificador en el centro del
tablero y caras sin número en pantalla.

**Cierre (2026-11-17).** Se cierra con una partida. Lo que buscaba la v3 se cumplió: nadie
sintió que ganó el dado, los turnos perdidos quedaron por debajo del 20% y hubo remontada.
Además, a los jugadores nuevos les gustó. Las reglas del Modificador quedan como están. Los
porcentajes y umbrales (los últimos sin jugar) se ajustan con partidas del MVP. Quedan
abiertos: la duración (32:45 con jugadores nuevos contra la meta de 15 min), si el líder en
piezas gana la mayoría y la variante sin castillos.

Veredicto: **PROCEED**. Las reglas pasan a `design/gdd/game-concept.md`.
