<!-- PROTOTYPE - NOT FOR PRODUCTION -->
<!-- Question: with the v1 fixes, do turns stay brisk and do combined attacks become real decisions? -->
<!-- Date: 2026-11-13 -->

# rolchess-v2-concept

**Status:** in progress

## Hypothesis

Con los arreglos de la v1 (ataques planificados con flechas, atravesar aliados, alcance en
zona, muro con vida, timer de turno), el turno sigue siendo ágil y el ataque combinado se
vuelve una decisión real. Se cumple si, en una **partida de corrido y simétrica**:
- el turno promedio queda por debajo del timer elegido y la partida dura **menos de 40 min**;
- en la mitad de los turnos o más se combina un ataque de 2 o más piezas sobre un objetivo;
- nadie dice que la ganó "el dado".

Comparar: con vs sin castillos (criterios del CD-PLAYTEST de la v1). El tablero hexagonal
queda descartado por ahora (2026-11-16): el código sigue, la opción está oculta.

## Cómo correrlo

1. Unity Hub → **Add → Add project from disk** → `prototypes/rolchess-v2-concept/`
   (editor 6000.3.28f1).
2. Pulsa **Play** en cualquier escena. Aparece la pantalla de opciones: tablero, castillos,
   tamaño y timer. **Empezar**.
3. Partida en la misma PC, turnándose con el ratón.

## Qué cambió desde la v1 (notas de la partida 1)

| Nota | En la v2 |
|---|---|
| 1 deshacer | Botón "Deshacer último movimiento" |
| 2, 8, 10 ataques | Se asignan con flechas y se resuelven todos juntos; daño sumado por objetivo, escudo una vez; vista previa del daño (si el golpe mata, solo una banda negra MUERE sobre la pieza, sin el número; ROTO en los muros) |
| 3, 18 alcance | Zona: cualquier casilla a esa distancia o menos (catapulta 2–6). El alcance de ataque se pinta en rojizo para la pieza seleccionada o la que está bajo el cursor, propia o rival |
| catapulta | Dispara a cualquier casilla a su alcance, aunque esté vacía, con daño completo en esa casilla y la mitad (redondeada hacia abajo) en todas las de alrededor: enemigos, aliados y muros (también los propios) |
| 4 reclutas | Aparecen del lado que mira al rival (y el error de asimetría de la v1 desaparece) |
| 5 castillos | En los extremos del tablero; opción de posición al azar: el tablero se divide en 9 sectores como un teclado de teléfono y los castillos (azul y rojo) caen en 4 y 6, 1 y 9 o 7 y 3 (se quitaron 1 y 3, 7 y 9, 1 y 7 y 3 y 9; 2 y 8 no entra en el tablero de 15 filas). La formación inicial mira al rival: de arriba hacia abajo cuando los dos castillos comparten columna, y en diagonal en 1 y 9 y 7 y 3. Sin castillos vale la misma lógica, con los Reyes en esos sectores |
| 6 Rey | Regla del cuaderno: sale apareciendo al lado / entra donde elijas; ese turno no se mueve más |
| 7 atravesar | Se atraviesan aliados, nunca enemigos |
| 9 dado | Se tira solo al empezar cada turno; dado grande en el centro, efecto en el panel y lista de caras siempre visible |
| caras del dado | 2: carga de refuerzo (hasta 4); al gastarla, +2 ataque y +2 defensa a todas tus piezas hasta tu próximo turno. 3: reclutas 4 soldados, arqueros o escuderos. 4: reclutas especiales: 4 jinetes, 2 catapultas o 1 demoledor. 5: turno normal, sin efecto (se quitó la retirada) |
| 11 muro | Solo el borde; 16 tramos (12 en hexagonal: el anillo a distancia 2 del Rey) de 30 de vida, sin escudo; cualquier pieza enemiga en alcance lo ataca; romper uno abre un hueco y ese castillo ya no recluta (regla del jugador: para reclutar hacen falta todos los muros) |
| 12 demoledor | Explota: daña todas las casillas vecinas (aliados y muros incluidos) y muere; la vista previa marca MUERE sobre él |
| 14 timer | 60 / 90 s o sin límite; al llegar a 0 se resuelven los ataques asignados y termina el turno |
| 15 auto | Pasa solo a ataque cuando no quedan movimientos; termina solo únicamente cuando no hay ningún ataque posible (2 s de aviso); los ataques asignados esperan al botón Resolver |
| 16 sin castillos | Opción: Rey atrás, reclutas a su alrededor; sin castillos no se reclutan demoledores |
| inicio | Cada jugador empieza con 3 columnas de 4: arqueros atrás, soldados, escuderos al frente; el rojo espejado. Pegadas al castillo, o cerca del Rey si no hay castillos, para que haya que acercarse al rival. Tablero de 24 × 15 |
| 17 hexagonal | Opción de tablero hexagonal: casillas con lado plano arriba, en columnas verticales, y fichas hexagonales |
| 19 estadísticas | Cada ficha muestra la letra grande y, en columna, V vida, A ataque, D defensa, en números grandes con sombra |
| medición | Además de los tiempos, cuenta el ataque combinado decisivo: turnos donde 2 o más piezas, sumando, matan (o rompen un muro) cuando ninguna sola lo haría, o atraviesan una defensa que cada una por separado no supera. Aparte, el conteo simple de 2+ piezas a un mismo objetivo |
| pantalla | Pensada para 16:9: tablero fijo de 24 × 15 que ocupa todo menos una franja a la derecha. En esa franja el azul ve sus acciones en la mitad de arriba y el rojo en la de abajo; la otra mitad muestra la información de referencia |

