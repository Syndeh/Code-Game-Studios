<!-- PROTOTYPE - NOT FOR PRODUCTION -->
<!-- Question: with the v4 rules, do players who know the game finish 3 matches in a row in 25 min or less (target 15-20, revised 2026-11-19)? -->
<!-- Date: 2026-11-19 -->

# rolchess-v4-concept

**Status:** in progress (2026-11-19).

Copia de `prototypes/rolchess-v3-concept/` (que queda intacta) con todos los cambios de reglas
decididos el 2026-11-19 durante `/map-systems`. Reglas vigentes: `design/gdd/game-concept.md`
(tabla "Reglas vigentes"); sistemas: `design/gdd/systems-index.md`.

## Hypothesis

Con las reglas v4, dos jugadores que ya conocen el juego terminan **3 partidas seguidas en
25 min o menos** (pilar 4 revisado el 2026-11-19: meta 15–20 min, techo 25; compuerta del
arte). Se cumple si el archivo de métricas muestra 3 partidas terminadas seguidas de 25 min
o menos (la racha se calcula con la columna `duracion_seg`).

*Cambio del 2026-11-19, después de la partida 1:* la hipótesis pedía 15 min o menos y que
nadie sintiera que el timer le cortaba jugadas. Los jugadores dijeron que la partida de 23
min, pareja, fue la más divertida, y que quedarse sin tiempo para mover o atacar es presión
que obliga a concentrarse. Los tiempos agotados se siguen midiendo, pero ya no son una falla.

**Supuesto riesgoso:** que los 33 min de la v3 fueran curva de aprendizaje y no las reglas.
Las métricas separan cuánto pesa la duración de cada turno y cuánto la cantidad de turnos;
eso dice qué regla tocar si no se llega.

## Cómo correrlo

1. Unity Hub → **Add → Add project from disk** → `prototypes/rolchess-v4-concept/`
   (editor 6000.3.28f1).
2. Pulsa **Play** en cualquier escena. En la pantalla de inicio elegí la posición de los
   castillos y el **timer**. La misma pantalla muestra la racha de la compuerta de 25 min.
   **Empezar**.
3. Partida en la misma PC, turnándose con el ratón.
4. Al terminar, el panel muestra la duración, la racha y dónde se guardó el archivo. El botón
   **Abrir carpeta de métricas** abre la carpeta.

## Métricas

