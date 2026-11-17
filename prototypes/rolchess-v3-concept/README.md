<!-- PROTOTYPE - NOT FOR PRODUCTION -->
<!-- Question: with the Modificador (state-dependent die odds shown before rolling), does the die stop deciding matches while comebacks stay possible? -->
<!-- Date: 2026-11-17 -->

# rolchess-v3-concept

**Status:** in progress

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
| Dado → **Modificador** | 5 caras: 1 Distracción (pierdes el turno) · 2 Grito de guerra (+1 carga, hasta 4; al usarla, +2 de ataque a todas tus piezas en ese turno) · 3 Llamada al servicio (reclutas 4 soldados, arqueros o escuderos) · 4 Cuerno de guerra (4 jinetes, 2 catapultas o 1 demoledor) · 5 Arremetida (mueves todas las piezas) |
| Probabilidades | Según tus piezas comparadas con las del rival, Rey incluido y muros no: Normal 20/20/20/20/20 · Pierde con 2/3 o menos 15/20/25/25/15 · Pierde con 1/3 o menos 10/20/30/30/10 · Gana con 3/2 o más 20/25/15/15/25 · Gana con 3/1 o más 20/30/10/10/30. Las de los dos jugadores se ven siempre en el panel de información |
| Grito de guerra | Ya no da defensa y dura solo el turno en que se usa (en la v2: +2 ataque y +2 defensa hasta el turno siguiente) |
| Timer | 60 s para mover + 30 s para atacar; desde el minuto 10 de partida, 40 + 20. Se acaba el de mover → pasa a atacar. Se acaba el de atacar → se resuelven los ataques asignados y termina el turno. Reclutar cuenta como tiempo de mover. Opción "Sin límite" |
| Alcance circular | Una casilla está en alcance si dx² + dy² ≤ alcance² + 1 (con 6, hasta 37). La catapulta no alcanza las 8 vecinas. Las piezas de alcance 1 siguen igual |
| Rey | Al seleccionarlo se marcan en celeste las casillas para salir del castillo (o entrar), sin botón. Ese turno no se mueve más |
| Arqueros en el muro | Un arquero puede moverse a un tramo libre de su propio muro (un arquero por tramo; el muro no se atraviesa). Arriba tiene +2 de defensa. El daño a esa casilla lo reciben el arquero (menos su defensa) y el muro (completo). Si el muro se rompe, el arquero queda en el hueco |
| Medición | Turnos perdidos sobre tiradas, conteo de cada cara y de cada tabla, tiempo agotado al mover y al atacar, y registro de cada tirada en la consola |

**Supuestos del prototipo** (no los dijo el jugador): para elegir la tabla se cuentan todas las
piezas vivas, Rey incluido; el arquero en el muro resta su defensa y el muro recibe el daño
completo; la tabla "pierde con 2/3" sumaba 110% y el jugador eligió bajar la Arremetida a 15%.
De la v2 siguen: movimiento en cruz, solo se atacan muros enemigos.

**Fuera:** escudero que cubre de flechas, torres, tirada para ver quién empieza, tablero
hexagonal (oculto).

## Findings

Pendiente: todavía no se jugó.