Sin cambios a pedido: la cara 1 del dado sigue haciendo perder el turno.

**Supuestos del prototipo** (no los dijo el jugador): en el cuadrado se mueve en cruz (en
hex, a las 6 vecinas);
solo se atacan muros enemigos (la explosión del demoledor sí daña los propios).

**Fuera:** escudero que cubre de flechas, torres, tirada para ver quién empieza.

## Findings

En curso. Partidas jugadas:

| Fecha | Config | Duración | Turno prom. (J1 / J2) | Más largo | Perdidos / por tiempo | Ataque combinado | Ganó |
|---|---|---|---|---|---|---|---|
| 2026-11-16 | Cuadrado 24×24, con castillos, timer 90 s | 13:42 | 0:47 (0:57 en 8 / 0:38 en 9) | 1:26 | 2 / 0 | no medido | Azul |
| 2026-11-16 | Cuadrado 24×24, castillos en 3 y 9, timer 90 s | 13:35 | 0:50 (0:43 en 8 / 0:57 en 8) | 1:20 | 4 / 0 | 13 de 16 turnos (81%) | Rojo |
| 2026-11-16 | Cuadrado 24×24, castillos en 4 y 6, timer 90 s | 14:03 | 1:05 (0:57 en 7 / 1:16 en 5) | 1:25 | 5 / 0 | 11 de 12 turnos (91%) | Azul |

**Partida del 2026-11-16.** Duración y ritmo cumplen: muy por debajo de 40 min y ningún turno
cortado por el timer de 90 s. El final fue un Rey rojo que recibió 60 de daño de una vez (dos
demoledores de 30), con dos jinetes azules muertos por la misma explosión y dos muros rojos
rotos. A vigilar: dos demoledores alcanzan para matar a un Rey de 30 de vida. No quedó
registrado con qué versión de las reglas se jugó (antes o después de "1 demoledor por
reclutamiento"). El criterio de ataque combinado no se medía todavía; el contador se agregó
después de esta partida.

**Partidas 2 y 3 del 2026-11-16** (ya con dado automático, catapulta en área, refuerzo y
castillos por sectores). Los tres criterios medibles se cumplen: ~14 min de partida, ningún
turno cortado por el timer y ataque combinado en 81% y 91% de los turnos jugados. Dos
reservas: (1) el contador de ataque combinado marcó todos los turnos con ataque (13 de 13 y
11 de 11), así que tal como está no distingue una decisión real de algo que pasa siempre;
(2) los turnos perdidos por la cara 1 fueron 4 de 20 y 5 de 17, y en la partida 3 el rojo
jugó 5 turnos contra 7 del azul y perdió. Falta preguntar a los jugadores si sintieron que
la ganó el dado.

Después de la partida 3 el contador se hizo más exigente (ataque combinado decisivo), así que
los 81% y 91% de arriba son del conteo simple y no se comparan con las partidas siguientes.

Pendiente: partidas sin castillos (las tres jugadas son cuadrado 24 con castillos y timer de
90 s), y la respuesta de los jugadores sobre el dado.