En `Application.persistentDataPath/rolchess-v4/` (en Windows,
`%USERPROFILE%\AppData\LocalLow\<compañía>\<producto>\rolchess-v4\`). Nada se envía a ningún lado.

| Archivo | Contenido |
|---|---|
| `partidas.csv` | Una fila por partida: fecha, timer, castillos, terminada, ganador, duración, si entró en 25 min (columna `en_15_min` en archivos creados antes del cambio), turnos, promedio de turno por jugador, turno más largo, turnos perdidos, tiempos agotados, usos del deshacer, conteo de cada cara del Modificador, turnos con ataque, combinado y decisivo, turnos en que el ganador iba atrás en piezas, remontada (el ganador tiró con una tabla "Pierde") y si el que iba adelante a mitad de partida ganó |
| `partida-AAAAMMDD-HHMMSS.json` | Lo mismo con el detalle de cada turno |
| `partida-AAAAMMDD-HHMMSS-turnos.csv` | Un turno por fila: jugador, cara, tabla, piezas propias y del rival, segundos, movidas, deshacer, perdido, tiempo agotado al mover y al atacar, si atacó, combinó y si el combinado fue decisivo |

Una partida que se cierra sin terminar se guarda con `terminada = no` y no cuenta para la
racha (ni a favor ni en contra).

## Qué cambió desde la v3

| Tema | En la v4 |
|---|---|
| Rey | Empieza en el centro de su castillo. Atraviesa sus propios muros con su movimiento normal; no puede quedarse sobre ellos. Se quitó la regla de entrar y salir (casillas celestes, "ese turno no se mueve más") |
| Catapulta | 12 de daño en la casilla elegida y 8 en las 8 de alrededor (v3: 10 y la mitad; primera v4: 14 y 10). Ataque de la ficha: 12 |
| Deshacer | Se pueden deshacer todos los movimientos del turno, del último al primero. El botón muestra cuántos hay. (La pila ya existía en la v3; el botón decía "último movimiento") |
| Timer | Se elige al empezar: Variable 90 → 60 (60 + 30, y 40 + 20 desde el minuto 10), Fijo 90 (60 + 30), Fijo 60 (40 + 20), Sin límite |
| Sin castillos | Opción quitada (variante descartada) |
| Distracción | Ya no se pierde el turno: no hay fase de movimiento, se pasa directo a atacar (con el timer de ataque) |
| Llamadas de reclutamiento | Llamada al servicio sale como mucho 6 veces por partida (antes 8). Cuerno de guerra: jinetes 3 llamadas (antes 4); catapultas y demoledores siguen en 4 |
| Escudero | Un aliado solo puede estar cubierto por un escudero. Un aliado ya cubierto no se resalta y, si se lo elige, aparece el aviso "Ese aliado ya está cubierto por otro escudero" |
| Métricas | Archivo por partida, tabla acumulada y racha de la compuerta (25 min desde el 2026-11-19) |

**Supuestos del prototipo** (no los dijo el jugador):
- Los timers fijos mantienen la proporción 2 a 1 entre mover y atacar.
- El Grito de guerra suma +2 también al daño de área de la catapulta (8 → 10).
- Con Distracción tampoco se recluta (la cara no es de reclutar); el turno empieza en la fase de ataque.
- La pila de deshacer solo tiene movimientos. Ni el reclutamiento ni la asignación de ataques entran en ella; las flechas ya se pueden cambiar antes de resolver. Esto quedó acordado con el jugador.
- "Remontada" es que el ganador haya tirado con una tabla "Pierde" al menos una vez. "Líder a mitad" se mide en el turno del medio de la partida.

Siguen todas las reglas de la v3: Modificador con probabilidades por estado, límites de
llamadas, jinete que salta enemigos, escudero que cubre, arqueros en el muro, alcance circular,
movimiento en cruz.

**Fuera:** menú principal, tutoriales, IA, idioma, arte, audio, online.

## Findings

En curso. Partidas jugadas:

| Fecha | Reglas | Timer | Duración | Tiradas | Turno prom. (J1 / J2) | Tiempo agotado (mover / atacar) | Combinado decisivo | Distracción | Ganó | En 25 min |
|---|---|---|---|---|---|---|---|---|---|---|
| 2026-11-19 | Primera v4: catapulta 14/10, Distracción pierde el turno | Variable 90 → 60 | 23:01 | 28 (24 jugados + 4 perdidos) | 0:56 / 0:58 | 7 / 19 de 24 | 19 de 24 (79%) | 4 de 28 (14%) | Rojo | sí |
| 2026-11-19 | Catapulta 12/8, Distracción con ataque; sin el cambio del escudero ni los límites nuevos de reclutamiento | Variable 90 → 60 | 22:52 | 34 (todos jugados) | 0:42 / 0:38 | 1 / 11 de 34 | 24 de 34 (70%) | 6 de 34 (17%) | Rojo | sí |

**Partida 1 (2026-11-19), el diseñador contra un jugador nuevo** (con muchas consultas). Comparada con la v3: 30% más corta (23:01 contra 32:45) porque hubo
menos turnos (28 tiradas contra 42), no turnos más rápidos (0:57 en las dos). El timer marca el
ritmo: el tiempo de atacar se agotó en 19 de 24 turnos. Combinado decisivo 79% (v3: 61%).
Tablas "Pierde" en 11 de 28 tiradas. El golpe final fue de 70 contra un Rey de 30 de vida.
Los jugadores dijeron que se divirtieron más que antes porque la partida fue pareja, y que
agotar el tiempo de mover o atacar presiona y concentra: no es un problema. A partir de esto
se revisó el pilar 4 (meta 15–20 min, techo 25). Después de esta partida cambiaron la
catapulta (12/8), la Distracción (sin fase de movimiento, con ataque) y el escudero (un aliado,
un solo escudero): la próxima partida ya no es comparable uno a uno.

*Partida 1 turno por turno* (`partida-20261119-134345-turnos.csv`): parejos hasta el turno 6
(17 contra 17). En el turno 6 rojo sacó Grito de guerra y azul pasó de 17 a 7 piezas en una
sola resolución. Desde el turno 7 azul tiró siempre con tablas "Pierde" (1/2 o menos 7 veces,
3/4 o menos 6), reclutó 6 veces, llegó a 12 contra 16 en el turno 23 y nunca pasó adelante:
rojo ganó en el turno 28. Esa cola de 22 turnos fue unos 16 de los 23 minutos. El pilar 3
funcionó (el que tenía más piezas ganó, y el que perdía siguió jugando), pero ese sostén
cuesta tiempo: es la tensión entre los pilares 3 y 4 que el concepto marcaba "a vigilar". El
jugador nuevo se ve en los datos: 7 movimientos deshechos en el turno 17 y 7 tiempos de mover
agotados. Salieron 8 Llamadas al servicio y 6 Cuernos de guerra.

**Partida 2 (2026-11-19), dos jugadores que ya conocían el juego**, sin saber las reglas
nuevas y sin el cambio del escudero. Misma duración que la partida 1 (22:52) con turnos más
rápidos (0:40 contra 0:57) y más turnos jugados (34; la Distracción ya no hace perder el
turno). Tiempo de atacar agotado en 11 de 34 turnos. Combinado decisivo 70%. Golpe final de 48
contra un Rey de 30. De esta partida salieron los límites nuevos de reclutamiento (Llamada al
servicio 6, jinetes 3).

*Partida 2 turno por turno* (`partida-20261119-141511-turnos.csv`): azul dominó la primera
mitad (reclutó en el turno 3; 11 contra 5 en el 14, con un Grito de guerra en el 13). Rojo
remontó con 4 y 5 piezas: tabla "Pierde 1/2", Llamada al servicio en el 16 y Cuerno de guerra
en el 18 y el 20; en el 18 se llevó 5 piezas y en el 19 igualó 6 a 6. Del 19 al 30 se
alternaron el liderazgo (6–6, 9–9, 6–6); rojo se despegó en el 31 con otra Llamada al servicio
y cerró con Grito de guerra. El ganador estuvo 17 turnos detrás y el que iba adelante a mitad
de partida perdió. Con Distracción, azul igual hizo ataques combinados decisivos (turnos 5
y 7): el jugador sigue cosechando.

*Las dos partidas juntas:* duraron 23 minutos por caminos distintos (partida 1: un turno
decide y 22 de cola sin remontada; partida 2: dominio, remontada y un tramo largo y parejo).
Las dos veces el sostén del pilar 3 es lo que lleva la partida más allá de 20 minutos. Pilar
3c (el que tiene más piezas gana 2 de cada 3): va 1 de 2, a vigilar.

*Roce del prototipo:* en los turnos 1, 5 y 7 de la partida 2 (los tres con Distracción) la
fase de ataque duró exactamente 30 s hasta agotar el reloj, y en el turno 1 no hubo ningún
ataque. Lo más probable es que un escudero con un aliado al lado cuente como acción posible y
evite que el turno se cierre solo. No cambia reglas, pero suma medio minuto vacío.

*Pruebas que no cuentan:* `partida-20261119-111506` (2 turnos) y `partida-20261119-131953`
(7 turnos, sin límite) quedaron sin terminar; se usaron para probar que el juego anda.

**A mirar en las próximas partidas:** cuántos turnos pasan entre el turno que decide la
partida y el final (si la cola sigue larga, las palancas son Rendirse, un cierre por tiempo
de la Máquina de turno o menos refuerzos para el que pierde) y si Grito de guerra decide
partidas solo (en la partida 1 una sola cara se llevó 10 piezas).
