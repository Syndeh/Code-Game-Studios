<!-- PROTOTYPE - NOT FOR PRODUCTION -->
<!-- Question: with the v5 rules (archer 1-4, escudero 1-2, catapult 10/6 with shield 2, soldier shield 3, recruiting lost with 2 walls down, no Modificador on turn 1, Arremetida only above 12 pieces, a 12:30 clock per player), do players who know the game finish 3 matches in a row in 25 min or less (target 15-20) by the board and not by the clock? -->
<!-- Date: 2026-11-24 -->

# rolchess-v5-concept

**Status:** in progress (2026-11-24).

Copia de `prototypes/rolchess-v4-concept/` (que queda intacta) con los cambios de reglas que
salieron de las partidas v4 del 2026-11-21. Todo lo demás es igual a la v4: ver su
[README](../rolchess-v4-concept/README.md) para las reglas heredadas, los supuestos y las
partidas anteriores. Estadísticas de piezas: `design/gdd/catalogo-de-piezas.md`.

## Hypothesis

Con las reglas v5, dos jugadores que ya conocen el juego siguen terminando **3 partidas
seguidas en 25 min o menos** (pilar 4: meta 15–20 min, techo 25). Se cumple si el archivo de
métricas muestra 3 partidas terminadas seguidas de 25 min o menos. Con el reloj de 12:30 por
jugador ninguna partida con timer pasa de 25 min, así que la pregunta pasa a ser **cómo**
terminan: la columna `motivo_fin` dice si fue Rey muerto, reloj agotado o rendición. Si muchas
terminan por reloj, el reloj está decidiendo partidas que el tablero no decidió.

**Supuesto riesgoso:** que los cambios defensivos (escudo del soldado, catapulta más débil,
arquero más corto, escudero que ataca a 2, reclutar con un muro roto) alarguen las partidas. Las 7 partidas
terminadas de la v4 del 2026-11-21 entraron todas en 25 min (8 a 21 min).

## Cómo correrlo

1. Unity Hub → **Add → Add project from disk** → `prototypes/rolchess-v5-concept/`
   (editor 6000.3.28f1).
2. Pulsa **Play** en cualquier escena. En la pantalla de inicio elegí el **timer**.
   **Empezar**.
3. Partida en la misma PC, turnándose con el ratón.
4. Al terminar, el panel muestra la duración, la racha y dónde se guardó el archivo.

Las reglas completas para quien juega por primera vez están en [COMO-JUGAR.md](COMO-JUGAR.md).

## Métricas

En `Application.persistentDataPath/rolchess-v5/` (en macOS,
`~/Library/Application Support/DefaultCompany/rolchess-v3-concept/rolchess-v5/`). Carpeta
separada de la v4: las partidas de una versión no se mezclan con las de la otra. Mismos
archivos y columnas que la v4 (`partidas.csv`, `partida-*.json`, `partida-*-turnos.csv`); el
campo `version` dice `v5`. Columnas nuevas en `partidas.csv`: `motivo_fin` (Rey muerto, Reloj
agotado, Rendición), `reloj_azul` y `reloj_rojo` (segundos que les quedaron),
`vida_quitada_azul`, `vida_quitada_rojo` y `reclutas_auto`. En `partida-*-turnos.csv`:
`reloj_al_empezar` y `recluta_auto`. El turno 1 aparece con modificador «Sin modificador
(turno 1)» y tabla `-`.

## Qué cambió desde la v4

| Tema | v4 | v5 |
|---|---|---|
| Arquero, alcance | 1–6 | **1–4** |
| Arquero sobre su muro | +2 de escudo | **+2 de escudo y +2 de alcance** (1–6); ataca desde el muro. Las casillas rojizas muestran el alcance real |
| Escudero, alcance de ataque | 1 | **1–2** (cubrir sigue siendo a un aliado vecino) |
| Catapulta, daño | 12 en la casilla, 8 alrededor | **10 en la casilla, 6 alrededor** (con Grito de guerra: 12 y 6; el Grito no suma al área) |
| Catapulta, escudo | 3 | **2** |
| Soldado, escudo | 2 | **3** |
| Castillo, reclutar | Con 1 muro derribado no se recluta | **Con 2 muros derribados** no se recluta |
| Posición de los castillos | Fija (4 y 6) o al azar (4 y 6, 1 y 9, 7 y 3) | **Siempre fija: sectores 4 y 6** (2026-11-25, GDD del Tablero) |
| Primer turno | Tiraba el Modificador | **Sin Modificador**: mueve (12 + Rey) y ataca, sin reclutar (Máquina de turno, regla 3) |
| Arremetida | Salía siempre | Solo con **más de 12 piezas sin el Rey**. Si no, su porcentaje (incluida la parte que recibe de las caras de reclutar) va entero a Grito de guerra. El panel de probabilidades lo avisa |
| Reloj de partida | Solo timer por etapa | Además, **12:30 por jugador** en total, que corre solo en su turno. Si llega a 0, ese jugador pierde, en cualquier fase. Se ven los dos relojes. Sin límite no tiene reloj. Reemplaza el cierre a los 25 min con desempate del GDD |
| Recluta y timeout | Si se acababa el tiempo de mover eligiendo el recluta, se perdía | Se recluta automáticamente el primer tipo disponible (Máquina de turno, regla 12) |
| Rendirse | No existía | Botón **Rendirse** (con confirmación), solo para el jugador del turno cuando tiene la mitad de las piezas del rival o menos. El que va ganando no lo ve |
| Vida quitada | No se medía | Se cuenta la vida realmente quitada a piezas y muros rivales (sin fuego amigo; lo que absorbe un escudero cuenta) |

Los textos en pantalla (ayuda de la catapulta, aviso de reclutamiento, panel de muros rotos)
leen los valores del código.

## De dónde salieron

Análisis de las 11 partidas terminadas de la v4 (2026-11-19 y 2026-11-21):

- **Reclutar decide más partidas que los vuelcos grandes.** En 7 de 11 el turno a partir del
  cual el ganador ya no estuvo empatado ni atrás fue una Llamada al servicio. Grito de guerra
  decidió 2 (vuelcos de −8 y −6 piezas).
- **Abrir reclutando pesa.** Azul abrió con Llamada o Cuerno en 7 partidas y ganó 5; en 3 no
  perdió nunca la ventaja. 17 contra 13 queda justo por encima del umbral de la tabla «Pierde
  (3/4 o menos)», así que rojo no recibe compensación.
- **El final con el Rey solo es corto** (4–13% de la partida) y el Rey solo puede remontar
  (partida `20261121-165452`).

El diseñador decidió los cambios de la tabla de arriba a partir de esas partidas. No tocan la
apertura ni los umbrales de las tablas: eso queda a observar.

## Findings

En curso. Sin partidas todavía.

**A mirar:** cuántas partidas terminan por reloj y quién iba adelante cuando pasó; si
alguien se rinde y en qué turno; si abrir sin Modificador saca la ventaja de reclutar en el
turno 1; cuántas veces queda Arremetida bloqueada (casi siempre al principio: se empieza con
12 piezas sin el Rey); si con el soldado más duro y la catapulta más débil las partidas se alargan; si
reclutar con un muro roto hace más largas las remontadas; si la ventaja de abrir reclutando
sigue.
