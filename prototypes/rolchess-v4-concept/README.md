<!-- PROTOTYPE - NOT FOR PRODUCTION -->
<!-- Question: with the v4 rules, do players who know the game finish 3 matches in a row in 15 min or less without feeling rushed? -->
<!-- Date: 2026-11-19 -->

# rolchess-v4-concept

**Status:** in progress (2026-11-19).

Copia de `prototypes/rolchess-v3-concept/` (que queda intacta) con todos los cambios de reglas
decididos el 2026-11-19 durante `/map-systems`. Reglas vigentes: `design/gdd/game-concept.md`
(tabla "Reglas vigentes"); sistemas: `design/gdd/systems-index.md`.

## Hypothesis

Con las reglas v4, dos jugadores que ya conocen el juego terminan **3 partidas seguidas en
15 min o menos** (pilar 4, compuerta del arte) sin sentirlas apuradas. Se cumple si:
- el archivo de métricas muestra 3 partidas terminadas seguidas con `en_15_min = si`;
- nadie dice que el timer le cortó jugadas que quería hacer.

**Supuesto riesgoso:** que los 33 min de la v3 fueran curva de aprendizaje y no las reglas.
Las métricas separan cuánto pesa la duración de cada turno y cuánto la cantidad de turnos;
eso dice qué regla tocar si no se llega.

## Cómo correrlo

1. Unity Hub → **Add → Add project from disk** → `prototypes/rolchess-v4-concept/`
   (editor 6000.3.28f1).
2. Pulsa **Play** en cualquier escena. En la pantalla de inicio elegí la posición de los
   castillos y el **timer**. La misma pantalla muestra la racha de la compuerta de 15 min.
   **Empezar**.
3. Partida en la misma PC, turnándose con el ratón.
4. Al terminar, el panel muestra la duración, la racha y dónde se guardó el archivo. El botón
   **Abrir carpeta de métricas** abre la carpeta.

## Métricas

En `Application.persistentDataPath/rolchess-v4/` (en Windows,
`%USERPROFILE%\AppData\LocalLow\<compañía>\<producto>\rolchess-v4\`). Nada se envía a ningún lado.

| Archivo | Contenido |
|---|---|
| `partidas.csv` | Una fila por partida: fecha, timer, castillos, terminada, ganador, duración, si entró en 15 min, turnos, promedio de turno por jugador, turno más largo, turnos perdidos, tiempos agotados, usos del deshacer, conteo de cada cara del Modificador, turnos con ataque, combinado y decisivo, turnos en que el ganador iba atrás en piezas, remontada (el ganador tiró con una tabla "Pierde") y si el que iba adelante a mitad de partida ganó |
| `partida-AAAAMMDD-HHMMSS.json` | Lo mismo con el detalle de cada turno |
| `partida-AAAAMMDD-HHMMSS-turnos.csv` | Un turno por fila: jugador, cara, tabla, piezas propias y del rival, segundos, movidas, deshacer, perdido, tiempo agotado al mover y al atacar, si atacó, combinó y si el combinado fue decisivo |

Una partida que se cierra sin terminar se guarda con `terminada = no` y no cuenta para la
racha (ni a favor ni en contra).

## Qué cambió desde la v3

| Tema | En la v4 |
|---|---|
| Rey | Empieza en el centro de su castillo. Atraviesa sus propios muros con su movimiento normal; no puede quedarse sobre ellos. Se quitó la regla de entrar y salir (casillas celestes, "ese turno no se mueve más") |
| Catapulta | 14 de daño en la casilla elegida y 10 en las 8 de alrededor (antes: 10 y la mitad). Ataque de la ficha: 14 |
| Deshacer | Se pueden deshacer todos los movimientos del turno, del último al primero. El botón muestra cuántos hay. (La pila ya existía en la v3; el botón decía "último movimiento") |
| Timer | Se elige al empezar: Variable 90 → 60 (60 + 30, y 40 + 20 desde el minuto 10), Fijo 90 (60 + 30), Fijo 60 (40 + 20), Sin límite |
| Sin castillos | Opción quitada (variante descartada) |
| Métricas | Archivo por partida, tabla acumulada y racha de la compuerta de 15 min |

**Supuestos del prototipo** (no los dijo el jugador):
- Los timers fijos mantienen la proporción 2 a 1 entre mover y atacar.
- El Grito de guerra suma +2 también al daño de área de la catapulta (10 → 12).
- La pila de deshacer solo tiene movimientos. Ni el reclutamiento ni la asignación de ataques entran en ella; las flechas ya se pueden cambiar antes de resolver. Esto quedó acordado con el jugador.
- "Remontada" es que el ganador haya tirado con una tabla "Pierde" al menos una vez. "Líder a mitad" se mide en el turno del medio de la partida.

Siguen todas las reglas de la v3: Modificador con probabilidades por estado, límites de
llamadas, jinete que salta enemigos, escudero que cubre, arqueros en el muro, alcance circular,
movimiento en cruz.

**Fuera:** menú principal, tutoriales, IA, idioma, arte, audio, online.

## Findings

En curso. Partidas jugadas:

| Fecha | Timer | Duración | Turnos | Turno prom. (J1 / J2) | En 15 min | Notas |
|---|---|---|---|---|---|---|
